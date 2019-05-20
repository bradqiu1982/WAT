using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAT.Models
{
    public class ContainerInfo
    {
        public static ContainerInfo GetInfo(string containname)
        {
            var ret = new ContainerInfo();
            var sql = @"Select  Left(pf.ProductFamilyName,4)
                          ,pt.ProductTypeName
                          ,pb.ProductName
                          ,c.containertype
                          ,c.containertype
                          ,sb.specname
                          ,pf.productfamilyname
                          ,c.factorystartdate
                         FROM 
                          insite.insite.Container c with (nolock)
                          inner join insite.insite.Product p with (nolock) on c.ProductID = P.ProductID
                          inner join insite.insite.ProductBase pb with (nolock) on p.ProductID = pb.RevOfRcdID
                          inner join insite.insite.ProductFamily pf with (nolock) on p.ProductFamilyID = pf.ProductFamilyID
                          inner join insite.insite.ProductType pt with (nolock) on p.ProductTypeID = pt.ProductTypeID

                          inner join insite.insite.currentstatus cs with(nolock) on cs.currentstatusid=c.currentstatusid
                          inner join insite.insite.spec s with(nolock) on cs.specid=s.specid
                          inner join insite.insite.specbase sb with(nolock) on sb.specbaseid=s.specbaseid

                         Where 
                          c.ContainerName = @containername";
            var dict = new Dictionary<string, string>();
            dict.Add("@containername", containname.ToUpper().Trim());
            var dbret = DataBaseUT.ExeAllenSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                ret.ProdFam = UT.O2S(line[0]);
                ret.ProductType = UT.O2S(line[1]);
                ret.ProductName = UT.O2S(line[2]);
                ret.containertype = UT.O2S(line[3]);
                ret.lottype = UT.O2S(line[4]);
                ret.SpecName = UT.O2S(line[5]);
                ret.productfamily = UT.O2S(line[6]);
                ret.factorystartdate = UT.O2T(line[7]);
                ret.containername = containname.ToUpper().Trim();
                ret.wafer = ret.containername.Split(new string[] { "E", "e" }, StringSplitOptions.RemoveEmptyEntries)[0].Trim();
            }

            return ret;
        }

        public ContainerInfo()
        {
            containername = "";
            ProdFam = "";
            ProductType = "";
            ProductName = "";
            productfamily = "";
            wafer = "";
            containertype = "";
            lottype = "";
            SpecName = "";
            factorystartdate = DateTime.Parse("1982-05-06 10:00:00");
        }
        
        public string containername { set; get; }
        public string ProdFam { set; get; }
        public string ProductType { set; get; }
        public string ProductName { set; get; }
        public string productfamily { set; get; }
        public string wafer { set; get; }
        public string containertype { set; get; }
        public string lottype { set; get; }
        public string SpecName { set; get; }
        public DateTime factorystartdate { set; get; }
    }
}