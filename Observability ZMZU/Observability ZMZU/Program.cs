using ClassLibrary;
using InteractionWithTheDatabase;
using InteractionWithTheDatabaseAndFileStorage;
using System.Collections.Generic;
namespace Observability_ZMZU
{
    public class Program
    {
        static void Main(string[] args)
        {
            //Dictionary<string, double> dict = CalculationObservability.CalculateObservability("D:\\Учеба\\Срезы", new DateTime(2023, 1, 11, 1, 0, 23), new DateTime(2023, 1, 11, 2, 23, 30), 1);
            //foreach (KeyValuePair<string, double> pair in dict)
            //{
            //    Console.WriteLine($"{pair.Key}: {pair.Value}");
            //}

            //List<string> a = FileStorageConnection.GetRastrFiles("D:\\Учеба\\Срезы", new DateTime(2023, 1, 11, 1, 0, 23), new DateTime(2023, 1, 11, 14, 2, 3));
            //foreach(string i in a)
            //{
            //    Console.WriteLine(i);
            //}

            //Dictionary<string, Dictionary<string, int>> resultOS = OS.GoOS(new List<int> {19967, 49004, 49004, 2, 19250}, FileStorageConnection.GetRastrFiles("D:\\Учеба\\Срезы", new DateTime(2023, 1, 11, 1, 0, 23), new DateTime(2023, 1, 11, 1, 7, 30)));
            //foreach(var i in resultOS)
            //{
            //    foreach(var j in i.Value)
            //    {
            //        Console.WriteLine($"Срез:{i.Key} ТИ:{j.Key} -> Результат ОС: {j.Value}");
            //    }
            //}

            //Dictionary<string, double> kn = OS.CalcularteKn(resultOS);
            ////Console.WriteLine("kn:");
            //foreach (var ti in kn)
            //{
            //    Console.WriteLine($"ТИ:{ti.Key} -> Вероятность неуспешного ОС: {ti.Value}");
            //}

            //Dictionary<string, double> kc = OS.CalcularteKc(resultOS);
            //Console.WriteLine("kc:");
            //foreach (var ti in kc)
            //{
            //    Console.WriteLine($"{ti.Key} -> {ti.Value}");
            //}
            //Console.ReadLine();
            //PrintAlignedChart(OS.CalcularteKc(resultOS));
            var myVariables = new Dictionary<string, double> { };
            while(true)
            {
                Console.WriteLine("Введите название переменной");
                string variable = Console.ReadLine();
                Console.WriteLine("Введите значение переменной");
                double value = Convert.ToDouble(Console.ReadLine());
                if(value == null || value == 56.0 || variable == null || variable == "56")
                {
                    break;
                }
                myVariables[variable] = value;

                
            }
            Console.WriteLine("Введите формулу:");
            Console.WriteLine($"{AdditionalCalculations.Evaluate(Console.ReadLine(), myVariables)}");

        }

        public static void PrintAlignedChart(Dictionary<string, double> data)
        {
            int chartHeight = 8;
            double maxValue = data.Values.Max();

            // Правильное выравнивание с PadRight
            int maxKeyLength = data.Keys.Max(k => k.Length);
            int padding = maxKeyLength + 4;

            Console.WriteLine("ГИСТОГРАММА С ВЫРАВНИВАНИЕМ");
            Console.WriteLine(new string('=', 60));

            foreach (var item in data)
            {
                // ПРАВИЛЬНО: используем PadRight для выравнивания
                Console.Write($"{item.Key.PadRight(padding)}");

                // График
                int barLength = (int)((item.Value / (double)maxValue) * 30);
                string bar = new string('█', barLength);

                Console.WriteLine($"{bar} {item.Value}");
            }
        }
    }
}