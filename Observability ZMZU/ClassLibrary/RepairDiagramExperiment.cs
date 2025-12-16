using ASTRALib;
using InteractionWithTheDatabase;
using InteractionWithTheDatabaseAndFileStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ClassLibrary
{
    public class RepairDiagramExperiment
    {
        public static void MakeRepairDiagram(string filePath, DateTime dateStart, DateTime dateEnd, int thinning, string filePathSv, int numberStart, int numberEnd, int parrNumber, bool status, double corrCoeff, int numberExperiment)
        {
            List<string> rastrFilesRoc = FileStorageConnection.GetRastrFiles(filePath, dateStart, dateEnd, thinning);
            List<string> rastrFilesMdp = FileStorageConnection.GetRastrFiles(filePath, dateStart, dateEnd, thinning, true);
            int numberRepairDiagram = DatabaseConection.GetMaxIdInTabel("changing_state_object", "id_object") + 1;
            string filePathSave = FileStorageConnection.MakeNewExperiment(1, numberRepairDiagram, filePathSv);
            string rath = "";
            List<string> repairFiles = new List<string> { };
            //Обращение к файлу RastrWin 3 до коррекции
            for (int i = 0; i < rastrFilesRoc.Count; i++)
            {
                DirectoryInfo directory = new DirectoryInfo(rastrFilesRoc[i]);
                if (i == 0)
                {
                    string thirdLastFolder = directory.Parent?.Parent?.Name;
                    rath = FileStorageConnection.MakeNewSlice(filePathSave, thirdLastFolder);
                }
                string nameSlice = directory.Parent?.Name;
                string newPath = FileStorageConnection.MakeNewSlice(rath, nameSlice) + $"\\{nameSlice}.rg2";
                repairFiles.Add(newPath);
                IRastr Rastr_do = new Rastr();
                IRastr Rastr_RG = new Rastr();
                IRastr Rastr_Rem_Sch = new Rastr();
                // Загрузка файлов до коррекции
                Rastr_do.Load(RG_KOD.RG_REPL, rastrFilesRoc[i], "");
                Rastr_RG.Load(RG_KOD.RG_REPL, rastrFilesMdp[i], "");
                Rastr_Rem_Sch.Load(RG_KOD.RG_REPL, rastrFilesMdp[i], "");
                //Обращение к таблице ТИ:Каналы, узлы, ветви
                ITable _tableTIСhannel1 = (ITable)Rastr_do.Tables.Item("ti");
                ITable _tableTIСhannel2 = (ITable)Rastr_RG.Tables.Item("node");
                ITable _tableTIСhannel3 = (ITable)Rastr_RG.Tables.Item("vetv");
                ITable _tableTIСhannel4 = (ITable)Rastr_RG.Tables.Item("Generator");
                int schet = _tableTIСhannel1.Size;
                int schet2 = _tableTIСhannel2.Size;
                int schet3 = _tableTIСhannel3.Size;
                int schet4 = _tableTIСhannel4.Size;

                //Обращение к таблице узлы
                ITable node = (ITable)Rastr_RG.Tables.Item("node");
                //Отбращение к колонке Номер
                ICol nom = (ICol)node.Cols.Item("ny");
                //Отбращение к колонке Название
                ICol nazv = (ICol)node.Cols.Item("name");
                // Обращение к колонке Pген
                ICol Pgen = (ICol)node.Cols.Item("pg");
                // Обращение к колонке Qген
                ICol Qgen = (ICol)node.Cols.Item("qg");
                // Обращение к колонке Pнаг
                ICol Pnag = (ICol)node.Cols.Item("pn");
                // Обращение к колонке Qнаг
                ICol Qnag = (ICol)node.Cols.Item("qn");
                //Отбращение к колонке U
                ICol vras = (ICol)node.Cols.Item("vras");

                ITable node2 = (ITable)Rastr_Rem_Sch.Tables.Item("node");
                // Обращение к колонке Pген
                ICol Pgen2 = (ICol)node2.Cols.Item("pg");
                // Обращение к колонке Qген
                ICol Qgen2 = (ICol)node2.Cols.Item("qg");
                // Обращение к колонке Pнаг
                ICol Pnag2 = (ICol)node2.Cols.Item("pn");
                // Обращение к колонке Qнаг
                ICol Qnag2 = (ICol)node2.Cols.Item("qn");
                //Отбращение к колонке U
                ICol vras2 = (ICol)node2.Cols.Item("vras");

                //Обращение к таблице ветви
                ITable vetv = (ITable)Rastr_RG.Tables.Item("vetv");
                //Отбращение к колонке Номер начала
                ICol nomNach = (ICol)vetv.Cols.Item("ip");
                //Отбращение к колонке Номер конца
                ICol nomKonch = (ICol)vetv.Cols.Item("iq");
                // Обращение к колонке Pнач
                ICol Pnach = (ICol)vetv.Cols.Item("pl_ip");
                // Обращение к колонке Qнач
                ICol Qnach = (ICol)vetv.Cols.Item("ql_ip");

                //Обращение к таблице ветви
                ITable vetv2 = (ITable)Rastr_Rem_Sch.Tables.Item("vetv");
                // Обращение к колонке Pнач
                ICol Pnach2 = (ICol)vetv2.Cols.Item("pl_ip");
                // Обращение к колонке Qнач
                ICol Qnach2 = (ICol)vetv2.Cols.Item("ql_ip");
                //Обращение к таблице генераторы(УР)
                ITable generator = (ITable)Rastr_RG.Tables.Item("Generator");
                // Обращение к колонке N узла
                ICol nodeG = (ICol)generator.Cols.Item("Node");
                // Обращение к колонке Pг_ген
                ICol Pg_gen = (ICol)generator.Cols.Item("P");
                // Обращение к колонке Qг_ген
                ICol Qg_gen = (ICol)generator.Cols.Item("Q");
                //Обращение к таблице генераторы(УР)
                ITable generator2 = (ITable)Rastr_Rem_Sch.Tables.Item("Generator");
                // Обращение к колонке Pг_ген
                ICol Pg_gen2 = (ICol)generator2.Cols.Item("P");
                // Обращение к колонке Qг_ген
                ICol Qg_gen2 = (ICol)generator2.Cols.Item("Q");
                //Обращение к таблице ТИ:Каналы
                ITable _tableTIСhannel = (ITable)Rastr_do.Tables.Item("ti");
                // Обращение к индексу значение
                ICol val = (ICol)_tableTIСhannel.Cols.Item("ti_val");
                //Отбращение к колонке код ОС
                ICol tg_kod = (ICol)_tableTIСhannel.Cols.Item("cod_oc");
                //Отбращение к колонке привязка
                ICol prv = (ICol)_tableTIСhannel.Cols.Item("prv_num");
                //Отбращение к колонке ИД1
                ICol id1 = (ICol)_tableTIСhannel.Cols.Item("id1");
                //Отбращение к колонке ИД2
                ICol id2 = (ICol)_tableTIСhannel.Cols.Item("id2");

                string nameTI = "";
                double numTI = 0;
                double homU = 0;
                string typeTI = "";

                string nameTI1 = "";
                double numTI1 = 0;
                double homU1 = 0;
                string typeTI1 = "";
                int res = 0;
                //Создание ремонтной схемы
                for (int index = 0; index < schet3; index++)
                {
                    //Обращение к таблице ветви
                    ITable vetvV = (ITable)Rastr_Rem_Sch.Tables.Item("vetv");
                    //Отбращение к колонке Номер начала
                    ICol nomNachV = (ICol)vetv.Cols.Item("ip");
                    //Отбращение к колонке Номер конца
                    ICol nomKonchV = (ICol)vetv.Cols.Item("iq");
                    //Отбращение к колонке Номер конца
                    ICol nomParr = (ICol)vetv.Cols.Item("np");
                    //Отбращение к колонке Статус
                    ICol sta = (ICol)vetvV.Cols.Item("sta");

                    ICol name = (ICol)vetvV.Cols.Item("name");

                    
                    if (Convert.ToInt32(nomNachV.get_ZN(index)) == numberStart)
                    {
                        if(Convert.ToInt32(nomKonchV.get_ZN(index)) == numberEnd)
                        {
                            if (Convert.ToInt32(nomParr.get_ZN(index)) == parrNumber)
                            {
                                sta.set_ZN(index, status);
                                Rastr_Rem_Sch.rgm("");
                                if (i == 0)
                                {
                                    DatabaseConection.WriteDataWithChangingState(numberRepairDiagram, Convert.ToString(name.get_ZN(index)), numberStart, numberEnd, parrNumber, status);
                                }
                                res++;
                                break;
                            }
                        }

                    }
                }
                if (res == 0)
                {
                    throw new ArgumentException($"Такой ветви в срезе {nameSlice} не существует");
                }
                //Парсинг узлов
                for (int index = 0; index < schet2; index++)
                {
                    double deltaU = Convert.ToDouble(vras.get_ZN(index)) - Convert.ToDouble(vras2.get_ZN(index));
                    double deltaPg = Convert.ToDouble(Pgen.get_ZN(index)) - Convert.ToDouble(Pgen2.get_ZN(index));
                    double deltaQg = Convert.ToDouble(Qgen.get_ZN(index)) - Convert.ToDouble(Qgen2.get_ZN(index));
                    double deltaPn = Convert.ToDouble(Pnag.get_ZN(index)) - Convert.ToDouble(Pnag2.get_ZN(index));
                    double deltaQn = Convert.ToDouble(Qnag.get_ZN(index)) - Convert.ToDouble(Qnag2.get_ZN(index));
                    int cc = 0;
                    for (int index2 = 0; index2 < schet; index2++)
                    {

                        if (Convert.ToInt32(id2.get_ZN(index2)) == 0)
                        {
                            if (Convert.ToInt32(tg_kod.get_ZN(index2)) == 1)
                            {
                                if (Convert.ToInt32(id1.get_ZN(index2)) == Convert.ToInt32(nom.get_ZN(index)))
                                {
                                    if (prv.get_ZN(index2).ToString() == "U")
                                    {
                                        double znach = Convert.ToDouble(val.get_ZN(index2));
                                        val.set_ZN(index2, znach + deltaU);
                                        cc++;
                                    }
                                    if (prv.get_ZN(index2).ToString() == "Pген")
                                    {
                                        double znach = Convert.ToDouble(val.get_ZN(index2));
                                        val.set_ZN(index2, znach + deltaPg);
                                        cc++;
                                    }
                                    if (prv.get_ZN(index2).ToString() == "Qген")
                                    {
                                        double znach = Convert.ToDouble(val.get_ZN(index2));
                                        val.set_ZN(index2, znach + deltaQg);
                                        cc++;
                                    }
                                    if (prv.get_ZN(index2).ToString() == "Pнаг")
                                    {
                                        double znach = Convert.ToDouble(val.get_ZN(index2));
                                        val.set_ZN(index2, znach + deltaPn);
                                        cc++;
                                    }
                                    if (prv.get_ZN(index2).ToString() == "Qнаг")
                                    {
                                        double znach = Convert.ToDouble(val.get_ZN(index2));
                                        val.set_ZN(index2, znach + deltaQn);
                                        cc++;
                                    }
                                }
                            }
                        }
                        if (cc == 5)
                        {
                            break;
                        }


                    }
                }
                //Парсинг ветвей
                for (int index = 0; index < schet3; index++)
                {
                    double deltaPnach = Convert.ToDouble(Pnach.get_ZN(index)) - Convert.ToDouble(Pnach2.get_ZN(index));
                    double deltaQnach = Convert.ToDouble(Qnach.get_ZN(index)) - Convert.ToDouble(Qnach2.get_ZN(index));
                    int cc = 0;
                    for (int index2 = 0; index2 < schet; index2++)
                    {
                        // Загрузка файлов
                        if (Convert.ToInt32(id2.get_ZN(index2)) == Convert.ToInt32(nomKonch.get_ZN(index)))
                        {
                            if (Convert.ToInt32(id1.get_ZN(index2)) == Convert.ToInt32(nomNach.get_ZN(index)))
                            {
                                if (Convert.ToInt32(tg_kod.get_ZN(index2)) == 1)
                                {
                                    if (prv.get_ZN(index2).ToString() == "Pнач")
                                    {
                                        double znach = Convert.ToDouble(val.get_ZN(index2));
                                        val.set_ZN(index2, znach + deltaPnach);
                                        cc++;
                                    }
                                    if (prv.get_ZN(index2).ToString() == "Qнач")
                                    {
                                        double znach = Convert.ToDouble(val.get_ZN(index2));
                                        val.set_ZN(index2, znach + deltaQnach);
                                        cc++;
                                    }
                                }
                            }
                        }
                        if (cc == 2)
                        {
                            break;
                        }


                    }

                }
                //Парсинг генераторов
                for (int index = 0; index < schet4; index++)
                {
                    double deltaP = Convert.ToDouble(Pg_gen.get_ZN(index)) - Convert.ToDouble(Pg_gen2.get_ZN(index));
                    double deltaQ = Convert.ToDouble(Qg_gen.get_ZN(index)) - Convert.ToDouble(Qg_gen2.get_ZN(index));
                    int cc = 0;
                    for (int index2 = 0; index2 < schet; index2++)
                    {
                        if (Convert.ToInt32(id2.get_ZN(index2)) == 0 && Convert.ToInt32(id1.get_ZN(index2)) == Convert.ToInt32(nodeG.get_ZN(index)))
                        {
                            if (Convert.ToInt32(tg_kod.get_ZN(index2)) == 1)
                            {
                                if (prv.get_ZN(index2).ToString() == "Pг_ген-р")
                                {
                                    double znach = Convert.ToDouble(val.get_ZN(index2));
                                    val.set_ZN(index2, znach + deltaP);
                                    cc++;
                                }
                                if (prv.get_ZN(index2).ToString() == "Qг_ген-р")
                                {
                                    double znach = Convert.ToDouble(val.get_ZN(index2));
                                    val.set_ZN(index2, znach + deltaQ);
                                    cc++;
                                }
                            }
                        }
                        if (cc == 2)
                        {
                            break;
                        }


                    }
                }
                Console.WriteLine("Генераторы изменены");
                Rastr_do.Save(newPath, "");
            }
            Dictionary<int, string> truncatedDict = DatabaseConection.GetTruncatedList(corrCoeff, numberExperiment);
            List<int> truncatedList = new List<int> { };
            foreach (var ti in truncatedDict)
            {
                truncatedList.Add(Convert.ToInt32(ti.Value));
            }
            int countTi = truncatedDict.Count;
            int idTM = DatabaseConection.GetMaxIdInTabel("calculation_complexity_coefficient", "id_tm_list") + 1;
            Dictionary<string, int> idSlice = DatabaseConection.GetSliceAndId();
            DatabaseConection.WriteDataWithTmList(idTM, truncatedDict);
            Dictionary<string, double> Kc = OS.CalcularteKc(OS.GoOS(truncatedList, repairFiles));
            Dictionary<int, Dictionary<double, List<int>>> complexityCoefficient = new Dictionary<int, Dictionary<double, List<int>>> { };
            int idComlexityCoeff = DatabaseConection.GetMaxIdInTabel("calculation_complexity_coefficient", "id_complexity_coefficient") + 1;
            Dictionary<string, int> sliceAndIdComplexity = new Dictionary<string, int> { };
            foreach (var i in Kc)
            {
                complexityCoefficient[idComlexityCoeff] = new Dictionary<double, List<int>> { { i.Value, new List<int> { idSlice[i.Key], countTi, idTM, numberRepairDiagram} } };
                sliceAndIdComplexity[i.Key] = idComlexityCoeff;
                idComlexityCoeff++;
            }
            DatabaseConection.WriteDataWithCalculationComplexityCoefficient(complexityCoefficient, true);
        }
    }
    
}
