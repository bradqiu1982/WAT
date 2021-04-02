using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAT.Models
{
    public class WKSpan
    {

        public static List<DateTime> RetrieveDateSpanByWeek(string startdate, string enddate)
        {
            var ret = _RetrieveDateSpanByWeek(startdate, enddate);
            if (ret.Count > 2)
            {
                var end1 = ret[ret.Count - 2];
                var end2 = ret[ret.Count - 1];
                if (string.Compare(end1.ToString("yyyy-MM-dd"), end2.ToString("yyyy-MM-dd")) == 0)
                {
                    var lastidx = ret.Count - 1;
                    ret.RemoveAt(lastidx);
                }
            }
            return ret;
        }

        private static List<DateTime> _RetrieveDateSpanByWeek(string startdate, string enddate)
        {
            var ret = new List<DateTime>();
            var sdate = DateTime.Parse(DateTime.Parse(startdate).ToString("yyyy-MM-dd") + " 00:00:00");
            ret.Add(sdate);
            var edate = DateTime.Parse(enddate);
            var firstweekend = RetrieveFirstWeek(startdate);
            if (firstweekend > edate)
            {
                ret.Add(DateTime.Parse(edate.ToString("yyyy-MM-dd") + " 00:00:00"));
                return ret;
            }

            if (firstweekend != UT.O2T(sdate))
            { ret.Add(firstweekend); }

            var temptimepoint = firstweekend;
            while (temptimepoint < edate)
            {
                temptimepoint = temptimepoint.AddDays(7);
                if (temptimepoint > edate)
                {
                    if (DateTime.Parse(enddate).DayOfWeek == DayOfWeek.Monday)
                    {
                        ret.Add(DateTime.Parse(DateTime.Parse(enddate).ToString("yyyy-MM-dd") + " 00:00:00"));
                    }
                    else
                    {
                        ret.Add(DateTime.Parse(enddate));
                    }

                    return ret;
                }
                else
                {
                    ret.Add(DateTime.Parse(temptimepoint.ToString("yyyy-MM-dd") + " 00:00:00"));
                }
            }

            return ret;
        }

        private static DateTime RetrieveFirstWeek(string startdate)
        {
            var sdate = DateTime.Parse(DateTime.Parse(startdate).ToString("yyyy-MM-dd") + " 00:00:00");
            if (sdate.DayOfWeek < DayOfWeek.Monday)
            {
                sdate = sdate.AddDays(1 - (int)sdate.DayOfWeek);
                return DateTime.Parse(sdate.ToString("yyyy-MM-dd") + " 00:00:00");
            }
            else if (sdate.DayOfWeek == DayOfWeek.Monday)
            {
                return DateTime.Parse(sdate.ToString("yyyy-MM-dd") + " 00:00:00");
            }
            else
            {
                sdate = sdate.AddDays(8 - (int)sdate.DayOfWeek);
                return DateTime.Parse(sdate.ToString("yyyy-MM-dd") + " 00:00:00");
            }
        }
    }
}