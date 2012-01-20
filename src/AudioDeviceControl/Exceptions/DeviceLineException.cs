using System;

namespace AudioDeviceControl.Exceptions
{
    public class DeviceLineException : Exception
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

        private string _DeviceLine;
        public string DeviceLine
        {
            get
            {
                return _DeviceLine;
            }
            set
            {
                _DeviceLine = value;
            }
        }

        public DeviceLineException(string deviceInterface, string deviceLine)
            : base()
        {
            DeviceInterface = deviceInterface;
            DeviceLine = deviceLine;
        }

        public DeviceLineException(string deviceInterface, string deviceLine, string message)
            : base(message)
        {
            DeviceInterface = deviceInterface;
            DeviceLine = deviceLine;
        }

        public DeviceLineException(string deviceInterface, string deviceLine, string message, Exception inner)
            : base(message, inner)
        {
            DeviceInterface = deviceInterface;
            DeviceLine = deviceLine;
        }
    }
}
