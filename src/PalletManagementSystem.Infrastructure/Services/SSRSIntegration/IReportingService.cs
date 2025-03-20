using System.Threading.Tasks;

namespace PalletManagementSystem.Infrastructure.Services.SSRSIntegration
{
    /// <summary>
    /// Interface for reporting service operations
    /// </summary>
    public interface IReportingService
    {
        /// <summary>
        /// Gets a report as a byte array
        /// </summary>
        /// <param name="reportPath">The report path</param>
        /// <param name="parameters">The report parameters</param>
        /// <returns>The report as a byte array</returns>
        Task<byte[]> GetReportAsync(string reportPath, object parameters);

        /// <summary>
        /// Prints a report to a specific printer
        /// </summary>
        /// <param name="reportPath">The report path</param>
        /// <param name="parameters">The report parameters</param>
        /// <param name="printerName">The printer name</param>
        /// <returns>True if printing was successful, false otherwise</returns>
        Task<bool> PrintReportAsync(string reportPath, object parameters, string printerName);

        /// <summary>
        /// Exports a report to a specific format
        /// </summary>
        /// <param name="reportPath">The report path</param>
        /// <param name="parameters">The report parameters</param>
        /// <param name="format">The export format</param>
        /// <returns>The report as a byte array</returns>
        Task<byte[]> ExportReportAsync(string reportPath, object parameters, string format);
    }
}