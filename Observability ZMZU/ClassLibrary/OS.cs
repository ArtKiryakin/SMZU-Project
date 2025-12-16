using ASTRALib;
using InteractionWithTheDatabase;
using InteractionWithTheDatabaseAndFileStorage;
using NCalc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;
using Microsoft.VisualBasic.FileIO;

namespace ClassLibrary
{
    public class OS
    {
        public static Dictionary<string, Dictionary<string, int>> GoOS(List<int> listTelemetry, List<string> listSlices, bool allTM = false)
        {
            Dictionary<string, Dictionary<string, int>> resultOS = new Dictionary<string, Dictionary<string, int>> { };
            foreach(string slice in listSlices)
            {
                DirectoryInfo directory = new DirectoryInfo(slice);
                string secondLastFolder = directory.Parent?.Name;
                string thirdLastFolder = directory.Parent?.Parent?.Name;
                string targetFolder = $"{thirdLastFolder} {secondLastFolder}";
                IRastr rastr = new Rastr();
                rastr.Load(RG_KOD.RG_REPL, slice, "");
                ITable tabelTI = (ITable)rastr.Tables.Item("ti");
                ICol tiNumber = (ICol)tabelTI.Cols.Item("Num");
                int schet = tabelTI.Count;
                List<int> listTINumbers = new List<int> { };
                List<string> listTINames = new List<string> { };
                
                for (int i = 0; i < schet; i++)
                {
                    listTINumbers.Add(Convert.ToInt32(tiNumber.get_ZN(i)));
                }
                if (!allTM)
                {
                    foreach (int telemetry in listTelemetry)
                    {
                        rastr.NewFile(slice);
                        tabelTI = (ITable)rastr.Tables.Item("ti");
                        tiNumber = (ICol)tabelTI.Cols.Item("Num");
                        ICol status = (ICol)tabelTI.Cols.Item("sta");
                        if (listTINumbers.Contains(telemetry))
                        {
                            int index = listTINumbers.IndexOf(telemetry);
                            status.set_ZN(index, true);
                            var os = rastr.opf("s");
                            Console.WriteLine($"Срез: {targetFolder}, ТИ: {telemetry}, {os}");
                            if (resultOS.ContainsKey(targetFolder))
                            {
                                resultOS[targetFolder][Convert.ToString(tiNumber.get_ZN(index))] = Convert.ToInt32(os);
                            }
                            else
                            {
                                resultOS[targetFolder] = new Dictionary<string, int> { };
                                resultOS[targetFolder][Convert.ToString(tiNumber.get_ZN(index))] = Convert.ToInt32(os);
                            }
                        }
                        
                    }
                }
                else
                {
                    for (int index = 0; index < schet; index++)
                    {
                        rastr.NewFile(slice);
                        tabelTI = (ITable)rastr.Tables.Item("ti");
                        tiNumber = (ICol)tabelTI.Cols.Item("Num");
                        ICol status = (ICol)tabelTI.Cols.Item("sta");
                        ICol typeTI = (ICol)tabelTI.Cols.Item("prv_num");
                        string type = Convert.ToString(typeTI.get_ZN(index));
                        if (type != "сост(Узел)" && type != "сост(Ветвь)" && type != "сост(Ген.)" && type != "сост(Реактор)")
                        {
                            status.set_ZN(index, true);
                            var os = rastr.opf("s");
                            string ti = Convert.ToString(tiNumber.get_ZN(index));
                            Console.WriteLine($"Срез: {targetFolder}, ТИ: {ti}, {os}");
                            if (resultOS.ContainsKey(targetFolder))
                            {
                                resultOS[targetFolder][ti] = Convert.ToInt32(os);
                            }
                            else
                            {
                                resultOS[targetFolder] = new Dictionary<string, int> { { ti, Convert.ToInt32(os) } };
                            }
                        }
                    }
                }

            }
            return resultOS;
        }

        
        public static List<string[]> ReadCsvWithIndex(string filePath)
        {
            var rows = new List<string[]>();

            using (var parser = new TextFieldParser(filePath))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(";");
                parser.HasFieldsEnclosedInQuotes = true;

                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();
                    rows.Add(fields);
                }
            }

            return rows; // Теперь можно обращаться как rows[rowIndex][columnIndex]
        }
        public static Dictionary<string, double> CalcularteProbability(Dictionary<string, Dictionary<string, int>> resultOS)
        {
            Dictionary<string, double> dictionaryKn = new Dictionary<string, double> { };
            Dictionary<string, List<int>> dictionaryOS = new Dictionary<string, List<int>> { };
            foreach (KeyValuePair<string, Dictionary<string, int>> slice in resultOS)
            {
                foreach (KeyValuePair<string, int> ti in slice.Value)
                {
                    if (dictionaryOS.ContainsKey(ti.Key))
                    {
                        dictionaryOS[ti.Key].Add(ti.Value);
                    }
                    else
                    {
                        dictionaryOS[ti.Key] = new List<int> { ti.Value };
                    }
                }
            }
            foreach (KeyValuePair<string, List<int>> ti in dictionaryOS)
            {
                dictionaryKn[ti.Key] = (Convert.ToDouble(ti.Value.Sum()) / Convert.ToDouble(ti.Value.Count));
            }
            return dictionaryKn;
        }

        public static Dictionary<string, double> CalcularteKc(Dictionary<string, Dictionary<string, int>> resultOS)
        {
            Dictionary<string, double> dictionaryKc = new Dictionary<string, double> { };
            Dictionary<string, List<int>> dictionaryOS = new Dictionary<string, List<int>> { };
            foreach (KeyValuePair<string, Dictionary<string, int>> slice in resultOS)
            {
                foreach (KeyValuePair<string, int> ti in slice.Value)
                {
                    if (dictionaryOS.ContainsKey(slice.Key))
                    {
                        dictionaryOS[slice.Key].Add(ti.Value);
                    }
                    else
                    {
                        dictionaryOS[slice.Key] = new List<int> { ti.Value };
                    }
                }
            }

            foreach (KeyValuePair<string, List<int>> ti in dictionaryOS)
            {
                dictionaryKc[ti.Key] = (Convert.ToDouble(ti.Value.Sum()) / Convert.ToDouble(ti.Value.Count));
            }
            return dictionaryKc;
        }
        public static Dictionary<string, double> CalcularteSumSlice(Dictionary<string, Dictionary<string, int>> resultOS)
        {
            Dictionary<string, double> dictionarySum = new Dictionary<string, double> { };
            Dictionary<string, List<int>> dictionaryOS = new Dictionary<string, List<int>> { };
            foreach (KeyValuePair<string, Dictionary<string, int>> slice in resultOS)
            {
                foreach (KeyValuePair<string, int> ti in slice.Value)
                {
                    if (dictionaryOS.ContainsKey(slice.Key))
                    {
                        dictionaryOS[slice.Key].Add(ti.Value);
                    }
                    else
                    {
                        dictionaryOS[slice.Key] = new List<int> { ti.Value };
                    }
                }
            }

            foreach (KeyValuePair<string, List<int>> ti in dictionaryOS)
            {
                dictionarySum[ti.Key] = Convert.ToDouble(ti.Value.Sum());
            }
            return dictionarySum;
        }

        public static void MakingNumberedList(string filePathSlices, DateTime startDateTime, DateTime endDateTime, int thinning = 1)
        {
            
            List<string> fileRathes = FileStorageConnection.GetRastrFiles(filePathSlices, startDateTime, endDateTime, thinning);
            Dictionary<string, Dictionary<string, int>> resultOS = GoOS(new List<int> { }, fileRathes, true);
            Dictionary<string, double> probabilityes = CalcularteProbability(resultOS);
            probabilityes = probabilityes.OrderByDescending(kvp => kvp.Value).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            List<int> list = new List<int> { };
            foreach (var i in probabilityes)
            {
                list.Add(Convert.ToInt32(i.Key));
            }
            Dictionary<string, double> Kc = CalcularteSumSlice(resultOS);
            double KcAverage = Kc.Values.Average();
            Dictionary<string, double> dictR = new Dictionary<string, double> { };
            for (int i = 0; i < probabilityes.Count; i++) 
            {
                double numerator = 0;
                double denominator1 = 0;
                double denominator2 = 0;
                var firstI = list.Take(i + 1).ToList();
                Console.WriteLine(firstI.Count);
                Dictionary<string, Dictionary<string, int>> shortResultOS = new Dictionary<string, Dictionary<string, int>> { };
                foreach (int k in firstI)
                {
                    foreach(var val in resultOS)
                    {
                        if (shortResultOS.ContainsKey(val.Key))
                        {
                            if (resultOS[val.Key].ContainsKey(k.ToString()))
                            {
                                shortResultOS[val.Key][k.ToString()] = resultOS[val.Key][k.ToString()];
                            }
                        }
                        else
                        {
                            if (resultOS[val.Key].ContainsKey(k.ToString()))
                            {
                                shortResultOS[val.Key] = new Dictionary<string, int> { { k.ToString(), resultOS[val.Key][k.ToString()] } };
                            }
                        }
                    }
                }
                Dictionary<string, double> kc = CalcularteSumSlice(shortResultOS);
                double kcAverage = kc.Values.Average();
                foreach (var j in kc)
                {
                    numerator += (Kc[j.Key] - KcAverage) * (j.Value - kcAverage);
                    denominator1 += Math.Pow((Kc[j.Key] - KcAverage), 2);
                    denominator2 += Math.Pow((j.Value - kcAverage), 2);
                }
                double denominator = Math.Sqrt(denominator1 * denominator2);
                double r = numerator / denominator;
                Console.WriteLine(r);
                dictR[Convert.ToString(list[i])] = r;
            }
            int maxId = DatabaseConection.GetMaxIdInTabel("numeric_television_measurements", "id_numeric_ti") + 1;
            int experimentId = DatabaseConection.CreateNewExperiment(3);
            int serialNumber = 1;
            Dictionary<string, List<double>> resultDict = new Dictionary<string, List<double>> { };
            Dictionary<string, int> idTI = DatabaseConection.GetTiAndId();
            foreach(var telemetry in probabilityes)
            {
                try
                {
                    resultDict[telemetry.Key] = new List<double> { Convert.ToDouble(maxId), Convert.ToDouble(idTI[telemetry.Key]), Convert.ToDouble(serialNumber), telemetry.Value, dictR[telemetry.Key], Convert.ToDouble(experimentId) };
                    maxId++;
                    serialNumber++;
                }
                catch
                {
                    continue;
                }
            }
            DatabaseConection.WriteDataWithCalculationCorrelation(resultDict);
        }


    }
}
