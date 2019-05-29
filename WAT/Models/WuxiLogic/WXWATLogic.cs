using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAT.Models
{
    public class WXWATLogic
    {
        public static WXWATLogic WATPassFail(string CouponID,string CurrentStepName)
        {
            return new WXWATLogic();
        }

        public static void Usage()
        {
            var wuxlogic = new WXWATLogic();
            if (!string.IsNullOrEmpty(wuxlogic.AppErrorMsg))
            {
                //hold
                //reason: wuxlogic.AppErrorMsg
            }
            else
            {
                if (wuxlogic.TestPass)
                {
                    //move next
                }
                else
                {
                    if (wuxlogic.ScrapIt)
                    {
                        //hold
                        //reason: "Scrap:" + wuxlogic.ResultReason
                    }
                    else
                    {
                        if (wuxlogic.NeedRetest)
                        {
                            //hold
                            //reason: wuxlogic.ResultReason
                        }
                        else
                        {
                            //hold
                            //reason: wuxlogic.ResultReason,wait for PE to judgement
                        }
                    }//end else
                }//end else
            }//end else
        }

        public WXWATLogic()
        {
            TestPass = false;
            NeedRetest = false;
            ScrapIt = false;
            ResultReason = "";
            AppErrorMsg = "";
        }

        public bool TestPass { set; get; } //test pass
        public bool NeedRetest { set; get; } //whether need to retest
        public bool ScrapIt { set; get; } //whether scrap
        public string ResultReason { set; get; } //retest/scrap reason
        public string AppErrorMsg { set; get; } //for app logic error
    }
}