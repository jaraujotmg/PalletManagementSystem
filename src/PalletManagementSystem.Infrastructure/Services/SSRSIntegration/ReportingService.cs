// src/PalletManagementSystem.Infrastructure/Services/SSRSIntegration/ReportingService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PalletManagementSystem.Core.Interfaces.Services;

namespace PalletManagementSystem.Infrastructure.Services.SSRSIntegration
{
    public class ReportingService : IReportingService
    {
        private readonly ISSRSClient _ssrsClient;
        private readonly IAppSettings _appSettings;
        private readonly ILogger<ReportingService> _logger;

        public ReportingService(
            ISSRSClient ssrsClient,
            IAppSettings appSettings,
            ILogger<ReportingService> logger = null)
        {
            _ssrsClient = ssrsClient ?? throw new ArgumentNullException(nameof(ssrsClient));
            _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
            _logger = logger;
        }

        public async Task<bool> GenerateAndPrintReportAsync(
            string reportName,
            Dictionary<string, string> parameters,
            string printerName)
        {
            try
            {
                // Get report server URL and path from configuration
                string reportServerUrl = _appSettings.GetSetting("ReportServer:Url");
                string reportPath = _appSettings.GetSetting($"ReportServer:Reports:{reportName}");

                if (string.IsNullOrEmpty(reportServerUrl) || string.IsNullOrEmpty(reportPath))
                {
                    _logger?.LogWarning($"ReportingService: Missing configuration for report {reportName}");
                    return false;
                }

                // Add the printer name to the parameters
                parameters["PrinterName"] = printerName;

                // Call SSRS client to generate and print the report
                bool result = await _ssrsClient.PrintReportAsync(reportServerUrl, reportPath, parameters);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"ReportingService: Error generating report {reportName}");
                return false;
            }
        }

        public async Task<byte[]> GenerateReportAsync(
            string reportName,
            Dictionary<string, string> parameters,
            string format = "PDF")
        {
            try
            {
                // Get report server URL and path from configuration
                string reportServerUrl = _appSettings.GetSetting("ReportServer:Url");
                string reportPath = _appSettings.GetSetting($"ReportServer:Reports:{reportName}");

                if (string.IsNullOrEmpty(reportServerUrl) || string.IsNullOrEmpty(reportPath))
                {
                    _logger?.LogWarning($"ReportingService: Missing configuration for report {reportName}");
                    return null;
                }

                // Call SSRS client to generate the report
                byte[] reportData = await _ssrsClient.GenerateReportAsync(reportServerUrl, reportPath, parameters, format);

                return reportData;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"ReportingService: Error generating report {reportName}");
                return null;
            }
        }
    }
}