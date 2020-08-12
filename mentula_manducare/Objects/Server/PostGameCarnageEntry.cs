using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using mentula_manducare.Classes;

namespace mentula_manducare.Objects.Server
{
    public class PostGameCarnageEntry
    {
        
        [ScriptIgnore]
        private MemoryHandler _memory;
        [ScriptIgnore]
        private byte _index;
        [ScriptIgnore]
        private static int baseOffset = 0x4DC722;
        [ScriptIgnore]
        private static int rtPCROffset = 0x4DD1EE;
        [ScriptIgnore]
        private static int PCROffset = 0x49F6B0;
        [ScriptIgnore]
        private int calcRTPCROffset;
        [ScriptIgnore]
        private int calcBaseOffset;
        [ScriptIgnore]
        private int calcPCROffset;
        public int EndGameIndex;
        public PostGameCarnageEntry(MemoryHandler memory, byte index)
        {
            this._memory = memory;
            this._index = index;
            this.calcBaseOffset = baseOffset + (0x94 * index);
            this.calcRTPCROffset = rtPCROffset + (0x36A * index);
            var tName = Gamertag;
            for (var i = 0; i < 16; i++)
                if (tName == _memory.ReadStringUnicode(PCROffset + (i * 0x110), 16, true))
                {
                    calcPCROffset = PCROffset + (i * 0x110);
                    EndGameIndex = i;
                }
        }
        #region BasicData
        public ulong XUID =>
            _memory.ReadULong(calcBaseOffset, true);
        public string Gamertag =>
            _memory.ReadStringUnicode(calcBaseOffset + 0xA,16, true);
        public byte PrimaryColor =>
            _memory.ReadByte(calcBaseOffset + 0x4A, true);
        public byte SecondaryColor =>
            _memory.ReadByte(calcBaseOffset + 0x4B, true);
        public byte PrimaryEmblem =>
            _memory.ReadByte(calcBaseOffset + 0x4C, true);
        public byte SecondaryEmblem =>
            _memory.ReadByte(calcBaseOffset + 0x4D, true);
        public byte PlayerModel =>
            _memory.ReadByte(calcBaseOffset + 0x4E, true);
        public byte EmblemForeground =>
            _memory.ReadByte(calcBaseOffset + 0x4F, true);
        public byte EmblemBackground =>
            _memory.ReadByte(calcBaseOffset + 0x50, true);
        public byte EmblemToggle =>
            _memory.ReadByte(calcBaseOffset + 0x51, true);
        public string ClanDescription =>
            _memory.ReadStringUnicode(calcBaseOffset + 0x5A, 16, true);
        public string ClanTag =>
            _memory.ReadStringUnicode(calcBaseOffset + 0x7A, 6, true);
        public byte Team =>
            _memory.ReadByte(calcBaseOffset + 0x86, true);
        public byte Handicap =>
            _memory.ReadByte(calcBaseOffset + 0x87, true);
        public byte Rank =>
            _memory.ReadByte(calcBaseOffset + 0x88, true);
        public byte Nameplate =>
            _memory.ReadByte(calcBaseOffset + 0x8B, true);

        #endregion
        #region GameData

        public string Place =>
            _memory.ReadStringUnicode(calcPCROffset + 0xE0, 16, true);

        public string Score =>
            _memory.ReadStringUnicode(calcPCROffset + 0x40, 16, true);

        public short Kills =>
            _memory.ReadShort(calcRTPCROffset, true);

        public short Assists =>
            _memory.ReadShort(calcRTPCROffset + 0x2, true);

        public short Deaths =>
            _memory.ReadShort(calcRTPCROffset + 0x4, true);

        public short Betrayals =>
            _memory.ReadShort(calcRTPCROffset + 0x6, true);

        public short Suicides =>
            _memory.ReadShort(calcRTPCROffset + 0x8, true);

        public short BestSpree =>
            _memory.ReadShort(calcRTPCROffset + 0xA, true);

        public short TimeAlive =>
            _memory.ReadShort(calcRTPCROffset + 0xC);

        //CTF
        public short FlagScores =>
            _memory.ReadShort(calcRTPCROffset + 0xE, true);

        public short FlagSteals =>
            _memory.ReadShort(calcRTPCROffset + 0x10, true);

        public short FlagSaves =>
            _memory.ReadShort(calcRTPCROffset + 0x12, true);

        public short FlagUnk =>
            _memory.ReadShort(calcRTPCROffset + 0x14, true);
        //Assault
        public short BombScores =>
            _memory.ReadShort(calcRTPCROffset + 0x18, true);

        public short BombKills =>
            _memory.ReadShort(calcRTPCROffset + 0x1A, true);

        public short BombGrabs =>
            _memory.ReadShort(calcRTPCROffset + 0x1C, true);

        //Oddball
        public short BallScore =>
            _memory.ReadShort(calcRTPCROffset + 0x20, true);

        public short BallKills =>
            _memory.ReadShort(calcRTPCROffset + 0x22, true);

        public short BallCarrierKills =>
            _memory.ReadShort(calcRTPCROffset + 0x24, true);
        //KotH
        public short KingKillsAsKing =>
            _memory.ReadShort(calcRTPCROffset + 0x26, true);

        public short KingKilledKings =>
            _memory.ReadShort(calcRTPCROffset + 0x28, true);

       //Juggernaut
       public short JuggKilledJuggs =>
           _memory.ReadShort(calcRTPCROffset + 0x3C, true);

       public short JuggKillsAsJugg =>
           _memory.ReadShort(calcRTPCROffset + 0x3E, true);

       public short JuggTime =>
           _memory.ReadShort(calcRTPCROffset + 0x40, true);
       //Territories
       public short TerrTaken =>
           _memory.ReadShort(calcRTPCROffset + 0x46, true);

       public short TerrLost =>
           _memory.ReadShort(calcRTPCROffset + 0x48, true);

       #endregion
        
        public short[] MedalData
       {
           get
           {
               var arr = new short[24];
               for (var i = 0; i < 24; i++)
                   arr[i] = _memory.ReadShort(calcRTPCROffset + 0x4A + (i * 2), true);
               return arr;
           }
       }

        public List<short[]> WeaponData
       {
           get
           {
               var list = new List<short[]>();
               for (var i = 0; i < 36; i++)
               {
                   var arr = new short[6];
                   for (var j = 0; j < 6; j++)
                       arr[j] = _memory.ReadShort(calcRTPCROffset + 0xDE + (i * 0x10) + (j * 2), true);
                   list.Add(arr);
               }

               return list;
           }
       }

        [ScriptIgnore]
        private List<int[]> versusData;
        public List<int[]> VersusData
        {
            get
            {
                if (versusData == null)
                {
                    var list = new List<int[]>();
                    for (var i = 0; i < 16; i++)
                        list.Add(new[] {_memory.ReadInt(calcPCROffset + 0x90 + (i * 0x4), true), 0});
                    versusData = list;
                }

                return versusData;
            }
            set => versusData = value;
        }
    }
}
