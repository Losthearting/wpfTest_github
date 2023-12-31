﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Utils;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;

namespace WPFTest.UI.Chapter3
{
    /// <summary>
    /// C2_SY1.xaml 的交互逻辑
    /// </summary>
    public partial class C3_SY2 : ChildPage
    {
        public C3_SY2()
        {
            m_objectCur = null;

            InitializeComponent();
        }

        public C3_SY2(MainWindow parent)
        {
            m_objectCur = null;

            InitializeComponent();
            this.parentWindow = parent;
        }

        private void ChildPage_Loaded(object sender, RoutedEventArgs e)
        {
            WINDOW_HANDLER = FindWindow(null, "wpfTest");

            HwndSource hWndSource;
            WindowInteropHelper wih = new WindowInteropHelper(this.parentWindow);
            hWndSource = HwndSource.FromHwnd(wih.Handle);
            //添加处理程序 
            hWndSource.AddHook(MainWindowProc);
        }

        //钩子函数，处理所收到的消息
        private IntPtr MainWindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WATCH_FILE:
                    {
                        showComment(strfileinfo);
                        break;
                    }

                case CAPTURE_FILE:
                    {
                        showComment(strfileCapture);
                        break;
                    }

                case CAPTURE_FOLDER:
                    {
                        showPath(strfolderCapture);
                        break;
                    }

                default:
                    {
                        break;
                    }
            }
            return hwnd;
        }

        private void clearComments()
        {
            listBox1.Items.Clear();
 //           textBox2.Text = "";
        }

        private void showComment(String comment)
        {
            if (MyStringUtil.isEmpty(comment)) {
                listBox1.Items.Add("");
                return;
            }

            listBox1.Items.Add(comment);
 //           textBox2.Text = comment;
        }

        private void showPath(String path)
        {
            textBox2.Text = path;
        }

        private void clearPath()
        {
            textBox2.Text = "";
        }

        //定义回调
        private delegate void updateDelegate(string comment);
        public void update(string comment)
        {
            //showComment((string)comment);
            if (!listBox1.Dispatcher.CheckAccess())
            {
                //声明，并实例化回调
                updateDelegate d = update;
                //使用回调
                listBox1.Dispatcher.Invoke(d, comment);
            }
            else
            {
                showComment(comment);
            }
        }
        
        //动态链接库引入
        [DllImport("kernel32.dll")]
        static extern int GetTickCount();

        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(
        IntPtr hWnd, // handle to destination window
        int Msg, // message
        int wParam, // first message parameter
        int lParam // second message parameter
        );

        public static IntPtr WINDOW_HANDLER;
        public static string w_dir;
        public static ManualResetEvent e_wdirth_end;

        public const int WM_USER = 0x400;
        public const int WATCH_FILE = WM_USER + 0x200;
        public const int CAPTURE_FILE = WM_USER + 0x201;
        public const int CAPTURE_FOLDER = WM_USER + 0x202;
        static ManualResetEvent capture_terminate_e;     //结束抓屏线程事件 
        static ManualResetEvent capture_this_one_e;      //抓屏事件
        public static ManualResetEvent[] me_cap = new ManualResetEvent[2];

        public static string strfileCapture;
        public static string strfolderCapture;
        static void Capture_screen(object obj)
        {
            C3_SY2 parent = obj as C3_SY2;

            int s_wid = Screen.PrimaryScreen.Bounds.Width;
            int s_height = Screen.PrimaryScreen.Bounds.Height;
            Bitmap b_1 = new Bitmap(s_wid, s_height);
            Graphics g_ = Graphics.FromImage(b_1);
            String init_dir_fn = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            String dest_fn = null;

            //用事件的方法来抓获图片 
            int index = WaitHandle.WaitAny(me_cap, 500);
            while (index != 0)
            {
                if (index == 1)
                {
                    dest_fn = init_dir_fn;
                    dest_fn += "\\bc\\";
                    if (!Directory.Exists(dest_fn))
                    {
                        Directory.CreateDirectory(dest_fn);
                    }
                    strfolderCapture = dest_fn;
//                    parent.showPath(strfolderCapture);
                    SendMessage(WINDOW_HANDLER, CAPTURE_FOLDER, 0, 0);

                    string fileName = GetTickCount().ToString();
                    fileName += "ab.bmp";
                    dest_fn += fileName;

                    strfileCapture = fileName;
                    SendMessage(WINDOW_HANDLER, CAPTURE_FILE, 0, 0);

                    g_.CopyFromScreen(0, 0, 0, 0, new System.Drawing.Size(s_wid, s_height));
                    b_1.Save(dest_fn, System.Drawing.Imaging.ImageFormat.Bmp);
                    capture_this_one_e.Reset();
                }
                index = WaitHandle.WaitAny(me_cap, 500);
            }
            g_.Dispose();
            b_1.Dispose();
        }

        public static string strfileinfo;
        public static void WatchDir()
        {
            long now_t = DateTime.Now.ToFileTime();
            DirectoryInfo d_info = new DirectoryInfo(w_dir);
            string new_filename;
            FileInfo[] f_ins = d_info.GetFiles();

            //e_wdirth_end有信号，为true，则不用阻塞等待，
            //e_wdirth_end无信号，为false，则需要阻塞等待
            while (!e_wdirth_end.WaitOne(500))   //
            {
                for (int i = 0; i < f_ins.Length; i++)
                {
                    now_t = DateTime.Now.ToFileTime();
                    if (File.Exists(f_ins[i].FullName))
                    {
                        strfileinfo = string.Format("监视到文件{0}\r\n", f_ins[i].FullName);
                        if (WINDOW_HANDLER != IntPtr.Zero)
                                SendMessage(WINDOW_HANDLER, WATCH_FILE, 0, 0);
                        string newDir = w_dir + "\\ref\\";
                        if (!Directory.Exists(newDir))
                        {
                            Directory.CreateDirectory(newDir);
                        }

                        new_filename = newDir + now_t.ToString() + f_ins[i].Name;
                        if (!File.Exists(new_filename))
                        {
                            File.Move(f_ins[i].FullName, new_filename);
                        }
                    }
                }
                f_ins = d_info.GetFiles();//重新获取新的目录信息
            }
        }

        //选择文件目录进行监视
        private void btn1_Click(object sender, RoutedEventArgs e)
        {
            clearComments();
            clearPath();
            btnClick_base(sender, e);

            e_wdirth_end = new ManualResetEvent(false);
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                w_dir = dialog.SelectedPath;
            }

            showPath(w_dir);
        }

        //启动文件目录监视线程
        private void btn2_Click(object sender, RoutedEventArgs e)
        {
            e_wdirth_end.Reset();
            Thread workThread = new Thread(new ThreadStart(WatchDir));
            workThread.IsBackground = true;
            workThread.Start();

            btnClick_base(sender, e);
        }

        //终止文件目录监视线程
        private void btn3_Click(object sender, RoutedEventArgs e)
        {
            e_wdirth_end.Set();

            btnClick_base(sender, e);
        }

        private void btn4_Click(object sender, RoutedEventArgs e)
        {
            clearComments();
            clearPath();

            btnClick_base(sender, e);

            //初始抓屏终止事件为未结束 
            capture_terminate_e = new ManualResetEvent(false);
            //初始捕获终止状态为未结束 
            capture_this_one_e = new ManualResetEvent(false);
            //启动捕捉线程
            me_cap[0] = capture_terminate_e;
            me_cap[1] = capture_this_one_e;
            ParameterizedThreadStart workStart = new ParameterizedThreadStart(Capture_screen);
            Thread workThread = new Thread(workStart);
            workThread.IsBackground = true;
            workThread.Start(this);
        }

        private void btn5_Click(object sender, RoutedEventArgs e)
        {
            capture_this_one_e.Set();

            btnClick_base(sender, e);
        }

        private void btn6_Click(object sender, RoutedEventArgs e)
        {
            clearComments();
            btnClick_base(sender, e);

            capture_terminate_e.Set();
        }
    }
}
