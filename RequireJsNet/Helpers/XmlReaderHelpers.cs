using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace RequireJsNet.Helpers
{
    public enum AttributeReadType
    {
        Mandatory,
        Optional
    }

    internal static class XmlReaderHelpers
    {
        public static string ReadStringAttribute(this XElement element, string attributeName, AttributeReadType readType = AttributeReadType.Mandatory)
        {
            if (element == null)
            {
                if (readType == AttributeReadType.Mandatory)
                {
                    throw new ArgumentNullException("element");    
                }

                return string.Empty;
            }

            var attribute = element.Attribute(attributeName);

            if (attribute == null)
            {
                if (readType == AttributeReadType.Mandatory)
                {
                    throw new NullReferenceException(string.Format("Attribute {0} is null.", attributeName));
                }

                return string.Empty;
            }

            return attribute.Value;
        }


        public static bool? ReadBooleanAttribute(this XElement element, string attributeName, AttributeReadType readType = AttributeReadType.Mandatory)
        {
            var stringVal = element.ReadStringAttribute(attributeName, readType);
            if (string.IsNullOrEmpty(stringVal) && readType == AttributeReadType.Optional)
            {
                return null;
            }

            return Convert.ToBoolean(stringVal);
        }

        public static List<string> ReadStringListAttribute(this XElement element, string attributeName, AttributeReadType readType = AttributeReadType.Mandatory)
        {
            var stringVal = element.ReadStringAttribute(attributeName, readType);
            if (string.IsNullOrEmpty(stringVal) && readType == AttributeReadType.Optional)
            {
                return null;
            }

            var result = stringVal.Split(',').Select(r => r.Trim()).Distinct().ToList();
            return result;
        }
    }
}
