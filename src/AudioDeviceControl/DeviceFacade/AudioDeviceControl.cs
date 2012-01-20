
using System.Collections.Generic;
using System;

namespace AudioDeviceControl
{
    public class AudioDeviceControl
    {
        protected Dictionary<string, List<AudioLineInfo>> captureDeviceDictionary;
        protected Dictionary<string, List<AudioLineInfo>> renderDeviceDictionary;

        protected Object deviceChangeLock = new Object();

        private string _CaptureDeviceInterface;
        public string CaptureDeviceInterface
        {
            get
            {
                return _CaptureDeviceInterface;
            }
            set
            {
                _CaptureDeviceInterface = value;
            }
        }

        private string _CaptureDeviceLine;
        public string CaptureDeviceLine
        {
            get
            {
                return _CaptureDeviceLine;
            }
            set
            {
                _CaptureDeviceLine = value;
            }
        }

        private string _RenderDeviceInterface;
        public string RenderDeviceInterface
        {
            get
            {
                return _RenderDeviceInterface;
            }
            set
            {
                _RenderDeviceInterface = value;
            }
        }

        private string _RenderDeviceLine;
        public string RenderDeviceLine
        {
            get
            {
                return _RenderDeviceLine;
            }
            set
            {
                _RenderDeviceLine = value;
            }
        }

        public AudioDeviceControl()
        {
            captureDeviceDictionary = new Dictionary<string, List<AudioLineInfo>>();
            renderDeviceDictionary = new Dictionary<string, List<AudioLineInfo>>();
        }
    }
}
