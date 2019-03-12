using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.IO;
using System.Web.Mvc;

namespace WAT.Models
{
    public class DieSortVM
    {
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
            var passbinnodes = root.SelectNodes("//BinDefinition[@BinQuality='Pass' and @BinCode]");
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

        private static Dictionary<string, string> GetSelectedXYDict(Dictionary<string, string> passedbinxydict,int selectcount=170)
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

            if (xlist.Count < 18 || ylist.Count < 27)
            { return new Dictionary<string, string>(); }

            var xsector = (xlist[xlist.Count-1] - xlist[0]) / 6;
            var ysector = (ylist[ylist.Count-1] - ylist[0]) / 9;

            var xstarterlist = new List<int>();
            xstarterlist.Add(xlist[0] + xsector);
            xstarterlist.Add(xlist[0] + 4 * xsector);

            var ystarterlist = new List<int>();
            ystarterlist.Add(ylist[0] + ysector);
            ystarterlist.Add(ylist[0] + 4*ysector);
            ystarterlist.Add(ylist[0] + 7*ysector);


            var rad = new Random(DateTime.Now.Millisecond);
            var idx = 0;
            var maxtimes = selectcount * 100;
            while (true)
            {
                var xrad = rad.Next(xsector);
                var yrad = rad.Next(ysector);

                foreach (var xstart in xstarterlist)
                {
                    foreach (var ystart in ystarterlist)
                    {
                        var k = (xstart + xrad).ToString() + ":::" + (ystart + yrad).ToString();
                        if (passedbinxydict.ContainsKey(k) && !ret.ContainsKey(k))
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

        public static bool SolveDieSortFile(string diefile,Controller ctrl)
        {
            var syscfgdict = CfgUtility.GetSysConfig(ctrl);
            var desfolder = syscfgdict["DIESORTSHARE"];
            var reviewfolder = syscfgdict["DIESORTREVIEW"];

            try
            {
                var doc = new XmlDocument();
                doc.Load(diefile);
                var namesp = doc.DocumentElement.GetAttribute("xmlns");
                doc = StripNamespace(doc);

                XmlElement root = doc.DocumentElement;
                var passedbinxydict = GetPassedBinXYDict(root);
                if (passedbinxydict.Count == 0)
                { return false; }

                var selectxydict = GetSelectedXYDict(passedbinxydict);
                if (selectxydict.Count == 0)
                { return false; }

                //foreach (var kv in passedbinxydict)
                //{
                //    var xystr = kv.Key.Split(new string[] { ":::" }, StringSplitOptions.RemoveEmptyEntries);
                //    foreach (XmlElement nd in root.SelectNodes("//BinCode[@X='" + xystr[0] + "' and @Y='" + xystr[1] + "']"))
                //    { nd.SetAttribute("Q", "pass"); }
                //}

                foreach (var kv in selectxydict)
                {
                    var xystr = kv.Key.Split(new string[] { ":::" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (XmlElement nd in root.SelectNodes("//BinCode[@X='"+xystr[0]+"' and @Y='"+xystr[1]+"']"))
                    { nd.SetAttribute("diesort","selected"); }
                }

                //var nodes = root.SelectNodes("//BinDefinition[@BinQuality='Pass']");
                //foreach (XmlElement nd in nodes)
                //{
                //    nd.SetAttribute("BinCount", "666");
                //}

                //foreach (XmlElement nd in root.SelectNodes("//BinCode[@X='240' and @Y='74']"))
                //{
                //    nd.ParentNode.RemoveChild(nd);
                //}

                doc.DocumentElement.SetAttribute("xmlns", namesp);
                var savename = Path.Combine(reviewfolder, Path.GetFileName(diefile));
                if (ExternalDataCollector.FileExist(ctrl, savename))
                { ExternalDataCollector.FileDelete(ctrl, savename); }
                doc.Save(savename);

                //doc = new XmlDocument();
                //doc.Load(diefile);
                //doc.Save(Path.Combine(desfolder, Path.GetFileName(diefile)));
            }
            catch (Exception ex) { return false; }

            return true;
        }

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

        public static bool LoadDieSortFile(Controller ctrl,string srcname)
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