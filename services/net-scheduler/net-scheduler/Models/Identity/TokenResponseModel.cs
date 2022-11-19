namespace NetScheduler.Models.Identity;

public class TokenResponseModel
{
    public TokenResponseModel(string token)
    {
        Token = token;
    }

    public string Token { get; set; }
}
