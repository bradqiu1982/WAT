using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WAT.Models;

namespace WAT.Controllers
{
    public class WaferController : Controller
    {
        public ActionResult WaferQUAL()
        {
            return View();
        }

        public JsonResult WaferQUALData()
        {
            var ssdate = Request.Form["sdate"];
            var sedate = Request.Form["edate"];
            var startdate = DateTime.Now;
            var enddate = DateTime.Now;

            if (!string.IsNullOrEmpty(ssdate) && !string.IsNullOrEmpty(sedate))
            {
                var sdate = DateTime.Parse(Request.Form["sdate"]);
                var edate = DateTime.Parse(Request.Form["edate"]);
                if (sdate < edate)
                {
                    startdate = DateTime.Parse(sdate.ToString("yyyy-MM") + "-01 00:00:00");
                    enddate = DateTime.Parse(edate.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(1).AddSeconds(-1);
                }
                else
                {
                    startdate = DateTime.Parse(edate.ToString("yyyy-MM") + "-01 00:00:00");
                    enddate = DateTime.Parse(sdate.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(1).AddSeconds(-1);
                }
            }
            else
            {
                startdate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(-6);
                enddate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(1).AddSeconds(-1);
            }

            var waferdata = WaferQUALVM.RetrieveWaferData(startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"));

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                waferdata = waferdata
            };
            return ret;
        }

        private void CopyFileToLocal(string src)
        {
            var syscfg = CfgUtility.GetSysConfig(this);

            var startidx = 0;
            while (src.IndexOf("/userfiles/", startidx) != -1)
            {
                var imgsidx = src.IndexOf("/userfiles/", startidx);
                var imgeidx = src.IndexOf("target", imgsidx);
                if (imgeidx != -1)
                {
                    startidx = imgeidx;
                    imgeidx = imgeidx + 1;
                    var imgstr = src.Substring(imgsidx, (imgeidx - imgsidx - 1));
                    var url = imgstr.Replace("\"", "").Trim();

                    var sharefile = syscfg["WAFERQUALREPORT"] + url.Replace("/", "\\");
                    var fn = Path.GetFileName(sharefile);


                    var folders = url.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (url.Contains("/docs/"))
                    {
                        string imgdir = Server.MapPath("~/userfiles") + "\\docs\\" + folders[folders.Count - 2] + "\\";
                        if (!Directory.Exists(imgdir))
                        { Directory.CreateDirectory(imgdir); }
                        ExternalDataCollector.FileCopy(this, sharefile, imgdir + fn, false);
                    }
                    else if (url.Contains("/images/"))
                    {
                        string imgdir = Server.MapPath("~/userfiles") + "\\images\\" + folders[folders.Count - 2] + "\\";
                        if (!Directory.Exists(imgdir))
                        { Directory.CreateDirectory(imgdir); }
                        ExternalDataCollector.FileCopy(this, sharefile, imgdir + fn, false);
                    }
                }
                else
                {
                    startidx = imgsidx + 3;
                }
            }
        }


        public JsonResult WaferQUALReport()
        {
            var wkey = Request.Form["wkey"];
            var wreportlist = WaferReport.RetrieveWaferReport(wkey);
            if (wreportlist.Count == 0)
            {
                var report = new
                {
                    time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    reporter = "System",
                    content = "TO BE EDIT"
                };

                var ret = new JsonResult();
                ret.MaxJsonLength = Int32.MaxValue;
                ret.Data = new
                {
                    success = true,
                    report = report
                };
                return ret;
            }
            else
            {
                CopyFileToLocal(wreportlist[0].Comment);
                var report = new
                {
                    time = wreportlist[0].CommentDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    reporter = wreportlist[0].Reporter.ToUpper().Replace("@FINISAR.COM", ""),
                    content = wreportlist[0].Comment
                };
                var ret = new JsonResult();
                ret.MaxJsonLength = Int32.MaxValue;
                ret.Data = new
                {
                    success = true,
                    report = report
                };
                return ret;
            }
        }


        public JsonResult AllenRawData()
        {
            var wf = Request.Form["wf"];
            var waferdata = AllenEVALData.RetriewAllenData(wf);
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                waferdata = waferdata
            };
            return ret;
        }

        private string ParseJSBase64(string msg)
        {
            string dummyData = msg.Trim().Replace(" ", "+");
            if (dummyData.Length % 4 > 0)
                dummyData = dummyData.PadRight(dummyData.Length + 4 - dummyData.Length % 4, '=');

            var bytes = Convert.FromBase64String(dummyData);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }

        public JsonResult StoreAllenComment()
        {
            var comment = ParseJSBase64(Request.Form["comment"]);
            var wf = Request.Form["wf"];
            WaferQUALVM.UpdateAllenWATComment(wf, comment);
            var ret = new JsonResult();
            ret.Data = new { };
            return ret;
        }

        public JsonResult LoadAllenComment()
        {
            var wf = Request.Form["wf"];
            var comment = WaferQUALVM.RetrieveAllenWATComment(wf);
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new {
                comment= comment
            };
            return ret;
        }

    }
}