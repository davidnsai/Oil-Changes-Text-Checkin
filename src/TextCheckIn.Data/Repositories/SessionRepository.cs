using TextCheckIn.Data.Repositories.Interfaces;
using TextCheckIn.Data.Entities;
using TextCheckIn.Data.Context;

namespace TextCheckIn.Data.Repositories
{
    public class SessionRepository : ISessionRepository
    {
        private readonly AppDbContext _context;

        public SessionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CheckInSession?> GetSessionAsync(Guid? sessionId)
        {
            return await _context.CheckInSessions.FindAsync(sessionId);
        }

        public async Task CreateSessionAsync(CheckInSession session)
        {
            _context.CheckInSessions.Add(session);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateSessionAsync(CheckInSession session)
        {
            _context.CheckInSessions.Update(session);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteSessionAsync(Guid sessionId)
        {
            var session = await _context.CheckInSessions.FindAsync(sessionId);
            if (session != null)
            {
                _context.CheckInSessions.Remove(session);
                await _context.SaveChangesAsync();
            }
        }
    }
}
