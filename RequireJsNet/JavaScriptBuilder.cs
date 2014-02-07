using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace RequireJsNet
{
    internal class JavaScriptBuilder
    {
        const string type = "application/javascript";

        public bool TagHasType { get; set; }

        private TagBuilder scriptTag = new TagBuilder("script");

        private StringBuilder content = new StringBuilder();

        public string Render()
        {
            scriptTag.InnerHtml = RenderContent();
            return scriptTag.ToString(TagRenderMode.Normal);
        }

        public string RenderContent()
        {
            return content.ToString();
        }

        public string RenderStatement()
        {
            if (TagHasType)
            {
                scriptTag.MergeAttribute("type", type);
            }

            scriptTag.InnerHtml = "";

            return scriptTag.ToString(TagRenderMode.Normal);
        }

        public void AddAttributesToStatement(string key, string value)
        {
            scriptTag.MergeAttribute(key, value);
        }

        public void AddStatement(string statement)
        {
            content.AppendLine(statement);
        }

    }
}
