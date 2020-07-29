namespace SharpC2.Models
{
    public class ClientAuthenticationResult
    {
        public AuthResult Result { get; set; }
        public string Token { get; set; }

        public enum AuthResult
        {
            LoginSuccess,
            BadPassword,
            NickInUse,
            InvalidRequest
        }
    }
}