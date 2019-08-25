using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mentula_manducare.Objects
{
    public class PlayerCollection : System.Collections.CollectionBase, IEnumerable<PlayerContainer>
    {
        public PlayerCollection(ServerContainer Server)
        {
            for(var i = 0; i < Server.PlayerCount; i++)
                Add(new PlayerContainer(Server.ServerMemory, i));
        }

        public PlayerContainer this[int index] => 
            (PlayerContainer) List[index];

        public PlayerContainer this[string playerName] 
            => List.Cast<PlayerContainer>().FirstOrDefault(playerContainer => playerContainer.Name == playerName);

        public void Add(PlayerContainer Player) =>
            List.Add(Player);

        public new IEnumerator<PlayerContainer> GetEnumerator()
        {
            foreach (PlayerContainer player in List)
                yield return player;
        }
    }
}
