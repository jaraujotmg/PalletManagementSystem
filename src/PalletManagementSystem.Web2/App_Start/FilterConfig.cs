// src/PalletManagementSystem.Web/App_Start/FilterConfig.cs
using System.Web.Mvc;

namespace PalletManagementSystem.Web2.App_Start
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}