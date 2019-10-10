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

    }

}