using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Content;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Mail;
using DotNetNuke.UI.Skins;
using DotNetNuke.UI.Skins.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Xml;
using System.Xml.Serialization;
using Twilio;

namespace Gafware.Modules.Reservations
{
	public class Helper
	{
		private DotNetNuke.Entities.Portals.PortalInfo PortalInfo;

		private int TabModuleId;

		private string LocalResourceFile;

		private string NotAvailable;

		private Gafware.Modules.Reservations.ModuleSettings ModuleSettings;

		private Gafware.Modules.Reservations.ReservationController _ProxyController;

		private List<CategoryInfo> _CategoryList;

		private bool? _IsProfesional;

		public static string edition;

		public static string[] editions;

		private static string[] publicKeys;

		private bool invalid;

		public static string ByteArrayToHexString(byte[] ba)
		{
			StringBuilder hex = new StringBuilder(ba.Length * 2);
			foreach (byte b in ba)
			{
				hex.AppendFormat("{0:x2}", b);
			}
			return hex.ToString();
		}

		public static byte[] HexStringToByteArray(string hex)
        {
			byte[] numArray = new byte[hex.Length / 2];
			for (int i = 0; i < hex.Length / 2; i++)
			{ 
				numArray[i] = byte.Parse(hex.Substring(i * 2, 2), NumberStyles.HexNumber);
			}
			return numArray;
		}

		protected List<CategoryInfo> CategoryList
		{
			get
			{
				if (this._CategoryList == null)
				{
					this._CategoryList = (new CategoryController()).GetCategoryList(this.TabModuleId);
				}
				return this._CategoryList;
			}
		}

		public string DateRangeSeparator
		{
			get
			{
				if (CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator != "-")
				{
					return "-";
				}
				return ":";
			}
		}

        public static bool IsjQuery17orHigher
		{
			get
			{
#pragma warning disable CS0618 // Type or member is obsolete
                if (!jQuery.UseHostedScript && jQuery.Version.CompareTo("1.7") >= 0)
#pragma warning restore CS0618 // Type or member is obsolete
                {
					return true;
				}
#pragma warning disable CS0618 // Type or member is obsolete
                if (!jQuery.UseHostedScript)
#pragma warning restore CS0618 // Type or member is obsolete
                {
					return false;
				}
#pragma warning disable CS0618 // Type or member is obsolete
                return Helper.GetjQueryVersionFromUrl(jQuery.HostedUrl).CompareTo("1.7") >= 0;
#pragma warning restore CS0618 // Type or member is obsolete
            }
		}

		private bool IsProfessional
		{
			get
			{
				if (!this._IsProfesional.HasValue)
				{
					this._IsProfesional = new bool?(Helper.GetEdition(this.ModuleSettings.ActivationCode) != "Standard");
				}
				return this._IsProfesional.Value;
			}
		}

		protected Gafware.Modules.Reservations.ReservationController ReservationController
		{
			get
			{
				if (this._ProxyController == null)
				{
					this._ProxyController = new Gafware.Modules.Reservations.ReservationController();
				}
				return this._ProxyController;
			}
		}

		static Helper()
		{
			Helper.editions = new string[] { "Standard", "Professional", "Enterprise" };
			Helper.publicKeys = new string[] { "0602000000A40000525341310004000001000100E584391B7F63B6484D8D8F27D2DC11ED8A47A3AF30E7701E0D47FD117B8C5A54992320654AA8D19465F16368C2E83A901C07DBA516A175CE2BC860C4D6C8822409D46E3FDDBA58E9734B71329AA1F37C55436C184A55A63B4D25C9EB42EC8FEC58FB2D066A34E99EBD869EB2635EDFD1079C51C483988A2323BA4BA3B1A77BAA", "0602000000A400005253413100040000010001003D43E1C605F9EC795C8E7B49B4F80F9FF13E6B4A16931A65791D5F2714454D13CA5DA5E52902B65C52BAC0B351C38FC955E913A58C498843116E862503472B57A769019B3C59C3F2F08F95128B9580AF52ECD6BB99DCB938AAEE3592B47BE9DE6C36FEA4270EEC913A33E838635A4D79D162EB3EA2094858966A5968FD4608B6", "0602000000A40000525341310004000001000100535B829EE45A8E0E428CF9BC2BF0BA9DC47720FDCD16A69F4DFBFE403BE8013079BBAA7500E27A91EA200BF9FDFC9EDDCE40E142AC5C84CDADEB7159E9BCCE17E15A66DA286E7088EC28B5009060B0D001A471458A4B4384A4B8C040EA90C16A53F71B322941933DC5A9A5EAB39F91EA69665F119663C83300CCEDC51C242E91" };
		}

		public Helper(int portalID, int tabModuleID, string localResourceFile)
		{
			this.PortalInfo = (new PortalController()).GetPortal(portalID);
			this.TabModuleId = tabModuleID;
			this.LocalResourceFile = localResourceFile;
			this.NotAvailable = Localization.GetString("NotAvailable", localResourceFile);
			this.ModuleSettings = new Gafware.Modules.Reservations.ModuleSettings(portalID, tabModuleID);
		}

		public static void AddModuleMessage(PortalModuleBase portalModuleBase, string message, ModuleMessage.ModuleMessageType moduleMessageType)
		{
			Skin.AddModuleMessage(portalModuleBase, message, moduleMessageType);
		}

		private void AddOrUpdateICalendarInfo(ICalendarInfo iCalendarInfo)
		{
			if (iCalendarInfo != null)
			{
				ICalendarController calendarController = new ICalendarController();
				if (iCalendarInfo.Sequence == 0)
				{
					calendarController.AddICalendar(iCalendarInfo);
					return;
				}
				calendarController.UpdateICalendar(iCalendarInfo);
			}
		}

		public static void AddOrUpdatePendingPaymentInfo(ReservationInfo eventInfo, int tabModuleId, int portalId, decimal amount, decimal refundableAmount, string currency, int createdByUserId, int status)
		{
			PendingPaymentInfo now = Helper.FindPendingPaymentInfoByEventIDAndStatus((new PendingPaymentController()).GetPendingPaymentList(tabModuleId), eventInfo.ReservationID, status);
			if (now == null)
			{
				(new PendingPaymentController()).AddPendingPayment(Helper.CreatePendingPaymentInfoFromEventInfo(eventInfo, tabModuleId, portalId, amount, refundableAmount, currency, createdByUserId, status));
				return;
			}
			PendingPaymentInfo pendingPaymentInfo = now;
			pendingPaymentInfo.Amount = pendingPaymentInfo.Amount + amount;
			PendingPaymentInfo pendingPaymentInfo1 = now;
			pendingPaymentInfo1.RefundableAmount = pendingPaymentInfo1.RefundableAmount + refundableAmount;
			now.LastModifiedOnDate = DateTime.Now;
			(new PendingPaymentController()).UpdatePendingPayment(now);
		}

		public static void AddOrUpdatePendingPaymentInfoFromPendingPaymentInfo(PendingPaymentInfo pendingPaymentInfo, decimal amount, decimal refundableAmount, int status)
		{
			PendingPaymentInfo now = Helper.FindPendingPaymentInfoByEventIDAndStatus((new PendingPaymentController()).GetPendingPaymentList(pendingPaymentInfo.TabModuleID), pendingPaymentInfo.ReservationID, status);
			if (now == null)
			{
				(new PendingPaymentController()).AddPendingPayment(Helper.CreatePendingPaymentInfoFromPendingPaymentInfo(pendingPaymentInfo, amount, refundableAmount, pendingPaymentInfo.Currency, status, pendingPaymentInfo.CreatedByUserID, pendingPaymentInfo.CreatedByDisplayName));
				return;
			}
			PendingPaymentInfo pendingPaymentInfo1 = now;
			pendingPaymentInfo1.Amount = pendingPaymentInfo1.Amount + amount;
			PendingPaymentInfo pendingPaymentInfo2 = now;
			pendingPaymentInfo2.RefundableAmount = pendingPaymentInfo2.RefundableAmount + refundableAmount;
			now.LastModifiedOnDate = DateTime.Now;
			(new PendingPaymentController()).UpdatePendingPayment(now);
		}

		public decimal CalculateCancellationFee(FeeScheduleType feeScheduleType, FlatFeeScheduleInfo flatFreeScheduleInfo, List<SeasonalFeeScheduleInfo> seasonalFeeScheduleList, DateTime startDateTime)
		{
			decimal cancellationFee;
			if (feeScheduleType == FeeScheduleType.Free)
			{
				return decimal.Zero;
			}
			if (feeScheduleType == FeeScheduleType.Flat)
			{
				return flatFreeScheduleInfo.CancellationFee;
			}
			List<SeasonalFeeScheduleInfo>.Enumerator enumerator = seasonalFeeScheduleList.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					SeasonalFeeScheduleInfo current = enumerator.Current;
					DateTime dateTime = new DateTime(startDateTime.Year, current.StartOnMonth, current.StartOnDay);
					DateTime dateTime1 = new DateTime(startDateTime.Year, current.EndByMonth, current.EndByDay);
					DateTime dateTime2 = dateTime1.AddDays(1);
					if (dateTime2 < dateTime)
					{
						dateTime2 = dateTime2.AddYears(1);
					}
					if (!(startDateTime >= dateTime) || !(startDateTime < dateTime2))
					{
						continue;
					}
					cancellationFee = current.CancellationFee;
					return cancellationFee;
				}
				return decimal.Zero;
			}
			finally
			{
				((IDisposable)enumerator).Dispose();
			}
		}

		public decimal CalculateReschedulingFee(FeeScheduleType feeScheduleType, FlatFeeScheduleInfo flatFreeScheduleInfo, List<SeasonalFeeScheduleInfo> seasonalFeeScheduleList, DateTime startDateTime)
		{
			decimal reschedulingFee;
			if (feeScheduleType == FeeScheduleType.Free)
			{
				return decimal.Zero;
			}
			if (feeScheduleType == FeeScheduleType.Flat)
			{
				return flatFreeScheduleInfo.ReschedulingFee;
			}
			List<SeasonalFeeScheduleInfo>.Enumerator enumerator = seasonalFeeScheduleList.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					SeasonalFeeScheduleInfo current = enumerator.Current;
					DateTime dateTime = new DateTime(startDateTime.Year, current.StartOnMonth, current.StartOnDay);
					DateTime dateTime1 = new DateTime(startDateTime.Year, current.EndByMonth, current.EndByDay);
					DateTime dateTime2 = dateTime1.AddDays(1);
					if (dateTime2 < dateTime)
					{
						dateTime2 = dateTime2.AddYears(1);
					}
					if (!(startDateTime >= dateTime) || !(startDateTime < dateTime2))
					{
						continue;
					}
					reschedulingFee = current.ReschedulingFee;
					return reschedulingFee;
				}
				return decimal.Zero;
			}
			finally
			{
				((IDisposable)enumerator).Dispose();
			}
		}

		public decimal CalculateSchedulingFee(FeeScheduleType feeScheduleType, FlatFeeScheduleInfo flatFreeScheduleInfo, List<SeasonalFeeScheduleInfo> seasonalFeeScheduleList, DateTime startDateTime, TimeSpan duration)
		{
			decimal totalMinutes;
			if (feeScheduleType != FeeScheduleType.Free)
			{
				DateTime dateTime = new DateTime();
				if (startDateTime != dateTime)
				{
					if (feeScheduleType == FeeScheduleType.Flat)
					{
						return flatFreeScheduleInfo.DepositFee + (flatFreeScheduleInfo.ReservationFee * ((decimal)duration.TotalMinutes / flatFreeScheduleInfo.Interval));
					}
					DateTime dateTime1 = startDateTime.Add(duration);
					List<SeasonalFeeScheduleInfo>.Enumerator enumerator = seasonalFeeScheduleList.GetEnumerator();
					try
					{
						while (enumerator.MoveNext())
						{
							SeasonalFeeScheduleInfo current = enumerator.Current;
							DateTime dateTime2 = new DateTime(startDateTime.Year, current.StartOnMonth, current.StartOnDay);
							dateTime = new DateTime(startDateTime.Year, current.EndByMonth, current.EndByDay);
							DateTime dateTime3 = dateTime.AddDays(1);
							if (dateTime3 < dateTime2)
							{
								dateTime3 = dateTime3.AddYears(1);
							}
							if (dateTime1 < dateTime2)
							{
								while (dateTime1 < dateTime2)
								{
									dateTime2 = dateTime2.AddYears(-1);
									dateTime3 = dateTime3.AddYears(-1);
								}
							}
							else if (startDateTime > dateTime3)
							{
								while (startDateTime > dateTime3)
								{
									dateTime2 = dateTime2.AddYears(1);
									dateTime3 = dateTime3.AddYears(1);
								}
							}
							if (!(startDateTime >= dateTime2) || !(startDateTime < dateTime3))
							{
								continue;
							}
							if (dateTime1 > dateTime3)
							{
								decimal reservationFee = current.ReservationFee;
								TimeSpan timeSpan = dateTime3.Subtract(startDateTime);
								totalMinutes = (reservationFee * ((decimal)timeSpan.TotalMinutes / current.Interval)) + this.CalculateSchedulingFee(feeScheduleType, flatFreeScheduleInfo, seasonalFeeScheduleList, dateTime3, dateTime1.Subtract(dateTime3));
								return totalMinutes;
							}
							else
							{
								totalMinutes = current.DepositFee + (current.ReservationFee * ((decimal)duration.TotalMinutes / current.Interval));
								return totalMinutes;
							}
						}
						return decimal.Zero;
					}
					finally
					{
						((IDisposable)enumerator).Dispose();
					}
				}
			}
			return decimal.Zero;
		}

		public bool CanModerate(int userID, int categoryID)
		{
			if (categoryID == Null.NullInteger || categoryID == 0)
			{
				if (!this.ModuleSettings.Moderate)
				{
					return false;
				}
				return this.FindUserInfoByUserId(this.ModuleSettings.ModeratorList, userID) != null;
			}
			CategorySettings categorySetting = new CategorySettings(this.PortalInfo.PortalID, this.TabModuleId, categoryID);
			if (!categorySetting.Moderate)
			{
				return false;
			}
			return this.FindUserInfoByUserId(categorySetting.ModeratorList, userID) != null;
		}

		public bool CanProcessPayment(int userID, int categoryID)
		{
			if (categoryID == Null.NullInteger || categoryID == 0)
			{
				return this.FindUserInfoByUserId(this.ModuleSettings.CashierList, userID) != null;
			}
			CategorySettings categorySetting = new CategorySettings(this.PortalInfo.PortalID, this.TabModuleId, categoryID);
			return this.FindUserInfoByUserId(categorySetting.CashierList, userID) != null;
		}

		public bool CanViewDuplicateReservations(int userID)
		{
			return this.FindUserInfoByUserId(this.ModuleSettings.DuplicateReservationsList, userID) != null;
		}

		public bool CanViewReservations(int userID)
		{
			if ((new ModuleSecurity((new ModuleController()).GetTabModule(this.TabModuleId))).HasEditPermissions)
			{
				return true;
			}
			return this.FindUserInfoByUserId(this.ModuleSettings.ViewReservationsList, userID) != null;
		}

		public static IEnumerable<string> ChunksUpto(string str, int maxChunkSize)
		{
			for (int i = 0; i < str.Length; i = i + maxChunkSize)
			{
				yield return str.Substring(i, Math.Min(maxChunkSize, str.Length - i));
			}
		}

		protected bool Conflicts(DateTime startDateTime1, int duration1, DateTime startDateTime2, int duration2)
		{
			if (startDateTime1 <= startDateTime2 && startDateTime1.AddMinutes((double)duration1) > startDateTime2)
			{
				return true;
			}
			if (startDateTime2 > startDateTime1)
			{
				return false;
			}
			return startDateTime2.AddMinutes((double)duration2) > startDateTime1;
		}

		private string CreateICalendarAttachmentFile(ReservationInfo eventInfo, string method, string summary, ICalendarInfo iCalendarInfo)
		{
			string empty = string.Concat(HttpContext.Current.Server.MapPath("~"), "\\Portals\\_default\\", this.Escape(summary), ".ics");
			StreamWriter streamWriter = null;
			try
			{
				try
				{
					streamWriter = new StreamWriter(empty);
					string str = Localization.GetString("MailAttachment", this.LocalResourceFile);
					str = str.Replace("{Method}", method);
					str = str.Replace("{UID}", iCalendarInfo.UID);
					int sequence = iCalendarInfo.Sequence;
					str = str.Replace("{Sequence}", sequence.ToString());
					str = str.Replace("{Organizer}", iCalendarInfo.Organizer).Replace("{Category}", (eventInfo.CategoryID == Null.NullInteger || eventInfo.CategoryID == 0 ? string.Empty : this.GetCategoryName(eventInfo.CategoryID)));
					DateTime startDateTime = eventInfo.StartDateTime;
					startDateTime = startDateTime.Subtract(this.ModuleSettings.TimeZone.GetUtcOffset(eventInfo.StartDateTime));
					str = str.Replace("{Start}", startDateTime.ToString("yyyyMMdd'T'HHmmss'Z'"));
					startDateTime = eventInfo.EndTime;
					startDateTime = startDateTime.Subtract(this.ModuleSettings.TimeZone.GetUtcOffset(eventInfo.EndTime));
					str = str.Replace("{End}", startDateTime.ToString("yyyyMMdd'T'HHmmss'Z'"));
					str = str.Replace("{Summary}", summary);
					str = str.Replace("{Description}", eventInfo.Description.Replace(Environment.NewLine, "\\n"));
					streamWriter.Write(str);
				}
				catch (Exception)
				{
					empty = string.Empty;
				}
			}
			finally
			{
				if (streamWriter != null)
				{
					streamWriter.Close();
				}
			}
			return empty;
		}

		public static PendingApprovalInfo CreatePendingApprovalInfoFromEventInfo(ReservationInfo eventInfo, int tabModuleId, int portalId, int createdByUserId, int status)
		{
			PendingApprovalInfo pendingApprovalInfo = new PendingApprovalInfo();
			Helper.SetPendingApprovalInfoPropertiesFromEventInfo(pendingApprovalInfo, eventInfo);
			pendingApprovalInfo.TabModuleID = tabModuleId;
			pendingApprovalInfo.PortalID = portalId;
			pendingApprovalInfo.CreatedByUserID = createdByUserId;
			pendingApprovalInfo.CreatedOnDate = DateTime.Now;
			pendingApprovalInfo.Status = status;
			return pendingApprovalInfo;
		}

		public static PendingApprovalInfo CreatePendingApprovalInfoInfoFromPendingPaymentInfo(PendingPaymentInfo pendingPaymentInfo, PendingApprovalStatus status)
		{
			return new PendingApprovalInfo()
			{
				TabModuleID = pendingPaymentInfo.TabModuleID,
				PortalID = pendingPaymentInfo.PortalID,
				ReservationID = pendingPaymentInfo.ReservationID,
				CategoryID = pendingPaymentInfo.CategoryID,
				StartDateTime = pendingPaymentInfo.StartDateTime,
				Duration = pendingPaymentInfo.Duration,
				FirstName = pendingPaymentInfo.FirstName,
				LastName = pendingPaymentInfo.LastName,
				Email = pendingPaymentInfo.Email,
				Phone = pendingPaymentInfo.Phone,
				Description = pendingPaymentInfo.Description,
				CreatedByUserID = pendingPaymentInfo.CreatedByUserID,
				CreatedOnDate = DateTime.Now,
				Status = (int)status
			};
		}

		public static PendingPaymentInfo CreatePendingPaymentInfoFromEventInfo(ReservationInfo eventInfo, int tabModuleId, int portalId, decimal amount, decimal refundableAmount, string currency, int createdByUserId, int status)
		{
			PendingPaymentInfo pendingPaymentInfo = new PendingPaymentInfo();
			Helper.SetPendingPaymentInfoPropertiesFromEventInfo(pendingPaymentInfo, eventInfo);
			pendingPaymentInfo.TabModuleID = tabModuleId;
			pendingPaymentInfo.PortalID = portalId;
			pendingPaymentInfo.Amount = amount;
			pendingPaymentInfo.RefundableAmount = refundableAmount;
			pendingPaymentInfo.Currency = currency;
			pendingPaymentInfo.CreatedByUserID = createdByUserId;
			pendingPaymentInfo.CreatedOnDate = DateTime.Now;
			pendingPaymentInfo.Status = status;
			return pendingPaymentInfo;
		}

		public static PendingPaymentInfo CreatePendingPaymentInfoFromPendingApprovalInfo(PendingApprovalInfo pendingApprovalInfo, decimal amount, decimal refundableAmount, string currency, PendingPaymentStatus status)
		{
			PendingPaymentInfo pendingPaymentInfo = new PendingPaymentInfo();
			Helper.SetPendingPaymentInfoPropertiesFromPendingApprovalInfo(pendingPaymentInfo, pendingApprovalInfo);
			pendingPaymentInfo.Amount = amount;
			pendingPaymentInfo.RefundableAmount = refundableAmount;
			pendingPaymentInfo.Currency = currency;
			pendingPaymentInfo.Status = (int)status;
			return pendingPaymentInfo;
		}

		public static PendingPaymentInfo CreatePendingPaymentInfoFromPendingPaymentInfo(PendingPaymentInfo _pendingPaymentInfo, decimal amount, decimal refundableAmount, string currency, int status, int createdByUserID, string createdByDisplayName)
		{
			PendingPaymentInfo now = Helper.CreatePendingPaymentInfoFromPendingPaymentInfo(_pendingPaymentInfo);
			now.Amount = amount;
			now.RefundableAmount = refundableAmount;
			now.Currency = currency;
			now.CreatedByUserID = createdByUserID;
			now.CreatedOnDate = DateTime.Now;
			now.Status = status;
			return now;
		}

		public static PendingPaymentInfo CreatePendingPaymentInfoFromPendingPaymentInfo(PendingPaymentInfo pendingPaymentInfo)
		{
			return new PendingPaymentInfo()
			{
				Amount = pendingPaymentInfo.Amount,
				CategoryID = pendingPaymentInfo.CategoryID,
				CreatedByUserID = pendingPaymentInfo.CreatedByUserID,
				Currency = pendingPaymentInfo.Currency,
				Description = pendingPaymentInfo.Description,
				Duration = pendingPaymentInfo.Duration,
				Email = pendingPaymentInfo.Email,
				ReservationID = pendingPaymentInfo.ReservationID,
				PendingApprovalID = pendingPaymentInfo.PendingApprovalID,
				Phone = pendingPaymentInfo.Phone,
				PortalID = pendingPaymentInfo.PortalID,
				RefundableAmount = pendingPaymentInfo.RefundableAmount,
				StartDateTime = pendingPaymentInfo.StartDateTime,
				Status = pendingPaymentInfo.Status,
				TabModuleID = pendingPaymentInfo.TabModuleID,
				FirstName = pendingPaymentInfo.FirstName,
				LastName = pendingPaymentInfo.LastName
			};
		}

		public static string Decrypt(string text)
		{
			return RijndaelSimple.Decrypt(text, "E4356C15-835B-4210-9685-70EE00EF6FB8", "48505FCA-6999-48D2-B850-EC83CCD26691", "SHA1", 2, "AAB9B429CE674285", 256);
		}

		private void DeleteICalendarAttachmentFile(string path)
		{
			if (path != string.Empty)
			{
				try
				{
					File.Delete(path);
				}
				catch (Exception)
				{
				}
			}
		}

		public static RecurrencePattern DeserializeRecurrencePattern(string serializedRecurrencePattern)
		{
			RecurrencePattern recurrencePattern;
			StringReader stringReader = null;
			try
			{
				stringReader = new StringReader(serializedRecurrencePattern);
				recurrencePattern = (RecurrencePattern)(new XmlSerializer(typeof(RecurrencePattern))).Deserialize(new XmlTextReader(stringReader));
			}
			finally
			{
				if (stringReader != null)
				{
					stringReader.Close();
				}
			}
			return recurrencePattern;
		}

		public static List<SeasonalFeeScheduleInfo> DeserializeSeasonalFeeScheduleList(string serializedFeeScheduleList)
		{
			List<SeasonalFeeScheduleInfo> seasonalFeeScheduleInfos;
			StringReader stringReader = null;
			try
			{
				stringReader = new StringReader(serializedFeeScheduleList);
				seasonalFeeScheduleInfos = new List<SeasonalFeeScheduleInfo>((SeasonalFeeScheduleInfo[])(new XmlSerializer(typeof(SeasonalFeeScheduleInfo[]))).Deserialize(new XmlTextReader(stringReader)));
			}
			finally
			{
				if (stringReader != null)
				{
					stringReader.Close();
				}
			}
			return seasonalFeeScheduleInfos;
		}

		public static void DisableAJAX(Control control)
		{
			AJAX.AddScriptManager(control.Page);
			object control1 = AJAX.GetScriptManager(control.Page);
			Reflection.InvokeMethod(control1.GetType(), "RegisterPostBackControl", control1, new object[] { control });
			/*Control control1 = AJAX.ScriptManagerControl(control.Page);
			if (control1 != null)
			{
				Reflection.InvokeMethod(control1.GetType(), "RegisterPostBackControl", control1, new object[] { control });
			}*/
		}

		public static void DisplayModuleMessageIfAny(PortalModuleBase portalModuleBase)
		{
			string item = portalModuleBase.Request.QueryString["ModuleMessage"];
			if (!string.IsNullOrEmpty(item))
			{
				string str = Localization.GetString(item, portalModuleBase.LocalResourceFile);
				if (!string.IsNullOrEmpty(str))
				{
					item = str;
				}
				ModuleMessage.ModuleMessageType moduleMessageType = ModuleMessage.ModuleMessageType.RedError;
				if (!string.IsNullOrEmpty(portalModuleBase.Request.QueryString["ModuleMessageType"]))
				{
					try
					{
						moduleMessageType = (ModuleMessage.ModuleMessageType)Enum.Parse(typeof(ModuleMessage.ModuleMessageType), portalModuleBase.Request.QueryString["ModuleMessageType"], true);
					}
					catch (Exception)
					{
					}
				}
				Skin.AddModuleMessage(portalModuleBase, item, moduleMessageType);
			}
		}

		private string DomainMapper(Match match)
		{
			IdnMapping idnMapping = new IdnMapping();
			string value = match.Groups[2].Value;
			try
			{
				value = idnMapping.GetAscii(value);
			}
			catch (ArgumentException)
			{
				this.invalid = true;
			}
			return string.Concat(match.Groups[1].Value, value);
		}

		public static string Encrypt(string text)
		{
			return RijndaelSimple.Encrypt(text, "E4356C15-835B-4210-9685-70EE00EF6FB8", "48505FCA-6999-48D2-B850-EC83CCD26691", "SHA1", 2, "AAB9B429CE674285", 256);
		}

		private string Escape(string fileName)
		{
			char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
			for (int i = 0; i < (int)invalidFileNameChars.Length; i++)
			{
				char chr = invalidFileNameChars[i];
				if (fileName.Contains(chr.ToString()))
				{
					fileName = fileName.Replace(chr.ToString(), string.Empty);
				}
			}
			return fileName;
		}

		public static PendingPaymentInfo FindPendingPaymentInfoByEventIDAndStatus(List<PendingPaymentInfo> pendingPaymentInfoList, int eventID, int status)
		{
			PendingPaymentInfo pendingPaymentInfo;
			List<PendingPaymentInfo>.Enumerator enumerator = pendingPaymentInfoList.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					PendingPaymentInfo current = enumerator.Current;
					if (current.ReservationID != eventID || current.Status != status)
					{
						continue;
					}
					pendingPaymentInfo = current;
					return pendingPaymentInfo;
				}
				return null;
			}
			finally
			{
				((IDisposable)enumerator).Dispose();
			}
		}

		public static PendingPaymentInfo FindPendingPaymentInfoByPendingApprovalID(List<PendingPaymentInfo> pendingPaymentInfoList, int pendingApprovalID)
		{
			PendingPaymentInfo pendingPaymentInfo;
			List<PendingPaymentInfo>.Enumerator enumerator = pendingPaymentInfoList.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					PendingPaymentInfo current = enumerator.Current;
					if (current.PendingApprovalID != pendingApprovalID)
					{
						continue;
					}
					pendingPaymentInfo = current;
					return pendingPaymentInfo;
				}
				return null;
			}
			finally
			{
				((IDisposable)enumerator).Dispose();
			}
		}

		private UserInfo FindUserInfoByUserId(ArrayList users, int userId)
		{
			UserInfo userInfo;
			IEnumerator enumerator = users.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					UserInfo current = (UserInfo)enumerator.Current;
					if (current.UserID != userId)
					{
						continue;
					}
					userInfo = current;
					return userInfo;
				}
				return null;
			}
			finally
			{
				IDisposable disposable = enumerator as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
		}

		public string GenerateVerificationCode(string email)
		{
			return this.GetLettersOrDigitsOnly(Convert.ToBase64String((new SHA256Managed()).ComputeHash(Encoding.UTF8.GetBytes(string.Concat(email.ToLower(), this.ModuleSettings.VerificationCodeSalt)))), 7);
		}

		public string GetCategoryName(int categoryID)
		{
			string name;
			if (categoryID != Null.NullInteger && categoryID != 0)
			{
				List<CategoryInfo>.Enumerator enumerator = this.CategoryList.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						CategoryInfo current = enumerator.Current;
						if (current.CategoryID != categoryID)
						{
							continue;
						}
						name = current.Name;
						return name;
					}
					return this.NotAvailable;
				}
				finally
				{
					((IDisposable)enumerator).Dispose();
				}
			}
			return this.NotAvailable;
		}

		public static string GetEdition(string activationCode)
		{
			if (Helper.edition == null)
			{
				/*int num = 0;
				while (num < (int)Helper.publicKeys.Length)
				{
					if (!Helper.ValidateActivationCode(activationCode, Helper.publicKeys[num]))
					{
						num++;
					}
					else
					{
						Helper.edition = Helper.editions[num];
						break;
					}
				}*/
				try
				{
					Helper.edition = Helper.Decrypt(ComponentBase<IHostController, HostController>.Instance.GetString(ModuleSettings.EDITION_KEY));
				}
				catch
                {
                }
			}
			return Helper.edition;
		}

		public static string GetFingerprint()
		{
			string empty = string.Empty;
			return string.Concat(Host.GUID, (!string.IsNullOrEmpty(empty) ? string.Concat("-", empty) : string.Empty));
		}

		public static string GetFingerprintKey()
		{
			string key = string.Concat(Host.GUID.Replace("-", string.Empty), Host.GUID.Replace("-", string.Empty));
			while (key.Length < 32)
			{
				key += "0";
			}
			return key;
		}

		public static string GetFriendlyAmount(decimal amount, string currency)
		{
			OrderedDictionary orderedDictionaries = new OrderedDictionary();
			((IDictionary)orderedDictionaries).Add("CAD", "$");
			((IDictionary)orderedDictionaries).Add("EUR", "€");
			((IDictionary)orderedDictionaries).Add("GBP", "£");
			((IDictionary)orderedDictionaries).Add("USD", "$");
			((IDictionary)orderedDictionaries).Add("YEN", "¥");
			string str = string.Concat((string)((IDictionary)orderedDictionaries)[currency], string.Format("{0:F}", amount));
			if (str.EndsWith(".00") || str.EndsWith(",00"))
			{
				str = str.Substring(0, str.Length - 3);
			}
			str = string.Concat(str, " ", currency);
			return str;
		}

		public DateTime GetFromDate(string range)
		{
			return this.TryParseDate(range.Split(this.DateRangeSeparator.ToCharArray())[0].Trim());
		}

		private ICalendarInfo GetICalendarInfo(ReservationInfo eventInfo)
		{
			ICalendarInfo calendar = (new ICalendarController()).GetICalendar(eventInfo.ReservationID);
			if (calendar != null)
			{
				ICalendarInfo sequence = calendar;
				sequence.Sequence = sequence.Sequence + 1;
			}
			else
			{
				calendar = new ICalendarInfo()
				{
					ReservationID = eventInfo.ReservationID,
					UID = Guid.NewGuid().ToString(),
					Sequence = 0,
					Organizer = this.ModuleSettings.MailFrom
				};
			}
			return calendar;
		}

		public static string GetjQueryVersionFromUrl(string url)
		{
			string empty;
			try
			{
				string[] strArrays = url.Split("/\\".ToCharArray());
				empty = strArrays[(int)strArrays.Length - 2];
			}
			catch (Exception)
			{
				empty = string.Empty;
			}
			return empty;
		}

		public string GetLettersOrDigitsOnly(string str, int length)
		{
			string empty = string.Empty;
			for (int i = 0; i < str.Length && empty.Length < length; i++)
			{
				if (char.IsLetterOrDigit(str[i]))
				{
					char chr = str[i];
					empty = string.Concat(empty, chr.ToString());
				}
			}
			return empty;
		}

		public static string GetMonthAndDay(int month, int day)
		{
			return (new DateTime(2000, month, day)).ToString("m");
		}

		public static string GetMonthName(int number)
		{
			return (new DateTime(2000, number, 1)).ToString("MMMM");
		}

		public static DateTime GetNow(TimeZoneInfo timeZone)
		{
			DateTime utcNow = DateTime.UtcNow;
			return utcNow.Add(timeZone.GetUtcOffset(DateTime.Now));
		}

		public static string GetRecurrencePatternText(IRecurrencePattern recurrencePattern)
		{
			int? every;
			int hours;
			DayPosition? dayPosition;
			DayType? dayType;
			string str;
			string str1;
			string empty;
			string str2;
			string str3 = string.Concat(new string[] { Globals.ApplicationPath, "/DesktopModules/", DesktopModuleController.GetDesktopModuleByModuleName("Gafware.Modules.Reservations", PortalSettings.Current.PortalId).FolderName, "/", Localization.LocalResourceDirectory, "/RecurrencePatternText" });
			StringBuilder stringBuilder = new StringBuilder();
			if (recurrencePattern.Pattern == Pattern.Daily)
			{
				every = recurrencePattern.Every;
				if (!every.HasValue)
				{
					stringBuilder.Append(Localization.GetString("Every weekday", str3));
				}
				else
				{
					StringBuilder stringBuilder1 = stringBuilder;
					string str4 = Localization.GetString("Every", str3);
					every = recurrencePattern.Every;
					if (every.Value == 1)
					{
						str2 = Localization.GetString("day", str3);
					}
					else
					{
						every = recurrencePattern.Every;
						str2 = string.Concat(every.ToString(), " ", Localization.GetString("days", str3));
					}
					stringBuilder1.Append(string.Concat(str4, " ", str2));
				}
			}
			else if (recurrencePattern.Pattern == Pattern.Weekly)
			{
				StringBuilder stringBuilder2 = stringBuilder;
				string str5 = Localization.GetString("Every", str3);
				every = recurrencePattern.Every;
				if (every.Value == 1)
				{
					empty = string.Empty;
				}
				else
				{
					every = recurrencePattern.Every;
					empty = string.Concat(every.ToString(), " ", Localization.GetString("weeks on", str3), " ");
				}
				stringBuilder2.Append(string.Concat(str5, " ", empty));
				ArrayList arrayLists = new ArrayList();
				if (recurrencePattern.Sunday)
				{
					arrayLists.Add(Localization.GetString("Sunday", str3));
				}
				if (recurrencePattern.Monday)
				{
					arrayLists.Add(Localization.GetString("Monday", str3));
				}
				if (recurrencePattern.Tuesday)
				{
					arrayLists.Add(Localization.GetString("Tuesday", str3));
				}
				if (recurrencePattern.Wednesday)
				{
					arrayLists.Add(Localization.GetString("Wednesday", str3));
				}
				if (recurrencePattern.Thursday)
				{
					arrayLists.Add(Localization.GetString("Thursday", str3));
				}
				if (recurrencePattern.Friday)
				{
					arrayLists.Add(Localization.GetString("Friday", str3));
				}
				if (recurrencePattern.Saturday)
				{
					arrayLists.Add(Localization.GetString("Saturday", str3));
				}
				int num = 1;
				foreach (string arrayList in arrayLists)
				{
					if (num != 1)
					{
						if (num >= arrayLists.Count)
						{
							stringBuilder.Append(Localization.GetString(", and ", str3));
						}
						else
						{
							stringBuilder.Append(Localization.GetString(", ", str3));
						}
					}
					stringBuilder.Append(arrayList);
					num++;
				}
			}
			else if (recurrencePattern.Pattern == Pattern.Monthly)
			{
				every = recurrencePattern.Day;
				if (!every.HasValue)
				{
					StringBuilder stringBuilder3 = stringBuilder;
					string[] lower = new string[] { Localization.GetString("The", str3), " ", null, null, null, null, null, null, null };
					dayPosition = recurrencePattern.DayPosition;
					lower[2] = dayPosition.ToString().ToLower();
					lower[3] = " ";
					dayType = recurrencePattern.DayType;
					lower[4] = dayType.ToString();
					lower[5] = " ";
					lower[6] = Localization.GetString("of every", str3);
					lower[7] = " ";
					every = recurrencePattern.Every;
					if ((every.GetValueOrDefault() == 1 ? every.HasValue : false))
					{
						str = Localization.GetString("month", str3);
					}
					else
					{
						every = recurrencePattern.Every;
						str = string.Concat(every.ToString(), " ", Localization.GetString("months", str3));
					}
					lower[8] = str;
					stringBuilder3.Append(string.Concat(lower));
				}
				else
				{
					StringBuilder stringBuilder4 = stringBuilder;
					string[] strArrays = new string[] { Localization.GetString("The day", str3), " ", null, null, null, null, null };
					every = recurrencePattern.Day;
					strArrays[2] = every.ToString();
					strArrays[3] = " ";
					strArrays[4] = Localization.GetString("of every", str3);
					strArrays[5] = " ";
					every = recurrencePattern.Every;
					if ((every.GetValueOrDefault() == 1 ? every.HasValue : false))
					{
						str1 = Localization.GetString("month", str3);
					}
					else
					{
						every = recurrencePattern.Every;
						str1 = string.Concat(every.ToString(), " ", Localization.GetString("months", str3));
					}
					strArrays[6] = str1;
					stringBuilder4.Append(string.Concat(strArrays));
				}
			}
			else if (recurrencePattern.Pattern == Pattern.Yearly)
			{
				every = recurrencePattern.Day;
				if (!every.HasValue)
				{
					string[] monthName = new string[] { Localization.GetString("The", str3), " ", null, null, null, null, null, null, null };
					dayPosition = recurrencePattern.DayPosition;
					monthName[2] = dayPosition.ToString().ToLower();
					monthName[3] = " ";
					dayType = recurrencePattern.DayType;
					monthName[4] = dayType.ToString();
					monthName[5] = " ";
					monthName[6] = Localization.GetString("of", str3);
					monthName[7] = " ";
					every = recurrencePattern.Month;
					monthName[8] = Helper.GetMonthName(every.Value);
					stringBuilder.Append(string.Concat(monthName));
				}
				else
				{
					string str6 = Localization.GetString("Every", str3);
					every = recurrencePattern.Month;
					int value = every.Value;
					every = recurrencePattern.Day;
					stringBuilder.Append(string.Concat(str6, " ", Helper.GetMonthAndDay(value, every.Value)));
				}
			}
			string str7 = Localization.GetString("effective", str3);
			DateTime startDate = recurrencePattern.StartDate;
			stringBuilder.Append(string.Concat(" ", str7, " ", startDate.ToShortDateString()));
			if (!recurrencePattern.EndDate.HasValue)
			{
				every = recurrencePattern.EndAfter;
				if (every.HasValue)
				{
					object[] objArray = new object[] { " ", Localization.GetString("ending after", str3), " ", null, null, null };
					every = recurrencePattern.EndAfter;
					objArray[3] = every.Value;
					objArray[4] = " ";
					objArray[5] = Localization.GetString("occurrences", str3);
					stringBuilder.Append(string.Concat(objArray));
				}
			}
			else
			{
				string str8 = Localization.GetString("until", str3);
				startDate = recurrencePattern.EndDate.Value;
				stringBuilder.Append(string.Concat(" ", str8, " ", startDate.ToShortDateString()));
			}
			if (recurrencePattern.IsAllDay)
			{
				stringBuilder.Append(string.Concat(" ", Localization.GetString("all day", str3)));
			}
			else
			{
				stringBuilder.Append(string.Concat(" ", Localization.GetString("from", str3), " "));
				startDate = recurrencePattern.StartDate;
				startDate = startDate.Add(recurrencePattern.StartTime);
				stringBuilder.Append(startDate.ToShortTimeString());
				if (recurrencePattern.Duration.TotalDays >= 1)
				{
					stringBuilder.Append(string.Concat(" ", Localization.GetString("for", str3), " "));
					stringBuilder.Append(recurrencePattern.Duration.Days);
					stringBuilder.Append(string.Concat(" ", Localization.GetString("day(s)", str3)));
					if (recurrencePattern.Duration.Hours > 0)
					{
						stringBuilder.Append(Localization.GetString(", ", str3));
						hours = recurrencePattern.Duration.Hours;
						stringBuilder.Append(hours.ToString());
						stringBuilder.Append(string.Concat(" ", Localization.GetString("hour(s)", str3)));
					}
					if (recurrencePattern.Duration.Minutes > 0)
					{
						stringBuilder.Append(Localization.GetString(", ", str3));
						hours = recurrencePattern.Duration.Minutes;
						stringBuilder.Append(hours.ToString());
						stringBuilder.Append(string.Concat(" ", Localization.GetString("minute(s)", str3)));
					}
				}
				else
				{
					stringBuilder.Append(string.Concat(" ", Localization.GetString("to", str3), " "));
					startDate = recurrencePattern.StartDate;
					startDate = startDate.Add(recurrencePattern.StartTime);
					startDate = startDate.Add(recurrencePattern.Duration);
					stringBuilder.Append(startDate.ToShortTimeString());
				}
			}
			stringBuilder.Append(".");
			return stringBuilder.ToString();
		}

		public int GetTabID(int portalID, int tabModuleID)
		{
			int tabID;
			IEnumerator enumerator = (new ModuleController()).GetModulesByDefinition(portalID, "Reservations").GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					DotNetNuke.Entities.Modules.ModuleInfo current = (DotNetNuke.Entities.Modules.ModuleInfo)enumerator.Current;
					if (current.TabModuleID != tabModuleID)
					{
						continue;
					}
					tabID = current.TabID;
					return tabID;
				}
				return Null.NullInteger;
			}
			finally
			{
				IDisposable disposable = enumerator as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
		}

		public DateTime GetToDate(string range)
		{
			if (!range.Contains(this.DateRangeSeparator))
			{
				return new DateTime();
			}
			return this.TryParseDate(range.Split(this.DateRangeSeparator.ToCharArray())[1].Trim());
		}

		public bool IsCashier(int userID)
		{
			bool flag = false;
			if (!this.ModuleSettings.AllowCategorySelection)
			{
				flag = this.FindUserInfoByUserId(this.ModuleSettings.CashierList, userID) != null;
			}
			else
			{
				foreach (CategoryInfo categoryList in this.CategoryList)
				{
					flag = this.CanProcessPayment(userID, categoryList.CategoryID);
					if (!flag)
					{
						continue;
					}
					return flag;
				}
			}
			return flag;
		}

		public bool IsModerator(int userID)
		{
			bool flag = false;
			if (!this.ModuleSettings.AllowCategorySelection)
			{
				flag = (!this.ModuleSettings.Moderate ? false : this.FindUserInfoByUserId(this.ModuleSettings.ModeratorList, userID) != null);
			}
			else
			{
				foreach (CategoryInfo categoryList in this.CategoryList)
				{
					flag = this.CanModerate(userID, categoryList.CategoryID);
					if (!flag)
					{
						continue;
					}
					return flag;
				}
			}
			return flag;
		}

		public bool IsValidEmail(string email)
		{
			this.invalid = false;
			if (string.IsNullOrEmpty(email))
			{
				return false;
			}
			email = Regex.Replace(email, "(@)(.+)$", new MatchEvaluator(this.DomainMapper));
			if (this.invalid)
			{
				return false;
			}
			return Regex.IsMatch(email, "^(?(\")(\"[^\"]+?\"@)|(([0-9a-z]((\\.(?!\\.))|[-!#\\$%&'\\*\\+/=\\?\\^`\\{\\}\\|~\\w])*)(?<=[0-9a-z])@))(?(\\[)(\\[(\\d{1,3}\\.){3}\\d{1,3}\\])|(([0-9a-z][-\\w]*[0-9a-z]*\\.)+[a-z0-9]{2,17}))$", RegexOptions.IgnoreCase);
		}

		public static bool IsValidEmail2(string email)
		{
			if (string.IsNullOrEmpty(email))
			{
				return false;
			}
			return Regex.IsMatch(email, "^(?(\")(\"[^\"]+?\"@)|(([0-9a-z]((\\.(?!\\.))|[-!#\\$%&'\\*\\+/=\\?\\^`\\{\\}\\|~\\w])*)(?<=[0-9a-z])@))(?(\\[)(\\[(\\d{1,3}\\.){3}\\d{1,3}\\])|(([0-9a-z][-\\w]*[0-9a-z]*\\.)+[a-z0-9]{2,17}))$", RegexOptions.IgnoreCase);
		}

		public static List<LocalizedEnum> LocalizeEnum(Type type, string localResourceFile)
		{
			List<LocalizedEnum> localizedEnums = new List<LocalizedEnum>();
			string[] names = Enum.GetNames(type);
			for (int i = 0; i < (int)names.Length; i++)
			{
				string str = names[i];
				localizedEnums.Add(new LocalizedEnum(Localization.GetString(str, localResourceFile), str, (int)Enum.Parse(type, str)));
			}
			return localizedEnums;
		}

		public static List<LocalizedEnum> LocalizeEnumSorted(Type type, string localResourceFile)
		{
			List<LocalizedEnum> localizedEnums = new List<LocalizedEnum>();
			string[] names = Enum.GetNames(type);
			for (int i = 0; i < (int)names.Length; i++)
			{
				string str = names[i];
				localizedEnums.Add(new LocalizedEnum(Localization.GetString(str, localResourceFile), str));
			}
			localizedEnums.Sort();
			return localizedEnums;
		}

		public void ModifyPendingApprovalStatus(PendingApprovalInfo pendingApprovalInfo, PendingApprovalStatus status, UserInfo moderatorUserInfo)
		{
			PendingPaymentInfo pendingPaymentInfo;
			bool flag;
			bool reservationID = pendingApprovalInfo.ReservationID == Null.NullInteger;
			List<PendingPaymentInfo> pendingPaymentList = null;
			PendingPaymentInfo reservationID1 = null;
			if (this.IsProfessional)
			{
				pendingPaymentList = (new PendingPaymentController()).GetPendingPaymentList(this.TabModuleId);
				reservationID1 = Helper.FindPendingPaymentInfoByPendingApprovalID(pendingPaymentList, pendingApprovalInfo.PendingApprovalID);
			}
			if (status != PendingApprovalStatus.Approved)
			{
				if (reservationID1 != null)
				{
					if (reservationID1.Status == 1)
					{
						if (reservationID)
						{
							pendingPaymentInfo = null;
						}
						else
						{
							pendingPaymentInfo = Helper.FindPendingPaymentInfoByEventIDAndStatus(pendingPaymentList, reservationID1.ReservationID, 5);
						}
						PendingPaymentInfo now = pendingPaymentInfo;
						if (now != null)
						{
							PendingPaymentInfo amount = now;
							amount.Amount = amount.Amount + (reservationID1.Amount * decimal.MinusOne);
							PendingPaymentInfo refundableAmount = now;
							refundableAmount.RefundableAmount = refundableAmount.RefundableAmount + (reservationID1.RefundableAmount * decimal.MinusOne);
							now.LastModifiedOnDate = DateTime.Now;
							(new PendingPaymentController()).UpdatePendingPayment(now);
						}
						else
						{
							now = (new PendingPaymentController()).AddPendingPayment(Helper.CreatePendingPaymentInfoFromPendingApprovalInfo(pendingApprovalInfo, reservationID1.Amount * decimal.MinusOne, reservationID1.RefundableAmount * decimal.MinusOne, this.ModuleSettings.Currency, PendingPaymentStatus.PendingRefund));
						}
						this.SendPendingDeclinationRefundMail(pendingApprovalInfo, reservationID1.Amount, reservationID1.Currency);
					}
					else if (reservationID1.Status == 8)
					{
						(new PendingPaymentController()).DeletePendingPayment(reservationID1.PendingPaymentID);
					}
				}
				this.SendDeclinedMail(pendingApprovalInfo);
			}
			else
			{
				ReservationInfo categoryID = (reservationID ? new ReservationInfo() : this.ReservationController.GetReservation(pendingApprovalInfo.ReservationID));
				categoryID.CategoryID = pendingApprovalInfo.CategoryID;
				categoryID.StartDateTime = pendingApprovalInfo.StartDateTime;
				categoryID.Duration = pendingApprovalInfo.Duration;
				categoryID.Description = pendingApprovalInfo.Description;
				ReservationInfo reservationInfo = categoryID;
				if (!this.ModuleSettings.SendReminders)
				{
					flag = false;
				}
				else
				{
					DateTime dateTime = Helper.GetNow(this.ModuleSettings.TimeZone);
					DateTime startDateTime = categoryID.StartDateTime;
					flag = dateTime < startDateTime.Subtract(this.ModuleSettings.SendRemindersWhen);
				}
				reservationInfo.SendReminder = flag;
				TimeSpan sendRemindersWhen = this.ModuleSettings.SendRemindersWhen;
				categoryID.SendReminderWhen = (int)sendRemindersWhen.TotalMinutes;
				if (categoryID.SendReminder)
				{
					categoryID.SendReminderVia = this.ModuleSettings.SendRemindersVia;
				}
				categoryID.RequireConfirmation = (reservationID1 != null || !this.ModuleSettings.RequireConfirmation ? false : categoryID.SendReminder);
				sendRemindersWhen = this.ModuleSettings.RequireConfirmationWhen;
				categoryID.RequireConfirmationWhen = (int)sendRemindersWhen.TotalMinutes;
				if (!reservationID)
				{
					categoryID.LastModifiedByUserID = (pendingApprovalInfo.CreatedByUserID == Null.NullInteger ? this.PortalInfo.AdministratorId : pendingApprovalInfo.CreatedByUserID);
					categoryID.LastModifiedOnDate = pendingApprovalInfo.CreatedOnDate;
				}
				else
				{
					categoryID.TabModuleID = pendingApprovalInfo.TabModuleID;
					categoryID.FirstName = pendingApprovalInfo.FirstName;
					categoryID.LastName = pendingApprovalInfo.LastName;
					categoryID.CreatedByUserID = (pendingApprovalInfo.CreatedByUserID == Null.NullInteger ? this.PortalInfo.AdministratorId : pendingApprovalInfo.CreatedByUserID);
					categoryID.CreatedOnDate = pendingApprovalInfo.CreatedOnDate;
					categoryID.Email = pendingApprovalInfo.Email;
					categoryID.Phone = pendingApprovalInfo.Phone;
				}
				categoryID = this.ReservationController.SaveReservation(categoryID);
				if (reservationID && this.IsProfessional)
				{
					CustomFieldValueController customFieldValueController = new CustomFieldValueController();
					foreach (CustomFieldValueInfo customFieldValueListByPendingApprovalID in (new CustomFieldValueController()).GetCustomFieldValueListByPendingApprovalID(pendingApprovalInfo.PendingApprovalID))
					{
						customFieldValueListByPendingApprovalID.ReservationID = categoryID.ReservationID;
						customFieldValueListByPendingApprovalID.LastModifiedByUserID = moderatorUserInfo.UserID;
						customFieldValueListByPendingApprovalID.LastModifiedOnDate = DateTime.Now;
						customFieldValueController.UpdateCustomFieldValue(customFieldValueListByPendingApprovalID);
					}
				}
				if (!reservationID)
				{
					Helper.UpdateDueAndPendingRefundPaymentInfoFromEventInfo(categoryID, this.TabModuleId);
				}
				pendingApprovalInfo.ReservationID = categoryID.ReservationID;
				if (reservationID1 != null && reservationID1.Status == 8)
				{
					PendingPaymentInfo now1 = Helper.FindPendingPaymentInfoByEventIDAndStatus(pendingPaymentList, categoryID.ReservationID, (reservationID1.Amount < decimal.Zero ? 5 : 7));
					if (now1 == null)
					{
						now1 = (new PendingPaymentController()).AddPendingPayment(Helper.CreatePendingPaymentInfoFromPendingApprovalInfo(pendingApprovalInfo, reservationID1.Amount, reservationID1.RefundableAmount, this.ModuleSettings.Currency, (reservationID1.Amount < decimal.Zero ? PendingPaymentStatus.PendingRefund : PendingPaymentStatus.Due)));
					}
					else
					{
						PendingPaymentInfo amount1 = now1;
						amount1.Amount = amount1.Amount + reservationID1.Amount;
						PendingPaymentInfo refundableAmount1 = now1;
						refundableAmount1.RefundableAmount = refundableAmount1.RefundableAmount + reservationID1.RefundableAmount;
						now1.LastModifiedOnDate = DateTime.Now;
						(new PendingPaymentController()).UpdatePendingPayment(now1);
					}
					if (reservationID1.Amount < decimal.Zero)
					{
						this.SendPendingRescheduleRefundMail(categoryID, reservationID1.Amount * decimal.MinusOne, this.ModuleSettings.Currency);
					}
					(new PendingPaymentController()).DeletePendingPayment(reservationID1.PendingPaymentID);
					reservationID1 = null;
				}
				if (!reservationID)
				{
					this.SendRescheduledMail(categoryID);
				}
				else
				{
					this.SendConfirmationMail(categoryID);
				}
			}
			pendingApprovalInfo.Status = (int)status;
			pendingApprovalInfo.LastModifiedByUserID = moderatorUserInfo.UserID;
			pendingApprovalInfo.LastModifiedOnDate = DateTime.Now;
			(new PendingApprovalController()).UpdatePendingApproval(pendingApprovalInfo);
			if (reservationID1 != null && reservationID1.ReservationID == Null.NullInteger && pendingApprovalInfo.ReservationID != Null.NullInteger)
			{
				reservationID1.ReservationID = pendingApprovalInfo.ReservationID;
				(new PendingPaymentController()).UpdatePendingPayment(reservationID1);
			}
		}

		public void ModifyPendingPaymentStatus(PendingPaymentInfo pendingPaymentInfo, PendingPaymentStatus status)
		{
			pendingPaymentInfo.Status = (int)status;
			pendingPaymentInfo.LastModifiedOnDate = DateTime.Now;
			(new PendingPaymentController()).UpdatePendingPayment(pendingPaymentInfo);
		}

		public bool MustModerate(Hashtable categorySettings, Hashtable moduleSettings, DateTime startDateTime, int duration)
		{
			bool flag;
			Hashtable hashtables = moduleSettings;
			if (categorySettings != null && categorySettings.ContainsKey(string.Concat("Moderation.", DayOfWeek.Monday.ToString())))
			{
				hashtables = categorySettings;
			}
			ArrayList arrayLists = new ArrayList();
			foreach (DayOfWeek value in Enum.GetValues(typeof(DayOfWeek)))
			{
				if (!hashtables.ContainsKey(string.Concat("Moderation.", value.ToString())) || !((string)hashtables[string.Concat("Moderation.", value.ToString())] != string.Empty))
				{
					continue;
				}
				string[] strArrays = ((string)hashtables[string.Concat("Moderation.", value.ToString())]).Split(new char[] { ';' });
				for (int i = 0; i < (int)strArrays.Length; i++)
				{
					string str = strArrays[i];
					WorkingHoursInfo workingHoursInfo = new WorkingHoursInfo()
					{
						DayOfWeek = value
					};
					if (str.ToLower() != "all day")
					{
						workingHoursInfo.StartTime = TimeSpan.Parse(str.Split(new char[] { '-' })[0]);
						workingHoursInfo.EndTime = TimeSpan.Parse(str.Split(new char[] { '-' })[1]);
					}
					else
					{
						workingHoursInfo.AllDay = true;
					}
					arrayLists.Add(workingHoursInfo);
				}
			}
			if (arrayLists.Count == 0)
			{
				return true;
			}
			IEnumerator enumerator = arrayLists.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					WorkingHoursInfo current = (WorkingHoursInfo)enumerator.Current;
					if (current.DayOfWeek != startDateTime.DayOfWeek || !current.AllDay && !this.Conflicts(startDateTime, duration, startDateTime.Date.Add(current.StartTime), (int)current.EndTime.Subtract(current.StartTime).TotalMinutes))
					{
						continue;
					}
					flag = true;
					return flag;
				}
				return false;
			}
			finally
			{
				IDisposable disposable = enumerator as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
		}

		public string ReplaceCustomFieldToken(List<CustomFieldValueInfo> list, string text)
		{
			string str;
			string empty = string.Empty;
			foreach (CustomFieldValueInfo customFieldValueInfo in list)
			{
				string[] label = new string[] { empty, null, null, null, null };
				label[1] = (empty == string.Empty ? string.Empty : Environment.NewLine);
				label[2] = customFieldValueInfo.Label;
				label[3] = new string('.', 12 - customFieldValueInfo.Label.Length % 12);
				if (customFieldValueInfo.CustomFieldDefinitionInfo.CustomFieldDefinitionType == CustomFieldDefinitionType.CheckBox)
				{
					str = Localization.GetString(customFieldValueInfo.Value, this.LocalResourceFile);
				}
				else
				{
					str = (customFieldValueInfo.Value == string.Empty ? this.NotAvailable : customFieldValueInfo.Value);
				}
				label[4] = str;
				empty = string.Concat(label);
			}
			empty = string.Concat(empty, (empty == string.Empty ? string.Empty : Environment.NewLine));
			return text.Replace("{CustomFields}", empty);
		}

		private string ReplaceDateTokens(string dateToken, string text, DateTime dateTime)
		{
			for (int i = text.IndexOf(string.Concat("{", dateToken, "|")); i != -1; i = text.IndexOf(string.Concat("{", dateToken, "|"), i + 1))
			{
				string str = text.Substring(i + string.Concat("{", dateToken, "|").Length, text.IndexOf("}", i) - i - string.Concat("{", dateToken, "|").Length);
				try
				{
					text = text.Replace(string.Concat(new string[] { "{", dateToken, "|", str, "}" }), dateTime.ToString(str));
				}
				catch (Exception)
				{
				}
			}
			return text.Replace(string.Concat("{", dateToken, "}"), dateTime.ToString());
		}

		public string ReplaceTokens(string text, PendingPaymentInfo pendingPaymentInfo, string feeType)
		{
			ReservationInfo reservationInfo = new ReservationInfo()
			{
				CategoryID = pendingPaymentInfo.CategoryID,
				Description = pendingPaymentInfo.Description,
				Duration = pendingPaymentInfo.Duration,
				Email = pendingPaymentInfo.Email,
				Phone = pendingPaymentInfo.Phone,
				StartDateTime = pendingPaymentInfo.StartDateTime,
				FirstName = pendingPaymentInfo.FirstName,
				LastName = pendingPaymentInfo.LastName
			};
			return this.ReplaceTokens(reservationInfo, text.Replace("{FeeType}", feeType), reservationInfo.FullName, true);
		}

		public string ReplaceTokens(ReservationInfo reservationInfo, string text, string recipientDisplayName, bool includeVerificationCodeSection = true)
		{
			List<CustomFieldValueInfo> customFieldValueListByReservationID = (new CustomFieldValueController()).GetCustomFieldValueListByReservationID(reservationInfo.ReservationID);
			customFieldValueListByReservationID.Sort((CustomFieldValueInfo x, CustomFieldValueInfo y) => x.SortOrder.CompareTo(y.SortOrder));
			UserInfo user = (new UserController()).GetUser(this.PortalInfo.PortalID, reservationInfo.CreatedByUserID);
			List<CustomFieldValueInfo> customFieldValueInfos = customFieldValueListByReservationID;
			ReservationInfo reservationInfo1 = reservationInfo;
			string str = this.ReplaceDateTokens("StartDateTime", text, reservationInfo.StartDateTime);
			DateTime startDateTime = reservationInfo.StartDateTime;
			string str1 = this.ReplaceDateTokens("EndDateTime", str, startDateTime.AddMinutes((double)reservationInfo.Duration)).Replace("{TimeZone}", this.ModuleSettings.TimeZone.DisplayName).Replace("{Recipient}", recipientDisplayName);
			int duration = reservationInfo.Duration / 60;
			string str2 = str1.Replace("{DurationHours}", duration.ToString());
			duration = reservationInfo.Duration % 60;
			string str3 = str2.Replace("{DurationMinutes}", duration.ToString());
			duration = reservationInfo.Duration / 60;
			string str4 = str3.Replace("{DurationTotalHours}", duration.ToString());
			duration = reservationInfo.Duration;
			return this.ReplaceUserTokens(user, this.ReplaceCustomFieldToken(customFieldValueInfos, this.ReplaceVerificationCodeTokens(reservationInfo1, str4.Replace("{DurationTotalMinutes}", duration.ToString()).Replace("{Category}", this.GetCategoryName(reservationInfo.CategoryID)).Replace("{Name}", reservationInfo.FullName).Replace("{Email}", reservationInfo.Email).Replace("{Phone}", (reservationInfo.Phone != string.Empty ? reservationInfo.Phone : this.NotAvailable)).Replace("{Description}", (reservationInfo.Description != string.Empty ? reservationInfo.Description : this.NotAvailable)), includeVerificationCodeSection)));
		}

		public string ReplaceTokens(PendingApprovalInfo pendingApprovalInfo, string text, string recipientDisplayName)
		{
			int tabID = this.GetTabID(pendingApprovalInfo.PortalID, pendingApprovalInfo.TabModuleID);
			string empty = string.Empty;
			string[] strArrays = new string[1];
			int pendingApprovalID = pendingApprovalInfo.PendingApprovalID;
			strArrays[0] = string.Concat("PendingApprovalID=", pendingApprovalID.ToString());
#pragma warning disable CS0618 // Type or member is obsolete
            string str = Globals.NavigateURL(tabID, empty, strArrays);
#pragma warning restore CS0618 // Type or member is obsolete
            List<CustomFieldValueInfo> customFieldValueListByPendingApprovalID = (new CustomFieldValueController()).GetCustomFieldValueListByPendingApprovalID(pendingApprovalInfo.PendingApprovalID);
			customFieldValueListByPendingApprovalID.Sort((CustomFieldValueInfo x, CustomFieldValueInfo y) => x.SortOrder.CompareTo(y.SortOrder));
			UserInfo createdByUserInfo = pendingApprovalInfo.CreatedByUserInfo;
			List<CustomFieldValueInfo> customFieldValueInfos = customFieldValueListByPendingApprovalID;
			string str1 = this.ReplaceDateTokens("StartDateTime", text, pendingApprovalInfo.StartDateTime);
			DateTime startDateTime = pendingApprovalInfo.StartDateTime;
			string str2 = this.ReplaceDateTokens("EndDateTime", str1, startDateTime.AddMinutes((double)pendingApprovalInfo.Duration)).Replace("{TimeZone}", this.ModuleSettings.TimeZone.DisplayName).Replace("{Url}", str).Replace("{Recipient}", recipientDisplayName);
			pendingApprovalID = pendingApprovalInfo.Duration / 60;
			string str3 = str2.Replace("{DurationHours}", pendingApprovalID.ToString());
			pendingApprovalID = pendingApprovalInfo.Duration % 60;
			string str4 = str3.Replace("{DurationMinutes}", pendingApprovalID.ToString());
			pendingApprovalID = pendingApprovalInfo.Duration / 60;
			string str5 = str4.Replace("{DurationTotalHours}", pendingApprovalID.ToString());
			pendingApprovalID = pendingApprovalInfo.Duration;
			return this.ReplaceUserTokens(createdByUserInfo, this.ReplaceCustomFieldToken(customFieldValueInfos, str5.Replace("{DurationTotalMinutes}", pendingApprovalID.ToString()).Replace("{Category}", this.GetCategoryName(pendingApprovalInfo.CategoryID)).Replace("{Name}", pendingApprovalInfo.FullName).Replace("{Email}", pendingApprovalInfo.Email).Replace("{Phone}", (pendingApprovalInfo.Phone != string.Empty ? pendingApprovalInfo.Phone : this.NotAvailable)).Replace("{Description}", (pendingApprovalInfo.Description != string.Empty ? pendingApprovalInfo.Description : this.NotAvailable))));
		}

		public string ReplaceTokens(string firstName, string lastName, string email, string phone, int category, string reason, string text, string recipientDisplayName)
		{
			return text.Replace("{Recipient}", recipientDisplayName).Replace("{Category}", this.GetCategoryName(category)).Replace("{Name}", string.Concat(firstName, " ", lastName)).Replace("{Email}", email).Replace("{Phone}", (phone != string.Empty ? phone : this.NotAvailable)).Replace("{Reason}", (reason != string.Empty ? HttpContext.Current.Server.HtmlDecode(reason) : this.NotAvailable));
		}

		public string ReplaceUserTokens(UserInfo userInfo, string text)
		{
			if (userInfo != null)
			{
				PropertyInfo[] properties = userInfo.Profile.GetType().GetProperties();
				for (int i = 0; i < (int)properties.Length; i++)
				{
					PropertyInfo propertyInfo = properties[i];
					try
					{
						object obj = (userInfo == null ? null : propertyInfo.GetValue(userInfo.Profile, new object[0]));
						text = text.Replace(string.Concat("{UserInfo.Profile.", propertyInfo.Name, "}"), (obj == null ? this.NotAvailable : obj.ToString()));
					}
					catch (Exception)
					{
					}
				}
			}
			return text;
		}

		public string ReplaceVerificationCodeTokens(ReservationInfo eventInfo, string text, bool includeVerificationCodeSection)
		{
			string empty;
			int num = text.IndexOf("{if:RequireVerificationCode}");
			if (num != -1)
			{
				int num1 = text.IndexOf("{/if:RequireVerificationCode}");
				if (!this.ModuleSettings.RequireVerificationCode || !includeVerificationCodeSection)
				{
					string str = text.Substring(0, num);
					if (num1 != -1)
					{
						int length = num1 + "{/if:RequireVerificationCode}".Length;
						num1 = length;
						if (length == text.Length)
						{
							goto Label1;
						}
						empty = text.Substring(num1);
						goto Label0;
					}
				Label1:
					empty = string.Empty;
				Label0:
					text = string.Concat(str, empty);
				}
				else
				{
					text = text.Replace("{if:RequireVerificationCode}", string.Empty).Replace("{/if:RequireVerificationCode}", string.Empty);
				}
			}
			return text.Replace("{VerificationCode}", this.GenerateVerificationCode(eventInfo.Email));
		}

		public void SendCancellationMail(ReservationInfo eventInfo)
		{
			try
			{
				string str = this.ReplaceTokens(eventInfo, this.ModuleSettings.CancellationMailSubject, eventInfo.FullName, true);
				string str1 = this.ReplaceTokens(eventInfo, this.ModuleSettings.CancellationMailBody, eventInfo.FullName, true);
				string empty = string.Empty;
				ICalendarInfo calendarInfo = null;
				if (this.ModuleSettings.AttachICalendar)
				{
					calendarInfo = this.GetICalendarInfo(eventInfo);
					empty = this.CreateICalendarAttachmentFile(eventInfo, "CANCEL", this.ReplaceTokens(eventInfo, this.ModuleSettings.ICalendarAttachmentFileName, eventInfo.FullName, true), calendarInfo);
				}
				if (!string.IsNullOrEmpty(eventInfo.Email))
				{
					Mail.SendMail(this.ModuleSettings.MailFrom, eventInfo.Email, string.Empty, str, str1, empty, this.ModuleSettings.CancellationMailBodyType, string.Empty, string.Empty, string.Empty, string.Empty);
				}
				this.DeleteICalendarAttachmentFile(empty);
				ArrayList bCCList = this.ModuleSettings.BCCList;
				if (eventInfo.CategoryID != Null.NullInteger && eventInfo.CategoryID != 0)
				{
					bCCList = (new CategorySettings(this.PortalInfo.PortalID, this.TabModuleId, eventInfo.CategoryID)).BCCList;
				}
				if (bCCList.Count != 0)
				{
					if (this.ModuleSettings.AttachICalendar)
					{
						empty = this.CreateICalendarAttachmentFile(eventInfo, "CANCEL", eventInfo.FullName, calendarInfo);
					}
					foreach (UserInfo userInfo in bCCList)
					{
						str = this.ReplaceTokens(eventInfo, this.ModuleSettings.CancellationMailSubject, userInfo.DisplayName, false);
						str1 = this.ReplaceTokens(eventInfo, this.ModuleSettings.CancellationMailBody, userInfo.DisplayName, false);
						Mail.SendMail(this.ModuleSettings.MailFrom, userInfo.Email, string.Empty, str, str1, empty, this.ModuleSettings.CancellationMailBodyType, string.Empty, string.Empty, string.Empty, string.Empty);
					}
					this.DeleteICalendarAttachmentFile(empty);
				}
				this.AddOrUpdateICalendarInfo(calendarInfo);
			}
			catch (Exception exception)
			{
				Exceptions.LogException(exception);
			}
		}

		public void SendConfirmationMail(ReservationInfo eventInfo)
		{
			try
			{
				string str = this.ReplaceTokens(eventInfo, this.ModuleSettings.ConfirmationMailSubject, eventInfo.FullName, true);
				string str1 = this.ReplaceTokens(eventInfo, this.ModuleSettings.ConfirmationMailBody, eventInfo.FullName, true);
				string empty = string.Empty;
				ICalendarInfo calendarInfo = null;
				if (this.ModuleSettings.AttachICalendar)
				{
					calendarInfo = this.GetICalendarInfo(eventInfo);
					empty = this.CreateICalendarAttachmentFile(eventInfo, "REQUEST", this.ReplaceTokens(eventInfo, this.ModuleSettings.ICalendarAttachmentFileName, eventInfo.FullName, true), calendarInfo);
				}
				if (!string.IsNullOrEmpty(eventInfo.Email))
				{
					Mail.SendMail(this.ModuleSettings.MailFrom, eventInfo.Email, string.Empty, str, str1, empty, this.ModuleSettings.ConfirmationMailBodyType, string.Empty, string.Empty, string.Empty, string.Empty);
				}
				this.DeleteICalendarAttachmentFile(empty);
				ArrayList bCCList = this.ModuleSettings.BCCList;
				if (eventInfo.CategoryID != Null.NullInteger && eventInfo.CategoryID != 0)
				{
					bCCList = (new CategorySettings(this.PortalInfo.PortalID, this.TabModuleId, eventInfo.CategoryID)).BCCList;
				}
				if (bCCList.Count != 0)
				{
					if (this.ModuleSettings.AttachICalendar)
					{
						empty = this.CreateICalendarAttachmentFile(eventInfo, "REQUEST", eventInfo.FullName, calendarInfo);
					}
					foreach (UserInfo userInfo in bCCList)
					{
						str = this.ReplaceTokens(eventInfo, this.ModuleSettings.ConfirmationMailSubject, userInfo.DisplayName, false);
						str1 = this.ReplaceTokens(eventInfo, this.ModuleSettings.ConfirmationMailBody, userInfo.DisplayName, false);
						Mail.SendMail(this.ModuleSettings.MailFrom, userInfo.Email, string.Empty, str, str1, empty, this.ModuleSettings.ConfirmationMailBodyType, string.Empty, string.Empty, string.Empty, string.Empty);
					}
					this.DeleteICalendarAttachmentFile(empty);
				}
				this.AddOrUpdateICalendarInfo(calendarInfo);
			}
			catch (Exception exception)
			{
				Exceptions.LogException(exception);
			}
		}

		public void SendDeclinedMail(PendingApprovalInfo pendingApprovalInfo)
		{
			try
			{
				string str = this.ReplaceTokens(pendingApprovalInfo, this.ModuleSettings.DeclinedMailSubject, pendingApprovalInfo.FullName);
				string str1 = this.ReplaceTokens(pendingApprovalInfo, this.ModuleSettings.DeclinedMailBody, pendingApprovalInfo.FullName);
				Mail.SendMail(this.ModuleSettings.MailFrom, pendingApprovalInfo.Email, string.Empty, str, str1, string.Empty, this.ModuleSettings.DeclinedMailBodyType, string.Empty, string.Empty, string.Empty, string.Empty);
			}
			catch (Exception exception)
			{
				Exceptions.LogException(exception);
			}
		}

		public void SendEmergencyRequestMail(string first, string last, string email, string phone, int category, string reason)
		{
			try
			{
				foreach (UserInfo bCCList in (new CategorySettings(this.PortalInfo.PortalID, this.TabModuleId, category)).BCCList)
				{
					string str = this.ReplaceTokens(first, last, email, phone, category, reason, this.ModuleSettings.EmergencyRequestMailSubject, bCCList.DisplayName);
					string str1 = this.ReplaceTokens(first, last, email, phone, category, reason, this.ModuleSettings.EmergencyRequestMailBody, bCCList.DisplayName);
					Mail.SendMail(this.ModuleSettings.MailFrom, bCCList.Email, string.Empty, str, str1, string.Empty, this.ModuleSettings.EmergencyRequestMailBodyType, string.Empty, string.Empty, string.Empty, string.Empty);
				}
			}
			catch (Exception exception)
			{
				Exceptions.LogException(exception);
			}
		}

		public void SendModeratorMail(PendingApprovalInfo pendingApprovalInfo)
		{
			CategorySettings categorySetting;
			try
			{
				if (this.ModuleSettings.AllowCategorySelection)
				{
					categorySetting = new CategorySettings(this.PortalInfo.PortalID, this.TabModuleId, pendingApprovalInfo.CategoryID);
				}
				else
				{
					categorySetting = null;
				}
				CategorySettings categorySetting1 = categorySetting;
				foreach (UserInfo userInfo in (categorySetting1 != null ? categorySetting1.ModeratorList : this.ModuleSettings.ModeratorList))
				{
					string str = this.ReplaceTokens(pendingApprovalInfo, this.ModuleSettings.ModeratorMailSubject, userInfo.DisplayName);
					string str1 = this.ReplaceTokens(pendingApprovalInfo, this.ModuleSettings.ModeratorMailBody, userInfo.DisplayName);
					Mail.SendMail(this.ModuleSettings.MailFrom, userInfo.Email, string.Empty, str, str1, string.Empty, this.ModuleSettings.ModeratorMailBodyType, string.Empty, string.Empty, string.Empty, string.Empty);
				}
			}
			catch (Exception exception)
			{
				Exceptions.LogException(exception);
			}
		}

		public void SendModificationMail(ReservationInfo eventInfo)
		{
			try
			{
				string str = this.ReplaceTokens(eventInfo, this.ModuleSettings.ModificationMailSubject, eventInfo.FullName, true);
				string str1 = this.ReplaceTokens(eventInfo, this.ModuleSettings.ModificationMailBody, eventInfo.FullName, true);
				string empty = string.Empty;
				ICalendarInfo calendarInfo = null;
				if (this.ModuleSettings.AttachICalendar)
				{
					calendarInfo = this.GetICalendarInfo(eventInfo);
					empty = this.CreateICalendarAttachmentFile(eventInfo, "REQUEST", this.ReplaceTokens(eventInfo, this.ModuleSettings.ICalendarAttachmentFileName, eventInfo.FullName, true), calendarInfo);
				}
				if (!string.IsNullOrEmpty(eventInfo.Email))
				{
					Mail.SendMail(this.ModuleSettings.MailFrom, eventInfo.Email, string.Empty, str, str1, empty, this.ModuleSettings.ModificationMailBodyType, string.Empty, string.Empty, string.Empty, string.Empty);
				}
				this.DeleteICalendarAttachmentFile(empty);
				ArrayList bCCList = this.ModuleSettings.BCCList;
				if (eventInfo.CategoryID != Null.NullInteger && eventInfo.CategoryID != 0)
				{
					bCCList = (new CategorySettings(this.PortalInfo.PortalID, this.TabModuleId, eventInfo.CategoryID)).BCCList;
				}
				if (bCCList.Count != 0)
				{
					if (this.ModuleSettings.AttachICalendar)
					{
						empty = this.CreateICalendarAttachmentFile(eventInfo, "REQUEST", eventInfo.FullName, calendarInfo);
					}
					foreach (UserInfo userInfo in bCCList)
					{
						str = this.ReplaceTokens(eventInfo, this.ModuleSettings.ModificationMailSubject, userInfo.DisplayName, false);
						str1 = this.ReplaceTokens(eventInfo, this.ModuleSettings.ModificationMailBody, userInfo.DisplayName, false);
						Mail.SendMail(this.ModuleSettings.MailFrom, userInfo.Email, string.Empty, str, str1, empty, this.ModuleSettings.ModificationMailBodyType, string.Empty, string.Empty, string.Empty, string.Empty);
					}
					this.DeleteICalendarAttachmentFile(empty);
				}
				this.AddOrUpdateICalendarInfo(calendarInfo);
			}
			catch (Exception exception)
			{
				Exceptions.LogException(exception);
			}
		}

		public void SendPendingCancellationRefundMail(ReservationInfo eventInfo, decimal amount, string currency)
		{
			try
			{
				string str = this.ReplaceTokens(eventInfo, this.ModuleSettings.PendingCancellationRefundMailSubject, this.ModuleSettings.PayPalAccount, true);
				string str1 = this.ReplaceTokens(eventInfo, this.ModuleSettings.PendingCancellationRefundMailBody, this.ModuleSettings.PayPalAccount, true);
				str = str.Replace("{Amount}", Helper.GetFriendlyAmount(amount, currency));
				str1 = str1.Replace("{Amount}", Helper.GetFriendlyAmount(amount, currency));
				ArrayList cashierList = this.ModuleSettings.CashierList;
				if (eventInfo.CategoryID != Null.NullInteger && eventInfo.CategoryID != 0)
				{
					cashierList = (new CategorySettings(this.PortalInfo.PortalID, this.TabModuleId, eventInfo.CategoryID)).CashierList;
				}
				if (cashierList.Count != 0)
				{
					foreach (UserInfo userInfo in cashierList)
					{
						str = this.ReplaceTokens(eventInfo, this.ModuleSettings.PendingCancellationRefundMailSubject, userInfo.DisplayName, false);
						str1 = this.ReplaceTokens(eventInfo, this.ModuleSettings.PendingCancellationRefundMailBody, userInfo.DisplayName, false);
						str = str.Replace("{Amount}", Helper.GetFriendlyAmount(amount, currency));
						str1 = str1.Replace("{Amount}", Helper.GetFriendlyAmount(amount, currency));
						Mail.SendMail(this.ModuleSettings.MailFrom, userInfo.Email, string.Empty, str, str1, string.Empty, this.ModuleSettings.PendingCancellationRefundMailBodyType, string.Empty, string.Empty, string.Empty, string.Empty);
					}
				}
			}
			catch (Exception exception)
			{
				Exceptions.LogException(exception);
			}
		}

		public void SendPendingDeclinationRefundMail(PendingApprovalInfo pendingApprovalInfo, decimal amount, string currency)
		{
			try
			{
				string str = this.ReplaceTokens(pendingApprovalInfo, this.ModuleSettings.PendingDeclinationRefundMailSubject, this.ModuleSettings.PayPalAccount);
				string str1 = this.ReplaceTokens(pendingApprovalInfo, this.ModuleSettings.PendingDeclinationRefundMailBody, this.ModuleSettings.PayPalAccount);
				str = str.Replace("{Amount}", Helper.GetFriendlyAmount(amount, currency));
				str1 = str1.Replace("{Amount}", Helper.GetFriendlyAmount(amount, currency));
				ArrayList cashierList = this.ModuleSettings.CashierList;
				if (pendingApprovalInfo.CategoryID != Null.NullInteger && pendingApprovalInfo.CategoryID != 0)
				{
					cashierList = (new CategorySettings(this.PortalInfo.PortalID, this.TabModuleId, pendingApprovalInfo.CategoryID)).CashierList;
				}
				if (cashierList.Count != 0)
				{
					foreach (UserInfo userInfo in cashierList)
					{
						str = this.ReplaceTokens(pendingApprovalInfo, this.ModuleSettings.PendingDeclinationRefundMailSubject, userInfo.DisplayName);
						str1 = this.ReplaceTokens(pendingApprovalInfo, this.ModuleSettings.PendingDeclinationRefundMailBody, userInfo.DisplayName);
						str = str.Replace("{Amount}", Helper.GetFriendlyAmount(amount, currency));
						str1 = str1.Replace("{Amount}", Helper.GetFriendlyAmount(amount, currency));
						Mail.SendMail(this.ModuleSettings.MailFrom, userInfo.Email, string.Empty, str, str1, string.Empty, this.ModuleSettings.PendingDeclinationRefundMailBodyType, string.Empty, string.Empty, string.Empty, string.Empty);
					}
				}
			}
			catch (Exception exception)
			{
				Exceptions.LogException(exception);
			}
		}

		public void SendPendingRescheduleRefundMail(ReservationInfo eventInfo, decimal amount, string currency)
		{
			try
			{
				string str = this.ReplaceTokens(eventInfo, this.ModuleSettings.PendingRescheduleRefundMailSubject, this.ModuleSettings.PayPalAccount, true);
				string str1 = this.ReplaceTokens(eventInfo, this.ModuleSettings.PendingRescheduleRefundMailBody, this.ModuleSettings.PayPalAccount, true);
				str = str.Replace("{Amount}", Helper.GetFriendlyAmount(amount, currency));
				str1 = str1.Replace("{Amount}", Helper.GetFriendlyAmount(amount, currency));
				ArrayList cashierList = this.ModuleSettings.CashierList;
				if (eventInfo.CategoryID != Null.NullInteger && eventInfo.CategoryID != 0)
				{
					cashierList = (new CategorySettings(this.PortalInfo.PortalID, this.TabModuleId, eventInfo.CategoryID)).CashierList;
				}
				if (cashierList.Count != 0)
				{
					foreach (UserInfo userInfo in cashierList)
					{
						str = this.ReplaceTokens(eventInfo, this.ModuleSettings.PendingRescheduleRefundMailSubject, userInfo.DisplayName, false);
						str1 = this.ReplaceTokens(eventInfo, this.ModuleSettings.PendingRescheduleRefundMailBody, userInfo.DisplayName, false);
						str = str.Replace("{Amount}", Helper.GetFriendlyAmount(amount, currency));
						str1 = str1.Replace("{Amount}", Helper.GetFriendlyAmount(amount, currency));
						Mail.SendMail(this.ModuleSettings.MailFrom, userInfo.Email, string.Empty, str, str1, string.Empty, this.ModuleSettings.PendingRescheduleRefundMailBodyType, string.Empty, string.Empty, string.Empty, string.Empty);
					}
				}
			}
			catch (Exception exception)
			{
				Exceptions.LogException(exception);
			}
		}

		public void SendReminderMail(ReservationInfo eventInfo)
		{
			try
			{
				string str = this.ReplaceTokens(eventInfo, this.ModuleSettings.ReminderMailSubject, eventInfo.FullName, true);
				string str1 = this.ReplaceTokens(eventInfo, this.ModuleSettings.ReminderMailBody, eventInfo.FullName, true);
				if (!string.IsNullOrEmpty(eventInfo.Email))
				{
					Mail.SendMail(this.ModuleSettings.MailFrom, eventInfo.Email, string.Empty, str, str1, string.Empty, this.ModuleSettings.ReminderMailBodyType, string.Empty, string.Empty, string.Empty, string.Empty);
				}
			}
			catch (Exception exception)
			{
				Exceptions.LogException(exception);
			}
		}

		public void SendReminderSMS(ReservationInfo eventInfo)
		{
			try
			{
				string str = this.ReplaceTokens(eventInfo, this.ModuleSettings.ReminderSMS, eventInfo.FullName, true);
				if (!string.IsNullOrEmpty(eventInfo.Phone) && !string.IsNullOrEmpty(this.ModuleSettings.TwilioAccountSID) && !string.IsNullOrEmpty(this.ModuleSettings.TwilioAuthToken) && !string.IsNullOrEmpty(this.ModuleSettings.TwilioFrom))
				{
					TwilioRestClient twilioRestClient = new TwilioRestClient(this.ModuleSettings.TwilioAccountSID, this.ModuleSettings.TwilioAuthToken);
					SMSMessage sMSMessage = null;
					try
					{
						sMSMessage = twilioRestClient.SendSmsMessage(this.ModuleSettings.TwilioFrom, eventInfo.Phone, str, string.Empty);
						if (sMSMessage != null && sMSMessage.RestException != null)
						{
							throw new Exception(sMSMessage.RestException.Message);
						}
						if (sMSMessage == null || string.IsNullOrEmpty(sMSMessage.Sid))
						{
							throw new Exception("SendSmsMessage returned null message or empty Sid.");
						}
					}
					catch (Exception exception)
					{
						Exceptions.LogException(exception);
					}
				}
			}
			catch (Exception exception1)
			{
				Exceptions.LogException(exception1);
			}
		}

		public void SendRescheduledMail(ReservationInfo eventInfo)
		{
			try
			{
				string str = this.ReplaceTokens(eventInfo, this.ModuleSettings.RescheduledMailSubject, eventInfo.FullName, true);
				string str1 = this.ReplaceTokens(eventInfo, this.ModuleSettings.RescheduledMailBody, eventInfo.FullName, true);
				string empty = string.Empty;
				ICalendarInfo calendarInfo = null;
				if (this.ModuleSettings.AttachICalendar)
				{
					calendarInfo = this.GetICalendarInfo(eventInfo);
					empty = this.CreateICalendarAttachmentFile(eventInfo, "REQUEST", this.ReplaceTokens(eventInfo, this.ModuleSettings.ICalendarAttachmentFileName, eventInfo.FullName, true), calendarInfo);
				}
				if (!string.IsNullOrEmpty(eventInfo.Email))
				{
					Mail.SendMail(this.ModuleSettings.MailFrom, eventInfo.Email, string.Empty, str, str1, empty, this.ModuleSettings.RescheduledMailBodyType, string.Empty, string.Empty, string.Empty, string.Empty);
				}
				this.DeleteICalendarAttachmentFile(empty);
				ArrayList bCCList = this.ModuleSettings.BCCList;
				if (eventInfo.CategoryID != Null.NullInteger && eventInfo.CategoryID != 0)
				{
					bCCList = (new CategorySettings(this.PortalInfo.PortalID, this.TabModuleId, eventInfo.CategoryID)).BCCList;
				}
				if (bCCList.Count != 0)
				{
					if (this.ModuleSettings.AttachICalendar)
					{
						empty = this.CreateICalendarAttachmentFile(eventInfo, "REQUEST", eventInfo.FullName, calendarInfo);
					}
					foreach (UserInfo userInfo in bCCList)
					{
						str = this.ReplaceTokens(eventInfo, this.ModuleSettings.RescheduledMailSubject, userInfo.DisplayName, false);
						str1 = this.ReplaceTokens(eventInfo, this.ModuleSettings.RescheduledMailBody, userInfo.DisplayName, false);
						Mail.SendMail(this.ModuleSettings.MailFrom, userInfo.Email, string.Empty, str, str1, empty, this.ModuleSettings.RescheduledMailBodyType, string.Empty, string.Empty, string.Empty, string.Empty);
					}
					this.DeleteICalendarAttachmentFile(empty);
				}
				this.AddOrUpdateICalendarInfo(calendarInfo);
			}
			catch (Exception exception)
			{
				Exceptions.LogException(exception);
			}
		}

		public void SendVerificationCodeMail(string email, string verificationCode)
		{
			try
			{
				string str = this.ModuleSettings.VerificationCodeMailSubject.Replace("{Email}", email).Replace("{VerificationCode}", verificationCode);
				string str1 = this.ModuleSettings.VerificationCodeMailBody.Replace("{Email}", email).Replace("{VerificationCode}", verificationCode);
				Mail.SendMail(this.ModuleSettings.MailFrom, email, string.Empty, str, str1, string.Empty, this.ModuleSettings.VerificationCodeMailBodyType, string.Empty, string.Empty, string.Empty, string.Empty);
			}
			catch (Exception exception)
			{
				Exceptions.LogException(exception);
			}
		}

		public static string SerializeRecurrencePattern(RecurrencePattern recurrencePattern)
		{
			string str;
			StringWriter stringWriter = null;
			try
			{
				stringWriter = new StringWriter();
				(new XmlSerializer(typeof(RecurrencePattern))).Serialize(new XmlTextWriter(stringWriter), recurrencePattern);
				str = stringWriter.ToString();
			}
			finally
			{
				if (stringWriter != null)
				{
					stringWriter.Close();
				}
			}
			return str;
		}

		public static string SerializeSeasonalFeeScheduleList(List<SeasonalFeeScheduleInfo> feeScheduleList)
		{
			string str;
			SeasonalFeeScheduleInfo[] array = feeScheduleList.ToArray();
			StringWriter stringWriter = null;
			try
			{
				stringWriter = new StringWriter();
				(new XmlSerializer(typeof(SeasonalFeeScheduleInfo[]))).Serialize(new XmlTextWriter(stringWriter), array);
				str = stringWriter.ToString();
			}
			finally
			{
				if (stringWriter != null)
				{
					stringWriter.Close();
				}
			}
			return str;
		}

		public static void SetEventInfoPropertiesFromPendingPaymentInfo(ReservationInfo reservationInfo, PendingPaymentInfo pendingPaymentInfo, Gafware.Modules.Reservations.ModuleSettings moduleSettings, int createdByUserID, string createdByDisplayName)
		{
			bool flag;
			reservationInfo.TabModuleID = pendingPaymentInfo.TabModuleID;
			reservationInfo.CategoryID = pendingPaymentInfo.CategoryID;
			reservationInfo.StartDateTime = pendingPaymentInfo.StartDateTime;
			reservationInfo.Duration = pendingPaymentInfo.Duration;
			reservationInfo.Description = pendingPaymentInfo.Description;
			ReservationInfo reservationInfo1 = reservationInfo;
			if (!moduleSettings.SendReminders)
			{
				flag = false;
			}
			else
			{
				DateTime now = Helper.GetNow(moduleSettings.TimeZone);
				DateTime startDateTime = reservationInfo.StartDateTime;
				flag = now < startDateTime.Subtract(moduleSettings.SendRemindersWhen);
			}
			reservationInfo1.SendReminder = flag;
			TimeSpan sendRemindersWhen = moduleSettings.SendRemindersWhen;
			reservationInfo.SendReminderWhen = (int)sendRemindersWhen.TotalMinutes;
			if (reservationInfo.SendReminder)
			{
				reservationInfo.SendReminderVia = moduleSettings.SendRemindersVia;
			}
			reservationInfo.RequireConfirmation = (!moduleSettings.RequireConfirmation ? false : reservationInfo.SendReminder);
			sendRemindersWhen = moduleSettings.RequireConfirmationWhen;
			reservationInfo.RequireConfirmationWhen = (int)sendRemindersWhen.TotalMinutes;
			if (reservationInfo.ReservationID != Null.NullInteger)
			{
				reservationInfo.LastModifiedByUserID = createdByUserID;
				reservationInfo.LastModifiedOnDate = pendingPaymentInfo.CreatedOnDate;
				return;
			}
			reservationInfo.FirstName = pendingPaymentInfo.FirstName;
			reservationInfo.LastName = pendingPaymentInfo.LastName;
			reservationInfo.CreatedByUserID = createdByUserID;
			reservationInfo.CreatedOnDate = pendingPaymentInfo.CreatedOnDate;
			reservationInfo.Email = pendingPaymentInfo.Email;
			reservationInfo.Phone = pendingPaymentInfo.Phone;
		}

		public static void SetPendingApprovalInfoPropertiesFromEventInfo(PendingApprovalInfo pendingApprovalInfo, ReservationInfo eventInfo)
		{
			pendingApprovalInfo.ReservationID = eventInfo.ReservationID;
			pendingApprovalInfo.CategoryID = (eventInfo.CategoryID == Null.NullInteger ? 0 : eventInfo.CategoryID);
			pendingApprovalInfo.StartDateTime = eventInfo.StartDateTime;
			pendingApprovalInfo.Duration = eventInfo.Duration;
			pendingApprovalInfo.FirstName = eventInfo.FirstName;
			pendingApprovalInfo.LastName = eventInfo.LastName;
			pendingApprovalInfo.Email = eventInfo.Email;
			pendingApprovalInfo.Phone = eventInfo.Phone;
			pendingApprovalInfo.Description = eventInfo.Description;
		}

		public static void SetPendingPaymentInfoPropertiesFromEventInfo(PendingPaymentInfo pendingPaymentInfo, ReservationInfo eventInfo)
		{
			pendingPaymentInfo.ReservationID = eventInfo.ReservationID;
			pendingPaymentInfo.CategoryID = (eventInfo.CategoryID == Null.NullInteger ? 0 : eventInfo.CategoryID);
			pendingPaymentInfo.StartDateTime = eventInfo.StartDateTime;
			pendingPaymentInfo.Duration = eventInfo.Duration;
			pendingPaymentInfo.FirstName = eventInfo.FirstName;
			pendingPaymentInfo.LastName = eventInfo.LastName;
			pendingPaymentInfo.Email = eventInfo.Email;
			pendingPaymentInfo.Phone = eventInfo.Phone;
			pendingPaymentInfo.Description = eventInfo.Description;
		}

		public static void SetPendingPaymentInfoPropertiesFromPendingApprovalInfo(PendingPaymentInfo pendingPaymentInfo, PendingApprovalInfo pendingApprovalInfo)
		{
			pendingPaymentInfo.PendingApprovalID = pendingApprovalInfo.PendingApprovalID;
			pendingPaymentInfo.ReservationID = pendingApprovalInfo.ReservationID;
			pendingPaymentInfo.TabModuleID = pendingApprovalInfo.TabModuleID;
			pendingPaymentInfo.PortalID = pendingApprovalInfo.PortalID;
			pendingPaymentInfo.CategoryID = pendingApprovalInfo.CategoryID;
			pendingPaymentInfo.StartDateTime = pendingApprovalInfo.StartDateTime;
			pendingPaymentInfo.Duration = pendingApprovalInfo.Duration;
			pendingPaymentInfo.FirstName = pendingApprovalInfo.FirstName;
			pendingPaymentInfo.LastName = pendingApprovalInfo.LastName;
			pendingPaymentInfo.Email = pendingApprovalInfo.Email;
			pendingPaymentInfo.Phone = pendingApprovalInfo.Phone;
			pendingPaymentInfo.Description = pendingApprovalInfo.Description;
			pendingPaymentInfo.CreatedByUserID = pendingApprovalInfo.CreatedByUserID;
			pendingPaymentInfo.CreatedOnDate = DateTime.Now;
		}

		public DateTime TryParseDate(string date)
		{
			DateTime dateTime = new DateTime();
			DateTime.TryParse(date, out dateTime);
			return dateTime;
		}

		public static void UpdateDueAndPendingRefundPaymentInfoFromEventInfo(ReservationInfo eventInfo, int tabModuleID)
		{
			PendingPaymentController pendingPaymentController = new PendingPaymentController();
			List<PendingPaymentInfo> pendingPaymentList = pendingPaymentController.GetPendingPaymentList(tabModuleID);
			PendingPaymentInfo now = Helper.FindPendingPaymentInfoByEventIDAndStatus(pendingPaymentList, eventInfo.ReservationID, 7);
			PendingPaymentInfo pendingPaymentInfo = Helper.FindPendingPaymentInfoByEventIDAndStatus(pendingPaymentList, eventInfo.ReservationID, 5);
			if (now != null)
			{
				Helper.SetPendingPaymentInfoPropertiesFromEventInfo(now, eventInfo);
				now.LastModifiedOnDate = DateTime.Now;
				pendingPaymentController.UpdatePendingPayment(now);
			}
			if (pendingPaymentInfo != null)
			{
				Helper.SetPendingPaymentInfoPropertiesFromEventInfo(pendingPaymentInfo, eventInfo);
				pendingPaymentInfo.LastModifiedOnDate = DateTime.Now;
				pendingPaymentController.UpdatePendingPayment(pendingPaymentInfo);
			}
		}

		public static bool ValidateActivationCode(string activationCode)
		{
			if (!string.IsNullOrEmpty(activationCode))
			{
				/*for (int i = 0; i < (int)Helper.publicKeys.Length; i++)
				{
					if (Helper.ValidateActivationCode(activationCode, Helper.publicKeys[i]))
					{
						return true;
					}
				}*/

				try
				{
					string verify = Helper.Encrypt(GetFingerprint().ToLower());
					return activationCode.Equals(verify.ToUpper());
				}
				catch
                {
                }
			}
			return false;
		}

		private static bool ValidateActivationCode(string activationCode, string _publicKey)
		{
			bool flag;
			try
			{
				byte[] bytes = Encoding.UTF8.GetBytes(Helper.GetFingerprint().ToLower());
				byte[] numArray = new byte[activationCode.Length / 2];
				for (int i = 0; i < activationCode.Length / 2; i++)
				{
					numArray[i] = byte.Parse(activationCode.Substring(i * 2, 2), NumberStyles.HexNumber);
				}
				byte[] numArray1 = new byte[_publicKey.Length / 2];
				for (int j = 0; j < _publicKey.Length / 2; j++)
				{
					numArray1[j] = byte.Parse(_publicKey.Substring(j * 2, 2), NumberStyles.HexNumber);
				}
				RSACryptoServiceProvider rSACryptoServiceProvider = new RSACryptoServiceProvider();
				rSACryptoServiceProvider.ImportCspBlob(numArray1);
				flag = rSACryptoServiceProvider.VerifyData(bytes, SHA1.Create(), numArray);
			}
			catch (Exception)
			{
				flag = false;
			}
			return flag;
		}
	}
}