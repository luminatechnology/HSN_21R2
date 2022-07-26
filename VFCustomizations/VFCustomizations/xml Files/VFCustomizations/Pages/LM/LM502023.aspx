<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="LM502023.aspx.cs" Inherits="Pages_LM502023" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="VFCustomizations.Graph.LUMVFAPI2102Process" PrimaryView="Transactions">
        <CallbackCommands>
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid AllowPaging="True" AdjustPageSize="Auto" SyncPosition="True" ID="grid" runat="server" DataSourceID="ds" Width="100%" Height="150px" SkinID="Primary" AllowAutoHide="false">
        <Levels>
            <px:PXGridLevel DataMember="Transactions">
                <Columns>
                    <px:PXGridColumn AllowCheckAll="True" DataField="Selected" Width="40" Type="CheckBox" TextAlign="Center" CommitChanges="True"></px:PXGridColumn>
                    <px:PXGridColumn DataField="ShipmentNbr" Width="150"></px:PXGridColumn>
                    <px:PXGridColumn DataField="LineNbr" Width="150"></px:PXGridColumn>
                    <px:PXGridColumn DataField="JobNo" Width="150"></px:PXGridColumn>
                    <px:PXGridColumn DataField="IncidentCatalogName" Width="150"></px:PXGridColumn>
                    <px:PXGridColumn DataField="PreviousCommitDate" Width="150" DisplayFormat="g"></px:PXGridColumn>
                    <px:PXGridColumn DataField="CommitDate" Width="150"></px:PXGridColumn>
                    <px:PXGridColumn DataField="HoldReason" Width="200"></px:PXGridColumn>
                    <px:PXGridColumn DataField="HoldDate" Width="150"></px:PXGridColumn>
                    <px:PXGridColumn DataField="HoldSatus" Width="150"></px:PXGridColumn>
                </Columns>
                <RowTemplate>
                    <px:PXTimeEdit ID="edPreviousCommitDate" runat="server" DisplayFormat="g" DataField="PreviousCommitDate"></px:PXTimeEdit>
                </RowTemplate>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150"></AutoSize>
        <ActionBar>
        </ActionBar>
        <Mode AllowAddNew="False" AllowDelete="False" />
    </px:PXGrid>
</asp:Content>
