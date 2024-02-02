<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="LM405000.aspx.cs" Inherits="Page_LM405000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%"
        TypeName="HSNCustomizations.Graph.LUMWarrantyHistoryQuery"
        PrimaryView="Filter">
        <CallbackCommands>
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>

<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid SyncPosition="True" ID="grid" runat="server" DataSourceID="ds" Width="100%" Height="150px" ActionsPosition="Top" SkinID="PrimaryInquire" AllowAutoHide="false">
        <Levels>
            <px:PXGridLevel DataMember="WarrantyHistory">
                <Columns>
                    <px:PXGridColumn DataField="WarrantySerialNbr" Width="200px"></px:PXGridColumn>
                    <px:PXGridColumn DataField="WarrantyStartDate" Width="150px"></px:PXGridColumn>
                    <px:PXGridColumn DataField="WarrantyMonths" Width="100px"></px:PXGridColumn>
                    <px:PXGridColumn DataField="WarrantyEndDate" Width="150px"></px:PXGridColumn>
                    <px:PXGridColumn DataField="Component" Width="130px"></px:PXGridColumn>
                    <px:PXGridColumn DataField="Remark" Width="200px"></px:PXGridColumn>
                    <px:PXGridColumn DataField="CreatedByID" Width="150px"></px:PXGridColumn>
                </Columns>
                <Mode AllowAddNew="False" AllowUpdate="False" />
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150"></AutoSize>
        <ActionBar>
        </ActionBar>
    </px:PXGrid>
</asp:Content>
