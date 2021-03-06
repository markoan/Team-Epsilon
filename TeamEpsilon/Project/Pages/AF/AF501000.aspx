<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AF501000.aspx.cs" Inherits="Page_AF501000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%"
        TypeName="AF.AFDataEnq"
        PrimaryView="Filter"
        >
		<CallbackCommands>

		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="Filter" Width="100%" Height="100px" AllowAutoHide="false">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartRow="True"/>
            <px:PXSelector ID="edGenericInq" runat="server" AlreadyLocalized="False" DataField="GenericInq" CommitChanges="True" DefaultLocale="">
            </px:PXSelector>
            <px:PXDateTimeEdit ID="edStartDate" runat="server" DataField="StartDate" CommitChanges="True">
            </px:PXDateTimeEdit>
            <px:PXDateTimeEdit ID="edEndDate" runat="server" DataField="EndDate" CommitChanges="True">
            </px:PXDateTimeEdit>
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Width="100%" Height="150px" SkinID="Details" AllowAutoHide="false">
		<Levels>
			<px:PXGridLevel DataMember="Result">
			    <Columns>
			        <px:PXGridColumn DataField="ResultID" Width="200px"></px:PXGridColumn>
			        <px:PXGridColumn DataField="ResultTstamp" Width="200px"></px:PXGridColumn>
			        <px:PXGridColumn DataField="ResultP10" Width="200px"></px:PXGridColumn>
			        <px:PXGridColumn DataField="ResultE50" Width="200px"></px:PXGridColumn>
			        <px:PXGridColumn DataField="ResultP90" Width="200px"></px:PXGridColumn>
			    </Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<ActionBar >
		</ActionBar>
	</px:PXGrid>
</asp:Content>
