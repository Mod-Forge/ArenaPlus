using System;
using System.Runtime.Serialization;

namespace ArenaPlus.Utils
{
    internal class AssertFailedException : Exception
    {
        public AssertFailedException(string message) : base(message)
        {
        }
    }

    internal class InvalidProgrammerException : InvalidOperationException
    {
        public InvalidProgrammerException(string message) : base(message + " you goof") { }
    }
}