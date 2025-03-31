// src/PalletManagementSystem.Web/Services/ISessionManager.cs
using PalletManagementSystem.Core.Models.Enums;
using System.Threading.Tasks;

namespace PalletManagementSystem.Web2.Services
{
    public interface ISessionManager
    {
        Division GetCurrentDivision();
        //void SetCurrentDivision(Division division);
        Task SetCurrentDivisionAsync(Division division); // Changed signature

        Platform GetCurrentPlatform();
        //void SetCurrentPlatform(Platform platform);
        Task SetCurrentPlatformAsync(Platform platform); // Changed signature

        bool IsTouchModeEnabled();
        void SetTouchModeEnabled(bool enabled);

        void SetPreferredPrinter(string printerType, string printerName);
        string GetPreferredPrinter(string printerType);
    }
}