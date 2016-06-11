﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using log4net;
using Parcs;
using Parcs.Api.Dto;

namespace HostServer.WebApi
{
    public class JobController : ApiController
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(JobController));

        // GET api/job
        public IEnumerable<JobInfoDto> Get()
        {
            return Server.Instance.GetCurrentJobs().Select(j => new JobInfoDto
            {
                Number = j.Number,
                Priority = j.Priority,
                JobStatus = ResolveJobStatus(j).ToString(),
                StartTimeUtc = j.StartTimeUtc,
                FinishTimeUtc = j.FinishTimeUtc,
                Username = j.Username,
                Points = j.PointDictionary.Values.Select(p => new PointInfoDto
                {
                    Number = p.Number,
                    IsFinished = p.IsFinished,
                    HostIpAddress = p.Host.IpAddress.ToString(),
                    StartTimeUtc = p.StartTimeUtc,
                    FinishTimeUtc = p.FinishTimeUtc
                }).ToArray()
            });
        }

        [HttpPost]
        [Route("cancel")]
        public IHttpActionResult CancelJob(CancelJobDto dto)
        {
            _log.Debug($"Cancelling Job N {dto.Number}...");
            Server.Instance.CancelJob(dto.Number);
            return Ok();
        }

        private JobStatus ResolveJobStatus(JobInfo jobInfo)
        {
            if (jobInfo.IsCancelled)
            {
                return JobStatus.Cancelled;
            }

            if (jobInfo.IsFinished)
            {
                return JobStatus.Finished;
            }
            
            if (jobInfo.PointDictionary.Any())
            {
                return jobInfo.NeedsPoint ? JobStatus.Partly : JobStatus.Running;
            }

            return JobStatus.Pending;
        }
    }
}
