using System;
using System.Collections.Generic;
using System.Text;

namespace AudioDeviceControl
{
    public class UnsupportedAudioDeviceControl : IAudioDeviceControl
    {

        public KeyValuePair<string, string> GetSelectedCaptureDeviceInfo()
        {
            throw new PlatformNotSupportedException();
        }

        public KeyValuePair<string, string> GetWindowsSelectedCaptureDeviceInfo()
        {
            throw new PlatformNotSupportedException();
        }

        public Dictionary<string, List<AudioLineInfo>> GetCaptureDevicesInfo()
        {
            throw new PlatformNotSupportedException();
        }

        public Dictionary<string, List<AudioLineInfo>> GetRenderDevicesInfo()
        {
            throw new PlatformNotSupportedException();
        }

        public void SetCaptureDevice(string deviceInterface, string deviceLine)
        {
            throw new PlatformNotSupportedException();
        }

        public void SetRenderDevice(string deviceInterface, string deviceLine)
        {
            throw new PlatformNotSupportedException();
        }

        public int GetCaptureVolume()
        {
            throw new PlatformNotSupportedException();
        }

        public int GetRenderVolume()
        {
            throw new PlatformNotSupportedException();
        }

        public int SetCaptureVolume(int volume)
        {
            throw new PlatformNotSupportedException();
        }

        public int SetRenderVolume(int volume)
        {
            throw new PlatformNotSupportedException();
        }

        public int IncrementCaptureVolume()
        {
            throw new PlatformNotSupportedException();
        }

        public int IncrementRenderVolume()
        {
            throw new PlatformNotSupportedException();
        }

        public int DecrementCaptureVolume()
        {
            throw new PlatformNotSupportedException();
        }

        public int DecrementRenderVolume()
        {
            throw new PlatformNotSupportedException();
        }

        public bool ToggleCaptureMute()
        {
            throw new PlatformNotSupportedException();
        }

        public bool ToggleRenderMute()
        {
            throw new PlatformNotSupportedException();
        }

        public bool SetCaptureMute(bool mute)
        {
            throw new PlatformNotSupportedException();
        }

        public bool SetRenderMute(bool mute)
        {
            throw new PlatformNotSupportedException();
        }

        public bool GetCaptureMuteStatus()
        {
            throw new PlatformNotSupportedException();
        }

        public bool GetRenderMuteStatus()
        {
            throw new PlatformNotSupportedException();
        }

        public int GetCaptureDeviceMasterPeakValue()
        {
            throw new PlatformNotSupportedException();
        }

        public void DisposeCaptureDeviceMasterPeakValue()
        {
            throw new PlatformNotSupportedException();
        }

        public int GetRenderDeviceMasterPeakValue()
        {
            throw new PlatformNotSupportedException();
        }

    }
}
