// src/PalletManagementSystem.Infrastructure/Services/SSRSIntegration/ISSRSClient.cs
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PalletManagementSystem.Infrastructure.Services.SSRSIntegration
{
    public interface ISSRSClient
    {
        Task<bool> PrintReportAsync(
            string reportServerUrl,
            string reportPath,
            Dictionary<string, string> parameters);

        Task<byte[]> GenerateReportAsync(
            string reportServerUrl,
            string reportPath,
            Dictionary<string, string> parameters,
            string format = "PDF");
    }
}