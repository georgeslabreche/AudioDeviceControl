using System;
using System.Collections.Generic;
using System.Text;
using CoreAudioApi;
using CoreAudioApi.Interfaces;
using System.Runtime.InteropServices;

namespace AudioDeviceControl.DeviceFacade
{
    public class AudioEndpointVolumeCallback : IAudioEndpointVolumeCallback
    {
        public AudioEndpointVolume _Parent;

        public AudioEndpointVolumeCallback(AudioEndpointVolume parent)
        {
            _Parent = parent;
        }

        public int OnNotify(IntPtr NotifyData)
        {
            return 0;
        }
    }
}
