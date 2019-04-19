using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAT.Models
{
    public class AllenEVALData
    {
        public static void LoadAllenData()
        {
            var now = DateTime.Now;
            var today7am = DateTime.Parse(now.ToString("yyyy-MM-dd") + " 07:00:00");
            var today6pm = DateTime.Parse(now.ToString("yyyy-MM-dd") + " 18:00:00");
            if (now < today7am || now > today6pm)
            { return; }

            var idx = 0;
            var waferlist = WaferQUALVM.LoadToDoAllenWafers();
            foreach (var wf in waferlist)
            {
                var ret = LoadData(wf);
                if (ret)
                { idx = idx + 1; }
                if (idx >= 10)
                { break; }
            }
        }

        private static bool LoadData(string wafer)
        {
            var ret = new List<AllenEVALData>();

            var sql = @"SELECT left(d.[CONTAINER_NUMBER],9) as wafer ,d.[CONTAINER_NUMBER]
                       ,d.[TIME_STAMP] ,cast(c.status as varchar) as container_status
                       ,c.containertype
                         ,CASE WHEN pb.productname like '%[_]%' THEN LEFT(pf.productfamilyname,4)+right(pb.productname,len(pb.productname)-charindex('_',pb.productname))
                         WHEN pt.producttypename in ('VCSEL Eval AC') THEN LEFT(pf.productfamilyname,4)+'AC'
                         WHEN wb.WORKFLOWNAME like '%cob%' THEN LEFT(pf.productfamilyname,4)+'COB'
                         ELSE LEFT(pf.productfamilyname,4) END +'_'+ pb.productname as Product
                       ,pt.producttypename
                       ,left(d.CONTAINER_NUMBER,6) as fablot
                       ,rj.[Runnum]
                       ,stuff(rj.[Runnum],6,1,'-') as runseq
                       ,left(rj.runnum,1) as reactor
                       ,left(rj.runnum,5) as batch
                       ,case when rj.scribenumber like '%-%' then left(rj.scribenumber,charindex('-',rj.scribenumber)) else 'NA' end as boule
                       ,CASE WHEN ship.[Shipable] = 1 THEN 'Shippable' ELSE 'Not Shippable' END as [Shippable]
                       ,cast(d.[TOOL_NAME] as varchar) as TOOL_NAME

                       ,upper(d.COMMON_TEST_NAME) as COMMON_TEST_NAME
                       ,upper(spc.ParameterName) as ParameterName
                       ,spc.DCDDefName
                       ,WB.WORKFLOWNAME 
                       ,CASE WHEN wb.WORKFLOWNAME like '%WAT%' and c.ContainerName like '[0-9][0-9][0-9][0-9][0-9][0-9]-[0-9][0-9]E01%' THEN 'WAT'
                        WHEN wb.WORKFLOWNAME like '%WAT%' and c.ContainerName not like '[0-9][0-9][0-9][0-9][0-9][0-9]-[0-9][0-9]E01%' THEN 'BR'
                       WHEN wb.WORKFLOWNAME like '%ORT%' THEN 'ORT'
                       WHEN wb.WORKFLOWNAME like '%QUAL%' OR wb.WORKFLOWNAME like '%HAST%' THEN 'QUAL'
                       WHEN wb.WORKFLOWNAME like '%BR%' THEN 'BR'
                       ELSE 'WAT'
                       END as WORKFLOWTYPE
                       ,d.[TEST_VALUE]
                       ,spc.LowerSpecLimit
                       ,spc.UpperSpecLimit
                       ,pb.productname
                       ,spc.ProductName as spcproductname
                         ,CAST(d.READ_POINT as varchar) as READ_POINT
                         ,CAST(d.READ_POINT as int) as READ_POINT_NUM
                       ,d.[UNIT_NUMBER]
                       ,d.[DIE_NUMBER]
                       ,d.[X]
                       ,d.[Y]
                       ,W.WORKFLOWREVISION
                       ,d.[CONTAINER_NUMBER]+cast(d.[UNIT_NUMBER] as varchar) as container_unit
   
                      FROM [EngrData].[insite].[EVAL_Generic_DataCollection_Data] d with(nolock)

                      inner join insite.insite.container c with(nolock) on c.containername=d.CONTAINER_NUMBER
                      inner join insite.insite.product p with(nolock) on p.productid=c.productid
                      inner join insite.insite.productbase pb with(nolock) on pb.productbaseid=p.productbaseid
                      inner join insite.insite.productfamily pf with(nolock) on pf.productfamilyid=p.productfamilyid
                      inner join insite.insite.producttype pt with(nolock) on pt.producttypeid=p.producttypeid

                      inner join insite.insite.Rpt_ReleasedJob rj with (nolock) on rj.[ContainerName]=left(d.container_number,9) and rj.factory = 'WAFER'


                      inner join insite.insite.CurrentStatus cs  with (nolock) ON c.CurrentStatusId = cs.CurrentStatusId
                      inner join insite.insite.WorkflowStep ws with (nolock) on cs.WorkflowStepID = ws.WorkflowStepID 
                      inner join insite.insite.workflow w with (nolock) on ws.workflowid = w.workflowid
                      inner join insite.insite.workflowbase wb with (nolock) on w.workflowbaseid = wb.workflowbaseid

                      left join engrdata.dbo.[Eval_XY_Coordinates] xy with(nolock) on xy.container=c.containername and xy.devicenumber=d.UNIT_NUMBER


                       LEFT JOIN(
                        SELECT c.containername, cw.containername as wcar_name, wcar.description as wcar_cause FROM insite.insite.container c with(nolock) 
                        LEFT JOIN [Insite].[insite].[IssueActualsHistory] iah with(nolock) on iah.[FromContainerId]=c.containerid
                        INNER JOIN insite.insite.container cw with(nolock) on cw.containerid=iah.[toContainerId]
                        LEFT JOIN insite.insite.view_WCAR_Information wcar with(nolock) on wcar.containername=cw.containername

                        where cw.containername like '%wcar%'
                       ) wsq on wsq.containername=LEFT(d.Container_number,charindex('-',d.Container_number)+2)
   
                      left join 
                        (
                         SELECT
                          upper(ParameterName) as ParameterName
                          ,Eval_ProductName as ProductName
                          ,[DCDefName] as DCDDefName
                          ,max(wafer_ll) as LowerSpecLimit
                          ,min(wafer_ul) as UpperSpecLimit
                          ,max(dut_ll) as LowerControlLimit
                          ,min(dut_ul) as UpperControlLimit
     
                          ,CASE WHEN DCDefName like '%_rp%' THEN cast(right(DCDefName,len(DCDefName)-charindex('rp',DCDefName)-1) as int) 
                          ELSE NULL END as READ_POINT
           
                         FROM [EngrData].[insite].[Eval_Specs_Bin_PassFail] with(nolock)

                         WHERE DCDefName like '%Eval%_rp%'
         
                         GROUP BY upper(ParameterName)
                          ,Eval_ProductName
                          ,[DCDefName]
                          ,CASE WHEN DCDefName like '%_rp%' THEN cast(right(DCDefName,len(DCDefName)-charindex('rp',DCDefName)-1) as int) 
                          ELSE NULL END

                        ) spc on spc.ProductName=pb.productname
                        and spc.ParameterName=d.COMMON_TEST_NAME + '_rp' + right(cast(cast(right(dcddefname,len(dcddefname)-charindex('rp',dcddefname)-1) as int)+100 as varchar),2)
                        and spc.READ_POINT = d.READ_POINT
    
                      LEFT JOIN [Insite].[insite].[DataCollectionHistory] dch with(nolock) on dch.[HistoryMainlineId]=xy.[HistorymainlineID]
                      LEFT JOIN [Insite].[insite].[dc_Eval_50up_Exclusion] e with(nolock) on e.[HistoryMainlineId]=xy.[HistorymainlineID] and e.[ParameterName] = 'comments'
                      LEFT JOIN [EngrData].[dbo].[Wafer_Shipment] ship with(nolock) on ship.wafer=left(c.containername,9)

                      where left(c.containername,9) in ('<wafernum>') 
                       and (spc.LowerSpecLimit is not null or UpperSpecLimit is not null) and d.[TEST_VALUE] is not null
                       and (WB.WORKFLOWNAME like 'Eval_VCSEL_50up_4inc%' or WB.WORKFLOWNAME like 'Eval_VCSEL_50up_3inc%' 
					   or WB.WORKFLOWNAME like 'Eval_VCSEL_ubDH_COB_4inc%' or WB.WORKFLOWNAME like 'Eval_VCSEL_HASTbDH_COB_2inc%')
                      and exclusion = 0 order by d.[CONTAINER_NUMBER] ,d.[TIME_STAMP]";

            sql = sql.Replace("<wafernum>", wafer);
            var dbret = DBUtility.ExeAllenSqlWithRes(sql);
            foreach (var l in dbret)
            {
                var tempvm = new AllenEVALData(O2S(l[0]),O2S(l[1]), DT2S(l[2]), O2S(l[3]), O2S(l[4]), O2S(l[5]), O2S(l[6]), O2S(l[7]), O2S(l[8]), O2S(l[9])
                                , O2S(l[10]), O2S(l[11]), O2S(l[12]), O2S(l[13]), O2S(l[14]), O2S(l[15]), O2S(l[16]), O2S(l[17]), O2S(l[18]), O2S(l[19])
                                , DOB2S(l[20]), DOB2S(l[21]), DOB2S(l[22]), O2S(l[23]), O2S(l[24]), O2S(l[25]), O2S(l[26]), O2S(l[27]), O2S(l[28]), O2S(l[29])
                                , O2S(l[30]), O2S(l[31]), O2S(l[32]));
                ret.Add(tempvm);
            }

            if (ret.Count > 0)
            { CleanData(wafer); }

            foreach (var item in ret)
            { StoreData(item); }

            if (ret.Count > 0)
            { UpdateWaferQualStatus(wafer); }
            else
            { WaferQUALVM.UpdateAllenWATData("AllenNoData", "TRUE", wafer); }

            if (ret.Count > 0)
            { return true; }

            return false;
        }

        private static void UpdateWaferQualStatus(string wafer)
        {
            var dict = new Dictionary<string, string>();
            dict.Add("@WaferNum", wafer);

            var sql = "select ValueCheck from AllenEVALData where  WaferNum=@WaferNum and ValueCheck <> 'PASS'";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            if (dbret.Count > 0)
            { WaferQUALVM.UpdateAllenWATData("AllenValCheck", "FAIL",wafer); }
            else
            { WaferQUALVM.UpdateAllenWATData("AllenValCheck", "PASS", wafer); }

            sql = "select WATResult from AllenEVALData where  WaferNum=@WaferNum and WATResult <> 'PASS'";
            dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            if (dbret.Count > 0)
            { WaferQUALVM.UpdateAllenWATData("AllenWATResult", "FAIL", wafer); }
            else
            { WaferQUALVM.UpdateAllenWATData("AllenWATResult", "PASS", wafer); }
        }

        private static void CleanData(string wafer)
        {
            var sql = "delete from AllenEVALData where WaferNum=@WaferNum";
            var dict = new Dictionary<string, string>();
            dict.Add("@WaferNum", wafer);
            DBUtility.ExeLocalSqlNoRes(sql, dict);

        }

        private static void StoreData(AllenEVALData data)
        {
            var sql = @"insert into AllenEVALData(WaferNum,ContainerNum,TimeStamp,ContainerStat,ContainerType,Product,ProductTpNm,Fablot,Runnum,Runseq,Reactor,Batch,Boule,Shippable
                            ,ToolName,TestName,Parameter,DCDName,WorkFlow,WorkFlowType,TestValue,LowLimit,HighLimit,PN,SpcPN,RP,RPNum,UnitNum,DieNum,X,Y,WorkFlowRev,ContainerUnit,ValueCheck,WATResult) values(
                            @WaferNum,@ContainerNum,@TimeStamp,@ContainerStat,@ContainerType,@Product,@ProductTpNm,@Fablot,@Runnum,@Runseq,@Reactor,@Batch,@Boule,@Shippable
                           ,@ToolName,@TestName,@Parameter,@DCDName,@WorkFlow,@WorkFlowType,@TestValue,@LowLimit,@HighLimit,@PN,@SpcPN,@RP,@RPNum,@UnitNum,@DieNum,@X,@Y,@WorkFlowRev,@ContainerUnit,@ValueCheck,@WATResult)";
            var dict = new Dictionary<string, string>();
            dict.Add("@WaferNum", data.WaferNum);
            dict.Add("@ContainerNum", data.ContainerNum);
            dict.Add("@TimeStamp", data.TimeStamp);
            dict.Add("@ContainerStat", data.ContainerStat);
            dict.Add("@ContainerType", data.ContainerType);
            dict.Add("@Product", data.Product);
            dict.Add("@ProductTpNm", data.ProductTpNm);
            dict.Add("@Fablot", data.Fablot);
            dict.Add("@Runnum", data.Runnum);
            dict.Add("@Runseq", data.Runseq);
            dict.Add("@Reactor", data.Reactor);
            dict.Add("@Batch", data.Batch);
            dict.Add("@Boule", data.Boule);
            dict.Add("@Shippable", data.Shippable);
            dict.Add("@ToolName", data.ToolName);
            dict.Add("@TestName", data.TestName);
            dict.Add("@Parameter", data.Parameter);
            dict.Add("@DCDName", data.DCDName);
            dict.Add("@WorkFlow", data.WorkFlow);
            dict.Add("@WorkFlowType", data.WorkFlowType);
            dict.Add("@TestValue", data.TestValue);
            dict.Add("@LowLimit", data.LowLimit);
            dict.Add("@HighLimit", data.HighLimit);
            dict.Add("@PN", data.PN);
            dict.Add("@SpcPN", data.SpcPN);
            dict.Add("@RP", data.RP);
            dict.Add("@RPNum", data.RPNum);
            dict.Add("@UnitNum", data.UnitNum);
            dict.Add("@DieNum", data.DieNum);
            dict.Add("@X", data.X);
            dict.Add("@Y", data.Y);
            dict.Add("@WorkFlowRev", data.WorkFlowRev);
            dict.Add("@ContainerUnit", data.ContainerUnit);
            dict.Add("@ValueCheck", data.ValueCheck);
            dict.Add("@WATResult", data.WATResult);
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        private static string O2S(object obj)
        {
            if (obj == null)
            { return string.Empty; }

            try
            {
                return Convert.ToString(obj);
            }
            catch (Exception ex) { return string.Empty; }
        }

        private static string DOB2S(object obj)
        {
            if (obj == null)
            { return string.Empty; }

            try
            {
                var d = Convert.ToDouble(obj);
                return d.ToString();
            }
            catch (Exception ex) { return string.Empty; }
        }

        private static string DT2S(object obj)
        {
            if (obj == null)
            { return string.Empty; }

            try
            {
                var d = Convert.ToDateTime(obj);
                return d.ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch (Exception ex) { return string.Empty; }
        }

        public static List<AllenEVALData> RetriewAllenData(string wafer)
        {
            var ret = new List<AllenEVALData>();

            var sql = @"select [WaferNum],[ContainerNum],[X],[Y],[Product],[Parameter],[TestValue],[LowLimit],[HighLimit],[RPNum],[WorkFlow],[ValueCheck],[TimeStamp],[WATResult]
                        from [WAT].[dbo].[AllenEVALData] where WaferNum = @WaferNum  order by [ContainerNum],[TimeStamp] asc";
            var dict = new Dictionary<string, string>();
            dict.Add("@WaferNum", wafer);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                var tempvm = new AllenEVALData();
                tempvm.WaferNum = O2S(line[0]);
                tempvm.ContainerNum = O2S(line[1]);
                tempvm.X = O2S(line[2]);
                tempvm.Y = O2S(line[3]);
                tempvm.Product = O2S(line[4]);
                tempvm.Parameter = O2S(line[5]);
                tempvm.TestValue = Math.Round(Convert.ToDouble(line[6]),6).ToString();
                tempvm.LowLimit = O2S(line[7]);
                tempvm.HighLimit = O2S(line[8]);
                tempvm.RPNum = O2S(line[9]);
                tempvm.WorkFlow = O2S(line[10]);
                tempvm.ValueCheck = O2S(line[11]);
                tempvm.TimeStamp = Convert.ToDateTime(line[12]).ToString("yyyy-MM-dd HH:mm:ss");
                tempvm.WATResult = O2S(line[13]);
                ret.Add(tempvm);
            }
            return ret;
        }

        private void ApplyWATPassLogic()
        {
            ValueCheck = "PASS";
            WATResult = "PASS";
            var vl = Convert.ToDouble(TestValue);
            if (!string.IsNullOrEmpty(LowLimit) && !string.IsNullOrEmpty(HighLimit))
            {
                var ll = Convert.ToDouble(LowLimit);
                var hl = Convert.ToDouble(HighLimit);
                if (vl >= ll && vl <= hl) { }
                else
                {
                    ValueCheck = "FAIL";
                    if (!WorkFlow.Contains("HASTbDH"))
                    {
                        WATResult = "FAIL";
                    }
                }
            }
            else if (!string.IsNullOrEmpty(LowLimit))
            {
                var ll = Convert.ToDouble(LowLimit);
                if (vl >= ll) { }
                else
                {
                    ValueCheck = "FAIL";
                    if (!WorkFlow.Contains("HASTbDH"))
                    {
                        WATResult = "FAIL";
                    }
                }
            }
            else if (!string.IsNullOrEmpty(HighLimit))
            {
                var hl = Convert.ToDouble(HighLimit);
                if (vl <= hl) { }
                else
                {
                    ValueCheck = "FAIL";
                    if (!WorkFlow.Contains("HASTbDH"))
                    {
                        WATResult = "FAIL";
                    }
                }
            }
        }

        public AllenEVALData(string waf,string cn,string tm,string cs,string ct,string pd,string pdt,string fab,string rnum,string rseq,
                                string react,string bat,string bou,string ship,string tool,string tn,string param,string dcd,string wf,string wft,string val,
                                string low,string high,string pn,string spcpn,string rp,string rpn,string un,string die,string x,string y,string wfv,string cu) {
            WaferNum =   waf;
            ContainerNum =   cn;
            TimeStamp =   tm;
            ContainerStat =   cs;
            ContainerType =   ct;
            Product =   pd;
            ProductTpNm =   pdt;
            Fablot =   fab;
            Runnum =   rnum;
            Runseq =   rseq;
            Reactor =   react;
            Batch =   bat;
            Boule =   bou;
            Shippable =   ship;
            ToolName =   tool;
            TestName =   tn;
            Parameter =   param;
            DCDName =   dcd;
            WorkFlow =   wf;
            WorkFlowType =   wft;
            TestValue =   val;
            LowLimit =   low;
            HighLimit =   high;
            PN =   pn;
            SpcPN =   spcpn;
            RP =   rp;
            RPNum =   rpn;
            UnitNum =   un;
            DieNum =   die;
            X =   x;
            Y =   y;
            WorkFlowRev =   wfv;
            ContainerUnit =   cu;

            ApplyWATPassLogic();
        }

        public AllenEVALData()
        {
            WaferNum = "";
            ContainerNum = "";
            TimeStamp = "";
            ContainerStat = "";
            ContainerType = "";
            Product = "";
            ProductTpNm = "";
            Fablot = "";
            Runnum = "";
            Runseq = "";
            Reactor = "";
            Batch = "";
            Boule = "";
            Shippable = "";
            ToolName = "";
            TestName = "";
            Parameter = "";
            DCDName = "";
            WorkFlow = "";
            WorkFlowType = "";
            TestValue = "";
            LowLimit = "";
            HighLimit = "";
            PN = "";
            SpcPN = "";
            RP = "";
            RPNum = "";
            UnitNum = "";
            DieNum = "";
            X = "";
            Y = "";
            WorkFlowRev = "";
            ContainerUnit = "";
            ValueCheck = "";
            WATResult = "";
        }

        public string WaferNum { set; get; }
        public string ContainerNum { set; get; }
        public string TimeStamp { set; get; }
        public string ContainerStat { set; get; }
        public string ContainerType { set; get; }
        public string Product { set; get; }
        public string ProductTpNm { set; get; }
        public string Fablot { set; get; }
        public string Runnum { set; get; }
        public string Runseq { set; get; }
        public string Reactor { set; get; }
        public string Batch { set; get; }
        public string Boule { set; get; }
        public string Shippable { set; get; }
        public string ToolName { set; get; }
        public string TestName { set; get; }
        public string Parameter { set; get; }
        public string DCDName { set; get; }
        public string WorkFlow { set; get; }
        public string WorkFlowType { set; get; }
        public string TestValue { set; get; }
        public string LowLimit { set; get; }
        public string HighLimit { set; get; }
        public string PN { set; get; }
        public string SpcPN { set; get; }
        public string RP { set; get; }
        public string RPNum { set; get; }
        public string UnitNum { set; get; }
        public string DieNum { set; get; }
        public string X { set; get; }
        public string Y { set; get; }
        public string WorkFlowRev { set; get; }
        public string ContainerUnit { set; get; }
        public string ValueCheck { set; get; }
        public string WATResult { set; get; }

    }
}