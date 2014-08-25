// RequireJS.NET
// Copyright VeriTech.io
// http://veritech.io
// Dual licensed under the MIT and GPL licenses:
// http://www.opensource.org/licenses/mit-license.php
// http://www.gnu.org/licenses/gpl.html

using RequireJsNet.Compressor.Parsing;

namespace RequireJsNet.Compressor.Transformations
{
    internal interface IRequireTransformation
    {
        RequireCall RequireCall { get; set; }

        void Execute(ref string script);

        int[] GetAffectedRange();
    }
}