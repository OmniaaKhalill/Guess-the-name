using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd
{
    enum PlayerState
    {
        Idle,FirstPlayer,SecondPlayer,Watcher
    }
    enum PlayAgain
    {
        None,
        Accepted,
        Refused
    }
    internal class Player
    {
        public int Id {  get;  }
        public string Name { get;  }
        public TcpClient Client { get;  }

        public PlayerState State { get; set; }
        public PlayAgain PlayAgain { get; set; }
        public Player(List<Player> players, string name,TcpClient client)
        {
            Id = new Random().Next(0, 1000);
            int[] ids = new int[players.Count];


            foreach (var player in players)
            {
                ids.Append(player.Id);
            }
            while (ids.Contains(Id))
            {
                Id = new Random().Next(0, 1000);
            }
            Name = name;
            Client = client;
            State = PlayerState.Idle;
            
        }

        //create room and change player state
        public Room CreateRoom(List<Room> rooms)
        {
            int id= new Random().Next(1,1000);
            int[] ids= new int[rooms.Count];
            this.State = PlayerState.FirstPlayer;
            
            foreach (var room in rooms)
            {
                    ids.Append(room.Id);
            }
            while (ids.Contains(Id))
            {
                id = new Random().Next(1, 1000);
            }                     
            Room tempRoom = new Room(id, this);
            return tempRoom;// give unique id to each room
        }

        
    }
}
