namespace ead_frontend.Models
{
    public class CollectionRequest
    {
        public string title { get; set; }
        public string identifier { get; set; }
        public int count { get; set; }
        public int? reopened { get; set; }
    }
}