using Microsoft.Extensions.Logging;
using ServiceStatus.Core.Abstractions;
using ServiceStatus.Core.Constants;
using ServiceStatus.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ServiceStatus.Core
{
    /// <summary>
    /// Service status check that can either be for DNS hostname or TCP requests
    /// </summary>
    public abstract class ServiceStatusCheck : IServiceStatusCheck
    {
        private static Regex _ipRegex = new Regex("[0-9]+\\.[0-9]+\\.[0-9]+\\.[0-9]+", RegexOptions.Compiled);
        protected readonly ILogger _logger;

        public abstract Dictionary<string, ServiceStatusRequirement> Responsibilities { get; }
        public abstract string Name { get; }
        public abstract TimeSpan? CacheDuration { get; }

        protected ServiceStatusCheck(ILogger logger)
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
            if (!TcpConnectTest(host, port))
                return new StatusCheckDetail("Tcp connect failed.", timer.ElapsedMilliseconds);

            return new StatusCheckDetail(StatusTypes.OK, timer.ElapsedMilliseconds);
        }

        /// <summary>
        /// Test connection with a TCP client
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        private bool TcpConnectTest(string host, int port)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    return client.ConnectAsync(host, port).Wait(5000);
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
            if (_ipRegex.IsMatch(hostNameOrAddress))
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
