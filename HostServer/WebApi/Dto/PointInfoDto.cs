namespace HostServer.WebApi.Dto
{
    public class PointInfoDto
    {
        public int Number { get; set; }
        public int ParentNumber { get; set; }
        public HostInfoDto HostInfo { get; set; }
    }
}
