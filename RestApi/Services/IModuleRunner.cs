using System.Threading.Tasks;
using Parcs.Api.Dto;

namespace RestApi.Services
{
    public interface IModuleRunner
    {
        Task<bool> TryRunMatrixModule(MatrixSize matrixSize, int pointCount, string serverIp);
    }
}
