using ASTRALib;
using System.Text;

namespace ClassLibrary
{
    public class CalculationObservability
    {
        public static List<PowerSystem> ReadNodeFile(string fileRath)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            List<PowerSystem> data = new List<PowerSystem> { };

            // Чтение в список массивов
            var csvData = File.ReadAllLines(fileRath, Encoding.GetEncoding(1251))
                             .Select(line => line.Split(';'))
                             .ToList();
            // Доступ к данным
            foreach (var row in csvData)
            {
                data.Add(new PowerSystem(row[0], Convert.ToInt32(row[1]), row[3], row[5]));
            }

            return data;
        }

        public static Dictionary<string, int> CalculationQuantityNodes(List<PowerSystem> list, bool unifiedEnergySystem = false)
        {
            Dictionary<string, int> nodesDictionary = new Dictionary<string, int> { };
            foreach (PowerSystem powerSystem in list)
            {
                string energuSystem;
                if (unifiedEnergySystem)
                {
                    energuSystem = powerSystem.UnifiedEnergySystem;
                }
                else
                {
                    energuSystem = powerSystem.EnergySystem;
                }
                if (nodesDictionary.ContainsKey(energuSystem))
                {
                    nodesDictionary[energuSystem] += 1;
                }
                else
                {
                    nodesDictionary[energuSystem] = 1;
                }
            }
            return nodesDictionary;
        }

        public static Dictionary<string, int> CalculationQuantityBranches(List<PowerSystem> list, string fileRastrPath, bool unifiedEnergySystem = false)
        {
            Dictionary<string, int> branchesDictionary = new Dictionary<string, int> { };
            IRastr rastr = new Rastr();
            rastr.Load(RG_KOD.RG_REPL, fileRastrPath, "");
            ITable tabelBranch = (ITable)rastr.Tables.Item("vetv");
            ICol branch = (ICol)tabelBranch.Cols.Item("ip");
            int schet = tabelBranch.Size;
            for (int index = 0; index < schet; index++)
            {
                foreach (PowerSystem powerSystem in list)
                {
                    string energuSystem;
                    if (unifiedEnergySystem)
                    {
                        energuSystem = powerSystem.UnifiedEnergySystem;
                    }
                    else
                    {
                        energuSystem = powerSystem.EnergySystem;
                    }
                    if (branchesDictionary.ContainsKey(energuSystem) && powerSystem.Node == Convert.ToInt32(branch.get_ZN(index)))
                    {
                        branchesDictionary[energuSystem] += 1;
                    }
                    else
                    {
                        if (powerSystem.Node == Convert.ToInt32(branch.get_ZN(index)))
                        {
                            branchesDictionary[energuSystem] = 1;
                        }
                    }
                }
            }
            return branchesDictionary;
        }
    }
}
