namespace SharpNest.Redis.Cache.Exceptions;

public class CacheSerializationException : Exception
{
    public CacheSerializationException()
    {
    }

    public CacheSerializationException(string message, params object[] args)
        : base(string.Format(message, args))
    {
    }
}
