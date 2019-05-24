using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAT.Models
{
    public class SampleCoordinate
    {

        public static List<SampleCoordinate> GetCoordinate(string containername,List<WATProbeTestData> srcdata)
        {
            var ret = new List<SampleCoordinate>();
            var sql = @"SELECT CONTAINER ,[DeviceNumber] ,Xcoord ,Ycoord ,exclusion ,HISTORYMAINLINEID ,notes
		                FROM [EngrData].[dbo].[Eval_XY_Coordinates] where [Container] = @containername";
            var dict = new Dictionary<string, string>();
            dict.Add("@containername", containername);
            var dbret = DataBaseUT.ExeAllenSqlWithRes(sql,dict);
            foreach (var line in dbret)
            {
                var tempvm = new SampleCoordinate();
                tempvm.Container = UT.O2S(line[0]);
                tempvm.DeviceNumber = UT.O2S(line[1]);
                tempvm.X = UT.O2S(line[2]);
                tempvm.Y = UT.O2S(line[3]);
                tempvm.exclusion = UT.O2S(line[4]);
                tempvm.HISTORYMAINLINEID = UT.O2S(line[5]);
                tempvm.Notes = UT.O2S(line[6]);
                ret.Add(tempvm);
            }

            var unitdict = new Dictionary<string, bool>();

            if (ret.Count <= 1)
            {
                ret.Clear();
                var dutlist = new List<int>();
                foreach (var item in srcdata)
                {
                    if (!unitdict.ContainsKey(item.UnitNum))
                    {
                        unitdict.Add(item.UnitNum,true);
                        dutlist.Add(UT.O2I(item.UnitNum));
                    }
                }//end foreach

                dutlist.Sort();
                dutlist.Reverse();
                var idx = 1;
                foreach (var item in dutlist)
                {
                    var tempvm = new SampleCoordinate();
                    tempvm.Container = containername;
                    tempvm.DeviceNumber = "";
                    tempvm.X = idx.ToString();
                    tempvm.Y = idx.ToString();
                    tempvm.exclusion = "0";
                    tempvm.HISTORYMAINLINEID = "";
                    tempvm.Notes = "DUMMY XY";
                    ret.Add(tempvm);
                    idx += 1;
                }
            }
            return ret;
        }

        public static Dictionary<string, bool> GetNonExclusionUnitDict(List<SampleCoordinate> srcdata)
        {
            var dict = new Dictionary<string, bool>();
            foreach (var item in srcdata)
            {
                if (string.Compare(item.exclusion, "true",true) != 0 && string.Compare(item.exclusion, "1",true) != 0)
                {
                    if (!dict.ContainsKey(item.DeviceNumber))
                    { dict.Add(item.DeviceNumber, true); }
                }
            }
            return dict;
        }

        public string Container { set; get; }
        public string DeviceNumber { set; get; }
        public string X { set; get; }
        public string Y { set; get; }
        public string exclusion { set; get; }
        public string HISTORYMAINLINEID { set; get; }
        public string Notes { set; get; }
    }
}