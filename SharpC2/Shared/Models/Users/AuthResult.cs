namespace Shared.Models
{
    public class AuthResult
    {
        public AuthStatus Status { get; set; }
        public string Token { get; set; }

        public enum AuthStatus
        {
            LogonSuccess,
            NickInUse,
            BadPassword
        }
    }
}