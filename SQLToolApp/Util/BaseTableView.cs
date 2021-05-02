using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLToolApp.Util
{
    public class BaseTableView : TableView
    {
        public BaseTableView() : base()
        {
            this.ShowDataNavigator = true;
            this.AllowColumnFiltering = true;
            this.ShowAutoFilterRow = true;
            this.AllowEditing = false;
            this.SearchPanelFindFilter = DevExpress.Data.Filtering.FilterCondition.Contains;
            this.SearchPanelCriteriaOperatorType = DevExpress.Xpf.Editors.CriteriaOperatorType.And;
        }

    }
}
