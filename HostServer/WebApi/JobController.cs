using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using HostServer.WebApi.Dto;
using Parcs;

namespace HostServer.WebApi
{
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
                Points = j.PointDictionary.Values.Select(p => new PointInfoDto
                {
                    Number = p.Number,
                    ParentNumber = p.ParentNumber,
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
