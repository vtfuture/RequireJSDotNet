using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using RequireJsNet.EntryPointResolver;

namespace RequireJsNet
{
    public class RequireEntryPointResolverCollection
    {
        private List<IEntryPointResolver> resolvers = new List<IEntryPointResolver>();

        public void Clear()
        {
            lock (resolvers)
            {
                resolvers.Clear();
            }
            
        }

        public void Prepend(IEntryPointResolver resolver)
        {
            lock (resolvers)
            {
                resolvers.Insert(0, resolver);
            }
        }

        public void Add(IEntryPointResolver resolver)
        {
            lock (resolvers)
            {
                resolvers.Add(resolver);
            }
        }

        internal string Resolve(ViewContext viewContext, string entryPointRoot)
        {
            string result = null;

            lock (resolvers)
            {
                foreach (var resolver in resolvers)
                {
                    result = resolver.Resolve(viewContext, entryPointRoot);
                    if (result != null)
                    {
                        break;
                    }
                }
            }

            return result;
        }
    }
}
