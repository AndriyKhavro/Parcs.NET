using System.Configuration;
using System.Threading.Tasks;
using System.Web.Http;
using Parcs.Api.Dto;
using RestApi.Services;

namespace RestApi.Controllers
{
    public class ParcsController : ApiController
    {
        private readonly IRestApiClient _restApiClient;

        private readonly string _hostServerUrl = ConfigurationManager.AppSettings["hostServerUrl"];

        public ParcsController(IRestApiClient restApiClient)
        {
            _restApiClient = restApiClient;
        }

        [HttpGet]
        [ActionName("job")]
        public Task<JobInfoDto[]> GetJobs()
        {
            return _restApiClient.GetAsync<JobInfoDto[]>($"{_hostServerUrl}/api/job");
        }

        [HttpGet]
        [ActionName("host")]
        public Task<JobInfoDto[]> GetHosts()
        {
            return _restApiClient.GetAsync<JobInfoDto[]>($"{_hostServerUrl}/api/host");
        }
    }
}