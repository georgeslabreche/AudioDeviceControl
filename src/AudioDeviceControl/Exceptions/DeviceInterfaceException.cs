using System;

namespace AudioDeviceControl.Exceptions
{
    public class DeviceInterfaceException : Exception 
    {
        private string _DeviceInterface;
        public string DeviceInterface
        {
            get
            {
                return _DeviceInterface;
            }
            set
            {
                _DeviceInterface = value;
            }
        }

        public DeviceInterfaceException(string deviceInterface)
            : base()
        {
            DeviceInterface = deviceInterface;
        }

        public DeviceInterfaceException(string deviceInterface, string message) : base(message) {
            DeviceInterface = deviceInterface;
        }

        public DeviceInterfaceException(string deviceInterface, string message, Exception inner)
            : base(message, inner)
        {
            DeviceInterface = deviceInterface;
        }
    }
}
