using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NewsletterApp.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NewsletterApp.Infrastructure.Services
{
    /// <summary>
    /// Email Template Service implementation
    /// Implements Template Method pattern for dynamic email template rendering
    /// Follows Open/Closed Principle - open for extension (new templates) without modifying existing code
    /// </summary>
    public class EmailTemplateService : IEmailTemplateService
    {
        private readonly string _templatesDirectory;
        private readonly ILogger<EmailTemplateService> _logger;
        private readonly Dictionary<string, string> _templateCache;
        private static readonly object _cacheLock = new object();

        // Template mapping for interests
        private static readonly Dictionary<string, string> InterestTemplateMap = new(StringComparer.OrdinalIgnoreCase)
        {
            { "Houses", "HousesNewsletter" },
            { "Apartments", "ApartmentsNewsletter" },
            { "SharedOwnership", "SharedOwnershipNewsletter" },
            { "Rental", "RentalNewsletter" },
            { "LandSourcing", "LandSourcingNewsletter" }
        };

        // Template mapping for subscriber types
        private static readonly Dictionary<string, string> SubscriberTypeTemplateMap = new(StringComparer.OrdinalIgnoreCase)
        {
            { "HomeBuilder", "HomeBuilderNewsletter" },
            { "HomeBuyer", "HomeBuyerNewsletter" }
        };

        public EmailTemplateService(IConfiguration configuration, ILogger<EmailTemplateService> logger)
        {
            _logger = logger;
            _templateCache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            // Get templates directory from configuration or use default
            var templatesPath = configuration["EmailSettings:TemplatesPath"];
            if (string.IsNullOrEmpty(templatesPath))
            {
                var env = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production";
                var basePath = AppDomain.CurrentDomain.BaseDirectory;
                
                // In development, we might want to look at the project folder, 
                // but for consistency with static files, we use wwwroot
                _templatesDirectory = Path.Combine(basePath, "wwwroot", "templates", "email");
                
                // If it doesn't exist relative to execution dir (typical in dev), try to find it in the API project dir
                if (!Directory.Exists(_templatesDirectory))
                {
                    _templatesDirectory = Path.Combine(basePath, "..", "..", "..", "API", "wwwroot", "templates", "email");
                }
            }
            else
            {
                _templatesDirectory = templatesPath;
            }

            _templatesDirectory = Path.GetFullPath(_templatesDirectory);
            _logger.LogInformation("Email templates directory: {Directory}", _templatesDirectory);
        }

        public IEnumerable<string> GetAvailableTemplates()
        {
            try
            {
                if (!Directory.Exists(_templatesDirectory))
                {
                    _logger.LogWarning("Templates directory does not exist: {Directory}", _templatesDirectory);
                    return Enumerable.Empty<string>();
                }

                return Directory.GetFiles(_templatesDirectory, "*.html")
                    .Select(f => Path.GetFileNameWithoutExtension(f))
                    .OrderBy(n => n);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available templates");
                return Enumerable.Empty<string>();
            }
        }

        public async Task<string> RenderTemplateAsync(string templateName, Dictionary<string, string> context)
        {
            if (string.IsNullOrWhiteSpace(templateName))
                throw new ArgumentException("Template name is required", nameof(templateName));

            string templateContent = await LoadTemplateAsync(templateName);

            // Replace placeholders with actual values
            foreach (var kvp in context)
            {
                var placeholder = $"{{{{{kvp.Key}}}}}";
                templateContent = templateContent.Replace(placeholder, kvp.Value ?? string.Empty);
            }

            return templateContent;
        }

        public string GetTemplateNameForInterest(string interest)
        {
            if (string.IsNullOrWhiteSpace(interest))
                return "GenericNewsletter";

            // Clean the interest value
            var cleanInterest = NormalizeInterestKey(interest);

            // Try exact match first
            if (InterestTemplateMap.TryGetValue(cleanInterest, out var templateName))
            {
                return templateName;
            }

            // Try fallback by checking normalized keys in dictionary
            foreach (var kvp in InterestTemplateMap)
            {
                if (NormalizeInterestKey(kvp.Key).Equals(cleanInterest, StringComparison.OrdinalIgnoreCase))
                {
                    return kvp.Value;
                }
            }

            _logger.LogInformation("No specific template for interest '{Interest}', using generic", interest);
            return "GenericNewsletter";
        }

        private static string NormalizeInterestKey(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            // Remove whitespace and non-alphanumeric characters to create a normalized key
            var chars = input.Where(c => char.IsLetterOrDigit(c)).ToArray();
            return new string(chars);
        }

        public string GetTemplateNameForSubscriberType(string subscriberType)
        {
            if (string.IsNullOrWhiteSpace(subscriberType))
                return "GenericNewsletter";

            var cleanType = subscriberType.Trim();

            if (SubscriberTypeTemplateMap.TryGetValue(cleanType, out var templateName))
            {
                return templateName;
            }

            _logger.LogInformation("No specific template for subscriber type '{Type}', using generic", subscriberType);
            return "GenericNewsletter";
        }

        public bool TemplateExists(string templateName)
        {
            if (string.IsNullOrWhiteSpace(templateName))
                return false;

            var templatePath = Path.Combine(_templatesDirectory, $"{templateName}.html");
            return File.Exists(templatePath);
        }

        public string GetBestTemplateName(string explicitTemplate, string subscriberType, IEnumerable<string> interests)
        {
            // Priority 1: explicit template
            if (!string.IsNullOrWhiteSpace(explicitTemplate) && TemplateExists(explicitTemplate))
            {
                return explicitTemplate;
            }

            // Priority 2: subscriber type mapping
            if (!string.IsNullOrWhiteSpace(subscriberType))
            {
                var typeTemplate = GetTemplateNameForSubscriberType(subscriberType);
                if (TemplateExists(typeTemplate))
                {
                    return typeTemplate;
                }
            }

            // Priority 3: try interests (prefer the first matching mapped interest)
            if (interests != null)
            {
                foreach (var interest in interests)
                {
                    if (string.IsNullOrWhiteSpace(interest)) continue;
                    var interestTemplate = GetTemplateNameForInterest(interest);
                    if (TemplateExists(interestTemplate))
                    {
                        return interestTemplate;
                    }
                }
            }

            // Fallback to generic
            return "GenericNewsletter";
        }

        private async Task<string> LoadTemplateAsync(string templateName)
        {
            // Check cache first
            lock (_cacheLock)
            {
                if (_templateCache.TryGetValue(templateName, out var cachedContent))
                {
                    return cachedContent;
                }
            }

            var templatePath = Path.Combine(_templatesDirectory, $"{templateName}.html");

            if (!File.Exists(templatePath))
            {
                _logger.LogWarning("Template not found: {TemplateName}. Falling back to generic.", templateName);
                templatePath = Path.Combine(_templatesDirectory, "GenericNewsletter.html");
                
                if (!File.Exists(templatePath))
                {
                    throw new FileNotFoundException($"Neither '{templateName}' nor fallback 'GenericNewsletter' template found");
                }
            }

            var content = await File.ReadAllTextAsync(templatePath);

            // Cache the template
            lock (_cacheLock)
            {
                _templateCache[templateName] = content;
            }

            return content;
        }
    }
}
