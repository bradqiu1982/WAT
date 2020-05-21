using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WAT.Models
{
    public class WATJobCheckVM
    {
        public static List<JobCheckVM> GetWATTodaysJobCheck(Controller ctrl)
        {
            var checkid = DateTime.Now.ToString("yyyy-MM-dd");
            return JobCheckVM.GetCheckVM("WATTEST",checkid, ctrl);
        }

        public static List<JobCheckVM> GetWATCheckVM(string startdate)
        {
            return JobCheckVM.GetAllCheckVM("WATTEST",startdate);
        }

        public static void UpdateWATCheckVM(string itemid, string mark, string status, string checkman)
        {
            var checkid = DateTime.Now.ToString("yyyy-MM-dd");
            JobCheckVM.UpdateCheckVM("WATTEST", itemid, checkid, mark, status, checkman);
        }

        public static bool WATCheckDone()
        {
            var checkid = DateTime.Now.ToString("yyyy-MM-dd")+" 00:00:00";
            var ret = GetWATCheckVM(checkid);
            if (ret.Count > 0)
            {
                foreach (var item in ret)
                {
                    if (!item.Status.Contains("DONE")) {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }


    }
}