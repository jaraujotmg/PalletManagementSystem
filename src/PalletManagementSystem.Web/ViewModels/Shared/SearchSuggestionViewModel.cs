// src/PalletManagementSystem.Web/ViewModels/Shared/SearchSuggestionViewModel.cs
using System.Collections.Generic;
using PalletManagementSystem.Core.DTOs;

namespace PalletManagementSystem.Web.ViewModels.Shared
{
    public class SearchSuggestionViewModel
    {
        public List<SearchSuggestionDto> Suggestions { get; set; } = new List<SearchSuggestionDto>();
        public string Keyword { get; set; }
        public string ActionUrl { get; set; }
    }
}