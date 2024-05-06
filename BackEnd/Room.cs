using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd
{
    internal class Room
    {
        public int Id { get; }
        public Player RoomOwner { get; }
        public Player RoomGuest { get; set;}
        public List<Player> Watchers { get;  }
        public string Selectedcategory { get; set; }
        public Game Game { get; private set; }
        public bool PendingJoin { get; set; } 
        public bool GameRunning { get; set; }
        public Room(int id,Player roomOwner)
        {
            
            this.Id = id;
            this.RoomOwner = roomOwner;
            this.PendingJoin = false;
            this.GameRunning = false;
            Watchers = new List<Player>();
        }
        
        public void CreateGame(string word)
        {
            Game= new Game(word);
            GameRunning = true;
        }

        
        public override string ToString()
        {
            if (RoomGuest!=null)
            {
                return "{" + $"{Id.ToString()},{GameRunning.ToString()},{RoomOwner.Name},{RoomGuest.Name}" + "}";
            }
            return "{"+ $"{Id.ToString()},{GameRunning.ToString()},{RoomOwner.Name},{PendingJoin.ToString()}" +"}";
        }

    }
}
