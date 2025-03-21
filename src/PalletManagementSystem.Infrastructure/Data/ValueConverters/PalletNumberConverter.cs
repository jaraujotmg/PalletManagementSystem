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
            v => Serialize(v),

            // Convert from string when reading from database
            v => Deserialize(v))
        {
        }

        private static string Serialize(PalletNumber palletNumber)
        {
            // Escape any pipe characters in the value
            string escapedValue = palletNumber.Value.Replace("|", "\\|");
            return $"{escapedValue}|{palletNumber.IsTemporary}|{palletNumber.Division}";
        }

        private static PalletNumber Deserialize(string value)
        {
            try
            {
                // Split the string, but handle escaped pipes
                string[] parts = SplitEscaped(value, '|');

                if (parts.Length != 3)
                    throw new ArgumentException("Invalid PalletNumber format");

                // Unescape any escaped pipe characters
                string palletNumberValue = parts[0].Replace("\\|", "|");
                bool isTemporary = bool.Parse(parts[1]);
                Division division = (Division)Enum.Parse(typeof(Division), parts[2]);

                return new PalletNumber(palletNumberValue, isTemporary, division);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Error deserializing PalletNumber: {value}", ex);
            }
        }

        private static string[] SplitEscaped(string input, char delimiter)
        {
            var result = new System.Collections.Generic.List<string>();
            var current = new System.Text.StringBuilder();
            bool escaped = false;

            foreach (char c in input)
            {
                if (escaped)
                {
                    current.Append(c);
                    escaped = false;
                }
                else if (c == '\\')
                {
                    escaped = true;
                }
                else if (c == delimiter)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            result.Add(current.ToString());
            return result.ToArray();
        }
    }
}