using System;

namespace AudioDeviceControl.Exceptions
{
    public class UnselectableDeviceLineException : DeviceLineException
    {
        public UnselectableDeviceLineException(string deviceInterface, string deviceLine)
            : base(deviceInterface, deviceLine){
        }

        public UnselectableDeviceLineException(string deviceInterface, string deviceLine, string message)
            : base(deviceInterface, deviceLine, message){
        }

        public UnselectableDeviceLineException(string deviceInterface, string deviceLine, string message, Exception inner)
            : base(deviceInterface, deviceLine, message, inner){
        }
    }
}
