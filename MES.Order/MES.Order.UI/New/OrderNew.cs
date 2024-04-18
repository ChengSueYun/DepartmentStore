﻿using DevExpress.Data;
using DevExpress.Utils.Extensions;
using DevExpress.XtraBars;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid.Views.Grid;
using MES.Order.Adapter;
using MES.Order.DAL.ViewModel;
using MES.Order.Infrastructure;
using MES.Order.Infrastructure.NewViewModel;
using MES.Order.Infrastructure.NewViewModel.Filter;
using MES.Order.Infrastructure.NewViewModel.Request;
using MES.Order.UI.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraGrid.Views.Base;
using THS.Infrastructure.Extensions;
using THS.Infrastructure.Page;

namespace MES.Order.UI.New
{
    public partial class OrderNew : XtraForm
    {
    #region 新增客戶

        private void gridView_AddCustomer_EditFormHidden(object                  sender
                                                       , EditFormHiddenEventArgs e)
        {
            try
            {
                if (e.Result == EditFormResult.Update)
                {
                    if (this.CustomerKeybindingSource.Current is CustomInfoViewModel request)
                    {
                        request.SetDefaultValue();
                        request.CreateUser = Environment.MachineName;
                        request.CreateDate = DateTime.Now;
                        var addOrUpdate = BasicUtility.CustomerInfoAdapter.AddOrUpdate(request);
                        while (addOrUpdate.IsCompleted)
                        {
                            Const.AllCustomerView                    = BasicUtility.CustomerInfoAdapter.GetAll().Result;
                            this.CustomerKeybindingSource.DataSource = Const.AllCustomerView;
                        }

                        _Request.Area     = request.Area;
                        _Request.Customer = request.Customer;
                    }
                }
            }
            catch (Exception exception)
            {
                throw new Exception(exception.ToString());
            }
        }

    #endregion

    #region 新增產品

        private void gridView_AddProduct_EditFormHidden(object sender, EditFormHiddenEventArgs e)
        {
            try
            {
                if (e.Result == EditFormResult.Update)
                {
                    if (this.ProductKeybindingSource.Current is ProductsInfoViewModel request)
                    {
                        request.SetDefaultValue();

                        var addOrUpdate = BasicUtility.ProductsInfoAdapter.AddOrUpdate(request);
                        while (addOrUpdate.IsCompleted)
                        {
                            Const.AllProductsView                   = BasicUtility.ProductsInfoAdapter.Get().Result;
                            this.ProductKeybindingSource.DataSource = Const.AllProductsView;
                        }

                        _Request.Factory = request.Factory;
                        _Request.Product = request.Product;
                    }
                }
            }
            catch (Exception exception)
            {
                throw new Exception(exception.ToString());
            }
        }

    #endregion

    #region FocusEvent

        private void gridView_ProductOrder_SelectionChanged(object                    sender
                                                          , SelectionChangedEventArgs e)
        {
            try
            {
                if (IfFocusData)
                {
                    var focusRow = this.orderInfoViewModelBindingSource.Current as OrderInfoViewModel;
                    if (focusRow != null && focusRow.Selection)
                    {
                        focusRow.Status     = GlobalCollection.OrderStatusCollection.LastOrDefault();
                        focusRow.UpdateDate = DateTime.Now;
                        _FocusData.AddOrReplace(x => x.Area     == focusRow.Area     &&
                                                     x.Customer == focusRow.Customer &&
                                                     x.Factory  == focusRow.Factory  &&
                                                     x.Product  == focusRow.Product, focusRow);
                        this.MessageTextBox.AppendText(
                                                       $@"{DateTime.Now:yyyy-MM-dd hh:mm:ss} 已拉單{focusRow.Area}-{focusRow.Customer}:{focusRow.Product}{Environment.NewLine}");
                    }
                    else if (focusRow != null && !focusRow.Selection)
                    {
                        focusRow.Status     = string.Empty;
                        focusRow.UpdateDate = DateTime.Now;
                        var exists = _FocusData.Exists(x => x.Area     == focusRow.Area     &&
                                                            x.Customer == focusRow.Customer &&
                                                            x.Factory  == focusRow.Factory  &&
                                                            x.Product  == focusRow.Product);
                        if (exists)
                        {
                            _FocusData.Remove(focusRow);
                            this.MessageTextBox.AppendText(
                                                           $@"<color=red>{DateTime.Now:yyyy-MM-dd hh:mm:ss}</color> 已取消拉單{focusRow.Area}-{focusRow.Customer}:{focusRow.Product}{Environment.NewLine}");
                        }
                    }

                    BasicUtility.OrderInfoAdapter.AddOrUpdate(focusRow);
                    this.focusTabPage.Text = $@"拉單({_FocusData.Count})";

                    this.FocusbindingSource.DataSource = _FocusData;
                    this.pivotGrid_FocusOrder.RefreshDataAsync().ConfigureAwait(true);
                }
            }
            catch (Exception exception)
            {
                throw new Exception(exception.ToString());
            }
        }

    #endregion

    #region Infrastructure

        public OrderNew()
        {
            this.InitializeComponent();
            try
            {
                this.InitialControls();
                this.BindAddPanelControl();
                this.btn_Query.PerformClick();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        private void Order_Enter(object sender, EventArgs e)
        {
            try
            {
                this._paging = new Paging
                               {
                                   PageIndex = 0, RowSize = 10, SortExpression = "OrderDate Desc"
                               };
                this.gridView_ProductOrder.HorzScrollVisibility                  = ScrollVisibility.Always;
                this.gridView_ProductOrder.VertScrollVisibility                  = ScrollVisibility.Always;
                this.gridView_ProductOrder.OptionsDetail.AllowExpandEmptyDetails = true;

                this.gridView_ProductOrder.OptionsNavigation.EnterMoveNextColumn = true;
                this.gridView_ProductOrder.OptionsNavigation.AutoFocusNewRow     = true;
            }
            catch (Exception exception)
            {
                throw new Exception(exception.ToString());
            }
        }

    #endregion

    #region Property

        private Paging           _paging;
        List<OrderInfoViewModel> _orderInfoViewModels = new List<OrderInfoViewModel>();

        private static OrderInfoRequest _request;

        public static OrderInfoRequest _Request
        {
            get => _request ?? (_request = new OrderInfoRequest());

            set => _request = value;
        }

        private static FilterOrderInfo s_filter;

        private static FilterOrderInfo _filter
        {
            get => s_filter ?? new FilterOrderInfo();

            set => s_filter = value;
        }

        private static List<OrderInfoViewModel> _FocusData { get; set; }

        private static bool IfFocusData;

    #endregion

    #region Initial

        public void InitialControls()
        {
            this.orderInfoRequestBindingSource.AddNew();
            this.filterOrderInfoBindingSource.AddNew();
            _filter  = this.filterOrderInfoBindingSource.Current as FilterOrderInfo;
            _Request = this.orderInfoRequestBindingSource.Current as OrderInfoRequest;
            _filter.SetDefaultValue();
            _Request.SetDefaultValue();
            if (_filter != null)
            {
                _filter.OrderDateStart = DateTime.Today.AddDays(-7);
                _filter.OrderDateEnd   = DateTime.Today;
            }

            this.CustomerTextEdit.Properties.TextEditStyle = TextEditStyles.Standard;
        }

        private void BindAddPanelControl()
        {
            this.AreaKeybindingSource.DataSource     = Const.AllAreaView;
            this.CustomerKeybindingSource.DataSource = Const.AllCustomerView;
            this.FactoryKeybindingSource.DataSource  = Const.AllFactoryView;
            this.ProductKeybindingSource.DataSource  = Const.AllProductsView;

            this.AreaKeybindingSource.DataSource     = Const.AllAreaView;
            this.CustomerKeybindingSource.DataSource = Const.AllCustomerView;
            this.FactoryKeybindingSource.DataSource  = Const.AllFactoryView;
            this.ProductKeybindingSource.DataSource  = Const.AllProductsView;

            this.SizSpecTextEdit.Properties.DataSource   = GlobalCollection.SiezSpcCollection;
            this.SizSpec_LookUpEdit.DataSource           = GlobalCollection.SiezSpcCollection;
            this.ColorSpec_LookUpEdit.DataSource         = GlobalCollection.ColorSpeCollection;
            this.ColorSpecTextEdit.Properties.DataSource = GlobalCollection.ColorSpeCollection;
            this.ProductType_ComboBox.Items.AddRange(GlobalCollection.ProductTypeCollection);
            this.Status_ComboBox.Items.AddRange(GlobalCollection.OrderStatusCollection);
        }

    #endregion

    #region Add Event

        private void CustomerTextEdit_EditValueChanging(object            sender
                                                      , ChangingEventArgs e)
        {
            try
            {
                //設定客戶
                if (e.NewValue != null)
                {
                    _Request.Customer = e.NewValue.ToString();

                    //設定地區
                    _Request.Area = Const.AllCustomerView.Find(x => x.Customer == _Request.Customer)?.Area;
                }
            }
            catch (Exception exception)
            {
                throw new Exception(exception.ToString());
            }
        }

        private void ProductTextEdit_EditValueChanging(object            sender
                                                     , ChangingEventArgs e)
        {
            if (e.NewValue != null)
            {
                //設定產品
                _Request.Product = e.NewValue.ToString();

                //設定廠商
                _Request.Factory = Const.AllProductsView.Find(x => x.Product == _request.Product)?.Factory;
            }
        }

    #endregion

    #region Button

        private async void button_Query_Click(object sender, EventArgs e)
        {
            try
            {
                _orderInfoViewModels                            = await BasicUtility.OrderInfoAdapter.Query(_filter);
                this.orderInfoViewModelBindingSource.DataSource = _orderInfoViewModels;
                this.gridView_ProductOrder.BestFitColumns();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        private void OrderNew_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.F5:
                    this.barItem_Save.PerformClick();
                    break;

                case Keys.F1:
                    IfFocusData = true;
                    this.barItem_LockRow.PerformClick();

                    break;

                case Keys.F3:
                    this.barItem_Delete.PerformClick();
                    break;
            }
        }

        private async void barItem_Save_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                var productInfo = Const.AllProductsView.Find(x => x.Product == _Request.Product &&
                                                                  x.Factory == _Request.Factory);
                _Request.ProductsInfo = productInfo;
                _Request.OrderDate    = DateTime.Now;
                _Request.SetDefaultValue();
                var addOrUpdate = await BasicUtility.OrderInfoAdapter.AddOrUpdate(_Request);
                if (addOrUpdate)
                {
                    this.MessageTextBox.AppendText(
                                                   $@"{DateTime.Now:yyyy-MM-dd hh:mm:ss} 已新增{_Request.Customer}:{_Request.Product}{Environment.NewLine}");

                    this.btn_Query.PerformClick();
                }
            }
            catch (Exception exception)
            {
                throw new Exception(exception.ToString());
            }
        }

        private async void barItem_Delete_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                var deleteRequest = this.orderInfoViewModelBindingSource.Current as OrderInfoViewModel;
                var delete        = await BasicUtility.OrderInfoAdapter.Delete(deleteRequest);
                if (delete)
                {
                    this.MessageTextBox.AppendText(
                                                   $@"{DateTime.Now:yyyy-MM-dd hh:mm:ss} 已刪除{deleteRequest.Customer}:{deleteRequest.Product}{Environment.NewLine}");
                    this.btn_Query.PerformClick();
                }
            }
            catch (Exception exception)
            {
                throw new Exception(exception.ToString());
            }
        }

        private void barItem_LockRow_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (IfFocusData)
            {
                IfFocusData                  = false;
                this.barItem_LockRow.Caption = @"鎖定列";
                this.FocusbindingSource.Clear();
                this.MessageTextBox.AppendText($@"{DateTime.Now:yyyy-MM-dd hh:mm:ss} 解除拉單狀態{Environment.NewLine}");
            }
            else
            {
                IfFocusData                  = true;
                this.barItem_LockRow.Caption = @"解除鎖定列";
                this.MessageTextBox.AppendText($@"{DateTime.Now:yyyy-MM-dd hh:mm:ss} 開始拉單狀態{Environment.NewLine}");
            }

            _FocusData = new List<OrderInfoViewModel>();
        }

        private void barItem_Update_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                var updateRequest = new List<OrderInfoViewModel>();
                var pOrder        = this.orderInfoViewModelBindingSource.DataSource as List<OrderInfoViewModel>;
                var checkedList   = pOrder.Where(x => x.Selection).ToList();

                var args = new XtraInputBoxArgs
                           {
                               Caption = "批次更新是否取貨", Prompt = "請選擇其一選項", DefaultButtonIndex = 0
                           };
                var editor = new ComboBoxEdit();
                editor.Properties.Items.AddRange(GlobalCollection.OrderStatusCollection);
                args.Editor = editor;
                if (XtraInputBox.Show(args) is KeyAndNameForCombo result)
                {
                    var resultCode = result.Code;
                    var pStatus    = resultCode;

                    foreach (var order in checkedList)
                    {
                        order.Status     = pStatus;
                        order.UpdateDate = DateTime.Now;
                        order.SetDefaultValue();
                        updateRequest.Add(order);
                        BasicUtility.OrderInfoAdapter.AddOrUpdate(updateRequest);
                        _FocusData.AddOrReplace(x => x.Area     == order.Area     &&
                                                     x.Customer == order.Customer &&
                                                     x.Factory  == order.Factory  &&
                                                     x.Product  == order.Product, order);
                        this.FocusbindingSource.DataSource = _FocusData;
                        this.gridView_Focus.RefreshData();
                        this.focusTabPage.Text = $@"拉單({_FocusData.Count})";
                    }

                    this.btn_Query.PerformClick();
                }
            }
            catch (Exception exception)
            {
                throw new Exception(exception.ToString());
            }
        }

        private async void Status_ComboBox_EditValueChanging(object sender, ChangingEventArgs e)
        {
            try
            {
                if (this.orderInfoViewModelBindingSource.Current is OrderInfoViewModel orderInfoViewModel)
                {
                    orderInfoViewModel.Status     = e.NewValue.ToString();
                    orderInfoViewModel.UpdateDate = DateTime.Now;
                    orderInfoViewModel.SetDefaultValue();
                    var addOrUpdate = await BasicUtility.OrderInfoAdapter.AddOrUpdate(orderInfoViewModel);
                    if (addOrUpdate)
                    {
                        this.MessageTextBox.AppendText(
                                                       $@"<color=red>{DateTime.Now:yyyy-MM-dd hh:mm:ss}</color> {orderInfoViewModel.Customer}:{orderInfoViewModel.Product}狀態已改為:{orderInfoViewModel.Status}{Environment.NewLine}");
                        var filterOrderInfo = new FilterOrderInfo
                                              {
                                                  Area = orderInfoViewModel.Area, Customer = orderInfoViewModel.Customer
                                                , Factory = orderInfoViewModel.Factory
                                                , Product = orderInfoViewModel.Product
                                                , OrderDateStart = orderInfoViewModel.OrderDate
                                                , OrderDateEnd = orderInfoViewModel.OrderDate
                                              };
                        var result = (await BasicUtility.OrderInfoAdapter.Query(filterOrderInfo)).FirstOrDefault();
                        _orderInfoViewModels.AddOrReplace(x => x.Area     == result?.Area     &&
                                                               x.Customer == result?.Customer &&
                                                               x.Factory  == result?.Factory  &&
                                                               x.Product  == result?.Product, result);
                        this.gridView_ProductOrder.RefreshData();
                    }
                }
            }
            catch (Exception exception)
            {
                throw new Exception(exception.ToString());
            }
        }

    #endregion

    #region Export

        private void barItem_Export_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.pivotGrid_FocusOrder.ShowPrintPreview();
        }

        private void pivotGrid_FocusOrder_CustomCellValue(object                                           sender
                                                        , DevExpress.XtraPivotGrid.PivotCellValueEventArgs e)
        {
            if (e.RowField != null && e.RowField.Caption == @"客戶" & e.DataField.Caption == @"數量")
            {
                e.Value = @"總金額:";
            }
        }

    #endregion

    #region Scroll Pagging

        private async void gridView_ProductOrder_TopRowChanged(object sender, EventArgs e)
        {
            try
            {
                var sourceGridView = (GridView)sender;
                if (sourceGridView.IsRowVisible(sourceGridView.DataRowCount - 1) != RowVisibleState.Visible)
                {
                    return;
                }

                if (this._paging.TotalCount <= this._orderInfoViewModels.Count)
                {
                    return;
                }

                this._paging.PageIndex++;
                var queryResult = await BasicUtility.OrderInfoAdapter.Query(_filter, this._paging);

                this.gridView_ProductOrder.BeginUpdate();
                this._orderInfoViewModels.AddRange(queryResult);
                this.gridView_ProductOrder.EndUpdate();
            }
            catch (Exception exception)
            {
                throw new Exception(exception.ToString());
            }
        }

        private async void orderInfoViewModelBindingSource_PositionChanged(object sender, EventArgs e)
        {
            try
            {
                var source = (BindingSource)sender;
                if (source.Position != this._orderInfoViewModels.Count - 1)
                {
                    return;
                }

                if (this._paging.TotalCount == this._orderInfoViewModels.Count)
                {
                    return;
                }

                this._paging.PageIndex++;
                var queryResult = await BasicUtility.OrderInfoAdapter.Query(_filter, this._paging);
                foreach (var item in queryResult)
                {
                    this._orderInfoViewModels.Add(item);
                }

                this.gridView_ProductOrder.RefreshData();
            }
            catch (Exception exception)
            {
                throw new Exception(exception.ToString());
            }
        }

    #endregion
    }
}