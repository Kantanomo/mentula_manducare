using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using mentula_manducare.Classes;
using mentula_manducare.Enums;

namespace mentula_manducare.Objects
{
    public class PlayerContainer
    {
        [ScriptIgnore]
        public MemoryHandler Memory;

        public int PlayerIndex;
        public PlayerContainer(MemoryHandler Memory, int PlayerIndex)
        {
            this.Memory = Memory;
            this.PlayerIndex = PlayerIndex;
        }

        public string Name
        {
            get => Memory.ReadStringUnicode(0x9917DA + (PlayerIndex * 0x40), 16, true);
            set => Memory.WriteStringUnicode(0x9917DA + (PlayerIndex * 0x40), value, true);
        }

        public Team Team
        {
            get => (Team)Memory.ReadByte(0x530F4C + (PlayerIndex * 0x128), true);
            set => Memory.WriteByte(0x530F4C + (PlayerIndex * 0x128), (byte) value, true);
        }

        public Biped Biped
        {
            get => (Biped) Memory.ReadByte(0x3000274C + (PlayerIndex * 0x204));
            set => Memory.WriteByte(0x3000274C + (PlayerIndex * 0x204), (byte) value);
        }

        public byte BipedPrimaryColor
            => Memory.ReadByte(0x530E8C + (PlayerIndex * 0x128), true);

        public byte BipedSecondaryColor
            => Memory.ReadByte(0x530E8D + (PlayerIndex * 0x128), true);

        public byte PrimaryEmblemColor
            => Memory.ReadByte(0x530E8E + (PlayerIndex * 0x128), true);

        public byte SecondaryEmblemColor
            => Memory.ReadByte(0x530E8F + (PlayerIndex * 0x128), true);

        public byte EmblemForeground
            => Memory.ReadByte(0x530E91 + (PlayerIndex * 0x128), true);

        public byte EmblemBackground
            => Memory.ReadByte(0x530E92 + (PlayerIndex * 0x128), true);

        public byte EmblemToggle
            => (byte) (Memory.ReadByte(0x530E93 + (PlayerIndex * 0x128), true) == 0 ? 1 : 0);

        public string EmblemURL =>
            $"http://halo.bungie.net/Stats/emblem.ashx?s=120&0={BipedPrimaryColor.ToString()}&1={BipedSecondaryColor.ToString()}&2={PrimaryEmblemColor.ToString()}&3={SecondaryEmblemColor.ToString()}&fi={EmblemForeground.ToString()}&bi={EmblemBackground.ToString()}&fl={EmblemToggle.ToString()}";
    }
}
