using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Test_case.Startup))]
namespace Test_case
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
