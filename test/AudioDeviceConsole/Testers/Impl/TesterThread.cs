using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AudioDeviceControl;

namespace AudioDeviceConsole
{
    public class TesterThread
    {
        public IAudioDeviceFacade AudioDeviceFacade { get; set; }

        // Volatile is used as hint to the compiler that this data member will be accessed by multiple threads.
        protected volatile bool _Shutdown;

        protected int sleepTime;

        /// <summary>
        /// Shutdown thread by stopping thread loop.
        /// </summary>
        public void Shutdown()
        {
            _Shutdown = true;
        }

        public bool isShutdown()
        {
            return _Shutdown;
        }

        public void SetSleepTime(int sleepTime)
        {
            this.sleepTime = sleepTime;
        }
    }
}
