using System;
using System.Collections.Generic;
using AudioDeviceControl;
using System.Threading;

namespace AudioDeviceConsole
{
    public class DeviceChanger : TesterThread, ITesterThread
    {    
        private Dictionary<string, List<string>> deviceInterfaceAndLineDictionary = null;

        public DeviceChanger()
        {
            _Shutdown = false;
            sleepTime = 7000;
        }

        /// <summary>
        /// Start switching.
        /// </summary>
        public void Start()
        {
            init();

            while (!_Shutdown)
            {
                // Switch between available devices.
                foreach (KeyValuePair<string, List<string>> kvp in deviceInterfaceAndLineDictionary)
                {
                    string interfaceName = kvp.Key;
                    List<string> lineList = kvp.Value;

                    foreach (string lineName in lineList)
                    {
                        Console.WriteLine("\tChanging device to " + interfaceName + " - " + lineName);

                        AudioDeviceFacade.AudioDeviceControl.SetCaptureDevice(interfaceName, lineName);

                        Thread.Sleep(sleepTime);
                    }
                }

            }
        }

        /// <summary>
        /// Retrieve capture devices information.
        /// </summary>
        private void init()
        {
            Dictionary<string, List<AudioLineInfo>> deviceDictionary = AudioDeviceFacade.AudioDeviceControl.GetCaptureDevicesInfo();

            deviceInterfaceAndLineDictionary = new Dictionary<string, List<string>>();

            foreach (KeyValuePair<string, List<AudioLineInfo>> kvp in deviceDictionary)
            {
                string deviceInterface = kvp.Key;
                List<AudioLineInfo> lineList = kvp.Value;

                foreach (AudioLineInfo lineInfo in lineList)
                {
                    if (deviceInterfaceAndLineDictionary.ContainsKey(deviceInterface))
                    {
                        deviceInterfaceAndLineDictionary[deviceInterface].Add(lineInfo.LineName);
                    }
                    else
                    {
                        List<string> currentLineList = new List<string>();
                        currentLineList.Add(lineInfo.LineName);
                        deviceInterfaceAndLineDictionary.Add(deviceInterface, currentLineList);
                    }
                    
                }

            }
        }
    }
}
