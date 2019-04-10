namespace CYQ.Data.ProjectTool
{
    using System;
    using System.Threading;
    using System.Windows.Forms;

    internal static class Program
    {
        private static int _IsEnglish;
        internal static string path = string.Empty;

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Exception exception = e.Exception;
            MessageBox.Show("UnhandledException ：" + exception.Message, "System Tip!");
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Application_ThreadException(sender, null);
        }

        [STAThread]
        private static void Main(string[] para)
        {
            AppDomain.CurrentDomain.AppendPrivatePath(Application.StartupPath + @"\Libs\");

            if (para.Length > 0)
            {
                path = para[0].TrimEnd(new char[] { '"' });
            }
            else
            {
                path = AppDomain.CurrentDomain.BaseDirectory;
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += new ThreadExceptionEventHandler(Program.Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(Program.CurrentDomain_UnhandledException);
            Application.Run(new OpForm());
        }

        //public static (string aaa,string bbb) test111()
        //{
        //    return
        //}


        //public static System.Tuples.Tuple<string,int,string> test111()
        //{
        //    return new System.Tuples.Tuple<string, int, string>() { };
        //}

        public static bool IsEnglish
        {
            get
            {
                if (_IsEnglish == -1)
                {
                    if (Thread.CurrentThread.CurrentCulture.Name.StartsWith("zh-"))
                    {
                        _IsEnglish = 0;
                    }
                    else
                    {
                        _IsEnglish = 1;
                    }
                }
                return (_IsEnglish == 1);
            }
        }
    }
}

