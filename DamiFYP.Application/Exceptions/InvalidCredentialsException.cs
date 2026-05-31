using System.Collections;

namespace DamiFYP.Application.Exceptions;

public class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException() { }
    public InvalidCredentialsException(string message) : base(message) { }
}