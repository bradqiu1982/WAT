using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.IO;
using System.Web.Mvc;
using System.Text;

namespace WAT.Models
{

    public class WAFERARRAY {
        public static string ARRAY1X1 = "1X1";
        public static string ARRAY1X4 = "1X4";
        public static string ARRAY1X12 = "1X12";
    }

    public class DieSortVM
    {
        public static void LoadAllDieSortFile(Controller ctrl)
        {
            var filetype = "DIESORT";
            var syscfgdict = CfgUtility.GetSysConfig(ctrl);
            var srcfolder = syscfgdict["DIESORTFOLDER"];
            var srcfiles = ExternalDataCollector.DirectoryEnumerateFiles(ctrl, srcfolder);

            var loadedfiledict = FileLoadedData.LoadedFiles(filetype);
            foreach (var srcf in srcfiles)
            {
                var srcfilename = Path.GetFileName(srcf).ToUpper();
                if (loadedfiledict.ContainsKey(srcfilename))
                { continue; }

                var desfile = ExternalDataCollector.DownloadShareFile(srcf, ctrl);
                if (desfile != null && ExternalDataCollector.FileExist(ctrl, desfile))
                {
                    if (SolveDieSortFile(desfile, ctrl))
                    {
                        FileLoadedData.UpdateLoadedFile(srcfilename, filetype);
                    }
                }
            }
        }

        public static bool LoadDieSortFile(Controller ctrl, string srcname)
        {
            var filetype = "DIESORT";
            var syscfgdict = CfgUtility.GetSysConfig(ctrl);
            var srcfolder = syscfgdict["DIESORTFOLDER"];
            var srcf = Path.Combine(srcfolder, srcname.ToUpper());
            if (ExternalDataCollector.FileExist(ctrl, srcf))
            {
                var desfile = ExternalDataCollector.DownloadShareFile(srcf, ctrl);
                if (desfile != null && ExternalDataCollector.FileExist(ctrl, desfile))
                {
                    FileLoadedData.RemoveLoadedFile(srcname.ToUpper(), filetype);
                    if (SolveDieSortFile(desfile, ctrl))
                    {
                        FileLoadedData.UpdateLoadedFile(srcname.ToUpper(), filetype);
                        return true;
                    }
                }//end if
            }//end if

            return false;
        }

        private static bool SolveDieSortFile(string diefile, Controller ctrl)
        {
            var syscfgdict = CfgUtility.GetSysConfig(ctrl);
            var desfolder = syscfgdict["DIESORTSHARE"];
            var reviewfolder = syscfgdict["DIESORTREVIEW"];

            var fs = Path.GetFileName(diefile);

            try
            {
                //get modify information
                var doc = new XmlDocument();
                doc.Load(diefile);
                var namesp = doc.DocumentElement.GetAttribute("xmlns");
                doc = StripNamespace(doc);
                XmlElement root = doc.DocumentElement;

                var wafer = "";
                var wfnodes = root.SelectNodes("//Substrate[@SubstrateId]");
                if (wfnodes.Count > 0)
                {
                    wafer = ((XmlElement)wfnodes[0]).GetAttribute("SubstrateId").Trim();
                }
                if (string.IsNullOrEmpty(wafer))
                {
                    WebLog.Log(fs,"DIESORT", "fail to convert file:" + diefile + ", fail to get wafer");
                    return false;
                }

                var arrayinfo = GetArrayFromWafer(fs,wafer);
                if (arrayinfo.Count == 0)
                {
                    WebLog.Log(fs,"DIESORT", "fail to convert file:" + diefile + ", fail to get array info by wafer " + wafer);
                    return false;
                }

                var array = arrayinfo[0];
                var bomdesc = System.Security.SecurityElement.Escape(arrayinfo[1]);
                var bompn = arrayinfo[2];
                var fpn = arrayinfo[3];
                var product = System.Security.SecurityElement.Escape(arrayinfo[4]);

                var selectcount = 0;
                if (syscfgdict.ContainsKey("DIESORTSAMPLE"+array))
                {
                    selectcount = Convert.ToInt32(syscfgdict["DIESORTSAMPLE" + array]);
                }
                else
                {
                    WebLog.Log(fs,"DIESORT", "fail to convert file:" + diefile + ", fail to get select count by array info DIESORTSAMPLE" + array);
                    return false;
                }

                var passedbinxydict = GetPassedBinXYDict(root);
                if (passedbinxydict.Count == 0)
                {
                    WebLog.Log(fs,"DIESORT", "fail to convert file:" + diefile + ", fail to get good bin xy dict");
                    return false;
                }

                var selectxydict = GetSelectedXYDict(passedbinxydict, selectcount,array);
                if (selectxydict.Count == 0)
                {
                    WebLog.Log(fs,"DIESORT", "fail to convert file:" + diefile + ", fail to get sample xy dict");
                    return false;
                }


                //try to write review file
                foreach (var kv in selectxydict)
                {
                    var xystr = kv.Key.Split(new string[] { ":::" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (XmlElement nd in root.SelectNodes("//BinCode[@X='" + xystr[0] + "' and @Y='" + xystr[1] + "']"))
                    { nd.SetAttribute("diesort", "selected"); }
                }

                var layoutnodes = root.SelectNodes("//Layout[@LayoutId]");
                if (layoutnodes.Count > 0)
                {
                    ((XmlElement)layoutnodes[0]).SetAttribute("BOMPN", bompn);
                    ((XmlElement)layoutnodes[0]).SetAttribute("FPN", fpn);
                    ((XmlElement)layoutnodes[0]).SetAttribute("PRODUCT", product);
                    ((XmlElement)layoutnodes[0]).SetAttribute("ARRAY", array);
                    ((XmlElement)layoutnodes[0]).SetAttribute("BOMDES", bomdesc);
                }

                doc.DocumentElement.SetAttribute("xmlns", namesp);
                var savename = Path.Combine(reviewfolder, Path.GetFileName(diefile));
                if (ExternalDataCollector.FileExist(ctrl, savename))
                { ExternalDataCollector.FileDelete(ctrl, savename); }
                doc.Save(savename);



                //try to write actual file
                doc = new XmlDocument();
                doc.Load(diefile);
                doc = StripNamespace(doc);
                root = doc.DocumentElement;
                foreach (var kv in selectxydict)
                {
                    var xystr = kv.Key.Split(new string[] { ":::" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (XmlElement nd in root.SelectNodes("//BinCode[@X='" + xystr[0] + "' and @Y='" + xystr[1] + "']"))
                    {
                        nd.InnerText = "A";
                    }
                }

                layoutnodes = root.SelectNodes("//Layout[@LayoutId]");
                if (layoutnodes.Count > 0)
                {
                    ((XmlElement)layoutnodes[0]).SetAttribute("BOMPN", bompn);
                    ((XmlElement)layoutnodes[0]).SetAttribute("FPN", fpn);
                    ((XmlElement)layoutnodes[0]).SetAttribute("PRODUCT", product);
                    ((XmlElement)layoutnodes[0]).SetAttribute("ARRAY", array);
                    ((XmlElement)layoutnodes[0]).SetAttribute("BOMDES", bomdesc);
                }

                doc.DocumentElement.SetAttribute("xmlns", namesp);
                savename = Path.Combine(desfolder, Path.GetFileName(diefile));
                if (ExternalDataCollector.FileExist(ctrl, savename))
                { ExternalDataCollector.FileDelete(ctrl, savename); }
                doc.Save(savename);


                //write sample X,Y csv file for sharing
                var sb = new StringBuilder();
                try
                {
                    var csvfile = Path.Combine(reviewfolder, Path.GetFileName(diefile) + ".csv");
                    if (ExternalDataCollector.FileExist(ctrl, csvfile))
                    { ExternalDataCollector.FileDelete(ctrl, csvfile); }

                    sb.Append("X,Y,BIN,\r\n");
                    foreach (var kv in selectxydict)
                    {
                        sb.Append(kv.Key.Replace(":::", ",") + "," + kv.Value + ",\r\n");
                    }
                    File.WriteAllText(csvfile, sb.ToString());
                }
                catch (Exception e)
                {
                    WebLog.Log(fs,"DIESORT", "fail to convert file:" + diefile + " reason:" + e.Message);
                    try
                    {
                        var csvfile = Path.Combine(reviewfolder, Path.GetFileName(diefile) + "-new.csv");
                        File.WriteAllText(csvfile, sb.ToString());
                    }
                    catch (Exception f) { WebLog.Log(fs,"DIESORT", "fail to convert file:" + diefile + " reason:" + f.Message); }
                }

                var mapfile = Path.GetFileName(diefile);
                //write sample X,Y database
                StoreWaferSampleData(mapfile, wafer, selectxydict, array, bompn, fpn);
                //write wafer related data
                StoreWaferPassBinData(mapfile, wafer, selectxydict,passedbinxydict, array, bompn, fpn,bomdesc,product);

            }
            catch (Exception ex) {
                WebLog.Log(fs,"DIESORT", "fail to convert file:" + diefile + " reason:" + ex.Message);
                return false;
            }

            return true;
        }

        private static void StoreWaferSampleData(string mapfile, string wafer, Dictionary<string, string> selectxy, string array,string mpn,string fpn)
        {
            var sql = "delete from WaferSampleData where MAPFILE = '<MAPFILE>'";
            sql = sql.Replace("<MAPFILE>", mapfile);
            DBUtility.ExeLocalSqlNoRes(sql);

            var updatetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            foreach (var kv in selectxy)
            {
                var xystr = kv.Key.Split(new string[] { ":::" }, StringSplitOptions.RemoveEmptyEntries);
                var x = xystr[0];
                var y = xystr[1];
                sql = "insert into WaferSampleData(WAFER,X,Y,BIN,MPN,FPN,PArray,MAPFILE,UpdateTime) values(@WAFER,@X,@Y,@BIN,@MPN,@FPN,@PArray,@MAPFILE,@UpdateTime)";
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
        }


        private static void StoreWaferPassBinData(string mapfile, string wafer, Dictionary<string, string> selectxy, Dictionary<string, string> passxy, string array, string mpn, string fpn,string pdesc,string product)
        {
            var sql = "delete from WaferPassBinData where MAPFILE = '<MAPFILE>'";
            sql = sql.Replace("<MAPFILE>", mapfile);
            DBUtility.ExeLocalSqlNoRes(sql);

            var channel = Convert.ToInt32(array.Replace("1X", ""));
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
                {samplecountdict[kv.Value] += channel;}
                else
                {samplecountdict.Add(kv.Value, channel);}
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

        private static XmlDocument StripNamespace(XmlDocument doc)
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

        private static bool CheckNextXDie(int xval, int yval, int count, Dictionary<string, string> passedbinxydict)
        {
            var idx = 0;
            var newxval = xval;
            while (idx < count)
            {
                newxval = newxval + 1;
                var k = newxval.ToString() + ":::" + yval.ToString();
                if (!passedbinxydict.ContainsKey(k))
                {
                    return false;
                }
                idx = idx + 1;
            }
            return true;
        }

        private static Dictionary<string, string> GetSelectedXYDict(Dictionary<string, string> passedbinxydict,int selectcount,string array)
        {
            var channel = Convert.ToInt32(array.Replace("1X", ""));

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

            var tempxlist = xdict.Keys.ToList();
            var ylist = ydict.Keys.ToList();

            tempxlist.Sort();
            ylist.Sort();

            var xlist = new List<int>();
            foreach (var x in tempxlist)
            {
                if ((x - 1) % channel == 0)
                {
                    xlist.Add(x);
                }
            }
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

                        if (passedbinxydict.ContainsKey(k) && !ret.ContainsKey(k))
                        {
                            if (channel > 1)
                            {
                                if (!CheckNextXDie(xval,yval,channel-1,passedbinxydict))
                                { continue; }
                            }

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

        private static List<string> GetArrayFromWafer(string fs,string wafer)
        {
            var sql = @"select distinct ProductName from [InsiteDB].[insite].ProductBase where ProductBaseId in (
                            select ProductBaseId from  [InsiteDB].[insite].Product
                            where ProductId  in (
                                SELECT distinct hml.ProductId FROM [InsiteDB].[insite].[dc_IQC_InspectionResult] (nolock) aoc 
                                left join [InsiteDB].[insite].HistoryMainline (nolock) hml on aoc.[HistoryMainlineId] = hml.HistoryMainlineId
                                where ParamValueString like '%<wafer>%'))";

            sql = sql.Replace("<wafer>", wafer);
            var pnlist = new List<string>();
            var dbret = DBUtility.ExeMESSqlWithRes(sql);
            foreach (var line in dbret)
            {
                pnlist.Add(Convert.ToString(line[0]));
            }


            if (pnlist.Count == 0)
            {
                sql = @"select distinct ProductName from [InsiteDB].[insite].ProductBase where ProductBaseId in (
                            select ProductBaseId from  [InsiteDB].[insite].Product
                            where ProductId  in (
                                SELECT distinct hml.ProductId FROM [InsiteDB].[insite].[dc_AOC_ManualInspection] (nolock) aoc 
                                left join [InsiteDB].[insite].HistoryMainline (nolock) hml on aoc.[HistoryMainlineId] = hml.HistoryMainlineId
                                where ParamValueString like '%<wafer>%'))";
                sql = sql.Replace("<wafer>", wafer);
                dbret = DBUtility.ExeMESSqlWithRes(sql);
                foreach (var line in dbret)
                {
                    pnlist.Add(Convert.ToString(line[0]));
                }
            }

            if (pnlist.Count == 0)
            {
                sql = @"SELECT distinct pb.ProductName
                          FROM [InsiteDB].[insite].[Container]  (nolock) c
                          left join [InsiteDB].[insite].Product (nolock) p on c.ProductId = p.ProductId
                          left join [InsiteDB].[insite].ProductBase (nolock) pb on pb.ProductBaseId = p.ProductBaseId
                           where c.SupplierLotNumber like '%<wafer>%'";

                sql = sql.Replace("<wafer>", wafer);
                dbret = DBUtility.ExeMESSqlWithRes(sql);
                foreach (var line in dbret)
                {
                    pnlist.Add(Convert.ToString(line[0]));
                }
            }

            if (pnlist.Count > 0)
            {
                var pncond = "('" + string.Join("','", pnlist) + "')";
                sql = "Select distinct PArray,[Desc],MPN,FPN,Product from WaferArray where (MPN in <pncond> or FPN in <pncond>)";
                sql = sql.Replace("<pncond>", pncond);
                dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
                foreach (var line in dbret)
                {
                    var tempvm = new List<string>();
                    tempvm.Add(Convert.ToString(line[0]).Trim().ToUpper());
                    tempvm.Add(Convert.ToString(line[1]).Trim().ToUpper());
                    tempvm.Add(Convert.ToString(line[2]).Trim().ToUpper());
                    tempvm.Add(Convert.ToString(line[3]).Trim().ToUpper());
                    tempvm.Add(Convert.ToString(line[4]).Trim().ToUpper());
                    return tempvm;
                }

                WebLog.Log(fs,"DIESORT", "fail to get array by pn " + pncond);
            }
            else
            {
                WebLog.Log(fs,"DIESORT", "fail to get pn by wafer " + wafer);
            }

            return new List<string>();

            //return "1X12";
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
                    { valdict[k] = 10; }
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
            var xlist = new List<int>();
            var ylist = new List<int>();
            foreach (var v in datalist)
            {
                var templist = new List<object>();
                templist.Add(v.X);
                templist.Add(v.Y);
                templist.Add(v.DieValue);
                data.Add(templist);

                xlist.Add(v.X);
                ylist.Add(v.Y);
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


        public DieSortVM()
        {
            X = 0;
            Y = 0;
            DieValue = 0;
        }

        public DieSortVM(int x, int y, int val)
        {
            X = x;
            Y = y;
            DieValue = val;
        }

        public int X { set; get; }
        public int Y { set; get; }
        public double DieValue {set;get;}


    }
}