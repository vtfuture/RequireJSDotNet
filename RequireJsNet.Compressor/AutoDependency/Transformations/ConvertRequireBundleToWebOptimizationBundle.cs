using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Optimization;

namespace RequireJsNet.Compressor.AutoDependency.Transformations
{
    /// <summary>
    /// Class takes the contents of a RequireJsNet.Compressor Bundle 
    /// and appends it to an existing System.Web.Optimization BundleResponse
    /// </summary>
    class ConvertRequireBundleToWebOptimizationBundle : IBundleTransform
    {
        /// <summary>
        /// RequireJsNet.Compressor Bundle 
        /// </summary>
        protected Bundle RequireBundle { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="requireBundle">RequireJsNet.Compressor Bundle</param>
        public ConvertRequireBundleToWebOptimizationBundle(Bundle requireBundle)
        {
            RequireBundle = requireBundle;
        }

        /// <summary>
        /// Processes the Bundle contents and transfers them
        /// </summary>
        /// <param name="context">the BundleContext</param>
        /// <param name="response">the BundleResponse</param>
        public void Process(BundleContext context, BundleResponse response)
        {
            foreach(var file in RequireBundle.Files)
            {
                response.Content += file.FileContent;
            }
        }
    }
}
