using System;
using System.Collections.Generic;
using System.Text;

namespace JsonLocalizationExtensions
{
    public class JsonLocalizationOptions
    {
        /// <summary>
        /// The relative path under application root where resource files are located.
        /// </summary>
        public string ResourcesPath { get; set; } = "Resources";

        /// <summary>
        /// RootNamespace
        /// use entry assembly name by default
        /// </summary>
        public string RootNamespace { get; set; }
    }
}
