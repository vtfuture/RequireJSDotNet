// RequireJS.NET
// Copyright VeriTech.io
// http://veritech.io
// Dual licensed under the MIT and GPL licenses:
// http://www.opensource.org/licenses/mit-license.php
// http://www.gnu.org/licenses/gpl.html

using System.Collections.Generic;
using System.Linq;

namespace RequireJsNet.Compressor.Parsing
{
    internal class VisitorResult
    {
        public VisitorResult()
        {
            RequireCalls = new List<RequireCall>();    
        }

        public List<RequireCall> RequireCalls { get; set; }

        public List<RequireCall> GetFlattened()
        {
            if (RequireCalls == null)
            {
                return null;
            }

            var result = new List<RequireCall>();

            var currentList = RequireCalls;
            currentList.ForEach(x => { result.Add(x); });   

            while (currentList != null && currentList.Any())
            {
                currentList = currentList.SelectMany(r => r.Children).ToList();
                currentList.ForEach(x => { result.Add(x); });    
            }

            return result;
        }
    }
}
