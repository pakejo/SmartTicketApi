using System.ComponentModel.DataAnnotations;

namespace SmartTicketApi.Models
{
    public class Event
    {
        public string Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        [Required]
        public string Type { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public string ContractAddress { get; set; }
        [Required]
        public double TicketPrice { get; set; }

        // Navigation properties
        public ICollection<Sale> Sales { get; set; }
        public string PromoterId { get; set; }
        public ApplicationUser Promoter { get; set; }
    }
}
