using System.ComponentModel.DataAnnotations;

namespace Sheleni_API.Models
{
    public class Service
    {
        [Key]
        public int ServiceID { get; set; }
        public string ServiceName { get; set; }
    }
}
