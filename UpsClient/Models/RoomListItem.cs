using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpsClient.Models
{
    public class RoomListItem
    {
        public int roomId { get; set; }
        public string state { get; set; }
        public string player1 { get; set; }
        public string player2 { get; set; }
        public string capacity { get; set; }

        public RoomListItem(int roomId,string state, string player1, string player2, string capacity) { 
            this.roomId = roomId;
            this.state = state;
            this.player1 = player1;
            this.player2 = player2;
            this.capacity = capacity;
        }   

    }
}
