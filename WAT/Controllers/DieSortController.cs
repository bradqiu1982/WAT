using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WAT.Models;

namespace WAT.Controllers
{
    public class DieSortController : Controller
    {
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
            var wafernum = Request.Form["wafernum"];
            var sampledata = DieSortVM.SampleData4Plan(wafernum);
            var waferorgdata = DieSortVM.OrgData4Plan(wafernum,this);
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                sampledata = sampledata,
                waferorgdata = waferorgdata
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


    }

}