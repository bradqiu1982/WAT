using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WXLogic
{
    public class WXFinEvalDispositionLog
    {
        public static void CleanFinEvalDispositionLog(string CouponGroup, bool sharedatatoallen,string RP)
        {
            if ((CouponGroup.Contains("E") || CouponGroup.Contains("R") || CouponGroup.Contains("T"))
            && sharedatatoallen)
            {
                var sql = "delete from [WAT].[dbo].[WXFinEvalDispositionLog] where ContainerName = @ContainerName and ReadPoint =@ReadPoint";
                var dict = new Dictionary<string, string>();
                dict.Add("@ContainerName", CouponGroup);
                dict.Add("@ReadPoint", RP);
                DBUtility.ExeLocalSqlNoRes(sql, dict);
            }
        }

        public static void WriteData(string CouponGroup, bool sharedatatoallen 
            , string WUL,string WLL,string Eval_PN,string Bin_PN, string paramname
            , List<double> values,string ProdFam,string RP)
        {
            if ((CouponGroup.Contains("E") || CouponGroup.Contains("R") || CouponGroup.Contains("T"))
                && sharedatatoallen && values.Count > 0)
            {
                var passfail = "FAIL";
                var actualvalue = values[0];
                var ul = UT.O2D(WUL);
                var ll = UT.O2D(WLL);
                var max = values.Max();
                var min = values.Min();

                if (!string.IsNullOrEmpty(WUL) && max > ul)
                { actualvalue = max; }
                else if (!string.IsNullOrEmpty(WLL) && min < ll)
                { actualvalue = min; }
                else if (!string.IsNullOrEmpty(WUL) && !string.IsNullOrEmpty(WUL))
                {
                    passfail = "PASS";
                    var mindelta = Math.Abs(min - ll);
                    var maxdelta = Math.Abs(max - ul);
                    if (maxdelta < mindelta)
                    { actualvalue = max; }
                    else
                    { actualvalue = min; }
                }

                var dict = new Dictionary<string, string>();
                dict.Add("@ContainerName", CouponGroup);
                dict.Add("@ProductName", Eval_PN);
                dict.Add("@ParameterName", paramname);
                dict.Add("@UpperSpecLimit", ul.ToString());
                dict.Add("@LowerSpecLimit", ll.ToString());
                dict.Add("@TargetValue", UT.O2S(Bin_PN));
                dict.Add("@ActualValue", actualvalue.ToString());
                dict.Add("@PassFail", passfail);
                dict.Add("@TxnDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                dict.Add("@ProdFam", ProdFam);
                dict.Add("@ReadPoint", RP);

                var sql = @"insert into [WAT].[dbo].[WXFinEvalDispositionLog](ContainerName,ProductName,ParameterName,UpperSpecLimit,LowerSpecLimit,TargetValue,ActualValue,PassFail,TxnDate,ProdFam,ReadPoint) 
                            values(@ContainerName,@ProductName,@ParameterName,@UpperSpecLimit,@LowerSpecLimit,@TargetValue,@ActualValue,@PassFail,@TxnDate,@ProdFam,@ReadPoint)";
                DBUtility.ExeLocalSqlNoRes(sql, dict);
            }
        }

    }
}
