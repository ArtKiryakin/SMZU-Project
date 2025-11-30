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
        public static Dictionary<string, Dictionary<List<string>, int>> GoOS(List<int> listTelemetry, List<string> listSlices)
        {
            Dictionary<string, Dictionary<List<string>, int>> resultOS = new Dictionary<string, Dictionary<List<string>, int>> { };
            foreach(string slice in listSlices)
            {
                DirectoryInfo directory = new DirectoryInfo(slice);
                string targetFolder = directory.Parent.Name;
                IRastr rastr = new Rastr();
                rastr.Load(RG_KOD.RG_REPL, slice, "");
                ITable tabelTI = (ITable)rastr.Tables.Item("ti");
                ICol tiNumber = (ICol)tabelTI.Cols.Item("Num");
                ICol name = (ICol)tabelTI.Cols.Item("name");
                ICol status = (ICol)tabelTI.Cols.Item("sta");
                int schet = tabelTI.Size;
                List<int> listTINumbers = new List<int> { };
                List<string> listTINames = new List<string> { };
                for (int i = 0; i < schet; i++)
                {
                    listTINumbers.Add(Convert.ToInt32(tiNumber.get_ZN(i)));
                    listTINames.Add(Convert.ToString(name.get_ZN(i)));
                }
                foreach(int telemetry in listTelemetry)
                {
                    if (listTINumbers.Contains(telemetry))
                    {
                        int index = listTINumbers.IndexOf(telemetry);
                        status.set_ZN(index, true);
                        var os = rastr.opf("s");
                        if (resultOS.ContainsKey(targetFolder))
                        {
                            resultOS[targetFolder][new List<string> { Convert.ToString(name.get_ZN(index)), Convert.ToString(tiNumber.get_ZN(index)) }] = Convert.ToInt32(os);
                        }
                        else
                        {
                            resultOS[targetFolder] = new Dictionary<List<string>, int> { };
                            resultOS[targetFolder][new List<string> { Convert.ToString(name.get_ZN(index)), Convert.ToString(tiNumber.get_ZN(index)) }] = Convert.ToInt32(os);
                        }
                    }
                }
            }
            return resultOS;
        }
    }
}
