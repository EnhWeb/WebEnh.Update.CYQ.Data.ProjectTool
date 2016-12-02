namespace CYQ.Data.ProjectTool
{
    using CYQ.Data.Orm;
    using System;

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

        public ProjectConfig()
        {
            base.SetInit(this, "ProjectConfig", "Txt Path={0};ts=0");
        }

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

