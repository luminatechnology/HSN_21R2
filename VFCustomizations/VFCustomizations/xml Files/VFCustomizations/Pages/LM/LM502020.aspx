<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="LM502020.aspx.cs" Inherits="Pages_LM502020" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="VFCustomizations.Graph.LUMVFAPI2101Process" PrimaryView="Transactions">
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
    <px:PXGrid AllowPaging="True" AdjustPageSize="Auto" SyncPosition="True" ID="grid" runat="server" DataSourceID="ds" Width="100%" Height="150px" SkinID="Primary" AllowAutoHide="false">
        <Levels>
            <px:PXGridLevel DataMember="Transactions">
                <Columns>
                    <px:PXGridColumn AllowCheckAll="True" DataField="Selected" Width="40" Type="CheckBox" TextAlign="Center" CommitChanges="True"></px:PXGridColumn>
                    <px:PXGridColumn DataField="ShipmentNbr" Width="150"></px:PXGridColumn>
                    <px:PXGridColumn DataField="LineNbr" Width="150"></px:PXGridColumn>
                    <px:PXGridColumn DataField="JobNo" Width="150"></px:PXGridColumn>
                    <px:PXGridColumn DataField="StartDateTime" Width="150" DisplayFormat="g"></px:PXGridColumn>
                    <px:PXGridColumn DataField="FinishDateTime" Width="150" DisplayFormat="g"></px:PXGridColumn>
                    <px:PXGridColumn DataField="TerminalID" Width="150"></px:PXGridColumn>
                    <px:PXGridColumn DataField="SerialNo" Width="150"></px:PXGridColumn>
                    <px:PXGridColumn DataField="SetupReason" Width="150"></px:PXGridColumn>
                    <px:PXGridColumn DataField="ProcessedDateTime" Width="150" DisplayFormat="g"></px:PXGridColumn>
                </Columns>
                <RowTemplate>
                    <px:PXTimeEdit ID="edStartDateTime" runat="server" DisplayFormat="g" DataField="StartDateTime"></px:PXTimeEdit>
                    <px:PXTimeEdit ID="edFinishDateTime" runat="server" DisplayFormat="g" DataField="FinishDateTime"></px:PXTimeEdit>
                    <px:PXTimeEdit ID="edProcessedDateTime" runat="server" DisplayFormat="g" DataField="StartDateTime"></px:PXTimeEdit>
                </RowTemplate>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150"></AutoSize>
        <ActionBar>
        </ActionBar>
        <Mode AllowAddNew="False" AllowDelete="False" />
    </px:PXGrid>
</asp:Content>
