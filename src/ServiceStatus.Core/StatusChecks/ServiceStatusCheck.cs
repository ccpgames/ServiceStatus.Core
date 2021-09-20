using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using ServiceStatus.Core.Abstractions;
using ServiceStatus.Core.Constants;
using ServiceStatus.Core.Models;

namespace ServiceStatus.Core
{
    /// <summary>
    /// Service status check that can either be for DNS hostname or TCP requests
    /// </summary>
    public abstract class ServiceStatusCheck : IServiceStatusCheck
    {
        private static readonly Regex s_ipRegex = new Regex("[0-9]+\\.[0-9]+\\.[0-9]+\\.[0-9]+", RegexOptions.Compiled);
        protected readonly ILogger _logger;

        public abstract Dictionary<string, ServiceStatusRequirement> Responsibilities { get; }
        public abstract string Name { get; }
        public abstract TimeSpan? CacheDuration { get; }

        protected ServiceStatusCheck(ILogger<ServiceStatusCheck> logger)
        {
            _logger = logger;
        }

        public abstract Task<StatusCheckDetail> ExecuteStatusCheckAsync();
        public abstract bool IsEnabled();

        protected async Task<StatusCheckDetail> TcpConnectionCheckAsync(string host, int port, Stopwatch timer)
        {
            // Try testing DNS
            if (!await DnsTestAsync(host).ConfigureAwait(false))
                return new StatusCheckDetail("Name resolution failed.", timer.ElapsedMilliseconds);

            // Try testing TCP
            return !await TcpConnectTest(host, port)
                ? new StatusCheckDetail("Tcp connect failed.", timer.ElapsedMilliseconds)
                : new StatusCheckDetail(StatusTypes.OK, timer.ElapsedMilliseconds);
        }

        /// <summary>
        /// Test connection with a TCP client
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        private async ValueTask<bool> TcpConnectTest(string host, int port)
        {
            CancellationToken token = default;
            try
            {
                using (var client = new TcpClient())
                {
                    using (var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
                    {
                        token = cancellationToken.Token;
#if NET5_0_OR_GREATER
                        await client.ConnectAsync(host, port, token).ConfigureAwait(false);
                        return true;
#else
                        using (token.Register(() => client.Close()))
                        {
                            await client.ConnectAsync(host, port).ConfigureAwait(false);
                            return true;
                        }
#endif
                    }
                }
            }
#if NET5_0_OR_GREATER
            catch (OperationCanceledException e) when (token.IsCancellationRequested)
            {
                _logger?.LogWarning(e, $"Unable to connect via TCP to {host}:{port}.");
                return false;
            }
#elif NETCOREAPP3_1 || NETSTANDARD2_1
            catch (ObjectDisposedException e) when (token.IsCancellationRequested)
            {
                _logger?.LogWarning(e, $"Unable to connect via TCP to {host}:{port}.");
                return false;
            }
#elif NETFRAMEWORK && NET48_OR_GREATER
            catch (NullReferenceException e) when (token.IsCancellationRequested)
            {
                _logger?.LogWarning(e, $"Unable to connect via TCP to {host}:{port}.");
                return false;
            }
#endif
        }

        /// <summary>
        /// Test connection to a hostname
        /// </summary>
        /// <param name="hostNameOrAddress"></param>
        /// <returns></returns>
        private async Task<bool> DnsTestAsync(string hostNameOrAddress)
        {
            if (string.IsNullOrEmpty(hostNameOrAddress))
                throw new ArgumentNullException(nameof(hostNameOrAddress));

            // Let's make sure this isn't just an IP address
            // if it is, we return true
            if (s_ipRegex.IsMatch(hostNameOrAddress))
                return true;

            try
            {
                IPHostEntry result = await Dns.GetHostEntryAsync(hostNameOrAddress).ConfigureAwait(false);
                return result.AddressList.Length > 0;
            }
            catch (SocketException e)
            {
                _logger?.LogWarning(e, $"Unable to resolve {hostNameOrAddress}. Reason: {e.SocketErrorCode}");

                if (e.SocketErrorCode == SocketError.HostNotFound)
                    return false;

                throw;
            }
        }
    }
}
