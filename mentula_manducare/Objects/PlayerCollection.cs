using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mentula_manducare.Objects
{
    public class PlayerCollection : System.Collections.CollectionBase
    {
        public PlayerCollection(ServerContainer Server)
        {
            for(var i = 0; i < Server.PlayerCount; i++)
                Add(new PlayerContainer(Server.ServerMemory, i));
        }

        public PlayerContainer this[int index] => 
            (PlayerContainer) List[index];

        public void Add(PlayerContainer Player) =>
            List.Add(Player);

    }
}
