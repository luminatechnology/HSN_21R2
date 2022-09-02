<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="LM202001.aspx.cs"
    Inherits="Page_LM202001" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Document" TypeName="HSNCustomizations.Graph.LUMApptQuestionnaireResultMaint">
        <CallbackCommands>
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="Document" Width="100%" Height="150px" AllowAutoHide="false" Caption="Questionnaire Result">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <%--<px:PXSelector runat="server" ID="editSrvOrdType" DataField="SrvOrdType" CommitChanges="True" Width="120px" AutoRefresh="True"></px:PXSelector>--%>
            <px:PXTextEdit runat="server" ID="editLineNbr" DataField="UniqueID" CommitChanges="True"></px:PXTextEdit>
            <px:PXSelector runat="server" ID="editApptRefNbr" DataField="ApptRefNbr" CommitChanges="True" Width="120px" AutoRefresh="True"></px:PXSelector>
            <px:PXSelector runat="server" ID="editQuestionnaireType" DataField="QuestionnaireType" Width="300px" CommitChanges="True"></px:PXSelector>
            <px:PXSelector ID="edUBranchID" runat="server" DataField="BranchID" CommitChanges="True" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXSelector runat="server" ID="editCustomerID" DataField="CustomerID"></px:PXSelector>
            <px:PXDateTimeEdit runat="server" ID="editExecutionDate" DataField="ExecutionDate"></px:PXDateTimeEdit>
            <px:PXSelector runat="server" ID="editeContactID" DataField="ContactID"></px:PXSelector>
            <px:PXTextEdit runat="server" ID="editPhone1" DataField="Contact__Phone1"></px:PXTextEdit>
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Height="400px" Style="z-index: 100;" Width="100%" DataMember="Answers" DataSourceID="ds">
        <Items>
            <px:PXTabItem Text="Questionnaire">
                <Template>
                    <px:PXGrid ID="PXGridAnswers" runat="server" Caption="Attributes" DataSourceID="ds" Height="400px" MatrixMode="True" Width="600px" SkinID="Attributes">
                        <Levels>
                            <px:PXGridLevel DataKeyNames="AttributeID,EntityType,EntityID" DataMember="Answers">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="M" StartColumn="True"></px:PXLayoutRule>
                                    <px:PXTextEdit ID="edParameterID" runat="server" DataField="AttributeID" Enabled="False"></px:PXTextEdit>
                                    <px:PXTextEdit ID="edAnswerValue" runat="server" DataField="Value"></px:PXTextEdit>
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn AllowShowHide="False" DataField="AttributeID" TextField="AttributeID_description" TextAlign="Left" Width="250px"></px:PXGridColumn>
                                    <px:PXGridColumn DataField="isRequired" TextAlign="Center" Type="CheckBox" Width="80px"></px:PXGridColumn>
                                    <px:PXGridColumn DataField="Value" Width="185px"></px:PXGridColumn>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
        </Items>
        <Items>
            <px:PXTabItem Text="STAFF">
                <Template>
                    <px:PXGrid ID="PXGridStaff" runat="server" Caption="STAFF" DataSourceID="ds" Height="150px" MatrixMode="True" Width="420px" SkinID="Attributes">
                        <Levels>
                            <px:PXGridLevel DataKeyNames="EmployeeID" DataMember="StaffList">
                                <RowTemplate>
                                    <px:PXSelector ID="edEmployeeID" runat="server" DataField="EmployeeID" Enabled="False"></px:PXSelector>
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="EmployeeID" DisplayMode="Hint" TextAlign="Left" Width="120px"></px:PXGridColumn>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" AllowUpload="False" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
        </Items>
    </px:PXTab>
</asp:Content>
