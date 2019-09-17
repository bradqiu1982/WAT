using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel;
using System.Windows.Documents;
using System.Diagnostics;


using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;

using CsvHelper;
using System.IO;


using NPOI.HSSF.UserModel;
using NPOI.HPSF;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.XSSF.Streaming;


namespace WAT.Models
{
    public class ExcelReader
    {
        private static Excel.Workbook OpenBook(Excel.Workbooks books, string fileName, bool readOnly, bool editable,
bool updateLinks)
        {

            Excel.Workbook book = books.Open(
                fileName, updateLinks, readOnly,
                Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                Type.Missing, editable, Type.Missing, Type.Missing, Type.Missing,
                Type.Missing, Type.Missing);
            return book;
        }

        private static void ReleaseRCM(object o)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(o);
            }
            catch
            {
            }
            finally
            {
                o = null;
            }
        }

        private static bool WholeLineEmpty(List<string> line)
        {
            bool ret = true;
            foreach (var item in line)
            {
                if (!string.IsNullOrEmpty(item.Trim()))
                {
                    ret = false;
                }
            }
            return ret;
        }

        private static void logthdinfo(string info)
        {
            var filename = "d:\\log\\excelexception-" + DateTime.Now.ToString("yyyy-MM-dd");
            if (System.IO.File.Exists(filename))
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


        public static List<List<string>> RetrieveDataFromExcel(string wholefn, string sheetname, int columns = 101)
        {
            var ext = Path.GetExtension(wholefn).ToUpper();
            if (ext.Contains("XLSX") || ext.Contains("XLSM"))
            {
                return RetrieveDataFromExcel_XLSX(wholefn, sheetname, columns);
            }
            else if (ext.Contains("XLS"))
            {
                var ret = RetrieveDataFromExcel_XLS(wholefn, sheetname, columns);
                if (ret.Count == 1
                    && ret[0].Count == 1
                    && ret[0][0].ToUpper().Contains("BIFF5"))
                {
                    return RetrieveDataFromExcel_OLD(wholefn, sheetname, columns);
                }
                return ret;
            }
            else if (ext.Contains("CSV"))
            {
                return RetrieveDataFromExcel_CSV(wholefn, sheetname, columns);
            }
            else
            {
                return RetrieveDataFromExcel_XLSX(wholefn, sheetname, columns);
            }
        }

        public static List<List<string>> RetrieveDataFromExcel_XLSX(string wholefn, string sheetname, int columns = 101)
        {
            var ret = new List<List<string>>();
            try
            {
                using (SpreadsheetDocument document =
                    SpreadsheetDocument.Open(wholefn, false))
                {
                    document.WorkbookPart.Workbook.CalculationProperties.ForceFullCalculation = true;
                    document.WorkbookPart.Workbook.CalculationProperties.FullCalculationOnLoad = true;

                    WorkbookPart wbPart = document.WorkbookPart;
                    Sheets theSheets = wbPart.Workbook.Sheets;
                    WorksheetPart wsPart = null;

                    if (!string.IsNullOrEmpty(sheetname))
                    {
                        foreach (Sheet item in theSheets)
                        {
                            if (string.Compare(item.Name, sheetname, true) == 0)
                            {
                                wsPart = (WorksheetPart)wbPart.GetPartById(item.Id);
                                break;
                            }
                        }
                    }
                    else
                    {
                        var idlist = new List<StringValue>();
                        foreach (Sheet item in theSheets)
                        {
                            idlist.Add(item.Id);
                        }
                        if (idlist.Count > 0)
                        {
                            idlist.Sort(delegate (StringValue obj1, StringValue obj2)
                            {
                                return obj1.Value.CompareTo(obj2.Value);
                            });
                            wsPart = (WorksheetPart)wbPart.GetPartById(idlist[0]);
                        }
                    }

                    if (wsPart == null)
                    { return ret; }

                    using (OpenXmlReader reader = OpenXmlReader.Create(wsPart))
                    {
                        while (reader.Read())
                        {
                            if (reader.ElementType == typeof(Row) && reader.IsStartElement)
                            {
                                Row row = (Row)reader.LoadCurrentElement();
                                var line = new List<string>();
                                var cells = row.Elements<Cell>();
                                foreach (Cell c in cells)
                                {
                                    line.Add(GetFormattedCellValue(wbPart, c));
                                    if (line.Count >= columns)
                                    { break; }
                                }
                                if (WholeLineEmpty(line)) { continue; }
                                ret.Add(line);
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                logthdinfo(DateTime.Now.ToString() + " Exception on " + wholefn + " :" + ex.Message + "\r\n\r\n");
            }
            return ret;
        }

        private static string GetFormattedCellValue(WorkbookPart workbookPart, Cell cell)
        {
            if (cell == null
                || cell.CellValue == null
                || string.IsNullOrEmpty(cell.CellValue.Text))
            {
                return "";
            }

            var numberingFormats = workbookPart.WorkbookStylesPart.Stylesheet.NumberingFormats;

            string value = "";
            try
            {
                if (cell.DataType == null) // number & dates
                {
                    int styleIndex = (int)cell.StyleIndex.Value;
                    CellFormat cellFormat = (CellFormat)workbookPart.WorkbookStylesPart.Stylesheet.CellFormats.ElementAt(styleIndex);
                    uint formatId = cellFormat.NumberFormatId.Value;

                    if (formatId >= 14 && formatId <= 22)
                    {
                        double oaDate;
                        if (double.TryParse(cell.CellValue.Text, out oaDate))
                        {
                            value = DateTime.FromOADate(oaDate).ToString("yyyy-MM-dd HH:mm:ss");
                        }
                    }
                    else if (formatId == 0)
                    {
                        value = cell.CellValue.Text;
                    }
                    else
                    {
                        NumberingFormat numberingFormat = null;
                        try
                        {
                            numberingFormat = numberingFormats.Cast<NumberingFormat>()
                               .SingleOrDefault(f => f.NumberFormatId.Value == formatId);
                        }
                        catch (Exception e) { }

                        if (numberingFormat != null && numberingFormat.FormatCode.Value.Contains("yy"))
                        {
                            double oaDate;
                            if (double.TryParse(cell.CellValue.Text, out oaDate))
                            {
                                value = DateTime.FromOADate(oaDate).ToString("yyyy-MM-dd HH:mm:ss");
                            }
                        }
                        else
                        {
                            value = cell.CellValue.Text;
                        }
                    }
                }
                else // Shared string or boolean
                {
                    switch (cell.DataType.Value)
                    {
                        case CellValues.SharedString:
                            SharedStringItem ssi = workbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ElementAt(int.Parse(cell.CellValue.Text));
                            value = ssi.Text.Text;
                            break;
                        case CellValues.Boolean:
                            value = cell.CellValue.Text == "0" ? "false" : "true";
                            break;
                        default:
                            value = cell.CellValue.Text;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                return "";
            }

            return value;
        }

        public static List<List<string>> RetrieveDataFromExcel_CSV(string wholefn, string sheetname, int columns = 101)
        {
            var ret = new List<List<string>>();
            try
            {
                using (var reader = new StreamReader(wholefn))
                using (var csv = new CsvReader(reader))
                {
                    while (csv.Read())
                    {
                        var line = new List<string>();
                        for (var idx = 0; idx < columns; idx++)
                        {
                            var val = "";
                            if (csv.TryGetField<string>(idx, out val))
                            {
                                line.Add(val);
                            }
                            else
                            {
                                break;
                            }
                        }//end for

                        if (WholeLineEmpty(line)) { continue; }
                        ret.Add(line);
                    }//end while
                }

            }
            catch (Exception ex)
            {
                logthdinfo(DateTime.Now.ToString() + " Exception on " + wholefn + " :" + ex.Message + "\r\n\r\n");
            }
            return ret;
        }

        private static string GetFormulaVal(IFormulaEvaluator formula, ICell c)
        {
            var ret = "";
            formula.EvaluateInCell(c);
            switch (c.CellType)
            {
                case NPOI.SS.UserModel.CellType.Numeric:
                    ret = c.NumericCellValue.ToString();
                    break;
                case NPOI.SS.UserModel.CellType.String:
                    ret = c.StringCellValue.ToString();
                    break;
            }
            return ret;
        }

        public static List<List<string>> RetrieveDataFromExcel_XLS(string wholefn, string sheetname, int columns = 101)
        {
            var ret = new List<List<string>>();
            HSSFWorkbook hssfwb = null;
            try
            {
                using (var fs = File.OpenRead(wholefn))
                {

                    hssfwb = new HSSFWorkbook(fs);
                    HSSFFormulaEvaluator formula = new HSSFFormulaEvaluator(hssfwb);

                    ISheet targetsheet = null;
                    if (!string.IsNullOrEmpty(sheetname))
                    {
                        targetsheet = hssfwb.GetSheet(sheetname);
                    }
                    else
                    {
                        targetsheet = hssfwb.GetSheetAt(0);
                    }

                    if (targetsheet == null)
                    { return ret; }

                    var rownum = targetsheet.LastRowNum;
                    for (var ridx = 0; ridx <= rownum; ridx++)
                    {
                        var row = targetsheet.GetRow(ridx);
                        var cells = row.Cells;
                        var line = new List<string>();
                        foreach (var c in cells)
                        {
                            if (c == null)
                            {
                                line.Add("");
                            }
                            else
                            {
                                switch (c.CellType)
                                {
                                    case NPOI.SS.UserModel.CellType.String:
                                        line.Add(c.StringCellValue);
                                        break;
                                    case NPOI.SS.UserModel.CellType.Numeric:
                                        if (DateUtil.IsCellDateFormatted(c))
                                        {
                                            line.Add(c.DateCellValue.ToString("yyyy-MM-dd HH:mm:ss"));
                                        }
                                        else
                                        {
                                            line.Add(c.NumericCellValue.ToString());
                                        }
                                        break;
                                    case NPOI.SS.UserModel.CellType.Boolean:
                                        line.Add(c.StringCellValue);
                                        break;
                                    case NPOI.SS.UserModel.CellType.Blank:
                                        line.Add("");
                                        break;
                                    case NPOI.SS.UserModel.CellType.Formula:
                                        line.Add(GetFormulaVal(formula, c));
                                        break;
                                    default:
                                        line.Add("");
                                        break;
                                }

                            }

                            if (line.Count > columns)
                            { break; }
                        }

                        if (WholeLineEmpty(line)) { continue; }

                        ret.Add(line);
                    }
                    hssfwb.Close();
                }

            }
            catch (Exception ex)
            {
                if (hssfwb != null)
                { hssfwb.Close(); }

                if (ex.Message.ToUpper().Contains("BIFF5"))
                {
                    ret.Clear();
                    var line = new List<string>();
                    line.Add(ex.Message);
                    ret.Add(line);
                }
                else
                {
                    logthdinfo(DateTime.Now.ToString() + " Exception on " + wholefn + " :" + ex.Message + "\r\n\r\n");
                }
            }

            return ret;
        }



        private static string[] columnFlag = {"A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P","Q","R","S","T","U","V","W","X","Y","Z",
              "AA","AB","AC","AD","AE","AF","AG","AH","AI","AJ","AK","AL","AM","AN","AO","AP","AQ","AR","AS","AT","AU","AV","AW","AX","AY","AZ",
              "BA","BB","BC","BD","BE","BF","BG","BH","BI","BJ","BK","BL","BM","BN","BO","BP","BQ","BR","BS","BT","BU","BV","BW","BX","BY","BZ",
              "CA","CB","CC","CD","CE","CF","CG","CH","CI","CJ","CK","CL","CM","CN","CO","CP","CQ","CR","CS","CT","CU","CV","CW","CX","CY","CZ",
              "DA","DB","DC","DD","DE","DF","DG","DH","DI","DJ","DK","DL","DM","DN","DO","DP","DQ","DR","DS","DT","DU","DV","DW","DX","DY","DZ",
              "EA","EB","EC","ED","EE","EF","EG","EH","EI","EJ","EK","EL","EM","EN","EO","EP","EQ","ER","ES","ET","EU","EV","EW","EX","EY","EZ",
              "FA","FB","FC","FD","FE","FF","FG","FH","FI","FJ","FK","FL","FM","FN","FO","FP","FQ","FR","FS","FT","FU","FV","FW","FX","FY","FZ",
              "GA","GB","GC","GD","GE","GF","GG","GH","GI","GJ","GK","GL","GM","GN","GO","GP","GQ","GR","GS","GT","GU","GV","GW","GX","GY","GZ",
              "HA","HB","HC","HD","HE","HF","HG","HH","HI","HJ","HK","HL","HM","HN","HO","HP","HQ","HR","HS","HT","HU","HV","HW","HX","HY","HZ",
              "IA","IB","IC","ID","IE","IF","IG","IH","II","IJ","IK","IL","IM","IN","IO","IP","IQ","IR","IS","IT","IU","IV","IW","IX","IY","IZ",
              "JA","JB","JC","JD","JE","JF","JG","JH","JI","JJ","JK","JL","JM","JN","JO","JP","JQ","JR","JS","JT","JU","JV","JW","JX","JY","JZ"};

        private static List<List<string>> RetrieveDataFromExcel2(Excel.Worksheet sheet, int columns = 99)
        {
            var ret = new List<List<string>>();
            var totalrow = 100000;
            int emptycount = 0;
            int alldatacount = 0;

            if (columns > 101)
            { columns = 101; }

            try
            {
                for (var rowidx = 1; rowidx < totalrow;)
                {
                    var range = sheet.get_Range(columnFlag[0] + rowidx.ToString(), columnFlag[columns] + (rowidx + 9999).ToString());
                    var saRet = (System.Object[,])range.get_Value(Type.Missing);

                    for (var rowcount = 1; rowcount <= 10000; rowcount++)
                    {
                        var newline = new List<string>();
                        for (var colidx = 1; colidx < columns; colidx++)
                        {
                            var val = saRet[rowcount, colidx];
                            if (val != null)
                            {
                                newline.Add(val.ToString().Replace("'", "").Replace("\"", "").Trim());
                            }
                            else
                            {
                                newline.Add("");
                            }
                        }

                        var empty = true;
                        foreach (var item in newline)
                        {
                            if (!string.IsNullOrEmpty(item.Trim()))
                            {
                                empty = false;
                            }
                        }

                        if (!empty)
                        {
                            emptycount = 0;
                            ret.Add(newline);
                            alldatacount += 1;
                        }
                        else
                        {
                            emptycount = emptycount + 1;
                            alldatacount += 1;
                        }


                        if (emptycount > 20 || alldatacount > 500000)
                        {
                            return ret;
                        }

                        if (alldatacount == totalrow - 10)
                        {
                            totalrow = totalrow + 100000;
                        }

                    }//end rowcount

                    rowidx = rowidx + 10000;

                }//end for rowidx
            }
            catch (Exception ex1){}
            return ret;
        }

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        public static List<List<string>> RetrieveDataFromExcel_OLD(string wholefn, string sheetname, int columns = 101)
        {
            var data = new List<List<string>>();

            Excel.Application excel = null;
            Excel.Workbook wkb = null;
            Excel.Workbooks books = null;
            Excel.Worksheet sheet = null;
            int hWnd = 0;
            uint processID = 0;

            try
            {
                excel = new Excel.Application();
                excel.DisplayAlerts = false;
                books = excel.Workbooks;
                wkb = OpenBook(books, wholefn, true, false, false);

                hWnd = excel.Application.Hwnd;
                GetWindowThreadProcessId((IntPtr)hWnd, out processID);

                if (string.IsNullOrEmpty(sheetname))
                {
                    sheet = wkb.Sheets[1] as Excel.Worksheet;
                }
                else
                {
                    sheet = wkb.Sheets[sheetname] as Excel.Worksheet;
                }

                var ret = RetrieveDataFromExcel2(sheet, columns);

                wkb.Close();
                excel.Quit();

                Marshal.ReleaseComObject(sheet);
                Marshal.ReleaseComObject(wkb);
                Marshal.ReleaseComObject(books);
                Marshal.ReleaseComObject(excel);

                if (wkb != null)
                    ReleaseRCM(wkb);
                if (excel != null)
                    ReleaseRCM(excel);

                sheet = null;
                wkb = null;
                books = null;
                excel = null;

                GC.Collect();
                GC.WaitForPendingFinalizers();

                try
                {
                    Process[] procs = Process.GetProcessesByName("EXCEL");
                    foreach (Process p in procs)
                    {
                        if (p.Id == processID)
                        {
                            p.Kill();
                        }
                        else
                        {
                            var btime = p.TotalProcessorTime;
                            new System.Threading.ManualResetEvent(false).WaitOne(200);
                            p.Refresh();
                            var etime = p.TotalProcessorTime;
                            if ((etime - btime).Ticks <= 10)
                            { p.Kill(); }
                        }
                    }
                }
                catch (Exception e) { }

                return ret;

            }
            catch (Exception ex)
            {
                logthdinfo(DateTime.Now.ToString() + " Exception on " + wholefn + " :" + ex.Message + "\r\n\r\n");

                if (sheet != null)
                    Marshal.ReleaseComObject(sheet);
                if (wkb != null)
                    Marshal.ReleaseComObject(wkb);
                if (books != null)
                    Marshal.ReleaseComObject(books);
                if (excel != null)
                    Marshal.ReleaseComObject(excel);

                if (wkb != null)
                    ReleaseRCM(wkb);
                if (excel != null)
                    ReleaseRCM(excel);

                sheet = null;
                wkb = null;
                books = null;
                excel = null;

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();

                try
                {
                    Process[] procs = Process.GetProcessesByName("EXCEL");
                    foreach (Process p in procs)
                    {
                        if (p.Id == processID)
                        {
                            p.Kill();
                        }
                        else
                        {
                            var btime = p.TotalProcessorTime;
                            new System.Threading.ManualResetEvent(false).WaitOne(200);
                            p.Refresh();
                            var etime = p.TotalProcessorTime;
                            if ((etime - btime).Ticks <= 10)
                            { p.Kill(); }
                        }
                    }
                }
                catch (Exception e) { }

                return data;
            }

        }

        public static List<List<string>> RetrieveDataFromExcel_bak(string wholefn, string sheetname)
        {
            var data = new List<List<string>>();
            Excel.Application excel = null;
            Excel.Workbook wkb = null;
            Excel.Workbooks books = null;
            Excel.Worksheet sheet = null;

            try
            {
                excel = new Excel.Application();
                excel.DisplayAlerts = false;
                books = excel.Workbooks;
                wkb = OpenBook(books, wholefn, true, false, false);

                if (string.IsNullOrEmpty(sheetname))
                {
                    sheet = wkb.Sheets[1] as Excel.Worksheet;
                }
                else
                {
                    sheet = wkb.Sheets[sheetname] as Excel.Worksheet;
                }


                var excelRange = sheet.UsedRange;
                object[,] valueArray = (object[,])excelRange.get_Value(
                Excel.XlRangeValueDataType.xlRangeValueDefault);
                var totalrows = sheet.UsedRange.Rows.Count;
                var totalcols = sheet.UsedRange.Columns.Count;

                for (int row = 1; row <= totalrows; ++row)
                {
                    var line = new List<string>();
                    for (int col = 1; col <= totalcols; ++col)
                    {
                        if (valueArray[row, col] == null)
                        {
                            line.Add(string.Empty);
                        }
                        else
                        {
                            line.Add(valueArray[row, col].ToString().Trim().Replace("'", "").Replace("\"", ""));
                        }
                    }
                    //if (!WholeLineEmpty(line))
                    //{
                    data.Add(line);
                    //}
                }


                wkb.Close();
                excel.Quit();

                Marshal.ReleaseComObject(sheet);
                Marshal.ReleaseComObject(wkb);
                Marshal.ReleaseComObject(books);
                Marshal.ReleaseComObject(excel);

                if (wkb != null)
                    ReleaseRCM(wkb);
                if (excel != null)
                    ReleaseRCM(excel);
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();

                return data;
            }
            catch (Exception ex)
            {
                logthdinfo(DateTime.Now.ToString() + " Exception on " + wholefn + " :" + ex.Message + "\r\n\r\n");

                if (ex.Message.Contains("DISP_E_OVERFLOW") && sheet != null)
                {
                    var ret = RetrieveDataFromExcel2(sheet);

                    if (sheet != null)
                        Marshal.ReleaseComObject(sheet);
                    if (wkb != null)
                        Marshal.ReleaseComObject(wkb);
                    if (books != null)
                        Marshal.ReleaseComObject(books);
                    if (excel != null)
                        Marshal.ReleaseComObject(excel);

                    if (wkb != null)
                        ReleaseRCM(wkb);
                    if (excel != null)
                        ReleaseRCM(excel);
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    return ret;
                }
                else
                {
                    data.Clear();

                    if (sheet != null)
                        Marshal.ReleaseComObject(sheet);
                    if (wkb != null)
                        Marshal.ReleaseComObject(wkb);
                    if (books != null)
                        Marshal.ReleaseComObject(books);
                    if (excel != null)
                        Marshal.ReleaseComObject(excel);

                    if (wkb != null)
                        ReleaseRCM(wkb);
                    if (excel != null)
                        ReleaseRCM(excel);
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    return data;
                }
            }

        }
    }
}