using ClassLibrary;
using Npgsql;
using System.Data;
using System.Xml.Linq;

namespace InteractionWithTheDatabase
{
    public class DatabaseConection
    {
        public static void FillingInNodeAffiliation()
        {
            string connectionString = "Host=localhost;Port = 5432;Database=Observability_SMZU;Username=postgres;Password=fhntv2001";
            List<PowerSystem> powerSystemsList = CalculationObservability.ReadNodeFile("D:\\Учеба\\Мой сем.4\\Нирс\\Территории\\all_csv.csv");
            int idUnifiedEnergySystem = 1;
            int idEnergySystem = 1;
            int idEnergyDistrict = 1;
            Dictionary<string, int> dictUnifiedEnergySystem = new Dictionary<string, int> { };
            Dictionary<string, List<int>> dictEnergySystem = new Dictionary<string, List<int>> { };
            Dictionary<string, List<int>> dictEnergyDistrict = new Dictionary<string, List<int>> { };
            foreach (PowerSystem powerSystem in powerSystemsList)
            {
                if (!dictUnifiedEnergySystem.ContainsKey(powerSystem.UnifiedEnergySystem))
                {
                    dictUnifiedEnergySystem[powerSystem.UnifiedEnergySystem] = idUnifiedEnergySystem;
                    idUnifiedEnergySystem++;
                }
            }
            foreach (PowerSystem powerSystem in powerSystemsList)
            {
                if (!dictEnergySystem.ContainsKey(powerSystem.EnergySystem))
                {
                    foreach(KeyValuePair<string, int> entry in dictUnifiedEnergySystem)
                    {
                        if(powerSystem.UnifiedEnergySystem == entry.Key)
                        {
                            dictEnergySystem[powerSystem.EnergySystem] = [idEnergySystem, entry.Value];
                            idEnergySystem++;
                        }
                    }
                }
            }
            foreach (PowerSystem powerSystem in powerSystemsList)
            {
                if (!dictEnergyDistrict.ContainsKey(powerSystem.EnergyDistrict))
                {
                    foreach (KeyValuePair<string, List<int>> entry in dictEnergySystem)
                    {
                        if (powerSystem.EnergySystem == entry.Key)
                        {
                            List<int> i = entry.Value;
                            dictEnergyDistrict[powerSystem.EnergyDistrict] = [idEnergyDistrict, i[0]];
                            idEnergyDistrict++;
                        }
                    }
                }
            }
            foreach (KeyValuePair<string, int> entry in dictUnifiedEnergySystem)
            {
                string name = entry.Key;      // Ключ - имя
                int id = entry.Value;        // Значение - возраст
            
                Console.WriteLine($"{name}, {id}");
            }
            foreach (KeyValuePair<string, List<int>> entry in dictEnergySystem)
            {
                string name = entry.Key;      // Ключ - имя
                List<int> id = entry.Value;        // Значение - возраст
            
                Console.WriteLine($"{name}, {id[0]}, {id[1]}");
            }
            foreach (KeyValuePair<string, List<int>> entry in dictEnergyDistrict)
            {
                string name = entry.Key;      // Ключ - имя
                List<int> id = entry.Value;        // Значение - возраст
            
                Console.WriteLine($"{name}, {id[0]}, {id[1]}");
            }
        }
    }
}
