using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAT.Models
{
    public class AllenLogicVerify
    {
        public static void Verify()
        {
            var filepath = "E:\\100waferlogicverify.xlsx";
            var rplist = new List<string>();
            rplist.Add("0");
            rplist.Add("1");
            rplist.Add("2");
            rplist.Add("3");

            var clist = new List<string>();
            clist.Add("E01");
            clist.Add("E06AA");
            clist.Add("E06AB");
            clist.Add("E08AA");
            clist.Add("E08AB");
            clist.Add("E10");

            var idx = 0;
            var data = ExcelReader.RetrieveDataFromExcel(filepath, "Sheet1", 4);
            foreach (var line in data)
            {
                if (idx == 0)
                {
                    idx += 1;
                    continue;
                }

                var wafer = line[0].Substring(0,9);
                var dcdname = line[1];
                foreach (var c in clist)
                {
                    foreach (var rp in rplist)
                    {
                        AllenWATLogic.PassFail(wafer+c, dcdname + rp,false,true);
                    }
                }
            }
        }


    }
}