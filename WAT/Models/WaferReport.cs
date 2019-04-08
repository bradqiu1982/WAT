using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAT.Models
{
    public class WaferReport
    {
        public static List<WaferReport> RetrieveWaferReport(string issuekey)
        {
            var ret = new List<WaferReport>();
            var csql = "select IssueKey,Comment,Reporter,CommentDate,CommentType from IssueComments where IssueKey = '<IssueKey>' and Removed <> 'TRUE'";
            csql = csql.Replace("<IssueKey>", issuekey);
            var cdbret = DBUtility.ExeNPITraceSqlWithRes(csql, null);
            foreach (var r in cdbret)
            {
                var tempcomment = new WaferReport();
                tempcomment.IssueKey = Convert.ToString(r[0]);
                tempcomment.dbComment = Convert.ToString(r[1]);
                tempcomment.Reporter = Convert.ToString(r[2]);
                tempcomment.CommentDate = DateTime.Parse(Convert.ToString(r[3]));
                tempcomment.CommentType = Convert.ToString(r[4]);
                ret.Add(tempcomment);
            }
            return ret;
        }

        public string IssueKey { set; get; }

        private string sComment = "";
        public string Comment
        {
            set { sComment = value; }
            get { return sComment; }
        }

        public string CommentType { set; get; }

        public string dbComment
        {
            get
            {
                if (string.IsNullOrEmpty(sComment))
                {
                    return "";
                }
                else
                {
                    try
                    {
                        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(sComment));
                    }
                    catch (Exception)
                    {
                        return "";
                    }
                }

            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    sComment = "";
                }
                else
                {
                    try
                    {
                        sComment = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(value));
                    }
                    catch (Exception)
                    {
                        sComment = "";
                    }

                }

            }
        }

        public string Reporter { set; get; }

        public DateTime CommentDate { set; get; }
        public string CommentDateStr
        {
            get
            {
                try
                {
                    return CommentDate.ToString("yyyy-MM-dd HH:mm:ss");
                }
                catch (Exception ex) { return string.Empty; }
            }
        }
        
    }
}