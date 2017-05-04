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
                    margin-left: 422px;
                    text-align: center;
                }

                .radios {
                    margin-left: 426px;
                    margin-right: 0px;
                }

                .img-style {
                    width: 549px;
                    height: 200px;
                    margin-left: 420px;
                }

                .RecomendedWords {
                    margin-left: 120px;
                }
            </style>

            <h1 style="text-align: center; width: 1267px; margin-left: 64px;">Welcome To Our Search Engine</h1>
            <img alt="Search" src="../search.jpg" class="img-style" /><br />
            <br />
            <br />
            <asp:TextBox ID="SearchWords" runat="server" TextMode="Search" Width="468px" CssClass="searchBox"></asp:TextBox>
            <asp:Button ID="Search" runat="server" Text="Search" OnClick="Search_Click" Width="82px" /><br />
            <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="SearchWords" ForeColor="Red" CssClass="searchBox" EnableClientScript="False" ValidateRequestMode="Enabled">Please enter text to search for</asp:RequiredFieldValidator>
            <asp:RadioButtonList ID="RadioButtonList1" runat="server" CssClass="radios" Width="164px">
                <asp:ListItem Text="Soundex" Value="Soundex" />
                <asp:ListItem Text="Spelling Correction" Value="spelling correction" />
            </asp:RadioButtonList>
            <h3 id="SearchResultsText" runat="server" style="margin-left: 120px">Search Results:</h3>
            <asp:DataList ID="searchResults" runat="server" Style="margin-left: 120px" CellPadding="4" ForeColor="#333333">
                <AlternatingItemStyle BackColor="White" />
                <FooterStyle BackColor="#507CD1" Font-Bold="True" ForeColor="White" />
                <HeaderStyle BackColor="#507CD1" Font-Bold="True" ForeColor="White" Font-Italic="False" Font-Names="Bodoni MT Poster Compressed" Font-Overline="False" Font-Size="Smaller" Font-Strikeout="False" Font-Underline="False" />
                <ItemStyle BackColor="#EFF3FB" />
                <ItemTemplate>
                    <a runat="server" href='<%#Container.DataItem?.ToString() ?? string.Empty %>'><%# Container.DataItem?.ToString() ?? string.Empty%></a>
                </ItemTemplate>
                <SelectedItemStyle BackColor="#D1DDF1" Font-Bold="True" ForeColor="#333333" />
            </asp:DataList>
            <asp:ListBox ID="ListBox1" runat="server" style="margin-left: 120px" EnableViewState="True" AutoPostBack="True" OnSelectedIndexChanged="ListBox1_SelectedIndexChanged" Width="184px"></asp:ListBox>
        </div>
    </form>
</body>
</html>
