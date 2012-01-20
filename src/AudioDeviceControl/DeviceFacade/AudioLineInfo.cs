
namespace AudioDeviceControl
{
    public class AudioLineInfo
    {
        private string _LineName;
        public string LineName
        {
            get
            {
                return _LineName;
            }
            set
            {
                _LineName = value;
            }
        }

        private string _FriendlyName;
        public string FriendlyName
        {
            get
            {
                return _FriendlyName;
            }
            set
            {
                _FriendlyName = value;
            }
        }

        private bool _Mutable;
        public bool Mutable
        {
            get
            {
                return _Mutable;
            }
            set
            {
                _Mutable = value;
            }
        }
    }
}
