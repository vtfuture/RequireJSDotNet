// RequireJS.NET
// Copyright VeriTech.io
// http://veritech.io
// Dual licensed under the MIT and GPL licenses:
// http://www.opensource.org/licenses/mit-license.php
// http://www.gnu.org/licenses/gpl.html

using System;
using System.Text;

namespace RequireJsNet.Compressor.Helpers
{
    internal static class FileHelpers
    {
        public static Encoding ParseEncoding(string encoding)
        {
            if (string.IsNullOrEmpty(encoding))
            {
                return Encoding.Default;
            }

            switch (encoding.ToLowerInvariant())
            {
                case "ascii":
                    return Encoding.ASCII;
                case "bigendianunicode":
                    return Encoding.BigEndianUnicode;
                case "unicode":
                    return Encoding.Unicode;
                case "utf32":
                case "utf-32":
                    return Encoding.UTF32;
                case "utf7":
                case "utf-7":
                    return Encoding.UTF7;
                case "utf8":
                case "utf-8":
                    return Encoding.UTF8;
                case "default":
                    return Encoding.Default;
                default:
                    throw new ArgumentException("Encoding: " + encoding + " is invalid.", "EncodingType");
            }
        }
    }
}
