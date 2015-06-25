using System;
using System.Collections.Generic;

namespace RequireJsNet.Helpers
{
    public class HashStore<T>
    {
        private readonly object _lock = new object();

        private Dictionary<string, T> store = new Dictionary<string, T>();

        public T GetOrSet(string hash, Func<T> compute)
        {
            if (!store.ContainsKey(hash))
            {
                lock (_lock)
                {
                    if (!store.ContainsKey(hash))
                    {
                        store.Add(hash, compute());
                    }
                }
            }

            return store[hash];
        }
    }
}