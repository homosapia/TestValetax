using TestValetax.DB.Entities;

namespace TestValetax.DB.Repositories.Interface
{
    public interface IJournalRepository : IRepository<ExceptionJournal>
    {
        Task<(IEnumerable<ExceptionJournal> Items, int TotalCount)> GetFilteredAsync(
            int skip,
            int take,
            DateTime? from = null,
            DateTime? to = null,
            string? search = null);
    }
}
