﻿<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="LM502021.aspx.cs" Inherits="Pages_LM502021" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="VFCustomizations.Graph.LUMVFAPI3001Process" PrimaryView="Transactions">
        <CallbackCommands>
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<%--<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="Filter" Width="100%" Height="50px" AllowAutoHide="false">
		<Template>
			<px:PXDateTimeEdit runat="server" ID="CstPXDateTimeEdit2" DataField="StartDate" CommitChanges="True" ></px:PXDateTimeEdit>
			<px:PXLayoutRule runat="server" ID="CstPXLayoutRule3" StartColumn="True" ></px:PXLayoutRule>
			<px:PXDateTimeEdit CommitChanges="True" runat="server" ID="CstPXDateTimeEdit1" DataField="EndDate" ></px:PXDateTimeEdit></Template>
	</px:PXFormView>
</asp:Content>--%>
<asp:Content ID="cont3" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid AllowPaging="True" SyncPosition="True" ID="grid" runat="server" DataSourceID="ds" Width="100%" Height="150px" SkinID="Primary" AllowAutoHide="false" PageSize="10">
        <Levels>
            <px:PXGridLevel DataMember="Transactions">
                <Columns>
                    <px:PXGridColumn AllowCheckAll="True" DataField="Selected" Width="40" Type="CheckBox" TextAlign="Center" CommitChanges="True"></px:PXGridColumn>
                    <px:PXGridColumn DataField="ShipmentNbr" Width="150"></px:PXGridColumn>
                    <px:PXGridColumn DataField="ShipmentType" Width="150"></px:PXGridColumn>
                    <px:PXGridColumn DataField="Status" Width="150"></px:PXGridColumn>
                    <px:PXGridColumn DataField="ShipDate" Width="150"></px:PXGridColumn>
                    <px:PXGridColumn DataField="CustomerID" Width="150"></px:PXGridColumn>
                    <px:PXGridColumn DataField="ShipmentDesc" Width="150"></px:PXGridColumn>
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150"></AutoSize>
        <ActionBar PagerVisible="Bottom">
            <PagerSettings Mode="NumericCompact" />
        </ActionBar>
        <Mode AllowAddNew="False" AllowDelete="False" />
    </px:PXGrid>
</asp:Content>
