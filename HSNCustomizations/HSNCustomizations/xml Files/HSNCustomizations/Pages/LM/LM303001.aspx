<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="LM303001.aspx.cs" Inherits="Page_LM303001" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" UDFTypeField="CustomerClassID" EnableAttributes="true" Width="100%" TypeName="HSNCustomizations.Graph.LUMCustomerStaffMaint" PrimaryView="MappingList">
        <CallbackCommands>
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" Style="z-index: 100;" Width="100%" Caption="Customer Staff Mapping List" SkinID="Details" Height="300px">
        <Levels>
            <px:PXGridLevel DataMember="MappingList">
                <RowTemplate>
                    <px:PXSegmentMask ID="edCustomerID" runat="server" AllowEdit="True" AllowAddNew="True" CommitChanges="True" DataField="CustomerID" DataSourceID="ds" AutoRefresh="True">
                    </px:PXSegmentMask>
                    <px:PXSelector ID="edEquipmentTypeID" runat="server" DataField="EquipmentTypeID"></px:PXSelector>
                    <px:PXSelector ID="edEmployeeID" runat="server" DataField="EmployeeID"></px:PXSelector>
                    <px:PXSegmentMask ID="edLocationID" runat="server" DataField="LocationID" CommitChanges="True" AutoRefresh="True" AllowEdit="True"></px:PXSegmentMask>
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="CustomerID" AllowNull="False" CommitChanges="True" />
                    <px:PXGridColumn DataField="LocationID" AllowNull="False" />
                    <px:PXGridColumn DataField="EquipmentTypeID" />
                    <px:PXGridColumn DataField="EmployeeID" AllowNull="False" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <CallbackCommands>
            <Save PostData="Page" />
        </CallbackCommands>
        <Mode AllowUpload="True" />
    </px:PXGrid>
</asp:Content>
