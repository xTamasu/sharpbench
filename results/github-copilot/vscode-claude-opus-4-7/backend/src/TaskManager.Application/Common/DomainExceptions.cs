// DomainExceptions.cs
// Custom exception types thrown by services. The global exception
// handling middleware in the API layer maps each one to a specific HTTP status.
namespace TaskManager.Application.Common;

/// <summary>Thrown when a request contains semantically invalid data (HTTP 400).</summary>
public class ValidationFailedException : Exception
{
    public ValidationFailedException(string message) : base(message) { }
}

/// <summary>Thrown when a referenced entity cannot be found (HTTP 404).</summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

/// <summary>Thrown when a user attempts an action they aren't allowed to perform (HTTP 403).</summary>
public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message) { }
}

/// <summary>Thrown when authentication fails (HTTP 401).</summary>
public class AuthenticationFailedException : Exception
{
    public AuthenticationFailedException(string message) : base(message) { }
}

/// <summary>Thrown on conflicts such as duplicate email registration (HTTP 409).</summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}
