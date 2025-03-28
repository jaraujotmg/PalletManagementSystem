// src/PalletManagementSystem.Web/ViewModels/Shared/ErrorViewModel.cs
using System;
using PalletManagementSystem.Web2.ViewModels.Shared;

namespace PalletManagementSystem.Web2.ViewModels.Shared
{
    public class ErrorViewModel : ViewModelBase
    {
        public string ErrorMessage { get; set; }
        public string RequestedUrl { get; set; }
        public string ReferrerUrl { get; set; }
        public Exception Exception { get; set; }
        public string ErrorCode { get; set; }
    }
}