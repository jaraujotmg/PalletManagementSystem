using System;
using System.Text.RegularExpressions;
using PalletManagementSystem.Core.Models.Enums;

namespace PalletManagementSystem.Core.Models.ValueObjects
{
    /// <summary>
    /// Value object representing a pallet number
    /// </summary>
    public class PalletNumber : IEquatable<PalletNumber>
    {
        private static readonly Regex TemporaryPattern = new Regex(@"^TEMP\d{3}$", RegexOptions.Compiled);
        private static readonly Regex ManufacturingPattern = new Regex(@"^P8\d{6}$", RegexOptions.Compiled);
        private static readonly Regex TechnicalCenterPattern = new Regex(@"^47\d{6}$", RegexOptions.Compiled);

        /// <summary>
        /// Gets the value of the pallet number
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Gets a value indicating whether this is a temporary pallet number
        /// </summary>
        public bool IsTemporary { get; }

        /// <summary>
        /// Gets the division this pallet number belongs to
        /// </summary>
        public Division Division { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PalletNumber"/> class
        /// </summary>
        /// <param name="value">The pallet number value</param>
        /// <param name="isTemporary">Indicates if this is a temporary number</param>
        /// <param name="division">The division this pallet belongs to</param>
        public PalletNumber(string value, bool isTemporary, Division division)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Pallet number cannot be null or empty", nameof(value));
            }

            // Validate pallet number format
            if (!ValidateFormat(value, isTemporary, division))
            {
                throw new ArgumentException($"Invalid pallet number format: {value} for division {division} and isTemporary={isTemporary}", nameof(value));
            }

            Value = value;
            IsTemporary = isTemporary;
            Division = division;
        }

        /// <summary>
        /// Creates a new temporary pallet number
        /// </summary>
        /// <param name="sequenceNumber">The sequence number to use</param>
        /// <param name="division">The division</param>
        /// <returns>A new temporary pallet number</returns>
        public static PalletNumber CreateTemporary(int sequenceNumber, Division division)
        {
            if (sequenceNumber <= 0)
            {
                throw new ArgumentException("Sequence number must be positive", nameof(sequenceNumber));
            }

            return new PalletNumber($"TEMP-{sequenceNumber:000}", true, division);
        }

        /// <summary>
        /// Creates a permanent pallet number based on division format rules
        /// </summary>
        /// <param name="sequenceNumber">The sequence number</param>
        /// <param name="division">The division</param>
        /// <returns>A new permanent pallet number</returns>
        public static PalletNumber CreatePermanent(int sequenceNumber, Division division)
        {
            if (sequenceNumber <= 0)
            {
                throw new ArgumentException("Sequence number must be positive", nameof(sequenceNumber));
            }

            string value;
            switch (division)
            {
                case Division.MA:
                    value = $"P8{sequenceNumber:00000}";  // Manufacturing uses P8 prefix
                    break;
                case Division.TC:
                    value = $"47{sequenceNumber:00000}";  // Technical Center uses 47 prefix
                    break;
                default:
                    throw new ArgumentException($"Unsupported division: {division}", nameof(division));
            }

            return new PalletNumber(value, false, division);
        }

        /// <summary>
        /// Validates the format of a pallet number
        /// </summary>
        /// <param name="value">The pallet number to validate</param>
        /// <param name="isTemporary">Whether it's a temporary number</param>
        /// <param name="division">The division</param>
        /// <returns>True if the format is valid, false otherwise</returns>
        private bool ValidateFormat(string value, bool isTemporary, Division division)
        {
            if (isTemporary)
            {
                return TemporaryPattern.IsMatch(value);
            }

            switch (division)
            {
                case Division.MA:
                    return ManufacturingPattern.IsMatch(value);
                case Division.TC:
                    return TechnicalCenterPattern.IsMatch(value);
                default:
                    return false;
            }
        }

        /// <summary>
        /// Try to parse a pallet number string into a PalletNumber object
        /// </summary>
        /// <param name="value">The string value to parse</param>
        /// <param name="palletNumber">The resulting pallet number if successful</param>
        /// <returns>True if parsing was successful, false otherwise</returns>
        public static bool TryParse(string value, out PalletNumber palletNumber)
        {
            palletNumber = null;

            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            // Check for temporary number format
            if (TemporaryPattern.IsMatch(value))
            {
                // Temporary numbers can be used for any division
                palletNumber = new PalletNumber(value, true, Division.MA);
                return true;
            }

            // Check for MA division format
            if (ManufacturingPattern.IsMatch(value))
            {
                palletNumber = new PalletNumber(value, false, Division.MA);
                return true;
            }

            // Check for TC division format
            if (TechnicalCenterPattern.IsMatch(value))
            {
                palletNumber = new PalletNumber(value, false, Division.TC);
                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Value;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return Equals(obj as PalletNumber);
        }

        /// <inheritdoc/>
        public bool Equals(PalletNumber other)
        {
            if (other is null)
            {
                return false;
            }

            return Value == other.Value &&
                   IsTemporary == other.IsTemporary &&
                   Division == other.Division;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(Value, IsTemporary, Division);
        }

        /// <summary>
        /// Determines whether two pallet numbers are equal
        /// </summary>
        /// <param name="left">The first pallet number</param>
        /// <param name="right">The second pallet number</param>
        /// <returns>True if equal, false otherwise</returns>
        public static bool operator ==(PalletNumber left, PalletNumber right)
        {
            if (left is null)
            {
                return right is null;
            }

            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two pallet numbers are not equal
        /// </summary>
        /// <param name="left">The first pallet number</param>
        /// <param name="right">The second pallet number</param>
        /// <returns>True if not equal, false otherwise</returns>
        public static bool operator !=(PalletNumber left, PalletNumber right)
        {
            return !(left == right);
        }

        // HashCode helper for C# < 8.0
        private static class HashCode
        {
            public static int Combine(object obj1, object obj2, object obj3)
            {
                unchecked
                {
                    int hash = 17;
                    hash = hash * 23 + (obj1?.GetHashCode() ?? 0);
                    hash = hash * 23 + (obj2?.GetHashCode() ?? 0);
                    hash = hash * 23 + (obj3?.GetHashCode() ?? 0);
                    return hash;
                }
            }
        }
    }
}