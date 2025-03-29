// src/PalletManagementSystem.Core/Interfaces/Services/IReportingService.cs
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PalletManagementSystem.Core.Interfaces.Services
{
    public interface IReportingService
    {
        Task<bool> GenerateAndPrintReportAsync(
            string reportName,
            Dictionary<string, string> parameters,
            string printerName);

        Task<byte[]> GenerateReportAsync(
            string reportName,
            Dictionary<string, string> parameters,
            string format = "PDF");
    }
}