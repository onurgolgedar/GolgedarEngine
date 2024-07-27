using System.Collections.Generic;
using System.Linq;

namespace GolgedarEngine
{
    public class Room
    {
        RoomData roomData;
        public Room(string roomName, RoomData roomData)
        {
            this.roomData = roomData;
            roomData.Load(roomName);

            RoomName = roomName;
        }

        public void Load()
        {
            Instances_SortedByCreation = roomData.Instances_SortedByCreation;
            Instances_SortedByDepth = roomData.Instances_SortedByDepth;
            InstancesWithCollisionMask_SortedByCreation = new List<GameObject>(Instances_SortedByCreation.Values.Where(x => x.CollisionMask.Size != Vector.Zero).ToList());
        }

        public SortedList<int, GameObject> Instances_SortedByCreation { get; internal set; } = new SortedList<int, GameObject>();
        public List<GameObject> InstancesWithCollisionMask_SortedByCreation { get; internal set; } = new List<GameObject>();
        public List<GameObject> Instances_SortedByDepth { get; internal set; } = new List<GameObject>();
        public string RoomName { get; }
    }
}