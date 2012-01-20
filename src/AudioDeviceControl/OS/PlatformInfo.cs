using System;

namespace AudioDeviceControl
{
    public static class PlatformInfo
    {
        private static string PROCESSOR_ARCHITECTURE_ENVIRONMENT_VARIABLE_NAME = "PROCESSOR_ARCHITECTURE";

        public static string WIN_95 = "95";
        public static string WIN_98SE = "98SE";
        public static string WIN_98 = "98";
        public static string WIN_ME = "Me";
        public static string WIN_NT_3_POINT_51 = "NT 3.51";
        public static string WIN_NT_4_POINT_0 = "NT 4.0";
        public static string WIN_2000 = "2000";
        public static string WIN_XP = "XP";
        public static string WIN_VISTA = "Vista";
        public static string WIN_7 = "7";

        private static string WIN_98_TO_98SE_REVISION = "2222A";


        /// <summary>
        /// Return OS Architecture type. (e.g 32 for 32-bit)
        /// </summary>
        /// <returns></returns>
        public static int getArchitecture()
        {
            string pa = Environment.GetEnvironmentVariable(PROCESSOR_ARCHITECTURE_ENVIRONMENT_VARIABLE_NAME);
            return ((String.IsNullOrEmpty(pa) || String.Compare(pa, 0, "x86", 0, 3, true) == 0) ? 32 : 64);
        }

        /// <summary>
        /// Get service pack number:
        /// </summary>
        /// <returns></returns>
        public static string getServicePack(){

            OperatingSystem os = Environment.OSVersion;
            return os.ServicePack;

        }

        /// <summary>
        /// Get the operating system;
        /// </summary>
        /// <returns></returns>
        public static string getOperatingSystem()
        {
            //Get Operating system information.
            OperatingSystem os = Environment.OSVersion;
            //Get version information about the os.
            Version vs = os.Version;

            //Variable to hold our return value
            string operatingSystem = "";

            if (os.Platform == PlatformID.Win32Windows)
            {
                //This is a pre-NT version of Windows
                switch (vs.Minor)
                {
                    case 0:
                        operatingSystem = WIN_95;
                        break;
                    case 10:
                        if (vs.Revision.ToString() == WIN_98_TO_98SE_REVISION)
                            operatingSystem = WIN_98SE;
                        else
                            operatingSystem = WIN_98;
                        break;
                    case 90:
                        operatingSystem = WIN_ME;
                        break;
                    default:
                        break;
                }
            }
            else if (os.Platform == PlatformID.Win32NT)
            {
                switch (vs.Major)
                {
                    case 3:
                        operatingSystem = WIN_NT_3_POINT_51;
                        break;
                    case 4:
                        operatingSystem = WIN_NT_4_POINT_0;
                        break;
                    case 5:
                        if (vs.Minor == 0)
                            operatingSystem = WIN_2000;
                        else
                            operatingSystem = WIN_XP;
                        break;
                    case 6:
                        if (vs.Minor == 0)
                            operatingSystem = WIN_VISTA;
                        else
                            operatingSystem = WIN_7;
                        break;
                    default:
                        break;
                }
            }

            return operatingSystem;
        }
    }
}
