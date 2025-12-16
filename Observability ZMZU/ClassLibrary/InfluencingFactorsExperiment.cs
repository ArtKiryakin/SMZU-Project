using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ASTRALib;
using InteractionWithTheDatabase;
using InteractionWithTheDatabaseAndFileStorage;
namespace ClassLibrary
{
    public class InfluencingFactorsExperiment
    {
        private static Dictionary<string, List<string>> typeFactorAndNameTebelColumnRastr = new Dictionary<string, List<string>> { { "Напряжение", new List<string> {"node", "vras"} }, { "P нагрузки", new List<string> {"node", "pn"} }, { "Q нагрузки", new List<string> {"node", "qn"} },
                                                                                { "P генерации", new List<string> {"node", "pg"} } , { "Q генерации", new List<string> {"node", "qg"} }, { "P перетока (нач.)", new List<string> {"vetv", "pl_ip"} },
                                                                                { "Q перетока (нач.)", new List<string> { "vetv", "ql_ip"} }, { "P перетока (кон.)", new List<string> { "vetv", "pl_iq"} }, { "Q перетока (кон.)", new List<string> { "vetv", "ql_iq"} }};

        public static void AssessmentInfluencingFactor(double correlationCoefficient, int numberExperiment, string typeFactor, 
                                                       string filePathSlices, DateTime startDateTime, DateTime endDateTime, 
                                                       int numberStart = 1, int thinning = 1, int numberEnd = 0, int numberParralel = 0)
        {
            Dictionary<int, string> truncatedDict = DatabaseConection.GetTruncatedList(correlationCoefficient, numberExperiment);
            List<int> truncatedList = new List<int> { };
            foreach(var ti in truncatedDict)
            {
                truncatedList.Add(Convert.ToInt32(ti.Value));
            }
            int countTi = truncatedDict.Count;
            List<string> filePathesRocDebug = FileStorageConnection.GetRastrFiles(filePathSlices, startDateTime, endDateTime, thinning);
            List<string> filePathesMdpDebug = FileStorageConnection.GetRastrFiles(filePathSlices, startDateTime, endDateTime, thinning, true);
            Dictionary<string, double> Kc = OS.CalcularteKc(OS.GoOS(truncatedList, filePathesRocDebug));
            int idComlexityCoeff = DatabaseConection.GetMaxIdInTabel("calculation_complexity_coefficient", "id_complexity_coefficient") + 1;
            int idTM = DatabaseConection.GetMaxIdInTabel("calculation_complexity_coefficient", "id_tm_list") + 1;
            Dictionary<string, int> idSlice = DatabaseConection.GetSliceAndId();
            DatabaseConection.WriteDataWithTmList(idTM, truncatedDict);
            Dictionary<int, Dictionary<double, List<int>>> complexityCoefficient = new Dictionary<int, Dictionary<double, List<int>>> { };
            Dictionary<string, int> sliceAndIdComplexity = new Dictionary<string, int> { };
            foreach(var i in Kc)
            {
                complexityCoefficient[idComlexityCoeff] = new Dictionary<double, List<int>> { { i.Value, new List<int> { idSlice[i.Key], countTi, idTM } } };
                sliceAndIdComplexity[i.Key] = idComlexityCoeff;
                idComlexityCoeff++;
            }
            DatabaseConection.WriteDataWithCalculationComplexityCoefficient(complexityCoefficient);
            int experiment = DatabaseConection.CreateNewExperiment(4);
            Dictionary<string, double> valuesFactor = GetValue(filePathesMdpDebug, typeFactor, numberStart, numberEnd, numberParralel);
            Dictionary<string, int> typesFactors = DatabaseConection.GetTypeInfluencingFactorAndId();
            Dictionary<int, Dictionary<double, List<int>>> influencingFactor = new Dictionary<int, Dictionary<double, List<int>>> { };
            int idInfluencingFactor = DatabaseConection.GetMaxIdInTabel("influencing_factor", "id_influencing_factor") + 1;
            List<int> idFactors = new List<int> { };
            foreach (var value in valuesFactor)
            {
                influencingFactor[idInfluencingFactor] = new Dictionary<double, List<int>> { { value.Value, new List<int> { typesFactors[typeFactor], numberStart, numberEnd, numberParralel, experiment, sliceAndIdComplexity[value.Key] } } };
                idFactors.Add(idInfluencingFactor);
                idInfluencingFactor++;
            }
            DatabaseConection.WriteDataWithInfluencingFactor(influencingFactor);

            double complexityCoefficientAverage = Kc.Values.Average();
            double influencingFactorAverage = valuesFactor.Values.Average();
            double numenator = 0;
            double denominator1 = 0;
            double denominator2 = 0;
            foreach (var slice in Kc)
            {
                numenator += (slice.Value - complexityCoefficientAverage) * (valuesFactor[slice.Key] - influencingFactorAverage);
                denominator1 += Math.Pow(slice.Value - complexityCoefficientAverage, 2);
                denominator2 += Math.Pow(valuesFactor[slice.Key] - influencingFactorAverage, 2);
            }
            double rDenom = Math.Sqrt(denominator1 * denominator2);
            double r = numenator / rDenom;
            double b = numenator / denominator2;
            double a = complexityCoefficientAverage - b * influencingFactorAverage;
            double pow = 0;
            foreach(var slice in Kc)
            {
                double trend = a + b * valuesFactor[slice.Key];
                pow += Math.Pow(slice.Value - trend, 2);
            }
            double standartDeviation = Math.Sqrt(pow / Convert.ToDouble(Kc.Count));
            int idParams = DatabaseConection.GetMaxIdInTabel("calculation_of_parameters_for_influencing_factors", "id_parameters") + 1;
            DatabaseConection.WriteDataWithInfluencingFactorAndCalculation(idParams, idFactors);
            DatabaseConection.WriteDataWithInfluencingFactorParameters(idParams, r, a, b, standartDeviation);
        }

        private static Dictionary<string, double> GetValue(List<string> filePathes, string typeFactor, int numberStart = 1, int numberEnd = 1, int numberParralel = 1)
        {
            string nameTabel = typeFactorAndNameTebelColumnRastr[typeFactor][0];
            string nameColumn = typeFactorAndNameTebelColumnRastr[typeFactor][1];
            Dictionary<string, double> value = new Dictionary<string, double> { };

            foreach (string path in filePathes)
            {
                DirectoryInfo directory = new DirectoryInfo(path);
                IRastr rastr = new Rastr();
                rastr.Load(RG_KOD.RG_REPL, path, "");
                ITable tabel = (ITable)rastr.Tables.Item(nameTabel);
                ICol tiVal = (ICol)tabel.Cols.Item(nameColumn);
                int schet = tabel.Count;
                if (nameTabel == "node")
                {
                    ICol tiNode = (ICol)tabel.Cols.Item("ny");
                    for (int index = 0; index < schet; index++)
                    {
                        if(Convert.ToInt32(tiNode.get_ZN(index)) == numberStart)
                        {
                            value[$"{directory.Parent?.Parent?.Name} {directory.Parent?.Name}"] = Convert.ToDouble(tiVal.get_ZN(index));
                        }
                    }
                }
                if (nameTabel == "vetv")
                {
                    ICol numStart = (ICol)tabel.Cols.Item("ip");
                    ICol numEnd = (ICol)tabel.Cols.Item("iq");
                    ICol numParr = (ICol)tabel.Cols.Item("np");
                    for (int index = 0; index < schet; index++)
                    {
                        if (Convert.ToInt32(numStart.get_ZN(index)) == numberStart && Convert.ToInt32(numEnd.get_ZN(index)) == numberEnd && Convert.ToInt32(numParr.get_ZN(index)) == numberParralel)
                        {
                            value[$"{directory.Parent?.Parent?.Name} {directory.Parent?.Name}"] = Convert.ToDouble(tiVal.get_ZN(index));
                        }
                    }
                }
            }
            return value;
        }
    }
}
