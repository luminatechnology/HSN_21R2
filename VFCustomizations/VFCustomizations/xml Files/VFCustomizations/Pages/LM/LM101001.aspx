<%@ Page Language="C#" MasterPageFile="~/MasterPages/TabView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="LM101001.aspx.cs" Inherits="Page_LM101001" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/TabView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%"
        TypeName="VFCustomizations.Graph.LUMVFPreferenceMaint" PrimaryView="VFPreference">
        <CallbackCommands>
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="VFPreference" Caption="API SETTING">
        <Template>
            <px:PXLayoutRule GroupCaption="API SETTING" runat="server" ID="CstPXLayoutRule1" StartGroup="True" LabelsWidth="L" ControlSize=""></px:PXLayoutRule>
            <px:PXTextEdit runat="server" ID="edUsername" DataField="Username"></px:PXTextEdit>
            <px:PXTextEdit runat="server" ID="edPassword" DataField="Password"></px:PXTextEdit>
            <px:PXTextEdit runat="server" ID="edAccessTokenURL" DataField="AccessTokenURL"></px:PXTextEdit>
            <px:PXTextEdit runat="server" ID="edApi3001url" DataField="Api3001url"></px:PXTextEdit>
            <px:PXTextEdit runat="server" ID="edApi6001url" DataField="Api6001url"></px:PXTextEdit>
            <px:PXTextEdit runat="server" ID="edApi2101url" DataField="Api2101url"></px:PXTextEdit>
            <px:PXTextEdit runat="server" ID="edApi2102url" DataField="Api2102url"></px:PXTextEdit>
            <px:PXLayoutRule GroupCaption="OTHER SETTING" runat="server" ID="PXLayoutRule1" StartGroup="True" LabelsWidth="L" ControlSize=""></px:PXLayoutRule>
            <px:PXCheckBox runat="server" ID="edEnableVFCustomizeField" DataField="EnableVFCustomizeField" AlignLeft="True"></px:PXCheckBox>
            <px:PXCheckBox runat="server" ID="edEnableValidationSerialNbr" DataField="EnableValidationSerialNbr" AlignLeft="True"></px:PXCheckBox>
        </Template>
    </px:PXFormView>
</asp:Content>
