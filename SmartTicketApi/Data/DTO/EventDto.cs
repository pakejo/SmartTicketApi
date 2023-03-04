using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using SmartTicketApi.Models;

namespace SmartTicketApi.Data.DTO
{
    public class EventDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public DateTime Date { get; set; }
        public string ContractAddress { get; set; }
        public string PromoterId { get; set; }
        public double TicketPrice { get; set; }
        public ICollection<string> Sales { get; set; }
    }
}
