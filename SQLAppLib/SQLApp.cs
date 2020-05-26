using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using DevExpress.XtraSplashScreen;
using System.Diagnostics;
using System.Management.Automation.Runspaces;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace SQLAppLib
{
    enum AnimateWindowFlag : uint
    {
        AW_HIDE = 0x00010000,
        AW_ACTIVATE = 0x00020000,
        AW_HOR_POSITIVE = 0x00000001,
        AW_HOR_NEGATIVE = 0x00000002,
        AW_VER_POSITIVE = 0x00000004,
        AW_VER_NEGATIVE = 0x00000008,
        AW_SLIDE = 0x00040000,
        AW_BLEND = 0x00080000,
        AW_CENTER = 0x00000010
    }
    public class SQLApp
    {
        private static string _currentDatabaseName;
        public static string CurrentDatabaseName
        {
            get
            {
                _currentDatabaseName = SQLDBUtil.GetCurrentDatabaseName();
                return _currentDatabaseName;
            }
        }

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filepath);
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern long GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filepath);
        [DllImport("kernel32", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Auto)]
        private static extern int GetPrivateProfileSection(string section, string lpszReturnBuffer, int nSize, string filepath);
        [DllImport("user32.dll")]
        private static extern int AnimateWindow(IntPtr hwand, int dwTime, AnimateWindowFlag dwFlags);
        public static string GetIniFile(string szFile, string szSection, string szKey)
        {
            StringBuilder tmp = new StringBuilder(255);
            long i = GetPrivateProfileString(szSection, szKey, "", tmp, 255, szFile);
            return tmp.ToString();
        }
        public static void SetIniFile(string szFile, string szSection, string szKey, string szData)
        {
            WritePrivateProfileString(szSection, szKey, szData, szFile);
        }
        public static List<string> GetKeysIniFile(string szFile, string szSection, int bufferSize = 1000)
        {
            string value = new string(' ', bufferSize);
            int uiNumCharCopied = GetPrivateProfileSection(szSection, value, bufferSize, szFile);
            List<string> lstOutputs = new List<string>();
            lstOutputs = value.Split('\0').Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.TrimEnd(Convert.ToChar(0)).Split('=').FirstOrDefault()).ToList();
            return lstOutputs;
        }
        public static bool IsNumber(string pText)
        {
            Regex regex = new Regex(@"^[-+]?[0-9]*\.?[0-9]+$");
            return regex.IsMatch(pText);
        }
        public static string GetFile(string strFile)
        {
            string Content = "";
            try
            {
                if (File.Exists(strFile))
                {
                    Content = File.ReadAllText(strFile);
                }
            }
            catch (Exception ex) { }
            return Content;
        }
        public static void WriteFile(string strFile, string strContent)
        {
            try
            {
                File.WriteAllText(strFile, strContent, Encoding.UTF8);
            }
            catch (Exception ex) { }
        }
        public static string GetTableNamePrefix(string strTableName)
        {
            return strTableName.Substring(0, strTableName.Length - 1);
        }
        public static bool CheckColumnByDataGridView(DataGridView dgvView, string strCol)
        {
            return dgvView.Columns.Cast<DataGridViewColumn>().Where(x => x.Name.Contains(strCol)).Count() != 0;
        }
        public static void ShowNotifyIcon(string title, string strContent)
        {
            NotifyIcon notifyIcon = new NotifyIcon();
            notifyIcon.Visible = true;
            if (!string.IsNullOrEmpty(title))
                notifyIcon.BalloonTipTitle = title;
            if (!string.IsNullOrEmpty(strContent))
                notifyIcon.BalloonTipText = strContent;
            notifyIcon.ShowBalloonTip(500);
        }
        public static void ShowAnimate(IntPtr hwand, int dwTime)
        {
            AnimateWindow(hwand, dwTime, AnimateWindowFlag.AW_HOR_POSITIVE | AnimateWindowFlag.AW_CENTER | AnimateWindowFlag.AW_BLEND);
        }
        public static void HideAnimate(IntPtr hwand, int dwTime)
        {
            AnimateWindow(hwand, dwTime, AnimateWindowFlag.AW_VER_NEGATIVE | AnimateWindowFlag.AW_BLEND | AnimateWindowFlag.AW_HIDE);
        }
        public static object GetObjectFromDataRow(DataRow row, object type)
        {
            foreach (DataColumn column in row.Table.Columns)
            {
                if (row[column] != DBNull.Value)
                {
                    PropertyInfo property = type.GetType().GetProperty(column.ColumnName);
                    property.SetValue(type, row[column], null);
                }
            }
            return type;
        }
        public static void SetFormTitle(Form frm, string strTitle = "")
        {
            string strTitleOld = frm.Text;
            string strNewTitle = strTitle;
            if (string.IsNullOrEmpty(strTitle)) strNewTitle = string.Format("{0} - {1}", strTitleOld, Application.ProductVersion);
            frm.Text = string.Format("{0} (Server: {1} - DB: {2})", strNewTitle, SQLDBUtil._strServer, CurrentDatabaseName);
        }
        public static void SetPropertyValue(object obj, string strPropertyName, object value)
        {
            PropertyInfo property = obj.GetType().GetProperty(strPropertyName);
            if (property != null)
            {
                property.SetValue(obj, value, null);
            }
        }
        public static object GetPropertyValue(object obj, string strPropertyName)
        {
            if (obj is DataRow)
                return (obj as DataRow).GetFieldName(strPropertyName);
            else if (obj is DataRowView)
                return (obj as DataRowView).Row.GetFieldName(strPropertyName);
            else
            {
                PropertyInfo property = obj.GetType().GetProperty(strPropertyName);
                if (property != null)
                {
                    return property.GetValue(obj, null);
                }
            }
            return null;
        }
        public static string ExecutedCommandLine(string strCmdLine)
        {
            Process pro = new Process();
            pro.StartInfo.FileName = "cmd.exe";
            pro.StartInfo.Arguments = strCmdLine;
            pro.StartInfo.UseShellExecute = false;
            pro.StartInfo.CreateNoWindow = true;
            pro.StartInfo.RedirectStandardOutput = true;
            pro.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            pro.Start();
            return pro.StandardOutput.ReadToEnd();
        }
        public static string ExecutedPowerShell(string strCmdLine)
        {
            Runspace runspace = RunspaceFactory.CreateRunspace();
            runspace.Open();
            Pipeline pipeline = runspace.CreatePipeline();
            pipeline.Commands.AddScript(strCmdLine);
            pipeline.Commands.Add("Out-String");
            
            Collection<PSObject> results = pipeline.Invoke();
            runspace.Close();
            StringBuilder builder = new StringBuilder();
            results.ToList().ForEach(x => builder.AppendLine(x.ToString()));
            return builder.ToString();
        }
    }

    public class SQLQuery
    {
        private static readonly string strSelect = "SELECT";
        private static readonly string strFrom = "FROM";
        private static readonly string strWhere = "WHERE";
        private static readonly string strHaving = "HAVING";
        private static readonly string strJoin = "JOIN";
        private static readonly string strLeftJoin = "LEFT JOIN";
        private static readonly string strRightJoin = "RIGHT JOIN";
        private static readonly string strInnerJoin = "INNER JOIN";
        private static readonly string strGroupBy = "GROUP BY";
        private static readonly string strOrderBy = "ORDER BY";
        private static readonly string strOn = "ON";
        private static readonly string strAnd = "AND";
        private static readonly string strOr = "OR";
        private static readonly string strLike = "LIKE";
        private static readonly string strEquals = "=";

        public static string GenerateQuery(string strQuery, string strFillter = "")
        {
            string strNewQuery = string.Empty;
            if (strFillter.Length > 0) strQuery = strQuery.RegexString(strFillter);
            strQuery = strQuery.RegexString(strWhere);
            string strOperator = strWhere;
            if (strQuery.Contains(strWhere)) strOperator = strAnd;
            strNewQuery = string.Format("{0} {1} {2}", strQuery.RegexString(strFillter), strOperator, strFillter);
            return strNewQuery;
        }
        public static string GenerateQuery(string strQuery, Dictionary<string, string> lstFillters)
        {
            if (lstFillters.Count == 0) return GenerateQuery(strQuery);
            return GenerateQuery(strQuery, GenerateFillter(lstFillters));
        }
        public static Dictionary<string, string> GenerateFillter(Dictionary<DataGridViewColumn, string> lstFillters)
        {
            Dictionary<string, string> dicNewFillter = new Dictionary<string, string>();
            string strOperater = string.Empty;
            string strValue = string.Empty;
            if (lstFillters == null || lstFillters.Count == 0) return dicNewFillter;
            foreach (KeyValuePair<DataGridViewColumn, string> item in lstFillters)
            {
                if (item.Key.ValueType == typeof(string))
                {
                    strOperater = strLike;
                    strValue = string.Format("'{0}%'", item.Value.RegexString(strLike));
                }
                else
                {
                    strOperater = strEquals;
                    strValue = item.Value.RegexString(strEquals);
                }
                dicNewFillter.AddItem(item.Key.Name, strOperater + " " + strValue);
            }
            return dicNewFillter;
        }
        public static string GenerateFillter(Dictionary<string, string> lstFillters)
        {
            if (lstFillters == null || lstFillters.Count == 0) return string.Empty;
            List<string> lstNewFillter = new List<string>();
            foreach (KeyValuePair<string, string> item in lstFillters)
            {
                string strValue = item.Value.Replace("False", "0").Replace("True", "1").Replace("false", "0").Replace("true", "1");
                lstNewFillter.AddItem(string.Format("{0} {1}", item.Key, strValue));
            }
            return string.Join(" " + strAnd + " ", lstNewFillter.ToArray());
        }
        private static string GenQuerySelect(string strQuery)
        {
            return "";
        }
    }
    public static class StringExtensions
    {
        public static string SearchRegex(this string[] name, string regex)
        {
            if (name == null) return string.Empty;
            var objStr = name.Where(x => x.Contains(regex)).Select(x => x.ToString());
            if (objStr == null || objStr.ToList().Count == 0) return string.Empty;
            return objStr.First();
        }
        public static string RegexString(this string fieldName, string strRegex = @"(\d)")
        {
            Regex myregex = new Regex(strRegex);
            return myregex.Replace(fieldName, "");
        }
    }
    public static class CollectionExtensions
    {
        public static void AddItem<TKey, TVal>(this Dictionary<TKey, TVal> dic, TKey key, TVal val)
        {
            if (dic.ContainsKey(key))
                dic[key] = val;
            else
                dic.Add(key, val);
        }
        public static void AddItem<T>(this List<T> lst, T obj)
        {
            if (!lst.Contains(obj))
                lst.Add(obj);
        }
        public static void BindingData(this BindingSource bds, object objData)
        {
            if (bds == null) return;
            bds.Clear();
            bds.DataSource = null;
            //bds.ResumeBinding();
            bds.DataSource = objData;
        }
    }
    public class SQLAppWaitingDialog
    {
        public static Thread CurrentThread;
        private static SQLWaitingDialog _waitForm;
        private static event HideDialogDelegate HideDialogEvent;
        private static event ShowDialogDelegate ShowDialogEvent;
        public static event StopDialogDelegate StopDialogEvent;
        public static bool IsShow = false;
        #region code cũ
        //public static void ShowWaitForm()
        //{
        //    StartThead(new ThreadStart(SQLAppWaitingDialog.Thread_CallBack_ShowWaitingDialog));
        //    Application.Idle += OnLoaded;
        //    Thread.Sleep(400);
        //}
        //public static void Show()
        //{
        //    StartThead(new ThreadStart(SQLAppWaitingDialog.Thread_CallBack_ShowWaitingDialog));
        //    Thread.Sleep(400);
        //}
        //public static void ShowDialog()
        //{
        //    try
        //    {
        //        if (CurrentThread != null)
        //        {
        //            IAsyncResult result = ShowDialogEvent.BeginInvoke(null, null);
        //            result.AsyncWaitHandle.WaitOne();
        //            ShowDialogEvent.EndInvoke(result);
        //        }
        //    }
        //    catch (Exception)
        //    {
        //    }
        //}
        //private static void Thread_CallBack_ShowWaitingDialog()
        //{
        //    if (_waitForm != null && !_waitForm.IsDisposed)
        //    {
        //        _waitForm = new SQLWaitingDialog();
        //        _waitForm.FormClosing += new FormClosingEventHandler(Thread_DialogForm_FormClosing);
        //    }
        //    if (_waitForm == null)
        //    {
        //        _waitForm = new SQLWaitingDialog();
        //        _waitForm.FormClosing += new FormClosingEventHandler(Thread_DialogForm_FormClosing);
        //    }
        //    Thread_ShowDialog();
        //}

        //private static void Thread_DialogForm_FormClosing(object sender, FormClosingEventArgs e)
        //{
        //    try
        //    {
        //        IAsyncResult result = StopDialogEvent.BeginInvoke(null, null);
        //        result.AsyncWaitHandle.WaitOne();
        //        StopDialogEvent.EndInvoke(result);
        //    }
        //    catch (Exception)
        //    {
        //    }
        //}

        //private static void Thread_ShowDialog()
        //{
        //    try
        //    {
        //        if (_waitForm != null)
        //        {
        //            if (_waitForm.InvokeRequired)
        //            {
        //                _waitForm.Invoke(new ShowDialogDelegate(Thread_ShowDialog));
        //            }
        //            else
        //            {
        //                _waitForm.TopMost = true;
        //                _waitForm.ShowDialog();
        //            }
        //        }
        //    }
        //    catch (System.Threading.ThreadAbortException ex)
        //    {
        //    }
        //    catch (Exception ex)
        //    {
        //    }
        //}

        //private static void StartThead(ThreadStart threadStart)
        //{
        //    if (!((CurrentThread != null) && CurrentThread.IsAlive))
        //    {
        //        CurrentThread = new Thread(threadStart);
        //        CurrentThread.IsBackground = true;
        //        CurrentThread.Start();
        //    }
        //    else
        //    {
        //        ShowDialog();
        //    }
        //}

        //private static void OnLoaded(object sender, EventArgs e)
        //{
        //    Application.Idle -= OnLoaded;
        //    HideDialog();
        //}
        //public static void CleanStopEventHandler()
        //{
        //    try
        //    {
        //        if (StopDialogEvent != null)
        //        {
        //            StopDialogDelegate delegate2 = (StopDialogDelegate)Delegate.Combine(new Delegate[] { StopDialogEvent });
        //            if (delegate2 != null)
        //            {
        //                foreach (StopDialogDelegate delegate3 in delegate2.GetInvocationList())
        //                {
        //                    StopDialogEvent -= delegate3;
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {
        //    }
        //}
        //public static void Close()
        //{
        //    try
        //    {
        //        CleanStopEventHandler();
        //    }
        //    catch (Exception)
        //    {
        //    }
        //    try
        //    {
        //        HideDialog();
        //    }
        //    catch (Exception)
        //    {
        //    }
        //}
        //public static void HideDialog()
        //{
        //    try
        //    {
        //        if (CurrentThread != null)
        //        {
        //            CleanStopEventHandler();
        //            IAsyncResult result = HideDialogEvent.BeginInvoke(null, null);
        //            result.AsyncWaitHandle.WaitOne();
        //            HideDialogEvent.EndInvoke(result);
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        if (CurrentThread != null)
        //        {
        //            CurrentThread.Abort();
        //            CurrentThread = null;
        //        }
        //    }
        //}
        //public static void Quit()
        //{
        //    try
        //    {
        //        if (CurrentThread != null)
        //        {
        //            while (CurrentThread.IsAlive)
        //            {
        //                CurrentThread.Abort();
        //                Thread.Sleep(1000);
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {
        //    }
        //}
        #endregion

        #region Code mới
        public static void Close()
        {
            HideDialog();
        }
        public static void HideDialog()
        {
            try
            {
                SplashScreenManager.CloseForm(false);
                IsShow = false;
            }
            catch (Exception)
            {
            }
        }
        public static bool IsWaiting()
        {
            if (SplashScreenManager.Default == null)
            {
                return false;
            }
            return SplashScreenManager.Default.IsSplashFormVisible;
        }
        public static void PerformStep()
        {
            if (SplashScreenManager.Default != null && IsShow)
            {
                SplashScreenManager.Default.SendCommand(WaitFormCommand.PerformStep, null);
            }
        }
        public static void Quit()
        {
            HideDialog();
        }
        public static void Show(Form pscrAtv = null)
        {
            SplashScreenManager.ShowForm(Form.ActiveForm, typeof(SQLWaitingDialog), true, true, false);
            IsShow = true;
        }
        public static void ShowDialog()
        {
            SplashScreenManager.ShowForm(Form.ActiveForm, typeof(SQLWaitingDialog), true, true, false);
            IsShow = true;
        }
        #endregion
        private delegate void HideDialogDelegate();

        private delegate void ShowDialogDelegate();

        public delegate void StopDialogDelegate();

        public enum WaitFormCommand
        {
            SetPosition,
            SetProgress,
            PerformStep,
            ProcessTitile
        }
    }
    public class SQLNotifycationAler
    {
        //private static frmNotifycation _frmNotifycation;
        //private static BackgroundWorker _worker;
        //public static void ShowWaitForm()
        //{
        //    if (_worker == null)
        //    {
        //        _worker = new BackgroundWorker();
        //        _worker.WorkerSupportsCancellation = true;
        //        _worker.DoWork += worker_DoWork;
        //        _worker.RunWorkerCompleted += worker_RunWorkerCompleted;
        //        if (_worker.IsBusy)
        //            _worker.CancelAsync();
        //        else
        //            _worker.RunWorkerAsync();
        //    }
        //}
        //public static void Show(string caption)
        //{
        //    _frmNotifycation = new frmNotifycation();
        //    _frmNotifycation.SetCaption(caption);
        //    _frmNotifycation.ShowDialog();
        //}
        //private static void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        //{
        //    if (_frmNotifycation != null)
        //    {
        //        _frmNotifycation.Close();
        //    }
        //}
        //private static void worker_DoWork(object sender, DoWorkEventArgs e)
        //{
        //    if (_worker.CancellationPending)
        //    {
        //        e.Cancel = true;
        //        return;
        //    }
        //    if (_frmNotifycation == null)
        //    {
        //        _frmNotifycation = new frmNotifycation();
        //        _frmNotifycation.FormClosing += new FormClosingEventHandler(Thread_DialogForm_FormClosing);
        //        Application.Idle += OnLoaded;
        //        _frmNotifycation.TopMost = true;
        //        Thread.Sleep(1000);
        //        _frmNotifycation.ShowDialog();
        //    }
        //}
        //private static void Thread_DialogForm_FormClosing(object sender, FormClosingEventArgs e)
        //{
        //    if (_worker != null && !_worker.CancellationPending)
        //    {
        //        _worker.CancelAsync();
        //        _worker.Dispose();
        //    }
        //}
        //private static void OnLoaded(object sender, EventArgs e)
        //{
        //    Application.Idle -= OnLoaded;
        //    worker_RunWorkerCompleted(null, null);
        //}
    }
    public static class ListExtra
    {
        public static string GetField(this DataRow r, string strName)
        {
            object obj = r.GetFieldName(strName);
            if (obj != null && obj.GetType() == typeof(DateTime))
                return Convert.ToDateTime(obj).ToShortDateString();
            return Convert.ToString(obj);
        }
        public static object GetFieldName(this DataRow r, string strName)
        {
            object obj = null;
            if (r.Table.Columns.Contains(strName))
                obj = r[strName];
            return obj;
        }
        public static T GetFieldName<T>(this DataRow r, string strName)
        {
            T obj = default(T);
            if (r.Table.Columns.Contains(strName))
                obj = (T)Convert.ChangeType(r[strName], typeof(T));
            return obj;
        }
    }
}