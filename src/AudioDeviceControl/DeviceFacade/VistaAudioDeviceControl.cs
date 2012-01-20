using System;
using System.Collections.Generic;
using AudioDeviceControl.Exceptions;
using AudioDeviceControl.Extensions;
using CoreAudioApi;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.InteropServices;

namespace AudioDeviceControl
{
    /// <summary>
    /// This class is a facade to the Vista Core Audio API Master Volume Control: http://www.codeproject.com/KB/vista/CoreAudio.aspx
    /// 
    /// From the website of the Vista Core Audio API:
    /// 
    /// Windows Vista features a completely new set of user-mode audio components that provide per application volume control. 
    /// All legacy APIs such as the waveXxx and mixerXxx functions and DirectSound have been rebuilt on top of these new components 
    /// so that without any code changes all 'old' applications support this new audio API. This is a great thing except when your 
    /// application is reading or querying the master volume settings of the operating system because all of a sudden, you are no 
    /// longer controling the master volume of the operating system but only of your own application. 
    /// 
    /// 
    /// CORE AUDIO API:
    /// 
    /// The new API is COM based, and split into four sub APIs which roughly do the following:
    ///     * MMDevice API - This API allows enumeration and instancing of the available audio devices in the system.
    ///     * WASAPI - This API allows playback and recording of audio streams.
    ///     * DeviceTopology API - This API allows access to hardware features such as bass, treble, and auto gain control.
    ///     * EndpointVolume API - This API allows access to the Volume and Peak meters.
    ///     
    /// The MMDevice and EndpointVolume APIs are needed to control the master volume and mute settings, while the APIs themselves 
    /// are a huge improvement since the legacy functions lack a nice managed interface, making working with them somehow an 
    /// unpleasent experience in C#. 
    /// 
    /// Writing COM interop code is not for the novice and is error-prone, hence creating a wrapper to do all the plumbing for us 
    /// seems a good idea.
    /// 
    /// </summary>
    public class VistaAudioDeviceControl : AudioDeviceControl, IAudioDeviceControl
    {


        // The maximum String length of a device friendly name.
        // If the device friendly name is longer than 31 characters then we must cut it at the 31st character.
        private const int DEVICE_FRIENDLY_NAME_MAX_LENGTH = 31;

        // Set device id to default.
        private int deviceId = -1;

        private MMDevice captureDevice = null;
        private MMDevice renderDevice = null;

        // Wave format object used to "turn on" the audio capture device so that we can get peak feed data.
        private S57.AudioDeviceControl.Wave.WaveFormat waveFormat = null;
        private S57.AudioDeviceControl.Wave.WaveInRecorder waveInRecorder = null;

        public VistaAudioDeviceControl()
        {
            initAudioDeviceControl();
        }

        private void initAudioDeviceControl(){
            MMDeviceEnumerator devEnum = new MMDeviceEnumerator();

            if (devEnum.EnumerateAudioEndPoints(EDataFlow.eCapture, EDeviceState.DEVICE_STATE_ACTIVE).Count > 0)
            {
                // Set capture device to default.
                captureDevice = devEnum.GetDefaultAudioEndpoint(EDataFlow.eCapture, ERole.eMultimedia);
                CaptureDeviceInterface = getDeviceInterfaceFriendlyName(captureDevice);
                CaptureDeviceLine = captureDevice.FriendlyName;
            }

            if (devEnum.EnumerateAudioEndPoints(EDataFlow.eRender, EDeviceState.DEVICE_STATE_ACTIVE).Count > 0)
            {
                // Set render device to default.
                renderDevice = devEnum.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia);
                RenderDeviceInterface = getDeviceInterfaceFriendlyName(renderDevice);
                RenderDeviceLine = renderDevice.FriendlyName;
            }
        }

        public KeyValuePair<string, string> GetSelectedCaptureDeviceInfo()
        {
            KeyValuePair<string, string> kvp;

            if (captureDevice != null)
            {
                // Get the name of the default's device interface
                string deviceInterface = getDeviceInterfaceFriendlyName(captureDevice);

                // Create the key value pair that will contain the device interface name as a key and the line name as a value.
                kvp = new KeyValuePair<string, string>(deviceInterface, captureDevice.FriendlyName);

            }
            else
            {
                kvp = new KeyValuePair<string, string>("", "");
            }

            return kvp;
        }

        public KeyValuePair<string, string> GetWindowsSelectedCaptureDeviceInfo()
        {
            KeyValuePair<string, string> kvp;

            // Retrieve the device selected by default in windows sound configuration
            MMDeviceEnumerator devEnum = new MMDeviceEnumerator();
           
            if(devEnum.EnumerateAudioEndPoints(EDataFlow.eCapture, EDeviceState.DEVICE_STATE_ACTIVE).Count > 0){
    
                MMDevice captureDevice = devEnum.GetDefaultAudioEndpoint(EDataFlow.eCapture, ERole.eMultimedia);

                // Get the name of the default's device interface
                string deviceInterface = getDeviceInterfaceFriendlyName(captureDevice);

                // Create the key value pair that will contain the device interface name as a key and the line name as a value.
                kvp = new KeyValuePair<string, string>(deviceInterface, captureDevice.FriendlyName);

            }else{
                kvp = new KeyValuePair<string, string>("", "");
            }

           return kvp;
        }

        /// <summary>
        /// Returns the volume level of the default capture device.
        /// </summary>
        /// <returns></returns>
        public int GetCaptureVolume()
        {
            return (int)(captureDevice.AudioEndpointVolume.MasterVolumeLevelScalar * 100);
        }

        /// <summary>
        /// Returns the volume level of the default render device.
        /// </summary>
        /// <returns></returns>
        public int GetRenderVolume()
        {
            return (int)(renderDevice.AudioEndpointVolume.MasterVolumeLevelScalar * 100);
        }

        public int SetCaptureVolume(int volume)
        {
            return setDeviceVolume(captureDevice, volume);
        }

        public int SetRenderVolume(int volume)
        {
            return setDeviceVolume(renderDevice, volume);
        }

        public int IncrementCaptureVolume()
        {
            return incrementDeviceVolume(captureDevice);
        }

        public int IncrementRenderVolume()
        {
            return incrementDeviceVolume(renderDevice);
        }

        public int DecrementCaptureVolume()
        {
            return decrementDeviceVolume(captureDevice);
        }

        public int DecrementRenderVolume()
        {
            return decrementDeviceVolume(renderDevice);
        }

        public bool ToggleCaptureMute()
        {
            return toggleDeviceMute(captureDevice);
        }

        public bool ToggleRenderMute()
        {
            return toggleDeviceMute(renderDevice);
        }

        public bool SetCaptureMute(bool mute)
        {
            return setDeviceMute(captureDevice, mute);
        }

        public bool SetRenderMute(bool mute)
        {
            return setDeviceMute(renderDevice, mute);
        }

        public bool GetCaptureMuteStatus()
        {
            return captureDevice.AudioEndpointVolume.Mute;
        }

        public bool GetRenderMuteStatus()
        {
            return renderDevice.AudioEndpointVolume.Mute;
        }

        public int GetCaptureDeviceMasterPeakValue()
        {
            // init WaveInRecorder if it has been disposed of.
            if (waveFormat == null)
            {
                // We are targetting a new capture device, so create a new WaveInRecorder instance.
                initWaveInRecorder();
            }
            lock (deviceChangeLock)
            {
                return (int)getDeviceMasterPeakValue(captureDevice);
            }
        }

        
        public void DisposeCaptureDeviceMasterPeakValue()
        {  
            if (waveInRecorder != null)
            {
                waveInRecorder.Dispose();
                waveFormat = null;
            }
        }
        

        public int GetRenderDeviceMasterPeakValue()
        {
            lock (deviceChangeLock)
            {
                return (int)getDeviceMasterPeakValue(renderDevice);
            }
        }

        /// <summary>
        /// Sets the capture device that we are to interface with.
        /// We must call this method every time we wish to change the capture device we wish to interface with.
        /// </summary>
        /// <param name="deviceInterfaceName">The name of the device interface (e.g. "XYZ Audio Adapter")</param>
        /// <param name="deviceLineName">The name of the line (e.g. "Microphone", "Line In"...</param>
        public void SetCaptureDevice(string deviceInterfaceName, string deviceLineName)
        {
            CaptureDeviceInterface = deviceInterfaceName;
            CaptureDeviceLine = deviceLineName;
            captureDevice = getCaptureDevice();

            // We are targetting a new capture device, so create a new WaveInRecorder instance.
            initWaveInRecorder();
        }

        /// <summary>
        /// Sets the render device that we are to interface with.
        /// We must call this method every time we wish to change the render device we wish to interface with.
        /// </summary>
        /// <param name="deviceInterfaceName">The name of the device interface (e.g. "XYZ Audio Adapter")</param>
        /// <param name="deviceLineName">The name of the line (e.g. "Microphone", "Line In"...</param>
        public void SetRenderDevice(string deviceInterfaceName, string deviceLineName)
        {
            RenderDeviceInterface = deviceInterfaceName;
            RenderDeviceLine = deviceLineName;
            renderDevice = getRenderDevice();
        }

        /// <summary>
        /// Get a list of capture device names.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, List<AudioLineInfo>> GetCaptureDevicesInfo()
        {
            MMDeviceCollection deviceCollection = getCaptureDeviceCollection();
            return getDeviceNames(deviceCollection, captureDeviceDictionary);
        }

        /// <summary>
        /// Get a list of render device names.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, List<AudioLineInfo>> GetRenderDevicesInfo()
        {
            MMDeviceCollection deviceCollection = getRenderDeviceCollection();
            return getDeviceNames(deviceCollection, renderDeviceDictionary);
        }


        /// <summary>
        /// Init WaveInRecorder for peak feed.
        /// </summary>
        /// 
        private void initWaveInRecorder()
        {
            lock (deviceChangeLock)
            {
                // Dispose waveInRecorder if it has been previously created.
                DisposeCaptureDeviceMasterPeakValue();

                waveFormat = new S57.AudioDeviceControl.Wave.WaveFormat(44100, 16, 2);
                waveInRecorder = new S57.AudioDeviceControl.Wave.WaveInRecorder(deviceId, waveFormat, 16384, 3, null);
            }
        }
        

        /// <summary>
        /// Set the volume of the device.
        /// </summary>
        /// <param name="eDataFlow"></param>
        /// <param name="volume"></param>
        /// <returns></returns>
        private int setDeviceVolume(MMDevice device, int volume)
        {
            // Enforce volume range.
            if (volume > 100)
            {
                volume = 100;
            }
            else if (volume < 0)
            {
                volume = 0;
            }

            device.AudioEndpointVolume.MasterVolumeLevelScalar = ((float)volume / 100);
            return (int)(device.AudioEndpointVolume.MasterVolumeLevelScalar * 100);
        }

        
        /// <summary>
        /// Get the peak meter of the device.
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        private float getDeviceMasterPeakValue(MMDevice device)
        {
            return device.AudioMeterInformation.MasterPeakValue * 100;
        }


    
        /// <summary>
        /// Get the capture device object.
        /// </summary>
        /// <returns></returns>
        private MMDevice getCaptureDevice()
        {
            MMDeviceCollection deviceCollection = getCaptureDeviceCollection();
            return getDevice(deviceCollection, CaptureDeviceInterface, CaptureDeviceLine);
        }

        /// <summary>
        /// Get the render device object.
        /// </summary>
        /// <returns></returns>
        private MMDevice getRenderDevice()
        {
            MMDeviceCollection deviceCollection = getRenderDeviceCollection();
            return getDevice(deviceCollection, RenderDeviceInterface, RenderDeviceLine);
            
        }
 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceCollection"></param>
        /// <returns></returns>
        private MMDevice getDevice(MMDeviceCollection deviceCollection, string deviceInterface, string deviceLine)
        {
            bool deviceInterfaceFound = false;

            for (int i = 0; i < deviceCollection.Count; i++)
            {
                MMDevice device = deviceCollection[i];
                string currentDeviceInterface = getDeviceInterfaceFriendlyName(device);

                if (currentDeviceInterface == deviceInterface)
                {
                    deviceInterfaceFound = true;
                    if (device.FriendlyName == deviceLine)
                    {
                        // Set the device id
                        deviceId = i;

                        // return the device.
                        return device;
                    }
                }
            }

            if (!deviceInterfaceFound)
            {
                throw new DeviceInterfaceException(deviceInterface, "The " + deviceInterface + " device interface was not found.");
            }
            else
            {
                throw new DeviceLineException(deviceInterface, deviceLine, "The " + deviceLine + " device line was not found for the " + deviceInterface + " interface.");
            }
        }

        
        /// <summary>
        /// Increment the volume of a specified device.
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        private int incrementDeviceVolume(MMDevice device)
        {
            device.AudioEndpointVolume.VolumeStepUp();
            return (int)(device.AudioEndpointVolume.MasterVolumeLevelScalar * 100);
        }

        
        /// <summary>
        /// Decrement the volume of a specified device.
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        private int decrementDeviceVolume(MMDevice device)
        {
            device.AudioEndpointVolume.VolumeStepDown();
            return (int)(device.AudioEndpointVolume.MasterVolumeLevelScalar * 100);

        }



        /// <summary>
        /// Toggles mutes for the specified device.
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        private bool toggleDeviceMute(MMDevice device)
        {
            device.AudioEndpointVolume.Mute = !device.AudioEndpointVolume.Mute;
            return device.AudioEndpointVolume.Mute;
        }

        /// <summary>
        /// Sets the mute status for the specified device.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="mute"></param>
        /// <returns></returns>
        private bool setDeviceMute(MMDevice device, bool mute)
        {
            device.AudioEndpointVolume.Mute = mute;
            return device.AudioEndpointVolume.Mute;
        }



        /// <summary>
        /// Retrieve the collection of capture devices.
        /// </summary>
        /// <returns></returns>
        private MMDeviceCollection getCaptureDeviceCollection()
        {
            MMDeviceEnumerator devEnum = new MMDeviceEnumerator();
            return devEnum.EnumerateAudioEndPoints(EDataFlow.eCapture, EDeviceState.DEVICE_STATE_ACTIVE);
        }


        /// <summary>
        /// Retrieve the collection of render devices.
        /// </summary>
        /// <returns></returns>
        private MMDeviceCollection getRenderDeviceCollection()
        {
            MMDeviceEnumerator devEnum = new MMDeviceEnumerator();
            return devEnum.EnumerateAudioEndPoints(EDataFlow.eRender, EDeviceState.DEVICE_STATE_ACTIVE);
        }

        /// <summary>
        /// Retrieve all the devices.
        /// </summary>
        /// <returns></returns>
        private MMDeviceCollection getAllDeviceCollection()
        {
            MMDeviceEnumerator devEnum = new MMDeviceEnumerator();
            return devEnum.EnumerateAudioEndPoints(EDataFlow.eAll, EDeviceState.DEVICE_STATE_ACTIVE);
        }

        private Dictionary<string, List<AudioLineInfo>> getDeviceNames(MMDeviceCollection deviceCollection, Dictionary<string, List<AudioLineInfo>> deviceDictionary)
        {
            // Clear dictionary of previously added data (in case some devices have been unplugged or new ones have been plugged in)
            deviceDictionary.Clear() ;

            for (int i = 0; i < deviceCollection.Count; i++)
            {
                MMDevice device = deviceCollection[i];

                // Get the device's interface name
                string deviceInterfaceFriendlyName = getDeviceInterfaceFriendlyName(device);

                // Which is set as a dictionary key if a different line already exists for it...
                if (!deviceDictionary.ContainsKey(deviceInterfaceFriendlyName))
                {
                    // If this device interface name hasn't already set as a dictionary key, then create a new entry for its
                    deviceDictionary[deviceInterfaceFriendlyName] = new List<AudioLineInfo>();
                }

                // Get the guid of the device.
                string deviceGuid = device.ID.Substring(device.ID.LastIndexOf("{") + 1, device.ID.LastIndexOf("}") - device.ID.LastIndexOf("{") -1);

                // Associate to the device's interface name a pair consisting of line name (confusingly known as the device friendly name) and the device's friendly name (the REAL friendly name) 
                string realDeviceFriendlyName = getDeviceFriendlyName(new Guid(deviceGuid), true);

                // Gather device line properties to associate with the device's interface name.
                AudioLineInfo audioLineInfo = new AudioLineInfo();

                // Line name is confusingly known as the device friendly name.
                audioLineInfo.LineName = device.FriendlyName;

                // The device's friendly name (the REAL friendly name). 
                audioLineInfo.FriendlyName = realDeviceFriendlyName;

                // Device line is always mutable.
                audioLineInfo.Mutable = true;
                
                // Associate the device's interface name with device line properties.
                deviceDictionary[deviceInterfaceFriendlyName].Add(audioLineInfo);

            }

            return deviceDictionary;
        }


        private string getDeviceInterfaceFriendlyName(MMDevice device){
            try{

                PropertyStore propStore = device.Properties;

                // Use our custom PropertyStore Extension object so that we can retrieve a property value based on the guid AND the pid.
                PropertyStoreExt propStoreExt = new PropertyStoreExt(propStore);
                PropertyStoreProperty propStoreProperty = propStoreExt[PKEYExt.PKEYContainer_DeviceInterface_FriendlyName.Guid, PKEYExt.PKEYContainer_DeviceInterface_FriendlyName.Pid];

                return (String)propStoreProperty.Value;
      
            }catch(Exception){
                return "Unknown";
            }
        }


        private string getDeviceFriendlyName(Guid deviceGuid, bool trimAtLengthLimit)
        {
            // Retrieve the available DirectSound capture devices
            Microsoft.DirectX.DirectSound.CaptureDevicesCollection audioDevices = new Microsoft.DirectX.DirectSound.CaptureDevicesCollection();

            for (int deviceId = 0; deviceId < audioDevices.Count; deviceId++)
            {
                if (audioDevices[deviceId].DriverGuid == deviceGuid)
                {
                    if (!String.IsNullOrEmpty(audioDevices[deviceId].Description))
                    {
                        if (trimAtLengthLimit && audioDevices[deviceId].Description.Length > DEVICE_FRIENDLY_NAME_MAX_LENGTH)
                        {
                            return audioDevices[deviceId].Description.Substring(0, DEVICE_FRIENDLY_NAME_MAX_LENGTH);
                        }
                        else
                        {
                            return audioDevices[deviceId].Description;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            return null;
        }
    }
}
