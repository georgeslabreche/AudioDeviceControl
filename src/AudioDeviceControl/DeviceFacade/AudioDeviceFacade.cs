using System;

namespace AudioDeviceControl
{
    public class AudioDeviceFacade : IAudioDeviceFacade
    {
        private IAudioDeviceControl _AudioDeviceControl;
        public IAudioDeviceControl AudioDeviceControl
        {
            get
            {
                return _AudioDeviceControl;
            }
            set
            {
                _AudioDeviceControl = value;
            }
        }
        
        public AudioDeviceFacade() {
            string operatingSystem = PlatformInfo.getOperatingSystem();

            if (operatingSystem == PlatformInfo.WIN_XP)
            {
                // If LoaderLock exception is throw here you need to go to Debug > Exceptions... > Managed Debugging Assistants. 
                // Uncheck Thrown checkbox for LoaderLock Exception.
                AudioDeviceControl = new XPAudioDeviceControl();
            }
            else if (operatingSystem == PlatformInfo.WIN_VISTA)
            {
                AudioDeviceControl = new VistaAudioDeviceControl();
            }
            else if (operatingSystem == PlatformInfo.WIN_7)
            {
                AudioDeviceControl = new Win7AudioDeviceControl();
            }
            else
            {
                AudioDeviceControl = new UnsupportedAudioDeviceControl();
            }       
        }
    }
}
