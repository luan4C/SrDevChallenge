using System;

namespace SIEG.SrDevChallenge.Domain.Exceptions;

public class UnexpectedException : Exception
{
    public UnexpectedException() { }

    public UnexpectedException(string message) : base(message) { }

    public UnexpectedException(string message, Exception innerException) : base(message, innerException) { }
}
