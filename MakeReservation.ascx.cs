using AuthorizeNet;
using DotNetNuke.Abstractions;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Security;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins.Controls;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Gafware.Modules.Reservations
{
    public partial class MakeReservation : PortalModuleBase, IActionable
    {
		protected Gafware.Modules.Reservations.Helper _Helper;

		protected Gafware.Modules.Reservations.ModuleSettings _ModuleSettings;

		protected CategorySettings _SelectedCategorySettings;

		protected string _None;

		protected List<CategoryInfo> _CategoryList;

		protected Gafware.Modules.Reservations.ReservationController _ReservationController;

		protected Gafware.Modules.Reservations.ReservationInfo _ReservationInfo;

		protected List<Gafware.Modules.Reservations.ReservationInfo> _ReservationList;

		protected Dictionary<int, CategorySettings> _CategorySettingsDictionary;

		protected Dictionary<int, List<DateTimeRange>> _WorkingHoursDictionary;

		protected Gafware.Modules.Reservations.PendingApprovalInfo _PendingApprovalInfo;

		protected List<Gafware.Modules.Reservations.PendingApprovalInfo> _PendingApprovalList;

		protected Gafware.Modules.Reservations.PendingPaymentInfo _PendingPaymentInfo;

		protected List<Gafware.Modules.Reservations.PendingPaymentInfo> _PendingPaymentList;

		protected List<Gafware.Modules.Reservations.ReservationInfo> _ComprehensiveReservationList;

		private bool? _IsProfesional;

		private Dictionary<int, List<DateTime>> _AvailableReservationStartTimesDictionary = new Dictionary<int, List<DateTime>>();

		private Dictionary<int, List<DateTime>> _AvailableReservationDays = new Dictionary<int, List<DateTime>>();

		private Gafware.Modules.Reservations.PendingPaymentInfo _DuePendingPaymentInfo;

		private bool MissingRequiredFieldsModuleMessageAdded;

		private List<CustomFieldDefinitionInfo> _CustomFieldDefinitionInfoList;

		private int _NumberOfDivs;

		private List<CustomFieldValueInfo> _CustomFieldValueInfoList;

		private readonly INavigationManager _navigationManager;
		public MakeReservation()
		{
			_navigationManager = DependencyProvider.GetRequiredService<INavigationManager>();
		}

		protected decimal Amount
		{
			get
			{
				if (ReservationInfo.ReservationID == Null.NullInteger)
				{
					return RefundableAmount;
				}
				decimal num = new decimal();
				if (!ModuleSettings.AllowCategorySelection || SelectedCategory == 0 || ReservationInfo.CategoryID == 0 || ReservationInfo.CategoryID == Null.NullInteger)
				{
					num = Helper.CalculateReschedulingFee(ModuleSettings.FeeScheduleType, ModuleSettings.FlatFeeScheduleInfo, ModuleSettings.SeasonalFeeScheduleList, ReservationInfo.StartDateTime);
				}
				else
				{
					CategorySettings item = CategorySettingsDictionary[ReservationInfo.CategoryID];
					num = Helper.CalculateReschedulingFee(item.FeeScheduleType, item.FlatFeeScheduleInfo, item.SeasonalFeeScheduleList, ReservationInfo.StartDateTime);
				}
				return (DueAmount + RefundableAmount) + num;
			}
		}

		private bool AreReservationsAvailable
		{
			get
			{
				if (ModuleSettings.AllowCategorySelection && !SelectCategoryLast)
				{
					return GetCategoryIDsToRender().Count != 0;
				}
				return GetAvailableReservationDays(0).Count != 0;
			}
		}

		protected int AvailableDaysCurrentPageIndex
		{
			get
			{
				if (ViewState["AvailableDaysCurrentPageIndex"] == null)
				{
					return 0;
				}
				return (int)ViewState["AvailableDaysCurrentPageIndex"];
			}
			set
			{
				ViewState["AvailableDaysCurrentPageIndex"] = value;
			}
		}

		protected int AvailableDurationsCurrentPageIndex
		{
			get
			{
				if (ViewState["AvailableDurationsCurrentPageIndex"] == null)
				{
					return 0;
				}
				return (int)ViewState["AvailableDurationsCurrentPageIndex"];
			}
			set
			{
				ViewState["AvailableDurationsCurrentPageIndex"] = value;
			}
		}

		protected int AvailableTimesCurrentPageIndex
		{
			get
			{
				if (ViewState["AvailableTimesCurrentPageIndex"] == null)
				{
					return 0;
				}
				return (int)ViewState["AvailableTimesCurrentPageIndex"];
			}
			set
			{
				ViewState["AvailableTimesCurrentPageIndex"] = value;
			}
		}

		protected int AvailableTimesOfDayCurrentPageIndex
		{
			get
			{
				if (ViewState["AvailableTimesOfDayCurrentPageIndex"] == null)
				{
					return 0;
				}
				return (int)ViewState["AvailableTimesOfDayCurrentPageIndex"];
			}
			set
			{
				ViewState["AvailableTimesOfDayCurrentPageIndex"] = value;
			}
		}

		protected decimal CancellationAmount
		{
			get
			{
				decimal dueAmount = new decimal();
				if (ReservationInfo.ReservationID != Null.NullInteger)
				{
					decimal num = new decimal();
					if (ReservationInfo.CategoryID == 0 || ReservationInfo.CategoryID == Null.NullInteger)
					{
						num = Helper.CalculateCancellationFee(ModuleSettings.FeeScheduleType, ModuleSettings.FlatFeeScheduleInfo, ModuleSettings.SeasonalFeeScheduleList, ReservationInfo.StartDateTime);
					}
					else
					{
						CategorySettings item = CategorySettingsDictionary[ReservationInfo.CategoryID];
						num = Helper.CalculateCancellationFee(item.FeeScheduleType, item.FlatFeeScheduleInfo, item.SeasonalFeeScheduleList, ReservationInfo.StartDateTime);
					}
					dueAmount = (DueAmount + CancellationRefundableAmount) + num;
				}
				return dueAmount;
			}
		}

		protected decimal CancellationRefundableAmount
		{
			get
			{
				decimal refundableAmount = new decimal();
				if (ReservationInfo.ReservationID != Null.NullInteger)
				{
					foreach (Gafware.Modules.Reservations.PendingPaymentInfo pendingPaymentList in (new PendingPaymentController()).GetPendingPaymentList(TabModuleId))
					{
						if (pendingPaymentList.ReservationID != ReservationInfo.ReservationID || pendingPaymentList.Status != 1 && pendingPaymentList.Status != 7 && pendingPaymentList.Status != 5 && pendingPaymentList.Status != 6)
						{
							continue;
						}
						refundableAmount = refundableAmount - pendingPaymentList.RefundableAmount;
					}
				}
				return refundableAmount;
			}
		}

		protected int CategoriesCurrentPageIndex
		{
			get
			{
				if (ViewState["CategoriesCurrentPageIndex"] == null)
				{
					return 0;
				}
				return (int)ViewState["CategoriesCurrentPageIndex"];
			}
			set
			{
				ViewState["CategoriesCurrentPageIndex"] = value;
			}
		}

		protected List<CategoryInfo> CategoryList
		{
			get
			{
				if (_CategoryList == null)
				{
					_CategoryList = new List<CategoryInfo>();
					List<CategoryInfo> categoryList = (new CategoryController()).GetCategoryList(TabModuleId);
					List<RoleInfo> portalRoles = (new RoleController()).GetRoles(PortalId).ToList(); //.GetPortalRoles(PortalId);
					for (int i = 0; i < categoryList.Count; i++)
					{
						foreach (int categoryPermissionsList in (new CategorySettings(PortalId, TabModuleId, categoryList[i].CategoryID)).CategoryPermissionsList)
						{
							if (!PortalSecurity.IsInRole(GetRoleNameByRoleID(new ArrayList(portalRoles), categoryPermissionsList)))
							{
								continue;
							}
							_CategoryList.Add(categoryList[i]);
							break;
						}
					}
				}
				return _CategoryList;
			}
		}

		protected Dictionary<int, CategorySettings> CategorySettingsDictionary
		{
			get
			{
				if (_CategorySettingsDictionary == null)
				{
					_CategorySettingsDictionary = new Dictionary<int, CategorySettings>();
					foreach (CategoryInfo categoryList in CategoryList)
					{
						_CategorySettingsDictionary.Add(categoryList.CategoryID, new CategorySettings(PortalId, TabModuleId, categoryList.CategoryID));
					}
				}
				return _CategorySettingsDictionary;
			}
		}

		protected List<Gafware.Modules.Reservations.ReservationInfo> ComprehensiveReservationList
		{
			get
			{
				if (_ComprehensiveReservationList == null)
				{
					_ComprehensiveReservationList = new List<Gafware.Modules.Reservations.ReservationInfo>();
					_ComprehensiveReservationList.AddRange(ReservationList);
					foreach (Gafware.Modules.Reservations.PendingApprovalInfo pendingApprovalList in PendingApprovalList)
					{
						Gafware.Modules.Reservations.ReservationInfo reservationInfo = new Gafware.Modules.Reservations.ReservationInfo()
						{
							CategoryID = pendingApprovalList.CategoryID,
							StartDateTime = pendingApprovalList.StartDateTime,
							Duration = pendingApprovalList.Duration
						};
						_ComprehensiveReservationList.Add(reservationInfo);
					}
					if (IsProfessional)
					{
						foreach (Gafware.Modules.Reservations.PendingPaymentInfo pendingPaymentList in PendingPaymentList)
						{
							if ((pendingPaymentList.Status != 0 || !(pendingPaymentList.CreatedOnDate.Add(ModuleSettings.PendingPaymentExpiration) > DateTime.Now)) && pendingPaymentList.Status != 4)
							{
								if (pendingPaymentList.Status != 0 || !(pendingPaymentList.CreatedOnDate.Add(ModuleSettings.PendingPaymentExpiration) <= DateTime.Now))
								{
									continue;
								}
								pendingPaymentList.Status = 3;
								pendingPaymentList.LastModifiedOnDate = DateTime.Now;
								(new PendingPaymentController()).UpdatePendingPayment(pendingPaymentList);
							}
							else
							{
								Gafware.Modules.Reservations.ReservationInfo reservationInfo1 = new Gafware.Modules.Reservations.ReservationInfo()
								{
									CategoryID = pendingPaymentList.CategoryID,
									StartDateTime = pendingPaymentList.StartDateTime,
									Duration = pendingPaymentList.Duration
								};
								_ComprehensiveReservationList.Add(reservationInfo1);
							}
						}
					}
					_ComprehensiveReservationList.Sort((Gafware.Modules.Reservations.ReservationInfo x, Gafware.Modules.Reservations.ReservationInfo y) => x.StartDateTime.CompareTo(y.StartDateTime));
				}
				return _ComprehensiveReservationList;
			}
		}

		private List<CustomFieldDefinitionInfo> CustomFieldDefinitionInfoList
		{
			get
			{
				if (_CustomFieldDefinitionInfoList == null)
				{
					_CustomFieldDefinitionInfoList = (new CustomFieldDefinitionController()).GetActiveCustomFieldDefinitionList(TabModuleId);
					_CustomFieldDefinitionInfoList.Sort(new MakeReservation.CustomFieldDefinitionInfoSortOrderComparer());
				}
				return _CustomFieldDefinitionInfoList;
			}
		}

		private List<CustomFieldValueInfo> CustomFieldValueInfoList
		{
			get
			{
				if (_CustomFieldValueInfoList == null)
				{
					if (!Null.IsNull(ReservationInfo.ReservationID))
					{
						_CustomFieldValueInfoList = (new CustomFieldValueController()).GetCustomFieldValueListByReservationID(ReservationInfo.ReservationID);
					}
					else if (!Null.IsNull(PendingPaymentInfo.PendingPaymentID))
					{
						_CustomFieldValueInfoList = (new CustomFieldValueController()).GetCustomFieldValueListByPendingPaymentID(PendingPaymentInfo.PendingPaymentID);
					}
					else if (!Null.IsNull(PendingApprovalInfo.PendingApprovalID))
					{
						_CustomFieldValueInfoList = (new CustomFieldValueController()).GetCustomFieldValueListByPendingApprovalID(PendingApprovalInfo.PendingApprovalID);
					}
				}
				return _CustomFieldValueInfoList;
			}
		}

		protected decimal DueAmount
		{
			get
			{
				if (DuePendingPaymentInfo == null)
				{
					return decimal.Zero;
				}
				return DuePendingPaymentInfo.Amount;
			}
		}

		protected Gafware.Modules.Reservations.PendingPaymentInfo DuePendingPaymentInfo
		{
			get
			{
				if (_DuePendingPaymentInfo == null && ReservationInfo.ReservationID != Null.NullInteger)
				{
					_DuePendingPaymentInfo = Gafware.Modules.Reservations.Helper.FindPendingPaymentInfoByEventIDAndStatus((new PendingPaymentController()).GetPendingPaymentList(TabModuleId), ReservationInfo.ReservationID, 7);
				}
				return _DuePendingPaymentInfo;
			}
		}

		protected bool HasEditPermissions
		{
			get
			{
				return (new ModuleSecurity(ModuleConfiguration)).HasEditPermissions;
			}
		}

		protected Gafware.Modules.Reservations.Helper Helper
		{
			get
			{
				if (_Helper == null)
				{
					_Helper = new Gafware.Modules.Reservations.Helper(PortalId, TabModuleId, LocalResourceFile);
				}
				return _Helper;
			}
		}

		protected bool IsCashier
		{
			get
			{
				if (UserId == Null.NullInteger)
				{
					return false;
				}
				return Helper.IsCashier(UserId);
			}
		}

		protected bool IsModerator
		{
			get
			{
				if (UserId == Null.NullInteger)
				{
					return false;
				}
				return Helper.IsModerator(UserId);
			}
		}

		private bool IsProfessional
		{
			get
			{
				if (!_IsProfesional.HasValue)
				{
					_IsProfesional = new bool?(Gafware.Modules.Reservations.Helper.GetEdition(ModuleSettings.ActivationCode) != "Standard");
				}
				return _IsProfesional.Value;
			}
		}

		protected TimeSpan MaxReservationDuration
		{
			get
			{
				if (ModuleSettings.AllowCategorySelection && SelectedCategory != 0)
				{
					return SelectedCategorySettings.ReservationDurationMax;
				}
				return ModuleSettings.ReservationDurationMax;
			}
		}

		protected TimeSpan MinReservationDuration
		{
			get
			{
				if (ModuleSettings.AllowCategorySelection && SelectedCategory != 0)
				{
					return SelectedCategorySettings.ReservationDuration;
				}
				return ModuleSettings.ReservationDuration;
			}
		}

		public ModuleActionCollection ModuleActions
		{
			get
			{
				ModuleActionCollection moduleActionCollection = new ModuleActionCollection();
				moduleActionCollection.Add(GetNextActionID(), Localization.GetString("Settings", LocalResourceFile), "", "", "action_settings.gif", EditUrl("EditSettings"), false, SecurityAccessLevel.Edit, true, false);
				if (!Gafware.Modules.Reservations.Helper.ValidateActivationCode(ModuleSettings.ActivationCode))
				{
					moduleActionCollection.Add(GetNextActionID(), Localization.GetString("Activate", LocalResourceFile), "", "", "action_settings.gif", EditUrl("Activate"), false, SecurityAccessLevel.Edit, true, false);
				}
				int nextActionID = GetNextActionID();
				string str = Localization.GetString("ManageCustomFields", LocalResourceFile);
				string[] strArrays = new string[1];
				int moduleId = ModuleId;
				strArrays[0] = string.Concat("mid=", moduleId.ToString());
				moduleActionCollection.Add(nextActionID, str, "", "", "action_settings.gif", _navigationManager.NavigateURL("ViewCustomFieldDefinitionList", strArrays), false, SecurityAccessLevel.Edit, true, false);
				return moduleActionCollection;
			}
		}

		protected Gafware.Modules.Reservations.ModuleSettings ModuleSettings
		{
			get
			{
				if (_ModuleSettings == null)
				{
					_ModuleSettings = new Gafware.Modules.Reservations.ModuleSettings(PortalId, TabModuleId);
				}
				return _ModuleSettings;
			}
		}

		protected string NotAvailable
		{
			get
			{
				if (_None == null)
				{
					_None = Localization.GetString("NotAvailable", LocalResourceFile);
				}
				return _None;
			}
		}

		protected Gafware.Modules.Reservations.PendingApprovalInfo PendingApprovalInfo
		{
			get
			{
				int num;
				if (_PendingApprovalInfo == null)
				{
					try
					{
						if (ViewState["PendingApprovalID"] != null)
						{
							_PendingApprovalInfo = (new PendingApprovalController()).GetPendingApproval((int)ViewState["PendingApprovalID"]);
						}
						else if (!IsPostBack && int.TryParse(Request.QueryString["PendingApprovalID"], out num))
						{
							_PendingApprovalInfo = (new PendingApprovalController()).GetPendingApproval(num);
							if (_PendingApprovalInfo.TabModuleID != TabModuleId)
							{
								_PendingApprovalInfo = null;
							}
						}
						if (_PendingApprovalInfo == null)
						{
							_PendingApprovalInfo = new Gafware.Modules.Reservations.PendingApprovalInfo();
						}
					}
					catch (Exception)
					{
						_PendingApprovalInfo = new Gafware.Modules.Reservations.PendingApprovalInfo();
					}
				}
				return _PendingApprovalInfo;
			}
			set
			{
				_PendingApprovalInfo = value;
			}
		}

		protected string PendingApprovalInfoCategoryName
		{
			get
			{
				return Helper.GetCategoryName(PendingApprovalInfo.CategoryID);
			}
		}

		protected List<Gafware.Modules.Reservations.PendingApprovalInfo> PendingApprovalList
		{
			get
			{
				if (_PendingApprovalList == null)
				{
					_PendingApprovalList = (
						from pendingApprovalInfo in (new PendingApprovalController()).GetPendingApprovalList(TabModuleId)
						where pendingApprovalInfo.Status == 0
						select pendingApprovalInfo).ToList<Gafware.Modules.Reservations.PendingApprovalInfo>();
				}
				return _PendingApprovalList;
			}
		}

		protected Gafware.Modules.Reservations.PendingPaymentInfo PendingPaymentInfo
		{
			get
			{
				int num;
				if (_PendingPaymentInfo == null)
				{
					try
					{
						if (ViewState["PendingPaymentID"] != null)
						{
							_PendingPaymentInfo = (new PendingPaymentController()).GetPendingPayment((int)ViewState["PendingPaymentID"]);
						}
						else if (!IsPostBack && int.TryParse(Request.QueryString["PendingPaymentID"], out num))
						{
							_PendingPaymentInfo = (new PendingPaymentController()).GetPendingPayment(num);
							if (_PendingPaymentInfo.TabModuleID != TabModuleId)
							{
								_PendingPaymentInfo = null;
							}
						}
						if (_PendingPaymentInfo == null)
						{
							_PendingPaymentInfo = new Gafware.Modules.Reservations.PendingPaymentInfo();
						}
					}
					catch (Exception)
					{
						_PendingPaymentInfo = new Gafware.Modules.Reservations.PendingPaymentInfo();
					}
				}
				return _PendingPaymentInfo;
			}
			set
			{
				_PendingPaymentInfo = value;
			}
		}

		protected string PendingPaymentInfoCategoryName
		{
			get
			{
				return Helper.GetCategoryName(PendingPaymentInfo.CategoryID);
			}
		}

		protected List<Gafware.Modules.Reservations.PendingPaymentInfo> PendingPaymentList
		{
			get
			{
				if (_PendingPaymentList == null)
				{
					_PendingPaymentList = (new PendingPaymentController()).GetPendingPaymentList(TabModuleId);
				}
				return _PendingPaymentList;
			}
		}

		protected string QueryStringPendingPaymentEmail
		{
			get
			{
				if (Page.IsPostBack)
				{
					return Null.NullString;
				}
				return Request.QueryString["Email"];
			}
		}

		protected PendingPaymentStatus QueryStringPendingPaymentStatus
		{
			get
			{
				PendingPaymentStatus pendingPaymentStatu;
				try
				{
					pendingPaymentStatu = (PendingPaymentStatus)Enum.Parse(typeof(PendingPaymentStatus), Request.QueryString["Status"]);
				}
				catch (Exception)
				{
					pendingPaymentStatu = PendingPaymentStatus.Processing;
				}
				return pendingPaymentStatu;
			}
		}

		protected decimal RefundableAmount
		{
			get
			{
				TimeSpan minReservationDuration = MinReservationDuration;
				if (SelectedDuration != new TimeSpan())
				{
					minReservationDuration = SelectedDuration;
				}
				decimal num = new decimal();
				DateTime selectedDateTime = SelectedDateTime;
				DateTime dateTime = new DateTime();
				DateTime dateTime1 = (selectedDateTime == dateTime ? SelectedDate : SelectedDateTime);
				num = (!ModuleSettings.AllowCategorySelection || SelectedCategory == 0 ? Helper.CalculateSchedulingFee(ModuleSettings.FeeScheduleType, ModuleSettings.FlatFeeScheduleInfo, ModuleSettings.SeasonalFeeScheduleList, dateTime1, minReservationDuration) : Helper.CalculateSchedulingFee(SelectedCategorySettings.FeeScheduleType, SelectedCategorySettings.FlatFeeScheduleInfo, SelectedCategorySettings.SeasonalFeeScheduleList, dateTime1, minReservationDuration));
				decimal refundableAmount = num;
				if (ReservationInfo.ReservationID != Null.NullInteger)
				{
					foreach (Gafware.Modules.Reservations.PendingPaymentInfo pendingPaymentList in (new PendingPaymentController()).GetPendingPaymentList(TabModuleId))
					{
						if (pendingPaymentList.ReservationID != ReservationInfo.ReservationID || pendingPaymentList.Status != 1 && pendingPaymentList.Status != 7 && pendingPaymentList.Status != 5 && pendingPaymentList.Status != 6)
						{
							continue;
						}
						refundableAmount = refundableAmount - pendingPaymentList.RefundableAmount;
					}
				}
				return refundableAmount;
			}
		}

		protected Gafware.Modules.Reservations.ReservationController ReservationController
		{
			get
			{
				if (_ReservationController == null)
				{
					_ReservationController = new Gafware.Modules.Reservations.ReservationController();
				}
				return _ReservationController;
			}
		}

		protected Gafware.Modules.Reservations.ReservationInfo ReservationInfo
		{
			get
			{
				if (_ReservationInfo == null)
				{
					try
					{
						_ReservationInfo = ReservationController.GetReservation(ViewState["ReservationInfo"] != null ? (int)ViewState["ReservationInfo"] : 0);
						if (_ReservationInfo == null)
						{
							_ReservationInfo = new Gafware.Modules.Reservations.ReservationInfo();
						}
					}
					catch (Exception)
					{
						_ReservationInfo = new Gafware.Modules.Reservations.ReservationInfo();
					}
				}
				return _ReservationInfo;
			}
			set
			{
				_ReservationInfo = value;
			}
		}

		protected List<Gafware.Modules.Reservations.ReservationInfo> ReservationList
		{
			get
			{
				if (_ReservationList == null)
				{
					TimeSpan minTimeAhead = ModuleSettings.MinTimeAhead;
					int daysAhead = ModuleSettings.DaysAhead;
					if (ModuleSettings.AllowCategorySelection)
					{
						foreach (CategoryInfo categoryList in CategoryList)
						{
							CategorySettings item = CategorySettingsDictionary[categoryList.CategoryID];
							if (item.DaysAhead > daysAhead)
							{
								daysAhead = item.DaysAhead;
							}
							if (item.MinTimeAhead >= minTimeAhead)
							{
								continue;
							}
							minTimeAhead = item.MinTimeAhead;
						}
					}
					Gafware.Modules.Reservations.ReservationController reservationController = ReservationController;
					int tabModuleId = TabModuleId;
					DateTime now = Gafware.Modules.Reservations.Helper.GetNow(ModuleSettings.TimeZone);
					now = now.Add(minTimeAhead);
					DateTime date = now.Date;
					now = Gafware.Modules.Reservations.Helper.GetNow(ModuleSettings.TimeZone);
					now = now.Date;
					_ReservationList = reservationController.GetReservationListByDateRangeAndCategoryID(tabModuleId, date, now.AddDays((double)(daysAhead + 1)), (!ModuleSettings.BindUponCategorySelection || SelectedCategory <= 0 ? Null.NullInteger : SelectedCategory));
					if (ModuleSettings.RequireConfirmation)
					{
						List<Gafware.Modules.Reservations.ReservationInfo> reservationInfos = new List<Gafware.Modules.Reservations.ReservationInfo>();
						foreach (Gafware.Modules.Reservations.ReservationInfo reservationInfo in _ReservationList)
						{
							if (!reservationInfo.RequireConfirmation)
							{
								reservationInfos.Add(reservationInfo);
							}
							else if (reservationInfo.Confirmed)
							{
								reservationInfos.Add(reservationInfo);
							}
							else if (Gafware.Modules.Reservations.Helper.GetNow(ModuleSettings.TimeZone) <= reservationInfo.StartDateTime.AddMinutes((double)(-1 * reservationInfo.RequireConfirmationWhen)))
							{
								reservationInfos.Add(reservationInfo);
							}
							else
							{
								Helper.SendCancellationMail(reservationInfo);
								ReservationController.DeleteReservation(reservationInfo.ReservationID);
							}
						}
						_ReservationList = reservationInfos;
					}
				}
				return _ReservationList;
			}
		}

		private bool SelectCategoryLast
		{
			get
			{
				return ModuleSettings.SelectCategoryLast;
			}
		}

		protected int SelectedCategory
		{
			get
			{
				if (ViewState["SelectedCategory"] == null)
				{
					return 0;
				}
				return (int)ViewState["SelectedCategory"];
			}
			set
			{
				ViewState["SelectedCategory"] = value;
			}
		}

		protected CategorySettings SelectedCategorySettings
		{
			get
			{
				if (_SelectedCategorySettings == null && SelectedCategory != 0)
				{
					_SelectedCategorySettings = CategorySettingsDictionary[SelectedCategory];
				}
				return _SelectedCategorySettings;
			}
		}

		protected DateTime SelectedDate
		{
			get
			{
				if (ViewState["SelectedDate"] == null)
				{
					return new DateTime();
				}
				return (DateTime)ViewState["SelectedDate"];
			}
			set
			{
				ViewState["SelectedDate"] = value;
			}
		}

		protected DateTime SelectedDateTime
		{
			get
			{
				if (ViewState["SelectedDateTime"] == null)
				{
					return new DateTime();
				}
				return (DateTime)ViewState["SelectedDateTime"];
			}
			set
			{
				ViewState["SelectedDateTime"] = value;
			}
		}

		protected TimeSpan SelectedDuration
		{
			get
			{
				if (ViewState["SelectedDuration"] == null)
				{
					return new TimeSpan();
				}
				return (TimeSpan)ViewState["SelectedDuration"];
			}
			set
			{
				ViewState["SelectedDuration"] = value;
			}
		}

		protected string SelectedTimeOfDay
		{
			get
			{
				if (ViewState["SelectedTimeOfDay"] == null)
				{
					return null;
				}
				return (string)ViewState["SelectedTimeOfDay"];
			}
			set
			{
				ViewState["SelectedTimeOfDay"] = value;
			}
		}

		protected Dictionary<int, List<DateTimeRange>> WorkingHoursDictionary
		{
			get
			{
				if (_WorkingHoursDictionary == null)
				{
					_WorkingHoursDictionary = new Dictionary<int, List<DateTimeRange>>();
					if (!ModuleSettings.AllowCategorySelection)
					{
						_WorkingHoursDictionary.Add(0, GetWorkingHoursList(null));
					}
					else
					{
						foreach (CategoryInfo categoryList in CategoryList)
						{
							_WorkingHoursDictionary.Add(categoryList.CategoryID, GetWorkingHoursList(CategorySettingsDictionary[categoryList.CategoryID]));
						}
					}
				}
				return _WorkingHoursDictionary;
			}
		}

		protected void AddModuleMessage(string message, ModuleMessage.ModuleMessageType moduleMessageType)
		{
			Gafware.Modules.Reservations.Helper.AddModuleMessage(this, message, moduleMessageType);
		}

		protected void AddToDateTimeRangeList(List<DateTimeRange> dateTimeRangeList, DateTime startDateTime, DateTime endDateTime)
		{
			DateTimeRange dateTimeRange = FindAndRemoveOverlappingDateTimeRange(dateTimeRangeList, startDateTime, endDateTime);
			if (dateTimeRange == null)
			{
				DateTimeRange dateTimeRange1 = new DateTimeRange()
				{
					StartDateTime = startDateTime,
					EndDateTime = endDateTime
				};
				dateTimeRangeList.Add(dateTimeRange1);
				return;
			}
			dateTimeRange.StartDateTime = (dateTimeRange.StartDateTime < startDateTime ? dateTimeRange.StartDateTime : startDateTime);
			dateTimeRange.EndDateTime = (dateTimeRange.EndDateTime > endDateTime ? dateTimeRange.EndDateTime : endDateTime);
			AddToDateTimeRangeList(dateTimeRangeList, dateTimeRange.StartDateTime, dateTimeRange.EndDateTime);
		}

		protected void ApproveCommandButtonClicked(object sender, EventArgs e)
		{
			try
			{
				Helper.ModifyPendingApprovalStatus(PendingApprovalInfo, PendingApprovalStatus.Approved, UserInfo);
				AddModuleMessage(Localization.GetString("Approved", LocalResourceFile), 0);
				BindPendingApprovalInfo();
				BindPendingApprovalModeration();
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
			}
		}

		protected void AvailableDaysPagerNextCommandButtonClicked(object source, EventArgs e)
		{
			AvailableDaysCurrentPageIndex = AvailableDaysCurrentPageIndex + 1;
			BindAvailableDays();
		}

		protected void AvailableDaysPagerPreviousCommandButtonClicked(object source, EventArgs e)
		{
			AvailableDaysCurrentPageIndex = AvailableDaysCurrentPageIndex - 1;
			if (AvailableDaysCurrentPageIndex < 0)
			{
				AvailableDaysCurrentPageIndex = 0;
			}
			BindAvailableDays();
		}

		protected void AvailableDaysRepeaterItemDataBound(object sender, RepeaterItemEventArgs e)
		{
			if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
			{
				DateTime selectedDate = SelectedDate;
				DateTime dateTime = new DateTime();
				if (selectedDate != dateTime && (DateTime)e.Item.DataItem == SelectedDate)
				{
					e.Item.FindControl("availableDayLinkButton").Visible = false;
					e.Item.FindControl("availableDayLabel").Visible = true;
					return;
				}
				e.Item.FindControl("availableDayLinkButton").Visible = (DateTime)e.Item.DataItem != Null.NullDate;
			}
		}

		protected void AvailableDurationsPagerNextCommandButtonClicked(object source, EventArgs e)
		{
			AvailableDurationsCurrentPageIndex = AvailableDurationsCurrentPageIndex + 1;
			BindAvailableDurations();
		}

		protected void AvailableDurationsPagerPreviousCommandButtonClicked(object source, EventArgs e)
		{
			AvailableDurationsCurrentPageIndex = AvailableDurationsCurrentPageIndex - 1;
			if (AvailableDurationsCurrentPageIndex < 0)
			{
				AvailableDurationsCurrentPageIndex = 0;
			}
			BindAvailableDurations();
		}

		protected void AvailableDurationsRepeaterItemDataBound(object sender, RepeaterItemEventArgs e)
		{
			if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
			{
				TimeSpan selectedDuration = SelectedDuration;
				TimeSpan timeSpan = new TimeSpan();
				if (selectedDuration != timeSpan && (TimeSpan)e.Item.DataItem == SelectedDuration)
				{
					e.Item.FindControl("availableDurationLinkButton").Visible = false;
					e.Item.FindControl("availableDurationLabel").Visible = true;
					return;
				}
				Control control = e.Item.FindControl("availableDurationLinkButton");
				TimeSpan dataItem = (TimeSpan)e.Item.DataItem;
				timeSpan = new TimeSpan();
				control.Visible = dataItem != timeSpan;
			}
		}

		protected void AvailableTimesOfDayPagerNextCommandButtonClicked(object source, EventArgs e)
		{
			AvailableTimesOfDayCurrentPageIndex = AvailableTimesOfDayCurrentPageIndex + 1;
			BindAvailableTimesOfDay();
		}

		protected void AvailableTimesOfDayPagerPreviousCommandButtonClicked(object source, EventArgs e)
		{
			AvailableTimesOfDayCurrentPageIndex = AvailableTimesOfDayCurrentPageIndex - 1;
			if (AvailableTimesOfDayCurrentPageIndex < 0)
			{
				AvailableTimesOfDayCurrentPageIndex = 0;
			}
			BindAvailableTimesOfDay();
		}

		protected void AvailableTimesOfDayRepeaterItemDataBound(object sender, RepeaterItemEventArgs e)
		{
			if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
			{
				if (SelectedTimeOfDay != null && (string)e.Item.DataItem == SelectedTimeOfDay)
				{
					e.Item.FindControl("availableTimeOfDayLinkButton").Visible = false;
					e.Item.FindControl("availableTimeOfDayLabel").Visible = true;
					return;
				}
				if ((string)e.Item.DataItem != Null.NullString && !IsTimeOfDayAvailable(SelectedCategory, SelectedDate, (string)e.Item.DataItem))
				{
					e.Item.FindControl("availableTimeOfDayLinkButton").Visible = false;
					e.Item.FindControl("unavailableTimeOfDayLabel").Visible = true;
					return;
				}
				e.Item.FindControl("availableTimeOfDayLinkButton").Visible = (string)e.Item.DataItem != Null.NullString;
			}
		}

		protected void AvailableTimesPagerNextCommandButtonClicked(object source, EventArgs e)
		{
			AvailableTimesCurrentPageIndex = AvailableTimesCurrentPageIndex + 1;
			BindAvailableTimes();
		}

		protected void AvailableTimesPagerPreviousCommandButtonClicked(object source, EventArgs e)
		{
			AvailableTimesCurrentPageIndex = AvailableTimesCurrentPageIndex - 1;
			if (AvailableTimesCurrentPageIndex < 0)
			{
				AvailableTimesCurrentPageIndex = 0;
			}
			BindAvailableTimes();
		}

		protected void AvailableTimesRepeaterItemDataBound(object sender, RepeaterItemEventArgs e)
		{
			if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
			{
				DateTime selectedDateTime = SelectedDateTime;
				DateTime dateTime = new DateTime();
				if (selectedDateTime != dateTime && (DateTime)e.Item.DataItem == SelectedDateTime)
				{
					e.Item.FindControl("availableTimeLinkButton").Visible = false;
					e.Item.FindControl("availableTimeLabel").Visible = true;
					return;
				}
				e.Item.FindControl("availableTimeLinkButton").Visible = (DateTime)e.Item.DataItem != Null.NullDate;
			}
		}

		protected void BindAvailableDays()
		{
			List<DateTime> availableReservationDays = GetAvailableReservationDays(SelectedCategory);
			if (availableReservationDays.Count != 0)
			{
				HtmlGenericControl htmlGenericControl = availableDaysHorizontalScroll;
				System.Web.UI.WebControls.Calendar calendar = availableDaysCalendar;
				bool displayCalendar = ModuleSettings.DisplayCalendar;
				bool flag = displayCalendar;
				calendar.Visible = displayCalendar;
				htmlGenericControl.Visible = !flag;
				if (ModuleSettings.DisplayCalendar)
				{
					availableDaysCalendar.SelectedDate = SelectedDate;
					System.Web.UI.WebControls.Calendar calendar1 = availableDaysCalendar;
					DateTime selectedDate = SelectedDate;
					DateTime dateTime = new DateTime();
					calendar1.VisibleDate = (selectedDate != dateTime ? SelectedDate : availableReservationDays[0]);
					return;
				}
				availableDaysRepeater.DataSource = availableReservationDays;
				availableDaysRepeater.DataBind();
			}
			else if (!ModuleSettings.AllowCategorySelection || SelectCategoryLast)
			{
				HideAllStepTables();
				AddModuleMessage(Localization.GetString("NoReservations", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
				return;
			}
		}

		protected void BindAvailableDurations()
		{
			TimeSpan selectedDuration;
			int selectedCategory = SelectedCategory;
			DateTime selectedDateTime = SelectedDateTime;
			DateTime dateTime = new DateTime();
			List<TimeSpan> availableDurations = GetAvailableDurations(selectedCategory, (selectedDateTime == dateTime ? SelectedDate : SelectedDateTime));
			HtmlGenericControl htmlGenericControl = durationHorizontalScroll;
			DropDownList dropDownList = durationDropDownList;
			bool flag = false;
			durationListBox.Visible = false;
			bool flag1 = flag;
			bool flag2 = flag1;
			dropDownList.Visible = flag1;
			htmlGenericControl.Visible = flag2;
			if (ModuleSettings.DurationSelectionMode == Gafware.Modules.Reservations.ModuleSettings.SelectionModeEnum.HorizontalScroll)
			{
				availableDurationsRepeater.DataSource = availableDurations;
				availableDurationsRepeater.DataBind();
				durationHorizontalScroll.Visible = true;
				return;
			}
			if (ModuleSettings.DurationSelectionMode != Gafware.Modules.Reservations.ModuleSettings.SelectionModeEnum.DropDownList)
			{
				durationListBox.Items.Clear();
				foreach (TimeSpan availableDuration in availableDurations)
				{
					ListItem listItem = new ListItem(GetFriendlyReservationDuration(availableDuration), availableDuration.ToString());
					durationListBox.Items.Add(listItem);
				}
				TimeSpan timeSpan = SelectedDuration;
				selectedDuration = new TimeSpan();
				if (timeSpan != selectedDuration)
				{
					ListBox str = durationListBox;
					selectedDuration = SelectedDuration;
					str.SelectedValue = selectedDuration.ToString();
				}
				durationListBox.Visible = true;
				return;
			}
			durationDropDownList.Items.Clear();
			foreach (TimeSpan availableDuration1 in availableDurations)
			{
				ListItem listItem1 = new ListItem(GetFriendlyReservationDuration(availableDuration1), availableDuration1.ToString());
				durationDropDownList.Items.Add(listItem1);
			}
			TimeSpan selectedDuration1 = SelectedDuration;
			selectedDuration = new TimeSpan();
			if (selectedDuration1 != selectedDuration)
			{
				DropDownList str1 = durationDropDownList;
				selectedDuration = SelectedDuration;
				str1.SelectedValue = selectedDuration.ToString();
			}
			else
			{
				durationDropDownList.Items.Insert(0, "...");
			}
			durationDropDownList.Visible = true;
		}

		protected void BindAvailableTimes()
		{
			DateTime selectedDateTime;
			List<DateTime> dateTimes = (ModuleSettings.DisplayTimeOfDay ? GetAvailableReservationStartTimes(SelectedCategory, SelectedDate, SelectedTimeOfDay) : GetAvailableReservationStartTimes(SelectedCategory, SelectedDate));
			HtmlGenericControl htmlGenericControl = timeHorizontalScroll;
			DropDownList dropDownList = timeDropDownList;
			bool flag = false;
			timeListBox.Visible = false;
			bool flag1 = flag;
			bool flag2 = flag1;
			dropDownList.Visible = flag1;
			htmlGenericControl.Visible = flag2;
			if (ModuleSettings.TimeSelectionMode == Gafware.Modules.Reservations.ModuleSettings.SelectionModeEnum.HorizontalScroll)
			{
				availableTimesRepeater.DataSource = dateTimes;
				availableTimesRepeater.DataBind();
				timeHorizontalScroll.Visible = true;
				return;
			}
			if (ModuleSettings.TimeSelectionMode != Gafware.Modules.Reservations.ModuleSettings.SelectionModeEnum.DropDownList)
			{
				timeListBox.Items.Clear();
				foreach (DateTime dateTime in dateTimes)
				{
					ListItem listItem = new ListItem(GetFriendlyReservationTime(dateTime), dateTime.ToString());
					timeListBox.Items.Add(listItem);
				}
				DateTime selectedDateTime1 = SelectedDateTime;
				selectedDateTime = new DateTime();
				if (selectedDateTime1 != selectedDateTime)
				{
					ListBox str = timeListBox;
					selectedDateTime = SelectedDateTime;
					str.SelectedValue = selectedDateTime.ToString();
				}
				timeListBox.Visible = true;
				return;
			}
			timeDropDownList.Items.Clear();
			foreach (DateTime dateTime1 in dateTimes)
			{
				ListItem listItem1 = new ListItem(GetFriendlyReservationTime(dateTime1), dateTime1.ToString());
				timeDropDownList.Items.Add(listItem1);
			}
			DateTime selectedDateTime2 = SelectedDateTime;
			selectedDateTime = new DateTime();
			if (selectedDateTime2 != selectedDateTime)
			{
				DropDownList str1 = timeDropDownList;
				selectedDateTime = SelectedDateTime;
				str1.SelectedValue = selectedDateTime.ToString();
			}
			else
			{
				timeDropDownList.Items.Insert(0, "...");
			}
			timeDropDownList.Visible = true;
		}

		protected void BindAvailableTimesOfDay()
		{
			List<string> timesOfDayToRender = GetTimesOfDayToRender(SelectedCategory, SelectedDate);
			HtmlGenericControl htmlGenericControl = timeOfDayHorizontalScroll;
			DropDownList dropDownList = timeOfDayDropDownList;
			bool flag = false;
			timeOfDayListBox.Visible = false;
			bool flag1 = flag;
			bool flag2 = flag1;
			dropDownList.Visible = flag1;
			htmlGenericControl.Visible = flag2;
			if (ModuleSettings.TimeOfDaySelectionMode == Gafware.Modules.Reservations.ModuleSettings.SelectionModeEnum.HorizontalScroll)
			{
				availableTimesOfDayRepeater.DataSource = timesOfDayToRender;
				availableTimesOfDayRepeater.DataBind();
				timeOfDayHorizontalScroll.Visible = true;
				return;
			}
			if (ModuleSettings.TimeOfDaySelectionMode != Gafware.Modules.Reservations.ModuleSettings.SelectionModeEnum.DropDownList)
			{
				timeOfDayListBox.Items.Clear();
				foreach (string empty in timesOfDayToRender)
				{
					ListItem listItem = new ListItem(GetFriendlyTimeOfDay(empty), empty);
					if (IsTimeOfDayAvailable(SelectedCategory, SelectedDate, empty))
					{
						ViewState[string.Concat(empty, ".class")] = string.Empty;
					}
					else
					{
						ViewState[string.Concat(empty, ".class")] = "Gafware_Modules_Reservations_UnavailableListItem";
					}
					timeOfDayListBox.Items.Add(listItem);
				}
				if (!string.IsNullOrEmpty(SelectedTimeOfDay))
				{
					timeOfDayListBox.SelectedValue = SelectedTimeOfDay;
				}
				timeOfDayListBox.Visible = true;
				return;
			}
			timeOfDayDropDownList.Items.Clear();
			foreach (string str in timesOfDayToRender)
			{
				ListItem listItem1 = new ListItem(GetFriendlyTimeOfDay(str), str);
				if (IsTimeOfDayAvailable(SelectedCategory, SelectedDate, str))
				{
					ViewState[string.Concat(str, ".class")] = string.Empty;
				}
				else
				{
					ViewState[string.Concat(str, ".class")] = "Gafware_Modules_Reservations_UnavailableListItem";
				}
				timeOfDayDropDownList.Items.Add(listItem1);
			}
			if (!string.IsNullOrEmpty(SelectedTimeOfDay))
			{
				timeOfDayDropDownList.SelectedValue = SelectedTimeOfDay;
			}
			else
			{
				timeOfDayDropDownList.Items.Insert(0, "...");
			}
			timeOfDayDropDownList.Visible = true;
		}

		protected void BindCategories()
		{
			List<int> categoryIDsToRender = GetCategoryIDsToRender();
			if (!SelectCategoryLast && categoryIDsToRender.Count == 0)
			{
				HideAllStepTables();
				AddModuleMessage(Localization.GetString("NoReservations", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
				return;
			}
			HtmlGenericControl htmlGenericControl = categoriesHorizontalScroll;
			DropDownList dropDownList = categoriesDropDownList;
			bool flag = false;
			categoriesListBox.Visible = false;
			bool flag1 = flag;
			bool flag2 = flag1;
			dropDownList.Visible = flag1;
			htmlGenericControl.Visible = flag2;
			if (ModuleSettings.CategorySelectionMode == Gafware.Modules.Reservations.ModuleSettings.SelectionModeEnum.HorizontalScroll)
			{
				categoriesRepeater.DataSource = categoryIDsToRender;
				categoriesRepeater.DataBind();
				categoriesHorizontalScroll.Visible = true;
				return;
			}
			if (ModuleSettings.CategorySelectionMode != Gafware.Modules.Reservations.ModuleSettings.SelectionModeEnum.DropDownList)
			{
				categoriesListBox.Items.Clear();
				foreach (int empty in categoryIDsToRender)
				{
					ListItem listItem = new ListItem(GetCategoryName(empty), empty.ToString());
					if (ModuleSettings.BindUponCategorySelection || IsCategoryAvailable(empty))
					{
						ViewState[string.Concat(empty, ".class")] = string.Empty;
					}
					else
					{
						ViewState[string.Concat(empty, ".class")] = "Gafware_Modules_Reservations_UnavailableListItem";
					}
					categoriesListBox.Items.Add(listItem);
				}
				if (SelectedCategory != 0)
				{
					categoriesListBox.SelectedValue = SelectedCategory.ToString();
				}
				categoriesListBox.Visible = true;
				return;
			}
			categoriesDropDownList.Items.Clear();
			foreach (int empty1 in categoryIDsToRender)
			{
				ListItem listItem1 = new ListItem(GetCategoryName(empty1), empty1.ToString());
				if (ModuleSettings.BindUponCategorySelection || IsCategoryAvailable(empty1))
				{
					ViewState[string.Concat(empty1, ".class")] = string.Empty;
				}
				else
				{
					ViewState[string.Concat(empty1, ".class")] = "Gafware_Modules_Reservations_UnavailableListItem";
				}
				categoriesDropDownList.Items.Add(listItem1);
			}
			if (SelectedCategory != 0)
			{
				categoriesDropDownList.SelectedValue = SelectedCategory.ToString();
			}
			else
			{
				categoriesDropDownList.Items.Insert(0, "...");
			}
			categoriesDropDownList.Visible = true;
		}

		private void BindCustomFieldTableRowRepeater()
		{
			List<string> strs = new List<string>();
			foreach (CustomFieldDefinitionInfo customFieldDefinitionInfoList in CustomFieldDefinitionInfoList)
			{
				if (strs.Count != 0 && customFieldDefinitionInfoList.AddToPreviousRow)
				{
					continue;
				}
				strs.Add((customFieldDefinitionInfoList.HideLabel ? string.Empty : customFieldDefinitionInfoList.Label));
			}
			customFieldTableRowRepeater.DataSource = strs;
			customFieldTableRowRepeater.DataBind();
		}

		private void BindCustomFieldTableRowRepeater2()
		{
			customFieldTableRowRepeater2.DataSource = CustomFieldDefinitionInfoList;
			customFieldTableRowRepeater2.DataBind();
		}

		protected void BindPendingApprovalInfo()
		{
			step4CategoryLabel2.Text = PendingApprovalInfoCategoryName;
			step4CategoryTableRow.Visible = step4CategoryLabel2.Text != NotAvailable;
			reservationDateTimeLabel.Text = GetReservationDateTime(PendingApprovalInfo);
			step4NameLabel2.Text = PendingApprovalInfo.FullName;
			step4EmailLabel2.Text = PendingApprovalInfo.Email;
			step4PhoneLabel2.Text = (PendingApprovalInfo.Phone != string.Empty ? PendingApprovalInfo.Phone : NotAvailable);
			step4DescriptionLabel.Text = (PendingApprovalInfo.Description != string.Empty ? PendingApprovalInfo.Description : NotAvailable);
			cancelReservationCommandButton.Visible = false;
			rescheduleReservationCommandButton.Visible = false;
			if (IsProfessional)
			{
				BindCustomFieldTableRowRepeater2();
			}
		}

		protected void BindPendingApprovalModeration()
		{
			DateTime createdOnDate;
			if (UserId != Null.NullInteger && Helper.CanModerate(UserId, PendingApprovalInfo.CategoryID))
			{
				lastActionTable.Visible = true;
				if (PendingApprovalInfo.Status == 0)
				{
					lastActionLabel.Text = Localization.GetString("CreatedBy", LocalResourceFile);
					lastActionByDisplayNameLabel.Text = (PendingApprovalInfo.CreatedByDisplayName != Null.NullString ? PendingApprovalInfo.CreatedByDisplayName : PendingApprovalInfo.FullName);
					Label str = lastActionDateLabel;
					createdOnDate = PendingApprovalInfo.CreatedOnDate;
					str.Text = createdOnDate.ToString("f");
					approveCommandButton.Visible = true;
					declineCommandButton.Visible = true;
				}
				else if (PendingApprovalInfo.Status != 1)
				{
					lastActionLabel.Text = Localization.GetString("DeclinedBy", LocalResourceFile);
					lastActionByDisplayNameLabel.Text = PendingApprovalInfo.LastModifiedByDisplayName;
					Label label = lastActionDateLabel;
					createdOnDate = PendingApprovalInfo.LastModifiedOnDate;
					label.Text = createdOnDate.ToString("f");
					approveCommandButton.Visible = false;
					declineCommandButton.Visible = false;
				}
				else
				{
					lastActionLabel.Text = Localization.GetString("ApprovedBy", LocalResourceFile);
					lastActionByDisplayNameLabel.Text = PendingApprovalInfo.LastModifiedByDisplayName;
					Label str1 = lastActionDateLabel;
					createdOnDate = PendingApprovalInfo.LastModifiedOnDate;
					str1.Text = createdOnDate.ToString("f");
					approveCommandButton.Visible = false;
					declineCommandButton.Visible = false;
				}
				doneCommandButton.Visible = false;
				returnCommandButton.Visible = true;
			}
		}

		protected void BindPendingPaymentInfo()
		{
			step4CategoryLabel2.Text = PendingPaymentInfoCategoryName;
			step4CategoryTableRow.Visible = step4CategoryLabel2.Text != NotAvailable;
			reservationDateTimeLabel.Text = GetReservationDateTime(PendingPaymentInfo);
			step4NameLabel2.Text = PendingPaymentInfo.FullName;
			step4EmailLabel2.Text = PendingPaymentInfo.Email;
			step4PhoneLabel2.Text = (PendingPaymentInfo.Phone != string.Empty ? PendingPaymentInfo.Phone : NotAvailable);
			step4DescriptionLabel.Text = (PendingPaymentInfo.Description != string.Empty ? PendingPaymentInfo.Description : NotAvailable);
			cancelReservationCommandButton.Visible = false;
			rescheduleReservationCommandButton.Visible = false;
			if (IsProfessional)
			{
				BindCustomFieldTableRowRepeater2();
			}
		}

		protected void BindReservationInfo()
		{
			bool flag;
			bool allowCancellations = ModuleSettings.AllowCancellations;
			bool allowRescheduling = ModuleSettings.AllowRescheduling;
			bool allowSchedulingAnotherReservation = ModuleSettings.AllowSchedulingAnotherReservation;
			if (ModuleSettings.AllowCategorySelection && ReservationInfo.CategoryID > 0)
			{
				CategorySettings categorySetting = new CategorySettings(PortalId, TabModuleId, ReservationInfo.CategoryID);
				allowCancellations = categorySetting.AllowCancellations;
				allowRescheduling = categorySetting.AllowRescheduling;
			}
			step4CategoryLabel2.Text = (string.IsNullOrEmpty(ReservationInfo.CategoryName) ? NotAvailable : ReservationInfo.CategoryName);
			step4CategoryTableRow.Visible = step4CategoryLabel2.Text != NotAvailable;
			reservationDateTimeLabel.Text = GetReservationDateTime(ReservationInfo);
			step4NameLabel2.Text = ReservationInfo.FullName;
			step4EmailLabel2.Text = ReservationInfo.Email;
			step4PhoneLabel2.Text = (ReservationInfo.Phone != string.Empty ? ReservationInfo.Phone : NotAvailable);
			step4DescriptionLabel.Text = (ReservationInfo.Description != string.Empty ? ReservationInfo.Description : NotAvailable);
			cancelReservationCommandButton.Visible = allowCancellations;
			if (IsProfessional)
			{
				BindCustomFieldTableRowRepeater2();
			}
			_AvailableReservationStartTimesDictionary = new Dictionary<int, List<DateTime>>();
			_ReservationList = null;
			_PendingApprovalList = null;
			if (IsProfessional)
			{
				_PendingPaymentList = null;
			}
			rescheduleReservationCommandButton.Visible = (!allowRescheduling ? false : AreReservationsAvailable);
			if (ReservationInfo.RequireConfirmation)
			{
				LinkButton linkButton = confirmReservationCommandButton;
				if (ReservationInfo.Confirmed)
				{
					flag = false;
				}
				else
				{
					DateTime startDateTime = ReservationInfo.StartDateTime;
					flag = startDateTime.Subtract(ModuleSettings.SendRemindersWhen) < Gafware.Modules.Reservations.Helper.GetNow(ModuleSettings.TimeZone);
				}
				linkButton.Visible = flag;
			}
		}

		protected void CalendarDayRender(object sender, DayRenderEventArgs e)
		{
			List<DateTime> availableReservationDays = GetAvailableReservationDays((SelectCategoryLast ? 0 : SelectedCategory));
			if (e.Day.IsOtherMonth)
			{
				e.Day.IsSelectable = false;
				e.Cell.Text = "";
				e.Cell.CssClass = string.Concat("Gafware_Modules_Reservations_HiddenDayStyle ", e.Cell.CssClass);
				return;
			}
			if (availableReservationDays.Contains(e.Day.Date))
			{
				if (e.Day.IsSelected)
				{
					e.Cell.Text = e.Day.DayNumberText;
				}
				return;
			}
			e.Day.IsSelectable = false;
			e.Cell.CssClass = string.Concat("Gafware_Modules_Reservations_UnavailableDayStyle ", e.Cell.CssClass);
		}

		protected void CalendarPreRender(object sender, EventArgs e)
		{
		}

		protected void CalendarSelectionChanged(object sender, EventArgs e)
		{
			SelectedDate = availableDaysCalendar.SelectedDate;
			SelectedDateChanged();
		}

		protected void CancelReservationCommandButtonClicked(object sender, EventArgs e)
		{
			if (IsProfessional)
			{
				decimal dueAmount = DueAmount;
				decimal cancellationAmount = CancellationAmount;
				if (dueAmount > decimal.Zero)
				{
					Gafware.Modules.Reservations.PendingPaymentInfo now = Gafware.Modules.Reservations.Helper.FindPendingPaymentInfoByEventIDAndStatus((new PendingPaymentController()).GetPendingPaymentList(TabModuleId), ReservationInfo.ReservationID, 7);
					now.Status = 2;
					now.LastModifiedOnDate = DateTime.Now;
					(new PendingPaymentController()).UpdatePendingPayment(now);
					_DuePendingPaymentInfo = null;
					if (cancellationAmount > decimal.Zero)
					{
						Gafware.Modules.Reservations.Helper.AddOrUpdatePendingPaymentInfo(ReservationInfo, TabModuleId, PortalId, cancellationAmount, CancellationRefundableAmount, ModuleSettings.Currency, UserId, 7);
					}
					else if (cancellationAmount < decimal.Zero)
					{
						Gafware.Modules.Reservations.Helper.AddOrUpdatePendingPaymentInfo(ReservationInfo, TabModuleId, PortalId, cancellationAmount, CancellationRefundableAmount, ModuleSettings.Currency, UserId, 5);
						Helper.SendPendingCancellationRefundMail(ReservationInfo, cancellationAmount * decimal.MinusOne, ModuleSettings.Currency);
					}
				}
				else if (cancellationAmount < decimal.Zero)
				{
					Gafware.Modules.Reservations.Helper.AddOrUpdatePendingPaymentInfo(ReservationInfo, TabModuleId, PortalId, cancellationAmount, CancellationRefundableAmount, ModuleSettings.Currency, UserId, 5);
					Helper.SendPendingCancellationRefundMail(ReservationInfo, cancellationAmount * decimal.MinusOne, ModuleSettings.Currency);
				}
			}
			Helper.SendCancellationMail(ReservationInfo);
			ReservationController.DeleteReservation(ReservationInfo.ReservationID);
			cancelReservationCommandButton.Visible = false;
			rescheduleReservationCommandButton.Visible = false;
			confirmReservationCommandButton.Visible = false;
			scheduleAnotherReservationCommandButton.Visible = false;
			AddModuleMessage(Localization.GetString("ReservationCancelled", LocalResourceFile), 0);
			ReservationInfo = new Gafware.Modules.Reservations.ReservationInfo();
		}

		private bool CanScheduleReservation(int categoryID, DateTime reservationTime, TimeSpan reservationDuration)
		{
			int maxConflictingReservations;
			CategorySettings item;
			if (CategorySettingsDictionary.ContainsKey(categoryID))
			{
				item = CategorySettingsDictionary[categoryID];
			}
			else
			{
				item = null;
			}
			CategorySettings categorySetting = item;
			bool flag = false;
			foreach (DateTimeRange dateTimeRange in (IEnumerable)WorkingHoursDictionary[categoryID])
			{
				if (!(dateTimeRange.StartDateTime <= reservationTime) || !(dateTimeRange.EndDateTime >= reservationTime.Add(reservationDuration)))
				{
					if (dateTimeRange.StartDateTime <= reservationTime)
					{
						continue;
					}
					if (flag)
					{
						maxConflictingReservations = ModuleSettings.MaxConflictingReservations;
						if (ModuleSettings.AllowCategorySelection && !ModuleSettings.PreventCrossCategoryConflicts)
						{
							maxConflictingReservations = categorySetting.MaxConflictingReservations;
						}
						if (GetHighestNumberOfConflictingReservations(ComprehensiveReservationList, categoryID, reservationTime, reservationDuration) >= maxConflictingReservations)
						{
							flag = false;
						}
					}
					return flag;
				}
				else
				{
					flag = true;
					if (flag)
					{
						maxConflictingReservations = ModuleSettings.MaxConflictingReservations;
						if (ModuleSettings.AllowCategorySelection && !ModuleSettings.PreventCrossCategoryConflicts)
						{
							maxConflictingReservations = categorySetting.MaxConflictingReservations;
						}
						if (GetHighestNumberOfConflictingReservations(ComprehensiveReservationList, categoryID, reservationTime, reservationDuration) >= maxConflictingReservations)
						{
							flag = false;
						}
					}
					return flag;
				}
			}
			if (flag)
			{
				maxConflictingReservations = ModuleSettings.MaxConflictingReservations;
				if (ModuleSettings.AllowCategorySelection && !ModuleSettings.PreventCrossCategoryConflicts)
				{
					maxConflictingReservations = categorySetting.MaxConflictingReservations;
				}
				if (GetHighestNumberOfConflictingReservations(ComprehensiveReservationList, categoryID, reservationTime, reservationDuration) >= maxConflictingReservations)
				{
					flag = false;
				}
			}
			return flag;
		}

		protected void CashierCommandButtonClicked(object sender, EventArgs e)
		{
			HttpResponse response = Response;
			string[] strArrays = new string[1];
			int moduleId = ModuleId;
			strArrays[0] = string.Concat("mid=", moduleId.ToString());
			response.Redirect(_navigationManager.NavigateURL("Cashier", strArrays));
		}

		protected void CategoriesDropDownListSelectedIndexChanged(object sender, EventArgs e)
		{
			SelectedCategory = int.Parse(categoriesDropDownList.SelectedValue);
			SelectedCategoryChanged();
		}

		protected void CategoriesListBoxSelectedIndexChanged(object sender, EventArgs e)
		{
			SelectedCategory = int.Parse(categoriesListBox.SelectedValue);
			SelectedCategoryChanged();
		}

		protected void CategoriesRepeaterItemDataBound(object sender, RepeaterItemEventArgs e)
		{
			if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
			{
				if (SelectedCategory != 0 && (int)e.Item.DataItem == SelectedCategory)
				{
					e.Item.FindControl("categoryLinkButton").Visible = false;
					e.Item.FindControl("categoryLabel").Visible = true;
					return;
				}
				if ((int)e.Item.DataItem != Null.NullInteger && !ModuleSettings.BindUponCategorySelection && !IsCategoryAvailable((int)e.Item.DataItem))
				{
					e.Item.FindControl("categoryLinkButton").Visible = false;
					e.Item.FindControl("unavailableCategoryLabel").Visible = true;
					return;
				}
				e.Item.FindControl("categoryLinkButton").Visible = (int)e.Item.DataItem != Null.NullInteger;
			}
		}

		protected void CategoryLinkButtonClicked(object sender, CommandEventArgs e)
		{
			SelectedCategory = int.Parse((string)e.CommandArgument);
			SelectedCategoryChanged();
		}

		private List<CustomFieldValueInfo> CollectCustomFieldValues()
		{
			string shortDateString;
			bool flag;
			List<CustomFieldValueInfo> customFieldValueInfos = new List<CustomFieldValueInfo>();
			int num = -1;
			int num1 = 0;
			foreach (CustomFieldDefinitionInfo customFieldDefinitionInfoList in CustomFieldDefinitionInfoList)
			{
				if (num == -1 || !customFieldDefinitionInfoList.AddToPreviousRow)
				{
					num++;
					num1 = 0;
				}
				else if (num != -1)
				{
					num1++;
				}
				CustomFieldValueInfo customFieldValueInfo = new CustomFieldValueInfo()
				{
					CustomFieldDefinitionID = customFieldDefinitionInfoList.CustomFieldDefinitionID
				};
				Repeater repeater = (Repeater)customFieldTableRowRepeater.Items[num].FindControl("customFieldTableCellRepeater");
				if (customFieldDefinitionInfoList.CustomFieldDefinitionType == CustomFieldDefinitionType.TextBox)
				{
					customFieldValueInfo.Value = ((TextBox)repeater.Items[num1].FindControl("textBox")).Text.Trim();
				}
				else if (customFieldDefinitionInfoList.CustomFieldDefinitionType == CustomFieldDefinitionType.CheckBox)
				{
					bool @checked = ((CheckBox)repeater.Items[num1].FindControl("checkBox")).Checked;
					customFieldValueInfo.Value = @checked.ToString();
				}
				else if (customFieldDefinitionInfoList.CustomFieldDefinitionType == CustomFieldDefinitionType.DropDownList)
				{
					customFieldValueInfo.Value = ((DropDownList)repeater.Items[num1].FindControl("dropDownList")).SelectedValue;
				}
				else if (customFieldDefinitionInfoList.CustomFieldDefinitionType == CustomFieldDefinitionType.RadioButtonList)
				{
					customFieldValueInfo.Value = ((RadioButtonList)repeater.Items[num1].FindControl("radioButtonList")).SelectedValue;
				}
				else if (customFieldDefinitionInfoList.CustomFieldDefinitionType == CustomFieldDefinitionType.CheckBoxList)
				{
					foreach (ListItem item in ((CheckBoxList)repeater.Items[num1].FindControl("checkBoxList")).Items)
					{
						if (!item.Selected)
						{
							continue;
						}
						CustomFieldValueInfo customFieldValueInfo1 = customFieldValueInfo;
						customFieldValueInfo1.Value = string.Concat(customFieldValueInfo1.Value, (string.IsNullOrEmpty(customFieldValueInfo.Value) ? string.Empty : ", "), item.Value);
					}
				}
				else if (customFieldDefinitionInfoList.CustomFieldDefinitionType == CustomFieldDefinitionType.ListBox)
				{
					foreach (ListItem listItem in ((ListBox)repeater.Items[num1].FindControl("listBox")).Items)
					{
						if (!listItem.Selected)
						{
							continue;
						}
						CustomFieldValueInfo customFieldValueInfo2 = customFieldValueInfo;
						customFieldValueInfo2.Value = string.Concat(customFieldValueInfo2.Value, (string.IsNullOrEmpty(customFieldValueInfo.Value) ? string.Empty : ", "), listItem.Value);
					}
				}
				if (customFieldDefinitionInfoList.CustomFieldDefinitionType == CustomFieldDefinitionType.Date)
				{
					DateTime? selectedDate = null;
					DateTime dateTime;
					if (DateTime.TryParse(((HtmlInputGenericControl)repeater.Items[num1].FindControl("datePicker")).Value, out dateTime))
					{
						selectedDate = dateTime;
					}
					CustomFieldValueInfo customFieldValueInfo3 = customFieldValueInfo;
					if (selectedDate.HasValue)
					{
						DateTime? nullable = selectedDate;
						DateTime value = new DateTime();
						if (nullable.HasValue)
						{
							flag = (nullable.HasValue ? nullable.GetValueOrDefault() == value : true);
						}
						else
						{
							flag = false;
						}
						if (flag)
						{
							goto Label1;
						}
						value = selectedDate.Value;
						shortDateString = value.ToShortDateString();
						goto Label0;
					}
				Label1:
					shortDateString = string.Empty;
				Label0:
					customFieldValueInfo3.Value = shortDateString;
				}
				customFieldValueInfo.CreatedByUserID = UserId;
				customFieldValueInfo.CreatedOnDate = DateTime.Now;
				customFieldValueInfos.Add(customFieldValueInfo);
			}
			return customFieldValueInfos;
		}

		protected void Confirm(bool pay)
		{
			bool flag;
			Hashtable settings;
			bool flag1 = false;
			TimeSpan selectedDuration = SelectedDuration;
			TimeSpan timeSpan = new TimeSpan();
			if (selectedDuration != timeSpan)
			{
				TimeSpan selectedDuration1 = SelectedDuration;
			}
			else
			{
				TimeSpan minReservationDuration = MinReservationDuration;
			}
			if (!SelectCategoryLast)
			{
				TimeSpan timeSpan1 = SelectedDuration;
				timeSpan = new TimeSpan();
				if (timeSpan1 == timeSpan)
				{
					DateTime selectedDateTime = SelectedDateTime;
					DateTime dateTime = new DateTime();
					flag1 = (selectedDateTime != dateTime ? SelectedTimeChanged() : SelectedDateChanged());
				}
				else
				{
					flag1 = SelectedDurationChanged();
				}
			}
			else
			{
				flag1 = SelectedCategoryChanged();
			}
			if (flag1)
			{
				bool reservationID = ReservationInfo.ReservationID == Null.NullInteger;
				PopulateEventInfoFromInput();
				List<CustomFieldValueInfo> customFieldValueInfos = null;
				if (IsProfessional)
				{
					customFieldValueInfos = CollectCustomFieldValues();
				}
				bool allowCancellations = ModuleSettings.AllowCancellations;
				bool allowRescheduling = ModuleSettings.AllowRescheduling;
				bool moderate = ModuleSettings.Moderate;
				int maxReservationsPerUser = ModuleSettings.MaxReservationsPerUser;
				if (ModuleSettings.AllowCategorySelection)
				{
					bool allowCancellations1 = SelectedCategorySettings.AllowCancellations;
					bool allowRescheduling1 = SelectedCategorySettings.AllowRescheduling;
					moderate = SelectedCategorySettings.Moderate;
					maxReservationsPerUser = SelectedCategorySettings.MaxReservationsPerUser;
				}
				if (reservationID && maxReservationsPerUser != Null.NullInteger)
				{
					int num = 0;
					foreach (Gafware.Modules.Reservations.ReservationInfo comprehensiveReservationList in ComprehensiveReservationList)
					{
						if (string.IsNullOrEmpty(comprehensiveReservationList.Email) || !(comprehensiveReservationList.Email == ReservationInfo.Email))
						{
							continue;
						}
						if (comprehensiveReservationList.CategoryID != SelectedCategory)
						{
							if (comprehensiveReservationList.CategoryID != (SelectedCategory == 0 ? -1 : SelectedCategory))
							{
								continue;
							}
						}
						num++;
					}
					if (num >= maxReservationsPerUser)
					{
						AddModuleMessage(Localization.GetString("MaxReservationsPerUserReached", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
						return;
					}
				}
				if (!moderate)
				{
					flag = false;
				}
				else
				{
					Gafware.Modules.Reservations.Helper helper = Helper;
					if (SelectedCategorySettings != null)
					{
						settings = SelectedCategorySettings.Settings;
					}
					else
					{
						settings = null;
					}
					flag = helper.MustModerate(settings, ModuleSettings.Settings, ReservationInfo.StartDateTime, ReservationInfo.Duration);
				}
				moderate = flag;
				decimal amount = new decimal();
				decimal refundableAmount = new decimal();
				decimal dueAmount = new decimal();
				if (IsProfessional)
				{
					amount = Amount;
					refundableAmount = RefundableAmount;
					dueAmount = DueAmount;
					if (pay && amount > decimal.Zero)
					{
						Gafware.Modules.Reservations.PendingPaymentInfo pendingPaymentInfo = (new PendingPaymentController()).AddPendingPayment(Gafware.Modules.Reservations.Helper.CreatePendingPaymentInfoFromEventInfo(ReservationInfo, TabModuleId, PortalId, amount - dueAmount, refundableAmount, ModuleSettings.Currency, UserId, 0));
						if (ReservationInfo.ReservationID == Null.NullInteger)
						{
							CustomFieldValueController customFieldValueController = new CustomFieldValueController();
							foreach (CustomFieldValueInfo pendingPaymentID in customFieldValueInfos)
							{
								pendingPaymentID.PendingPaymentID = pendingPaymentInfo.PendingPaymentID;
								customFieldValueController.AddCustomFieldValue(pendingPaymentID);
							}
						}
						MakePayment(pendingPaymentInfo, amount);
						return;
					}
				}
				if (moderate && (UserId == Null.NullInteger || !Helper.CanModerate(UserId, SelectedCategory)))
				{
					PendingApprovalInfo = (new PendingApprovalController()).AddPendingApproval(Gafware.Modules.Reservations.Helper.CreatePendingApprovalInfoFromEventInfo(ReservationInfo, TabModuleId, PortalId, UserId, 0));
					if (IsProfessional)
					{
						if (amount != decimal.Zero)
						{
							(new PendingPaymentController()).AddPendingPayment(Gafware.Modules.Reservations.Helper.CreatePendingPaymentInfoFromPendingApprovalInfo(PendingApprovalInfo, amount, refundableAmount, ModuleSettings.Currency, PendingPaymentStatus.PendingApproval));
						}
						if (ReservationInfo.ReservationID == Null.NullInteger)
						{
							CustomFieldValueController customFieldValueController1 = new CustomFieldValueController();
							foreach (CustomFieldValueInfo pendingApprovalID in customFieldValueInfos)
							{
								pendingApprovalID.PendingApprovalID = PendingApprovalInfo.PendingApprovalID;
								customFieldValueController1.AddCustomFieldValue(pendingApprovalID);
							}
						}
					}
					AddModuleMessage(Localization.GetString("ReservationPendingApproval", LocalResourceFile), 0);
					Helper.SendModeratorMail(PendingApprovalInfo);
					HideAllStepTables();
					step4Table.Visible = true;
					BindPendingApprovalInfo();
					ReservationInfo = new Gafware.Modules.Reservations.ReservationInfo();
					return;
				}
				ReservationInfo = ReservationController.SaveReservation(ReservationInfo);
				if (IsProfessional)
				{
					if (reservationID)
					{
						CustomFieldValueController customFieldValueController2 = new CustomFieldValueController();
						foreach (CustomFieldValueInfo customFieldValueInfo in customFieldValueInfos)
						{
							customFieldValueInfo.ReservationID = ReservationInfo.ReservationID;
							customFieldValueController2.AddCustomFieldValue(customFieldValueInfo);
						}
					}
					if (!reservationID)
					{
						Gafware.Modules.Reservations.Helper.UpdateDueAndPendingRefundPaymentInfoFromEventInfo(ReservationInfo, TabModuleId);
					}
					if (amount < decimal.Zero)
					{
						PendingPaymentController pendingPaymentController = new PendingPaymentController();
						Gafware.Modules.Reservations.PendingPaymentInfo now = null;
						if (!reservationID)
						{
							now = Gafware.Modules.Reservations.Helper.FindPendingPaymentInfoByEventIDAndStatus(pendingPaymentController.GetPendingPaymentList(TabModuleId), ReservationInfo.ReservationID, 5);
						}
						if (now == null)
						{
							now = pendingPaymentController.AddPendingPayment(Gafware.Modules.Reservations.Helper.CreatePendingPaymentInfoFromEventInfo(ReservationInfo, TabModuleId, PortalId, amount, refundableAmount, ModuleSettings.Currency, UserId, 5));
						}
						else
						{
							Gafware.Modules.Reservations.PendingPaymentInfo amount1 = now;
							amount1.Amount = amount1.Amount + amount;
							Gafware.Modules.Reservations.PendingPaymentInfo refundableAmount1 = now;
							refundableAmount1.RefundableAmount = refundableAmount1.RefundableAmount + refundableAmount;
							now.LastModifiedOnDate = DateTime.Now;
							pendingPaymentController.UpdatePendingPayment(now);
						}
						Helper.SendPendingRescheduleRefundMail(ReservationInfo, amount * decimal.MinusOne, ModuleSettings.Currency);
					}
					else if (amount > decimal.Zero)
					{
						PendingPaymentController pendingPaymentController1 = new PendingPaymentController();
						Gafware.Modules.Reservations.PendingPaymentInfo now1 = null;
						if (!reservationID)
						{
							now1 = Gafware.Modules.Reservations.Helper.FindPendingPaymentInfoByEventIDAndStatus(pendingPaymentController1.GetPendingPaymentList(TabModuleId), ReservationInfo.ReservationID, 7);
						}
						if (now1 == null)
						{
							now1 = pendingPaymentController1.AddPendingPayment(Gafware.Modules.Reservations.Helper.CreatePendingPaymentInfoFromEventInfo(ReservationInfo, TabModuleId, PortalId, amount, refundableAmount, ModuleSettings.Currency, UserId, 7));
						}
						else
						{
							Gafware.Modules.Reservations.PendingPaymentInfo pendingPaymentInfo1 = now1;
							pendingPaymentInfo1.Amount = pendingPaymentInfo1.Amount + (amount - dueAmount);
							Gafware.Modules.Reservations.PendingPaymentInfo refundableAmount2 = now1;
							refundableAmount2.RefundableAmount = refundableAmount2.RefundableAmount + refundableAmount;
							now1.LastModifiedOnDate = DateTime.Now;
							pendingPaymentController1.UpdatePendingPayment(now1);
							_DuePendingPaymentInfo = null;
						}
					}
				}
				HideAllStepTables();
				step4Table.Visible = true;
				AddModuleMessage(Localization.GetString("ReservationScheduled", LocalResourceFile), 0);
				if (!reservationID)
				{
					Helper.SendRescheduledMail(ReservationInfo);
				}
				else
				{
					Helper.SendConfirmationMail(ReservationInfo);
				}
				DisplayScheduleAnotherReservation();
				BindReservationInfo();
			}
		}

		protected void ConfirmReservationCommandButtonClicked(object sender, EventArgs e)
		{
			ReservationInfo.Confirmed = true;
			ReservationController.UpdateReservation(ReservationInfo);
			AddModuleMessage(Localization.GetString("ReservationConfirmed", LocalResourceFile), 0);
			confirmReservationCommandButton.Visible = false;
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

		protected void CustomFieldTableCellRepeater_ItemDataBound(object sender, RepeaterItemEventArgs e)
		{
			DateTime dateTime;
			if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
			{
				double num = 100 / (double)_NumberOfDivs;
				HtmlGenericControl label = (HtmlGenericControl)e.Item.FindControl("customFieldLabelTableCell");
				label.Style.Add("width", string.Concat(num, "%"));
				((HtmlGenericControl)e.Item.FindControl("customFieldTableCell")).Style.Add("width", string.Concat(num, "%"));
				CustomFieldDefinitionInfo dataItem = (CustomFieldDefinitionInfo)e.Item.DataItem;
				string customFieldValue = GetCustomFieldValue(dataItem, false);
				label.Visible = (e.Item.ItemIndex == 0 ? false : !dataItem.HideLabel);
				if (label.Visible)
				{
					((Label)label.FindControl("label")).Text = dataItem.Label;
				}
				CustomValidator isRequired = (CustomValidator)e.Item.FindControl("requiredFieldValidator");
				isRequired.Visible = dataItem.IsRequired;
				if (dataItem.CustomFieldDefinitionType == CustomFieldDefinitionType.TextBox)
				{
					TextBox textBox = (TextBox)e.Item.FindControl("textBox");
					textBox.Visible = true;
					textBox.Text = customFieldValue;
					if (!string.IsNullOrEmpty(dataItem.OptionType))
					{
						textBox.Rows = int.Parse(dataItem.OptionType);
						if (textBox.Rows != 1)
						{
							textBox.TextMode = TextBoxMode.MultiLine;
						}
					}
					textBox.Attributes.Add("placeholder", dataItem.Title);
					if (dataItem.IsRequired)
					{
						textBox.CssClass = string.Concat(textBox.CssClass, " Gafware_Modules_Reservations_Required");
					}
					isRequired.ControlToValidate = textBox.ID;
					return;
				}
				if (dataItem.CustomFieldDefinitionType == CustomFieldDefinitionType.CheckBox)
				{
					CheckBox title = (CheckBox)e.Item.FindControl("checkBox");
					title.Visible = true;
					if (!string.IsNullOrEmpty(customFieldValue))
					{
						title.Checked = bool.Parse(customFieldValue);
					}
					title.Text = dataItem.Title;
					return;
				}
				if (dataItem.CustomFieldDefinitionType == CustomFieldDefinitionType.Date)
				{
					HtmlInputGenericControl datePicker = (HtmlInputGenericControl)e.Item.FindControl("datePicker");
					datePicker.Visible = true;
					if (DateTime.TryParse(customFieldValue, out dateTime))
					{
						datePicker.Value = dateTime.ToString("yyyy-MM-dd");
					}
					if (dataItem.IsRequired)
					{
						datePicker.Attributes["class"] = string.Concat(datePicker.Attributes["class"], " Gafware_Modules_Reservations_Required");
					}
					isRequired.ControlToValidate = datePicker.ID;
					return;
				}
				List<CustomFieldDefinitionListItemInfo> customFieldDefinitionListItemList = (new CustomFieldDefinitionListItemController()).GetCustomFieldDefinitionListItemList(dataItem.CustomFieldDefinitionID);
				customFieldDefinitionListItemList.Sort((CustomFieldDefinitionListItemInfo x, CustomFieldDefinitionListItemInfo y) => x.SortOrder.CompareTo(y.SortOrder));
				if (dataItem.CustomFieldDefinitionType == CustomFieldDefinitionType.DropDownList)
				{
					DropDownList dropDownList = (DropDownList)e.Item.FindControl("dropDownList");
					dropDownList.Visible = true;
					if (!string.IsNullOrEmpty(customFieldValue) && customFieldDefinitionListItemList.Count<CustomFieldDefinitionListItemInfo>((CustomFieldDefinitionListItemInfo item) => item.Value == customFieldValue) != 0)
					{
						dropDownList.SelectedValue = customFieldValue;
					}
					isRequired.ControlToValidate = dropDownList.ID;
					dropDownList.DataSource = customFieldDefinitionListItemList;
					dropDownList.DataTextField = "Text";
					dropDownList.DataValueField = "Value";
					dropDownList.DataBind();
					if (!string.IsNullOrEmpty(dataItem.Title))
					{
						dropDownList.Items.Insert(0, new ListItem(dataItem.Title, string.Empty));
					}
					else if (!dataItem.IsRequired)
					{
						dropDownList.Items.Insert(0, new ListItem(string.Empty, string.Empty));
					}
					if (dataItem.IsRequired)
					{
						dropDownList.CssClass = string.Concat(dropDownList.CssClass, " Gafware_Modules_Reservations_Required");
						return;
					}
				}
				else if (dataItem.CustomFieldDefinitionType == CustomFieldDefinitionType.ListBox)
				{
					ListBox numberOfRows = (ListBox)e.Item.FindControl("listBox");
					numberOfRows.Visible = true;
					numberOfRows.Rows = dataItem.NumberOfRows;
					numberOfRows.SelectionMode = (dataItem.IsMultiSelect ? ListSelectionMode.Multiple : ListSelectionMode.Single);
					isRequired.ControlToValidate = numberOfRows.ID;
					numberOfRows.DataSource = customFieldDefinitionListItemList;
					numberOfRows.DataTextField = "Text";
					numberOfRows.DataValueField = "Value";
					numberOfRows.DataBind();
					if (!string.IsNullOrEmpty(customFieldValue))
					{
						List<string> strs = new List<string>(customFieldValue.Split(",".ToCharArray()));
						foreach (ListItem listItem in numberOfRows.Items)
						{
							if (strs.Count<string>((string item) => item.Trim() == listItem.Value) == 0)
							{
								continue;
							}
							listItem.Selected = true;
						}
					}
					if (dataItem.IsRequired)
					{
						ListBox listBox = numberOfRows;
						listBox.CssClass = string.Concat(listBox.CssClass, " Gafware_Modules_Reservations_Required");
						return;
					}
				}
				else if (dataItem.CustomFieldDefinitionType == CustomFieldDefinitionType.RadioButtonList)
				{
					RadioButtonList repeatDirection = (RadioButtonList)e.Item.FindControl("radioButtonList");
					repeatDirection.Visible = true;
					if (!string.IsNullOrEmpty(customFieldValue) && customFieldDefinitionListItemList.Count<CustomFieldDefinitionListItemInfo>((CustomFieldDefinitionListItemInfo item) => item.Value == customFieldValue) != 0)
					{
						repeatDirection.SelectedValue = customFieldValue;
					}
					repeatDirection.RepeatDirection = dataItem.RepeatDirection;
					repeatDirection.RepeatLayout = RepeatLayout.Flow;
					isRequired.ControlToValidate = repeatDirection.ID;
					repeatDirection.DataSource = customFieldDefinitionListItemList;
					repeatDirection.DataTextField = "Text";
					repeatDirection.DataValueField = "Value";
					repeatDirection.DataBind();
					if (dataItem.IsRequired)
					{
						RadioButtonList radioButtonList = repeatDirection;
						radioButtonList.CssClass = string.Concat(radioButtonList.CssClass, " Gafware_Modules_Reservations_Required");
						return;
					}
				}
				else if (dataItem.CustomFieldDefinitionType == CustomFieldDefinitionType.CheckBoxList)
				{
					CheckBoxList checkBoxList = (CheckBoxList)e.Item.FindControl("checkBoxList");
					checkBoxList.Visible = true;
					checkBoxList.RepeatDirection = dataItem.RepeatDirection;
					checkBoxList.RepeatLayout = RepeatLayout.Flow;
					checkBoxList.DataSource = customFieldDefinitionListItemList;
					checkBoxList.DataTextField = "Text";
					checkBoxList.DataValueField = "Value";
					checkBoxList.DataBind();
					if (!string.IsNullOrEmpty(customFieldValue))
					{
						List<string> strs1 = new List<string>(customFieldValue.Split(",".ToCharArray()));
						foreach (ListItem listItem1 in checkBoxList.Items)
						{
							if (strs1.Count<string>((string item) => item.Trim() == listItem1.Value) == 0)
							{
								continue;
							}
							listItem1.Selected = true;
						}
					}
				}
			}
		}

		protected void CustomFieldTableRowRepeater_ItemDataBound(object sender, RepeaterItemEventArgs e)
		{
			_NumberOfDivs = 1;
			if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
			{
				int num = 0;
				int num1 = -1;
				List<CustomFieldDefinitionInfo> customFieldDefinitionInfos = new List<CustomFieldDefinitionInfo>();
				foreach (CustomFieldDefinitionInfo customFieldDefinitionInfoList in CustomFieldDefinitionInfoList)
				{
					if (num == 0 || !customFieldDefinitionInfoList.AddToPreviousRow)
					{
						num1++;
					}
					if (num1 == e.Item.ItemIndex)
					{
						customFieldDefinitionInfos.Add(customFieldDefinitionInfoList);
					}
					else if (num1 > e.Item.ItemIndex)
					{
						break;
					}
					num++;
				}
				num = 0;
				foreach (CustomFieldDefinitionInfo customFieldDefinitionInfo in customFieldDefinitionInfos)
				{
					if (num != 0)
					{
						if (!customFieldDefinitionInfo.HideLabel)
						{
							_NumberOfDivs = _NumberOfDivs + 1;
						}
						_NumberOfDivs = _NumberOfDivs + 1;
					}
					else
					{
						num++;
					}
				}
				Repeater repeater = (Repeater)e.Item.FindControl("customFieldTableCellRepeater");
				repeater.DataSource = customFieldDefinitionInfos;
				repeater.DataBind();
			}
		}

		protected void DateLinkButtonClicked(object sender, CommandEventArgs e)
		{
			SelectedDate = DateTime.Parse((string)e.CommandArgument);
			SelectedDateChanged();
		}

		protected void DateTimeLinkButtonClicked(object sender, CommandEventArgs e)
		{
			SelectedDateTime = DateTime.Parse((string)e.CommandArgument);
			SelectedTimeChanged();
		}

		protected void DeclineCommandButtonClicked(object sender, EventArgs e)
		{
			try
			{
				Helper.ModifyPendingApprovalStatus(PendingApprovalInfo, PendingApprovalStatus.Declined, UserInfo);
				AddModuleMessage(Localization.GetString("Declined", LocalResourceFile), 0);
				BindPendingApprovalInfo();
				BindPendingApprovalModeration();
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
			}
		}

		private void DisplayScheduleAnotherReservation()
		{
			scheduleAnotherReservationCommandButton.Visible = (!ModuleSettings.AllowSchedulingAnotherReservation ? false : AreReservationsAvailable);
		}

		protected void DoneCommandButtonClicked(object sender, EventArgs e)
		{
			if (ModuleSettings.RedirectUrl == string.Empty)
			{
				Response.Redirect(_navigationManager.NavigateURL());
				return;
			}
			Response.Redirect(ModuleSettings.RedirectUrl);
		}

		protected void DuplicateReservationsCommandButtonClicked(object sender, EventArgs e)
		{
			HttpResponse response = Response;
			string[] strArrays = new string[2];
			int moduleId = ModuleId;
			strArrays[0] = string.Concat("mid=", moduleId.ToString());
			strArrays[1] = "List=DuplicateReservationsListSettings";
			response.Redirect(_navigationManager.NavigateURL("DuplicateReservations", strArrays));
		}

		protected void DurationDropDownListSelectedIndexChanged(object sender, EventArgs e)
		{
			SelectedDuration = TimeSpan.Parse(durationDropDownList.SelectedValue);
			SelectedDurationChanged();
		}

		protected void DurationLinkButtonClicked(object sender, CommandEventArgs e)
		{
			SelectedDuration = TimeSpan.Parse((string)e.CommandArgument);
			SelectedDurationChanged();
		}

		protected void DurationListBoxSelectedIndexChanged(object sender, EventArgs e)
		{
			SelectedDuration = TimeSpan.Parse(durationListBox.SelectedValue);
			SelectedDurationChanged();
		}

		protected DateTimeRange FindAndRemoveOverlappingDateTimeRange(List<DateTimeRange> dateTimeRangeList, DateTime startDateTime, DateTime endDateTime)
		{
			DateTimeRange item;
			int num = -1;
			int num1 = 0;
			foreach (DateTimeRange dateTimeRange in dateTimeRangeList)
			{
				if (!WorkingHoursOverlap(startDateTime, endDateTime, dateTimeRange.StartDateTime, dateTimeRange.EndDateTime))
				{
					num1++;
				}
				else
				{
					num = num1;
					item = null;
					if (num != -1)
					{
						item = dateTimeRangeList[num];
						dateTimeRangeList.RemoveAt(num);
					}
					return item;
				}
			}
			item = null;
			if (num != -1)
			{
				item = dateTimeRangeList[num];
				dateTimeRangeList.RemoveAt(num);
			}
			return item;
		}

		private Gafware.Modules.Reservations.PendingApprovalInfo FindByEventID(List<Gafware.Modules.Reservations.PendingApprovalInfo> pendingApprovalInfoList, int eventID)
		{
			List<Gafware.Modules.Reservations.PendingApprovalInfo>.Enumerator enumerator = pendingApprovalInfoList.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Gafware.Modules.Reservations.PendingApprovalInfo current = enumerator.Current;
					if (current.ReservationID != eventID)
					{
						continue;
					}
					return current;
				}
			}
			finally
			{
				((IDisposable)enumerator).Dispose();
			}
			return null;
		}

		private Gafware.Modules.Reservations.PendingPaymentInfo FindByEventID(List<Gafware.Modules.Reservations.PendingPaymentInfo> pendingPaymentInfoList, int eventID)
		{
			List<Gafware.Modules.Reservations.PendingPaymentInfo>.Enumerator enumerator = pendingPaymentInfoList.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Gafware.Modules.Reservations.PendingPaymentInfo current = enumerator.Current;
					if (current.ReservationID != eventID)
					{
						continue;
					}
					return current;
				}
			}
			finally
			{
				((IDisposable)enumerator).Dispose();
			}
			return null;
		}

		private Gafware.Modules.Reservations.PendingPaymentInfo FindByEventIDAndStatus(List<Gafware.Modules.Reservations.PendingPaymentInfo> pendingPaymentInfoList, int eventID, PendingPaymentStatus status)
		{
			List<Gafware.Modules.Reservations.PendingPaymentInfo>.Enumerator enumerator = pendingPaymentInfoList.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Gafware.Modules.Reservations.PendingPaymentInfo current = enumerator.Current;
					if (current.ReservationID != eventID || current.Status != (int)status)
					{
						continue;
					}
					return current;
				}
			}
			finally
			{
				((IDisposable)enumerator).Dispose();
			}
			return null;
		}

		private CategoryInfo FindCategoryInfoByCategoryID(int categoryID)
		{
			List<CategoryInfo>.Enumerator enumerator = CategoryList.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					CategoryInfo current = enumerator.Current;
					if (current.CategoryID != categoryID)
					{
						continue;
					}
					return current;
				}
			}
			finally
			{
				((IDisposable)enumerator).Dispose();
			}
			return null;
		}

		public IEnumerable<Control> FindChildControlsByType(Control control, Type type)
		{
			IEnumerable<Control> controls = control.Controls.Cast<Control>();
			return
				from c in controls.SelectMany<Control, Control>((Control ctrl) => FindChildControlsByType(ctrl, type)).Concat<Control>(controls)
				where c.GetType() == type
				select c;
		}

		private List<CategoryInfo> GetAvailableCategories(DateTime date)
		{
			List<CategoryInfo> categoryInfos = new List<CategoryInfo>();
			foreach (CategoryInfo categoryList in CategoryList)
			{
				if (GetAvailableReservationStartTimes(categoryList.CategoryID, date).Count <= 0)
				{
					continue;
				}
				categoryInfos.Add(categoryList);
			}
			return categoryInfos;
		}

		private List<CategoryInfo> GetAvailableCategories(DateTime date, TimeSpan time)
		{
			List<CategoryInfo> categoryInfos = new List<CategoryInfo>();
			foreach (CategoryInfo categoryList in CategoryList)
			{
				if (!GetAvailableReservationStartTimes(categoryList.CategoryID).Contains(date.Date.Add(time)))
				{
					continue;
				}
				categoryInfos.Add(categoryList);
			}
			return categoryInfos;
		}

		private List<CategoryInfo> GetAvailableCategories(DateTime date, TimeSpan time, TimeSpan duration)
		{
			List<CategoryInfo> categoryInfos = new List<CategoryInfo>();
			foreach (CategoryInfo categoryList in CategoryList)
			{
				if (!GetAvailableDurations(categoryList.CategoryID, date.Date.Add(time)).Contains(duration))
				{
					continue;
				}
				categoryInfos.Add(categoryList);
			}
			return categoryInfos;
		}

		private List<TimeSpan> GetAvailableDurations(int categoryID, DateTime dateTime)
		{
			List<TimeSpan> timeSpans = new List<TimeSpan>();
			if (ModuleSettings.AllowCategorySelection && categoryID == 0)
			{
				foreach (CategoryInfo availableCategory in GetAvailableCategories(dateTime.Date, dateTime.TimeOfDay))
				{
					timeSpans.AddRange(GetAvailableDurations(availableCategory.CategoryID, dateTime));
				}
				timeSpans = timeSpans.Distinct<TimeSpan>().ToList<TimeSpan>();
				timeSpans.Sort();
				return timeSpans;
			}
			TimeSpan reservationDuration = ModuleSettings.ReservationDuration;
			TimeSpan reservationDurationMax = ModuleSettings.ReservationDurationMax;
			TimeSpan reservationDurationInterval = ModuleSettings.ReservationDurationInterval;
			if (ModuleSettings.AllowCategorySelection)
			{
				reservationDuration = CategorySettingsDictionary[categoryID].ReservationDuration;
				reservationDurationMax = CategorySettingsDictionary[categoryID].ReservationDurationMax;
				reservationDurationInterval = CategorySettingsDictionary[categoryID].ReservationDurationInterval;
			}
			for (TimeSpan i = reservationDuration; i <= reservationDurationMax && CanScheduleReservation(categoryID, dateTime, i); i = i.Add(reservationDurationInterval))
			{
				timeSpans.Add(i);
			}
			return timeSpans;
		}

		private List<DateTime> GetAvailableReservationDays(int categoryID)
		{
			if (_AvailableReservationDays.ContainsKey(categoryID))
			{
				return _AvailableReservationDays[categoryID];
			}
			List<DateTime> dateTimes = new List<DateTime>();
			foreach (DateTime availableReservationStartTime in GetAvailableReservationStartTimes(categoryID))
			{
				if (dateTimes.Contains(availableReservationStartTime.Date))
				{
					continue;
				}
				dateTimes.Add(availableReservationStartTime.Date);
			}
			_AvailableReservationDays.Add(categoryID, dateTimes);
			return dateTimes;
		}

		protected string GetAvailableReservationsCountToRender(int categoryID)
		{
			if (categoryID == Null.NullInteger)
			{
				return Null.NullString;
			}
			return string.Concat("(", GetAvailableReservationStartTimes(categoryID).Count, ")");
		}

		private List<DateTime> GetAvailableReservationStartTimes(int categoryID)
		{
			if (_AvailableReservationStartTimesDictionary.ContainsKey(categoryID))
			{
				return _AvailableReservationStartTimesDictionary[categoryID];
			}
			List<DateTime> dateTimes = new List<DateTime>();
			if (ModuleSettings.AllowCategorySelection && categoryID == 0)
			{
				foreach (CategoryInfo categoryList in CategoryList)
				{
					dateTimes.AddRange(GetAvailableReservationStartTimes(categoryList.CategoryID));
				}
				dateTimes = dateTimes.Distinct<DateTime>().ToList<DateTime>();
				dateTimes.Sort();
				_AvailableReservationStartTimesDictionary.Add(categoryID, dateTimes);
				return dateTimes;
			}
			TimeSpan minTimeAhead = ModuleSettings.MinTimeAhead;
			int daysAhead = ModuleSettings.DaysAhead;
			TimeSpan reservationInterval = ModuleSettings.ReservationInterval;
			TimeSpan reservationDuration = ModuleSettings.ReservationDuration;
			if (ModuleSettings.AllowCategorySelection)
			{
				CategorySettings item = CategorySettingsDictionary[categoryID];
				daysAhead = item.DaysAhead;
				minTimeAhead = item.MinTimeAhead;
				reservationInterval = item.ReservationInterval;
				reservationDuration = item.ReservationDuration;
			}
			DateTime date = DateTime.Now.Date;
			DateTime dateTime = date.Add(new TimeSpan(daysAhead, 0, 0, 0));
			date = DateTime.Now.Add(minTimeAhead);
			DateTime date1 = date.Date;
			List<DateTimeRange> dateTimeRanges = WorkingHoursDictionary[categoryID];
			while (date1 <= dateTime)
			{
				foreach (DateTime timeSlot in GetTimeSlots(dateTimeRanges, date1, reservationInterval))
				{
					if (!(timeSlot > DateTime.Now.Add(minTimeAhead)) || !CanScheduleReservation(categoryID, timeSlot, reservationDuration))
					{
						continue;
					}
					dateTimes.Add(timeSlot);
				}
				date1 = date1.AddDays(1);
			}
			_AvailableReservationStartTimesDictionary.Add(categoryID, dateTimes);
			return dateTimes;
		}

		private List<DateTime> GetAvailableReservationStartTimes(int categoryID, DateTime date)
		{
			List<DateTime> dateTimes = new List<DateTime>();
			List<DateTime> availableReservationStartTimes = GetAvailableReservationStartTimes(categoryID);
			foreach (DateTime availableReservationStartTime in availableReservationStartTimes)
			{
				if (availableReservationStartTime.Date <= date)
				{
					if (availableReservationStartTime.Date != date.Date)
					{
						continue;
					}
					dateTimes.Add(availableReservationStartTime);
				}
				else
				{
					return dateTimes;
				}
			}
			return dateTimes;
		}

		private List<DateTime> GetAvailableReservationStartTimes(int categoryID, DateTime date, string timeOfDayName)
		{
			List<DateTime> dateTimes = new List<DateTime>();
			foreach (TimeOfDayInfo timeOfDayList in ModuleSettings.TimeOfDayList)
			{
				if (timeOfDayList.Name != timeOfDayName)
				{
					continue;
				}
				foreach (DateTime availableReservationStartTime in GetAvailableReservationStartTimes(categoryID, date))
				{
					if (!(availableReservationStartTime >= date.Date.Add(timeOfDayList.StartTime)) || !(availableReservationStartTime < date.Date.Add(timeOfDayList.EndTime)))
					{
						continue;
					}
					dateTimes.Add(availableReservationStartTime);
				}
			}
			return dateTimes;
		}

		protected string GetAvailableTimesCount(string timeOfDay)
		{
			if (timeOfDay == Null.NullString)
			{
				return Null.NullString;
			}
			return string.Concat("(", GetAvailableReservationStartTimes(SelectedCategory, SelectedDate, timeOfDay).Count, ")");
		}

		private List<int> GetCategoryIDsToRender()
		{
			List<int> nums = new List<int>();
			if (!SelectCategoryLast)
			{
				foreach (CategoryInfo categoryList in CategoryList)
				{
					if (!ModuleSettings.BindUponCategorySelection && !ModuleSettings.DisplayUnavailableCategories && GetAvailableReservationDays(categoryList.CategoryID).Count <= 1)
					{
						continue;
					}
					nums.Add(categoryList.CategoryID);
				}
			}
			else
			{
				foreach (CategoryInfo categoryInfo in CategoryList)
				{
					TimeSpan reservationDuration = CategorySettingsDictionary[categoryInfo.CategoryID].ReservationDuration;
					if (!ModuleSettings.DisplayUnavailableCategories)
					{
						DateTime selectedDate = SelectedDate;
						TimeSpan timeOfDay = SelectedDateTime.TimeOfDay;
						TimeSpan selectedDuration = SelectedDuration;
						TimeSpan timeSpan = new TimeSpan();
						if (GetAvailableCategories(selectedDate, timeOfDay, (selectedDuration != timeSpan ? SelectedDuration : reservationDuration)).Count<CategoryInfo>((CategoryInfo _categoryInfo) => _categoryInfo.CategoryID == categoryInfo.CategoryID) == 0)
						{
							continue;
						}
					}
					nums.Add(categoryInfo.CategoryID);
				}
			}
			return nums;
		}

		protected string GetCategoryName(int categoryID)
		{
			return string.Concat(Helper.GetCategoryName(categoryID), (SelectCategoryLast || ModuleSettings.BindUponCategorySelection || !ModuleSettings.DisplayRemainingReservations ? string.Empty : string.Concat(" ", GetAvailableReservationsCountToRender(categoryID))));
		}

		protected string GetCustomFieldValue(CustomFieldDefinitionInfo customFieldDefinitionInfo, bool localize = true)
		{
			string empty = string.Empty;
			if (CustomFieldValueInfoList != null)
			{
				List<CustomFieldValueInfo> list = (
					from customFieldValueInfo in CustomFieldValueInfoList
					where customFieldValueInfo.CustomFieldDefinitionID == customFieldDefinitionInfo.CustomFieldDefinitionID
					select customFieldValueInfo).ToList<CustomFieldValueInfo>();
				if (list.Count != 0)
				{
					empty = list[0].Value;
					if (localize && customFieldDefinitionInfo.CustomFieldDefinitionType == CustomFieldDefinitionType.CheckBox)
					{
						empty = Localization.GetString(empty, LocalResourceFile);
					}
				}
			}
			if (localize && string.IsNullOrEmpty(empty))
			{
				empty = NotAvailable;
			}
			return empty;
		}

		protected string GetFriendlyDate(DateTime dateTime)
		{
			string str;
			str = (CultureInfo.CurrentCulture.DateTimeFormat.LongDatePattern.StartsWith("dddd") ? "ddd " : string.Empty);
			string str1 = CultureInfo.CurrentCulture.DateTimeFormat.MonthDayPattern.Replace("MMMM", "MMM");
			int day = dateTime.Day;
			return dateTime.ToString(string.Concat(str, str1.Replace("dd", string.Concat(day.ToString(), (Thread.CurrentThread.CurrentCulture.ToString().StartsWith("en") ? string.Concat("'", GetOrdinal(dateTime.Day), "'") : string.Empty)))));
		}

		protected string GetFriendlyReservationDuration(TimeSpan timeSpan)
		{
			if (timeSpan.TotalMinutes % 10080 == 0)
			{
				return string.Concat(timeSpan.TotalDays / 7, (timeSpan.TotalDays / 7 > 1 ? Localization.GetString("Weeks", LocalResourceFile) : Localization.GetString("Week", LocalResourceFile)));
			}
			if (timeSpan.TotalMinutes % 1440 == 0)
			{
				return string.Concat(timeSpan.TotalDays, (timeSpan.TotalDays > 1 ? Localization.GetString("Days", LocalResourceFile) : Localization.GetString("Day", LocalResourceFile)));
			}
			if (timeSpan.TotalDays > 1 && timeSpan.TotalMinutes % 360 == 0)
			{
				return string.Concat(timeSpan.TotalDays, Localization.GetString("Days", LocalResourceFile));
			}
			if (timeSpan.TotalMinutes % 15 == 0 && timeSpan.TotalHours >= 1)
			{
				return string.Concat(timeSpan.TotalHours, (timeSpan.TotalHours > 1 ? Localization.GetString("Hours", LocalResourceFile) : Localization.GetString("Hour", LocalResourceFile)));
			}
			return string.Concat(timeSpan.TotalMinutes, (timeSpan.TotalMinutes > 1 ? Localization.GetString("Minutes", LocalResourceFile) : Localization.GetString("Minute", LocalResourceFile)));
		}

		protected string GetFriendlyReservationTime(DateTime dateTime)
		{
			TimeSpan reservationDuration = ModuleSettings.ReservationDuration;
			if (SelectedCategorySettings != null)
			{
				reservationDuration = SelectedCategorySettings.ReservationDuration;
			}
			if (!ModuleSettings.DisplayEndTime)
			{
				return GetFriendlyTime(dateTime);
			}
			return string.Concat(GetFriendlyTime(dateTime).Replace(":00am", "am").Replace(":00pm", "pm"), " - ", GetFriendlyTime(dateTime.Add(reservationDuration)).Replace(":00am", "am").Replace(":00pm", "pm"));
		}

		protected string GetFriendlyTime(DateTime dateTime)
		{
			return dateTime.ToShortTimeString().Replace(" AM", "am").Replace(" PM", "pm");
		}

		protected string GetFriendlyTimeOfDay(string timeOfDay)
		{
			if (timeOfDay == Null.NullString)
			{
				return Null.NullString;
			}
			return string.Concat(timeOfDay, (ModuleSettings.DisplayRemainingReservations ? string.Concat(" ", GetAvailableTimesCount(timeOfDay)) : string.Empty));
		}

		protected int GetHighestNumberOfConflictingReservations(List<Gafware.Modules.Reservations.ReservationInfo> reservations, int categoryID, DateTime dateTime, TimeSpan duration)
		{
			ArrayList arrayLists = new ArrayList();
			foreach (Gafware.Modules.Reservations.ReservationInfo reservation in reservations)
			{
				if (reservation.StartDateTime < dateTime.Add(duration))
				{
					if (!ModuleSettings.PreventCrossCategoryConflicts && categoryID != 0 && reservation.CategoryID != categoryID || !Conflicts(dateTime, (int)duration.TotalMinutes, reservation.StartDateTime, reservation.Duration))
					{
						continue;
					}
					arrayLists.Add(reservation);
				}
				else
				{
					break;
				}
			}
			int num = 0;
			for (DateTime i = dateTime; i < dateTime.Add(duration); i = i.AddMinutes(1))
			{
				int num1 = 0;
				foreach (Gafware.Modules.Reservations.ReservationInfo arrayList in arrayLists)
				{
					if (!Conflicts(i, 1, arrayList.StartDateTime, arrayList.Duration))
					{
						continue;
					}
					num1++;
				}
				if (num1 > num)
				{
					num = num1;
				}
			}
			return num;
		}

		public string GetOrdinal(int input)
		{
			switch (input % 100)
			{
				case 11:
				case 12:
				case 13:
					{
						return "th";
					}
			}
			switch (input % 10)
			{
				case 1:
					{
						return "st";
					}
				case 2:
					{
						return "nd";
					}
				case 3:
					{
						return "rd";
					}
			}
			return "th";
		}

		private string GetPhoneLettersOrDigits(string phone)
		{
			string str = "";
			for (int i = 0; i < phone.Length; i++)
			{
				if (char.IsLetterOrDigit(phone[i]))
				{
					char chr = phone[i];
					str = string.Concat(str, chr.ToString());
				}
			}
			return str;
		}

		protected string GetReservationDateTime(DateTime startDateTime, double duration)
		{
			DateTime dateTime = startDateTime.AddMinutes(duration);
			if (startDateTime.TimeOfDay != new TimeSpan())
			{
				if (startDateTime.Date == dateTime.Date)
				{
					return string.Concat(new string[] { startDateTime.ToLongDateString(), " ", startDateTime.ToShortTimeString(), " - ", dateTime.ToShortTimeString() });
				}
				return string.Concat(new string[] { startDateTime.ToLongDateString(), " ", startDateTime.ToShortTimeString(), " - ", dateTime.ToLongDateString(), " ", dateTime.ToShortTimeString() });
			}
			if (duration == 1440)
			{
				return startDateTime.ToLongDateString();
			}
			if (duration % 1440 == 0)
			{
				string longDateString = startDateTime.ToLongDateString();
				DateTime dateTime1 = dateTime.AddDays(-1);
				return string.Concat(longDateString, " - ", dateTime1.ToLongDateString());
			}
			if (startDateTime.Date == dateTime.Date)
			{
				return string.Concat(new string[] { startDateTime.ToLongDateString(), " ", startDateTime.ToShortTimeString(), " - ", dateTime.ToShortTimeString() });
			}
			return string.Concat(new string[] { startDateTime.ToLongDateString(), " ", startDateTime.ToShortTimeString(), " - ", dateTime.ToLongDateString(), " ", dateTime.ToShortTimeString() });
		}

		protected string GetReservationDateTime(Gafware.Modules.Reservations.ReservationInfo reservationInfo)
		{
			return GetReservationDateTime(reservationInfo.StartDateTime, (double)reservationInfo.Duration);
		}

		protected string GetReservationDateTime(Gafware.Modules.Reservations.PendingApprovalInfo eventInfo)
		{
			return GetReservationDateTime(eventInfo.StartDateTime, (double)eventInfo.Duration);
		}

		protected string GetReservationDateTime(Gafware.Modules.Reservations.PendingPaymentInfo eventInfo)
		{
			return GetReservationDateTime(eventInfo.StartDateTime, (double)eventInfo.Duration);
		}

		private string GetRoleNameByRoleID(ArrayList roles, int roleID)
		{
			IEnumerator enumerator = roles.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					RoleInfo current = (RoleInfo)enumerator.Current;
					if (current.RoleID != roleID)
					{
						continue;
					}
					return current.RoleName;
				}
				if (roleID == -3)
				{
					return "Unauthenticated Users";
				}
				if (roleID == -1)
				{
					return "All Users";
				}
			}
			finally
			{
				IDisposable disposable = enumerator as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
			return null;
		}

		protected ArrayList GetTimeSlots(List<DateTimeRange> dateTimeRangeList, DateTime date, TimeSpan reservationInterval)
		{
			ArrayList arrayLists = new ArrayList();
			foreach (DateTimeRange dateTimeRange in dateTimeRangeList)
			{
				if (!Conflicts(dateTimeRange.StartDateTime, (int)dateTimeRange.EndDateTime.Subtract(dateTimeRange.StartDateTime).TotalMinutes, date, 1440))
				{
					if (dateTimeRange.StartDateTime.Date <= date)
					{
						continue;
					}
					return arrayLists;
				}
				else
				{
					DateTime dateTime = (dateTimeRange.StartDateTime.Date == date ? dateTimeRange.StartDateTime : date);
					while (dateTime.Add(reservationInterval) <= dateTimeRange.EndDateTime)
					{
						if (dateTime.Date == date)
						{
							arrayLists.Add(dateTime);
							dateTime = dateTime.Add(reservationInterval);
						}
						else
						{
							break;
						}
					}
				}
			}
			return arrayLists;
		}

		protected List<string> GetTimesOfDayToRender(int categoryID, DateTime dateTime)
		{
			List<string> strs = new List<string>();
			List<DateTime> availableReservationStartTimes = GetAvailableReservationStartTimes(categoryID, dateTime);
			if (availableReservationStartTimes.Count != 0)
			{
				foreach (TimeOfDayInfo timeOfDayList in ModuleSettings.TimeOfDayList)
				{
					if (ModuleSettings.DisplayUnavailableTimeOfDay)
					{
						strs.Add(timeOfDayList.Name);
					}
					else
					{
						foreach (DateTime availableReservationStartTime in availableReservationStartTimes)
						{
							if (!(availableReservationStartTime >= dateTime.Date.Add(timeOfDayList.StartTime)) || !(availableReservationStartTime < dateTime.Date.Add(timeOfDayList.EndTime)))
							{
								continue;
							}
							strs.Add(timeOfDayList.Name);
							break;
						}
					}
				}
			}
			return strs;
		}

		protected List<DateTimeRange> GetWorkingHoursList(CategorySettings categorySettings)
		{
			string[] strArrays;
			int k;
			DateTime dateTime;
			List<DateTimeRange> dateTimeRanges = new List<DateTimeRange>();
			TimeSpan minTimeAhead = ModuleSettings.MinTimeAhead;
			int daysAhead = ModuleSettings.DaysAhead;
			if (ModuleSettings.AllowCategorySelection)
			{
				minTimeAhead = categorySettings.MinTimeAhead;
				daysAhead = categorySettings.DaysAhead;
			}
			List<RecurrencePattern> recurrencePatterns = new List<RecurrencePattern>();
			Hashtable settings = null;
			if (categorySettings != null)
			{
				settings = categorySettings.Settings;
				if (!settings.ContainsKey("WorkingHours.1"))
				{
					settings = ModuleSettings.Settings;
				}
			}
			else
			{
				settings = ModuleSettings.Settings;
			}
			for (int i = 1; settings.ContainsKey(string.Concat("WorkingHours.", i)) && !string.IsNullOrEmpty((string)settings[string.Concat("WorkingHours.", i)]); i++)
			{
				recurrencePatterns.Add(Gafware.Modules.Reservations.Helper.DeserializeRecurrencePattern((string)settings[string.Concat("WorkingHours.", i)]));
			}
			DateTime date = DateTime.Now.Add(minTimeAhead);
			DateTime date1 = date.Date;
			date = DateTime.Now.Date;
			DateTime dateTime1 = date.Add(new TimeSpan(daysAhead, 0, 0, 0));
			List<DateTime> dateTimes = new List<DateTime>();
			foreach (RecurrencePattern recurrencePattern in recurrencePatterns)
			{
				OccurrenceCalculator occurrenceCalculator = new OccurrenceCalculator(recurrencePattern);
				for (DateTime? j = (occurrenceCalculator.OccursOnDay(date1) ? new DateTime?(date1) : occurrenceCalculator.CalculateNextOccurrence(date1)); j.HasValue && j.Value <= dateTime1; j = occurrenceCalculator.CalculateNextOccurrence(j.Value))
				{
					string item = null;
					if (categorySettings != null && categorySettings.WorkingHoursExceptionsDefined)
					{
						if (categorySettings.IsDefined(j.Value.ToString("d", CultureInfo.InvariantCulture)))
						{
							Hashtable hashtables = categorySettings.Settings;
							date = j.Value;
							item = (string)hashtables[date.ToString("d", CultureInfo.InvariantCulture)];
						}
					}
					else if (ModuleSettings.IsDefined(j.Value.ToString("d", CultureInfo.InvariantCulture)))
					{
						Hashtable settings1 = ModuleSettings.Settings;
						date = j.Value;
						item = (string)settings1[date.ToString("d", CultureInfo.InvariantCulture)];
					}
					if (item == null)
					{
						date = j.Value;
						DateTime dateTime2 = date.Add(recurrencePattern.StartTime);
						DateTime dateTime3 = dateTime2.Add(recurrencePattern.Duration);
						AddToDateTimeRangeList(dateTimeRanges, dateTime2, dateTime3);
					}
					else
					{
						date = j.Value;
						dateTimes.Add(date.Date);
						if (item != string.Empty)
						{
							strArrays = item.Split(new char[] { ';' });
							for (k = 0; k < (int)strArrays.Length; k++)
							{
								string str = strArrays[k];
								date = j.Value;
								DateTime date2 = date.Date;
								date = j.Value;
								DateTime date3 = date.Date;
								if (str.ToLower() == "all day")
								{
									date3 = date3.AddDays(1);
								}
								else if (item.ToLower() != "none")
								{
									date2 = date2.Add(TimeSpan.Parse(str.Split(new char[] { '-' })[0]));
									date3 = date3.Add(TimeSpan.Parse(str.Split(new char[] { '-' })[1]));
								}
								AddToDateTimeRangeList(dateTimeRanges, date2, date3);
							}
						}
					}
				}
			}
			Hashtable hashtables1 = (categorySettings == null || !categorySettings.WorkingHoursExceptionsDefined ? ModuleSettings.Settings : categorySettings.Settings);
			foreach (string key in hashtables1.Keys)
			{
				if (!DateTime.TryParse(key, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime) || dateTimes.Contains(dateTime))
				{
					continue;
				}
				string item1 = (string)hashtables1[key];
				if (string.IsNullOrEmpty(item1))
				{
					continue;
				}
				strArrays = item1.Split(new char[] { ';' });
				for (k = 0; k < (int)strArrays.Length; k++)
				{
					string str1 = strArrays[k];
					DateTime date4 = dateTime.Date;
					DateTime dateTime4 = dateTime.Date;
					if (str1.ToLower() == "all day")
					{
						dateTime4 = dateTime4.AddDays(1);
					}
					else if (str1.ToLower() != "none")
					{
						date4 = date4.Add(TimeSpan.Parse(str1.Split(new char[] { '-' })[0]));
						dateTime4 = dateTime4.Add(TimeSpan.Parse(str1.Split(new char[] { '-' })[1]));
					}
					AddToDateTimeRangeList(dateTimeRanges, date4, dateTime4);
				}
			}
			dateTimeRanges.Sort();
			return dateTimeRanges;
		}

		protected void HideAllStepTables()
		{
			foreach (Control control in Controls)
			{
				if (control == actionsTable || control == editionPlaceHolder || !(control.ID != "Gafware_Modules_Reservations_AuthorizeNetSIMForm_Hidden"))
				{
					continue;
				}
				control.Visible = false;
			}
		}

		private string HMAC_MD5(string key, string value)
		{
			byte[] bytes = (new ASCIIEncoding()).GetBytes(key);
			byte[] numArray = (new ASCIIEncoding()).GetBytes(value);
			byte[] numArray1 = (new HMACMD5(bytes)).ComputeHash(numArray);
			string str = "";
			for (int i = 0; i < (int)numArray1.Length; i++)
			{
				str = string.Concat(str, numArray1[i].ToString("x").PadLeft(2, '0'));
			}
			return str;
		}

		protected bool IsCategoryAvailable(int categoryID)
		{
			DateTime dateTime;
			if (!SelectCategoryLast)
			{
				return GetAvailableReservationStartTimes(categoryID).Count > 0;
			}
			if (SelectedDuration == new TimeSpan())
			{
				int num = categoryID;
				DateTime selectedDateTime = SelectedDateTime;
				dateTime = new DateTime();
				return IsCategoryAvailable(num, (selectedDateTime != dateTime ? SelectedDateTime : SelectedDate));
			}
			int num1 = categoryID;
			DateTime selectedDateTime1 = SelectedDateTime;
			dateTime = new DateTime();
			return IsCategoryAvailable(num1, (selectedDateTime1 != dateTime ? SelectedDateTime : SelectedDate), SelectedDuration);
		}

		protected bool IsCategoryAvailable(int categoryID, DateTime dateTime)
		{
			return GetAvailableReservationStartTimes(categoryID, dateTime.Date).Contains(dateTime);
		}

		protected bool IsCategoryAvailable(int categoryID, DateTime dateTime, TimeSpan duration)
		{
			if (!IsCategoryAvailable(categoryID, dateTime))
			{
				return false;
			}
			return GetAvailableDurations(categoryID, dateTime).Contains(duration);
		}

		protected bool IsTimeOfDayAvailable(int categoryID, DateTime dateTime, string timeOfDayName)
		{
			return GetAvailableReservationStartTimes(categoryID, dateTime, timeOfDayName).Count > 0;
		}

		private bool IsValidVerificationCode(string email, string verificationCode)
		{
			if (!ModuleSettings.RequireVerificationCode)
			{
				return true;
			}
			return Helper.GenerateVerificationCode(email) == verificationCode;
		}

		private void MakeAuthorizeNetSIMPayment(Gafware.Modules.Reservations.PendingPaymentInfo pendingPaymentInfo, decimal amount, string feeType)
		{
			string empty = string.Empty;
			empty = (!ModuleSettings.AuthorizeNetTestMode ? "<form action = 'https://secure.authorize.net/gateway/transact.dll' method = 'post'>" : "<form action = 'https://test.authorize.net/gateway/transact.dll' method = 'post'>");
			int totalSeconds = (new Random()).Next(0, 1000);
			string str = totalSeconds.ToString();
			TimeSpan utcNow = DateTime.UtcNow - new DateTime(1970, 1, 1);
			totalSeconds = (int)utcNow.TotalSeconds;
			string str1 = totalSeconds.ToString();
			string authorizeNetTransactionKey = ModuleSettings.AuthorizeNetTransactionKey;
			string[] authorizeNetApiLogin = new string[] { ModuleSettings.AuthorizeNetApiLogin, "^", str, "^", str1, "^", null, null, null };
			authorizeNetApiLogin[6] = string.Format(CultureInfo.InvariantCulture, "{0:F}", new object[] { amount });
			authorizeNetApiLogin[7] = "^";
			authorizeNetApiLogin[8] = pendingPaymentInfo.Currency;
			string str2 = HMAC_MD5(authorizeNetTransactionKey, string.Concat(authorizeNetApiLogin));
			empty = string.Concat(empty, "<input type = 'hidden' name = 'x_fp_hash' value = '", str2, "' />");
			empty = string.Concat(empty, "<input type = 'hidden' name = 'x_fp_sequence' value = '", str, "' />");
			empty = string.Concat(empty, "<input type = 'hidden' name = 'x_fp_timestamp' value = '", str1, "' />");
			empty = string.Concat(empty, "<input type = 'hidden' name = 'x_login' value = '", ModuleSettings.AuthorizeNetApiLogin, "' />");
			empty = string.Concat(empty, "<input type = 'hidden' name = 'x_amount' value = '", string.Format(CultureInfo.InvariantCulture, "{0:F}", new object[] { amount }), "' />");
			empty = string.Concat(empty, "<input type = 'hidden' name = 'x_show_form' value = 'PAYMENT_FORM' />");
			string[] host = new string[] { empty, "<input type = 'hidden' name = 'x_relay_url' value = '", null, null, null, null };
			host[2] = (Request.IsSecureConnection ? "https://" : "http://");
			host[3] = Request.Url.Host;
			host[4] = ResolveUrl("AuthorizeNetSIMRelayResponse.aspx");
			host[5] = "' />";
			empty = string.Concat(host);
			empty = string.Concat(empty, "<input type = 'hidden' name = 'x_relay_response' value = 'TRUE' />");
			empty = string.Concat(empty, "<input type = 'hidden' name = 'x_currency_code' value = '", pendingPaymentInfo.Currency, "' />");
			empty = string.Concat(new object[] { empty, "<input type = 'hidden' name = 'x_invoice_num' value = '", pendingPaymentInfo.PendingPaymentID, "' />" });
			empty = string.Concat(empty, "<input type = 'hidden' name = 'x_description' value = '", (new PortalSecurity()).InputFilter(Helper.ReplaceTokens(ModuleSettings.ItemDescription, pendingPaymentInfo, feeType), (PortalSecurity.FilterFlag)6), "' />");
			int tabId = TabId;
			string empty1 = string.Empty;
			string[] strArrays = new string[3];
			totalSeconds = pendingPaymentInfo.PendingPaymentID;
			strArrays[0] = string.Concat("PendingPaymentID=", totalSeconds.ToString());
			PendingPaymentStatus pendingPaymentStatu = PendingPaymentStatus.Void;
			strArrays[1] = string.Concat("Status=", pendingPaymentStatu.ToString());
			strArrays[2] = string.Concat("Email=", pendingPaymentInfo.Email);
			empty = string.Concat(empty, "<input type = 'hidden' name = 'x_cancel_url' value = '", _navigationManager.NavigateURL(tabId, empty1, strArrays), "' />");
			empty = string.Concat(empty, SIMFormGenerator.EndForm());
			Gafware_Modules_Reservations_AuthorizeNetSIMForm_Hidden.Value = Server.HtmlEncode(empty);
			Gafware_Modules_Reservations_AuthorizeNetSIMForm_Hidden.Visible = true;
			AddModuleMessage(Localization.GetString("RedirectingToPaymentPage", LocalResourceFile), ModuleMessage.ModuleMessageType.BlueInfo);
			actionsTable.Visible = false;
			HideAllStepTables();
		}

		private void MakePayment(Gafware.Modules.Reservations.PendingPaymentInfo pendingPaymentInfo, decimal amount)
		{
			MakePayment(pendingPaymentInfo, amount, (ReservationInfo.ReservationID == Null.NullInteger ? Localization.GetString("ReservationFee", LocalResourceFile) : Localization.GetString("RescheduleFee", LocalResourceFile)));
		}

		private void MakePayment(Gafware.Modules.Reservations.PendingPaymentInfo pendingPaymentInfo, decimal amount, string feeType)
		{
			if (ModuleSettings.PaymentMethod == PaymentMethod.PayPalPaymentsStandard)
			{
				MakePayPalPayment(pendingPaymentInfo, amount, feeType);
				return;
			}
			if (ModuleSettings.PaymentMethod == PaymentMethod.AuthorizeNetSIM)
			{
				MakeAuthorizeNetSIMPayment(pendingPaymentInfo, amount, feeType);
			}
		}

		private void MakePayPalPayment(Gafware.Modules.Reservations.PendingPaymentInfo pendingPaymentInfo, decimal amount, string feeType)
		{
			HttpResponse response = Response;
			string[] payPalUrl = new string[21];
			payPalUrl[0] = ModuleSettings.PayPalUrl;
			payPalUrl[1] = (ModuleSettings.PayPalUrl.EndsWith("/") ? string.Empty : "/");
			payPalUrl[2] = "cgi-bin/webscr?cmd=_xclick&business=";
			payPalUrl[3] = Server.UrlPathEncode(ModuleSettings.PayPalAccount);
			payPalUrl[4] = "&item_name=";
			payPalUrl[5] = Server.UrlPathEncode((new PortalSecurity()).InputFilter(Helper.ReplaceTokens(ModuleSettings.ItemDescription, pendingPaymentInfo, feeType), (PortalSecurity.FilterFlag)6));
			payPalUrl[6] = "&item_number=";
			int pendingPaymentID = pendingPaymentInfo.PendingPaymentID;
			payPalUrl[7] = pendingPaymentID.ToString();
			payPalUrl[8] = "&quantity=1&custom=";
			HttpServerUtility server = Server;
			DateTime startDateTime = pendingPaymentInfo.StartDateTime;
			payPalUrl[9] = server.UrlPathEncode(startDateTime.ToString());
			payPalUrl[10] = "&amount=";
			payPalUrl[11] = Server.UrlPathEncode(string.Format(CultureInfo.InvariantCulture, "{0:F}", new object[] { amount }));
			payPalUrl[12] = "&currency_code=";
			payPalUrl[13] = Server.UrlPathEncode(pendingPaymentInfo.Currency);
			payPalUrl[14] = "&return=";
			HttpServerUtility httpServerUtility = Server;
			int tabId = TabId;
			string empty = string.Empty;
			string[] strArrays = new string[3];
			pendingPaymentID = pendingPaymentInfo.PendingPaymentID;
			strArrays[0] = string.Concat("PendingPaymentID=", pendingPaymentID.ToString());
			PendingPaymentStatus pendingPaymentStatu = PendingPaymentStatus.Paid;
			strArrays[1] = string.Concat("Status=", pendingPaymentStatu.ToString());
			strArrays[2] = string.Concat("Email=", pendingPaymentInfo.Email);
			payPalUrl[15] = httpServerUtility.UrlPathEncode(_navigationManager.NavigateURL(tabId, empty, strArrays));
			payPalUrl[16] = "&cancel_return=";
			HttpServerUtility server1 = Server;
			int num = TabId;
			string str = string.Empty;
			string[] strArrays1 = new string[3];
			pendingPaymentID = pendingPaymentInfo.PendingPaymentID;
			strArrays1[0] = string.Concat("PendingPaymentID=", pendingPaymentID.ToString());
			pendingPaymentStatu = PendingPaymentStatus.Void;
			strArrays1[1] = string.Concat("Status=", pendingPaymentStatu.ToString());
			strArrays1[2] = string.Concat("Email=", pendingPaymentInfo.Email);
			payPalUrl[17] = server1.UrlPathEncode(_navigationManager.NavigateURL(num, str, strArrays1));
			payPalUrl[18] = "&notify_url=";
			payPalUrl[19] = Server.UrlPathEncode(string.Concat((Request.IsSecureConnection ? "https://" : "http://"), Request.Url.Host, ResolveUrl("IPN.aspx")));
			payPalUrl[20] = "&undefined_quantity=&no_note=1&no_shipping=1";
			response.Redirect(string.Concat(payPalUrl));
		}

		protected void ModerateCommandButtonClicked(object sender, EventArgs e)
		{
			HttpResponse response = Response;
			string[] strArrays = new string[1];
			int moduleId = ModuleId;
			strArrays[0] = string.Concat("mid=", moduleId.ToString());
			response.Redirect(_navigationManager.NavigateURL("Moderate", strArrays));
		}

		protected void Page_Init(object sender, EventArgs e)
		{
			SetTheme();
			if (SelectCategoryLast)
			{
				categoryTableRowPlaceHolder.Controls.Remove(categoryTableRow);
				categoryTableRowPlaceHolder2.Controls.Add(categoryTableRow);
			}
			moderateCommandButton.Click += new EventHandler(ModerateCommandButtonClicked);
			duplicateReservationsCommandButton.Click += new EventHandler(DuplicateReservationsCommandButtonClicked);
            mainSettingsCommandButton.Click += new EventHandler(MainSettingsCommandButtonClicked);
			viewReservationsCommandButton.Click += new EventHandler(ViewReservationsCommandButtonClicked);
			viewReservationsCalendarCommandButton.Click += new EventHandler(ViewReservationsCalendarCommandButtonClicked);
			viewEditAReservationCommandButton.Click += new EventHandler(ViewEditAnReservationCommandButtonClicked);
			contactInfoBackCommandButton.Click += new EventHandler(Step2BackCommandButtonClicked);
			contactInfoNextCommandButton.Click += new EventHandler(Step2NextCommandButtonClicked);
			contactInfoConfirmCommandButton.Click += new EventHandler(Step2ConfirmCommandButtonClicked);
			step3BackCommandButton.Click += new EventHandler(Step3BackCommandButtonClicked);
			step3ConfirmCommandButton.Click += new EventHandler(Step3ConfirmCommandButtonClicked);
			step3NextCommandButton.Click += new EventHandler(Step3NextCommandButtonClicked);
			step3ConfirmAndPayLaterCommandButton.Click += new EventHandler(Step3ConfirmAndPayLaterCommandButtonClicked);
			contactInfoConfirmAndPayLaterCommandButton.Click += new EventHandler(Step2ConfirmAndPayLaterCommandButtonClicked);
			viewEditStep1BackCommandButton.Click += new EventHandler(ViewEditStep1BackCommandButtonClicked);
			viewEditStep1NextCommandButton.Click += new EventHandler(ViewEditStep1NextCommandButtonClicked);
			viewEditStep2BackCommandButton.Click += new EventHandler(ViewEditStep2BackCommandButtonClicked);
			cancelReservationCommandButton.Click += new EventHandler(CancelReservationCommandButtonClicked);
			rescheduleReservationCommandButton.Click += new EventHandler(RescheduleReservationCommandButtonClicked);
			confirmReservationCommandButton.Click += new EventHandler(ConfirmReservationCommandButtonClicked);
			scheduleAnotherReservationCommandButton.Click += new EventHandler(ScheduleAnotherReservationCommandButtonClicked);
			doneCommandButton.Click += new EventHandler(DoneCommandButtonClicked);
			approveCommandButton.Click += new EventHandler(ApproveCommandButtonClicked);
			declineCommandButton.Click += new EventHandler(DeclineCommandButtonClicked);
			returnCommandButton.Click += new EventHandler(ReturnCommandButtonClicked);
			if (IsProfessional)
			{
				cashierCommandButton.Click += new EventHandler(CashierCommandButtonClicked);
				payCommandButton.Click += new EventHandler(PayCommandButtonClicked);
			}
			firstNameTextBox.Attributes.Add("placeholder", Localization.GetString("FirstName", LocalResourceFile));
			lastNameTextBox.Attributes.Add("placeholder", Localization.GetString("LastName", LocalResourceFile));
			emailTextBox.Attributes.Add("placeholder", Localization.GetString("Email", LocalResourceFile));
			phoneTextBox.Attributes.Add("placeholder", Localization.GetString("Phone", LocalResourceFile));
			descriptionTextbox.Attributes.Add("placeholder", Localization.GetString("Comments", LocalResourceFile));
			viewEditEmailTextBox.Attributes.Add("placeholder", Localization.GetString("Email", LocalResourceFile));
			viewEditPhoneTextBox.Attributes.Add("placeholder", Localization.GetString("Phone", LocalResourceFile));
			viewEditVerificationCodeTextBox.Attributes.Add("placeholder", Localization.GetString("VerificationCode", LocalResourceFile));
		}

        private void MainSettingsCommandButtonClicked(object sender, EventArgs e)
        {
			HttpResponse response = Response;
			string[] strArrays = new string[1];
			int moduleId = ModuleId;
			strArrays[0] = string.Concat("mid=", moduleId.ToString());
			response.Redirect(_navigationManager.NavigateURL("EditSettings", strArrays));
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			Gafware.Modules.Reservations.PendingApprovalInfo pendingApproval;
			try
			{
				if (!IsPostBack)
				{
					Gafware.Modules.Reservations.Helper.DisplayModuleMessageIfAny(this);
					editionPlaceHolder.Controls.Add(new LiteralControl(string.Concat("<!--Edition: ", Gafware.Modules.Reservations.Helper.GetEdition(ModuleSettings.ActivationCode), "-->")));
				}
				if (!Gafware.Modules.Reservations.Helper.ValidateActivationCode(ModuleSettings.ActivationCode))
				{
					DateTime now = DateTime.Now;
					try
					{
						now = DateTime.Parse(Gafware.Modules.Reservations.Helper.Decrypt(ComponentBase<IHostController, HostController>.Instance.GetString(ModuleSettings.INSTALLEDON_KEY)), CultureInfo.InvariantCulture);
					}
					catch (Exception)
					{
					}
					string str = string.Concat(new string[] { "https://www.gafware.com/DesktopModules/Gafware/Reservations/PurchaseRedirect.aspx?Product=", Server.UrlEncode("The Reservations Module"), "&Edition=Standard&ReturnUrl=", Server.UrlEncode(EditUrl("Activate")), "&CancelUrl=", Server.UrlEncode(_navigationManager.NavigateURL()), "&Version=", Server.UrlEncode(ModuleConfiguration.DesktopModule.Version) });
					string str1 = string.Concat(new string[] { "https://www.gafware.com/DesktopModules/Gafware/Reservations/PurchaseRedirect.aspx?Product=", Server.UrlEncode("The Reservations Module"), "&Edition=Professional&ReturnUrl=", Server.UrlEncode(EditUrl("Activate")), "&CancelUrl=", Server.UrlEncode(_navigationManager.NavigateURL()), "&Version=", Server.UrlEncode(ModuleConfiguration.DesktopModule.Version) });
					string str2 = string.Concat(new string[] { "https://www.gafware.com/DesktopModules/Gafware/Reservations/PurchaseRedirect.aspx?Product=", Server.UrlEncode("The Reservations Module"), "&Edition=", Server.UrlEncode("Enterprise"), "&ReturnUrl=", Server.UrlEncode(EditUrl("Activate")), "&CancelUrl=", Server.UrlEncode(_navigationManager.NavigateURL()), "&Version=", Server.UrlEncode(ModuleConfiguration.DesktopModule.Version) });
					string[] strArrays = new string[1];
					int moduleId = ModuleId;
					strArrays[0] = string.Concat("mid=", moduleId.ToString());
					string str3 = _navigationManager.NavigateURL("Activate", strArrays);
					string str4 = string.Concat("<a href=\"", str, "\">*Click here*</a>");
					string str5 = string.Concat("<a href=\"", str1, "\">*click here*</a>");
					string str6 = string.Concat("<a href=\"", str2, "\">*click here*</a>");
					string str7 = string.Concat("<a href=\"", str3, "\">*click here*</a>");
					string str8 = string.Format("<br /><br />{0} to purchase the Standard Edition instantly thru PayPal, {1} for the Professional Edition or {2} for the Enterprise Edition. You don't need a PayPal account - a credit/debit card will suffice. No reinstallation/reconfiguration will be required. The data you have entered will be preserved.<br /><br />Alternatively, you can visit the developer's store at <a href=\"http://store.dnnsoftware.com/vendor-profile/dnn-specialists\">http://store.dnnsoftware.com/vendor-profile/dnn-specialists</a>. You will have to uninstall and reinstall/reconfigure the module accordingly, and the data you have entered will be lost.<br /><br />If you have already purchased the module, please {3} to activate your copy.", new object[] { str4, str5, str6, str7 });
					if (DateTime.Now >= now || DateTime.Now.AddDays(30) < now)
					{
						AddModuleMessage(string.Concat(Localization.GetString("TrialEnded", LocalResourceFile), str8), ModuleMessage.ModuleMessageType.RedError);
						HideAllStepTables();
						actionsTable.Visible = false;
						return;
					}
					else if (Page.User.Identity.IsAuthenticated)
					{
						string str9 = "Your copy of The Reservations Module is set to expire on {0}.";
						AddModuleMessage(string.Concat(string.Format(str9, now.ToShortDateString()), str8), ModuleMessage.ModuleMessageType.YellowWarning);
					}
				}
				if (!IsPostBack && !ModuleSettings.IsDefined("VerificationCodeSalt"))
				{
					ModuleController moduleController = new ModuleController();
					RecurrencePattern recurrencePattern = new RecurrencePattern()
					{
						StartDate = DateTime.Now.Date,
						StartTime = new TimeSpan(8, 0, 0),
						Duration = new TimeSpan(9, 0, 0),
						Pattern = Pattern.Daily,
						EveryWeekDay = true
					};
					moduleController.UpdateTabModuleSetting(TabModuleId, "WorkingHours.1", Gafware.Modules.Reservations.Helper.SerializeRecurrencePattern(recurrencePattern));
					int tabModuleId = TabModuleId;
					Guid guid = Guid.NewGuid();
					moduleController.UpdateTabModuleSetting(tabModuleId, "VerificationCodeSalt", guid.ToString());
					ModuleController.SynchronizeModule(ModuleId);
					_ModuleSettings = null;
				}
				if (ModuleSettings.AllowCategorySelection && CategoryList.Count == 0)
				{
					HideAllStepTables();
					AddModuleMessage(Localization.GetString("NoCategories", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
				}
				else if (!IsPostBack)
				{
					availableDaysCalendar.PrevMonthText = string.Concat("<img src=\"", TemplateSourceDirectory, "/Images/back.png\">");
					availableDaysCalendar.NextMonthText = string.Concat("<img src=\"", TemplateSourceDirectory, "/Images/next.png\">");
					if (HasEditPermissions && ModuleSettings.RequireVerificationCode && ModuleSettings.ConfirmationMailBody.IndexOf("{VerificationCode}") == -1)
					{
						AddModuleMessage(Localization.GetString("VerificationCodeWarning", LocalResourceFile), ModuleMessage.ModuleMessageType.YellowWarning);
					}
					mainSettingsCommandButton.Visible = Helper.CanViewReservations(UserId);
					viewReservationsCommandButton.Visible = Helper.CanViewReservations(UserId);
					moderateCommandButton.Visible = IsModerator;
					cashierCommandButton.Visible = IsCashier;
					duplicateReservationsCommandButton.Visible = Helper.CanViewDuplicateReservations(UserId);
					HtmlGenericControl htmlGenericControl = descriptionTableRow;
					HtmlGenericControl htmlGenericControl1 = descriptionTableRow2;
					bool allowDescription = ModuleSettings.AllowDescription;
					bool flag = allowDescription;
					htmlGenericControl1.Visible = allowDescription;
					htmlGenericControl.Visible = flag;
					phoneTextBoxRequiredFieldValidator.Visible = ModuleSettings.RequirePhone;
					if (ModuleSettings.RequirePhone)
					{
						TextBox textBox = phoneTextBox;
						textBox.CssClass = string.Concat(textBox.CssClass, " Gafware_Modules_Reservations_Required");
					}
					CustomValidator customValidator = emailTextBoxRequiredFieldValidator;
					HtmlGenericControl htmlGenericControl2 = viewEditPhoneTableRow1;
					bool allowLookupByPhone = ModuleSettings.AllowLookupByPhone;
					flag = allowLookupByPhone;
					htmlGenericControl2.Visible = allowLookupByPhone;
					customValidator.Visible = !flag;
					HtmlGenericControl htmlGenericControl3 = viewEditVerificationCodeTableRow;
					HtmlGenericControl htmlGenericControl4 = viewEditVerificationCodeTableRow2;
					bool requireVerificationCode = ModuleSettings.RequireVerificationCode;
					flag = requireVerificationCode;
					htmlGenericControl4.Visible = requireVerificationCode;
					htmlGenericControl3.Visible = flag;
					bool flag1 = true;
					if (IsProfessional && PendingPaymentInfo.PendingPaymentID != Null.NullInteger)
					{
						DisplayScheduleAnotherReservation();
						if (QueryStringPendingPaymentStatus != PendingPaymentStatus.Paid)
						{
							if (QueryStringPendingPaymentStatus == PendingPaymentStatus.Void && PendingPaymentInfo.Status == 0)
							{
								PendingPaymentInfo.Status = 2;
								PendingPaymentInfo.LastModifiedOnDate = DateTime.Now;
								(new PendingPaymentController()).UpdatePendingPayment(PendingPaymentInfo);
								Response.Redirect(_navigationManager.NavigateURL());
							}
							else if (QueryStringPendingPaymentStatus == PendingPaymentStatus.Due)
							{
								BindReservationInfo();
								HideAllStepTables();
								step4Table.Visible = true;
								flag1 = false;
							}
						}
						else if (PendingPaymentInfo.Status == 7)
						{
							AddModuleMessage(Localization.GetString("DueProcessing", LocalResourceFile), 0);
							Gafware.Modules.Reservations.ReservationInfo reservation = ReservationController.GetReservation(PendingPaymentInfo.ReservationID);
							if (reservation == null)
							{
								Response.Redirect(_navigationManager.NavigateURL());
							}
							ReservationInfo = reservation;
							BindReservationInfo();
							HideAllStepTables();
							step4Table.Visible = true;
							flag1 = false;
							PendingPaymentInfo = new Gafware.Modules.Reservations.PendingPaymentInfo();
						}
						else if (PendingPaymentInfo.Status == 0 || PendingPaymentInfo.Status == 4)
						{
							AddModuleMessage(Localization.GetString("ReservationPendingPayment", LocalResourceFile), 0);
							BindPendingPaymentInfo();
							HideAllStepTables();
							step4Table.Visible = true;
							flag1 = false;
						}
						else if (PendingPaymentInfo.Status == 1)
						{
							if (PendingPaymentInfo.PendingApprovalID == Null.NullInteger)
							{
								pendingApproval = null;
							}
							else
							{
								pendingApproval = (new PendingApprovalController()).GetPendingApproval(PendingPaymentInfo.PendingApprovalID);
							}
							Gafware.Modules.Reservations.PendingApprovalInfo pendingApprovalInfo = pendingApproval;
							if ((pendingApprovalInfo == null || pendingApprovalInfo.Status == 1) && PendingPaymentInfo.ReservationID != Null.NullInteger)
							{
								Gafware.Modules.Reservations.ReservationInfo reservationInfo = ReservationController.GetReservation(PendingPaymentInfo.ReservationID);
								if (reservationInfo == null)
								{
									Response.Redirect(_navigationManager.NavigateURL());
								}
								ReservationInfo = reservationInfo;
								AddModuleMessage(Localization.GetString("ReservationScheduled", LocalResourceFile), 0);
								BindReservationInfo();
							}
							else
							{
								PendingApprovalInfo = pendingApprovalInfo;
								if (PendingApprovalInfo.PendingApprovalID == Null.NullInteger)
								{
									Response.Redirect(_navigationManager.NavigateURL());
								}
								else
								{
									AddModuleMessage(Localization.GetString("ReservationPendingApproval", LocalResourceFile), 0);
									BindPendingApprovalInfo();
								}
							}
							HideAllStepTables();
							step4Table.Visible = true;
							flag1 = false;
							PendingPaymentInfo = new Gafware.Modules.Reservations.PendingPaymentInfo();
						}
						else if (PendingPaymentInfo.Status == 3)
						{
							AddModuleMessage(Localization.GetString("PendingPaymentTimeout", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
							flag1 = true;
							PendingPaymentInfo = new Gafware.Modules.Reservations.PendingPaymentInfo();
						}
					}
					else if (PendingApprovalInfo.PendingApprovalID != Null.NullInteger)
					{
						if (UserId == Null.NullInteger)
						{
							string str10 = HttpUtility.UrlEncode(HttpContext.Current.Request.RawUrl);
							if (PortalSettings.LoginTabId == Null.NullInteger)
							{
								Response.Redirect(_navigationManager.NavigateURL(TabId, "login", new string[] { string.Concat("returnurl=", str10) }));
							}
							else
							{
								Response.Redirect(_navigationManager.NavigateURL(PortalSettings.LoginTabId, string.Empty, new string[] { string.Concat("returnurl=", str10) }));
							}
						}
						else if (Helper.CanModerate(UserId, PendingApprovalInfo.CategoryID))
						{
							BindPendingApprovalInfo();
							BindPendingApprovalModeration();
							HideAllStepTables();
							step4Table.Visible = true;
							flag1 = false;
						}
					}
					if (!string.IsNullOrEmpty(Request.QueryString["EventID"]) && (Helper.CanViewReservations(UserId) || Helper.CanViewDuplicateReservations(UserId)))
					{
						try
						{
							ReservationInfo = ReservationController.GetReservation(int.Parse(Request.QueryString["EventID"]));
							BindReservationInfo();
							HideAllStepTables();
							step4Table.Visible = true;
							flag1 = false;
							if (!string.IsNullOrEmpty(Request.QueryString["ReturnUrl"]))
							{
								returnCommandButton.Visible = true;
							}
						}
						catch (Exception)
						{
						}
					}
					if (IsProfessional)
					{
						BindCustomFieldTableRowRepeater();
					}
					if (flag1)
					{
						if (UserId == Null.NullInteger)
						{
							firstNameTextBox.Text = Request.QueryString["FirstName"];
							lastNameTextBox.Text = Request.QueryString["LastName"];
							emailTextBox.Text = Request.QueryString["Email"];
							phoneTextBox.Text = Request.QueryString["Phone"];
							if (!ModuleSettings.ContactInfoFirst)
							{
								step3BackCommandButton.Visible = false;
								step3NextCommandButton.Visible = true;
								step3ConfirmCommandButton.Visible = false;
								contactInfoBackCommandButton.Visible = true;
								contactInfoNextCommandButton.Visible = false;
								contactInfoConfirmCommandButton.Visible = true;
							}
							else
							{
								contactInfoBackCommandButton.Visible = false;
								contactInfoConfirmCommandButton.Visible = false;
								contactInfoNextCommandButton.Visible = true;
								step3ConfirmCommandButton.Visible = true;
								step3NextCommandButton.Visible = false;
								step3BackCommandButton.Visible = true;
							}
						}
						else
						{
							firstNameTextBox.Text = UserInfo.FirstName;
							lastNameTextBox.Text = UserInfo.LastName;
							emailTextBox.Text = UserInfo.Email;
							phoneTextBox.Text = UserInfo.Profile.Telephone;
							if (!ModuleSettings.ContactInfoFirst)
							{
								step3BackCommandButton.Visible = false;
								step3NextCommandButton.Visible = !ModuleSettings.SkipContactInfoForAuthenticatedUsers;
								step3ConfirmCommandButton.Visible = ModuleSettings.SkipContactInfoForAuthenticatedUsers;
								contactInfoBackCommandButton.Visible = true;
								contactInfoNextCommandButton.Visible = false;
								contactInfoConfirmCommandButton.Visible = true;
							}
							else
							{
								contactInfoBackCommandButton.Visible = false;
								contactInfoConfirmCommandButton.Visible = false;
								contactInfoNextCommandButton.Visible = true;
								step3ConfirmCommandButton.Visible = true;
								step3NextCommandButton.Visible = false;
								step3BackCommandButton.Visible = !ModuleSettings.SkipContactInfoForAuthenticatedUsers;
							}
						}
						if (!ModuleSettings.ContactInfoFirst || UserId != Null.NullInteger && ModuleSettings.SkipContactInfoForAuthenticatedUsers)
						{
							Step2NextCommandButtonClicked(sender, e);
						}
						else
						{
							SetSelectedCategoryFromQueryString();
							if (!ModuleSettings.BindUponCategorySelection || SelectedCategory > 0)
							{
								if (!ModuleSettings.AllowCategorySelection || SelectCategoryLast)
								{
									BindAvailableDays();
								}
								else
								{
									BindCategories();
									if (SelectedCategory > 0)
									{
										BindAvailableDays();
									}
								}
							}
						}
					}
				}
			}
			catch (Exception exception2)
			{
				Exceptions.ProcessModuleLoadException(this, exception2);
			}
		}

		protected void Page_PreRender(object sender, EventArgs e)
		{
			if (!IsPostBack)
			{
				Label label = step2ConfirmCommandButtonLabel;
				Label label1 = step3ConfirmCommandButtonLabel;
				string str = Localization.GetString("Confirm", LocalResourceFile);
				string str1 = str;
				label1.Text = str;
				label.Text = str1;
				cancelReservationCommandButtonLabel.Text = Localization.GetString("cancelReservationCommandButton", LocalResourceFile);
			}
			if (IsProfessional)
			{
				Page.ClientScript.RegisterOnSubmitStatement(Page.GetType(), "clearPlaceholders", "if (typeof (clearPlaceholders) == 'function') clearPlaceholders();");
				Label label2 = (ModuleSettings.ContactInfoFirst || UserId != Null.NullInteger && ModuleSettings.SkipContactInfoForAuthenticatedUsers ? step3ConfirmCommandButtonLabel : step2ConfirmCommandButtonLabel);
				Control control = (ModuleSettings.ContactInfoFirst || UserId != Null.NullInteger && ModuleSettings.SkipContactInfoForAuthenticatedUsers ? step3ConfirmAndPayLaterCommandButton : contactInfoConfirmAndPayLaterCommandButton);
				if (!ModuleSettings.AllowPayLater || !(Amount > decimal.Zero))
				{
					control.Visible = false;
				}
				else
				{
					control.Visible = true;
				}
				if (Amount > decimal.Zero)
				{
					label2.Text = string.Concat(Localization.GetString("ConfirmAndPay", LocalResourceFile), " ", Gafware.Modules.Reservations.Helper.GetFriendlyAmount(Amount, ModuleSettings.Currency));
				}
				else if (Amount < decimal.Zero)
				{
					label2.Text = string.Format(Localization.GetString("ConfirmAndRequestRefund", LocalResourceFile), Gafware.Modules.Reservations.Helper.GetFriendlyAmount(Amount * decimal.MinusOne, ModuleSettings.Currency));
				}
				if (CancellationAmount < decimal.Zero)
				{
					cancelReservationCommandButtonLabel.Text = string.Format(Localization.GetString("CancelAndRequestRefund", LocalResourceFile), Gafware.Modules.Reservations.Helper.GetFriendlyAmount(CancellationAmount * decimal.MinusOne, ModuleSettings.Currency));
				}
				if (DueAmount <= decimal.Zero)
				{
					payCommandButton.Visible = false;
				}
				else
				{
					payCommandButton.Visible = true;
					payCommandButtonLabel.Text = string.Format(Localization.GetString("payCommandButton", LocalResourceFile), Gafware.Modules.Reservations.Helper.GetFriendlyAmount(DueAmount, ModuleSettings.Currency));
				}
				foreach (DropDownList dropDownList in FindChildControlsByType(customFieldTableRowRepeater, typeof(DropDownList)))
				{
					foreach (ListItem item in dropDownList.Items)
					{
						if (item.Value != string.Empty)
						{
							continue;
						}
						if (dropDownList.CssClass.IndexOf("Gafware_Modules_Reservations_Required") != -1)
						{
							item.Attributes.Add("disabled", "disabled");
						}
						if (string.IsNullOrEmpty(dropDownList.SelectedValue))
						{
							item.Attributes.Add("selected", "selected");
						}
						item.Attributes.Add("class", "placeholder");
					}
				}
			}
			if (ModuleSettings.AllowCategorySelection)
			{
				if (ModuleSettings.CategorySelectionMode == Gafware.Modules.Reservations.ModuleSettings.SelectionModeEnum.DropDownList)
				{
					foreach (ListItem listItem in categoriesDropDownList.Items)
					{
						listItem.Attributes["class"] = (string)ViewState[string.Concat(listItem.Value, ".class")];
						if (listItem.Attributes["class"] != "Gafware_Modules_Reservations_UnavailableListItem")
						{
							continue;
						}
						listItem.Attributes["disabled"] = "disabled";
					}
				}
				else if (ModuleSettings.CategorySelectionMode == Gafware.Modules.Reservations.ModuleSettings.SelectionModeEnum.ListBox)
				{
					foreach (ListItem item1 in categoriesListBox.Items)
					{
						item1.Attributes["class"] = (string)ViewState[string.Concat(item1.Value, ".class")];
						if (item1.Attributes["class"] != "Gafware_Modules_Reservations_UnavailableListItem")
						{
							continue;
						}
						item1.Attributes["disabled"] = "disabled";
					}
				}
			}
			if (ModuleSettings.DisplayTimeOfDay)
			{
				if (ModuleSettings.TimeOfDaySelectionMode == Gafware.Modules.Reservations.ModuleSettings.SelectionModeEnum.DropDownList)
				{
					foreach (ListItem listItem1 in timeOfDayDropDownList.Items)
					{
						listItem1.Attributes["class"] = (string)ViewState[string.Concat(listItem1.Value, ".class")];
						if (listItem1.Attributes["class"] != "Gafware_Modules_Reservations_UnavailableListItem")
						{
							continue;
						}
						listItem1.Attributes["disabled"] = "disabled";
					}
				}
				else if (ModuleSettings.TimeOfDaySelectionMode == Gafware.Modules.Reservations.ModuleSettings.SelectionModeEnum.ListBox)
				{
					foreach (ListItem item2 in timeOfDayListBox.Items)
					{
						item2.Attributes["class"] = (string)ViewState[string.Concat(item2.Value, ".class")];
						if (item2.Attributes["class"] != "Gafware_Modules_Reservations_UnavailableListItem")
						{
							continue;
						}
						item2.Attributes["disabled"] = "disabled";
					}
				}
			}
		}

		protected void PayCommandButtonClicked(object sender, EventArgs e)
		{
			MakePayment(DuePendingPaymentInfo, DueAmount, Localization.GetString("BalanceFee", LocalResourceFile));
		}

		private void PopulateEventInfoFromInput()
		{
			int totalMinutes;
			bool flag;
			ReservationInfo.TabModuleID = TabModuleId;
			ReservationInfo.CategoryID = (ModuleSettings.AllowCategorySelection ? SelectedCategory : 0);
			Gafware.Modules.Reservations.ReservationInfo reservationInfo = ReservationInfo;
			DateTime selectedDateTime = SelectedDateTime;
			DateTime startDateTime = new DateTime();
			reservationInfo.StartDateTime = (selectedDateTime == startDateTime ? SelectedDate : SelectedDateTime);
			Gafware.Modules.Reservations.ReservationInfo reservationInfo1 = ReservationInfo;
			TimeSpan selectedDuration = SelectedDuration;
			TimeSpan minReservationDuration = new TimeSpan();
			if (selectedDuration != minReservationDuration)
			{
				minReservationDuration = SelectedDuration;
				totalMinutes = (int)minReservationDuration.TotalMinutes;
			}
			else
			{
				minReservationDuration = MinReservationDuration;
				totalMinutes = (int)minReservationDuration.TotalMinutes;
			}
			reservationInfo1.Duration = totalMinutes;
			Gafware.Modules.Reservations.ReservationInfo reservationInfo2 = ReservationInfo;
			if (!ModuleSettings.SendReminders)
			{
				flag = false;
			}
			else
			{
				DateTime now = Gafware.Modules.Reservations.Helper.GetNow(ModuleSettings.TimeZone);
				startDateTime = ReservationInfo.StartDateTime;
				flag = now < startDateTime.Subtract(ModuleSettings.SendRemindersWhen);
			}
			reservationInfo2.SendReminder = flag;
			Gafware.Modules.Reservations.ReservationInfo totalMinutes1 = ReservationInfo;
			minReservationDuration = ModuleSettings.SendRemindersWhen;
			totalMinutes1.SendReminderWhen = (int)minReservationDuration.TotalMinutes;
			if (ReservationInfo.SendReminder)
			{
				ReservationInfo.SendReminderVia = ModuleSettings.SendRemindersVia;
				ReservationInfo.ReminderSent = false;
			}
			ReservationInfo.RequireConfirmation = (!ModuleSettings.RequireConfirmation ? false : ReservationInfo.SendReminder);
			Gafware.Modules.Reservations.ReservationInfo totalMinutes2 = ReservationInfo;
			minReservationDuration = ModuleSettings.RequireConfirmationWhen;
			totalMinutes2.RequireConfirmationWhen = (int)minReservationDuration.TotalMinutes;
			if (ReservationInfo.ReservationID != Null.NullInteger)
			{
				ReservationInfo.LastModifiedByUserID = UserId;
				ReservationInfo.LastModifiedOnDate = DateTime.Now;
				return;
			}
			ReservationInfo.FirstName = (new PortalSecurity()).InputFilter(firstNameTextBox.Text, (PortalSecurity.FilterFlag)6);
			ReservationInfo.LastName = (new PortalSecurity()).InputFilter(lastNameTextBox.Text, (PortalSecurity.FilterFlag)6);
			ReservationInfo.Email = (new PortalSecurity()).InputFilter(emailTextBox.Text, (PortalSecurity.FilterFlag)6);
			ReservationInfo.Phone = (phoneTextBox.Text != string.Empty ? (new PortalSecurity()).InputFilter(phoneTextBox.Text, (PortalSecurity.FilterFlag)6) : NotAvailable);
			ReservationInfo.Description = (new PortalSecurity()).InputFilter(descriptionTextbox.Text.Trim(), (PortalSecurity.FilterFlag)6);
			ReservationInfo.CreatedByUserID = UserId;
			ReservationInfo.CreatedOnDate = DateTime.Now;
		}

		protected void RescheduleReservationCommandButtonClicked(object sender, EventArgs e)
		{
			step3BackCommandButton.Visible = true;
			step3ConfirmCommandButton.Visible = true;
			step3NextCommandButton.Visible = false;
			Step2NextCommandButtonClicked(null, null);
		}

		protected void ReturnCommandButtonClicked(object sender, EventArgs e)
		{
			try
			{
				if (Request.QueryString["ReturnUrl"] != null)
				{
					Response.Redirect(Request.QueryString["ReturnUrl"]);
				}
				else
				{
					ModerateCommandButtonClicked(sender, e);
				}
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
			}
		}

		protected override object SaveViewState()
		{
			try
			{
				ViewState["ReservationInfo"] = (ReservationInfo.ReservationID != Null.NullInteger ? (object)ReservationInfo.ReservationID : (object)null);
				ViewState["PendingApprovalID"] = (PendingApprovalInfo.PendingApprovalID != Null.NullInteger ? (object)PendingApprovalInfo.PendingApprovalID : (object)null);
				if (IsProfessional)
				{
					ViewState["PendingPaymentID"] = (PendingPaymentInfo.PendingPaymentID != Null.NullInteger ? (object)PendingPaymentInfo.PendingPaymentID : (object)null);
				}
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
			}
			return base.SaveViewState();
		}

		protected void ScheduleAnotherReservationCommandButtonClicked(object sender, EventArgs e)
		{
			if (IsProfessional)
			{
				BindCustomFieldTableRowRepeater();
			}
			if (ReservationInfo.ReservationID != Null.NullInteger)
			{
				firstNameTextBox.Text = ReservationInfo.FirstName;
				lastNameTextBox.Text = ReservationInfo.LastName;
				emailTextBox.Text = (ReservationInfo.Email != NotAvailable ? ReservationInfo.Email : string.Empty);
				phoneTextBox.Text = (ReservationInfo.Phone != NotAvailable ? ReservationInfo.Phone : string.Empty);
			}
			else if (PendingApprovalInfo.PendingApprovalID != Null.NullInteger)
			{
				firstNameTextBox.Text = PendingApprovalInfo.FirstName;
				lastNameTextBox.Text = PendingApprovalInfo.LastName;
				emailTextBox.Text = (PendingApprovalInfo.Email != NotAvailable ? PendingApprovalInfo.Email : string.Empty);
				phoneTextBox.Text = (PendingApprovalInfo.Phone != NotAvailable ? PendingApprovalInfo.Phone : string.Empty);
			}
			else if (IsProfessional && PendingPaymentInfo.PendingPaymentID != Null.NullInteger)
			{
				firstNameTextBox.Text = PendingPaymentInfo.FirstName;
				lastNameTextBox.Text = PendingPaymentInfo.LastName;
				emailTextBox.Text = (PendingPaymentInfo.Email != NotAvailable ? PendingPaymentInfo.Email : string.Empty);
				phoneTextBox.Text = (PendingPaymentInfo.Phone != NotAvailable ? PendingPaymentInfo.Phone : string.Empty);
			}
			ReservationInfo = new Gafware.Modules.Reservations.ReservationInfo();
			PendingApprovalInfo = new Gafware.Modules.Reservations.PendingApprovalInfo();
			PendingPaymentInfo = new Gafware.Modules.Reservations.PendingPaymentInfo();
			Step2NextCommandButtonClicked(sender, e);
		}

		protected bool SelectedCategoryChanged()
		{
			if (SelectCategoryLast)
			{
				return SelectedCategoryChangedLast();
			}
			DateTime dateTime = new DateTime();
			SelectedDate = dateTime;
			SelectedTimeOfDay = null;
			dateTime = new DateTime();
			SelectedDateTime = dateTime;
			SelectedDuration = new TimeSpan();
			availableDayTableRow.Visible = false;
			timesOfDayTableRow.Visible = false;
			timesTableRow.Visible = false;
			durationsTableRow.Visible = false;
			AvailableDaysCurrentPageIndex = 0;
			step3NextTable.Visible = false;
			if (IsCategoryAvailable(SelectedCategory))
			{
				availableDayTableRow.Visible = true;
				BindAvailableDays();
				BindCategories();
				return true;
			}
			if (GetCategoryIDsToRender().Count != 0)
			{
				AddModuleMessage(Localization.GetString("CategoryNotAvailable", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
			}
			if (!ModuleSettings.BindUponCategorySelection)
			{
				SelectedCategory = 0;
			}
			BindCategories();
			return false;
		}

		protected bool SelectedCategoryChangedLast()
		{
			step3NextTable.Visible = false;
			if (IsCategoryAvailable(SelectedCategory))
			{
				BindCategories();
				step3NextTable.Visible = true;
				return true;
			}
			DateTime selectedDateTime = SelectedDateTime;
			DateTime dateTime = new DateTime();
			if ((selectedDateTime != dateTime ? SelectedTimeChanged() : SelectedDateChanged()))
			{
				AddModuleMessage(Localization.GetString("CategoryNotAvailable", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
			}
			return false;
		}

		protected bool SelectedDateChanged()
		{
			bool flag;
			SelectedTimeOfDay = null;
			DateTime item = new DateTime();
			SelectedDateTime = item;
			TimeSpan timeSpan = new TimeSpan();
			SelectedDuration = timeSpan;
			timesOfDayTableRow.Visible = false;
			timesTableRow.Visible = false;
			durationsTableRow.Visible = false;
			step3NextTable.Visible = false;
			if (SelectCategoryLast)
			{
				SelectedCategory = 0;
				categoryTableRow.Visible = false;
			}
			if (!ModuleSettings.DisplayTimeOfDay)
			{
				AvailableTimesCurrentPageIndex = 0;
			}
			else
			{
				AvailableTimesOfDayCurrentPageIndex = 0;
			}
			List<DateTime> availableReservationDays = GetAvailableReservationDays(SelectedCategory);
			if (!availableReservationDays.Contains(SelectedDate))
			{
				if ((!ModuleSettings.AllowCategorySelection || SelectCategoryLast || SelectedCategoryChanged()) && !ModuleSettings.AllowCategorySelection && availableReservationDays.Count != 0)
				{
					AddModuleMessage(Localization.GetString("DayNotAvailable", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
				}
				if (!ModuleSettings.AllowCategorySelection || SelectCategoryLast)
				{
					item = new DateTime();
					SelectedDate = item;
					BindAvailableDays();
				}
				return false;
			}
			BindAvailableDays();
			TimeSpan minReservationDuration = MinReservationDuration;
			TimeSpan maxReservationDuration = MaxReservationDuration;
			if (SelectCategoryLast)
			{
				List<TimeSpan> timeSpans = new List<TimeSpan>();
				List<TimeSpan> timeSpans1 = new List<TimeSpan>();
				foreach (CategoryInfo availableCategory in GetAvailableCategories(SelectedDate))
				{
					timeSpans.Add(CategorySettingsDictionary[availableCategory.CategoryID].ReservationDuration);
					timeSpans1.Add(CategorySettingsDictionary[availableCategory.CategoryID].ReservationDurationMax);
				}
				minReservationDuration = timeSpans.Min<TimeSpan>();
				maxReservationDuration = timeSpans1.Max<TimeSpan>();
			}
			List<DateTime> availableReservationStartTimes = GetAvailableReservationStartTimes(SelectedCategory, SelectedDate);
			if (minReservationDuration.TotalHours % 24 != 0 || availableReservationStartTimes.Count != 1)
			{
				flag = false;
			}
			else
			{
				item = availableReservationStartTimes[0];
				TimeSpan timeOfDay = item.TimeOfDay;
				timeSpan = new TimeSpan();
				flag = timeOfDay == timeSpan;
			}
			if (!flag)
			{
				if (!ModuleSettings.DisplayTimeOfDay)
				{
					timesTableRow.Visible = true;
					BindAvailableTimes();
				}
				else
				{
					timesOfDayTableRow.Visible = true;
					BindAvailableTimesOfDay();
				}
			}
			else if (minReservationDuration < maxReservationDuration)
			{
				BindAvailableDurations();
				durationsTableRow.Visible = true;
			}
			else if (!SelectCategoryLast)
			{
				step3NextTable.Visible = true;
			}
			else
			{
				BindCategories();
				categoryTableRow.Visible = true;
			}
			return true;
		}

		protected bool SelectedDurationChanged()
		{
			int selectedCategory = SelectedCategory;
			DateTime selectedDateTime = SelectedDateTime;
			DateTime dateTime = new DateTime();
			List<TimeSpan> availableDurations = GetAvailableDurations(selectedCategory, (selectedDateTime == dateTime ? SelectedDate : SelectedDateTime));
			step3NextTable.Visible = false;
			if (SelectCategoryLast)
			{
				SelectedCategory = 0;
				categoryTableRow.Visible = false;
			}
			if (!availableDurations.Contains(SelectedDuration))
			{
				if (SelectedTimeChanged())
				{
					AddModuleMessage(Localization.GetString("DurationNotAvailable", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
				}
				return false;
			}
			BindAvailableDurations();
			if (!SelectCategoryLast)
			{
				step3NextTable.Visible = true;
			}
			else
			{
				BindCategories();
				categoryTableRow.Visible = true;
			}
			return true;
		}

		protected bool SelectedTimeChanged()
		{
			SelectedDuration = new TimeSpan();
			durationsTableRow.Visible = false;
			step3NextTable.Visible = false;
			if (SelectCategoryLast)
			{
				SelectedCategory = 0;
				categoryTableRow.Visible = false;
			}
			AvailableDurationsCurrentPageIndex = 0;
			if (!((ModuleSettings.DisplayTimeOfDay ? GetAvailableReservationStartTimes(SelectedCategory, SelectedDate, SelectedTimeOfDay) : GetAvailableReservationStartTimes(SelectedCategory, SelectedDate))).Contains(SelectedDateTime))
			{
				if (ModuleSettings.DisplayTimeOfDay && SelectedTimeOfDayChanged() || !ModuleSettings.DisplayTimeOfDay && SelectedDateChanged())
				{
					AddModuleMessage(Localization.GetString("TimeNotAvailable", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
				}
				return false;
			}
			BindAvailableTimes();
			TimeSpan minReservationDuration = MinReservationDuration;
			TimeSpan maxReservationDuration = MaxReservationDuration;
			if (SelectCategoryLast)
			{
				List<TimeSpan> timeSpans = new List<TimeSpan>();
				List<TimeSpan> timeSpans1 = new List<TimeSpan>();
				foreach (CategoryInfo availableCategory in GetAvailableCategories(SelectedDate))
				{
					timeSpans.Add(CategorySettingsDictionary[availableCategory.CategoryID].ReservationDuration);
					timeSpans1.Add(CategorySettingsDictionary[availableCategory.CategoryID].ReservationDurationMax);
				}
				minReservationDuration = timeSpans.Min<TimeSpan>();
				maxReservationDuration = timeSpans1.Max<TimeSpan>();
			}
			if (minReservationDuration < maxReservationDuration)
			{
				BindAvailableDurations();
				durationsTableRow.Visible = true;
			}
			else if (!SelectCategoryLast)
			{
				step3NextTable.Visible = true;
			}
			else
			{
				BindCategories();
				categoryTableRow.Visible = true;
			}
			return true;
		}

		protected bool SelectedTimeOfDayChanged()
		{
			SelectedDateTime = new DateTime();
			SelectedDuration = new TimeSpan();
			timesTableRow.Visible = false;
			durationsTableRow.Visible = false;
			if (SelectCategoryLast)
			{
				SelectedCategory = 0;
				categoryTableRow.Visible = false;
			}
			step3NextTable.Visible = false;
			AvailableTimesCurrentPageIndex = 0;
			if (IsTimeOfDayAvailable(SelectedCategory, SelectedDate, SelectedTimeOfDay))
			{
				BindAvailableTimesOfDay();
				BindAvailableTimes();
				timesTableRow.Visible = true;
				return true;
			}
			if (SelectedDateChanged())
			{
				AddModuleMessage(Localization.GetString("TimeOfDayNotAvailable", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
			}
			return false;
		}

		protected void SendVerificationCode(object sender, EventArgs e)
		{
			if (Page.IsValid)
			{
				Helper.SendVerificationCodeMail(viewEditEmailTextBox.Text, Helper.GenerateVerificationCode(viewEditEmailTextBox.Text));
				AddModuleMessage(Localization.GetString("VerificationCodeSent", LocalResourceFile), 0);
			}
		}

		private void SetSelectedCategoryFromQueryString()
		{
			if (ModuleSettings.AllowCategorySelection && !string.IsNullOrEmpty(Request.QueryString["Category"]))
			{
				foreach (int categoryIDsToRender in GetCategoryIDsToRender())
				{
					if (Helper.GetCategoryName(categoryIDsToRender).ToLower() != Request.QueryString["Category"].ToLower())
					{
						continue;
					}
					SelectedCategory = categoryIDsToRender;
				}
			}
		}

		public void SetTheme()
		{
			string str = string.Concat(new string[] { TemplateSourceDirectory, "/Themes/", ModuleSettings.Theme, "/", ModuleSettings.Theme, ".css" });
			HtmlLink htmlLink = new HtmlLink()
			{
				Href = str
			};
			htmlLink.Attributes.Add("rel", "stylesheet");
			htmlLink.Attributes.Add("type", "text/css");
			Page.Header.Controls.Add(htmlLink);
			str = string.Concat(new string[] { TemplateSourceDirectory, "/Themes/", ModuleSettings.Theme, "/", ModuleSettings.Theme, "-LTIE8.css" });
			htmlLink = new HtmlLink()
			{
				Href = str
			};
			htmlLink.Attributes.Add("rel", "stylesheet");
			htmlLink.Attributes.Add("type", "text/css");
			Page.Header.Controls.Add(new LiteralControl("<!--[if LT IE 8]>"));
			Page.Header.Controls.Add(htmlLink);
			Page.Header.Controls.Add(new LiteralControl("<![endif]-->"));
		}

		protected void Step2BackCommandButtonClicked(object sender, EventArgs e)
		{
			HideAllStepTables();
			step3Table.Visible = true;
		}

		protected void Step2ConfirmAndPayLaterCommandButtonClicked(object sender, EventArgs e)
		{
			if (Page.IsValid)
			{
				Confirm(false);
			}
		}

		protected void Step2ConfirmCommandButtonClicked(object sender, EventArgs e)
		{
			if (Page.IsValid)
			{
				Confirm(true);
			}
		}

		protected void Step2NextCommandButtonClicked(object sender, EventArgs e)
		{
			if (sender != contactInfoNextCommandButton || Page.IsValid)
			{
				HideAllStepTables();
				SelectedCategory = 0;
				DateTime dateTime = new DateTime();
				SelectedDate = dateTime;
				SelectedTimeOfDay = null;
				dateTime = new DateTime();
				SelectedDateTime = dateTime;
				categoryTableRow.Visible = false;
				availableDayTableRow.Visible = false;
				timesOfDayTableRow.Visible = false;
				timesTableRow.Visible = false;
				durationsTableRow.Visible = false;
				step3NextTable.Visible = false;
				CategoriesCurrentPageIndex = 0;
				AvailableDaysCurrentPageIndex = 0;
				AvailableTimesOfDayCurrentPageIndex = 0;
				AvailableTimesCurrentPageIndex = 0;
				step3Table.Visible = true;
				SetSelectedCategoryFromQueryString();
				if (!ModuleSettings.AllowCategorySelection || SelectCategoryLast)
				{
					availableDayTableRow.Visible = true;
					BindAvailableDays();
				}
				else
				{
					categoryTableRow.Visible = true;
					BindCategories();
					if (SelectedCategory > 0)
					{
						availableDayTableRow.Visible = true;
						BindAvailableDays();
						return;
					}
				}
			}
		}

		protected void Step3BackCommandButtonClicked(object sender, EventArgs e)
		{
			HideAllStepTables();
			if (ReservationInfo.ReservationID == Null.NullInteger)
			{
				contactInfoDiv.Visible = true;
				return;
			}
			step4Table.Visible = true;
		}

		protected void Step3ConfirmAndPayLaterCommandButtonClicked(object sender, EventArgs e)
		{
			Confirm(false);
		}

		protected void Step3ConfirmCommandButtonClicked(object sender, EventArgs e)
		{
			Confirm(true);
		}

		protected void Step3NextCommandButtonClicked(object sender, EventArgs e)
		{
			HideAllStepTables();
			contactInfoDiv.Visible = true;
		}

		protected void TimeDropDownListSelectedIndexChanged(object sender, EventArgs e)
		{
			SelectedDateTime = DateTime.Parse(timeDropDownList.SelectedValue);
			SelectedTimeChanged();
		}

		protected void TimeListBoxSelectedIndexChanged(object sender, EventArgs e)
		{
			SelectedDateTime = DateTime.Parse(timeListBox.SelectedValue);
			SelectedTimeChanged();
		}

		protected void TimeOfDayDropDownListSelectedIndexChanged(object sender, EventArgs e)
		{
			SelectedTimeOfDay = timeOfDayDropDownList.SelectedValue;
			SelectedTimeOfDayChanged();
		}

		protected void TimeOfDayLinkButtonClicked(object sender, CommandEventArgs e)
		{
			SelectedTimeOfDay = (string)e.CommandArgument;
			SelectedTimeOfDayChanged();
		}

		protected void TimeOfDayListBoxSelectedIndexChanged(object sender, EventArgs e)
		{
			SelectedTimeOfDay = timeOfDayListBox.SelectedValue;
			SelectedTimeOfDayChanged();
		}

		protected void ValidateEmail(object sender, ServerValidateEventArgs e)
		{
			WebControl webControl = (WebControl)((BaseValidator)sender).NamingContainer.FindControl(((BaseValidator)sender).ControlToValidate);
			if (Helper.IsValidEmail(e.Value))
			{
				webControl.CssClass = webControl.CssClass.Replace(" Gafware_Modules_Reservations_Invalid", string.Empty);
				return;
			}
			if (!webControl.CssClass.Contains(" Gafware_Modules_Reservations_Invalid"))
			{
				WebControl webControl1 = webControl;
				webControl1.CssClass = string.Concat(webControl1.CssClass, " Gafware_Modules_Reservations_Invalid");
			}
			AddModuleMessage(Localization.GetString("InvalidEmail", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
			e.IsValid = false;
		}

		protected void ValidateRequired(object sender, ServerValidateEventArgs e)
		{
			WebControl webControl = (WebControl)((BaseValidator)sender).NamingContainer.FindControl(((BaseValidator)sender).ControlToValidate);
			if (e.Value != string.Empty)
			{
				webControl.CssClass = webControl.CssClass.Replace(" Gafware_Modules_Reservations_Invalid", string.Empty);
				return;
			}
			if (!webControl.CssClass.Contains(" Gafware_Modules_Reservations_Invalid"))
			{
				WebControl webControl1 = webControl;
				webControl1.CssClass = string.Concat(webControl1.CssClass, " Gafware_Modules_Reservations_Invalid");
			}
			if (!MissingRequiredFieldsModuleMessageAdded)
			{
				AddModuleMessage(Localization.GetString("MissingRequiredFields", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
				MissingRequiredFieldsModuleMessageAdded = true;
			}
			e.IsValid = false;
		}

		protected void ValidateViewEditEmail(object sender, ServerValidateEventArgs e)
		{
			e.IsValid = (!ModuleSettings.AllowLookupByPhone || viewEditEmailTextBox.Text != string.Empty ? true : viewEditPhoneTextBox.Text != string.Empty);
			if (!e.IsValid)
			{
				TextBox textBox = viewEditEmailTextBox;
				if (!textBox.CssClass.Contains(" Gafware_Modules_Reservations_Invalid"))
				{
					TextBox textBox1 = textBox;
					textBox1.CssClass = string.Concat(textBox1.CssClass, " Gafware_Modules_Reservations_Invalid");
				}
				if (!MissingRequiredFieldsModuleMessageAdded)
				{
					AddModuleMessage(Localization.GetString("MissingRequiredFields", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
					MissingRequiredFieldsModuleMessageAdded = true;
				}
			}
		}

		protected void ViewEditAnReservationCommandButtonClicked(object sender, EventArgs e)
		{
			HideAllStepTables();
			viewEditStep1Table.Visible = true;
			if (UserId != Null.NullInteger)
			{
				viewEditEmailTextBox.Text = UserInfo.Email;
				viewEditEmailTextBox.Enabled = !ModuleSettings.SkipContactInfoForAuthenticatedUsers;
			}
		}

		protected void ViewEditStep1BackCommandButtonClicked(object sender, EventArgs e)
		{
			HideAllStepTables();
			if (!ModuleSettings.ContactInfoFirst || UserId != Null.NullInteger && ModuleSettings.SkipContactInfoForAuthenticatedUsers)
			{
				Step2NextCommandButtonClicked(sender, e);
			}
			else
			{
				contactInfoDiv.Visible = true;
				SetSelectedCategoryFromQueryString();
				if (!ModuleSettings.BindUponCategorySelection || SelectedCategory > 0)
				{
					if (ModuleSettings.AllowCategorySelection)
					{
						BindCategories();
						return;
					}
					BindAvailableDays();
					return;
				}
			}
		}

		protected void ViewEditStep1NextCommandButtonClicked(object sender, EventArgs e)
		{
			bool flag;
			if (Page.IsValid)
			{
				if (viewEditEmailTextBox.Text != string.Empty && !IsValidVerificationCode(viewEditEmailTextBox.Text.Trim(), viewEditVerificationCodeTextBox.Text.Trim()))
				{
					AddModuleMessage(Localization.GetString("IncorrectVerificationCode", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
					return;
				}
				List<Gafware.Modules.Reservations.ReservationInfo> reservationInfos = new List<Gafware.Modules.Reservations.ReservationInfo>();
				bool flag1 = false;
				bool flag2 = false;
				bool flag3 = false;
				foreach (Gafware.Modules.Reservations.ReservationInfo reservationList in ReservationList)
				{
					flag = (viewEditEmailTextBox.Text == string.Empty ? false : reservationList.Email.ToLower() == viewEditEmailTextBox.Text.ToLower());
					if (!flag)
					{
						flag1 = (viewEditPhoneTextBox.Text == string.Empty ? false : GetPhoneLettersOrDigits(reservationList.Phone.ToLower()) == GetPhoneLettersOrDigits(viewEditPhoneTextBox.Text.ToLower()));
						flag2 = IsValidVerificationCode(reservationList.Email, viewEditVerificationCodeTextBox.Text.Trim());
					}
					if (!(flag | flag1) || FindByEventID(PendingApprovalList, reservationList.ReservationID) != null || IsProfessional && (FindByEventIDAndStatus(PendingPaymentList, reservationList.ReservationID, PendingPaymentStatus.Processing) != null || FindByEventIDAndStatus(PendingPaymentList, reservationList.ReservationID, PendingPaymentStatus.Held) != null))
					{
						continue;
					}
					if (!flag1)
					{
						reservationInfos.Add(reservationList);
					}
					else if (!flag2)
					{
						flag3 = true;
					}
					else
					{
						reservationInfos.Add(reservationList);
					}
				}
				if (reservationInfos.Count != 0)
				{
					HideAllStepTables();
					if (reservationInfos.Count == 1)
					{
						ReservationInfo = reservationInfos[0];
						BindReservationInfo();
						step4Table.Visible = true;
						return;
					}
					viewEditRepeater.DataSource = reservationInfos;
					viewEditRepeater.DataBind();
					viewEditStep2Table.Visible = true;
					return;
				}
				if (flag3)
				{
					AddModuleMessage(Localization.GetString("IncorrectVerificationCode2", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
					return;
				}
				AddModuleMessage(Localization.GetString("NoReservation", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
			}
		}

		protected void ViewEditStep2BackCommandButtonClicked(object sender, EventArgs e)
		{
			HideAllStepTables();
			viewEditStep1Table.Visible = true;
		}

		protected void ViewEditStep2EventCommandButtonClicked(object sender, CommandEventArgs e)
		{
			foreach (Gafware.Modules.Reservations.ReservationInfo reservationList in ReservationList)
			{
				if (reservationList.ReservationID != int.Parse((string)e.CommandArgument))
				{
					continue;
				}
				ReservationInfo = reservationList;
				HideAllStepTables();
				BindReservationInfo();
				step4Table.Visible = true;
				return;
			}
			HideAllStepTables();
			BindReservationInfo();
			step4Table.Visible = true;
		}

		protected void ViewReservationsCalendarCommandButtonClicked(object sender, EventArgs e)
		{
			HttpResponse response = Response;
			string[] strArrays = new string[1];
			int moduleId = ModuleId;
			strArrays[0] = string.Concat("mid=", moduleId.ToString());
			response.Redirect(_navigationManager.NavigateURL("ViewReservationsCalendar", strArrays));
		}

        protected void ViewReservationsCommandButtonClicked(object sender, EventArgs e)
		{
			HttpResponse response = Response;
			string[] strArrays = new string[2];
			int moduleId = ModuleId;
			strArrays[0] = string.Concat("mid=", moduleId.ToString());
			strArrays[1] = "List=ViewReservationsListSettings";
			response.Redirect(_navigationManager.NavigateURL("ViewReservations", strArrays));
		}

		protected bool WorkingHoursOverlap(DateTime startDateTime1, DateTime endDateTime1, DateTime startDateTime2, DateTime endDateTime2)
		{
			if (startDateTime1 <= startDateTime2 && endDateTime1 >= startDateTime2)
			{
				return true;
			}
			if (startDateTime2 > startDateTime1)
			{
				return false;
			}
			return endDateTime2 >= startDateTime1;
		}

		private class CustomFieldDefinitionInfoSortOrderComparer : IComparer<CustomFieldDefinitionInfo>
		{
			public CustomFieldDefinitionInfoSortOrderComparer()
			{
			}

			public int Compare(CustomFieldDefinitionInfo x, CustomFieldDefinitionInfo y)
			{
				return x.SortOrder.CompareTo(y.SortOrder);
			}
		}
	}
}