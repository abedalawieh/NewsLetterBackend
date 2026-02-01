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
    public class EmailTemplateService : IEmailTemplateService
    {
        private readonly string _templatesDirectory;
        private readonly ILogger<EmailTemplateService> _logger;
        private readonly Dictionary<string, string> _templateCache;
        private static readonly object _cacheLock = new object();

        private static readonly Dictionary<string, string> InterestTemplateMap = new(StringComparer.OrdinalIgnoreCase)
        {
            { "Houses", "HousesNewsletter" },
            { "Apartments", "ApartmentsNewsletter" },
            { "SharedOwnership", "SharedOwnershipNewsletter" },
            { "Rental", "RentalNewsletter" },
            { "LandSourcing", "LandSourcingNewsletter" }
        };


        public EmailTemplateService(IConfiguration configuration, ILogger<EmailTemplateService> logger)
        {
            _logger = logger;
            _templateCache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var templatesPath = configuration["EmailSettings:TemplatesPath"];
            if (string.IsNullOrEmpty(templatesPath))
            {
                var env = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production";
                var basePath = AppDomain.CurrentDomain.BaseDirectory;
                
                _templatesDirectory = Path.Combine(basePath, "wwwroot", "templates", "email");
                
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

            var cleanInterest = NormalizeInterestKey(interest);

            if (InterestTemplateMap.TryGetValue(cleanInterest, out var templateName))
            {
                return templateName;
            }

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
            var chars = input.Where(c => char.IsLetterOrDigit(c)).ToArray();
            return new string(chars);
        }


        public bool TemplateExists(string templateName)
        {
            if (string.IsNullOrWhiteSpace(templateName))
                return false;

            var templatePath = Path.Combine(_templatesDirectory, $"{templateName}.html");
            return File.Exists(templatePath);
        }

        public string GetBestTemplateName(string explicitTemplate, IEnumerable<string> interests)
        {
            if (!string.IsNullOrWhiteSpace(explicitTemplate) && TemplateExists(explicitTemplate))
            {
                return explicitTemplate;
            }

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

            return "GenericNewsletter";
        }

        private async Task<string> LoadTemplateAsync(string templateName)
        {
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

            lock (_cacheLock)
            {
                _templateCache[templateName] = content;
            }

            return content;
        }
    }
}
