using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAT.Models
{
    public class WATProbeTestData
    {
        public static List<WATProbeTestData> GetData(string containername,int rp)
        {
            var ret = new List<WATProbeTestData>();
            var sql = @" SELECT  TIME_STAMP ,CONTAINER_NUMBER ,TOOL_NAME
                          ,READ_POINT ,UNIT_NUMBER ,X ,Y
                          ,COMMON_TEST_NAME ,TEST_VALUE
                          ,PROBE_VALUE ,BIN_NUMBER ,BIN_NAME
                          FROM [EngrData].[insite].[Get_EVAL_Data_By_CONTAINER_NUMBER_Link_PROBE_DATA] with(nolock) 
                           where  [CONTAINER_NUMBER] = @containername and READ_POINT  IN ('0','1','2','3','"
                            +(rp-1).ToString()+"','"+rp.ToString()+"')";

            var dict = new Dictionary<string, string>();
            dict.Add("@containername", containername);

            var dbret = DataBaseUT.ExeAllenSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                var tempvm = new WATProbeTestData();
                tempvm.TimeStamp = UT.O2T(line[0]);
                tempvm.ContainerNum = UT.O2S(line[1]);
                tempvm.ToolName = UT.O2S(line[2]);

                tempvm.RP = UT.O2S(line[3]);
                tempvm.UnitNum = UT.O2S(line[4]);
                tempvm.X = UT.O2S(line[5]);

                tempvm.Y = UT.O2S(line[6]);
                tempvm.CommonTestName = UT.O2S(line[7]);
                tempvm.TestValue = UT.O2D(line[8]);

                tempvm.ProbeValue = UT.Db2S(line[9]);
                tempvm.BinNum = UT.O2S(line[10]);
                tempvm.BinName = UT.O2S(line[11]);

                ret.Add(tempvm);
            }

            return ret;
        }

        public static int GetReadCount(List<WATProbeTestData> srcdata,string rp)
        {
            var dict1 = new Dictionary<string, WATProbeTestData>();
            foreach (var item in srcdata) {
                if (string.Compare(item.RP, rp, true) == 0)
                {
                    var key = item.RP + ":" + item.TimeStamp.ToString() + ":" + item.UnitNum + ":" + item.CommonTestName;
                    if (!dict1.ContainsKey(key))
                    {
                        var tempvm = new WATProbeTestData();
                        tempvm.RP = item.RP;
                        tempvm.TimeStamp = item.TimeStamp;
                        tempvm.UnitNum = item.UnitNum;
                        tempvm.CommonTestName = item.CommonTestName;
                        dict1.Add(key, tempvm);
                    }//end if
                }//end if
            }//end foreach

            var vallist = dict1.Values.ToList();
            var dict2 = new Dictionary<string, int>();
            foreach (var item in vallist)
            {
                var key = item.UnitNum + ":" + item.CommonTestName;
                if (dict2.ContainsKey(key))
                { dict2[key] += 1; }
                else
                { dict2.Add(key, 1); }
            }//end foreach

            var clist = dict2.Values.ToList();
            return clist.Max();
        }

        public WATProbeTestData()
        {
            TimeStamp = DateTime.Parse("1982-05-06 10:00:00");
            ContainerNum = "";
            ToolName = "";
            RP = "";
            UnitNum = "";
            X = "";
            Y = "";
            CommonTestName = "";
            TestValue = 0.0;
            ProbeValue = "";
            BinNum = "";
            BinName = "";
        }

        public DateTime TimeStamp { set; get; }
        public string ContainerNum { set; get; }
        public string ToolName { set; get; }
        public string RP { set; get; }
        public string UnitNum { set; get; }
        public string X { set; get; }
        public string Y { set; get; }
        public string CommonTestName { set; get; }
        public double TestValue { set; get; }
        public string ProbeValue { set; get; }
        public string BinNum { set; get; }
        public string BinName { set; get; }
    }
}