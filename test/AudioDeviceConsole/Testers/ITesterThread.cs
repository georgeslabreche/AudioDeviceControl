using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AudioDeviceControl;

namespace AudioDeviceConsole
{
    public interface ITesterThread
    {
        IAudioDeviceFacade AudioDeviceFacade { get; set; }

        /// <summary>
        /// Shutdown thread by stopping thread loop.
        /// </summary>
        void Shutdown();

        /// <summary>
        /// Check is thread shutdown status.
        /// </summary>
        /// <returns></returns>
        bool isShutdown();

    }
}
