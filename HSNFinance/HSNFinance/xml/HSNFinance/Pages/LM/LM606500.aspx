<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="LM606500.aspx.cs" Inherits="Page_LM606500" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%"
        TypeName="HSNFinance.Graph.LumINItemCostHistMaint"
        PrimaryView="MasterViewFilter">
        <CallbackCommands>
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="MasterViewFilter" Width="100%" Height="45px" AllowAutoHide="false">
        <Template>
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartRow="True"></px:PXLayoutRule>
            <px:PXSelector CommitChanges="True" runat="server" ID="CstPXSelector1" DataField="FinPeriodID"></px:PXSelector>
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Width="100%" Height="150px" SkinID="Details" AllowAutoHide="false">
        <Levels>
            <px:PXGridLevel DataMember="DetailsView">
                <Columns>
                    <px:PXGridColumn DataField="InventoryID" Width="70"></px:PXGridColumn>
                    <px:PXGridColumn DataField="WareHouseID_SiteCD" Width="140"></px:PXGridColumn>
                    <px:PXGridColumn DataField="EndingQty_FinYtdQty" Width="100"></px:PXGridColumn>
                    <px:PXGridColumn TextField="" DataField="PeriodQtyWithin30D" Width="100"></px:PXGridColumn>
                    <px:PXGridColumn DataField="PeriodQtyFrom30Dto60D" Width="100"></px:PXGridColumn>
                    <px:PXGridColumn DataField="PeriodQtyFrom60Dto90D" Width="100"></px:PXGridColumn>
                    <px:PXGridColumn DataField="PeriodQtyFrom90Dto120D" Width="100"></px:PXGridColumn>
                    <px:PXGridColumn DataField="PeriodQtyFrom120Dto150D" Width="100"></px:PXGridColumn>
                    <px:PXGridColumn DataField="PeriodQtyFrom150Dto180D" Width="100"></px:PXGridColumn>
                    <px:PXGridColumn DataField="PeriodQtyFrom180Dto210D" Width="100"></px:PXGridColumn>
                    <px:PXGridColumn DataField="PeriodQtyFrom210Dto240D" Width="100"></px:PXGridColumn>
                    <px:PXGridColumn DataField="PeriodQtyFrom240Dto270D" Width="100"></px:PXGridColumn>
                    <px:PXGridColumn DataField="PeriodQtyFrom270Dto300D" Width="100"></px:PXGridColumn>
                    <px:PXGridColumn DataField="PeriodQtyFrom300Dto330D" Width="100"></px:PXGridColumn>
                    <px:PXGridColumn DataField="PeriodQtyFrom330Dto360D" Width="100"></px:PXGridColumn>
                    <px:PXGridColumn DataField="PeriodQtyFrom13Mto24M" Width="100"></px:PXGridColumn>
                    <px:PXGridColumn DataField="PeriodQtyFrom25Mto36M" Width="100"></px:PXGridColumn>
                    <px:PXGridColumn DataField="PeriodQtyFrom37Mto48M" Width="100"></px:PXGridColumn>
                    <px:PXGridColumn DataField="PeriodQtyOver4Y" Width="100"></px:PXGridColumn>
                    <px:PXGridColumn DataField="EndingCost_FinYtdCost" Width="100"></px:PXGridColumn>
                    <px:PXGridColumn DataField="PeriodCostWithin30D" Width="100"></px:PXGridColumn>
                    <px:PXGridColumn DataField="PeriodCostFrom30Dto60D" Width="100"></px:PXGridColumn>
                    <px:PXGridColumn DataField="PeriodCostFrom60Dto90D" Width="100"></px:PXGridColumn>
                    <px:PXGridColumn DataField="PeriodCostFrom90Dto120D" Width="100"></px:PXGridColumn>
                    <px:PXGridColumn DataField="PeriodCostFrom120Dto150D" Width="100"></px:PXGridColumn>
                    <px:PXGridColumn DataField="PeriodCostFrom150Dto180D" Width="100"></px:PXGridColumn>
                    <px:PXGridColumn DataField="PeriodCostFrom180Dto210D" Width="100"></px:PXGridColumn>
                    <px:PXGridColumn DataField="PeriodCostFrom210Dto240D" Width="100"></px:PXGridColumn>
                    <px:PXGridColumn DataField="PeriodCostFrom240Dto270D" Width="100"></px:PXGridColumn>
                    <px:PXGridColumn DataField="PeriodCostFrom270Dto300D" Width="100"></px:PXGridColumn>
                    <px:PXGridColumn DataField="PeriodCostFrom300Dto330D" Width="100"></px:PXGridColumn>
                    <px:PXGridColumn DataField="PeriodCostFrom330Dto360D" Width="100"></px:PXGridColumn>
                    <px:PXGridColumn DataField="PeriodCostFrom13Mto24M" Width="100"></px:PXGridColumn>
                    <px:PXGridColumn DataField="PeriodCostFrom25Mto36M" Width="100"></px:PXGridColumn>
                    <px:PXGridColumn DataField="PeriodCostFrom37Mto48M" Width="100"></px:PXGridColumn>
                    <px:PXGridColumn DataField="PeriodCostOver4Y" Width="100"></px:PXGridColumn>
                    <px:PXGridColumn DataField="ItemDescr" Width="280"></px:PXGridColumn>
                    <px:PXGridColumn DataField="ItemClassCD" Width="140"></px:PXGridColumn>
                    <px:PXGridColumn DataField="ItemClassDescr" Width="280"></px:PXGridColumn>
                    <px:PXGridColumn DataField="WareHouse_SiteID_Descr" Width="250"></px:PXGridColumn>
                    <px:PXGridColumn DataField="LastActivityPeriod" Width="70"></px:PXGridColumn>
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <ActionBar>
        </ActionBar>
    </px:PXGrid>
</asp:Content>
