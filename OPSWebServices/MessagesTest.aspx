<%@ Page language="c#" Codebehind="MessagesTest.aspx.cs" validateRequest="false" AutoEventWireup="True" Inherits="OPSWebServices.MessagesTest" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<HTML>
	<HEAD>
		<title>MessagesTest</title>
		<meta content="Microsoft Visual Studio .NET 7.1" name="GENERATOR">
		<meta content="C#" name="CODE_LANGUAGE">
		<meta content="JavaScript" name="vs_defaultClientScript">
		<meta content="http://schemas.microsoft.com/intellisense/ie5" name="vs_targetSchema">
		<LINK href="tree-view.css" type="text/css" rel="./style/stylesheet">
	</HEAD>
	<body MS_POSITIONING="GridLayout">
		<form id="Form1" method="post" runat="server">
			&nbsp;
			<DIV style="Z-INDEX: 100; LEFT: 40px; WIDTH: 736px; POSITION: absolute; TOP: 160px; HEIGHT: 96px"
				ms_positioning="FlowLayout">
				<asp:xml id="XmlRdo" runat="server" Visible="true" EnableViewState="False"></asp:xml></DIV>
			<DIV style="Z-INDEX: 101; LEFT: 40px; OVERFLOW: auto; WIDTH: 736px; POSITION: absolute; TOP: 264px; HEIGHT: 390px"
				ms_positioning="FlowLayout">
				<asp:xml id="XmlRdo2" runat="server" Visible="true" EnableViewState="False"></asp:xml></DIV>
			<HR style="Z-INDEX: 102; LEFT: 32px; POSITION: absolute; TOP: 144px" width="100%" color="#0033cc"
				SIZE="1">
			<DIV style="Z-INDEX: 104; LEFT: 40px; WIDTH: 736px; POSITION: absolute; TOP: 48px; HEIGHT: 40px"
				ms_positioning="FlowLayout">
				<asp:xml id="XmlQry" runat="server" EnableViewState="False"></asp:xml>
				<asp:TextBox id="TxtBxQry" runat="server" Width="608px" Height="40px"></asp:TextBox>
				<asp:Button id="BttnCall" runat="server" Text="Call Service" CausesValidation="False" onclick="BttnCall_Click"></asp:Button></DIV>
			<DIV title="Answer" style="DISPLAY: inline; FONT-SIZE: medium; Z-INDEX: 106; LEFT: 32px; WIDTH: 70px; COLOR: blue; POSITION: absolute; TOP: 120px; HEIGHT: 15px"
				ms_positioning="FlowLayout">Answer</DIV>
			<HR style="Z-INDEX: 107; LEFT: 32px; POSITION: absolute; TOP: 40px" width="100%" color="#0033cc"
				SIZE="1">
			<DIV title="Qry" style="DISPLAY: inline; FONT-SIZE: medium; Z-INDEX: 108; LEFT: 32px; WIDTH: 70px; COLOR: blue; POSITION: absolute; TOP: 16px; HEIGHT: 15px"
				ms_positioning="FlowLayout">Qry</DIV>
		</form>
	</body>
</HTML>
