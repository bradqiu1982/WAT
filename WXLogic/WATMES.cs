using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WATMoveDLL;

namespace WXLogic
{
    class WATMES
    {
        public static void MESMove(List<string> snlist,string workflow,string moveto,string comment)
        {
            var refErrMsg = "";
            new Thread(() => {
                try
                {
                    var wmdll = new WATMove();
                    wmdll.MoveNonStd(snlist, workflow, moveto, comment, ref refErrMsg);
                }
                catch (Exception ex) { }
            }).Start();

        }
    }
}
