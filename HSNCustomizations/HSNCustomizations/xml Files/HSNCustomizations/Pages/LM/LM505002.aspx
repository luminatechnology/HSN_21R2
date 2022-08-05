﻿<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="LM505002.aspx.cs" Inherits="Page_LM505002" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%"
        TypeName="HSNCustomizations.Graph.ProcessSCBPaymentRefundMaint"
        PrimaryView="Filter">
        <CallbackCommands>
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection" DefaultControlID="edPayTypeID">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M"></px:PXLayoutRule>
            <px:PXSelector CommitChanges="True" ID="edPayTypeID" runat="server" DataField="PayTypeID" AutoRefresh="True"></px:PXSelector>
            <px:PXSegmentMask CommitChanges="True" ID="edPayAccountID" runat="server" DataField="PayAccountID" AutoRefresh="True"></px:PXSegmentMask>
            <px:PXDateTimeEdit CommitChanges="True" ID="edAdjDate" runat="server" DataField="AdjDate"></px:PXDateTimeEdit>
            <%--<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" ></px:PXLayoutRule>--%>
            <%--            <px:PXNumberEdit ID="edGLBalance" runat="server" DataField="GLBalance" Enabled="False" ></px:PXNumberEdit>
            <px:PXNumberEdit ID="edCashBalance" runat="server" DataField="CashBalance" Enabled="False" ></px:PXNumberEdit>
            <px:PXSelector ID="edCuryID" runat="server" DataField="CuryID" ></px:PXSelector>--%>
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="288px" Style="z-index: 100" Width="100%" Caption="ARPayments" AllowPaging="true" AdjustPageSize="Auto"
        SkinID="PrimaryInquire" SyncPosition="True" FastFilterFields="ExtRefNbr,RefNbr">
        <Levels>
            <px:PXGridLevel DataMember="ARPaymentList">
                <Columns>
                    <px:PXGridColumn DataField="ExtRefNbr"></px:PXGridColumn>
                    <px:PXGridColumn DataField="RefNbr" LinkCommand="viewDocument"></px:PXGridColumn>
                    <px:PXGridColumn DataField="CustomerID"></px:PXGridColumn>
                    <px:PXGridColumn DataField="UsrBankSwiftAttributes" Width="280" />
                    <px:PXGridColumn DataField="UsrBankAccnamattributes" Width="280" />
                    <px:PXGridColumn DataField="UsrBankAccNbrttributes" Width="280" />
                    <px:PXGridColumn DataField="CuryOrigDocAmt" TextAlign="Right"></px:PXGridColumn>
                    <px:PXGridColumn DataField="AdjDate"></px:PXGridColumn>
                    <px:PXGridColumn DataField="DocType"></px:PXGridColumn>
                </Columns>
                <RowTemplate></RowTemplate>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="400"></AutoSize>
    </px:PXGrid>
</asp:Content>
