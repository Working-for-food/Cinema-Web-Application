namespace Application.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

public class NotFoundDomainException : DomainException
{
    public NotFoundDomainException(string message) : base(message) { }
}

public class ConflictDomainException : DomainException
{
    public ConflictDomainException(string message) : base(message) { }
}
