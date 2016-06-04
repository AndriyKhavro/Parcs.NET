using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Owin;
using Microsoft.Practices.Unity;
using Owin;
using RestApi.Bootstrap;
using RestApi.Controllers;

[assembly: OwinStartup(typeof(Startup))]

namespace RestApi.Bootstrap
{
    public class Startup
    {
        protected IUnityContainer UnityContainer;

        public void Configuration(IAppBuilder appBuilder)
        {
            ConfigureShutdown(appBuilder);
            UnityContainer = UnityConfig.RegisterComponents();

            var configuration = new HttpConfiguration();
            WebApiConfig.Register(configuration, UnityContainer);

            appBuilder.UseWebApi(configuration);
        }

        protected virtual void Shutdown()
        {
            UnityContainer.Dispose();
        }

        private void ConfigureShutdown(IAppBuilder appBuilder)
        {
            var context = new OwinContext(appBuilder.Properties);
            var token = context.Get<CancellationToken>("host.OnAppDisposing");
            if (token != CancellationToken.None)
            {
                token.Register(Shutdown);
            }
        }
    }
}