using System;
using AudioDeviceControl;
using System.Threading;

namespace AudioDeviceConsole
{
    public class PeakFeeder : TesterThread, ITesterThread
    {

        public PeakFeeder()
        {
            _Shutdown = false;
            sleepTime = 100;
        }

        public void Start()
        {
            while (!_Shutdown)
            {
                int peakValue = AudioDeviceFacade.AudioDeviceControl.GetCaptureDeviceMasterPeakValue();
                Console.WriteLine(peakValue);

                Thread.Sleep(sleepTime);
            }

            AudioDeviceFacade.AudioDeviceControl.DisposeCaptureDeviceMasterPeakValue();
        }
    }
}
