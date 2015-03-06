// RequireJS.NET
// Copyright VeriTech.io
// http://veritech.io
// Dual licensed under the MIT and GPL licenses:
// http://www.opensource.org/licenses/mit-license.php
// http://www.gnu.org/licenses/gpl.html

using RequireJsNet.Models;

namespace RequireJsNet.Configuration
{
	public interface IConfigReader
	{
		string Path { get; }

		ConfigurationCollection ReadConfig();
	}
}
