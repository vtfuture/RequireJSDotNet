// RequireJS.NET
// Copyright VeriTech.io
// http://veritech.io
// Dual licensed under the MIT and GPL licenses:
// http://www.opensource.org/licenses/mit-license.php
// http://www.gnu.org/licenses/gpl.html

using System.Collections.Generic;

using Jint.Parser.Ast;

using RequireJsNet.Compressor.Models;

namespace RequireJsNet.Compressor.Parsing
{
	/// <summary>
	/// Enum for require call types
	/// </summary>
	internal enum RequireCallType
	{
		Define,
		Require
	}

	/// <summary>
	/// Represents a require call
	/// </summary>
	internal class RequireCall
	{
		/// <summary>
		/// constructs a new require call
		/// </summary>
		public RequireCall()
		{
			Children = new List<RequireCall>();
			Dependencies = new List<string>();
		}

		/// <summary>
		/// The id of the call
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// The parent node of the call
		/// </summary>
		public NodeWithChildren ParentNode { get; set; }

		/// <summary>
		/// Whether the call is a module or not
		/// </summary>
		public bool IsModule { get; set; }

		/// <summary>
		/// Whether it is a require or define call
		/// </summary>
		public RequireCallType Type { get; set; }

		/// <summary>
		/// A List of dependencies
		/// </summary>
		public List<string> Dependencies { get; set; }

		/// <summary>
		/// SUbsequent require calls
		/// </summary>
		public List<RequireCall> Children { get; set; }

		/// <summary>
		/// a node with one single dependency
		/// </summary>
		public Literal SingleDependencyNode { get; set; }

		/// <summary>
		/// A node that is the module identifier
		/// </summary>
		public Literal ModuleIdentifierNode { get; set; }

		/// <summary>
		/// A node that is an array of dependencies
		/// </summary>
		public ArrayExpression DependencyArrayNode { get; set; }

		/// <summary>
		/// A node that is the module definition
		/// </summary>
		public FunctionExpression ModuleDefinitionNode { get; set; }
	}
}
