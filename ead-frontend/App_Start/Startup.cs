using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ead_frontend.App_Start.Startup))]
namespace ead_frontend.App_Start
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}