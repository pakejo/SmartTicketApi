using SmartTicketApi.Models;

namespace SmartTicketApi.Data.Repository
{
    public class SaleRepository : GenericRepository<Sale>, ISaleRepository
    {
        public SaleRepository(SmartTicketApiContext context) : base(context)
        {
        }
    }
}
