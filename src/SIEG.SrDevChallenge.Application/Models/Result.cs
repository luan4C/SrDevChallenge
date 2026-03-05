using System;

namespace SIEG.SrDevChallenge.Application.Models;

public class Result<T>(T item, string message)
{
    public T Item { get; set; } = item;
    public string Message { get; set; } = message;
}
