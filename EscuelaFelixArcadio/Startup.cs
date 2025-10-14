using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(EscuelaFelixArcadio.Startup))]
namespace EscuelaFelixArcadio
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
