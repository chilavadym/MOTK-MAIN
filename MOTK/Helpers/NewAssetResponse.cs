using MOTK.Enums;

namespace MOTK.Helpers
{
    public class NewAssetResponse
    {
        public ESaveCancel Response { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
    }
}
