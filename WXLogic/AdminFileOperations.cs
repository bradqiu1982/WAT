using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace WXLogic
{
    public class AdminFileOperations
    {
        public static List<string> DirectoryEnumerateAllFiles(string dirname)
        {
            try
            {
                var syscfgdict = WXCfg.GetSysCfg();
                var folderpwd = syscfgdict["SHAREFOLDERPWD"];

                using (NativeMethods cv = new NativeMethods("brad.qiu", "china", folderpwd))
                {
                    var ret = new Dictionary<string, string>();
                    var nofolderfiles = Directory.GetFiles(dirname);
                    foreach (var f in nofolderfiles)
                    {
                        var uf = f.ToUpper();
                        if (!ret.ContainsKey(uf))
                        { ret.Add(uf, f); }
                    }

                    var folders = Directory.GetDirectories(dirname);
                    foreach (var fd in folders)
                    {
                        var fs = Directory.GetFiles(fd);
                        foreach (var f in fs)
                        {
                            var uf = f.ToUpper();
                            if (!ret.ContainsKey(uf))
                            { ret.Add(uf, f); }
                        }
                    }
                    return ret.Values.ToList();
                }
            }
            catch (Exception ex)
            {
                return new List<string>();
            }
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

        public static List<int> GetDieOneByFile(string FileName)
        {
            var ret = new List<int>();
            var syscfgdict = WXCfg.GetSysCfg();

            var folderpwd = syscfgdict["SHAREFOLDERPWD"];
            using (NativeMethods cv = new NativeMethods("brad.qiu", "china", folderpwd))
            {
                var doc = new XmlDocument();
                doc.Load(FileName);
                var namesp = doc.DocumentElement.GetAttribute("xmlns");
                doc = StripNamespace(doc);
                XmlElement root = doc.DocumentElement;
                var dieonenodelist = root.SelectNodes("//BinDefinition[@BinDescription='DIE_ONE']");
                var dieonebincode = "";
                foreach (XmlElement nd in dieonenodelist)
                {
                    try
                    {
                        dieonebincode = nd.GetAttribute("BinCode");
                        break;
                    }
                    catch (Exception ex) { }
                }

                if (string.IsNullOrEmpty(dieonebincode))
                { return ret; }

                var dieonex = "";
                var dieoney = "";

                var bincodelist = root.SelectNodes("//BinCode[@X and @Y]");
                foreach (XmlElement nd in bincodelist)
                {
                    try
                    {
                        if (string.Compare(nd.InnerText, dieonebincode, true) == 0)
                        {
                            dieonex = nd.GetAttribute("X");
                            dieoney = nd.GetAttribute("Y");
                            break;
                        }
                    }
                    catch (Exception ex) { }
                }

                if (string.IsNullOrEmpty(dieonex) || string.IsNullOrEmpty(dieoney))
                { return ret; }

                var dieonexs = new List<int>();
                bincodelist = root.SelectNodes("//BinCode[@X and @Y='"+dieoney+"']");
                foreach (XmlElement nd in bincodelist)
                {
                    try
                    {
                        dieonexs.Add(UT.O2I(nd.GetAttribute("X")));
                    }
                    catch (Exception ex) { }
                }

                if (dieonexs.Count == 0)
                { return ret; }

                ret.Add(UT.O2I(dieonex));
                ret.Add(dieonexs.Min());
            }

            return ret;
        }

        public static List<int> GetDieOneByWafer(string wafer)
        {
            var ret = new List<int>();
            var syscfgdict = WXCfg.GetSysCfg();
            var reviewdir = syscfgdict["DIESORTREVIEW"];
            var fs = "";
            var allfiles = DirectoryEnumerateAllFiles(reviewdir);
            foreach (var f in allfiles)
            {
                if (f.Contains(wafer))
                {
                    fs = f;
                    break;
                }
            }

            if (string.IsNullOrEmpty(fs))
            { return ret; }

            return GetDieOneByFile(fs);
        }


    }
}
