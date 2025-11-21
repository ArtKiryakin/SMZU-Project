using ClassLibrary;
namespace Observability_ZMZU
{
    public class Program
    {
        static void Main(string[] args)
        {
            var list = CalculationObservability.ReadNodeFile("D:\\Учеба\\Мой сем.4\\Нирс\\Территории\\all_csv.csv");
            var list2 = CalculationObservability.CalculationQuantityBranches(list, "D:\\Учеба\\Срезы\\2023_01_11\\00_04_35\\roc_debug_before_OC");

            foreach (KeyValuePair<string, int> entry in list2)
            {
                Console.WriteLine($"Энергосистема: {entry.Key}, Кол-во ветвей: {entry.Value}");
            }
            Console.ReadLine();
        }
    }
}