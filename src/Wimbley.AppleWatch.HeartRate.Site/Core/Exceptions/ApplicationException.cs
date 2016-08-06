using System;

namespace Wimbley.AppleWatch.HeartRate.Site.Core.Exceptions
{
    // Exception "base" class to be used when displaying custom exceptions with friendly error messages to end user.
    //      meant as a way to prevent showing error messages of "null reference: object not set to an instance of an object" and show a "see log file for more details" 
    //      for system exceptions.
    public class ApplicationException : Exception
    {
        public ApplicationException(string message) : base(message) { }

        public ApplicationException(string message, Exception innerException) : base(message, innerException) { }
    }
}
