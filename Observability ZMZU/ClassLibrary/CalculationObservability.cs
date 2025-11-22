using ASTRALib;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
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
                data.Add(new PowerSystem(row[0], Convert.ToInt32(row[1]), row[2], row[3], row[5]));
            }

            return data;
        }

        public static Dictionary<string, int> CalculationQuantityNodes(List<PowerSystem> list, int typeSystem = 1)
        {
            Dictionary<string, int> nodesDictionary = new Dictionary<string, int> { };
            foreach (PowerSystem powerSystem in list)
            {
                string energuSystem;
                switch (typeSystem)
                {
                    case 1:
                    {
                        energuSystem = powerSystem.EnergyDistrict;
                        break;
                    }
                    case 2:
                    {
                        energuSystem = powerSystem.EnergySystem;
                        break;
                    }
                    case 3:
                    {
                        energuSystem = powerSystem.UnifiedEnergySystem;
                        break;
                    }
                    default:
                    {
                        energuSystem = powerSystem.EnergyDistrict;
                        break;
                    }
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

        public static Dictionary<string, int> CalculationQuantityBranches(List<PowerSystem> list, string fileRastrPath, int typeSystem = 1)
        {
            Dictionary<string, int> branchesDictionary = new Dictionary<string, int> { };
            IRastr rastr = new Rastr();
            rastr.Load(RG_KOD.RG_REPL, fileRastrPath, "");
            ITable tabelBranch = (ITable)rastr.Tables.Item("vetv");
            ICol start_number = (ICol)tabelBranch.Cols.Item("ip");
            ICol end_number = (ICol)tabelBranch.Cols.Item("iq");
            List<List<int>> listBranchesNumbers = new List<List<int>> { };
            int schet = tabelBranch.Size;
            Dictionary<string, List<int>> dictionaryEnergySystems = new Dictionary<string, List<int>> { };

            for (int index = 0; index < schet; index++)
            {
                List<int> numbers = new List<int> { Convert.ToInt32(start_number.get_ZN(index)), Convert.ToInt32(end_number.get_ZN(index)) };
                listBranchesNumbers.Add(numbers);
            }
            foreach (PowerSystem powerSystem in list)
            {
                string energuSystem;
                switch (typeSystem)
                {
                    case 1:
                        {
                            energuSystem = powerSystem.EnergyDistrict;
                            break;
                        }
                    case 2:
                        {
                            energuSystem = powerSystem.EnergySystem;
                            break;
                        }
                    case 3:
                        {
                            energuSystem = powerSystem.UnifiedEnergySystem;
                            break;
                        }
                    default:
                        {
                            energuSystem = powerSystem.EnergyDistrict;
                            break;
                        }
                }
                if (dictionaryEnergySystems.ContainsKey(energuSystem))
                {
                    dictionaryEnergySystems[energuSystem].Add(powerSystem.Node);
                }
                else
                {
                    dictionaryEnergySystems[energuSystem] = new List<int> { powerSystem.Node };
                }
            }
            foreach (List<int> branch in listBranchesNumbers)
            {
                foreach (KeyValuePair<string, List<int>> entry in dictionaryEnergySystems)
                {
                    if (entry.Value.Contains(branch[0]) && entry.Value.Contains(branch[1]))
                    {
                        Calculation(branchesDictionary, entry.Key);
                        break;
                    }
                    else
                    {
                        if (entry.Value.Contains(branch[0]))
                        {
                            Calculation(branchesDictionary, entry.Key);
                        }
                        if (entry.Value.Contains(branch[1]))
                        {
                            Calculation(branchesDictionary, entry.Key);
                        }
                    }
                }
            }
            void Calculation(Dictionary<string, int> branchesDictionary, string name)
            {
                if (branchesDictionary.ContainsKey(name))
                {
                    branchesDictionary[name] += 1;
                }
                else
                {
                    branchesDictionary[name] = 1;
                }
            }
            return branchesDictionary;
        }
    }
}
