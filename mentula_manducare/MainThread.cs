using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using mentula_manducare;
using mentula_manducare.Classes;
using mentula_manducare.Objects;

namespace MentulaManducare
{
    class MainThread
    {
        private static int InputTop = 0;
        private static int InputLeft = 0;
        private static bool yes = true;
        const uint ENABLE_QUICK_EDIT = 0x0040;
        public static Update Updater = new Update();
        public static DateTime UpdateInterval = DateTime.Now;
        
        // STD_INPUT_HANDLE (DWORD): -10 is the standard input device.
        const int STD_INPUT_HANDLE = -10;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        public static string[] MutableErrors = new []
        {
            "Microsoft.AspNet.SignalR.Hubs.ConnectionIdProxy",
            "HKLM\\Software\\Microsoft\\Fusion!EnableLog",
            "Could not load file or assembly 'mscorlib.XmlSerializers",
            "The requested Performance Counter is not a custom counter",
            ".Load"
        };
        static void Main(string[] args)
        {
        //Gotta love new Features that probably break absolutely everything made in the past
        //https://stackoverflow.com/questions/13656846/how-to-programmatic-disable-c-sharp-console-applications-quick-edit-mode/36720802#36720802
            IntPtr consoleHandle = GetStdHandle(STD_INPUT_HANDLE);
            uint consoleMode;
            GetConsoleMode(consoleHandle, out consoleMode);
            consoleMode &= ~ENABLE_QUICK_EDIT;
            SetConsoleMode(consoleHandle, consoleMode);


            #region Fuck Liberals
            WriteLine(String.Format("{0," + ((Console.WindowWidth / 2) + (113 / 2)) + "}", "MMMMMMMM               MMMMMMMM               AAA                  GGGGGGGGGGGGG               AAA               "));
            WriteLine(String.Format("{0," + ((Console.WindowWidth / 2) + (113 / 2)) + "}", "M:::::::M             M:::::::M              A:::A              GGG::::::::::::G              A:::A              "));
            WriteLine(String.Format("{0," + ((Console.WindowWidth / 2) + (113 / 2)) + "}", "M::::::::M           M::::::::M             A:::::A           GG:::::::::::::::G             A:::::A             "));
            WriteLine(String.Format("{0," + ((Console.WindowWidth / 2) + (113 / 2)) + "}", "M:::::::::M         M:::::::::M            A:::::::A         G:::::GGGGGGGG::::G            A:::::::A            "));
            WriteLine(String.Format("{0," + ((Console.WindowWidth / 2) + (113 / 2)) + "}", "M::::::::::M       M::::::::::M           A:::::::::A       G:::::G       GGGGGG           A:::::::::A           "));
            WriteLine(String.Format("{0," + ((Console.WindowWidth / 2) + (113 / 2)) + "}", "M:::::::::::M     M:::::::::::M          A:::::A:::::A     G:::::G                        A:::::A:::::A          "));
            WriteLine(String.Format("{0," + ((Console.WindowWidth / 2) + (113 / 2)) + "}", "M:::::::M::::M   M::::M:::::::M         A:::::A A:::::A    G:::::G                       A:::::A A:::::A         "));
            WriteLine(String.Format("{0," + ((Console.WindowWidth / 2) + (113 / 2)) + "}", "M::::::M M::::M M::::M M::::::M        A:::::A   A:::::A   G:::::G    GGGGGGGGGG        A:::::A   A:::::A        "));
            WriteLine(String.Format("{0," + ((Console.WindowWidth / 2) + (113 / 2)) + "}", "M::::::M  M::::M::::M  M::::::M       A:::::A     A:::::A  G:::::G    G::::::::G       A:::::A     A:::::A       "));
            WriteLine(String.Format("{0," + ((Console.WindowWidth / 2) + (113 / 2)) + "}", "M::::::M   M:::::::M   M::::::M      A:::::AAAAAAAAA:::::A G:::::G    GGGGG::::G      A:::::AAAAAAAAA:::::A      "));
            WriteLine(String.Format("{0," + ((Console.WindowWidth / 2) + (113 / 2)) + "}", "M::::::M    M:::::M    M::::::M     A:::::::::::::::::::::AG:::::G        G::::G     A:::::::::::::::::::::A     "));
            WriteLine(String.Format("{0," + ((Console.WindowWidth / 2) + (113 / 2)) + "}", "M::::::M     MMMMM     M::::::M    A:::::AAAAAAAAAAAAA:::::AG:::::G       G::::G    A:::::AAAAAAAAAAAAA:::::A    "));
            WriteLine(String.Format("{0," + ((Console.WindowWidth / 2) + (113 / 2)) + "}", "M::::::M               M::::::M   A:::::A             A:::::AG:::::GGGGGGGG::::G   A:::::A             A:::::A   "));
            WriteLine(String.Format("{0," + ((Console.WindowWidth / 2) + (113 / 2)) + "}", "M::::::M               M::::::M  A:::::A               A:::::AGG:::::::::::::::G  A:::::A               A:::::A  "));
            WriteLine(String.Format("{0," + ((Console.WindowWidth / 2) + (113 / 2)) + "}", "M::::::M               M::::::M A:::::A                 A:::::A GGG::::::GGG:::G A:::::A                 A:::::A "));
            WriteLine(String.Format("{0," + ((Console.WindowWidth / 2) + (113 / 2)) + "}", "MMMMMMMM               MMMMMMMMAAAAAAA                   AAAAAAA   GGGGGG   GGGGAAAAAAA                   AAAAAAA"));
            #endregion
            AppDomain.CurrentDomain.FirstChanceException += (sender, eventArgs) =>
            {

                if (!MutableErrors.Any(eventArgs.Exception.ToString().Contains))
                {
                    //if(eventArgs.Exception.ToString().Contains("SignalR"))
                    //  WebSocketThread.StartWebApp();
#if DEBUG
                    WriteLine(
                        $"An exception has occured within the application please check the error log in {Logger.LogBase}",
                        true);
#endif
                    Logger.AppendToLog("ErrorLog1", eventArgs.Exception.ToString());
                }

            };
            yes = false;

#if !DEBUG
            Updater.CheckUpdates();
#endif
            Console.Title = $"Mentula Manducare {Updater.CurrentVersion}";
            //Start new thread for actual processing.
            Task.Factory.StartNew(WebSocketThread.Run);

            Task.Factory.StartNew(ServerThread.Run);

            Task.Factory.StartNew(ConsoleInputThread.Run);
           

            while (true)
            {
#if !DEBUG
                if (DateTime.Now - UpdateInterval > TimeSpan.FromMinutes(10))
                {
                    Updater.CheckUpdates();
                    UpdateInterval = DateTime.Now;
                }
#endif
                Thread.Sleep(60000);
            }
        }

        public static void WriteLine(object Input, bool isInput = false)
        {

            //Console.SetCursorPosition(InputLeft, InputTop);
            //
            if (isInput == false) Console.ResetColor();
            if (yes) Console.ForegroundColor = ConsoleColor.Yellow;
            Console.SetCursorPosition(0, InputTop);
            Console.WriteLine(Input);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, InputTop + 1);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Enter Command:" + new string(' ', Console.WindowWidth - 16));
            
            InputTop++;

            //Console.SetCursorPosition(InputLeft, InputTop);
        }

      
        public static string BasePath
        {
            get
            {
                if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Mentula\\"))
                    Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Mentula\\");
                if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Mentula\\Logs"))
                    Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Mentula\\Logs");
                if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Mentula\\Settings"))
                    Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Mentula\\Settings");
                if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Mentula\\Update"))
                    Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Mentula\\Update");
                return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Mentula\\";
            }
        }
    }
}
