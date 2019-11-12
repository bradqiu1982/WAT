using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAT.Models
{
    public class BinSubstitute
    {

        private static List<BinSubstitute> LoadProductBin()
        {
            var ret = new List<BinSubstitute>();
            var bins = new string[] { "50", "51", "52", "53", "54", "55", "56", "57", "58", "59" }.ToList();
            var bdict = new Dictionary<string, bool>();
            foreach (var b in bins)
            { bdict.Add(b, true); }
            var sql = @"SELECT  PRODUCT, BINNUM FROM  [EngrData].[dbo].[NeoMAP_REBIN__Production]
	                    WHERE  PEI = 'P' AND PRODUCT in (select  DISTINCT LEFT(PRODFAM,4) from [Insite].[insite].[100PCT_BIN])
	                    GROUP BY  PRODUCT, BINNUM ORDER BY 1";
            var dbret = DBUtility.ExeAllenSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var tempvm = new BinSubstitute();
                tempvm.Product = UT.O2S(line[0]);
                tempvm.Bin = UT.O2S(line[1]);
                if (bdict.ContainsKey(tempvm.Bin))
                {
                    ret.Add(tempvm);
                }
            }

            return ret;
        }

        private static Dictionary<string,BinSubstitute> LoadBinPNDict()
        {
            var ret = new Dictionary<string, BinSubstitute>();

            var sql = @"SELECT distinct [Device],left([BinProdfam],4),[BinNumber],BinJobPartNumber
                          FROM [Insite].[insite].[BinJobPartnumbers] where [BinProdfam] is not null 
                          and [BinNumber] is not null and BinJobPartNumber is not null";
            var dbret = DBUtility.ExeAllenSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var tempvm = new BinSubstitute();
                tempvm.Product = UT.O2S(line[0]);
                tempvm.BinFam = UT.O2S(line[1]);
                tempvm.Bin = UT.O2S(line[2]);
                tempvm.BinPN = UT.O2S(line[3]);

                var dkey = tempvm.Product + ":::" + tempvm.Bin;
                var bkey = tempvm.BinFam + ":::" + tempvm.Bin;
                if (!ret.ContainsKey(dkey))
                { ret.Add(dkey, tempvm); }
                if (!ret.ContainsKey(bkey))
                { ret.Add(bkey,tempvm); }
            }

            return ret;
        }

        private static Dictionary<string, Dictionary<string, BinSubstitute>> LoadSubBinSpec(List<BinSubstitute> pblist)
        {
            var ret = new Dictionary<string, Dictionary<string, BinSubstitute>>();
            foreach (var pb in pblist)
            {
                var sql = @"SELECT  Product, BinNum, FilterType, TestName, LoLimit, HiLimit FROM 
                        	[EngrData].[dbo].[NeoMAP_REBIN__Production] WHERE Product = @Product AND BINNUM = @Bin AND PEI = 'P'";
                var dict = new Dictionary<string, string>();
                dict.Add("@Product",pb.Product);
                dict.Add("@Bin", pb.Bin);
                var dbret = DBUtility.ExeAllenSqlWithRes(sql, dict);
                foreach (var line in dbret)
                {
                    var pd = UT.O2S(line[0]).ToUpper();
                    var bi = UT.O2S(line[1]).ToUpper();
                    var ft = UT.O2S(line[2]).ToUpper();
                    var tsn = UT.O2S(line[3]).ToUpper();
                    var ll = UT.O2D(line[4]);
                    var hl = UT.O2D(line[5]);
                    var tempvm = new BinSubstitute(pd, bi, ft, tsn, ll, hl);

                    var pbkey = pd + ":::" + bi;

                    if (ret.ContainsKey(pbkey))
                    {
                        if (!ret[pbkey].ContainsKey(tsn))
                        {
                            ret[pbkey].Add(tsn, tempvm);
                        }
                    }
                    else
                    {
                        var tdict = new Dictionary<string, BinSubstitute>();
                        tdict.Add(tsn, tempvm);
                        ret.Add(pbkey, tdict);
                    }
                }//end foreach
            }//end foreach

            return ret;
        }

        private static bool MatchSpec(Dictionary<string, BinSubstitute> srcdict, Dictionary<string, BinSubstitute> tdict)
        {
            foreach (var skv in srcdict)
            {
                if (tdict.ContainsKey(skv.Key))
                {
                    if (string.Compare(skv.Value.FilterType,tdict[skv.Key].FilterType,true) == 0
                        && tdict[skv.Key].LowLimit >= skv.Value.LowLimit
                        && tdict[skv.Key].HighLimit <= skv.Value.HighLimit)
                    { }
                    else
                    { return false; }
                }
                else
                {
                    return false;
                }
            }
            return true;
        }



        public static void RefreshBinSubstituteFromAllen()
        {
            var pblist = LoadProductBin();
            var pndict = LoadBinPNDict();
            var allspec = LoadSubBinSpec(pblist);

            var matchdata = new List<List<BinSubstitute>>();
            var bins = new string[] { "50","51","52", "53", "54", "55", "56", "57", "58", "59" }.ToList();
            foreach (var cbkv in allspec)
            {
                var cpdbkey = cbkv.Key.Split(new string[] { ":::" }, StringSplitOptions.RemoveEmptyEntries);
                var cpd = cpdbkey[0];
                var cbin = cpdbkey[1];
                foreach (var tbin in bins)
                {
                    if (cbin.Contains(tbin))
                    { continue; }

                    var tdpdkey = cpd + ":::" + tbin;
                    if (allspec.ContainsKey(tdpdkey))
                    {
                        var srcdict = cbkv.Value;
                        var tdict = allspec[tdpdkey];
                        if (MatchSpec(srcdict, tdict))
                        {
                            var tmplist = new List<BinSubstitute>();
                            tmplist.Add(new BinSubstitute(cpd, tbin, "", "", 0, 0));
                            tmplist.Add(new BinSubstitute(cpd, cbin, "", "", 0, 0));
                            matchdata.Add(tmplist);
                        }//end if
                    }//end if
                }//end foreach
            }//end foreach

            var ret = new List<List<BinSubstitute>>();
            foreach (var md in matchdata)
            {
                var tkey = md[0].Product + ":::" + md[0].Bin;
                var skey = md[1].Product + ":::" + md[1].Bin;
                if (pndict.ContainsKey(tkey) && pndict.ContainsKey(skey))
                {
                    md[0].BinFam = pndict[tkey].BinFam;
                    md[0].BinPN = pndict[tkey].BinPN;
                    md[1].BinFam = pndict[skey].BinFam;
                    md[1].BinPN = pndict[skey].BinPN;
                    var tmplist = new List<BinSubstitute>();
                    tmplist.Add(md[0]);
                    tmplist.Add(md[1]);
                    ret.Add(tmplist);
                }
            }

            foreach (var md in ret)
            { StoreBinSubstitute(md); }

        }



        private static void StoreBinSubstitute(List<BinSubstitute> data)
        {
            var FromDevice = data[0].Product;
            var FromBinFam = data[0].BinFam;
            var FromBin = data[0].Bin;
            var FromBinPN = data[0].BinPN;

            var ToDevice = data[1].Product;
            var ToBinFam = data[1].BinFam;
            var ToBin = data[1].Bin;
            var ToBinPN = data[1].BinPN;

            if (HasData(FromBinFam, FromBin))
            {
                UpdateData( FromDevice,  FromBinFam,  FromBin,  FromBinPN
              ,  ToDevice,  ToBinFam,  ToBin,  ToBinPN); }
            else
            {
                InsertData( FromDevice, FromBinFam, FromBin, FromBinPN
            , ToDevice, ToBinFam, ToBin, ToBinPN);
            }
        }

        private static bool HasData(string FromBinFam,string FromBin)
        {
            var sql = "select FromBinFam,FromBin from WAT.dbo.WATBINSubstitute where FromBinFam = @FromBinFam and FromBin = @FromBin";
            var dict = new Dictionary<string, string>();
            dict.Add("@FromBinFam", FromBinFam);
            dict.Add("@FromBin", FromBin);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            if (dbret.Count > 0)
            { return true; }
            return false;
        }

        private static void UpdateData(string FromDevice,string FromBinFam,string FromBin,string FromBinPN
            ,string ToDevice,string ToBinFam,string ToBin,string ToBinPN)
        {
            var sql = @"update WAT.dbo.WATBINSubstitute set FromDevice = @FromDevice,FromBinPN=@FromBinPN 
                , ToDevice = @ToDevice, ToBinFam = @ToBinFam, ToBin = @ToBin, ToBinPN = @ToBinPN where FromBinFam = @FromBinFam and FromBin = @FromBin";

            var dict = new Dictionary<string, string>();
            dict.Add("@FromDevice", FromDevice);
            dict.Add("@FromBinFam", FromBinFam);
            dict.Add("@FromBin", FromBin);
            dict.Add("@FromBinPN", FromBinPN);
            dict.Add("@ToDevice", ToDevice);
            dict.Add("@ToBinFam", ToBinFam);
            dict.Add("@ToBin", ToBin);
            dict.Add("@ToBinPN", ToBinPN);
            DBUtility.ExeLocalSqlNoRes(sql, dict);

        }

        private static void InsertData(string FromDevice, string FromBinFam, string FromBin, string FromBinPN
            , string ToDevice, string ToBinFam, string ToBin, string ToBinPN)
        {
            var sql = @"insert into WAT.dbo.WATBINSubstitute(FromDevice, FromBinFam, FromBin, FromBinPN
            , ToDevice, ToBinFam, ToBin, ToBinPN) values(@FromDevice, @FromBinFam, @FromBin, @FromBinPN
            , @ToDevice, @ToBinFam, @ToBin, @ToBinPN)";
            var dict = new Dictionary<string, string>();

            dict.Add("@FromDevice", FromDevice);
            dict.Add("@FromBinFam", FromBinFam);
            dict.Add("@FromBin", FromBin);
            dict.Add("@FromBinPN", FromBinPN);
            dict.Add("@ToDevice", ToDevice);
            dict.Add("@ToBinFam", ToBinFam);
            dict.Add("@ToBin", ToBin);
            dict.Add("@ToBinPN", ToBinPN);

            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public static List<object> GetBinSubstituteWafers()
        {
            var ret = new List<object>();

            var sql = @"select distinct r.wafer,r.result,p.BIN,p.BINCount,bs.ToBin,bs.FromBinPN,bs.ToBinPN from WAT.dbo.WATResult  (nolock) r
                        left join [WAT].[dbo].[WaferPassBinData] (nolock) p on r.wafer = p.WAFER
                        left join [EngrData].[dbo].[WXEvalPN] (nolock) ep on ep.WaferNum = r.wafer
                        left join WAT.dbo.WATBINSubstitute (nolock) bs on ep.Product = bs.FromDevice and p.BIN = bs.FromBin
                        left join WAT.dbo.SlovedBinSubstitute (nolock) s on s.Wafer = r.wafer and s.FromBin = p.BIN
                        where r.teststep = 'POSTHTOL2JUDGEMENT' and r.result not like 'fail to%' 
                        and r.result not like '%necessary coupon%' and p.BINCount > 100 and ep.Product is not null 
                        and bs.ToBin is not null and s.Wafer is null order by wafer";

            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                ret.Add(new
                {
                    wafer = UT.O2S(line[0]),
                    result = UT.O2S(line[1]),
                    frombin = UT.O2S(line[2]),
                    bincount = UT.O2I(line[3]).ToString(),
                    tobin = UT.O2S(line[4]),
                    fpn = UT.O2S(line[5]),
                    tpn = UT.O2S(line[6])
                });
            }

            return ret;
        }

        public static void AddSolvedBinSubstitute(string wafer, string fbin)
        {
            var sql = "insert into WAT.dbo.SlovedBinSubstitute(Wafer,FromBin) values(@Wafer,@FromBin)";
            var dict = new Dictionary<string, string>();
            dict.Add("@Wafer", wafer);
            dict.Add("@FromBin", fbin);
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public BinSubstitute(string pd,string bi,string ft,string tsn,double ll, double hl)
        {
            Product = pd;
            Bin = bi;
            FilterType = ft;
            TestName = tsn;
            LowLimit = ll;
            HighLimit = hl;
            BinPN = "";
            BinFam = "";
        }

        public BinSubstitute()
        {
            Product = "";
            Bin = "";
            FilterType = "";
            TestName = "";
            LowLimit = 0;
            HighLimit = 0;
            BinPN = "";
            BinFam = "";
        }

        public string Product{ set; get; }
        public string Bin { set; get; }
        public string FilterType { set; get; }
        public string TestName { set; get; }
        public double LowLimit { set; get; }
        public double HighLimit { set; get; }
        public string BinPN { set; get; }
        public string BinFam { set; get; }

    }
}