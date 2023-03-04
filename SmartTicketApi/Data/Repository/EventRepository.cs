using Microsoft.EntityFrameworkCore;
using SmartTicketApi.Models;

namespace SmartTicketApi.Data.Repository
{
    public class EventRepository : GenericRepository<Event>, IEventRepository
    {
        public EventRepository(SmartTicketApiContext context) : base(context)
        {
        }

        public Task<bool> EventWithSameNameExists(string name) => context.Event.AnyAsync(e => e.Name == name);

        public Task<List<Event>> GetEventsOfType(string eventType) => context.Event.Where(e => e.Type == eventType).ToListAsync();

        public Task<Event?> GetEventWithPromoterInfo(string id) => context.Event.Where(e => e.Id == id).Include(e => e.Promoter).FirstOrDefaultAsync();
        public Task<List<Event>> GetFutureEvents() => context.Event.Where(e => e.Date >= DateTime.UtcNow).ToListAsync();
    }
}
