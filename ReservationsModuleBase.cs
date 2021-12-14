using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Gafware.Modules.Reservations
{
	public class ReservationsModuleBase : PortalModuleBase
	{
		public const string INSTALLEDON_KEY = "Gafware_Reservations_InstalledOn";

		public const string EDITION_KEY = "Gafware_Reservations_Edition";

		public const string TIMEZONE_KEY = "TimeZone";

		public const string ALLOWCANCELLATIONS_KEY = "AllowCancellations";

		public const string ALLOWCATEGORYSELECTION_KEY = "AllowCategorySelection";

		public const string SELECTCATEGORYLAST_KEY = "SelectCategoryLast";

		public const string PREVENTCROSSCATEGORYCONFLICTS_KEY = "AllowCrossCategoryConflicts";

		public const string CATEGORYSELECTIONMODE_KEY = "CategorySelectionMode";

		public const string TIMEOFDAYSELECTIONMODE_KEY = "TimeOfDaySelectionMode";

		public const string TIMESELECTIONMODE_KEY = "TimeSelectionMode";

		public const string DURATIONSELECTIONMODE_KEY = "DurationSelectionMode";

		public const string DISPLAYUNAVAILABLECATEGORIES_KEY = "DisplayUnavailableCategories";

		public const string DISPLAYREMAININGRESERVATIONS_KEY = "DisplayRemainingReservations";

		public const string ALLOWDESCRIPTION_KEY = "AllowDescription";

		public const string ALLOWRESCHEDULING_KEY = "AllowRescheduling";

		public const string ALLOWSCHEDULINGANOTHERRESERVATION_KEY = "AllowSchedulingAnotherReservation";

		public const string RESERVATIONINTERVAL_KEY = "ReservationInterval";

		public const string RESERVATIONDURATION_KEY = "ReservationDuration";

		public const string RESERVATIONDURATIONMAX_KEY = "ReservationDurationMax";

		public const string RESERVATIONDURATIONINTERVAL_KEY = "ReservationDurationInterval";

		public const string BCCLIST_KEY = "BCCList";

		public const string MINTIMEAHEAD_KEY = "MinTimeAhead";

		public const string DAYSAHEAD_KEY = "DaysAhead";

		public const string DISPLAYCALENDAR_KEY = "DisplayCalendar";

		public const string DISPLAYTIMEOFDAY_KEY = "DisplayTimeOfDay";

		public const string DISPLAYUNAVAILABLETIMEOFDAY_KEY = "DisplayUnavailableTimeOfDay";

		public const string DISPLAYENDTIME_KEY = "DisplayEndTime";

		public const string ICALENDARATTACHMENTFILENAME_KEY = "ICalendarAttachmentFileName";

		public const string ACTIVATIONCODE_KEY = "Gafware_Reservations_Activation";

		public const string SENDREMINDERS_KEY = "SendReminders";

		public const string SENDREMINDERSWHEN_KEY = "SendRemindersWhen";

		public const string SENDREMINDERSVIA_KEY = "SendRemindersVia";

		public const string REQUIRECONFIRMATION_KEY = "RequireConfirmation";

		public const string REQUIRECONFIRMATIONWHEN_KEY = "RequireConfirmationWhen";

		public const string THEME_KEY = "Theme";

		public const string MODERATE_KEY = "Moderate";

		public const string MODERATORLIST_KEY = "GlobalModeratorList";

		public const string REDIRECTURL_KEY = "RedirectUrl";

		public const string REQUIREEMAIL_KEY = "RequireEmail";

		public const string REQUIREPHONE_KEY = "RequirePhone";

		public const string ALLOWLOOKUPBYPHONE_KEY = "AllowLookupByPhone";

		public const string MAXCONFLICTINGRESERVATIONS_KEY = "MaxReservationsPerTimeSlot";

		public const string CURRENCY_KEY = "Currency";

		public const string TIMEOFDAYLIST_KEY = "TimesOfDay";

		public const string REQUIREVERIFICATIONCODE_KEY = "RequireVerificationCode";

		public const string VERIFICATIONCODESALT_KEY = "VerificationCodeSalt";

		public const string VIEWRESERVATIONSLIST_KEY = "ViewReservationsList";

		public const string DUPLICATERESERVATIONSLIST_KEY = "DuplicateReservationsList";

		public const string BINDUPONCATEGORYSELECTION_KEY = "BindUponCategorySelection";

		public const string CONTACTINFOFIRST_KEY = "ContactInfoFirst";

		public const string SKIPCONTACTINFOFORAUTHENTICATEDUSERS_KEY = "SkipContactInfoForAuthenticatedUsers";

		public const string MAXRESERVATIONSPERUSER_KEY = "MaxReservationsPerUser";

		public const string ATTACHICALENDAR_KEY = "AttachiCalendar";

		public const string MAILFROM_KEY = "MailFrom";

		public const string CONFIRMATIONMAILSUBJECT_KEY = "ConfirmationMailSubject";

		public const string CONFIRMATIONMAILBODY_KEY = "ConfirmationMailBody";

		public const string CONFIRMATIONMAILBODYTYPE_KEY = "ConfirmationMailBodyType";

		public const string MODIFICATIONMAILSUBJECT_KEY = "ModificationMailSubject";

		public const string MODIFICATIONMAILBODY_KEY = "ModificationMailBody";

		public const string MODIFICATIONMAILBODYTYPE_KEY = "ModificationMailBodyType";

		public const string CANCELLATIONMAILSUBJECT_KEY = "CancellationMailSubject";

		public const string CANCELLATIONMAILBODY_KEY = "CancellationMailBody";

		public const string CANCELLATIONMAILBODYTYPE_KEY = "CancellationMailBodyType";

		public const string RESCHEDULEDMAILSUBJECT_KEY = "RescheduledMailSubject";

		public const string RESCHEDULEDMAILBODY_KEY = "RescheduledMailBody";

		public const string RESCHEDULEDMAILBODYTYPE_KEY = "RescheduledMailBodyType";

		public const string PENDINGRESCHEDULEREFUNDMAILSUBJECT_KEY = "PendingRescheduleRefundMailSubject";

		public const string PENDINGRESCHEDULEREFUNDMAILBODY_KEY = "PendingRescheduleRefundMailBody";

		public const string PENDINGRESCHEDULEREFUNDMAILBODYTYPE_KEY = "PendingRescheduleRefundMailBodyType";

		public const string PENDINGCANCELLATIONREFUNDMAILSUBJECT_KEY = "PendingCancellationRefundMailSubject";

		public const string PENDINGCANCELLATIONREFUNDMAILBODY_KEY = "PendingCancellationRefundMailBody";

		public const string PENDINGCANCELLATIONREFUNDMAILBODYTYPE_KEY = "PendingCancellationRefundMailBodyType";

		public const string PENDINGDECLINATIONREFUNDMAILSUBJECT_KEY = "PendingDeclinationRefundMailSubject";

		public const string PENDINGDECLINATIONREFUNDMAILBODY_KEY = "PendingDeclinationRefundMailBody";

		public const string PENDINGDECLINATIONREFUNDMAILBODYTYPE_KEY = "PendingDeclinationRefundMailBodyType";

		public const string MODERATORMAILSUBJECT_KEY = "ModeratorMailSubject";

		public const string MODERATORMAILBODY_KEY = "ModeratorMailBody";

		public const string MODERATORMAILBODYTYPE_KEY = "ModeratorMailBodyType";

		public const string DECLINEDMAILSUBJECT_KEY = "DeclinedMailSubject";

		public const string DECLINEDMAILBODY_KEY = "DeclinedMailBody";

		public const string DECLINEDMAILBODYTYPE_KEY = "DeclinedMailBodyType";

		public const string VERIFICATIONCODEMAILSUBJECT_KEY = "VerificationCodeMailSubject";

		public const string VERIFICATIONCODEMAILBODY_KEY = "VerificationCodeMailBody";

		public const string VERIFICATIONCODEMAILBODYTYPE_KEY = "VerificationCodeMailBodyType";

		public const string DUPLICATERESERVATIONMAILSUBJECT_KEY = "DuplicateReservationMailSubject";

		public const string DUPLICATERESERVATIONMAILBODY_KEY = "DuplicateReservationMailBody";

		public const string DUPLICATERESERVATIONMAILBODYTYPE_KEY = "DuplicateReservationMailBodyType";

		public const string EMERGENCYREQUESTMAILSUBJECT_KEY = "EmergencyRequestMailSubject";

		public const string EMERGENCYREQUESTMAILBODY_KEY = "EmergencyRequestMailBody";

		public const string EMERGENCYREQUESTMAILBODYTYPE_KEY = "EmergencyRequestMailBodyType";

		public const string REMINDERMAILSUBJECT_KEY = "ReminderMailSubject";

		public const string REMINDERMAILBODY_KEY = "ReminderMailBody";

		public const string REMINDERMAILBODYTYPE_KEY = "ReminderMailBodyType";

		public const string TWILIOACCOUNTSID_KEY = "TwilioAccountSID";

		public const string TWILIOAUTHTOKEN_KEY = "TwilioAuthToken";

		public const string TWILIOFROM_KEY = "TwilioFrom";

		public const string REMINDERSMS_KEY = "ReminderSMS";

		public const string CATEGORYPERMISSIONS_KEY = "CategoryPermissions";

		public const string FEESCHEDULETYPE_KEY = "FeeScheduleType";

		public const string SCHEDULINGFEEINTERVAL_KEY = "SchedulingFeeInterval";

		public const string ALLOWPAYLATER_KEY = "AllowPayLater";

		public const string DEPOSITFEE_KEY = "DepositFee";

		public const string SCHEDULINGFEE_KEY = "SchedulingFee";

		public const string RESCHEDULINGFEE_KEY = "ReschedulingFee";

		public const string CANCELLATIONFEE_KEY = "CancellationFee";

		public const string CASHIERLIST_KEY = "CashierList";

		public const string PAYMENTMETHOD_KEY = "PaymentMethod";

		public const string PENDINGPAYMENTEXPIRATION_KEY = "PendingPaymentExpiration";

		public const string PAYPALURL_KEY = "PayPalSite";

		public const string PAYPALACCOUNT_KEY = "PayPalAccount";

		public const string PAYPALITEMDESCRIPTION_KEY = "PayPalItemDescription";

		public const string AUTHORIZENETAPILOGIN_KEY = "AuthorizeNetApiLogin";

		public const string AUTHORIZENETTRANSACTIONKEY_KEY = "AuthorizeNetTransactionKey";

		public const string AUTHORIZENETTESTMODE_KEY = "AuthorizeNetTestMode";

		public const string AUTHORIZENETMERCHANTHASH_KEY = "AuthorizeNetMerchantHash";

		/*private int PortalId { get; set; }

		private int TabModuleId { get; set; }*/

		private string _FolderName;

		/*public ModuleSettings(int portalId, int tabModuleId)
		{
			PortalId = portalId;
			TabModuleId = tabModuleId;
		}*/

		/*private Hashtable _Settings;
		public Hashtable Settings
		{
			get
			{
				if (_Settings == null)
				{

					_Settings = (new ModuleController()).GetTabModule(TabModuleId).TabModuleSettings;
				}
				return _Settings;
			}
		}*/

		public string ActivationCode
		{
			get
			{
				string str = ComponentBase<IHostController, HostController>.Instance.GetString(ACTIVATIONCODE_KEY);
				if (str == null || str == string.Empty)
				{
					str = Null.NullString;
				}
				return str;
			}
		}

		public bool AllowCancellations
		{
			get
			{
				string item = (string)Settings["AllowCancellations"] ?? bool.TrueString;
				return bool.Parse(item);
			}
		}

		public bool AllowCategorySelection
		{
			get
			{
				string item = (string)Settings["AllowCategorySelection"] ?? bool.FalseString;
				return bool.Parse(item);
			}
		}

		public bool AllowDescription
		{
			get
			{
				string item = (string)Settings["AllowDescription"] ?? bool.TrueString;
				return bool.Parse(item);
			}
		}

		public bool AllowLookupByPhone
		{
			get
			{
				string item = (string)Settings["AllowLookupByPhone"] ?? bool.FalseString;
				return bool.Parse(item);
			}
		}

		public bool AllowPayLater
		{
			get
			{
				string item = (string)Settings["AllowPayLater"] ?? bool.FalseString;
				return bool.Parse(item);
			}
		}

		public bool AllowRescheduling
		{
			get
			{
				string item = (string)Settings["AllowRescheduling"] ?? bool.TrueString;
				return bool.Parse(item);
			}
		}

		public bool AllowSchedulingAnotherReservation
		{
			get
			{
				string item = (string)Settings["AllowSchedulingAnotherReservation"] ?? bool.FalseString;
				return bool.Parse(item);
			}
		}

		public bool AttachICalendar
		{
			get
			{
				string item = (string)Settings["AttachiCalendar"] ?? bool.FalseString;
				return bool.Parse(item);
			}
		}

		public string AuthorizeNetApiLogin
		{
			get
			{
				return (string)Settings["AuthorizeNetApiLogin"];
			}
		}

		public string AuthorizeNetMerchantHash
		{
			get
			{
				return (string)Settings["AuthorizeNetMerchantHash"];
			}
		}

		public bool AuthorizeNetTestMode
		{
			get
			{
				string item = (string)Settings["AuthorizeNetTestMode"];
				if (string.IsNullOrEmpty(item))
				{
					item = bool.FalseString;
				}
				return bool.Parse(item);
			}
		}

		public string AuthorizeNetTransactionKey
		{
			get
			{
				return (string)Settings["AuthorizeNetTransactionKey"];
			}
		}

		public virtual ArrayList BCCList
		{
			get
			{
				string item = (string)Settings["BCCList"] ?? string.Empty;
				//return DeserializeUserIDList(item);
				return DeserializeEmailList(item);
			}
		}

		public bool BindUponCategorySelection
		{
			get
			{
				if (!AllowCategorySelection || SelectCategoryLast || PreventCrossCategoryConflicts)
				{
					return false;
				}
				string item = (string)Settings["BindUponCategorySelection"] ?? bool.FalseString;
				return bool.Parse(item);
			}
		}

		public decimal CancellationFee
		{
			get
			{
				string item = (string)Settings["CancellationFee"] ?? "0";
				return decimal.Parse(item, CultureInfo.InvariantCulture);
			}
		}

		public string CancellationMailBody
		{
			get
			{
				string item = (string)Settings["CancellationMailBody"] ?? Localization.GetString("CancellationMailBody", LocalResourceFile);
				return item;
			}
		}

		public string CancellationMailBodyType
		{
			get
			{
				string item = (string)Settings["CancellationMailBodyType"] ?? Localization.GetString("CancellationMailBodyType", LocalResourceFile);
				return item;
			}
		}

		public string CancellationMailSubject
		{
			get
			{
				string item = (string)Settings["CancellationMailSubject"] ?? Localization.GetString("CancellationMailSubject", LocalResourceFile);
				return item;
			}
		}

		public virtual ArrayList CashierList
		{
			get
			{
				string item = (string)Settings["CashierList"] ?? string.Empty;
				//return DeserializeUserIDList(item);
				return DeserializeEmailList(item);
			}
		}

		public virtual ArrayList CategoryPermissionsList
		{
			get
			{
				string item = (string)Settings["CategoryPermissions"] ?? "-1";
				return DeserializeRoleIDList(item);
			}
		}

		public ReservationsModuleSettingsBase.SelectionModeEnum CategorySelectionMode
		{
			get
			{
				string item = (string)Settings["CategorySelectionMode"] ?? 1.ToString();
				return (ReservationsModuleSettingsBase.SelectionModeEnum)AdjustSelectionModeBasedOnjQuery(int.Parse(item));
			}
		}

		public string ConfirmationMailBody
		{
			get
			{
				string item = (string)Settings["ConfirmationMailBody"] ?? Localization.GetString("ConfirmationMailBody", LocalResourceFile);
				return item;
			}
		}

		public string ConfirmationMailBodyType
		{
			get
			{
				string item = (string)Settings["ConfirmationMailBodyType"] ?? Localization.GetString("ConfirmationMailBodyType", LocalResourceFile);
				return item;
			}
		}

		public string ConfirmationMailSubject
		{
			get
			{
				string item = (string)Settings["ConfirmationMailSubject"] ?? Localization.GetString("ConfirmationMailSubject", LocalResourceFile);
				return item;
			}
		}

		public bool ContactInfoFirst
		{
			get
			{
				string item = (string)Settings["ContactInfoFirst"] ?? bool.FalseString;
				return bool.Parse(item);
			}
		}

		public string Currency
		{
			get
			{
				return (string)Settings["Currency"] ?? "USD";
			}
		}

		public int DaysAhead
		{
			get
			{
				string item = (string)Settings["DaysAhead"] ?? "30";
				return int.Parse(item);
			}
		}

		public string DeclinedMailBody
		{
			get
			{
				string item = (string)Settings["DeclinedMailBody"] ?? Localization.GetString("DeclinedMailBody", LocalResourceFile);
				return item;
			}
		}

		public string DeclinedMailBodyType
		{
			get
			{
				string item = (string)Settings["DeclinedMailBodyType"] ?? Localization.GetString("DeclinedMailBodyType", LocalResourceFile);
				return item;
			}
		}

		public string DeclinedMailSubject
		{
			get
			{
				string item = (string)Settings["DeclinedMailSubject"] ?? Localization.GetString("DeclinedMailSubject", LocalResourceFile);
				return item;
			}
		}

		public decimal DepositFee
		{
			get
			{
				string item = (string)Settings["DepositFee"] ?? "0";
				return decimal.Parse(item, CultureInfo.InvariantCulture);
			}
		}

		public bool DisplayCalendar
		{
			get
			{
				string item = (string)Settings["DisplayCalendar"] ?? bool.TrueString;
				if (bool.Parse(item))
				{
					return true;
				}
				return !Helper.IsjQuery17orHigher;
			}
		}

		public bool DisplayEndTime
		{
			get
			{
				string item = (string)Settings["DisplayEndTime"] ?? bool.TrueString;
				return bool.Parse(item);
			}
		}

		public bool DisplayRemainingReservations
		{
			get
			{
				string item = (string)Settings["DisplayRemainingReservations"] ?? bool.TrueString;
				return bool.Parse(item);
			}
		}

		public bool DisplayTimeOfDay
		{
			get
			{
				string item = (string)Settings["DisplayTimeOfDay"] ?? bool.TrueString;
				return bool.Parse(item);
			}
		}

		public bool DisplayUnavailableCategories
		{
			get
			{
				string item = (string)Settings["DisplayUnavailableCategories"] ?? bool.FalseString;
				return bool.Parse(item);
			}
		}

		public bool DisplayUnavailableTimeOfDay
		{
			get
			{
				string item = (string)Settings["DisplayUnavailableTimeOfDay"] ?? bool.TrueString;
				return bool.Parse(item);
			}
		}

		public string DuplicateReservationMailBody
		{
			get
			{
				string item = (string)Settings["DuplicateReservationMailBody"] ?? Localization.GetString("DuplicateReservationMailBody", LocalResourceFile);
				return item;
			}
		}

		public string DuplicateReservationMailBodyType
		{
			get
			{
				string item = (string)Settings["DuplicateReservationMailBodyType"] ?? Localization.GetString("DuplicateReservationMailBodyType", LocalResourceFile);
				return item;
			}
		}

		public string DuplicateReservationMailSubject
		{
			get
			{
				string item = (string)Settings["DuplicateReservationMailSubject"] ?? Localization.GetString("DuplicateReservationMailSubject", LocalResourceFile);
				return item;
			}
		}

		public virtual ArrayList DuplicateReservationsList
		{
			get
			{
				string item = (string)Settings["DuplicateReservationsList"] ?? string.Empty;
				//return DeserializeUserIDList(item);
				return DeserializeEmailList(item);
			}
		}

		public ReservationsModuleSettingsBase.SelectionModeEnum DurationSelectionMode
		{
			get
			{
				string item = (string)Settings["DurationSelectionMode"] ?? 1.ToString();
				return (ReservationsModuleSettingsBase.SelectionModeEnum)AdjustSelectionModeBasedOnjQuery(int.Parse(item));
			}
		}

		public string EmergencyRequestMailBody
		{
			get
			{
				string item = (string)Settings["EmergencyRequestMailBody"] ?? Localization.GetString("EmergencyRequestMailBody", LocalResourceFile);
				return item;
			}
		}

		public string EmergencyRequestMailBodyType
		{
			get
			{
				string item = (string)Settings["EmergencyRequestMailBodyType"] ?? Localization.GetString("EmergencyRequestMailBodyType", LocalResourceFile);
				return item;
			}
		}

		public string EmergencyRequestMailSubject
		{
			get
			{
				string item = (string)Settings["EmergencyRequestMailSubject"] ?? Localization.GetString("EmergencyRequestMailSubject", LocalResourceFile);
				return item;
			}
		}

		public Gafware.Modules.Reservations.FeeScheduleType FeeScheduleType
		{
			get
			{
				string item = (string)Settings["FeeScheduleType"] ?? (SchedulingFee != decimal.Zero || ReschedulingFee != decimal.Zero || CancellationFee != decimal.Zero ? Gafware.Modules.Reservations.FeeScheduleType.Flat.ToString() : Gafware.Modules.Reservations.FeeScheduleType.Free.ToString());
				return (Gafware.Modules.Reservations.FeeScheduleType)Enum.Parse(typeof(Gafware.Modules.Reservations.FeeScheduleType), item);
			}
		}

		public Gafware.Modules.Reservations.FlatFeeScheduleInfo FlatFeeScheduleInfo
		{
			get
			{
				Gafware.Modules.Reservations.FlatFeeScheduleInfo flatFeeScheduleInfo = new Gafware.Modules.Reservations.FlatFeeScheduleInfo()
				{
					DepositFee = DepositFee,
					ReservationFee = SchedulingFee,
					ReschedulingFee = ReschedulingFee,
					CancellationFee = CancellationFee
				};
				if (!Settings.ContainsKey("SchedulingFeeInterval"))
				{
					flatFeeScheduleInfo.Interval = (int)ReservationDuration.TotalMinutes;
				}
				else
				{
					flatFeeScheduleInfo.Interval = int.Parse((string)Settings["SchedulingFeeInterval"]);
				}
				return flatFeeScheduleInfo;
			}
		}

		private string FolderName
		{
			get
			{
				if (_FolderName == null)
				{
					_FolderName = DesktopModuleController.GetDesktopModuleByModuleName("Gafware.Modules.Reservations", PortalId).FolderName;
				}
				return _FolderName;
			}
		}

		public string ICalendarAttachmentFileName
		{
			get
			{
				string item = (string)Settings["ICalendarAttachmentFileName"] ?? Localization.GetString("ICalendarAttachmentFileName", LocalResourceFile);
				return item;
			}
		}

		public string this[string key]
		{
			get
			{
				return (string)Settings[key];
			}
		}

		public string ItemDescription
		{
			get
			{
				string item = (string)Settings["PayPalItemDescription"] ?? Localization.GetString("PayPalItemDescription", LocalResourceFile);
				return item;
			}
		}

		/*private string LocalResourceFile
		{
			get
			{
				return string.Concat(new string[] { Globals.ApplicationPath, "/DesktopModules/", FolderName, "/", Localization.LocalResourceDirectory, "/MakeReservation" });
			}
		}*/

		public string MailFrom
		{
			get
			{
				string item = (string)Settings["MailFrom"] ?? (new PortalController()).GetPortal(PortalId).Email;
				return item;
			}
		}

		public int MaxConflictingReservations
		{
			get
			{
				string item = (string)Settings["MaxReservationsPerTimeSlot"] ?? "1";
				return int.Parse(item);
			}
		}

		public int MaxReservationsPerUser
		{
			get
			{
				string item = (string)Settings["MaxReservationsPerUser"] ?? Null.NullInteger.ToString();
				return int.Parse(item);
			}
		}

		public TimeSpan MinTimeAhead
		{
			get
			{
				string item = (string)Settings["MinTimeAhead"];
				if (item == null)
				{
					TimeSpan timeSpan = new TimeSpan();
					item = timeSpan.ToString();
				}
				return TimeSpan.Parse(item);
			}
		}

		public bool Moderate
		{
			get
			{
				string item = (string)Settings["Moderate"] ?? bool.FalseString;
				return bool.Parse(item);
			}
		}

		public virtual ArrayList ModeratorList
		{
			get
			{
				string item = (string)Settings["GlobalModeratorList"] ?? string.Empty;
				//return DeserializeUserIDList(item);
				return DeserializeEmailList(item);
			}
		}

		public string ModeratorMailBody
		{
			get
			{
				string item = (string)Settings["ModeratorMailBody"] ?? Localization.GetString("ModeratorMailBody", LocalResourceFile);
				return item;
			}
		}

		public string ModeratorMailBodyType
		{
			get
			{
				string item = (string)Settings["ModeratorMailBodyType"] ?? Localization.GetString("ModeratorMailBodyType", LocalResourceFile);
				return item;
			}
		}

		public string ModeratorMailSubject
		{
			get
			{
				string item = (string)Settings["ModeratorMailSubject"] ?? Localization.GetString("ModeratorMailSubject", LocalResourceFile);
				return item;
			}
		}

		public string ModificationMailBody
		{
			get
			{
				string item = (string)Settings["ModificationMailBody"] ?? Localization.GetString("ModificationMailBody", LocalResourceFile);
				return item;
			}
		}

		public string ModificationMailBodyType
		{
			get
			{
				string item = (string)Settings["ModificationMailBodyType"] ?? Localization.GetString("ModificationMailBodyType", LocalResourceFile);
				return item;
			}
		}

		public string ModificationMailSubject
		{
			get
			{
				string item = (string)Settings["ModificationMailSubject"] ?? Localization.GetString("ModificationMailSubject", LocalResourceFile);
				return item;
			}
		}

		public Gafware.Modules.Reservations.PaymentMethod PaymentMethod
		{
			get
			{
				string item = (string)Settings["PaymentMethod"];
				if (string.IsNullOrEmpty(item))
				{
					item = Gafware.Modules.Reservations.PaymentMethod.PayPalPaymentsStandard.ToString();
				}
				return (Gafware.Modules.Reservations.PaymentMethod)Enum.Parse(typeof(Gafware.Modules.Reservations.PaymentMethod), item);
			}
		}

		public string PayPalAccount
		{
			get
			{
				return (string)Settings["PayPalAccount"] ?? string.Empty;
			}
		}

		public string PayPalUrl
		{
			get
			{
				return (string)Settings["PayPalSite"] ?? "https://www.paypal.com";
			}
		}

		public string PendingCancellationRefundMailBody
		{
			get
			{
				string item = (string)Settings["PendingCancellationRefundMailBody"] ?? Localization.GetString("PendingCancellationRefundMailBody", LocalResourceFile);
				return item;
			}
		}

		public string PendingCancellationRefundMailBodyType
		{
			get
			{
				string item = (string)Settings["PendingCancellationRefundMailBodyType"] ?? Localization.GetString("PendingCancellationRefundMailBodyType", LocalResourceFile);
				return item;
			}
		}

		public string PendingCancellationRefundMailSubject
		{
			get
			{
				string item = (string)Settings["PendingCancellationRefundMailSubject"] ?? Localization.GetString("PendingCancellationRefundMailSubject", LocalResourceFile);
				return item;
			}
		}

		public string PendingDeclinationRefundMailBody
		{
			get
			{
				string item = (string)Settings["PendingDeclinationRefundMailBody"] ?? Localization.GetString("PendingDeclinationRefundMailBody", LocalResourceFile);
				return item;
			}
		}

		public string PendingDeclinationRefundMailBodyType
		{
			get
			{
				string item = (string)Settings["PendingDeclinationRefundMailBodyType"] ?? Localization.GetString("PendingDeclinationRefundMailBodyType", LocalResourceFile);
				return item;
			}
		}

		public string PendingDeclinationRefundMailSubject
		{
			get
			{
				string item = (string)Settings["PendingDeclinationRefundMailSubject"] ?? Localization.GetString("PendingDeclinationRefundMailSubject", LocalResourceFile);
				return item;
			}
		}

		public TimeSpan PendingPaymentExpiration
		{
			get
			{
				string item = (string)Settings["PendingPaymentExpiration"] ?? "30";
				return new TimeSpan(0, int.Parse(item), 0);
			}
		}

		public string PendingRescheduleRefundMailBody
		{
			get
			{
				string item = (string)Settings["PendingRescheduleRefundMailBody"] ?? Localization.GetString("PendingRescheduleRefundMailBody", LocalResourceFile);
				return item;
			}
		}

		public string PendingRescheduleRefundMailBodyType
		{
			get
			{
				string item = (string)Settings["PendingRescheduleRefundMailBodyType"] ?? Localization.GetString("PendingRescheduleRefundMailBodyType", LocalResourceFile);
				return item;
			}
		}

		public string PendingRescheduleRefundMailSubject
		{
			get
			{
				string item = (string)Settings["PendingRescheduleRefundMailSubject"] ?? Localization.GetString("PendingRescheduleRefundMailSubject", LocalResourceFile);
				return item;
			}
		}

		public bool PreventCrossCategoryConflicts
		{
			get
			{
				string item = (string)Settings["AllowCrossCategoryConflicts"] ?? bool.FalseString;
				return bool.Parse(item);
			}
		}

		public string RedirectUrl
		{
			get
			{
				return (string)Settings["RedirectUrl"] ?? string.Empty;
			}
		}

		public string ReminderMailBody
		{
			get
			{
				string item = (string)Settings["ReminderMailBody"] ?? Localization.GetString("ReminderMailBody", LocalResourceFile);
				return item;
			}
		}

		public string ReminderMailBodyType
		{
			get
			{
				string item = (string)Settings["ReminderMailBodyType"] ?? Localization.GetString("ReminderMailBodyType", LocalResourceFile);
				return item;
			}
		}

		public string ReminderMailSubject
		{
			get
			{
				string item = (string)Settings["ReminderMailSubject"] ?? Localization.GetString("ReminderMailSubject", LocalResourceFile);
				return item;
			}
		}

		public string ReminderSMS
		{
			get
			{
				string item = (string)Settings["ReminderSMS"] ?? Localization.GetString("ReminderSMS", LocalResourceFile);
				return item;
			}
		}

		public bool RequireConfirmation
		{
			get
			{
				string item = (string)Settings["RequireConfirmation"] ?? bool.FalseString;
				return bool.Parse(item);
			}
		}

		public TimeSpan RequireConfirmationWhen
		{
			get
			{
				string item = (string)Settings["RequireConfirmationWhen"];
				if (item == null)
				{
					TimeSpan timeSpan = new TimeSpan(0, 8, 0, 0);
					item = timeSpan.ToString();
				}
				return TimeSpan.Parse(item);
			}
		}

		public bool RequireEmail
		{
			get
			{
				string item = (string)Settings["RequireEmail"] ?? bool.TrueString;
				return bool.Parse(item);
			}
		}

		public bool RequirePhone
		{
			get
			{
				string item = (string)Settings["RequirePhone"] ?? bool.FalseString;
				return bool.Parse(item);
			}
		}

		public bool RequireVerificationCode
		{
			get
			{
				string item = (string)Settings["RequireVerificationCode"] ?? bool.FalseString;
				return bool.Parse(item);
			}
		}

		public string RescheduledMailBody
		{
			get
			{
				string item = (string)Settings["RescheduledMailBody"] ?? Localization.GetString("RescheduledMailBody", LocalResourceFile);
				return item;
			}
		}

		public string RescheduledMailBodyType
		{
			get
			{
				string item = (string)Settings["RescheduledMailBodyType"] ?? Localization.GetString("RescheduledMailBodyType", LocalResourceFile);
				return item;
			}
		}

		public string RescheduledMailSubject
		{
			get
			{
				string item = (string)Settings["RescheduledMailSubject"] ?? Localization.GetString("RescheduledMailSubject", LocalResourceFile);
				return item;
			}
		}

		public decimal ReschedulingFee
		{
			get
			{
				string item = (string)Settings["ReschedulingFee"] ?? "0";
				return decimal.Parse(item, CultureInfo.InvariantCulture);
			}
		}

		public TimeSpan ReservationDuration
		{
			get
			{
				string item = (string)Settings["ReservationDuration"] ?? "60";
				return new TimeSpan(0, int.Parse(item), 0);
			}
		}

		public TimeSpan ReservationDurationInterval
		{
			get
			{
				string item = (string)Settings["ReservationDurationInterval"];
				if (item == null)
				{
					return ReservationDuration;
				}
				return new TimeSpan(0, int.Parse(item), 0);
			}
		}

		public TimeSpan ReservationDurationMax
		{
			get
			{
				string item = (string)Settings["ReservationDurationMax"];
				if (item == null)
				{
					return ReservationDuration;
				}
				return new TimeSpan(0, int.Parse(item), 0);
			}
		}

		public TimeSpan ReservationInterval
		{
			get
			{
				string item = (string)Settings["ReservationInterval"];
				if (item == null)
				{
					return ReservationDuration;
				}
				return new TimeSpan(0, int.Parse(item), 0);
			}
		}

		public decimal SchedulingFee
		{
			get
			{
				string item = (string)Settings["SchedulingFee"] ?? "0";
				return decimal.Parse(item, CultureInfo.InvariantCulture);
			}
		}

		public List<SeasonalFeeScheduleInfo> SeasonalFeeScheduleList
		{
			get
			{
				string empty = string.Empty;
				for (int i = 1; Settings.ContainsKey(string.Concat("SeasonalFeeScheduleList.", i)); i++)
				{
					empty = string.Concat(empty, Settings[string.Concat("SeasonalFeeScheduleList.", i)]);
				}
				if (string.IsNullOrEmpty(empty))
				{
					return new List<SeasonalFeeScheduleInfo>();
				}
				return Helper.DeserializeSeasonalFeeScheduleList(empty);
			}
		}

		public bool SelectCategoryLast
		{
			get
			{
				if (!AllowCategorySelection)
				{
					return false;
				}
				string item = (string)Settings["SelectCategoryLast"] ?? bool.FalseString;
				return bool.Parse(item);
			}
		}

		public bool SendReminders
		{
			get
			{
				string item = (string)Settings["SendReminders"] ?? bool.FalseString;
				return bool.Parse(item);
			}
		}

		public int SendRemindersVia
		{
			get
			{
				string item = (string)Settings["SendRemindersVia"] ?? 1.ToString();
				return int.Parse(item);
			}
		}

		public TimeSpan SendRemindersWhen
		{
			get
			{
				string item = (string)Settings["SendRemindersWhen"];
				if (item == null)
				{
					TimeSpan timeSpan = new TimeSpan(1, 0, 0, 0);
					item = timeSpan.ToString();
				}
				return TimeSpan.Parse(item);
			}
		}

		public bool SkipContactInfoForAuthenticatedUsers
		{
			get
			{
				string item = (string)Settings["SkipContactInfoForAuthenticatedUsers"] ?? bool.FalseString;
				return bool.Parse(item);
			}
		}

		public string Theme
		{
			get
			{
				return (string)Settings["Theme"] ?? "Responsive";
			}
		}

		public virtual ArrayList TimeOfDayList
		{
			get
			{
				string item = (string)Settings["TimesOfDay"] ?? "Morning,00:00:00-12:00:00;Afternoon,12:00:00-17:00:00;Evening,17:00:00-20:00:00;Night,20:00:00-1.00:00:00";
				return DeserializeTimeOfDayList(item);
			}
		}

		public ReservationsModuleSettingsBase.SelectionModeEnum TimeOfDaySelectionMode
		{
			get
			{
				string item = (string)Settings["TimeOfDaySelectionMode"] ?? 1.ToString();
				return (ReservationsModuleSettingsBase.SelectionModeEnum)AdjustSelectionModeBasedOnjQuery(int.Parse(item));
			}
		}

		public ReservationsModuleSettingsBase.SelectionModeEnum TimeSelectionMode
		{
			get
			{
				string item = (string)Settings["TimeSelectionMode"] ?? 1.ToString();
				return (ReservationsModuleSettingsBase.SelectionModeEnum)AdjustSelectionModeBasedOnjQuery(int.Parse(item));
			}
		}

		public TimeZoneInfo TimeZone
		{
			get
			{
				string item = (string)Settings["TimeZone"];
				if (item == null)
				{
					return PortalSettings.Current.TimeZone;
				}
				return TimeZoneInfo.FindSystemTimeZoneById(item);
			}
		}

		public string TwilioAccountSID
		{
			get
			{
				return (string)Settings["TwilioAccountSID"] ?? string.Empty;
			}
		}

		public string TwilioAuthToken
		{
			get
			{
				return (string)Settings["TwilioAuthToken"] ?? string.Empty;
			}
		}

		public string TwilioFrom
		{
			get
			{
				return (string)Settings["TwilioFrom"] ?? string.Empty;
			}
		}

		public string VerificationCodeMailBody
		{
			get
			{
				string item = (string)Settings["VerificationCodeMailBody"] ?? Localization.GetString("VerificationCodeMailBody", LocalResourceFile);
				return item;
			}
		}

		public string VerificationCodeMailBodyType
		{
			get
			{
				string item = (string)Settings["VerificationCodeMailBodyType"] ?? Localization.GetString("VerificationCodeMailBodyType", LocalResourceFile);
				return item;
			}
		}

		public string VerificationCodeMailSubject
		{
			get
			{
				string item = (string)Settings["VerificationCodeMailSubject"] ?? Localization.GetString("VerificationCodeMailSubject", LocalResourceFile);
				return item;
			}
		}

		public string VerificationCodeSalt
		{
			get
			{
				string item = (string)Settings["VerificationCodeSalt"];
				if (item == null)
				{
					throw new NotSupportedException();
				}
				return item;
			}
		}

		public virtual ArrayList ViewReservationsList
		{
			get
			{
				string item = (string)Settings["ViewReservationsList"] ?? string.Empty;
				//return DeserializeUserIDList(item);
				return DeserializeEmailList(item);
			}
		}

        private int AdjustSelectionModeBasedOnjQuery(int selectionMode)
		{
			if (selectionMode == 1 && !Helper.IsjQuery17orHigher)
			{
				selectionMode = 2;
			}
			return selectionMode;
		}

		public ArrayList DeserializeRoleIDList(string serializedRoleIDList)
		{
			int num;
			ArrayList arrayLists = new ArrayList();
			string[] strArrays = serializedRoleIDList.Split(new char[] { ',' });
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				if (int.TryParse(strArrays[i], out num))
				{
					arrayLists.Add(num);
				}
			}
			return arrayLists;
		}

		public ArrayList DeserializeTimeOfDayList(string serializedTimesOfDayList)
		{
			ArrayList arrayLists = new ArrayList();
			if (serializedTimesOfDayList != string.Empty)
			{
				string[] strArrays = serializedTimesOfDayList.Split(new char[] { ';' });
				for (int i = 0; i < (int)strArrays.Length; i++)
				{
					string str = strArrays[i];
					TimeOfDayInfo timeOfDayInfo = new TimeOfDayInfo()
					{
						Name = str.Split(new char[] { ',' })[0],
						StartTime = TimeSpan.Parse(str.Split(new char[] { ',' })[1].Split(new char[] { '-' })[0]),
						EndTime = TimeSpan.Parse(str.Split(new char[] { ',' })[1].Split(new char[] { '-' })[1])
					};
					arrayLists.Add(timeOfDayInfo);
				}
				arrayLists.Sort();
			}
			return arrayLists;
		}

		public ArrayList DeserializeUserIDList(string serializedUserIDList)
		{
			ArrayList arrayLists = new ArrayList();
			if (serializedUserIDList != string.Empty)
			{
				string[] strArrays = serializedUserIDList.Split(new char[] { ';' });
				for (int i = 0; i < (int)strArrays.Length; i++)
				{
					string str = strArrays[i];
					UserInfo user = (new UserController()).GetUser(PortalId, int.Parse(str));
					if (user != null)
					{
						arrayLists.Add(user);
					}
				}
				arrayLists.Sort(new UserInfoComparer());
			}
			return arrayLists;
		}

		public ArrayList DeserializeEmailList(string serializedEmailList)
		{
			ArrayList arrayLists = new ArrayList();
			if (serializedEmailList != string.Empty)
			{
				string[] strArrays = serializedEmailList.Split(new char[] { ';' });
				for (int i = 0; i < (int)strArrays.Length; i++)
				{
					string str = strArrays[i];
					if (Helper.IsValidEmail2(str))
					{
						IList<UserInfo> users = (new UserController()).GetUsersBasicSearch(PortalId, 0, 1, "Email", true, "Email", str);
						if (users != null && users.Count > 0)
						{
							arrayLists.Add(users[0]);
						}
						else
						{
							UserInfo user = new UserInfo();
							user.UserID = -1;
							user.Email = str;
							user.DisplayName = str;
							arrayLists.Add(user);
						}
					}
					else
                    {
						UserInfo user = (new UserController()).GetUser(PortalId, int.Parse(str));
						if (user != null)
						{
							arrayLists.Add(user);
						}
					}
				}
				arrayLists.Sort(new EmailComparer());
			}
			return arrayLists;
		}

		public bool IsDefined(string settingName)
		{
			return Settings.ContainsKey(settingName);
		}

		public string SerializeRoleIDList(ArrayList roleIDList)
		{
			string empty = string.Empty;
			foreach (int num in roleIDList)
			{
				empty = string.Concat(empty, (empty == string.Empty ? string.Empty : ","), num.ToString());
			}
			return empty;
		}

		public string SerializeTimeOfDayList(ArrayList timesOfDayList)
		{
			string empty = string.Empty;
			foreach (TimeOfDayInfo timeOfDayInfo in timesOfDayList)
			{
				string[] name = new string[] { empty, null, null, null, null, null, null };
				name[1] = (empty == string.Empty ? string.Empty : ";");
				name[2] = timeOfDayInfo.Name;
				name[3] = ",";
				TimeSpan startTime = timeOfDayInfo.StartTime;
				name[4] = startTime.ToString();
				name[5] = "-";
				startTime = timeOfDayInfo.EndTime;
				name[6] = startTime.ToString();
				empty = string.Concat(name);
			}
			return empty;
		}

		public string SerializeUserIDList(ArrayList userInfoList)
		{
			string str;
			string empty = string.Empty;
			foreach (UserInfo userInfo in userInfoList)
			{
				string str1 = empty;
				str = (empty == string.Empty ? string.Empty : ";");
				int userID = userInfo.UserID;
				empty = string.Concat(str1, str, userID.ToString());
			}
			return empty;
		}

		public string SerializeEmailList(ArrayList userInfoList)
		{
			string str;
			string empty = string.Empty;
			foreach (UserInfo userInfo in userInfoList)
			{
				string str1 = empty;
				str = (empty == string.Empty ? string.Empty : ";");
				string email = userInfo.Email;
				empty = string.Concat(str1, str, email);
			}
			return empty;
		}

		public enum SelectionModeEnum
		{
			HorizontalScroll = 1,
			DropDownList = 2,
			ListBox = 3
		}
	}
}