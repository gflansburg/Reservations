<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Cashier.ascx.cs" Inherits="Gafware.Modules.Reservations.Cashier" %>
<%@ register tagprefix="dnn" tagname="Label" src="~/controls/LabelControl.ascx" %>
<%@ register tagprefix="dnn" assembly="DotNetNuke" namespace="DotNetNuke.UI.WebControls" %>
<script type="text/javascript">
    if (typeof jQuery != 'undefined') {
        (function ($) {
            $(function () {
                var pageRequestManager = Sys.WebForms.PageRequestManager.getInstance();

                if (pageRequestManager != null) {
                    pageRequestManager.add_endRequest(function () {
                        bindEventHandlers();
                    });
                }

                bindEventHandlers();
            });
        })(jQuery);

        function bindEventHandlers() {
            $('.filterTextBox').keypress(function (event) {
                if (event.keyCode == '13') {
                    event.preventDefault();
                    $('.filterImageButton').trigger("click");
                }
            });
        }
    }
</script>
<div runat="server" id="topPagingControlDiv" class="Gafware_Modules_Reservations_DataGrid_TopPagerStyle"
    visible="false">
    <dnn:pagingcontrol id="topPagingControl" runat="server" enableviewstate="true" />
</div>
<asp:datagrid id="dataGrid" runat="server" datakeyfield="PendingPaymentID" enableviewstate="true"
    autogeneratecolumns="False" gridlines="None" width="100%" onsortcommand="SortCommand"
    onitemcommand="ItemCommand" onitemcreated="ItemCreated" onitemdatabound="ItemDataBound"
    cssclass="Gafware_Modules_Reservations_Normal Gafware_Modules_Reservations_DataGrid">
    <headerstyle cssclass="Gafware_Modules_Reservations_SubHead Gafware_Modules_Reservations_DataGrid_HeaderStyle" />
    <itemstyle cssclass="Gafware_Modules_Reservations_Normal Gafware_Modules_Reservations_DataGrid_ItemStyle" />
    <alternatingitemstyle cssclass="Gafware_Modules_Reservations_Normal Gafware_Modules_Reservations_DataGrid_AlternatingItemStyle" />
    <pagerstyle cssclass="Gafware_Modules_Reservations_DataGrid_PagerStyle" />
    <columns>
        <asp:templatecolumn>
            <headerstyle cssclass="Gafware_Modules_Reservations_DataGrid_HeaderStyle_Button"
                width="25" />
            <itemstyle cssclass="Gafware_Modules_Reservations_DataGrid_ItemStyle_Button" width="25" />
            <headertemplate>
                <asp:imagebutton runat="server" commandname="Settings" imageurl="~/images/icon_hostsettings_16px.gif"
                    width="16" height="16" resourcekey="Settings" visible="<%#HasEditPermissions%>" />
            </headertemplate>
            <itemtemplate>
                <asp:label runat="server" id="pendingPaymentID" visible="false" text='<%#DataBinder.Eval( Container.DataItem, "PendingPaymentID" ) %>' />
                <asp:imagebutton runat="server" id="markAsPaidCommandButton" imageurl="grant.gif"
                    width="16" height="16" resourcekey="markAsPaidCommandButton" commandname="Paid"
                    onclientclick=<%#"return confirm('" + Localization.GetString( "ConfirmMarkAsPaid", LocalResourceFile ) + "');"%>
                    visible='<%#( int )DataBinder.Eval( Container.DataItem, "Status" ) == ( int )Gafware.Modules.Reservations.PendingPaymentStatus.Due %>' />
                <asp:imagebutton runat="server" id="markAsRefundedCommandButton" imageurl="grant.gif"
                    width="16" height="16" resourcekey="markAsRefundedCommandButton" commandname="Refunded"
                    onclientclick=<%#"return confirm('" + Localization.GetString( "ConfirmMarkAsRefunded", LocalResourceFile ) + "');"%>
                    visible='<%#( int )DataBinder.Eval( Container.DataItem, "Status" ) == ( int )Gafware.Modules.Reservations.PendingPaymentStatus.PendingRefund %>' />
            </itemtemplate>
        </asp:templatecolumn>
    </columns>
</asp:datagrid>
<asp:label runat="server" id="numberOfRecordsFoundLabel" cssclass="Gafware_Modules_Reservations_Normal Gafware_Modules_Reservations_DataGrid_NumberOfRecordsFound" />
<div runat="server" id="bottomPagingControlDiv" class="Gafware_Modules_Reservations_DataGrid_BottomPagerStyle"
    visible="false">
    <dnn:pagingcontrol id="bottomPagingControl" runat="server" enableviewstate="true" />
</div>
<div style="width: 100%; text-align: center; padding-top: 10px;">
    <asp:linkbutton runat="server" id="returnCommandButton" style="width: 80px;" cssclass="Gafware_Modules_Reservations_CommandButton"
        onclick="CancelCommandButtonClicked" causesvalidation="false"><asp:image runat="server" imageurl="~/images/lt.gif" width="16" height="16" resourcekey="cancelCommandButton" /><asp:label runat="server" resourcekey="cancelCommandButton" /></asp:linkbutton>
</div>
