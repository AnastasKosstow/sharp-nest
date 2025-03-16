using SharpNest.Samples.Redis.Api.Models;

namespace SharpNest.Samples.Redis.Api.Services;

public interface IUserService
{
    Task SetUserAsync(User user, CancellationToken cancellationToken = default);
    Task<User> GetUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<User> TryGetUserAsync(int userId, CancellationToken cancellationToken = default);
    Task RemoveUserAsync(int userId, CancellationToken cancellationToken = default);
}