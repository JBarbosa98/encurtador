namespace Encurtador.Api.Application;

public sealed class LinkValidationException : Exception
{
    public LinkValidationException(string message) : base(message)
    {
    }
}

public sealed class LinkDuplicateCodeException : Exception
{
    public LinkDuplicateCodeException(string message) : base(message)
    {
    }
}

public sealed class LinkNotFoundException : Exception
{
    public LinkNotFoundException(string message) : base(message)
    {
    }
}

public sealed class LinkExpiredException : Exception
{
    public LinkExpiredException(string message) : base(message)
    {
    }
}
