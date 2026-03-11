using Microsoft.EntityFrameworkCore;
using TestValetax.DB.Entities;
using TestValetax.DB.Repositories.Interface;

namespace TestValetax.DB.Repositories
{
    public class JournalRepository : Repository<ExceptionJournal>, IJournalRepository
    {
        public JournalRepository(ApplicationContext context) : base(context)
        {
        }

        public async Task<(IEnumerable<ExceptionJournal> Items, int TotalCount)> GetFilteredAsync(
            int skip,
            int take,
            DateTime? from = null,
            DateTime? to = null,
            string? search = null)
        {
            var query = _dbSet.AsQueryable();

            if (from.HasValue)
                query = query.Where(j => j.Timestamp >= from.Value);

            if (to.HasValue)
                query = query.Where(j => j.Timestamp <= to.Value);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(j =>
                    j.StackTrace.Contains(search) ||
                    j.ExceptionType.Contains(search));

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(j => j.Timestamp)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}
