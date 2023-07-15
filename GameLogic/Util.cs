using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GRPG.GameLogic
{
    public class CounterDict<T> : IEnumerable<KeyValuePair<T, int>>
    {
        private Dictionary<T, int> _counts = new Dictionary<T, int>();

        public CounterDict() {}
        public CounterDict(T key, int count) => this[key] = count;
        public CounterDict(IEnumerable<KeyValuePair<T, int>> counts) => SetAll(counts);

        public int this[T key]  
        {  
            get { return _counts.ContainsKey(key) ? _counts[key] : 0; }  
            set { _counts[key] = value <= 0 ? 0 : value; }  
        } 
        public IEnumerator<KeyValuePair<T, int>> GetEnumerator() => _counts.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _counts.GetEnumerator();

        public bool Contains(T key) => this[key] > 0;
        public void Add(T key, int count) => this[key] += count;

        public void Add(IEnumerable<KeyValuePair<T, int>> counts)
        {
            foreach (var item in counts) this[item.Key] += item.Value;
        }
        public void Substract(IEnumerable<KeyValuePair<T, int>> counts)
        {
            foreach (var item in counts) this[item.Key] -= item.Value;
        }
        public void SetAll(IEnumerable<KeyValuePair<T, int>> counts)
        {
            foreach (var item in counts) this[item.Key] = item.Value;
        }
        public void DecrementAll()
        {
            foreach (var key in _counts.Keys.ToArray()) this[key] -= 1;
        }
        public bool Contains(IEnumerable<KeyValuePair<T, int>> counts)
        {
            foreach (var item in counts)
            {
                if (this[item.Key] < item.Value) return false;
            }
            return true;
        }
    }
    
    public static class Dice
    {
        private static System.Random _rnd = new System.Random();
        public static int Roll(int numSides) => _rnd.Next(numSides) + 1;
    }

    public static class Util
    {
        public static List<int> GetNeighbours(Mission mission, int location)
        {
            var result = new List<int>();
            for (int i = 0; i < mission.NumLocations; i++)
            {
                if (mission.Connections[location, i].CanMove) result.Add(i);
            }
            return result;
        }
        public static List<int> GetNeighbours(Actor actor) 
            => GetNeighbours(actor.Mission, actor.Location);
        public static List<Actor> GetActors(Mission mission, int location, Team team) 
            => mission.Actors.Where(a => a.Location == location && a.Team == team).ToList();

        public static Dictionary<Actor, List<int>> GetPathToActorsOnTeam(Actor actor, Team team)
        {
            var sorted = new Dictionary<int, List<int>>();
            var next = new Dictionary<int, List<int>>();

            // Start from actor's location
            sorted.Add(actor.Location, new List<int>());
            next.Add(actor.Location, new List<int>());

            // Loop while there is more locations to be added to _sorted
            while (next.Count > 0)
            {
                // Keep track of what we added to _sorted so we can traverse BFS
                var newlyAdded = new Dictionary<int, List<int>>();

                // Go through the next set of nodes (with their paths in the dict)
                foreach (var item in next)
                {
                    // Add each non-visited child and its path to _sorted and newlyAdded
                    foreach (var child in Util.GetNeighbours(actor.Mission, item.Key))
                    {
                        if (sorted.ContainsKey(child)) continue;
                        var path = item.Value.ToList();
                        path.Add(child);
                        sorted.Add(child, path);
                        newlyAdded.Add(child, path);
                    }
                }
                next = newlyAdded;
            }

            // Get the result from sorted nodes
            var result = new Dictionary<Actor, List<int>>();
            foreach (var locpath in sorted)
                foreach (var a in GetActors(actor.Mission, locpath.Key, team)
                    .Where(a => a.Status != ActorStatus.Downed)
                    .Where(a => !a.Equals(actor) && a.Team == team))
                    result.Add(a, locpath.Value);
            return result;
        }
   }

    public static class FFS
    {
        public static void Log(string format, params object[] arg)
        {
            System.Console.WriteLine(format, arg);
        }
    }
}