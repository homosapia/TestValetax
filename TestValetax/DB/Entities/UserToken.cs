namespace TestValetax.DB.Entities
{
    public class UserToken
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsActive { get; set; }
    }
}
