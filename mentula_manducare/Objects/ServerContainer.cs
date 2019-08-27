using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
        private bool _xDelayFlop = false;
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
        public Process ServerProcess { get; set; }
        public MemoryHandler ServerMemory { get; set; }
        public ConsoleProxy ConsoleProxy { get; set; }
        public bool AutoRestart = true;
        public int AFKKicktime = 0;
        public string LogName =>
            Instance.Replace(':', '_');

        public SettingsCollection Settings;
        public ServerContainer(Process serverProcess)
        {
            this.ServerProcess = serverProcess;
            ServerMemory = new MemoryHandler(ServerProcess);
            WebGuid = Guid.NewGuid(); //Used to handle the issues with server indexing with detaching/attaching instances.
            GetLaunchParameters(this);
            Settings = new SettingsCollection(LogName);
        }

        public void LaunchConsoleProxy()
        {
            ConsoleProxy = new ConsoleProxy(this.Instance);
            LoadSettings();
            CurrentPlayers = new PlayerCollection(this);
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
                    if (!LobbyRunning)
                    {
                        RunCountdown =
                            false; //If lobby is frozen keep canceling the count down and set delay timer to absurd value
                        XDelayTimer = short.MaxValue;
                        RunCountdown = true;
                    }
                    else
                    {
                        if (ForceXDelay & !_xDelayFlop & RunCountdown)
                        {
                            //Fire only after initial countdown starts, reset and change time.
                            RunCountdown = false;
                            XDelayTimer = ForcedXDelayTimer;
                            RunCountdown = true;
                            _xDelayFlop = true;
                        }
                        if (_xDelayFlop & !RunCountdown)
                            _xDelayFlop = false;
                    }
                    break;
                }
                case GameState.Starting:
                    break;
                case GameState.InGame:
                {
                    //Because of dynamic player table issues instead of processing the whole player collection
                    //just iterate through all possible options, It ain't perfect but it works.
                    if (ForcedBiped != Biped.Disabled)
                        for (var i = 0; i < 16; i++)
                            ServerMemory.WriteByte(0x3000274C + (i * 0x204), (byte) ForcedBiped);
                    //EVENTUALLY REMOVABLE
                    if (DoBattleRifleVelocityOverride)
                        BattleRifleVelocity = BattleRifleVelocityOverride;

                    if(AFKKicktime != 0)
                        foreach (PlayerContainer playerContainer in CurrentPlayers)
                        {
                            playerContainer.TickAFKCheck();
                            if (playerContainer.HasMoved)
                            {
                                playerContainer.HasMoved = false;
                                ConsoleProxy.SendMessage(
                                    $"{playerContainer.Name} that was a close one.");
                                }
                            if (DateTime.Now - playerContainer.LastMovement > TimeSpan.FromSeconds(AFKKicktime * 0.8) &&
                                !playerContainer.IsWarned)
                            {
                                playerContainer.IsWarned = true;
                                    ConsoleProxy.SendMessage(
                                        $"{playerContainer.Name} you are about to be kicked for being AFK you should move.");
                            }
                            if (DateTime.Now - playerContainer.LastMovement > TimeSpan.FromSeconds(AFKKicktime) &&
                                !playerContainer.isAFK)
                            {
                                playerContainer.isAFK = true;
                                ConsoleProxy.SendMessage($"{playerContainer.Name} was kicked for being AFK.");
                                ConsoleProxy.KickPlayer(playerContainer.Name);
                            }
                        }

                    break;
                }
                case GameState.PostGame:
                {
                    _xDelayFlop = false;

                    break;
                }
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

        public void SaveSettings()
        {
            Settings.AddSetting("XDelayTimer", ForcedXDelayTimer.ToString());
            Settings.AddSetting("ForcedBiped", ForcedBiped.ToString());
            Settings.AddSetting("Privacy", Privacy.ToString());
            Settings.AddSetting("MaxPlayers", MaxPlayers.ToString());
            Settings.AddSetting("BRFix", BattleRifleVelocityOverride.ToString());
            Settings.AddSetting("Description", FormattedDescription);
            Settings.AddSetting("AFKTimer", AFKKicktime.ToString());
        }

        public void LoadSettings()
        {
            XDelayTimer = int.Parse(Settings.GetSetting("XDelayTimer", ForcedXDelayTimer.ToString()));
            ForcedBiped = (Biped)Enum.Parse(typeof(Biped), Settings.GetSetting("ForcedBiped", ForcedBiped.ToString()));
            Privacy = (Privacy) Enum.Parse(typeof(Privacy), Settings.GetSetting("Privacy", Privacy.ToString()));
            MaxPlayers = int.Parse(Settings.GetSetting("MaxPlayers", MaxPlayers.ToString()));
            BattleRifleVelocityOverride = float.Parse(Settings.GetSetting("BRFix", BattleRifleVelocityOverride.ToString()));
            Description = Settings.GetSetting("Description", "");
            AFKKicktime = int.Parse(Settings.GetSetting("AFKTimer", "0"));
        }
        public string FormattedName =>
            $"{Index} - {Name}:{Instance}";

        [ScriptIgnore] public PlayerCollection CurrentPlayers;

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

        public int MaxPlayers {
            get => ServerMemory.ReadByte(0x534858, true);
            set => ConsoleProxy.Players(value);
        }

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

        public Privacy Privacy
        {
            get => (Privacy) ServerMemory.ReadByte(0x534850, true);
            set
            {
                if(value == Enums.Privacy.Closed)
                    ServerMemory.WriteByte(0x534850, (byte) Privacy.Closed, true);
                else
                    ConsoleProxy.Privacy(value);
            }
        }

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
            get => ServerMemory.ReadInt(0x534870, true);
            set => ServerMemory.WriteInt(0x534870, value, true);
        }

        public bool RunCountdown
        {
            get => ServerMemory.ReadBool(0x53486C, true);
            set => ServerMemory.WriteBool(0x53486C, value, true);
        }

        public bool LobbyRunning = true;
        

        public void ForceStartLobby()
        {
            RunCountdown = true;
            XDelayTimer = 0;
        }

        //All of this Battle rifle shit will eventually be removable.
        public bool DoBattleRifleVelocityOverride => BattleRifleVelocityOverride != 1200f;
        public float BattleRifleVelocityOverride = 1200f;

        public float BattleRifleVelocity
        {
            set
            {
                ServerMemory.WriteFloat(ServerMemory.BlamCachePointer(0xA4EC88), value);
                ServerMemory.WriteFloat(ServerMemory.BlamCachePointer(0xA4EC8C), value);
            }
        }


        private byte[] description_;
        public string FormattedDescription { get; set; }
        public string Description
        {
            get { return Encoding.Unicode.GetString(description_); }
            set
            {
                FormattedDescription = value;
                var bytes = new List<byte>();
                var strings = ServerContainer.SymbolsSpecifier.Split(value);
                var charArray = new[] { '[', ']' };
                foreach (var string_ in strings)
                {
                    if (SymbolsSpecifier.Match(string_).Success)
                    {
                        bytes.Add(byte.Parse(string_.Trim(charArray).Substring(0, 2), NumberStyles.AllowHexSpecifier));
                        bytes.Add(byte.Parse(string_.Trim(charArray).Substring(2, 2), NumberStyles.AllowHexSpecifier));
                    }
                    else
                        bytes.AddRange(Encoding.Unicode.GetBytes(string_));
                }
                bytes.Add(00);
                bytes.Add(00);
                description_ = bytes.ToArray();
                ServerMemory.WriteMemory(true, 0x5347F8, bytes.ToArray());
            }
        }
        /* ================================ *
         *  Static Properties and methods   *
         * ================================ */


        public static Regex ServerInstanceMatch = new Regex(@"-instance:[(\d|\w)]+");
        public static Regex ServerLiveMatch = new Regex(@"-live+");
        public static Regex SymbolsSpecifier = new Regex("(\\[(?:[0-9A-Fa-f]{4}|0-9A-Fa-f]{6})\\])");
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
