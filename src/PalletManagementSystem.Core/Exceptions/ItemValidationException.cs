using System;

namespace PalletManagementSystem.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when item validation fails
    /// </summary>
    public class ItemValidationException : DomainException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemValidationException"/> class
        /// </summary>
        public ItemValidationException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemValidationException"/> class with a message
        /// </summary>
        /// <param name="message">The error message</param>
        public ItemValidationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemValidationException"/> class with a message and inner exception
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="innerException">The inner exception</param>
        public ItemValidationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}