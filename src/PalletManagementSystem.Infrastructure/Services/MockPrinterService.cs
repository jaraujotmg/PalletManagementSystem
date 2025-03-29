// src/PalletManagementSystem.Infrastructure/Services/MockPrinterService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PalletManagementSystem.Core.Interfaces.Services;
using PalletManagementSystem.Core.Models.Enums;

namespace PalletManagementSystem.Infrastructure.Services
{
    public class MockPrinterService : IPrinterService
    {
        public async Task<bool> PrintPalletListAsync(int palletId)
        {
            // Just simulate success
            return true;
        }

        public async Task<bool> PrintItemLabelAsync(int itemId)
        {
            // Just simulate success
            return true;
        }

        public async Task<string> GetPalletListPrinterAsync(Division division, Platform platform, bool hasSpecialClient)
        {
            // Return a dummy printer name
            return "MockPalletListPrinter";
        }

        public async Task<string> GetItemLabelPrinterAsync(Division division, Platform platform, bool isSpecialClient)
        {
            // Return a dummy printer name
            return "MockItemLabelPrinter";
        }

        public async Task<bool> SetDefaultPalletListPrinterAsync(string username, string printerName)
        {
            // Just simulate success
            return true;
        }

        public async Task<bool> SetDefaultItemLabelPrinterAsync(string username, string printerName)
        {
            // Just simulate success
            return true;
        }

        public async Task<string> GetDefaultPalletListPrinterAsync(string username)
        {
            // Return a dummy printer name
            return "DefaultPalletListPrinter";
        }

        public async Task<string> GetDefaultItemLabelPrinterAsync(string username)
        {
            // Return a dummy printer name
            return "DefaultItemLabelPrinter";
        }

        public async Task<IEnumerable<string>> GetAvailablePrintersAsync(PrinterType printerType)
        {
            // Return some dummy printer names
            var printers = new List<string>();

            switch (printerType)
            {
                case PrinterType.PalletList:
                    printers.Add("PalletListPrinter1");
                    printers.Add("PalletListPrinter2");
                    printers.Add("SharedPrinter");
                    break;
                case PrinterType.ItemLabel:
                    printers.Add("LabelPrinter1");
                    printers.Add("LabelPrinter2");
                    printers.Add("SharedPrinter");
                    break;
            }

            return printers;
        }
    }
}