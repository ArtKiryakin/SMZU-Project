using ClassLibrary;
using InteractionWithTheDatabase;
using InteractionWithTheDatabaseAndFileStorage;
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
            Dictionary<string, Dictionary<List<string>, int>> resultOS = OS.GoOS(new List<int> {19967, 49004}, FileStorageConnection.GetRastrFiles("D:\\Учеба\\Срезы", new DateTime(2023, 1, 11, 1, 0, 23), new DateTime(2023, 1, 11, 2, 23, 30)));
            foreach (var system in resultOS)
            {
                foreach (var nodesData in system.Value)
                {
                    string nodesString = string.Join(", ", nodesData.Key);

                    Console.WriteLine($"│ {system.Key} │ {nodesString} │ {nodesData.Value} │");
                }
            }
            Console.ReadLine();
        }
    }
}