using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAT.Models
{
    public class WATGoldSample
    {
        public static List<string> GetWATTesterList()
        {
            var ret = new List<string>();
            var sql = "select distinct TestStation from insite.dbo.ProductionResult where Containername like 'GS%'";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                ret.Add(UT.O2S(line[0]));
            }
            return ret;
        }

        public static Dictionary<string, Dictionary<string, List<double>>> GetGoldData(string teststation, string param, string startdate, string enddate)
        {
            var ret = new Dictionary<string,Dictionary<string, List<double>>>();

            var sql = "select <param>,TestTimeStamp,Containername,ChannelInfo from insite.dbo.ProductionResult where TestStation = @TestStation and Containername like 'GS%' and TestTimeStamp > @startdate and TestTimeStamp < @enddate";
            sql = sql.Replace("<param>", param);
            var dict = new Dictionary<string, string>();
            dict.Add("@TestStation", teststation);
            dict.Add("@startdate", startdate);
            dict.Add("@enddate", enddate);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                if (line[0] != System.DBNull.Value)
                {
                    var val = UT.O2D(line[0]);
                    var date = UT.O2T(line[1]).ToString("yyyy-MM-dd");
                    var GSKey = UT.O2S(line[2]).Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries)[0].ToUpper().Trim()+ "_" + UT.O2S(line[3]);
                    if (ret.ContainsKey(GSKey))
                    {
                        if (ret[GSKey].ContainsKey(date))
                        {
                            ret[GSKey][date].Add(val);
                        }
                        else
                        {
                            var tmplist = new List<double>();
                            tmplist.Add(val);
                            ret[GSKey].Add(date, tmplist);
                        }
                    }
                    else
                    {
                        var tempdict = new Dictionary<string, List<double>>();
                        var tmplist = new List<double>();
                        tmplist.Add(val);
                        tempdict.Add(date, tmplist);
                        ret.Add(GSKey, tempdict);
                    }
                }
            }
            return ret;
        }

    }
}