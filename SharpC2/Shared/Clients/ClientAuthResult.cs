public class ClientAuthResponse
{
    public ClientAuthResult Result { get; set; }
    public string Token { get; set; }
}

public enum ClientAuthResult
{
    LoginSuccess,
    BadPassword,
    NickInUse,
    InvalidRequest
}