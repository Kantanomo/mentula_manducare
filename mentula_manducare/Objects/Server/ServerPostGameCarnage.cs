using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using MentulaManducare;

namespace mentula_manducare.Objects.Server
{
    public class ServerPostGameCarnage
    {
        [ScriptIgnore]
        private ServerContainer Server;
        [ScriptIgnore]
        private static string BasePath = $"{MainThread.BasePath}\\Stats";

       

        public ServerPostGameCarnage(ServerContainer Server)
        {
            this.Server = Server;
        }

        public string VariantName =>
            Server.ServerMemory.ReadStringUnicode(0x4DC3D4, 16, true);

        public byte VariantType =>
            Server.ServerMemory.ReadByte(0x4DC414, true);

        public string Scenario =>
            Server.ServerMemory.ReadStringUnicode(0x4DC504, 200, true).Split('\\').Last();

        public List<PostGameCarnageEntry> Players
        {
            get
            {
                var list = new List<PostGameCarnageEntry>();
                for (byte index = 0; index < 16; index++)
                {
                    var newEntry = new PostGameCarnageEntry(Server.ServerMemory, index);
                    if (newEntry.XUID != 0 && newEntry.Gamertag != "")
                        list.Add(newEntry);
                }

                //Shrink Versus Table to the size of players
                foreach (var t in list)
                    t.VersusData.RemoveRange(list.Count, (16 - list.Count));

                //Populate Versus Data
                for (var i = 0; i < list.Count; i++)
                {
                    for (var j = 0; j < list.Count; j++)
                    {
                        list[i].VersusData[list[j].EndGameIndex][1] = list[j].VersusData[list[i].EndGameIndex][0];
                    }
                }

                return list;
            }
        }

        public void SaveJSON()
        {
            File.AppendAllText($"{BasePath}\\{Server.FileSafeName}_{DateTime.Now.ToFileTimeUtc()}.json",
                new JavaScriptSerializer().Serialize(this));
        }
    }
}
