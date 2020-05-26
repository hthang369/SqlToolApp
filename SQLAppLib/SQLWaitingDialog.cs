using DevExpress.XtraWaitForm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SQLAppLib
{
    public partial class SQLWaitingDialog : WaitForm
    {
        //private ManualResetEvent initEvent;
        //private ManualResetEvent abortEvent;
        //private bool requiresClose;
        Image img;
        System.Windows.Forms.Timer time;
        Graphics g;
        int Xscr, Yscr, x, y;
        Rectangle rec;
        Random rand;
        string strStartupPath;
        string[] strFiles;
        const int WM_NCLBUTTONDOWN = 0xA1;
        const int HT_CAPTION = 0x2;
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private void WaitingDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Z)
            {
                int idx = rand.Next(1, strFiles.Count());
                string[] files = strFiles.Where(x => x.StartsWith(strStartupPath + "\\" + idx.ToString().PadLeft(3, '0'))).ToArray();
                if (files.Length > 0)
                    LoadImage(files.First());
            }
        }

        private void WaitingDialog_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
        }
        public SQLWaitingDialog()
        {
            //initEvent = new ManualResetEvent(false);
            //abortEvent = new ManualResetEvent(false);
            InitializeComponent();
            time = new System.Windows.Forms.Timer();
            time.Interval = 100;
            time.Tick += time_Tick;
            x = 0;
            y = 0;
            rand = new Random();
            strStartupPath = Application.StartupPath + "\\Data";
            if (Directory.Exists(strStartupPath))
            {
                strFiles = Directory.GetFiles(strStartupPath);
                int idx = rand.Next(1, strFiles.Count());
                string[] files = strFiles.Where(y => y.StartsWith(strStartupPath + "\\" + idx.ToString().PadLeft(3, '0'))).ToArray();
                if (files.Length > 0)
                    LoadImage(files.First());
            }
            else
            {
                progressBar1.Visible = true;
                pictureBox1.Dock = DockStyle.Fill;
                this.Size = new Size(200, 40);
                pictureBox1.Visible = false;
            }
        }
        //private void AbortWork()
        //{
        //    abortEvent.Set();
        //}
        private void time_Tick(object sender, EventArgs e)
        {
            if (x < (img.Width - Xscr)) x += Xscr;
            else
            {
                x = 0;
                if (y < (img.Height - Yscr)) y += Yscr;
                else y = 0;
            }
            Bitmap map = new Bitmap(Xscr, Yscr);
            g = Graphics.FromImage(map);
            g.DrawImage(img, rec, x, y, Xscr, Yscr, GraphicsUnit.Pixel);
            pictureBox1.Image = map;
        }

        private void LoadImage(string strPath)
        {
            img = Image.FromFile(strPath);
            FileInfo info = new FileInfo(strPath);
            string[] arrSize = info.Name.Replace(info.Extension, "").Split('_');
            Xscr = img.Width / Convert.ToInt32(arrSize[2]);
            Yscr = img.Height / Convert.ToInt32(arrSize[1]);
            Bitmap map = new Bitmap(Xscr, Yscr);
            rec = new Rectangle(0, 0, Xscr, Yscr);
            g = Graphics.FromImage(map);
            g.DrawImage(img, rec, x, y, Xscr, Yscr, GraphicsUnit.Pixel);
            pictureBox1.Size = new Size(Xscr, Yscr);
            this.Size = new Size(Xscr + 10, Yscr + 40);
            pictureBox1.Image = map;
            time.Enabled = true;
        }
        //public void Begin()
        //{
        //    try
        //    {
        //        this.initEvent.WaitOne();
        //        base.Invoke(new MethodInvoker(this.DoBegin));
        //    }
        //    catch (Exception)
        //    {
        //    }
        //}
        //public void End()
        //{
        //    if (this.requiresClose)
        //    {
        //        try
        //        {
        //            base.Invoke(new MethodInvoker(this.DoEnd));
        //        }
        //        catch (Exception)
        //        {
        //        }
        //    }
        //}
        //private void DoEnd()
        //{
        //    base.Close();
        //}
        //private void DoBegin()
        //{
        //}
        //protected override void OnClosing(CancelEventArgs e)
        //{
        //    this.requiresClose = false;
        //    this.AbortWork();
        //    base.OnClosing(e);
        //}
        //protected override void OnLoad(EventArgs e)
        //{
        //    base.OnLoad(e);
        //    this.initEvent.Set();
        //}
    }
}
