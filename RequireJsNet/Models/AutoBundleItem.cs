// RequireJS.NET
// Copyright VeriTech.io
// http://veritech.io
// Dual licensed under the MIT and GPL licenses:
// http://www.opensource.org/licenses/mit-license.php
// http://www.gnu.org/licenses/gpl.html

namespace RequireJsNet.Models
{
    public class AutoBundleItem
    {
        public string Directory { get; set; }

        public string File { get; set; }

        public string BundleId { get; set; }

        public override string ToString()
        {
            return ToStringIfSet(Directory, nameof(Directory)) + ToStringIfSet(File, nameof(File)) + ToStringIfSet(BundleId, nameof(BundleId));
        }

        /// <summary>
        /// Returns <paramref name="name"/>: <paramref name="condition"/> if <paramref name="condition"/> is non-empty.
        /// </summary>
        /// <param name="condition">condition to check</param>
        /// <param name="name">name of condition</param>
        /// <returns><paramref name="name"/>: <paramref name="condition"/> and string.Empty</returns>
        private string ToStringIfSet(string condition, string name)
        {
            if (string.IsNullOrEmpty(condition))
                return string.Empty;
            else
                return $"{name}: {condition} ";
        }
    }
}
