using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


//如果要支持xlsx格式表格，请在本机电脑安装这个
//http://download.microsoft.com/download/7/0/3/703ffbcb-dc0c-4e19-b0da-1463960fdcdb/AccessDatabaseEngine.exe

namespace ExcelTool
{
    class Program
    {
        private static string SourceExcelPath; //源excel路径
        private static string OutBytesFilePath; //bytes文件路径
        private static string OutCSharpFilePath; //c#脚本路径
        private static string OutLuaFilePath; //lua脚本路径

        private static string OutBytesFilePath_Server; //服务器端表格文件路径
        private static string OutCSharpFilePath_Server; //服务器端c#脚本路径

        private static string OutHotfixFilePath;//热更层C#脚本路径


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
                    if (arr.Length > 3) OutLuaFilePath = arr[3].Trim();
                    if (arr.Length > 4) OutBytesFilePath_Server = arr[4].Trim();
                    if (arr.Length > 5) OutCSharpFilePath_Server = arr[5].Trim();
                    if (arr.Length > 6) OutHotfixFilePath = arr[6].Trim();
                }
            }
        }

        public static List<string> ReadFiles(string path)
        {
            string[] arr = Directory.GetFiles(path);

            List<string> lst = new List<string>();

            int len = arr.Length;
            for (int i = 0; i < len; i++)
            {
                string filePath = arr[i];
                FileInfo file = new FileInfo(filePath);
                if (file.Name.IndexOf("~$") > -1)
                {
                    continue;
                }
                if (file.Extension.Equals(".xls") || file.Extension.Equals(".xlsx"))
                {
                    ReadData(file.Extension.Equals(".xls"), file.FullName, file.Name.Substring(0, file.Name.LastIndexOf('.')));
                }
            }

            return lst;
        }


        private static void ReadData(bool isXls, string filePath, string fileName)
        {

            if (string.IsNullOrWhiteSpace(filePath)) return;

            //把表格复制一下
            string newPath = filePath + ".temp";

            File.Copy(filePath, newPath, true);

            string tableName = "Sheet1";
            string strConn = "";
            if (isXls)
            {
                strConn = "Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + newPath + ";" + "Extended Properties='Excel 8.0;HDR=NO;IMEX=1';";
            }
            else
            {
                strConn = "Provider = Microsoft.ACE.OLEDB.12.0; Data Source =" + newPath + ";Extended Properties='Excel 12.0;HDR=NO;IMEX=1'";
            }

            DataTable dt = null;

            string strExcel = "";
            OleDbDataAdapter myCommand = null;
            DataSet ds = null;
            strExcel = string.Format("select * from [{0}$]", tableName);
            myCommand = new OleDbDataAdapter(strExcel, strConn);
            ds = new DataSet();
            myCommand.Fill(ds, "table1");
            dt = ds.Tables[0];
            myCommand.Dispose();

            File.Delete(newPath);

            if (fileName.Equals("Sys_Localization", StringComparison.CurrentCultureIgnoreCase))
            {
                //多语言表 单独处理
                CreateLocalization(fileName, dt);
            }
            else
            {
                CreateData(fileName, dt);
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
                CreateLuaEntity(fileName, tableHeadArr);

                CreateServerEntity(fileName, tableHeadArr, buffer);

                CreateHotfixEntity(fileName, tableHeadArr);
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
            sbr.Append("namespace YouYou\r\n");
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
            sbr.Append("namespace YouYou\r\n");
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

        private static void CreateHotfixEntity(string fileName, string[,] dataArr)
        {
            if (dataArr == null) return;
            if (string.IsNullOrWhiteSpace(OutHotfixFilePath)) return;

            //生成代码Entity
            StringBuilder sbr = new StringBuilder();
            sbr.Append("using System.Collections;\r\n");
            sbr.Append("\r\n");
            sbr.Append("namespace Hotfix\r\n");
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


            using (FileStream fs = new FileStream(string.Format("{0}/{1}Entity.cs", OutHotfixFilePath, fileName), FileMode.Create))
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
            sbr.Append("namespace Hotfix\r\n");
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
            using (FileStream fs = new FileStream(string.Format("{0}/{1}DBModel.cs", OutHotfixFilePath, fileName), FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(sbr.ToString());
                }
            }

            Console.WriteLine("Hotix=>" + fileName + " CreateEntityComplete");
        }

        /// <summary>
        /// 创建Lua的实体
        /// </summary>
        private static void CreateLuaEntity(string fileName, string[,] dataArr)
        {
            if (dataArr == null) return;
            if (string.IsNullOrWhiteSpace(OutLuaFilePath)) return;

            //生成lua的Entity
            StringBuilder sbr = new StringBuilder();
            sbr.AppendFormat("{0}Entity = {{ ", fileName);
            for (int i = 0; i < dataArr.GetLength(0); i++)
            {

                if (i == dataArr.GetLength(0) - 1)
                {
                    if (dataArr[i, 1].Equals("string", StringComparison.CurrentCultureIgnoreCase))
                    {
                        sbr.AppendFormat("{0} = \"\"", dataArr[i, 0]);
                    }
                    else
                    {
                        sbr.AppendFormat("{0} = 0", dataArr[i, 0]);
                    }
                }
                else
                {
                    if (dataArr[i, 1].Equals("string", StringComparison.CurrentCultureIgnoreCase))
                    {
                        sbr.AppendFormat("{0} = \"\", ", dataArr[i, 0]);
                    }
                    else
                    {
                        sbr.AppendFormat("{0} = 0, ", dataArr[i, 0]);
                    }
                }
            }
            sbr.Append(" }\r\n");

            sbr.Append("\r\n");
            sbr.AppendFormat("{0}Entity.__index = {0}Entity;\r\n", fileName);
            sbr.Append("\r\n");
            sbr.AppendFormat("function {0}Entity.New(", fileName);
            for (int i = 0; i < dataArr.GetLength(0); i++)
            {
                if (i == dataArr.GetLength(0) - 1)
                {
                    sbr.AppendFormat("{0}", dataArr[i, 0]);
                }
                else
                {
                    sbr.AppendFormat("{0}, ", dataArr[i, 0]);
                }
            }
            sbr.Append(")\r\n");
            sbr.Append("    local self = { };\r\n");
            sbr.Append("");
            sbr.AppendFormat("    setmetatable(self, {0}Entity);\r\n", fileName);
            sbr.Append("\r\n");
            for (int i = 0; i < dataArr.GetLength(0); i++)
            {
                sbr.AppendFormat("    self.{0} = {0};\r\n", dataArr[i, 0]);
            }
            sbr.Append("\r\n");
            sbr.Append("    return self;\r\n");
            sbr.Append("end");

            using (FileStream fs = new FileStream(string.Format("{0}/{1}Entity.bytes", OutLuaFilePath, fileName), FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(sbr.ToString());
                }
            }

            //===============生成lua的DBModel
            sbr.Clear();
            sbr.Append("--数据访问\r\n");
            sbr.AppendFormat("{0}DBModel = {{ }}\r\n", fileName);
            sbr.Append("\r\n");
            sbr.AppendFormat("local this = {0}DBModel;\r\n", fileName);
            sbr.Append("\r\n");
            sbr.AppendFormat("local {0}Table = {{ }}; --定义表格\r\n", fileName.ToLower());
            sbr.Append("\r\n");
            sbr.AppendFormat("function {0}DBModel.LoadList()\r\n", fileName);
            sbr.Append("    GameInit.AddTotalLoadTableCount();\r\n");
            sbr.AppendFormat("    CS.YouYou.GameEntry.Lua:LoadDataTable(\"{0}\", this.LoadFormMS);\r\n", fileName);
            sbr.Append("end\r\n");
            sbr.Append("\r\n");
            sbr.AppendFormat("function {0}DBModel.LoadFormMS(ms)\r\n", fileName);
            sbr.Append("    local rows = ms:ReadInt();\r\n");
            sbr.Append("    ms:ReadInt();\r\n");
            sbr.Append("\r\n");
            sbr.Append("    for i = 0, rows, 1 do\r\n");
            sbr.AppendFormat("        {0}Table[#{0}Table + 1] = {1}Entity.New(\r\n", fileName.ToLower(), fileName);

            string str = "";
            for (int i = 0; i < dataArr.GetLength(0); i++)
            {
                if (dataArr[i, 1].Equals("byte", StringComparison.CurrentCultureIgnoreCase))
                {
                    str += string.Format("                ms:Read{1}(),\r\n", dataArr[i, 0], ChangeTypeName(dataArr[i, 1]));
                }
                else
                {
                    str += string.Format("                ms:Read{1}(),\r\n", dataArr[i, 0], ChangeTypeName(dataArr[i, 1]));
                }
            }
            str = str.TrimEnd(',', '\r', '\n');
            sbr.AppendFormat("{0}\r\n", str);
            sbr.Append("        );\r\n");
            sbr.Append("    end\r\n");
            sbr.Append("    GameInit.LoadOneTableComplete();\r\n");
            sbr.Append("end\r\n");
            sbr.Append("\r\n");
            sbr.AppendFormat("function {0}DBModel.GetList()\r\n", fileName);
            sbr.AppendFormat("    return {0}Table;\r\n", fileName.ToLower());
            sbr.Append("end");
            sbr.Append("\r\n");
            sbr.Append("\r\n");
            sbr.AppendFormat("function {0}DBModel.GetEntity(id)\r\n", fileName);
            sbr.AppendFormat("    local ret = nil;\r\n");
            sbr.AppendFormat("    for i = 1, #{0}Table, 1 do\r\n", fileName.ToLower());
            sbr.AppendFormat("        if ({0}Table[i].Id == id) then\r\n", fileName.ToLower());
            sbr.AppendFormat("            ret = {0}Table[i];\r\n", fileName.ToLower());
            sbr.AppendFormat("            break;\r\n");
            sbr.AppendFormat("        end\r\n");
            sbr.AppendFormat("    end\r\n");
            sbr.AppendFormat("    return ret;\r\n");
            sbr.AppendFormat("end");

            using (FileStream fs = new FileStream(string.Format("{0}/{1}DBModel.bytes", OutLuaFilePath, fileName), FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(sbr.ToString());
                }
            }

            Console.WriteLine("Lua表格=>" + fileName + " 生成实体脚本完毕");
        }

        /// <summary>
        /// 创建服务器端实体
        /// </summary>
        private static void CreateServerEntity(string fileName, string[,] dataArr, byte[] buffer)
        {
            if (dataArr == null) return;
            if (string.IsNullOrWhiteSpace(OutCSharpFilePath_Server)) return;
            if (string.IsNullOrWhiteSpace(OutBytesFilePath_Server)) return;

            //生成Byte文件
            {
                if (!Directory.Exists(OutBytesFilePath_Server)) Directory.CreateDirectory(OutBytesFilePath_Server);
                FileStream fs = new FileStream(string.Format("{0}\\{1}", OutBytesFilePath_Server, fileName + ".bytes"), FileMode.Create);
                fs.Write(buffer, 0, buffer.Length);
                fs.Close();
                Console.WriteLine("服务器端表格=>" + fileName + " 生成bytes文件完毕");
            }

            //生成代码Entity
            StringBuilder sbr = new StringBuilder();
            sbr.Append("\r\n");
            sbr.Append("using System.Collections;\r\n");
            sbr.Append("\r\n");
            sbr.Append("/// <summary>\r\n");
            sbr.AppendFormat("/// {0}实体\r\n", fileName);
            sbr.Append("/// </summary>\r\n");
            sbr.AppendFormat("public partial class {0}Entity : DataTableEntityBase\r\n", fileName);
            sbr.Append("{\r\n");

            for (int i = 0; i < dataArr.GetLength(0); i++)
            {
                if (i == 0) continue;
                sbr.Append("    /// <summary>\r\n");
                sbr.AppendFormat("    /// {0}\r\n", dataArr[i, 2]);
                sbr.Append("    /// </summary>\r\n");
                sbr.AppendFormat("    public {0} {1};\r\n", dataArr[i, 1], dataArr[i, 0]);
                sbr.Append("\r\n");
            }

            sbr.Append("}\r\n");


            using (FileStream fs = new FileStream(string.Format("{0}/{1}Entity.cs", OutCSharpFilePath_Server, fileName), FileMode.Create))
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
            sbr.Append("/// <summary>\r\n");
            sbr.AppendFormat("/// {0}数据管理\r\n", fileName);
            sbr.Append("/// </summary>\r\n");
            sbr.AppendFormat("public partial class {0}DBModel : DataTableDBModelBase<{0}DBModel, {0}Entity>\r\n", fileName);
            sbr.Append("{\r\n");

            sbr.Append("    /// <summary>\r\n");
            sbr.Append("    /// 文件名称\r\n");
            sbr.Append("    /// </summary>\r\n");
            sbr.AppendFormat("    public override string DataTableName {{ get {{ return \"{0}\"; }} }}\r\n", fileName);
            sbr.Append("\r\n");


            sbr.Append("    /// <summary>\r\n");
            sbr.Append("    /// 加载列表\r\n");
            sbr.Append("    /// </summary>\r\n");
            sbr.Append("    protected override void LoadList(MMO_MemoryStream ms)\r\n");
            sbr.Append("    {\r\n");
            sbr.Append("        int rows = ms.ReadInt();\r\n");
            sbr.Append("        int columns = ms.ReadInt();\r\n");
            sbr.Append("\r\n");
            sbr.Append("        for (int i = 0; i < rows; i++)\r\n");
            sbr.Append("        {\r\n");
            sbr.AppendFormat("            {0}Entity entity = new {0}Entity();\r\n", fileName);

            for (int i = 0; i < dataArr.GetLength(0); i++)
            {
                if (dataArr[i, 1].Equals("byte", StringComparison.CurrentCultureIgnoreCase))
                {
                    sbr.AppendFormat("            entity.{0} = (byte)ms.Read{1}();\r\n", dataArr[i, 0], ChangeTypeName(dataArr[i, 1]));
                }
                else
                {
                    sbr.AppendFormat("            entity.{0} = ms.Read{1}();\r\n", dataArr[i, 0], ChangeTypeName(dataArr[i, 1]));
                }
            }

            sbr.Append("\r\n");
            sbr.Append("            m_List.Add(entity);\r\n");
            sbr.Append("            m_Dic[entity.Id] = entity;\r\n");
            sbr.Append("        }\r\n");
            sbr.Append("    }\r\n");

            sbr.Append("}");
            using (FileStream fs = new FileStream(string.Format("{0}/{1}DBModel.cs", OutCSharpFilePath_Server, fileName), FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(sbr.ToString());
                }
            }
            Console.WriteLine("服务器表格=>" + fileName + " 生成实体脚本完毕");
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

        #region 创建多语言表
        private static void CreateLocalization(string fileName, DataTable dt)
        {
            try
            {
                if (!Directory.Exists(OutBytesFilePath + "Localization/")) Directory.CreateDirectory(OutBytesFilePath + "Localization/");

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

                int newcolumns = columns - 3; //减去前三列 后面表示有多少种语言

                int currKeyColumn = 2; //当前的Key列
                int currValueColumn = 3; //当前的值列

                tableHeadArr = new string[columns, 3];

                while (newcolumns > 0)
                {
                    newcolumns--;

                    #region 写入文件
                    byte[] buffer = null;

                    using (MMO_MemoryStream ms = new MMO_MemoryStream())
                    {
                        ms.WriteInt(rows - 3); //减去表头的三行
                        ms.WriteInt(2); //多语言表 只有2列 Key Value

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
                                    if (j == currKeyColumn)
                                    {
                                        //写入key
                                        string value = dt.Rows[i][j].ToString().Trim();
                                        ms.WriteUTF8String(value);
                                    }
                                    else if (j == currValueColumn)
                                    {
                                        //写入value
                                        string value = dt.Rows[i][j].ToString().Trim();
                                        ms.WriteUTF8String(value);
                                    }
                                }
                            }
                        }
                        buffer = ms.ToArray();
                    }

                    //写入文件
                    FileStream fs = new FileStream(string.Format("{0}/Localization/{1}", OutBytesFilePath, tableHeadArr[currValueColumn, 0] + ".bytes"), FileMode.Create);
                    fs.Write(buffer, 0, buffer.Length);
                    fs.Close();

                    currValueColumn++;
                    #endregion
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("表格=>" + fileName + " 处理失败:" + ex.Message);
            }
        }
        #endregion
    }
}