using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAT.Models
{
    public class SampleVehicles
    {

        public static List<SampleVehicles> GetData(List<RequiredVehicles> reqv,string containername)
        {
            var ret = new List<SampleVehicles>();

            var evalpnlist = new List<string>();
            foreach (var r in reqv)
            { evalpnlist.Add(r.Eval_PN); }

            if (evalpnlist.Count == 0)
            { return ret; }

            var sql = @"select EvalPartNumber from  [EngrData].[insite].[FinEvalJobStartInfo] 
						WHERE inactive = 0 and sampleAC=1 and EvalPartNumber in <PNCOND>
						GROUP BY EvalPartNumber";
            var pncond = "('" + string.Join("','", evalpnlist) + "')";
            sql = sql.Replace("<PNCOND>", pncond);

            var dbret = DataBaseUT.ExeAllenSqlWithRes(sql);
            var FPN = new List<string>();
            foreach (var line in dbret)
            {
                FPN.Add(UT.O2S(line[0]));
            }

            if (FPN.Count == 0)
            { return ret; }

            pncond = "('" + string.Join("','", FPN) + "')";
            sql = @"select
				*
				,rank() over(partition by eval_productname order by containername desc) as AC_container_order
				
				from

				(
					SELECT
					c.containername
					,pf.eval_productname
					,pf.bin_product
					,sum(CASE WHEN dl.passfail is null then 1 else 0 end) as missing
					from insite.insite.container c with(nolock)
					inner join insite.insite.product p with(nolock) on p.productid=c.productid
					inner join insite.insite.productbase pb with(nolock) on pb.productbaseid=p.productbaseid
					inner join [EngrData].[insite].[Eval_Specs_Bin_PassFail] pf with(nolock) on pf.[Eval_ProductName]=pb.productname
					left join [Insite].[insite].[FinEvalDispositionLog] dl with(nolock) on dl.containername=c.containername and rtrim(ltrim(str(dl.targetvalue)))=pf.bin_product and dl.parametername=pf.parametername
					where pb.productname in <PNCOND>
					and c.containername not like '%wcar%'
					and c.ContainerType in ('N','Q','W')
					and pf.bin_product is not null
					group by c.containername
					,pf.eval_productname
					,pf.bin_product
			
				) sq
				where sq.missing = 0 and sq.containername < @containername
				order by containername desc,eval_productname,bin_product";

            sql = sql.Replace("<PNCOND>", pncond);
            var dict = new Dictionary<string, string>();
            dict.Add("@containername", containername);

            dbret = DataBaseUT.ExeAllenSqlWithRes(sql,dict);
            foreach (var line in dbret)
            {
                var tempvm = new SampleVehicles();
                tempvm.ContainerName = UT.O2S(line[0]);
                tempvm.Eval_PN = UT.O2S(line[1]);
                tempvm.Bin_PN = UT.O2S(line[2]);
                tempvm.Miss = UT.O2S(line[3]);
                tempvm.AC_order = UT.Db2S(line[4]);
                ret.Add(tempvm);
            }

            return ret;
        }


        public string ContainerName { set; get; }
        public string Eval_PN { set; get; }
        public string Bin_PN { set; get; }
        public string Miss { set; get; }
        public string AC_order { set; get; }
    }
}