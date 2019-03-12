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


        private object RetrieveDieChartData(List<DieSortVM> datalist)
        {
            var data = new List<List<object>>();
            var xlist = new List<int>();
            var ylist = new List<int>();
            foreach (var v in datalist)
            {
                var templist = new List<object>();
                templist.Add(v.X);
                templist.Add(v.Y);
                templist.Add(v.DieValue);
                data.Add(templist);

                xlist.Add(v.X);
                ylist.Add(v.Y);
            }
            var serial = new List<object>();
            serial.Add(new
            {
                name = "Die Sort",
                data = data,
                boostThreshold = 100,
                borderWidth =  0,
                nullColor = "#EFEFEF"
            });

            xlist.Sort();
            ylist.Sort();
            return new {
                id = "die_sort_id",
                title = "Die Sort Pick Map",
                serial = serial,
                xmax = xlist[xlist.Count -1]+20,
                ymax = ylist[ylist.Count -1]+20
            };
        }

        public JsonResult ReviewDieData()
        {
            var fs = Request.Form["fs"].ToUpper();
            var filetype = "DIESORT";
            var loadedfiledict = FileLoadedData.LoadedFiles(filetype);

            if (!loadedfiledict.ContainsKey(fs))
            {
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
                    var chartdata = RetrieveDieChartData(datalist);


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
            if (DieSortVM.LoadDieSortFile(this, fs))
            {
                var syscfgdict = CfgUtility.GetSysConfig(this);
                var reviewfolder = syscfgdict["DIESORTREVIEW"];
                var filepath = System.IO.Path.Combine(reviewfolder, fs);
                var datalist = DieSortVM.RetrieveReviewData(filepath);
                var chartdata = RetrieveDieChartData(datalist);

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
                ret.Data = new
                {
                    sucess = false
                };
                return ret;
            }
        }

    }

}