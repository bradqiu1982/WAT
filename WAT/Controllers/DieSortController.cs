using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using WAT.Models;

namespace WAT.Controllers
{
    public class DieSortController : Controller
    {
        private ActionResult Jump2Welcome(string url)
        {
            var dict = new RouteValueDictionary();
            dict.Add("url", url);
            return RedirectToAction("Welcome", "Main", dict);
        }

        private bool CheckName(string ip, string url)
        {
            var machinename = MachineUserMap.GetUserMachineName(ip);
            if (machinename.Count == 0)
            { return false; }
            else
            {
                MachineUserMap.LoginLog(machinename[0], machinename[1], url);
                return true;
            }
        }

        public JsonResult LoadSortedFiles()
        {
            var filetype = "WAFER";
            var loadedfiledict = FileLoadedData.LoadedFiles(filetype);
            var fs = loadedfiledict.Keys.ToList();
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                filelist = fs
            };
            return ret;
        }

        public ActionResult ReviewDieSortedMapFile()
        {
            return View();
        }


        public JsonResult ReviewDieData()
        {
            var wafer = Request.Form["fs"].ToUpper();

            WebLog.LogVisitor(Request.UserHostName, "try to review file:" + wafer);

            var filetype = "WAFER";
            var loadedfiledict = FileLoadedData.LoadedFiles(filetype);

            if (!loadedfiledict.ContainsKey(wafer))
            {
                WebLog.Log(wafer,"DIESORT", "try to review file:" + wafer + ", has not been converted");

                var ret = new JsonResult();
                ret.Data = new
                {
                    sucess = false
                };
                return ret;
            }
            else
            {
                var syscfgdict = CfgUtility.GetSysConfig(this);
                var reviewfolder = syscfgdict["DIESORTREVIEW"];
                var allreviewfiles = ExternalDataCollector.DirectoryEnumerateAllFiles(this, reviewfolder);
                var fs = "";
                foreach (var f in allreviewfiles)
                {
                    if (f.Contains(wafer))
                    {
                        fs = f;
                        break;
                    }
                }
               
                if (ExternalDataCollector.FileExist(this, fs))
                {
                    var datalist = DieSortVM.RetrieveReviewData(fs);
                    var chartdata = DieSortVM.RetrieveDieChartData(datalist);
                    var pnarrayinfo = DieSortVM.GetBomPNArrayInfo(fs);
                    var sampledata = DieSortVM.RetrieveSampleData(wafer);

                    var ret = new JsonResult();
                    ret.MaxJsonLength = Int32.MaxValue;
                    ret.Data = new
                    {
                        sucess = true,
                        chartdata = chartdata,
                        pn = pnarrayinfo[0],
                        warray = pnarrayinfo[1],
                        desc = pnarrayinfo[2],
                        sampledata = sampledata
                    };
                    return ret;
                }
                else
                {
                    WebLog.Log(wafer,"DIESORT", "try to review file:" + wafer + ", not exist in review folder");

                    var ret = new JsonResult();
                    ret.Data = new
                    {
                        sucess = false
                    };
                    return ret;
                }
            }//end else
        }

        public JsonResult ReConstructDieSort()
        {
            var wafer = Request.Form["fs"].ToUpper();
            var offeredpn = Request.Form["pn"];
            var ctype = Request.Form["ctype"];

            var supply = false;
            if (!string.IsNullOrEmpty(ctype) && ctype.Contains("S"))
            {
                supply = true;
            }

            WebLog.LogVisitor(Request.UserHostName, "try to re-construct file:" + wafer);

            var allfiles = DieSortVM.GetAllWaferFile(this);
            if (DieSortVM.SolveANewWafer(wafer,allfiles,this,offeredpn,supply))
            {
                var syscfgdict = CfgUtility.GetSysConfig(this);
                var reviewfolder = syscfgdict["DIESORTREVIEW"];
                var allreviewfiles = ExternalDataCollector.DirectoryEnumerateAllFiles(this, reviewfolder);
                var fs = "";
                foreach (var f in allreviewfiles)
                {
                    if (f.Contains(wafer))
                    {
                        fs = f;
                        break;
                    }
                }

                var datalist = DieSortVM.RetrieveReviewData(fs);
                var chartdata = DieSortVM.RetrieveDieChartData(datalist);
                var pnarrayinfo = DieSortVM.GetBomPNArrayInfo(fs);
                var sampledata = DieSortVM.RetrieveSampleData(wafer);

                var ret = new JsonResult();
                ret.MaxJsonLength = Int32.MaxValue;
                ret.Data = new
                {
                    sucess = true,
                    chartdata = chartdata,
                    pn = pnarrayinfo[0],
                    warray = pnarrayinfo[1],
                    desc = pnarrayinfo[2],
                    sampledata = sampledata
                };
                return ret;
            }
            else
            {
                WebLog.Log(wafer,"DIESORT", "fail to re-construct file:" + wafer );

                var ret = new JsonResult();
                ret.Data = new
                {
                    sucess = false
                };
                return ret;
            }
        }


        public JsonResult UpdateIgnoreDieSort()
        {
            var fs = Request.Form["fs"].ToUpper();
            WebLog.UpdateIgnoreDieSort(fs);
            var ret = new JsonResult();
            ret.Data = new { sucess = true };
            return ret;
        }

        public JsonResult GetFailedConvertFile()
        {
            var datalist = WebLog.GetFailedConvertFiles();
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new {
                datalist = datalist
            };
            return ret;
        }

        public ActionResult DieSortFiailedConvertFiles()
        {
            return View();
        }

        public ActionResult Wafer4Planning()
        { return View(); }

        public JsonResult LoadSortedWafers()
        {
            var waferlist = DieSortVM.SortedWaferNum();
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                wafers = waferlist
            };

            return ret;
        }

        public JsonResult LoadWaferData4Plan()
        {
            var wafernum = Request.Form["wafernum"].Trim();
            var sampledata = DieSortVM.waferBinSubstitude(wafernum,this);
            var waferbindata = DieSortVM.BinData4Plan(wafernum,this);
            var wafersrcdata = DieSortVM.SrcData4Plan(wafernum,this);

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                sampledata = sampledata,
                waferbindata = waferbindata,
                wafersrcdata = wafersrcdata
            };

            return ret;
        }

        public ActionResult DownLoadPDMapFile()
        {
            return View();
        }

        public JsonResult DownLoadPDMapFileData()
        {
            var syscfgdict = CfgUtility.GetSysConfig(this);
            var wf = Request.Form["wf"].Trim();

            var productfm = "";
            if (wf.Length == 13)
            {
                productfm = WXEvalPN.GetProductFamilyFromSherman(wf);
            }
            else
            {
                productfm = WXEvalPN.GetProductFamilyFromAllen(wf);
                if (string.IsNullOrEmpty(productfm))
                {
                    productfm = WXEvalPN.GetProductFamilyFromSherman(wf);
                }
            }

            var allfilelist = new List<string>();
            if (string.IsNullOrEmpty(productfm))
            {
                allfilelist = DieSortVM.GetAllWaferFile(this);
            }
            else
            {

                var srcfolder = syscfgdict["DIESORTFOLDER"]+"\\"+productfm;
                allfilelist = ExternalDataCollector.DirectoryEnumerateAllFiles(this, srcfolder);
            }

            var fs = "";
            foreach (var f in allfilelist)
            {
                if (f.Contains(wf))
                {
                    fs = f;
                }
            }

            if (string.IsNullOrEmpty(fs))
            {
                var ret = new JsonResult();
                ret.MaxJsonLength = Int32.MaxValue;
                ret.Data = new
                {
                    sucess = false,
                    MSG = "Fail to get map file for PD wafer: "+wf
                };
                return ret;
            }
            else
            {
                var desf = syscfgdict["PDSHAREFOLDER"] + "\\" + System.IO.Path.GetFileName(fs);
                ExternalDataCollector.FileCopy(this, fs, desf, true);
                if (ExternalDataCollector.FileExist(this, desf))
                {
                    var pddatalist = DieSortVM.RetrievePDData(desf);
                    var chartdata = DieSortVM.RetrieveDieChartData(pddatalist, "pd_wafer_id", "PD " + wf + " Distribution");
                    var ret = new JsonResult();
                    ret.MaxJsonLength = Int32.MaxValue;
                    ret.Data = new
                    {
                        sucess = true,
                        chartdata = chartdata
                    };
                    return ret;
                }
                else
                {
                    var ret = new JsonResult();
                    ret.MaxJsonLength = Int32.MaxValue;
                    ret.Data = new
                    {
                        sucess = false,
                        MSG = "Fail to download PD map file of wafer: " + wf
                    };
                    return ret;
                }//end else
            }//end else
        }

        public ActionResult WaferBinSubstitute()
        {
            return View();
        }

        public JsonResult LoadBinSubstituteData()
        {
            var binsubdata = BinSubstitute.GetBinSubstituteWafers();
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                binsubdata = binsubdata
            };
            return ret;
        }

        public JsonResult ConvertBinMapFileData()
        {
            var MSG = "the map file is converted!";
            var wf = Request.Form["wf"];
            var fbin = Request.Form["fbin"];
            var tbin = Request.Form["tbin"];

            var msg = DieSortVM.ConvertBinMapFileData(wf,fbin,tbin,this);
            if (!string.IsNullOrEmpty(msg))
            { MSG = msg; }
            else
            { BinSubstitute.AddSolvedBinSubstitute(wf, fbin); }

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                MSG = MSG
            };
            return ret;
        }

        public ActionResult SamplePick4Ipoh()
        { return View(); }

        public JsonResult SamplePick4IpohData() {
            var wafer = Request.Form["wafer"];
            var MSG = DieSortVM.PickSample4Ipoh(wafer,this);
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                MSG = MSG
            };
            return ret;
        }

        public ActionResult E01SamplePick4Ipoh()
        {
            var url = "/DieSort/E01SamplePick4Ipoh";
            if (!CheckName(Request.UserHostName, url))
            { return Jump2Welcome(url); }

            return View();
        }

        public JsonResult E01SamplePick4IpohData()
        {
            var wafer = Request.Form["wafer"].Trim();
            var MSG = DieSortVM.PickE01Sample4Ipoh(wafer, this);
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                MSG = MSG
            };
            return ret;
        }

        public ActionResult WATSamplePick()
        {
            var url = "/DieSort/WATSamplePick";
            if (!CheckName(Request.UserHostName, url))
            { return Jump2Welcome(url); }

            return View();
        }

        public JsonResult WATSamplePickData()
        {
            var syscfg = CfgUtility.GetSysConfig(this);
            var marks = Request.Form["marks"];
            List<string> allwflist = (List<string>)Newtonsoft.Json.JsonConvert.DeserializeObject(marks, (new List<string>()).GetType());
            var wfdatalist = new List<object>();
            var allfile = ExternalDataCollector.DirectoryEnumerateFiles(this, syscfg["DIESORTSHARE"]);

            var idx = 0;
            var wflist = new List<string>();
            foreach (var item in allwflist)
            {
                wflist.Add(item);
                idx++;
                if (idx > 10)
                { break; }
            }

            var existwf = new List<string>();
            foreach (var wf in wflist)
            {
                foreach (var f in allfile)
                {
                    if (f.Contains(wf))
                    {
                        existwf.Add(wf);
                    }//end if
                }//end foreach
            }//end foreach

            var probewf = new List<string>();
            foreach (var wf in wflist)
            {
                if (existwf.Contains(wf))
                {
                    wfdatalist.Add(new
                    {
                        wf = wf,
                        stat = "SAMPLE MAP FILE EXIST"
                    });
                }
                else
                {
                    probewf.Add(wf);
                }
            }

            if (probewf.Count > 0)
            {
                Models.WXProbeData.PrepareNeoMapData2Allen(probewf);
            }//end if

            var samplewf = new List<string>();
            foreach (var wf in probewf)
            {
                if (Models.WXProbeData.AllenHasData(wf))
                { samplewf.Add(wf); }
                else
                {
                    wfdatalist.Add(new
                    {
                        wf = wf,
                        stat = "FAIL,NO PROBE DATA"
                    });
                }
            }

            var allfiles = DieSortVM.GetAllWaferFile(this);
            foreach (var wf in samplewf)
            {
                if (DieSortVM.SolveANewWafer(wf, allfiles, this, "", false))
                {
                    wfdatalist.Add(new
                    {
                        wf = wf,
                        stat = "OK"
                    });
                }
                else
                {
                    wfdatalist.Add(new
                    {
                        wf = wf,
                        stat = "FAIL,No SAMPLE MAP FILE GENERATED"
                    });
                }
            }

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                wfdatalist = wfdatalist
            };
            return ret;
        }

        public ActionResult IIVISamplePick()
        {
            var url = "/DieSort/IIVISamplePick";
            if (!CheckName(Request.UserHostName, url))
            { return Jump2Welcome(url); }

            return View();
        }

        public JsonResult IIVISamplePickData()
        {
            //get query condition
            var syscfg = CfgUtility.GetSysConfig(this);
            var marks = Request.Form["marks"];
            List<string> allwflist = (List<string>)Newtonsoft.Json.JsonConvert.DeserializeObject(marks, (new List<string>()).GetType());
            var idx = 0;
            var wflist = new List<string>();
            foreach (var item in allwflist)
            {
                wflist.Add(item);
                idx++;
                if (idx > 10)
                { break; }
            }

            var wfdatalist = new List<object>();

            //get all exist files
            var mapfolder = syscfg["IIVIMAPFILE"];
            var samplefolder = syscfg["IIVISAMPLEPICK"];
            //var requestcnt = UT.O2I(syscfg["IIVISAMPLESIZE"]);
            var IIVIPNs = syscfg["IIVIPN"].Split(new string[] { ","},StringSplitOptions.RemoveEmptyEntries);
            var IIVIPNDict = new Dictionary<string, string>();
            foreach (var ip in IIVIPNs)
            {
                var pmap = ip.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                IIVIPNDict.Add(pmap[0], pmap[1]);
            }

            var allsamplefile = ExternalDataCollector.DirectoryEnumerateFiles(this, samplefolder);
            var allsrcfile = ExternalDataCollector.DirectoryEnumerateFiles(this, mapfolder);
            var samplefsdict = new Dictionary<string, bool>();
            foreach (var ef in allsamplefile)
            {
                var fs = System.IO.Path.GetFileNameWithoutExtension(ef);
                if (!samplefsdict.ContainsKey(fs)) { samplefsdict.Add(fs, true); }
            }
            var srcdict = new Dictionary<string, bool>();
            foreach (var sf in allsrcfile)
            {
                var fs = System.IO.Path.GetFileName(sf).Trim().ToUpper();
                if (!srcdict.ContainsKey(fs)) { srcdict.Add(fs, true); }
            }

            //check exist files
            var tobewf = new List<string>();
            foreach (var wf in wflist)
            {
                var stat = IIVIVcselVM.CheckWaferFile(wf, allsamplefile, samplefsdict, allsrcfile, srcdict);
                if (!string.IsNullOrEmpty(stat))
                {
                    wfdatalist.Add(new
                    {
                        wf = wf,
                        stat = stat
                    });
                }
                else
                { tobewf.Add(wf); }
            }

            //try to pick sample data
            foreach (var wf in tobewf)
            {
                var stat = IIVIVcselVM.SolveIIVIWafer(wf, allsrcfile, mapfolder, samplefolder, syscfg,IIVIPNDict);
                wfdatalist.Add(new
                {
                    wf = wf,
                    stat = stat
                });
            }//end foreach

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                wfdatalist = wfdatalist
            };
            return ret;
        }

        public ActionResult Wafer2DC()
        {
            return View();
        }

        public JsonResult WF2DCData()
        {
            var marks = Request.Form["marks"];
            List<string> allwflist = (List<string>)Newtonsoft.Json.JsonConvert.DeserializeObject(marks, (new List<string>()).GetType());
            var idx = 0;
            var wflist = new List<string>();
            foreach (var item in allwflist)
            {
                wflist.Add(item.Replace("'","").Trim().ToUpper()
                    .Split(new string[]{ "E","R","T" },StringSplitOptions.RemoveEmptyEntries)[0]);
                idx++;
                if (idx > 10)
                { break; }
            }

            var wfcond = "('"+string.Join("','",wflist)+"')";

            var wfdatalist = new List<object>();
            var sql = @"select distinct ddc.ParamValueString,dc.ParamValueString,mf.MfgOrderName,mf.ReleaseDate from [InsiteDB].[insite].[dc_IQC_InspectionResult] (nolock) dc 
	                 inner join  [InsiteDB].[insite].[dc_IQC_InspectionResult] ddc (nolock) on ddc.HistoryMainlineId = dc.HistoryMainlineId 
	                 left join InsiteDB.insite.container fco (nolock) on fco.ContainerName = dc.ParamValueString
	                 left join insitedb.insite.IssueActualsHistory iah with(nolock) on iah.FromContainerId = fco.ContainerId
	                 left join  InsiteDB.insite.container co with (nolock) on iah.ToContainerId = co.ContainerId
	                 left join InsiteDB.insite.MfgOrder mf (nolock) on co.MfgOrderId = mf.MfgOrderId
	                 where dc.ParameterName = 'QAID' and ddc.ParamValueString in "+wfcond;

            var dbret = DBUtility.ExeRealMESSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var wf = UT.O2S(line[0]);
                var dc = UT.O2S(line[1]);
                var jo = UT.O2S(line[2]);
                var date = UT.O2T(line[3]).ToString("yyyy-MM-dd");
                wfdatalist.Add(new {
                    wf = wf,
                    dc = dc,
                    jo = jo,
                    date = date
                });
            }

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                wfdatalist = wfdatalist
            };
            return ret;
        }


    }

}