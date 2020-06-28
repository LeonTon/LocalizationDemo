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
        /// </summary>
        public string RootNamespace { get; set; }
    }
}
