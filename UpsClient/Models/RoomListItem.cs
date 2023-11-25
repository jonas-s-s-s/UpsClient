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

        public RoomListItem(int roomId, string state, string player1, string player2)
        {
            if(state == "0")
            {
                this.state = "Idle";
            } else if(state == "1")
            {
                this.state = "Running";
            } else if(state == "2")
            {
                this.state = "Paused";
            }

            this.roomId = roomId;
            this.player1 = player1;
            this.player2 = player2;
        }

        public static List<RoomListItem> ParseRoomList(string input)
        {
            List<RoomListItem> roomList = new List<RoomListItem>();

            string[] roomSegments = input.Split(new[] { "},{" }, StringSplitOptions.None);
            foreach (var roomSegment in roomSegments)
            {
                string cleanSegment = roomSegment.Trim('{', '}');
                string[] items = cleanSegment.Split(',');

                if (items.Length == 4)
                {
                    int roomId = int.Parse(items[2]);
                    string state = items[3];
                    string player1 = items[0];
                    string player2 = items[1];

                    RoomListItem roomItem = new RoomListItem(roomId, state, player1, player2);
                    roomList.Add(roomItem);
                }
                else
                {
                    throw new Exception($"ParseRoomList(): Invalid room definition: {roomSegment}");
                }
            }

            return roomList;
        }

    }
}
