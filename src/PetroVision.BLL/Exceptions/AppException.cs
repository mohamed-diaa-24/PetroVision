namespace PetroVision.BLL.Exceptions;

public abstract class AppException : Exception
{
    protected AppException(string message) : base(message) { }
}

public class NotFoundException : AppException
{
    public NotFoundException(string message) : base(message) { }
}

public class ConflictException : AppException
{
    public ConflictException(string message) : base(message) { }
}

public class ForbiddenException : AppException
{
    public ForbiddenException(string message) : base(message) { }
}

public class InvalidCredentialsException : AppException
{
    public InvalidCredentialsException(string message = "Invalid email or password.") : base(message) { }
}
