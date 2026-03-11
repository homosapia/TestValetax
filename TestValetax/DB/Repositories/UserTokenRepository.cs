using Microsoft.EntityFrameworkCore;
using TestValetax.DB.Entities;
using TestValetax.DB.Repositories.Interface;

namespace TestValetax.DB.Repositories
{
    public class UserTokenRepository : Repository<UserToken>, IUserTokenRepository
    {
        public UserTokenRepository(ApplicationContext context) : base(context)
        {
        }

        public async Task<UserToken?> GetByCodeAsync(string code)
        {
            return await _dbSet
                .FirstOrDefaultAsync(t => t.Code == code && t.IsActive);
        }

        public async Task<UserToken?> GetValidTokenAsync(string token)
        {
            return await _dbSet
                .FirstOrDefaultAsync(t => t.Token == token
                    && t.IsActive
                    && t.ExpiresAt > DateTime.UtcNow);
        }

        public async Task DeactivateOldTokensAsync(string code)
        {
            var tokens = await _dbSet
                .Where(t => t.Code == code && t.IsActive)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.IsActive = false;
            }

            _dbSet.UpdateRange(tokens);
        }
    }
}
