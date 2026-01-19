namespace QuizApp.Backend.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set;} = null!;
        public DateTime Expires { get; set;}
        public DateTime Created { get; set;}
        public DateTime? Revoked { get; set;}
        public string? ReplacedByToken { get; set;}
        public int UserId { get; set;}
        public User User { get; set;} = null!;
        public bool IsActive => Revoked == null && !IsExpired;
        public bool IsExpired => DateTime.UtcNow >= Expires;

    }
}