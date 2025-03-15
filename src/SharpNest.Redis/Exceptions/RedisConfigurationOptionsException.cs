namespace SharpNest.Redis.Exceptions;

public class RedisConfigurationOptionsException : Exception
{
    public RedisConfigurationOptionsException()
    {
    }

    public RedisConfigurationOptionsException(string message, params object[] args)
        : base(string.Format(message, args))
    {
    }
}
