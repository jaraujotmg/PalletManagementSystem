using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace PalletManagementSystem.Core.Extensions
{
    /// <summary>
    /// Extension methods for enums to work with Description attributes
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Gets the description attribute value for an enum value
        /// </summary>
        /// <typeparam name="T">The enum type</typeparam>
        /// <param name="value">The enum value</param>
        /// <returns>The description or the enum name if no description is found</returns>
        public static string GetDescription<T>(this T value) where T : Enum
        {
            var field = value.GetType().GetField(value.ToString());

            if (field == null)
                return value.ToString();

            var attribute = field.GetCustomAttribute<DescriptionAttribute>();
            return attribute?.Description ?? value.ToString();
        }

        /// <summary>
        /// Gets the enum value from a description
        /// </summary>
        /// <typeparam name="T">The enum type</typeparam>
        /// <param name="description">The description to look for</param>
        /// <returns>The enum value or default if not found</returns>
        public static T GetValueFromDescription<T>(string description) where T : Enum
        {
            foreach (var field in typeof(T).GetFields())
            {
                if (field.GetCustomAttribute(typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
                {
                    if (attribute.Description == description)
                        return (T)field.GetValue(null);
                }
                else if (field.Name == description)
                {
                    return (T)field.GetValue(null);
                }
            }

            // If no match is found, try to directly parse the enum
            if (Enum.TryParse(typeof(T), description, out object result))
            {
                return (T)result;
            }

            throw new ArgumentException($"'{description}' is not a valid description or value for {typeof(T).Name}");
        }

        /// <summary>
        /// Gets all values of an enum
        /// </summary>
        /// <typeparam name="T">The enum type</typeparam>
        /// <returns>Array of all enum values</returns>
        public static T[] GetAllValues<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T)).Cast<T>().ToArray();
        }

        /// <summary>
        /// Gets all descriptions for an enum type
        /// </summary>
        /// <typeparam name="T">The enum type</typeparam>
        /// <returns>Array of all descriptions</returns>
        public static string[] GetAllDescriptions<T>() where T : Enum
        {
            return GetAllValues<T>().Select(v => v.GetDescription()).ToArray();
        }
    }
}