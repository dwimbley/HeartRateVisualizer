using System;

namespace Wimbley.AppleWatch.HeartRate.Site.Core.Exceptions
{
    public class FileExtensionNotSupportedException : ApplicationException
    {
        public FileExtensionNotSupportedException(string message) : base(message) { }

        public FileExtensionNotSupportedException(string message, Exception innerException) : base(message, innerException) { }
    }
}
