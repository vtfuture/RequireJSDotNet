// RequireJS.NET
// Copyright VeriTech.io
// http://veritech.io
// Dual licensed under the MIT and GPL licenses:
// http://www.opensource.org/licenses/mit-license.php
// http://www.gnu.org/licenses/gpl.html

using System;
using System.Collections.Generic;
using System.Linq;

namespace RequireJsNet.Models
{
    public class RequirePath
    {

        public RequirePath(string key)
        {
            this.Key = key;
        }

        public RequirePath(string key, params string[] values)
            :this(key)
        {
            this.Add(values);
        }

        public string Key { get; }

        public string DefaultBundle { get; set; }

        public IEnumerable<string> Value { get { return _value; } }
        readonly HashSet<string> _value = new HashSet<string>();

        public void Add(params string[] values)
        {
            foreach (var value in values)
                this._value.Add(value);
        }

        public void Add(IEnumerable<string> values)
        {
            foreach (var value in values)
                this._value.Add(value);
        }

        public void ReplaceValues(IEnumerable<string> values)
        {
            this._value.Clear();
            this.Add(values.ToArray());
        }

        public void ReplaceValue(string oldValue, string newValue)
        {
            foreach (var value in _value)
            {
                var count = _value.RemoveWhere(s => s.ToLower() == oldValue.ToLower());
                if (count > 0)
                    _value.Add(newValue);
            }
        }
    }
}
