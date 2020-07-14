using SFML.System;
using System.Collections.Generic;
using System.Linq;

namespace GolgedarEngine
{
   public abstract class RoomData
   {
      public RoomData()
      {
         Instances_SortedByDepth = new List<GameObject>();
      }

      public abstract void Load(string roomName);
      public void PutInstance(GameObject gameObject, Vector2f position = new Vector2f(), short depth = 0)
      {
         gameObject.Position = position;
         gameObject.Depth = depth;

         Instances_SortedByDepth.Add(gameObject);
         Instances_SortedByDepth.OrderBy(gameObject => gameObject.Depth);

         Instances_SortedByCreation.TryAdd(InstanceCount, gameObject);
         InstanceCount = Instances_SortedByCreation.Count;
      }

      public int InstanceCount { get; internal set; }
      public List<GameObject> Instances_SortedByDepth { get; internal set; }
      public SortedList<int, GameObject> Instances_SortedByCreation { get; internal set; } = new SortedList<int, GameObject>();
   }
}