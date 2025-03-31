// src/PalletManagementSystem.Infrastructure/Data/ValueConverters/PalletNumberConverter.cs
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PalletManagementSystem.Core.Models.Enums;
using PalletManagementSystem.Core.Models.ValueObjects;
using System;
using System.Collections.Generic; // Needed for List<string>
using System.Diagnostics;       // For Debug.WriteLine
using System.Text;              // Needed for StringBuilder

namespace PalletManagementSystem.Infrastructure.Data.ValueConverters
{
    public class PalletNumberConverter : ValueConverter<PalletNumber, string>
    {
        public PalletNumberConverter() : base(
            // Convert PalletNumber TO string for DB
            v => Serialize(v),
            // Convert string FROM DB to PalletNumber
            v => Deserialize(v))
        {
        }

        private static string Serialize(PalletNumber palletNumber)
        {
            if (palletNumber == null) return null; // Handle null input gracefully

            // Escapes pipes in the core value
            string escapedValue = palletNumber.Value?.Replace("|", "\\|") ?? string.Empty;
            // Creates the composite string: VALUE|ISTEMPORARY|DIVISION
            return $"{escapedValue}|{palletNumber.IsTemporary}|{palletNumber.Division}";
        }

        private static PalletNumber Deserialize(string dbValue)
        {
            // This method EXPECTS a string like "TEMP-001|True|MA" or "P812345|False|MA"
            try
            {
                if (string.IsNullOrEmpty(dbValue))
                {
                    Debug.WriteLine($"Warning: Cannot deserialize null or empty PalletNumber string from database.");
                    return null; // Return null if the DB value is null/empty
                }

                // Split the string by '|', handling escaped pipes '\'
                string[] parts = SplitEscaped(dbValue, '|');

                if (parts.Length != 3)
                {
                    Debug.WriteLine($"ERROR: Invalid PalletNumber format received from database: '{dbValue}'. Expected 3 parts, got {parts.Length}.");
                    // Decide how to handle invalid format. Returning null might hide data corruption.
                    // Throwing is often better during development. Let's throw for clarity.
                    throw new FormatException($"Invalid PalletNumber database format. Expected 3 parts separated by '|', but got {parts.Length} for value '{dbValue}'.");
                }

                // Part 0: Pallet Number Value
                string palletNumberValue = parts[0].Replace("\\|", "|");
                if (string.IsNullOrWhiteSpace(palletNumberValue))
                {
                    Debug.WriteLine($"ERROR: PalletNumber value part is empty after deserializing: '{dbValue}'");
                    throw new FormatException($"PalletNumber value part cannot be empty in database string '{dbValue}'.");
                }


                // --- MODIFIED PART ---
                // Part 1: Is Temporary Flag (Robust Parsing)
                // Trim whitespace, compare case-insensitively to "True" or check for "1". Default to false otherwise.
                bool isTemporary = parts[1].Trim().Equals("True", StringComparison.OrdinalIgnoreCase) || parts[1].Trim() == "1" || parts[0].StartsWith("T");
                // ---------------------


                // Part 2: Division Enum
                // Use TryParse for safer enum conversion, default to a specific value (e.g., MA) or throw on failure.
                if (!Enum.TryParse<Division>(parts[2], true, out Division division)) // true for case-insensitive
                {
                    Debug.WriteLine($"ERROR: Invalid Division value '{parts[2]}' found in database string: '{dbValue}'. Defaulting to MA.");
                    // Decide: Throw or default? Let's default for robustness as requested, but log clearly.
                    division = Division.MA; // Default to MA if parsing fails
                                            // Alternatively, throw:
                                            // throw new FormatException($"Invalid Division value '{parts[2]}' in database string '{dbValue}'.");
                }


                // Construct the PalletNumber object using the parsed values
                return new PalletNumber(palletNumberValue, isTemporary, division);
            }
            // Catch specific exceptions for better debugging, but now primarily for FormatExceptions or unexpected issues
            catch (FormatException formatEx)
            {
                Debug.WriteLine($"ERROR: FormatException during PalletNumber deserialization: '{dbValue}'. Message: {formatEx.Message}");
                // Rethrowing helps identify the root cause during development.
                // In production, you might return null or a default depending on requirements.
                throw new ArgumentException($"Error deserializing PalletNumber due to format issue: '{dbValue}'.", formatEx);
            }
            catch (Exception ex) // Catch-all for unexpected errors during construction or parsing
            {
                Debug.WriteLine($"ERROR: Unexpected exception during PalletNumber deserialization: '{dbValue}'. Message: {ex.Message}");
                // Rethrow or return null/default
                throw new ArgumentException($"Unexpected error deserializing PalletNumber: '{dbValue}'. See inner exception.", ex);
            }
        }

        // Helper function to split by delimiter, respecting '\' as an escape character
        private static string[] SplitEscaped(string input, char delimiter)
        {
            // Consider edge cases: null input, empty input
            if (input == null) return new string[0]; // Or handle as error?
            if (input == string.Empty) return new string[] { "" }; // Single empty part

            var result = new List<string>();
            var current = new StringBuilder();
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
                    // Do not append the escape char itself yet, wait for the next char
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
            // If the string ends with an escape character, it's arguably malformed.
            // Decide how to handle: append it? ignore it? throw?
            // Let's treat it as a literal backslash at the end for now.
            if (escaped) current.Append('\\');

            result.Add(current.ToString());
            return result.ToArray();
        }
    }
}