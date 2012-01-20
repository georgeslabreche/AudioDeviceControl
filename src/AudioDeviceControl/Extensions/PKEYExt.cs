using System;
using System.Collections.Generic;
using System.Text;

namespace AudioDeviceControl.Extensions
{
    /// <summary>
    /// The CoreAudioApi's PKEY implementation is all wrong in the guid value for PKEY_DeviceInterface_FriendlyName: It used the one assigned to PKEY_Device_FriendlyName.
    /// Furthermore, there isn't any pid indication.
    /// </summary>
    public static class PKEYExt
    {
        
        public static readonly Guid PKEY_Device_FriendlyName = new Guid( 0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0);
        public static readonly Guid PKEY_DeviceInterface_FriendlyName = new Guid(0xb3f8fa53, 0x0004, 0x438e, 0x90, 0x03, 0x51, 0xa4, 0x6e, 0x13, 0x9b, 0xfc);

        public static readonly PKEYExtContainer PKEYContainer_Device_FriendlyName = new PKEYExtContainer(PKEY_Device_FriendlyName, 14);
        public static readonly PKEYExtContainer PKEYContainer_DeviceInterface_FriendlyName = new PKEYExtContainer(PKEY_DeviceInterface_FriendlyName, 6);
    }
}
