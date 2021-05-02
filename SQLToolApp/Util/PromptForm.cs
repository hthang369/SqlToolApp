using SQLAppLib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace SQLToolApp.Util
{
    public class PromptForm
    {
        public static Window _frmParent;
        public static MessageBoxResult Show(string title, string promptText, ref string value, bool bIsText, bool bIsCombobox, bool isShowLstTbl, string[] lstFunctionList,
                                        InputBoxValidation validation)
        {
            ViewModels.PopupViewModel popupView = new ViewModels.PopupViewModel();
            Views.BasePopupWindow popup = new Views.BasePopupWindow() { DataContext = popupView, Height = 150, Width = 600 };
            popupView.Header = title;
            popupView.Title = promptText;
            popupView.valueReturn = value;
            popupView.isText = bIsText;
            if (isShowLstTbl)
            {
                DataTable dt = SQLDBUtil.GetDataTableByDataSet(SQLDBUtil.GetAllTables());
                popupView.dataSource = dt.Select().Select(x => Convert.ToString(x[0])).ToList();
            }
            else
                popupView.dataSource = lstFunctionList;
            Views.PopupView view = popup.waitLoadView.LoadingChild as Views.PopupView;
            if (bIsText)
                view.txtInput.Focus();
            else
                view.cboInput.Focus();
            //view.txtInpu
            //frmSearch frmInput = new frmSearch(_frmParent, bIsText, bIsCombobox, isShowLstTbl);
            //frmInput.SetCaption(promptText);
            //frmInput.Text = title;
            //frmInput.SetDataSourceCombobox(lstFunctionList);
            //if (bIsText)
            //    frmInput.SetText(value);
            //else
            //    frmInput.SetSelectedText(value);
            //frmInput.GetControlFocus();
            //frmInput.StartPosition = FormStartPosition.CenterScreen;
            //frmInput.ResumeLayout(false);
            //frmInput.PerformLayout();
            //SQLApp.SetFormTitle(frmInput);
            //string text = (bIsText) ? frmInput.GetText() : frmInput.GetSelectedText();
            //if (validation != null)
            //{
            //    frmInput.FormClosing += delegate (object sender, FormClosingEventArgs e)
            //    {
            //        if (frmInput.MessageBoxResult == MessageBoxResult.OK)
            //        {
            //            string errorText = validation(text);
            //            if (e.Cancel = (errorText != ""))
            //            {
            //                MessageBox.Show(frmInput, errorText, "Validation Error",
            //                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            //                frmInput.GetControlFocus();
            //            }
            //        }
            //    };
            //}
            DevExpress.Mvvm.UICommand uIOkCommand = new DevExpress.Mvvm.UICommand(MessageBoxResult.OK, "OK", popupView.okCommand, true, false, null, true, System.Windows.Controls.Dock.Left);
            DevExpress.Mvvm.UICommand uICancelCommand = new DevExpress.Mvvm.UICommand(MessageBoxResult.Cancel, "Cancel", null, false, true, null, true, System.Windows.Controls.Dock.Left);
            List<DevExpress.Mvvm.UICommand> lst = new List<DevExpress.Mvvm.UICommand>() { uIOkCommand, uICancelCommand };
            popup.ShowDialog(lst);
            value = Convert.ToString(popupView.valueReturn);
            //value = (bIsText) ? frmInput.GetText() : frmInput.GetSelectedText();
            MessageBoxResult dialogResult = popup.DialogButtonResult;
            return dialogResult;
        }
        public static MessageBoxResult ShowText(string title, string promptText, ref string value)
        {
            return Show(title, promptText, ref value, true, false, false, null, null);
        }
        public static MessageBoxResult ShowCombobox(string title, string promptText, ref string value)
        {
            return Show(title, promptText, ref value, false, true, true, null, null);
        }
        public static MessageBoxResult ShowCombobox(string title, string promptText, string[] lstSource, ref string value)
        {
            return Show(title, promptText, ref value, false, true, false, lstSource, null);
        }
        public delegate string InputBoxValidation(string errorMessage);
    }
    public class TextBoxUtil : TextBox
    {
        public TextBoxUtil()
        {
            this.KeyDown += textBox_KeyDown;
        }

        private void textBox_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
                this.SelectAll();
            if (e.Control && e.Shift && e.KeyCode == Keys.S)
            {
                string str = System.Windows.Forms.Clipboard.GetText();
                str = str.TrimEnd(Environment.NewLine.ToCharArray());
                string[] lst = str.Split('\n');
                this.ResetText();
                this.Text = string.Join(",", lst);
            }
        }
    }
}
