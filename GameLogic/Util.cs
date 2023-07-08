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
            set { 
                if (value <= 0)
                {
                    if (_counts.ContainsKey(key)) _counts.Remove(key);
                    return;
                }
                if (!_counts.ContainsKey(key)) _counts.Add(key, value);
                else _counts[key] = value;               
            }  
        } 
        public IEnumerator<KeyValuePair<T, int>> GetEnumerator() => _counts.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public bool Contains(T key) => _counts.ContainsKey(key);
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
            foreach (var item in this) this[item.Key] -= 1;
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