// RequireJS.NET
// Copyright VeriTech.io
// http://veritech.io
// Dual licensed under the MIT and GPL licenses:
// http://www.opensource.org/licenses/mit-license.php
// http://www.gnu.org/licenses/gpl.html

namespace RequireJsNet.Compressor.Parsing
{
	/// <summary>
	/// Represents a single script line of a file
	/// </summary>
	internal class ScriptLine
	{
		/// <summary>
		/// The text of the line
		/// </summary>
		public string LineText { get; set; }

		/// <summary>
		/// At which index the line starts 
		/// </summary>
		public int StartingIndex { get; set; }

		/// <summary>
		/// The length of the new line
		/// </summary>
		public int NewLineLength { get; set; }
	}
}
