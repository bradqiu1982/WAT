﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.InteropServices;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml;

namespace WAT.Models
{
    public class ExternalDataCollector
    {
        #region FILEOPERATE
        public static string DownloadShareFile(string srcfile, Controller ctrl)
        {
            try
            {
                if (ExternalDataCollector.FileExist(ctrl, srcfile))
                {
                    var filename = System.IO.Path.GetFileName(srcfile);
                    var descfolder = ctrl.Server.MapPath("~/userfiles") + "\\docs\\ShareFile\\";
                    if (!ExternalDataCollector.DirectoryExists(ctrl, descfolder))
                        ExternalDataCollector.CreateDirectory(ctrl, descfolder);
                    var descfile = descfolder + System.IO.Path.GetFileNameWithoutExtension(srcfile) + System.IO.Path.GetExtension(srcfile);
                    ExternalDataCollector.FileCopy(ctrl, srcfile, descfile, true);
                    return descfile;
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private static List<List<string>> RetrieveDataFromExcelWithAuth(Controller ctrl, string filename, string sheetname = null, int columns = 101)
        {
            try
            {
                var syscfgdict = CfgUtility.GetSysConfig(ctrl);
                var folderuser = syscfgdict["SHAREFOLDERUSER"];
                var folderdomin = syscfgdict["SHAREFOLDERDOMIN"];
                var folderpwd = syscfgdict["SHAREFOLDERPWD"];

                using (NativeMethods cv = new NativeMethods(folderuser, folderdomin, folderpwd))
                {
                    return ExcelReader.RetrieveDataFromExcel(filename, sheetname, columns);
                }
            }
            catch (Exception ex)
            {
                return new List<List<string>>();
            }
        }

        private static List<List<string>> RetrieveDataFromExcelWithAuthByColmnName(Controller ctrl, string filename,List<string> colname, string sheetname = null, int columns = 101)
        {
            var ret = new List<List<string>>();
            try
            {
                var syscfgdict = CfgUtility.GetSysConfig(ctrl);
                var folderuser = syscfgdict["SHAREFOLDERUSER"];
                var folderdomin = syscfgdict["SHAREFOLDERDOMIN"];
                var folderpwd = syscfgdict["SHAREFOLDERPWD"];

                using (NativeMethods cv = new NativeMethods(folderuser, folderdomin, folderpwd))
                {
                    var rawdata = ExcelReader.RetrieveDataFromExcel(filename, sheetname, columns);
                    if (rawdata.Count > 0)
                    {
                        var coldict = new Dictionary<string, int>();
                        var colidx = 0;
                        foreach (var item in rawdata[0])
                        {
                            if (!string.IsNullOrEmpty(item) && !coldict.ContainsKey(item.ToUpper()))
                            { coldict.Add(item.ToUpper(), colidx); }
                            colidx = colidx + 1;
                        }

                        foreach (var name in colname)
                        {
                            if (!coldict.ContainsKey(name))
                            {
                                logthdinfo("FILE: " + filename + " does not contains column: " + name);
                                return new List<List<string>>();
                            }
                        }

                        var rowidx = 0;
                        foreach (var line in rawdata)
                        {
                            if (rowidx == 0)
                            {
                                rowidx = rowidx + 1;
                                continue;
                            }

                            var templine = new List<string>();
                            foreach (var name in colname)
                            {
                                templine.Add(line[coldict[name]]);
                            }
                            ret.Add(templine);
                        }

                        return ret;
                    }
                    else
                    {
                        return new List<List<string>>();
                    }
                }//end using
            }
            catch (Exception ex)
            {
                return new List<List<string>>();
            }
        }

        public static bool FileExist(Controller ctrl, string filename)
        {
            try
            {
                var syscfgdict = CfgUtility.GetSysConfig(ctrl);
                var folderuser = syscfgdict["SHAREFOLDERUSER"];
                var folderdomin = syscfgdict["SHAREFOLDERDOMIN"];
                var folderpwd = syscfgdict["SHAREFOLDERPWD"];

                using (NativeMethods cv = new NativeMethods(folderuser, folderdomin, folderpwd))
                {
                    return File.Exists(filename);
                }
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public static void FileDelete(Controller ctrl, string filename)
        {
            try
            {
                var syscfgdict = CfgUtility.GetSysConfig(ctrl);
                var folderuser = syscfgdict["SHAREFOLDERUSER"];
                var folderdomin = syscfgdict["SHAREFOLDERDOMIN"];
                var folderpwd = syscfgdict["SHAREFOLDERPWD"];

                using (NativeMethods cv = new NativeMethods(folderuser, folderdomin, folderpwd))
                {
                    File.Delete(filename);
                }
            }
            catch (Exception ex)
            {
            }

        }

        private static void FileCopy(Controller ctrl, string src, string des, bool overwrite, bool checklocal = false)
        {
            try
            {
                var syscfgdict = CfgUtility.GetSysConfig(ctrl);
                var folderuser = syscfgdict["SHAREFOLDERUSER"];
                var folderdomin = syscfgdict["SHAREFOLDERDOMIN"];
                var folderpwd = syscfgdict["SHAREFOLDERPWD"];

                using (NativeMethods cv = new NativeMethods(folderuser, folderdomin, folderpwd))
                {
                    if (checklocal)
                    {
                        if (File.Exists(des))
                        {
                            return;
                        }
                    }

                    File.Copy(src, des, overwrite);
                }
            }
            catch (Exception ex)
            {
            }
        }

        public static void CreateDirectory(Controller ctrl, string dirname)
        {
            try
            {
                var syscfgdict = CfgUtility.GetSysConfig(ctrl);
                var folderuser = syscfgdict["SHAREFOLDERUSER"];
                var folderdomin = syscfgdict["SHAREFOLDERDOMIN"];
                var folderpwd = syscfgdict["SHAREFOLDERPWD"];

                using (NativeMethods cv = new NativeMethods(folderuser, folderdomin, folderpwd))
                {
                    Directory.CreateDirectory(dirname);
                }
            }
            catch (Exception ex)
            { }

        }

        private static bool DirectoryExists(Controller ctrl, string dirname)
        {
            try
            {
                var syscfgdict = CfgUtility.GetSysConfig(ctrl);
                var folderuser = syscfgdict["SHAREFOLDERUSER"];
                var folderdomin = syscfgdict["SHAREFOLDERDOMIN"];
                var folderpwd = syscfgdict["SHAREFOLDERPWD"];

                using (NativeMethods cv = new NativeMethods(folderuser, folderdomin, folderpwd))
                {
                    return Directory.Exists(dirname);
                }
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public static List<string> DirectoryEnumerateFiles(Controller ctrl, string dirname)
        {
            try
            {
                var syscfgdict = CfgUtility.GetSysConfig(ctrl);
                var folderuser = syscfgdict["SHAREFOLDERUSER"];
                var folderdomin = syscfgdict["SHAREFOLDERDOMIN"];
                var folderpwd = syscfgdict["SHAREFOLDERPWD"];

                using (NativeMethods cv = new NativeMethods(folderuser, folderdomin, folderpwd))
                {
                    var ret = new List<string>();
                    ret.AddRange(Directory.EnumerateFiles(dirname));
                    return ret;
                }
            }
            catch (Exception ex)
            {
                return new List<string>();
            }
        }

        private static string ConvertToDateStr(string datestr)
        {
            if (string.IsNullOrEmpty(datestr))
            {
                return "1982-05-06 10:00:00";
            }
            try
            {
                return DateTime.Parse(datestr).ToString();
            }
            catch (Exception ex) { return "1982-05-06 10:00:00"; }
        }

        private static double ConvertToDoubleVal(string val)
        {
            if (string.IsNullOrEmpty(val))
                return -99999;
            try
            {
                return Convert.ToDouble(val);
            }
            catch (Exception ex)
            {
                return -99999;
            }
        }

        private static void logthdinfo(string info, string file_dir = "WAT")
        {
            try
            {
                var filename = "d:\\log\\" + file_dir + "-" + DateTime.Now.ToString("yyyy-MM-dd");
                if (File.Exists(filename))
                {
                    var content = System.IO.File.ReadAllText(filename);
                    content = content + "\r\n" + DateTime.Now.ToString() + " : " + info;
                    System.IO.File.WriteAllText(filename, content);
                }
                else
                {
                    System.IO.File.WriteAllText(filename, DateTime.Now.ToString() + " : " + info);
                }
            }
            catch (Exception ex)
            { }

        }

        #endregion
    }

    public class NativeMethods : IDisposable
    {

        // obtains user token  

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]

        static extern bool LogonUser(string pszUsername, string pszDomain, string pszPassword,

            int dwLogonType, int dwLogonProvider, ref IntPtr phToken);



        // closes open handes returned by LogonUser  

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]

        extern static bool CloseHandle(IntPtr handle);
        [DllImport("Advapi32.DLL")]
        static extern bool ImpersonateLoggedOnUser(IntPtr hToken);
        [DllImport("Advapi32.DLL")]
        static extern bool RevertToSelf();
        const int LOGON32_PROVIDER_DEFAULT = 0;
        const int LOGON32_LOGON_NEWCREDENTIALS = 2;

        private bool disposed;

        public NativeMethods(string sUsername, string sDomain, string sPassword)
        {

            // initialize tokens  

            IntPtr pExistingTokenHandle = new IntPtr(0);
            IntPtr pDuplicateTokenHandle = new IntPtr(0);
            try
            {
                // get handle to token  
                bool bImpersonated = LogonUser(sUsername, sDomain, sPassword,

                    LOGON32_LOGON_NEWCREDENTIALS, LOGON32_PROVIDER_DEFAULT, ref pExistingTokenHandle);
                if (true == bImpersonated)
                {

                    if (!ImpersonateLoggedOnUser(pExistingTokenHandle))
                    {
                        int nErrorCode = Marshal.GetLastWin32Error();
                        throw new Exception("ImpersonateLoggedOnUser error;Code=" + nErrorCode);
                    }
                }
                else
                {
                    int nErrorCode = Marshal.GetLastWin32Error();
                    throw new Exception("LogonUser error;Code=" + nErrorCode);
                }

            }

            finally
            {
                // close handle(s)  
                if (pExistingTokenHandle != IntPtr.Zero)
                    CloseHandle(pExistingTokenHandle);
                if (pDuplicateTokenHandle != IntPtr.Zero)
                    CloseHandle(pDuplicateTokenHandle);
            }

        }

        protected virtual void Dispose(bool disposing)
        {

            if (!disposed)
            {
                RevertToSelf();
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}