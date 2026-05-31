public interface ITokenService
{
    public string GenerateToken(long userId, string email, string role);
}