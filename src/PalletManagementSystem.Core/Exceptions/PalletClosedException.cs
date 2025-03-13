using System;

namespace PalletManagementSystem.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when trying to modify a closed pallet
    /// </summary>
    public class PalletClosedException : DomainException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PalletClosedException"/> class
        /// </summary>
        public PalletClosedException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PalletClosedException"/> class with a message
        /// </summary>
        /// <param name="message">The error message</param>
        public PalletClosedException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PalletClosedException"/> class with a message and inner exception
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="innerException">The inner exception</param>
        public PalletClosedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}