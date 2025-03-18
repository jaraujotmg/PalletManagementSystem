using System;
using PalletManagementSystem.Core.Models.Enums;

namespace PalletManagementSystem.Core.Models.ValueObjects
{
    /// <summary>
    /// Value object representing a pallet number
    /// </summary>
    public class PalletNumber : IEquatable<PalletNumber>
    {
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
            //string value = division switch
            //{
            //    Division.MA => $"P8{sequenceNumber:00000}",  // Manufacturing uses P8 prefix
            //    Division.TC => $"47{sequenceNumber:00000}",  // Technical Center uses 47 prefix
            //    _ => throw new ArgumentException($"Unsupported division: {division}", nameof(division))
            //};

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

        /// <summary>
        /// Helper class for C# 7.3 HashCode calculation
        /// </summary>
        private static class HashCode
        {
            /// <summary>
            /// Combines hash codes
            /// </summary>
            /// <param name="obj1">First object</param>
            /// <param name="obj2">Second object</param>
            /// <param name="obj3">Third object</param>
            /// <returns>Combined hash code</returns>
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