using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PalletManagementSystem.Infrastructure.Services.SSRSIntegration
{
    /// <summary>
    /// Client for SQL Server Reporting Services
    /// </summary>
    public class SSRSClient : ISSRSClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SSRSClient> _logger;
        private readonly string _ssrsBaseUrl;
        private readonly string _username;
        private readonly string _password;

        /// <summary>
        /// Initializes a new instance of the <see cref="SSRSClient"/> class
        /// </summary>
        /// <param name="httpClient">The HTTP client</param>
        /// <param name="configuration">The configuration</param>
        /// <param name="logger">The logger</param>
        public SSRSClient(HttpClient httpClient, IConfiguration configuration, ILogger<SSRSClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Get SSRS configuration
            _ssrsBaseUrl = _configuration["SSRS:BaseUrl"];
            _username = _configuration["SSRS:Username"];
            _password = _configuration["SSRS:Password"];

            // Configure HttpClient
            if (!string.IsNullOrEmpty(_username) && !string.IsNullOrEmpty(_password))
            {
                var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_username}:{_password}"));
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
            }
        }

        /// <inheritdoc/>
        public async Task<byte[]> GetReportAsync(string reportPath, IDictionary<string, string> parameters)
        {
            try
            {
                var url = BuildReportUrl(reportPath, parameters);
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error getting report from SSRS. Status code: {response.StatusCode}, Error: {errorContent}");
                    throw new Exception($"Failed to get report. Status code: {response.StatusCode}");
                }

                return await response.Content.ReadAsByteArrayAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting report {reportPath} from SSRS");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<byte[]> ExportReportAsync(string reportPath, IDictionary<string, string> parameters, string format)
        {
            try
            {
                var url = BuildReportUrl(reportPath, parameters, format);
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error exporting report from SSRS. Status code: {response.StatusCode}, Error: {errorContent}");
                    throw new Exception($"Failed to export report. Status code: {response.StatusCode}");
                }

                return await response.Content.ReadAsByteArrayAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error exporting report {reportPath} from SSRS");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> PrintReportAsync(string reportPath, IDictionary<string, string> parameters, string printerName)
        {
            try
            {
                // In a real implementation, this would use the SSRS web service to render and print the report
                // For this sample, we'll get the report PDF and simulate printing

                // Add printer parameter
                if (parameters == null)
                {
                    parameters = new Dictionary<string, string>();
                }

                parameters["rs:Command"] = "Render";
                parameters["rs:Format"] = "PDF";
                parameters["rs:PrinterName"] = printerName;

                var url = BuildReportUrl(reportPath, parameters);
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error printing report from SSRS. Status code: {response.StatusCode}, Error: {errorContent}");
                    return false;
                }

                _logger.LogInformation($"Successfully sent print job to printer {printerName} for report {reportPath}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error printing report {reportPath} to printer {printerName}");
                return false;
            }
        }

        /// <inheritdoc/>
        public IDictionary<string, string> ConvertParametersToDict(object parameters)
        {
            if (parameters == null)
                return new Dictionary<string, string>();

            var result = new Dictionary<string, string>();
            var properties = parameters.GetType().GetProperties();

            foreach (var prop in properties)
            {
                var value = prop.GetValue(parameters);
                if (value != null)
                {
                    result.Add(prop.Name, value.ToString());
                }
            }

            return result;
        }

        /// <summary>
        /// Builds the URL for a report
        /// </summary>
        /// <param name="reportPath">The report path</param>
        /// <param name="parameters">The report parameters</param>
        /// <param name="format">The optional format</param>
        /// <returns>The report URL</returns>
        private string BuildReportUrl(string reportPath, IDictionary<string, string> parameters, string format = null)
        {
            var urlBuilder = new StringBuilder();

            // Base URL
            urlBuilder.Append(_ssrsBaseUrl.TrimEnd('/'));

            // Report path
            urlBuilder.Append(reportPath.StartsWith("/") ? reportPath : $"/{reportPath}");

            // Format
            if (!string.IsNullOrEmpty(format))
            {
                urlBuilder.Append($"&rs:Format={format}");
            }

            // Parameters
            if (parameters != null && parameters.Count > 0)
            {
                var isFirstParam = urlBuilder.ToString().Contains("?") ? false : true;

                foreach (var param in parameters)
                {
                    urlBuilder.Append(isFirstParam ? "?" : "&");
                    urlBuilder.Append(Uri.EscapeDataString(param.Key));
                    urlBuilder.Append("=");
                    urlBuilder.Append(Uri.EscapeDataString(param.Value));

                    isFirstParam = false;
                }
            }

            return urlBuilder.ToString();
        }
    }
}