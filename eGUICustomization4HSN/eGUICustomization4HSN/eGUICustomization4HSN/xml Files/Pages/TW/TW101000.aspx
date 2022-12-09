<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="TW101000.aspx.cs" Inherits="Page_TW101000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%"
        TypeName="eGUICustomization4HSN.Graph.TWNGUIPrefMaint"
        PrimaryView="GUIPreferences">
		<CallbackCommands>

		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView SyncPosition="True" ID="form" runat="server" DataSourceID="ds" DataMember="GUIPreferences" Width="100%" AllowAutoHide="false">
		<Template>
			<px:PXLayoutRule ControlSize="M" LabelsWidth="M" runat="server" ID="CstPXLayoutRule6" StartRow="True" ></px:PXLayoutRule>
			<px:PXLayoutRule runat="server" ID="CstPXLayoutRule8" StartGroup="True" GroupCaption="Numbering Settings" ></px:PXLayoutRule>
			<px:PXSelector runat="server" ID="CstPXSelector10" DataField="GUI3CopiesNumbering" AllowEdit="True" ></px:PXSelector>
			<px:PXSelector runat="server" ID="CstPXSelector11" DataField="GUI2CopiesNumbering" AllowEdit="True" ></px:PXSelector>
			<px:PXSelector AllowEdit="True" runat="server" ID="CstPXSelector12" DataField="MediaFileNumbering" ></px:PXSelector>
			<px:PXLayoutRule ControlSize="M" LabelsWidth="M" runat="server" ID="CstPXLayoutRule7" StartRow="True" ></px:PXLayoutRule>
			<px:PXLayoutRule LabelsWidth="M" runat="server" ID="CstPXLayoutRule9" StartGroup="True" GroupCaption="Registration" ></px:PXLayoutRule>
			<px:PXTextEdit runat="server" ID="CstPXTextEdit1" DataField="TaxRegistrationID" ></px:PXTextEdit>
			<px:PXTextEdit runat="server" ID="CstPXTextEdit2" DataField="OurTaxNbr" CommitChanges="True"  ></px:PXTextEdit>
			<px:PXTextEdit runat="server" ID="CstPXTextEdit3" DataField="ZeroTaxTaxCntry" ></px:PXTextEdit>
			<px:PXTextEdit runat="server" ID="CstPXTextEdit4" DataField="CompanyName" ></px:PXTextEdit>
			<px:PXTextEdit runat="server" ID="CstPXTextEdit5" DataField="AddressLine" ></px:PXTextEdit>
			<px:PXTextEdit runat="server" ID="CstPXTextEdit6" DataField="AESKey" />
			<px:PXSegmentMask AllowEdit="True" runat="server" ID="CstPXSegmentMask24" DataField="PlasticBag" ></px:PXSegmentMask>
			<px:PXLayoutRule runat="server" ID="CstPXLayoutRule16" StartRow="True" ControlSize="M" ></px:PXLayoutRule>
			<px:PXLayoutRule runat="server" ID="CstPXLayoutRule17" StartGroup="True" GroupCaption="FTP Info" LabelsWidth="M" ></px:PXLayoutRule>
			<px:PXTextEdit runat="server" ID="CstPXTextEdit7" DataField="Url" ></px:PXTextEdit>
			<px:PXTextEdit runat="server" ID="CstPXTextEdit8" DataField="UserName" ></px:PXTextEdit>
			<px:PXTextEdit runat="server" ID="CstPXTextEdit9" DataField="Password" ></px:PXTextEdit>
			<px:PXLayoutRule runat="server" ID="PXLayoutRule1" StartGroup="True" GroupCaption="Online Store FTP Info" LabelsWidth="M" ></px:PXLayoutRule>
			<px:PXTextEdit runat="server" ID="CstPXTextEdit10" DataField="OnlineUrl" ></px:PXTextEdit>
			<px:PXTextEdit runat="server" ID="CstPXTextEdi11" DataField="OnlineUN" ></px:PXTextEdit>
			<px:PXTextEdit runat="server" ID="CstPXTextEdit12" DataField="OnlinePW" ></px:PXTextEdit></Template>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" ></AutoSize>
	</px:PXFormView>
</asp:Content>