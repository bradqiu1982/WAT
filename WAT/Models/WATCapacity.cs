using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAT.Models
{
    public class WATCapacity
    {

        public List<WATCapacity> GetWATCapacity()
        {
            var ret = new List<WATCapacity>();
            var sql = @"select left(c.containername,10) as wafer,min(c.TestTimeStamp) as mintime,REPLACE(REPLACE('1x'+ep.AppVal1+ ' ' +r.RealRate,'14G','10G'),'28G','25G') as vtype FROM [Insite].[dbo].[ProductionResult] c with(nolock) 
                      left join wat.dbo.WXEvalPN ep with (nolock) on ep.WaferNum = left(c.Containername,9)
                      left join wat.dbo.WXEvalPNRate r on left(ep.EvalPN,7) = r.EvalPN
                      where len(c.Containername) = 20 and c.TestStep = 'PRLL_VCSEL_Pre_Burn_in_Test' and c.TestTimeStamp > '2020-01-01 00:00:00'  and  r.RealRate is not null
                      group by left(c.containername,10),r.RealRate,ep.AppVal1  order by mintime asc";

            sql = @"select left(c.containername,14) as wafer,min(c.TestTimeStamp) as mintime,REPLACE(REPLACE('1x'+ep.AppVal1+ ' ' +r.RealRate,'14G','10G'),'28G','25G') as vtype FROM [Insite].[dbo].[ProductionResult] c with(nolock) 
                  left join wat.dbo.WXEvalPN ep with (nolock) on ep.WaferNum = left(c.Containername,13)
                  left join wat.dbo.WXEvalPNRate r on left(ep.EvalPN,7) = r.EvalPN
                  where len(c.Containername) = 24 and c.TestStep = 'PRLL_VCSEL_Pre_Burn_in_Test' and c.TestTimeStamp > '2020-01-01 00:00:00'   and  r.RealRate is not null
                  group by left(c.containername,14),r.RealRate,ep.AppVal1  order by mintime asc";

        //    select left(c.containername, 10) as wafer,min(c.TestTimeStamp) as mintime,REPLACE(REPLACE('1x' + ep.AppVal1 + ' ' + r.RealRate, '14G', '10G'), '28G', '25G') as vtype FROM[Insite].[dbo].[ProductionResult] c with(nolock)
        //left join wat.dbo.WXEvalPN ep with(nolock) on ep.WaferNum = left(c.Containername, 9)
      
        //left join wat.dbo.WXEvalPNRate r on left(ep.EvalPN, 7) = r.EvalPN
      
        //where len(c.Containername) = 20 and c.TestStep = 'PRLL_VCSEL_Pre_Burn_in_Test' and c.TestTimeStamp > '2020-01-01 00:00:00'  and  r.RealRate is not null
      
        //group by left(c.containername, 10), r.RealRate, ep.AppVal1  order by mintime asc
      

        //select left(c.containername, 14) as wafer, min(c.TestTimeStamp) as mintime, REPLACE(REPLACE('1x' + ep.AppVal1 + ' ' + r.RealRate, '14G', '10G'), '28G', '25G') as vtype FROM[Insite].[dbo].[ProductionResult] c with(nolock)
      
        //left join wat.dbo.WXEvalPN ep with(nolock) on ep.WaferNum = left(c.Containername, 13)
      
        //left join wat.dbo.WXEvalPNRate r on left(ep.EvalPN, 7) = r.EvalPN
      
        //where len(c.Containername) = 24 and c.TestStep = 'PRLL_VCSEL_Pre_Burn_in_Test' and c.TestTimeStamp > '2020-01-01 00:00:00'   and  r.RealRate is not null
      
        //group by left(c.containername, 14), r.RealRate, ep.AppVal1  order by mintime asc
            return ret;
        }

        public WATCapacity()
        {
            Wafer = "";
            BinDict = new Dictionary<string, int>();
        }
        public string Wafer { set; get; }
        public DateTime WFDate { set; get; }
        public string WFDateStr { get {
                try
                {
                    return WFDate.ToString("yyyy/MM/dd");
                }
                catch (Exception ex) { return ""; }
            } }
        public Dictionary<string, int> BinDict { set; get; }
    }
}