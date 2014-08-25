// RequireJS.NET
// Copyright VeriTech.io
// http://veritech.io
// Dual licensed under the MIT and GPL licenses:
// http://www.opensource.org/licenses/mit-license.php
// http://www.gnu.org/licenses/gpl.html

using RequireJsNet.Compressor.Parsing;

namespace RequireJsNet.Compressor.Transformations
{
    internal class DepsToLowerTransformation : IRequireTransformation
    {
        public RequireCall RequireCall { get; set; }

        public static DepsToLowerTransformation Create(RequireCall requireCall)
        {
            return new DepsToLowerTransformation
            {
                RequireCall = requireCall
            };
        }

        public void Execute(ref string script)
        {
            var range = this.GetAffectedRange();

            var preDeps = script.Substring(0, range[0]);
            var postDeps = script.Substring(range[1], script.Length - range[1]);
            var deps = script.Substring(range[0], range[1] - range[0]).ToLower();

            script = preDeps + deps + postDeps;
        }

        public int[] GetAffectedRange()
        {
            return RequireCall.DependencyArrayNode.Range;
        }
    }
}
