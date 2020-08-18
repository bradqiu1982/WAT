using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace WAT.Models
{
    public class IIVIVcselVM
    {
        public static string SolveIIVIWafer(string wf, List<string> allsrcfile, string mapfolder, string samplefolder
            , Dictionary<string,string> syscfg, Dictionary<string, string> iivipndict)
        {
            //get coordate probe data
            var coordprobedata = GetCoordProbeData(wf, allsrcfile);
            if (coordprobedata.Count == 0)
            { return "FAILED TO GET COORDINATE or PROBE DATA"; }
            var coordlines = coordprobedata[0];
            var probelines = coordprobedata[1];

            //parse map file
            var mapsrcfs = mapfolder + "\\" + wf + ".txt";
            var alllines = System.IO.File.ReadAllLines(mapsrcfs);
            var goodbindict = new Dictionary<string, bool>();
            var goodbinxylist = new List<string>();
            var appendinfo = new StringBuilder();
            var waferinfo = new List<List<string>>();
            var rows = 0;
            var maxcols = 0;
            var device = "";

            var reach_eom = 0;
            foreach (var line in alllines)
            {
                if (reach_eom == 0 && line.Contains("_EOM_"))
                { reach_eom = 1; }

                if (reach_eom == 0)
                {
                    var cols = 0;
                    var templine = new List<string>();
                    foreach (var ch in line)
                    {
                        var val = UT.O2S(ch);

                        if (val.Contains("1"))
                        {
                            var xykey = cols.ToString() + ":::" + rows.ToString();
                            goodbindict.Add(xykey, true);
                            goodbinxylist.Add(xykey);
                            templine.Add("X");
                        }
                        else
                        { templine.Add(val); }
                        cols++;
                    }
                    waferinfo.Add(templine);
                    rows++;
                    if (cols > maxcols)
                    { maxcols = cols; }
                }
                else
                {
                    if (line.ToUpper().Contains("DEVICE"))
                    { device = line.ToUpper().Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1].Trim(); }

                    appendinfo.Append(line);
                    appendinfo.Append("\r\n");
                }
            }

            //check coordinate file with map file
            if (coordlines.Count < goodbinxylist.Count + 1 || coordlines.Count < 2)
            { return "GOOD BIN COUNT NOT MATCH COORDINATE COUNT"; }


            var coords = coordlines[1].Trim().Split(new string[] { "_", " " }, StringSplitOptions.RemoveEmptyEntries);
            var arraysize = UT.O2I(coords[2]) - UT.O2I(coords[0]) + 1;
            var requestcnt = 250;
            if (syscfg.ContainsKey("IIVISAMPLESIZE" + arraysize))
            {
                requestcnt = UT.O2I(syscfg["IIVISAMPLESIZE" + arraysize]);
            }

            //get select dict
            var selectdict = GetSelectDict(goodbindict, maxcols, rows, requestcnt);
            if (selectdict.Count == 0)
            {
                return "NO ENOUGH GOOD BIN";
            }
            else
            {
                var pn = "1350690";
                if (iivipndict.ContainsKey(device))
                { pn = iivipndict[device]; }

                var stat = StoreWaferInfo(wf, coordlines, goodbinxylist,selectdict,pn, probelines);
                if (!string.IsNullOrEmpty(stat)) { return stat; }

                //write sample map file
                foreach (var kv in selectdict)
                {
                    var sxsy = kv.Key.Split(new string[] { ":::" }, StringSplitOptions.RemoveEmptyEntries);
                    var sx = UT.O2I(sxsy[0]);
                    var sy = UT.O2I(sxsy[1]);
                    waferinfo[sy][sx] = "1";
                }

                var samplefs = samplefolder + "\\" + wf + ".txt";
                var samplepicktxt = new StringBuilder();
                foreach (var wline in waferinfo)
                {
                    foreach (var ws in wline)
                    { samplepicktxt.Append(ws); }
                    samplepicktxt.Append("\r\n");
                }
                samplepicktxt.Append(appendinfo);
                System.IO.File.WriteAllText(samplefs, samplepicktxt.ToString());

                return "OK";
            }//end else
        }

        private static string StoreWaferInfo(string wf, List<string> coordlines, List<string> goodbinxylist,Dictionary<string,bool> selectdict,string pn,List<string> probelines)
        {
            try
            {
                var dataexist = IIVICoorExist(wf);
                if (!dataexist)
                {
                    var stat = StoreWaferProbe(wf, probelines);
                    if (!string.IsNullOrEmpty(stat))
                    { return stat; }

                    DieSortVM.StoreWaferSrcData(wf, wf, "1", goodbinxylist.Count.ToString(), "Pass", "GOOD", "true", "100", pn, "");
                }

                var arraysize = 0;
                var bidx = 1;
                foreach (var gb in goodbinxylist)
                {
                    var coords = coordlines[bidx].Trim().Split(new string[] { "_", " " }, StringSplitOptions.RemoveEmptyEntries);
                    arraysize = UT.O2I(coords[2]) - UT.O2I(coords[0]) + 1;
                    var coordstr = coords[0] + ":::" + coords[1];

                    var sampled = "";
                    if (selectdict.ContainsKey(gb))
                    {
                        sampled = "TRUE";
                        StoreIIVISampleData(wf, wf, coords[0], coords[3], arraysize, pn);
                    }
                    StoreIIVICoord(wf, gb, coordstr, arraysize, sampled, dataexist);
                    bidx++;
                }

                if (!dataexist)
                { StoreEvalPN(wf, pn, arraysize); }
            }
            catch (Exception ex)
            {
                return "CRASH IN STORE WAFER INFO";
            }

            return string.Empty;
        }

        private static void StoreIIVISampleData(string mapfile, string wafer, string x,string y,int arraysize,string pn)
        {
            var updatetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var sql = "insert into WaferSampleData(WAFER,X,Y,BIN,MPN,FPN,PArray,MAPFILE,UpdateTime) values(@WAFER,@X,@Y,@BIN,@MPN,@FPN,@PArray,@MAPFILE,@UpdateTime)";
            var dict = new Dictionary<string, string>();
            dict.Add("@WAFER", wafer);
            dict.Add("@X", x);
            dict.Add("@Y", y);
            dict.Add("@BIN", "1");
            dict.Add("@MPN", pn);
            dict.Add("@FPN", pn);
            dict.Add("@PArray", "1X"+arraysize.ToString());
            dict.Add("@MAPFILE", mapfile);
            dict.Add("@UpdateTime", updatetime);
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public static bool IIVICoorExist(string wf)
        {
            var sql = "select top 1 Wafer from [WAT].[dbo].[IIVICoord] where Wafer = @Wafer";
            var dict = new Dictionary<string, string>();
            dict.Add("@Wafer",wf);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            if (dbret.Count > 0)
            { return true; }
            else
            { return false; }
        }

        private static void StoreIIVICoord(string wf,string colrow,string xy,int arraysize,string sampled,bool dataexist)
        {
            if (dataexist)
            {
                var sql = "update [WAT].[dbo].[IIVICoord] set Sampled =@Sampled  where Wafer = @Wafer";
                var dict = new Dictionary<string, string>();
                dict.Add("@Wafer", wf);
                dict.Add("@Sampled", sampled);
                DBUtility.ExeLocalSqlNoRes(sql, dict);
            }
            else
            {
                var sql = "insert into [WAT].[dbo].[IIVICoord](Wafer,ColRow,XY,ArraySize,Sampled) values(@Wafer,@ColRow,@XY,@ArraySize,@Sampled)";
                var dict = new Dictionary<string, string>();
                dict.Add("@Wafer", wf);
                dict.Add("@ColRow", colrow);
                dict.Add("@XY", xy);
                dict.Add("@ArraySize", arraysize.ToString());
                dict.Add("@Sampled", sampled);
                DBUtility.ExeLocalSqlNoRes(sql, dict);
            }
        }

        public static Dictionary<string, bool> GetIIVICoordDict(string wafer)
        {
            var sql = "select XY,ArraySize from [WAT].[dbo].[IIVICoord] where Wafer=@Wafer";
            var dict = new Dictionary<string, string>();
            dict.Add("@Wafer", wafer);
            var ret = new Dictionary<string, bool>();
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                var xystr = UT.O2S(line[0]).Split(new string[] { ":::" }, StringSplitOptions.RemoveEmptyEntries);
                var sz = UT.O2I(line[1]);

                var x = UT.O2I(xystr[0]);
                var y = UT.O2I(xystr[1]);

                for (var idx = 0; idx < sz; idx++)
                {
                    var k = (x + idx).ToString() + ":::" + y.ToString();
                    if (!ret.ContainsKey(k))
                    { ret.Add(k, true); }
                }
            }
            return ret;
        }

        private static string StoreWaferProbe(string wf,List<string> probelines)
        {
            var xyidx = 2;
            var slopidx = 3;
            var ithidx = 6;
            var ridx = 15;

            if (probelines.Count < 2)
            { return "NO PROBE TEST DATA"; }

            var idx = 0;
            var titles = probelines[0].ToUpper().Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var t in titles)
            {
                if (t.Contains("LASER"))
                { xyidx = idx; }
                if (string.Compare(t, "ETA") == 0)
                { slopidx = idx; }
                if (string.Compare(t, "ITH") == 0)
                { ithidx = idx; }
                if (string.Compare(t, "R6") == 0)
                { ridx = idx; }
                idx++;
            }

            var idxlist = new List<int>();
            idxlist.Add(xyidx);idxlist.Add(slopidx);
            idxlist.Add(ithidx);idxlist.Add(ridx);

            var nessarycols = idxlist.Max();

            var sql = @"insert into [EngrData].[dbo].[VR_Eval_Pts_Data_Basic](EntryTime,WaferID,Xcoord,Ycoord,Ith,Wafer,SeriesR,SlopEff) 
                        values(@EntryTime,@WaferID,@Xcoord,@Ycoord,@Ith,@Wafer,@SeriesR,@SlopEff)";
            var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            for (var i = 1;i < probelines.Count;i++)
            {
                var datas = probelines[i].Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (datas.Count > nessarycols)
                {
                    var xys = datas[xyidx].Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (xys.Count == 2)
                    {
                        var dict = new Dictionary<string, string>();
                        dict.Add("@EntryTime", now);
                        dict.Add("@WaferID", wf);
                        dict.Add("@Xcoord", xys[0]);
                        dict.Add("@Ycoord", xys[1]);
                        dict.Add("@Ith", datas[ithidx]);
                        dict.Add("@Wafer", wf);
                        dict.Add("@SeriesR", datas[ridx]);
                        dict.Add("@SlopEff", datas[slopidx]);

                        DBUtility.ExeLocalSqlNoRes(sql, dict);
                    }//end if
                }//end if
           }//end for

            return string.Empty;
        }

        private static void StoreEvalPN(string wf,string pn, int arraysize)
        {
            var templist = new List<WXEvalPN>();
            var tempvm = new WXEvalPN();
            tempvm.EvalPN = pn + "_B";
            tempvm.DCDName = "Eval_50up_rp";
            tempvm.LotType = "W";
            tempvm.EvalBin = "E08";
            templist.Add(tempvm);

            tempvm = new WXEvalPN();
            tempvm.EvalPN = pn + "_C";
            tempvm.DCDName = "Eval_50up_rp";
            tempvm.LotType = "W";
            tempvm.EvalBin = "E06";
            templist.Add(tempvm);

            tempvm = new WXEvalPN();
            tempvm.EvalPN = pn + "_HB";
            tempvm.DCDName = "Eval_COB_rp";
            tempvm.LotType = "W";
            tempvm.EvalBin = "E10";
            templist.Add(tempvm);

            foreach (var item in templist)
            {
                var sql = @"insert into WAT.dbo.WXEvalPN(WaferNum,EvalPN,DCDName,LotType,EvalBinName,Product,AppVal1) values(@WaferNum,@EvalPN,@DCDName,@LotType,@EvalBinName,@Product,@array)";
                var dict = new Dictionary<string, string>();
                dict.Add("@WaferNum", wf);
                dict.Add("@EvalPN", item.EvalPN);
                dict.Add("@DCDName", item.DCDName);
                dict.Add("@LotType", item.LotType);
                dict.Add("@EvalBinName", item.EvalBin);
                dict.Add("@Product", pn);
                dict.Add("@array", arraysize.ToString());
                DBUtility.ExeLocalSqlWithRes(sql, dict);
            }

            var rate = new WXEvalPNRate();
            rate.EvalPN = pn;
            rate.RealRate = "25G";
            rate.TreatRate = "25G";
            rate.UpdatePNRate();
        }

        private static List<List<string>> GetCoordProbeData(string wf, List<string> allsrcfile)
        {
            try
            {
                var coordfile = "";
                var probefile = "";
                foreach (var sf in allsrcfile)
                {
                    var ssf = sf.ToUpper();
                    if (ssf.Contains(wf) && ssf.Contains("GOOD.TXT"))
                    { coordfile = sf; }
                    if (ssf.Contains(wf) && ssf.Contains("_REPORT.TXT"))
                    { probefile = sf; }

                    if (!string.IsNullOrEmpty(coordfile) && !string.IsNullOrEmpty(probefile))
                    { break; }
                }

                var coordlines = System.IO.File.ReadAllLines(coordfile).ToList();
                var probelines = System.IO.File.ReadAllLines(probefile).ToList();
                var ret = new List<List<string>>();
                ret.Add(coordlines);
                ret.Add(probelines);
                return ret;
            }
            catch (Exception ex) { return new List<List<string>>(); }

        }

        public static string CheckWaferFile(string wf, List<string> allsamplefile, Dictionary<string, bool> samplefsdict
            , List<string> allsrcfile, Dictionary<string, bool> srcdict)
        {
            if (samplefsdict.ContainsKey(wf))
            { return "SAMPLE MAP FILE EXIST"; }

            if (srcdict.ContainsKey(wf + ".TXT"))
            {
                var coordfile = "";
                var probefile = "";

                foreach (var sf in allsrcfile)
                {
                    var ssf = sf.ToUpper();
                    if (ssf.Contains(wf) && ssf.Contains("GOOD.TXT"))
                    { coordfile = sf; }
                    if (ssf.Contains(wf) && ssf.Contains("_REPORT.TXT"))
                    { probefile = sf; }

                    if (!string.IsNullOrEmpty(coordfile) && !string.IsNullOrEmpty(probefile))
                    { break; }
                }

                if (string.IsNullOrEmpty(coordfile) || string.IsNullOrEmpty(probefile))
                { return "COORDINATE FILE or PROBE FILE NOT EXIST"; }
            }
            else
            { return "ORIGINAL MAP FILE NOT EXIST"; }

            return string.Empty;
        }

        private static Dictionary<string, bool> GetSelectDict(Dictionary<string, bool> goodbindict, int cols, int rows, int requestcnt)
        {
            var selectdict = new Dictionary<string, bool>();
            var halfrow = rows / 2;
            var halfcol = cols / 2;
            var rowlist = new List<int>();
            var collist = new List<int>();
            rowlist.Add(0); rowlist.Add(halfrow);
            collist.Add(0); collist.Add(halfcol);

            var rad = new Random(DateTime.Now.Millisecond);
            var trytimes = 0;
            var maxtimes = 140000;
            while (true)
            {
                foreach (var c in collist)
                {
                    foreach (var r in rowlist)
                    {
                        var xrad = rad.Next(halfcol);
                        var yrad = rad.Next(halfrow);
                        var xykey = (c + xrad).ToString() + ":::" + (r + yrad).ToString();
                        if (goodbindict.ContainsKey(xykey) && !selectdict.ContainsKey(xykey))
                        {
                            selectdict.Add(xykey, true);
                            if (selectdict.Count >= requestcnt)
                            {
                                return selectdict;
                            }
                        }
                    }
                }

                if (trytimes > maxtimes)
                { break; }
                trytimes++;
            }

            return new Dictionary<string, bool>();
        }


    }
}