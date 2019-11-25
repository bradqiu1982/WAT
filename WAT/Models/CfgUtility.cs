using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WAT.Models
{
    public class CfgUtility
    {
        public static Dictionary<string, string> GetSysConfig(Controller ctrl)
        {
            var lines = new List<string>();
            if (ctrl != null)
            {
                lines = System.IO.File.ReadAllLines(ctrl.Server.MapPath("~/Scripts/WATCfg.txt")).ToList();
            }
            else
            {
                lines = System.IO.File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory+ @"\Scripts\WATCfg.txt").ToList();
            }
            var ret = new Dictionary<string, string>();
            foreach (var line in lines)
            {
                if (line.Contains("##"))
                {
                    continue;
                }

                if (line.Contains(":::"))
                {
                    var kvpair = line.Split(new string[] { ":::" }, StringSplitOptions.RemoveEmptyEntries);
                    if (!ret.ContainsKey(kvpair[0].Trim()) && kvpair.Length > 1)
                    {
                        ret.Add(kvpair[0].Trim(), kvpair[1].Trim());
                    }
                }//end if
            }//end foreach
            return ret;
        }

        //public static Dictionary<string, string> GetNPIMachine(Controller ctrl)
        //{
        //    var lines = System.IO.File.ReadAllLines(ctrl.Server.MapPath("~/Scripts/npidepartmentmachine.cfg"));
        //    var ret = new Dictionary<string, string>();
        //    foreach (var line in lines)
        //    {
        //        if (line.Contains("##"))
        //        {
        //            continue;
        //        }

        //        if (line.Contains(":::"))
        //        {
        //            var kvpair = line.Split(new string[] { ":::" }, StringSplitOptions.RemoveEmptyEntries);
        //            if (!ret.ContainsKey(kvpair[0].Trim()))
        //            {
        //                ret.Add(kvpair[0].Trim().ToUpper(), kvpair[1].Trim());
        //            }
        //        }//end if
        //    }//end foreach
        //    return ret;
        //}

    }
}