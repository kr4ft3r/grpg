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

    public static class FFS
    {
        public static void Log(string format, params object[] arg)
        {
            System.Console.WriteLine(format, arg);
        }
    }
}