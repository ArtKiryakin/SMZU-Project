using ASTRALib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary
{
    public class OS
    {
        public static Dictionary<string, Dictionary<string, int>> GoOS(List<int> listTelemetry, List<string> listSlices)
        {
            Dictionary<string, Dictionary<string, int>> resultOS = new Dictionary<string, Dictionary<string, int>> { };
            foreach(string slice in listSlices)
            {
                DirectoryInfo directory = new DirectoryInfo(slice);
                string targetFolder = directory.Parent.Name;
                IRastr rastr = new Rastr();
                rastr.Load(RG_KOD.RG_REPL, slice, "");
                ITable tabelTI = (ITable)rastr.Tables.Item("ti");
                ICol tiNumber = (ICol)tabelTI.Cols.Item("Num");
                ICol status = (ICol)tabelTI.Cols.Item("sta");
                int schet = tabelTI.Size;
                List<int> listTINumbers = new List<int> { };
                List<string> listTINames = new List<string> { };
                for (int i = 0; i < schet; i++)
                {
                    listTINumbers.Add(Convert.ToInt32(tiNumber.get_ZN(i)));
                }
                foreach(int telemetry in listTelemetry)
                {
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
            return resultOS;
        }

        public static Dictionary<string, double> CalcularteKn(Dictionary<string, Dictionary<string, int>> resultOS)
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
    }
}
