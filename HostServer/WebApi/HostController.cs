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
                ProcessorCount = h.IsConnected ? h.ProcessorCount : 0,
                LinpackResult = h.IsConnected ? h.LinpackResult : 0,
                IsConnected = h.IsConnected
            });
        } 
    }
}
