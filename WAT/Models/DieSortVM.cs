using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.IO;
using System.Web.Mvc;
using System.Text;
using System.Web.Caching;

namespace WAT.Models
{

    public class DieSortVM
    {
        public static List<string> GetAllWaferFile(Controller ctrl,string prodfam=null)
        {
            if (ctrl != null)
            {
                var cachelist = (List<string>)ctrl.HttpContext.Cache.Get("allwafermapfiles");
                if (cachelist != null)
                { return cachelist; }
            }


            var syscfgdict = CfgUtility.GetSysConfig(ctrl);
            var srcfolder = syscfgdict["DIESORTFOLDER"];
            if (!string.IsNullOrEmpty(prodfam))
            { srcfolder = srcfolder + "\\" + prodfam; }

            var allwafermapfiles = ExternalDataCollector.DirectoryEnumerateAllFiles(ctrl, srcfolder);

            if (ctrl != null && string.IsNullOrEmpty(prodfam))
            {
                if (ctrl.HttpContext.Cache != null)
                {
                    ctrl.HttpContext.Cache.Insert("allwafermapfiles", allwafermapfiles, null, DateTime.Now.AddHours(2), Cache.NoSlidingExpiration);
                }
            }

            return allwafermapfiles;
        }

        public static Dictionary<string,bool> GetInspectedWaferInPast5Days()
        {
            var ret = new Dictionary<string, bool>();

            var latestdate = DateTime.Now.AddDays(-5).ToString("yyyy-MM-dd HH:mm:ss");

            var sql = @"SELECT distinct [ParamValueString]
                          FROM [InsiteDB].[insite].[dc_IQC_InspectionResult] irs  with (nolock) 
                          left join InsiteDB.insite.DataCollectionHistory dch with (nolock) on irs.DataCollectionHistoryID = dch.DataCollectionHistoryId
                          left join InsiteDB.insite.Product pd with (nolock) on pd.ProductId = dch.ProductId 
                            where ParameterName = 'VendorLotNumber' and ParamValueString like '%-%' and ( right(left(ParamValueString,7),1) = '-' or right(left(ParamValueString,6),1) = '-' )
	                        and len(ParamValueString) > 8 and dch.TxnDate > '<latestdate>' and pd.Description like '%VCSEL%'";
            sql = sql.Replace("<latestdate>", latestdate);

            var dbret = DBUtility.ExeRealMESSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var wf = UT.O2S(line[0]);
                if (wf.Length == 12)
                {
                    wf = wf.Substring(0, 9);
                }
                else if (wf.Length > 12)
                {
                    wf = wf.Substring(0, 13);
                }
                else
                { continue; }

                if (!ret.ContainsKey(wf))
                { ret.Add(wf, true); }
            }

            return ret;
        }

        public static void ScanNewWafer(Controller ctrl)
        {
            var inspectedwafer = GetInspectedWaferInPast5Days();
            if (inspectedwafer.Count == 0)
            { return; }

            int wait = 0;
            foreach (var wf in inspectedwafer)
            {
                if (wf.Key.Length == 9)
                {
                    if (!Models.WXProbeData.AllenHasData(wf.Key))
                    {
                        Models.WXProbeData.AddProbeTrigge2Allen(wf.Key);
                        wait += 1;
                    }
                }
            }

            if (wait > 0)
            { new System.Threading.ManualResetEvent(false).WaitOne(5000*60*wait); }

            var filetype = "WAFER";
            var allwffiles = GetAllWaferFile(ctrl);

            var allwfdict = new Dictionary<string, string>();
            foreach (var f in allwffiles)
            {
                var wf = Path.GetFileNameWithoutExtension(f);
                if (inspectedwafer.ContainsKey(wf))
                {
                    var createtime = System.IO.File.GetLastWriteTime(f);
                    if (createtime > DateTime.Parse("2019-02-01 00:00:00"))
                    {
                        var uf = f.ToUpper();
                        if (!allwfdict.ContainsKey(uf))
                        { allwfdict.Add(uf, f); }
                    }
                }
            }

            var solvedwafer = FileLoadedData.LoadedFiles(filetype);
            var nofmwafer = FileLoadedData.LoadedFiles("NOFAMILY");
            var noprobewafer = FileLoadedData.LoadedFiles("NOPROBE");

            foreach (var wkv in allwfdict)
            {
                var wf = Path.GetFileNameWithoutExtension(wkv.Key).ToUpper();
                if (!solvedwafer.ContainsKey(wf) 
                    && !nofmwafer.ContainsKey(wf) 
                    && !noprobewafer.ContainsKey(wf))
                {
                    if (SolveANewWafer(wf, allwffiles, ctrl, "", false))
                    {
                        solvedwafer.Add(wf, true);
                    }
                    else
                    {
                        solvedwafer.Add(wf, true);
                    }
                }
            }

        }

        public static bool SolveANewWafer(string wafer, List<string> allwffiles,Controller ctrl,string offeredpn,bool supply)
        {
            FileLoadedData.RemoveLoadedFile(wafer, "");

            var syscfgdict = CfgUtility.GetSysConfig(ctrl);
            var towho = syscfgdict["DIESORTWARINGLIST"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var desfolder = syscfgdict["DIESORTSHARE"];
            var reviewfolder = syscfgdict["DIESORTREVIEW"];

            //check wafer's product family
            var productfm = "";
            var sixinch = false;
            if (wafer.Length == 13)
            {
                productfm = WXEvalPN.GetProductFamilyFromSherman(wafer);
                if (!string.IsNullOrEmpty(productfm))
                { sixinch = true; }
                else
                {
                    FileLoadedData.UpdateLoadedFile(wafer, "NOFAMILY");
                    if (!WebLog.CheckEmailRecord(wafer, "EM-PRODFM"))
                    {
                        EmailUtility.SendEmail(ctrl, "DIE SORT WAFER FATAL ERROR-" + wafer, towho, "Detail: Fail to get product family by wafer " + wafer);
                        new System.Threading.ManualResetEvent(false).WaitOne(300);
                    }

                    WebLog.Log(wafer, "DIESORT", "fail to to get product family by wafer " + wafer);
                    return false;
                }
            }
            else
            {
                productfm = WXEvalPN.GetProductFamilyFromAllen(wafer);
                if (string.IsNullOrEmpty(productfm))
                {
                    productfm = WXEvalPN.GetProductFamilyFromSherman(wafer);
                    if (!string.IsNullOrEmpty(productfm))
                    { sixinch = true; }
                    else
                    {
                        FileLoadedData.UpdateLoadedFile(wafer, "NOFAMILY");
                        if (!WebLog.CheckEmailRecord(wafer, "EM-PRODFM"))
                        {
                            EmailUtility.SendEmail(ctrl, "DIE SORT WAFER FATAL ERROR-"+wafer, towho, "Detail: Fail to get product family by wafer " + wafer);
                            new System.Threading.ManualResetEvent(false).WaitOne(300);
                        }
                    
                        WebLog.Log(wafer, "DIESORT", "fail to to get product family by wafer " + wafer);
                        return false;
                    }
                }
            }


            //get wafer's array info
            var waferarray = GetWaferArrayInfo(productfm, sixinch);
            if (string.IsNullOrEmpty(waferarray))
            {
                if (!WebLog.CheckEmailRecord(wafer, "EM-WAFARRAY"))
                {
                    EmailUtility.SendEmail(ctrl, "DIE SORT WAFER FATAL ERROR-" + wafer, towho, "Detail: Fail to get wafer array by wafer " + wafer);
                    new System.Threading.ManualResetEvent(false).WaitOne(300);
                }

                WebLog.Log(wafer, "DIESORT", "fail to to get array by wafer " + wafer);
                return false;
            }


            //get wafer's sample count infor
            var SampleCountKey = "";
            if (sixinch)
            { SampleCountKey = "DIESORTSAMPLE1X" + waferarray + "_6INCH"; }
            else
            { SampleCountKey = "DIESORTSAMPLE1X" + waferarray + "_4INCH"; }
            if (!syscfgdict.ContainsKey(SampleCountKey))
            {
                if (!WebLog.CheckEmailRecord(wafer, "EM-CFG"))
                {
                    EmailUtility.SendEmail(ctrl, "DIE SORT WAFER FATAL ERROR-" + wafer, towho, "Detail: Fail to get configure by sample count key " + SampleCountKey);
                    new System.Threading.ManualResetEvent(false).WaitOne(300);
                }

                WebLog.Log(wafer, "DIESORT", "fail to get configure by sample count key " + SampleCountKey);
                return false;
            }
            var SampleCount = O2I(syscfgdict[SampleCountKey]);
            var arraySTR = "1X" + waferarray;

            //prepare eval_pn and probe test data
            if (!PrepareData4WAT(wafer))
            {
                FileLoadedData.UpdateLoadedFile(wafer,"NOPROBE");

                if (!WebLog.CheckEmailRecord(wafer, "EM-WAT"))
                {
                    EmailUtility.SendEmail(ctrl, "DIE SORT WAFER FATAL ERROR-" + wafer, towho, "Detail: Fail to prepare WAT data by wafer " + wafer);
                    new System.Threading.ManualResetEvent(false).WaitOne(300);
                }

                WebLog.Log(wafer, "DIESORT", " fail to prepare WAT probe test data by wafer " + wafer);
                return false;
            }

            if (wafer.Length == 13)
            {
                return SixInchDieSort(wafer, allwffiles, ctrl, offeredpn, supply, productfm, waferarray, syscfgdict, towho, SampleCount, arraySTR, reviewfolder, desfolder);
            }
            else
            {
                return FourInchDieSort(wafer,allwffiles, ctrl, offeredpn, supply, productfm, waferarray, syscfgdict, towho, SampleCount , arraySTR, reviewfolder, desfolder);
            }
        }

        private static bool FourInchDieSort(string wafer, List<string> allwffiles, Controller ctrl, string offeredpn, bool supply
            ,string productfm,string waferarray,Dictionary<string,string> syscfgdict,List<string> towho,int SampleCount
            ,string arraySTR,string reviewfolder,string desfolder)
        {
            //get the actual wafer file
            var swaferfile = "";
            foreach (var f in allwffiles)
            {
                var uf = f.ToUpper();
                if (uf.Contains(productfm.ToUpper()) && uf.Contains(wafer.ToUpper()))
                {
                    swaferfile = f;
                    break;
                }
            }

            if (string.IsNullOrEmpty(swaferfile))
            {
                foreach (var f in allwffiles)
                {
                    var uf = f.ToUpper();
                    if (uf.Contains(wafer.ToUpper()))
                    {
                        swaferfile = f;
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(swaferfile))
            {
                WebLog.Log(wafer, "DIESORT", " map file does not exist: " + swaferfile);
                return false;
            }


            var waferfile = ExternalDataCollector.DownloadShareFile(swaferfile, ctrl);
            if (waferfile == null)
            {
                WebLog.Log(wafer, "DIESORT", " fail to download map file: " + swaferfile);
                return false;
            }

            try
            {
                //get modify information
                var doc = new XmlDocument();
                doc.Load(waferfile);
                var namesp = doc.DocumentElement.GetAttribute("xmlns");
                doc = StripNamespace(doc);
                XmlElement root = doc.DocumentElement;

                if (!CheckSourceFile(root, ctrl))
                { return false; }

                var bomdesc = "";
                var bompn = "";
                var fpn = "";
                var product = "";

                if (wafer.Length == 13)
                {

                }
                else
                {
                    var pnlist = GetPnFromWafer(wafer, offeredpn);
                    if (pnlist.Count > 0)
                    {
                        var vcselinfo = GetVCSELInfoFromPNList(wafer, pnlist);
                        if (vcselinfo.Count > 0)
                        {
                            bomdesc = System.Security.SecurityElement.Escape(vcselinfo[0]);
                            bompn = vcselinfo[1];
                            fpn = vcselinfo[2];
                            product = System.Security.SecurityElement.Escape(vcselinfo[3]);
                        }
                    }
                }

                var bin57dict = new Dictionary<string, string>();
                var bin57countkey = "DIESORTSAMPLEBIN57X" + waferarray;
                if (syscfgdict.ContainsKey(bin57countkey))
                {
                    var samplecount = UT.O2I(syscfgdict[bin57countkey]);
                    bin57dict = GetBin57Dict(root, samplecount);
                }

                var passedbinxydict = GetPassedBinXYDict(root);
                if (passedbinxydict.Count == 0)
                {
                    if (!WebLog.CheckEmailRecord(wafer, "EM-PASSBIN"))
                    {
                        EmailUtility.SendEmail(ctrl, "DIE SORT SOURCE FILE " + wafer + " FATAL ERROR", towho, "Detail: Fail to get good bin xy dict");
                        new System.Threading.ManualResetEvent(false).WaitOne(300);
                    }

                    WebLog.Log(wafer, "DIESORT", "fail to convert file:" + waferfile + ", fail to get good bin xy dict");
                    return false;
                }

                var selectxydict = GetSelectedXYDict(passedbinxydict, SampleCount, bin57dict);
                if (selectxydict.Count == 0)
                {
                    if (!WebLog.CheckEmailRecord(wafer, "EM-SELECT"))
                    {
                        EmailUtility.SendEmail(ctrl, "DIE SORT SOURCE FILE " + wafer + " FATAL ERROR", towho, "Detail: Fail to get sample xy dict");
                        new System.Threading.ManualResetEvent(false).WaitOne(300);
                    }

                    WebLog.Log(wafer, "DIESORT", "fail to convert file:" + waferfile + ", fail to get sample xy dict");
                    return false;
                }


                //try to write review file
                foreach (var kv in selectxydict)
                {
                    var xystr = kv.Key.Split(new string[] { ":::" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (XmlElement nd in root.SelectNodes("//BinCode[@X='" + xystr[0] + "' and @Y='" + xystr[1] + "']"))
                    { nd.SetAttribute("diesort", "selected"); }
                }

                foreach (var kv in bin57dict)
                {
                    var xystr = kv.Key.Split(new string[] { ":::" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (XmlElement nd in root.SelectNodes("//BinCode[@X='" + xystr[0] + "' and @Y='" + xystr[1] + "']"))
                    { nd.SetAttribute("diesort", "selected2"); }
                }

                var layoutnodes = root.SelectNodes("//Layout[@LayoutId]");
                if (layoutnodes.Count > 0)
                {
                    ((XmlElement)layoutnodes[0]).SetAttribute("BOMPN", bompn);
                    ((XmlElement)layoutnodes[0]).SetAttribute("FPN", fpn);
                    ((XmlElement)layoutnodes[0]).SetAttribute("PRODUCT", product);
                    ((XmlElement)layoutnodes[0]).SetAttribute("ARRAY", arraySTR);
                    ((XmlElement)layoutnodes[0]).SetAttribute("BOMDES", bomdesc);
                }

                doc.DocumentElement.SetAttribute("xmlns", namesp);
                var savename = Path.Combine(reviewfolder, Path.GetFileName(waferfile));
                if (ExternalDataCollector.FileExist(ctrl, savename))
                { ExternalDataCollector.FileDelete(ctrl, savename); }
                doc.Save(savename);



                //try to write actual file
                doc = new XmlDocument();
                doc.Load(waferfile);
                doc = StripNamespace(doc);
                root = doc.DocumentElement;

                var bin99count = 0;
                var allbincodelist = root.SelectNodes("//BinCode[@X and @Y]");
                foreach (XmlElement nd in allbincodelist)
                {
                    var binnum = UT.O2I(nd.InnerText);
                    if ((binnum >= 30 && binnum <= 39))
                    { }
                    else
                    {
                        nd.InnerText = "99";
                        bin99count += 1;
                    }
                }
                bin99count = bin99count - selectxydict.Count - bin57dict.Count;

                foreach (var kv in selectxydict)
                {
                    var xystr = kv.Key.Split(new string[] { ":::" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (XmlElement nd in root.SelectNodes("//BinCode[@X='" + xystr[0] + "' and @Y='" + xystr[1] + "']"))
                    {
                        nd.InnerText = "1";
                    }
                }

                foreach (var kv in bin57dict)
                {
                    var xystr = kv.Key.Split(new string[] { ":::" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (XmlElement nd in root.SelectNodes("//BinCode[@X='" + xystr[0] + "' and @Y='" + xystr[1] + "']"))
                    {
                        nd.InnerText = "2";
                    }
                }

                var defpnodelist = root.SelectNodes("//BinDefinitions");
                var defpnode = defpnodelist.Item(0);
                var alldefnodes = root.SelectNodes("//BinDefinition[@BinCode]");
                var clonenode = alldefnodes.Item(0).Clone();
                foreach (XmlElement dn in alldefnodes)
                {
                    var binnum = UT.O2I(dn.GetAttribute("BinCode"));
                    if (binnum < 30 || binnum > 39)
                    { defpnode.RemoveChild(dn); }
                }

                XmlElement bin99node = doc.CreateElement("BinDefinition");
                bin99node.SetAttribute("BinCode", "99");
                bin99node.SetAttribute("BinCount", bin99count.ToString());
                bin99node.SetAttribute("BinDescription", "Bin_99");
                bin99node.SetAttribute("Pick", "false");
                defpnode.AppendChild(bin99node);

                XmlElement binode = doc.CreateElement("BinDefinition");
                binode.SetAttribute("BinCode", "1");
                binode.SetAttribute("BinCount", selectxydict.Count.ToString());
                binode.SetAttribute("BinDescription", "GOOD1");
                binode.SetAttribute("Pick", "true");
                defpnode.AppendChild(binode);

                if (bin57dict.Count > 0)
                {
                    XmlElement bin57node = doc.CreateElement("BinDefinition");
                    bin57node.SetAttribute("BinCode", "2");
                    bin57node.SetAttribute("BinCount", bin57dict.Count.ToString());
                    bin57node.SetAttribute("BinDescription", "GOOD2");
                    bin57node.SetAttribute("Pick", "true");
                    defpnode.AppendChild(bin57node);
                }

                //layoutnodes = root.SelectNodes("//Layout[@LayoutId]");
                //if (layoutnodes.Count > 0)
                //{
                //    ((XmlElement)layoutnodes[0]).SetAttribute("BOMPN", bompn);
                //    ((XmlElement)layoutnodes[0]).SetAttribute("FPN", fpn);
                //    ((XmlElement)layoutnodes[0]).SetAttribute("PRODUCT", product);
                //    ((XmlElement)layoutnodes[0]).SetAttribute("ARRAY", arraySTR);
                //    ((XmlElement)layoutnodes[0]).SetAttribute("BOMDES", bomdesc);
                //}

                doc.DocumentElement.SetAttribute("xmlns", namesp);
                savename = Path.Combine(desfolder, Path.GetFileName(waferfile));
                if (ExternalDataCollector.FileExist(ctrl, savename))
                { ExternalDataCollector.FileDelete(ctrl, savename); }
                doc.Save(savename);


                ////write sample X,Y csv file for sharing
                //var sb = new StringBuilder();
                //try
                //{
                //    var csvfile = Path.Combine(reviewfolder, Path.GetFileName(waferfile) + ".csv");
                //    if (ExternalDataCollector.FileExist(ctrl, csvfile))
                //    { ExternalDataCollector.FileDelete(ctrl, csvfile); }

                //    sb.Append("X,Y,BIN,\r\n");
                //    foreach (var kv in selectxydict)
                //    {
                //        sb.Append(kv.Key.Replace(":::", ",") + "," + kv.Value + ",\r\n");
                //    }
                //    File.WriteAllText(csvfile, sb.ToString());
                //}
                //catch (Exception e)
                //{
                //    WebLog.Log(wafer, "DIESORT", "fail to convert file:" + waferfile + " reason:" + e.Message);
                //    try
                //    {
                //        var csvfile = Path.Combine(reviewfolder, Path.GetFileName(waferfile) + "-new.csv");
                //        File.WriteAllText(csvfile, sb.ToString());
                //    }
                //    catch (Exception f) { WebLog.Log(wafer, "DIESORT", "fail to convert file:" + waferfile + " reason:" + f.Message); }
                //}

                var mapfile = Path.GetFileName(waferfile);
                //write sample X,Y database
                StoreWaferSampleData(mapfile, wafer, selectxydict, bin57dict, arraySTR, bompn, fpn, supply);

                //write wafer related data
                var allselectdict = new Dictionary<string, string>();
                foreach (var s in selectxydict)
                { allselectdict.Add(s.Key, s.Value); }
                foreach (var s in bin57dict)
                { allselectdict.Add(s.Key, s.Value); }
                StoreWaferPassBinData(mapfile, wafer, allselectdict, passedbinxydict, arraySTR, bompn, fpn, bomdesc, product);

                FileLoadedData.UpdateLoadedFile(wafer, "WAFER");

            }
            catch (Exception ex) { return false; }

            return true;
        }

        private static bool SixInchDieSort(string wafer, List<string> allwffiles, Controller ctrl, string offeredpn, bool supply
            , string productfm, string waferarray, Dictionary<string, string> syscfgdict, List<string> towho, int SampleCount
            , string arraySTR, string reviewfolder, string desfolder)
        {
            //get the actual wafer file

            var smapfile = SixInchMapFileData.LoadMapFileData(wafer);

            if (smapfile.BaseArrayCoordinate.Count == 0)
            {
                WebLog.Log(wafer, "DIESORT", " map file does not exist: " + wafer);
                return false;
            }

            try
            {
                var bomdesc = "";
                var bompn = "";
                var fpn = "";
                var product = "";

                if (smapfile.BinArrayCoordinate.Count == 0)
                {
                    if (!WebLog.CheckEmailRecord(wafer, "EM-PASSBIN"))
                    {
                        EmailUtility.SendEmail(ctrl, "DIE SORT SOURCE FILE " + wafer + " FATAL ERROR", towho, "Detail: Fail to get good bin xy dict");
                        new System.Threading.ManualResetEvent(false).WaitOne(300);
                    }

                    WebLog.Log(wafer, "DIESORT", "fail to convert :" + wafer + ", fail to get good bin xy dict");
                    return false;
                }

                var selectxydict = smapfile.GetSampleDict(SampleCount);
                if (selectxydict.Count == 0)
                {
                    if (!WebLog.CheckEmailRecord(wafer, "EM-SELECT"))
                    {
                        EmailUtility.SendEmail(ctrl, "DIE SORT SOURCE FILE " + wafer + " FATAL ERROR", towho, "Detail: Fail to get sample xy dict");
                        new System.Threading.ManualResetEvent(false).WaitOne(300);
                    }

                    WebLog.Log(wafer, "DIESORT", "fail to convert :" + wafer + ", fail to get sample xy dict");
                    return false;
                }

                var reviewfile = Path.Combine(reviewfolder, wafer+".xml");
                var diesortnewfile = Path.Combine(desfolder, wafer + ".xml");
                if (ExternalDataCollector.FileExist(ctrl, reviewfile))
                { ExternalDataCollector.FileDelete(ctrl, reviewfile); }
                if (ExternalDataCollector.FileExist(ctrl, diesortnewfile))
                { ExternalDataCollector.FileDelete(ctrl, diesortnewfile); }
                smapfile.GenerateMapFile(selectxydict, reviewfile, diesortnewfile);

                var bin57dict = new Dictionary<string, string>();
                //write sample X,Y database
                StoreWaferSampleData(wafer, wafer, selectxydict, bin57dict, arraySTR, bompn, fpn, supply);

                //write wafer related data
                var allselectdict = new Dictionary<string, string>();
                foreach (var s in selectxydict)
                { allselectdict.Add(s.Key, s.Value); }
                var passedbinxydict = new Dictionary<string, string>();
                foreach (var bc in smapfile.BinArrayCoordinate)
                {
                    var key = UT.O2S(bc.X) + ":::" + UT.O2S(bc.Y);
                    if (!passedbinxydict.ContainsKey(key))
                    { passedbinxydict.Add(key,bc.Bin.ToString()); }
                }

                StoreWaferPassBinData(wafer, wafer, allselectdict, passedbinxydict, arraySTR, bompn, fpn, bomdesc, product);

                FileLoadedData.UpdateLoadedFile(wafer, "WAFER");
            }
            catch (Exception ex) { return false; }

            return true;
        }



        public static string ConvertBinMapFileData(string wf, string fbin, string tbin, Controller ctrl)
        {
            var syscfg = CfgUtility.GetSysConfig(ctrl);
            var fs = "";

            var pct100fs = ExternalDataCollector.DirectoryEnumerateFiles(ctrl, syscfg["DIESORT100PCT"]);
            foreach (var f in pct100fs)
            {
                if (f.Contains(wf))
                {
                    fs = f;
                    break;
                }
            }

            if (string.IsNullOrEmpty(fs))
            {
                var productfm = "";
                if (wf.Length == 13)
                {
                    productfm = WXEvalPN.GetProductFamilyFromSherman(wf);
                }
                else
                {
                    productfm = WXEvalPN.GetProductFamilyFromAllen(wf);
                    if (string.IsNullOrEmpty(productfm))
                    {
                        productfm = WXEvalPN.GetProductFamilyFromSherman(wf);
                    }
                }

                var allfilelist = new List<string>();
                if (string.IsNullOrEmpty(productfm))
                {
                    allfilelist = DieSortVM.GetAllWaferFile(ctrl);
                }
                else
                {

                    var srcfolder = syscfg["DIESORTFOLDER"] + "\\" + productfm;
                    allfilelist = ExternalDataCollector.DirectoryEnumerateAllFiles(ctrl, srcfolder);
                }

                foreach (var f in allfilelist)
                {
                    if (f.Contains(wf))
                    {
                        fs = f;
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(fs))
            { return "Fail to find original map from all system!"; }

            var doc = new XmlDocument();
            doc.Load(fs);
            var namesp = doc.DocumentElement.GetAttribute("xmlns");
            doc = StripNamespace(doc);
            var root = doc.DocumentElement;
            var allbincodelist = root.SelectNodes("//BinCode[@X and @Y]");
            foreach (XmlElement nd in allbincodelist)
            {
                if (string.Compare(nd.InnerText, fbin, true) == 0)
                {
                    nd.InnerText = tbin;
                    nd.SetAttribute("ORG", fbin);
                }
            }

            doc.DocumentElement.SetAttribute("xmlns", namesp);
            var savename = Path.Combine(syscfg["DIESORT100PCT"], Path.GetFileName(fs));
            if (ExternalDataCollector.FileExist(ctrl, savename))
            { ExternalDataCollector.FileDelete(ctrl, savename); }
            doc.Save(savename);

            return string.Empty;
        }

        public static string GetWaferArrayInfo(string productfm, bool sixinch)
        {
            var dict = new Dictionary<string, string>();
            dict.Add("@productfm", productfm);

            if (!sixinch)
            {
                var sql = @"select na.Array_Length from  [EngrData].[dbo].[NeoMAP_MWR_Arrays] na with (nolock) where na.product_out = @productfm";
                var dbret = DBUtility.ExeAllenSqlWithRes(sql, dict);
                foreach (var line in dbret)
                {
                    if (line[0] != System.DBNull.Value)
                    {
                        return O2S(line[0]);
                    }
                }
                return "1";
            }
            else
            {
                var sql = @"SELECT ARRAY_COUNT_X FROM [ShermanData].[dbo].[PRODUCT_VIEW] WITH (NOLOCK) WHERE PRODUCT_FAMILY = @productfm";
                var dbret = DBUtility.ExeShermanSqlWithRes(sql, dict);
                foreach (var line in dbret)
                {
                    if (line[0] != System.DBNull.Value)
                    {
                        return O2S(line[0]);
                    }
                }
                return "1";
            }

        }


        public static void CleanWaferSrcData(string fs)
        {
            var sql = "delete from WaferSrcData where MAPFILE = @MAPFILE";
            var dict = new Dictionary<string, string>();
            dict.Add("@MAPFILE", fs);
            DBUtility.ExeLocalSqlNoRes(sql,dict);
        }

        public static void StoreWaferSrcData(string MAPFILE, string WAFER, string BinCode
            , string BinCount, string BinQuality, string BinDescription, string Pick, string Yield,string LayoutId,string BinRate)
        {
            var sql = @"insert into WaferSrcData(MAPFILE,WAFER,BinCode,BinCount,BinQuality,BinDescription,Pick,Yield,LayoutId,BinRate,UpdateTime) 
                         values(@MAPFILE,@WAFER,@BinCode,@BinCount,@BinQuality,@BinDescription,@Pick,@Yield,@LayoutId,@BinRate,@UpdateTime)";
            var dict = new Dictionary<string, string>();
            dict.Add("@MAPFILE", MAPFILE);
            dict.Add("@WAFER", WAFER);
            dict.Add("@BinCode", BinCode);
            dict.Add("@BinCount", BinCount);
            dict.Add("@BinQuality", BinQuality);
            dict.Add("@BinDescription", BinDescription);
            dict.Add("@Pick", Pick);
            dict.Add("@Yield", Yield);
            dict.Add("@LayoutId", LayoutId);
            dict.Add("@BinRate", BinRate);
            dict.Add("@UpdateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }


        private static bool CheckSourceFile(XmlElement root, Controller ctrl)
        {
            var syscfgdict = CfgUtility.GetSysConfig(ctrl);
            var towho = syscfgdict["DIESORTWARINGLIST"].Split(new string[] { ";" },StringSplitOptions.RemoveEmptyEntries).ToList();

            var wafer = "";
            var wfnodes = root.SelectNodes("//Substrate[@SubstrateId]");
            if (wfnodes.Count > 0)
            {
                wafer = ((XmlElement)wfnodes[0]).GetAttribute("SubstrateId").Trim();
            }

            var layoutid = "";
            var layoutnodes = root.SelectNodes("//Layout[@LayoutId]");
            if (layoutnodes.Count > 0)
            {
                layoutid = ((XmlElement)layoutnodes[0]).GetAttribute("LayoutId").Trim();
            }

            var goodbin = 0;
            var allbin = 0;

            var binnodes = root.SelectNodes("//BinDefinition[@BinCode and @BinCount and @BinQuality and @BinDescription and @Pick]");
            foreach (XmlElement node in binnodes)
            {
                try
                {
                    var BinCount = node.GetAttribute("BinCount");
                    var BinQuality = node.GetAttribute("BinQuality");
                    allbin += Convert.ToInt32(BinCount);
                    if (string.Compare(BinQuality, "pass", true) == 0)
                    { goodbin += Convert.ToInt32(BinCount); }
                }
                catch (Exception ex) { }
            }

            if (allbin == 0)
            {
                if (!WebLog.CheckEmailRecord(wafer, "EM-BINCOUNT0"))
                {
                    EmailUtility.SendEmail(ctrl, "DIE SORT SOURCE FILE " + wafer + " FATAL ERROR", towho, "Detail: Failed to get any Bin count");
                    new System.Threading.ManualResetEvent(false).WaitOne(300);
                }
                WebLog.Log(wafer, "DIESORT", "fail to check source file,all bin count is 0!");
                return false;
            }

            var yield = (double)goodbin / (double)allbin * 100.0;
            var yieldstr = Math.Round(yield, 2).ToString();
            if (yield < 30.0)
            {
                if (!WebLog.CheckEmailRecord(wafer, "EM-YIELD"))
                {
                    EmailUtility.SendEmail(ctrl, "DIE SORT SOURCE FILE " + wafer + " WARNING", towho, "Detail:  Wafer yield is less than 30%, it is " + yieldstr + "%");
                    new System.Threading.ManualResetEvent(false).WaitOne(300);
                }
            }


            CleanWaferSrcData(wafer);

            foreach (XmlElement node in binnodes)
            {
                try
                {
                    var BinCode = node.GetAttribute("BinCode");
                    var BinCount = node.GetAttribute("BinCount");
                    var BinQuality = node.GetAttribute("BinQuality");
                    var BinDescription = node.GetAttribute("BinDescription");
                    var Pick = node.GetAttribute("Pick");
                        
                    var c = Convert.ToInt32(BinCount);
                    var binrate = Math.Round( (double)c / (double)allbin * 100.0,2).ToString();
                    StoreWaferSrcData(wafer, wafer, BinCode, BinCount, BinQuality, BinDescription, Pick, yieldstr,layoutid,binrate);
                }
                catch (Exception ex) { }
            }

            //var bin1nodes = root.SelectNodes("//BinDefinition[@BinCode='1']");
            //if (bin1nodes.Count > 0)
            //{
            //    if (!WebLog.CheckEmailRecord(wafer, "EM-BIN1"))
            //    {
            //        EmailUtility.SendEmail(ctrl, "DIE SORT SOURCE FILE " + wafer + " FATAL ERROR", towho, "Detail: Wafer contains BIN 1 ");
            //        new System.Threading.ManualResetEvent(false).WaitOne(300);
            //    }

            //    WebLog.Log(wafer, "DIESORT", "fail to check source file,bin 1 exist");
            //    return false;
            //}

            return true;
        }


        public static bool PrepareProbeData(string wafernum)
        {
            return WXProbeData.PrepareProbeData(wafernum);
        }

        public static bool PrepareEvalPN(string wafernum)
        {
            return WXEvalPN.PrepareEvalPN(wafernum);
        }

        public static bool PrepareData4WAT(string wafernum)
        {
            if (!PrepareProbeData(wafernum))
            { return false; }

            if (!PrepareEvalPN(wafernum))
            { return false; }

            if (wafernum.Length == 9)
            { WATApertureSize.PrepareAPConst2162(wafernum); }
            
            return true;
        }

        private static void StoreWaferSampleData(string mapfile, string wafer, Dictionary<string, string> selectxy, Dictionary<string, string> bin57dict, string array,string mpn,string fpn,bool supply)
        {
            if (!supply)
            {
                var sql = "delete from WaferSampleData where MAPFILE = '<MAPFILE>'";
                sql = sql.Replace("<MAPFILE>", mapfile);
                DBUtility.ExeLocalSqlNoRes(sql);
            }

            var updatetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            foreach (var kv in selectxy)
            {
                var xystr = kv.Key.Split(new string[] { ":::" }, StringSplitOptions.RemoveEmptyEntries);
                var x = xystr[0];
                var y = xystr[1];
                var sql = "insert into WaferSampleData(WAFER,X,Y,BIN,MPN,FPN,PArray,MAPFILE,UpdateTime) values(@WAFER,@X,@Y,@BIN,@MPN,@FPN,@PArray,@MAPFILE,@UpdateTime)";
                var dict = new Dictionary<string, string>();
                dict.Add("@WAFER",wafer);
                dict.Add("@X",x);
                dict.Add("@Y",y);
                dict.Add("@BIN",kv.Value);
                dict.Add("@MPN",mpn);
                dict.Add("@FPN",fpn);
                dict.Add("@PArray", array);
                dict.Add("@MAPFILE",mapfile);
                dict.Add("@UpdateTime", updatetime);
                DBUtility.ExeLocalSqlNoRes(sql, dict);
            }

            foreach (var kv in bin57dict)
            {
                var xystr = kv.Key.Split(new string[] { ":::" }, StringSplitOptions.RemoveEmptyEntries);
                var x = xystr[0];
                var y = xystr[1];
                var sql = "insert into WaferSampleData(WAFER,X,Y,BIN,MPN,FPN,PArray,MAPFILE,UpdateTime) values(@WAFER,@X,@Y,@BIN,@MPN,@FPN,@PArray,@MAPFILE,@UpdateTime)";
                var dict = new Dictionary<string, string>();
                dict.Add("@WAFER", wafer);
                dict.Add("@X", x);
                dict.Add("@Y", y);
                dict.Add("@BIN", "57X");
                dict.Add("@MPN", mpn);
                dict.Add("@FPN", fpn);
                dict.Add("@PArray", array);
                dict.Add("@MAPFILE", mapfile);
                dict.Add("@UpdateTime", updatetime);
                DBUtility.ExeLocalSqlNoRes(sql, dict);
            }
        }



        private static void StoreWaferPassBinData(string mapfile, string wafer, Dictionary<string, string> selectxy, Dictionary<string, string> passxy, string array, string mpn, string fpn,string pdesc,string product)
        {
            var sql = "delete from WaferPassBinData where MAPFILE = '<MAPFILE>'";
            sql = sql.Replace("<MAPFILE>", mapfile);
            DBUtility.ExeLocalSqlNoRes(sql);

            //var channel = Convert.ToInt32(array.Replace("1X", ""));
            var passbincountdict = new Dictionary<string, int>();
            foreach (var kv in passxy)
            {
                if (passbincountdict.ContainsKey(kv.Value))
                {passbincountdict[kv.Value] += 1;}
                else
                {passbincountdict.Add(kv.Value, 1);}
            }

            var samplecountdict = new Dictionary<string, int>();
            foreach (var kv in selectxy)
            {
                if (samplecountdict.ContainsKey(kv.Value))
                {samplecountdict[kv.Value] += 1;}
                else
                {samplecountdict.Add(kv.Value, 1);}
            }

            var updatetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            foreach (var binkv in passbincountdict)
            {
                var bincount = binkv.Value;
                if (samplecountdict.ContainsKey(binkv.Key))
                { bincount = bincount - samplecountdict[binkv.Key]; }

                sql = "insert into WaferPassBinData(WAFER,MPN,FPN,PArray,PDesc,Product,BIN,BINCount,MAPFILE,UpdateTime) values(@WAFER,@MPN,@FPN,@PArray,@PDesc,@Product,@BIN,@BINCount,@MAPFILE,@UpdateTime)";

                var dict = new Dictionary<string, string>();
                dict.Add("@WAFER", wafer);
                dict.Add("@MPN", mpn);
                dict.Add("@FPN", fpn);
                dict.Add("@PArray", array);
                dict.Add("@PDesc", pdesc);
                dict.Add("@Product", product);
                dict.Add("@BIN",binkv.Key);
                dict.Add("@BINCount",bincount.ToString());
                dict.Add("@MAPFILE", mapfile);
                dict.Add("@UpdateTime", updatetime);
                DBUtility.ExeLocalSqlNoRes(sql, dict);
            }
        }

        public static List<string> GetBomPNArrayInfo(string diefile)
        {
            var doc = new XmlDocument();
            doc.Load(diefile);
            var namesp = doc.DocumentElement.GetAttribute("xmlns");
            doc = StripNamespace(doc);
            XmlElement root = doc.DocumentElement;

            var layoutnodes = root.SelectNodes("//Layout[@BOMPN and @ARRAY and @BOMDES]");
            if (layoutnodes.Count > 0)
            {
                var pn = ((XmlElement)layoutnodes[0]).GetAttribute("BOMPN");
                var array = ((XmlElement)layoutnodes[0]).GetAttribute("ARRAY");
                var desc = ((XmlElement)layoutnodes[0]).GetAttribute("BOMDES");

                var tempvm = new List<string>();
                tempvm.Add(pn);tempvm.Add(array);tempvm.Add(desc);
                return tempvm;
            }

            var ret = new List<string>();
            ret.Add("");ret.Add(""); ret.Add(""); ret.Add("");
            return ret;
        }

        public static XmlDocument StripNamespace(XmlDocument doc)
        {
            if (doc.DocumentElement.NamespaceURI.Length > 0)
            {
                doc.DocumentElement.SetAttribute("xmlns", "");
                // must serialize and reload for this to take effect
                XmlDocument newDoc = new XmlDocument();
                newDoc.LoadXml(doc.OuterXml);
                return newDoc;
            }
            else
            {
                return doc;
            }
        }

        private static Dictionary<string, string> GetBin57Dict(XmlElement root, int samplecount)
        {
            var ret = new Dictionary<string, string>();
            //var goodbin57 = root.SelectNodes("//BinDefinition[@BinQuality='Pass' and @BinCode='57' and @Pick='true']");
            //if (goodbin57.Count > 0)
            //{
            //    var all57list = new List<string>();
            //    var bincodelist = root.SelectNodes("//BinCode[@X and @Y]");
            //    foreach (XmlElement nd in bincodelist)
            //    {
            //        if (string.Compare(nd.InnerText, "57", true) == 0)
            //        {
            //            all57list.Add(nd.GetAttribute("X") + ":::" + nd.GetAttribute("Y"));
            //        }
            //    }

            //    if (samplecount < all57list.Count)
            //    {
            //        var sectionlist = new List<int>();
            //        sectionlist.Add(0);
            //        sectionlist.Add(all57list.Count / 4);
            //        sectionlist.Add(all57list.Count / 2);
            //        sectionlist.Add(all57list.Count * 3 / 4);
            //        var maxsection = all57list.Count / 4;
            //        var rad = new Random(DateTime.Now.Millisecond);

            //        while (ret.Count < samplecount)
            //        {
            //            foreach (var s in sectionlist)
            //            {
            //                var sidx = s + rad.Next(maxsection);
            //                if (sidx < all57list.Count && !ret.ContainsKey(all57list[sidx]))
            //                {
            //                    ret.Add(all57list[sidx], "57");
            //                    if (ret.Count == samplecount)
            //                    { return ret; }
            //                }
            //            }//end foreach
            //        }//end while
            //    }//end if
            //}
            return ret;
        }


        private static Dictionary<string,string> GetPassedBinXYDict(XmlElement root,string defbin="")
        {
            var passbinnodes = root.SelectNodes("//BinDefinition[@BinQuality='Pass' and @BinCode and @Pick='true']");
            if (passbinnodes.Count == 0)
            { return new Dictionary<string, string>(); }

            var passbinlist = new List<string>();
            if (!string.IsNullOrEmpty(defbin))
            {
                passbinlist.Add(defbin);
            }
            else
            {
                foreach (XmlElement nd in passbinnodes)
                {
                   passbinlist.Add(nd.GetAttribute("BinCode"));
                }
            }

            var ret = new Dictionary<string, string>();
            var bincodelist = root.SelectNodes("//BinCode[@X and @Y]");
            foreach (XmlElement nd in bincodelist)
            {
                if (passbinlist.Contains(nd.InnerText))
                {
                    nd.SetAttribute("Q", "pass");

                    var k = nd.GetAttribute("X")+":::"+nd.GetAttribute("Y");
                    if (!ret.ContainsKey(k))
                    { ret.Add(k,nd.InnerText); }
                }
            }
            return ret;
        }

        //private static bool CheckNextXDie(int xval, int yval, int count, Dictionary<string, string> passedbinxydict)
        //{
        //    var idx = 0;
        //    var newxval = xval;
        //    while (idx < count)
        //    {
        //        newxval = newxval + 1;
        //        var k = newxval.ToString() + ":::" + yval.ToString();
        //        if (!passedbinxydict.ContainsKey(k))
        //        {
        //            return false;
        //        }
        //        idx = idx + 1;
        //    }
        //    return true;
        //}

        private static Dictionary<string, string> GetSelectedXYDict(Dictionary<string, string> passedbinxydict,int selectcount,Dictionary<string,string> bin57dict)
        {
            var ret = new Dictionary<string, string>();
            var xdict = new Dictionary<int,bool>();
            var ydict = new Dictionary<int,bool>();

            foreach (var kv in passedbinxydict)
            {
                try {
                    var xystr = kv.Key.Split(new string[] { ":::" }, StringSplitOptions.RemoveEmptyEntries);
                    var x = Convert.ToInt32(xystr[0]);
                    var y = Convert.ToInt32(xystr[1]);
                    if (!xdict.ContainsKey(x))
                    { xdict.Add(x,true); }
                    if (!ydict.ContainsKey(y))
                    { ydict.Add(y, true); }
                } catch (Exception ex) { }
            }

            var xlist = xdict.Keys.ToList();
            var ylist = ydict.Keys.ToList();

            xlist.Sort();
            ylist.Sort();

            var xsector = xlist.Count / 2;
            var ysector = (ylist[ylist.Count-1] - ylist[0]) / 3;

            var xstarterlist = new List<int>();
            xstarterlist.Add(0);
            xstarterlist.Add(1);

            var ystarterlist = new List<int>();
            ystarterlist.Add(ylist[0]);
            ystarterlist.Add(ylist[0] + ysector);
            ystarterlist.Add(ylist[0] + 2*ysector);


            var rad = new Random(DateTime.Now.Millisecond);
            var idx = 0;
            var maxtimes = selectcount * 100;
            while (true)
            {
                foreach (var xstart in xstarterlist)
                {
                    foreach (var ystart in ystarterlist)
                    {
                        var xrad = rad.Next(xsector);
                        var xval = 0;
                        var xidx = xstart * xsector + xrad;
                        if (xidx < xlist.Count)
                        { xval = xlist[xidx]; }
                        else
                        { xval = xlist[xidx-1]; }
                        

                        var yrad = rad.Next(ysector);
                        var yval = ystart + yrad;
                        var k = xval.ToString() + ":::" + yval.ToString();

                        if (passedbinxydict.ContainsKey(k) && !ret.ContainsKey(k) && !bin57dict.ContainsKey(k))
                        {
                             ret.Add(k, passedbinxydict[k]);
                            if (ret.Count == selectcount)
                            { return ret; }
                        }
                    }
                }
                                
                if (idx > maxtimes)
                { break; }
                idx = idx + 1;
            }

            ret.Clear();
            return ret;
        }

        private static List<string> GetPnFromWafer(string wafer,string offeredpn)
        {
            var pnlist = new List<string>();

            if (!string.IsNullOrEmpty(offeredpn))
            {
                pnlist.Add(offeredpn);
            }
            else
            {
                var sql = "";

                sql = @"SELECT distinct Product FROM [Insite].[insite].[Rpt_ReleasedJob] 
                        where left(ContainerName,9) = '<wafer>' and Factory = 'BIN'";
                sql = sql.Replace("<wafer>", wafer);
                var dbret = DBUtility.ExeAllenSqlWithRes(sql);
                foreach (var line in dbret)
                {
                    pnlist.Add(Convert.ToString(line[0]));
                }

                if (pnlist.Count == 0)
                {
                    sql = @"SELECT distinct bp.BinJobPartNumber FROM [Insite].[insite].[Rpt_ReleasedJob] rj
                        left join [Insite].[insite].[BinJobPartnumbers] bp on bp.ChipPartNumber = rj.product
                        where Factory = 'CHIP' and left(ContainerName,9) = '<wafer>' and bp.BinJobPartNumber is not null";

                    sql = sql.Replace("<wafer>", wafer);
                    dbret = DBUtility.ExeAllenSqlWithRes(sql);
                    foreach (var line in dbret)
                    {
                        pnlist.Add(Convert.ToString(line[0]));
                    }
                }

                //if (pnlist.Count == 0)
                //{
                //    sql = @"select distinct ProductName from [InsiteDB].[insite].ProductBase where ProductBaseId in (
                //                select ProductBaseId from  [InsiteDB].[insite].Product
                //                where ProductId  in (
                //                    SELECT distinct hml.ProductId FROM [InsiteDB].[insite].[dc_IQC_InspectionResult] (nolock) aoc 
                //                    left join [InsiteDB].[insite].HistoryMainline (nolock) hml on aoc.[HistoryMainlineId] = hml.HistoryMainlineId
                //                    where ParamValueString like '%<wafer>%'))";

                //    sql = sql.Replace("<wafer>", wafer);
                //    dbret = DBUtility.ExeMESSqlWithRes(sql);
                //    foreach (var line in dbret)
                //    {
                //        pnlist.Add(Convert.ToString(line[0]));
                //    }
                //}

                //if (pnlist.Count == 0)
                //{
                //    sql = @"select distinct ProductName from [InsiteDB].[insite].ProductBase where ProductBaseId in (
                //                select ProductBaseId from  [InsiteDB].[insite].Product
                //                where ProductId  in (
                //                    SELECT distinct hml.ProductId FROM [InsiteDB].[insite].[dc_AOC_ManualInspection] (nolock) aoc 
                //                    left join [InsiteDB].[insite].HistoryMainline (nolock) hml on aoc.[HistoryMainlineId] = hml.HistoryMainlineId
                //                    where ParamValueString like '%<wafer>%'))";
                //    sql = sql.Replace("<wafer>", wafer);
                //    dbret = DBUtility.ExeMESSqlWithRes(sql);
                //    foreach (var line in dbret)
                //    {
                //        pnlist.Add(Convert.ToString(line[0]));
                //    }
                //}

                //if (pnlist.Count == 0)
                //{
                //    sql = @"SELECT distinct pb.ProductName
                //              FROM [InsiteDB].[insite].[Container]  (nolock) c
                //              left join [InsiteDB].[insite].Product (nolock) p on c.ProductId = p.ProductId
                //              left join [InsiteDB].[insite].ProductBase (nolock) pb on pb.ProductBaseId = p.ProductBaseId
                //               where c.SupplierLotNumber like '%<wafer>%'";

                //    sql = sql.Replace("<wafer>", wafer);
                //    dbret = DBUtility.ExeMESSqlWithRes(sql);
                //    foreach (var line in dbret)
                //    {
                //        pnlist.Add(Convert.ToString(line[0]));
                //    }
                //}
            }

            return pnlist;
        }


        public static List<string> GetVCSELInfoFromPNList(string wafer, List<string> pnlist)
        {
            if (pnlist.Count > 0)
            {
                var pncond = "('" + string.Join("','", pnlist) + "')";
                var sql = "Select distinct [Desc],MPN,FPN,Product from WaferArray where (MPN in <pncond> or FPN in <pncond>)";
                sql = sql.Replace("<pncond>", pncond);
                var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
                foreach (var line in dbret)
                {
                    var tempvm = new List<string>();
                    tempvm.Add(Convert.ToString(line[0]).Trim().ToUpper());
                    tempvm.Add(Convert.ToString(line[1]).Trim().ToUpper());
                    tempvm.Add(Convert.ToString(line[2]).Trim().ToUpper());
                    tempvm.Add(Convert.ToString(line[3]).Trim().ToUpper());
                    return tempvm;
                }

                WebLog.Log(wafer, "DIESORT", "fail to get array by pn " + pncond);
            }

            return new List<string>();
        }



        public static List<DieSortVM> RetrieveReviewData(string diefile)
        {
            var valdict = new Dictionary<string, int>();

            var doc = new XmlDocument();
            doc.Load(diefile);
            var namesp = doc.DocumentElement.GetAttribute("xmlns");
            doc = StripNamespace(doc);
            XmlElement root = doc.DocumentElement;
            var bincodelist = root.SelectNodes("//BinCode[@X and @Y]");
            foreach (XmlElement nd in bincodelist)
            {
                try
                {
                    var k = nd.GetAttribute("X") + ":::" + nd.GetAttribute("Y");
                    if (!valdict.ContainsKey(k))
                    { valdict.Add(k,0); }
                }
                catch (Exception ex) { }
            }

            bincodelist = root.SelectNodes("//BinCode[@X and @Y and @Q]");
            foreach (XmlElement nd in bincodelist)
            {
                try
                {
                    var k = nd.GetAttribute("X") + ":::" + nd.GetAttribute("Y");
                    if (valdict.ContainsKey(k))
                    { valdict[k] = 5; }
                }
                catch (Exception ex) { }
            }

            bincodelist = root.SelectNodes("//BinCode[@X and @Y and @diesort]");
            foreach (XmlElement nd in bincodelist)
            {
                try
                {
                    var k = nd.GetAttribute("X") + ":::" + nd.GetAttribute("Y");
                    if (valdict.ContainsKey(k))
                    {
                        var sval = nd.GetAttribute("diesort");
                        if (string.Compare(sval, "selected", true) != 0)
                        { valdict[k] = 1; }
                        else
                        { valdict[k] = 10; }
                    }
                }
                catch (Exception ex) { }
            }

            var ret = new List<DieSortVM>();
            foreach (var kv in valdict)
            {
                try
                {
                    var xystr = kv.Key.Split(new string[] { ":::" }, StringSplitOptions.RemoveEmptyEntries);
                    var x = Convert.ToInt32(xystr[0]);
                    var y = Convert.ToInt32(xystr[1]);
                    ret.Add(new DieSortVM(x, y, kv.Value));
                }
                catch (Exception ex) { }
            }

            return ret;
        }

        public static List<DieSortVM> RetrievePDData(string diefile)
        {
            var valdict = new Dictionary<string, int>();

            var doc = new XmlDocument();
            doc.Load(diefile);
            var namesp = doc.DocumentElement.GetAttribute("xmlns");
            doc = StripNamespace(doc);
            XmlElement root = doc.DocumentElement;

            var goodbindict = new Dictionary<string, bool>();
            var goodbins = root.SelectNodes("//BinDefinition[@BinDescription='GOOD']");
            foreach (XmlElement nd in goodbins)
            {
                try
                {
                    var bc = nd.GetAttribute("BinCode");
                    if (!goodbindict.ContainsKey(bc))
                    { goodbindict.Add(bc, true); }
                }
                catch (Exception ex) { }
            }

            var ret = new List<DieSortVM>();
            var bincodelist = root.SelectNodes("//BinCode[@X and @Y]");
            foreach (XmlElement nd in bincodelist)
            {
                try
                {
                    var x = nd.GetAttribute("X");
                    var y = nd.GetAttribute("Y");
                    if (goodbindict.ContainsKey(nd.InnerText.Trim()))
                    { ret.Add(new DieSortVM(UT.O2I(x), UT.O2I(y),5)); }
                    else
                    { ret.Add(new DieSortVM(UT.O2I(x), UT.O2I(y), 0)); }
                }
                catch (Exception ex) { }
            }

            return ret;
        }

        public static List<DieSortVM> RetrieveCMPData(string diefile)
        {
            var ret = new List<DieSortVM>();

            var max = 1000;
            var idx = 1;

            var doc = new XmlDocument();
            doc.Load(diefile);
            var namesp = doc.DocumentElement.GetAttribute("xmlns");
            doc = StripNamespace(doc);
            XmlElement root = doc.DocumentElement;
            var bincodelist = root.SelectNodes("//BinCode[@X and @Y]");
            foreach (XmlElement nd in bincodelist)
            {
                try
                {
                    var x = Convert.ToInt32(nd.GetAttribute("X"));
                    var y = Convert.ToInt32(nd.GetAttribute("Y"));
                    if (idx % 100 == 0)
                    {
                        ret.Add(new DieSortVM(x, y, 10));
                    }
                    else
                    {
                        ret.Add(new DieSortVM(x, y, 5));
                    }
                    if (idx == max)
                    { break; }
                    idx = idx + 1;
                }
                catch (Exception ex) { }
            }

            return ret;
        }

        public static object RetrieveDieChartData(List<DieSortVM> datalist,string id = "die_sort_id",string title="Die Sort Pick Map")
        {
            var data = new List<List<object>>();
            var xdict = new Dictionary<int,bool>();
            var ydict = new Dictionary<int, bool>();

            foreach (var v in datalist)
            {
                var templist = new List<object>();
                templist.Add(v.X);
                templist.Add(v.Y);
                templist.Add(v.DieValue);
                data.Add(templist);

                if (!xdict.ContainsKey(v.X))
                { xdict.Add(v.X, true); }
                if (!ydict.ContainsKey(v.Y))
                { ydict.Add(v.Y, true); }
            }

            var serial = new List<object>();
            serial.Add(new
            {
                name = "Die Sort",
                data = data,
                boostThreshold = 100,
                borderWidth = 0,
               nullColor = "#EFEFEF",
               turboThreshold = 100
            });

            var xlist = new List<int>();
            var ylist = new List<int>();
            xlist.AddRange(xdict.Keys);
            ylist.AddRange(ydict.Keys);
            xlist.Sort();
            ylist.Sort();
            return new
            {
                id = id,
                title = title,
                serial = serial,
                xmax = xlist[xlist.Count - 1] + 20,
                ymax = ylist[ylist.Count - 1] + 20
            };
        }

        public static List<DieSortVM> RetrieveSampleData(string mf)
        {
            var ret = new List<DieSortVM>();
            var sql = @"select distinct ws.WAFER,ws.X,ws.Y,ws.BIN,wc.LayoutId,ws.MPN,ws.FPN,ws.PArray,wa.[Desc],wa.Tech,ws.MAPFILE
                          ,ws.UpdateTime from [WAT].[dbo].[WaferSampleData] (nolock) ws
                          inner join  [WAT].[dbo].[WaferSrcData] (nolock) wc on wc.WAFER = ws.WAFER
                          left join  [WAT].[dbo].WaferArray (nolock) wa on wa.MPN = ws.MPN
                          where ws.WAFER = @MAPFILE";

            var dict = new Dictionary<string, string>();
            dict.Add("@MAPFILE", mf);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            foreach (var l in dbret)
            {
                ret.Add(new DieSortVM(O2S(l[0]), O2S(l[1]), O2S(l[2]), O2S(l[3])
                                    , O2S(l[4]), O2S(l[5]), O2S(l[6]), O2S(l[7])
                                    , O2S(l[8]), O2S(l[9]), O2S(l[10]),Convert.ToDateTime(l[11]).ToString("yyyy-MM-dd HH:mm:ss")));
            }
            return ret;
        }

        public static List<string> SortedWaferNum()
        {
            var ret = new List<string>();
            var sql = "select distinct WAFER from [WAT].[dbo].[WaferPassBinData] order by WAFER";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                ret.Add(UT.O2S(line[0]));
            }
            return ret;
        }

        public static List<object> waferBinSubstitude(string wafer)
        {
            var ret = new List<object>();

            var dict = new Dictionary<string, string>();
            dict.Add("@wafer", wafer);
           
            var sql = @" select distinct FromDevice,FromBin,ToBin FROM [WAT].[dbo].[WATBINSubstitute] where FromDevice in(
                        	select distinct Product FROM [WAT].[dbo].[WXEvalPN] where WaferNum = @wafer)";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                ret.Add(new
                {
                    Wafer = wafer,
                    FromDevice = UT.O2S(line[0]),
                    FromBin = UT.O2S(line[1]),
                    ToBin = UT.O2S(line[2])
                });
            }
           
            return ret;
        }

        public static List<object> GetSampleXY(string wafer)
        {
            var ret = new List<object>();
            var dict = new Dictionary<string, string>();
            dict.Add("@wafer", wafer);
            var sql = "select X,Y from [WAT].[dbo].[WaferSampleData] where wafer = @wafer";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                ret.Add(new
                {
                    X=UT.O2S(line[0]),
                    Y=UT.O2S(line[1])
                });
            }
            return ret;
        }

        public static List<object> BinData4Plan(string wafer)
        {
            var ret = new List<object>();

            if (wafer.Length == 13)
            {
                var sql = "select wafer,bin,bincount from [WAT].[dbo].[WaferPassBinData] where WAFER = @wafer";
                var dict = new Dictionary<string, string>();
                dict.Add("@wafer", wafer);
                var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
                foreach (var line in dbret)
                {
                    ret.Add(new
                    {
                        Wafer = UT.O2S(line[0]),
                        BIN = UT.O2S(line[1]),
                        Count = UT.O2I(line[2])
                    });
                }
            }
            else
            {
                var srcdict = new Dictionary<string, int>();
                var sql = "select wafer,bincode,bincount from [WAT].[dbo].[WaferSrcData] where WAFER = @wafer and BinQuality = 'Pass'";
                var dict = new Dictionary<string, string>();
                dict.Add("@wafer", wafer);
                var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
                foreach (var line in dbret)
                {
                    var bin = UT.O2S(line[1]);
                    var cnt = UT.O2I(line[2]);
                    if (!srcdict.ContainsKey(bin))
                    { srcdict.Add(bin, cnt); }
                }

                var sampdict = new Dictionary<string, int>();
                sql = "select bin,count(bin) from [WAT].[dbo].[WaferSampleData] where WAFER = @wafer group by bin";
                dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
                foreach (var line in dbret)
                {
                    var bin = UT.O2S(line[0]);
                    var cnt = UT.O2I(line[1]);
                    if (!sampdict.ContainsKey(bin))
                    { sampdict.Add(bin, cnt); }
                }

                foreach (var skv in srcdict)
                {
                    var bin = skv.Key;
                    var cnt = skv.Value;
                    if (sampdict.ContainsKey(bin))
                    {
                        cnt = cnt - sampdict[bin];
                    }
                    ret.Add(new
                    {
                        Wafer = wafer,
                        BIN = bin,
                        Count = cnt
                    });
                }
            }


            return ret;
        }

        public static List<object> SrcData4Plan(string wafer)
        {
            var ret = new List<object>();

            var srcdict = new Dictionary<string, int>();
            var sql = "select wafer,bincode,bincount from [WAT].[dbo].[WaferSrcData] where WAFER = @wafer and BinQuality = 'Pass'";
            var dict = new Dictionary<string, string>();
            dict.Add("@wafer", wafer);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                ret.Add(new
                {
                    Wafer = UT.O2S(line[0]),
                    BIN = UT.O2S(line[1]),
                    Count = UT.O2I(line[2])
                });
            }

            return ret;
        }

        public static string PickSample4Ipoh(string wafer,Controller ctrl)
        {
            var syscfgdict = CfgUtility.GetSysConfig(ctrl);

            var allwffiles = DieSortVM.GetAllWaferFile(ctrl);
            var productfm = "";
            if (wafer.Length == 13)
            {
                productfm = WXEvalPN.GetProductFamilyFromSherman(wafer);
                if (string.IsNullOrEmpty(productfm))
                { return "fail to to get product family by wafer " + wafer; }
            }
            else
            {
                productfm = WXEvalPN.GetProductFamilyFromAllen(wafer);
                if (string.IsNullOrEmpty(productfm))
                {
                    productfm = WXEvalPN.GetProductFamilyFromSherman(wafer);
                    if (string.IsNullOrEmpty(productfm))
                    { return "fail to to get product family by wafer " + wafer; }
                }
            }

            //get the actual wafer file
            var swaferfile = "";
            foreach (var f in allwffiles)
            {
                var uf = f.ToUpper();
                if (uf.Contains(productfm.ToUpper()) && uf.Contains(wafer.ToUpper()))
                {
                    swaferfile = f;
                    break;
                }
            }

            if (string.IsNullOrEmpty(swaferfile))
            {
                foreach (var f in allwffiles)
                {
                    var uf = f.ToUpper();
                    if (uf.Contains(wafer.ToUpper()))
                    {
                        swaferfile = f;
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(swaferfile))
            {
                return "map file does not exist";
            }


            var waferfile = ExternalDataCollector.DownloadShareFile(swaferfile, ctrl);
            if (waferfile == null)
            { return "fail to download map file"; }

            try
            {
                //get modify information
                var doc = new XmlDocument();
                doc.Load(waferfile);
                var namesp = doc.DocumentElement.GetAttribute("xmlns");
                doc = StripNamespace(doc);
                XmlElement root = doc.DocumentElement;

                var passedbinxydict = GetIPOHPassedBinXYDict(root);
                if (passedbinxydict.Count == 0)
                { return "fail to get any good bin"; }

                var bin1dict = new Dictionary<string, string>();
                var bin2dict = new Dictionary<string, string>();
                GetIPOHSelectedXYDict(passedbinxydict, bin1dict, bin2dict
                    ,UT.O2I(syscfgdict["IPOHBIN1CNT"]), UT.O2I(syscfgdict["IPOHBIN2CNT"]));
                if (bin1dict.Count == 0)
                {
                    return "fail to get sample";
                }

                var allbincodelist = root.SelectNodes("//BinCode[@X and @Y]");
                foreach (XmlElement nd in allbincodelist)
                {
                    var binnum = UT.O2I(nd.InnerText);
                    if ((binnum >= 30 && binnum <= 39))
                    { }
                    else
                    {
                        nd.InnerText = "99";
                    }
                }

                foreach (var kv in bin1dict)
                {
                    var xystr = kv.Key.Split(new string[] { ":::" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (XmlElement nd in root.SelectNodes("//BinCode[@X='" + xystr[0] + "' and @Y='" + xystr[1] + "']"))
                    {
                        nd.InnerText = "1";
                    }
                }

                foreach (var kv in bin2dict)
                {
                    var xystr = kv.Key.Split(new string[] { ":::" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (XmlElement nd in root.SelectNodes("//BinCode[@X='" + xystr[0] + "' and @Y='" + xystr[1] + "']"))
                    {
                        nd.InnerText = "2";
                    }
                }


                var desfolder = syscfgdict["DIESORTSHARE"];
                doc.DocumentElement.SetAttribute("xmlns", namesp);
                var savename = Path.Combine(desfolder, Path.GetFileNameWithoutExtension(waferfile)+"_IPOH"+Path.GetExtension(waferfile));
                if (ExternalDataCollector.FileExist(ctrl, savename))
                { ExternalDataCollector.FileDelete(ctrl, savename); }
                doc.Save(savename);

            }
            catch (Exception ex) { return ex.Message; }

            return string.Empty;
        }

        private static Dictionary<string, string> GetIPOHPassedBinXYDict(XmlElement root)
        {
            var passbinnodes = root.SelectNodes("//BinDefinition[@BinQuality='Pass' and @BinCode and @Pick='true']");
            if (passbinnodes.Count == 0)
            { return new Dictionary<string, string>(); }

            var passbinlist = new List<string>();
            foreach (XmlElement nd in passbinnodes)
            {
                var bincode = nd.GetAttribute("BinCode");
                if(bincode.Contains("55")
                    ||bincode.Contains("57")
                    ||bincode.Contains("59"))
                passbinlist.Add(bincode);
            }
            if (passbinlist.Count == 0)
            { return new Dictionary<string, string>(); }

            var ret = new Dictionary<string, string>();
            var bincodelist = root.SelectNodes("//BinCode[@X and @Y]");
            foreach (XmlElement nd in bincodelist)
            {
                if (passbinlist.Contains(nd.InnerText))
                {
                    var k = nd.GetAttribute("X") + ":::" + nd.GetAttribute("Y");
                    if (!ret.ContainsKey(k))
                    { ret.Add(k, nd.InnerText); }
                }
            }
            return ret;
        }

        public static string PickE01Sample4Ipoh(string wafer, Controller ctrl)
        {
            var syscfgdict = CfgUtility.GetSysConfig(ctrl);
            var ipohfolder = syscfgdict["DIESORTIPOH"];

            var existfs = ExternalDataCollector.DirectoryEnumerateFiles(ctrl, ipohfolder);
            foreach (var f in existfs)
            {
                if (f.Contains(wafer))
                {
                    return "IPOH sample map file has already exist, wafer " + wafer;
                }
            }

            var productfm = "";
            if (wafer.Length == 13)
            {
                productfm = WXEvalPN.GetProductFamilyFromSherman(wafer);
                if (string.IsNullOrEmpty(productfm))
                { return "fail to to get product family by wafer " + wafer; }
            }
            else
            {
                productfm = WXEvalPN.GetProductFamilyFromAllen(wafer);
                if (string.IsNullOrEmpty(productfm))
                {
                    productfm = WXEvalPN.GetProductFamilyFromSherman(wafer);
                    if (string.IsNullOrEmpty(productfm))
                    { return "fail to to get product family by wafer " + wafer; }
                }
            }

            var allwffiles = DieSortVM.GetAllWaferFile(ctrl, productfm);

            //get the actual wafer file
            var swaferfile = "";
            foreach (var f in allwffiles)
            {
                var uf = f.ToUpper();
                if (uf.Contains(productfm.ToUpper()) && uf.Contains(wafer.ToUpper()))
                {
                    swaferfile = f;
                    break;
                }
            }

            if (string.IsNullOrEmpty(swaferfile))
            {
                foreach (var f in allwffiles)
                {
                    var uf = f.ToUpper();
                    if (uf.Contains(wafer.ToUpper()))
                    {
                        swaferfile = f;
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(swaferfile))
            {
                return "map file does not exist in Sherman share folder";
            }


            var waferfile = ExternalDataCollector.DownloadShareFile(swaferfile, ctrl);
            if (waferfile == null)
            { return "fail to download map file of wafer "+wafer; }

            PrepareEvalPN(wafer);

            try
            {
                //get modify information
                var doc = new XmlDocument();
                doc.Load(waferfile);
                var namesp = doc.DocumentElement.GetAttribute("xmlns");
                doc = StripNamespace(doc);
                XmlElement root = doc.DocumentElement;

                //store src data
                CleanWaferSrcData(wafer);
                var binnodes = root.SelectNodes("//BinDefinition[@BinCode and @BinCount and @BinQuality and @BinDescription and @Pick]");
                foreach (XmlElement node in binnodes)
                {
                    try
                    {
                        var BinCode = node.GetAttribute("BinCode");
                        var BinCount = node.GetAttribute("BinCount");
                        var BinQuality = node.GetAttribute("BinQuality");
                        var BinDescription = node.GetAttribute("BinDescription");
                        var Pick = node.GetAttribute("Pick");

                        StoreWaferSrcData(wafer, wafer, BinCode, BinCount, BinQuality, BinDescription, Pick, "", "", "");
                    }
                    catch (Exception ex) { }
                }

                //get pass bin
                var passedbinxydict = GetIPOHE01PassedBinXYDict(root);
                if (passedbinxydict.Count == 0)
                { return "fail to get any good bin for wafer "+ wafer; }

                //get bin1 bin2
                var bin1dict = new Dictionary<string, string>();
                var bin2dict = new Dictionary<string, string>();
                GetIPOHSelectedXYDict(passedbinxydict, bin1dict, bin2dict
                    , UT.O2I(syscfgdict["IPOHE01CNT"]), UT.O2I(syscfgdict["IPOHE07CNT"]));

                if (bin1dict.Count == 0)
                {
                    return "fail to get sample";
                }

                var allbincodelist = root.SelectNodes("//BinCode[@X and @Y]");
                foreach (XmlElement nd in allbincodelist)
                {
                    var binnum = UT.O2I(nd.InnerText);
                    if ((binnum >= 30 && binnum <= 39))
                    { }
                    else
                    {
                        nd.InnerText = "99";
                    }
                }

                foreach (var kv in bin1dict)
                {
                    var xystr = kv.Key.Split(new string[] { ":::" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (XmlElement nd in root.SelectNodes("//BinCode[@X='" + xystr[0] + "' and @Y='" + xystr[1] + "']"))
                    {
                        nd.InnerText = "1";
                    }
                }

                foreach (var kv in bin2dict)
                {
                    var xystr = kv.Key.Split(new string[] { ":::" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (XmlElement nd in root.SelectNodes("//BinCode[@X='" + xystr[0] + "' and @Y='" + xystr[1] + "']"))
                    {
                        nd.InnerText = "2";
                    }
                }

                doc.DocumentElement.SetAttribute("xmlns", namesp);
                var savename = Path.Combine(ipohfolder, Path.GetFileName(waferfile));
                if (ExternalDataCollector.FileExist(ctrl, savename))
                { ExternalDataCollector.FileDelete(ctrl, savename); }
                doc.Save(savename);

                StoreIPOHSampleData(wafer, wafer, bin1dict, bin2dict);
            }
            catch (Exception ex) { return ex.Message; }

            return string.Empty;
        }

        private static void StoreIPOHSampleData(string mapfile, string wafer, Dictionary<string, string> bin1dict, Dictionary<string, string> bin2dict)
        {
            {
                var sql = "delete from WaferSampleData where MAPFILE = '<MAPFILE>'";
                sql = sql.Replace("<MAPFILE>", mapfile);
                DBUtility.ExeLocalSqlNoRes(sql);
            }

            var updatetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            foreach (var kv in bin1dict)
            {
                var xystr = kv.Key.Split(new string[] { ":::" }, StringSplitOptions.RemoveEmptyEntries);
                var x = xystr[0];
                var y = xystr[1];
                var sql = "insert into WaferSampleData(WAFER,X,Y,BIN,MPN,FPN,PArray,MAPFILE,UpdateTime) values(@WAFER,@X,@Y,@BIN,@MPN,@FPN,@PArray,@MAPFILE,@UpdateTime)";
                var dict = new Dictionary<string, string>();
                dict.Add("@WAFER", wafer);
                dict.Add("@X", x);
                dict.Add("@Y", y);
                dict.Add("@BIN", kv.Value);
                dict.Add("@MPN", "1");
                dict.Add("@FPN", "");
                dict.Add("@PArray", "");
                dict.Add("@MAPFILE", mapfile);
                dict.Add("@UpdateTime", updatetime);
                DBUtility.ExeLocalSqlNoRes(sql, dict);
            }

            foreach (var kv in bin2dict)
            {
                var xystr = kv.Key.Split(new string[] { ":::" }, StringSplitOptions.RemoveEmptyEntries);
                var x = xystr[0];
                var y = xystr[1];
                var sql = "insert into WaferSampleData(WAFER,X,Y,BIN,MPN,FPN,PArray,MAPFILE,UpdateTime) values(@WAFER,@X,@Y,@BIN,@MPN,@FPN,@PArray,@MAPFILE,@UpdateTime)";
                var dict = new Dictionary<string, string>();
                dict.Add("@WAFER", wafer);
                dict.Add("@X", x);
                dict.Add("@Y", y);
                dict.Add("@BIN", kv.Value);
                dict.Add("@MPN", "2");
                dict.Add("@FPN", "");
                dict.Add("@PArray", "");
                dict.Add("@MAPFILE", mapfile);
                dict.Add("@UpdateTime", updatetime);
                DBUtility.ExeLocalSqlNoRes(sql, dict);
            }
        }


        public static string GetAllBinFromMapFile(string wafer,Dictionary<string,string> bindict, Controller ctrl)
        {
            var syscfgdict = CfgUtility.GetSysConfig(ctrl);

            var productfm = WXEvalPN.GetLocalProductFam(wafer);
            var allwffiles = DieSortVM.GetAllWaferFile(ctrl);
            var arraysize = WXEvalPN.GetLocalWaferArray(wafer);

            //if (wafer.Length == 13)
            //{
            //    productfm = WXEvalPN.GetProductFamilyFromSherman(wafer);
            //    if (string.IsNullOrEmpty(productfm))
            //    { return "fail to to get product family by wafer " + wafer; }
            //}
            //else
            //{
            //    productfm = WXEvalPN.GetProductFamilyFromAllen(wafer);
            //    if (string.IsNullOrEmpty(productfm))
            //    {
            //        productfm = WXEvalPN.GetProductFamilyFromSherman(wafer);
            //        if (string.IsNullOrEmpty(productfm))
            //        { return "fail to to get product family by wafer " + wafer; }
            //    }
            //}

            //get the actual wafer file
            var swaferfile = "";
            long filesize = 0;
            var flist = new List<string>();
            foreach (var f in allwffiles)
            {
                var uf = f.ToUpper();
                if (uf.Contains(wafer.ToUpper()))
                {
                    flist.Add(uf);
                }
            }

            foreach (var f in flist)
            {
                System.IO.FileInfo fi = new System.IO.FileInfo(f);
                if (fi.Length > filesize)
                {
                    swaferfile = f;
                    filesize = fi.Length;
                }
            }

            if (string.IsNullOrEmpty(swaferfile))
            {
                return "map file does not exist";
            }


            var waferfile = ExternalDataCollector.DownloadShareFile(swaferfile, ctrl);
            if (waferfile == null)
            { return "fail to download map file"; }

            try
            {
                //get modify information
                var doc = new XmlDocument();
                doc.Load(waferfile);
                var namesp = doc.DocumentElement.GetAttribute("xmlns");
                doc = StripNamespace(doc);
                XmlElement root = doc.DocumentElement;

                var bincodelist = root.SelectNodes("//BinCode[@X and @Y]");
                foreach (XmlElement nd in bincodelist)
                {
                    var x = nd.GetAttribute("X");
                    var y = nd.GetAttribute("Y");
                    var k = x + ":::" + y;
                    if (!bindict.ContainsKey(k))
                    { bindict.Add(k, nd.InnerText); }
                }//end foreach
            }catch (Exception ex) { return ex.Message; }

            return string.Empty;
        }

        public static int Get_First_Singlet_From_Array_Coord(int DIE_ONE_X, int DIE_ONE_FIELD_MIN_X, int arrayx, int Array_Count)
        {
            var new_x = ((arrayx - DIE_ONE_X
                + Math.Floor((double)(DIE_ONE_X - DIE_ONE_FIELD_MIN_X) / Array_Count)) * Array_Count)
                + DIE_ONE_FIELD_MIN_X;
            if (Array_Count != 1)
            { return (int)Math.Round(new_x, 0) - 1; }
            else
            { return (int)Math.Round(new_x, 0); }
        }

        private static Dictionary<string, string> GetIPOHE01PassedBinXYDict(XmlElement root)
        {
            var passbinnodes = root.SelectNodes("//BinDefinition[@BinQuality='Pass' and @BinCode and @Pick='true']");
            if (passbinnodes.Count == 0)
            { return new Dictionary<string, string>(); }

            var passbinlist = new List<string>();
            foreach (XmlElement nd in passbinnodes)
            {
                var bincode = nd.GetAttribute("BinCode");
                passbinlist.Add(bincode);
            }
            if (passbinlist.Count == 0)
            { return new Dictionary<string, string>(); }

            var ret = new Dictionary<string, string>();
            var bincodelist = root.SelectNodes("//BinCode[@X and @Y]");
            foreach (XmlElement nd in bincodelist)
            {
                if (passbinlist.Contains(nd.InnerText))
                {
                    var k = nd.GetAttribute("X") + ":::" + nd.GetAttribute("Y");
                    if (!ret.ContainsKey(k))
                    { ret.Add(k, nd.InnerText); }
                }
            }
            return ret;
        }

        private static void GetIPOHSelectedXYDict(Dictionary<string, string> passedbinxydict, Dictionary<string, string> bin1dict
            , Dictionary<string, string> bin2dict,int bin1cnt, int bin2cnt)
        {
            var xdict = new Dictionary<int, bool>();
            var ydict = new Dictionary<int, bool>();

            foreach (var kv in passedbinxydict)
            {
                try
                {
                    var xystr = kv.Key.Split(new string[] { ":::" }, StringSplitOptions.RemoveEmptyEntries);
                    var x = Convert.ToInt32(xystr[0]);
                    var y = Convert.ToInt32(xystr[1]);
                    if (!xdict.ContainsKey(x))
                    { xdict.Add(x, true); }
                    if (!ydict.ContainsKey(y))
                    { ydict.Add(y, true); }
                }
                catch (Exception ex) { }
            }

            var xlist = xdict.Keys.ToList();
            var ylist = ydict.Keys.ToList();

            xlist.Sort();
            ylist.Sort();

            var xsector = xlist.Count / 2;
            var ysector = (ylist[ylist.Count - 1] - ylist[0]) / 3;

            var xstarterlist = new List<int>();
            xstarterlist.Add(0);
            xstarterlist.Add(1);

            var ystarterlist = new List<int>();
            ystarterlist.Add(ylist[0]);
            ystarterlist.Add(ylist[0] + ysector);
            ystarterlist.Add(ylist[0] + 2 * ysector);


            var rad = new Random(DateTime.Now.Millisecond);
            var idx = 0;
            var maxtimes = 140000;
            while (true)
            {
                foreach (var xstart in xstarterlist)
                {
                    foreach (var ystart in ystarterlist)
                    {
                        var xrad = rad.Next(xsector);
                        var xval = 0;
                        var xidx = xstart * xsector + xrad;
                        if (xidx < xlist.Count)
                        { xval = xlist[xidx]; }
                        else
                        { xval = xlist[xidx - 1]; }


                        var yrad = rad.Next(ysector);
                        var yval = ystart + yrad;
                        var k = xval.ToString() + ":::" + yval.ToString();

                        if (passedbinxydict.ContainsKey(k))
                        {
                            if (bin1dict.Count < bin1cnt)
                            {
                                if (!bin1dict.ContainsKey(k))
                                {
                                    bin1dict.Add(k, passedbinxydict[k]);
                                }
                            }
                            else
                            {
                                if (!bin2dict.ContainsKey(k) && !bin1dict.ContainsKey(k))
                                {
                                    bin2dict.Add(k, passedbinxydict[k]);
                                    if (bin2dict.Count >= bin2cnt)
                                    {
                                        return;
                                    }
                                }
                            }
                        }//match pass die
                    }//end foreach
                }//end foreach

                if (idx > maxtimes)
                { break; }
                idx = idx + 1;
            }//end while(true)

        }

        public static void ModifyMapFileAsExpect(Controller ctrl)
        {
            var doc = new XmlDocument();
            doc.Load(@"\\wux-engsys01\HPU\192321-30.xml");
            var namesp = doc.DocumentElement.GetAttribute("xmlns");
            doc = StripNamespace(doc);
            var root = doc.DocumentElement;

            var allbincodelist = root.SelectNodes("//BinCode[@X='229' and @Y and @Q='pass']");
            foreach (XmlElement nd in allbincodelist)
            { nd.InnerText = "1"; }

            doc.DocumentElement.SetAttribute("xmlns", namesp);
            var savename = @"\\wux-engsys01\HPU\192321-30_new.xml";
            if (ExternalDataCollector.FileExist(ctrl, savename))
            { ExternalDataCollector.FileDelete(ctrl, savename); }
            doc.Save(savename);
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

        private static int O2I(object obj)
        {
            if (obj == null)
            { return 0; }

            try
            {
                return Convert.ToInt32(obj);
            }
            catch (Exception ex) { return 0; }
        }

        public static Coord<short, short> Get_Array_Coord_From_Singlet_Coord(short DIE_ONE_X, short DIE_ONE_FIELD_MIN_X, Coord<short, short> singlet_coord, int Array_Count)
        {
            var new_x = DIE_ONE_X +
                Math.Floor((double)(singlet_coord.X - DIE_ONE_FIELD_MIN_X) / Array_Count) -
                Math.Floor((double)(DIE_ONE_X - DIE_ONE_FIELD_MIN_X) / Array_Count);
            return new Coord<short, short>((short)Math.Round(new_x, 0), singlet_coord.Y);
        }

        public static Coord<short, short> Get_First_Singlet_From_Array_Coord(short DIE_ONE_X, short DIE_ONE_FIELD_MIN_X, Coord<short, short> array_coord, int Array_Count)
        {
            var new_x = ((array_coord.X - DIE_ONE_X
                + Math.Floor((double)(DIE_ONE_X - DIE_ONE_FIELD_MIN_X) / Array_Count)) * Array_Count)
                + DIE_ONE_FIELD_MIN_X;
            return new Coord<short, short>((short)Math.Round(new_x, 0), array_coord.Y);
        }

        public DieSortVM()
        {
            X = 0;
            Y = 0;
            DieValue = 0;

            Wafer = "";
            XX = "";
            YY = "";
            BIN = "";
            LayoutId = "";
            MPN = "";
            FPN = "";
            PArray = "";
            Des = "";
            Tech = "";
            MapFile = "";
            UpdateTime = "";
        }

        public DieSortVM(int x, int y, int val)
        {
            X = x;
            Y = y;
            DieValue = val;

            Wafer = "";
            XX = "";
            YY = "";
            BIN = "";
            LayoutId = "";
            MPN = "";
            FPN = "";
            PArray = "";
            Des = "";
            Tech = "";
            MapFile = "";
            UpdateTime = "";
        }

        public DieSortVM(string wf, string x, string y, string bin, string lid, string mpn
            , string fpn, string array, string des, string tec, string mf, string ut)
        {
            Wafer = wf;
            XX = x;
            YY = y;
            BIN = bin;
            LayoutId = lid;
            MPN = mpn;
            FPN = fpn;
            PArray = array;
            Des = des;
            Tech = tec;
            MapFile = mf;
            UpdateTime = ut;
        }

        public int X { set; get; }
        public int Y { set; get; }
        public double DieValue {set;get;}


        public string Wafer { set; get; }
        public string XX { set; get; }
        public string YY { set; get; }
        public string BIN { set; get; }
        public string LayoutId { set; get; }
        public string MPN { set; get; }
        public string FPN { set; get; }
        public string PArray { set; get; }
        public string Des { set; get; }
        public string Tech { set; get; }
        public string MapFile { set; get; }
        public string UpdateTime { set; get; }
    }

    public sealed class Coord<XCoord, YCoord>
: IEquatable<Coord<XCoord, YCoord>>
    {
        private readonly XCoord x;
        private readonly YCoord y;
        public Coord(XCoord x, YCoord y)
        {
            this.x = x;
            this.y = y;
        }
        public XCoord X
        {
            get { return x; }
        }
        public YCoord Y
        {
            get { return y; }
        }
        public bool Equals(Coord<XCoord, YCoord> other)
        {
            if (other == null)
            {
                return false;
            }
            return EqualityComparer<XCoord>.Default.Equals(this.X, other.X) &&
            EqualityComparer<YCoord>.Default.Equals(this.Y, other.Y);
        }
        public override bool Equals(object o)
        {
            return Equals(o as Coord<XCoord, YCoord>);
        }
        public override int GetHashCode()
        {
            return EqualityComparer<XCoord>.Default.GetHashCode(x) * 37 +
                   EqualityComparer<YCoord>.Default.GetHashCode(y);
        }
    }
}