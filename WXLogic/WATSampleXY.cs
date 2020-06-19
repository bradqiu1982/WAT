using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WXLogic
{
    public class WATSampleXY
    {
        public static string GetArrayFromDieSort(string wafer)
        {
            var dict = new Dictionary<string, string>();
            dict.Add("@wafer", wafer);
            var sql = "SELECT top 1 [PArray] FROM [WAT].[dbo].[WaferSampleData] where WAFER = @wafer";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                return Convert.ToString(line[0]).ToUpper().Replace("1X", "");
            }
            return string.Empty;
        }


        public static string GetArrayFromAllenSherman(string wafer)
        {
            var dict = new Dictionary<string, string>();
            dict.Add("@WaferNum", wafer);
            var sql = "select top 1 AppVal1 from [WAT].[dbo].[WXEvalPN] where [WaferNum] = @WaferNum";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            var array = "";
            if (dbret.Count > 0)
            { array = UT.O2S(dbret[0][0]); }
            if (!string.IsNullOrEmpty(array))
            { return array; }


            var sixinch = false;
            var productfm = "";
            if (wafer.Length == 13)
            {
                productfm = WXEvalPN.GetProductFamilyFromSherman(wafer);
                if (!string.IsNullOrEmpty(productfm))
                { sixinch = true; }
            }
            else
            {
                productfm = WXEvalPN.GetProductFamilyFromAllen(wafer);
                if (string.IsNullOrEmpty(productfm))
                {
                    productfm = WXEvalPN.GetProductFamilyFromSherman(wafer);
                    if (!string.IsNullOrEmpty(productfm))
                    { sixinch = true; }
                }
            }

            if (string.IsNullOrEmpty(productfm))
            { return string.Empty; }

            return GetWaferArrayInfoByPF(productfm, sixinch);
        }

        private static string GetWaferArrayInfoByPF(string productfm, bool sixinch)
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
                        return UT.O2S(line[0]);
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
                        return UT.O2S(line[0]);
                    }
                }
                return "1";
            }
        }

        //public static int Get_First_Singlet_From_Array_Coord(int DIE_ONE_X, int DIE_ONE_FIELD_MIN_X,int arrayx, int Array_Count)
        //{
        //    var new_x = ((arrayx - DIE_ONE_X
        //        + Math.Floor((double)(DIE_ONE_X - DIE_ONE_FIELD_MIN_X) / Array_Count)) * Array_Count)
        //        + DIE_ONE_FIELD_MIN_X;
        //    if (Array_Count != 1)
        //    { return (int)Math.Round(new_x, 0)-1; }
        //    else
        //    { return (int)Math.Round(new_x, 0); }
        //}

        private static List<char> MatchSameLenStr(string ogp, string sa,int len)
        {
            var ret = new List<char>();
            var ogparray = ogp.ToCharArray();
            var saarray = sa.ToCharArray();
            for (var i = 0; i < len; i++)
            {
                if (ogparray[i] == saarray[i])
                { ret.Add(ogparray[i]); }
            }
            return ret;
        }

        private static List<char> MatchDifLenStr(string ogp, string sa)
        {
            var ogplen = ogp.Length;
            var salen = sa.Length;
            var ls = "";var ll = 0;
            var ss = "";var sl = 0;
            if (ogplen > salen)
            {
                ls = ogp;ll = ogplen;
                ss = sa;sl = salen;
            }
            else
            {
                ls = sa;ll = salen;
                ss = ogp;sl = ogplen;
            }

            var matchlists = new List<List<char>>();
            var times = ll - sl + 1;
            for (var idx = 0;idx < times;idx++)
            {
                var sub = ls.Substring(idx, sl);
                matchlists.Add(MatchSameLenStr(sub, ss,sl));
            }

            matchlists.Sort(delegate(List<char> obj1, List<char> obj2) {
                return obj2.Count.CompareTo(obj1.Count);
            });

            return matchlists[0];
        }

        private static bool Match5Str(string ogpx,string ogpy,int ogplen,string sax,string say,int salen)
        {
            try
            {
                var matchtargetlen = 0;
                if (ogplen == salen)
                { matchtargetlen = ogplen - 1; }
                else if (ogplen > salen)
                { matchtargetlen = salen; }
                else
                { matchtargetlen = ogplen; }

                var matchcharlist = new List<char>();

                if (ogpx.Length == sax.Length)
                { matchcharlist.AddRange(MatchSameLenStr(ogpx,sax,sax.Length)); }
                else
                { matchcharlist.AddRange(MatchDifLenStr(ogpx, sax)); }

                if (ogpy.Length == say.Length)
                { matchcharlist.AddRange(MatchSameLenStr(ogpy,say, say.Length)); }
                else
                { matchcharlist.AddRange(MatchDifLenStr(ogpy, say)); }

                if (matchcharlist.Count >= matchtargetlen)
                { return true; }
            }
            catch (Exception e) {
                return false;
            }
            return false;
        }

        //private static List<WATSampleXY> CorrectXYBySamplePick(string wafer,List<int> dieonex,int arraycount,List<WATSampleXY> ogpdata)
        //{
        //    var newxy = new List<WATSampleXY>();
        //    var xydict = new Dictionary<string, XYVAL>();
        //    var rxydict = new Dictionary<string, XYVAL>();

        //    var xdict = new Dictionary<string, List<XYVAL>>();
        //    var ydict = new Dictionary<string, List<XYVAL>>();

        //    var sql = "select distinct X,Y from [WAT].[dbo].[WaferSampleData] where wafer like '<wafer>%'";
        //    sql = sql.Replace("<wafer>", wafer);
        //    var dbret = DBUtility.ExeLocalSqlWithRes(sql);
        //    foreach (var line in dbret)
        //    {
        //        var ax = UT.O2S(line[0]);
        //        var y = UT.O2S(line[1]);

        //        var x = Get_First_Singlet_From_Array_Coord(dieonex[0], dieonex[1], UT.O2I(ax), arraycount).ToString();

        //        var key = x + ":::" + y;
        //        xydict.Add(key, new XYVAL(x, y, "0", 0));

        //        var rx = (UT.O2I(x) + (arraycount - 1)).ToString();
        //        var rkey = rx + ":::" + y;
        //        rxydict.Add(rkey, new XYVAL(x, y, "0", 0));
        //    }

        //    var markeddict = new Dictionary<string, bool>();

        //    var xyleftdata = new List<WATSampleXY>();
        //    foreach (var ogp in ogpdata)
        //    {
        //        var key = ogp.X + ":::" + ogp.Y;
        //        if (xydict.ContainsKey(key))
        //        {
        //            if (!markeddict.ContainsKey(key))
        //            { markeddict.Add(key, true); }
        //            newxy.Add(new WATSampleXY(ogp.CouponID, ogp.ChannelInfo, xydict[key].X, xydict[key].Y));
        //        }
        //        else
        //        {
        //            xyleftdata.Add(new WATSampleXY(ogp.CouponID, ogp.ChannelInfo, ogp.X, ogp.Y));
        //        }
        //    }

        //    var xysearchleftdata = new List<WATSampleXY>();
        //    foreach (var ogp in xyleftdata)
        //    {
        //        var matchkeys = new List<string>();
        //        var ogplen = ogp.X.Length + ogp.Y.Length;

        //        foreach (var xykv in xydict)
        //        {
        //            var sax = xykv.Value.X;
        //            var say = xykv.Value.Y;
        //            var salen = sax.Length + say.Length;

        //            if (!markeddict.ContainsKey(xykv.Key) && Match5Str(ogp.X,ogp.Y,ogplen,sax,say,salen))
        //            {
        //                matchkeys.Add(xykv.Key);
        //            }
        //        }//end foreach

        //        if (matchkeys.Count == 1)
        //        {
        //            if (!markeddict.ContainsKey(matchkeys[0]))
        //            { markeddict.Add(matchkeys[0], true); }

        //            newxy.Add(new WATSampleXY(ogp.CouponID, ogp.ChannelInfo, xydict[matchkeys[0]].X, xydict[matchkeys[0]].Y));
        //        }
        //        else
        //        { xysearchleftdata.Add(new WATSampleXY(ogp.CouponID, ogp.ChannelInfo, ogp.X, ogp.Y)); }
        //    }

        //    if (arraycount == 1)
        //    {
        //        //newxy.AddRange(xysearchleftdata);
        //    }
        //    else
        //    {
        //        var rxysearchleftdata = new List<WATSampleXY>();
        //        foreach (var ogp in xysearchleftdata)
        //        {
        //            var matchkeys = new List<string>();
        //            var ogplen = ogp.X.Length + ogp.Y.Length;

        //            foreach (var xykv in rxydict)
        //            {
        //                var rxystr = xykv.Key.Split(new string[] { ":::" }, StringSplitOptions.RemoveEmptyEntries);
        //                var sax = rxystr[0];
        //                var say = rxystr[1];
        //                var salen = sax.Length + say.Length;

        //                if (!markeddict.ContainsKey(xykv.Key) && Match5Str(ogp.X, ogp.Y, ogplen, sax, say, salen))
        //                {
        //                    matchkeys.Add(xykv.Key);
        //                }
        //            }//end foreach

        //            if (matchkeys.Count == 1)
        //            {
        //                newxy.Add(new WATSampleXY(ogp.CouponID, ogp.ChannelInfo, rxydict[matchkeys[0]].X, rxydict[matchkeys[0]].Y));
        //            }
        //            else
        //            { rxysearchleftdata.Add(new WATSampleXY(ogp.CouponID, ogp.ChannelInfo, ogp.X, ogp.Y)); }
        //        }
        //        //newxy.AddRange(rxysearchleftdata);
        //    }

        //    return newxy;
        //}

        public static void CorrectChannelIndex(List<WATSampleXY> ogporder, int arraysize)
        {
            foreach (var item in ogporder)
            {
                var ch = UT.O2I(item.ChannelInfo);
                if (arraysize == 12)
                {
                    if (ch > 12)
                    { item.ChannelInfo = (ch - 12).ToString(); }
                    else
                    { item.ChannelInfo = (ch + 12).ToString(); }
                }
                else
                {
                    if (ch > 16)
                    { item.ChannelInfo = (ch - 16).ToString(); }
                    else
                    { item.ChannelInfo = (ch + 16).ToString(); }
                }
            }
        }

        //private static List<WATSampleXY> LoadOGPFromMEDB(string coupongroup,int arraysize,string wafer)
        //{
        //    var ret = new List<WATSampleXY>();

        //    var watcfg = WXCfg.GetSysCfg();
        //    if (watcfg.ContainsKey("OGPCONNSTR") && watcfg.ContainsKey("OGPSQLSTR"))
        //    {
        //        try
        //        {
        //            var ogpconn = DBUtility.GetConnector(watcfg["OGPCONNSTR"]);
        //            var sql = watcfg["OGPSQLSTR"];
        //            sql = sql.Replace("<coupongroup>", coupongroup);

        //            var dbret = DBUtility.ExeSqlWithRes(ogpconn, sql);
        //            DBUtility.CloseConnector(ogpconn);

        //            foreach (var line in dbret)
        //            {
        //                var tempvm = new WATSampleXY();
        //                tempvm.CouponID = UT.O2S(line[0]);
        //                tempvm.ChannelInfo = UT.O2S(line[1]);
        //                if (arraysize != 1)
        //                {
        //                    var tempx = UT.O2I(UT.O2S(line[2]).Replace("X", ""));
        //                    if (tempx % arraysize == 0)
        //                    { tempvm.X = (tempx - (arraysize - 1)).ToString(); }
        //                    else
        //                    { tempvm.X = UT.O2I(UT.O2S(line[2]).Replace("X", "")).ToString(); }
        //                }
        //                else
        //                {
        //                    tempvm.X = UT.O2I(UT.O2S(line[2]).Replace("X", "")).ToString();
        //                }
        //                tempvm.Y = UT.O2I(UT.O2S(line[3]).Replace("Y", "")).ToString();
        //                ret.Add(tempvm);
        //            }
        //        }
        //        catch (Exception ex) { }
        //    }

        //    var dieonex = AdminFileOperations.GetDieOneByWafer(wafer);
        //    if (dieonex.Count > 0)
        //    {
        //        ret = CorrectXYBySamplePick(wafer, dieonex, arraysize, ret);
        //    }

        //    return ret;
        //}

        public static List<WATSampleXY> LoadOGPFromLocalDB(string coupongroup, int arraysize, string wafer)
        {
            var ret = new List<WATSampleXY>();
            var sql = @"SELECT f.SN,s.ImgVal,s.ChildCat,s.ImgOrder FROM [WAT].[dbo].[OGPFatherImg] f with(nolock)
                        inner join [WAT].[dbo].[SonImg] s with (nolock) on f.MainImgKey = s.MainImgKey
                        where f.SN like '<coupongroup>%' order by SN,ImgOrder asc";
            sql = sql.Replace("<coupongroup>", coupongroup);

            var dict = new Dictionary<string, WATSampleXY>();
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var sn = UT.O2S(line[0]);
                var imgval = UT.O2S((char)UT.O2I(line[1]));
                var cat = UT.O2S(line[2]).ToUpper();
                if (dict.ContainsKey(sn))
                {
                    if (cat.Contains("X"))
                    { dict[sn].X += imgval; }
                    else
                    { dict[sn].Y += imgval; }
                }
                else
                {
                    var tempvm = new WATSampleXY();
                    if (cat.Contains("X"))
                    { tempvm.X += imgval; }
                    else
                    { tempvm.Y += imgval; }
                    dict.Add(sn, tempvm);
                }
            }

            foreach (var kv in dict)
            {
                if (!kv.Key.Contains(":::"))
                { continue; }
                if (kv.Value.X.Contains("XXXX") || kv.Value.Y.Contains("YYYY"))
                { continue; }

                var snidx = kv.Key.Split(new string[] { ":::" }, StringSplitOptions.RemoveEmptyEntries);
                var tempvm = new WATSampleXY();
                tempvm.CouponID = snidx[0];
                tempvm.ChannelInfo = snidx[1];

                var X = UT.O2I(kv.Value.X.Replace("X", "").Replace("x", "").Replace("Y", "").Replace("y", ""));
                if (arraysize != 1 && X % arraysize == 0)
                { X = X - (arraysize - 1); }
                tempvm.X = X.ToString();
                var Y = UT.O2I(kv.Value.Y.Replace("Y", "").Replace("y", "").Replace("X", "").Replace("x", "")).ToString();
                tempvm.Y = Y;

                ret.Add(tempvm);
            }

            return ret;
        }

        public static List<WATSampleXY> LoadOGPFromLocalDBByWafer(string coupongroup, int arraysize, string wafer)
        {
            var ret = new List<WATSampleXY>();
            var sql = @"SELECT f.SN,s.ImgVal,s.ChildCat,s.ImgOrder FROM [WAT].[dbo].[OGPFatherImg] f with(nolock)
                        inner join [WAT].[dbo].[SonImg] s with (nolock) on f.MainImgKey = s.MainImgKey
                        where f.WaferNum like '<coupongroup>%' order by SN,ImgOrder asc";
            sql = sql.Replace("<coupongroup>", coupongroup);

            var dict = new Dictionary<string, WATSampleXY>();
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var sn = UT.O2S(line[0]);
                var imgval = UT.O2S((char)UT.O2I(line[1]));
                var cat = UT.O2S(line[2]).ToUpper();
                if (dict.ContainsKey(sn))
                {
                    if (cat.Contains("X"))
                    { dict[sn].X += imgval; }
                    else
                    { dict[sn].Y += imgval; }
                }
                else
                {
                    var tempvm = new WATSampleXY();
                    if (cat.Contains("X"))
                    { tempvm.X += imgval; }
                    else
                    { tempvm.Y += imgval; }
                    dict.Add(sn, tempvm);
                }
            }

            foreach (var kv in dict)
            {
                if (kv.Value.X.Contains("XXXX") || kv.Value.Y.Contains("YYYY"))
                { continue; }

                //var snidx = kv.Key.Split(new string[] { ":::" }, StringSplitOptions.RemoveEmptyEntries);
                var tempvm = new WATSampleXY();
                tempvm.CouponID = kv.Key;
                tempvm.ChannelInfo = "0";

                var X = UT.O2I(kv.Value.X.Replace("X", "").Replace("x", "").Replace("Y", "").Replace("y", ""));
                if (arraysize != 1 && X % arraysize == 0)
                { X = X - (arraysize - 1); }
                tempvm.X = X.ToString();
                var Y = UT.O2I(kv.Value.Y.Replace("Y", "").Replace("y", "").Replace("X", "").Replace("x", "")).ToString();
                tempvm.Y = Y;

                ret.Add(tempvm);
            }

            return ret;
        }

        public static List<WATSampleXY> GetSampleXYByCouponGroup(string coupongroup)
        {
            var ret = new List<WATSampleXY>();
            var wafer = coupongroup.ToUpper().Split(new string[] { "E","R","T" }, StringSplitOptions.RemoveEmptyEntries)[0];
            var array = GetArrayFromDieSort(wafer);
            if (string.IsNullOrEmpty(array))
            { array = GetArrayFromAllenSherman(wafer); }
            if (string.IsNullOrEmpty(array))
            { return ret; }

            var arraysize = UT.O2I(array);

            ret = LoadOGPFromLocalDB(coupongroup, arraysize, wafer);
            //if (ret.Count == 0)
            //{ ret = LoadOGPFromMEDB(coupongroup, arraysize, wafer); }
            

            if (string.Compare(array, "1") == 0)
            {
                CorrectChannelIndex(ret, 1);
                return ret;
            }
            else
            {
                var newret = new List<WATSampleXY>();
                

                foreach (var item in ret)
                {
                    for (var die = 0; die < arraysize; die++)
                    {
                        var tempvm = new WATSampleXY();
                        tempvm.CouponID = item.CouponID;
                        tempvm.ChannelInfo = ((UT.O2I(item.ChannelInfo) - 1) * arraysize + 1 + die).ToString();
                        tempvm.X = (UT.O2I(item.X) + die).ToString();
                        tempvm.Y = item.Y;
                        newret.Add(tempvm);
                    }
                }

                CorrectChannelIndex(newret, arraysize);
                return newret;
            }

        }

        WATSampleXY()
        {
            CouponID = "";
            ChannelInfo = "";
            X = "";
            Y = "";
        }

        WATSampleXY(string cp,string ch,string x,string y)
        {
            CouponID = cp;
            ChannelInfo = ch;
            X = x;
            Y = y;
        }

        public string CouponID { set; get; }
        public string ChannelInfo { set; get; }
        public string X { set; get; }
        public string Y { set; get; }
    }
}