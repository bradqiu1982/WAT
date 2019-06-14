using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAT.Models
{
    public class PassFailCoupon4Comparing
    {
        public static List<PassFailCoupon4Comparing> GetData(string containername,string dcdname,List<WATPassFailCoupon> coupondata)
        {
            var ret = new List<PassFailCoupon4Comparing>();
            foreach (var citem in coupondata)
            {
                if (string.IsNullOrEmpty(citem.Bin_PN))
                { continue; }

                var tempvm = new PassFailCoupon4Comparing();
                tempvm.ContainerName = containername;
                tempvm.DCDName = dcdname;
                tempvm.ProductName = citem.Eval_PN;
                tempvm.ParameterName = citem.ParamName;
                tempvm.UpperSpecLimit = citem.UpperLimit;
                tempvm.LowerSpecLimit = citem.LowLimit;
                tempvm.TargetValue = citem.Bin_PN;
                tempvm.ActualValue = ActVal(citem.MinVal,citem.MaxVal,citem.UpperLimit,citem.LowLimit);
                tempvm.PassFail = (citem.fails == 1) ? "FAIL" : "PASS";
                
                ret.Add(tempvm);
            }
            return ret;
        }

        public static void StoreComparingData(string containername, string dcdname,List<PassFailCoupon4Comparing> srcdata)
        {
            var dict = new Dictionary<string, string>();
            dict.Add("@ContainerName", containername);
            dict.Add("@DCDName", dcdname);
            var sql = @"delete from [WAT].[dbo].[PassFailCoupon4Comparing] where ContainerName = @ContainerName and DCDName = @DCDName";
            DBUtility.ExeLocalSqlNoRes(sql, dict);

            foreach (var item in srcdata)
            {
                dict = new Dictionary<string, string>();
                dict.Add("@ContainerName", item.ContainerName);
                dict.Add("@DCDName", item.DCDName);
                dict.Add("@ProductName", item.ProductName);
                dict.Add("@ParameterName", item.ParameterName);
                dict.Add("@UpperSpecLimit", item.UpperSpecLimit);
                dict.Add("@LowerSpecLimit", item.LowerSpecLimit);
                dict.Add("@TargetValue", item.TargetValue);
                dict.Add("@ActualValue", item.ActualValue);
                dict.Add("@PassFail", item.PassFail);

                sql = @"insert into [WAT].[dbo].[PassFailCoupon4Comparing](ContainerName,DCDName,ProductName,ParameterName,UpperSpecLimit,LowerSpecLimit,TargetValue,ActualValue,PassFail) 
                            values(@ContainerName,@DCDName,@ProductName,@ParameterName,@UpperSpecLimit,@LowerSpecLimit,@TargetValue,@ActualValue,@PassFail)";

                DBUtility.ExeLocalSqlNoRes(sql, dict);
            }
        }

        private static string ActVal(string minVal, string maxVal, string uplimit, string lowlimit)
        {
            var min = UT.O2D(minVal);
            var max = UT.O2D(maxVal);
            var ul = UT.O2D(uplimit);
            var ll = UT.O2D(lowlimit);
            if (max > ul)
            { return Math.Round(max,3).ToString(); }
            else if (min < ll)
            { return Math.Round(min,3).ToString(); }
            else if (Math.Abs(ul - max) < Math.Abs(min - ll))
            { return Math.Round(max,3).ToString(); }
            else
            { return Math.Round(min,3).ToString(); }
        }

        public string ContainerName { set; get; }
        public string DCDName { set; get; }
        public string ProductName { set; get; }
        public string ParameterName { set; get; }
        public string UpperSpecLimit { set; get; }
        public string LowerSpecLimit { set; get; }
        public string TargetValue { set; get; }
        public string ActualValue { set; get; }
        public string PassFail { set; get; }

    }
}