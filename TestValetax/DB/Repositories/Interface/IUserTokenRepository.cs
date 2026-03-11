using TestValetax.DB.Entities;

namespace TestValetax.DB.Repositories.Interface
{
    public interface IUserTokenRepository : IRepository<UserToken>
    {
        Task<UserToken?> GetByCodeAsync(string code);
        Task<UserToken?> GetValidTokenAsync(string token);
        Task DeactivateOldTokensAsync(string code);
    }
}
