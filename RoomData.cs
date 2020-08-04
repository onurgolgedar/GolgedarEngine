using SFML.System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GolgedarEngine
{
   public abstract class RoomData
   {
      public RoomData()
      {
         Instances_SortedByDepth = new List<GameObject>();
      }

      public abstract ImmutableList<string> GetRooms();
      public abstract void SetDesign(string roomName);
      public void PutInstance(GameObject gameObject, short depth = 0)
      {
         PutInstance(gameObject, new Vector2f(float.MinValue, float.MinValue), depth);
      }
      public void PutInstance(GameObject gameObject, Vector2f position, short depth = 0)
      {
         gameObject.Position = position;
         gameObject.Depth = depth;

         Instances_SortedByDepth.Add(gameObject);

         Instances_SortedByCreation.TryAdd(InstanceCount, gameObject);
         InstanceCount = Instances_SortedByCreation.Count;
      }

      internal void Load(string roomName)
      {
         InstanceCount = 0;
         Instances_SortedByDepth.Clear();
         Instances_SortedByCreation.Clear();

         SetDesign(roomName);

         Instances_SortedByDepth = Instances_SortedByDepth.OrderBy(gameObject => -gameObject.Depth).ToList();
      }

      public int InstanceCount { get; internal set; }
      public List<GameObject> Instances_SortedByDepth { get; internal set; }
      public SortedList<int, GameObject> Instances_SortedByCreation { get; internal set; } = new SortedList<int, GameObject>();
   }
}