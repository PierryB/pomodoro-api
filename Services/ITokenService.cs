using pomodoro_api.Models;

namespace pomodoro_api.Services;

public interface ITokenService
{
    string GenerateToken(User user);
}
