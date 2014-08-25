using System.Text;
using System.Web.Mvc;

namespace RequireJsNet
{
    internal class JavaScriptBuilder
    {
        private const string Type = "application/javascript";

        private readonly TagBuilder scriptTag = new TagBuilder("script");

        private readonly StringBuilder content = new StringBuilder();

        private bool hasNewLine = false;

        public bool TagHasType { get; set; }

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
                scriptTag.MergeAttribute("type", Type);
            }

            scriptTag.InnerHtml = string.Empty;

            return scriptTag.ToString(TagRenderMode.Normal);
        }

        public void AddAttributesToStatement(string key, string value)
        {
            scriptTag.MergeAttribute(key, value);
        }

        public void AddStatement(string statement)
        {
            if (!hasNewLine)
            {
                content.AppendLine();
                hasNewLine = true;
            }

            content.AppendLine(statement);
        }
    }
}
