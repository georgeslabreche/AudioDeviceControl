using System;

namespace AudioDeviceControl.Exceptions
{
    public class PlatformNotSupportedException : Exception
    {
        public PlatformNotSupportedException(): base() {

        }

        public PlatformNotSupportedException(string message) : base(message) {
            
        }

        public PlatformNotSupportedException(string message, Exception inner): base(message, inner) {

        }
    }
}
