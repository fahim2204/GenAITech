using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace GenAITech.Models
{
    public class GenAISite
    {
        public int Id { get; set; }
        public string GenAIName { get; set; }
        public string Summary { get; set; }
        public string? ImageFilename { get; set; }
        public string? AnchorLink { get; set; }
        public int? Like { get; set; }
    }
}
