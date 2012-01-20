using System;
using AudioDeviceControl;
using System.Collections.Generic;
using System.Threading;


namespace AudioDeviceConsole
{

    class Program
    {
        private static IAudioDeviceFacade audio = new AudioDeviceFacade();

        private static PeakFeeder peakFeeder;
        private static DeviceChanger deviceChanger;
        private static VolumeChanger volumeChanger;
        private static MuteToggler muteToggler;

        private static Thread peakFeederThread;
        private static Thread deviceChangerThread;
        private static Thread volumeChangerThread;
        private static Thread muteTogglerThread;

        static void Main(string[] args)
        {
            programSelection();      
        }

        private static void programSelection()
        {
            bool doLoop = true;


            while (doLoop)
            {
                Console.WriteLine("\n\nWhat do you want to do?");
                Console.WriteLine("\t1. Test audio device control on a selected device.");
                Console.WriteLine("\t2. Test swicthing between devices.");
                Console.WriteLine("\t3. Concurrency Test:\n\t\t - Peak feeding.\n\t\t - Device switching.");
                Console.WriteLine("\t4. Concurrency Test:\n\t\t - Peak feeding.\n\t\t - Device switching.\n\t\t - Volume setting.");
                Console.WriteLine("\t5. Concurrency Test:\n\t\t - Mute toggling.\n\t\t - Peak feeding.");
                Console.WriteLine("\t6. Exit.");

                int choice = -1;

                try
                {
                    choice = Int32.Parse(Console.ReadLine());

                    if (choice == 1)
                    {
                        doLoop = false;
                        runGenericAudioDeviceControlProgram();

                        
                    }
                    else if (choice == 2)
                    {
                        doLoop = false;
                        startDeviceSwitchingProgram(500);
                        waitForExitInstruction();
                        
                    }
                    else if (choice == 3)
                    {
                        doLoop = false;
                        startDeviceSwitchingProgram(2000);
                        startPeakFeedingProgram(100);
                        waitForExitInstruction();
                    }
                    else if (choice == 4)
                    {
                        doLoop = false;
                        startDeviceSwitchingProgram(7000);
                        startPeakFeedingProgram(100);
                        startVolumeChangingProgram(2000);
                        waitForExitInstruction();
                    }
                    else if (choice == 5)
                    {
                        doLoop = false;
                        audio.AudioDeviceControl.SetCaptureMute(true);
                        startPeakFeedingProgram(100);
                        startMuteTogglingProgram(3000);
                        waitForExitInstruction();
                    }

                    else if (choice == 6)
                    {
                        Environment.Exit(0);
                    }
                    else
                    {
                        doLoop = true;
                    }
                }
                catch (Exception)
                {
                    choice = -1;
                    doLoop = true; 
                }

                
            }

        }

        /// <summary>
        /// 
        /// </summary>
        private static void waitForExitInstruction()
        {
            Console.ReadLine();
            Console.WriteLine("\nStopping program...");

            stopConcurrencyTestPrograms();

            programSelection();
        }

        /// <summary>
        /// Start the mute toggling thread.
        /// </summary>
        /// <param name="sleepTime"></param>
        private static void startMuteTogglingProgram(int sleepTime)
        {
            muteToggler = new MuteToggler();
            muteToggler.SetSleepTime(sleepTime);
            muteToggler.AudioDeviceFacade = audio;

            muteTogglerThread = new Thread(muteToggler.Start);
            muteTogglerThread.Start();
        }

        /// <summary>
        /// Start the peak feeding thread.
        /// </summary>
        private static void startPeakFeedingProgram(int sleepTime)
        {
            peakFeeder = new PeakFeeder();
            peakFeeder.SetSleepTime(sleepTime);
            peakFeeder.AudioDeviceFacade = audio;

            peakFeederThread = new Thread(peakFeeder.Start);
            peakFeederThread.Start();
        }

        /// <summary>
        /// Start the device switching thread.
        /// </summary>
        private static void startDeviceSwitchingProgram(int sleepTime)
        {
            deviceChanger = new DeviceChanger();
            deviceChanger.SetSleepTime(sleepTime);
            deviceChanger.AudioDeviceFacade = audio;

            deviceChangerThread = new Thread(deviceChanger.Start);
            deviceChangerThread.Start();
        }

        /// <summary>
        /// Start the volume changing thread.
        /// </summary>
        private static void startVolumeChangingProgram(int sleepTime)
        {
            volumeChanger = new VolumeChanger();
            volumeChanger.AudioDeviceFacade = audio;
            volumeChanger.SetSleepTime(sleepTime);

            volumeChangerThread = new Thread(volumeChanger.Start);
            volumeChangerThread.Start();
        }

        public static void stopConcurrencyTestPrograms()
        {
            shutdownAndJoinThread(peakFeeder, peakFeederThread);
            shutdownAndJoinThread(volumeChanger, volumeChangerThread);
            shutdownAndJoinThread(deviceChanger, deviceChangerThread);
            shutdownAndJoinThread(muteToggler, muteTogglerThread);
        }

        /// <summary>
        /// Shutdown a thread.
        /// </summary>
        /// <param name="testerThread"></param>
        /// <param name="thread"></param>
        private static void shutdownAndJoinThread(ITesterThread testerThread, Thread thread)
        {
            // Shutdown the running thread.
            if (testerThread != null && !testerThread.isShutdown())
            {
                testerThread.Shutdown();
            }

            // Use the Join method to block the current thread 
            // until the object's thread terminates.
            if (thread != null)
            {
                thread.Join(3000);
            }
        }

        private static void runGenericAudioDeviceControlProgram()
        {

            string[,] lineChoiceArray = deviceSelection();

            string lineChoiceString = Console.ReadLine();
            if (lineChoiceString == "x")
            {
                Environment.Exit(0);
            }
            else
            {
                bool doLoop = true;

                while(doLoop){
                    try
                    {
                        int lineChoice = Int32.Parse(lineChoiceString) - 1;

                        setDevice(lineChoiceArray, lineChoice);

                        while (true)
                        {

                            string line = Console.ReadLine();
                            if (line == "+")
                            {
                                doLoop = false;
                                Console.WriteLine("Volume: " + audio.AudioDeviceControl.IncrementCaptureVolume());
                            }
                            else if (line == "-")
                            {
                                doLoop = false;
                                Console.WriteLine("Volume: " + audio.AudioDeviceControl.DecrementCaptureVolume());
                            }
                            else if (line == "m")
                            {
                                doLoop = false;
                                Console.WriteLine("Mute: " + audio.AudioDeviceControl.ToggleCaptureMute());
                            }
                            else if (line == "p")
                            {
                                doLoop = false;
                                for (int i = 0; i < 100; i++)
                                {
                                    Console.WriteLine("Peak Feed: " + audio.AudioDeviceControl.GetCaptureDeviceMasterPeakValue());
                                    Thread.Sleep(100);
                                }

                                audio.AudioDeviceControl.DisposeCaptureDeviceMasterPeakValue();

                            }
                            else if (line == "d")
                            {
                                doLoop = false;
                                displaySelectedDevice();
                            }

                            else if (line == "n")
                            {
                                doLoop = false;
                                lineChoiceArray = deviceSelection();
                                lineChoice = Int32.Parse(Console.ReadLine()) - 1;
                                setDevice(lineChoiceArray, lineChoice);
                            }

                            else if (line == "?")
                            {
                                doLoop = false;
                                displayHelp();
                            }

                            else if (line == "x")
                            {
                                Environment.Exit(0);
                            }
                            else
                            {
                                doLoop = true;
                                Console.WriteLine("Invalid choice. Try again.");
                                lineChoiceString = Console.ReadLine();
                            }
                        }


                    }
                    catch (Exception)
                    {
                        doLoop = true;
                        Console.WriteLine("Invalid choice. Try again.");
                        lineChoiceString = Console.ReadLine();
                    }

                }
            }
        }

        private static string[,] deviceSelection()
        {
            Console.WriteLine("Select one of the available capture lines:\n");

            Dictionary<string, List<AudioLineInfo>> deviceDictionary = audio.AudioDeviceControl.GetCaptureDevicesInfo();

            string[,] lineChoiceArray = new string[100, 2];
            int choiceIndex = 1;

            foreach (KeyValuePair<string, List<AudioLineInfo>> kvp in deviceDictionary)
            {
                Console.WriteLine("\tDevice Interface: " + kvp.Key);
                Console.WriteLine("\tDevice Line(s): ");
                foreach (AudioLineInfo lineInfo in kvp.Value)
                {
                    Console.WriteLine("\t\t" + choiceIndex + " - " + lineInfo.LineName);

                    // store choices so that we can set them when user selects.
                    lineChoiceArray[choiceIndex - 1, 0] = kvp.Key;
                    lineChoiceArray[choiceIndex - 1, 1] = lineInfo.LineName;

                    choiceIndex++;
                }

                Console.WriteLine();
            }

            Console.WriteLine("\t\tx - Exit.");

            return lineChoiceArray;
        }

        private static void setDevice(string[,] lineChoiceArray, int lineChoice)
        {
            audio.AudioDeviceControl.SetCaptureDevice(lineChoiceArray[lineChoice, 0], lineChoiceArray[lineChoice, 1]);
            displaySelectedDevice();
            displayHelp();
        }

        private static void displayHelp()
        {
            Console.WriteLine("Input Instructions to send to this capture line:\n");
            Console.WriteLine("\t+ Volume Up.");
            Console.WriteLine("\t- Volume Down.");
            Console.WriteLine("\tm Mute.");
            Console.WriteLine("\tp Peak Feed.");
            Console.WriteLine("\td Display selected device.");
            Console.WriteLine("\tn Select new device.");
            Console.WriteLine("\t? Help.");
            Console.WriteLine("\tx Exit.");
            Console.WriteLine();

        }

        private static void displaySelectedDevice()
        {
            KeyValuePair<string, string> devicekvp = audio.AudioDeviceControl.GetSelectedCaptureDeviceInfo();
            Console.WriteLine("You have selected: " + devicekvp.Key + " - " + devicekvp.Value);
            Console.WriteLine();
        }
    }
}
