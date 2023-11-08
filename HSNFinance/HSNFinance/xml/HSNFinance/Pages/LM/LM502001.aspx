<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="LM502001.aspx.cs" Inherits="Page_LM502001" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%"
        TypeName="HSNFinance.Graph.LUMPrepaymentForCashSalesProcess"
        PrimaryView="Filter">
        <CallbackCommands>
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="Filter" Width="100%" Height="80px" AllowAutoHide="false">
        <Template>
            <px:PXSelector runat="server" ID="edCustomerID" DataField="CustomerID" CommitChanges="true" Width="250px"></px:PXSelector>
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid SyncPosition="True" ID="grid" runat="server" DataSourceID="ds" Width="100%" Height="150px" ActionsPosition="Top" SkinID="PrimaryInquire" AllowAutoHide="false">
        <Levels>
            <px:PXGridLevel DataMember="Transactions">
                <Columns>
                    <px:PXGridColumn TextAlign="Center" AllowCheckAll="True" Type="CheckBox" DataField="Selected" Width="60"></px:PXGridColumn>
                    <px:PXGridColumn DataField="CustomerID" Width="140"></px:PXGridColumn>
                    <px:PXGridColumn DataField="PrepaymentRefNbr" Width="280"></px:PXGridColumn>
                    <px:PXGridColumn DataField="Status" Width="180"></px:PXGridColumn>
                    <px:PXGridColumn DataField="InvoiceNbr" Width="120"></px:PXGridColumn>
                    <px:PXGridColumn DataField="CuryExtPrice" Width="120"></px:PXGridColumn>
                    <px:PXGridColumn DataField="CuryUnappliedBal" Width="120"></px:PXGridColumn>
                </Columns>
                <RowTemplate>
                    <px:PXSelector runat="server" ID="edGridCustomerID" DataField="CustomerID"></px:PXSelector>
                </RowTemplate>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150"></AutoSize>
        <ActionBar>
        </ActionBar>
    </px:PXGrid>
</asp:Content>
