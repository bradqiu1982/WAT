using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAT.Models
{

    public class CBOXData
    {
        public static List<object> CBOXFromRaw(List<double> listdata, double llimit, double hlimit)
        {
            var ret = new List<object>();

            var cbox = GetBoxData(listdata, llimit, hlimit);

            var outlierlist = new List<VXVal>();
            var rad = new System.Random(DateTime.Now.Second);
            var idx = 0;

            var allout = false;
            if (listdata[0] > hlimit || listdata[listdata.Count - 1] < llimit)
            { allout = true; }

            foreach (var data in listdata)
            {
                if ((data < llimit )
                    || (data > hlimit) || allout)
                {
                    var tempdata = new VXVal();
                    tempdata.ival = data;
                    if (idx % 2 == 0)
                    { tempdata.x = rad.NextDouble() / 5.0; }
                    else
                    { tempdata.x = 0 - rad.NextDouble() / 5.0; }
                    outlierlist.Add(tempdata);
                    idx = idx + 1;
                }
            }

            ret.Add(cbox);
            ret.Add(outlierlist);
            return ret;
        }

        private static double GetBoxMeanValue(List<double> rawdata)
        {
            if ((rawdata.Count % 2) == 0)
            {
                var mididx1 = rawdata.Count / 2;
                var mididx2 = mididx1 + 1;
                return (rawdata[mididx1] + rawdata[mididx2]) / 2.0;
            }
            else
            {
                var mididx = (rawdata.Count + 1) / 2;
                return rawdata[mididx];
            }
        }

        private static CBOXData GetBoxData(List<double> rawdata, double llimit, double hlimit)
        {
            var mean = 0.0;
            var min = 0.0;
            var max = 0.0;
            var lower = 0.0;
            var upper = 0.0;
            if ((rawdata.Count % 2) == 0)
            {
                var mididx1 = rawdata.Count / 2;
                var mididx2 = mididx1 + 1;

                mean = (rawdata[mididx1] + rawdata[mididx2]) / 2.0;
                //min = rawdata[0];
                //max = rawdata[rawdata.Count - 1];

                var lowlist = new List<double>();
                var upperlist = new List<double>();
                for (var idx = 0; idx < mididx2; idx++)
                {
                    lowlist.Add(rawdata[idx]);
                }
                for (var idx = mididx2; idx < rawdata.Count; idx++)
                {
                    upperlist.Add(rawdata[idx]);
                }

                lower = GetBoxMeanValue(lowlist);
                upper = GetBoxMeanValue(upperlist);


            }
            else
            {
                var mididx = (rawdata.Count + 1) / 2;
                mean = rawdata[mididx];
                //min = rawdata[0];
                //max = rawdata[rawdata.Count - 1];

                var lowlist = new List<double>();
                var upperlist = new List<double>();
                for (var idx = 0; idx < mididx; idx++)
                {
                    lowlist.Add(rawdata[idx]);
                }
                for (var idx = mididx + 1; idx < rawdata.Count; idx++)
                {
                    upperlist.Add(rawdata[idx]);
                }

                lower = GetBoxMeanValue(lowlist);
                upper = GetBoxMeanValue(upperlist);
            }

            var cbox = new CBOXData();
            cbox.mean = mean;
            cbox.lower = lower;
            cbox.upper = upper;

            max = upper + (upper - lower) * 1.5;
            min = lower - (upper - lower) * 1.5;

            if (min < rawdata[0])
            { min = rawdata[0]; }

            if (max > rawdata[rawdata.Count - 1])
            { max = rawdata[rawdata.Count - 1]; }

            //if (upper > hlimit && mean < hlimit && mean > llimit)
            //{
            //    upper = hlimit;
            //    cbox.upper = upper;
            //}

            //if (lower < llimit && mean < hlimit && mean > llimit)
            //{
            //    lower = llimit;
            //    cbox.lower = lower;
            //}

            //if (max > hlimit && upper <= hlimit)
            //{ max = hlimit; }

            //if (min < llimit && lower >= llimit)
            //{ min = llimit; }

            cbox.max = max;
            cbox.min = min;

            return cbox;
        }

        public CBOXData()
        {
            mean = 0;
            min = 0;
            max = 0;
            lower = 0;
            upper = 0;
        }
        public double mean { set; get; }
        public double min { set; get; }
        public double max { set; get; }
        public double lower { set; get; }
        public double upper { set; get; }
    }

    public class VXVal
    {
        public VXVal()
        {
            x = 0.0;
            ival = 0.0;
        }

        public double x { set; get; }
        public double ival { set; get; }
    }
}