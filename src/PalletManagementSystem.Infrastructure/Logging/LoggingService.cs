using Microsoft.Extensions.Logging;
using PalletManagementSystem.Infrastructure.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PalletManagementSystem.Infrastructure.Logging
{
    /// <summary>
    /// Service for application logging
    /// </summary>
    public class LoggingService
    {
        private readonly ILogger<LoggingService> _logger;
        private readonly UserContext _userContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingService"/> class
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="userContext">The user context</param>
        public LoggingService(ILogger<LoggingService> logger, UserContext userContext)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
        }

        /// <summary>
        /// Logs an information message with user context
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="properties">Additional properties</param>
        public void LogInfo(string message, Dictionary<string, object> properties = null)
        {
            try
            {
                var enrichedProperties = EnrichWithUserContext(properties);
                _logger.LogInformation(FormatMessage(message, enrichedProperties));
            }
            catch (Exception ex)
            {
                // Log the original message even if enrichment fails
                _logger.LogInformation(message);
                _logger.LogWarning(ex, "Error enriching log message with user context");
            }
        }

        /// <summary>
        /// Logs a warning message with user context
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="properties">Additional properties</param>
        public void LogWarning(string message, Dictionary<string, object> properties = null)
        {
            try
            {
                var enrichedProperties = EnrichWithUserContext(properties);
                _logger.LogWarning(FormatMessage(message, enrichedProperties));
            }
            catch (Exception ex)
            {
                // Log the original message even if enrichment fails
                _logger.LogWarning(message);
                _logger.LogWarning(ex, "Error enriching log message with user context");
            }
        }

        /// <summary>
        /// Logs an error message with user context
        /// </summary>
        /// <param name="exception">The exception</param>
        /// <param name="message">The message</param>
        /// <param name="properties">Additional properties</param>
        public void LogError(Exception exception, string message, Dictionary<string, object> properties = null)
        {
            try
            {
                var enrichedProperties = EnrichWithUserContext(properties);
                _logger.LogError(exception, FormatMessage(message, enrichedProperties));
            }
            catch
            {
                // Log the original message and exception even if enrichment fails
                _logger.LogError(exception, message);
            }
        }

        /// <summary>
        /// Logs an audit event with user context
        /// </summary>
        /// <param name="action">The action performed</param>
        /// <param name="entityType">The entity type</param>
        /// <param name="entityId">The entity ID</param>
        /// <param name="details">Additional details</param>
        public void LogAudit(string action, string entityType, string entityId, string details = null)
        {
            try
            {
                var properties = new Dictionary<string, object>
                {
                    { "Action", action },
                    { "EntityType", entityType },
                    { "EntityId", entityId }
                };

                if (!string.IsNullOrEmpty(details))
                {
                    properties.Add("Details", details);
                }

                var enrichedProperties = EnrichWithUserContext(properties);
                _logger.LogInformation(FormatMessage("AUDIT", enrichedProperties));
            }
            catch (Exception ex)
            {
                // Log a basic audit message even if enrichment fails
                _logger.LogInformation($"AUDIT: {action} {entityType} {entityId} by {_userContext.GetUsername()}");
                _logger.LogWarning(ex, "Error enriching audit log message with user context");
            }
        }

        /// <summary>
        /// Logs a security event with user context
        /// </summary>
        /// <param name="action">The security action</param>
        /// <param name="outcome">The outcome (success/failure)</param>
        /// <param name="details">Additional details</param>
        public void LogSecurity(string action, string outcome, string details = null)
        {
            try
            {
                var properties = new Dictionary<string, object>
                {
                    { "SecurityAction", action },
                    { "Outcome", outcome }
                };

                if (!string.IsNullOrEmpty(details))
                {
                    properties.Add("Details", details);
                }

                var enrichedProperties = EnrichWithUserContext(properties);
                _logger.LogInformation(FormatMessage("SECURITY", enrichedProperties));
            }
            catch (Exception ex)
            {
                // Log a basic security message even if enrichment fails
                _logger.LogInformation($"SECURITY: {action} {outcome} by {_userContext.GetUsername()}");
                _logger.LogWarning(ex, "Error enriching security log message with user context");
            }
        }

        /// <summary>
        /// Logs a performance event with user context
        /// </summary>
        /// <param name="operation">The operation performed</param>
        /// <param name="durationMs">The duration in milliseconds</param>
        /// <param name="details">Additional details</param>
        public void LogPerformance(string operation, long durationMs, string details = null)
        {
            try
            {
                var properties = new Dictionary<string, object>
                {
                    { "Operation", operation },
                    { "DurationMs", durationMs }
                };

                if (!string.IsNullOrEmpty(details))
                {
                    properties.Add("Details", details);
                }

                var enrichedProperties = EnrichWithUserContext(properties);
                _logger.LogInformation(FormatMessage("PERFORMANCE", enrichedProperties));
            }
            catch (Exception ex)
            {
                // Log a basic performance message even if enrichment fails
                _logger.LogInformation($"PERFORMANCE: {operation} took {durationMs}ms");
                _logger.LogWarning(ex, "Error enriching performance log message with user context");
            }
        }

        /// <summary>
        /// Creates a performance timer for logging operation duration
        /// </summary>
        /// <param name="operation">The operation name</param>
        /// <returns>A disposable timer that logs performance when disposed</returns>
        public IDisposable TimeOperation(string operation)
        {
            return new PerformanceTimer(this, operation);
        }

        #region Helper Methods

        /// <summary>
        /// Enriches properties with user context
        /// </summary>
        /// <param name="properties">The properties to enrich</param>
        /// <returns>The enriched properties</returns>
        private Dictionary<string, object> EnrichWithUserContext(Dictionary<string, object> properties)
        {
            var result = properties ?? new Dictionary<string, object>();

            // Add user context
            var username = _userContext.GetUsername();
            if (!string.IsNullOrEmpty(username))
            {
                result["Username"] = username;
            }

            // Add division and platform
            var division = _userContext.GetDivision();
            var platform = _userContext.GetPlatform();
            result["Division"] = division.ToString();
            result["Platform"] = platform.ToString();

            return result;
        }

        /// <summary>
        /// Formats a message with properties
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="properties">The properties</param>
        /// <returns>The formatted message</returns>
        private string FormatMessage(string message, Dictionary<string, object> properties)
        {
            if (properties == null || !properties.Any())
            {
                return message;
            }

            var sb = new StringBuilder(message);
            sb.Append(" {");

            var first = true;
            foreach (var property in properties)
            {
                if (!first)
                {
                    sb.Append(", ");
                }

                sb.Append(property.Key);
                sb.Append(": ");
                sb.Append(property.Value);

                first = false;
            }

            sb.Append("}");

            return sb.ToString();
        }

        #endregion

        #region PerformanceTimer

        /// <summary>
        /// Timer for logging operation performance
        /// </summary>
        private class PerformanceTimer : IDisposable
        {
            private readonly LoggingService _loggingService;
            private readonly string _operation;
            private readonly System.Diagnostics.Stopwatch _stopwatch;

            /// <summary>
            /// Initializes a new instance of the <see cref="PerformanceTimer"/> class
            /// </summary>
            /// <param name="loggingService">The logging service</param>
            /// <param name="operation">The operation name</param>
            public PerformanceTimer(LoggingService loggingService, string operation)
            {
                _loggingService = loggingService;
                _operation = operation;
                _stopwatch = System.Diagnostics.Stopwatch.StartNew();
            }

            /// <inheritdoc/>
            public void Dispose()
            {
                _stopwatch.Stop();
                _loggingService.LogPerformance(_operation, _stopwatch.ElapsedMilliseconds);
            }
        }

        #endregion
    }
}