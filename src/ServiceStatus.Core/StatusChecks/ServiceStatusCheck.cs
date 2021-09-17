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
            try
            {
                using (var client = new TcpClient())
                {
#if NET5_0_OR_GREATER
                    using (var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
                    {
                        try
                        {
                            await client.ConnectAsync(host, port, cancellationToken.Token).ConfigureAwait(false);
                            return true;
                        }
                        catch (SocketException) when (cancellationToken.IsCancellationRequested)
                        {
                            return false;
                        }
                    }
#else
                    var delayTask = Task.Delay(5000);
                    return await Task.WhenAny(client.ConnectAsync(host, port)) != delayTask;
#endif
                }
            }
            catch (AggregateException a) when (a.InnerException is SocketException s)
            {
                _logger?.LogWarning(s, $"Unable to connect via TCP to {host}:{port}. Reason: {s.SocketErrorCode}.");
                return false;
            }
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
