using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using mentula_manducare.Classes;
using mentula_manducare.Enums;
using Microsoft.Win32;

namespace mentula_manducare.Objects
{
    public class ServerContainer
    {
        public int Index { get; set; }
        public Guid WebGuid { get; set; }
        public string Instance { get; set; }
        private string _Name = "";

        public string Name
        {
            get
            {
                if(_Name == "")
                    _Name = ServerMemory.ReadStringUnicode(0x52FC88, 16, true);
                return _Name;
            }
        }

        public bool isLive { get; set; }
        public string Description { get; set; } //Add proper handling for setting description
        public Process ServerProcess { get; set; }
        public MemoryHandler ServerMemory { get; set; }
        public ConsoleProxy ConsoleProxy { get; set; }
        public bool AutoRestart = true;
        public string LogName =>
            Instance.Replace(':', '_');
        public ServerContainer(Process serverProcess)
        {
            this.ServerProcess = serverProcess;
            ServerMemory = new MemoryHandler(ServerProcess);
            WebGuid = Guid.NewGuid(); //Used to handle the issues with server indexing with detaching/attaching instances.
            GetLaunchParameters(this);
        }

        public void LaunchConsoleProxy()
        {
            ConsoleProxy = new ConsoleProxy(this.Instance);
        }

        public void KillConsoleProxy()
        {
            ConsoleProxy.Console_.Kill();
        }

        public void KillServer(bool allowrestart = true)
        {
            this.AutoRestart = allowrestart;
            this.ServerProcess.Kill();
            
        }
        public string FormattedName =>
            $"{Index} - {Name}:{Instance}";
        [ScriptIgnore]
        public PlayerCollection CurrentPlayers =>
            new PlayerCollection(this);

        public GameState GameState =>
            (GameState) ServerMemory.ReadByte(0x3C40AC, true);

        public int PlayerCount =>
            ServerMemory.ReadByte(0x53329C, true);

        private string _ServiceName = "";

        public string ServiceName
        {
            get
            {
                if(_ServiceName == "")
                    _ServiceName = ServerMemory.ReadStringUnicode(0x3B4AC4, 50, true);
                return _ServiceName;
            }
        }
            

        public string RegistryPath =>
            $"SYSTEM\\ControlSet001\\Services\\{ServiceName}\\";

        public string BanRegistry =>
            $"{RegistryPath}\\Parameters\\LIVE\\Banned Gamers";


        public string CurrentVariantMap =>
            ServerMemory.ReadStringUnicode(0x534894, 200, true).Split('\\').Last();


        public string CurrentVariantName =>
            ServerMemory.ReadStringUnicode(0x534A18, 100, true);

        //The next three properties are in fact the gayest things I have ever had to figure out.

        public int NextVariantIndex =>
            ServerMemory.ReadInt(0x48D6EC, true);

        public string NextVariantMap =>
            ServerMemory.ReadStringAscii(0x450854 + (NextVariantIndex * 0x21C), 200, true).Split('\\').Last();

        public string NextVariantName =>
            ServerMemory.ReadStringUnicode(0x4506C4 + (NextVariantIndex * 0x21C), 200, true);


        public List<string> GetBannedGamers =>
            Registry.LocalMachine.OpenSubKey(BanRegistry)?.GetValueNames().ToList();

        /* ================================ *
         *  Static Properties and methods   *
         * ================================ */


        public static Regex ServerInstanceMatch = new Regex(@"-instance:[(\d|\w)]+");
        public static Regex ServerLiveMatch = new Regex(@"-live+");
        public static void GetLaunchParameters(ServerContainer container)
        {
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + container.ServerProcess.Id))
            using (ManagementObjectCollection objects = searcher.Get())
            {
                //WHAT ELSE CAN BE EXTRACTED
                var CommandLine = objects.Cast<ManagementBaseObject>().SingleOrDefault()?["CommandLine"]?.ToString();
                container.Instance = ServerInstanceMatch.Matches(CommandLine)[0]?.Value;
                container.isLive = ServerLiveMatch.IsMatch(CommandLine);
            }
        }
    }
}
