using System;

namespace SIEG.SrDevChallenge.Domain.Exceptions;

public class NotFoundException(string message) : Exception(message)
{
}
