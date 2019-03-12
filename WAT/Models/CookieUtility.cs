﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;

namespace WAT.Models
{
    public class CookieUtility
    {
        public static bool SetCookie(Controller ctrl, Dictionary<string, string> values)
        {
            try
            {
                HttpCookie ck = null;

                if (ctrl.Request.Cookies["WATsupport"] != null)
                {
                    ck = ctrl.Request.Cookies["WATsupport"];
                    foreach (var item in values)
                    {
                        ck.Values[item.Key] = Convert.ToBase64String(UTF8Encoding.UTF8.GetBytes(item.Value));
                    }

                    if (ctrl.Response.Cookies["WATsupport"] != null)
                    {
                        ctrl.Response.SetCookie(ck);
                    }
                    else
                    {
                        ctrl.Response.AppendCookie(ck);
                    }

                }
                else
                {
                    ck = new HttpCookie("WATsupport");
                    ck.Expires = DateTime.Now.AddDays(7);
                    foreach (var item in values)
                    {
                        ck.Values[item.Key] = Convert.ToBase64String(UTF8Encoding.UTF8.GetBytes(item.Value));
                    }

                    if (ctrl.Response.Cookies["WATsupport"] != null)
                    {
                        ctrl.Response.SetCookie(ck);
                    }
                    else
                    {
                        ctrl.Response.AppendCookie(ck);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static Dictionary<string, string> UnpackCookie(Controller ctrl)
        {
            return UnpackCookie(ctrl.Request);
        }

        public static Dictionary<string, string> UnpackCookie(HttpRequestBase req)
        {

            var ret = new Dictionary<string, string>();

            if (req.Cookies["WATsupport"] != null)
            {
                try
                {
                    var ck = req.Cookies["WATsupport"];
                    foreach (var key in ck.Values.AllKeys)
                    {
                        ret.Add(key, UTF8Encoding.UTF8.GetString(Convert.FromBase64String(ck.Values[key])));
                    }
                    return ret;
                }
                catch (Exception ex)
                {
                    ret.Clear();
                    return ret;
                }
            }
            else
            {
                ret.Clear();
                return ret;
            }
        }

        public static bool RemoveCookie(Controller ctrl)
        {
            ctrl.Response.Cookies["WATsupport"].Expires = DateTime.Now.AddDays(-1000);
            return true;
        }
    }
}