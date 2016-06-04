using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using RestApi.Services;

namespace RestApi.Controllers
{
    public class ChartController : ApiController
    {
        private readonly IRestApiClient _restApiClient;

        public ChartController(IRestApiClient restApiClient)
        {
            _restApiClient = restApiClient;
        }

        
    }
}