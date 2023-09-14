<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="LM503000.aspx.cs" Inherits="Page_LM503000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%"
        TypeName="HSNHighcareCistomizations.Graph.HighcareReturnProcess"
        PrimaryView="Filter">
        <CallbackCommands></CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="Filter" Width="100%" Height="80px" AllowAutoHide="false">
        <Template>
            <px:PXDropDown runat="server" ID="edProcessType" DataField="ProcessType" CommitChanges="true" Width="300px"></px:PXDropDown>
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid SyncPosition="True" ID="grid" runat="server" DataSourceID="ds" Width="100%" Height="150px" ActionsPosition="Top" SkinID="PrimaryInquire" AllowAutoHide="false">
        <Levels>
            <px:PXGridLevel DataMember="Transactions">
                <Columns>
                    <px:PXGridColumn AllowCheckAll="True" DataField="Selected" Width="40" Type="CheckBox" TextAlign="Center" CommitChanges="True"></px:PXGridColumn>
                    <px:PXGridColumn DataField="DocType"></px:PXGridColumn>
                    <px:PXGridColumn DataField="RefNbr"></px:PXGridColumn>
                    <px:PXGridColumn DataField="Status"></px:PXGridColumn>
                    <px:PXGridColumn DataField="DocDate"></px:PXGridColumn>
                    <px:PXGridColumn DataField="CustomerID"></px:PXGridColumn>
                    <px:PXGridColumn DataField="CuryOrigDocAmt"></px:PXGridColumn>
                    <px:PXGridColumn DataField="SOInvoice__SOOrderNbr"></px:PXGridColumn>
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150"></AutoSize>
        <ActionBar>
        </ActionBar>
    </px:PXGrid>
</asp:Content>
