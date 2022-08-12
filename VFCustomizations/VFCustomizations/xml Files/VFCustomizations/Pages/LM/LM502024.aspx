<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="LM502024.aspx.cs" Inherits="Pages_LM502024" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="VFCustomizations.Graph.LUMVFAPIInterfaceProcess" PrimaryView="VFSourceData">
        <CallbackCommands>
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid AllowPaging="True" AdjustPageSize="Auto" SyncPosition="True" ID="grid" runat="server" DataSourceID="ds" Width="100%" Height="150px" SkinID="Primary" AllowAutoHide="false">
        <Levels>
            <px:PXGridLevel DataMember="VFSourceData">
                <Columns>
                    <px:PXGridColumn AllowCheckAll="True" DataField="Selected" Width="40" Type="CheckBox" TextAlign="Center" CommitChanges="True"></px:PXGridColumn>
                    <px:PXGridColumn DataField="Apiname" Width="150px"></px:PXGridColumn>
                    <px:PXGridColumn DataField="UniqueID" Width="150px"></px:PXGridColumn>
                    <px:PXGridColumn DataField="ServiceType" Width="120px"></px:PXGridColumn>
                    <px:PXGridColumn DataField="IsProcessed" Width="130px" Type="CheckBox"></px:PXGridColumn>
                    <px:PXGridColumn DataField="ErrorMessage" Width="200px"></px:PXGridColumn>
                    <px:PXGridColumn DataField="CreatedDateTime" Width="150px" DisplayFormat="g"></px:PXGridColumn>
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150"></AutoSize>
        <ActionBar>
        </ActionBar>
        <Mode AllowUpload="True" />
    </px:PXGrid>
    <px:PXSmartPanel ID="pnlJsonPanel" runat="server" CaptionVisible="True" Caption="JsonViewer"
        Style="position: static" LoadOnDemand="True" Key="JsonViewer" AutoCallBack-Target="frmMyCommand"
        AutoCallBack-Command="Refresh" DesignView="Content">
        <px:PXFormView ID="frmMyCommand" runat="server" SkinID="Transparent" DataMember="JsonViewer" DataSourceID="ds" EmailingGraph="">
            <Template>
                <px:PXLayoutRule runat="server" ControlSize="L" LabelsWidth="L" StartColumn="True" />
                <px:PXRichTextEdit ID="PXRichTextEdit1" runat="server" DataField="JsonSource"></px:PXRichTextEdit>
                <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
                    <px:PXButton ID="btnMyCommandCancel" runat="server" DialogResult="Cancel" Text="Confirm" />
                </px:PXPanel>
            </Template>
        </px:PXFormView>
    </px:PXSmartPanel>
</asp:Content>
