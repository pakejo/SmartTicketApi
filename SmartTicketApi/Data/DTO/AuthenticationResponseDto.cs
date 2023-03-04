namespace SmartTicketApi.Data.DTO
{
    public class AuthenticationResponseDto
    {
        public string Token { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}
