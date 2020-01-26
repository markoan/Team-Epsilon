<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AF100000.aspx.cs" Inherits="Page_AF100000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
  <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%"
        TypeName="AF.AFSetupMaint"
        PrimaryView="AFSetupView"
        >
    <CallbackCommands>

    </CallbackCommands>
  </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
  <px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="AFSetupView" Width="100%" AllowAutoHide="false">
    <Template>
      <px:PXLayoutRule ControlSize="XL" LabelsWidth="XL" ID="PXLayoutRule1" runat="server" StartRow="True"></px:PXLayoutRule>
      <px:PXTextEdit runat="server" ID="CstPXTextEdit1" DataField="AFAccessKey" />
      <px:PXTextEdit runat="server" ID="CstPXTextEdit2" DataField="AFSecretKey" />
      <px:PXTextEdit runat="server" ID="CstPXTextEdit3" DataField="AFBucketName" />
      <px:PXTextEdit runat="server" ID="CstPXTextEdit4" DataField="AFDirectoryName" />
      <px:PXTextEdit runat="server" ID="CstPXTextEdit5" DataField="AFOutDirectoryName" />
    </Template>
    <AutoSize Container="Window" Enabled="True" MinHeight="200" ></AutoSize>
  </px:PXFormView>
</asp:Content>