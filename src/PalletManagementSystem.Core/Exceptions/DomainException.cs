using System;

namespace PalletManagementSystem.Core.Exceptions
{
    /// <summary>
    /// Base exception for all domain-specific exceptions
    /// </summary>
    public class DomainException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DomainException"/> class
        /// </summary>
        public DomainException() : base() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainException"/> class with a message
        /// </summary>
        /// <param name="message">The error message</param>
        public DomainException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainException"/> class with a message and inner exception
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="innerException">The inner exception</param>
        public DomainException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when attempting to modify a closed pallet
    /// </summary>
    public class PalletClosedException : DomainException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PalletClosedException"/> class
        /// </summary>
        public PalletClosedException() : base("Operation cannot be performed on a closed pallet") { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PalletClosedException"/> class with a message
        /// </summary>
        /// <param name="message">The error message</param>
        public PalletClosedException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PalletClosedException"/> class with a message and inner exception
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="innerException">The inner exception</param>
        public PalletClosedException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when item validation fails
    /// </summary>
    public class ItemValidationException : DomainException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemValidationException"/> class
        /// </summary>
        public ItemValidationException() : base("Item validation failed") { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemValidationException"/> class with a message
        /// </summary>
        /// <param name="message">The error message</param>
        public ItemValidationException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemValidationException"/> class with a message and inner exception
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="innerException">The inner exception</param>
        public ItemValidationException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when pallet validation fails
    /// </summary>
    public class PalletValidationException : DomainException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PalletValidationException"/> class
        /// </summary>
        public PalletValidationException() : base("Pallet validation failed") { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PalletValidationException"/> class with a message
        /// </summary>
        /// <param name="message">The error message</param>
        public PalletValidationException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PalletValidationException"/> class with a message and inner exception
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="innerException">The inner exception</param>
        public PalletValidationException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when an entity is not found
    /// </summary>
    public class EntityNotFoundException : DomainException
    {
        /// <summary>
        /// Gets the ID of the entity that was not found
        /// </summary>
        public object EntityId { get; }

        /// <summary>
        /// Gets the type of the entity that was not found
        /// </summary>
        public string EntityType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityNotFoundException"/> class
        /// </summary>
        /// <param name="entityType">The type of entity</param>
        /// <param name="entityId">The ID of the entity</param>
        public EntityNotFoundException(string entityType, object entityId)
            : base($"{entityType} with ID {entityId} was not found")
        {
            EntityType = entityType;
            EntityId = entityId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityNotFoundException"/> class with a message
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="entityType">The type of entity</param>
        /// <param name="entityId">The ID of the entity</param>
        public EntityNotFoundException(string message, string entityType, object entityId)
            : base(message)
        {
            EntityType = entityType;
            EntityId = entityId;
        }
    }
}