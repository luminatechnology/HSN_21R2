﻿<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="LM502022.aspx.cs" Inherits="Pages_LM502022" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="VFCustomizations.Graph.LUMVFAPI6001Process" PrimaryView="Transactions">
        <CallbackCommands>
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid SyncPosition="True" ID="grid" runat="server" DataSourceID="ds" Width="100%" Height="150px" SkinID="Primary" AllowAutoHide="false">
        <Levels>
            <px:PXGridLevel DataMember="Transactions">
                <Columns>
                    <px:PXGridColumn AllowCheckAll="True" DataField="Selected" Width="40" Type="CheckBox" TextAlign="Center" CommitChanges="True"></px:PXGridColumn>
                    <px:PXGridColumn DataField="RefNbr" Width="150"></px:PXGridColumn>
                    <px:PXGridColumn DataField="TranDate" Width="150"></px:PXGridColumn>
                    <px:PXGridColumn DataField="FinPeriodID" Width="150"></px:PXGridColumn>
                    <px:PXGridColumn DataField="ExtRefNbr" Width="150"></px:PXGridColumn>
                    <px:PXGridColumn DataField="TranDesc" Width="150"></px:PXGridColumn>
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150"></AutoSize>
        <ActionBar>
        </ActionBar>
        <Mode AllowAddNew="False" AllowDelete="False" />
    </px:PXGrid>
</asp:Content>
