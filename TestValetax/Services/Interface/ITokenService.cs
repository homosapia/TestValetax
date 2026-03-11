namespace TestValetax.Services.Interface
{
    public interface ITokenService
    {
        string GenerateToken(string code);
        string GenerateUniqueToken();
    }

}
