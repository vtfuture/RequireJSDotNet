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
        public object TransformationData { get; set; }

        public RequireCall RequireCall { get; set; }

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
