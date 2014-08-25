using System.Collections.Generic;
using System.Linq;

namespace RequireJsNet.Compressor.Transformations
{
    internal class TransformationCollection
    {
        public TransformationCollection()
        {
            Transformations = new List<IRequireTransformation>();
        }

        private List<IRequireTransformation> Transformations { get; set; }

        public void Add(IRequireTransformation transformation)
        {
            Transformations.Add(transformation);
        }

        public void ExecuteAll(ref string script)
        {
            // order all transformations from right to left so that we won't mess up indexes
            // since they shouldn't interesect, it won't matter if we compare the starting or the ending index
            var ordered = Transformations.OrderByDescending(r => r.GetAffectedRange()[0]);
            foreach (var transformation in ordered)
            {
                transformation.Execute(ref script);    
            }
        }
    }
}
