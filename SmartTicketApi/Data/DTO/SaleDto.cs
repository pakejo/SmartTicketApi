using SmartTicketApi.Models;

namespace SmartTicketApi.Data.DTO
{
    public class SaleDto
    {
        public string Id { get; set; }
        public DateTime CreationDate { get; set; }
        public int Token { get; set; }

        // Navidation properties
        public string UserId { get; set; }
        public string EventId { get; set; }
    }
}
