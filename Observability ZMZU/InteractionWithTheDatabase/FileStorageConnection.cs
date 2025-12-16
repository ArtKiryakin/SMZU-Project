using ASTRALib;
using InteractionWithTheDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractionWithTheDatabaseAndFileStorage
{
    public class FileStorageConnection
    {
        public static List<PowerSystem> ReadNodeFile(string filePath)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            List<PowerSystem> data = new List<PowerSystem> { };

            // Чтение в список массивов
            var csvData = File.ReadAllLines(filePath, Encoding.GetEncoding(1251))
                             .Select(line => line.Split(';'))
                             .ToList();
            // Доступ к данным
            foreach (var row in csvData)
            {
                data.Add(new PowerSystem(Convert.ToInt32(row[1]), row[2], row[3], row[5]));
            }

            return data;
        }
        public static List<List<int>> GetBranches(string fileRastrPath)
        {
            List<List<int>> listBranchs = new List<List<int>> { };
            IRastr rastr = new Rastr();
            rastr.Load(RG_KOD.RG_REPL, fileRastrPath, "");
            ITable tabelBranch = (ITable)rastr.Tables.Item("vetv");
            ICol start_number = (ICol)tabelBranch.Cols.Item("ip");
            ICol end_number = (ICol)tabelBranch.Cols.Item("iq");
            int schet = tabelBranch.Size;
            for (int index = 0; index < schet; index++)
            {
                List<int> numbers = new List<int> { Convert.ToInt32(start_number.get_ZN(index)), Convert.ToInt32(end_number.get_ZN(index)) };
                listBranchs.Add(numbers);
            }
            return listBranchs;
        }

        public static List<string> GetRastrFiles(string filePathSlices, DateTime startDateTime, DateTime endDateTime, int thinning = 1, bool mdpDebug = false)
        {
            string datePath = $"{startDateTime.Year}_{startDateTime:MM}_{startDateTime:dd}";
            string filePath = filePathSlices + $"\\{datePath}";
            DirectoryInfo directory = new DirectoryInfo(filePath);
            string[] foldername = directory.GetDirectories().Select(dir => dir.Name).ToArray();
            DirectoryInfo[] dirs = directory.GetDirectories();
            string timeStart = $"{startDateTime:HH}_{startDateTime:mm}_{startDateTime:ss}";
            string timeEnd = $"{endDateTime:HH}_{endDateTime:mm}_{endDateTime:ss}";
            List<string> listRastrBeforeTinning = new List<string> { };
            List<string> listRastr = new List<string> { };
            while (timeStart != timeEnd)
            {
                if (foldername.Contains(timeStart))
                {
                    int index = Array.IndexOf(foldername, timeStart);
                    FileInfo[] dO;
                    if (mdpDebug)
                    {
                        dO = dirs[index].GetFiles("mdp_debug_1*");
                    }
                    else
                    {
                        dO = dirs[index].GetFiles("roc_debug_after_OC*");
                    }
                    listRastrBeforeTinning.Add($"{dO[0]}");
                }
                startDateTime += TimeSpan.FromSeconds(1);
                timeStart = $"{startDateTime:HH}_{startDateTime:mm}_{startDateTime:ss}";
            }
            for (int i = 0; i < listRastrBeforeTinning.Count; i+= thinning)
            {
                listRastr.Add(listRastrBeforeTinning[i]);
            }
            return listRastr;
        }

        public static void MakeFileSporage(string filePath)
        {
            string mainFolder = Path.Combine(filePath, "Результаты");
            Directory.CreateDirectory(mainFolder);

            // 2. Создать две вложенные папки
            string subFolder1 = Path.Combine(mainFolder, "Файлы с ремонтными схемами");
            string subFolder2 = Path.Combine(mainFolder, "Файлы с дорасчетами");

            Directory.CreateDirectory(subFolder1);
            Directory.CreateDirectory(subFolder2);
        }
        
        public static string MakeNewExperiment(int experimentType, int numberExperiment, string filePathSave)
        {
            
            switch (experimentType)
            {
                case 1:
                {
                    string filePath = filePathSave + "\\Результаты" + "\\Файлы с ремонтными схемами";
                    string mainFolder = Path.Combine(filePath, $"Ремонтная схема {numberExperiment}");
                    Directory.CreateDirectory(mainFolder);
                    return filePath + $"\\Ремонтная схема {numberExperiment}";

                }
                case 2:
                {
                    string filePath = filePathSave + "\\Результаты" + "\\Файлы с дорасчетами";
                    string mainFolder = Path.Combine(filePath, $"Эксперимент {numberExperiment}");
                    Directory.CreateDirectory(mainFolder);
                    return filePath + $"\\Эксперимент {numberExperiment}";
                }
                default:
                {
                    string filePath = filePathSave + "\\Результаты" + "\\Файлы с ремонтными схемами";
                    string mainFolder = Path.Combine(filePath, $"Ремонтная схема {numberExperiment}");
                    Directory.CreateDirectory(mainFolder);
                    return filePath + $"\\Ремонтная схема {numberExperiment}";
                }
            }
        }

        public static string MakeNewSlice(string filePathSave, string nameSlice)
        {
            string mainFolder = Path.Combine(filePathSave, $"{nameSlice}");
            Directory.CreateDirectory(mainFolder);
            return filePathSave + $"\\{nameSlice}";
        }
    }
}
