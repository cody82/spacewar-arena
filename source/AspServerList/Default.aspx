<%@ Page Title="Startseite" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true"
    CodeBehind="Default.aspx.cs" Inherits="AspServerList._Default" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <h2>
        Willkommen bei ASP.NET!
    </h2>
    <p>
        Weitere Informationen zu ASP.NET finden Sie auf <a href="http://www.asp.net" title="ASP.NET-Website">www.asp.net</a>.
    </p>
    <p>
        <a href="http://go.microsoft.com/fwlink/?LinkID=152368"
            title="MSDN-ASP.NET-Dokumente">Dokumentation finden Sie auch unter ASP.NET bei MSDN</a>.
    </p>
    <p>
        <asp:GridView ID="GridView1" runat="server" Width="100%">
        </asp:GridView>
    </p>
</asp:Content>
