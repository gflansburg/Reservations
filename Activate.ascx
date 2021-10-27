<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Activate.ascx.cs" Inherits="Gafware.Modules.Reservations.Activate" %>
<%@ register tagprefix="dnn" tagname="Label" src="~/controls/LabelControl.ascx" %>
<%@ register tagprefix="dnn" assembly="DotNetNuke" namespace="DotNetNuke.UI.WebControls" %>
<table cellspacing="10" cellpadding="0" border="0" align="center">
    <tr>
        <td align="left">
            <asp:label id="activationTitleLabel" runat="server" cssclass="Gafware_Modules_Reservations_SubHead" resourcekey="activationTitleLabel" />
        </td>
    </tr>
    <tr>
        <td align="left">
            <asp:label id="activationSubTitleLabel" runat="server" cssclass="Gafware_Modules_Reservations_SubHead" resourcekey="activationSubTitleLabel" />
        </td>
    </tr>
    <tr>
        <td style="padding-top: 20px">
            <table cellpadding="0" cellspacing="10" border="0" align="center" style="text-align: left;
                border: solid 1px #cccccc">
                <tr>
                    <td style="width: 350px" align="left">
<%--                        <asp:label resourcekey="activationInvoiceLabel" runat="server" cssclass="Gafware_Modules_Reservations_SubHead"
                            controlname="activationInvoiceTextBox" />--%>
                            <asp:label resourcekey="editionLabel" runat="server" cssclass="Gafware_Modules_Reservations_SubHead"
                                controlname="editionTextBox" />

                    </td>
                </tr>
                <tr>
                    <td style="white-space: nowrap">
<%--                        <asp:textbox runat="server" id="activationInvoiceTextBox" cssclass="Gafware_Modules_Reservations_Input"
                            width="350" validationgroup="activationValidationGroup1" style="vertical-align: middle" />
                        <asp:requiredfieldvalidator runat="server" text='<%#"<img src=\"" + TemplateSourceDirectory + "/../../images/required.gif\"  style=\"vertical-align: middle\" />"%>'
                            controltovalidate="activationInvoiceTextBox" display="dynamic" validationgroup="activationValidationGroup1"
                            enableclientscript="false" />--%>
                        <asp:DropDownList runat="server" id="editionList" cssclass="Gafware_Modules_Reservations_Input"
                            width="350" validationgroup="activationValidationGroup1" style="vertical-align: middle">
                        </asp:DropDownList>
                    </td>
                </tr>
                <tr>
                    <td style="width: 350px">
                        <asp:label resourcekey="activationFingerprintLabel" runat="server" cssclass="Gafware_Modules_Reservations_SubHead"
                            controlname="activationFingerprintTextBox" />
                    </td>
                </tr>
                <tr>
                    <td style="white-space: nowrap">
                        <asp:textbox runat="server" id="activationFingerprintTextBox" cssclass="Gafware_Modules_Reservations_Input"
                            width="350" validationgroup="activationValidationGroup1" readonly="true" />
                    </td>
                </tr>
                <tr>
                    <td style="width: 350px">
                        <asp:label resourcekey="activationEmailLabel" runat="server" cssclass="Gafware_Modules_Reservations_SubHead" controlname="activationEmailTextBox" />
                    </td>
                </tr>
                <tr>
                    <td>
                        <asp:textbox runat="server" id="activationEmailTextBox" cssclass="Gafware_Modules_Reservations_Input"
                            width="350" validationgroup="activationValidationGroup1" style="vertical-align: middle" />
                        <asp:regularexpressionvalidator runat="server" id="validator3" text='<%#"<img src=\"" + TemplateSourceDirectory + "/../../images/required.gif\"  style=\"vertical-align: middle\" />"%>'
                            controltovalidate="activationEmailTextBox" display="dynamic"
                            validationexpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" validationgroup="activationValidationGroup1"
                            enableclientscript="false" />
                    </td>
                </tr>
                <tr>
                    <td align="right">
                        <asp:linkbutton width="200" runat="server" id="requestActivationCodeCommandButton"
                            onclick="RequestActivationCodeCommandButtonClicked" cssclass="Gafware_Modules_Reservations_CommandButton"
                            causesvalidation="True" validationgroup="activationValidationGroup1"><asp:image runat="server" imageurl="~/images/register.gif" /><asp:label runat="server" resourcekey="requestActivationCodeCommandButton" /></asp:linkbutton>
                    </td>
                </tr>
                <tr>
                    <td style="width: 350px">
                        <asp:label resourcekey="activationCodeLabel" runat="server" cssclass="Gafware_Modules_Reservations_SubHead" controlname="activationCodeTextBox" />
                    </td>
                </tr>
                <tr>
                    <td valign="top">
                        <asp:textbox runat="server" id="activationCodeTextBox" cssclass="Gafware_Modules_Reservations_Input"
                            textmode="MultiLine" width="350" rows="7" height="110" validationgroup="activationValidationGroup2"
                            style="vertical-align: middle" />
                        <asp:requiredfieldvalidator runat="server" id="validator4" text='<%#"<img src=\"" + TemplateSourceDirectory + "/../../images/required.gif\"  style=\"vertical-align: middle\" />"%>'
                            controltovalidate="activationCodeTextBox" display="dynamic"
                            validationgroup="activationValidationGroup2" enableclientscript="false" style="vertical-align: top" />
                    </td>
                </tr>
                <tr>
                    <td align="right">
                        <asp:linkbutton width="100" runat="server" id="activateCommandButton" onclick="ActivateCommandButtonClicked"
                            cssclass="Gafware_Modules_Reservations_CommandButton" causesvalidation="True"
                            validationgroup="activationValidationGroup2"><asp:image runat="server" imageurl="grant.gif" /><asp:label runat="server" resourcekey="activateCommandButton" /></asp:linkbutton>
                    </td>
                </tr>
            </table>
        </td>
    </tr>
</table>
