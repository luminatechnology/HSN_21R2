<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="LM202000.aspx.cs"
    Inherits="Page_LM202000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Document" TypeName="HSNCustomizations.Graph.QuestionnaireTypeMaint">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand Name="Delete" PostData="Self" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <%--<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />--%>
            <%--<px:PXDSCallbackCommand Name="Last" PostData="Self" />--%>
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="Document" Width="100%" Height="80px" AllowAutoHide="false">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXSelector runat="server" ID="editQuestionnaireType" DataField="QuestionnaireType" CommitChanges="True" Width="120px"></px:PXSelector>
            <px:PXTextEdit runat="server" ID="editDescription" DataField="Description" Width="300px"></px:PXTextEdit>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Height="400px" Style="z-index: 100;" Width="100%" DataMember="Mapping" DataSourceID="ds">
        <Items>
            <px:PXTabItem Text="Attributes">
                <Template>
                    <px:PXLayoutRule runat="server" StartRow="true" StartGroup="true" LabelsWidth="S" ControlSize="XM" />
                    <px:PXGrid ID="AttributesGrid" runat="server" SkinID="Details" DataSourceID="ds" Width="100%" Height="280px" BorderWidth="0px">
                        <Levels>
                            <px:PXGridLevel DataMember="Mapping">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                    <px:PXSelector ID="edCRAttributeID" runat="server" DataField="AttributeID" AutoRefresh="true" FilterByAllFields="True" />
                                    <px:PXTextEdit ID="edDescription2" runat="server" AllowNull="False" DataField="Description" />
                                    <px:PXCheckBox ID="chkRequired" runat="server" DataField="Required" />
                                    <px:PXNumberEdit ID="edSortOrder" runat="server" DataField="SortOrder" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="IsActive" AllowNull="False" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="AttributeID" DisplayFormat="&gt;aaaaaaaaaa" Width="81px" AutoCallBack="True" LinkCommand="CRAttribute_ViewDetails" />
                                    <px:PXGridColumn AllowNull="False" DataField="Description" Width="351px" />
                                    <px:PXGridColumn DataField="SortOrder" TextAlign="Right" Width="54px" />
                                    <px:PXGridColumn AllowNull="False" DataField="Required" TextAlign="Center" Type="CheckBox" CommitChanges="true" />
                                    <px:PXGridColumn AllowNull="False" DataField="ControlType" Type="DropDownList" Width="63px" />
                                    <px:PXGridColumn AllowNull="False" DataField="AttributeCategory" Type="DropDownList" CommitChanges="true" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="False" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
        </Items>
    </px:PXTab>
</asp:Content>
