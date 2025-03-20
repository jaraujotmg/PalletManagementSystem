using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PalletManagementSystem.Core.Models.Enums;
using PalletManagementSystem.Core.Models.ValueObjects;
using System;

namespace PalletManagementSystem.Infrastructure.Data.ValueConverters
{
    /// <summary>
    /// Value converter for PalletNumber value object
    /// </summary>
    public class PalletNumberConverter : ValueConverter<PalletNumber, string>
    {
        public PalletNumberConverter() : base(
            // Convert to string when saving to database
            v => $"{v.Value}|{v.IsTemporary}|{v.Division}",
            // Convert from string when reading from database
            v =>
            {
                var parts = v.Split('|');
                if (parts.Length != 3)
                    throw new ArgumentException("Invalid PalletNumber format");

                var value = parts[0];
                var isTemporary = bool.Parse(parts[1]);
                var division = Enum.Parse<Division>(parts[2]);

                return new PalletNumber(value, isTemporary, division);
            })
        {
        }
    }
}