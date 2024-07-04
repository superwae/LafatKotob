namespace Lafatkotob.Services.TokenService
{
    public interface ITokenSerive
    {
        public string GenerateJwtToken(string userName, string userId, List<string> roles);
    }

}
