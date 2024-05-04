using System;
using System.Collections.Generic;
using System.Dynamic;
using static CSharp2sem.Program;
using System.Threading.Tasks;
namespace CSharp2sem
{

    public class Program
    {
        public class Cache
        {
            private readonly  TimeSpan _timeLifeRecord;
            private readonly Int32 _maxCountRecord;
            private readonly Dictionary<string, DateTime> _timeLifeRecords = new Dictionary<string, DateTime>();
            public readonly Dictionary<string, object> _cache = new Dictionary<string, object>();
            
            public Cache(TimeSpan timeLifeRecord, Int32 maxCountRecord) { 
                this._timeLifeRecord = timeLifeRecord;
                this._maxCountRecord = maxCountRecord;
                _cache.EnsureCapacity(maxCountRecord);
                _timeLifeRecords.EnsureCapacity(maxCountRecord);
            }

            public object? Get(string key)
            {

                if (!(_cache.ContainsKey(key)))
                {
                    throw new KeyNotFoundException();
                }

                return _cache[key];
            }
            public async void Save(object CacheData, string key)
            {
                
 
                if ( _cache.ContainsKey(key) ){
                    throw new ArgumentException();
                }

                if (_cache.Count == _maxCountRecord)
                {

                    RemovedOldestObject(); // удаление самого старого объекта

                }

                _cache.Add(key, CacheData);
                DateTime timeNow = DateTime.Now;
                _timeLifeRecords.Add(key, timeNow);
                await ClearCacheAsync();   // вызов асинхронного метода
                Thread.Sleep(4000);

                Console.WriteLine(_cache.Count);
            }
            private void RemovedOldestObject()
            {
                
                DateTime timeMax = DateTime.MaxValue;
                string? key = null;
                foreach (var item in _timeLifeRecords)
                {

                    if (item.Value < timeMax)
                    {
                        timeMax = item.Value;//DateTime.Now.TimeOfDay - timeMax.TimeOfDay;
                        key = item.Key;
                    }
            
                }
                if (key != null)
                {
                    _timeLifeRecords.Remove(key);
                    _cache.Remove(key);
                }

            }
            private async Task ClearCacheAsync()
            {
                Console.WriteLine("Начало метода PrintAsync"); // выполняется синхронно
                await Task.Run(() => Print());
                Console.WriteLine("Конец метода PrintAsync");
            }
            void Print()
            {
                Thread.Sleep(3000);     // имитация продолжительной работы
                Console.WriteLine("Hello METANIT.COM");
            }

        }
        public static int Main()
        {
            TimeSpan time = new TimeSpan(0, 0, 5, 0);
            var cache = new Cache(time, 2);
            cache.Save("sdsds", "1");
            Console.WriteLine(cache.Get("1"));
            cache.Save("sdsSDSDSds", "2");
            Console.WriteLine(cache.Get("2"));
            cache.Save("sdsSSDFFDSDSDSds", "0");
            Console.WriteLine(cache.Get("0"));

            return 0;
        }

    }
}