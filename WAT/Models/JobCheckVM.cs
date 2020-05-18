using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WAT.Models
{
    public class JobCheckVM
    {
        public static List<JobCheckVM> GetCheckVM(string pj,string checkid,Controller ctrl)
        {
            var ret = new List<JobCheckVM>();

            var dict = new Dictionary<string, string>();
            dict.Add("@Project", pj);
            dict.Add("@CheckID", checkid);
            var sql = "select Project,CheckItemID,CheckItem,Mark,MarkNeed,Status,CheckID,CheckMan,CheckDate from JobCheckVM where Project=@Project and CheckID=@CheckID order by CheckItemID asc";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,dict);
            if (dbret.Count == 0)
            {
                var lines = System.IO.File.ReadAllLines(ctrl.Server.MapPath("~/Scripts/JOBCHECKLIST.txt")).ToList();
                foreach (var line in lines)
                {
                    if (!line.Contains("#"))
                    {
                        var items = line.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                        if (string.Compare(items[0], pj) == 0)
                        {
                            var tempvm = new JobCheckVM();
                            tempvm.Project = UT.O2S(items[0]);
                            tempvm.CheckItemID = UT.O2S(items[1]);
                            tempvm.CheckItem = UT.O2S(items[2]);
                            tempvm.MarkNeed = UT.O2S(items[3]);
                            tempvm.Status = "PENDING";
                            tempvm.CheckID = checkid;
                            ret.Add(tempvm);
                        }
                    }
                }

                foreach (var item in ret)
                { item.StoreData(); }

            }
            else
            {
                foreach (var line in dbret)
                {
                    var tempvm = new JobCheckVM();
                    tempvm.Project = UT.O2S(line[0]);
                    tempvm.CheckItemID = UT.O2S(line[1]);
                    tempvm.CheckItem = UT.O2S(line[2]);
                    tempvm.Mark = UT.O2S(line[3]);
                    tempvm.MarkNeed = UT.O2S(line[4]);
                    tempvm.Status = UT.O2S(line[5]);
                    tempvm.CheckID = UT.O2S(line[6]);
                    tempvm.CheckMan = UT.O2S(line[7]);
                    tempvm.CheckDate = UT.T2S(line[8]);
                    ret.Add(tempvm);
                }
            }

            return ret;
        }

        public static List<JobCheckVM> GetAllCheckVM(string pj)
        {
            var ret = new List<JobCheckVM>();

            var dict = new Dictionary<string, string>();
            dict.Add("@Project", pj);
            var sql = "select Project,CheckItemID,CheckItem,Mark,MarkNeed,Status,CheckID,CheckMan,CheckDate from JobCheckVM where Project=@Project order by CheckDate asc,CheckItemID asc";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);

            foreach (var line in dbret)
            {
                var tempvm = new JobCheckVM();
                tempvm.Project = UT.O2S(line[0]);
                tempvm.CheckItemID = UT.O2S(line[1]);
                tempvm.CheckItem = UT.O2S(line[2]);
                tempvm.Mark = UT.O2S(line[3]);
                tempvm.MarkNeed = UT.O2S(line[4]);
                tempvm.Status = UT.O2S(line[5]);
                tempvm.CheckID = UT.O2S(line[6]);
                tempvm.CheckMan = UT.O2S(line[7]);
                tempvm.CheckDate = UT.T2S(line[8]);
                ret.Add(tempvm);
            }

            return ret;
        }

        public static void UpdateCheckVM(string pj,string itemid,string checkid,string mark,string status,string checkman)
        {
            var sql = "update JobCheckVM set Mark=@Mark,Status=@Status,CheckMan=@CheckMan where Project=@Project and CheckItemID=@CheckItemID and CheckID=@CheckID";
            var dict = new Dictionary<string, string>();
            dict.Add("@Mark", mark);
            dict.Add("@Status", status);
            dict.Add("@CheckMan", checkman);

            dict.Add("@Project", pj);
            dict.Add("@CheckItemID", itemid);
            dict.Add("@CheckID", checkid);

            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        private void StoreData()
        {
            var dict = new Dictionary<string, string>();
            dict.Add("@Project", Project);
            dict.Add("@CheckItemID", CheckItemID.ToString());
            dict.Add("@CheckItem", CheckItem);
            dict.Add("@Mark", Mark);

            dict.Add("@MarkNeed", MarkNeed);
            dict.Add("@Status", Status);
            dict.Add("@CheckID", CheckID);
            dict.Add("@CheckMan", CheckMan);
            dict.Add("@CheckDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            var sql = "insert into JobCheckVM(Project,CheckItemID,CheckItem,Mark,MarkNeed,Status,CheckID,CheckMan,CheckDate) values(@Project,@CheckItemID,@CheckItem,@Mark,@MarkNeed,@Status,@CheckID,@CheckMan,@CheckDate)";
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public JobCheckVM()
        {
            Project = "";
            CheckItemID = "-1";
            CheckItem = "";
            Mark = "";
            MarkNeed = "FALSE";
            Status = "PENDING";
            CheckID = "";
            CheckMan = "";
            CheckDate = "";
        }

        public string Project { set; get; }
        public string CheckItemID { set; get; }
        public string CheckItem { set; get; }
        public string Mark { set; get; }
        public string MarkNeed { set; get; }
        //PENDING,DONE
        public string Status { set; get; }
        public string CheckID { set; get; }
        public string CheckMan { set; get; }
        public string CheckDate { set; get; }
    }
}