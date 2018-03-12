using Microsoft.Extensions.Logging;
using ServiceStatus.Core.Constants;
using ServiceStatus.Core.Models;
using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ServiceStatus.Core.Sql
{
    public abstract class SqlServerStatusCheck : ServiceStatusCheck
    {
        protected SqlServerStatusCheck(ILogger logger) : base(logger) { }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public abstract string ResolveConnectionString();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override async Task<StatusCheckDetail> ExecuteStatusCheckAsync()
        {
            // Start a new timer
            var timer = Stopwatch.StartNew();

            try
            {
                var connectionStringBuilder = new SqlConnectionStringBuilder(ResolveConnectionString());

                try
                {
                    // Create a connection to the server
                    using (var connection = new SqlConnection(connectionStringBuilder.ConnectionString))
                    {
                        // Create a command to run on the server
                        using (var command = new SqlCommand("SELECT 1 as ping", connection))
                        {
                            // Set the command timeout
                            command.CommandTimeout = 5;

                            // Open the conection to the server
                            await connection.OpenAsync().ConfigureAwait(false);

                            // Run the command 
                            if ((int)await command.ExecuteScalarAsync().ConfigureAwait(false) == 1)
                            {
                                // Close the connection to the server
                                connection.Close();

                                return new StatusCheckDetail(StatusTypes.OK, timer.ElapsedMilliseconds);
                            }
                        }

                        throw new InvalidOperationException("We should never get here!");
                    }
                }
                catch (Exception e)
                {
                    _logger?.LogWarning(e, $"Unable to check SQL server @ {connectionStringBuilder?.DataSource}.{connectionStringBuilder?.InitialCatalog}. Reason: {e.Message}");

                    // Default port number for SQL server is 1433
                    int port = 1433;

                    // Check whether the data source has a port associated with it
                    if (connectionStringBuilder.DataSource.Contains(","))
                    {
                        int.TryParse(connectionStringBuilder?.DataSource?.Split(',')?[1], out port);
                    }
                    
                    // Run TCP check to see if the server is responsive
                    StatusCheckDetail tcpCheck = await TcpConnectionCheckAsync(connectionStringBuilder?.DataSource, port, timer).ConfigureAwait(false);

                    if (tcpCheck.Value != StatusTypes.OK)
                        return tcpCheck;

                    return new StatusCheckDetail(e.Message, timer.ElapsedMilliseconds);
                }
            }
            catch (Exception e)
            {
                _logger?.LogWarning(e, "Unable to parse connection string");

                return new StatusCheckDetail("Failed to parse connection string", timer.ElapsedMilliseconds);
            }
        }
    }
}
