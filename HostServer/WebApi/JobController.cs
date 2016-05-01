using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using HostServer.WebApi.Dto;
using Parcs;

namespace HostServer.WebApi
{
    [EnableCors(headers:"*", origins:"*", methods:"*")]
    public class JobController : ApiController
    {
        // GET api/job
        public IEnumerable<JobInfoDto> Get()
        {
            return Server.Instance.GetCurrentJobs().Select(j => new JobInfoDto
            {
                Number = j.Number,
                Priority = j.Priority,
                NeedsPoint = j.NeedsPoint,
                IsFinished = j.IsFinished,
                Points = j.PointDictionary.Values.Select(p => new PointInfoDto
                {
                    Number = p.Number,
                    ParentNumber = p.ParentNumber,
                    IsFinished = p.IsFinished,
                    HostInfo = new HostInfoDto
                    {
                        IpAddress = p.Host.IpAddress.ToString(),
                        PointCount = p.Host.PointCount
                    }
                }).ToArray()
            });
        }
    }
}
