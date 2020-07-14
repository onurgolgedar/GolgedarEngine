using System.Collections.Generic;

namespace GolgedarEngine
{
   public class Room
   {
      public SortedList<int, GameObject> Instances_SortedByCreation { get; internal set; }
      public SortedList<int, GameObject> Instances_SortedByDepth { get; internal set; }
      public string RoomName { get; }

      public Room(string roomName, RoomData roomData)
      {
         roomData.Load(roomName);

         Instances_SortedByCreation = roomData.Instances_SortedByCreation;
         Instances_SortedByDepth = roomData.Instances_SortedByCreation;
         RoomName = roomName;
      }
   }
}