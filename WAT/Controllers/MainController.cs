using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WAT.Models;

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
                heartbeatlog("start heartbeat");

                heartbeatlog("DieSortVM.LoadAllDieSortFile");
                try
                {
                    DieSortVM.LoadAllDieSortFile(this);
                }
                catch (Exception ex) { }

                heartbeatlog("WaferQUALVM.LoadWUXIWaferQUAL");
                try
                {
                    WaferQUALVM.LoadNewWaferFromMES();
                    WaferQUALVM.LoadWUXIWaferQUAL();
                }
                catch (Exception ex) { }


                heartbeatlog("AllenEVALData.LoadAllenData");
                try
                {
                    AllenEVALData.LoadAllenData();
                }
                catch (Exception ex) { }

                heartbeatlog("end heartbeat");
            }
            catch (Exception ex) { }

            return View();
        }

        public ActionResult LoadWaferQUAL()
        {
            WaferQUALVM.LoadNewWaferFromMES();
            WaferQUALVM.LoadWUXIWaferQUAL();
            return View("HeartBeat");
        }

        public ActionResult LoadAllenData()
        {
            AllenEVALData.LoadAllenData();
            return View("HeartBeat");
        }

        public ActionResult AllenLogic()
        {
            //AllenWATLogic.PassFaile("184051-80E01", "Eval_50up_rp03");
            AllenWATLogic.PassFaile("184637-30E01", "Eval_50up_rp04");
            return View("HeartBeat");
        }



    }
}