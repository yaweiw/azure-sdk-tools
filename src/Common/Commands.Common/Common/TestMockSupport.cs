
namespace Microsoft.WindowsAzure.Commands.Utilities.Common
{
    public class TestMockSupport
    {
        //a.k.a when you run under Playback mode
        public static bool RunningMocked { get; set; }

        public static void Delay(int milliSeconds)
        {
            if (!RunningMocked)
            {
                System.Threading.Thread.Sleep(milliSeconds);
            }
        }
    }
}
