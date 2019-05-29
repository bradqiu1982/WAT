using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAT.Models
{
    public class RequiredContainers
    {
        public static List<RequiredContainers> GetContainers(List<RequiredVehicles> reqv,string containername,List<SampleVehicles> samplevehicle)
        {
            var ret = new List<RequiredContainers>();

            var evalpnlist = new List<string>();
            foreach (var r in reqv)
            { evalpnlist.Add(r.Eval_PN); }

            if (evalpnlist.Count == 0)
            { return ret; }

            var pncond = "('" + string.Join("','", evalpnlist) + "')";
            var sql = @"select
				pb.productname
				,containername
				from insite.insite.container c with(nolock)
				inner join insite.insite.product p with(nolock) on p.productid=c.productid
				inner join insite.insite.productbase pb with(nolock) on pb.productbaseid=p.productbaseid
				inner join insite.insite.factory f with(nolock) on f.factoryid=p.factoryid
				left join [Insite].[insite].[SplitHistory] shparent with(nolock) on shparent.fromcontainerid=c.containerid
				left join [Insite].[insite].[SplitHistoryDetails] shdchild with(nolock) on shdchild.tocontainerid=c.containerid

				inner join insite.insite.currentstatus cs with(nolock) on cs.currentstatusid=c.currentstatusid
				inner join insite.insite.spec s with(nolock) on s.specid=cs.specid
				inner join insite.insite.specbase sb with(nolock) on sb.specbaseid=s.specbaseid

				where left(c.containername,9) = left(@containername,9)
				and f.factoryname in ('eval')
				and substring(containername,11,2) <= 10
				and shparent.fromcontainerid is null
				and pb.productname in <PNCOND>

				and not(c.[status] = 2 and sb.specname in ('Eval_CoordinateInput_100pct','Eval_BP/BS'))";

            var dict = new Dictionary<string, string>();
            dict.Add("@containername", containername);
            sql = sql.Replace("<PNCOND>", pncond);

            var dbret = DataBaseUT.ExeAllenSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                var tempvm = new RequiredContainers();
                tempvm.PN = UT.O2S(line[0]);
                tempvm.ContainerName = UT.O2S(line[1]);
                ret.Add(tempvm);
            }

            foreach (var item in samplevehicle)
            {
                if (string.Compare(item.AC_order, "1", true) == 0)
                {
                    var tempvm = new RequiredContainers();
                    tempvm.PN = item.Eval_PN;
                    tempvm.ContainerName = item.ContainerName;
                    ret.Add(tempvm);
                }
            }

            return ret;
        }


        public string PN { set; get; }
        public string ContainerName { set; get; }
    }
}