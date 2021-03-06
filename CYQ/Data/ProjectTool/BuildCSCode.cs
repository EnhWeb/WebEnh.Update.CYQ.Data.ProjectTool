﻿namespace CYQ.Data.ProjectTool
{
    using CYQ.Data;
    using CYQ.Data.SQL;
    using CYQ.Data.Table;
    using CYQ.Data.Tool;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Text;

    /// <summary>
    /// BuildCSCode
    /// </summary>
    internal class BuildCSCode
    {
        /// <summary>
        /// 创建结束后事件
        /// </summary>
        internal static  event CreateEndHandle OnCreateEnd;

        private static void AppendText(StringBuilder sb, string text, params string[] format)
        {
            if (text.IndexOf("{0}") > -1)
            {
                sb.AppendFormat(text + "\r\n", (object[]) format);
            }
            else
            {
                sb.AppendLine(text);
            }
        }

        private static void BuildSingTableEntityText(string tableName, string description, ProjectConfig config, string dbName)
        {
            string str = FormatKey(tableName);
            if (config.MapName)
            {
                str = FixName(tableName);
            }
            bool flag = config.BuildMode.StartsWith("纯") || config.BuildMode.Contains("DBFast");
            try
            {
                StringBuilder sb = new StringBuilder(50000);
                string str2 = string.Format(config.NameSpace, dbName+"DB").TrimEnd(new char[] { '.' });
                AppendText(sb, "using System;", new string[0]);
                AppendText(sb, "using System.ComponentModel.DataAnnotations;", new string[0]);
                AppendText(sb, "using System.ComponentModel;", new string[0]);
                if (!config.ForTwoOnly)
                {
                    AppendText(sb, "using System.ComponentModel.DataAnnotations.Schema;", new string[0]);
                }
                AppendText(sb, "", new string[0]);
                AppendText(sb, "namespace {0}", new string[] { str2 });
                AppendText(sb, "{", new string[0]);

                if (!string.IsNullOrEmpty(description))
                {
                    description = description.Replace("\r\n","    ").Replace("\r","  ").Replace("\n","  ");
                    AppendText(sb, "    /// <summary>", new string[0]);
                    AppendText(sb, "    /// {0}", new string[] { description });
                    AppendText(sb, "    /// </summary>", new string[0]);
                    AppendText(sb, "    [DisplayName(\"{0}\")]", new string[] { description });//实体表名的指定方法不是这样操作的，需更新，有空再更新
                }
                if (!config.ForTwoOnly)
                {
                    AppendText(sb, "    [Table(\"{0}\")]", str);
                }
                AppendText(sb, "    public class {0}{1}", new string[] { str + config.EntitySuffix, flag ? "" : " : CYQ.Data.Orm.OrmBase" });
                AppendText(sb, "    {", new string[0]);
                if (!flag)//如果是ORM，则进行下面的生成
                {
                    AppendText(sb, "        public {0}()", new string[] { str + config.EntitySuffix });
                    AppendText(sb, "        {", new string[0]);
                    AppendText(sb, "            base.SetInit(this, \"{0}\", \"{1}\");", new string[] { tableName, config.Name });
                    AppendText(sb, "        }", new string[0]);
                }
                MDataColumn columns = DBTool.GetColumns(tableName, config.Conn);
                if (columns.Count > 0)
                {
                    string name = string.Empty;
                    Type type = null;
                    
                    if (config.ForTwoOnly) // vs2015 模式
                    {
                        foreach (MCellStruct struct2 in columns)
                        {
                            name = struct2.ColumnName;
                            if (config.MapName)
                            {
                                name = FixName(name);
                            }
                            type = DataType.GetType(struct2.SqlType);
                            string typename = FormatType(type.Name, type.IsValueType, config.ValueTypeNullable);
                            // 详细描述
                            string Longhand_Description = struct2.Description
                            #region Longhand_Description 处理
                                .Replace("\r\n", "        /// ")
                                .Replace("\r", "        /// ")
                                .Replace("\n", "        /// ")
                                .Replace("        /// ", "\r\n        /// ")
                                ;//移除回车等换行字符串

                            if (string.IsNullOrEmpty(Longhand_Description))
                            {
                                Longhand_Description = "[ 无说明描术 ]";
                            }
                            #endregion
                            // 名称简写
                            string Shorthand_Description = struct2.Description;
                            #region Shorthand_Description 处理
                            if (string.IsNullOrEmpty(Shorthand_Description))
                            {
                                Shorthand_Description = "[ 无说明描术 ]";
                            }
                            else
                            {
                                Shorthand_Description = Shorthand_Description.Replace("\r\n", "    ").Replace("\r", "  ").Replace("\n", "  ");

                                #region //  从零字符开始，取到指定标志字符处                                
                                int index = Shorthand_Description.IndexOfAny(new char[] { '(', '（', ':', '：', ' ', '　', ',', '，', '|', '｜', '.', '。' });
                                index = index == -1 ? Shorthand_Description.Length : index;
                                Shorthand_Description = Shorthand_Description.Substring(0, index);
                                #endregion
                            }
                            #endregion

                            AppendText(sb, "        #region [  public     {0} {1} {2}  ]", new string[] { typename.PadRight(15), name.PadRight(30), Shorthand_Description.PadRight(30) });//添加换行

                            AppendText(sb, "        /// <summary>", new string[0]);
                            AppendText(sb, "        /// 私有变量：{0}", new string[] { Longhand_Description });
                            AppendText(sb, "        /// </summary>", new string[0]);
                            AppendText(sb, "        private {0} _{1};", new string[] { typename, name });

                            AppendText(sb, "        /// <summary>", new string[0]);
                            AppendText(sb, "        /// {0}", new string[] { Longhand_Description });
                            AppendText(sb, "        /// </summary>", new string[0]);
                            AppendText(sb, "        [Display(Name = \"{0}\")]", new string[] { Shorthand_Description });
                            AppendText(sb, "        public {0} {1}", new string[] { typename, name });
                            AppendText(sb, "        {", new string[0]);
                            AppendText(sb, "            get", new string[0]);
                            AppendText(sb, "            {", new string[0]);
                            AppendText(sb, "                return _{0};", new string[] { name });
                            AppendText(sb, "            }", new string[0]);
                            AppendText(sb, "            set", new string[0]);
                            AppendText(sb, "            {", new string[0]);
                            AppendText(sb, "                _{0} = value;", new string[] { name });
                            AppendText(sb, "            }", new string[0]);
                            AppendText(sb, "        }", new string[0]);
                            AppendText(sb, "        #endregion", new string[0]);//添加换行
                        }
                    }
                    else // 新模式
                    {
                        foreach (MCellStruct struct3 in columns)
                        {
                            name = struct3.ColumnName;
                            if (config.MapName)
                            {
                                name = FixName(name);
                            }
                            type = DataType.GetType(struct3.SqlType);
                            if (!string.IsNullOrEmpty(struct3.Description))
                            {
                                // 详细描述
                                string Longhand_Description = struct3.Description
                                #region Longhand_Description 处理
                                .Replace("\r\n", "        /// ")
                                    .Replace("\r", "        /// ")
                                    .Replace("\n", "        /// ")
                                    .Replace("        /// ", "\r\n        /// ")
                                    ;//移除回车等换行字符串

                                if (string.IsNullOrEmpty(Longhand_Description))
                                {
                                    Longhand_Description = "[ 无说明描术 ]";
                                }
                                #endregion
                                // 名称简写
                                string Shorthand_Description = struct3.Description;
                                #region Shorthand_Description 处理
                                if (string.IsNullOrEmpty(Shorthand_Description))
                                {
                                    Shorthand_Description = "[ 无说明描术 ]";
                                }
                                else
                                {
                                    Shorthand_Description = Shorthand_Description.Replace("\r\n", "    ").Replace("\r", "  ").Replace("\n", "  ");

                                    #region //  从零字符开始，取到指定标志字符处                                
                                    int index = Shorthand_Description.IndexOfAny(new char[] { '(', '（', ':', '：', ' ', '　', ',', '，', '|', '｜', '.', '。' });
                                    index = index == -1 ? Shorthand_Description.Length : index;
                                    Shorthand_Description = Shorthand_Description.Substring(0, index);
                                    #endregion
                                }
                                #endregion

                                struct3.Description = struct3.Description.Replace("\r\n", "    ").Replace("\r", "  ").Replace("\n", "  ");
                                AppendText(sb, "        /// <summary>", new string[0]);
                                AppendText(sb, "        /// {0}", new string[] { Longhand_Description });
                                AppendText(sb, "        /// </summary>", new string[0]);
                                AppendText(sb, "        [Display(Name = \"{0}\")]", new string[] { Shorthand_Description });
                            }
                            if (name.ToUpper() == "ID")
                            {
                                AppendText(sb, "        [DataObjectField(true)]");
                            }
                            AppendText(sb, "        public {0} {1} {{ get; set; }}", new string[] { FormatType(type.Name, type.IsValueType, config.ValueTypeNullable), name });
                            AppendText(sb,"");
                        }
                    }
                }
                sb.Append(
@"    }
}");
                File.WriteAllText(config.ProjectPath.TrimEnd(new char[] { '/', '\\' }) + @"\" + str + ".cs", sb.ToString(), Encoding.UTF8);
            }
            catch (Exception exception)
            {
                CYQ.Data.Log.WriteLogToTxt(exception);
            }
        }

        private static void BuildTableEntityText(Dictionary<string, string> tables, ProjectConfig config, string dbName)
        {
            foreach (KeyValuePair<string, string> pair in tables)
            {
                BuildSingTableEntityText(pair.Key, pair.Value, config, dbName);
            }
        }

        private static void BuildTableEnumText(Dictionary<string, string> tables, ProjectConfig config, string dbName)
        {
            try
            {
                StringBuilder builder = new StringBuilder(50000);

                string str = string.Format(config.NameSpace, dbName+"DB").TrimEnd(new char[] { '.' });//得到命名空间名称

                builder.AppendFormat(
                    @"using System;
using System.ComponentModel.DataAnnotations;
namespace {0}
{{
", str);//开始


                builder.Append($"    #region [    {dbName} 数据库中表名枚举    ]\r\n");
                builder.AppendFormat("    /// <summary>\r\n");
                builder.AppendFormat("    /// {0} 数据库中表集合枚举\r\n", dbName);
                builder.AppendFormat("    /// </summary>\r\n");
                //builder.AppendFormat("    [System.ComponentModel.DisplayName(\"{0}\")]",string.Empty);  //需自定义显示值的属性，暂未实现
                builder.Append(
                                config.MutilDatabase    //如果是选择了多数据库
                                ? string.Format("    public enum TableNames{0}Db\r\n    {{\r\n", dbName)          //如果是选择的多个数据库则返回这行
                                : string.Format("    public enum TableNames\r\n    {{\r\n", string.Empty)  //如果选择的不是多个数据库则返回这行
                                );

                foreach (KeyValuePair<string, string> pair in tables)//添加表名enum
                {
                    // 处理回车或换行
                    string strDisplayName = pair.Value.Replace("\r\n", "").Replace("\r","").Replace("\n","");//移除回车等换行字符串
                    strDisplayName = strDisplayName == "" ? pair.Key : strDisplayName;

                    builder.AppendFormat("        /// <summary>\r\n");
                    builder.AppendFormat("        /// enum 表名：{0}\r\n", strDisplayName);
                    builder.AppendFormat("        /// </summary>\r\n");
                    builder.AppendFormat("        [Display(Name = \"{0}\")]\r\n", strDisplayName);
                    builder.AppendFormat("        {0} ,\r\n\r\n", FormatKey(pair.Key));//表名
                }
                //builder[builder.Length - 1] = ' ';
                builder.Remove(builder.Length - 4, 4);
                builder.Append("\r\n    }");//结束表名的enum
                builder.Append("\r\n    #endregion\r\n");//结束



                builder.Append("\r\n    #region [    单个表枚举（带字段名和属性显示值）    ]\r\n");
                foreach (KeyValuePair<string, string> pair2 in tables)
                {
                    builder.Append(GetFiledEnum(pair2.Key, pair2.Value, config));
                }

                builder.Append(
@"    #endregion
}");//结束


                string str2 = $"TableNames-{dbName}.cs";
                if (config.MutilDatabase)//如果是选择的多个数据库则保存为单独文件
                {
                    str2 = $"TableNames-{dbName}DB.cs";
                }
                File.WriteAllText(config.ProjectPath.TrimEnd(new char[] { '/', '\\' }) + @"\" + str2, builder.ToString(), Encoding.UTF8);
            }
            catch (Exception exception)
            {
                CYQ.Data.Log.WriteLogToTxt(exception);
            }
        }

        /// <summary>
        /// 创建
        /// </summary>
        /// <param name="KeyConnName_ValueTableNames">KeyConnName_ValueTableNames  (key 配置名称),(表名，如果为 all 则所有表，否则为：tablename1,tablename2,tablename3,)</param>
        internal static void Create(object KeyConnName_ValueTableNames)
        {
            int count = 0;
            try
            {
                KeyValuePair<string, string> keyValuePair = (KeyValuePair<string, string>)KeyConnName_ValueTableNames;
                string str = Convert.ToString(keyValuePair.Key);
                string tablenames = keyValuePair.Value;

                using (ProjectConfig config = new ProjectConfig())
                {
                    try
                    {
                        if (config.Fill("Name='" + str + "'"))
                        {
                            string dbName = string.Empty;
                            string errInfo = string.Empty;

                            Dictionary<string, string> tables = DBTool.GetTables(config.Conn, out dbName,out errInfo);

                            Dictionary<string, string> selectTables = new Dictionary<string, string>();

                            if (tablenames == "all") // 只留下已选择的表
                            {
                                selectTables = tables;
                            }
                            else
                            {
                                string[] selectTableNames = tablenames.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                                foreach (var tablename in selectTableNames)
                                {
                                    selectTables.Add(tablename, tables[tablename]);
                                }
                            }

                            if ((selectTables != null) && (selectTables.Count > 0))
                            {
                                char ch = dbName[0];
                                dbName = ch.ToString().ToUpper() + dbName.Substring(1, dbName.Length - 1);
                                count = selectTables.Count;
                                if (config.BuildMode.Contains("枚举"))
                                {
                                    BuildTableEnumText(selectTables, config, dbName);
                                }
                                else
                                {
                                    BuildTableEntityText(selectTables, config, dbName);
                                }
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        CYQ.Data.Log.WriteLogToTxt(exception);
                    }
                }
            }
            finally
            {
                if (OnCreateEnd != null)
                {
                    OnCreateEnd(count);
                }
            }
        }

        internal static string FixName(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                name = name.ToLower();
                if (name == "id")
                {
                    return "ID";
                }
                bool flag = name.EndsWith("id");
                string[] strArray = name.Split(new char[] { '_', '-', ' ' });
                if (strArray.Length == 1)
                {
                    char ch = name[0];
                    name = ch.ToString().ToUpper() + name.Substring(1, name.Length - 1);
                }
                else
                {
                    name = string.Empty;
                    foreach (string str in strArray)
                    {
                        if (!string.IsNullOrEmpty(str))
                        {
                            if (str.Length == 1)
                            {
                                name = name + str.ToUpper();
                            }
                            else
                            {
                                char ch2 = str[0];
                                name = name + ch2.ToString().ToUpper() + str.Substring(1, str.Length - 1);
                            }
                        }
                    }
                }
                if (flag)
                {
                    name = name.Substring(0, name.Length - 2) + "ID";
                }
            }
            return name;
        }

        private static string FormatKey(string key)
        {
            return key.Replace("-", "_").Replace(" ", "");
        }

        private static string FormatType(string tName, bool isValueType, bool nullable)
        {
            string str = tName;
            if (str != null)
            {
                if (!(str == "Int32"))
                {
                    if (str == "String")
                    {
                        tName = "string";
                        nullable = false;
                    }
                    else if (str == "Boolean")
                    {
                        tName = "bool";
                    }
                    else if (str == "Int64")
                    {
                        tName = "long";
                    }
                    else if (str == "Int16")
                    {
                        tName = "short";
                    }
                }
                else
                {
                    tName = "int";
                }
            }
            if ((nullable && isValueType) && !tName.EndsWith("[]"))
            {
                tName = tName + "?";
            }
            return tName;
        }

        /// <summary>
        /// 得到enum
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="tableName_displayName">表名（显示名[数据库中备注的名称]）</param>
        /// <param name="config"></param>
        /// <returns></returns>
        private static string GetFiledEnum(string tableName,string tableName_displayName, ProjectConfig config)
        {
            StringBuilder builder = new StringBuilder();

            // 处理回车或换行
            tableName_displayName = tableName_displayName.Replace("\r\n", "").Replace("\r", "").Replace("\n", ""); ;
            tableName_displayName = tableName_displayName == "" ? tableName : tableName_displayName;


            builder.AppendFormat("    #region [    表名：{0}    备注名：{1}    ]\r\n", tableName.PadRight(45), tableName_displayName.PadRight(30));
            builder.AppendFormat("    /// <summary>\r\n", string.Empty);
            builder.AppendFormat("    /// enum 表名：{0}\r\n", tableName_displayName);
            builder.AppendFormat("    /// </summary>\r\n", string.Empty);
            builder.AppendFormat("    public enum {0}\r\n", FormatKey(tableName));
            builder.AppendFormat("    {{");
            try
            {
                MDataColumn columns = DBTool.GetColumns(tableName, config.Conn);
                if (columns.Count > 0)
                {
                    for (int i = 0; i < columns.Count; i++)//进行列的输出
                    {
                        string str = FormatKey(columns[i].ColumnName);
                        // 处理回车或换行
                        string strDisplayName = columns[i].Description.Replace("\r\n", "").Replace("\r", "").Replace("\n", ""); ;
                        strDisplayName = strDisplayName == "" ? str : strDisplayName;

                        builder.AppendFormat("\r\n");
                        builder.AppendFormat("        /// <summary>\r\n", string.Empty);
                        builder.AppendFormat("        /// enum 字段名：{0}\r\n", strDisplayName);
                        builder.AppendFormat("        /// </summary>\r\n", string.Empty);
                        builder.AppendFormat("        [Display(Name = \"{0}\")]\r\n", strDisplayName);

                        builder.AppendFormat("        {0} ,\r\n", str, strDisplayName);
                    }
                    builder.Append("    }\r\n");
                }
                else
                {
                    builder.Append("    }\r\n");
                }
            }
            catch (Exception exception)
            {
                CYQ.Data.Log.WriteLogToTxt(exception);
            }
            builder.AppendFormat("    #endregion\r\n");


            return builder.ToString();
        }

        internal delegate void CreateEndHandle(int count);
    }
}

