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

            //Container Info
            var containinfo = ContainerInfo.GetInfo(containername);
            if (string.IsNullOrEmpty(containinfo.containername))
            {
                System.Windows.MessageBox.Show("Fail to get container info.....");
                return;
            }

            //SPEC
            var allspec = SpecBinPassFail.GetAllSpec();
            var dutminitem = SpecBinPassFail.GetMinDUT(containinfo.ProductName, dcdname, allspec);
            if (dutminitem.Count == 0)
            {
                System.Windows.MessageBox.Show("Fail to get min DUT count.....");
                return;
            }

            var shippable = 1;

            //PROBE COUNT
            var probecount = ProbeDataQty.GetCount(containinfo.containername);

            //WAT PROB
            var watprobeval = WATProbeTestData.GetData(containinfo.containername);
            if (watprobeval.Count == 0)
            {
                System.Windows.MessageBox.Show("Fail to get wat prob data.....");
                return;
            }

            //WAT PROB FILTER
            var watprobevalfiltered = WATProbeTestDataFiltered.GetFilteredData(watprobeval, rp.ToString());
            if (watprobevalfiltered.Count == 0)
            {
                System.Windows.MessageBox.Show("Fail to get wat prob filtered data.....");
                return;
            }

            //FAIL MODE
            var spec4fmode = SpecBinPassFail.GetParam4FailMode(containinfo.ProductName, rp.ToString(), allspec);
            var failmodes = WATProbeTestDataFiltered.GetWATFailureModes(watprobevalfiltered, spec4fmode);

            var binpndict = SpecBinPassFail.RetrieveBinDict(containinfo.ProductName, allspec);
            var coupondata = WATCouponStats.GetCouponData(watprobevalfiltered, binpndict);
            if (coupondata.Count == 0)
            {
                System.Windows.MessageBox.Show("Fail to get wat coupon data.....");
                return;
            }

            var passfailunitspec = SpecBinPassFail.GetSpecByPNDCDName(containinfo.ProductName, dcdname, allspec);
            var passfailunitdata = WATPassFailUnit.GetPFUnitData(rp.ToString(), dcdname, passfailunitspec, watprobevalfiltered, coupondata);
            
            return;
        }

    }
}