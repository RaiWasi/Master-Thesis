using System;
using System.Threading;
using System.Runtime.InteropServices;
using erviceHDMapPedPathPredition;

namespace ServiceHDMapPedPathPredition
{
    class Program
    {
        // Declare the SetConsoleCtrlHandler function
        // as external and receiving a delegate.
        [DllImport("Kernel32")]
        public static extern bool SetConsoleCtrlHandler(HandlerRoutine handler, bool add);

        // A delegate type to be used as the handler routine
        // for SetConsoleCtrlHandler.
        public delegate bool HandlerRoutine(CtrlTypes ctrlType);

        // An enumerated type for the control messages
        // sent to the handler routine.
        public enum CtrlTypes {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }


        static void Main(string[] args)
        {
            string configfilepath = args[1];
            string configName = args[3];

            var simpleReceiver = new GPS_SimpleReceiver(configfilepath, configName);
            var cancelEvent = new ManualResetEvent(false);
            var ctrlCHandler = new HandlerRoutine(ctrlType => {
                cancelEvent.Set();
                return true;
            });

            //register handler for Ctrl+C
            SetConsoleCtrlHandler(ctrlCHandler, true);

            try
            {
                simpleReceiver.StartNode();
                cancelEvent.WaitOne();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                simpleReceiver.StopNode();
            }

            //deregister handler for ctrl+C
            SetConsoleCtrlHandler(ctrlCHandler, false);

        }
    }
}
