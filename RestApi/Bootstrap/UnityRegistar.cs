using Microsoft.Practices.Unity;
using RestApi.Services;

namespace RestApi.Bootstrap
{
    public class UnityRegistar
    {
        public void Register(IUnityContainer container)
        {
            container.RegisterType<IRestApiClient, RestApiClient>();
        }
    }
}