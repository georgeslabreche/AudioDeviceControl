using System.Threading;
using System;

namespace AudioDeviceConsole
{
    public class VolumeChanger : TesterThread, ITesterThread
    {

        int[] volumeArray;

        public VolumeChanger()
        {
            sleepTime = 2000;
            volumeArray = new int[]{0, 25, 50, 75, 100};
        }

        public void Start()
        {
            int arrayIndex = 0;

            while (!_Shutdown)
            {
                // Reset the index if we reach the array limit.
                if(arrayIndex > 4){
                    arrayIndex = 0;
                }

                int oldCaptureVolume = AudioDeviceFacade.AudioDeviceControl.GetCaptureVolume();
                int newCaptureVolume = AudioDeviceFacade.AudioDeviceControl.SetCaptureVolume(volumeArray[arrayIndex]);

                Console.WriteLine("\tSetting Volume: " + oldCaptureVolume + " -> " + newCaptureVolume + ".");

                arrayIndex++;

                Thread.Sleep(sleepTime);
            }
        }
    }
}
