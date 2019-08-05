using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WAT.Models;

namespace WAT.Controllers
{
    public class AllenWaferController : ApiController
    {
        [HttpGet]
        public string GetWaferArray(string WAFER)
        {
            if (string.IsNullOrEmpty(WAFER) || string.IsNullOrEmpty(WAFER.Trim()))
            { return string.Empty; }

            return WATSampleXY.GetArrayFromAllen(WAFER.Trim());
        }
    }
}
