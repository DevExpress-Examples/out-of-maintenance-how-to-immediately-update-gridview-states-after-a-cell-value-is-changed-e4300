using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.Data;
using DevExpress.XtraGrid;
using DevExpress.XtraEditors.Repository;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        Dictionary<string, int> summaryCache = new Dictionary<string, int>();
        private void Form1_Load(object sender, EventArgs e)
        {
            colorEdit1.EditValue = Color.LightBlue;
            colorEdit2.EditValue = Color.LightGray;

            gridControl1.DataSource = CreateDataSource();
            gridControl1.ForceInitialize();

            CreateUnboundColumn();

            string[] intColumns = new string[] { "Value1", "Value2" };

            RepositoryItemCheckEdit reCheckEdit = gridControl1.RepositoryItems.Add("CheckEdit") as RepositoryItemCheckEdit;
            reCheckEdit.EditValueChanged += new EventHandler(reCheckEdit_EditValueChanged);
            gridView1.Columns["Selected"].ColumnEdit = reCheckEdit;

            RepositoryItemTextEdit reTextEdit = gridControl1.RepositoryItems.Add("TextEdit") as RepositoryItemTextEdit;
            reTextEdit.EditValueChanged += new EventHandler(reTextEdit_EditValueChanged);
            reTextEdit.EditValueChangedFiringMode = DevExpress.XtraEditors.Controls.EditValueChangedFiringMode.Buffered;            

            foreach (string columnName in intColumns)
            {
                summaryCache.Add(columnName, 0);
                gridView1.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;
                gridView1.Columns[columnName].SummaryItem.DisplayFormat = "Custom Sum = {0}";
                gridView1.Columns[columnName].ColumnEdit = reTextEdit;               

            }
            gridView1.OptionsView.ShowFooter = true;
        }

        void reTextEdit_EditValueChanged(object sender, EventArgs e)
        {
            BeginInvoke(new MethodInvoker(delegate
            {
                gridView1.PostEditor();
                gridView1.UpdateCurrentRow();
            }));
        }

        void reCheckEdit_EditValueChanged(object sender, EventArgs e)
        {
            gridView1.PostEditor();
            gridView1.UpdateSummary();
        }

        private void CreateUnboundColumn()
        {
            // Create an unbound column.
            GridColumn unbColumn = gridView1.Columns.AddField("RowSum");
            unbColumn.VisibleIndex = gridView1.Columns.Count;
            unbColumn.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
            // Disable editing.
            unbColumn.OptionsColumn.AllowEdit = false;
            // Customize the appearance settings.
            unbColumn.AppearanceCell.BackColor = Color.LemonChiffon;
        }

        private static DataTable CreateDataSource()
        {
            DataTable someDT = new DataTable();
            someDT.Columns.Add("Selected", typeof(bool));
            someDT.Columns.Add("Value1", typeof(int));
            someDT.Columns.Add("Value2", typeof(int));

            Random myRand = new Random();
            for (int i = 0; i < 20; i++)
            {
                someDT.Rows.Add(i % 2, myRand.Next(1, 100), myRand.Next(1, 100));
            }
            return someDT;
        }
        
        private void gridView1_CustomUnboundColumnData(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDataEventArgs e)
        {
            if (e.IsGetData && e.Column.FieldName == "RowSum")
            {
                e.Value = Convert.ToInt32((e.Row as DataRowView)["Value1"]) + Convert.ToInt32((e.Row as DataRowView)["Value2"]);
            }
        }

        private void gridView1_CustomSummaryCalculate(object sender, DevExpress.Data.CustomSummaryEventArgs e)
        {
            if (e.Item is GridColumnSummaryItem)
            {
                GridView currentView = sender as GridView;
                GridColumnSummaryItem summaryItem = e.Item as GridColumnSummaryItem;

                // Initialization
                if (e.SummaryProcess == CustomSummaryProcess.Start)
                {
                    summaryCache[summaryItem.FieldName] = 0;
                }
                // Calculation
                if (e.SummaryProcess == CustomSummaryProcess.Calculate)
                {
                    bool needsToCalulate = (bool)currentView.GetRowCellValue(e.RowHandle, currentView.Columns["Selected"]);
                    if (needsToCalulate)
                    {
                        summaryCache[summaryItem.FieldName] += Convert.ToInt32(e.FieldValue);
                    }
                    
                }
                // Finalization
                if (e.SummaryProcess == CustomSummaryProcess.Finalize)
                {
                    e.TotalValue = summaryCache[summaryItem.FieldName];
                }
            }
        }

        private void gridView1_RowStyle(object sender, DevExpress.XtraGrid.Views.Grid.RowStyleEventArgs e)
        {
            GridView currentView = sender as GridView;
            int rowSum = Convert.ToInt32(currentView.GetRowCellValue(e.RowHandle, currentView.Columns["RowSum"]));
            if (rowSum > 50 && rowSum < 100)
                e.Appearance.BackColor = (Color)colorEdit1.EditValue;
            else if (rowSum >= 100)
                e.Appearance.BackColor = (Color)colorEdit2.EditValue;
        }
    }
}
