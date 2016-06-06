using DataAccess.Auth;
using DataAccess.Logs;
using Microsoft.Practices.Unity;
using RestApi.Services;

namespace RestApi.Bootstrap
{
    public class UnityRegistar
    {
        public void Register(IUnityContainer container)
        {
            container.RegisterType<IRestApiClient, RestApiClient>();
            container.RegisterType<ILogEntryRepository, LogEntryRepository>();
            container.RegisterType<IParcsService, ParcsService>();
            container.RegisterType<IAuthRepository, AuthRepository>();
        }
    }
}