using System;
using System.Collections.Generic;
using System.Text;

namespace AudioDeviceControl.Extensions
{
    public class PKEYExtContainer
    {
        private Guid _Guid;
        public Guid Guid
        {
            get
            {
                return _Guid;
            }
            set
            {
                _Guid = value;
            }
        }

        private int _Pid;
        public int Pid {
            get
            {
                return _Pid;
            }
            set
            {
                _Pid = value;
            }
        }

        public PKEYExtContainer(Guid guid, int pid)
        {
            Guid = guid;
            Pid = pid;
        }
    }
}
