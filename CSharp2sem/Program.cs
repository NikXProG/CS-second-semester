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
            private readonly Dictionary<string, DateTime> _timeLifeRecords;
            private readonly Dictionary<string, object> _cache;
            private readonly Timer _timer;

            public Cache(TimeSpan timeLifeRecord, Int32 maxCountRecord) {
                _timeLifeRecord = timeLifeRecord;
                _maxCountRecord = maxCountRecord;
                _timeLifeRecords = new Dictionary<string, DateTime>();
                _cache = new Dictionary<string, object>();
                _timer = new Timer(CheckExpiredRecords!, null, timeLifeRecord, timeLifeRecord);
            }

            public object Get(string key)
            {
                lock (_cache) { 
                    if (!_cache.ContainsKey(key))
                    {
                        throw new KeyNotFoundException();
                    }

                    return _cache[key];
                }
            }
            public void Save(object cacheData, string key)
            {

                lock(_cache){

                    if (_cache.ContainsKey(key))
                    {
                        throw new ArgumentException("Ключ уже существует в кэше.");
                    }

                    if (_cache.Count == _maxCountRecord)
                    {
                        RemoveOldestObject(); // удаление самого старого объекта
                    }

                    _cache.Add(key, cacheData);
                    DateTime timeNow = DateTime.Now;
                    _timeLifeRecords.Add(key, timeNow);

                }    
 
 
            }
            private void RemoveOldestObject()
            {
                DateTime timeMax = DateTime.MaxValue;
                string? key = null;

                foreach (var item in _timeLifeRecords)
                {
                    if (item.Value < timeMax)
                    {
                        timeMax = item.Value;
                        key = item.Key;
                    }
                }

                if (key != null)
                {
                    _timeLifeRecords.Remove(key);
                    _cache.Remove(key);
                }
            }

            private void CheckExpiredRecords(object state)
            {
                lock (_cache)
                {
                    List<string> expiredKeys = new List<string>();

                    foreach (var kvp in _timeLifeRecords)
                    {
                        if (DateTime.Now - kvp.Value > _timeLifeRecord)
                        {
                            expiredKeys.Add(kvp.Key);
                        }
                    }

                    foreach (var key in expiredKeys)
                    {
                        _timeLifeRecords.Remove(key);
                        _cache.Remove(key);
                    }
                }
            }

        }
        public static async Task<int> Main()
        {

            try
            {
                TimeSpan time = TimeSpan.FromSeconds(3);
                var cache = new Cache(time, 2);

                var task = Task.Run(() =>
                {
                    cache.Save("sdsds", "1");
                    Console.WriteLine($"Выполнился первый поток. Данные записи: {cache.Get("1")}");
                });


                var task1 = Task.Run(() => 
                {
                    cache.Save("SDSADASDSADSAD", "2");
                    Console.WriteLine($"Выполнился второй поток. Данные записи: {cache.Get("2")}");
                });

                await Task.WhenAll(task, task1);

                await Task.Delay(4000); // Ждем время

                var task2 = Task.Run(() =>
                {
                    cache.Save("adasdsadsadsaSDSAdsadasd", "3");
                    Console.WriteLine($"Выполнился третий поток. Данные записи: {cache.Get("3")}");
                });

                var task3 = Task.Run(() =>
                {
                    cache.Save("sdssdsdsds", "4");
                    Console.WriteLine($"Выполнился четвертый поток. Данные записи: {cache.Get("4")}");
                });

                await Task.WhenAll(task2, task3);

                Console.WriteLine(cache.Get("1"));

                return 0;

            }
            catch (KeyNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
                return -1;

            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -2;
            }



        }

    }
}