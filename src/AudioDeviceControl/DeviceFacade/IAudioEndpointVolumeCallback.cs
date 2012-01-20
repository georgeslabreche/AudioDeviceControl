using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace AudioDeviceControl.DeviceFacade
{
    //[System.Guid("657804FA-D6AD-4496-8A60-352752AF4F89"),
    // InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAudioEndpointVolumeCallback
    {
        int OnNotify(IntPtr pNotifyData);
    };
}
