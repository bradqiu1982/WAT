using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WXLogic
{
    public class SixIMapData
    {
        public SixIMapData()
        {
            X = 0;
            Y = 0;
            Bin = "99";
            BinName = "";
            PN = "";
        }

        public int X { set; get; }
        public int Y { set; get; }
        public string Bin { set; get; }
        public string BinName { set; get; }
        public string PN { set; get; }
    }

    public class SixIMapFile
    {
        public static bool GenerateMapFile(string wafer, string binfile)
        {
            var smapfile = SixIMapFile.LoadMapFileData(wafer);
            if (smapfile.BaseArrayCoordinate.Count > 0 && smapfile.BinArrayCoordinate.Count > 0)
            {
                return smapfile.RealGenerateMapFile(binfile);
            }
            return true;
        }

        private static SixIMapFile LoadMapFileData(string wafer)
        {
            var ret = new SixIMapFile();
            ret.ProdFam = WXEvalPN.GetProductFamilyFromSherman(wafer);
            ret.ArraySize = UT.O2I(GetWaferArrayInfoByPF(ret.ProdFam));
            ret.BaseArrayCoordinate = GetBaseArrayCoordinate(ret.ProdFam);
            foreach (var bc in ret.BaseArrayCoordinate)
            {
                if (bc.BinName.ToUpper().Contains("DIE_ONE"))
                {
                    ret.DieOne.X = bc.X;
                    ret.DieOne.Y = bc.Y;
                    ret.DieOne.Bin = bc.Bin;
                    ret.DieOne.BinName = bc.BinName;
                    break;
                }
            }//end foreach

            if (string.IsNullOrEmpty(ret.DieOne.BinName))
            {
                return ret;
            }

            var minxlist = new List<int>();
            foreach (var bc in ret.BaseArrayCoordinate)
            {
                if (bc.Y == ret.DieOne.Y)
                {
                    minxlist.Add(bc.X);
                }
            }

            if (minxlist.Count > 0)
            {
                ret.DieOneMin.X = minxlist.Min();
                ret.DieOneMin.Y = ret.DieOne.Y;
            }

            ret.BinArrayCoordinate = GetBinArrayCoordinate(ret.ArraySize, wafer);
            ret.Wafer = wafer;

            return ret;
        }



        private bool RealGenerateMapFile(string binfile)
        {
            var ret = true;

            var xygooddict = new Dictionary<string, SixIMapData>();
            var goodbincount = new Dictionary<string, int>();
            foreach (var bc in BinArrayCoordinate)
            {
                var key = UT.O2S(bc.X) + ":::" + UT.O2S(bc.Y);
                if (!xygooddict.ContainsKey(key))
                {
                    xygooddict.Add(key, bc);
                }
                PN = bc.PN;

                if (goodbincount.ContainsKey(bc.Bin))
                { goodbincount[bc.Bin] += 1; }
                else
                { goodbincount.Add(bc.Bin, 1); }
            }

            var bin32cnt = 0;
            var bin34cnt = 0;
            var orgbin99cnt = 0;
            foreach (var bc in BaseArrayCoordinate)
            {
                var key = UT.O2S(bc.X) + ":::" + UT.O2S(bc.Y);
                if (xygooddict.ContainsKey(key))
                {
                    bc.Bin = xygooddict[key].Bin;
                    bc.BinName = xygooddict[key].BinName;
                }

                if (string.Compare(bc.Bin, "32") == 0)
                { bin32cnt++; }
                if (string.Compare(bc.Bin, "34") == 0)
                { bin34cnt++; }
                if (string.Compare(bc.Bin, "99") == 0)
                { orgbin99cnt++; }
            }

            //var bin99cnt = 0;
            //var samplecoordinates = new List<SixIMapData>();
            //foreach (var bc in BaseArrayCoordinate)
            //{
            //    var tempvm = new SixIMapData();
            //    tempvm.X = bc.X;
            //    tempvm.Y = bc.Y;
            //    tempvm.Bin = bc.Bin;
            //    tempvm.BinName = bc.BinName;

            //    var key = UT.O2S(bc.X) + ":::" + UT.O2S(bc.Y);
            //    var bin = UT.O2I(bc.Bin);
            //    if (bin >= 30 && bin < 40)
            //    { }
            //    else
            //    {
            //        if (sampledict.ContainsKey(key))
            //        { tempvm.Bin = "1"; }
            //        else
            //        {
            //            tempvm.Bin = "99";
            //            bin99cnt++;
            //        }
            //    }

            //    samplecoordinates.Add(tempvm);
            //}

            var dieonedict = new Dictionary<string, bool>();
            dieonedict.Add(UT.O2S(DieOne.X) + ":::" + UT.O2S(DieOne.Y), true);

            StringBuilder headsb = new StringBuilder();
            headsb.Append("<?xml version=\"1.0\" encoding=\"ASCII\"?>\r\n");
            headsb.Append("<MapData xmlns=\"urn:semi-org:xsd.E142-1.V0211.SubstrateMap\">\r\n");
            headsb.Append("<Layouts><Layout LayoutId=\"" + PN + "\" DefaultUnits=\"microns\" TopLevel=\"true\" BOMPN=\"XXXXXXX\" FPN=\"XXXXXXX\" PRODUCT=\"VCSEL\" ARRAY=\"1X" + ArraySize + "\" BOMDES=\"PMA4\"><Dimension X=\"1\" Y=\"1\" /><DeviceSize X=\"1000\" Y=\"250\" /></Layout></Layouts>\r\n");
            headsb.Append("<Substrates><Substrate SubstrateType=\"Wafer\" SubstrateId=\"" + Wafer + "\" /></Substrates>\r\n");
            headsb.Append("<SubstrateMaps><SubstrateMap SubstrateType=\"Wafer\" SubstrateId=\"" + Wafer + "\" LayoutSpecifier=\"" + PN + "\"><Overlay MapName=\"BinMap_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + "\">\r\n");
            headsb.Append("<ReferenceDevices><ReferenceDevice Name=\"DIE_ONE\"><Coordinates X=\"" + DieOne.X + "\" Y=\"" + DieOne.Y + "\" /></ReferenceDevice></ReferenceDevices>\r\n");
            headsb.Append("<BinCodeMap BinType=\"Ascii\" NullBin=\".\" MapType=\"Coordinate\"><BinDefinitions>\r\n");
            headsb.Append("<BinDefinition BinCode=\"" + DieOne.Bin + "\" BinCount=\"1\" BinQuality=\"Skip\" BinDescription=\"DIE_ONE\" Pick=\"false\" />\r\n");
            headsb.Append("<BinDefinition BinCode=\"32\" BinCount=\"" + bin32cnt + "\" BinQuality=\"Skip\" BinDescription=\"INK_ONLY\" Pick=\"false\" />\r\n");
            headsb.Append("<BinDefinition BinCode=\"34\" BinCount=\"" + bin34cnt + "\" BinQuality=\"Skip\" BinDescription=\"HAM\" Pick=\"false\" />\r\n");


            //StringBuilder samplesb = new StringBuilder();
            //samplesb.Append("<BinDefinition BinCode=\"99\" BinCount=\"" + bin99cnt + "\" BinDescription=\"Bin_99\" Pick=\"false\" />\r\n");
            //samplesb.Append("<BinDefinition BinCode=\"1\" BinCount=\"" + sampledict.Count + "\" BinDescription=\"GOOD1\" Pick=\"true\" />\r\n");
            //samplesb.Append("</BinDefinitions><BinCode X=\"" + DieOne.X + "\" Y=\"" + DieOne.Y + "\">" + DieOne.Bin + "</BinCode>\r\n");
            //foreach (var bc in samplecoordinates)
            //{
            //    var key = UT.O2S(bc.X) + ":::" + UT.O2S(bc.Y);
            //    if (!dieonedict.ContainsKey(key))
            //    { samplesb.Append("<BinCode X=\"" + bc.X + "\" Y=\"" + bc.Y + "\">" + bc.Bin + "</BinCode>\r\n"); }
            //}


            StringBuilder orgsb = new StringBuilder();
            orgsb.Append("<BinDefinition BinCode=\"99\" BinCount=\"" + orgbin99cnt + "\" BinDescription=\"Bin_99\" Pick=\"false\" />\r\n");
            foreach (var kv in goodbincount)
            {
                orgsb.Append("<BinDefinition BinCode=\"" + kv.Key + "\" BinCount=\"" + kv.Value + "\" BinQuality=\"Pass\" BinDescription=\"GOOD\" Pick=\"true\" />\r\n");
            }
            orgsb.Append("</BinDefinitions><BinCode X=\"" + DieOne.X + "\" Y=\"" + DieOne.Y + "\">" + DieOne.Bin + "</BinCode>\r\n");
            foreach (var bc in BaseArrayCoordinate)
            {
                var key = UT.O2S(bc.X) + ":::" + UT.O2S(bc.Y);
                if (!dieonedict.ContainsKey(key))
                { orgsb.Append("<BinCode X=\"" + bc.X + "\" Y=\"" + bc.Y + "\">" + bc.Bin + "</BinCode>\r\n"); }
            }

            //StringBuilder reviewsb = new StringBuilder();
            //reviewsb.Append("<BinDefinition BinCode=\"99\" BinCount=\"" + orgbin99cnt + "\" BinDescription=\"Bin_99\" Pick=\"false\" />\r\n");
            //foreach (var kv in goodbincount)
            //{
            //    reviewsb.Append("<BinDefinition BinCode=\"" + kv.Key + "\" BinCount=\"" + kv.Value + "\" BinQuality=\"Pass\" BinDescription=\"GOOD\" Pick=\"true\" />\r\n");
            //}
            //reviewsb.Append("</BinDefinitions><BinCode X=\"" + DieOne.X + "\" Y=\"" + DieOne.Y + "\">" + DieOne.Bin + "</BinCode>\r\n");
            //foreach (var bc in BaseArrayCoordinate)
            //{
            //    var key = UT.O2S(bc.X) + ":::" + UT.O2S(bc.Y);
            //    if (!dieonedict.ContainsKey(key))
            //    {
            //        if (xygooddict.ContainsKey(key) && sampledict.ContainsKey(key))
            //        { reviewsb.Append("<BinCode X=\"" + bc.X + "\" Y=\"" + bc.Y + "\" Q=\"pass\" diesort=\"selected\">" + bc.Bin + "</BinCode>\r\n"); }
            //        else if (xygooddict.ContainsKey(key))
            //        { reviewsb.Append("<BinCode X=\"" + bc.X + "\" Y=\"" + bc.Y + "\" Q=\"pass\">" + bc.Bin + "</BinCode>\r\n"); }
            //        else
            //        { reviewsb.Append("<BinCode X=\"" + bc.X + "\" Y=\"" + bc.Y + "\">" + bc.Bin + "</BinCode>\r\n"); }
            //    }
            //}


            StringBuilder endsb = new StringBuilder();
            endsb.Append("</BinCodeMap></Overlay></SubstrateMap></SubstrateMaps></MapData>\r\n");

            StringBuilder orgfilesb = new StringBuilder();
            orgfilesb.Append(headsb.ToString());
            orgfilesb.Append(orgsb.ToString());
            orgfilesb.Append(endsb.ToString());

            System.IO.File.WriteAllText(binfile, orgfilesb.ToString());

            //StringBuilder samplefilesb = new StringBuilder();
            //samplefilesb.Append(headsb.ToString());
            //samplefilesb.Append(samplesb.ToString());
            //samplefilesb.Append(endsb.ToString());

            //System.IO.File.WriteAllText(diesortnewfile, samplefilesb.ToString());

            //StringBuilder reviewfilesb = new StringBuilder();
            //reviewfilesb.Append(headsb.ToString());
            //reviewfilesb.Append(reviewsb.ToString());
            //reviewfilesb.Append(endsb.ToString());

            //System.IO.File.WriteAllText(reviewfile, reviewfilesb.ToString());

            return ret;
        }

        public List<SixIMapData> GetBaseSingleCoordinate()
        {
            var ret = new List<SixIMapData>();
            if (BaseArrayCoordinate.Count > 0)
            {
                if (ArraySize == 1)
                {
                    return BaseArrayCoordinate;
                }
                else
                {
                    var axlist = new List<string>();
                    var aylist = new List<string>();
                    foreach (var bc in BaseArrayCoordinate)
                    {
                        axlist.Add(bc.X.ToString());
                        aylist.Add(bc.Y.ToString());
                        //var x = Get_First_Singlet_From_Array_Coord(DieOne.X, DieOneMin.X, bc.X, ArraySize);
                        //for(var die = 0; die < ArraySize; die++)
                        //{
                        //    var tempvm = new SixInchMapData();
                        //    tempvm.X = x + die;
                        //    tempvm.Y = bc.Y;
                        //    tempvm.Bin = bc.Bin;
                        //    tempvm.BinName = bc.BinName;
                        //    ret.Add(tempvm);
                        //}
                    }//end foreach

                    var xylist = SixInchArray2SingletCoord(ProdFam, axlist, aylist);
                    var xlist = xylist[0];
                    var ylist = xylist[1];
                    var idx = 0;
                    foreach (var bc in BaseArrayCoordinate)
                    {
                        for (var die = 0; die < ArraySize; die++)
                        {
                            var tempvm = new SixIMapData();
                            tempvm.X = xlist[idx] + die;
                            tempvm.Y = bc.Y;
                            tempvm.Bin = bc.Bin;
                            tempvm.BinName = bc.BinName;
                            ret.Add(tempvm);
                        }
                        idx++;
                    }
                }
            }
            return ret;
        }

        public Dictionary<string, string> GetSampleDict(int samplesize)
        {
            var xlist = new List<int>();
            var ylist = new List<int>();
            var xydict = new Dictionary<string, string>();
            var sampledict = new Dictionary<string, string>();
            var maxtimes = samplesize * 100;
            foreach (var bc in BinArrayCoordinate)
            {
                xlist.Add(bc.X);
                ylist.Add(bc.Y);
                var key = UT.O2S(bc.X) + ":::" + UT.O2S(bc.Y);
                if (!xydict.ContainsKey(key))
                { xydict.Add(key, bc.Bin.ToString()); }
            }
            xlist.Sort();
            ylist.Sort();
            var xsec = xlist.Count / 2;
            var ysec = (ylist[ylist.Count - 1] - ylist[0]) / 3;

            var xstart = new List<int>();
            xstart.Add(0);
            xstart.Add(1);

            var ystart = new List<int>();
            ystart.Add(ylist[0]);
            ystart.Add(ylist[0] + ysec);
            ystart.Add(ylist[0] + 2 * ysec);

            var rad = new Random(DateTime.Now.Millisecond);
            var idx = 0;
            while (true)
            {
                foreach (var xs in xstart)
                {
                    foreach (var ys in ystart)
                    {
                        var xrad = rad.Next(xsec);
                        var xval = 0;
                        var xidx = xs * xsec + xrad;
                        if (xidx < xlist.Count)
                        { xval = xlist[xidx]; }
                        else
                        { xval = xlist[xidx - 1]; }

                        var yrad = rad.Next(ysec);
                        var yval = ys + yrad;
                        var k = xval.ToString() + ":::" + yval.ToString();
                        if (xydict.ContainsKey(k) && !sampledict.ContainsKey(k))
                        {
                            sampledict.Add(k, xydict[k]);
                            if (sampledict.Count == samplesize)
                            { return sampledict; }
                        }
                    }//end foreach
                }//end foreach

                if (idx > maxtimes)
                { break; }
                idx = idx + 1;
            }

            return new Dictionary<string, string>();

        }

        public List<List<int>> SixInchArray2SingletCoord(string prodfam, List<string> xlist, List<string> ylist)
        {
            var ret = new List<List<int>>();

            var xcond = string.Join(",", xlist);
            var ycond = string.Join(",", ylist);
            var dict = new Dictionary<string, string>();
            dict.Add("@Comma_Delimited_Xs", xcond.ToUpper().Replace("X", "").Replace("Y", ""));
            dict.Add("@Comma_Delimited_Ys", ycond.ToUpper().Replace("X", "").Replace("Y", ""));
            dict.Add("@Array_Product_Family", prodfam);
            dict.Add("@True_For_Singlet_Output_False_For_Array", "1");
            var dbret = DBUtility.ExeShermanStoreProcedureWithRes("Convert_Coords", dict);

            var ridx = 0;
            var xidx = 5;
            var yidx = 6;

            var tempxlist = new List<int>();
            var tempylist = new List<int>();

            foreach (var line in dbret)
            {
                if (ridx == 0)
                {
                    var colidx = 0;
                    foreach (var item in line)
                    {
                        var cname = UT.O2S(item).ToUpper();
                        if (string.Compare(cname, "SingletX", true) == 0
                            || string.Compare(cname, "Singlet X", true) == 0)
                        { xidx = colidx; }
                        if (string.Compare(cname, "SingletY", true) == 0
                            || string.Compare(cname, "Singlet Y", true) == 0)
                        { yidx = colidx; }
                        colidx++;
                    }//end foreach
                    ridx++;
                    continue;
                }

                tempxlist.Add(UT.O2I(line[xidx]));
                tempylist.Add(UT.O2I(line[yidx]));
            }

            ret.Add(tempxlist);
            ret.Add(tempylist);
            return ret;
        }

        public List<List<int>> SixInchSinglet2ArrayCoord(string prodfam, List<string> xlist, List<string> ylist)
        {
            var ret = new List<List<int>>();

            var xcond = string.Join(",", xlist);
            var ycond = string.Join(",", ylist);
            var dict = new Dictionary<string, string>();
            dict.Add("@Comma_Delimited_Xs", xcond.ToUpper().Replace("X", "").Replace("Y", ""));
            dict.Add("@Comma_Delimited_Ys", ycond.ToUpper().Replace("X", "").Replace("Y", ""));
            dict.Add("@Array_Product_Family", prodfam);
            dict.Add("@True_For_Singlet_Output_False_For_Array", "0");
            var dbret = DBUtility.ExeShermanStoreProcedureWithRes("Convert_Coords", dict);

            var ridx = 0;
            var xidx = 3;
            var yidx = 4;

            var tempxlist = new List<int>();
            var tempylist = new List<int>();

            foreach (var line in dbret)
            {
                if (ridx == 0)
                {
                    var colidx = 0;
                    foreach (var item in line)
                    {
                        var cname = UT.O2S(item).ToUpper();
                        if (string.Compare(cname, "ArrayX", true) == 0
                            || string.Compare(cname, "Array X", true) == 0)
                        { xidx = colidx; }
                        if (string.Compare(cname, "ArrayY", true) == 0
                            || string.Compare(cname, "Array Y", true) == 0)
                        { yidx = colidx; }
                        colidx++;
                    }//end foreach
                    ridx++;
                    continue;
                }

                tempxlist.Add(UT.O2I(line[xidx]));
                tempylist.Add(UT.O2I(line[yidx]));
            }

            ret.Add(tempxlist);
            ret.Add(tempylist);
            return ret;
        }

        private static List<SixIMapData> GetBaseArrayCoordinate(string prodfam)
        {
            var ret = new List<SixIMapData>();
            var sql = " SELECT * FROM [dbo].[Get_100PCT_Map_All_Dies] ( '" + prodfam + "')";
            var dbret = DBUtility.ExeShermanSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var tempvm = new SixIMapData();
                tempvm.X = UT.O2I(line[2]);
                tempvm.Y = UT.O2I(line[3]);

                var bin = UT.O2I(line[7]);
                if (bin >= 30 && bin < 40)
                { tempvm.Bin = bin.ToString(); }
                else
                { tempvm.Bin = "99"; }

                tempvm.BinName = UT.O2S(line[8]);
                ret.Add(tempvm);
            }
            return ret;
        }

        private static List<SixIMapData> GetBinArrayCoordinate(int arraysize, string wafer)
        {
            var ret = new List<SixIMapData>();

            var typename = "Wafer Final";//"Final Probe AVI Merge";
            if (arraysize != 1)
            { typename = "Wafer Final"; }//"Final Bins AVI Merged"; }

            //var typename = "Final Probe AVI Merge";
            //if (arraysize != 1)
            //{ typename = "Final Bins AVI Merged"; }

            var sql = "SELECT * FROM [dbo].[Get_Probe_Bins] ( '" + wafer + "' ,'" + typename + "')";
            var dbret = DBUtility.ExeShermanSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var binname = UT.O2S(line[22]).ToUpper();
                if (binname.Contains("GOOD"))
                {
                    var tempvm = new SixIMapData();
                    tempvm.X = UT.O2I(line[19]);
                    tempvm.Y = UT.O2I(line[20]);
                    tempvm.Bin = UT.O2S(line[21]);
                    tempvm.BinName = binname;
                    tempvm.PN = UT.O2S(line[5]);
                    ret.Add(tempvm);
                }
            }
            return ret;
        }

        public static Dictionary<string, int> GetBinDistribution(string wafer)
        {
            var ret = new Dictionary<string, int>();

            var typename = "Wafer Final";
            var sql = "SELECT X,Y,BIN_NUMBER,BIN_NAME FROM [dbo].[Get_Probe_Bins] ( '" + wafer + "' ,'" + typename + "')";
            var dbret = DBUtility.ExeShermanSqlWithRes(sql);
            if (dbret.Count > 0)
            {
                foreach (var line in dbret)
                {
                    var binname = UT.O2S(line[3]).ToUpper();
                    if (binname.Contains("GOOD"))
                    {
                        var bin = UT.O2S(line[2]);
                        if (!ret.ContainsKey(bin))
                        { ret.Add(bin, 1); }
                        else
                        { ret[bin] += 1; }
                    }
                }
            }
            else
            {
                var ProdFam = WXEvalPN.GetProductFamilyFromSherman(wafer);
                var ArraySize = UT.O2I(GetWaferArrayInfoByPF(ProdFam));
                typename = "Final Probe AVI Merge";
                if (ArraySize != 1)
                { typename = "Final Bins AVI Merged"; }
                sql = "SELECT X,Y,BIN_NUMBER,BIN_NAME FROM [dbo].[Get_Probe_Bins] ( '" + wafer + "' ,'" + typename + "')";
                dbret = DBUtility.ExeShermanSqlWithRes(sql);
                foreach (var line in dbret)
                {
                    var binname = UT.O2S(line[3]).ToUpper();
                    if (binname.Contains("GOOD"))
                    {
                        var bin = UT.O2S(line[2]);
                        if (!ret.ContainsKey(bin))
                        { ret.Add(bin, 1); }
                        else
                        { ret[bin] += 1; }
                    }
                }
            }

            return ret;
        }

        private static string GetWaferArrayInfoByPF(string productfm)
        {
            var dict = new Dictionary<string, string>();
            dict.Add("@productfm", productfm);
            var sql = @"SELECT ARRAY_COUNT_X FROM [ShermanData].[dbo].[PRODUCT_VIEW] WITH (NOLOCK) WHERE PRODUCT_FAMILY = @productfm";
            var dbret = DBUtility.ExeShermanSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                if (line[0] != System.DBNull.Value)
                {
                    return UT.O2S(line[0]);
                }
            }
            return "1";
        }

        public SixIMapFile()
        {
            BaseArrayCoordinate = new List<SixIMapData>();
            BinArrayCoordinate = new List<SixIMapData>();
            ArraySize = 1;
            ProdFam = "";
            DieOne = new SixIMapData();
            DieOneMin = new SixIMapData();
            Wafer = "";
            PN = "";
        }

        public List<SixIMapData> BaseArrayCoordinate { set; get; }
        public List<SixIMapData> BinArrayCoordinate { set; get; }
        public int ArraySize { set; get; }
        public string ProdFam { set; get; }
        public SixIMapData DieOne { set; get; }
        public SixIMapData DieOneMin { set; get; }
        public string Wafer { set; get; }
        public string PN { set; get; }
    }
}
