using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Windows;

namespace WAT.Models
{
    public class AllenWATLogic
    {
        //return value PASS;RETEST,REASON;SCRAP,REASON;EXCEPTION,REASON
        public static void PassFaile(string containername,string dcdname_)
        {
            var dcdname = dcdname_.Replace("_dp", "_rp").Replace("_BurnInTest","");
            var rp = Convert.ToInt32(dcdname.Split(new string[] { "_rp" }, StringSplitOptions.RemoveEmptyEntries)[1]);
            var containinfo = ContainerInfo.GetInfo(containername);
            if (string.IsNullOrEmpty(containinfo.containername))
            {
                System.Windows.MessageBox.Show("Fail to get container info.....");
                return;
            }

            var dutminqty = DUTMinQTY.GetCount(containinfo.ProductName, dcdname);
            var shippable = 1;
            var probecount = ProbeDataQty.GetCount(containinfo.containername);

            var watprobeval = WATProbeTestData.GetData(containinfo.containername);
            if (watprobeval.Count == 0)
            {
                System.Windows.MessageBox.Show("Fail to get wat prob data.....");
                return;
            }

            

            return;
        }

    }
}