// RequireJS.NET
// Copyright VeriTech.io
// http://veritech.io
// Dual licensed under the MIT and GPL licenses:
// http://www.opensource.org/licenses/mit-license.php
// http://www.gnu.org/licenses/gpl.html

using System.Collections.Generic;
using System.Linq;

using RequireJsNet.Models;

namespace RequireJsNet.Validation
{
    internal class ConfigValidator
    {
        private const string PathElement = "path";
        private const string ShimElement = "shim";
        private const string MapElement = "map";

        private static readonly string[] ComponentsToValidate = new string[]
        {
            PathElement,
            ShimElement,
            MapElement
        };  

        private readonly ConfigurationCollection collection;

        private readonly List<string> validatedComponents = new List<string>();

        private readonly List<ConfigValidationError> errors = new List<ConfigValidationError>();

        public ConfigValidator(ConfigurationCollection collection)
        {
            this.collection = collection;
        }

        public bool IsValid
        {
            get
            {
                if (ValidationComplete())
                {
                    return this.errors.Any();
                }

                Validate();
                return this.errors.Any();
            }
        }

        public List<ConfigValidationError> GetErrors()
        {
            if (ValidationComplete())
            {
                return this.errors;
            }

            Validate();
            return this.errors;
        }


        private bool ValidationComplete()
        {
            if (ComponentsToValidate.ToList().Except(this.validatedComponents).Any())
            {
                return false;
            }

            return true;
        }

        private void Validate()
        {
            ValidatePaths();
            this.validatedComponents.Add(PathElement);
            ValidateShims();
            this.validatedComponents.Add(ShimElement);
            ValidateMaps();
            this.validatedComponents.Add(MapElement);
        }

        private void ValidatePaths()
        {
            var paths = collection.Paths;
            var indexedPaths = paths.PathList.Select((r, i) => new
            {
                Index = i,
                Element = r
            });

            var entriesWithoutKeys = indexedPaths.Where(r => string.IsNullOrEmpty(r.Element.Key));
            foreach (var entry in entriesWithoutKeys)
            {
                this.errors.Add(new ConfigValidationError
                {
                    ElementKey = entry.Element.Key,
                    ElementProperty = "key",
                    ElementType = PathElement,
                    Index = entry.Index,
                    Message = "All paths must have a key defined"
                });
            }

            var entriesWithKeys = indexedPaths.Where(r => !string.IsNullOrEmpty(r.Element.Key));

            var doubledEntries = from path in entriesWithKeys
                group path by path.Element.Key into groupedPaths
                where groupedPaths.Count() > 1
                select new
                {
                    Key = groupedPaths.Key,
                    Indexes = groupedPaths.Select(r => r.Index),
                    Elements = groupedPaths.Select(r => r.Element)
                };
            foreach (var entry in doubledEntries)
            {
                this.errors.Add(new ConfigValidationError
                {
                    ElementKey = entry.Key,
                    ElementProperty = "key",
                    ElementType = PathElement,
                    Index = entry.Indexes.Last(),
                    Message = string.Format(
                                            "Duplicate key {0} was found at positions {1}", 
                                            entry.Key,
                                            string.Join(", ", entry.Indexes))
                });
            }

        }

        private void ValidateShims()
        {
            // TODO: validate
        }

        private void ValidateMaps()
        {
            // TODO: validate
        }
    }
}
