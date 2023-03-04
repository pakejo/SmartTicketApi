using System.Numerics;

namespace SmartTicketApi.Data.DTO
{
    public class EventBalanceResponseDto
    {
        public decimal Ether { get; set; }
        public decimal Gwei { get; set; }
        public decimal Mwei { get; set; }
    }
}
