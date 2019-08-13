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
        public static string[] MutableErrors = new []
        {
            "Microsoft.AspNet.SignalR.Hubs.ConnectionIdProxy",
            "HKLM\\Software\\Microsoft\\Fusion!EnableLog",
            "Could not load file or assembly 'mscorlib.XmlSerializers",
            "The requested Performance Counter is not a custom counter"
        };
        static void Main(string[] args)
        {
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
#if DEBUG
                if (!MutableErrors.Any(eventArgs.Exception.ToString().Contains))
                {
                    WriteLine(
                        $"An exception has occured within the application please check the error log in {Logger.LogBase}",
                        true);
                    Logger.AppendToLog("ErrorLog", eventArgs.Exception.ToString());
                }
#endif
            };
            yes = false;
            //Start new thread for actual processing.
            Task.Factory.StartNew(WebSocketThread.Run);

            Task.Factory.StartNew(ServerThread.Run);



            while (true)
            {
                var input = Console.ReadLine();
                WriteLine($"Command: {input}", true);
                ExecuteCommand(input);
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

        public static void ExecuteCommand(string input)
        {
            string Command = input.Split(' ').First();
            List<string> Params = input.Split(' ').ToList();
            Params.RemoveAt(0);
            switch (Command.ToLower())
            {
                case "adduser":
                {
                    if (Params.Count != 2)
                        WriteLine("Incorrect amount of parameters provided, please provide a username and password",
                            true);
                    else
                    {
                        WebSocketThread.Users.Add(Params[0], Params[1]);
                        WriteLine($"User {Params[0]} added", true);
                    }

                    break;
                }
                case "removeuser":
                {
                    if (Params.Count != 1)
                        WriteLine("Incorrect amount of parameters provided, please provide a username", true);
                    else
                    {
                        WebSocketThread.Users.Remove(Params[0]);
                        WriteLine($"User {Params[0]} Removed", true);
                    }

                    break;
                }
                case "listuser":
                {
                    WriteLine("Users:", true);
                    WebSocketThread.Users.AsList.ForEach((user => { WriteLine(user.Username, true); }));
                    break;
                }
                case "resetuser":
                {
                    WriteLine(
                        WebSocketThread.Users.ResetPassword(Params[0], Params[1])
                            ? $"Password reset for {Params[0]}"
                            : "Invalid username",
                        true);
                    break;
                }
                case "listserver":
                {
                    WriteLine($"==Listing {ServerThread.Servers.Count} Servers==", true);
                    foreach (ServerContainer server in ServerThread.Servers)
                    {
                        WriteLine($"Index: {server.FormattedName}", true);
                    }
                    break;
                }
                case "stopserver":
                {
                    if (int.Parse(Params[0]) <= ServerThread.Servers.Count)
                    {
                        WriteLine($"Stopping Server {Params[0]}, Service will have to be restarted manually", true);
                        ServerThread.Servers[int.Parse(Params[0])].KillServer(false);
                    }
                    else
                    {
                        WriteLine($"Invalid Server Index given.");
                    }

                    break;
                }
                case "restartserver":
                {
                    if (int.Parse(Params[0]) <= ServerThread.Servers.Count)
                    {
                        WriteLine($"Restarting Server {Params[0]}....", true);
                        ServerThread.Servers[int.Parse(Params[0])].KillServer();
                    }
                    else
                    {
                        WriteLine($"Invalid Server Index given.");
                    }
                    break;
                }

            default:
                {
                    WriteLine("Invalid Command", true);
                    break;
                }
            }
        }

        public static string BasePath
        {
            get
            {
                if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Mentula\\"))
                    Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Mentula\\");
                if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Mentula\\Logs"))
                    Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Mentula\\Logs");
                return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Mentula\\";
            }
        }
    }
}
