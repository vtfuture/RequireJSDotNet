using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RequireJsNet.Models;
using RequireJsNet.Validation;

namespace RequireJsNet
{
    internal class ConfigValidator
    {
        private const string PathElement = "path";
        private const string ShimElement = "shim";
        private const string MapElement = "map";

        private readonly ConfigurationCollection collection;

        public ConfigValidator(ConfigurationCollection collection)
        {
            this.collection = collection;
        }

        private static readonly string[] ComponentsToValidate = new string[]
        {
            PathElement,
            ShimElement,
            MapElement
        };  
        private readonly List<string> ValidatedComponents = new List<string>();  
        private readonly List<ConfigValidationError> Errors = new List<ConfigValidationError>();

        public bool IsValid
        {
            get
            {
                if (ValidationComplete())
                {
                    return Errors.Any();
                }
                Validate();
                return Errors.Any();
            }
        }

        public List<ConfigValidationError> GetErrors()
        {
            if (ValidationComplete())
            {
                return Errors;
            }
            Validate();
            return Errors;
        }


        private bool ValidationComplete()
        {
            if (ComponentsToValidate.ToList().Except(ValidatedComponents).Any())
            {
                return false;
            }
            return true;
        }

        private void Validate()
        {
            ValidatePaths();
            ValidatedComponents.Add(PathElement);
            ValidateShims();
            ValidatedComponents.Add(ShimElement);
            ValidateMaps();
            ValidatedComponents.Add(MapElement);
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
                Errors.Add(new ConfigValidationError
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
                Errors.Add(new ConfigValidationError
                {
                    ElementKey = entry.Key,
                    ElementProperty = "key",
                    ElementType = PathElement,
                    Index = entry.Indexes.Last(),
                    Message = string.Format("Duplicate key {0} was found at positions {1}", 
                                                entry.Key,
                                                String.Join(", ", entry.Indexes))
                });
            }

        }

        private void ValidateShims()
        {
            //TODO: validate
        }

        private void ValidateMaps()
        {
            //TODO: validate
        }

    }
}
