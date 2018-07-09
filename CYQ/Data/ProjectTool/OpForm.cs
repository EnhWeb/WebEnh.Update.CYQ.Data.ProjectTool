﻿namespace CYQ.Data.ProjectTool
{
    using CYQ.Data.Table;
    using CYQ.Data.Tool;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows.Forms;

    public class OpForm : Form
    {
        private Button btnBuild;
        private Button btnOpenFolder;
        private Button btnOpenProjectFolder;
        private Button btnTestConn;
        private CheckBox chbForTwoOnly;
        private CheckBox chbMapName;
        private CheckBox chbMutilDatabase;
        private CheckBox chbValueTypeNullable;
        private IContainer components;
        private ComboBox ddlBuildMode;
        private ComboBox ddlDBType;
        private ComboBox ddlName;
        private GroupBox gbBuild;
        private GroupBox gbConn;
        private bool isIniting;
        private Label lbCodeMode;
        private Label lbConn;
        private Label lbDalType;
        private Label lbDefaultNameSpace;
        private Label lbEntityBean;
        private Label lbForDbName;
        private Label lbName;
        private Label lbSavePath;
        private LinkLabel lnkCopyPath;
        private LinkLabel lnkGotoUrl;
        private LinkLabel lnkOpenFolder;
        private ToolTip toolTip1;
        private TextBox txtConn;
        private TextBox txtEntitySuffix;
        private TextBox txtNameSpace;
        private LinkLabel linkLabel1;
        private GroupBox groupBox1;
        private GroupBox groupBox2;
        private ListView listView1;
        private ColumnHeader columnHeader1;
        private ColumnHeader columnHeader2;
        private Panel panel2;
        private Panel panel1;
        private LinkLabel linkLabel3;
        private LinkLabel linkLabel2;
        private LinkLabel linkLabel4;
        private Button button1;
        private TextBox textBox1;
        private TextBox txtProjectPath;

        public OpForm()
        {
            this.InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            this.DealWithEnglish();
            BuildCSCode.OnCreateEnd += new BuildCSCode.CreateEndHandle(this.BuildCSCode_OnCreateEnd);
        }

        private void btnBuild_Click(object sender, EventArgs e)
        {
            string str = this.txtConn.Text.Trim();
            if (!string.IsNullOrEmpty(str) && DBTool.TestConn(str))
            {
                if (MessageBox.Show("To continue？", "Tip", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    string path = this.txtProjectPath.Text.Trim();
                    if (!Directory.Exists(path))
                    {
                        try
                        {
                            Directory.CreateDirectory(path);
                        }
                        catch (Exception exception)
                        {
                            MessageBox.Show(exception.Message, "Tip");
                            return;
                        }
                    }
                    string connName = this.SaveConfig();
                    string tablenames = this.GetSelectTableNames();

                    System.Collections.Generic.KeyValuePair<string, string> KeyConnName_ValueTableNames = new System.Collections.Generic.KeyValuePair<string, string>(connName,tablenames);

                    this.btnBuild.Enabled = false;
                    new Thread(new ParameterizedThreadStart(BuildCSCode.Create)) { IsBackground = true }.Start(KeyConnName_ValueTableNames);
                }
            }
            else
            {
                MessageBox.Show("Fail！", "Tip");
            }
        }

        private string GetSelectTableNames()
        {
            try
            {
                string tablenames = string.Empty;
                foreach(ListViewItem item in listView1.Items)
                {
                    if (item.Checked)
                    {
                        tablenames += $"{item.Text},";
                    }
                }

                if(string.IsNullOrWhiteSpace(tablenames))
                {
                    MessageBox.Show("当前未选择表，请选择！");
                }

                return tablenames;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"GetSelectTableNames:{ex.Message}");
                return "all";
            }            
        }

        private void btnOpenFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            string str = this.txtProjectPath.Text.Trim();
            if (!string.IsNullOrEmpty(str) && Directory.Exists(str))
            {
                dialog.SelectedPath = str;
            }
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                this.txtProjectPath.Text = dialog.SelectedPath;
            }
        }

        private void btnOpenProjectFolder_Click(object sender, EventArgs e)
        {
            string str = this.txtProjectPath.Text.Trim();
            if (!string.IsNullOrEmpty(str))
            {
                if (!Directory.Exists(str))
                {
                    MessageBox.Show("Directory not Exists :" + str, "Tip");
                }
                else
                {
                    Process.Start(str);
                }
            }
        }

        private void btnTestConn_Click(object sender, EventArgs e)
        {
            this.btnTestConn.Enabled = false;
            new Thread(new ThreadStart(this.TestConn)) { IsBackground = true }.Start();
        }

        private void BuildCSCode_OnCreateEnd(int count)
        {
            this.btnBuild.Enabled = true;
            MessageBox.Show("OK，Total : " + count + " tables", "Tip");
        }

        private void ddlBuildMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedIndex = this.ddlBuildMode.SelectedIndex;
            this.chbMutilDatabase.Visible = this.chbMapName.Visible = this.chbMutilDatabase.Enabled = selectedIndex == 0;
            this.txtEntitySuffix.Visible = this.txtEntitySuffix.Enabled = this.chbForTwoOnly.Visible = this.chbMapName.Visible = this.chbForTwoOnly.Enabled = this.chbValueTypeNullable.Visible = this.chbValueTypeNullable.Enabled = selectedIndex > 0;
            if (selectedIndex == 0)
            {
                this.txtNameSpace.Text = this.txtNameSpace.Text.Replace("Entity", "Enum");
            }
            else
            {
                this.txtNameSpace.Text = this.txtNameSpace.Text.Replace("Enum", "Entity");
            }
        }

        private void ddlName_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.LoadConfig(this.ddlName.Text.Trim());
        }

        private void ddlProvider_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!this.isIniting)
            {
                switch (this.ddlDBType.SelectedIndex)
                {
                    case 0:
                        this.txtConn.Text = "server=.;database=demo;uid=sa;pwd=123456";
                        return;

                    case 1:
                        this.txtConn.Text = "Provider=MSDAORA;Data Source=ip/dbname;User ID=sa;Password=123456";
                        return;

                    case 2:
                        this.txtConn.Text = "host=127.0.0.1;Port=3306;Database=demo;uid=sa;pwd=123456";
                        return;

                    case 3:
                        this.txtConn.Text = "Data Source={0};failifmissing=false";
                        return;

                    case 4:
                        this.txtConn.Text = "Data Source=127.0.0.1;Port=5000;UID=sa;PWD='123456';Database='Demo'";
                        return;

                    case 5:
                        this.txtConn.Text = "Provider=Microsoft.Jet.OLEDB.4.0; Data Source={0}";
                        return;

                    case 6:
                        this.txtConn.Text = "Provider=Microsoft.ACE.OLEDB.12.0; Data Source={0}";
                        return;

                    case 7:
                        this.txtConn.Text = "Txt Path={0}App_Data";
                        return;

                    case 8:
                        this.txtConn.Text = "Xml Path={0}App_Data";
                        return;
                }
            }
        }

        private void DealWithEnglish()
        {
            if (Program.IsEnglish)
            {
                this.Text = this.Text.Replace("配置工具", "Config Tool");
                this.lbCodeMode.Text = "CodeMode";
                this.lbConn.Text = "Connection";
                this.lbDalType.Text = "DB Type";
                this.lbDefaultNameSpace.Text = "NameSpace";
                this.lbEntityBean.Text = "Entity SubFix";
                this.lbName.Text = "Name";
                this.lbForDbName.Text = "{0} - For DataBaseName";
                this.lbSavePath.Text = "Save Path";
                this.btnTestConn.Text = "Test Connect";
                this.btnBuild.Text = "Build Code";
                this.chbMutilDatabase.Text = "Mutil DataBase";
                this.chbForTwoOnly.Text = "For vs2005";
                this.chbMapName.Text = "Map Name";
                this.chbValueTypeNullable.Text = "Nullable";
                this.lnkCopyPath.Text = "CopyPath";
                this.lnkGotoUrl.Text = "Source Url";
                this.lnkOpenFolder.Text = "OpenFolder";
                this.ddlBuildMode.Items.Clear();
                this.ddlBuildMode.Items.Add("Enum for (MAction/MProc)");
                this.ddlBuildMode.Items.Add("Entity for OrmBase");
                this.ddlBuildMode.Items.Add("Entity for DBFast");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitConfig()
        {
            using (ProjectConfig config = new ProjectConfig())
            {
                MDataTable table = config.Select();
                if (table.Rows.Count > 0)
                {
                    foreach (MDataRow row in table.Rows)
                    {
                        this.ddlName.Items.Add(row.Get<string>("Name"));
                        if (row.Get<bool>("IsMain"))
                        {
                            this.ddlName.Text = row.Get<string>("Name");
                        }
                    }
                }
                else
                {
                    this.ddlName.Text = "DefaultConn";
                }
            }
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem(new string[] {
            "table name",
            "table desc"}, -1);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OpForm));
            this.ddlDBType = new System.Windows.Forms.ComboBox();
            this.lbDalType = new System.Windows.Forms.Label();
            this.lbConn = new System.Windows.Forms.Label();
            this.txtConn = new System.Windows.Forms.TextBox();
            this.btnTestConn = new System.Windows.Forms.Button();
            this.ddlName = new System.Windows.Forms.ComboBox();
            this.lbName = new System.Windows.Forms.Label();
            this.chbMutilDatabase = new System.Windows.Forms.CheckBox();
            this.lbSavePath = new System.Windows.Forms.Label();
            this.txtProjectPath = new System.Windows.Forms.TextBox();
            this.gbConn = new System.Windows.Forms.GroupBox();
            this.gbBuild = new System.Windows.Forms.GroupBox();
            this.txtEntitySuffix = new System.Windows.Forms.TextBox();
            this.lbEntityBean = new System.Windows.Forms.Label();
            this.lbForDbName = new System.Windows.Forms.Label();
            this.chbMapName = new System.Windows.Forms.CheckBox();
            this.chbForTwoOnly = new System.Windows.Forms.CheckBox();
            this.chbValueTypeNullable = new System.Windows.Forms.CheckBox();
            this.btnOpenProjectFolder = new System.Windows.Forms.Button();
            this.txtNameSpace = new System.Windows.Forms.TextBox();
            this.btnOpenFolder = new System.Windows.Forms.Button();
            this.lbDefaultNameSpace = new System.Windows.Forms.Label();
            this.ddlBuildMode = new System.Windows.Forms.ComboBox();
            this.lbCodeMode = new System.Windows.Forms.Label();
            this.btnBuild = new System.Windows.Forms.Button();
            this.lnkGotoUrl = new System.Windows.Forms.LinkLabel();
            this.lnkOpenFolder = new System.Windows.Forms.LinkLabel();
            this.lnkCopyPath = new System.Windows.Forms.LinkLabel();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.linkLabel2 = new System.Windows.Forms.LinkLabel();
            this.linkLabel3 = new System.Windows.Forms.LinkLabel();
            this.linkLabel4 = new System.Windows.Forms.LinkLabel();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.gbConn.SuspendLayout();
            this.gbBuild.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // ddlDBType
            // 
            this.ddlDBType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ddlDBType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddlDBType.FormattingEnabled = true;
            this.ddlDBType.Items.AddRange(new object[] {
            "Mssql",
            "Oracle",
            "MySql",
            "SQLite",
            "Sybase",
            "Access/Excel(2003 only)",
            "Access/Excel(2007 above)",
            "Txt",
            "Xml"});
            this.ddlDBType.Location = new System.Drawing.Point(104, 47);
            this.ddlDBType.Name = "ddlDBType";
            this.ddlDBType.Size = new System.Drawing.Size(439, 20);
            this.ddlDBType.TabIndex = 0;
            this.ddlDBType.SelectedIndexChanged += new System.EventHandler(this.ddlProvider_SelectedIndexChanged);
            // 
            // lbDalType
            // 
            this.lbDalType.AutoSize = true;
            this.lbDalType.Location = new System.Drawing.Point(21, 50);
            this.lbDalType.Name = "lbDalType";
            this.lbDalType.Size = new System.Drawing.Size(77, 12);
            this.lbDalType.TabIndex = 1;
            this.lbDalType.Text = "数据库类型：";
            this.lbDalType.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lbConn
            // 
            this.lbConn.AutoSize = true;
            this.lbConn.Location = new System.Drawing.Point(21, 76);
            this.lbConn.Name = "lbConn";
            this.lbConn.Size = new System.Drawing.Size(77, 12);
            this.lbConn.TabIndex = 1;
            this.lbConn.Text = "链接字符串：";
            this.lbConn.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtConn
            // 
            this.txtConn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtConn.Location = new System.Drawing.Point(104, 73);
            this.txtConn.Name = "txtConn";
            this.txtConn.Size = new System.Drawing.Size(603, 21);
            this.txtConn.TabIndex = 2;
            // 
            // btnTestConn
            // 
            this.btnTestConn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnTestConn.Location = new System.Drawing.Point(560, 24);
            this.btnTestConn.Name = "btnTestConn";
            this.btnTestConn.Size = new System.Drawing.Size(133, 23);
            this.btnTestConn.TabIndex = 3;
            this.btnTestConn.Text = "测试链接";
            this.btnTestConn.UseVisualStyleBackColor = true;
            this.btnTestConn.Click += new System.EventHandler(this.btnTestConn_Click);
            // 
            // ddlName
            // 
            this.ddlName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ddlName.FormattingEnabled = true;
            this.ddlName.Location = new System.Drawing.Point(104, 21);
            this.ddlName.Name = "ddlName";
            this.ddlName.Size = new System.Drawing.Size(439, 20);
            this.ddlName.TabIndex = 0;
            this.ddlName.SelectedIndexChanged += new System.EventHandler(this.ddlName_SelectedIndexChanged);
            // 
            // lbName
            // 
            this.lbName.AutoSize = true;
            this.lbName.Location = new System.Drawing.Point(33, 24);
            this.lbName.Name = "lbName";
            this.lbName.Size = new System.Drawing.Size(65, 12);
            this.lbName.TabIndex = 1;
            this.lbName.Text = "配置名称：";
            this.lbName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // chbMutilDatabase
            // 
            this.chbMutilDatabase.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chbMutilDatabase.AutoSize = true;
            this.chbMutilDatabase.Checked = true;
            this.chbMutilDatabase.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chbMutilDatabase.Location = new System.Drawing.Point(520, 24);
            this.chbMutilDatabase.Name = "chbMutilDatabase";
            this.chbMutilDatabase.Size = new System.Drawing.Size(120, 16);
            this.chbMutilDatabase.TabIndex = 4;
            this.chbMutilDatabase.Text = "项目有多个数据库";
            this.chbMutilDatabase.UseVisualStyleBackColor = true;
            // 
            // lbSavePath
            // 
            this.lbSavePath.AutoSize = true;
            this.lbSavePath.Location = new System.Drawing.Point(9, 122);
            this.lbSavePath.Name = "lbSavePath";
            this.lbSavePath.Size = new System.Drawing.Size(89, 12);
            this.lbSavePath.TabIndex = 1;
            this.lbSavePath.Text = "文件保存路径：";
            this.lbSavePath.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtProjectPath
            // 
            this.txtProjectPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtProjectPath.Location = new System.Drawing.Point(104, 119);
            this.txtProjectPath.Name = "txtProjectPath";
            this.txtProjectPath.Size = new System.Drawing.Size(507, 21);
            this.txtProjectPath.TabIndex = 2;
            this.toolTip1.SetToolTip(this.txtProjectPath, "配围起到项目对应存放路径，以自动进行替换");
            // 
            // gbConn
            // 
            this.gbConn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbConn.Controls.Add(this.txtConn);
            this.gbConn.Controls.Add(this.ddlDBType);
            this.gbConn.Controls.Add(this.btnTestConn);
            this.gbConn.Controls.Add(this.ddlName);
            this.gbConn.Controls.Add(this.lbDalType);
            this.gbConn.Controls.Add(this.lbName);
            this.gbConn.Controls.Add(this.lbConn);
            this.gbConn.Location = new System.Drawing.Point(4, 12);
            this.gbConn.Name = "gbConn";
            this.gbConn.Size = new System.Drawing.Size(718, 100);
            this.gbConn.TabIndex = 5;
            this.gbConn.TabStop = false;
            this.gbConn.Text = "数据库 链接配置";
            this.gbConn.Enter += new System.EventHandler(this.gbConn_Enter);
            // 
            // gbBuild
            // 
            this.gbBuild.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbBuild.Controls.Add(this.txtEntitySuffix);
            this.gbBuild.Controls.Add(this.lbEntityBean);
            this.gbBuild.Controls.Add(this.lbForDbName);
            this.gbBuild.Controls.Add(this.chbMapName);
            this.gbBuild.Controls.Add(this.chbForTwoOnly);
            this.gbBuild.Controls.Add(this.chbValueTypeNullable);
            this.gbBuild.Controls.Add(this.btnOpenProjectFolder);
            this.gbBuild.Controls.Add(this.txtNameSpace);
            this.gbBuild.Controls.Add(this.btnOpenFolder);
            this.gbBuild.Controls.Add(this.txtProjectPath);
            this.gbBuild.Controls.Add(this.lbSavePath);
            this.gbBuild.Controls.Add(this.chbMutilDatabase);
            this.gbBuild.Controls.Add(this.lbDefaultNameSpace);
            this.gbBuild.Controls.Add(this.ddlBuildMode);
            this.gbBuild.Controls.Add(this.lbCodeMode);
            this.gbBuild.Location = new System.Drawing.Point(4, 118);
            this.gbBuild.Name = "gbBuild";
            this.gbBuild.Size = new System.Drawing.Size(718, 150);
            this.gbBuild.TabIndex = 6;
            this.gbBuild.TabStop = false;
            this.gbBuild.Text = "生成代码配置";
            this.gbBuild.Enter += new System.EventHandler(this.gbBuild_Enter);
            // 
            // txtEntitySuffix
            // 
            this.txtEntitySuffix.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtEntitySuffix.Location = new System.Drawing.Point(104, 50);
            this.txtEntitySuffix.Name = "txtEntitySuffix";
            this.txtEntitySuffix.Size = new System.Drawing.Size(298, 21);
            this.txtEntitySuffix.TabIndex = 11;
            this.txtEntitySuffix.Text = "Bean";
            // 
            // lbEntityBean
            // 
            this.lbEntityBean.AutoSize = true;
            this.lbEntityBean.Location = new System.Drawing.Point(21, 53);
            this.lbEntityBean.Name = "lbEntityBean";
            this.lbEntityBean.Size = new System.Drawing.Size(77, 12);
            this.lbEntityBean.TabIndex = 10;
            this.lbEntityBean.Text = "实体类后缀：";
            this.lbEntityBean.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lbForDbName
            // 
            this.lbForDbName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lbForDbName.AutoSize = true;
            this.lbForDbName.Location = new System.Drawing.Point(515, 88);
            this.lbForDbName.Name = "lbForDbName";
            this.lbForDbName.Size = new System.Drawing.Size(125, 12);
            this.lbForDbName.TabIndex = 9;
            this.lbForDbName.Text = "{0} - 代表数据库名称";
            // 
            // chbMapName
            // 
            this.chbMapName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chbMapName.AutoSize = true;
            this.chbMapName.Checked = true;
            this.chbMapName.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chbMapName.Location = new System.Drawing.Point(612, 55);
            this.chbMapName.Name = "chbMapName";
            this.chbMapName.Size = new System.Drawing.Size(102, 16);
            this.chbMapName.TabIndex = 8;
            this.chbMapName.Text = "去除‘_’符号";
            this.toolTip1.SetToolTip(this.chbMapName, "按Pascal大小写规范格式化");
            this.chbMapName.UseVisualStyleBackColor = true;
            this.chbMapName.Visible = false;
            // 
            // chbForTwoOnly
            // 
            this.chbForTwoOnly.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chbForTwoOnly.AutoSize = true;
            this.chbForTwoOnly.Checked = true;
            this.chbForTwoOnly.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chbForTwoOnly.Location = new System.Drawing.Point(519, 55);
            this.chbForTwoOnly.Name = "chbForTwoOnly";
            this.chbForTwoOnly.Size = new System.Drawing.Size(84, 16);
            this.chbForTwoOnly.TabIndex = 8;
            this.chbForTwoOnly.Text = "兼容vs2005";
            this.chbForTwoOnly.UseVisualStyleBackColor = true;
            this.chbForTwoOnly.Visible = false;
            // 
            // chbValueTypeNullable
            // 
            this.chbValueTypeNullable.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chbValueTypeNullable.AutoSize = true;
            this.chbValueTypeNullable.Location = new System.Drawing.Point(408, 55);
            this.chbValueTypeNullable.Name = "chbValueTypeNullable";
            this.chbValueTypeNullable.Size = new System.Drawing.Size(96, 16);
            this.chbValueTypeNullable.TabIndex = 8;
            this.chbValueTypeNullable.Text = "值类型可Null";
            this.chbValueTypeNullable.UseVisualStyleBackColor = true;
            this.chbValueTypeNullable.Visible = false;
            // 
            // btnOpenProjectFolder
            // 
            this.btnOpenProjectFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOpenProjectFolder.Location = new System.Drawing.Point(667, 118);
            this.btnOpenProjectFolder.Name = "btnOpenProjectFolder";
            this.btnOpenProjectFolder.Size = new System.Drawing.Size(45, 23);
            this.btnOpenProjectFolder.TabIndex = 7;
            this.btnOpenProjectFolder.Text = "open";
            this.btnOpenProjectFolder.UseVisualStyleBackColor = true;
            this.btnOpenProjectFolder.Click += new System.EventHandler(this.btnOpenProjectFolder_Click);
            // 
            // txtNameSpace
            // 
            this.txtNameSpace.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtNameSpace.Location = new System.Drawing.Point(104, 85);
            this.txtNameSpace.Name = "txtNameSpace";
            this.txtNameSpace.Size = new System.Drawing.Size(400, 21);
            this.txtNameSpace.TabIndex = 2;
            this.txtNameSpace.Text = "Web.Enums.{0}";
            // 
            // btnOpenFolder
            // 
            this.btnOpenFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOpenFolder.Location = new System.Drawing.Point(618, 119);
            this.btnOpenFolder.Name = "btnOpenFolder";
            this.btnOpenFolder.Size = new System.Drawing.Size(42, 23);
            this.btnOpenFolder.TabIndex = 6;
            this.btnOpenFolder.Text = "...";
            this.btnOpenFolder.UseVisualStyleBackColor = true;
            this.btnOpenFolder.Click += new System.EventHandler(this.btnOpenFolder_Click);
            // 
            // lbDefaultNameSpace
            // 
            this.lbDefaultNameSpace.AutoSize = true;
            this.lbDefaultNameSpace.Location = new System.Drawing.Point(9, 88);
            this.lbDefaultNameSpace.Name = "lbDefaultNameSpace";
            this.lbDefaultNameSpace.Size = new System.Drawing.Size(89, 12);
            this.lbDefaultNameSpace.TabIndex = 1;
            this.lbDefaultNameSpace.Text = "默认名称空间：";
            this.lbDefaultNameSpace.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // ddlBuildMode
            // 
            this.ddlBuildMode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ddlBuildMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddlBuildMode.FormattingEnabled = true;
            this.ddlBuildMode.Items.AddRange(new object[] {
            "枚举型（MAction/MProc）- 推荐",
            "实体型（ORM操作方式）",
            "纯实体类"});
            this.ddlBuildMode.Location = new System.Drawing.Point(104, 20);
            this.ddlBuildMode.Name = "ddlBuildMode";
            this.ddlBuildMode.Size = new System.Drawing.Size(400, 20);
            this.ddlBuildMode.TabIndex = 0;
            this.ddlBuildMode.SelectedIndexChanged += new System.EventHandler(this.ddlBuildMode_SelectedIndexChanged);
            // 
            // lbCodeMode
            // 
            this.lbCodeMode.AutoSize = true;
            this.lbCodeMode.Location = new System.Drawing.Point(33, 23);
            this.lbCodeMode.Name = "lbCodeMode";
            this.lbCodeMode.Size = new System.Drawing.Size(65, 12);
            this.lbCodeMode.TabIndex = 1;
            this.lbCodeMode.Text = "编码模式：";
            this.lbCodeMode.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnBuild
            // 
            this.btnBuild.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBuild.Location = new System.Drawing.Point(107, 589);
            this.btnBuild.Name = "btnBuild";
            this.btnBuild.Size = new System.Drawing.Size(507, 38);
            this.btnBuild.TabIndex = 5;
            this.btnBuild.Text = "生成文件";
            this.btnBuild.UseVisualStyleBackColor = true;
            this.btnBuild.Click += new System.EventHandler(this.btnBuild_Click);
            // 
            // lnkGotoUrl
            // 
            this.lnkGotoUrl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lnkGotoUrl.AutoSize = true;
            this.lnkGotoUrl.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.lnkGotoUrl.LinkColor = System.Drawing.Color.Red;
            this.lnkGotoUrl.Location = new System.Drawing.Point(637, 641);
            this.lnkGotoUrl.Name = "lnkGotoUrl";
            this.lnkGotoUrl.Size = new System.Drawing.Size(77, 12);
            this.lnkGotoUrl.TabIndex = 7;
            this.lnkGotoUrl.TabStop = true;
            this.lnkGotoUrl.Text = "源码下载地址";
            this.lnkGotoUrl.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkGotoUrl_LinkClicked);
            // 
            // lnkOpenFolder
            // 
            this.lnkOpenFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lnkOpenFolder.AutoSize = true;
            this.lnkOpenFolder.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.lnkOpenFolder.LinkColor = System.Drawing.Color.Blue;
            this.lnkOpenFolder.Location = new System.Drawing.Point(554, 641);
            this.lnkOpenFolder.Name = "lnkOpenFolder";
            this.lnkOpenFolder.Size = new System.Drawing.Size(77, 12);
            this.lnkOpenFolder.TabIndex = 7;
            this.lnkOpenFolder.TabStop = true;
            this.lnkOpenFolder.Text = "打开软件目录";
            this.lnkOpenFolder.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkOpenFolder_LinkClicked);
            // 
            // lnkCopyPath
            // 
            this.lnkCopyPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lnkCopyPath.AutoSize = true;
            this.lnkCopyPath.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.lnkCopyPath.LinkColor = System.Drawing.Color.BlueViolet;
            this.lnkCopyPath.Location = new System.Drawing.Point(469, 641);
            this.lnkCopyPath.Name = "lnkCopyPath";
            this.lnkCopyPath.Size = new System.Drawing.Size(77, 12);
            this.lnkCopyPath.TabIndex = 8;
            this.lnkCopyPath.TabStop = true;
            this.lnkCopyPath.Text = "复制完整路径";
            this.lnkCopyPath.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkCopyPath_LinkClicked);
            // 
            // linkLabel1
            // 
            this.linkLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.linkLabel1.LinkColor = System.Drawing.Color.Red;
            this.linkLabel1.Location = new System.Drawing.Point(17, 641);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(149, 12);
            this.linkLabel1.TabIndex = 7;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "WebEnh升级版源码下载地址";
            this.toolTip1.SetToolTip(this.linkLabel1, "支持[Display(Name=\"字段名\")]");
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Location = new System.Drawing.Point(-8, 632);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(747, 1);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "groupBox1";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.panel2);
            this.groupBox2.Controls.Add(this.panel1);
            this.groupBox2.Location = new System.Drawing.Point(4, 274);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(718, 309);
            this.groupBox2.TabIndex = 10;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "表选择";
            // 
            // listView1
            // 
            this.listView1.CheckBoxes = true;
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView1.HideSelection = false;
            this.listView1.ImeMode = System.Windows.Forms.ImeMode.Disable;
            listViewItem1.StateImageIndex = 0;
            this.listView1.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1});
            this.listView1.LabelWrap = false;
            this.listView1.Location = new System.Drawing.Point(0, 0);
            this.listView1.Name = "listView1";
            this.listView1.ShowGroups = false;
            this.listView1.Size = new System.Drawing.Size(712, 251);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "表名称";
            this.columnHeader1.Width = 300;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "表描述";
            this.columnHeader2.Width = 370;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.button1);
            this.panel1.Controls.Add(this.textBox1);
            this.panel1.Controls.Add(this.linkLabel4);
            this.panel1.Controls.Add(this.linkLabel3);
            this.panel1.Controls.Add(this.linkLabel2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(3, 17);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(712, 38);
            this.panel1.TabIndex = 1;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.listView1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(3, 55);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(712, 251);
            this.panel2.TabIndex = 2;
            // 
            // linkLabel2
            // 
            this.linkLabel2.AutoSize = true;
            this.linkLabel2.Location = new System.Drawing.Point(579, 12);
            this.linkLabel2.Name = "linkLabel2";
            this.linkLabel2.Size = new System.Drawing.Size(29, 12);
            this.linkLabel2.TabIndex = 0;
            this.linkLabel2.TabStop = true;
            this.linkLabel2.Text = "全选";
            this.linkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel2_LinkClicked);
            // 
            // linkLabel3
            // 
            this.linkLabel3.AutoSize = true;
            this.linkLabel3.Location = new System.Drawing.Point(614, 12);
            this.linkLabel3.Name = "linkLabel3";
            this.linkLabel3.Size = new System.Drawing.Size(41, 12);
            this.linkLabel3.TabIndex = 1;
            this.linkLabel3.TabStop = true;
            this.linkLabel3.Text = "全不选";
            this.linkLabel3.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel3_LinkClicked);
            // 
            // linkLabel4
            // 
            this.linkLabel4.AutoSize = true;
            this.linkLabel4.Location = new System.Drawing.Point(661, 12);
            this.linkLabel4.Name = "linkLabel4";
            this.linkLabel4.Size = new System.Drawing.Size(29, 12);
            this.linkLabel4.TabIndex = 2;
            this.linkLabel4.TabStop = true;
            this.linkLabel4.Text = "反选";
            this.linkLabel4.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel4_LinkClicked);
            // 
            // textBox1
            // 
            this.textBox1.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.textBox1.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.textBox1.HideSelection = false;
            this.textBox1.Location = new System.Drawing.Point(8, 9);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(246, 21);
            this.textBox1.TabIndex = 3;
            this.toolTip1.SetToolTip(this.textBox1, "用来查找指定表，可通过表名称和表描术来查找。");
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(260, 9);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(34, 21);
            this.button1.TabIndex = 4;
            this.button1.Text = "搜";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // OpForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(726, 662);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.lnkCopyPath);
            this.Controls.Add(this.lnkOpenFolder);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.lnkGotoUrl);
            this.Controls.Add(this.gbBuild);
            this.Controls.Add(this.gbConn);
            this.Controls.Add(this.btnBuild);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(600, 388);
            this.Name = "OpForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CYQ.Data 配置工具 V2.0     WebEnh.Update  内部版本号：V1.9（2017-06-23）";
            this.Load += new System.EventHandler(this.OpForm_Load);
            this.gbConn.ResumeLayout(false);
            this.gbConn.PerformLayout();
            this.gbBuild.ResumeLayout(false);
            this.gbBuild.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void lnkCopyPath_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Clipboard.SetText(Application.ExecutablePath);
            MessageBox.Show("Copy Path OK!", "Tip");
        }

        private void lnkGotoUrl_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            StartHttp("http://www.cyqdata.com/download/article-detail-426");
        }

        private void lnkOpenFolder_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(AppDomain.CurrentDomain.BaseDirectory);
        }

        private string currectConn = "";

        private void LoadConfig(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                using (ProjectConfig config = new ProjectConfig())
                {
                    if (config.Fill("Name='" + name + "'"))
                    {
                        config.UI.SetToAll(this, new object[0]);

                        currectConn = config.Conn;

                        this.LoadTables(config);
                    }
                }
            }
        }

        private void LoadTables(ProjectConfig config = null)
        {
            // 读取表名
            string dbName = string.Empty;
            string errInfo = string.Empty;
            try
            {
                string conn = config == null ? currectConn : config.Conn;
                var tables = CYQ.Data.Tool.DBTool.GetTables(conn, out dbName, out errInfo);

                if (!string.IsNullOrWhiteSpace(errInfo))
                {
                    throw new Exception(errInfo);
                }

                listView1.Items.Clear();

                textBox1.AutoCompleteCustomSource.Clear();
                foreach (var item in tables)
                {
                    textBox1.AutoCompleteCustomSource.Add(item.Key);
                    if (!string.IsNullOrWhiteSpace(item.Value))
                        textBox1.AutoCompleteCustomSource.Add(item.Value);

                    listView1.Items.Add(new ListViewItem(new[] { item.Key, item.Value }) { Checked = true });
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"读取表时出错：{ex.Message}");
            }
        }

        private void OpForm_Load(object sender, EventArgs e)
        {
            //版本号自动生成
            this.Text = $"CYQ.Data 配置工具 V2.0  -  WebEnh.Update 枚举实体生成工具 InsideVersion：{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}（2017-06-23）";
            //CYQ.Data 配置工具 V2.0     WebEnh.Update  内部版本号：V1.9（2017-06-23）

            this.isIniting = true;
            this.ddlDBType.SelectedIndex = 0;
            this.ddlBuildMode.SelectedIndex = 0;
            this.InitConfig();
            if (string.IsNullOrEmpty(this.txtProjectPath.Text))
            {
                this.txtProjectPath.Text = Program.path+"生成\\";
            }
            this.isIniting = false;
        }

        private void ResetMainState()
        {
            MDataTable table = null;
            using (ProjectConfig config = new ProjectConfig())
            {
                table = config.Select();
            }
            if (table.Rows.Count > 0)
            {
                foreach (MDataRow row in table.Rows)
                {
                    row.Set("IsMain", false);
                }
                table.AcceptChanges(AcceptOp.Update);
            }
        }

        private string SaveConfig()
        {
            string str = this.ddlName.Text.Trim();
            if (string.IsNullOrEmpty(str))
            {
                str = "DefaultConn";
            }
            this.ResetMainState();
            bool flag = false;
            using (ProjectConfig config = new ProjectConfig())
            {
                config.UI.SetAutoParentControl(this.gbConn, new object[] { this.gbBuild });
                if (config.Fill("Name='" + str + "'"))
                {
                    config.IsMain = true;
                    flag = config.Update(null, true);
                    bool mutilDatabase = config.MutilDatabase;
                }
                else
                {
                    config.IsMain = true;
                    if (config.Insert(true))
                    {
                        this.ddlName.Items.Add(str);
                        flag = true;
                    }
                }
            }
            if (!flag)
            {
                MessageBox.Show("Save fail", "Tip");
            }
            return str;
        }

        [DllImport("shell32.dll")]
        private static extern IntPtr ShellExecute(IntPtr hwnd, string lpOperation, string lpFile, string lpParameters, string lpDirectory, int nShowCmd);
        protected static void StartHttp(string url)
        {
            try
            {
                ShellExecute(IntPtr.Zero, "open", url, "", "", 4);
            }
            catch
            {
                Process.Start("IEXPLORE.EXE", url);
            }
        }

        private void TestConn()
        {
            try
            {
                string conn = this.txtConn.Text.Trim();
                string msg = string.Empty;
                if (DBTool.TestConn(conn, out msg))
                {
                    this.SaveConfig();
                    MessageBox.Show("OK!", "Tip");
                }
                else
                {
                    MessageBox.Show("Fail：" + msg, "Tip");
                }
            }
            catch
            {
            }
            finally
            {
                this.btnTestConn.Enabled = true;
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            StartHttp("https://github.com/EnhWeb/WebEnh.Update.CYQ.Data.ProjectTool.git");
        }

        private void gbConn_Enter(object sender, EventArgs e)
        {

        }

        private void gbBuild_Enter(object sender, EventArgs e)
        {

        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            foreach (ListViewItem item in listView1.Items)
            {
                item.Checked = true;
            }
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            foreach (ListViewItem item in listView1.Items)
            {
                item.Checked = false;
            }
        }

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            foreach (ListViewItem item in listView1.Items)
            {
                item.Checked = !item.Checked;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.LoadTables();

            foreach (ListViewItem item in listView1.Items)
            {
                if (item.Text.ToLower().Trim().Contains(textBox1.Text.ToLower().Trim()))
                {
                    
                }
                else
                {
                    item.Remove();
                }
            }
        }
    }
}

