<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %> 
<%@ Register Tagprefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LargeListPagingWebPart.ascx.cs" Inherits="SharePointPOCLargeLIstPaging.LargeListPagingWebPart.LargeListPagingWebPart" %>

<style>


.gridview table {
    width:100% !important;
}
/* the style for the table headers */
.gridview th
{
    font-size:12px !important;
    font-weight: bold;
    color: Navy;
    border: 1px solid #808080;
    padding: 4px;
    background-color:#eee;

}
/* the style for the normal table cells */
.gridview td
{
    font-size:12px !important;
    padding: 4px;
    border: 1px solid #808080;
}
.pageInfo{
    padding-left:5px;
    padding-right:5px;
} 

</style>
<table>
    <tr>
        <td>Sort by: </td>
        <td>
            <asp:DropDownList ID="ddlSortBy" OnSelectedIndexChanged="ddlSortBy_SelectedIndexChanged" AutoPostBack="true" runat="server">
                 <asp:ListItem Value="ModifiedAsc" Text="Modified (ASC)" />
                <asp:ListItem Value="ModifiedDesc" Text="Modified (DESC)" />
                <asp:ListItem Selected="True" Value="CreatedAsc" Text="Created (ASC)" />
                <asp:ListItem Value="CreatedDesc" Text="Created (DESC)" />
                <asp:ListItem Value="IdAsc" Text="ID ASC" />
                <asp:ListItem Value="IdDesc" Text="ID DESC" />
                <asp:ListItem Value="TitleAsc" Text="Title (A > Z)" />
                <asp:ListItem Value="TitleDesc" Text="Title (Z > A)" />
            </asp:DropDownList>
        </td>
       
         <td>Page Size: </td>
        <td>
            <asp:DropDownList ID="ddlPageSize" OnSelectedIndexChanged="ddlPageSize_SelectedIndexChanged" AutoPostBack="true" runat="server">
                <asp:ListItem Selected="True" Value="10" Text="10" />
                <asp:ListItem Value="20" Text="20" />
                <asp:ListItem Value="50" Text="50" />
                <asp:ListItem Value="100" Text="100" />
            </asp:DropDownList>
        </td>
    </tr>
</table><br />

<asp:Label Text="" ID="lblPageStatastics" runat="server" />

<br /><br />

<asp:gridview id="grdView"  CssClass="gridview"
        autogeneratecolumns="true" 
        runat="server">
</asp:gridview>
<br />

<asp:Panel ID="_ContainerPanel" runat="server">
    
</asp:Panel>

<br />

<asp:Label Text="" ID="lblErrorMessage" runat="server" />