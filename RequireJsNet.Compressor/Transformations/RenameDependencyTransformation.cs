using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequireJsNet.Compressor.Transformations
{
    using RequireJsNet.Compressor.Parsing;

    internal class RenameDependencyTransformation : IRequireTransformation
    {
        public RequireCall RequireCall { get; set; }

        protected string OriginalName { get; set; }

        protected string NewName { get; set; }

        public static RenameDependencyTransformation Create(RequireCall requireCall, string originalName, string newName)
        {
            return new RenameDependencyTransformation
            {
                RequireCall = requireCall,
                NewName = newName,
                OriginalName = originalName
            };
        }

        public void Execute(ref string script)
        {
            throw new NotImplementedException();
        }

        public int[] GetAffectedRange()
        {
            return RequireCall.DependencyArrayNode.Range;
        }
    }
}
