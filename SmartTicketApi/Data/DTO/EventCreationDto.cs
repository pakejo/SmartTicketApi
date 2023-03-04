using System.ComponentModel.DataAnnotations;

namespace SmartTicketApi.Data.DTO
{
    public class EventCreationDto
    {
        [Required(ErrorMessage = "Event name is mandatory")]
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        [Required(ErrorMessage = "Event type is mandatory")]
        public string Type { get; set; }
        
        [Required(ErrorMessage = "The event date is mandatory")]
        public DateTime Date { get; set; }
        
        [Required]
        public string UserWalletPassword { get; set; }

        [Required]
        public double TicketPrice { get; set; }
    }
}
