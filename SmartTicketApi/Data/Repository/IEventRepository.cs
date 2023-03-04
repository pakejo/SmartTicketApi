using SmartTicketApi.Models;

namespace SmartTicketApi.Data.Repository
{
    public interface IEventRepository : IRepository<Event>
    {
        /// <summary>
        /// Gets an event info including the promoter data
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Event?> GetEventWithPromoterInfo(string id);

        /// <summary>
        /// Get all events that will happend after today
        /// </summary>
        /// <returns>Future events list</returns>
        Task<List<Event>> GetFutureEvents();

        /// <summary>
        /// Get all events of the given type
        /// </summary>
        /// <param name="eventType">Event type</param>
        /// <returns>All event of the same type</returns>
        Task<List<Event>> GetEventsOfType(string eventType);

        /// <summary>
        /// Checks if it already exists an event with the same name
        /// </summary>
        /// <param name="name">Event name</param>
        /// <returns>true if there is an event with 
        /// the same name in the system, false otherwise</returns>
        Task<bool> EventWithSameNameExists(string name);
    }
}
