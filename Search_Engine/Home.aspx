<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Home.aspx.cs" Inherits="Search_Engine.Home" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
   <div style="direction: ltr">

<style>
        .searchBox {
            margin-left: 494px; 
text-align:center;
        }
        .radios {
            margin-left: 600px;
        }
       .img-style {
            width: 549px;
            height: 200px;
            margin-left: 420px;
        }
    </style>

    <h1 style="text-align:center; width: 1267px; margin-left: 64px;">Welcome To Our Search Engine</h1>
    <img alt="Search" src="../search.jpg" class="img-style" /><br />
       <br />
       <br />
    <asp:TextBox ID="SearchWords" runat="server" TextMode="Search" Width="300px" CssClass="searchBox"></asp:TextBox>
    <asp:Button ID="Search" runat="server" Text="Search" OnClick="Search_Click" Width="82px" /><br/>
    <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="SearchWords" ForeColor="Red" CssClass="searchBox" EnableClientScript="False" ValidateRequestMode="Enabled">Please enter text to search for</asp:RequiredFieldValidator>
    <asp:RadioButtonList ID="RadioButtonList1" runat="server" CssClass="radios">
        <asp:ListItem Text="Soundex" Value="Soundex" />
        <asp:ListItem Text="K-gram" Value="K-gram" />
    </asp:RadioButtonList>
    <h3 style="margin-left: 120px">Search Results:</h3>
            <asp:DataList ID="searchResults" runat="server" style="margin-left: 120px" CellPadding="4" ForeColor="#333333">
                <AlternatingItemStyle BackColor="White" />
                <FooterStyle BackColor="#507CD1" Font-Bold="True" ForeColor="White" />
                <HeaderStyle BackColor="#507CD1" Font-Bold="True" ForeColor="White" />
                <ItemStyle BackColor="#EFF3FB" />
                <ItemTemplate>
                    <a  runat="server"  href='<%#Container.DataItem?.ToString() ?? string.Empty %>'><%# Container.DataItem?.ToString() ?? string.Empty%></a>
                </ItemTemplate>
                <SelectedItemStyle BackColor="#D1DDF1" Font-Bold="True" ForeColor="#333333" />
            </asp:DataList>
        </div>
    </form>
</body>
</html>