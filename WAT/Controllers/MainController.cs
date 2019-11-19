using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WAT.Models;
using WXLogic;

namespace WAT.Controllers
{
    public class MainController : Controller
    {
        // GET: Main
        public ActionResult Index()
        {
            return View();
        }

        private void heartbeatlog(string msg)
        {
            try
            {
                var filename = "log" + DateTime.Now.ToString("yyyy-MM-dd");
                var wholefilename = Server.MapPath("~/userfiles") + "\\" + filename;

                var content = "";
                if (System.IO.File.Exists(wholefilename))
                {
                    content = System.IO.File.ReadAllText(wholefilename);
                }
                content = content + msg + " @ " + DateTime.Now.ToString() + "\r\n";
                System.IO.File.WriteAllText(wholefilename, content);
            }
            catch (Exception ex)
            { }
        }


        public ActionResult HeartBeat()
        {
            try
            {
                var heartbeatinprocess = Server.MapPath("~/userfiles") + "\\" + "InHeartBeatProcess";
                if (System.IO.File.Exists(heartbeatinprocess))
                {
                    var lastinprocesstime = System.IO.File.GetLastWriteTime(heartbeatinprocess);
                    if ((DateTime.Now - lastinprocesstime).Hours >= 6)
                    { System.IO.File.Delete(heartbeatinprocess); }
                    else
                    { return View(); }
                }
                System.IO.File.WriteAllText(heartbeatinprocess, "hello");

                heartbeatlog("start heartbeat");

                var dailyscan = Server.MapPath("~/userfiles") + "\\" + "dailyscan_"+ DateTime.Now.ToString("yyyy-MM-dd");
                if (!System.IO.File.Exists(dailyscan))
                {
                    System.IO.File.WriteAllText(dailyscan, "hello");

                    heartbeatlog("DieSortVM.ScanNewWafer");
                    try
                    {
                        DieSortVM.ScanNewWafer(this);
                    }
                    catch (Exception ex) { }

                    heartbeatlog("WuxiWATData4MG.RefreshWATStatusDaily");
                    try
                    {
                        WuxiWATData4MG.RefreshWATStatusDaily();
                    }
                    catch (Exception ex) { }

                    heartbeatlog("BinSubstitute.RefreshBinSubstituteFromAllen");
                    try
                    {
                        BinSubstitute.RefreshBinSubstituteFromAllen();
                    }
                    catch (Exception ex) { }
                }

                try
                {
                    WATOven.RefreshDailyOvenData(this);
                }
                catch (Exception ex) { }

                //heartbeatlog("WaferQUALVM.LoadWUXIWaferQUAL");
                //try
                //{
                //    WaferQUALVM.LoadNewWaferFromMES();
                //    WaferQUALVM.LoadWUXIWaferQUAL();
                //}
                //catch (Exception ex) { }


                //heartbeatlog("AllenEVALData.LoadAllenData");
                //try
                //{
                //    AllenEVALData.LoadAllenData();
                //}
                //catch (Exception ex) { }

                heartbeatlog("end heartbeat");

                try
                { System.IO.File.Delete(heartbeatinprocess); }
                catch (Exception ex) { }
            }
            catch (Exception ex) { }

            return View();
        }

        //public ActionResult LoadWaferQUAL()
        //{
        //    WaferQUALVM.LoadNewWaferFromMES();
        //    WaferQUALVM.LoadWUXIWaferQUAL();
        //    return View("HeartBeat");
        //}

        public ActionResult LoadAllenData()
        {
            AllenEVALData.LoadAllenData();
            return View("HeartBeat");
        }

        public ActionResult AllenLogic()
        {
            //AllenWATLogic.PassFaile("184051-80E01", "Eval_50up_rp03");
            //var logicres1 = AllenWATLogic.PassFail("184051-80E01", "Eval_50up_rp03", true);
            //var logicres2 = AllenWATLogic.PassFail("184051-80E01", "Eval_50up_rp03");

            //var logicres1 = AllenWATLogic.PassFail("184637-30E01", "Eval_50up_rp04", true);
            //var logicres2 = AllenWATLogic.PassFail("184637-30E01", "Eval_50up_rp04");

            return View("HeartBeat");
        }

        public ActionResult MoveDB()
        {
            var cfg = CfgUtility.GetSysConfig(this);
            var FromConnect = cfg["FromConnect"];
            var ToConnect = cfg["ToConnect"];
            var MOVETABLELIST = cfg["MOVETABLELIST"].Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            foreach (var tab in MOVETABLELIST)
            {
                var ret = MoveDataBase.MoveDB(tab,FromConnect,ToConnect);
                if (ret)
                {
                    System.Windows.MessageBox.Show("Sucess to moved table: " + tab);
                }
                else
                {
                    System.Windows.MessageBox.Show("Fail to moved table: " + tab);
                }
            }

            return View("HeartBeat");
        }

        //public ActionResult PrepareFakeData()
        //{
        //    WXProbeData.PrepareProbeData("184637-30");
        //    WXOriginalWATData.PrepareFakeDataFromAllen("184637-30E01");

        //    return View("HeartBeat");
        //}

        //public ActionResult WXLogic()
        //{
        //    //WXProbeData.PrepareProbeData("184637-30");
        //    //WXOriginalWATData.PrepareDataFromAllen("184637-30E01");
        //    var wxlogic = new WXWATLogic();
        //    wxlogic.WATPassFail("184637-30E01", "POSTHTOL2JUDGEMENT");

        //    return View("HeartBeat");
        //}

        //public ActionResult ScanNewWafer() {
        //    DieSortVM.ScanNewWafer(this);
        //    return View("HeartBeat");
        //}

        public ActionResult VerifyAllenLogic()
        {
            AllenLogicVerify.Verify();
            return View("HeartBeat");
        }


        public ActionResult ScanNewVCSEL()
        {
            DieSortVM.ScanNewWafer(this);
            return View("HeartBeat");
        }

        //public ActionResult PrepareData4WAT()
        //{
        //    var wlist = new List<string>();
        //    wlist.Add("191106-70");
        //    wlist.Add("191531-10");
        //    wlist.Add("191007-70");

        //    //wlist.Add("190601-20");
        //    //wlist.Add("190628-30");
        //    //wlist.Add("190717-30");
        //    foreach (var w in wlist)
        //    {
        //        DieSortVM.PrepareData4WAT(w);
        //    }

        //    return View("HeartBeat");
        //}

        public ActionResult LoadOGPData()
        {
            var ogpdata = WXLogic.WATSampleXY.GetSampleXYByCouponGroup("172015-30E08");
            return View("HeartBeat");
        }

        public ActionResult GetInspectWafer()
        {
            var inspectwafer = DieSortVM.GetInspectedWaferInPast5Days();
            return View("HeartBeat");
        }

        public ActionResult RefreshWATStatusDaily()
        {
            WuxiWATData4MG.RefreshWATStatusDaily();
            return View("HeartBeat");
        }

        public ActionResult RefreshBinSubstituteFromAllen() {
            BinSubstitute.RefreshBinSubstituteFromAllen();
            return View("HeartBeat");
        }

        public ActionResult NeomapToAllen()
        { return View(); }

        public JsonResult NeomapToAllenData()
        {
            var marks = Request.Form["marks"];
            List<string> wflist = (List<string>)Newtonsoft.Json.JsonConvert.DeserializeObject(marks, (new List<string>()).GetType());
            var wfdatalist = new List<object>();
            foreach (var wf in wflist)
            {
                if (Models.WXProbeData.AllenHasData(wf))
                {
                    wfdatalist.Add(new {
                        wf = wf,
                        stat = "OK"
                    });
                }
                else
                {
                    if (Models.WXProbeData.AddProbeTrigge2Allen(wf))
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
                            stat = "NG"
                        });
                    }
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


        public ActionResult Prepare4WAT()
        { return View(); }

        public JsonResult Prepare4WATData()
        {
            var marks = Request.Form["marks"];
            List<string> wflist = (List<string>)Newtonsoft.Json.JsonConvert.DeserializeObject(marks, (new List<string>()).GetType());
            var wfdatalist = new List<object>();
            foreach (var wf in wflist)
            {
                if (DieSortVM.PrepareData4WAT(wf))
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
                        stat = "NG"
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

        public ActionResult ModifyMapFile()
        {
            DieSortVM.ModifyMapFileAsExpect(this);
            return View("HeartBeat");

        }


        //public ActionResult LoadOvenData()
        //{
        //    var syscfg = CfgUtility.GetSysConfig(this);
        //    var oventesters = syscfg["WATOVEN"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();

        //    var startdate = DateTime.Parse("2019-11-03 00:00:00");
        //    var enddate = DateTime.Parse("2019-11-19 00:00:00");

        //    while (startdate <= enddate)
        //    {
        //        foreach (var tester in oventesters)
        //        {
        //            WATOven.LoadOvenData(startdate.ToString("yyyy-MM-dd HH:mm:ss")
        //                , startdate.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss"), tester.ToUpper());
        //        }
        //        startdate = startdate.AddDays(1);
        //    }

        //    return View("HeartBeat");
        //}

        public ActionResult RefreshDailyOvenData()
        {
            WATOven.RefreshDailyOvenData(this);
            return View("HeartBeat");
        }

    }
}