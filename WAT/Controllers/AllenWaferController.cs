﻿using System;
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
        ////http://wuxinpi.china.ads.finisar.com:9090/api/allenwafer?wafer=172015-30

        [HttpGet]
        public string GetWaferArray(string WAFER)
        {
            if (string.IsNullOrEmpty(WAFER) || string.IsNullOrEmpty(WAFER.Trim()))
            { return string.Empty; }

            return WATSampleXY.GetArrayFromAllen(WAFER.Trim());
        }

        //http://wuxinpi.china.ads.finisar.com:9090/api/allenwafer/GetWaferCoordinate?wafernum=191418-20
        [HttpGet]
        public string GetWaferCoordinate(string WAFERNUM)
        {
            var colist = DieSortVM.GetSampleXY(WAFERNUM);
            return Newtonsoft.Json.JsonConvert.SerializeObject(colist);
        }
    }
}
