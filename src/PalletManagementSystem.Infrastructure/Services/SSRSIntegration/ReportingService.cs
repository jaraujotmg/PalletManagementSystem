using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace PalletManagementSystem.Infrastructure.Services.SSRSIntegration
{
    /// <summary>
    /// Implementation of the reporting service
    /// </summary>
    public class ReportingService : IReportingService
    {
        private readonly ISSRSClient _ssrsClient;
        private readonly ILogger<ReportingService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportingService"/> class
        /// </summary>
        /// <param name="ssrsClient">The SSRS client</param>
        /// <param name="logger">The logger</param>
        public ReportingService(ISSRSClient ssrsClient, ILogger<ReportingService> logger)
        {
            _ssrsClient = ssrsClient ?? throw new ArgumentNullException(nameof(ssrsClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<byte[]> GetReportAsync(string reportPath, object parameters)
        {
            try
            {
                var paramDict = _ssrsClient.ConvertParametersToDict(parameters);
                return await _ssrsClient.GetReportAsync(reportPath, paramDict);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting report {reportPath}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> PrintReportAsync(string reportPath, object parameters, string printerName)
        {
            try
            {
                var paramDict = _ssrsClient.ConvertParametersToDict(parameters);
                return await _ssrsClient.PrintReportAsync(reportPath, paramDict, printerName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error printing report {reportPath} to printer {printerName}");
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<byte[]> ExportReportAsync(string reportPath, object parameters, string format)
        {
            try
            {
                var paramDict = _ssrsClient.ConvertParametersToDict(parameters);
                return await _ssrsClient.ExportReportAsync(reportPath, paramDict, format);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error exporting report {reportPath} to {format}");
                throw;
            }
        }
    }
}