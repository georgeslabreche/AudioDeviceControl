using System;
using System.Collections.Generic;
using System.Text;
using CoreAudioApi;

namespace AudioDeviceControl.Extensions
{
    public class PropertyStoreExt
    {
        private PropertyStore propertyStore;

        public PropertyStoreExt(PropertyStore propertyStore)
        {
            this.propertyStore = propertyStore;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="pid">A property identifier (PID)</param>
        /// <returns></returns>
        public bool Contains(Guid guid, int pid)
        {
            for (int i = 0; i < propertyStore.Count; i++)
            {
                PropertyKey key = propertyStore.Get(i);

                if (key.fmtid == guid && key.pid == pid)
                {
                    return true;
                }
            }

            return false;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="pid">A property identifier (PID)</param>
        /// <returns></returns>
        public PropertyStoreProperty this[Guid guid, int pid]
        {
            get
            {
                for(int i = 0; i < propertyStore.Count; i++){

                    PropertyKey key = propertyStore.Get(i);
                    if (key.fmtid == guid && key.pid == pid)
                    {
                        return propertyStore[i];
                    }
                }
                return null;
            }
        }

    }
}
