using SharpNest.Redis.Cache;
using SharpNest.Samples.Redis.Api.Models;

namespace SharpNest.Samples.Redis.Api.Services;

internal class UserService(IRedisCache cache, ILogger<UserService> logger) : IUserService
{
    private readonly IRedisCache _cache = cache;
    private readonly ILogger<UserService> _logger = logger;

    public async Task SetUserAsync(User user, CancellationToken cancellationToken = default)
    {
        string key = $"user:profile:{user.Id}";

        await _cache.SetAsync(key, user, options =>
        {
            options.Expiration = TimeSpan.FromMinutes(10);
        }, cancellationToken);
    }

    public async Task<User> GetUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        string key = $"user:profile:{userId}";

        User user = default;
        try
        {
            user = await _cache.GetAsync<User>(key, cancellationToken);
        }
        catch (Exception ex)
        {
            // handle exeption
            _logger.LogError("Exception message: {message}.", ex.Message);
        }

        return user;
    }

    public async Task<User> TryGetUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        string key = $"user:profile:{userId}";

        var (Success, Result) = await _cache.TryGetAsync<User>(key, cancellationToken);
        if (!Success)
        {
            // handle null result
            _logger.LogError("User with Id: {id} not found.", userId);
        }

        return Result;
    }

    public async Task RemoveUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        string key = $"user:profile:{userId}";

        await _cache.RemoveAsync(key, cancellationToken);
    }
}
