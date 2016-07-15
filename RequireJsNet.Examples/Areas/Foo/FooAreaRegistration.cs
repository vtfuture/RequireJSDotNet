using System.Web.Mvc;

namespace RequireJsNet.Examples.Areas.Foo
{
    public class FooAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Foo";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Foo_default",
                "Foo/{controller}/{action}/{id}",
                new { controller="Home", action = "Index", id = UrlParameter.Optional },
                namespaces: new string[] { "RequireJsNet.Examples.Areas.Foo.Controllers" }
            );
        }
    }
}