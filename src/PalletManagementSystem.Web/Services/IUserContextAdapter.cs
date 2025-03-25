// src/PalletManagementSystem.Web/Services/IUserContextAdapter.cs
using PalletManagementSystem.Core.Models.Enums;

namespace PalletManagementSystem.Web.Services
{
    public interface IUserContextAdapter
    {
        string GetUsername();
        string GetDisplayName();
        string GetEmail();
        bool IsInRole(string role);
        string[] GetRoles();
        bool CanEditPallets();
        bool CanClosePallets();
        bool CanEditItems();
        bool CanMoveItems();
        Division GetDivision();
        Platform GetPlatform();
    }
}