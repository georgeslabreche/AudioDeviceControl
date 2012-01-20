
using System.Collections.Generic;
namespace AudioDeviceControl
{
    public interface IAudioDeviceControl
    {
        KeyValuePair<string, string> GetSelectedCaptureDeviceInfo();

        KeyValuePair<string, string> GetWindowsSelectedCaptureDeviceInfo();

        Dictionary<string, List<AudioLineInfo>> GetCaptureDevicesInfo();

        Dictionary<string, List<AudioLineInfo>> GetRenderDevicesInfo();

        void SetCaptureDevice(string deviceInterface, string deviceLine);

        void SetRenderDevice(string deviceInterface, string deviceLine);

        int GetCaptureVolume();

        int GetRenderVolume();

        int SetCaptureVolume(int volume);

        int SetRenderVolume(int volume);

        int IncrementCaptureVolume();

        int IncrementRenderVolume();

        int DecrementCaptureVolume();

        int DecrementRenderVolume();

        bool ToggleCaptureMute();

        bool ToggleRenderMute();

        bool SetCaptureMute(bool mute);

        bool SetRenderMute(bool mute);

        bool GetCaptureMuteStatus();

        bool GetRenderMuteStatus();

        int GetCaptureDeviceMasterPeakValue();

        void DisposeCaptureDeviceMasterPeakValue();

        int GetRenderDeviceMasterPeakValue();

    }
}
