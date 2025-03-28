// src/PalletManagementSystem.Web/Services/ISessionManager.cs
using PalletManagementSystem.Core.Models.Enums;

namespace PalletManagementSystem.Web2.Services
{
    public interface ISessionManager
    {
        Division GetCurrentDivision();
        void SetCurrentDivision(Division division);

        Platform GetCurrentPlatform();
        void SetCurrentPlatform(Platform platform);

        bool IsTouchModeEnabled();
        void SetTouchModeEnabled(bool enabled);

        void SetPreferredPrinter(string printerType, string printerName);
        string GetPreferredPrinter(string printerType);
    }
}