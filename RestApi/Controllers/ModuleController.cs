using System;
using System.Threading.Tasks;
using System.Web.Http;
using Parcs.Api.Dto;
using RestApi.Services;

namespace RestApi.Controllers
{
    [Authorize]
    public class ModuleController: ApiController
    {
        private readonly IModuleRunner _moduleRunner;
        private readonly IParcsService _parcsService;

        public ModuleController(IModuleRunner moduleRunner, IParcsService parcsService)
        {
            _moduleRunner = moduleRunner;
            _parcsService = parcsService;
        }

        [HttpPost]
        [ActionName("matrix")]
        public async Task<IHttpActionResult> RunMatrixModule(MatrixModuleDto moduleDto)
        {
            var serverIp = await _parcsService.GetServerIp();
            bool isRunSuccessfully = await _moduleRunner.TryRunMatrixModule(moduleDto.MatrixSize, moduleDto.PointCount, serverIp, moduleDto.Priority);
            if (!isRunSuccessfully)
            {
                throw new InvalidOperationException("Module didn't start successfully");
            }
            return Ok();
        }
    }
}