using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAT.Models
{
    public class WATAnalyzeComment
    {
        public static void AddComment(string watid, string comment)
        {
            var dict = new Dictionary<string, string>();
            dict.Add("@watid", watid);
            dict.Add("@commentid", Guid.NewGuid().ToString("N"));
            dict.Add("@comment", comment);
            dict.Add("@updatetime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            var sql = "insert into WATAnalyzeComment(watid,commentid,comment,updatetime) values(@watid,@commentid,@comment,@updatetime)";
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public static List<WATAnalyzeComment> GetComment(string watid)
        {
            var ret = new List<WATAnalyzeComment>();

            var dict = new Dictionary<string, string>();
            dict.Add("@watid", watid);
            var sql = "select watid,commentid,comment,updatetime from WATAnalyzeComment where watid=@watid order by updatetime desc";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                var tempvm = new WATAnalyzeComment();
                tempvm.watid = UT.O2S(line[0]);
                tempvm.commentid = UT.O2S(line[1]);
                tempvm.comment = UT.O2S(line[2]);
                tempvm.updatetime = UT.O2T(line[3]).ToString("yyyy-MM-dd HH:mm:ss");
                ret.Add(tempvm);
            }
            return ret;
        }

        public static Dictionary<string, bool> GetAllWATid()
        {
            var ret = new Dictionary<string, bool>();
            var sql = "select distinct watid from WATAnalyzeComment";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var watid = UT.O2S(line[0]);
                if (!ret.ContainsKey(watid))
                { ret.Add(watid, true); }
            }
            return ret;
        }

        public string watid { set; get; }
        public string commentid { set; get; }
        public string comment { set; get; }
        public string updatetime { set; get; }
    }
}