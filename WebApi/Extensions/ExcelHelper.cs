using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using MiniExcelLibs;
using MiniExcelLibs.OpenXml;
using NPOI.SS.UserModel;
using NPOI.XSSF.Streaming;
using NPOI.XSSF.UserModel;

namespace NetCoreTemp.WebApi.Extensions
{
    public class ExcelHelper
    {
        /// <summary>
        /// 暂存数据队列
        /// </summary>
        private ConcurrentQueue<DataRow> _arrRows = new ConcurrentQueue<DataRow>();

        /// <summary>
        /// 表名
        /// </summary>
        string _tableName = "";

        public ExcelHelper()
        {
        }

        /// <summary>
        /// DataTable转excel
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] DataTableToExcelBySXSSF(string fileName, DataTable data)
        {
            //刷新到磁盘时的最大数据数
            var dumpSize = 1000;
            var workbook = new SXSSFWorkbook(dumpSize);
            ////压缩xml临时文件(POI3.8 才有)
            //workbook.setCompressTempFiles(true);
            //创建sheet
            var sheet = (SXSSFSheet)workbook.CreateSheet(fileName);
            sheet.DefaultColumnWidth = 20;
            sheet.ForceFormulaRecalculation = true;

            //设置表头
            var rowTitle = sheet.CreateRow(0);
            for (int k = 0; k < data.Columns.Count; k++)
            {
                var ctRow = rowTitle.CreateCell(k + 1);
                ctRow.SetCellValue(data.Columns[k].ColumnName);
            }

            //设置表内容        
            for (int i = 1; i <= data.Rows.Count; i++)
            {
                var row = sheet.CreateRow(i);
                for (int j = 1; j <= data.Columns.Count; j++)
                {
                    var cell = row.CreateCell(j);
                    cell.SetCellValue(data.Rows[i - 1][j - 1].ToString());
                    //cell.CellStyle = headStyle;
                }
                //数据刷到磁盘
                if (i % dumpSize == 0)
                    sheet.FlushRows();
            }
            //获取字节序列
            using (MemoryStream ms = new MemoryStream())
            {
                workbook.Write(ms);
                workbook.Dispose();
                workbook = null;
                return ms.ToArray();
            }
        }

        /// <summary>
        /// DataTable转excel
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="sheetName">sheet名称</param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static void DataTable2ExcelBySXSSF(FileStream fileStream, string sheetName, DataTable data)
        {
            //刷新到磁盘时的最大数据数
            var dumpSize = 1000;
            var workbook = new SXSSFWorkbook(dumpSize);
            //压缩xml临时文件
            workbook.CompressTempFiles = true;
            //创建sheet
            var sheet = (SXSSFSheet)workbook.CreateSheet(sheetName);
            sheet.DefaultColumnWidth = 20;
            sheet.ForceFormulaRecalculation = true;

            //设置表头
            var rowTitle = sheet.CreateRow(0);
            for (int k = 0; k < data.Columns.Count; k++)
            {
                var ctRow = rowTitle.CreateCell(k + 1);
                ctRow.SetCellValue(data.Columns[k].ColumnName);
            }

            //设置表内容        
            for (int i = 1; i <= data.Rows.Count; i++)
            {
                var row = sheet.CreateRow(i);
                for (int j = 1; j <= data.Columns.Count; j++)
                {
                    var cell = row.CreateCell(j);
                    cell.SetCellValue(data.Rows[i - 1][j - 1].ToString());
                    //cell.CellStyle = headStyle;
                }
                //数据刷到磁盘
                if (i % dumpSize == 0)
                    sheet.FlushRows();
            }
            workbook.Write(fileStream);
            workbook.Dispose();
        }

        /// <summary>
        /// DataTable写入文件到ISheet
        /// </summary>
        /// <param name="sheet">Excel Sheet</param>
        /// <param name="data">DataTable 数据源</param>
        /// <param name="rowNum">开始写入数据行号</param>
        public static void WriteData2SXSSFSheet(ISheet sheet, DataTable data, int rowNum = 0)
        {
            //刷新到磁盘时的最大数据数
            var dumpSize = 1000;

            sheet.DefaultColumnWidth = 20;
            sheet.ForceFormulaRecalculation = true;

            if (rowNum == 0)
            {
                //设置表头
                var rowTitle = sheet.CreateRow(0);
                for (int k = 0; k < data.Columns.Count; k++)
                {
                    var ctRow = rowTitle.CreateCell(k + 1);
                    ctRow.SetCellValue(data.Columns[k].ColumnName);
                }
                rowNum++;
            }

            //设置表内容        
            for (int i = rowNum; i <= data.Rows.Count; i++)
            {
                var row = sheet.CreateRow(i);
                for (int j = 1; j <= data.Columns.Count; j++)
                {
                    var cell = row.CreateCell(j);
                    cell.SetCellValue(data.Rows[i - 1][j - 1].ToString());
                }
                //数据刷到磁盘
                if (i % dumpSize == 0)
                    ((SXSSFSheet)sheet).FlushRows();
            }
        }

        /// <summary>
        /// 执行（并行读写方式）
        /// </summary>
        /// <param name="_tempDir">文档写入路径</param>
        /// <param name="_page">第几页（获取数据Task暂未实现）</param>
        /// <param name="write2OneCsv">写入一个文档</param>
        /// <param name="encoding">编码格式</param>
        /// <returns></returns>
        public async Task<string> WriteDataTable2CSV(string _tempDir, int _page, bool write2OneCsv = false, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.GetEncoding("UTF-8");
            var source = new CancellationTokenSource();
            var token = source.Token;

            string filepath = Path.Combine(_tempDir, $"{_tableName}-{(write2OneCsv ? "" : $"-{_page}")}-{DateTime.Now.to_Long()}.csv");
            var taskMain = Task.Run(async () =>
            {
                var conf = new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture);
                conf.Encoding = encoding;

                var fileStream = new FileStream(filepath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 102400);
                var streamWriter = new StreamWriter(fileStream, conf.Encoding);
                var writer = new CsvWriter(streamWriter, conf);
                //计数器
                var rowNum = 0;

                while (true)
                {
                    if (rowNum == 500000 && !write2OneCsv)
                    {
                        #region flush到文件

                        await writer.DisposeAsync();
                        await streamWriter.DisposeAsync();
                        await fileStream.DisposeAsync();

                        #endregion

                        #region 创建新的文件

                        _page++;
                        filepath = Path.Combine(_tempDir, $"{_tableName}-{_page}-{DateTime.Now.to_Long()}.csv");
                        fileStream = new FileStream(filepath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 102400);
                        streamWriter = new StreamWriter(fileStream, conf.Encoding);
                        writer = new CsvWriter(streamWriter, conf);

                        #endregion
                        rowNum = 0;
                    }
                    if (token.IsCancellationRequested && !_arrRows.Any())
                    {
                        await writer.DisposeAsync();
                        await streamWriter.DisposeAsync();
                        await fileStream.DisposeAsync();

                        return;
                    }
                    DataRow dr = null;
                    while (_arrRows.TryDequeue(out dr))
                    {
                        if (rowNum == 0)
                        {
                            for (int k = 0; k < dr.Table.Columns.Count; k++)
                            {
                                writer.WriteField(dr.Table.Columns[k].ColumnName);
                            }
                            writer.NextRecord();
                        }
                        for (int j = 1; j <= dr.Table.Columns.Count; j++)
                        {
                            var val = dr[j - 1];
                            if (!(val is string))
                                writer.WriteField(string.IsNullOrEmpty(val.ToString()) ? "NA" : val.ToString());
                            else
                                writer.WriteField(val);
                        }
                        writer.NextRecord();
                        rowNum++;
                        if (rowNum % 100000 == 0)
                            await writer.FlushAsync();
                        if (rowNum == 500000)
                        {
                            break;
                        }
                    }
                    await Task.Delay(10);
                }
            });
            var ArrTask = new List<Task>();

            //读取数据
            var tsk = Task.Run(async () =>
            {
                var batchCount = 100000;
                var skip = 0;
                Task taskEnqueue = null;
                while (true)
                {
                    DataTable _dt = new DataTable();
                    //var sql = $"{SQLStr} limit {batchCount} offset {skip}";
                    //_dt = await _sugarClient.Ado.GetDataTableAsync(sql);

                    if (_dt != null && _dt.Rows.Count > 0)
                    {
                        skip += _dt.Rows.Count;
                        if (taskEnqueue != null && taskEnqueue.Status == TaskStatus.Running)
                            await taskEnqueue;
                        taskEnqueue = Task.Run(() =>
                        {
                            for (var x = 0; x < _dt.Rows.Count; x++)
                            {
                                DataRow row = _dt.Rows[x];
                                _arrRows.Enqueue(row);
                                row = null;
                            }
                        });
                        if (_dt.Rows.Count < batchCount)
                            break;
                    }
                    else
                    {
                        break;
                    }
                }
            });

            await tsk;
            //结束
            source.Cancel();
            await taskMain;
            return filepath;
        }

        /// <summary>
        /// MiniExcel
        /// https://toscode.gitee.com/qww.haluan.com/MiniExcel
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> ReadStreamByMiniExcel<T>(Stream stream) where T : class, new()
        {
            var config = new OpenXmlConfiguration()
            {
                //TableStyles = MiniExcelLibs.OpenXml.TableStyles.None,
                FillMergedCells = true
            };
            //var sheetNames = MiniExcel.GetSheetNames(stream);
            //var columns = MiniExcel.GetColumns(stream, useHeaderRow: true);
            var rows = await stream.QueryAsync<T>(excelType: ExcelType.XLSX);
            //var enu = rows.GetEnumerator();
            //while (enu.MoveNext())
            //{

            //}
            return rows;
        }
    }

    /// <summary>
    /// Excel与类mapping
    /// </summary>
    public class ImportMapping
    {
        [Key]
        [Display(Name = "Id", Description = "")]
        public long Id { get; set; }

        [Display(Name = "导入类型", Description = "_type.FullName")]
        [Required, MaxLength(100)]
        public string ImportType { get; set; }

        [Display(Name = "展示字段名", Description = " propertyinfo.DisplayFieldName")]
        [Required, MaxLength(50)]
        public string DisplayFieldName { get; set; }

        [Display(Name = "字段名", Description = " propertyinfo.Name")]
        [Required, MaxLength(50)]
        public string FieldName { get; set; }

        [Display(Name = "字段类型", Description = "propertyinfo.PropertyType")]
        [Required, MaxLength(50)]
        public string FieldType { get; set; }

        [Display(Name = "序列", Description = "")]
        public int Orders { get; set; }

        [Display(Name = "Excel字段名", Description = "")]
        [MaxLength(50)]
        public string MappingFieldName { get; set; }

        [Display(Name = "是否为泛型", Description = "")]
        public bool isGenerictype { get; set; }

    }

    /// <summary>
    /// 使用OpenXML-SAX方式读取Excel，内存占用小
    /// 读取百兆Excel文件，不会内存溢出
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ReadExcelByOpenXml<T> where T : class, new()
    {
        Type _type { get; set; }
        Type nulltype = typeof(Nullable<>);
        PropertyInfo[] ArrProp = new PropertyInfo[] { };
        PropertyInfo[] Props_new = new PropertyInfo[] { };
        List<ImportMapping> _arrColumnMapping = null;
        public bool _firstIsColumnName { get; set; } = true;

        private MemoryCache _cache = MemoryCache.Default;

        public ReadExcelByOpenXml()
        {
            _type = typeof(T);
            Object _arrProp = _cache.Get(_type.FullName);
            if (_arrProp == null)
            {
                _arrProp = _type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                _cache.Set(_type.FullName, _arrProp, new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddHours(8) });
            }
            ArrProp = (PropertyInfo[])_arrProp;
        }

        #region 构造函数

        public ReadExcelByOpenXml(Dictionary<string, string> arrColumnMapping, bool firstIsColumnName = true) : this(firstIsColumnName, arrColumnMapping, null)
        {
        }

        public ReadExcelByOpenXml(IEnumerable<ImportMapping> arrColumnMapping, bool firstIsColumnName = true) : this(firstIsColumnName, null, arrColumnMapping)
        {
        }

        private ReadExcelByOpenXml(bool firstIsColumnName, Dictionary<string, string> arrDictColumnMapping, IEnumerable<ImportMapping> arrColumnMapping) : this()
        {
            _firstIsColumnName = firstIsColumnName;
            if (arrDictColumnMapping?.Any() == true)
            {
                _arrColumnMapping = arrDictColumnMapping?.Select((x, i) => new ImportMapping
                {
                    Orders = i,
                    FieldName = x.Value,
                    MappingFieldName = x.Key
                }).ToList();
            }
            else
            {
                _arrColumnMapping = arrColumnMapping.ToList();
            }
        }

        #endregion

        public IEnumerable<T> ReadData(Stream stream)
        {
            using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(stream, false))
            {
                return ReadAllCellValues(spreadsheetDocument);
            }
        }

        //这种SAX的读取方式很高效,至于是读一行提交一行好还是读100行再提交100行好自己决定.
        //这种SAX的方式对读取超大xlsx文件不存在内存占用过大和慢的问题.
        public IEnumerable<T> ReadData(string fileName)
        {
            using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(fileName, false))
            {
                return ReadAllCellValues(spreadsheetDocument);
            }
        }

        private IEnumerable<T> ReadAllCellValues(SpreadsheetDocument spreadsheetDocument)
        {
            var ArrResult = new List<T>();
            WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;
            var Tobj = default(T);
            //sheet&row 序号
            int _sheetNum = 0, rowNum = 0, countNum = 0;
            Props_new = new PropertyInfo[ArrProp.Length];
            foreach (WorksheetPart worksheetPart in workbookPart.WorksheetParts)
            {
                _sheetNum++;
                OpenXmlReader reader = OpenXmlReader.Create(worksheetPart);
                rowNum = 0;//行计数器初始化
                while (reader.Read())
                {
                    //if (reader.ElementType == typeof(Worksheet))
                    //{
                    //    if (reader.ReadFirstChild())
                    //    {
                    //        SheetProperties properties = (SheetProperties)reader.LoadCurrentElement();
                    //    }
                    //}
                    if (reader.ElementType == typeof(Row))
                    {
                        if (rowNum == 0)
                        {
                            #region 读取列名与T字段顺序对应

                            reader.ReadFirstChild();
                            var i = 0;
                            do
                            {
                                if (reader.ElementType == typeof(Cell))
                                {
                                    string columnName = "";
                                    //首行是列名
                                    if (_firstIsColumnName)
                                    {
                                        Cell c = (Cell)reader.LoadCurrentElement();


                                        if (c.DataType != null && c.DataType == CellValues.SharedString)
                                        {
                                            SharedStringItem ssi = workbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ElementAt(int.Parse(c.CellValue.InnerText));

                                            columnName = ssi.Text.Text;
                                        }
                                        else
                                        {
                                            columnName = c.CellValue?.InnerText;
                                        }
                                    }
                                    else
                                    {
                                        if (_arrColumnMapping == null || _arrColumnMapping?.Any() == false)
                                            columnName = $"_{i.ToString("000")}";
                                        else
                                        {
                                            //首行不是列名，按照顺序 赋值
                                            if (i < _arrColumnMapping?.Count())
                                            {
                                                //Dictionary
                                                columnName = _arrColumnMapping.ElementAt(i).MappingFieldName;
                                            }
                                        }
                                    }
                                    if (!string.IsNullOrEmpty(columnName))
                                    {
                                        //判断首行不是列名且_arrColumnMapping是空时的列顺序
                                        int columnOrder = -1;
                                        if (columnName.IndexOf('_') == 0)
                                        {
                                            var newcolumnName = columnName.Substring(1);
                                            if (!int.TryParse(newcolumnName, out columnOrder))
                                                columnOrder = -1;
                                        }
                                        if (columnOrder >= 0)
                                        {
                                            if (columnOrder < ArrProp.Length)
                                                Props_new[i] = ArrProp[columnOrder + 1];
                                        }
                                        else
                                        {
                                            var ColumnMapping = _arrColumnMapping?.Where(x => x.MappingFieldName == columnName).FirstOrDefault();
                                            if (ColumnMapping != null && !string.IsNullOrEmpty(ColumnMapping.FieldName))
                                            {
                                                columnName = ColumnMapping.FieldName;
                                            }
                                            var prop = ArrProp.FirstOrDefault(x => columnName.ToLower() == x.Name.ToLower());
                                            Props_new[i] = prop;
                                        }
                                    }
                                    i++;
                                }
                            } while (reader.ReadNextSibling());

                            #endregion
                        }
                        if (rowNum == 0 && _firstIsColumnName)
                        {
                            rowNum++;
                            countNum++;
                            continue;
                        }
                        Tobj = new T();
                        if (SetRowData(reader, workbookPart, Tobj))
                        {
                            ArrResult.Add(Tobj);
                        }
                        rowNum++;
                        countNum++;
                    }
                }
            }
            return ArrResult;
        }

        Boolean SetRowData(OpenXmlReader reader, WorkbookPart workbookPart, T Tobj)
        {
            reader.ReadFirstChild();
            var isAllNull = true;
            int j = 0;
            do
            {
                if (reader.ElementType == typeof(Cell))
                {
                    var prop = Props_new[j];
                    var propName = prop?.Name;
                    Cell cell = (Cell)reader.LoadCurrentElement();

                    if (cell == null)
                    {
                        if (!string.IsNullOrEmpty(propName))
                            prop.SetValue(Tobj, null);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(propName)) continue;
                        string cellVal;

                        if (cell.DataType != null && cell.DataType == CellValues.SharedString)
                        {
                            SharedStringItem ssi = workbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ElementAt(int.Parse(cell.CellValue.InnerText));

                            cellVal = ssi.Text.Text;
                        }
                        else
                        {
                            cellVal = cell.CellValue?.InnerText;
                        }

                        var ptype = prop.PropertyType;
                        //获取int?,decimal?
                        if (ptype.IsGenericType)
                        {
                            var TGenericTypeDefinition = ptype.GetGenericTypeDefinition();
                            if (TGenericTypeDefinition == nulltype)
                            {
                                var argsType = ptype.GetGenericArguments();
                                if (argsType.Any())
                                    ptype = argsType.FirstOrDefault();
                            }
                        }
                        object val = null;
                        if (!string.IsNullOrEmpty(cellVal))
                            isAllNull = false;
                        if (ptype == typeof(int))
                        {
                            if (int.TryParse(cellVal, out int intval))
                                val = intval;
                        }
                        else if (ptype == typeof(long))
                        {
                            if (long.TryParse(cellVal, out long longval))
                                val = longval;
                        }
                        else if (ptype == typeof(decimal))
                        {
                            cellVal = cellVal.Replace("%", "");
                            if (decimal.TryParse(cellVal, out decimal decimalval))
                                val = decimalval;
                        }
                        else if (ptype == typeof(float))
                        {
                            cellVal = cellVal.Replace("%", "");
                            if (float.TryParse(cellVal, out float floatval))
                                val = floatval;
                        }
                        else if (ptype == typeof(double))
                        {
                            cellVal = cellVal.Replace("%", "");
                            if (double.TryParse(cellVal, out double doubleval))
                                val = doubleval;
                        }
                        else if (ptype == typeof(bool))
                        {
                            var strVal = cellVal.Trim().ToUpper();
                            if (int.TryParse(strVal, out int intboolval))
                            {
                                if (intboolval <= 0)
                                    strVal = "False";
                                else
                                    strVal = "True";
                            }
                            else
                            {
                                if (propName.ToLower().IndexOf("sex") >= 0)
                                {
                                    if (strVal == "F" || strVal == "FEMALE")
                                        strVal = "False";
                                    if (strVal == "M" || strVal == "MALE")
                                        strVal = "True";
                                    else
                                        strVal = "False";
                                }
                                else
                                {
                                    if (strVal == "N" || strVal == "NO")
                                        strVal = "False";
                                    if (strVal == "Y" || strVal == "YES")
                                        strVal = "True";
                                    else
                                        strVal = "False";
                                }
                            }
                            if (bool.TryParse(strVal, out bool boolval))
                                val = boolval;
                        }
                        else if (ptype == typeof(DateTime))
                        {
                            /*
                            * /\s /	// 匹配任何空白字符，包括空格、制表符、换页符等等。等价于 [ \f\n\r\t\v]。
                            * /\x20 /	// 匹配一个空格。
                            * /\f /	// 匹配一个换页符。
                            * /\n /	// 匹配一个换行符。
                            * /\r /	// 匹配一个回车符。
                            */
                            var rgx = new System.Text.RegularExpressions.Regex("[-|/]", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                            var strVal = rgx.Replace(cellVal, "");
                            if (DateTime.TryParseExact(strVal, new string[] { "yyyyMM", "yyyyMMdd", "yyyyMMdd HH:mm", "yyyyMMdd HH:mm:ss", "yyyyMMdd HH:mm:ss.fff", "yyyyMMddTHH:mm", "yyyyMMddTHH:mm:ss", "yyyyMMddTHH:mm:ss.fff" }, null, System.Globalization.DateTimeStyles.None, out DateTime DateTimeval))
                                val = DateTimeval;
                        }
                        else if (prop.PropertyType == typeof(string))
                        {
                            val = cellVal;
                        }
                        if (val != null || prop.PropertyType.IsGenericType)
                            prop.SetValue(Tobj, val);
                    }
                    j++;
                }
            } while (reader.ReadNextSibling());

            if (isAllNull)
                return false;
            else
                return true;
        }

        String GetCellValue(WorkbookPart workbookPart, Cell c)
        {
            string cellValue;
            if (c.DataType != null && c.DataType == CellValues.SharedString)
            {
                SharedStringItem ssi = workbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ElementAt(int.Parse(c.CellValue.InnerText));
                cellValue = ssi.Text.Text;
            }
            else
            {
                cellValue = c.CellValue.InnerText;
            }
            return cellValue;
        }
    }

}
