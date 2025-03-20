using System.Collections.Generic;
using System.Threading.Tasks;

namespace PalletManagementSystem.Infrastructure.Services.SSRSIntegration
{
    /// <summary>
    /// Interface for SQL Server Reporting Services client
    /// </summary>
    public interface ISSRSClient
    {
        /// <summary>
        /// Gets a report as a byte array
        /// </summary>
        /// <param name="reportPath">The report path</param>
        /// <param name="parameters">The report parameters</param>
        /// <returns>The report as a byte array</returns>
        Task<byte[]> GetReportAsync(string reportPath, IDictionary<string, string> parameters);

        /// <summary>
        /// Exports a report to a specific format
        /// </summary>
        /// <param name="reportPath">The report path</param>
        /// <param name="parameters">The report parameters</param>
        /// <param name="format">The export format</param>
        /// <returns>The report as a byte array</returns>
        Task<byte[]> ExportReportAsync(string reportPath, IDictionary<string, string> parameters, string format);

        /// <summary>
        /// Prints a report to a specific printer
        /// </summary>
        /// <param name="reportPath">The report path</param>
        /// <param name="parameters">The report parameters</param>
        /// <param name="printerName">The printer name</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task<bool> PrintReportAsync(string reportPath, IDictionary<string, string> parameters, string printerName);

        /// <summary>
        /// Converts an object to a dictionary of string parameters
        /// </summary>
        /// <param name="parameters">The parameters object</param>
        /// <returns>A dictionary of string parameters</returns>
        IDictionary<string, string> ConvertParametersToDict(object parameters);
    }
}