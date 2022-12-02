<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="LM201001.aspx.cs" Inherits="Page_LM201001" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="HSNFinance.Graph.LUMRevenueInventoryAccountMaint" PrimaryView="RevenueAcctMapping">
        <CallbackCommands>
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid SyncPosition="True" ID="grid" runat="server" DataSourceID="ds" Width="100%" SkinID="Primary" AllowAutoHide="false" AllowPaging="True">
        <Levels>
            <px:PXGridLevel DataMember="RevenueAcctMapping">
                <Columns>
                    <px:PXGridColumn DataField="SrvOrderType" Width="120" />
                    <px:PXGridColumn DataField="AccountID" Width="150" />
                    <px:PXGridColumn DataField="SubAccountID" Width="150" />
                    <px:PXGridColumn DataField="RevenueReasonCode" Width="200" />
                </Columns>
                <RowTemplate>
                    <px:PXSelector runat="server" ID="edSrvOrderType" DataField="SrvOrderType" AutoRefresh="true"></px:PXSelector>
                    <px:PXSelector runat="server" ID="edAccountID" DataField="AccountID"></px:PXSelector>
                    <px:PXSelector runat="server" ID="edSubAccountID" DataField="SubAccountID" />
                    <px:PXSelector runat="server" ID="edRevenueReasonCode" DataField="RevenueReasonCode" AutoRefresh="true"></px:PXSelector>
                </RowTemplate>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True"></AutoSize>
        <ActionBar PagerVisible="Bottom">
            <PagerSettings Mode="NumericCompact" />
        </ActionBar>
        <Mode AllowUpload="True" />
    </px:PXGrid>
</asp:Content>
