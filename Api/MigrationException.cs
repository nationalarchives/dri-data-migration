using System;

namespace Api;

public class MigrationException : Exception
{
    public MigrationException() : base() { }
    public MigrationException(string message) : base(message) { }
}
