using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
