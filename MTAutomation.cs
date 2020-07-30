using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.Entity;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid;
using DevExpress.XtraEditors.Controls;

namespace MTAutomation
{

    public partial class MTAutomation : Form
    {

        private MTProviderEntities db = new MTProviderEntities();
        private string symbol = "";
        private BindingSource bindingSource = new BindingSource();
        GridView selGV = new GridView();

        public MTAutomation()
        {
            InitializeComponent();
        }

        private void FillPositions()
        {
            IList<PositionsDTO> positions = (from p in db.Positions
                                             join s in db.MTSymbols on p.SymbolName equals s.Name
                                             where p.IsClose == false && p.TicketNo != 0
                                             orderby p.PositionState ascending
                                             select new PositionsDTO
                                             {
                                                 ID = p.ID,
                                                 RowNO = p.RowNO,
                                                 PositionStateStr = !p.PositionState ? "Buy" : "Sell",
                                                 Volume = p.Volume,
                                                 SymbolName = p.SymbolName,
                                                 MarketPrice = !p.PositionState ? s.BidPrice : s.AskPrice,
                                                 Profit = !p.PositionState ? (s.BidPrice - p.Price) * p.Volume * 100000 * s.PerUSDAsk : (p.Price - s.AskPrice) * p.Volume * 100000 * s.PerUSDBid,
                                                 ProfitPercent = (!p.PositionState ? (s.BidPrice - p.Price) * p.Volume * 100000 * s.PerUSDAsk : (p.Price - s.AskPrice) * p.Volume * 100000 * s.PerUSDBid) / (p.Volume * 100000 * p.PriceInUSD / p.Leverage) * 100,
                                                 Price = p.Price,
                                                 UserName = p.UserName,
                                                 Leverage = p.Leverage
                                             }).ToList();

            dgwOpenPosition.AutoGenerateColumns = false;
            dgwOpenPosition.DataSource = positions;


            IList<PositionsDTO> closePositions = (from p in db.Positions
                                                  join s in db.MTSymbols on p.SymbolName equals s.Name
                                                  where p.IsClose == true && p.TicketNo != 0
                                                  orderby p.PositionState ascending
                                                  select new PositionsDTO
                                                  {
                                                      ID = p.ID,
                                                      RowNO = p.RowNO,
                                                      PositionStateStr = !p.PositionState ? "Buy" : "Sell",
                                                      Volume = p.Volume,
                                                      SymbolName = p.SymbolName,
                                                      MarketPrice = !p.PositionState ? s.BidPrice : s.AskPrice,
                                                      Profit = !p.PositionState ? (s.BidPrice - p.Price) * p.Volume * 100000 * s.PerUSDAsk : (p.Price - s.AskPrice) * p.Volume * 100000 * s.PerUSDBid,
                                                      ProfitPercent = (!p.PositionState ? (s.BidPrice - p.Price) * p.Volume * 100000 * s.PerUSDAsk : (p.Price - s.AskPrice) * p.Volume * 100000 * s.PerUSDBid) / (p.Volume * 100000 * p.PriceInUSD / p.Leverage) * 100,
                                                      Price = p.Price,
                                                      UserName = p.UserName,
                                                      Leverage = p.Leverage
                                                  }).ToList();

            dgwClosePosition.AutoGenerateColumns = false;
            dgwClosePosition.DataSource = closePositions;
        }

        private void MTAutomation_Load(object sender, EventArgs e)
        {
            FillPositions();


            List<MTSymbol> symbols = db.MTSymbols.Include("MTHistoryMS").ToList();
            BindingList<MTSymbol> bindingSymbols = new BindingList<MTSymbol>(symbols);
            gvControlHistory.DataSource = bindingSymbols;
            //gvControlHistory.DataSource = symbols;
            gvDetailHistory.OptionsBehavior.AllowAddRows = DevExpress.Utils.DefaultBoolean.False;

            repositoryItemLookUpEdit1.DataSource = db.MTPeriods.ToList();
            repositoryItemLookUpEdit1.DisplayMember = "PeriodName";
            repositoryItemLookUpEdit1.ValueMember = "PeriodID";

            //backgroundWorker1.RunWorkerAsync();


            txtAvailableBudget.Text = Properties.Settings.Default["AvailableBudget"].ToString();
            txtDescisionMakingQuantity.Text = Properties.Settings.Default["DescisionMakingQuantity"].ToString();
            txtMaxDealingLot.Text = Properties.Settings.Default["MaxDealingLot"].ToString();
            txtMinDealingLot.Text = Properties.Settings.Default["MinDealingLot"].ToString();
            txtMinMarginNeeded.Text = Properties.Settings.Default["MinMarginNeeded"].ToString();

        }

        private void gvlHistory_MasterRowGetRelationName(object sender, DevExpress.XtraGrid.Views.Grid.MasterRowGetRelationNameEventArgs e)
        {
            e.RelationName = "MTHistoryMS";
        }

        private void gvlHistory_MasterRowGetChildList(object sender, DevExpress.XtraGrid.Views.Grid.MasterRowGetChildListEventArgs e)
        {
            GridView view = sender as GridView;
            MTSymbol c = (MTSymbol)view.GetRow(e.RowHandle);
            BindingList<MTHistoryM> bindingHistory = new BindingList<MTHistoryM>(c.MTHistoryMS.ToList());
            bindingSource.DataSource = bindingHistory;

            e.ChildList = bindingSource;

            MTProviderEntities db1 = new MTProviderEntities();
            IList<MTHistoryM> mtHistoryMS = db1.MTHistoryMS.Where(a => a.Symbol == c.Name).ToList();
            foreach (MTHistoryM mtHistory in mtHistoryMS)
            {
                MTHistoryM mtHistoryMSOld = c.MTHistoryMS.Where(a => a.ID == mtHistory.ID).FirstOrDefault();

                if (mtHistory.AllTicks != mtHistoryMSOld.AllTicks)
                    mtHistoryMSOld.AllTicks = mtHistory.AllTicks;
                if (mtHistory.MyTicks != mtHistoryMSOld.MyTicks)
                    mtHistoryMSOld.MyTicks = mtHistory.MyTicks;

            }

            //e.ChildList = bindingHistory;
        }

        private void gvlHistory_MasterRowGetRelationCount(object sender, MasterRowGetRelationCountEventArgs e)
        {
            e.RelationCount = 1;
        }

        private void gvDetailHistory_RowUpdated(object sender, DevExpress.XtraGrid.Views.Base.RowObjectEventArgs e)
        {
            GridView view = sender as GridView;
            if (view.IsNewItemRow(e.RowHandle))
            {
                //view.ValidateEditor();
                MTHistoryM c = (MTHistoryM)e.Row;
                symbol = c.Symbol;
                db.MTHistoryMS.Add((MTHistoryM)e.Row);
                //view.CollapseGroupRow(view.SourceRowHandle);
            }
        }

        private void gvDetailHistory_InitNewRow(object sender, InitNewRowEventArgs e)
        {
            GridView view = sender as GridView;
            view.SetRowCellValue(e.RowHandle, view.Columns["Symbol"], symbol);
            view.SetRowCellValue(e.RowHandle, view.Columns["AutoBuySell"], false);

            //view.FocusedColumn = view.VisibleColumns[0];
            //gvDetailHistory.FocusedRowHandle = e.RowHandle;//DevExpress.XtraGrid.GridControl.NewItemRowHandle;
            //gvDetailHistory.FocusedColumn = gvDetailHistory.VisibleColumns[0];
        }

        private void gvlHistory_MasterRowExpanded(object sender, CustomMasterRowEventArgs e)
        {
            GridView view = sender as GridView;
            MTSymbol c = (MTSymbol)view.GetRow(e.RowHandle);

            GridView childView = view.GetDetailView(e.RowHandle, e.RelationIndex) as GridView;
            if (childView != null)
            {
                symbol = c.Name;
                childView.AddNewRow();
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            db.SaveChanges();
            //if (!backgroundWorker1.IsBusy)
            //    backgroundWorker1.RunWorkerAsync();
        }

        private void gvDetailHistory_ShownEditor(object sender, EventArgs e)
        {
            GridView view = sender as GridView;
            view.ActiveEditor.IsModified = true;
        }

        private void gvDetailHistory_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            GridView gv = (sender as GridView);

            if (gv.FocusedColumn.FieldName == coldtStartTime.FieldName)
            {
                DateTime date = new DateTime();
                //var value = gv.GetRowCellValue(gv.FocusedRowHandle, coldtStartTime);
                if (e.Value == null)
                {
                    e.Valid = false;
                    e.ErrorText = "Start Time can't be empty";
                }
                else
                {
                    if (String.IsNullOrEmpty(e.Value.ToString()))
                    {
                        e.Valid = false;
                        e.ErrorText = "Start Time can't be empty";
                    }
                    else
                    {
                        if (!DateTime.TryParse(e.Value.ToString(), out date))
                        {
                            e.Valid = false;
                            e.ErrorText = "Start Time isn't valid date";
                        }
                        else
                        {
                            if (date == DateTime.MinValue)
                            {
                                e.Valid = false;
                                e.ErrorText = "Start Time isn't valid date";
                            }
                        }
                    }
                }

            }
            if (gv.FocusedColumn.FieldName == coldtPeriod.FieldName)
            {
                if (e.Value == null)
                {
                    e.Valid = false;
                    e.ErrorText = "Please select one item";
                }
                else
                {
                    if (String.IsNullOrEmpty(e.Value.ToString()))
                    {
                        e.Valid = false;
                        e.ErrorText = "Please select one item";
                    }
                    else
                    {
                        if (int.Parse(e.Value.ToString()) == 0)
                        {
                            e.Valid = false;
                            e.ErrorText = "Please select one item";
                        }
                        else
                        {
                            bool valid = true;
                            for (int i = 0; i < gv.DataRowCount; i++)
                            {
                                //valid = false;
                                //foreach (GridColumn column in gv.Columns)
                                //{

                                if (i != gv.FocusedRowHandle)
                                {
                                    if (gv.GetRowCellValue(i, coldtPeriod).Equals(e.Value))
                                    {
                                        //valid = true;
                                        valid = false;
                                        break;
                                    }
                                }
                                //}
                                //if (!valid) break;
                            }
                            if (!valid)
                            {
                                e.Valid = false;
                                e.ErrorText = "Please select another period, this period is duplicated";
                            }
                        }
                    }
                }
            }
        }

        private void gvDetailHistory_ValidateRow(object sender, DevExpress.XtraGrid.Views.Base.ValidateRowEventArgs e)
        {
            GridView gv = (sender as GridView);

            DateTime date = new DateTime();
            var valueTime = gv.GetRowCellValue(e.RowHandle, coldtStartTime);
            var valuePeriod = gv.GetRowCellValue(e.RowHandle, coldtPeriod);
            if (valueTime == null)
            {
                e.Valid = false;
                e.ErrorText = "Start Time can't be empty";
            }
            else
            {
                if (String.IsNullOrEmpty(valueTime.ToString()))
                {
                    e.Valid = false;
                    e.ErrorText = "Start Time can't be empty";
                }
                else
                {
                    if (!DateTime.TryParse(valueTime.ToString(), out date))
                    {
                        e.Valid = false;
                        e.ErrorText = "Start Time isn't valid date";
                    }
                    else
                    {
                        if (date == DateTime.MinValue)
                        {
                            e.Valid = false;
                            e.ErrorText = "Start Time isn't valid date";
                        }
                    }
                }
            }

            if (valuePeriod == null)
            {
                e.Valid = false;
                e.ErrorText = "Please select one item";
            }
            else
            {
                if (String.IsNullOrEmpty(valuePeriod.ToString()))
                {
                    e.Valid = false;
                    e.ErrorText = "Please select one item";
                }
                else
                {
                    if (int.Parse(valuePeriod.ToString()) == 0)
                    {
                        e.Valid = false;
                        e.ErrorText = "Please select one item";
                    }
                    else
                    {

                        bool valid = true;
                        for (int i = 0; i < gv.DataRowCount; i++)
                        {
                            //valid = false;
                            //foreach (GridColumn column in gv.Columns)
                            //{
                            if (i != e.RowHandle)
                            {
                                if (gv.GetRowCellValue(i, coldtPeriod).Equals(gv.GetRowCellValue(e.RowHandle, coldtPeriod)))
                                {
                                    //valid = true;
                                    valid = false;
                                    break;
                                }
                            }
                            //}
                            //if (!valid) break;
                        }
                        if (!valid)
                        {
                            e.Valid = false;
                            e.ErrorText = "Please select another period, this period is duplicated";
                        }
                    }
                }
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            FillPositions();
        }

        private void gvControlHistory_ProcessGridKey(object sender, KeyEventArgs e)
        {
            GridControl grid = sender as GridControl;
            GridView view = grid.FocusedView as GridView;
            if (view.Name.Equals("gvDetailHistory"))
                if (e.KeyData == Keys.Delete)
                {
                    MTHistoryM historyMS = (MTHistoryM)view.GetRow(view.GetSelectedRows()[0]);
                    IEnumerable<MTHistoryDT> historyDTs = db.MTHistoryDTs.Where(a => a.MSID == historyMS.ID).ToArray();
                    if (historyDTs.Count() > 0)
                        db.MTHistoryDTs.RemoveRange(historyDTs);
                    db.MTHistoryMS.Remove(historyMS);
                    view.DeleteSelectedRows();
                    e.Handled = true;
                }
        }

        private void gvDetailHistory_InvalidRowException(object sender, DevExpress.XtraGrid.Views.Base.InvalidRowExceptionEventArgs e)
        {
            GridView gv = (sender as GridView);

            var valueTime = gv.GetRowCellValue(e.RowHandle, coldtStartTime);
            var valuePeriod = gv.GetRowCellValue(e.RowHandle, coldtPeriod);
            if ((valueTime == null && valuePeriod == null) || (DateTime.Parse(valueTime.ToString()) == DateTime.MinValue && int.Parse(valuePeriod.ToString()) == 0))
            {
                e.ExceptionMode = ExceptionMode.Ignore;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            double tmpDouble;
            if (double.TryParse(txtAvailableBudget.Text, out tmpDouble))
                Properties.Settings.Default["AvailableBudget"] = txtAvailableBudget.Text;
            else
                MessageBox.Show("Please enter correct number for Available Budget");

            int tmpInt;
            if (int.TryParse(txtDescisionMakingQuantity.Text, out tmpInt))
                Properties.Settings.Default["DescisionMakingQuantity"] = txtDescisionMakingQuantity.Text;
            else
                MessageBox.Show("Please enter correct number for Descision Making Quantity");

            Properties.Settings.Default["MaxDealingLot"] = txtMaxDealingLot.Text;
            Properties.Settings.Default["MinDealingLot"] = txtMinDealingLot.Text;
            Properties.Settings.Default["MinMarginNeeded"] = txtMinMarginNeeded.Text.Substring(0, 5);
            Properties.Settings.Default.Save();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            MTProviderEntities db1 = new MTProviderEntities();
            IList<MTHistoryM> mtHistoryMS = db1.MTHistoryMS/*.Where(a => a.AllTicks != a.MyTicks || a.AllTicks == 0)*/.ToList();
            while (mtHistoryMS.Count != 0)
            {
                if (worker.CancellationPending == true)
                {
                    e.Cancel = true;
                    break;
                }
                else
                {
                    // Perform a time consuming operation and report progress.
                    System.Threading.Thread.Sleep(1000);
                    BindingList<MTHistoryM> mtHistoryMSOlds = this.bindingSource.DataSource as BindingList<MTHistoryM>;
                    if (mtHistoryMSOlds != null)
                    {
                        foreach (MTHistoryM mtHistory in mtHistoryMS)
                        {

                            foreach (GridView gridView in gvControlHistory.ViewCollection)
                            {
                                int rowHandle = gridView.LocateByValue("ID", mtHistory.ID);
                                if (rowHandle != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                                {
                                    MTHistoryM mtHistoryMSOld = mtHistoryMSOlds.Where(a => a.ID == mtHistory.ID).FirstOrDefault();
                                    if ((mtHistory.AllTicks != mtHistoryMSOld.AllTicks) || (mtHistory.MyTicks != mtHistoryMSOld.MyTicks))
                                    {
                                        worker.ReportProgress(rowHandle, mtHistory);
                                        selGV = gridView;
                                    }
                                }
                            }




                            //MTHistoryM mtHistoryMSOld = mtHistoryMSOlds.Where(a => a.ID == mtHistory.ID).FirstOrDefault();
                            ////MTHistoryM mtHistoryMSOld = db.MTHistoryMS.Where(a => a.ID == mtHistory.ID).FirstOrDefault();

                            //if (mtHistory.AllTicks != mtHistoryMSOld.AllTicks)
                            //    mtHistoryMSOlds[0].AllTicks = mtHistory.AllTicks;
                            //if (mtHistory.MyTicks != mtHistoryMSOld.MyTicks)
                            //    mtHistoryMSOlds[0].MyTicks = mtHistory.MyTicks;



                            //iterate through each expanded grid  
                            //foreach (GridView gridView in gvControlHistory.ViewCollection)
                            //{
                            //    //find row handle within grid that contains the field value you are searching for  
                            //    //int rowHandle = gridView.LocateByValue("theFieldName", someObjectVal);

                            //    int rowHandle = gridView.LocateByValue("ID", mtHistory.ID);
                            //    if (rowHandle != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                            //    {
                            //        MTHistoryM mtHistoryMSOld = db.MTHistoryMS.Where(a => a.ID == mtHistory.ID).FirstOrDefault();
                            //        if (mtHistory.AllTicks != 0)
                            //            gridView.SetRowCellValue(rowHandle, "AllTicks", mtHistory.AllTicks);
                            //        if (mtHistory.MyTicks != 0)
                            //        {
                            //            gridView.SetRowCellValue(rowHandle, "MyTicks", mtHistory.MyTicks);
                            //            //mtHistoryMSOld.MyTicks = mtHistory.MyTicks;
                            //        }
                            //    }
                            //}
                        }
                    }

                    db1 = new MTProviderEntities();
                    mtHistoryMS = db1.MTHistoryMS/*.Where(a => a.AllTicks != a.MyTicks || a.AllTicks == 0)*/.ToList();

                }


            }

        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //GridView gridView = gvControlHistory.ViewCollection[0] as GridView;
            // selGV.FocusedRowHandle = e.ProgressPercentage;

            BindingList<MTHistoryM> mtHistoryMSOlds = this.bindingSource.DataSource as BindingList<MTHistoryM>;
            MTHistoryM mtHistory = e.UserState as MTHistoryM;
            MTHistoryM mtHistoryMSOld = mtHistoryMSOlds.Where(a => a.ID == mtHistory.ID).FirstOrDefault();

            if (mtHistory.AllTicks != mtHistoryMSOld.AllTicks)
                mtHistoryMSOld.AllTicks = mtHistory.AllTicks;
            if (mtHistory.MyTicks != mtHistoryMSOld.MyTicks)
                mtHistoryMSOld.MyTicks = mtHistory.MyTicks;

            selGV.SetRowCellValue(e.ProgressPercentage, "coldtLoading", (int.Parse(mtHistory.MyTicks.ToString()) * 100 / int.Parse(mtHistory.AllTicks.ToString())));

            //gridView.SetRowCellValue(int.Parse(e.UserState.ToString()), "coldtLoading", e.ProgressPercentage);

        }


        private void gvDetailHistory_CustomUnboundColumnData(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDataEventArgs e)
        {
            if (e.Column.Name == coldtLoading.Name && e.IsGetData)
            {

                object myTicks = ((MTHistoryM)e.Row).MyTicks;
                object allTicks = ((MTHistoryM)e.Row).AllTicks;
                if (myTicks != null && allTicks != null && int.Parse(allTicks.ToString()) != 0)
                    e.Value = Convert.ToDecimal((int.Parse(myTicks.ToString()) * 100 / int.Parse(allTicks.ToString())));
                else
                    e.Value = 0;
            }
        }

        private void GvDetailHistory_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            GridView gridView = sender as GridView;
            gridView.UpdateCurrentRow();
        }
    }
}
