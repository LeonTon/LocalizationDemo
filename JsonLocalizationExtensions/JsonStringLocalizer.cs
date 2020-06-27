using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace JsonLocalizationExtensions
{
    internal class JsonStringLocalizer : IStringLocalizer
    {
        private readonly ConcurrentDictionary<string, Dictionary<string, string>> _resourcesCache = new ConcurrentDictionary<string, Dictionary<string, string>>();
        private readonly string _resourcesPath;
        private readonly string _resourceName;
        private readonly ILogger _logger;

        private string _resourceFileLocation;

        public JsonStringLocalizer(
            JsonLocalizationOptions localizationOptions,
            string resourceName,
            ILogger logger)
        {
            _resourceName = resourceName ?? throw new ArgumentNullException(nameof(resourceName));
            _logger = logger ?? NullLogger.Instance;
            _resourcesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, localizationOptions.ResourcesPath);
        }

        public LocalizedString this[string name]
        {
            get
            {
                if (name == null)
                {
                    throw new ArgumentNullException(nameof(name));
                }

                var value = GetStringSafely(name);
                return new LocalizedString(name, value ?? name, resourceNotFound: value == null, searchedLocation: _resourceFileLocation);
            }
        }

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                if (name == null)
                {
                    throw new ArgumentNullException(nameof(name));
                }

                var format = GetStringSafely(name);
                var value = string.Format(format ?? name, arguments);
                return new LocalizedString(name, value, resourceNotFound: format == null, searchedLocation: _resourceFileLocation);
            }
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) =>
            GetAllStrings(includeParentCultures, CultureInfo.CurrentUICulture);

        public IStringLocalizer WithCulture(CultureInfo culture) => this;

        private IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures, CultureInfo culture)
        {
            if (culture == null)
            {
                throw new ArgumentNullException(nameof(culture));
            }

            var resourceNames = includeParentCultures
                ? GetAllStringsFromCultureHierarchy(culture)
                : GetAllResourceStrings(culture);

            foreach (var name in resourceNames)
            {
                var value = GetStringSafely(name);
                yield return new LocalizedString(name, value ?? name, resourceNotFound: value == null, searchedLocation: _resourceFileLocation);
            }
        }

        private string GetStringSafely(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            var resources = GetResources(CultureInfo.CurrentUICulture.Name);

            var value = resources?.ContainsKey(name) ?? false ? resources[name] : null;

            return value;
        }

        private IEnumerable<string> GetAllStringsFromCultureHierarchy(CultureInfo startingCulture)
        {
            var currentCulture = startingCulture;
            var resourceNames = new HashSet<string>();

            while (currentCulture.Equals(currentCulture.Parent) == false)
            {
                var cultureResourceNames = GetAllResourceStrings(currentCulture);

                if (cultureResourceNames != null)
                {
                    foreach (var resourceName in cultureResourceNames)
                    {
                        resourceNames.Add(resourceName);
                    }
                }

                currentCulture = currentCulture.Parent;
            }

            return resourceNames;
        }

        private IEnumerable<string> GetAllResourceStrings(CultureInfo culture)
        {
            var resources = GetResources(culture.Name);
            return resources?.Select(r => r.Key);
        }

        private Dictionary<string, string> GetResources(string culture)
        {
            return _resourcesCache.GetOrAdd(culture, _ =>
            {
                var withCultureFileName = !string.IsNullOrWhiteSpace(_resourceName) ?
                string.Join(".", _resourceName.Replace('.', Path.DirectorySeparatorChar), $"{culture}.json")
                : "";

                var withoutCultrueFileName = !string.IsNullOrWhiteSpace(_resourceName) ?
                string.Join(".", _resourceName.Replace('.', Path.DirectorySeparatorChar), "json")
                : "";

                //default use with culture name resource  
                _resourceFileLocation = Path.Combine(_resourcesPath, withCultureFileName);

                //file is not exist use without culture name resource
                if (!File.Exists(_resourceFileLocation))
                {
                    _resourceFileLocation = Path.Combine(_resourcesPath, withoutCultrueFileName);
                }

                Dictionary<string, string> value = null;
                var content = File.ReadAllText(_resourceFileLocation, Encoding.UTF8);
                if (!string.IsNullOrWhiteSpace(content))
                {
                    try
                    {
                        value = JsonConvert.DeserializeObject<Dictionary<string, string>>(content.Trim());
                    }
                    catch (Exception e)
                    {
                        _logger.LogWarning(e, $"invalid json content, path: {_resourceFileLocation}, content: {content}");
                    }
                }

                return value;
            });
        }
    }
}
