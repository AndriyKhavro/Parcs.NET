using System.Collections.Generic;

namespace HostServer.WebApi.Dto
{
    public class JobInfoDto
    {
        public int Number { get; set; }
        public int Priority { get; set; }
        public bool NeedsPoint { get; set; }
        public ICollection<PointInfoDto> Points { get; set; } 
    }
}
