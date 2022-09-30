﻿<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MakeReservation.ascx.cs" Inherits="Gafware.Modules.Reservations.MakeReservation" %>
<%@ register tagprefix="dnn" assembly="DotNetNuke" namespace="DotNetNuke.UI.WebControls" %>
<%@ register tagprefix="dnn" tagname="Label" src="~/controls/LabelControl.ascx" %>
<%@ register tagprefix="dnn" tagname="TextEditor" src="~/controls/TextEditor.ascx" %>
<script type="text/javascript" language="javascript" src='<%=TemplateSourceDirectory + "/jquery.responsive.js" %>'></script>
<script type="text/javascript" language="javascript" src='<%=TemplateSourceDirectory + "/jquery.placeholder.js" %>'></script>
<script type="text/javascript" language="javascript" src='<%=TemplateSourceDirectory + "/sly.min.js" %>'></script>
<script type="text/javascript" language="javascript" src='<%=TemplateSourceDirectory + "/jquery.sly.js" %>'></script>
<script type="text/javascript" language="javascript" src='<%=TemplateSourceDirectory + "/jquery.authorize.net.js?version=1.1.0" %>'></script>
<input runat="server" id="Gafware_Modules_Reservations_AuthorizeNetSIMForm_Hidden" class="Gafware_Modules_Reservations_AuthorizeNetSIMForm_Hidden" type="hidden" enableviewstate="false" visible="false" clientidmode="static" />
<div class="Gafware_Modules_Reservations">
    <div runat="server" id="contactInfoDiv" class="Gafware_Modules_Reservations_Step Gafware_Modules_Reservations_ContactInfo">
        <div class="Gafware_Modules_Reservations_Instructions">
            <asp:label runat="server" resourcekey="contactInfoTitleLabel" cssclass="Gafware_Modules_Reservations_Head" />
            <asp:label runat="server" resourcekey="contactInfoSubTitleLabel" cssclass="Gafware_Modules_Reservations_Normal" />
        </div>
        <div class="Gafware_Modules_Reservations_FormContainer">
            <div class="Gafware_Modules_Reservations_Form">
                <div class="Gafware_Modules_Reservations_FormItem">
                    <asp:label id="firstNameLabel" runat="server" cssclass="Gafware_Modules_Reservations_SubHead"
                        resourcekey="firstNameLabel" />
                    <asp:textbox runat="server" id="firstNameTextBox" cssclass="Gafware_Modules_Reservations_Input Gafware_Modules_Reservations_Required" />
                    <asp:customvalidator runat="server" controltovalidate="firstNameTextBox" onservervalidate="ValidateRequired"
                        validateemptytext="true" validationgroup="contactInfoValidationGroup" />
                </div>
                <div class="Gafware_Modules_Reservations_FormItem">
                    <asp:label id="lastNameLabel" runat="server" cssclass="Gafware_Modules_Reservations_SubHead"
                        resourcekey="lastNameLabel" />
                    <asp:textbox runat="server" id="lastNameTextBox" cssclass="Gafware_Modules_Reservations_Input Gafware_Modules_Reservations_Required" />
                    <asp:customvalidator runat="server" controltovalidate="lastNameTextBox" onservervalidate="ValidateRequired"
                        validateemptytext="true" validationgroup="contactInfoValidationGroup" />
                </div>
                <div class="Gafware_Modules_Reservations_FormItem">
                    <asp:label id="emailLabel" runat="server" cssclass="Gafware_Modules_Reservations_SubHead"
                        resourcekey="emailLabel" />
                    <asp:textbox runat="server" id="emailTextBox" cssclass="Gafware_Modules_Reservations_Input Gafware_Modules_Reservations_Required" />
                    <asp:customvalidator runat="server" controltovalidate="emailTextBox" onservervalidate="ValidateRequired"
                        validateemptytext="true" validationgroup="contactInfoValidationGroup" />
                    <asp:customvalidator runat="server" controltovalidate="emailTextBox" onservervalidate="ValidateEmail"
                        validateemptytext="false" validationgroup="contactInfoValidationGroup" />
                </div>
                <div class="Gafware_Modules_Reservations_FormItem">
                    <asp:label id="phoneLabel" runat="server" resourcekey="phoneLabel" cssclass="Gafware_Modules_Reservations_SubHead" />
                    <asp:textbox runat="server" id="phoneTextBox" cssclass="Gafware_Modules_Reservations_Input" />
                    <asp:customvalidator runat="server" id="phoneTextBoxRequiredFieldValidator" controltovalidate="phoneTextBox"
                        onservervalidate="ValidateRequired" validateemptytext="true" validationgroup="contactInfoValidationGroup"
                        visible="false" />
                </div>
                <asp:repeater runat="server" id="customFieldTableRowRepeater" onitemdatabound="CustomFieldTableRowRepeater_ItemDataBound">
                    <itemtemplate>
                        <div class="Gafware_Modules_Reservations_FormItem">
                            <asp:label id="label" runat="server" cssclass="Gafware_Modules_Reservations_SubHead"
                                text="<%#Container.DataItem%>" />
                            <asp:repeater runat="server" id="customFieldTableCellRepeater" onitemdatabound="CustomFieldTableCellRepeater_ItemDataBound">
                                <headertemplate>
                                    <div class="Gafware_Modules_Reservations_CustomFields">
                                </headertemplate>
                                <itemtemplate>
                                    <div runat="server" id="customFieldLabelTableCell" class="Gafware_Modules_Reservations_CustomFields_Label">
                                        <asp:label id="label" runat="server" cssclass="Gafware_Modules_Reservations_SubHead" />
                                    </div>
                                    <div runat="server" id="customFieldTableCell">
                                        <asp:customvalidator runat="server" id="requiredFieldValidator" onservervalidate="ValidateRequired"
                                            validateemptytext="true" validationgroup="contactInfoValidationGroup" style="display:none" />
                                        <asp:label runat="server" id="customFieldDefinitionIDLabel" text='<%#DataBinder.Eval( Container.DataItem, "CustomFieldDefinitionID" )%>'
                                            visible="false" />
                                        <asp:textbox runat="server" id="textBox" cssclass="Gafware_Modules_Reservations_Input"
                                            width="100%" visible="false" validationgroup="contactInfoValidationGroup" />
                                        <asp:checkbox runat="server" id="checkBox" cssclass="Gafware_Modules_Reservations_Normal Gafware_Modules_Reservations_CheckBox"
                                            visible="false" validationgroup="contactInfoValidationGroup" />
                                        <asp:dropdownlist runat="server" id="dropDownList" cssclass="Gafware_Modules_Reservations_Input"
                                            width="100%" visible="false" validationgroup="contactInfoValidationGroup" />
                                        <asp:listbox runat="server" id="listBox" cssclass="Gafware_Modules_Reservations_Input"
                                            width="100%" visible="false" validationgroup="contactInfoValidationGroup" />
                                        <asp:checkboxlist runat="server" id="checkBoxList" cssclass="Gafware_Modules_Reservations_Normal"
                                            visible="false" repeatlayout="Table" />
                                        <asp:radiobuttonlist runat="server" id="radioButtonList" cssclass="Gafware_Modules_Reservations_Normal"
                                            visible="false" validationgroup="contactInfoValidationGroup" repeatlayout="Table" />
                                        <input type="date" id="datePicker" runat="server" cssclass="Gafware_Modules_Reservations_Input" width="100%" visible="false" />
                                    </div>
                                </itemtemplate>
                                <footertemplate>
                                    </div>
                                </footertemplate>
                            </asp:repeater>
                        </div>
                    </itemtemplate>
                </asp:repeater>
                <div runat="server" id="descriptionTableRow" class="Gafware_Modules_Reservations_FormItem">
                    <asp:label id="descriptionLabel" runat="server" resourcekey="descriptionLabel" cssclass="Gafware_Modules_Reservations_SubHead" />
                    <asp:textbox runat="server" id="descriptionTextbox" textmode="MultiLine" rows="10"
                        height="150" cssclass="Gafware_Modules_Reservations_Titleable Gafware_Modules_Reservations_Input" />
                </div>
            </div>
            <div class="Gafware_Modules_Reservations_Buttons">
                <asp:linkbutton runat="server" id="contactInfoBackCommandButton" cssclass="Gafware_Modules_Reservations_CommandButton_Link"
                    causesvalidation="False" visible="false"><asp:image runat="server" imageurl="Images/back.png" /><asp:label runat="server" resourcekey="contactInfoBackCommandButton" /></asp:linkbutton>
                <asp:linkbutton runat="server" id="contactInfoNextCommandButton" cssclass="Gafware_Modules_Reservations_CommandButton_Red" style="float: right"
                    validationgroup="contactInfoValidationGroup"><asp:label runat="server" resourcekey="contactInfoNextCommandButton" /></asp:linkbutton>
                <asp:linkbutton runat="server" id="contactInfoConfirmCommandButton" cssclass="Gafware_Modules_Reservations_CommandButton_Green" style="float: right"
                    validationgroup="contactInfoValidationGroup"><asp:label runat="server" id="step2ConfirmCommandButtonLabel" /></asp:linkbutton>
                <asp:linkbutton runat="server" id="contactInfoConfirmAndPayLaterCommandButton" cssclass="Gafware_Modules_Reservations_CommandButton_Green" style="float: right"
                    validationgroup="contactInfoValidationGroup" visible="false"><asp:label runat="server" id="contactInfoConfirmAndPayLaterCommandButtonLabel" resourcekey="ConfirmAndPayLater" /></asp:linkbutton>
            </div>
        </div>
    </div>
    <div runat="server" id="step3Table" visible="false" class="Gafware_Modules_Reservations_Step Gafware_Modules_Reservations_Time">
        <div class="Gafware_Modules_Reservations_Instructions">
            <asp:label runat="server" resourcekey="step3TitleLabel" cssclass="Gafware_Modules_Reservations_Head" />
            <asp:label runat="server" resourcekey="step3SubTitleLabel" cssclass="Gafware_Modules_Reservations_Normal" />
        </div>
        <div class="Gafware_Modules_Reservations_FormContainer">
            <div class="Gafware_Modules_Reservations_Form">
                <asp:placeholder runat="server" id="categoryTableRowPlaceHolder">
                    <div runat="server" id="categoryTableRow" visible="false">
                        <asp:label runat="server" cssclass="Gafware_Modules_Reservations_SubHead" resourcekey="categoryLabel" />
                        <div runat="server" id="categoriesHorizontalScroll" class="Gafware_Modules_Reservations_HorizontalScroll">
                            <asp:linkbutton runat="server" id="categoriesPagerPreviousCommandButton" cssclass="Gafware_Modules_Reservations_LessCommandButton"
                                causesvalidation="False" onclientclick="return false;"><asp:image runat="server" imageurl="Images/back.png" /></asp:linkbutton>
                            <asp:linkbutton runat="server" id="categoriesPagerNextCommandButton" cssclass="Gafware_Modules_Reservations_MoreCommandButton"
                                causesvalidation="False" onclientclick="return false;"><asp:image runat="server" imageurl="Images/next.png" /></asp:linkbutton>
                            <div class="Gafware_Modules_Reservations_HorizontalScroll_Items">
                                <div>
                                    <asp:repeater runat="server" id="categoriesRepeater" onitemdatabound="CategoriesRepeaterItemDataBound">
                                        <itemtemplate>
                                            <asp:linkbutton runat="server" id="categoryLinkButton" oncommand="CategoryLinkButtonClicked"
                                                commandargument="<%#Container.DataItem%>" text="<%#GetCategoryName( ( int )Container.DataItem )%>" />
                                            <asp:label runat="server" id="categoryLabel" cssclass="Gafware_Modules_Reservations_HorizontalScroll_Item_Selected"
                                                text="<%#GetCategoryName( ( int )Container.DataItem )%>" visible="false" />
                                            <asp:label runat="server" id="unavailableCategoryLabel" cssclass="Gafware_Modules_Reservations_HorizontalScroll_Item_Unavailable"
                                                text="<%#GetCategoryName( ( int )Container.DataItem )%>" visible="false" />
                                        </itemtemplate>
                                    </asp:repeater>
                                </div>
                            </div>
                        </div>
                        <asp:dropdownlist runat="server" id="categoriesDropDownList" autopostback="true"
                            cssclass="Gafware_Modules_Reservations_Input"
                            onselectedindexchanged="CategoriesDropDownListSelectedIndexChanged" />
                        <asp:listbox runat="server" id="categoriesListBox" autopostback="true" cssclass="Gafware_Modules_Reservations_Input"
                            rows="4" onselectedindexchanged="CategoriesListBoxSelectedIndexChanged" />
                    </div>
                </asp:placeholder>
                <div runat="server" id="availableDayTableRow" visible="false">
                    <asp:label runat="server" cssclass="Gafware_Modules_Reservations_SubHead" resourcekey="availableDayLabel" />
                    <div runat="server" id="availableDaysHorizontalScroll" class="Gafware_Modules_Reservations_HorizontalScroll">
                        <asp:linkbutton runat="server" id="availableDaysPagerPreviousCommandButton" cssclass="Gafware_Modules_Reservations_LessCommandButton"
                            causesvalidation="False" onclientclick="return false;"><asp:image runat="server" imageurl="Images/back.png" /></asp:linkbutton>
                        <asp:linkbutton runat="server" id="availableDaysPagerNextCommandButton" cssclass="Gafware_Modules_Reservations_MoreCommandButton"
                            causesvalidation="False" onclientclick="return false;"><asp:image runat="server" imageurl="Images/next.png" /></asp:linkbutton>
                        <div class="Gafware_Modules_Reservations_HorizontalScroll_Items">
                            <div>
                                <asp:repeater runat="server" id="availableDaysRepeater" onitemdatabound="AvailableDaysRepeaterItemDataBound">
                                    <itemtemplate>
                                        <asp:linkbutton runat="server" id="availableDayLinkButton"
                                            oncommand="DateLinkButtonClicked" commandargument="<%#( ( DateTime )Container.DataItem ).ToShortDateString( )%>"
                                            text="<%#GetFriendlyDate( ( DateTime )Container.DataItem )%>" />
                                        <asp:label runat="server" id="availableDayLabel" cssclass="Gafware_Modules_Reservations_HorizontalScroll_Item_Selected"
                                            text="<%#GetFriendlyDate( ( DateTime )Container.DataItem )%>" visible="false" />
                                    </itemtemplate>
                                </asp:repeater>
                            </div>
                        </div>
                    </div>
                    <asp:calendar runat="server" cssclass="Gafware_Modules_Reservations_Calendar"
                        id="availableDaysCalendar" ondayrender="CalendarDayRender" onprerender="CalendarPreRender" onselectionchanged="CalendarSelectionChanged"
                        cellpadding="0" cellspacing="0" nextprevformat="CustomText">
                        <dayheaderstyle cssclass="Gafware_Modules_Reservations_DayHeaderStyle Gafware_Modules_Reservations_SubHead" />
                        <daystyle cssclass="Gafware_Modules_Reservations_DayStyle Gafware_Modules_Reservations_Normal" />
                        <nextprevstyle cssclass="Gafware_Modules_Reservations_NextPrevStyle" />
                        <othermonthdaystyle cssclass="Gafware_Modules_Reservations_OtherMonthDayStyle Gafware_Modules_Reservations_DayStyle Gafware_Modules_Reservations_Normal" />
                        <selecteddaystyle cssclass="Gafware_Modules_Reservations_SelectedDayStyle Gafware_Modules_Reservations_DayStyle Gafware_Modules_Reservations_Normal" />
                        <selectorstyle cssclass="Gafware_Modules_Reservations_SelectorStyle" />
                        <titlestyle cssclass="Gafware_Modules_Reservations_TitleStyle Gafware_Modules_Reservations_SubHead" backcolor="Transparent" />
                        <todaydaystyle cssclass="Gafware_Modules_Reservations_TodayDayStyle Gafware_Modules_Reservations_DayStyle Gafware_Modules_Reservations_Normal" />
                        <weekenddaystyle cssclass="Gafware_Modules_Reservations_WeekendDayStyle Gafware_Modules_Reservations_DayStyle Gafware_Modules_Reservations_Normal" />
                    </asp:calendar>
                </div>
                <div runat="server" id="timesOfDayTableRow" visible="false">
                    <asp:label runat="server" cssclass="Gafware_Modules_Reservations_SubHead" resourcekey="timeOfDayLabel" />
                    <div runat="server" id="timeOfDayHorizontalScroll" class="Gafware_Modules_Reservations_HorizontalScroll">
                        <asp:linkbutton runat="server" id="availableTimesOfDayPagerPreviousCommandButton" cssclass="Gafware_Modules_Reservations_LessCommandButton"
                            causesvalidation="False" onclientclick="return false;"><asp:image runat="server" imageurl="Images/back.png" /></asp:linkbutton>
                        <asp:linkbutton runat="server" id="availableTimesOfDayPagerNextCommandButton" cssclass="Gafware_Modules_Reservations_MoreCommandButton"
                            causesvalidation="False" onclientclick="return false;"><asp:image runat="server" imageurl="Images/next.png" /></asp:linkbutton>
                        <div class="Gafware_Modules_Reservations_HorizontalScroll_Items">
                            <div>
                                <asp:repeater runat="server" id="availableTimesOfDayRepeater" onitemdatabound="AvailableTimesOfDayRepeaterItemDataBound">
                                    <itemtemplate>
                                        <asp:linkbutton runat="server" id="availableTimeOfDayLinkButton"
                                            oncommand="TimeOfDayLinkButtonClicked" commandargument="<%#( string )Container.DataItem%>"
                                            text="<%#GetFriendlyTimeOfDay( ( string )Container.DataItem )%>" />
                                        <asp:label runat="server" id="availableTimeOfDayLabel" cssclass="Gafware_Modules_Reservations_HorizontalScroll_Item_Selected"
                                            text="<%#GetFriendlyTimeOfDay( ( string )Container.DataItem )%>" visible="false" />
                                        <asp:label runat="server" id="unavailableTimeOfDayLabel" cssclass="Gafware_Modules_Reservations_HorizontalScroll_Item_Unavailable"
                                            text="<%#GetFriendlyTimeOfDay( ( string )Container.DataItem )%>" visible="false" />
                                    </itemtemplate>
                                </asp:repeater>
                            </div>
                        </div>
                    </div>
                    <asp:dropdownlist runat="server" id="timeOfDayDropDownList" autopostback="true" cssclass="Gafware_Modules_Reservations_Input"
                        onselectedindexchanged="TimeOfDayDropDownListSelectedIndexChanged" />
                    <asp:listbox runat="server" id="timeOfDayListBox" autopostback="true" cssclass="Gafware_Modules_Reservations_Input"
                        rows="4" onselectedindexchanged="TimeOfDayListBoxSelectedIndexChanged" />
                </div>
                <div runat="server" id="timesTableRow" visible="false">
                    <asp:label runat="server" cssclass="Gafware_Modules_Reservations_SubHead" resourcekey="reservationTimeLabel" />
                    <div runat="server" id="timeHorizontalScroll" class="Gafware_Modules_Reservations_HorizontalScroll">
                        <asp:linkbutton runat="server" id="availableTimesPagerPreviousCommandButton" cssclass="Gafware_Modules_Reservations_LessCommandButton"
                            causesvalidation="False" onclientclick="return false;"><asp:image runat="server" imageurl="Images/back.png" /></asp:linkbutton>
                        <asp:linkbutton runat="server" id="availableTimesPagerNextCommandButton" cssclass="Gafware_Modules_Reservations_MoreCommandButton"
                            causesvalidation="False" onclientclick="return false;"><asp:image runat="server" imageurl="Images/next.png" /></asp:linkbutton>
                        <div class="Gafware_Modules_Reservations_HorizontalScroll_Items">
                            <div>
                                <asp:repeater runat="server" id="availableTimesRepeater" onitemdatabound="AvailableTimesRepeaterItemDataBound">
                                    <itemtemplate>
                                        <asp:linkbutton runat="server" id="availableTimeLinkButton"
                                            oncommand="DateTimeLinkButtonClicked" commandargument="<%#( ( DateTime )Container.DataItem ).ToString( )%>"
                                            text="<%#GetFriendlyReservationTime( ( DateTime )Container.DataItem )%>" />
                                        <asp:label runat="server" id="availableTimeLabel" cssclass="Gafware_Modules_Reservations_HorizontalScroll_Item_Selected"
                                            text="<%#GetFriendlyReservationTime( ( DateTime )Container.DataItem )%>" visible="false" />
                                    </itemtemplate>
                                </asp:repeater>
                            </div>
                        </div>
                    </div>
                    <asp:dropdownlist runat="server" id="timeDropDownList" autopostback="true" cssclass="Gafware_Modules_Reservations_Input"
                        onselectedindexchanged="TimeDropDownListSelectedIndexChanged" />
                    <asp:listbox runat="server" id="timeListBox" autopostback="true" cssclass="Gafware_Modules_Reservations_Input"
                        rows="4" onselectedindexchanged="TimeListBoxSelectedIndexChanged" />
                </div>
                <div runat="server" id="durationsTableRow" visible="false">
                    <asp:label runat="server" cssclass="Gafware_Modules_Reservations_SubHead" resourcekey="reservationDurationLabel" />
                    <div runat="server" id="durationHorizontalScroll" class="Gafware_Modules_Reservations_HorizontalScroll">
                        <asp:linkbutton runat="server" id="availableDurationsPagerPreviousCommandButton" cssclass="Gafware_Modules_Reservations_LessCommandButton"
                                causesvalidation="False" onclientclick="return false;"><asp:image runat="server" imageurl="Images/back.png" /></asp:linkbutton>
                        <asp:linkbutton runat="server" id="availableDurationsPagerNextCommandButton" cssclass="Gafware_Modules_Reservations_MoreCommandButton"
                            causesvalidation="False" onclientclick="return false;"><asp:image runat="server" imageurl="Images/next.png" /></asp:linkbutton>
                        <div class="Gafware_Modules_Reservations_HorizontalScroll_Items">
                            <div>
                                <asp:repeater runat="server" id="availableDurationsRepeater" onitemdatabound="AvailableDurationsRepeaterItemDataBound">
                                    <itemtemplate>
                                        <asp:linkbutton runat="server" id="availableDurationLinkButton"
                                            oncommand="DurationLinkButtonClicked" commandargument="<%#( ( TimeSpan )Container.DataItem ).ToString( )%>"
                                            text="<%#GetFriendlyReservationDuration( ( TimeSpan )Container.DataItem )%>" />
                                        <asp:label runat="server" id="availableDurationLabel" cssclass="Gafware_Modules_Reservations_HorizontalScroll_Item_Selected"
                                            text="<%#GetFriendlyReservationDuration( ( TimeSpan )Container.DataItem )%>" visible="false" />
                                    </itemtemplate>
                                </asp:repeater>
                            </div>
                        </div>
                    </div>
                    <asp:dropdownlist runat="server" id="durationDropDownList" autopostback="true" cssclass="Gafware_Modules_Reservations_Input"
                        onselectedindexchanged="DurationDropDownListSelectedIndexChanged" />
                    <asp:listbox runat="server" id="durationListBox" autopostback="true" cssclass="Gafware_Modules_Reservations_Input"
                        rows="4" onselectedindexchanged="DurationListBoxSelectedIndexChanged" />
                </div>
                <asp:placeholder runat="server" id="categoryTableRowPlaceHolder2" />
            </div>
            <div class="Gafware_Modules_Reservations_Buttons">
                <asp:linkbutton runat="server" id="step3BackCommandButton" cssclass="Gafware_Modules_Reservations_CommandButton_Link"
                    causesvalidation="False" visible="false"><asp:image runat="server" imageurl="Images/back.png" /><asp:label runat="server" resourcekey="step3BackCommandButton" /></asp:linkbutton>
                <asp:placeholder runat="server" id="step3NextTable">
                    <asp:linkbutton runat="server" id="step3NextCommandButton" cssclass="Gafware_Modules_Reservations_CommandButton_Red" style="float: right"
                        causesvalidation="False"><asp:label runat="server" resourcekey="step3NextCommandButton" /></asp:linkbutton>
                    <asp:linkbutton runat="server" id="step3ConfirmCommandButton" cssclass="Gafware_Modules_Reservations_CommandButton_Green" style="float: right"
                        causesvalidation="False"><asp:label runat="server" id="step3ConfirmCommandButtonLabel" /></asp:linkbutton>
                    <asp:linkbutton runat="server" id="step3ConfirmAndPayLaterCommandButton" cssclass="Gafware_Modules_Reservations_CommandButton_Green" style="float: right"
                        causesvalidation="False" visible="false"><asp:label runat="server" id="step3ConfirmAndPayLaterCommandButtonLabel" resourcekey="ConfirmAndPayLater" /></asp:linkbutton>
                </asp:placeholder>
            </div>
        </div>
    </div>
    <div runat="server" id="viewEditStep1Table" visible="false" class="Gafware_Modules_Reservations_Step Gafware_Modules_Reservations_Lookup">
        <div class="Gafware_Modules_Reservations_Instructions">
            <asp:label runat="server" resourcekey="viewEditStep1TitleLabel" cssclass="Gafware_Modules_Reservations_Head" />
            <asp:label runat="server" resourcekey="viewEditStep1TitleSubLabel" cssclass="Gafware_Modules_Reservations_Normal" />
        </div>
        <div class="Gafware_Modules_Reservations_FormContainer">
            <div class="Gafware_Modules_Reservations_Form">
                <div class="Gafware_Modules_Reservations_FormItem">
                    <asp:label id="viewEditEmailLabel" runat="server" cssclass="Gafware_Modules_Reservations_SubHead"
                        resourcekey="emailLabel" />
                    <asp:textbox runat="server" id="viewEditEmailTextBox" cssclass="Gafware_Modules_Reservations_Input Gafware_Modules_Reservations_Required" />
                    <asp:customvalidator runat="server" id="emailTextBoxRequiredFieldValidator" controltovalidate="viewEditEmailTextBox" onservervalidate="ValidateRequired"
                        validateemptytext="true" validationgroup="viewEditStep1ValidationGroup" />
                    <asp:customvalidator runat="server" controltovalidate="viewEditEmailTextBox" onservervalidate="ValidateEmail"
                        validateemptytext="false" validationgroup="viewEditStep1ValidationGroup" />
                    <asp:customvalidator runat="server" controltovalidate="viewEditEmailTextBox" onservervalidate="ValidateViewEditEmail"
                        validateemptytext="true" validationgroup="viewEditStep1ValidationGroup" />
                    <asp:customvalidator runat="server" controltovalidate="viewEditEmailTextBox" onservervalidate="ValidateRequired"
                        validateemptytext="true" validationgroup="viewEditStep1VerificationCodeValidationGroup" />
                    <asp:customvalidator runat="server" controltovalidate="viewEditEmailTextBox" onservervalidate="ValidateEmail"
                        validateemptytext="false" validationgroup="viewEditStep1VerificationCodeValidationGroup" />
                </div>
                <div runat="server" id="viewEditPhoneTableRow1" class="Gafware_Modules_Reservations_FormItem">
                    <asp:label id="viewEditPhoneLabel" runat="server" cssclass="Gafware_Modules_Reservations_SubHead"
                        resourcekey="orPhoneLabel" />
                    <asp:textbox runat="server" id="viewEditPhoneTextBox" cssclass="Gafware_Modules_Reservations_Input Gafware_Modules_Reservations_Required" />
                </div>
                <div runat="server" id="viewEditVerificationCodeTableRow" class="Gafware_Modules_Reservations_FormItem">
                    <asp:label id="viewEditVerificationCodeLabel" runat="server" cssclass="Gafware_Modules_Reservations_SubHead"
                        resourcekey="verificationCodeLabel" />
                    <asp:textbox runat="server" id="viewEditVerificationCodeTextBox" textmode="Password" cssclass="Gafware_Modules_Reservations_Input Gafware_Modules_Reservations_Required" />
                    <asp:customvalidator runat="server" controltovalidate="viewEditVerificationCodeTextBox" onservervalidate="ValidateRequired"
                        validateemptytext="true" validationgroup="viewEditStep1ValidationGroup" />
                </div>
                <div runat="server" id="viewEditVerificationCodeTableRow2" class="Gafware_Modules_Reservations_FormItem" style="text-align: right;">
                    <asp:linkbutton runat="server" cssclass="Gafware_Modules_Reservations_CommandButton_Link" onclick="SendVerificationCode"
                        validationgroup="viewEditStep1VerificationCodeValidationGroup"><asp:label runat="server" resourcekey="viewEditVerificationCodeLabel" /></asp:linkbutton>
                </div>
            </div>
            <div class="Gafware_Modules_Reservations_Buttons">
                <asp:linkbutton runat="server" id="viewEditStep1BackCommandButton" cssclass="Gafware_Modules_Reservations_CommandButton_Link"
                    causesvalidation="False"><asp:image runat="server" imageurl="Images/back.png" /><asp:label runat="server" resourcekey="viewEditStep1BackCommandButton" /></asp:linkbutton>
                <asp:linkbutton runat="server" id="viewEditStep1NextCommandButton" cssclass="Gafware_Modules_Reservations_CommandButton_Red" style="float: right"
                    validationgroup="viewEditStep1ValidationGroup"><asp:label runat="server" resourcekey="viewEditStep1NextCommandButton" /></asp:linkbutton>
            </div>
        </div>
    </div>
    <div runat="server" id="viewEditStep2Table" visible="false" class="Gafware_Modules_Reservations_Step Gafware_Modules_Reservations_Lookup_2">
        <div class="Gafware_Modules_Reservations_Instructions">
            <asp:label runat="server" resourcekey="viewEditStep2TitleLabel" cssclass="Gafware_Modules_Reservations_Head" style="text-transform: none" />
            <asp:label runat="server" resourcekey="viewEditStep2SubTitleLabel" cssclass="Gafware_Modules_Reservations_Normal" />
        </div>
        <div class="Gafware_Modules_Reservations_FormContainer">
            <div class="Gafware_Modules_Reservations_Form">
                <asp:repeater runat="server" id="viewEditRepeater">
                    <itemtemplate>
                        <div class="Gafware_Modules_Reservations_FormItem">
                            <asp:linkbutton runat="server" cssclass="Gafware_Modules_Reservations_CommandButton_Link"
                                causesvalidation="False" visible="false"><asp:label runat="server" resourcekey="returnCommandButton" /></asp:linkbutton>
                            <asp:linkbutton runat="server" cssclass="Gafware_Modules_Reservations_CommandButton_Gray"
                                oncommand="ViewEditStep2EventCommandButtonClicked" commandargument='<%#DataBinder.Eval( Container.DataItem, "ReservationID" )%>'><asp:image runat="server" imageurl="~/images/calendar.png" /><asp:label runat="server" text='<%#GetReservationDateTime( ( Gafware.Modules.Reservations.ReservationInfo )Container.DataItem )%>' /></asp:linkbutton>
                        </div>
                    </itemtemplate>
                </asp:repeater>
            </div>
            <div class="Gafware_Modules_Reservations_Buttons">
                <asp:linkbutton runat="server" id="viewEditStep2BackCommandButton" cssclass="Gafware_Modules_Reservations_CommandButton_Link"
                    causesvalidation="False"><asp:image runat="server" imageurl="Images/back.png" /><asp:label runat="server" resourcekey="viewEditStep2BackCommandButton" /></asp:linkbutton>
            </div>
        </div>
    </div>
    <div runat="server" id="step4Table" visible="false" class="Gafware_Modules_Reservations_ReservationInfo">
        <div class="Gafware_Modules_Reservations_Step">
            <div class="Gafware_Modules_Reservations_Instructions">
                <asp:label runat="server" resourcekey="step4TitleLabel" cssclass="Gafware_Modules_Reservations_Head" />
                <asp:label runat="server" resourcekey="step4SubTitleLabel" cssclass="Gafware_Modules_Reservations_Normal" />
            </div>
            <div class="Gafware_Modules_Reservations_FormContainer">
                <div class="Gafware_Modules_Reservations_Form">
                    <div class="Gafware_Modules_Reservations_FormItem">
                        <asp:label runat="server" cssclass="Gafware_Modules_Reservations_SubHead" resourcekey="When" />
                        <asp:label id="reservationDateTimeLabel" runat="server" cssclass="Gafware_Modules_Reservations_Normal" />
                    </div>
                    <div runat="server" id="step4CategoryTableRow" class="Gafware_Modules_Reservations_FormItem">
                        <asp:label runat="server" cssclass="Gafware_Modules_Reservations_SubHead" resourcekey="step4CategoryLabel" />
                        <asp:label id="step4CategoryLabel2" runat="server" cssclass="Gafware_Modules_Reservations_Normal" />
                    </div>
                    <div class="Gafware_Modules_Reservations_FormItem">
                        <asp:label runat="server" cssclass="Gafware_Modules_Reservations_SubHead" resourcekey="step4NameLabel" />
                        <asp:label id="step4NameLabel2" runat="server" cssclass="Gafware_Modules_Reservations_Normal" />
                    </div>
                    <div class="Gafware_Modules_Reservations_FormItem">
                        <asp:label runat="server" cssclass="Gafware_Modules_Reservations_SubHead" resourcekey="step4EmailLabel" />
                        <asp:label id="step4EmailLabel2" runat="server" cssclass="Gafware_Modules_Reservations_Normal" />
                    </div>
                    <div class="Gafware_Modules_Reservations_FormItem">
                        <asp:label runat="server" cssclass="Gafware_Modules_Reservations_SubHead" resourcekey="step4PhoneLabel" />
                        <asp:label id="step4PhoneLabel2" runat="server" cssclass="Gafware_Modules_Reservations_Normal" />
                    </div>
                    <asp:repeater runat="server" id="customFieldTableRowRepeater2">
                        <itemtemplate>
                            <div class="Gafware_Modules_Reservations_FormItem">
                                <asp:label id="label" runat="server" cssclass="Gafware_Modules_Reservations_SubHead" text='<%#DataBinder.Eval( Container.DataItem, "Label" )%>' />
                                <asp:label runat="server" id="customFieldValueLabel" cssclass="Gafware_Modules_Reservations_Normal" text='<%#GetCustomFieldValue( ( Gafware.Modules.Reservations.CustomFieldDefinitionInfo )Container.DataItem, true )%>' />
                            </div>
                        </itemtemplate>
                    </asp:repeater>
                    <div runat="server" id="descriptionTableRow2" class="Gafware_Modules_Reservations_FormItem">
                        <asp:label runat="server" cssclass="Gafware_Modules_Reservations_SubHead" resourcekey="descriptionLabel" />
                        <pre class="Gafware_Modules_Reservations_Pre Gafware_Modules_Reservations_Normal" style="white-space: pre-wrap"><asp:label id="step4DescriptionLabel" runat="server" cssclass="Gafware_Modules_Reservations_Normal" /></pre>
                    </div>
                </div>
                <div class="Gafware_Modules_Reservations_Buttons">
                    <asp:linkbutton runat="server" id="returnCommandButton" cssclass="Gafware_Modules_Reservations_CommandButton_Link"
                        causesvalidation="False" visible="false"><asp:image runat="server" imageurl="Images/back.png" /><asp:label runat="server" resourcekey="returnCommandButton" /></asp:linkbutton>
                    <asp:linkbutton runat="server" id="confirmReservationCommandButton" cssclass="Gafware_Modules_Reservations_CommandButton_Gray"
                        causesvalidation="False" visible="false"><asp:label runat="server" resourcekey="Confirm" /></asp:linkbutton>
                    <asp:linkbutton runat="server" id="cancelReservationCommandButton" cssclass="Gafware_Modules_Reservations_CommandButton_Gray"
                        causesvalidation="False" visible="false"><asp:label runat="server" id="cancelReservationCommandButtonLabel" /></asp:linkbutton>
                    <asp:linkbutton runat="server" id="rescheduleReservationCommandButton" cssclass="Gafware_Modules_Reservations_CommandButton_Gray"
                        causesvalidation="False" visible="false"><asp:label runat="server" resourcekey="rescheduleReservationCommandButton" /></asp:linkbutton>
                    <asp:linkbutton runat="server" id="payCommandButton" cssclass="Gafware_Modules_Reservations_CommandButton_Gray"
                        causesvalidation="False" visible="false"><asp:label runat="server" id="payCommandButtonLabel" /></asp:linkbutton>
                    <asp:linkbutton runat="server" id="scheduleAnotherReservationCommandButton" cssclass="Gafware_Modules_Reservations_CommandButton_Gray"
                        causesvalidation="False" visible="false"><asp:label runat="server" resourcekey="scheduleAnotherReservationCommandButton" /></asp:linkbutton>
                    <asp:linkbutton runat="server" id="approveCommandButton" cssclass="Gafware_Modules_Reservations_CommandButton_Gray"
                        causesvalidation="False" visible="false"><asp:label runat="server" resourcekey="approveCommandButton" /></asp:linkbutton>
                    <asp:linkbutton runat="server" id="declineCommandButton" cssclass="Gafware_Modules_Reservations_CommandButton_Gray"
                        causesvalidation="False" visible="false"><asp:label runat="server" resourcekey="declineCommandButton" /></asp:linkbutton>
                    <asp:linkbutton runat="server" id="doneCommandButton" cssclass="Gafware_Modules_Reservations_CommandButton_Gray"
                        causesvalidation="False"><asp:label runat="server" resourcekey="doneCommandButton" /></asp:linkbutton>
                </div>
                <div runat="server" id="lastActionTable" class="Gafware_Modules_Reservations_LastAction" visible="false">
                    <asp:label runat="server" cssclass="Gafware_Modules_Reservations_SubHead" id="lastActionLabel" />
                    <asp:label runat="server" cssclass="Gafware_Modules_Reservations_SubHead" id="lastActionByDisplayNameLabel" />
                    <asp:label runat="server" cssclass="Gafware_Modules_Reservations_SubHead" resourcekey="lastActionOnLabel" />
                    <asp:label runat="server" cssclass="Gafware_Modules_Reservations_SubHead" id="lastActionDateLabel" />
                </div>
            </div>
        </div>
    </div>
    <div runat="server" id="actionsTable" class="Gafware_Modules_Reservations_Actions">
        <asp:linkbutton runat="server" id="viewEditAReservationCommandButton" cssclass="Gafware_Modules_Reservations_CommandButton_Blue"
            causesvalidation="False"><asp:label runat="server" resourcekey="viewEditAReservationCommandButton" /></asp:linkbutton>
        <asp:linkbutton runat="server" id="viewReservationsCommandButton" cssclass="Gafware_Modules_Reservations_CommandButton_Gray"
            causesvalidation="False"><asp:label runat="server" resourcekey="viewReservationsCommandButton" /></asp:linkbutton>
        <asp:linkbutton runat="server" visible="false" id="viewReservationsCalendarCommandButton" cssclass="Gafware_Modules_Reservations_CommandButton_Gray"
            causesvalidation="False"><asp:label runat="server" resourcekey="viewReservationsCalendarCommandButton" /></asp:linkbutton>
        <asp:linkbutton runat="server" id="moderateCommandButton" cssclass="Gafware_Modules_Reservations_CommandButton_Gray"
            causesvalidation="False"><asp:label runat="server" resourcekey="moderateCommandButton" /></asp:linkbutton>
        <asp:linkbutton runat="server" id="cashierCommandButton" cssclass="Gafware_Modules_Reservations_CommandButton_Gray"
            causesvalidation="False"><asp:label runat="server" resourcekey="cashierCommandButton" /></asp:linkbutton>
        <asp:linkbutton runat="server" id="duplicateReservationsCommandButton" cssclass="Gafware_Modules_Reservations_CommandButton_Gray"
            causesvalidation="False"><asp:label runat="server" resourcekey="duplicateReservationsCommandButton" /></asp:linkbutton>
        <asp:linkbutton runat="server" id="mainSettingsCommandButton" cssclass="Gafware_Modules_Reservations_CommandButton_Gray"
            causesvalidation="False"><asp:label runat="server" resourcekey="mainSettingsCommandButton" /></asp:linkbutton>
    </div>
    <asp:placeholder runat="server" id="editionPlaceHolder" />
    <%--<div class="console"></div>--%>
</div>

