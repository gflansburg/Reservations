﻿<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ViewReservations.ascx.cs" Inherits="Gafware.Modules.Reservations.ViewReservations" %>
<%@ register tagprefix="dnn" tagname="Label" src="~/controls/LabelControl.ascx" %>
<%@ register tagprefix="dnn" assembly="DotNetNuke" namespace="DotNetNuke.UI.WebControls" %>
<script type="text/javascript">
    if (typeof (jQuery) == 'function') {
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

        var bindDocumentClick = true;

        function bindEventHandlers() {
            $('.filterTextBox').keypress(function (event) {
                if (event.keyCode == '13') {
                    event.preventDefault();
                    $('.filterImageButton').trigger("click");
                }
            });

            $(".Gafware_Modules_Reservations_DateFilter_AbsoluteTable").click(function (e) {
                hide = false;
            });

            $(".Gafware_Modules_Reservations_DateFilter_TextBox").click(function (e) {
                hide = false;
            });

            $(".Gafware_Modules_Reservations_DateFilter_TextBox").keyup(function (e) {
                $(".Gafware_Modules_Reservations_DateFilter_AbsoluteTable").hide();
            });

            $(".Gafware_Modules_Reservations_DateFilter_TableCell").mouseenter(function (e) {
                var date = parseInt($(this).attr("date"));
                var columnName = $(this).attr("columnName");
                var selectedDate = $(".Gafware_Modules_Reservations_DateFilter_" + columnName + "_AbsoluteTable").attr("date");

                if (selectedDate != null && selectedDate != "") {
                    selectedDate = parseInt(selectedDate);

                    var start = selectedDate > date ? date : selectedDate;
                    var end = selectedDate > date ? selectedDate : date;

                    var i = start + 1;

                    $(".Gafware_Modules_Reservations_DateFilter_" + columnName + "_TableCell a").css("background-color", "");

                    for (; i < end; i++) {
                        var _class = $(".Gafware_Modules_Reservations_DateFilter_" + columnName + "_TableCell_" + i).attr("class");

                        if (_class == null || _class.indexOf("Gafware_Modules_Reservations_SelectedDayStyle") == -1)
                        { $(".Gafware_Modules_Reservations_DateFilter_" + columnName + "_TableCell_" + i + " a").css("background-color", "#D8F0FF"); }
                    }
                }
            });

            $(".Gafware_Modules_Reservations_DateFilter_TableCell").mouseleave(function (e) {
                var columnName = $(this).attr("columnName");
                $(".Gafware_Modules_Reservations_DateFilter_" + columnName + "_TableCell a").css("background-color", "");
            });

            if (bindDocumentClick) {
                $(document).click(function (e) {
                    if (hide) {
                        $(".Gafware_Modules_Reservations_DateFilter_AbsoluteTable").hide();
                    }

                    hide = true;
                });

                bindDocumentClick = false;
            }
        }

        var hide = true;

        function showFilterCalendar(columnName) {
            if (typeof jQuery != 'undefined') {
                $(".Gafware_Modules_Reservations_DateFilter_AbsoluteTable").hide();
                $(".Gafware_Modules_Reservations_DateFilter_" + columnName + "_AbsoluteTable").show();
            }
        }
    }
</script>
<div runat="server" id="topPagingControlDiv" class="Gafware_Modules_Reservations_DataGrid_TopPagerStyle"
    visible="false">
    <dnn:pagingcontrol id="topPagingControl" runat="server" enableviewstate="true" />
</div>
<asp:datagrid id="dataGrid" runat="server" datakeyfield="ReservationID" enableviewstate="true"
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
                width="50" />
            <itemstyle cssclass="Gafware_Modules_Reservations_DataGrid_ItemStyle_Button" width="50" />
            <headertemplate>
                <asp:imagebutton runat="server" commandname="Settings" imageurl="~/images/icon_hostsettings_16px.gif"
                    width="16" height="16" resourcekey="Settings" visible="<%#HasEditPermissions%>" />
            </headertemplate>
            <itemtemplate>
                <asp:label runat="server" id="reservationID" visible="false" text='<%#DataBinder.Eval( Container.DataItem, "ReservationID" ) %>' />
                <asp:imagebutton runat="server" id="viewCommandButton" commandname="View" imageurl="~/images/view.gif"
                    width="16" height="16" resourcekey="viewCommandButton" />
            </itemtemplate>
        </asp:templatecolumn>
    </columns>
</asp:datagrid>
<asp:label runat="server" id="numberOfRecordsFoundLabel" cssclass="Gafware_Modules_Reservations_Normal Gafware_Modules_Reservations_DataGrid_NumberOfRecordsFound" />
<div runat="server" id="bottomPagingControlDiv" class="Gafware_Modules_Reservations_DataGrid_BottomPagerStyle"
    visible="false">
    <dnn:pagingcontrol id="bottomPagingControl" runat="server" enableviewstate="true" />
</div>
<table cellpadding="0" cellspacing="0" border="0" width="100%" runat="server" id="buttonsTable">
    <tr>
        <td align="left" style="padding-top: 10px">
            <asp:linkbutton runat="server" id="returnCommandButton" style="width: 80px;" cssclass="Gafware_Modules_Reservations_CommandButton"
            onclick="CancelCommandButtonClicked" causesvalidation="false"><asp:image runat="server" imageurl="~/images/lt.gif" width="16" height="16" resourcekey="cancelCommandButton" /><asp:label runat="server" resourcekey="cancelCommandButton" /></asp:linkbutton>
        </td>
        <td align="right" style="padding-top: 10px">
            <asp:linkbutton runat="server" id="printCommandButton" style="width: 140px;" cssclass="Gafware_Modules_Reservations_CommandButton"
            causesvalidation="false"><asp:image runat="server" imageurl="~/images/print.gif" width="16" height="16" resourcekey="printCommandButton" /><asp:label runat="server" resourcekey="printCommandButton" /></asp:linkbutton>
        </td>
    </tr>
</table>
