using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Xml;

using static System.Runtime.InteropServices.JavaScript.JSType;


namespace CSharp2sem
{
    public struct Info
    {
        public object? patient { get; set; }


    }
    public class Program
    {

        public abstract class Person
        {


        }
        public class Patient : Person
        {
            public bool healthStatus { get; set; }
            public int indexPatient { get; set; }
            public Patient()
            {
                this.healthStatus = (new Random().Next(2) == 1) ? true : false;
            }

        }
        public class Doctor : Person
        {
            public bool busyStatus { get; set; } = false;
            public int timeForHelp { get; set; }

            public Doctor(int T)
            {
                this.timeForHelp = new Random().Next(T);
            }

        }

        public class InfectiousDiseasesDepartment
        {
            private readonly int _countDoctors;
            private readonly int _countPatients;
            private const int T = 30;
            private Patient?[] pPatients;
            private Doctor?[] pDoctors;
            private const int RoomSize = 4;
            private Patient?[] people_room = new Patient[RoomSize];
            private int countPatientsTreated = 0;

            public InfectiousDiseasesDepartment(int N, int M)
            {
                this._countPatients = N;
                this._countDoctors = M;
                pPatients = new Patient[N];
                pDoctors = new Doctor[M];
                for (int i = 0; i < N; i++) pPatients[i] = new Patient();
                for (int i = 0; i < M; i++) pDoctors[i] = new Doctor(T);
            }

            public async Task jsonRecordingFileAsync(Patient pat)
            {
                var info = new Info
                {
                    patient = pat
                };
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true // Установка параметра для форматирования с отступами и переносами строк
                };
                await using (FileStream fs = File.Create("config.json"))
                {
                    await JsonSerializer.SerializeAsync(fs, info, options);
                    // Запись данных асинхронно
                }
            }

            public void examinationRoom()
            {
                for (int i = 0; i < pPatients.Length; i++)
                {
                    if ((pPatients[i] != null) && (countPatientsTreated == 0))
                    {
                        // запихаем первого пациента в смотровую и относительно его здоровья 
                        // будем оценивать остальных
                        pPatients[i]!.indexPatient = i + 1;
                        jsonRecordingFileAsync(pPatients[i]!);
                        people_room![0] = pPatients[i]!;
                        pPatients[i] = null;
                        countPatientsTreated++;
                    }

                    if ((pPatients[i] != null) && (pPatients[i]!.healthStatus == people_room?[0]!.healthStatus))
                    {
                        pPatients[i]!.indexPatient = i + 1;
                        jsonRecordingFileAsync(pPatients[i]!);
                        people_room[countPatientsTreated] = pPatients[i]!;
                        pPatients[i] = null;
                        countPatientsTreated++;
                        System.Threading.Thread.Sleep(1000);
                    }

                    if (countPatientsTreated == people_room?.Length)
                    {
                        break;
                    }
                }
            }
        }

        public static async Task Main()
        {
            Random rand = new Random();
            int N = rand.Next(1, 8);
            int M = rand.Next(1, 3);
            InfectiousDiseasesDepartment apart = new(N, M);
            apart.examinationRoom();
            await apart.jsonRecordingFileAsync();
        }

    }
}
/*                {

                pPatient newPatient = pPatients[countPatientsTreated]!;

                if (people_room[0].healthStatus == newPatient.healthStatus)
                {
                    people_room[countPatientsTreated++] = newPatient;
                }
                else
                {
                    pPatient newPatient = pPatients[countPatientsTreated]!;
                }
                Console.WriteLine(countPatientsTreated);




            }*/