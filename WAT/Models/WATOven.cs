using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAT.Models
{
    public class WATOven
    {
        public static List<object> GetOvenData(string teststation, string startdate, string enddate,int freq,string coupongroupid = null)
        {
            var ret = new List<object>();

            var tempdict = new Dictionary<string, List<double>>();
            var currentdict = new Dictionary<string, List<double>>();

            var sql = @"SELECT value,Load_Time FROM [BMS41].[dbo].[BI_Data] where  Data_Set_ID in 
                ( select Data_Set_ID FROM [BMS41].[dbo].[BI_Data]  where Tag = 'START'  and Load_Time > @startdate and Load_Time < @enddate and Value like '%E08%' and Value like '%13mA%') 
                and Tag = 'DATA'  and Load_Time > @startdate and Load_Time < @enddate and ID%" + freq+@" = 0 order by Load_Time asc";

            if (!string.IsNullOrEmpty(coupongroupid))
            {
                sql = @"SELECT value,Load_Time FROM [BMS41].[dbo].[BI_Data] where  Data_Set_ID in 
                ( select Data_Set_ID FROM [BMS41].[dbo].[BI_Data]  where Tag = 'START' and Value like '%"+coupongroupid+@"%') 
                and Tag = 'DATA' order by Load_Time asc";
            }

            var dict = new Dictionary<string, string>();
            dict.Add("@startdate", startdate);
            dict.Add("@enddate", enddate);

            var dbret = DBUtility.ExeOvenSqlWithRes(teststation,sql, dict);

            foreach (var line in dbret)
            {
                if (line[0] != System.DBNull.Value)
                {
                    var val = UT.O2S(line[0]);
                    var date = UT.O2T(line[1]).ToString("yyyy-MM-dd");

                    if (val.ToUpper().Contains("OVENTEMPERATURE"))
                    {
                        var valstr = val.ToUpper().Split(new string[] { "OVENTEMPERATURE" }, StringSplitOptions.None)[1]
                            .Split(new string[] { ":", "," }, StringSplitOptions.None)[1].Replace("\"", "").Replace("}", "").Replace(" ", "");
                        
                        if (valstr.Length > 0 && !valstr.Contains("NAN"))
                        {
                            var v = UT.O2D(valstr);
                            if (v > 0 && v <= 150)
                            {
                                if (!tempdict.ContainsKey(date))
                                {
                                    var templist = new List<double>();
                                    templist.Add(v);
                                    tempdict.Add(date, templist);
                                }
                                else
                                { tempdict[date].Add(v); }
                            }
                        }

                    }

                    if (val.ToUpper().Contains("I[MA]"))
                    {
                        var valstr = val.ToUpper().Split(new string[] { "I[MA]" }, StringSplitOptions.None)[1]
                            .Split(new string[] { ":", "," }, StringSplitOptions.None)[1].Replace("\"", "").Replace("}", "").Replace(" ", "");

                        if (valstr.Length > 0 && !valstr.Contains("NAN"))
                        {
                            var v = UT.O2D(valstr);
                            if (v > 0 && v <= 30)
                            {
                                if (!currentdict.ContainsKey(date))
                                {
                                    var templist = new List<double>();
                                    templist.Add(v);
                                    currentdict.Add(date, templist);
                                }
                                else
                                { currentdict[date].Add(v); }
                            }
                        }
                    }
                }

            }

            if (tempdict.Count > 0)
            {
                ret.Add(tempdict);
                ret.Add(currentdict);
            }

            return ret;
        }
    }

}