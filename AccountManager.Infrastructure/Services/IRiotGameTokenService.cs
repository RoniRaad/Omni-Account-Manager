namespace AccountManager.Infrastructure.Services
{
    public interface IRiotGameTokenService
    {
        string GetLeagueCommandlineParams();
        bool TryGetPortAndToken(out string token, out string port);
    }
}