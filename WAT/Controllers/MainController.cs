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

        public ActionResult HeartBeat()
        {
            try
            {
                DieSortVM.LoadAllDieSortFile(this);
            }
            catch (Exception ex) { }

            return View();
        }
    }
}