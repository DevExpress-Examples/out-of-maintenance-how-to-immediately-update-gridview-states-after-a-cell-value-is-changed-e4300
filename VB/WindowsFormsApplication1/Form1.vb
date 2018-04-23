Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Data
Imports System.Drawing
Imports System.Linq
Imports System.Text
Imports System.Windows.Forms
Imports DevExpress.XtraGrid.Columns
Imports DevExpress.XtraGrid.Views.Grid
Imports DevExpress.Data
Imports DevExpress.XtraGrid
Imports DevExpress.XtraEditors.Repository

Namespace WindowsFormsApplication1
	Partial Public Class Form1
		Inherits Form
		Public Sub New()
			InitializeComponent()
		End Sub

		Private summaryCache As New Dictionary(Of String, Integer)()
		Private Sub Form1_Load(ByVal sender As Object, ByVal e As EventArgs) Handles MyBase.Load
			colorEdit1.EditValue = Color.LightBlue
			colorEdit2.EditValue = Color.LightGray

			gridControl1.DataSource = CreateDataSource()
			gridControl1.ForceInitialize()

			CreateUnboundColumn()

			Dim intColumns() As String = { "Value1", "Value2" }

			Dim reCheckEdit As RepositoryItemCheckEdit = TryCast(gridControl1.RepositoryItems.Add("CheckEdit"), RepositoryItemCheckEdit)
			AddHandler reCheckEdit.EditValueChanged, AddressOf reCheckEdit_EditValueChanged
			gridView1.Columns("Selected").ColumnEdit = reCheckEdit

			Dim reTextEdit As RepositoryItemTextEdit = TryCast(gridControl1.RepositoryItems.Add("TextEdit"), RepositoryItemTextEdit)
			AddHandler reTextEdit.EditValueChanged, AddressOf reTextEdit_EditValueChanged
			reTextEdit.EditValueChangedFiringMode = DevExpress.XtraEditors.Controls.EditValueChangedFiringMode.Buffered

			For Each columnName As String In intColumns
				summaryCache.Add(columnName, 0)
				gridView1.Columns(columnName).SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom
				gridView1.Columns(columnName).SummaryItem.DisplayFormat = "Custom Sum = {0}"
				gridView1.Columns(columnName).ColumnEdit = reTextEdit

			Next columnName
			gridView1.OptionsView.ShowFooter = True
		End Sub

		Private Sub reTextEdit_EditValueChanged(ByVal sender As Object, ByVal e As EventArgs)
			BeginInvoke(New MethodInvoker(Function() AnonymousMethod1()))
		End Sub
		
		Private Function AnonymousMethod1() As Boolean
			gridView1.PostEditor()
			gridView1.UpdateCurrentRow()
			Return True
		End Function

		Private Sub reCheckEdit_EditValueChanged(ByVal sender As Object, ByVal e As EventArgs)
			gridView1.PostEditor()
			gridView1.UpdateSummary()
		End Sub

		Private Sub CreateUnboundColumn()
			' Create an unbound column.
			Dim unbColumn As GridColumn = gridView1.Columns.AddField("RowSum")
			unbColumn.VisibleIndex = gridView1.Columns.Count
			unbColumn.UnboundType = DevExpress.Data.UnboundColumnType.Integer
			' Disable editing.
			unbColumn.OptionsColumn.AllowEdit = False
			' Customize the appearance settings.
			unbColumn.AppearanceCell.BackColor = Color.LemonChiffon
		End Sub

		Private Shared Function CreateDataSource() As DataTable
			Dim someDT As New DataTable()
			someDT.Columns.Add("Selected", GetType(Boolean))
			someDT.Columns.Add("Value1", GetType(Integer))
			someDT.Columns.Add("Value2", GetType(Integer))

			Dim myRand As New Random()
			For i As Integer = 0 To 19
				someDT.Rows.Add(i Mod 2, myRand.Next(1, 100), myRand.Next(1, 100))
			Next i
			Return someDT
		End Function

		Private Sub gridView1_CustomUnboundColumnData(ByVal sender As Object, ByVal e As DevExpress.XtraGrid.Views.Base.CustomColumnDataEventArgs) Handles gridView1.CustomUnboundColumnData
			If e.IsGetData AndAlso e.Column.FieldName = "RowSum" Then
				e.Value = Convert.ToInt32((TryCast(e.Row, DataRowView))("Value1")) + Convert.ToInt32((TryCast(e.Row, DataRowView))("Value2"))
			End If
		End Sub

		Private Sub gridView1_CustomSummaryCalculate(ByVal sender As Object, ByVal e As DevExpress.Data.CustomSummaryEventArgs) Handles gridView1.CustomSummaryCalculate
			If TypeOf e.Item Is GridColumnSummaryItem Then
				Dim currentView As GridView = TryCast(sender, GridView)
				Dim summaryItem As GridColumnSummaryItem = TryCast(e.Item, GridColumnSummaryItem)

				' Initialization
				If e.SummaryProcess = CustomSummaryProcess.Start Then
					summaryCache(summaryItem.FieldName) = 0
				End If
				' Calculation
				If e.SummaryProcess = CustomSummaryProcess.Calculate Then
					Dim needsToCalulate As Boolean = CBool(currentView.GetRowCellValue(e.RowHandle, currentView.Columns("Selected")))
					If needsToCalulate Then
						summaryCache(summaryItem.FieldName) += Convert.ToInt32(e.FieldValue)
					End If

				End If
				' Finalization
				If e.SummaryProcess = CustomSummaryProcess.Finalize Then
					e.TotalValue = summaryCache(summaryItem.FieldName)
				End If
			End If
		End Sub

		Private Sub gridView1_RowStyle(ByVal sender As Object, ByVal e As DevExpress.XtraGrid.Views.Grid.RowStyleEventArgs) Handles gridView1.RowStyle
			Dim currentView As GridView = TryCast(sender, GridView)
			Dim rowSum As Integer = Convert.ToInt32(currentView.GetRowCellValue(e.RowHandle, currentView.Columns("RowSum")))
			If rowSum > 50 AndAlso rowSum < 100 Then
				e.Appearance.BackColor = CType(colorEdit1.EditValue, Color)
			ElseIf rowSum >= 100 Then
				e.Appearance.BackColor = CType(colorEdit2.EditValue, Color)
			End If
		End Sub
	End Class
End Namespace
