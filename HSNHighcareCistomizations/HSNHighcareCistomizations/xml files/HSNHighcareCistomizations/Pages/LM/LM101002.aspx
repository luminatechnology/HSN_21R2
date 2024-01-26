<%@ Page Language="C#" MasterPageFile="~/MasterPages/TabView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="LM101002.aspx.cs" Inherits="Page_LM101002" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/TabView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%"
        TypeName="HSNHighcareCistomizations.Graph.HighcarePreferenceMaint" PrimaryView="HighcarePreference">
        <CallbackCommands>
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="HighcarePreference" Caption="API SETTING">
        <Template>
            <px:PXLayoutRule GroupCaption="API SETTING" runat="server" ID="CstPXLayoutRule1" StartGroup="True" LabelsWidth="L" ControlSize=""></px:PXLayoutRule>
            <px:PXTextEdit runat="server" ID="edReturnUrl" DataField="ReturnUrl"></px:PXTextEdit>
            <px:PXTextEdit runat="server" ID="edSecretKey" DataField="SecretKey" TextMode="Password"></px:PXTextEdit>
        </Template>
    </px:PXFormView>
</asp:Content>
