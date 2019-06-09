using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace WAT.Models
{
    public class WXCfg
    {
        public static Dictionary<string, string> GetSysCfg()
        {
            var ret = new Dictionary<string, string>();
            try
            {
                var fs = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WXCfg.txt");
                if (File.Exists(fs))
                {
                    var lines = System.IO.File.ReadAllLines(fs);
                    foreach (var line in lines)
                    {
                        if (line.Contains("##"))
                        {
                            continue;
                        }

                        if (line.Contains(":::"))
                        {
                            var kvpair = line.Split(new string[] { ":::" }, StringSplitOptions.RemoveEmptyEntries);
                            if (!ret.ContainsKey(kvpair[0].Trim()) && kvpair.Length > 1)
                            {
                                ret.Add(kvpair[0].Trim(), kvpair[1].Trim());
                            }
                        }//end if
                    }//end foreach
                }
            }
            catch (Exception ex) { }

            return ret;
        }


    }
}