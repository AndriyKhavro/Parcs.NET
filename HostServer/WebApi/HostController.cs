using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Parcs.Api.Dto;

namespace HostServer.WebApi
{
    public class HostController : ApiController
    {
        public IEnumerable<HostInfoDto> Get()
        {
            return Server.Instance.HostList.Select(h => new HostInfoDto
            {
                IpAddress = h.IpAddress.ToString(),
                PointCount = h.PointCount,
                ProcessorCount = h.ProcessorCount,
                LinpackResult = h.LinpackResult
            });
        } 
    }
}
