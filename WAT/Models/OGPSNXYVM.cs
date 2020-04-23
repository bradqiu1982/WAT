using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAT.Models
{
    public class OGPSNXYVM
    {
        public static Dictionary<string, OGPSNXYVM> GetLocalOGPXYSNDict(string wafernum)
        {
            var sql = @"SELECT f.SN,s.ImgVal,s.ChildCat,s.ImgOrder,f.MainImgKey,f.CaptureImg,f.RAWImgURL,f.Appv_4,WaferNum,s.Appv_1,f.Appv_3 FROM [WAT].[dbo].[OGPFatherImg] f with(nolock)
                        inner join [WAT].[dbo].[SonImg] s with (nolock) on f.MainImgKey = s.MainImgKey
                        where WaferNum like '<wafernum>%' order by SN,ImgOrder asc";
            sql = sql.Replace("<wafernum>", wafernum);

            var dict = new Dictionary<string, OGPSNXYVM>();
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var sn = UT.O2S(line[0]);
                var k = UT.O2S(line[4]) + ":" + sn;

                var imgval = "";
                var ival = UT.O2I(line[1]);
                if (ival != -1)
                { imgval = Convert.ToString((char)ival); }

                var rate = UT.O2S(line[9]);
                var frate = 0.0;
                if (!string.IsNullOrEmpty(rate))
                { frate = UT.O2D(rate); }

                var cat = UT.O2S(line[2]).ToUpper();
                if (dict.ContainsKey(k))
                {
                    if (cat.Contains("X"))
                    {
                        dict[k].X += imgval;
                        dict[k].XConfidence = dict[k].XConfidence * frate * 0.01;
                    }
                    else
                    {
                        dict[k].Y += imgval;
                        dict[k].YConfidence = dict[k].YConfidence * frate * 0.01;
                    }
                }
                else
                {
                    var tempvm = new OGPSNXYVM();
                    tempvm.MainImgKey = UT.O2S(line[4]);
                    tempvm.CaptureImg = UT.O2S(line[5]);
                    tempvm.SN = sn;
                    tempvm.RawImg = UT.O2S(line[6]);
                    tempvm.Modified = UT.O2S(line[7]);
                    tempvm.WaferNum = UT.O2S(line[8]);
                    tempvm.OCRFile = UT.O2S(line[10]);

                    if (cat.Contains("X"))
                    {
                        tempvm.X += imgval;
                        tempvm.XConfidence = tempvm.XConfidence * frate * 0.01;
                    }
                    else
                    {
                        tempvm.Y += imgval;
                        tempvm.YConfidence = tempvm.YConfidence * frate * 0.01;
                    }
                    dict.Add(k, tempvm);
                }
            }

            return dict;
        }



        public OGPSNXYVM()
        {
            MainImgKey = "";
            CaptureImg = "";
            RawImg = "";
            SN = "";
            X = "";
            Y = "";
            MX = "";
            MY = "";
            Modified = "";
            WaferNum = "";
            Product = "";
            XConfidence = 1.0;
            YConfidence = 1.0;
            OCRFile = "";
            Bin = "";
        }

        public string MainImgKey { set; get; }
        public string CaptureImg { set; get; }
        public string RawImg { set; get; }
        public string SN { set; get; }
        public string X { set; get; }
        public string Y { set; get; }
        public string MX { set; get; }
        public string MY { set; get; }
        public string Checked
        {
            get
            {
                if (X.ToUpper().Contains(MX) && Y.ToUpper().Contains(MY)
                    && !string.IsNullOrEmpty(MX) && !string.IsNullOrEmpty(MY)
                    && X.ToUpper().Substring(0, 1).Contains("X")
                    && Y.ToUpper().Substring(0, 1).Contains("Y"))
                { return "CHECKED"; }
                else
                { return ""; }
            }
        }
        public string Modified { set; get; }
        public string WaferNum { set; get; }
        public string Product { set; get; }
        public double XConfidence { set; get; }
        public double YConfidence { set; get; }
        public int XYConfidence
        {
            get
            {
                if (XConfidence < 0.83)
                { return 3; }
                if (YConfidence < 0.83)
                { return 3; }
                return 1;
            }
        }

        public int CFDLevel
        {
            get
            {
                if (XConfidence * YConfidence >= 0.9)
                { return 1; }
                else if (XConfidence <= 0.8 || YConfidence <= 0.8)
                { return 3; }
                else if (XConfidence * YConfidence <= 0.7)
                { return 3; }
                else
                { return 2; }
            }
        }

        public string OCRFile{ set; get; }
        public string Bin { set; get; }
    }
}