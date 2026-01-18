using com.QuizAppBackend.Dtos;

namespace com.QuizAppBackend.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResult> AuthenticateAsync(LoginRequest loginRequest, CancellationToken cancellationToken = default);
        Task<AuthResult> RegisterAsync(RegisterRequest registerRequest, CancellationToken cancellationToken = default);
        Task<AuthResult?> RefreshTokenAsync(string token, CancellationToken cancellationToken = default);
        Task<bool> RevokeTokenAsync(string token, CancellationToken cancellationToken = default);
    }
}