using Npgsql;
using System.Data;
using System.Text;
using System.Xml.Linq;
using ASTRALib;
using InteractionWithTheDatabaseAndFileStorage;
namespace InteractionWithTheDatabase
{
    public class DatabaseConection
    {
        public static void WriteDataOfPowerSystems()
        {
            string connectionString = "Host=localhost;Port = 5432;Database=Observability_SMZU;Username=postgres;Password=fhntv2001";
            List<PowerSystem> powerSystemsList = FileStorageConnection.ReadNodeFile("D:\\Учеба\\Мой сем.4\\Нирс\\Территории\\all_csv.csv");
            int idUnifiedEnergySystem = 1;
            int idEnergySystem = 1;
            int idEnergyDistrict = 1;
            int idNode = 1;
            Dictionary<string, int> dictUnifiedEnergySystem = new Dictionary<string, int> { };
            Dictionary<string, List<int>> dictEnergySystem = new Dictionary<string, List<int>> { };
            Dictionary<string, List<int>> dictEnergyDistrict = new Dictionary<string, List<int>> { };
            Dictionary<int, List<int>> dictNode = new Dictionary<int, List<int>> { };
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
            foreach (PowerSystem powerSystem in powerSystemsList)
            {
                if (!dictNode.ContainsKey(powerSystem.Node))
                {
                    foreach (KeyValuePair<string, List<int>> entry in dictEnergyDistrict)
                    {
                        if (powerSystem.EnergyDistrict == entry.Key)
                        {
                            List<int> i = entry.Value;
                            dictNode[powerSystem.Node] = [idNode, i[0]];
                            idNode++;
                        }
                    }
                }
            }
            List<List<int>> listBranches = FileStorageConnection.GetBranches("D:\\Учеба\\Срезы\\2023_01_11\\00_04_35\\roc_debug_before_OC");
            Dictionary<int, List<int>> listBranchesId = new Dictionary<int, List<int>> { };
            int idBranch = 1;
            foreach (List<int> branch in listBranches)
            {
                List<int> listBrach = new List<int> { };
                foreach(KeyValuePair<int, List<int>> entry in dictNode)
                {
                    if (branch[0] == entry.Key)
                    {
                        List<int> i = entry.Value;
                        listBrach.Add(i[0]);
                    }
                }
                foreach (KeyValuePair<int, List<int>> entry in dictNode)
                {
                    if (branch[1] == entry.Key)
                    {
                        List<int> i = entry.Value;
                        listBrach.Add(i[0]);
                    }
                }
                listBranchesId[idBranch] = listBrach;
                idBranch++;
            }
            using var connection = new NpgsqlConnection(connectionString);
            connection.Open();
            foreach (KeyValuePair<string, int> system in dictUnifiedEnergySystem)
            {

                string sql = "INSERT INTO  unified_energy_systems (id_unified_energy_system, name_unified_energy_system) VALUES (@id_unified_energy_system, @name_unified_energy_system)";

                using var command = new NpgsqlCommand(sql, connection);
                command.Parameters.AddWithValue("@id_unified_energy_system", system.Value);      // Берем i-й элемент из массива
                command.Parameters.AddWithValue("@name_unified_energy_system", system.Key);    // Берем i-й элемент из массива
                int rowsAffected = command.ExecuteNonQuery();

            }
            Console.WriteLine("Добавлены ОЭС");
            foreach (KeyValuePair<string, List<int>> system in dictEnergySystem)
            {
                List<int> id = system.Value;
                string sql = "INSERT INTO  energy_systems (id_energy_system, name_energy_system, id_unified_energy_system) VALUES (@id_energy_system, @name_energy_system, @id_unified_energy_system)";
                using var command = new NpgsqlCommand(sql, connection);
                command.Parameters.AddWithValue("@id_energy_system", id[0]);      // Берем i-й элемент из массива
                command.Parameters.AddWithValue("@name_energy_system", system.Key);
                command.Parameters.AddWithValue("@id_unified_energy_system", id[1]); // Берем i-й элемент из массива
                int rowsAffected = command.ExecuteNonQuery();
            }
            Console.WriteLine("Добавлены ЭС");
            foreach (KeyValuePair<string, List<int>> system in dictEnergyDistrict)
            {
                List<int> id = system.Value;
                string sql = "INSERT INTO  energy_districts (id_energy_district, name_energy_district, id_energy_system) VALUES (@id_energy_district, @name_energy_district, @id_energy_system)";
                using var command = new NpgsqlCommand(sql, connection);
                command.Parameters.AddWithValue("@id_energy_district", id[0]);      // Берем i-й элемент из массива
                command.Parameters.AddWithValue("@name_energy_district", system.Key);
                command.Parameters.AddWithValue("@id_energy_system", id[1]); // Берем i-й элемент из массива
                int rowsAffected = command.ExecuteNonQuery();
            }
            Console.WriteLine("Добавлены Энергорайоны");
            foreach (KeyValuePair<int, List<int>> system in dictNode)
            {
                List<int> id = system.Value;
                string sql = "INSERT INTO  nodes (id_node, number_node, id_energy_district) VALUES (@id_node, @number_node, @id_energy_district)";
                using var command = new NpgsqlCommand(sql, connection);
                command.Parameters.AddWithValue("@id_node", id[0]);      // Берем i-й элемент из массива
                command.Parameters.AddWithValue("@number_node", system.Key);
                command.Parameters.AddWithValue("@id_energy_district", id[1]); // Берем i-й элемент из массива
                int rowsAffected = command.ExecuteNonQuery();
            }
            Console.WriteLine("Добавлены Узлы");
            foreach (KeyValuePair<int, List<int>> system in listBranchesId)
            {
                List<int> id = system.Value;
                string sql = "INSERT INTO  brantches (id_branch, id_start_number, id_end_number) VALUES (@id_branch, @id_start_number, @id_end_number)";
                using var command = new NpgsqlCommand(sql, connection);
                command.Parameters.AddWithValue("@id_branch", system.Key);      // Берем i-й элемент из массива
                command.Parameters.AddWithValue("@id_start_number", id[0]);
                command.Parameters.AddWithValue("@id_end_number", id[1]); // Берем i-й элемент из массива
                int rowsAffected = command.ExecuteNonQuery();
            }
            Console.WriteLine("Добавлены Ветви");
        }

        public static void GoConnect()
        {
            string connectionString = "Host=localhost;Port = 5432;Database=Observability_SMZU;Username=postgres;Password=fhntv2001";
            using var connection = new NpgsqlConnection(connectionString);
            connection.Open();

            // Запрос для получения всех таблиц
            string sql = @"
                            SELECT table_name 
                            FROM information_schema.tables 
                            WHERE table_schema = 'public' 
                            ORDER BY table_name";

            using var command = new NpgsqlCommand(sql, connection);
            using var reader = command.ExecuteReader();

            Console.WriteLine("Таблицы в базе данных:");
            while (reader.Read())
            {
                Console.WriteLine($"{reader.GetString(0)}");
            }
        }

        public static List<PowerSystem> ReadDataOfPowerSystems()
        {
            List<PowerSystem> listSystems = new List<PowerSystem> { };
            string connectionString = "Host=localhost;Port = 5432;Database=Observability_SMZU;Username=postgres;Password=fhntv2001";
            using var connection = new NpgsqlConnection(connectionString);
            connection.Open();
            string sql = "SELECT * FROM get_info_of_power_systems";
            using var command = new NpgsqlCommand(sql, connection);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                string energyDistrict = reader.GetString("name_energy_district");
                string emenergySystem = reader.GetString("name_energy_system");
                string unifiedEnergySystem = reader.GetString("name_unified_energy_system");
                int node = reader.GetInt32("number_node");
                listSystems.Add(new PowerSystem(node, energyDistrict, emenergySystem, unifiedEnergySystem));
            }
            return listSystems;
        }
    }
}
