using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace RequireJsNet
{
	public class RequireCallBuilderCollection : List<string>
	{
		public string BuildCalls()
		{
			var builder = new StringBuilder();
			builder.AppendLine();
			builder.AppendFormat("require({0});", Newtonsoft.Json.JsonConvert.SerializeObject(this));

			var tagBuilder = new TagBuilder("script");
			tagBuilder.InnerHtml = builder.ToString();

			return tagBuilder.ToString();
		}
	}
}
