using System.Numerics;

namespace SmartTicketApi.Models
{
    public class Sale
    {
        public string Id { get; set; }
        public DateTime CreationDate { get; set; }

        // Navidation properties
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public string EventId { get; set; }
        public Event Event { get; set; }
        public int Token { get; set; }
    }
}
