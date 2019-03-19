﻿using System;
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
            var filetype = "DIESORT";
            var loadedfiledict = FileLoadedData.LoadedFiles(filetype);
            var fs = loadedfiledict.Keys.ToList();
            var ret = new JsonResult();
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
            var fs = Request.Form["fs"].ToUpper();

            WebLog.LogVisitor(Request.UserHostName, "try to review file:" + fs);

            var filetype = "DIESORT";
            var loadedfiledict = FileLoadedData.LoadedFiles(filetype);

            if (!loadedfiledict.ContainsKey(fs))
            {
                WebLog.Log("DieSort", "try to review file:" + fs + ", has not been converted");

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
                var filepath = System.IO.Path.Combine(reviewfolder, fs);
                if (ExternalDataCollector.FileExist(this, filepath))
                {
                    var datalist = DieSortVM.RetrieveReviewData(filepath);
                    var chartdata = DieSortVM.RetrieveDieChartData(datalist);
                    var pnarrayinfo = DieSortVM.GetBomPNArrayInfo(filepath);

                    var ret = new JsonResult();
                    ret.MaxJsonLength = Int32.MaxValue;
                    ret.Data = new
                    {
                        sucess = true,
                        chartdata = chartdata,
                        pn = pnarrayinfo[0],
                        warray = pnarrayinfo[1],
                        desc = pnarrayinfo[2]
                    };
                    return ret;
                }
                else
                {
                    WebLog.Log("DieSort", "try to review file:" + fs + ", not exist in review folder");

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
            var fs = Request.Form["fs"].ToUpper();

            WebLog.LogVisitor(Request.UserHostName, "try to re-construct file:" + fs);

            if (DieSortVM.LoadDieSortFile(this, fs))
            {
                var syscfgdict = CfgUtility.GetSysConfig(this);
                var reviewfolder = syscfgdict["DIESORTREVIEW"];
                var filepath = System.IO.Path.Combine(reviewfolder, fs);
                var datalist = DieSortVM.RetrieveReviewData(filepath);
                var chartdata = DieSortVM.RetrieveDieChartData(datalist);
                var pnarrayinfo = DieSortVM.GetBomPNArrayInfo(filepath);

                var ret = new JsonResult();
                ret.MaxJsonLength = Int32.MaxValue;
                ret.Data = new
                {
                    sucess = true,
                    chartdata = chartdata,
                    pn = pnarrayinfo[0],
                    warray = pnarrayinfo[1],
                    desc = pnarrayinfo[2]

                };
                return ret;
            }
            else
            {
                WebLog.Log("DieSort", "fail to re-construct file:" + fs );

                var ret = new JsonResult();
                ret.Data = new
                {
                    sucess = false
                };
                return ret;
            }
        }

        public ActionResult CompareDieSortedMapFile()
        {
            return View();
        }

        public JsonResult CompareDieSortData()
        {
            var fs = Request.Form["fs"].ToUpper();

            WebLog.LogVisitor(Request.UserHostName, "try to compare file:" + fs);

            var filetype = "DIESORT";
            var loadedfiledict = FileLoadedData.LoadedFiles(filetype);

            if (!loadedfiledict.ContainsKey(fs))
            {
                WebLog.Log("DieSort", "try to compare file:" + fs+", has not been converted");

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
                var originalfilefolder = syscfgdict["DIESORTFOLDER"];
                var newfilefolder = syscfgdict["DIESORTSHARE"];

                var orgfile = System.IO.Path.Combine(originalfilefolder, fs);
                var newfile = System.IO.Path.Combine(newfilefolder, fs);
                if (ExternalDataCollector.FileExist(this, orgfile) && ExternalDataCollector.FileExist(this, newfile))
                {
                    var orgdatalist = DieSortVM.RetrieveCMPData(orgfile);
                    var newdatalist = DieSortVM.RetrieveCMPData(newfile);

                    var ochartdata = DieSortVM.RetrieveDieChartData(orgdatalist,"die_sort_org_id","Orignal Die Data");
                    var nchartdata = DieSortVM.RetrieveDieChartData(newdatalist, "die_sort_new_id", "New Die Data");
                    var pnarrayinfo = DieSortVM.GetBomPNArrayInfo(newfile);

                    var ret = new JsonResult();
                    ret.MaxJsonLength = Int32.MaxValue;
                    ret.Data = new
                    {
                        sucess = true,
                        ochartdata = ochartdata,
                        nchartdata = nchartdata,
                        pn = pnarrayinfo[0],
                        warray = pnarrayinfo[1],
                        desc = pnarrayinfo[2]
                    };
                    return ret;
                }
                else
                {
                    var ret = new JsonResult();
                    ret.Data = new
                    {
                        sucess = false
                    };
                    return ret;
                }
            }//end else
        }


    }

}