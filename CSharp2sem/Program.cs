
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CSharp2sem
{
    class Program
    {
        public class DiseasePerson
        {

            // person probability

            private Dictionary<string, string> states;
            private Dictionary<string, List<string>> connections;

            // person probability

            private readonly double _infect;
            private readonly double _cure;

            Random _random;

            public DiseasePerson(Dictionary<string, object> data, double p_infect, double p_cure)
            {

                this._infect = p_infect;
                this._cure = p_cure;

                var initial_infected = ( (Newtonsoft.Json.Linq.JArray) data["initial_infected"]).ToObject<List<string>>() ?? throw new NullReferenceException("Неправильно описан объект initial infected в JSON файле");
                states = ( (Newtonsoft.Json.Linq.JArray) data["people"]).ToObject<List<string>>()?.ToDictionary(person => person, person => initial_infected.Contains(person) ? "infected" : "healthy") ?? throw new NullReferenceException("Неправильно описан объект people в JSON файле");
                connections = ( (Newtonsoft.Json.Linq.JObject) data["connections"]).ToObject<Dictionary<string, List<string>>>() ?? throw new NullReferenceException("Неправильно описан объект connections в JSON файле");
                _random = new Random();
            }

            #region SpreadDisease

            public async Task SpreadDisease()
            {
                List<string> new_infected = new List<string>();
                List<string> new_immune = new List<string>();

                foreach (var person in states.Keys)
                {
                    // ищем больного

                    if (states[person] == "infected")
                    {
                        // выявление статуса с теми, с кем он контактировал.

                        foreach (var contacted in connections[person])
                        {

                            if (states[contacted] == "healthy" && (_random.NextDouble() < _infect) )
                            {
                                new_infected.Add(contacted);
                            }

                        }



                        if (_random.NextDouble() < _cure)
                        {
                            // человек выздоровил

                            // c вероятностью 0.3 он приобретает имунитет к болезни

                            if (_random.NextDouble() < 0.3)
                            {
                                new_immune.Add(person);
                            }
                            else
                            {
                                states[person] = "healthy";
                            }

                        }
                    }
                }

                foreach (var person in new_infected)
                {
                    states[person] = "infected";
                }

                foreach (var person in new_immune)
                {
                    states[person] = "immune";
                }


                await Task.CompletedTask;
            }

            public async Task Run(int days)
            {
                for (int i = 0; i < days; i++)
                {
                    await SpreadDisease();
                }
            }

            #endregion

            #region findOfMetods 
            public List<string> FindHealthy()
            {
                return states.Where(personStatus => personStatus.Value == "healthy").Select(person => person.Key).ToList()!;
            }

            public List<string> FindImmune()
            {
                return states.Where(personStatus => personStatus.Value == "immune").Select(person => person.Key).ToList()!;
            }

            public List<string> FindInfected()
            {
                return states.Where(personStatus => personStatus.Value == "infected").Select(person => person.Key).ToList();
            }

            public List<string> FindImmunePersonAndSick()
            {
                var immuneWithSick = new List<string>();

                foreach (var person in FindImmune() )
                {
                    if (connections[person].All(connected => states[connected] == "infected"))
                    {
                        immuneWithSick.Add(person);
                    }
                }

                return immuneWithSick;
            }

            public List<string> FindHealthyPersonAndSick()
            {
                var healthyWithSick = new List<string>();

                foreach (var person in FindHealthy())
                {
                    if (connections[person].All(connected => states[connected] == "infected"))
                    {
                        healthyWithSick.Add(person);
                    }
                }

                return healthyWithSick;
            }

            #endregion

        }



        public static async Task<Dictionary<string, object>> ReadDataAsync(string filePath)
        {
            var json = await File.ReadAllTextAsync(filePath);
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(json) ?? throw new InvalidDataException("Не удалось считать данные JSON файла");
        }


        public static async Task<int> Main(string[] args)
        {
            try
            {

                string jsonFilePath = "data.json";
                var data = await ReadDataAsync(jsonFilePath);

                var systemSpreadDisease = new DiseasePerson(data, 0.6, 0.3);
                await systemSpreadDisease.Run(10);

                Console.WriteLine("Healthy people: " + string.Join(", ", systemSpreadDisease.FindHealthy()));

                Console.WriteLine("Immune people: " + string.Join(", ", systemSpreadDisease.FindImmune()));

                Console.WriteLine("Infected people: " + string.Join(", ", systemSpreadDisease.FindInfected()));
                
                if (systemSpreadDisease.FindImmunePersonAndSick().Count == 0)
                {
                    Console.WriteLine("Immune with sick people: there are no such people");
                }else
                {
                    Console.WriteLine("Immune with sick people: " + string.Join(", ", systemSpreadDisease.FindImmunePersonAndSick()));
                }

                if (systemSpreadDisease.FindHealthyPersonAndSick().Count == 0)
                {
                    Console.WriteLine("Immune with sick people: there are no such people");
                }
                else
                {
                    Console.WriteLine("Healthy with all sick people: " + string.Join(", ", systemSpreadDisease.FindHealthyPersonAndSick()));
                }

               

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 6;
            }
        }
    }
}