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
                AppendText(sb, "using System;\r\n", new string[0]);
                AppendText(sb, "namespace {0}", new string[] { str2 });
                AppendText(sb, "{", new string[0]);
                if (!string.IsNullOrEmpty(description))
                {
                    description = description.Replace("\r\n","    ").Replace("\r","  ").Replace("\n","  ");
                    AppendText(sb, "    /// <summary>", new string[0]);
                    AppendText(sb, "    /// {0}", new string[] { description });
                    AppendText(sb, "    /// </summary>", new string[0]);
                    AppendText(sb, "    [Display(Name=\"{0}\")]", new string[] { description });
                }
                AppendText(sb, "    public class {0} {1}", new string[] { str + config.EntitySuffix, flag ? "" : ": CYQ.Data.Orm.OrmBase" });
                AppendText(sb, "    {", new string[0]);
                if (!flag)
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
                            AppendText(sb, "        private {0} _{1};", new string[] { FormatType(type.Name, type.IsValueType, config.ValueTypeNullable), name });
                            if (!string.IsNullOrEmpty(struct2.Description))
                            {
                                struct2.Description = struct2.Description.Replace("\r\n", "    ").Replace("\r", "  ").Replace("\n", "  ");
                                AppendText(sb, "        /// <summary>", new string[0]);
                                AppendText(sb, "        /// {0}", new string[] { struct2.Description });
                                AppendText(sb, "        /// </summary>", new string[0]);
                                AppendText(sb, "        [Display(Name=\"{0}\")]", new string[] { struct2.Description });
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
                string str = string.Format(config.NameSpace, dbName).TrimEnd(new char[] { '.' });
                builder.AppendFormat("using System;\r\n\r\nnamespace {0}\r\n{{\r\n", str);
                builder.Append(config.MutilDatabase ? string.Format("    public enum {0}Enum {{", dbName) : "    public enum TableNames {");
                foreach (KeyValuePair<string, string> pair in tables)
                {
                    builder.Append(" " + FormatKey(pair.Key) + " ,");
                }
                builder[builder.Length - 1] = '}';
                builder.Append("\r\n\r\n    #region 枚举 \r\n");
                foreach (KeyValuePair<string, string> pair2 in tables)
                {
                    builder.Append(GetFiledEnum(pair2.Key, config));
                }
                builder.Append("    #endregion\r\n}");
                string str2 = "TableNames.cs";
                if (config.MutilDatabase)
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

        private static string GetFiledEnum(string tableName, ProjectConfig config)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("    public enum " + FormatKey(tableName) + " {");
            try
            {
                MDataColumn columns = DBTool.GetColumns(tableName, config.Conn);
                if (columns.Count > 0)
                {
                    for (int i = 0; i < columns.Count; i++)
                    {
                        string str = FormatKey(columns[i].ColumnName);
                        if (i == 0)
                        {
                            builder.Append(" " + str);
                        }
                        else
                        {
                            builder.Append(", " + str);
                        }
                    }
                    builder.Append(" }\r\n");
                }
                else
                {
                    builder.Append("}\r\n");
                }
            }
            catch (Exception exception)
            {
                CYQ.Data.Log.WriteLogToTxt(exception);
            }
            return builder.ToString();
        }

        internal delegate void CreateEndHandle(int count);
    }
}

