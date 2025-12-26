using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel; // xls
using NPOI.XSSF.UserModel; // xlsx


namespace ExcelTool
{
    class Program
    {
        private static string SourceExcelPath; //源excel路径
        private static string OutBytesFilePath; //bytes文件路径
        private static string OutCSharpFilePath; //c#脚本路径

        static void Main(string[] args)
        {
            LoadConfig();
            ReadFiles(SourceExcelPath);

            Console.WriteLine("全部生成完毕");
            Console.ReadLine();
        }

        private static void LoadConfig()
        {
            string configPath = Environment.CurrentDirectory + "/config.txt";

            if (File.Exists(configPath))
            {
                string str = "";
                using (FileStream fs = new FileStream(configPath, FileMode.Open))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        str = sr.ReadToEnd();
                    }
                }

                if (!string.IsNullOrWhiteSpace(str))
                {
                    string[] arr = str.Split('\n');

                    SourceExcelPath = arr[0].Trim();
                    OutBytesFilePath = arr[1].Trim();
                    OutCSharpFilePath = arr[2].Trim();
                }
            }
        }

        public static void ReadFiles(string path)
        {
            string[] arr = Directory.GetFiles(path);

            for (int i = 0; i < arr.Length; i++)
            {
                string filePath = arr[i];
                FileInfo file = new FileInfo(filePath);
                if (file.Name.IndexOf("~$") > -1)
                {
                    continue;
                }
                if (file.Extension.Equals(".xls") || file.Extension.Equals(".xlsx"))
                {
                    string fileName = file.Name.Substring(0, file.Name.LastIndexOf('.'));

                    string tempExcel = null;
                    try
                    {
                        tempExcel = CopyExcelToTemp(file.FullName);
                        DataTable table = LoadExcel(tempExcel);
                        //创建普通表
                        CreateData(fileName, table);
                    }
                    finally
                    {
                        if (!string.IsNullOrEmpty(tempExcel) && File.Exists(tempExcel))
                        {
                            File.Delete(tempExcel);
                        }
                    }
                }
            }
        }

        //复制临时文件 防止资源占用导致生成错误
        public static string CopyExcelToTemp(string sourcePath)
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "ExcelTemp");
            if (!Directory.Exists(tempDir))
                Directory.CreateDirectory(tempDir);

            string tempPath = Path.Combine(
                tempDir,
                Guid.NewGuid().ToString("N") + Path.GetExtension(sourcePath));

            // 关键点：FileShare.ReadWrite
            using (var sourceStream = new FileStream(
                sourcePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite))
            using (var targetStream = new FileStream(
                tempPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None))
            {
                sourceStream.CopyTo(targetStream);
            }

            return tempPath;
        }

        public static DataTable LoadExcel(string filePath, string sheetName = null)
        {
            IWorkbook workbook;
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                if (filePath.EndsWith(".xls", StringComparison.OrdinalIgnoreCase))
                    workbook = new HSSFWorkbook(fs);
                else if (filePath.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                    workbook = new XSSFWorkbook(fs);
                else
                    throw new Exception("Unsupported excel format");
            }

            // 👉 关键：创建公式计算器
            IFormulaEvaluator evaluator =
                workbook.GetCreationHelper().CreateFormulaEvaluator();

            ISheet sheet = string.IsNullOrEmpty(sheetName)
                ? workbook.GetSheetAt(0)
                : workbook.GetSheet(sheetName);

            if (sheet == null)
                throw new Exception("Sheet not found");

            DataTable table = new DataTable(sheet.SheetName);

            // 表头
            IRow headerRow = sheet.GetRow(sheet.FirstRowNum);
            int cellCount = headerRow.LastCellNum;

            for (int i = 0; i < cellCount; i++)
            {
                table.Columns.Add(headerRow.GetCell(i)?.ToString() ?? $"Column{i}");
            }

            // 数据行
            for (int i = sheet.FirstRowNum; i <= sheet.LastRowNum; i++)
            {
                IRow row = sheet.GetRow(i);
                if (row == null) continue;

                DataRow dataRow = table.NewRow();

                for (int j = 0; j < cellCount; j++)
                {
                    ICell cell = row.GetCell(j);
                    dataRow[j] = GetCellValue(cell, evaluator);
                }

                table.Rows.Add(dataRow);
            }

            return table;
        }
        private static object GetCellValue(ICell cell, IFormulaEvaluator evaluator)
        {
            if (cell == null) return string.Empty;

            switch (cell.CellType)
            {
                case CellType.String:
                    return cell.StringCellValue;

                case CellType.Numeric:
                    if (DateUtil.IsCellDateFormatted(cell))
                        return cell.DateCellValue;
                    else
                        return cell.NumericCellValue;

                case CellType.Boolean:
                    return cell.BooleanCellValue;

                case CellType.Formula:
                    return GetFormulaValue(cell, evaluator);

                default:
                    return cell.ToString();
            }
        }
        private static object GetFormulaValue(ICell cell, IFormulaEvaluator evaluator)
        {
            var eval = evaluator.Evaluate(cell);
            if (eval == null) return string.Empty;

            switch (eval.CellType)
            {
                case CellType.String:
                    return eval.StringValue;

                case CellType.Numeric:
                    return eval.NumberValue;

                case CellType.Boolean:
                    return eval.BooleanValue;

                default:
                    return eval.FormatAsString();
            }
        }


        #region 创建普通表
        //表头
        static string[,] tableHeadArr = null;

        private static void CreateData(string fileName, DataTable dt)
        {
            try
            {
                //数据格式 行数 列数 二维数组每项的值 这里不做判断 都用string存储
                tableHeadArr = null;

                byte[] buffer = null;

                using (MMO_MemoryStream ms = new MMO_MemoryStream())
                {
                    int rows = dt.Rows.Count;
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        //防止空行
                        if (string.IsNullOrWhiteSpace(dt.Rows[i][0].ToString()))
                        {
                            rows = i;
                            break;
                        }
                    }

                    int columns = dt.Columns.Count;
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        //防止空列
                        if (string.IsNullOrWhiteSpace(dt.Rows[0][i].ToString()))
                        {
                            columns = i;
                            break;
                        }
                    }

                    tableHeadArr = new string[columns, 3];

                    ms.WriteInt(rows - 3); //减去表头的三行
                    ms.WriteInt(columns);
                    for (int i = 0; i < rows; i++)
                    {
                        for (int j = 0; j < columns; j++)
                        {
                            if (i < 3)
                            {
                                tableHeadArr[j, i] = dt.Rows[i][j].ToString().Trim();
                            }
                            else
                            {
                                string type = tableHeadArr[j, 1];
                                string value = dt.Rows[i][j].ToString().Trim();

                                //Console.WriteLine("type=" + type + "||" + "value=" + value);

                                switch (type.ToLower())
                                {
                                    case "int":
                                        ms.WriteInt(string.IsNullOrWhiteSpace(value) ? 0 : int.Parse(value));
                                        break;
                                    case "long":
                                        ms.WriteLong(string.IsNullOrWhiteSpace(value) ? 0 : long.Parse(value));
                                        break;
                                    case "short":
                                        ms.WriteShort(string.IsNullOrWhiteSpace(value) ? (short)0 : short.Parse(value));
                                        break;
                                    case "float":
                                        ms.WriteFloat(string.IsNullOrWhiteSpace(value) ? 0 : float.Parse(value));
                                        break;
                                    case "byte":
                                        ms.WriteByte(string.IsNullOrWhiteSpace(value) ? (byte)0 : byte.Parse(value));
                                        break;
                                    case "bool":
                                        ms.WriteBool(string.IsNullOrWhiteSpace(value) ? false : bool.Parse(value));
                                        break;
                                    case "double":
                                        ms.WriteDouble(string.IsNullOrWhiteSpace(value) ? 0 : double.Parse(value));
                                        break;
                                    default:
                                        ms.WriteUTF8String(value);
                                        break;
                                }
                            }
                        }
                    }
                    buffer = ms.ToArray();
                }

                CreateEntity(fileName, tableHeadArr, buffer);
            }
            catch (Exception ex)
            {
                Console.WriteLine("表格=>" + fileName + " 处理失败:" + ex.Message);
            }
        }

        /// <summary>
        /// 创建客户端实体
        /// </summary>
        private static void CreateEntity(string fileName, string[,] dataArr, byte[] buffer)
        {
            if (dataArr == null) return;

            //生成Byte文件
            {
                if (!Directory.Exists(OutBytesFilePath)) Directory.CreateDirectory(OutBytesFilePath);
                FileStream fs = new FileStream(string.Format("{0}\\{1}", OutBytesFilePath, fileName + ".bytes"), FileMode.Create);
                fs.Write(buffer, 0, buffer.Length);
                fs.Close();
                Console.WriteLine("客户端表格=>" + fileName + " 生成bytes文件完毕");
            }

            //生成代码Entity
            StringBuilder sbr = new StringBuilder();
            sbr.Append("using System.Collections;\r\n");
            sbr.Append("\r\n");
            sbr.Append("namespace YouYouFramework\r\n");
            sbr.Append("{\r\n");
            sbr.Append("    /// <summary>\r\n");
            sbr.AppendFormat("      /// {0}实体\r\n", fileName);
            sbr.Append("    /// </summary>\r\n");
            sbr.AppendFormat("    public partial class {0}Entity : DataTableEntityBase\r\n", fileName);
            sbr.Append("    {\r\n");

            for (int i = 0; i < dataArr.GetLength(0); i++)
            {
                if (i == 0) continue;
                sbr.Append("        /// <summary>\r\n");
                sbr.AppendFormat("        /// {0}\r\n", dataArr[i, 2]);
                sbr.Append("        /// </summary>\r\n");
                sbr.AppendFormat("        public {0} {1};\r\n", dataArr[i, 1], dataArr[i, 0]);
                sbr.Append("\r\n");
            }

            sbr.Append("    }\r\n");
            sbr.Append("}\r\n");


            using (FileStream fs = new FileStream(string.Format("{0}/{1}Entity.cs", OutCSharpFilePath, fileName), FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(sbr.ToString());
                }
            }

            //生成代码DBModel
            sbr.Clear();
            sbr.Append("\r\n");
            sbr.Append("using System.Collections;\r\n");
            sbr.Append("using System.Collections.Generic;\r\n");
            sbr.Append("using System;\r\n");
            sbr.Append("\r\n");
            sbr.Append("namespace YouYouFramework\r\n");
            sbr.Append("{\r\n");
            sbr.Append("    /// <summary>\r\n");
            sbr.AppendFormat("    /// {0}数据管理\r\n", fileName);
            sbr.Append("    /// </summary>\r\n");
            sbr.AppendFormat("    public partial class {0}DBModel : DataTableDBModelBase<{0}DBModel, {0}Entity>\r\n", fileName);
            sbr.Append("    {\r\n");

            sbr.Append("        /// <summary>\r\n");
            sbr.Append("        /// 文件名称\r\n");
            sbr.Append("        /// </summary>\r\n");
            sbr.AppendFormat("        public override string DataTableName {{ get {{ return \"{0}\"; }} }}\r\n", fileName);
            sbr.Append("\r\n");


            sbr.Append("        /// <summary>\r\n");
            sbr.Append("        /// 加载列表\r\n");
            sbr.Append("        /// </summary>\r\n");
            sbr.Append("        protected override void LoadList(MMO_MemoryStream ms)\r\n");
            sbr.Append("        {\r\n");
            sbr.Append("            int rows = ms.ReadInt();\r\n");
            sbr.Append("            int columns = ms.ReadInt();\r\n");
            sbr.Append("\r\n");
            sbr.Append("            for (int i = 0; i < rows; i++)\r\n");
            sbr.Append("            {\r\n");
            sbr.AppendFormat("                {0}Entity entity = new {0}Entity();\r\n", fileName);

            for (int i = 0; i < dataArr.GetLength(0); i++)
            {
                if (dataArr[i, 1].Equals("byte", StringComparison.CurrentCultureIgnoreCase))
                {
                    sbr.AppendFormat("                entity.{0} = (byte)ms.Read{1}();\r\n", dataArr[i, 0], ChangeTypeName(dataArr[i, 1]));
                }
                else
                {
                    sbr.AppendFormat("                entity.{0} = ms.Read{1}();\r\n", dataArr[i, 0], ChangeTypeName(dataArr[i, 1]));
                }
            }

            sbr.Append("\r\n");
            sbr.Append("                m_List.Add(entity);\r\n");
            sbr.Append("                m_Dic[entity.Id] = entity;\r\n");
            sbr.Append("            }\r\n");
            sbr.Append("        }\r\n");
            sbr.Append("    }\r\n");

            sbr.Append("}");
            using (FileStream fs = new FileStream(string.Format("{0}/{1}DBModel.cs", OutCSharpFilePath, fileName), FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(sbr.ToString());
                }
            }

            Console.WriteLine("客户端表格=>" + fileName + " 生成实体脚本完毕");
        }


        private static string ChangeTypeName(string type)
        {
            string str = string.Empty;

            switch (type)
            {
                case "byte":
                    str = "Byte";
                    break;
                case "int":
                    str = "Int";
                    break;
                case "short":
                    str = "Short";
                    break;
                case "long":
                    str = "Long";
                    break;
                case "float":
                    str = "Float";
                    break;
                case "string":
                    str = "UTF8String";
                    break;
            }

            return str;
        }
        #endregion

    }
}