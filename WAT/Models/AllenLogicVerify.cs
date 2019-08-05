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
            rplist.Add("3");

            var idx = 0;
            var data = ExcelReader.RetrieveDataFromExcel(filepath, null, 4);
            foreach (var line in data)
            {
                if (idx == 0)
                {
                    idx += 1;
                    continue;
                }

                var wafer = line[0];
                var dcdname = line[1];

                foreach (var rp in rplist)
                {
                    //AllenWATLogic.ForVerify(wafer, dcdname + rp);
                    AllenWATLogic.PassFail(wafer, dcdname + rp,false,true);
                }

            }
        }


    }
}