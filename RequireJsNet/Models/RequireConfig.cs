namespace RequireJsNet.Models
{
    internal class RequireConfig
    {
        public string BaseUrl { get; set; }

        public string Locale { get; set; }

        public RequirePaths Paths { get; set; }

        public RequireShim Shim { get; set; }
    }
}
