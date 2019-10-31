using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using WXLogic;


namespace WAT.Models
{
    public class WATAnalyzeVM
    {
        public static string ConvertRP2RAWTestStep(string rp)
        {
            if (rp.Contains("RP01"))
            { return "PRLL_VCSEL_Post_Burn_in_Test"; }
            else if (rp.Contains("RP02"))
            { return "PRLL_Post_HTOL1_Test"; }
            else if (rp.Contains("RP03"))
            { return "PRLL_Post_HTOL2_Test"; }
            else if (rp.Contains("RP00"))
            { return "PRLL_VCSEL_Pre_Burn_in_Test"; }
            else
            { return ""; }
        }

        public static string ConvertRP2JudgementStep(string rp)
        {
            if (rp.Contains("RP01"))
            { return "POSTBIJUDGEMENT"; }
            else if (rp.Contains("RP02"))
            { return "POSTHTOL1JUDGEMENT"; }
            else if (rp.Contains("RP03"))
            { return "POSTHTOL2JUDGEMENT"; }
            else if (rp.Contains("RP00"))
            { return "PREBIJUDGEMENT"; }
            else
            { return ""; }
        }

        public static List<string> GetWATRawParamList()
        {
            var retlist = new List<string>();
            retlist.Add("BVR_LD_A");
            retlist.Add("PO_LD_W");
            retlist.Add("VF_LD_V");
            retlist.Add("SLOPE_WperA");
            retlist.Add("THOLD_A");
            retlist.Add("R_LD_ohm");
            retlist.Add("IMAX_A");
            return retlist;
        }

        public static List<string> GetWATLogicParamList()
        {
            var rawlist = GetWATRawParamList();

            var retlist = new List<string>();
            var sql = @"select distinct left([ParameterName],len([ParameterName])-5) from [EngrData].[insite].[Eval_Specs_Bin_PassFail] where DCDefName like 'Eval_50up%' 
                         and (ParameterName like '%_RP00' or ParameterName like '%_RP01' or ParameterName like '%_RP02' or ParameterName like '%_RP03') order by left([ParameterName],len([ParameterName])-5)";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var p = UT.O2S(line[0]);
                if (rawlist.Contains(p))
                { continue; }

                retlist.Add(p + "_LOGIC");
            }
            retlist.Add("THOLD_A_rd_LOGIC");

            retlist.Add("DIth");
            retlist.Add("DPO");
            retlist.Add("Dvf");
            retlist.Add("Assembly");
            retlist.Add("DPO_rd");
            return retlist;
        }

        public static string RealParam(string param)
        {
            if (string.Compare(param, "DIth", true) == 0)
            { return "THOLD_A_rd_LOGIC"; }
            if (string.Compare(param, "DPO", true) == 0)
            { return "PO_LD_W_dB_ref0_LOGIC"; }
            if (string.Compare(param, "Dvf", true) == 0)
            { return "VF_LD_V_ad_ref1_LOGIC"; }
            if (string.Compare(param, "Assembly", true) == 0)
            { return "R_LD_ohm_c-p_LOGIC"; }
            if (string.Compare(param, "DPO_rd", true) == 0)
            { return "PO_LD_W_rd_ref1_LOGIC"; }

            return param;
        }

        private static List<double> getWATSpecLimit_(string param, List<string> wflist)
        {
            var wafercond = "('" + string.Join("','", wflist) + "')";
            var ret = new List<double>();
            var sql = @"select distinct Wafer_LL,Wafer_UL from [EngrData].[insite].[Eval_Specs_Bin_PassFail] where ParameterName like '<param>%' 
                        and DCDefName like 'Eval_50up%' and Eval_ProductName in (select EvalPN from  EngrData.dbo.WXEvalPN where WaferNum in <wafercond> ) order by Wafer_LL,Wafer_UL desc";

            sql = sql.Replace("<param>",param).Replace("<wafercond>", wafercond);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql);

            if (dbret.Count == 0)
            {
                sql = @"select distinct Wafer_LL,Wafer_UL from [EngrData].[insite].[Eval_Specs_Bin_PassFail] where ParameterName like '<param>%' 
                        and DCDefName like 'Eval_50up%'";
                sql = sql.Replace("<param>", param);
                dbret = DBUtility.ExeLocalSqlWithRes(sql);
            }

            var minlist = new List<double>();
            var maxlist = new List<double>();
            foreach (var line in dbret)
            {
                if (line[0] == null || line[0] == DBNull.Value)
                { }
                else
                { minlist.Add(UT.O2D(line[0])); }

                if (line[1] == null || line[1] == DBNull.Value)
                { }
                else
                { maxlist.Add(UT.O2D(line[1])); }
            }

            if (minlist.Count > 0 && maxlist.Count > 0)
            {
                ret.Add(minlist.Min());
                ret.Add(maxlist.Max());
            }
            else if (minlist.Count > 0)
            {
                ret.Add(minlist.Min());
                ret.Add(99999.0);
            }
            else if(maxlist.Count > 0)
            {
                ret.Add(-99999.0);
                ret.Add(maxlist.Max());
            }

            return ret;
        }

        public static List<double> GetWATSpecLimit(string param, List<string> wflist)
        {
            var limit = getWATSpecLimit_(param, wflist);
            if (limit.Count == 0)
            { limit = getWATSpecLimit_(param.ToUpper().Split(new[] { "_REF" }, StringSplitOptions.RemoveEmptyEntries)[0],wflist); }

            if (limit.Count == 0)
            {
                limit.Add(-99999.0);
                limit.Add(99999.0);
            }

            return limit;
        }

        private static List<XYVAL> GetWUXIWATRawDataByWF(string param, string wf, string teststep,double lowrange,double highrange)
        {
            var samplexy = WXLogic.WATSampleXY.GetSampleXYByCouponGroup(wf);
            var samplexydict = new Dictionary<string, WXLogic.WATSampleXY>();
            foreach (var sitem in samplexy)
            {
                var key = sitem.CouponID + "-" + sitem.ChannelInfo;
                if (!samplexydict.ContainsKey(key))
                { samplexydict.Add(key, sitem); }
            }

            var containerdict = new Dictionary<string, bool>();

            var ret = new List<XYVAL>();
            var sql = "select <param>,Containername,ChannelInfo from Insite.dbo.ProductionResult where (Containername like '<coupongroup>E08%' or Containername like '<coupongroup>R08%') and TestStep='<TestStep>' order by TestTimeStamp desc";
            sql = sql.Replace("<param>", param).Replace("<coupongroup>", wf).Replace("<TestStep>", teststep);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                if (!string.IsNullOrEmpty(UT.O2S(line[0])))
                {
                    var val = UT.O2D(line[0]);
                    if (val > lowrange && val < highrange)
                    {
                        var Containername = UT.O2S(line[1]).Substring(0, 14);
                        var ChannelInfo = UT.O2S(line[2]);
                        var xykey = Containername + "-" + ChannelInfo;
                        if (containerdict.ContainsKey(xykey))
                        { continue; }
                        else
                        { containerdict.Add(xykey,true); }

                        if (samplexydict.ContainsKey(xykey))
                        {
                            var UnitNum = (UT.O2I(Containername.Substring(12, 2)) * 10000 + UT.O2I(ChannelInfo)).ToString();
                            var X = samplexydict[xykey].X;
                            var Y = samplexydict[xykey].Y;
                            ret.Add(new XYVAL(X,Y,UnitNum,val));
                        }
                    }
                }
            }
            return ret;
        }

        public static Dictionary<string, List<XYVAL>> GetWUXIWATRawData(string param, List<string> wflist, string rp, double lowrange, double highrange)
        {
            var ret = new Dictionary<string, List<XYVAL>>();
            var teststep = ConvertRP2RAWTestStep(rp);
            foreach (var wf in wflist)
            {
                var paramval = GetWUXIWATRawDataByWF(param, wf, teststep, lowrange, highrange);
                if (paramval.Count > 10)
                {
                    paramval.Sort(delegate (XYVAL obj1, XYVAL obj2) {
                        return obj1.Val.CompareTo(obj2.Val);
                    });
                    ret.Add(wf, paramval);
                }
            }
            return ret;
        }

        private static List<XYVAL> GetALLENWATRawDataByWF(string param, string wf, string RP, double lowrange, double highrange)
        {
            var rete01 = new List<XYVAL>();
            var rete08 = new List<XYVAL>();

            var sql = "";
            if (wf.ToUpper().Contains("E0"))
            {
                sql = @"SELECT  d.TEST_VALUE,d.[CONTAINER_NUMBER],UNIT_NUMBER ,X ,Y FROM [EngrData].[insite].[Get_EVAL_Data_By_CONTAINER_NUMBER_Link_PROBE_DATA] (nolock) d 
                        left join [EngrData].[dbo].[Eval_XY_Coordinates] (nolock) e on e.[DeviceNumber] = d.[UNIT_NUMBER] and e.[CONTAINER] = d.[CONTAINER_NUMBER]
                        where  ([CONTAINER_NUMBER]  like '<coupongroup>%') 
                        and READ_POINT = '<RP>' and COMMON_TEST_NAME= '<param>' and (e.exclusion <> 'true' and e.exclusion <> '1')";
            }
            else
            {
                sql = @"SELECT  d.TEST_VALUE,d.[CONTAINER_NUMBER],UNIT_NUMBER ,X ,Y FROM [EngrData].[insite].[Get_EVAL_Data_By_CONTAINER_NUMBER_Link_PROBE_DATA] (nolock) d 
                        left join [EngrData].[dbo].[Eval_XY_Coordinates] (nolock) e on e.[DeviceNumber] = d.[UNIT_NUMBER] and e.[CONTAINER] = d.[CONTAINER_NUMBER]
                        where  ([CONTAINER_NUMBER]  like '<coupongroup>E08%' or [CONTAINER_NUMBER]  like '<coupongroup>E01%') 
                        and READ_POINT = '<RP>' and COMMON_TEST_NAME= '<param>' and (e.exclusion <> 'true' and e.exclusion <> '1')";
            }

            sql = sql.Replace("<param>", param).Replace("<coupongroup>", wf).Replace("<RP>", RP);

            var dbret = DBUtility.ExeAllenSqlWithRes(sql);
            foreach (var line in dbret)
            {
                if (!string.IsNullOrEmpty(UT.O2S(line[0])))
                {
                    var val = UT.O2D(line[0]);
                    if (val > lowrange && val < highrange)
                    {
                        var unitnum = UT.O2S(line[2]);
                        var x = UT.O2S(line[3]);
                        var y = UT.O2S(line[4]);
                        if (UT.O2S(line[1]).ToUpper().Contains("E08"))
                        { rete08.Add(new XYVAL(x,y,unitnum,val)); }
                        else
                        { rete01.Add(new XYVAL(x, y, unitnum, val)); }
                    }
                }
            }

            if (rete08.Count > 10)
            { return rete08; }
            else
            { return rete01; }
        }

        public static List<string> GetE08SampleWaferList(List<string> wxwflist,int max)
        {
            var ret = new List<string>();

            var wafercond = "('" + string.Join("','", wxwflist) + "')";
            var sql = @"SELECT distinct d.[CONTAINER_NUMBER] FROM [EngrData].[insite].[Get_EVAL_Data_By_CONTAINER_NUMBER_Link_PROBE_DATA] (nolock) d 
                        left join insite.insite.Container c  (nolock) on d.[CONTAINER_NUMBER] = c.ContainerName
                        left join insite.insite.Product p  (nolock) on c.ProductID = P.ProductID
                        left join insite.insite.ProductFamily pf  (nolock) on p.ProductFamilyID = pf.ProductFamilyID
                        where d.[CONTAINER_NUMBER] like '%E08%' and d.READ_POINT = '3' and Left(pf.ProductFamilyName,4) in (
                        select distinct Left(bp.BinProdfam,4) from insite.insite.Container (nolock) cc 
                        left join insite.insite.Product pp  (nolock) on cc.ProductID = pP.ProductID
                        left join insite.insite.ProductFamily ppf  (nolock) on pp.ProductFamilyID = ppf.ProductFamilyID
                        left join [Insite].[insite].[BinJobPartnumbers] bp (nolock) on bp.Device =  Left(ppf.ProductFamilyName,4)
                        where cc.ContainerName in <wafercond> 
                        )  order by d.[CONTAINER_NUMBER] desc";
            sql = sql.Replace("<wafercond>", wafercond);
            var dbret = DBUtility.ExeAllenSqlWithRes(sql);
            if (dbret.Count == 0)
            {
                sql = @"SELECT distinct d.[CONTAINER_NUMBER] FROM [EngrData].[insite].[Get_EVAL_Data_By_CONTAINER_NUMBER_Link_PROBE_DATA] (nolock) d
                        where d.[CONTAINER_NUMBER] like '%E08%' and d.READ_POINT = '3' order by d.[CONTAINER_NUMBER] desc";
                dbret = DBUtility.ExeAllenSqlWithRes(sql);
            }

            var count = 0;
            foreach (var line in dbret)
            {
                ret.Add(UT.O2S(line[0]));

                count++;
                if (count >= max)
                { break; }
            }
            return ret;
        }

        public static Dictionary<string, List<XYVAL>> GetALLENWATRawData(string param, List<string> wflist, string rp, double lowrange, double highrange)
        {
            var ret = new Dictionary<string, List<XYVAL>>();
            foreach (var wf in wflist)
            {
                var paramval = GetALLENWATRawDataByWF(param, wf, rp.Replace("RP0",""), lowrange, highrange);
                if (paramval.Count > 10)
                {
                    paramval.Sort(delegate (XYVAL obj1, XYVAL obj2) {
                        return obj1.Val.CompareTo(obj2.Val);
                    });
                    ret.Add(wf, paramval);
                }
            }
            return ret;
        }


        private static List<XYVAL> GetWUXIWATLogicDataByWF(string param, string wf, string jstepname, double lowrange, double highrange)
        {
            var wxlogic = new WXLogic.WXWATLogic();
            wxlogic.AllowToMoveMapFile = false;
            wxlogic.AnalyzeParam = param.ToUpper();
            wxlogic.WATPassFail(wf+"E08", jstepname);

            var ret = new List<XYVAL>();
            foreach (var val in wxlogic.AnalyzeParamData)
            {
                if (val.Val > lowrange && val.Val < highrange)
                {
                    ret.Add(val);
                }
            }
            return ret;
        }

        public static Dictionary<string, List<XYVAL>> GetWUXIWATLogicData(string param, List<string> wflist, string rp, double lowrange, double highrange)
        {
            var ret = new Dictionary<string, List<XYVAL>>();
            var jstepname = ConvertRP2JudgementStep(rp);
            foreach (var wf in wflist)
            {
                var paramval = GetWUXIWATLogicDataByWF(param, wf, jstepname, lowrange, highrange);
                if (paramval.Count > 10)
                {
                    paramval.Sort(delegate(XYVAL obj1,XYVAL obj2) {
                        return obj1.Val.CompareTo(obj2.Val);
                    });
                    ret.Add(wf, paramval);
                }
            }
            return ret;
        }

        private static List<XYVAL> GetALLENWATLogicDataByWF(string param, string wf, string rp, double lowrange, double highrange)
        {
            var logicdata = new List<XYVAL>();

            if (wf.ToUpper().Contains("E0"))
            {
                var ret = AllenWATLogic.PassFail(wf, "Eval_50up_" + rp.ToLower(), false, false, param.ToUpper());
                logicdata.AddRange(ret.AnalyzeParamData);
            }
            else
            {
                var ret = AllenWATLogic.PassFail(wf+"E08AA", "Eval_50up_"+rp.ToLower(), false,false,param.ToUpper());
                if (ret.AnalyzeParamData.Count > 0)
                {
                    logicdata.AddRange(ret.AnalyzeParamData);
                    ret = AllenWATLogic.PassFail(wf + "E08AB", "Eval_50up_" + rp.ToLower(), false, false, param.ToUpper());
                    logicdata.AddRange(ret.AnalyzeParamData);
                }
                else
                {
                    ret = AllenWATLogic.PassFail(wf + "E01", "Eval_50up_" + rp.ToLower(), false, false, param.ToUpper());
                    logicdata.AddRange(ret.AnalyzeParamData);
                }
            }

            var retval = new List<XYVAL>();
            foreach (var val in logicdata)
            {
                if (val.Val > lowrange && val.Val < highrange)
                {
                    retval.Add(val);
                }
            }
            return retval;
        }

        public static Dictionary<string, List<XYVAL>> GetALLENWATLogicData(string param, List<string> wflist, string rp, double lowrange, double highrange)
        {
            var ret = new Dictionary<string, List<XYVAL>>();
            foreach (var wf in wflist)
            {
                var paramval = GetALLENWATLogicDataByWF(param.ToUpper(), wf, rp, lowrange, highrange);
                if (paramval.Count > 10)
                {
                    paramval.Sort(delegate (XYVAL obj1, XYVAL obj2) {
                        return obj1.Val.CompareTo(obj2.Val);
                    });
                    ret.Add(wf, paramval);
                }
            }
            return ret;
        }

        public static int Get_First_Singlet_From_Array_Coord(int DIE_ONE_X, int DIE_ONE_FIELD_MIN_X, int arrayx, int Array_Count)
        {
            var new_x = ((arrayx - DIE_ONE_X
                + Math.Floor((double)(DIE_ONE_X - DIE_ONE_FIELD_MIN_X) / Array_Count)) * Array_Count)
                + DIE_ONE_FIELD_MIN_X;
            if (Array_Count != 1)
            {
                return (int)Math.Round(new_x, 0) - 1;
            }
            else
            {
                return (int)Math.Round(new_x, 0);
            }
        }

        //public static List<int> GetDieOneOfWafer(string wafer,Controller ctrl)
        //{
        //    var ret = new List<int>();
        //    var syscfgdict = CfgUtility.GetSysConfig(ctrl);
        //    var reviewdir = syscfgdict["DIESORTREVIEW"];
        //    var fs = "";
        //    var allfiles = ExternalDataCollector.DirectoryEnumerateAllFiles(ctrl,reviewdir);
        //    foreach (var f in allfiles)
        //    {
        //        if (f.Contains(wafer))
        //        {
        //            fs = f;
        //            break;
        //        }
        //    }

        //    if (string.IsNullOrEmpty(fs))
        //    { return ret; }

        //    var folderuser = syscfgdict["SHAREFOLDERUSER"];
        //    var folderdomin = syscfgdict["SHAREFOLDERDOMIN"];
        //    var folderpwd = syscfgdict["SHAREFOLDERPWD"];

        //    using (NativeMethods cv = new NativeMethods(folderuser, folderdomin, folderpwd))
        //    {
        //        var doc = new XmlDocument();
        //        doc.Load(fs);
        //        var namesp = doc.DocumentElement.GetAttribute("xmlns");
        //        doc = DieSortVM.StripNamespace(doc);
        //        XmlElement root = doc.DocumentElement;
        //        var dieonenodelist = root.SelectNodes("//BinDefinition[@BinDescription='DIE_ONE']");
        //        var dieonebincode = "";
        //        foreach (XmlElement nd in dieonenodelist)
        //        {
        //            try
        //            {
        //                dieonebincode = nd.GetAttribute("BinCode");
        //                break;
        //            }
        //            catch (Exception ex) { }
        //        }

        //        if (string.IsNullOrEmpty(dieonebincode))
        //        { return ret; }

        //        var dieonex = "";
        //        var dieoney = "";

        //        var bincodelist = root.SelectNodes("//BinCode[@X and @Y]");
        //        foreach (XmlElement nd in bincodelist)
        //        {
        //            try
        //            {
        //                if (string.Compare(nd.InnerText, dieonebincode, true) == 0)
        //                {
        //                    dieonex = nd.GetAttribute("X");
        //                    dieoney = nd.GetAttribute("Y");
        //                    break;
        //                }
        //            }
        //            catch (Exception ex) { }
        //        }

        //        if (string.IsNullOrEmpty(dieonex) || string.IsNullOrEmpty(dieoney))
        //        { return ret; }

        //        var dieonexs = new List<int>();
        //        bincodelist = root.SelectNodes("//BinCode[@X and @Y='" + dieoney + "']");
        //        foreach (XmlElement nd in bincodelist)
        //        {
        //            try
        //            {
        //                dieonexs.Add(UT.O2I(nd.GetAttribute("X")));
        //            }
        //            catch (Exception ex) { }
        //        }

        //        if (dieonexs.Count == 0)
        //        { return ret; }

        //        ret.Add(UT.O2I(dieonex));
        //        ret.Add(dieonexs.Min());
        //    }

        //    return ret;
        //}

        public static Dictionary<string, bool> GetWaferCoordinate(string wafer,Controller ctrl)
        {
            var ret = new Dictionary<string, bool>();
            var array = WXLogic.WATSampleXY.GetArrayFromDieSort(wafer);
            if (string.IsNullOrEmpty(array))
            { array = WXLogic.WATSampleXY.GetArrayFromAllen(wafer); }
            if (string.IsNullOrEmpty(array))
            { return ret; }
            var arraysize = UT.O2I(array);

            var dieonex = AdminFileOperations.GetDieOneOfWafer(wafer);
            if (dieonex.Count == 0)
            { return ret; }

            var syscfgdict = CfgUtility.GetSysConfig(ctrl);
            var reviewfolder = syscfgdict["DIESORTREVIEW"];
            var allfiles = ExternalDataCollector.DirectoryEnumerateAllFiles(ctrl, reviewfolder);
            var fs = "";
            foreach (var f in allfiles)
            {
                if (f.Contains(wafer))
                {
                    fs = f;
                    break;
                }
            }

            if (string.IsNullOrEmpty(fs))
            {
                allfiles = DieSortVM.GetAllWaferFile(ctrl);
                foreach (var f in allfiles)
                {
                    if (f.Contains(wafer))
                    {
                        fs = f;
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(fs))
            { return ret; }

            var folderuser = syscfgdict["SHAREFOLDERUSER"];
            var folderdomin = syscfgdict["SHAREFOLDERDOMIN"];
            var folderpwd = syscfgdict["SHAREFOLDERPWD"];
            using (NativeMethods cv = new NativeMethods(folderuser, folderdomin, folderpwd))
            {
                var doc = new XmlDocument();
                doc.Load(fs);
                var namesp = doc.DocumentElement.GetAttribute("xmlns");
                doc = DieSortVM.StripNamespace(doc);
                XmlElement root = doc.DocumentElement;
                var bincodelist = root.SelectNodes("//BinCode[@X and @Y]");
                foreach (XmlElement nd in bincodelist)
                {
                    try
                    {
                        var ax = nd.GetAttribute("X");
                        var y = nd.GetAttribute("Y");
                        var x = Get_First_Singlet_From_Array_Coord(dieonex[0], dieonex[1], UT.O2I(ax), arraysize).ToString();

                        if (arraysize == 1)
                        {
                            var k = x + ":::" + y;
                            if (!ret.ContainsKey(k))
                            { ret.Add(k, true); }
                        }
                        else
                        {
                            for (var die = 0; die < arraysize; die++)
                            {
                                var k = (UT.O2I(x)+die) + ":::" + y;
                                if (!ret.ContainsKey(k))
                                { ret.Add(k, true); }
                            }
                        }
                    }
                    catch (Exception ex) { }
                }
            }
            return ret;
        }

    }

}