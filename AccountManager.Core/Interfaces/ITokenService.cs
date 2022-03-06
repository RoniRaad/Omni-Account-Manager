namespace AccountManager.Core.Interfaces
{
    public interface ITokenService
    {
        bool TryGetPortAndToken(out string token, out string port);
    }
}