using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using System.Web;
using Wutnu.Data;
using System.Web.Mvc;

[assembly: OwinStartup(typeof(Wutnu.Startup))]

namespace Wutnu
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
