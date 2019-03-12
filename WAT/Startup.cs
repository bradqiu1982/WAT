using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WAT.Startup))]
namespace WAT
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
