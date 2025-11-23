using ClassLibrary;
using InteractionWithTheDatabase;
namespace Observability_ZMZU
{
    public class Program
    {
        static void Main(string[] args)
        {
            //var list = CalculationObservability.ReadNodeFile("D:\\Учеба\\Мой сем.4\\Нирс\\Территории\\all_csv.csv");
            //var list2 = CalculationObservability.CalculationQuantityBranches(list, "D:\\Учеба\\Срезы\\2023_01_11\\00_04_35\\roc_debug_before_OC", 3);
            // Использование
            // Использование
            DatabaseConection.FillingInNodeAffiliation();
            //var list2 = CalculationObservability.CalculationQuantityNodes(list, 3);
            //foreach (KeyValuePair<string, int> entry in list2)
            //{
            //    Console.WriteLine($"Энергосистема: {entry.Key}, Кол-во : {entry.Value}");
            //}
            Console.ReadLine();
        }
    }
}