using ASTRALib;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using InteractionWithTheDatabase;
using System.Reflection;
using InteractionWithTheDatabaseAndFileStorage;
using System.Collections.Generic;
using NCalc;
namespace ClassLibrary
{
    public class CalculationObservability
    {
        public static Dictionary<string, int> CalculationQuantityNodes(int typeSystem = 1)
        {
            List<PowerSystem> list = DatabaseConection.ReadDataOfPowerSystems();
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

        public static Dictionary<string, int> CalculationQuantityBranches(string fileRastrPath, int typeSystem = 1)
        {
            List<PowerSystem> list = DatabaseConection.ReadDataOfPowerSystems();
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
        public static Dictionary<string, int> CalculationQuantityTI(string fileRastrPath, int typeSystem = 1)
        {
            List<PowerSystem> list = DatabaseConection.ReadDataOfPowerSystems();
            Dictionary<string, int> tiDictionary = new Dictionary<string, int> { };
            IRastr rastr = new Rastr();
            rastr.Load(RG_KOD.RG_REPL, fileRastrPath, "");
            ITable tabelTI = (ITable)rastr.Tables.Item("ti");
            ICol nodeNumber = (ICol)tabelTI.Cols.Item("id1");
            int schet = tabelTI.Size;
            List<int> listTI = new List<int> { };
            for (int i = 0; i < schet; i++)
            {
                listTI.Add(Convert.ToInt32(nodeNumber.get_ZN(i)));
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
                if (tiDictionary.ContainsKey(energuSystem))
                {
                    tiDictionary[energuSystem] += listTI.Count(ti => ti == powerSystem.Node);
                }
                else
                {
                    tiDictionary[energuSystem] = listTI.Count(ti => ti == powerSystem.Node);
                }
            }
            return tiDictionary;
        }

        public static Dictionary<string, double> CalculateObservability(string filePathSlices, DateTime startDateTime, DateTime endDateTime, int typeSystem = 1)
        {
            Dictionary<string, double> dictObservability = new Dictionary<string, double> { };
            Dictionary<string, int> dictNodes = CalculationQuantityNodes(typeSystem);
            List<string> fileRastr = FileStorageConnection.GetRastrFiles(filePathSlices, startDateTime, endDateTime);
            foreach(string filePath in fileRastr)
            {
                Dictionary<string, int> dictBranches = CalculationQuantityBranches(filePath, typeSystem);
                Dictionary<string, int> dictTI = CalculationQuantityTI(filePath, typeSystem);
                foreach (KeyValuePair<string, int> system in dictNodes)
                {
                    if (dictObservability.ContainsKey(system.Key))
                    {
                        dictObservability[system.Key] += Convert.ToDouble(dictTI[system.Key]) / Convert.ToDouble(dictBranches[system.Key] + dictNodes[system.Key]);
                    }
                    else
                    {
                        dictObservability[system.Key] = Convert.ToDouble(dictTI[system.Key]) / Convert.ToDouble(dictBranches[system.Key] + dictNodes[system.Key]);
                    }
                }
            }
            foreach (KeyValuePair<string, int> system in dictNodes)
            {
                dictObservability[system.Key] /= Convert.ToDouble(fileRastr.Count);
            }
            return dictObservability;
        }

        public static Dictionary<string, double> CalculaneReability(string filePathSlices, DateTime startDateTime, DateTime endDateTime, int typeSystem = 1, string districtName = "")
        {
            Dictionary<string, double> dictReability = new Dictionary<string, double> { };
            Dictionary<string, List<int>> dictDistrict = DatabaseConection.ReadDataOfEnergyDistrictAndTM(typeSystem);
            Dictionary<string, List<double>> dictDistrictOS = new Dictionary<string, List<double>> { };
            List<string> fileRastr = FileStorageConnection.GetRastrFiles(filePathSlices, startDateTime, endDateTime);
            if (districtName == "")
            {
                foreach (var district in dictDistrict)
                {

                    Dictionary<string, double> osRes = OS.CalcularteProbability(OS.GoOS(district.Value, fileRastr));
                    foreach (var ti in osRes)
                    {
                        if (dictDistrictOS.ContainsKey(district.Key))
                        {
                            dictDistrictOS[district.Key].Add(ti.Value);
                        }
                        else
                        {
                            dictDistrictOS[district.Key] = new List<double> { ti.Value };
                        }
                    }
                }
            }
            else
            {
                Dictionary<string, double> osRes = OS.CalcularteProbability(OS.GoOS(dictDistrict[districtName], fileRastr));
                foreach (var ti in osRes)
                {
                    if (dictDistrictOS.ContainsKey(districtName))
                    {
                        dictDistrictOS[districtName].Add(ti.Value);
                    }
                    else
                    {
                        dictDistrictOS[districtName] = new List<double> { ti.Value };
                    }
                }
            }
            foreach (var district in dictDistrictOS)
            {
                List<double> uniqueNumbers = district.Value.Distinct().ToList();
                double summ = 0;
                foreach (int number in uniqueNumbers)
                {
                    int count = district.Value.Count(x => x == number);
                    summ += Math.Pow(number, 10) * Convert.ToDouble(count);
                }
                dictReability[district.Key] = summ / Math.Pow(10, 13);
            }
            return dictReability;
        }
    }
}
