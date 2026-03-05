using System;

namespace SIEG.SrDevChallenge.Domain.Exceptions;

public class ConflictException : Exception
{
    public ConflictException() { }

    public ConflictException(string message) : base(message) { }

}
