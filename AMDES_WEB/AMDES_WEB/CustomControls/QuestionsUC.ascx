﻿<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="QuestionsUC.ascx.cs"
    Inherits="AMDES_WEB.CustomControls.QuestionsUC" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="act" %>
<tr>
    <td style="vertical-align: top; width: 5%;">
        <div>
            <h4>
                Q<asp:Label ID="lblQnID" runat="server" Text=""></asp:Label>.&nbsp;</h4>
        </div>
    </td>
    <td style="vertical-align: top; width: 80%;">
        <h4>
            <asp:Literal ID="lblQn" runat="server"></asp:Literal></h4>
        <asp:Image ID="imgQn" runat="server" Visible="false" />
    </td>
    <td style="min-width: 10%; max-width: 60px">
        <asp:CheckBox ID="chkAns" runat="server" AutoPostBack="True" OnCheckedChanged="chkAns_CheckedChanged" />
        <act:ToggleButtonExtender ID="ToggleEx" runat="server" TargetControlID="chkAns" ImageWidth="50"
            ImageHeight="25" CheckedImageAlternateText="Yes" UncheckedImageAlternateText="No"
            UncheckedImageUrl="~/img/ToggleButton_Unchecked.png" CheckedImageUrl="~/img/ToggleButton_Checked.png" />
    </td>
</tr>
