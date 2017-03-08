namespace CYQ.Data.ProjectTool
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

    internal class BuildCSCode
    {
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
                string str2 = string.Format(config.NameSpace, dbName).TrimEnd(new char[] { '.' });
                AppendText(sb, "using System;", new string[0]);
                AppendText(sb, "using System.ComponentModel.DataAnnotations;\r\n", new string[0]);
                AppendText(sb, "namespace {0}", new string[] { str2 });
                AppendText(sb, "{", new string[0]);

                if (!string.IsNullOrEmpty(description))
                {
                    description = description.Replace("\r\n","    ").Replace("\r","  ").Replace("\n","  ");
                    AppendText(sb, "    /// <summary>", new string[0]);
                    AppendText(sb, "    /// {0}", new string[] { description });
                    AppendText(sb, "    /// </summary>", new string[0]);
                    AppendText(sb, "    [System.ComponentModel.DisplayName(\"{0}\")]", new string[] { description });//实体表名的指定方法不是这样操作的，需更新，有空再更新
                }
                AppendText(sb, "    public class {0} {1}", new string[] { str + config.EntitySuffix, flag ? "" : ": CYQ.Data.Orm.OrmBase" });
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
                    if (config.ForTwoOnly)
                    {
                        foreach (MCellStruct struct2 in columns)
                        {
                            name = struct2.ColumnName;
                            if (config.MapName)
                            {
                                name = FixName(name);
                            }
                            type = DataType.GetType(struct2.SqlType);

                            AppendText(sb, "\r\n", new string[0]);//添加换行
                            if (!string.IsNullOrEmpty(struct2.Description))
                            {
                                AppendText(sb, "        /// <summary>", new string[0]);
                                AppendText(sb, "        /// 私有变量：{0}", new string[] { struct2.Description });
                                AppendText(sb, "        /// </summary>", new string[0]);
                            }
                            AppendText(sb, "        private {0} _{1};", new string[] { FormatType(type.Name, type.IsValueType, config.ValueTypeNullable), name });
                            if (!string.IsNullOrEmpty(struct2.Description))
                            {
                                struct2.Description = struct2.Description.Replace("\r\n", "    ").Replace("\r", "  ").Replace("\n", "  ");
                                #region //  从零字符开始，取到指定标志字符处                                
                                int index = struct2.Description.IndexOfAny(new char[] { '(', '（', ':', '：', ' ', '　', ',', '，','|','｜','.','。' });
                                index = index == -1 ? struct2.Description.Length : index;
                                struct2.Description = struct2.Description.Substring(0, index);
                                #endregion
                                AppendText(sb, "        /// <summary>", new string[0]);
                                AppendText(sb, "        /// {0}", new string[] { struct2.Description });
                                AppendText(sb, "        /// </summary>", new string[0]);
                                AppendText(sb, "        [Display(Name = \"{0}\")]", new string[] { struct2.Description });
                            }
                            AppendText(sb, "        public {0} {1}", new string[] { FormatType(type.Name, type.IsValueType, config.ValueTypeNullable), name });
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
                        }
                    }
                    else
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
                                struct3.Description = struct3.Description.Replace("\r\n", "    ").Replace("\r", "  ").Replace("\n", "  ");
                                AppendText(sb, "        /// <summary>", new string[0]);
                                AppendText(sb, "        /// {0}", new string[] { struct3.Description });
                                AppendText(sb, "        /// </summary>", new string[0]);
                            }
                            AppendText(sb, "        public {0} {1} {{ get; set; }}", new string[] { FormatType(type.Name, type.IsValueType, config.ValueTypeNullable), name });
                        }
                    }
                }
                AppendText(sb, "    }", new string[0]);
                AppendText(sb, "}", new string[0]);
                File.WriteAllText(config.ProjectPath.TrimEnd(new char[] { '/', '\\' }) + @"\" + str + ".cs", sb.ToString(), Encoding.Default);
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

                string str = string.Format(config.NameSpace, dbName).TrimEnd(new char[] { '.' });//得到命名空间名称







                builder.AppendFormat("using System;\r\n\r\nnamespace {0}\r\n{{\r\n", str);//开始


                builder.Append("\r\n\r\n    #region //    数据库中表名枚举 \r\n");
                builder.AppendFormat("    /// <summary>\r\n");
                builder.AppendFormat("    /// {0} 数据库中表集合枚举\r\n", dbName);
                builder.AppendFormat("    /// </summary>\r\n");
                //builder.AppendFormat("    [System.ComponentModel.DisplayName(\"{0}\")]",string.Empty);  //需自定义显示值的属性，暂未实现
                builder.Append(
                                config.MutilDatabase    //如果是选择了多数据库
                                ? string.Format("    public enum {0}Enum\r\n    {{\r\n", dbName)          //如果是选择的多个数据库则返回这行
                                : string.Format("    public enum TableNames\r\n    {{\r\n", string.Empty)  //如果选择的不是多个数据库则返回这行
                                );

                foreach (KeyValuePair<string, string> pair in tables)//添加表名enum
                {
                    // 处理回车或换行
                    string strDisplayName = pair.Value.Replace("\r\n", "").Replace("\r","").Replace("\n","");//移除回车等换行字符串
                    strDisplayName = strDisplayName == "" ? pair.Key : strDisplayName;

                    builder.AppendFormat("        /// <summary>\r\n");
                    builder.AppendFormat("        /// enum表名：{0}\r\n", strDisplayName);
                    builder.AppendFormat("        /// </summary>\r\n");
                    builder.AppendFormat("        [System.ComponentModel.DataAnnotations.Display(Name = \"{0}\")]\r\n", strDisplayName);
                    builder.AppendFormat("        {0} ,\r\n\r\n", FormatKey(pair.Key));//表名
                }
                //builder[builder.Length - 1] = ' ';
                builder.Remove(builder.Length - 4, 4);
                builder.Append("\r\n    }");//结束表名的enum
                builder.Append("\r\n    #endregion\r\n");//结束







                builder.Append("\r\n\r\n    #region //    单个表枚举（带字段名和属性显示值）\r\n");
                foreach (KeyValuePair<string, string> pair2 in tables)
                {
                    builder.Append(GetFiledEnum(pair2.Key, pair2.Value, config));
                }

                builder.Append("    #endregion\r\n}");//结束


                string str2 = "TableNames.cs";
                if (config.MutilDatabase)//如果是选择的多个数据库则保存为单独文件
                {
                    str2 = dbName + "Enum.cs";
                }
                File.WriteAllText(config.ProjectPath.TrimEnd(new char[] { '/', '\\' }) + @"\" + str2, builder.ToString(), Encoding.Default);
            }
            catch (Exception exception)
            {
                CYQ.Data.Log.WriteLogToTxt(exception);
            }
        }

        internal static void Create(object nameObj)
        {
            int count = 0;
            try
            {
                string str = Convert.ToString(nameObj);
                using (ProjectConfig config = new ProjectConfig())
                {
                    try
                    {
                        if (config.Fill("Name='" + str + "'"))
                        {
                            string dbName = string.Empty;
                            Dictionary<string, string> tables = DBTool.GetTables(config.Conn, out dbName);
                            if ((tables != null) && (tables.Count > 0))
                            {
                                char ch = dbName[0];
                                dbName = ch.ToString().ToUpper() + dbName.Substring(1, dbName.Length - 1);
                                count = tables.Count;
                                if (config.BuildMode.Contains("枚举"))
                                {
                                    BuildTableEnumText(tables, config, dbName);
                                }
                                else
                                {
                                    BuildTableEntityText(tables, config, dbName);
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


            builder.AppendFormat("    #region //    表名：{0}    \t\t备注名：{1}\r\n", tableName, tableName_displayName);
            builder.AppendFormat("    /// <summary>\r\n", string.Empty);
            builder.AppendFormat("    /// enum表名：{0}\r\n", tableName_displayName);
            builder.AppendFormat("    /// </summary>\r\n", string.Empty);
            builder.AppendFormat("    public enum {0}\r\n", FormatKey(tableName));
            builder.AppendFormat("    {{\r\n");
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
                        builder.AppendFormat("        /// enum字段名：{0}\r\n", strDisplayName);
                        builder.AppendFormat("        /// </summary>\r\n", string.Empty);
                        builder.AppendFormat("        [System.ComponentModel.DataAnnotations.Display(Name = \"{0}\")]\r\n", strDisplayName);

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

