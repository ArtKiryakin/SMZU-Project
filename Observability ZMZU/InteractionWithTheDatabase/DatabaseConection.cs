using Npgsql;
using System.Data;
using System.Text;
using System.Xml.Linq;
using ASTRALib;
using InteractionWithTheDatabaseAndFileStorage;
using System;
using System.Collections.Generic;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Numerics;
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
                    foreach (KeyValuePair<string, int> entry in dictUnifiedEnergySystem)
                    {
                        if (powerSystem.UnifiedEnergySystem == entry.Key)
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
                foreach (KeyValuePair<int, List<int>> entry in dictNode)
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

        public static void WriteDataOfTypeTelevisionMeasurement()
        {
            List<string> listPathes = FileStorageConnection.GetRastrFiles("D:\\Учеба\\Срезы", new DateTime(2023, 1, 11, 0, 0, 23), new DateTime(2023, 1, 11, 0, 0, 34));
            IRastr rastr = new Rastr();
            rastr.Load(RG_KOD.RG_REPL, listPathes[0], "");
            ITable tabelTI = (ITable)rastr.Tables.Item("type");
            ICol typeTI = (ICol)tabelTI.Cols.Item("prv_num");
            int schet = tabelTI.Size;
            Dictionary<int, string> dictTypeTI = new Dictionary<int, string> { };
            int indexTI = 0;
            for (int i = 0; i < schet; i++)
            {
                if (!dictTypeTI.ContainsValue(Convert.ToString(typeTI.get_ZN(i))))
                {
                    indexTI++;
                    dictTypeTI[indexTI] = Convert.ToString(typeTI.get_ZN(i));
                }
            }
            string connectionString = "Host=localhost;Port = 5432;Database=Observability_SMZU;Username=postgres;Password=fhntv2001";
            using var connection = new NpgsqlConnection(connectionString);
            connection.Open();
            foreach (KeyValuePair<int, string> system in dictTypeTI)
            {
                string sql = "INSERT INTO  type_television_measurement (id_type_television_measurement, type_television_measurement) VALUES (@id_type_television_measurement, @type_television_measurement)";
                using var command = new NpgsqlCommand(sql, connection);
                command.Parameters.AddWithValue("@id_type_television_measurement", system.Key);      // Берем i-й элемент из массива
                command.Parameters.AddWithValue("@type_television_measurement", system.Value);    // Берем i-й элемент из массива
                int rowsAffected = command.ExecuteNonQuery();
            }
            Console.WriteLine("Таблица типов ТИ заполнена");
        }

        private static DataTable ReadTable(NpgsqlConnection connection, string query)
        {
            DataTable table = new DataTable();

            using (var cmd = new NpgsqlCommand(query, connection))
            using (var adapter = new NpgsqlDataAdapter(cmd))
            {
                adapter.Fill(table);
            }

            return table;
        }
        public static void WriteDataOfTelevisionMeasurements()
        {
            List<string> listPathes = FileStorageConnection.GetRastrFiles("D:\\Учеба\\Срезы", new DateTime(2023, 1, 11, 0, 0, 23), new DateTime(2023, 1, 11, 23, 59, 59));
            Dictionary<int, List<int>> dictTI = new Dictionary<int, List<int>> { };
            int indexTI = 0;
            Dictionary<int, int> dictNodes = new Dictionary<int, int> { };
            string connectionString = "Host=localhost;Port = 5432;Database=Observability_SMZU;Username=postgres;Password=fhntv2001";
            Dictionary<string, int> dictType = new Dictionary<string, int> { };
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var table1Data = ReadTable(connection, "SELECT * FROM get_info_of_energy_district_and_nodes");
                foreach (DataRow row in table1Data.Rows)
                {

                    int idEnergyDistrict = Convert.ToInt32(row["id_energy_district"]);
                    int node = Convert.ToInt32(row["number_node"]);
                    dictNodes[node] = idEnergyDistrict;
                }

                var table2Data = ReadTable(connection, "SELECT * FROM type_television_measurement");
                foreach (DataRow row in table2Data.Rows)
                {

                    int idTypeTelevisionMeasurement = Convert.ToInt32(row["id_type_television_measurement"]);
                    string typeTelevisionMeasurement = Convert.ToString(row["type_television_measurement"]);
                    dictType[typeTelevisionMeasurement] = idTypeTelevisionMeasurement;
                }

                foreach (string path in listPathes)
                {
                    IRastr rastr = new Rastr();
                    rastr.Load(RG_KOD.RG_REPL, path, "");
                    ITable tabelTI = (ITable)rastr.Tables.Item("type");
                    ICol numTI = (ICol)tabelTI.Cols.Item("Num");
                    ICol numberNode = (ICol)tabelTI.Cols.Item("id1");
                    ICol typeTI = (ICol)tabelTI.Cols.Item("prv_num");
                    int schet = tabelTI.Size;
                    Console.WriteLine(path);
                    for (int i = 0; i < schet; i++)
                    {
                        if (dictNodes.ContainsKey(Convert.ToInt32(numberNode.get_ZN(i))))
                        {
                            List<int> searchList = new List<int> { Convert.ToInt32(numTI.get_ZN(i)), dictNodes[Convert.ToInt32(numberNode.get_ZN(i))], dictType[Convert.ToString(typeTI.get_ZN(i))] };
                            if (!dictTI.Values.Any(list => list.SequenceEqual(searchList)))
                            {
                                indexTI++;
                                dictTI[indexTI] = new List<int> { Convert.ToInt32(numTI.get_ZN(i)), dictNodes[Convert.ToInt32(numberNode.get_ZN(i))], dictType[Convert.ToString(typeTI.get_ZN(i))] };
                            }
                        }

                    }
                }
                foreach (KeyValuePair<int, List<int>> system in dictTI)
                {
                    string sql = "INSERT INTO  television_measurements (id_television_measurement, id_type_television_measurement, number_television_measurement, id_energy_district) " +
                                "VALUES (@id_television_measurement, @id_type_television_measurement, @number_television_measurement, @id_energy_district)";
                    using var command = new NpgsqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@id_television_measurement", system.Key);
                    command.Parameters.AddWithValue("@id_type_television_measurement", system.Value[2]);
                    command.Parameters.AddWithValue("@number_television_measurement", system.Value[0]);
                    command.Parameters.AddWithValue("@id_energy_district", system.Value[1]);
                    int rowsAffected = command.ExecuteNonQuery();
                }
            }
        }
        public static void WriteDataOfSlices()
        {
            List<string> listPathes = FileStorageConnection.GetRastrFiles("D:\\Учеба\\Срезы", new DateTime(2023, 1, 11, 0, 0, 23), new DateTime(2023, 1, 11, 23, 59, 59));
            Dictionary<int, string> dictSlice = new Dictionary<int, string> { };
            int id = 1;
            foreach (string path in listPathes)
            {
                DirectoryInfo dir = new DirectoryInfo(path);
                string secondLastFolder = dir.Parent?.Name; 
                string thirdLastFolder = dir.Parent?.Parent?.Name;
                dictSlice[id] = $"{thirdLastFolder} {secondLastFolder}";
                id++;
            }
            string connectionString = "Host=localhost;Port = 5432;Database=Observability_SMZU;Username=postgres;Password=fhntv2001";
            using var connection = new NpgsqlConnection(connectionString);
            connection.Open();
            foreach (var system in dictSlice)
            {

                string sql = "INSERT INTO  slices (id_slice, name_slise) VALUES (@id_slice, @name_slise)";

                using var command = new NpgsqlCommand(sql, connection);
                command.Parameters.AddWithValue("@id_slice", system.Key);      // Берем i-й элемент из массива
                command.Parameters.AddWithValue("@name_slise", system.Value);    // Берем i-й элемент из массива
                int rowsAffected = command.ExecuteNonQuery();

            }
        }

        public static void WriteDataOfTypesExperiments()
        {
            Dictionary<int, string> dictTypesExperiments = new Dictionary<int, string>
            {
                { 1, "Расчет надежности" },
                { 2, "Создание дорасчетов" },
                { 3, "Создание нумерованного списка ТИ" },
                { 4, "Оценка влияющих факторов" }
            };
            string connectionString = "Host=localhost;Port = 5432;Database=Observability_SMZU;Username=postgres;Password=fhntv2001";
            using var connection = new NpgsqlConnection(connectionString);
            connection.Open();
            foreach (var system in dictTypesExperiments)
            {

                string sql = "INSERT INTO  type_experiments (id_type_experiment, name_experiment) VALUES (@id_type_experiment, @name_experiment)";

                using var command = new NpgsqlCommand(sql, connection);
                command.Parameters.AddWithValue("@id_type_experiment", system.Key);      // Берем i-й элемент из массива
                command.Parameters.AddWithValue("@name_experiment", system.Value);    // Берем i-й элемент из массива
                int rowsAffected = command.ExecuteNonQuery();

            }
        }

        public static Dictionary<string, List<int>> ReadDataOfEnergyDistrictAndTM(int typeSystem = 1)
        {
            Dictionary<string, List<int>> dictDistrict = new Dictionary<string, List<int>> { };
            string connectionString = "Host=localhost;Port = 5432;Database=Observability_SMZU;Username=postgres;Password=fhntv2001";
            using var connection = new NpgsqlConnection(connectionString);
            connection.Open();
            string sql = "SELECT * FROM districts_and_tm";
            using var command = new NpgsqlCommand(sql, connection);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                string energyDistrict;
                int numberTM;
                try
                {
                    switch (typeSystem)
                    {

                        case 1:
                            {
                                energyDistrict = reader.GetString("name_energy_district");
                                numberTM = reader.GetInt32("number_television_measurement");
                                break;
                            }
                        case 2:
                            {
                                energyDistrict = reader.GetString("name_energy_system");
                                numberTM = reader.GetInt32("number_television_measurement");
                                break;
                            }
                        case 3:
                            {
                                energyDistrict = reader.GetString("name_unified_energy_system");
                                numberTM = reader.GetInt32("number_television_measurement");
                                break;
                            }
                        default:
                            {
                                energyDistrict = reader.GetString("name_energy_district");
                                numberTM = reader.GetInt32("number_television_measurement");
                                break;
                            }
                    }
                    if (dictDistrict.ContainsKey(energyDistrict))
                    {
                        dictDistrict[energyDistrict].Add(numberTM);
                    }
                    else
                    {
                        dictDistrict[energyDistrict] = new List<int> { numberTM };
                    }
                }
                catch (Exception ex)
                {
                }
                
            }
            return dictDistrict;
        }
        public static int CreateNewExperiment(int typeExperiment)
        {
            int id = 0;
            int number = 0;
            List<int> ides = new List<int> { };
            Dictionary<int, List<int>> experiments = new Dictionary<int, List<int>> { };
            string connectionString = "Host=localhost;Port = 5432;Database=Observability_SMZU;Username=postgres;Password=fhntv2001";
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string sql = "SELECT * FROM experiments";
                using var command1 = new NpgsqlCommand(sql, connection);
                using (var reader = command1.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ides.Add(reader.GetInt32("id_experiment"));
                        if (experiments.ContainsKey(reader.GetInt32("id_type_experiment")))
                        {
                            experiments[reader.GetInt32("id_type_experiment")].Add(reader.GetInt32("number_experiment"));
                        }
                        else
                        {
                            experiments[reader.GetInt32("id_type_experiment")] = new List<int> { reader.GetInt32("number_experiment") };
                        }
                    }
                }
                DateTime currentDateTime = DateTime.Now;
                DateOnly currentDate = DateOnly.FromDateTime(currentDateTime);
                TimeOnly currentTime = TimeOnly.FromDateTime(currentDateTime);
                if (ides.Count == 0)
                {
                    id = 1;
                    number = 1;
                }
                else
                {
                    id = ides.Max() + 1;
                    if (!experiments.ContainsKey(typeExperiment) || experiments[typeExperiment].Count == 0)
                    {
                        number = 1;
                    }
                    else
                    {
                        number = experiments[typeExperiment].Max() + 1;
                    }
                }
                sql = "INSERT INTO experiments (id_experiment, date_experiment, time_experiment, number_experiment, id_type_experiment) " +
                                    "VALUES (@id_experiment, @date_experiment, @time_experiment, @number_experiment, @id_type_experiment)";
                using var command2 = new NpgsqlCommand(sql, connection);
                command2.Parameters.AddWithValue("@id_experiment", id);
                command2.Parameters.AddWithValue("@date_experiment", currentDate);
                command2.Parameters.AddWithValue("@time_experiment", currentTime);
                command2.Parameters.AddWithValue("@number_experiment", number);
                command2.Parameters.AddWithValue("@id_type_experiment", typeExperiment);
                int rowsAffected = command2.ExecuteNonQuery();
            }
            return id;
        }

        public static int GetMaxIdInTabel(string tabelName, string rowName)
        {
            List<int> ides = new List<int> { };
            string connectionString = "Host=localhost;Port = 5432;Database=Observability_SMZU;Username=postgres;Password=fhntv2001";
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string sql = $"SELECT * FROM {tabelName}";
                using var command1 = new NpgsqlCommand(sql, connection);
                using (var reader = command1.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ides.Add(reader.GetInt32($"{rowName}"));
                    }
                    if (ides.Count == 0)
                    {
                        return 0;
                    }
                    else
                    {
                        return ides.Max();
                    }
                }
            }
        }

        public static void WriteDataWithCalculationCorrelation(Dictionary<string, List<double>> calcDict) 
        {
            string sql = "INSERT INTO numeric_television_measurements (id_numeric_ti, id_television_measurement, serial_number, probability_of_unsuccessful_ca, correlation_coefficient, id_experiment) " +
                         "VALUES (@id_numeric_ti, @id_television_measurement, @serial_number, @probability_of_unsuccessful_ca, @correlation_coefficient, @id_experiment)";
            string connectionString = "Host=localhost;Port = 5432;Database=Observability_SMZU;Username=postgres;Password=fhntv2001";
            using var connection = new NpgsqlConnection(connectionString);
            connection.Open();
            using var command = new NpgsqlCommand(sql, connection);
            foreach (var ti in calcDict)
            {
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@id_numeric_ti", Convert.ToInt32(ti.Value[0]));
                command.Parameters.AddWithValue("@id_television_measurement", Convert.ToInt32(ti.Value[1]));
                command.Parameters.AddWithValue("@serial_number", Convert.ToInt32(ti.Value[2]));
                command.Parameters.AddWithValue("@probability_of_unsuccessful_ca", ti.Value[3]);
                command.Parameters.AddWithValue("@correlation_coefficient", ti.Value[4]);
                command.Parameters.AddWithValue("@id_experiment", Convert.ToInt32(ti.Value[5]));
                int rowsAffected = command.ExecuteNonQuery();
            }
        }
        public static void WriteDataWithTypeInfluencingFactors()
        {
            Dictionary<string, List<int>> typesDict = new Dictionary<string, List<int>> { { "Напряжение", new List<int> {1, 1} }, { "P нагрузки", new List<int> {2, 2} }, { "Q нагрузки", new List<int> {3, 3} }, 
                                                                                { "P генерации", new List<int> {4, 5} } , { "Q генерации", new List<int> {5, 7} }, { "P перетока (нач.)", new List<int> {6, 9} },
                                                                                { "Q перетока (нач.)", new List<int> {7, 10} }, { "P перетока (кон.)", new List<int> {8, 11} }, { "Q перетока (кон.)", new List<int> {9, 12} }};
            string sql = "INSERT INTO type_influencing_factor (id_type_influencing_factor, name_influencing_factor, id_type_television_measurement) " +
                         "VALUES (@id_type_influencing_factor, @name_influencing_factor, @id_type_television_measurement)";
            string connectionString = "Host=localhost;Port = 5432;Database=Observability_SMZU;Username=postgres;Password=fhntv2001";
            using var connection = new NpgsqlConnection(connectionString);
            connection.Open();
            using var command = new NpgsqlCommand(sql, connection);
            foreach (var type in typesDict)
            {
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@id_type_influencing_factor", type.Value[0]);
                command.Parameters.AddWithValue("@name_influencing_factor", type.Key);
                command.Parameters.AddWithValue("@id_type_television_measurement", type.Value[1]);
                int rowsAffected = command.ExecuteNonQuery();
            }
        }

        public static void WriteDataWithTmList(int idTM, Dictionary<int, string> truncatedList)
        {
            string sql = "INSERT INTO television_measurements_for_complexity_coefficient (id_tm_list, id_numeric_ti) " +
                         "VALUES (@id_tm_list, @id_numeric_ti)";
            string connectionString = "Host=localhost;Port = 5432;Database=Observability_SMZU;Username=postgres;Password=fhntv2001";
            using var connection = new NpgsqlConnection(connectionString);
            connection.Open();
            using var command = new NpgsqlCommand(sql, connection);
            foreach (var id in truncatedList)
            {
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@id_tm_list", idTM);
                command.Parameters.AddWithValue("@id_numeric_ti", id.Key);
                int rowsAffected = command.ExecuteNonQuery();
            }
        }

        public static void WriteDataWithCalculationComplexityCoefficient(Dictionary<int, Dictionary<double, List<int>>> complexityCoefficient, bool repairDiagram = false)
        {
            string sql = "INSERT INTO calculation_complexity_coefficient (id_complexity_coefficient, id_repair_diagram, id_slice, value, id_tm_list, count_ti) " +
                         "VALUES (@id_complexity_coefficient, @id_repair_diagram, @id_slice, @value, @id_tm_list, @count_ti)";
            string connectionString = "Host=localhost;Port = 5432;Database=Observability_SMZU;Username=postgres;Password=fhntv2001";
            using var connection = new NpgsqlConnection(connectionString);
            connection.Open();
            using var command = new NpgsqlCommand(sql, connection);
            foreach (var coeff in complexityCoefficient)
            {
                foreach(var data in coeff.Value)
                {
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@id_complexity_coefficient", coeff.Key);
                    if (repairDiagram)
                    {
                        command.Parameters.AddWithValue("@id_repair_diagram", data.Value[3]);
                    }
                    else
                    {
                        command.Parameters.AddWithValue("@id_repair_diagram", DBNull.Value);
                    }
                    command.Parameters.AddWithValue("@id_slice", data.Value[0]);
                    command.Parameters.AddWithValue("@value", data.Key);
                    command.Parameters.AddWithValue("@id_tm_list", data.Value[2]);
                    command.Parameters.AddWithValue("@count_ti", data.Value[1]);
                    int rowsAffected = command.ExecuteNonQuery();
                }
            }
        }

        public static void WriteDataWithInfluencingFactor(Dictionary<int, Dictionary<double, List<int>>> influencingFactor)
        {
            string sql = "INSERT INTO influencing_factor (id_influencing_factor, id_type_influencing_factor, value, number_start, number_end, number_parallelism, id_experiment, id_complexity_coeffocient) " +
                         "VALUES (@id_influencing_factor, @id_type_influencing_factor, @value, @number_start, @number_end, @number_parallelism, @id_experiment, @id_complexity_coeffocient)";
            string connectionString = "Host=localhost;Port = 5432;Database=Observability_SMZU;Username=postgres;Password=fhntv2001";
            using var connection = new NpgsqlConnection(connectionString);
            connection.Open();
            using var command = new NpgsqlCommand(sql, connection);
            foreach (var factor in influencingFactor)
            {
                foreach (var data in factor.Value)
                {
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@id_influencing_factor", factor.Key);
                    if (data.Value[2] != 0)
                    {
                        command.Parameters.AddWithValue("@number_end", data.Value[2]);
                        command.Parameters.AddWithValue("@number_parallelism", data.Value[3]);
                    }
                    else
                    {
                        command.Parameters.AddWithValue("@number_end", DBNull.Value);
                        command.Parameters.AddWithValue("@number_parallelism", DBNull.Value);
                    }
                    command.Parameters.AddWithValue("@id_type_influencing_factor", data.Value[0]);
                    command.Parameters.AddWithValue("@value", data.Key);
                    command.Parameters.AddWithValue("@number_start", data.Value[1]);
                    command.Parameters.AddWithValue("@id_experiment", data.Value[4]);
                    command.Parameters.AddWithValue("@id_complexity_coeffocient", data.Value[5]);
                    int rowsAffected = command.ExecuteNonQuery();
                }
            }
        }

        public static void WriteDataWithInfluencingFactorAndCalculation(int idParams, List<int> listIdFactors)
        {
            string sql = "INSERT INTO influencing_factors_for_calculation (id_influencing_factor, id_parameters) " +
                         "VALUES (@id_influencing_factor, @id_parameters)";
            string connectionString = "Host=localhost;Port = 5432;Database=Observability_SMZU;Username=postgres;Password=fhntv2001";
            using var connection = new NpgsqlConnection(connectionString);
            connection.Open();
            using var command = new NpgsqlCommand(sql, connection);
            foreach (var factor in listIdFactors)
            {

                command.Parameters.Clear();
                command.Parameters.AddWithValue("@id_influencing_factor", factor);
                command.Parameters.AddWithValue("@id_parameters", idParams);
                int rowsAffected = command.ExecuteNonQuery();
                
            }
        }
        public static void WriteDataWithInfluencingFactorParameters(int idParams, double corrCoeff, double a, double b, double standDev)
        {
            string sql = "INSERT INTO  calculation_of_parameters_for_influencing_factors (id_parameters, correlation_coefficient, standard_deviation, a_for_regression_equation, b_for_regression_equation) " +
                         "VALUES (@id_parameters, @correlation_coefficient, @standard_deviation, @a_for_regression_equation, @b_for_regression_equation)";
            string connectionString = "Host=localhost;Port = 5432;Database=Observability_SMZU;Username=postgres;Password=fhntv2001";
            using var connection = new NpgsqlConnection(connectionString);
            connection.Open();
            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@id_parameters", idParams);
            command.Parameters.AddWithValue("@correlation_coefficient", corrCoeff);
            command.Parameters.AddWithValue("@standard_deviation", standDev);
            command.Parameters.AddWithValue("@a_for_regression_equation", a);
            command.Parameters.AddWithValue("@b_for_regression_equation", b);
            int rowsAffected = command.ExecuteNonQuery();
        }

        public static void WriteDataWithChangingState(int idObject, string nameObject, int numberStart, int numberEnd, int numberParr, bool state)
        {
            string sql = "INSERT INTO  changing_state_object (id_object, name_object, number_start, number_end, number_parallelism, state) " +
                         "VALUES (@id_object, @name_object, @number_start, @number_end, @number_parallelism, @state)";
            string connectionString = "Host=localhost;Port = 5432;Database=Observability_SMZU;Username=postgres;Password=fhntv2001";
            using var connection = new NpgsqlConnection(connectionString);
            connection.Open();
            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@id_object", idObject);
            command.Parameters.AddWithValue("@name_object", nameObject);
            command.Parameters.AddWithValue("@number_start", numberStart);
            command.Parameters.AddWithValue("@number_end", numberEnd);
            command.Parameters.AddWithValue("@number_parallelism", numberParr);
            command.Parameters.AddWithValue("@state", state);
            int rowsAffected = command.ExecuteNonQuery();
        }

        public static Dictionary<string, int> GetTiAndId()
        {
            Dictionary<string, int> result = new Dictionary<string, int> { };
            string connectionString = "Host=localhost;Port = 5432;Database=Observability_SMZU;Username=postgres;Password=fhntv2001";
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string sql = "SELECT * FROM television_measurements";
                using var command1 = new NpgsqlCommand(sql, connection);
                using (var reader = command1.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result[Convert.ToString(reader.GetInt32("number_television_measurement"))] = reader.GetInt32("id_television_measurement");
                    }
                }
            }
                return result;
        }

        public static Dictionary<string, int> GetSliceAndId()
        {
            Dictionary<string, int> result = new Dictionary<string, int> { };
            string connectionString = "Host=localhost;Port = 5432;Database=Observability_SMZU;Username=postgres;Password=fhntv2001";
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string sql = "SELECT * FROM slices";
                using var command1 = new NpgsqlCommand(sql, connection);
                using (var reader = command1.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result[reader.GetString("name_slise")] = reader.GetInt32("id_slice");
                    }
                }
            }
            return result;
        }

        public static Dictionary<string, int> GetTypeInfluencingFactorAndId()
        {
            Dictionary<string, int> result = new Dictionary<string, int> { };
            string connectionString = "Host=localhost;Port = 5432;Database=Observability_SMZU;Username=postgres;Password=fhntv2001";
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string sql = "SELECT * FROM type_influencing_factor";
                using var command1 = new NpgsqlCommand(sql, connection);
                using (var reader = command1.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result[reader.GetString("name_influencing_factor")] = reader.GetInt32("id_type_influencing_factor");
                    }
                }
            }
            return result;
        }
        public static int GetIdExperimentWithNumber(int idTypeExp, int numberExperiment)
        {
            int idExperiment = 0;
            string connectionString = "Host=localhost;Port = 5432;Database=Observability_SMZU;Username=postgres;Password=fhntv2001";
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string sql = "SELECT * FROM experiments";
                using var command1 = new NpgsqlCommand(sql, connection);
                using (var reader = command1.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader.GetInt32("id_type_experiment") == idTypeExp && reader.GetInt32("number_experiment") == numberExperiment)
                        {
                            idExperiment = reader.GetInt32("id_experiment");
                        }

                    }
                }
            }
            return idExperiment;
        }
        public static Dictionary<int, string> GetTruncatedList(double correlationCoefficient, int numberExperiment)
        {
            Dictionary<int, string> truncatedList = new Dictionary<int, string> { };
            Dictionary<string, int> tiDict = GetTiAndId();
            int idExperiment = GetIdExperimentWithNumber(3, numberExperiment);
            string connectionString = "Host=localhost;Port = 5432;Database=Observability_SMZU;Username=postgres;Password=fhntv2001";
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string sql = "SELECT * FROM numeric_television_measurements";
                using var command1 = new NpgsqlCommand(sql, connection);
                using (var reader = command1.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if(reader.GetInt32("id_experiment") == idExperiment) 
                        { 
                            if(reader.GetDouble("correlation_coefficient") < correlationCoefficient)
                            {
                                foreach (var ti in tiDict)
                                {
                                    if(ti.Value == reader.GetInt32("id_television_measurement"))
                                    {
                                        truncatedList[reader.GetInt32("id_numeric_ti")] = ti.Key;
                                    }
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }
            return truncatedList;
        }
    }
}
