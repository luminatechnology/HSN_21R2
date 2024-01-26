<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="LM502030.aspx.cs" Inherits="Page_LM502030" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="HSNCustomizations.Graph.LUMPrintInventoryBarcodeLabels" PrimaryView="Filter">
		<CallbackCommands>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="Filter" Width="100%" Height="50px" AllowAutoHide="false">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM"/>
			<px:PXNumberEdit runat="server" ID="CstPXNumberEdit1" DataField="StartSeq" AllowNull="false" ></px:PXNumberEdit>
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Width="100%" Height="150px" SkinID="Inquire" AllowAutoHide="false" SyncPosition="true">
		<Levels>
			<px:PXGridLevel DataMember="Result">
				<RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                    <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="True" />
                </RowTemplate>
			    <Columns>
					<px:PXGridColumn DataField="SortOrder" Width="80"></px:PXGridColumn>
			        <px:PXGridColumn DataField="InventoryID" Width="150"></px:PXGridColumn>
					<px:PXGridColumn DataField="InventoryID_Description" Width="300"></px:PXGridColumn>
                    <px:PXGridColumn DataField="PrintQty" Width="140" Decimals="0"></px:PXGridColumn>
			    </Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<Mode AllowUpload="true" AllowAddNew="false" AllowDelete="false" />
		<ActionBar>
		</ActionBar>
	</px:PXGrid>
</asp:Content>