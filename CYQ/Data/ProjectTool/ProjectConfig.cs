namespace CYQ.Data.ProjectTool
{
    using CYQ.Data.Orm;
    using System;

    /// <summary>
    /// ProjectConfig ORM 实体
    /// </summary>
    public class ProjectConfig : OrmBase
    {
        private string _BuildMode;
        private string _Conn;
        private string _DBType;
        private string _EntitySuffix;
        private bool _ForTwoOnly;
        private int _ID;
        private bool _IsMain;
        private bool _MapName;
        private bool _MutilDatabase;
        private string _Name;
        private string _NameSpace;
        private string _ProjectPath;
        private bool _ValueTypeNullable;

        /// <summary>
        /// 构造函数
        /// </summary>
        public ProjectConfig()
        {
            base.SetInit(this, "ProjectConfig", "Txt Path={0};ts=0");
        }

        /// <summary>
        /// 编译器模式
        /// </summary>
        public string BuildMode
        {
            get
            {
                return this._BuildMode;
            }
            set
            {
                this._BuildMode = value;
            }
        }

        /// <summary>
        /// 链接名称
        /// </summary>
        public string Conn
        {
            get
            {
                return this._Conn;
            }
            set
            {
                this._Conn = value;
            }
        }

        /// <summary>
        /// 数据库类别
        /// </summary>
        public string DBType
        {
            get
            {
                return this._DBType;
            }
            set
            {
                this._DBType = value;
            }
        }

        /// <summary>
        /// 实体后缀名称
        /// </summary>
        public string EntitySuffix
        {
            get
            {
                return this._EntitySuffix;
            }
            set
            {
                this._EntitySuffix = value;
            }
        }

        /// <summary>
        /// ForTwoOnly
        /// </summary>
        public bool ForTwoOnly
        {
            get
            {
                return this._ForTwoOnly;
            }
            set
            {
                this._ForTwoOnly = value;
            }
        }

        /// <summary>
        /// 主键ID
        /// </summary>
        public int ID
        {
            get
            {
                return this._ID;
            }
            set
            {
                this._ID = value;
            }
        }

        /// <summary>
        /// IsMain
        /// </summary>
        public bool IsMain
        {
            get
            {
                return this._IsMain;
            }
            set
            {
                this._IsMain = value;
            }
        }

        /// <summary>
        /// MapName
        /// </summary>
        public bool MapName
        {
            get
            {
                return this._MapName;
            }
            set
            {
                this._MapName = value;
            }
        }

        /// <summary>
        /// 是否多数据库
        /// </summary>
        public bool MutilDatabase
        {
            get
            {
                return this._MutilDatabase;
            }
            set
            {
                this._MutilDatabase = value;
            }
        }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get
            {
                return this._Name;
            }
            set
            {
                this._Name = value;
            }
        }

        /// <summary>
        /// 命名空间名称
        /// </summary>
        public string NameSpace
        {
            get
            {
                return this._NameSpace;
            }
            set
            {
                this._NameSpace = value;
            }
        }

        /// <summary>
        /// 生成保存到的项目路径
        /// </summary>
        public string ProjectPath
        {
            get
            {
                return this._ProjectPath;
            }
            set
            {
                this._ProjectPath = value;
            }
        }

        /// <summary>
        /// 值类型是否可为空
        /// </summary>
        public bool ValueTypeNullable
        {
            get
            {
                return this._ValueTypeNullable;
            }
            set
            {
                this._ValueTypeNullable = value;
            }
        }
    }
}

