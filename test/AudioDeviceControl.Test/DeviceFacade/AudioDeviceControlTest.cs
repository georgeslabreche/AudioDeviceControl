
using NUnit.Framework;
using AudioDeviceControl;

namespace AudioDeviceControl.Test.DeviceFacade
{
    [TestFixture]
    public class AudioDeviceControlTest
    {
        private IAudioDeviceControl audioDeviceControl;
        private int volumeLevelBeforeTest;

        [TestFixtureSetUp]
        protected void TestFixtureSetUp()
        {
            IAudioDeviceFacade audioDeviceFacade = new AudioDeviceFacade();
            audioDeviceControl = audioDeviceFacade.AudioDeviceControl;

            // Get the volume set before we run the test so that it can be reset to it's original value once we are done.
            volumeLevelBeforeTest = audioDeviceFacade.AudioDeviceControl.GetCaptureVolume();
        }

 
        [TestFixtureTearDown]
        protected void TestFixtureTearDown()
        {
            // Reset volume to value it was before the test
            audioDeviceControl.SetCaptureVolume(volumeLevelBeforeTest);
        }

   
        [TestCase(100)]
        [TestCase(75)]
        [TestCase(50)]
        [TestCase(25)]
        public void TestGetCaptureVolume(int expectedVolume)
        {
            // Set a test value for the volume we expect to retrieve.
            audioDeviceControl.SetCaptureVolume(expectedVolume);

            // Retrieve the volume
            int actualVolume = audioDeviceControl.GetCaptureVolume();
  
            //to allow floating point precision we are giving it a 1% error margin
            Assert.GreaterOrEqual(expectedVolume + 1, actualVolume);
            Assert.LessOrEqual(expectedVolume - 1, actualVolume);
        }
        


        [TestCase(100)]
        [TestCase(75)]
        [TestCase(50)]
        [TestCase(25)]
        [TestCase(0)]
        [TestCase(101)]
        [TestCase(250)]
        [TestCase(-1)]
        [TestCase(-250)]
        public void TestSetCaptureVolume(int expectedVolume)
        {

            // Set the volume
            int returnedVolume = audioDeviceControl.SetCaptureVolume(expectedVolume);
            
            // Retrieve the volume
            int actualVolume = audioDeviceControl.GetCaptureVolume();

            
            // Take into account volume percentage range restrictions.
            if (expectedVolume > 100)
            {
                expectedVolume = 100;

            }
            
            else if (expectedVolume < 0)
            {
                expectedVolume = 0;
            }

            //to allow floating point precision we are giving it a 1% error margin
            Assert.GreaterOrEqual(expectedVolume+1, actualVolume);
            Assert.LessOrEqual(expectedVolume-1, actualVolume);

            Assert.GreaterOrEqual(expectedVolume + 1, returnedVolume);
            Assert.LessOrEqual(expectedVolume - 1, returnedVolume);
        }

        
        [TestCase(45, 16)]
        [TestCase(99, 2)]
        [TestCase(98, 15)]
        public void TestIncrementCaptureVolume(int volumeStart, int numberOfIncrements)
        {
            // Set the volume
            audioDeviceControl.SetCaptureVolume(volumeStart);

            int actualVolume = -1;
            for (int i = 0; i < numberOfIncrements; i++ )
            {
                actualVolume = audioDeviceControl.IncrementCaptureVolume();
            }

            int expectedVolume = audioDeviceControl.GetCaptureVolume();
            if (expectedVolume > 100)
            {
                expectedVolume = 100;
            }

            Assert.AreEqual(expectedVolume, actualVolume);
        }
        

        [TestCase(45, 16)]
        [TestCase(1, 2)]
        [TestCase(2, 15)]
        public void TestDecrementCaptureVolume(int volumeStart, int numberOfDecrements)
        {
            // Set the volume
            audioDeviceControl.SetCaptureVolume(volumeStart);

            int actualVolume = -1;
            for (int i = 0; i < numberOfDecrements; i++)
            {
                actualVolume = audioDeviceControl.DecrementCaptureVolume();
            }

            int expectedVolume = audioDeviceControl.GetCaptureVolume();

            if (expectedVolume < 0)
            {
                expectedVolume = 0;
            }

            Assert.AreEqual(expectedVolume, actualVolume);
        }

    }
}
