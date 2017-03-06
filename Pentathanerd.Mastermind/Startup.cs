using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Pentathanerd.Mastermind.Startup))]
namespace Pentathanerd.Mastermind
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}