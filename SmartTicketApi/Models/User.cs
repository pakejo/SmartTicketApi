using Microsoft.AspNetCore.Identity;

namespace SmartTicketApi.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string WalletAddress { get; set; }
        public ICollection<Sale> Sales { get; set; }
        public ICollection<Event> Events { get; set; }
    }
}
