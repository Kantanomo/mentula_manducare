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
using MentulaManducare;
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

        public void Tick()
        {
            switch (GameState)
            {
                case GameState.Lobby:
                {
                    if (ForceXDelay)
                        XDelayTimer = ForcedXDelayTimer;
                    break;
                }
                case GameState.Starting:
                    break;
                case GameState.InGame:
                {
                    //Biped Force
                    break;
                }
                case GameState.PostGame:
                    break;
                case GameState.MatchMaking:
                    break;
                case GameState.Unknown:
                    break;
                default:
                {
                    MainThread.WriteLine(
                        $"What in the absolute tom fuckery is going on {FormattedName} Has reached a Gamestate that doesn't exist.",
                        true);
                    break;
                }
            }
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

        public int MaxPlayers =>
            ServerMemory.ReadByte(0x534858, true);

        public string RegistryPath =>
            $"SYSTEM\\ControlSet001\\Services\\{ServiceName}\\";

        public string BanRegistry =>
            $"{RegistryPath}\\Parameters\\LIVE\\Banned Gamers";

        public string VIPRegistry =>
            $"{RegistryPath}\\Parameters\\LIVE\\VIPs";

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

        public Privacy Privacy =>
            (Privacy) ServerMemory.ReadByte(0x534850, true);

        public List<string> GetBannedGamers =>
            Registry.LocalMachine.OpenSubKey(BanRegistry)?.GetValueNames().ToList();

        public List<string> GetVIPGamers =>
            Registry.LocalMachine.OpenSubKey(VIPRegistry)?.GetValueNames().ToList();

        public Biped ForcedBiped = Biped.Disabled;

        public bool ForceXDelay => ForcedXDelayTimer != 10; //If XDelayTimer is not base value return true.
        public int ForcedXDelayTimer = 10; //10 is base XDelay timing.
        
        
        
        //Look at these dirty whores below
        public int XDelayTimer
        {
            get => ServerMemory.ReadInt(0x53484C, true);
            set => ServerMemory.WriteInt(0x53484C, value, true);
        }

        public bool RunCountdown
        {
            get => ServerMemory.ReadBool(0x53486C, true);
            set => ServerMemory.WriteBool(0x53486C, value, true);
        }

        public bool LobbyRunning
        {
            get => ServerMemory.ReadBool(0x53484C, true);
            set => ServerMemory.WriteBool(0x53484C, !value, true);
        }
        

        public void ForceStartLobby()
        {
            var tLobby = LobbyRunning;
            LobbyRunning = !tLobby;
            RunCountdown = true;
            XDelayTimer = 0;
            LobbyRunning = tLobby;
        }

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
