using System;
using System.Threading;
using AudioDeviceControl.Exceptions;
using WaveLib.AudioMixer;
using System.Collections;
using System.Collections.Generic;

namespace AudioDeviceControl
{
    /// <summary>
    /// This class is a facade to the Audio Library: http://www.codeproject.com/KB/audio-video/AudioLib.aspx
    /// 
    /// From the website of Audio Library:
    /// 
    /// The main object Mixers contains two Mixer objects, Playback and Recording; those will work with default devices, 
    /// but can be changed by setting the property Mixers.Playback.DeviceId or Mixers.Recording.DeviceId.
    /// 
    /// I made it as simple as possible by hiding all the flat Win32 API implementation from the developer and creating 
    /// a hierarchical set of objects.
    /// 
    /// Every mixer is autonomous, meaning you can have Playback mixer set to one soundcard and Recording mixer to 
    /// another one. Also each one contains two arrays or MixerLines (Lines, UserLines).
    /// 
    /// Lines object will contain all lines inside the mixer, for example all the lines that don't have controls 
    /// associated with it or lines that are not source lines.
    /// 
    /// UserLines will contains all lines that the developer can interact with, having controls like Volume, Mute, 
    /// Fadder, Bass, etc. (basically it is the same as Lines object but a filter was applied to it).
    /// 
    /// Every Line will contain a collection of controls, like Mute, Volume, Bass, Fader, etc.
    /// </summary>
    public class XPAudioDeviceControl : AudioDeviceControl, IAudioDeviceControl
    {

        private const String MIXERLINE_CAPTURE = "Capture";
        private const String MIXERLINE_RECORDING_CONTROL = "Recording Control";

        private string _UnselectableMixerLineName = "";

        private int _VolumeUnit = 1;

        public int VolumeUnit
        {
            get
            {
                return _VolumeUnit;
            }
            set
            {
                _VolumeUnit = value;
            }
        }

        private Mixers mixers = null;
        
        private Mixer captureMixer;
        private Mixer renderMixer;

        private const int SAMPLE_FREQUENCY = 44100;
        private const int MIN_VOLUME_VALUE = 0;
        private const int MAX_VOLUME_VALUE = 65535;
        private const int MAX_SAMPLE_VALUE = 32768;
        private const int SAMPLES = 8;
        private static int[] SAMPLE_FORMAT_ARRAY = { SAMPLES, 2, 1 };

        private Microsoft.DirectX.DirectSound.CaptureBuffer buffer;
        private int sampleDelay = 100;
        private int frameDelay = 10;


        public XPAudioDeviceControl()
        {
            //audioDevices = new Microsoft.DirectX.DirectSound.CaptureDevicesCollection();

            mixers = new Mixers(true);
            captureMixer = mixers.Recording;
            renderMixer = mixers.Playback;

            if (captureMixer != null)
            {
                // Set the current default values:
                // For the Device interface
                CaptureDeviceInterface = captureMixer.DeviceDetail.MixerName;

                // For the Device line
                if (captureMixer.UserLines.Count > 0)
                {
                    _UnselectableMixerLineName = captureMixer.UserLines[0].Name;
                    MixerLine selectedCaptureMixerLine = getSelectedMixerLine(captureMixer);
                    CaptureDeviceLine = selectedCaptureMixerLine.Name;
                }
                else
                {
                    throw new DeviceInterfaceException(CaptureDeviceInterface, "No capture line detected for the " + CaptureDeviceInterface + " device interface.");
                }

            }

            // Not supported for now.
            // RenderDeviceInterface = renderMixer.DeviceDetail.MixerName;
            // MixerLine selectedRenderMixerLine = getSelectedMixerLine(renderMixer);
            // RenderDeviceLine = selectedRenderMixerLine.Name;

        }


        public KeyValuePair<string, string> GetSelectedCaptureDeviceInfo()
        {
            KeyValuePair<string, string> kvp;

            if (captureMixer != null)
            {
                MixerLine selectedCaptureMixerLine = getSelectedMixerLine(captureMixer);

                // Create the key value pair that will contain the device interface name as a key and the line name as a value.
                kvp = new KeyValuePair<string, string>(captureMixer.DeviceDetail.MixerName, selectedCaptureMixerLine.Name);

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
            Mixers mixers = new Mixers(true);
            Mixer captureMixer = mixers.Recording;

            if (captureMixer != null)
            {
                MixerLine selectedCaptureMixerLine = getSelectedMixerLine(captureMixer);

                // Create the key value pair that will contain the device interface name as a key and the line name as a value.
                kvp = new KeyValuePair<string, string>(captureMixer.DeviceDetail.MixerName, selectedCaptureMixerLine.Name);
            }
            else
            {
                kvp = new KeyValuePair<string, string>("", "");
            }

            return kvp;
        }

        public Dictionary<string, List<AudioLineInfo>> GetCaptureDevicesInfo()
        {
            return getDeviceNames(captureMixer, captureDeviceDictionary);
        }

        public Dictionary<string, List<AudioLineInfo>> GetRenderDevicesInfo()
        {
            return getDeviceNames(renderMixer, renderDeviceDictionary);
        }

        public Dictionary<string, List<AudioLineInfo>> getDeviceNames(Mixer mixer, Dictionary<string, List<AudioLineInfo>> deviceDictionary)
        {
            // Clear dictionary of previously added data (in case some devices have been unplugged or new ones have been plugged in)
            deviceDictionary.Clear();
    
            foreach (MixerDetail mixerDetail in mixer.Devices)
            {
                // Get the device's interface name
                string deviceInterfaceFriendlyName = mixerDetail.MixerName;

                // Which is set as a dictionary key if a different line already exists for it...
                if (!deviceDictionary.ContainsKey(deviceInterfaceFriendlyName))
                {
                    // If this device interface name hasn't already set as a dictionary key, then create a new entry for it.
                    deviceDictionary[deviceInterfaceFriendlyName] = new List<AudioLineInfo>();
                }

                // Create a mixer object based on the current deviceId so that we can retrieve the lines.
                mixers = new Mixers(true);
                Mixer mixer2 = null;

                // Get proper mixer type
                if (mixer.MixerType == MixerType.Playback){
                    mixer2 = mixers.Playback;
                }

                else if(mixer.MixerType == MixerType.Recording){
                    mixer2 = mixers.Recording;
                }

                // Set the device id
                mixer2.DeviceId = mixerDetail.DeviceId;

                // Retrieve the mixer lines and set them in the dictionar
                foreach (MixerLine mixerLine in mixer2.UserLines)
                {
                    /**
                     * Create the line name and interface friendly name (mixer name) pair to associate with the mixer name.
                     * 
                     * But what? We are associating the mixer name with the mixer name? Isn't this unecessary redundancy?
                     * We have too because these two values are only identical under XP. In Vista and Windows7 The will be different so this
                     * is why we structure the response this way.
                     * Check out the Vista implementation for further details.
                     */
                    //KeyValuePair<string, string> lineNameAndFriendlyNamePair = new KeyValuePair<string, string>(mixerLine.Name, mixerDetail.MixerName);

                    // Gather device line properties to associate with the device's interface name.
                    AudioLineInfo audioLineInfo = new AudioLineInfo();

                    // Line name is confusingly known as the device friendly name.
                    audioLineInfo.LineName = mixerLine.Name;

                    // The device's friendly name (the REAL friendly name). 
                    audioLineInfo.FriendlyName = mixerDetail.MixerName;

                    // Is the device line mutable
                    audioLineInfo.Mutable = mixerLine.ContainsMute;

                    deviceDictionary[mixerDetail.MixerName].Add(audioLineInfo);
               
                }
  
            }

            return deviceDictionary;
        }

        
        public int GetCaptureVolume()
        {
            MixerLine mixerLine = getSelectedMixerLine(captureMixer);
            return normalizeVolumeValue(mixerLine.Volume, mixerLine.VolumeMax);    
        }

        
        public int SetCaptureVolume(int percentVolume)
        {     
            MixerLine mixerLine = getSelectedMixerLine(captureMixer);
            int volume = denormalizeVolumeValue(percentVolume, mixerLine.VolumeMax);

            mixerLine.Volume = volume;
            return normalizeVolumeValue(mixerLine.Volume, mixerLine.VolumeMax);
        }


        public int IncrementCaptureVolume()
        {

            int volume = GetCaptureVolume();

            if (volume < 100)
            {
                int newVolume = volume + VolumeUnit;
                SetCaptureVolume(newVolume);

                return newVolume;
            }

            return 100;

        }

        public int DecrementCaptureVolume()
        {
            int volume = GetCaptureVolume();
            if (volume > 0)
            {
                int newVolume = volume - VolumeUnit;
                SetCaptureVolume(newVolume);

                return newVolume;
            }

            return 0;
        }

        public bool ToggleCaptureMute()
        {
            MixerLine mixerLine = getSelectedMixerLine(captureMixer);
            if (mixerLine.ContainsMute)
            {
                mixerLine.Mute = !mixerLine.Mute;
            }

            return mixerLine.Mute;
        }

        public bool SetCaptureMute(bool mute)
        {
            MixerLine mixerLine = getSelectedMixerLine(captureMixer);
            if (mixerLine.ContainsMute)
            {
                mixerLine.Mute = mute;
            }

            return mixerLine.Mute;
        }

        public bool GetCaptureMuteStatus()
        {
            MixerLine mixerLine = getSelectedMixerLine(captureMixer);
            return mixerLine.Mute;
        }

        

        public int GetCaptureDeviceMasterPeakValue()
        {
            lock (deviceChangeLock)
            {
                int masterPeakLevel = 0;

                int tempFrameDelay = frameDelay;
                int tempSampleDelay = sampleDelay;
                if (buffer != null)
                {
                    Array samples = buffer.Read(0, typeof(Int16), Microsoft.DirectX.DirectSound.LockFlag.FromWriteCursor, SAMPLE_FORMAT_ARRAY);

                    // for each channel, determine the step size necessary for each iteration
                    int leftGoal = 0;
                    int rightGoal = 0;

                    // average across all samples to get the goals
                    for (int i = 0; i < SAMPLES; i++)
                    {
                        leftGoal += (Int16)samples.GetValue(i, 0, 0);
                        rightGoal += (Int16)samples.GetValue(i, 1, 0);
                    }

                    leftGoal = (int)Math.Abs(leftGoal / SAMPLES);
                    rightGoal = (int)Math.Abs(rightGoal / SAMPLES);

                    masterPeakLevel = (leftGoal + rightGoal) / 2;

                    int percentMasterPeakLevel = (masterPeakLevel * 100) / MAX_SAMPLE_VALUE;

                    return percentMasterPeakLevel;
                }

                return 0;
                
            }

        }
        
        public void DisposeCaptureDeviceMasterPeakValue()
        {
            // terminate previous device's DirectSound setup (for peak feed).
            terminateMasterPeakRetrieval();
        }


        public int GetRenderVolume()
        {
            throw new NotImplementedException();
        }

        public int SetRenderVolume(int volume)
        {
            throw new NotImplementedException();
        }

        public int IncrementRenderVolume()
        {
            throw new NotImplementedException();
        }

        public int DecrementRenderVolume()
        {
            throw new NotImplementedException();
        }

        public bool ToggleRenderMute()
        {
            throw new NotImplementedException();
        }

        public bool SetRenderMute(bool mute)
        {
            throw new NotImplementedException();
        }

        public bool GetRenderMuteStatus()
        {
            throw new NotImplementedException();
        }

        public int GetRenderDeviceMasterPeakValue()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Sets the capture device that we are to interface with.
        /// We must call this method every time we wish to change the capture device we wish to interface with.
        /// </summary>
        /// <param name="deviceInterfaceName">The name of the device interface (e.g. "XYZ Audio Adapter")</param>
        /// <param name="deviceLineName">The name of the line (e.g. "Microphone", "Line In"...</param>
        public void SetCaptureDevice(string deviceInterface, string deviceLine)
        {
            lock (deviceChangeLock)
            {
                CaptureDeviceInterface = deviceInterface;
                CaptureDeviceLine = deviceLine;

                setDeviceInterface(captureMixer, CaptureDeviceInterface);
                IEnumerator captureLineEnum = captureMixer.UserLines.GetEnumerator();
                setDeviceLine(captureLineEnum, CaptureDeviceInterface, CaptureDeviceLine);

                // terminate previous device's DirectSound setup (for peak feed).
                terminateMasterPeakRetrieval();

                // init new device's DirectSound setup (for peak feed).
                initMasterPeakRetrieval();
            }
        }

        /// <summary>
        /// Sets the render device that we are to interface with.
        /// We must call this method every time we wish to change the render device we wish to interface with.
        /// </summary>
        /// <param name="deviceInterfaceName">The name of the device interface (e.g. "XYZ Audio Adapter")</param>
        /// <param name="deviceLineName">The name of the line (e.g. "Microphone", "Line In"...</param>
        public void SetRenderDevice(string deviceInterface, string deviceLine)
        {
            RenderDeviceInterface = deviceInterface;
            RenderDeviceLine = deviceLine;

            setDeviceInterface(renderMixer, RenderDeviceInterface);
            IEnumerator recordinLineEnum = renderMixer.UserLines.GetEnumerator();
            setDeviceLine(recordinLineEnum, RenderDeviceInterface, RenderDeviceLine);
        }



        /// <summary>
        /// Get the device Mixer Line that has been set to operate on.
        /// </summary>
        /// <param name="mixer"></param>
        /// <returns></returns>
        private MixerLine getSelectedMixerLine(Mixer mixer)
        {

            IEnumerator recordinLineEnum = mixer.UserLines.GetEnumerator();
            MixerLine mixerLine = null;

            while (recordinLineEnum.MoveNext())
            {
                mixerLine = (MixerLine)recordinLineEnum.Current;
                if (mixerLine.ContainsSelected)
                {
                    if (mixerLine.Selected)
                    {
                        return mixerLine;
                    }

                }
                else if (mixerLine.Name == _UnselectableMixerLineName)
                {
                    return mixerLine;
                }
            }


            string deviceLineName = null;
            if (mixer.MixerType == MixerType.Playback)
            {
                deviceLineName = RenderDeviceLine;
            }
            else
            {
                deviceLineName = CaptureDeviceLine;
            }

            throw new UnselectableDeviceLineException(mixer.DeviceDetail.MixerName, deviceLineName, "The target device line was unselectable.");
        }


        /// <summary>
        /// Denormalize a pecent volume value int a device specifice value.
        /// </summary>
        /// <param name="percentVolume"></param>
        /// <param name="denormalizedVolumeMax"></param>
        /// <returns></returns>
        private int denormalizeVolumeValue(int percentVolume, int denormalizedVolumeMax)
        {
            // Enforce maximum and minimum value that can be set.
            if (percentVolume > 100)
            {
                percentVolume = 100;
            }
            else if (percentVolume < 0)
            {
                percentVolume = 0;
            }


            double denormalizedVolume = ((double)percentVolume / 100) * denormalizedVolumeMax;
            return (int)denormalizedVolume;
        }

        /// <summary>
        /// Normalize the given volume value to a percentage value.
        /// </summary>
        /// <param name="denormalizedVolume"></param>
        /// <param name="denormalizedVolumeMax"></param>
        /// <returns></returns>
        private int normalizeVolumeValue(int denormalizedVolume, int denormalizedVolumeMax)
        {
            double percentVolume = Math.Round(((double)denormalizedVolume / (double)denormalizedVolumeMax) * 100);
            return (int)percentVolume;
        }

        

        private void initMasterPeakRetrieval()
        {
            Microsoft.DirectX.DirectSound.CaptureDevicesCollection audioDevices = new Microsoft.DirectX.DirectSound.CaptureDevicesCollection();
            Microsoft.DirectX.DirectSound.DeviceInformation deviceInformation = audioDevices[captureMixer.DeviceId];
            initMasterPeakRetrieval(deviceInformation);
        }

        private void initMasterPeakRetrieval(string deviceName)
        {
            Microsoft.DirectX.DirectSound.CaptureDevicesCollection audioDevices = new Microsoft.DirectX.DirectSound.CaptureDevicesCollection();
            for (int deviceId = 0; deviceId < audioDevices.Count; deviceId++)
            {
                if (audioDevices[deviceId].Description == deviceName)
                {
                    initMasterPeakRetrieval(audioDevices[deviceId]);
                    break;
                }
            }
        }

        private void initMasterPeakRetrieval(Microsoft.DirectX.DirectSound.DeviceInformation deviceInformation)
        {
   
            Microsoft.DirectX.DirectSound.CaptureDevicesCollection audioDevices = new Microsoft.DirectX.DirectSound.CaptureDevicesCollection();

            for (int deviceIndex = 0; deviceIndex < audioDevices.Count; deviceIndex++)
            {
                if (captureMixer.DeviceDetail.MixerName == audioDevices[deviceIndex].Description)
                {
                    // captureMixer.DeviceDetail.MixerName
                    // initialize the capture buffer on the defaut device
                    Microsoft.DirectX.DirectSound.Capture cap = new Microsoft.DirectX.DirectSound.Capture(audioDevices[deviceIndex].DriverGuid);
                    Microsoft.DirectX.DirectSound.CaptureBufferDescription desc = new Microsoft.DirectX.DirectSound.CaptureBufferDescription();
                    Microsoft.DirectX.DirectSound.WaveFormat wf = new Microsoft.DirectX.DirectSound.WaveFormat();
                    wf.BitsPerSample = 16;
                    wf.SamplesPerSecond = SAMPLE_FREQUENCY;
                    wf.Channels = 2;
                    wf.BlockAlign = (short)(wf.Channels * wf.BitsPerSample / 8);
                    wf.AverageBytesPerSecond = wf.BlockAlign * wf.SamplesPerSecond;
                    wf.FormatTag = Microsoft.DirectX.DirectSound.WaveFormatTag.Pcm;

                    desc.Format = wf;
                    desc.BufferBytes = SAMPLES * wf.BlockAlign;

                    buffer = new Microsoft.DirectX.DirectSound.CaptureBuffer(desc, cap);
                    buffer.Start(true);
                }
            }	
           
        }

        private void terminateMasterPeakRetrieval()
        {
            if (buffer != null)
            {
                if (buffer.Capturing)
                {
                    buffer.Stop();
                }

                buffer.Dispose();
                buffer = null;
            }
        }
 

        /// <summary>
        /// Set the device interface to be used
        /// </summary>
        /// <param name="mixer"></param>
        /// <param name="deviceInterface">The name of the device interface (e.g. "XYZ Audio Adapter")</param>
        private void setDeviceInterface(Mixer mixer, string deviceInterface)
        {

            bool deviceFound = false;

            MixerDetails mixerDetails = mixer.Devices;

            foreach (MixerDetail mixerDetail in mixerDetails)
            {
                if (mixerDetail.MixerName == deviceInterface)
                {
                    
                    if (mixer.MixerType == MixerType.Recording)
                    {
                        mixers.Recording.DeviceId = mixerDetail.DeviceId;
                    }
                    else if (mixer.MixerType == MixerType.Playback)
                    {
                        mixers.Playback.DeviceId = mixerDetail.DeviceId;
                    }

                    mixer.DeviceId = mixerDetail.DeviceId;
                    deviceFound = true;
                    break;
                }
            }

            if (!deviceFound)
            {
                throw new DeviceInterfaceException(deviceInterface, "The " + deviceInterface + " device interface was not found.");
            }
        }


        /// <summary>
        /// Set the line used by the selected device.
        /// </summary>
        /// <param name="lineEnumerator"></param>
        /// <param name="deviceInterface">The name of the device interface (e.g. "XYZ Audio Adapter")</param>
        /// <param name="deviceLine">The name of the line (e.g. "Microphone", "Line In"...</param>
        private void setDeviceLine(IEnumerator lineEnumerator, string deviceInterface, string deviceLine)
        {
            MixerLine mixerLine = null;
            
            bool lineFound = false;
            while (lineEnumerator.MoveNext())
            {
                mixerLine = (MixerLine)lineEnumerator.Current;

                if (mixerLine.Name == deviceLine)
                {
                    if (!mixerLine.ContainsSelected)
                    {
                        _UnselectableMixerLineName = deviceLine;
                    }
                    else
                    {
                        if (!mixerLine.Selected)
                        {
                            mixerLine.Selected = true;
                        }
                        _UnselectableMixerLineName = "";
                    }

                    lineFound = true;
                    break;
                }
            }

            if (!lineFound)
            {
                throw new DeviceLineException(deviceInterface, deviceLine, "The " + deviceLine + " device line was not found for the " + deviceInterface + " interface.");
            } 
        }
    }
}
