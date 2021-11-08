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
using Telerik.Web.UI;

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
				if (this.ReservationInfo.ReservationID == Null.NullInteger)
				{
					return this.RefundableAmount;
				}
				decimal num = new decimal();
				if (!this.ModuleSettings.AllowCategorySelection || this.SelectedCategory == 0 || this.ReservationInfo.CategoryID == 0 || this.ReservationInfo.CategoryID == Null.NullInteger)
				{
					num = this.Helper.CalculateReschedulingFee(this.ModuleSettings.FeeScheduleType, this.ModuleSettings.FlatFeeScheduleInfo, this.ModuleSettings.SeasonalFeeScheduleList, this.ReservationInfo.StartDateTime);
				}
				else
				{
					CategorySettings item = this.CategorySettingsDictionary[this.ReservationInfo.CategoryID];
					num = this.Helper.CalculateReschedulingFee(item.FeeScheduleType, item.FlatFeeScheduleInfo, item.SeasonalFeeScheduleList, this.ReservationInfo.StartDateTime);
				}
				return (this.DueAmount + this.RefundableAmount) + num;
			}
		}

		private bool AreReservationsAvailable
		{
			get
			{
				if (this.ModuleSettings.AllowCategorySelection && !this.SelectCategoryLast)
				{
					return this.GetCategoryIDsToRender().Count != 0;
				}
				return this.GetAvailableReservationDays(0).Count != 0;
			}
		}

		protected int AvailableDaysCurrentPageIndex
		{
			get
			{
				if (this.ViewState["AvailableDaysCurrentPageIndex"] == null)
				{
					return 0;
				}
				return (int)this.ViewState["AvailableDaysCurrentPageIndex"];
			}
			set
			{
				this.ViewState["AvailableDaysCurrentPageIndex"] = value;
			}
		}

		protected int AvailableDurationsCurrentPageIndex
		{
			get
			{
				if (this.ViewState["AvailableDurationsCurrentPageIndex"] == null)
				{
					return 0;
				}
				return (int)this.ViewState["AvailableDurationsCurrentPageIndex"];
			}
			set
			{
				this.ViewState["AvailableDurationsCurrentPageIndex"] = value;
			}
		}

		protected int AvailableTimesCurrentPageIndex
		{
			get
			{
				if (this.ViewState["AvailableTimesCurrentPageIndex"] == null)
				{
					return 0;
				}
				return (int)this.ViewState["AvailableTimesCurrentPageIndex"];
			}
			set
			{
				this.ViewState["AvailableTimesCurrentPageIndex"] = value;
			}
		}

		protected int AvailableTimesOfDayCurrentPageIndex
		{
			get
			{
				if (this.ViewState["AvailableTimesOfDayCurrentPageIndex"] == null)
				{
					return 0;
				}
				return (int)this.ViewState["AvailableTimesOfDayCurrentPageIndex"];
			}
			set
			{
				this.ViewState["AvailableTimesOfDayCurrentPageIndex"] = value;
			}
		}

		protected decimal CancellationAmount
		{
			get
			{
				decimal dueAmount = new decimal();
				if (this.ReservationInfo.ReservationID != Null.NullInteger)
				{
					decimal num = new decimal();
					if (this.ReservationInfo.CategoryID == 0 || this.ReservationInfo.CategoryID == Null.NullInteger)
					{
						num = this.Helper.CalculateCancellationFee(this.ModuleSettings.FeeScheduleType, this.ModuleSettings.FlatFeeScheduleInfo, this.ModuleSettings.SeasonalFeeScheduleList, this.ReservationInfo.StartDateTime);
					}
					else
					{
						CategorySettings item = this.CategorySettingsDictionary[this.ReservationInfo.CategoryID];
						num = this.Helper.CalculateCancellationFee(item.FeeScheduleType, item.FlatFeeScheduleInfo, item.SeasonalFeeScheduleList, this.ReservationInfo.StartDateTime);
					}
					dueAmount = (this.DueAmount + this.CancellationRefundableAmount) + num;
				}
				return dueAmount;
			}
		}

		protected decimal CancellationRefundableAmount
		{
			get
			{
				decimal refundableAmount = new decimal();
				if (this.ReservationInfo.ReservationID != Null.NullInteger)
				{
					foreach (Gafware.Modules.Reservations.PendingPaymentInfo pendingPaymentList in (new PendingPaymentController()).GetPendingPaymentList(base.TabModuleId))
					{
						if (pendingPaymentList.ReservationID != this.ReservationInfo.ReservationID || pendingPaymentList.Status != 1 && pendingPaymentList.Status != 7 && pendingPaymentList.Status != 5 && pendingPaymentList.Status != 6)
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
				if (this.ViewState["CategoriesCurrentPageIndex"] == null)
				{
					return 0;
				}
				return (int)this.ViewState["CategoriesCurrentPageIndex"];
			}
			set
			{
				this.ViewState["CategoriesCurrentPageIndex"] = value;
			}
		}

		protected List<CategoryInfo> CategoryList
		{
			get
			{
				if (this._CategoryList == null)
				{
					this._CategoryList = new List<CategoryInfo>();
					List<CategoryInfo> categoryList = (new CategoryController()).GetCategoryList(base.TabModuleId);
					List<RoleInfo> portalRoles = (new RoleController()).GetRoles(base.PortalId).ToList(); //.GetPortalRoles(base.PortalId);
					for (int i = 0; i < categoryList.Count; i++)
					{
						foreach (int categoryPermissionsList in (new CategorySettings(base.PortalId, base.TabModuleId, categoryList[i].CategoryID)).CategoryPermissionsList)
						{
							if (!PortalSecurity.IsInRole(this.GetRoleNameByRoleID(new ArrayList(portalRoles), categoryPermissionsList)))
							{
								continue;
							}
							this._CategoryList.Add(categoryList[i]);
							break;
						}
					}
				}
				return this._CategoryList;
			}
		}

		protected Dictionary<int, CategorySettings> CategorySettingsDictionary
		{
			get
			{
				if (this._CategorySettingsDictionary == null)
				{
					this._CategorySettingsDictionary = new Dictionary<int, CategorySettings>();
					foreach (CategoryInfo categoryList in this.CategoryList)
					{
						this._CategorySettingsDictionary.Add(categoryList.CategoryID, new CategorySettings(base.PortalId, base.TabModuleId, categoryList.CategoryID));
					}
				}
				return this._CategorySettingsDictionary;
			}
		}

		protected List<Gafware.Modules.Reservations.ReservationInfo> ComprehensiveReservationList
		{
			get
			{
				if (this._ComprehensiveReservationList == null)
				{
					this._ComprehensiveReservationList = new List<Gafware.Modules.Reservations.ReservationInfo>();
					this._ComprehensiveReservationList.AddRange(this.ReservationList);
					foreach (Gafware.Modules.Reservations.PendingApprovalInfo pendingApprovalList in this.PendingApprovalList)
					{
						Gafware.Modules.Reservations.ReservationInfo reservationInfo = new Gafware.Modules.Reservations.ReservationInfo()
						{
							CategoryID = pendingApprovalList.CategoryID,
							StartDateTime = pendingApprovalList.StartDateTime,
							Duration = pendingApprovalList.Duration
						};
						this._ComprehensiveReservationList.Add(reservationInfo);
					}
					if (this.IsProfessional)
					{
						foreach (Gafware.Modules.Reservations.PendingPaymentInfo pendingPaymentList in this.PendingPaymentList)
						{
							if ((pendingPaymentList.Status != 0 || !(pendingPaymentList.CreatedOnDate.Add(this.ModuleSettings.PendingPaymentExpiration) > DateTime.Now)) && pendingPaymentList.Status != 4)
							{
								if (pendingPaymentList.Status != 0 || !(pendingPaymentList.CreatedOnDate.Add(this.ModuleSettings.PendingPaymentExpiration) <= DateTime.Now))
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
								this._ComprehensiveReservationList.Add(reservationInfo1);
							}
						}
					}
					this._ComprehensiveReservationList.Sort((Gafware.Modules.Reservations.ReservationInfo x, Gafware.Modules.Reservations.ReservationInfo y) => x.StartDateTime.CompareTo(y.StartDateTime));
				}
				return this._ComprehensiveReservationList;
			}
		}

		private List<CustomFieldDefinitionInfo> CustomFieldDefinitionInfoList
		{
			get
			{
				if (this._CustomFieldDefinitionInfoList == null)
				{
					this._CustomFieldDefinitionInfoList = (new CustomFieldDefinitionController()).GetActiveCustomFieldDefinitionList(base.TabModuleId);
					this._CustomFieldDefinitionInfoList.Sort(new MakeReservation.CustomFieldDefinitionInfoSortOrderComparer());
				}
				return this._CustomFieldDefinitionInfoList;
			}
		}

		private List<CustomFieldValueInfo> CustomFieldValueInfoList
		{
			get
			{
				if (this._CustomFieldValueInfoList == null)
				{
					if (!Null.IsNull(this.ReservationInfo.ReservationID))
					{
						this._CustomFieldValueInfoList = (new CustomFieldValueController()).GetCustomFieldValueListByReservationID(this.ReservationInfo.ReservationID);
					}
					else if (!Null.IsNull(this.PendingPaymentInfo.PendingPaymentID))
					{
						this._CustomFieldValueInfoList = (new CustomFieldValueController()).GetCustomFieldValueListByPendingPaymentID(this.PendingPaymentInfo.PendingPaymentID);
					}
					else if (!Null.IsNull(this.PendingApprovalInfo.PendingApprovalID))
					{
						this._CustomFieldValueInfoList = (new CustomFieldValueController()).GetCustomFieldValueListByPendingApprovalID(this.PendingApprovalInfo.PendingApprovalID);
					}
				}
				return this._CustomFieldValueInfoList;
			}
		}

		protected decimal DueAmount
		{
			get
			{
				if (this.DuePendingPaymentInfo == null)
				{
					return decimal.Zero;
				}
				return this.DuePendingPaymentInfo.Amount;
			}
		}

		protected Gafware.Modules.Reservations.PendingPaymentInfo DuePendingPaymentInfo
		{
			get
			{
				if (this._DuePendingPaymentInfo == null && this.ReservationInfo.ReservationID != Null.NullInteger)
				{
					this._DuePendingPaymentInfo = Gafware.Modules.Reservations.Helper.FindPendingPaymentInfoByEventIDAndStatus((new PendingPaymentController()).GetPendingPaymentList(base.TabModuleId), this.ReservationInfo.ReservationID, 7);
				}
				return this._DuePendingPaymentInfo;
			}
		}

		protected bool HasEditPermissions
		{
			get
			{
				return (new ModuleSecurity(base.ModuleConfiguration)).HasEditPermissions;
			}
		}

		protected Gafware.Modules.Reservations.Helper Helper
		{
			get
			{
				if (this._Helper == null)
				{
					this._Helper = new Gafware.Modules.Reservations.Helper(base.PortalId, base.TabModuleId, base.LocalResourceFile);
				}
				return this._Helper;
			}
		}

		protected bool IsCashier
		{
			get
			{
				if (base.UserId == Null.NullInteger)
				{
					return false;
				}
				return this.Helper.IsCashier(base.UserId);
			}
		}

		protected bool IsModerator
		{
			get
			{
				if (base.UserId == Null.NullInteger)
				{
					return false;
				}
				return this.Helper.IsModerator(base.UserId);
			}
		}

		private bool IsProfessional
		{
			get
			{
				if (!this._IsProfesional.HasValue)
				{
					this._IsProfesional = new bool?(Gafware.Modules.Reservations.Helper.GetEdition(this.ModuleSettings.ActivationCode) != "Standard");
				}
				return this._IsProfesional.Value;
			}
		}

		protected TimeSpan MaxReservationDuration
		{
			get
			{
				if (this.ModuleSettings.AllowCategorySelection && this.SelectedCategory != 0)
				{
					return this.SelectedCategorySettings.ReservationDurationMax;
				}
				return this.ModuleSettings.ReservationDurationMax;
			}
		}

		protected TimeSpan MinReservationDuration
		{
			get
			{
				if (this.ModuleSettings.AllowCategorySelection && this.SelectedCategory != 0)
				{
					return this.SelectedCategorySettings.ReservationDuration;
				}
				return this.ModuleSettings.ReservationDuration;
			}
		}

		public ModuleActionCollection ModuleActions
		{
			get
			{
				ModuleActionCollection moduleActionCollection = new ModuleActionCollection();
				moduleActionCollection.Add(base.GetNextActionID(), Localization.GetString("Settings", base.LocalResourceFile), "", "", "action_settings.gif", base.EditUrl("EditSettings"), false, SecurityAccessLevel.Edit, true, false);
				if (!Gafware.Modules.Reservations.Helper.ValidateActivationCode(this.ModuleSettings.ActivationCode))
				{
					moduleActionCollection.Add(base.GetNextActionID(), Localization.GetString("Activate", base.LocalResourceFile), "", "", "action_settings.gif", base.EditUrl("Activate"), false, SecurityAccessLevel.Edit, true, false);
				}
				int nextActionID = base.GetNextActionID();
				string str = Localization.GetString("ManageCustomFields", base.LocalResourceFile);
				string[] strArrays = new string[1];
				int moduleId = base.ModuleId;
				strArrays[0] = string.Concat("mid=", moduleId.ToString());
				moduleActionCollection.Add(nextActionID, str, "", "", "action_settings.gif", _navigationManager.NavigateURL("ViewCustomFieldDefinitionList", strArrays), false, SecurityAccessLevel.Edit, true, false);
				return moduleActionCollection;
			}
		}

		protected Gafware.Modules.Reservations.ModuleSettings ModuleSettings
		{
			get
			{
				if (this._ModuleSettings == null)
				{
					this._ModuleSettings = new Gafware.Modules.Reservations.ModuleSettings(base.PortalId, base.TabModuleId);
				}
				return this._ModuleSettings;
			}
		}

		protected string NotAvailable
		{
			get
			{
				if (this._None == null)
				{
					this._None = Localization.GetString("NotAvailable", base.LocalResourceFile);
				}
				return this._None;
			}
		}

		protected Gafware.Modules.Reservations.PendingApprovalInfo PendingApprovalInfo
		{
			get
			{
				int num;
				if (this._PendingApprovalInfo == null)
				{
					try
					{
						if (this.ViewState["PendingApprovalID"] != null)
						{
							this._PendingApprovalInfo = (new PendingApprovalController()).GetPendingApproval((int)this.ViewState["PendingApprovalID"]);
						}
						else if (!base.IsPostBack && int.TryParse(base.Request.QueryString["PendingApprovalID"], out num))
						{
							this._PendingApprovalInfo = (new PendingApprovalController()).GetPendingApproval(num);
							if (this._PendingApprovalInfo.TabModuleID != base.TabModuleId)
							{
								this._PendingApprovalInfo = null;
							}
						}
						if (this._PendingApprovalInfo == null)
						{
							this._PendingApprovalInfo = new Gafware.Modules.Reservations.PendingApprovalInfo();
						}
					}
					catch (Exception)
					{
						this._PendingApprovalInfo = new Gafware.Modules.Reservations.PendingApprovalInfo();
					}
				}
				return this._PendingApprovalInfo;
			}
			set
			{
				this._PendingApprovalInfo = value;
			}
		}

		protected string PendingApprovalInfoCategoryName
		{
			get
			{
				return this.Helper.GetCategoryName(this.PendingApprovalInfo.CategoryID);
			}
		}

		protected List<Gafware.Modules.Reservations.PendingApprovalInfo> PendingApprovalList
		{
			get
			{
				if (this._PendingApprovalList == null)
				{
					this._PendingApprovalList = (
						from pendingApprovalInfo in (new PendingApprovalController()).GetPendingApprovalList(base.TabModuleId)
						where pendingApprovalInfo.Status == 0
						select pendingApprovalInfo).ToList<Gafware.Modules.Reservations.PendingApprovalInfo>();
				}
				return this._PendingApprovalList;
			}
		}

		protected Gafware.Modules.Reservations.PendingPaymentInfo PendingPaymentInfo
		{
			get
			{
				int num;
				if (this._PendingPaymentInfo == null)
				{
					try
					{
						if (this.ViewState["PendingPaymentID"] != null)
						{
							this._PendingPaymentInfo = (new PendingPaymentController()).GetPendingPayment((int)this.ViewState["PendingPaymentID"]);
						}
						else if (!base.IsPostBack && int.TryParse(base.Request.QueryString["PendingPaymentID"], out num))
						{
							this._PendingPaymentInfo = (new PendingPaymentController()).GetPendingPayment(num);
							if (this._PendingPaymentInfo.TabModuleID != base.TabModuleId)
							{
								this._PendingPaymentInfo = null;
							}
						}
						if (this._PendingPaymentInfo == null)
						{
							this._PendingPaymentInfo = new Gafware.Modules.Reservations.PendingPaymentInfo();
						}
					}
					catch (Exception)
					{
						this._PendingPaymentInfo = new Gafware.Modules.Reservations.PendingPaymentInfo();
					}
				}
				return this._PendingPaymentInfo;
			}
			set
			{
				this._PendingPaymentInfo = value;
			}
		}

		protected string PendingPaymentInfoCategoryName
		{
			get
			{
				return this.Helper.GetCategoryName(this.PendingPaymentInfo.CategoryID);
			}
		}

		protected List<Gafware.Modules.Reservations.PendingPaymentInfo> PendingPaymentList
		{
			get
			{
				if (this._PendingPaymentList == null)
				{
					this._PendingPaymentList = (new PendingPaymentController()).GetPendingPaymentList(base.TabModuleId);
				}
				return this._PendingPaymentList;
			}
		}

		protected string QueryStringPendingPaymentEmail
		{
			get
			{
				if (this.Page.IsPostBack)
				{
					return Null.NullString;
				}
				return base.Request.QueryString["Email"];
			}
		}

		protected PendingPaymentStatus QueryStringPendingPaymentStatus
		{
			get
			{
				PendingPaymentStatus pendingPaymentStatu;
				try
				{
					pendingPaymentStatu = (PendingPaymentStatus)Enum.Parse(typeof(PendingPaymentStatus), base.Request.QueryString["Status"]);
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
				TimeSpan minReservationDuration = this.MinReservationDuration;
				if (this.SelectedDuration != new TimeSpan())
				{
					minReservationDuration = this.SelectedDuration;
				}
				decimal num = new decimal();
				DateTime selectedDateTime = this.SelectedDateTime;
				DateTime dateTime = new DateTime();
				DateTime dateTime1 = (selectedDateTime == dateTime ? this.SelectedDate : this.SelectedDateTime);
				num = (!this.ModuleSettings.AllowCategorySelection || this.SelectedCategory == 0 ? this.Helper.CalculateSchedulingFee(this.ModuleSettings.FeeScheduleType, this.ModuleSettings.FlatFeeScheduleInfo, this.ModuleSettings.SeasonalFeeScheduleList, dateTime1, minReservationDuration) : this.Helper.CalculateSchedulingFee(this.SelectedCategorySettings.FeeScheduleType, this.SelectedCategorySettings.FlatFeeScheduleInfo, this.SelectedCategorySettings.SeasonalFeeScheduleList, dateTime1, minReservationDuration));
				decimal refundableAmount = num;
				if (this.ReservationInfo.ReservationID != Null.NullInteger)
				{
					foreach (Gafware.Modules.Reservations.PendingPaymentInfo pendingPaymentList in (new PendingPaymentController()).GetPendingPaymentList(base.TabModuleId))
					{
						if (pendingPaymentList.ReservationID != this.ReservationInfo.ReservationID || pendingPaymentList.Status != 1 && pendingPaymentList.Status != 7 && pendingPaymentList.Status != 5 && pendingPaymentList.Status != 6)
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
				if (this._ReservationController == null)
				{
					this._ReservationController = new Gafware.Modules.Reservations.ReservationController();
				}
				return this._ReservationController;
			}
		}

		protected Gafware.Modules.Reservations.ReservationInfo ReservationInfo
		{
			get
			{
				if (this._ReservationInfo == null)
				{
					try
					{
						this._ReservationInfo = this.ReservationController.GetReservation(this.ViewState["ReservationInfo"] != null ? (int)this.ViewState["ReservationInfo"] : 0);
						if (this._ReservationInfo == null)
						{
							this._ReservationInfo = new Gafware.Modules.Reservations.ReservationInfo();
						}
					}
					catch (Exception)
					{
						this._ReservationInfo = new Gafware.Modules.Reservations.ReservationInfo();
					}
				}
				return this._ReservationInfo;
			}
			set
			{
				this._ReservationInfo = value;
			}
		}

		protected List<Gafware.Modules.Reservations.ReservationInfo> ReservationList
		{
			get
			{
				if (this._ReservationList == null)
				{
					TimeSpan minTimeAhead = this.ModuleSettings.MinTimeAhead;
					int daysAhead = this.ModuleSettings.DaysAhead;
					if (this.ModuleSettings.AllowCategorySelection)
					{
						foreach (CategoryInfo categoryList in this.CategoryList)
						{
							CategorySettings item = this.CategorySettingsDictionary[categoryList.CategoryID];
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
					Gafware.Modules.Reservations.ReservationController reservationController = this.ReservationController;
					int tabModuleId = base.TabModuleId;
					DateTime now = Gafware.Modules.Reservations.Helper.GetNow(this.ModuleSettings.TimeZone);
					now = now.Add(minTimeAhead);
					DateTime date = now.Date;
					now = Gafware.Modules.Reservations.Helper.GetNow(this.ModuleSettings.TimeZone);
					now = now.Date;
					this._ReservationList = reservationController.GetReservationListByDateRangeAndCategoryID(tabModuleId, date, now.AddDays((double)(daysAhead + 1)), (!this.ModuleSettings.BindUponCategorySelection || this.SelectedCategory <= 0 ? Null.NullInteger : this.SelectedCategory));
					if (this.ModuleSettings.RequireConfirmation)
					{
						List<Gafware.Modules.Reservations.ReservationInfo> reservationInfos = new List<Gafware.Modules.Reservations.ReservationInfo>();
						foreach (Gafware.Modules.Reservations.ReservationInfo reservationInfo in this._ReservationList)
						{
							if (!reservationInfo.RequireConfirmation)
							{
								reservationInfos.Add(reservationInfo);
							}
							else if (reservationInfo.Confirmed)
							{
								reservationInfos.Add(reservationInfo);
							}
							else if (Gafware.Modules.Reservations.Helper.GetNow(this.ModuleSettings.TimeZone) <= reservationInfo.StartDateTime.AddMinutes((double)(-1 * reservationInfo.RequireConfirmationWhen)))
							{
								reservationInfos.Add(reservationInfo);
							}
							else
							{
								this.Helper.SendCancellationMail(reservationInfo);
								this.ReservationController.DeleteReservation(reservationInfo.ReservationID);
							}
						}
						this._ReservationList = reservationInfos;
					}
				}
				return this._ReservationList;
			}
		}

		private bool SelectCategoryLast
		{
			get
			{
				return this.ModuleSettings.SelectCategoryLast;
			}
		}

		protected int SelectedCategory
		{
			get
			{
				if (this.ViewState["SelectedCategory"] == null)
				{
					return 0;
				}
				return (int)this.ViewState["SelectedCategory"];
			}
			set
			{
				this.ViewState["SelectedCategory"] = value;
			}
		}

		protected CategorySettings SelectedCategorySettings
		{
			get
			{
				if (this._SelectedCategorySettings == null && this.SelectedCategory != 0)
				{
					this._SelectedCategorySettings = this.CategorySettingsDictionary[this.SelectedCategory];
				}
				return this._SelectedCategorySettings;
			}
		}

		protected DateTime SelectedDate
		{
			get
			{
				if (this.ViewState["SelectedDate"] == null)
				{
					return new DateTime();
				}
				return (DateTime)this.ViewState["SelectedDate"];
			}
			set
			{
				this.ViewState["SelectedDate"] = value;
			}
		}

		protected DateTime SelectedDateTime
		{
			get
			{
				if (this.ViewState["SelectedDateTime"] == null)
				{
					return new DateTime();
				}
				return (DateTime)this.ViewState["SelectedDateTime"];
			}
			set
			{
				this.ViewState["SelectedDateTime"] = value;
			}
		}

		protected TimeSpan SelectedDuration
		{
			get
			{
				if (this.ViewState["SelectedDuration"] == null)
				{
					return new TimeSpan();
				}
				return (TimeSpan)this.ViewState["SelectedDuration"];
			}
			set
			{
				this.ViewState["SelectedDuration"] = value;
			}
		}

		protected string SelectedTimeOfDay
		{
			get
			{
				if (this.ViewState["SelectedTimeOfDay"] == null)
				{
					return null;
				}
				return (string)this.ViewState["SelectedTimeOfDay"];
			}
			set
			{
				this.ViewState["SelectedTimeOfDay"] = value;
			}
		}

		protected Dictionary<int, List<DateTimeRange>> WorkingHoursDictionary
		{
			get
			{
				if (this._WorkingHoursDictionary == null)
				{
					this._WorkingHoursDictionary = new Dictionary<int, List<DateTimeRange>>();
					if (!this.ModuleSettings.AllowCategorySelection)
					{
						this._WorkingHoursDictionary.Add(0, this.GetWorkingHoursList(null));
					}
					else
					{
						foreach (CategoryInfo categoryList in this.CategoryList)
						{
							this._WorkingHoursDictionary.Add(categoryList.CategoryID, this.GetWorkingHoursList(this.CategorySettingsDictionary[categoryList.CategoryID]));
						}
					}
				}
				return this._WorkingHoursDictionary;
			}
		}

		protected void AddModuleMessage(string message, ModuleMessage.ModuleMessageType moduleMessageType)
		{
			Gafware.Modules.Reservations.Helper.AddModuleMessage(this, message, moduleMessageType);
		}

		protected void AddToDateTimeRangeList(List<DateTimeRange> dateTimeRangeList, DateTime startDateTime, DateTime endDateTime)
		{
			DateTimeRange dateTimeRange = this.FindAndRemoveOverlappingDateTimeRange(dateTimeRangeList, startDateTime, endDateTime);
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
			this.AddToDateTimeRangeList(dateTimeRangeList, dateTimeRange.StartDateTime, dateTimeRange.EndDateTime);
		}

		protected void ApproveCommandButtonClicked(object sender, EventArgs e)
		{
			try
			{
				this.Helper.ModifyPendingApprovalStatus(this.PendingApprovalInfo, PendingApprovalStatus.Approved, base.UserInfo);
				this.AddModuleMessage(Localization.GetString("Approved", base.LocalResourceFile), 0);
				this.BindPendingApprovalInfo();
				this.BindPendingApprovalModeration();
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
			}
		}

		protected void AvailableDaysPagerNextCommandButtonClicked(object source, EventArgs e)
		{
			this.AvailableDaysCurrentPageIndex = this.AvailableDaysCurrentPageIndex + 1;
			this.BindAvailableDays();
		}

		protected void AvailableDaysPagerPreviousCommandButtonClicked(object source, EventArgs e)
		{
			this.AvailableDaysCurrentPageIndex = this.AvailableDaysCurrentPageIndex - 1;
			if (this.AvailableDaysCurrentPageIndex < 0)
			{
				this.AvailableDaysCurrentPageIndex = 0;
			}
			this.BindAvailableDays();
		}

		protected void AvailableDaysRepeaterItemDataBound(object sender, RepeaterItemEventArgs e)
		{
			if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
			{
				DateTime selectedDate = this.SelectedDate;
				DateTime dateTime = new DateTime();
				if (selectedDate != dateTime && (DateTime)e.Item.DataItem == this.SelectedDate)
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
			this.AvailableDurationsCurrentPageIndex = this.AvailableDurationsCurrentPageIndex + 1;
			this.BindAvailableDurations();
		}

		protected void AvailableDurationsPagerPreviousCommandButtonClicked(object source, EventArgs e)
		{
			this.AvailableDurationsCurrentPageIndex = this.AvailableDurationsCurrentPageIndex - 1;
			if (this.AvailableDurationsCurrentPageIndex < 0)
			{
				this.AvailableDurationsCurrentPageIndex = 0;
			}
			this.BindAvailableDurations();
		}

		protected void AvailableDurationsRepeaterItemDataBound(object sender, RepeaterItemEventArgs e)
		{
			if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
			{
				TimeSpan selectedDuration = this.SelectedDuration;
				TimeSpan timeSpan = new TimeSpan();
				if (selectedDuration != timeSpan && (TimeSpan)e.Item.DataItem == this.SelectedDuration)
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
			this.AvailableTimesOfDayCurrentPageIndex = this.AvailableTimesOfDayCurrentPageIndex + 1;
			this.BindAvailableTimesOfDay();
		}

		protected void AvailableTimesOfDayPagerPreviousCommandButtonClicked(object source, EventArgs e)
		{
			this.AvailableTimesOfDayCurrentPageIndex = this.AvailableTimesOfDayCurrentPageIndex - 1;
			if (this.AvailableTimesOfDayCurrentPageIndex < 0)
			{
				this.AvailableTimesOfDayCurrentPageIndex = 0;
			}
			this.BindAvailableTimesOfDay();
		}

		protected void AvailableTimesOfDayRepeaterItemDataBound(object sender, RepeaterItemEventArgs e)
		{
			if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
			{
				if (this.SelectedTimeOfDay != null && (string)e.Item.DataItem == this.SelectedTimeOfDay)
				{
					e.Item.FindControl("availableTimeOfDayLinkButton").Visible = false;
					e.Item.FindControl("availableTimeOfDayLabel").Visible = true;
					return;
				}
				if ((string)e.Item.DataItem != Null.NullString && !this.IsTimeOfDayAvailable(this.SelectedCategory, this.SelectedDate, (string)e.Item.DataItem))
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
			this.AvailableTimesCurrentPageIndex = this.AvailableTimesCurrentPageIndex + 1;
			this.BindAvailableTimes();
		}

		protected void AvailableTimesPagerPreviousCommandButtonClicked(object source, EventArgs e)
		{
			this.AvailableTimesCurrentPageIndex = this.AvailableTimesCurrentPageIndex - 1;
			if (this.AvailableTimesCurrentPageIndex < 0)
			{
				this.AvailableTimesCurrentPageIndex = 0;
			}
			this.BindAvailableTimes();
		}

		protected void AvailableTimesRepeaterItemDataBound(object sender, RepeaterItemEventArgs e)
		{
			if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
			{
				DateTime selectedDateTime = this.SelectedDateTime;
				DateTime dateTime = new DateTime();
				if (selectedDateTime != dateTime && (DateTime)e.Item.DataItem == this.SelectedDateTime)
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
			List<DateTime> availableReservationDays = this.GetAvailableReservationDays(this.SelectedCategory);
			if (availableReservationDays.Count != 0)
			{
				HtmlGenericControl htmlGenericControl = this.availableDaysHorizontalScroll;
				System.Web.UI.WebControls.Calendar calendar = this.availableDaysCalendar;
				bool displayCalendar = this.ModuleSettings.DisplayCalendar;
				bool flag = displayCalendar;
				calendar.Visible = displayCalendar;
				htmlGenericControl.Visible = !flag;
				if (this.ModuleSettings.DisplayCalendar)
				{
					this.availableDaysCalendar.SelectedDate = this.SelectedDate;
					System.Web.UI.WebControls.Calendar calendar1 = this.availableDaysCalendar;
					DateTime selectedDate = this.SelectedDate;
					DateTime dateTime = new DateTime();
					calendar1.VisibleDate = (selectedDate != dateTime ? this.SelectedDate : availableReservationDays[0]);
					return;
				}
				this.availableDaysRepeater.DataSource = availableReservationDays;
				this.availableDaysRepeater.DataBind();
			}
			else if (!this.ModuleSettings.AllowCategorySelection || this.SelectCategoryLast)
			{
				this.HideAllStepTables();
				this.AddModuleMessage(Localization.GetString("NoReservations", base.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
				return;
			}
		}

		protected void BindAvailableDurations()
		{
			TimeSpan selectedDuration;
			int selectedCategory = this.SelectedCategory;
			DateTime selectedDateTime = this.SelectedDateTime;
			DateTime dateTime = new DateTime();
			List<TimeSpan> availableDurations = this.GetAvailableDurations(selectedCategory, (selectedDateTime == dateTime ? this.SelectedDate : this.SelectedDateTime));
			HtmlGenericControl htmlGenericControl = this.durationHorizontalScroll;
			DropDownList dropDownList = this.durationDropDownList;
			bool flag = false;
			this.durationListBox.Visible = false;
			bool flag1 = flag;
			bool flag2 = flag1;
			dropDownList.Visible = flag1;
			htmlGenericControl.Visible = flag2;
			if (this.ModuleSettings.DurationSelectionMode == Gafware.Modules.Reservations.ModuleSettings.SelectionModeEnum.HorizontalScroll)
			{
				this.availableDurationsRepeater.DataSource = availableDurations;
				this.availableDurationsRepeater.DataBind();
				this.durationHorizontalScroll.Visible = true;
				return;
			}
			if (this.ModuleSettings.DurationSelectionMode != Gafware.Modules.Reservations.ModuleSettings.SelectionModeEnum.DropDownList)
			{
				this.durationListBox.Items.Clear();
				foreach (TimeSpan availableDuration in availableDurations)
				{
					ListItem listItem = new ListItem(this.GetFriendlyReservationDuration(availableDuration), availableDuration.ToString());
					this.durationListBox.Items.Add(listItem);
				}
				TimeSpan timeSpan = this.SelectedDuration;
				selectedDuration = new TimeSpan();
				if (timeSpan != selectedDuration)
				{
					ListBox str = this.durationListBox;
					selectedDuration = this.SelectedDuration;
					str.SelectedValue = selectedDuration.ToString();
				}
				this.durationListBox.Visible = true;
				return;
			}
			this.durationDropDownList.Items.Clear();
			foreach (TimeSpan availableDuration1 in availableDurations)
			{
				ListItem listItem1 = new ListItem(this.GetFriendlyReservationDuration(availableDuration1), availableDuration1.ToString());
				this.durationDropDownList.Items.Add(listItem1);
			}
			TimeSpan selectedDuration1 = this.SelectedDuration;
			selectedDuration = new TimeSpan();
			if (selectedDuration1 != selectedDuration)
			{
				DropDownList str1 = this.durationDropDownList;
				selectedDuration = this.SelectedDuration;
				str1.SelectedValue = selectedDuration.ToString();
			}
			else
			{
				this.durationDropDownList.Items.Insert(0, "...");
			}
			this.durationDropDownList.Visible = true;
		}

		protected void BindAvailableTimes()
		{
			DateTime selectedDateTime;
			List<DateTime> dateTimes = (this.ModuleSettings.DisplayTimeOfDay ? this.GetAvailableReservationStartTimes(this.SelectedCategory, this.SelectedDate, this.SelectedTimeOfDay) : this.GetAvailableReservationStartTimes(this.SelectedCategory, this.SelectedDate));
			HtmlGenericControl htmlGenericControl = this.timeHorizontalScroll;
			DropDownList dropDownList = this.timeDropDownList;
			bool flag = false;
			this.timeListBox.Visible = false;
			bool flag1 = flag;
			bool flag2 = flag1;
			dropDownList.Visible = flag1;
			htmlGenericControl.Visible = flag2;
			if (this.ModuleSettings.TimeSelectionMode == Gafware.Modules.Reservations.ModuleSettings.SelectionModeEnum.HorizontalScroll)
			{
				this.availableTimesRepeater.DataSource = dateTimes;
				this.availableTimesRepeater.DataBind();
				this.timeHorizontalScroll.Visible = true;
				return;
			}
			if (this.ModuleSettings.TimeSelectionMode != Gafware.Modules.Reservations.ModuleSettings.SelectionModeEnum.DropDownList)
			{
				this.timeListBox.Items.Clear();
				foreach (DateTime dateTime in dateTimes)
				{
					ListItem listItem = new ListItem(this.GetFriendlyReservationTime(dateTime), dateTime.ToString());
					this.timeListBox.Items.Add(listItem);
				}
				DateTime selectedDateTime1 = this.SelectedDateTime;
				selectedDateTime = new DateTime();
				if (selectedDateTime1 != selectedDateTime)
				{
					ListBox str = this.timeListBox;
					selectedDateTime = this.SelectedDateTime;
					str.SelectedValue = selectedDateTime.ToString();
				}
				this.timeListBox.Visible = true;
				return;
			}
			this.timeDropDownList.Items.Clear();
			foreach (DateTime dateTime1 in dateTimes)
			{
				ListItem listItem1 = new ListItem(this.GetFriendlyReservationTime(dateTime1), dateTime1.ToString());
				this.timeDropDownList.Items.Add(listItem1);
			}
			DateTime selectedDateTime2 = this.SelectedDateTime;
			selectedDateTime = new DateTime();
			if (selectedDateTime2 != selectedDateTime)
			{
				DropDownList str1 = this.timeDropDownList;
				selectedDateTime = this.SelectedDateTime;
				str1.SelectedValue = selectedDateTime.ToString();
			}
			else
			{
				this.timeDropDownList.Items.Insert(0, "...");
			}
			this.timeDropDownList.Visible = true;
		}

		protected void BindAvailableTimesOfDay()
		{
			List<string> timesOfDayToRender = this.GetTimesOfDayToRender(this.SelectedCategory, this.SelectedDate);
			HtmlGenericControl htmlGenericControl = this.timeOfDayHorizontalScroll;
			DropDownList dropDownList = this.timeOfDayDropDownList;
			bool flag = false;
			this.timeOfDayListBox.Visible = false;
			bool flag1 = flag;
			bool flag2 = flag1;
			dropDownList.Visible = flag1;
			htmlGenericControl.Visible = flag2;
			if (this.ModuleSettings.TimeOfDaySelectionMode == Gafware.Modules.Reservations.ModuleSettings.SelectionModeEnum.HorizontalScroll)
			{
				this.availableTimesOfDayRepeater.DataSource = timesOfDayToRender;
				this.availableTimesOfDayRepeater.DataBind();
				this.timeOfDayHorizontalScroll.Visible = true;
				return;
			}
			if (this.ModuleSettings.TimeOfDaySelectionMode != Gafware.Modules.Reservations.ModuleSettings.SelectionModeEnum.DropDownList)
			{
				this.timeOfDayListBox.Items.Clear();
				foreach (string empty in timesOfDayToRender)
				{
					ListItem listItem = new ListItem(this.GetFriendlyTimeOfDay(empty), empty);
					if (this.IsTimeOfDayAvailable(this.SelectedCategory, this.SelectedDate, empty))
					{
						this.ViewState[string.Concat(empty, ".class")] = string.Empty;
					}
					else
					{
						this.ViewState[string.Concat(empty, ".class")] = "Gafware_Modules_Reservations_UnavailableListItem";
					}
					this.timeOfDayListBox.Items.Add(listItem);
				}
				if (!string.IsNullOrEmpty(this.SelectedTimeOfDay))
				{
					this.timeOfDayListBox.SelectedValue = this.SelectedTimeOfDay;
				}
				this.timeOfDayListBox.Visible = true;
				return;
			}
			this.timeOfDayDropDownList.Items.Clear();
			foreach (string str in timesOfDayToRender)
			{
				ListItem listItem1 = new ListItem(this.GetFriendlyTimeOfDay(str), str);
				if (this.IsTimeOfDayAvailable(this.SelectedCategory, this.SelectedDate, str))
				{
					this.ViewState[string.Concat(str, ".class")] = string.Empty;
				}
				else
				{
					this.ViewState[string.Concat(str, ".class")] = "Gafware_Modules_Reservations_UnavailableListItem";
				}
				this.timeOfDayDropDownList.Items.Add(listItem1);
			}
			if (!string.IsNullOrEmpty(this.SelectedTimeOfDay))
			{
				this.timeOfDayDropDownList.SelectedValue = this.SelectedTimeOfDay;
			}
			else
			{
				this.timeOfDayDropDownList.Items.Insert(0, "...");
			}
			this.timeOfDayDropDownList.Visible = true;
		}

		protected void BindCategories()
		{
			List<int> categoryIDsToRender = this.GetCategoryIDsToRender();
			if (!this.SelectCategoryLast && categoryIDsToRender.Count == 0)
			{
				this.HideAllStepTables();
				this.AddModuleMessage(Localization.GetString("NoReservations", base.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
				return;
			}
			HtmlGenericControl htmlGenericControl = this.categoriesHorizontalScroll;
			DropDownList dropDownList = this.categoriesDropDownList;
			bool flag = false;
			this.categoriesListBox.Visible = false;
			bool flag1 = flag;
			bool flag2 = flag1;
			dropDownList.Visible = flag1;
			htmlGenericControl.Visible = flag2;
			if (this.ModuleSettings.CategorySelectionMode == Gafware.Modules.Reservations.ModuleSettings.SelectionModeEnum.HorizontalScroll)
			{
				this.categoriesRepeater.DataSource = categoryIDsToRender;
				this.categoriesRepeater.DataBind();
				this.categoriesHorizontalScroll.Visible = true;
				return;
			}
			if (this.ModuleSettings.CategorySelectionMode != Gafware.Modules.Reservations.ModuleSettings.SelectionModeEnum.DropDownList)
			{
				this.categoriesListBox.Items.Clear();
				foreach (int empty in categoryIDsToRender)
				{
					ListItem listItem = new ListItem(this.GetCategoryName(empty), empty.ToString());
					if (this.ModuleSettings.BindUponCategorySelection || this.IsCategoryAvailable(empty))
					{
						this.ViewState[string.Concat(empty, ".class")] = string.Empty;
					}
					else
					{
						this.ViewState[string.Concat(empty, ".class")] = "Gafware_Modules_Reservations_UnavailableListItem";
					}
					this.categoriesListBox.Items.Add(listItem);
				}
				if (this.SelectedCategory != 0)
				{
					this.categoriesListBox.SelectedValue = this.SelectedCategory.ToString();
				}
				this.categoriesListBox.Visible = true;
				return;
			}
			this.categoriesDropDownList.Items.Clear();
			foreach (int empty1 in categoryIDsToRender)
			{
				ListItem listItem1 = new ListItem(this.GetCategoryName(empty1), empty1.ToString());
				if (this.ModuleSettings.BindUponCategorySelection || this.IsCategoryAvailable(empty1))
				{
					this.ViewState[string.Concat(empty1, ".class")] = string.Empty;
				}
				else
				{
					this.ViewState[string.Concat(empty1, ".class")] = "Gafware_Modules_Reservations_UnavailableListItem";
				}
				this.categoriesDropDownList.Items.Add(listItem1);
			}
			if (this.SelectedCategory != 0)
			{
				this.categoriesDropDownList.SelectedValue = this.SelectedCategory.ToString();
			}
			else
			{
				this.categoriesDropDownList.Items.Insert(0, "...");
			}
			this.categoriesDropDownList.Visible = true;
		}

		private void BindCustomFieldTableRowRepeater()
		{
			List<string> strs = new List<string>();
			foreach (CustomFieldDefinitionInfo customFieldDefinitionInfoList in this.CustomFieldDefinitionInfoList)
			{
				if (strs.Count != 0 && customFieldDefinitionInfoList.AddToPreviousRow)
				{
					continue;
				}
				strs.Add((customFieldDefinitionInfoList.HideLabel ? string.Empty : customFieldDefinitionInfoList.Label));
			}
			this.customFieldTableRowRepeater.DataSource = strs;
			this.customFieldTableRowRepeater.DataBind();
		}

		private void BindCustomFieldTableRowRepeater2()
		{
			this.customFieldTableRowRepeater2.DataSource = this.CustomFieldDefinitionInfoList;
			this.customFieldTableRowRepeater2.DataBind();
		}

		protected void BindPendingApprovalInfo()
		{
			this.step4CategoryLabel2.Text = this.PendingApprovalInfoCategoryName;
			this.step4CategoryTableRow.Visible = this.step4CategoryLabel2.Text != this.NotAvailable;
			this.reservationDateTimeLabel.Text = this.GetReservationDateTime(this.PendingApprovalInfo);
			this.step4NameLabel2.Text = this.PendingApprovalInfo.FullName;
			this.step4EmailLabel2.Text = this.PendingApprovalInfo.Email;
			this.step4PhoneLabel2.Text = (this.PendingApprovalInfo.Phone != string.Empty ? this.PendingApprovalInfo.Phone : this.NotAvailable);
			this.step4DescriptionLabel.Text = (this.PendingApprovalInfo.Description != string.Empty ? this.PendingApprovalInfo.Description : this.NotAvailable);
			this.cancelReservationCommandButton.Visible = false;
			this.rescheduleReservationCommandButton.Visible = false;
			if (this.IsProfessional)
			{
				this.BindCustomFieldTableRowRepeater2();
			}
		}

		protected void BindPendingApprovalModeration()
		{
			DateTime createdOnDate;
			if (base.UserId != Null.NullInteger && this.Helper.CanModerate(base.UserId, this.PendingApprovalInfo.CategoryID))
			{
				this.lastActionTable.Visible = true;
				if (this.PendingApprovalInfo.Status == 0)
				{
					this.lastActionLabel.Text = Localization.GetString("CreatedBy", base.LocalResourceFile);
					this.lastActionByDisplayNameLabel.Text = (this.PendingApprovalInfo.CreatedByDisplayName != Null.NullString ? this.PendingApprovalInfo.CreatedByDisplayName : this.PendingApprovalInfo.FullName);
					Label str = this.lastActionDateLabel;
					createdOnDate = this.PendingApprovalInfo.CreatedOnDate;
					str.Text = createdOnDate.ToString("f");
					this.approveCommandButton.Visible = true;
					this.declineCommandButton.Visible = true;
				}
				else if (this.PendingApprovalInfo.Status != 1)
				{
					this.lastActionLabel.Text = Localization.GetString("DeclinedBy", base.LocalResourceFile);
					this.lastActionByDisplayNameLabel.Text = this.PendingApprovalInfo.LastModifiedByDisplayName;
					Label label = this.lastActionDateLabel;
					createdOnDate = this.PendingApprovalInfo.LastModifiedOnDate;
					label.Text = createdOnDate.ToString("f");
					this.approveCommandButton.Visible = false;
					this.declineCommandButton.Visible = false;
				}
				else
				{
					this.lastActionLabel.Text = Localization.GetString("ApprovedBy", base.LocalResourceFile);
					this.lastActionByDisplayNameLabel.Text = this.PendingApprovalInfo.LastModifiedByDisplayName;
					Label str1 = this.lastActionDateLabel;
					createdOnDate = this.PendingApprovalInfo.LastModifiedOnDate;
					str1.Text = createdOnDate.ToString("f");
					this.approveCommandButton.Visible = false;
					this.declineCommandButton.Visible = false;
				}
				this.doneCommandButton.Visible = false;
				this.returnCommandButton.Visible = true;
			}
		}

		protected void BindPendingPaymentInfo()
		{
			this.step4CategoryLabel2.Text = this.PendingPaymentInfoCategoryName;
			this.step4CategoryTableRow.Visible = this.step4CategoryLabel2.Text != this.NotAvailable;
			this.reservationDateTimeLabel.Text = this.GetReservationDateTime(this.PendingPaymentInfo);
			this.step4NameLabel2.Text = this.PendingPaymentInfo.FullName;
			this.step4EmailLabel2.Text = this.PendingPaymentInfo.Email;
			this.step4PhoneLabel2.Text = (this.PendingPaymentInfo.Phone != string.Empty ? this.PendingPaymentInfo.Phone : this.NotAvailable);
			this.step4DescriptionLabel.Text = (this.PendingPaymentInfo.Description != string.Empty ? this.PendingPaymentInfo.Description : this.NotAvailable);
			this.cancelReservationCommandButton.Visible = false;
			this.rescheduleReservationCommandButton.Visible = false;
			if (this.IsProfessional)
			{
				this.BindCustomFieldTableRowRepeater2();
			}
		}

		protected void BindReservationInfo()
		{
			bool flag;
			bool allowCancellations = this.ModuleSettings.AllowCancellations;
			bool allowRescheduling = this.ModuleSettings.AllowRescheduling;
			bool allowSchedulingAnotherReservation = this.ModuleSettings.AllowSchedulingAnotherReservation;
			if (this.ModuleSettings.AllowCategorySelection && this.ReservationInfo.CategoryID > 0)
			{
				CategorySettings categorySetting = new CategorySettings(base.PortalId, base.TabModuleId, this.ReservationInfo.CategoryID);
				allowCancellations = categorySetting.AllowCancellations;
				allowRescheduling = categorySetting.AllowRescheduling;
			}
			this.step4CategoryLabel2.Text = (string.IsNullOrEmpty(this.ReservationInfo.CategoryName) ? this.NotAvailable : this.ReservationInfo.CategoryName);
			this.step4CategoryTableRow.Visible = this.step4CategoryLabel2.Text != this.NotAvailable;
			this.reservationDateTimeLabel.Text = this.GetReservationDateTime(this.ReservationInfo);
			this.step4NameLabel2.Text = this.ReservationInfo.FullName;
			this.step4EmailLabel2.Text = this.ReservationInfo.Email;
			this.step4PhoneLabel2.Text = (this.ReservationInfo.Phone != string.Empty ? this.ReservationInfo.Phone : this.NotAvailable);
			this.step4DescriptionLabel.Text = (this.ReservationInfo.Description != string.Empty ? this.ReservationInfo.Description : this.NotAvailable);
			this.cancelReservationCommandButton.Visible = allowCancellations;
			if (this.IsProfessional)
			{
				this.BindCustomFieldTableRowRepeater2();
			}
			this._AvailableReservationStartTimesDictionary = new Dictionary<int, List<DateTime>>();
			this._ReservationList = null;
			this._PendingApprovalList = null;
			if (this.IsProfessional)
			{
				this._PendingPaymentList = null;
			}
			this.rescheduleReservationCommandButton.Visible = (!allowRescheduling ? false : this.AreReservationsAvailable);
			if (this.ReservationInfo.RequireConfirmation)
			{
				LinkButton linkButton = this.confirmReservationCommandButton;
				if (this.ReservationInfo.Confirmed)
				{
					flag = false;
				}
				else
				{
					DateTime startDateTime = this.ReservationInfo.StartDateTime;
					flag = startDateTime.Subtract(this.ModuleSettings.SendRemindersWhen) < Gafware.Modules.Reservations.Helper.GetNow(this.ModuleSettings.TimeZone);
				}
				linkButton.Visible = flag;
			}
		}

		protected void CalendarDayRender(object sender, DayRenderEventArgs e)
		{
			List<DateTime> availableReservationDays = this.GetAvailableReservationDays((this.SelectCategoryLast ? 0 : this.SelectedCategory));
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
			this.SelectedDate = this.availableDaysCalendar.SelectedDate;
			this.SelectedDateChanged();
		}

		protected void CancelReservationCommandButtonClicked(object sender, EventArgs e)
		{
			if (this.IsProfessional)
			{
				decimal dueAmount = this.DueAmount;
				decimal cancellationAmount = this.CancellationAmount;
				if (dueAmount > decimal.Zero)
				{
					Gafware.Modules.Reservations.PendingPaymentInfo now = Gafware.Modules.Reservations.Helper.FindPendingPaymentInfoByEventIDAndStatus((new PendingPaymentController()).GetPendingPaymentList(base.TabModuleId), this.ReservationInfo.ReservationID, 7);
					now.Status = 2;
					now.LastModifiedOnDate = DateTime.Now;
					(new PendingPaymentController()).UpdatePendingPayment(now);
					this._DuePendingPaymentInfo = null;
					if (cancellationAmount > decimal.Zero)
					{
						Gafware.Modules.Reservations.Helper.AddOrUpdatePendingPaymentInfo(this.ReservationInfo, base.TabModuleId, base.PortalId, cancellationAmount, this.CancellationRefundableAmount, this.ModuleSettings.Currency, base.UserId, 7);
					}
					else if (cancellationAmount < decimal.Zero)
					{
						Gafware.Modules.Reservations.Helper.AddOrUpdatePendingPaymentInfo(this.ReservationInfo, base.TabModuleId, base.PortalId, cancellationAmount, this.CancellationRefundableAmount, this.ModuleSettings.Currency, base.UserId, 5);
						this.Helper.SendPendingCancellationRefundMail(this.ReservationInfo, cancellationAmount * decimal.MinusOne, this.ModuleSettings.Currency);
					}
				}
				else if (cancellationAmount < decimal.Zero)
				{
					Gafware.Modules.Reservations.Helper.AddOrUpdatePendingPaymentInfo(this.ReservationInfo, base.TabModuleId, base.PortalId, cancellationAmount, this.CancellationRefundableAmount, this.ModuleSettings.Currency, base.UserId, 5);
					this.Helper.SendPendingCancellationRefundMail(this.ReservationInfo, cancellationAmount * decimal.MinusOne, this.ModuleSettings.Currency);
				}
			}
			this.Helper.SendCancellationMail(this.ReservationInfo);
			this.ReservationController.DeleteReservation(this.ReservationInfo.ReservationID);
			this.cancelReservationCommandButton.Visible = false;
			this.rescheduleReservationCommandButton.Visible = false;
			this.confirmReservationCommandButton.Visible = false;
			this.scheduleAnotherReservationCommandButton.Visible = false;
			this.AddModuleMessage(Localization.GetString("ReservationCancelled", base.LocalResourceFile), 0);
			this.ReservationInfo = new Gafware.Modules.Reservations.ReservationInfo();
		}

		private bool CanScheduleReservation(int categoryID, DateTime reservationTime, TimeSpan reservationDuration)
		{
			int maxConflictingReservations;
			CategorySettings item;
			if (this.CategorySettingsDictionary.ContainsKey(categoryID))
			{
				item = this.CategorySettingsDictionary[categoryID];
			}
			else
			{
				item = null;
			}
			CategorySettings categorySetting = item;
			bool flag = false;
			foreach (DateTimeRange dateTimeRange in (IEnumerable)this.WorkingHoursDictionary[categoryID])
			{
				if (!(dateTimeRange.StartDateTime <= reservationTime) || !(dateTimeRange.EndDateTime >= reservationTime.Add(reservationDuration)))
				{
					if (dateTimeRange.StartDateTime <= reservationTime)
					{
						continue;
					}
					if (flag)
					{
						maxConflictingReservations = this.ModuleSettings.MaxConflictingReservations;
						if (this.ModuleSettings.AllowCategorySelection && !this.ModuleSettings.PreventCrossCategoryConflicts)
						{
							maxConflictingReservations = categorySetting.MaxConflictingReservations;
						}
						if (this.GetHighestNumberOfConflictingReservations(this.ComprehensiveReservationList, categoryID, reservationTime, reservationDuration) >= maxConflictingReservations)
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
						maxConflictingReservations = this.ModuleSettings.MaxConflictingReservations;
						if (this.ModuleSettings.AllowCategorySelection && !this.ModuleSettings.PreventCrossCategoryConflicts)
						{
							maxConflictingReservations = categorySetting.MaxConflictingReservations;
						}
						if (this.GetHighestNumberOfConflictingReservations(this.ComprehensiveReservationList, categoryID, reservationTime, reservationDuration) >= maxConflictingReservations)
						{
							flag = false;
						}
					}
					return flag;
				}
			}
			if (flag)
			{
				maxConflictingReservations = this.ModuleSettings.MaxConflictingReservations;
				if (this.ModuleSettings.AllowCategorySelection && !this.ModuleSettings.PreventCrossCategoryConflicts)
				{
					maxConflictingReservations = categorySetting.MaxConflictingReservations;
				}
				if (this.GetHighestNumberOfConflictingReservations(this.ComprehensiveReservationList, categoryID, reservationTime, reservationDuration) >= maxConflictingReservations)
				{
					flag = false;
				}
			}
			return flag;
		}

		protected void CashierCommandButtonClicked(object sender, EventArgs e)
		{
			HttpResponse response = base.Response;
			string[] strArrays = new string[1];
			int moduleId = base.ModuleId;
			strArrays[0] = string.Concat("mid=", moduleId.ToString());
			response.Redirect(_navigationManager.NavigateURL("Cashier", strArrays));
		}

		protected void CategoriesDropDownListSelectedIndexChanged(object sender, EventArgs e)
		{
			this.SelectedCategory = int.Parse(this.categoriesDropDownList.SelectedValue);
			this.SelectedCategoryChanged();
		}

		protected void CategoriesListBoxSelectedIndexChanged(object sender, EventArgs e)
		{
			this.SelectedCategory = int.Parse(this.categoriesListBox.SelectedValue);
			this.SelectedCategoryChanged();
		}

		protected void CategoriesRepeaterItemDataBound(object sender, RepeaterItemEventArgs e)
		{
			if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
			{
				if (this.SelectedCategory != 0 && (int)e.Item.DataItem == this.SelectedCategory)
				{
					e.Item.FindControl("categoryLinkButton").Visible = false;
					e.Item.FindControl("categoryLabel").Visible = true;
					return;
				}
				if ((int)e.Item.DataItem != Null.NullInteger && !this.ModuleSettings.BindUponCategorySelection && !this.IsCategoryAvailable((int)e.Item.DataItem))
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
			this.SelectedCategory = int.Parse((string)e.CommandArgument);
			this.SelectedCategoryChanged();
		}

		private List<CustomFieldValueInfo> CollectCustomFieldValues()
		{
			string shortDateString;
			bool flag;
			List<CustomFieldValueInfo> customFieldValueInfos = new List<CustomFieldValueInfo>();
			int num = -1;
			int num1 = 0;
			foreach (CustomFieldDefinitionInfo customFieldDefinitionInfoList in this.CustomFieldDefinitionInfoList)
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
				Repeater repeater = (Repeater)this.customFieldTableRowRepeater.Items[num].FindControl("customFieldTableCellRepeater");
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
					DateTime? selectedDate = ((RadDatePicker)repeater.Items[num1].FindControl("datePicker")).SelectedDate;
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
				customFieldValueInfo.CreatedByUserID = base.UserId;
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
			TimeSpan selectedDuration = this.SelectedDuration;
			TimeSpan timeSpan = new TimeSpan();
			if (selectedDuration != timeSpan)
			{
				TimeSpan selectedDuration1 = this.SelectedDuration;
			}
			else
			{
				TimeSpan minReservationDuration = this.MinReservationDuration;
			}
			if (!this.SelectCategoryLast)
			{
				TimeSpan timeSpan1 = this.SelectedDuration;
				timeSpan = new TimeSpan();
				if (timeSpan1 == timeSpan)
				{
					DateTime selectedDateTime = this.SelectedDateTime;
					DateTime dateTime = new DateTime();
					flag1 = (selectedDateTime != dateTime ? this.SelectedTimeChanged() : this.SelectedDateChanged());
				}
				else
				{
					flag1 = this.SelectedDurationChanged();
				}
			}
			else
			{
				flag1 = this.SelectedCategoryChanged();
			}
			if (flag1)
			{
				bool reservationID = this.ReservationInfo.ReservationID == Null.NullInteger;
				this.PopulateEventInfoFromInput();
				List<CustomFieldValueInfo> customFieldValueInfos = null;
				if (this.IsProfessional)
				{
					customFieldValueInfos = this.CollectCustomFieldValues();
				}
				bool allowCancellations = this.ModuleSettings.AllowCancellations;
				bool allowRescheduling = this.ModuleSettings.AllowRescheduling;
				bool moderate = this.ModuleSettings.Moderate;
				int maxReservationsPerUser = this.ModuleSettings.MaxReservationsPerUser;
				if (this.ModuleSettings.AllowCategorySelection)
				{
					bool allowCancellations1 = this.SelectedCategorySettings.AllowCancellations;
					bool allowRescheduling1 = this.SelectedCategorySettings.AllowRescheduling;
					moderate = this.SelectedCategorySettings.Moderate;
					maxReservationsPerUser = this.SelectedCategorySettings.MaxReservationsPerUser;
				}
				if (reservationID && maxReservationsPerUser != Null.NullInteger)
				{
					int num = 0;
					foreach (Gafware.Modules.Reservations.ReservationInfo comprehensiveReservationList in this.ComprehensiveReservationList)
					{
						if (string.IsNullOrEmpty(comprehensiveReservationList.Email) || !(comprehensiveReservationList.Email == this.ReservationInfo.Email))
						{
							continue;
						}
						if (comprehensiveReservationList.CategoryID != this.SelectedCategory)
						{
							if (comprehensiveReservationList.CategoryID != (this.SelectedCategory == 0 ? -1 : this.SelectedCategory))
							{
								continue;
							}
						}
						num++;
					}
					if (num >= maxReservationsPerUser)
					{
						this.AddModuleMessage(Localization.GetString("MaxReservationsPerUserReached", base.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
						return;
					}
				}
				if (!moderate)
				{
					flag = false;
				}
				else
				{
					Gafware.Modules.Reservations.Helper helper = this.Helper;
					if (this.SelectedCategorySettings != null)
					{
						settings = this.SelectedCategorySettings.Settings;
					}
					else
					{
						settings = null;
					}
					flag = helper.MustModerate(settings, this.ModuleSettings.Settings, this.ReservationInfo.StartDateTime, this.ReservationInfo.Duration);
				}
				moderate = flag;
				decimal amount = new decimal();
				decimal refundableAmount = new decimal();
				decimal dueAmount = new decimal();
				if (this.IsProfessional)
				{
					amount = this.Amount;
					refundableAmount = this.RefundableAmount;
					dueAmount = this.DueAmount;
					if (pay && amount > decimal.Zero)
					{
						Gafware.Modules.Reservations.PendingPaymentInfo pendingPaymentInfo = (new PendingPaymentController()).AddPendingPayment(Gafware.Modules.Reservations.Helper.CreatePendingPaymentInfoFromEventInfo(this.ReservationInfo, base.TabModuleId, base.PortalId, amount - dueAmount, refundableAmount, this.ModuleSettings.Currency, base.UserId, 0));
						if (this.ReservationInfo.ReservationID == Null.NullInteger)
						{
							CustomFieldValueController customFieldValueController = new CustomFieldValueController();
							foreach (CustomFieldValueInfo pendingPaymentID in customFieldValueInfos)
							{
								pendingPaymentID.PendingPaymentID = pendingPaymentInfo.PendingPaymentID;
								customFieldValueController.AddCustomFieldValue(pendingPaymentID);
							}
						}
						this.MakePayment(pendingPaymentInfo, amount);
						return;
					}
				}
				if (moderate && (base.UserId == Null.NullInteger || !this.Helper.CanModerate(base.UserId, this.SelectedCategory)))
				{
					this.PendingApprovalInfo = (new PendingApprovalController()).AddPendingApproval(Gafware.Modules.Reservations.Helper.CreatePendingApprovalInfoFromEventInfo(this.ReservationInfo, base.TabModuleId, base.PortalId, base.UserId, 0));
					if (this.IsProfessional)
					{
						if (amount != decimal.Zero)
						{
							(new PendingPaymentController()).AddPendingPayment(Gafware.Modules.Reservations.Helper.CreatePendingPaymentInfoFromPendingApprovalInfo(this.PendingApprovalInfo, amount, refundableAmount, this.ModuleSettings.Currency, PendingPaymentStatus.PendingApproval));
						}
						if (this.ReservationInfo.ReservationID == Null.NullInteger)
						{
							CustomFieldValueController customFieldValueController1 = new CustomFieldValueController();
							foreach (CustomFieldValueInfo pendingApprovalID in customFieldValueInfos)
							{
								pendingApprovalID.PendingApprovalID = this.PendingApprovalInfo.PendingApprovalID;
								customFieldValueController1.AddCustomFieldValue(pendingApprovalID);
							}
						}
					}
					this.AddModuleMessage(Localization.GetString("ReservationPendingApproval", base.LocalResourceFile), 0);
					this.Helper.SendModeratorMail(this.PendingApprovalInfo);
					this.HideAllStepTables();
					this.step4Table.Visible = true;
					this.BindPendingApprovalInfo();
					this.ReservationInfo = new Gafware.Modules.Reservations.ReservationInfo();
					return;
				}
				this.ReservationInfo = this.ReservationController.SaveReservation(this.ReservationInfo);
				if (this.IsProfessional)
				{
					if (reservationID)
					{
						CustomFieldValueController customFieldValueController2 = new CustomFieldValueController();
						foreach (CustomFieldValueInfo customFieldValueInfo in customFieldValueInfos)
						{
							customFieldValueInfo.ReservationID = this.ReservationInfo.ReservationID;
							customFieldValueController2.AddCustomFieldValue(customFieldValueInfo);
						}
					}
					if (!reservationID)
					{
						Gafware.Modules.Reservations.Helper.UpdateDueAndPendingRefundPaymentInfoFromEventInfo(this.ReservationInfo, base.TabModuleId);
					}
					if (amount < decimal.Zero)
					{
						PendingPaymentController pendingPaymentController = new PendingPaymentController();
						Gafware.Modules.Reservations.PendingPaymentInfo now = null;
						if (!reservationID)
						{
							now = Gafware.Modules.Reservations.Helper.FindPendingPaymentInfoByEventIDAndStatus(pendingPaymentController.GetPendingPaymentList(base.TabModuleId), this.ReservationInfo.ReservationID, 5);
						}
						if (now == null)
						{
							now = pendingPaymentController.AddPendingPayment(Gafware.Modules.Reservations.Helper.CreatePendingPaymentInfoFromEventInfo(this.ReservationInfo, base.TabModuleId, base.PortalId, amount, refundableAmount, this.ModuleSettings.Currency, base.UserId, 5));
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
						this.Helper.SendPendingRescheduleRefundMail(this.ReservationInfo, amount * decimal.MinusOne, this.ModuleSettings.Currency);
					}
					else if (amount > decimal.Zero)
					{
						PendingPaymentController pendingPaymentController1 = new PendingPaymentController();
						Gafware.Modules.Reservations.PendingPaymentInfo now1 = null;
						if (!reservationID)
						{
							now1 = Gafware.Modules.Reservations.Helper.FindPendingPaymentInfoByEventIDAndStatus(pendingPaymentController1.GetPendingPaymentList(base.TabModuleId), this.ReservationInfo.ReservationID, 7);
						}
						if (now1 == null)
						{
							now1 = pendingPaymentController1.AddPendingPayment(Gafware.Modules.Reservations.Helper.CreatePendingPaymentInfoFromEventInfo(this.ReservationInfo, base.TabModuleId, base.PortalId, amount, refundableAmount, this.ModuleSettings.Currency, base.UserId, 7));
						}
						else
						{
							Gafware.Modules.Reservations.PendingPaymentInfo pendingPaymentInfo1 = now1;
							pendingPaymentInfo1.Amount = pendingPaymentInfo1.Amount + (amount - dueAmount);
							Gafware.Modules.Reservations.PendingPaymentInfo refundableAmount2 = now1;
							refundableAmount2.RefundableAmount = refundableAmount2.RefundableAmount + refundableAmount;
							now1.LastModifiedOnDate = DateTime.Now;
							pendingPaymentController1.UpdatePendingPayment(now1);
							this._DuePendingPaymentInfo = null;
						}
					}
				}
				this.HideAllStepTables();
				this.step4Table.Visible = true;
				this.AddModuleMessage(Localization.GetString("ReservationScheduled", base.LocalResourceFile), 0);
				if (!reservationID)
				{
					this.Helper.SendRescheduledMail(this.ReservationInfo);
				}
				else
				{
					this.Helper.SendConfirmationMail(this.ReservationInfo);
				}
				this.DisplayScheduleAnotherReservation();
				this.BindReservationInfo();
			}
		}

		protected void ConfirmReservationCommandButtonClicked(object sender, EventArgs e)
		{
			this.ReservationInfo.Confirmed = true;
			this.ReservationController.UpdateReservation(this.ReservationInfo);
			this.AddModuleMessage(Localization.GetString("ReservationConfirmed", base.LocalResourceFile), 0);
			this.confirmReservationCommandButton.Visible = false;
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
				double num = 100 / (double)this._NumberOfDivs;
				HtmlGenericControl label = (HtmlGenericControl)e.Item.FindControl("customFieldLabelTableCell");
				label.Style.Add("width", string.Concat(num, "%"));
				((HtmlGenericControl)e.Item.FindControl("customFieldTableCell")).Style.Add("width", string.Concat(num, "%"));
				CustomFieldDefinitionInfo dataItem = (CustomFieldDefinitionInfo)e.Item.DataItem;
				string customFieldValue = this.GetCustomFieldValue(dataItem, false);
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
						TextBox textBox1 = textBox;
						textBox1.CssClass = string.Concat(textBox1.CssClass, " Gafware_Modules_Reservations_Required");
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
					RadDatePicker radDatePicker = (RadDatePicker)e.Item.FindControl("datePicker");
					radDatePicker.Visible = true;
					if (DateTime.TryParse(customFieldValue, out dateTime))
					{
						radDatePicker.SelectedDate = new DateTime?(dateTime);
					}
					if (dataItem.IsRequired)
					{
						RadDatePicker radDatePicker1 = radDatePicker;
						radDatePicker1.CssClass = string.Concat(radDatePicker1.CssClass, " Gafware_Modules_Reservations_Required");
					}
					isRequired.ControlToValidate = radDatePicker.ID;
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
						DropDownList dropDownList1 = dropDownList;
						dropDownList1.CssClass = string.Concat(dropDownList1.CssClass, " Gafware_Modules_Reservations_Required");
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
			this._NumberOfDivs = 1;
			if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
			{
				int num = 0;
				int num1 = -1;
				List<CustomFieldDefinitionInfo> customFieldDefinitionInfos = new List<CustomFieldDefinitionInfo>();
				foreach (CustomFieldDefinitionInfo customFieldDefinitionInfoList in this.CustomFieldDefinitionInfoList)
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
							this._NumberOfDivs = this._NumberOfDivs + 1;
						}
						this._NumberOfDivs = this._NumberOfDivs + 1;
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
			this.SelectedDate = DateTime.Parse((string)e.CommandArgument);
			this.SelectedDateChanged();
		}

		protected void DateTimeLinkButtonClicked(object sender, CommandEventArgs e)
		{
			this.SelectedDateTime = DateTime.Parse((string)e.CommandArgument);
			this.SelectedTimeChanged();
		}

		protected void DeclineCommandButtonClicked(object sender, EventArgs e)
		{
			try
			{
				this.Helper.ModifyPendingApprovalStatus(this.PendingApprovalInfo, PendingApprovalStatus.Declined, base.UserInfo);
				this.AddModuleMessage(Localization.GetString("Declined", base.LocalResourceFile), 0);
				this.BindPendingApprovalInfo();
				this.BindPendingApprovalModeration();
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
			}
		}

		private void DisplayScheduleAnotherReservation()
		{
			this.scheduleAnotherReservationCommandButton.Visible = (!this.ModuleSettings.AllowSchedulingAnotherReservation ? false : this.AreReservationsAvailable);
		}

		protected void DoneCommandButtonClicked(object sender, EventArgs e)
		{
			if (this.ModuleSettings.RedirectUrl == string.Empty)
			{
				base.Response.Redirect(_navigationManager.NavigateURL());
				return;
			}
			base.Response.Redirect(this.ModuleSettings.RedirectUrl);
		}

		protected void DuplicateReservationsCommandButtonClicked(object sender, EventArgs e)
		{
			HttpResponse response = base.Response;
			string[] strArrays = new string[2];
			int moduleId = base.ModuleId;
			strArrays[0] = string.Concat("mid=", moduleId.ToString());
			strArrays[1] = "List=DuplicateReservationsListSettings";
			response.Redirect(_navigationManager.NavigateURL("DuplicateReservations", strArrays));
		}

		protected void DurationDropDownListSelectedIndexChanged(object sender, EventArgs e)
		{
			this.SelectedDuration = TimeSpan.Parse(this.durationDropDownList.SelectedValue);
			this.SelectedDurationChanged();
		}

		protected void DurationLinkButtonClicked(object sender, CommandEventArgs e)
		{
			this.SelectedDuration = TimeSpan.Parse((string)e.CommandArgument);
			this.SelectedDurationChanged();
		}

		protected void DurationListBoxSelectedIndexChanged(object sender, EventArgs e)
		{
			this.SelectedDuration = TimeSpan.Parse(this.durationListBox.SelectedValue);
			this.SelectedDurationChanged();
		}

		protected DateTimeRange FindAndRemoveOverlappingDateTimeRange(List<DateTimeRange> dateTimeRangeList, DateTime startDateTime, DateTime endDateTime)
		{
			DateTimeRange item;
			int num = -1;
			int num1 = 0;
			foreach (DateTimeRange dateTimeRange in dateTimeRangeList)
			{
				if (!this.WorkingHoursOverlap(startDateTime, endDateTime, dateTimeRange.StartDateTime, dateTimeRange.EndDateTime))
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
				from c in controls.SelectMany<Control, Control>((Control ctrl) => this.FindChildControlsByType(ctrl, type)).Concat<Control>(controls)
				where c.GetType() == type
				select c;
		}

		private List<CategoryInfo> GetAvailableCategories(DateTime date)
		{
			List<CategoryInfo> categoryInfos = new List<CategoryInfo>();
			foreach (CategoryInfo categoryList in this.CategoryList)
			{
				if (this.GetAvailableReservationStartTimes(categoryList.CategoryID, date).Count <= 0)
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
			foreach (CategoryInfo categoryList in this.CategoryList)
			{
				if (!this.GetAvailableReservationStartTimes(categoryList.CategoryID).Contains(date.Date.Add(time)))
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
			foreach (CategoryInfo categoryList in this.CategoryList)
			{
				if (!this.GetAvailableDurations(categoryList.CategoryID, date.Date.Add(time)).Contains(duration))
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
			if (this.ModuleSettings.AllowCategorySelection && categoryID == 0)
			{
				foreach (CategoryInfo availableCategory in this.GetAvailableCategories(dateTime.Date, dateTime.TimeOfDay))
				{
					timeSpans.AddRange(this.GetAvailableDurations(availableCategory.CategoryID, dateTime));
				}
				timeSpans = timeSpans.Distinct<TimeSpan>().ToList<TimeSpan>();
				timeSpans.Sort();
				return timeSpans;
			}
			TimeSpan reservationDuration = this.ModuleSettings.ReservationDuration;
			TimeSpan reservationDurationMax = this.ModuleSettings.ReservationDurationMax;
			TimeSpan reservationDurationInterval = this.ModuleSettings.ReservationDurationInterval;
			if (this.ModuleSettings.AllowCategorySelection)
			{
				reservationDuration = this.CategorySettingsDictionary[categoryID].ReservationDuration;
				reservationDurationMax = this.CategorySettingsDictionary[categoryID].ReservationDurationMax;
				reservationDurationInterval = this.CategorySettingsDictionary[categoryID].ReservationDurationInterval;
			}
			for (TimeSpan i = reservationDuration; i <= reservationDurationMax && this.CanScheduleReservation(categoryID, dateTime, i); i = i.Add(reservationDurationInterval))
			{
				timeSpans.Add(i);
			}
			return timeSpans;
		}

		private List<DateTime> GetAvailableReservationDays(int categoryID)
		{
			if (this._AvailableReservationDays.ContainsKey(categoryID))
			{
				return this._AvailableReservationDays[categoryID];
			}
			List<DateTime> dateTimes = new List<DateTime>();
			foreach (DateTime availableReservationStartTime in this.GetAvailableReservationStartTimes(categoryID))
			{
				if (dateTimes.Contains(availableReservationStartTime.Date))
				{
					continue;
				}
				dateTimes.Add(availableReservationStartTime.Date);
			}
			this._AvailableReservationDays.Add(categoryID, dateTimes);
			return dateTimes;
		}

		protected string GetAvailableReservationsCountToRender(int categoryID)
		{
			if (categoryID == Null.NullInteger)
			{
				return Null.NullString;
			}
			return string.Concat("(", this.GetAvailableReservationStartTimes(categoryID).Count, ")");
		}

		private List<DateTime> GetAvailableReservationStartTimes(int categoryID)
		{
			if (this._AvailableReservationStartTimesDictionary.ContainsKey(categoryID))
			{
				return this._AvailableReservationStartTimesDictionary[categoryID];
			}
			List<DateTime> dateTimes = new List<DateTime>();
			if (this.ModuleSettings.AllowCategorySelection && categoryID == 0)
			{
				foreach (CategoryInfo categoryList in this.CategoryList)
				{
					dateTimes.AddRange(this.GetAvailableReservationStartTimes(categoryList.CategoryID));
				}
				dateTimes = dateTimes.Distinct<DateTime>().ToList<DateTime>();
				dateTimes.Sort();
				this._AvailableReservationStartTimesDictionary.Add(categoryID, dateTimes);
				return dateTimes;
			}
			TimeSpan minTimeAhead = this.ModuleSettings.MinTimeAhead;
			int daysAhead = this.ModuleSettings.DaysAhead;
			TimeSpan reservationInterval = this.ModuleSettings.ReservationInterval;
			TimeSpan reservationDuration = this.ModuleSettings.ReservationDuration;
			if (this.ModuleSettings.AllowCategorySelection)
			{
				CategorySettings item = this.CategorySettingsDictionary[categoryID];
				daysAhead = item.DaysAhead;
				minTimeAhead = item.MinTimeAhead;
				reservationInterval = item.ReservationInterval;
				reservationDuration = item.ReservationDuration;
			}
			DateTime date = DateTime.Now.Date;
			DateTime dateTime = date.Add(new TimeSpan(daysAhead, 0, 0, 0));
			date = DateTime.Now.Add(minTimeAhead);
			DateTime date1 = date.Date;
			List<DateTimeRange> dateTimeRanges = this.WorkingHoursDictionary[categoryID];
			while (date1 <= dateTime)
			{
				foreach (DateTime timeSlot in this.GetTimeSlots(dateTimeRanges, date1, reservationInterval))
				{
					if (!(timeSlot > DateTime.Now.Add(minTimeAhead)) || !this.CanScheduleReservation(categoryID, timeSlot, reservationDuration))
					{
						continue;
					}
					dateTimes.Add(timeSlot);
				}
				date1 = date1.AddDays(1);
			}
			this._AvailableReservationStartTimesDictionary.Add(categoryID, dateTimes);
			return dateTimes;
		}

		private List<DateTime> GetAvailableReservationStartTimes(int categoryID, DateTime date)
		{
			List<DateTime> dateTimes = new List<DateTime>();
			List<DateTime> availableReservationStartTimes = this.GetAvailableReservationStartTimes(categoryID);
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
			foreach (TimeOfDayInfo timeOfDayList in this.ModuleSettings.TimeOfDayList)
			{
				if (timeOfDayList.Name != timeOfDayName)
				{
					continue;
				}
				foreach (DateTime availableReservationStartTime in this.GetAvailableReservationStartTimes(categoryID, date))
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
			return string.Concat("(", this.GetAvailableReservationStartTimes(this.SelectedCategory, this.SelectedDate, timeOfDay).Count, ")");
		}

		private List<int> GetCategoryIDsToRender()
		{
			List<int> nums = new List<int>();
			if (!this.SelectCategoryLast)
			{
				foreach (CategoryInfo categoryList in this.CategoryList)
				{
					if (!this.ModuleSettings.BindUponCategorySelection && !this.ModuleSettings.DisplayUnavailableCategories && this.GetAvailableReservationDays(categoryList.CategoryID).Count <= 1)
					{
						continue;
					}
					nums.Add(categoryList.CategoryID);
				}
			}
			else
			{
				foreach (CategoryInfo categoryInfo in this.CategoryList)
				{
					TimeSpan reservationDuration = this.CategorySettingsDictionary[categoryInfo.CategoryID].ReservationDuration;
					if (!this.ModuleSettings.DisplayUnavailableCategories)
					{
						DateTime selectedDate = this.SelectedDate;
						TimeSpan timeOfDay = this.SelectedDateTime.TimeOfDay;
						TimeSpan selectedDuration = this.SelectedDuration;
						TimeSpan timeSpan = new TimeSpan();
						if (this.GetAvailableCategories(selectedDate, timeOfDay, (selectedDuration != timeSpan ? this.SelectedDuration : reservationDuration)).Count<CategoryInfo>((CategoryInfo _categoryInfo) => _categoryInfo.CategoryID == categoryInfo.CategoryID) == 0)
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
			return string.Concat(this.Helper.GetCategoryName(categoryID), (this.SelectCategoryLast || this.ModuleSettings.BindUponCategorySelection || !this.ModuleSettings.DisplayRemainingReservations ? string.Empty : string.Concat(" ", this.GetAvailableReservationsCountToRender(categoryID))));
		}

		protected string GetCustomFieldValue(CustomFieldDefinitionInfo customFieldDefinitionInfo, bool localize = true)
		{
			string empty = string.Empty;
			if (this.CustomFieldValueInfoList != null)
			{
				List<CustomFieldValueInfo> list = (
					from customFieldValueInfo in this.CustomFieldValueInfoList
					where customFieldValueInfo.CustomFieldDefinitionID == customFieldDefinitionInfo.CustomFieldDefinitionID
					select customFieldValueInfo).ToList<CustomFieldValueInfo>();
				if (list.Count != 0)
				{
					empty = list[0].Value;
					if (localize && customFieldDefinitionInfo.CustomFieldDefinitionType == CustomFieldDefinitionType.CheckBox)
					{
						empty = Localization.GetString(empty, base.LocalResourceFile);
					}
				}
			}
			if (localize && string.IsNullOrEmpty(empty))
			{
				empty = this.NotAvailable;
			}
			return empty;
		}

		protected string GetFriendlyDate(DateTime dateTime)
		{
			string str;
			str = (CultureInfo.CurrentCulture.DateTimeFormat.LongDatePattern.StartsWith("dddd") ? "ddd " : string.Empty);
			string str1 = CultureInfo.CurrentCulture.DateTimeFormat.MonthDayPattern.Replace("MMMM", "MMM");
			int day = dateTime.Day;
			return dateTime.ToString(string.Concat(str, str1.Replace("dd", string.Concat(day.ToString(), (Thread.CurrentThread.CurrentCulture.ToString().StartsWith("en") ? string.Concat("'", this.GetOrdinal(dateTime.Day), "'") : string.Empty)))));
		}

		protected string GetFriendlyReservationDuration(TimeSpan timeSpan)
		{
			if (timeSpan.TotalMinutes % 10080 == 0)
			{
				return string.Concat(timeSpan.TotalDays / 7, (timeSpan.TotalDays / 7 > 1 ? Localization.GetString("Weeks", base.LocalResourceFile) : Localization.GetString("Week", base.LocalResourceFile)));
			}
			if (timeSpan.TotalMinutes % 1440 == 0)
			{
				return string.Concat(timeSpan.TotalDays, (timeSpan.TotalDays > 1 ? Localization.GetString("Days", base.LocalResourceFile) : Localization.GetString("Day", base.LocalResourceFile)));
			}
			if (timeSpan.TotalDays > 1 && timeSpan.TotalMinutes % 360 == 0)
			{
				return string.Concat(timeSpan.TotalDays, Localization.GetString("Days", base.LocalResourceFile));
			}
			if (timeSpan.TotalMinutes % 15 == 0 && timeSpan.TotalHours >= 1)
			{
				return string.Concat(timeSpan.TotalHours, (timeSpan.TotalHours > 1 ? Localization.GetString("Hours", base.LocalResourceFile) : Localization.GetString("Hour", base.LocalResourceFile)));
			}
			return string.Concat(timeSpan.TotalMinutes, (timeSpan.TotalMinutes > 1 ? Localization.GetString("Minutes", base.LocalResourceFile) : Localization.GetString("Minute", base.LocalResourceFile)));
		}

		protected string GetFriendlyReservationTime(DateTime dateTime)
		{
			TimeSpan reservationDuration = this.ModuleSettings.ReservationDuration;
			if (this.SelectedCategorySettings != null)
			{
				reservationDuration = this.SelectedCategorySettings.ReservationDuration;
			}
			if (!this.ModuleSettings.DisplayEndTime)
			{
				return this.GetFriendlyTime(dateTime);
			}
			return string.Concat(this.GetFriendlyTime(dateTime).Replace(":00am", "am").Replace(":00pm", "pm"), " - ", this.GetFriendlyTime(dateTime.Add(reservationDuration)).Replace(":00am", "am").Replace(":00pm", "pm"));
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
			return string.Concat(timeOfDay, (this.ModuleSettings.DisplayRemainingReservations ? string.Concat(" ", this.GetAvailableTimesCount(timeOfDay)) : string.Empty));
		}

		protected int GetHighestNumberOfConflictingReservations(List<Gafware.Modules.Reservations.ReservationInfo> reservations, int categoryID, DateTime dateTime, TimeSpan duration)
		{
			ArrayList arrayLists = new ArrayList();
			foreach (Gafware.Modules.Reservations.ReservationInfo reservation in reservations)
			{
				if (reservation.StartDateTime < dateTime.Add(duration))
				{
					if (!this.ModuleSettings.PreventCrossCategoryConflicts && categoryID != 0 && reservation.CategoryID != categoryID || !this.Conflicts(dateTime, (int)duration.TotalMinutes, reservation.StartDateTime, reservation.Duration))
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
					if (!this.Conflicts(i, 1, arrayList.StartDateTime, arrayList.Duration))
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
			return this.GetReservationDateTime(reservationInfo.StartDateTime, (double)reservationInfo.Duration);
		}

		protected string GetReservationDateTime(Gafware.Modules.Reservations.PendingApprovalInfo eventInfo)
		{
			return this.GetReservationDateTime(eventInfo.StartDateTime, (double)eventInfo.Duration);
		}

		protected string GetReservationDateTime(Gafware.Modules.Reservations.PendingPaymentInfo eventInfo)
		{
			return this.GetReservationDateTime(eventInfo.StartDateTime, (double)eventInfo.Duration);
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
				if (!this.Conflicts(dateTimeRange.StartDateTime, (int)dateTimeRange.EndDateTime.Subtract(dateTimeRange.StartDateTime).TotalMinutes, date, 1440))
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
			List<DateTime> availableReservationStartTimes = this.GetAvailableReservationStartTimes(categoryID, dateTime);
			if (availableReservationStartTimes.Count != 0)
			{
				foreach (TimeOfDayInfo timeOfDayList in this.ModuleSettings.TimeOfDayList)
				{
					if (this.ModuleSettings.DisplayUnavailableTimeOfDay)
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
			TimeSpan minTimeAhead = this.ModuleSettings.MinTimeAhead;
			int daysAhead = this.ModuleSettings.DaysAhead;
			if (this.ModuleSettings.AllowCategorySelection)
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
					settings = this.ModuleSettings.Settings;
				}
			}
			else
			{
				settings = this.ModuleSettings.Settings;
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
					else if (this.ModuleSettings.IsDefined(j.Value.ToString("d", CultureInfo.InvariantCulture)))
					{
						Hashtable settings1 = this.ModuleSettings.Settings;
						date = j.Value;
						item = (string)settings1[date.ToString("d", CultureInfo.InvariantCulture)];
					}
					if (item == null)
					{
						date = j.Value;
						DateTime dateTime2 = date.Add(recurrencePattern.StartTime);
						DateTime dateTime3 = dateTime2.Add(recurrencePattern.Duration);
						this.AddToDateTimeRangeList(dateTimeRanges, dateTime2, dateTime3);
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
								this.AddToDateTimeRangeList(dateTimeRanges, date2, date3);
							}
						}
					}
				}
			}
			Hashtable hashtables1 = (categorySettings == null || !categorySettings.WorkingHoursExceptionsDefined ? this.ModuleSettings.Settings : categorySettings.Settings);
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
					this.AddToDateTimeRangeList(dateTimeRanges, date4, dateTime4);
				}
			}
			dateTimeRanges.Sort();
			return dateTimeRanges;
		}

		protected void HideAllStepTables()
		{
			foreach (Control control in this.Controls)
			{
				if (control == this.actionsTable || control == this.editionPlaceHolder || !(control.ID != "Gafware_Modules_Reservations_AuthorizeNetSIMForm_Hidden"))
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
			if (!this.SelectCategoryLast)
			{
				return this.GetAvailableReservationStartTimes(categoryID).Count > 0;
			}
			if (this.SelectedDuration == new TimeSpan())
			{
				int num = categoryID;
				DateTime selectedDateTime = this.SelectedDateTime;
				dateTime = new DateTime();
				return this.IsCategoryAvailable(num, (selectedDateTime != dateTime ? this.SelectedDateTime : this.SelectedDate));
			}
			int num1 = categoryID;
			DateTime selectedDateTime1 = this.SelectedDateTime;
			dateTime = new DateTime();
			return this.IsCategoryAvailable(num1, (selectedDateTime1 != dateTime ? this.SelectedDateTime : this.SelectedDate), this.SelectedDuration);
		}

		protected bool IsCategoryAvailable(int categoryID, DateTime dateTime)
		{
			return this.GetAvailableReservationStartTimes(categoryID, dateTime.Date).Contains(dateTime);
		}

		protected bool IsCategoryAvailable(int categoryID, DateTime dateTime, TimeSpan duration)
		{
			if (!this.IsCategoryAvailable(categoryID, dateTime))
			{
				return false;
			}
			return this.GetAvailableDurations(categoryID, dateTime).Contains(duration);
		}

		protected bool IsTimeOfDayAvailable(int categoryID, DateTime dateTime, string timeOfDayName)
		{
			return this.GetAvailableReservationStartTimes(categoryID, dateTime, timeOfDayName).Count > 0;
		}

		private bool IsValidVerificationCode(string email, string verificationCode)
		{
			if (!this.ModuleSettings.RequireVerificationCode)
			{
				return true;
			}
			return this.Helper.GenerateVerificationCode(email) == verificationCode;
		}

		private void MakeAuthorizeNetSIMPayment(Gafware.Modules.Reservations.PendingPaymentInfo pendingPaymentInfo, decimal amount, string feeType)
		{
			string empty = string.Empty;
			empty = (!this.ModuleSettings.AuthorizeNetTestMode ? "<form action = 'https://secure.authorize.net/gateway/transact.dll' method = 'post'>" : "<form action = 'https://test.authorize.net/gateway/transact.dll' method = 'post'>");
			int totalSeconds = (new Random()).Next(0, 1000);
			string str = totalSeconds.ToString();
			TimeSpan utcNow = DateTime.UtcNow - new DateTime(1970, 1, 1);
			totalSeconds = (int)utcNow.TotalSeconds;
			string str1 = totalSeconds.ToString();
			string authorizeNetTransactionKey = this.ModuleSettings.AuthorizeNetTransactionKey;
			string[] authorizeNetApiLogin = new string[] { this.ModuleSettings.AuthorizeNetApiLogin, "^", str, "^", str1, "^", null, null, null };
			authorizeNetApiLogin[6] = string.Format(CultureInfo.InvariantCulture, "{0:F}", new object[] { amount });
			authorizeNetApiLogin[7] = "^";
			authorizeNetApiLogin[8] = pendingPaymentInfo.Currency;
			string str2 = this.HMAC_MD5(authorizeNetTransactionKey, string.Concat(authorizeNetApiLogin));
			empty = string.Concat(empty, "<input type = 'hidden' name = 'x_fp_hash' value = '", str2, "' />");
			empty = string.Concat(empty, "<input type = 'hidden' name = 'x_fp_sequence' value = '", str, "' />");
			empty = string.Concat(empty, "<input type = 'hidden' name = 'x_fp_timestamp' value = '", str1, "' />");
			empty = string.Concat(empty, "<input type = 'hidden' name = 'x_login' value = '", this.ModuleSettings.AuthorizeNetApiLogin, "' />");
			empty = string.Concat(empty, "<input type = 'hidden' name = 'x_amount' value = '", string.Format(CultureInfo.InvariantCulture, "{0:F}", new object[] { amount }), "' />");
			empty = string.Concat(empty, "<input type = 'hidden' name = 'x_show_form' value = 'PAYMENT_FORM' />");
			string[] host = new string[] { empty, "<input type = 'hidden' name = 'x_relay_url' value = '", null, null, null, null };
			host[2] = (base.Request.IsSecureConnection ? "https://" : "http://");
			host[3] = base.Request.Url.Host;
			host[4] = base.ResolveUrl("AuthorizeNetSIMRelayResponse.aspx");
			host[5] = "' />";
			empty = string.Concat(host);
			empty = string.Concat(empty, "<input type = 'hidden' name = 'x_relay_response' value = 'TRUE' />");
			empty = string.Concat(empty, "<input type = 'hidden' name = 'x_currency_code' value = '", pendingPaymentInfo.Currency, "' />");
			empty = string.Concat(new object[] { empty, "<input type = 'hidden' name = 'x_invoice_num' value = '", pendingPaymentInfo.PendingPaymentID, "' />" });
			empty = string.Concat(empty, "<input type = 'hidden' name = 'x_description' value = '", (new PortalSecurity()).InputFilter(this.Helper.ReplaceTokens(this.ModuleSettings.ItemDescription, pendingPaymentInfo, feeType), (PortalSecurity.FilterFlag)6), "' />");
			int tabId = base.TabId;
			string empty1 = string.Empty;
			string[] strArrays = new string[3];
			totalSeconds = pendingPaymentInfo.PendingPaymentID;
			strArrays[0] = string.Concat("PendingPaymentID=", totalSeconds.ToString());
			PendingPaymentStatus pendingPaymentStatu = PendingPaymentStatus.Void;
			strArrays[1] = string.Concat("Status=", pendingPaymentStatu.ToString());
			strArrays[2] = string.Concat("Email=", pendingPaymentInfo.Email);
			empty = string.Concat(empty, "<input type = 'hidden' name = 'x_cancel_url' value = '", _navigationManager.NavigateURL(tabId, empty1, strArrays), "' />");
			empty = string.Concat(empty, SIMFormGenerator.EndForm());
			this.Gafware_Modules_Reservations_AuthorizeNetSIMForm_Hidden.Value = base.Server.HtmlEncode(empty);
			this.Gafware_Modules_Reservations_AuthorizeNetSIMForm_Hidden.Visible = true;
			this.AddModuleMessage(Localization.GetString("RedirectingToPaymentPage", base.LocalResourceFile), ModuleMessage.ModuleMessageType.BlueInfo);
			this.actionsTable.Visible = false;
			this.HideAllStepTables();
		}

		private void MakePayment(Gafware.Modules.Reservations.PendingPaymentInfo pendingPaymentInfo, decimal amount)
		{
			this.MakePayment(pendingPaymentInfo, amount, (this.ReservationInfo.ReservationID == Null.NullInteger ? Localization.GetString("ReservationFee", base.LocalResourceFile) : Localization.GetString("RescheduleFee", base.LocalResourceFile)));
		}

		private void MakePayment(Gafware.Modules.Reservations.PendingPaymentInfo pendingPaymentInfo, decimal amount, string feeType)
		{
			if (this.ModuleSettings.PaymentMethod == PaymentMethod.PayPalPaymentsStandard)
			{
				this.MakePayPalPayment(pendingPaymentInfo, amount, feeType);
				return;
			}
			if (this.ModuleSettings.PaymentMethod == PaymentMethod.AuthorizeNetSIM)
			{
				this.MakeAuthorizeNetSIMPayment(pendingPaymentInfo, amount, feeType);
			}
		}

		private void MakePayPalPayment(Gafware.Modules.Reservations.PendingPaymentInfo pendingPaymentInfo, decimal amount, string feeType)
		{
			HttpResponse response = base.Response;
			string[] payPalUrl = new string[21];
			payPalUrl[0] = this.ModuleSettings.PayPalUrl;
			payPalUrl[1] = (this.ModuleSettings.PayPalUrl.EndsWith("/") ? string.Empty : "/");
			payPalUrl[2] = "cgi-bin/webscr?cmd=_xclick&business=";
			payPalUrl[3] = base.Server.UrlPathEncode(this.ModuleSettings.PayPalAccount);
			payPalUrl[4] = "&item_name=";
			payPalUrl[5] = base.Server.UrlPathEncode((new PortalSecurity()).InputFilter(this.Helper.ReplaceTokens(this.ModuleSettings.ItemDescription, pendingPaymentInfo, feeType), (PortalSecurity.FilterFlag)6));
			payPalUrl[6] = "&item_number=";
			int pendingPaymentID = pendingPaymentInfo.PendingPaymentID;
			payPalUrl[7] = pendingPaymentID.ToString();
			payPalUrl[8] = "&quantity=1&custom=";
			HttpServerUtility server = base.Server;
			DateTime startDateTime = pendingPaymentInfo.StartDateTime;
			payPalUrl[9] = server.UrlPathEncode(startDateTime.ToString());
			payPalUrl[10] = "&amount=";
			payPalUrl[11] = base.Server.UrlPathEncode(string.Format(CultureInfo.InvariantCulture, "{0:F}", new object[] { amount }));
			payPalUrl[12] = "&currency_code=";
			payPalUrl[13] = base.Server.UrlPathEncode(pendingPaymentInfo.Currency);
			payPalUrl[14] = "&return=";
			HttpServerUtility httpServerUtility = base.Server;
			int tabId = base.TabId;
			string empty = string.Empty;
			string[] strArrays = new string[3];
			pendingPaymentID = pendingPaymentInfo.PendingPaymentID;
			strArrays[0] = string.Concat("PendingPaymentID=", pendingPaymentID.ToString());
			PendingPaymentStatus pendingPaymentStatu = PendingPaymentStatus.Paid;
			strArrays[1] = string.Concat("Status=", pendingPaymentStatu.ToString());
			strArrays[2] = string.Concat("Email=", pendingPaymentInfo.Email);
			payPalUrl[15] = httpServerUtility.UrlPathEncode(_navigationManager.NavigateURL(tabId, empty, strArrays));
			payPalUrl[16] = "&cancel_return=";
			HttpServerUtility server1 = base.Server;
			int num = base.TabId;
			string str = string.Empty;
			string[] strArrays1 = new string[3];
			pendingPaymentID = pendingPaymentInfo.PendingPaymentID;
			strArrays1[0] = string.Concat("PendingPaymentID=", pendingPaymentID.ToString());
			pendingPaymentStatu = PendingPaymentStatus.Void;
			strArrays1[1] = string.Concat("Status=", pendingPaymentStatu.ToString());
			strArrays1[2] = string.Concat("Email=", pendingPaymentInfo.Email);
			payPalUrl[17] = server1.UrlPathEncode(_navigationManager.NavigateURL(num, str, strArrays1));
			payPalUrl[18] = "&notify_url=";
			payPalUrl[19] = base.Server.UrlPathEncode(string.Concat((base.Request.IsSecureConnection ? "https://" : "http://"), base.Request.Url.Host, base.ResolveUrl("IPN.aspx")));
			payPalUrl[20] = "&undefined_quantity=&no_note=1&no_shipping=1";
			response.Redirect(string.Concat(payPalUrl));
		}

		protected void ModerateCommandButtonClicked(object sender, EventArgs e)
		{
			HttpResponse response = base.Response;
			string[] strArrays = new string[1];
			int moduleId = base.ModuleId;
			strArrays[0] = string.Concat("mid=", moduleId.ToString());
			response.Redirect(_navigationManager.NavigateURL("Moderate", strArrays));
		}

		protected void Page_Init(object sender, EventArgs e)
		{
			this.SetTheme();
			if (this.SelectCategoryLast)
			{
				this.categoryTableRowPlaceHolder.Controls.Remove(this.categoryTableRow);
				this.categoryTableRowPlaceHolder2.Controls.Add(this.categoryTableRow);
			}
			this.moderateCommandButton.Click += new EventHandler(this.ModerateCommandButtonClicked);
			this.duplicateReservationsCommandButton.Click += new EventHandler(this.DuplicateReservationsCommandButtonClicked);
			this.viewReservationsCommandButton.Click += new EventHandler(this.ViewReservationsCommandButtonClicked);
			this.viewReservationsCalendarCommandButton.Click += new EventHandler(this.ViewReservationsCalendarCommandButtonClicked);
			this.viewEditAReservationCommandButton.Click += new EventHandler(this.ViewEditAnReservationCommandButtonClicked);
			this.contactInfoBackCommandButton.Click += new EventHandler(this.Step2BackCommandButtonClicked);
			this.contactInfoNextCommandButton.Click += new EventHandler(this.Step2NextCommandButtonClicked);
			this.contactInfoConfirmCommandButton.Click += new EventHandler(this.Step2ConfirmCommandButtonClicked);
			this.step3BackCommandButton.Click += new EventHandler(this.Step3BackCommandButtonClicked);
			this.step3ConfirmCommandButton.Click += new EventHandler(this.Step3ConfirmCommandButtonClicked);
			this.step3NextCommandButton.Click += new EventHandler(this.Step3NextCommandButtonClicked);
			this.step3ConfirmAndPayLaterCommandButton.Click += new EventHandler(this.Step3ConfirmAndPayLaterCommandButtonClicked);
			this.contactInfoConfirmAndPayLaterCommandButton.Click += new EventHandler(this.Step2ConfirmAndPayLaterCommandButtonClicked);
			this.viewEditStep1BackCommandButton.Click += new EventHandler(this.ViewEditStep1BackCommandButtonClicked);
			this.viewEditStep1NextCommandButton.Click += new EventHandler(this.ViewEditStep1NextCommandButtonClicked);
			this.viewEditStep2BackCommandButton.Click += new EventHandler(this.ViewEditStep2BackCommandButtonClicked);
			this.cancelReservationCommandButton.Click += new EventHandler(this.CancelReservationCommandButtonClicked);
			this.rescheduleReservationCommandButton.Click += new EventHandler(this.RescheduleReservationCommandButtonClicked);
			this.confirmReservationCommandButton.Click += new EventHandler(this.ConfirmReservationCommandButtonClicked);
			this.scheduleAnotherReservationCommandButton.Click += new EventHandler(this.ScheduleAnotherReservationCommandButtonClicked);
			this.doneCommandButton.Click += new EventHandler(this.DoneCommandButtonClicked);
			this.approveCommandButton.Click += new EventHandler(this.ApproveCommandButtonClicked);
			this.declineCommandButton.Click += new EventHandler(this.DeclineCommandButtonClicked);
			this.returnCommandButton.Click += new EventHandler(this.ReturnCommandButtonClicked);
			if (this.IsProfessional)
			{
				this.cashierCommandButton.Click += new EventHandler(this.CashierCommandButtonClicked);
				this.payCommandButton.Click += new EventHandler(this.PayCommandButtonClicked);
			}
			this.firstNameTextBox.Attributes.Add("placeholder", Localization.GetString("FirstName", base.LocalResourceFile));
			this.lastNameTextBox.Attributes.Add("placeholder", Localization.GetString("LastName", base.LocalResourceFile));
			this.emailTextBox.Attributes.Add("placeholder", Localization.GetString("Email", base.LocalResourceFile));
			this.phoneTextBox.Attributes.Add("placeholder", Localization.GetString("Phone", base.LocalResourceFile));
			this.descriptionTextbox.Attributes.Add("placeholder", Localization.GetString("Comments", base.LocalResourceFile));
			this.viewEditEmailTextBox.Attributes.Add("placeholder", Localization.GetString("Email", base.LocalResourceFile));
			this.viewEditPhoneTextBox.Attributes.Add("placeholder", Localization.GetString("Phone", base.LocalResourceFile));
			this.viewEditVerificationCodeTextBox.Attributes.Add("placeholder", Localization.GetString("VerificationCode", base.LocalResourceFile));
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			Gafware.Modules.Reservations.PendingApprovalInfo pendingApproval;
			try
			{
				if (!base.IsPostBack)
				{
					Gafware.Modules.Reservations.Helper.DisplayModuleMessageIfAny(this);
					this.editionPlaceHolder.Controls.Add(new LiteralControl(string.Concat("<!--Edition: ", Gafware.Modules.Reservations.Helper.GetEdition(this.ModuleSettings.ActivationCode), "-->")));
				}
				if (!Gafware.Modules.Reservations.Helper.ValidateActivationCode(this.ModuleSettings.ActivationCode))
				{
					DateTime now = DateTime.Now;
					try
					{
						now = DateTime.Parse(Gafware.Modules.Reservations.Helper.Decrypt(ComponentBase<IHostController, HostController>.Instance.GetString(ModuleSettings.INSTALLEDON_KEY)), CultureInfo.InvariantCulture);
					}
					catch (Exception)
					{
					}
					string str = string.Concat(new string[] { "https://www.gafware.com/DesktopModules/Gafware/Reservations/PurchaseRedirect.aspx?Product=", base.Server.UrlEncode("The Reservations Module"), "&Edition=Standard&ReturnUrl=", base.Server.UrlEncode(base.EditUrl("Activate")), "&CancelUrl=", base.Server.UrlEncode(_navigationManager.NavigateURL()), "&Version=", base.Server.UrlEncode(base.ModuleConfiguration.DesktopModule.Version) });
					string str1 = string.Concat(new string[] { "https://www.gafware.com/DesktopModules/Gafware/Reservations/PurchaseRedirect.aspx?Product=", base.Server.UrlEncode("The Reservations Module"), "&Edition=Professional&ReturnUrl=", base.Server.UrlEncode(base.EditUrl("Activate")), "&CancelUrl=", base.Server.UrlEncode(_navigationManager.NavigateURL()), "&Version=", base.Server.UrlEncode(base.ModuleConfiguration.DesktopModule.Version) });
					string str2 = string.Concat(new string[] { "https://www.gafware.com/DesktopModules/Gafware/Reservations/PurchaseRedirect.aspx?Product=", base.Server.UrlEncode("The Reservations Module"), "&Edition=", base.Server.UrlEncode("Enterprise"), "&ReturnUrl=", base.Server.UrlEncode(base.EditUrl("Activate")), "&CancelUrl=", base.Server.UrlEncode(_navigationManager.NavigateURL()), "&Version=", base.Server.UrlEncode(base.ModuleConfiguration.DesktopModule.Version) });
					string[] strArrays = new string[1];
					int moduleId = base.ModuleId;
					strArrays[0] = string.Concat("mid=", moduleId.ToString());
					string str3 = _navigationManager.NavigateURL("Activate", strArrays);
					string str4 = string.Concat("<a href=\"", str, "\">*Click here*</a>");
					string str5 = string.Concat("<a href=\"", str1, "\">*click here*</a>");
					string str6 = string.Concat("<a href=\"", str2, "\">*click here*</a>");
					string str7 = string.Concat("<a href=\"", str3, "\">*click here*</a>");
					string str8 = string.Format("<br /><br />{0} to purchase the Standard Edition instantly thru PayPal, {1} for the Professional Edition or {2} for the Enterprise Edition. You don't need a PayPal account - a credit/debit card will suffice. No reinstallation/reconfiguration will be required. The data you have entered will be preserved.<br /><br />Alternatively, you can visit the developer's store at <a href=\"http://store.dnnsoftware.com/vendor-profile/dnn-specialists\">http://store.dnnsoftware.com/vendor-profile/dnn-specialists</a>. You will have to uninstall and reinstall/reconfigure the module accordingly, and the data you have entered will be lost.<br /><br />If you have already purchased the module, please {3} to activate your copy.", new object[] { str4, str5, str6, str7 });
					if (DateTime.Now >= now || DateTime.Now.AddDays(30) < now)
					{
						this.AddModuleMessage(string.Concat(Localization.GetString("TrialEnded", base.LocalResourceFile), str8), ModuleMessage.ModuleMessageType.RedError);
						this.HideAllStepTables();
						this.actionsTable.Visible = false;
						return;
					}
					else if (this.Page.User.Identity.IsAuthenticated)
					{
						string str9 = "Your copy of The Reservations Module is set to expire on {0}.";
						this.AddModuleMessage(string.Concat(string.Format(str9, now.ToShortDateString()), str8), ModuleMessage.ModuleMessageType.YellowWarning);
					}
				}
				if (!base.IsPostBack && !this.ModuleSettings.IsDefined("VerificationCodeSalt"))
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
					moduleController.UpdateTabModuleSetting(base.TabModuleId, "WorkingHours.1", Gafware.Modules.Reservations.Helper.SerializeRecurrencePattern(recurrencePattern));
					int tabModuleId = base.TabModuleId;
					Guid guid = Guid.NewGuid();
					moduleController.UpdateTabModuleSetting(tabModuleId, "VerificationCodeSalt", guid.ToString());
					ModuleController.SynchronizeModule(base.ModuleId);
					this._ModuleSettings = null;
				}
				if (this.ModuleSettings.AllowCategorySelection && this.CategoryList.Count == 0)
				{
					this.HideAllStepTables();
					this.AddModuleMessage(Localization.GetString("NoCategories", base.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
				}
				else if (!base.IsPostBack)
				{
					this.availableDaysCalendar.PrevMonthText = string.Concat("<img src=\"", this.TemplateSourceDirectory, "/Images/back.png\">");
					this.availableDaysCalendar.NextMonthText = string.Concat("<img src=\"", this.TemplateSourceDirectory, "/Images/next.png\">");
					if (this.HasEditPermissions && this.ModuleSettings.RequireVerificationCode && this.ModuleSettings.ConfirmationMailBody.IndexOf("{VerificationCode}") == -1)
					{
						this.AddModuleMessage(Localization.GetString("VerificationCodeWarning", base.LocalResourceFile), ModuleMessage.ModuleMessageType.YellowWarning);
					}
					this.viewReservationsCommandButton.Visible = this.Helper.CanViewReservations(base.UserId);
					this.moderateCommandButton.Visible = this.IsModerator;
					this.cashierCommandButton.Visible = this.IsCashier;
					this.duplicateReservationsCommandButton.Visible = this.Helper.CanViewDuplicateReservations(base.UserId);
					HtmlGenericControl htmlGenericControl = this.descriptionTableRow;
					HtmlGenericControl htmlGenericControl1 = this.descriptionTableRow2;
					bool allowDescription = this.ModuleSettings.AllowDescription;
					bool flag = allowDescription;
					htmlGenericControl1.Visible = allowDescription;
					htmlGenericControl.Visible = flag;
					this.phoneTextBoxRequiredFieldValidator.Visible = this.ModuleSettings.RequirePhone;
					if (this.ModuleSettings.RequirePhone)
					{
						TextBox textBox = this.phoneTextBox;
						textBox.CssClass = string.Concat(textBox.CssClass, " Gafware_Modules_Reservations_Required");
					}
					CustomValidator customValidator = this.emailTextBoxRequiredFieldValidator;
					HtmlGenericControl htmlGenericControl2 = this.viewEditPhoneTableRow1;
					bool allowLookupByPhone = this.ModuleSettings.AllowLookupByPhone;
					flag = allowLookupByPhone;
					htmlGenericControl2.Visible = allowLookupByPhone;
					customValidator.Visible = !flag;
					HtmlGenericControl htmlGenericControl3 = this.viewEditVerificationCodeTableRow;
					HtmlGenericControl htmlGenericControl4 = this.viewEditVerificationCodeTableRow2;
					bool requireVerificationCode = this.ModuleSettings.RequireVerificationCode;
					flag = requireVerificationCode;
					htmlGenericControl4.Visible = requireVerificationCode;
					htmlGenericControl3.Visible = flag;
					bool flag1 = true;
					if (this.IsProfessional && this.PendingPaymentInfo.PendingPaymentID != Null.NullInteger)
					{
						this.DisplayScheduleAnotherReservation();
						if (this.QueryStringPendingPaymentStatus != PendingPaymentStatus.Paid)
						{
							if (this.QueryStringPendingPaymentStatus == PendingPaymentStatus.Void && this.PendingPaymentInfo.Status == 0)
							{
								this.PendingPaymentInfo.Status = 2;
								this.PendingPaymentInfo.LastModifiedOnDate = DateTime.Now;
								(new PendingPaymentController()).UpdatePendingPayment(this.PendingPaymentInfo);
								base.Response.Redirect(_navigationManager.NavigateURL());
							}
							else if (this.QueryStringPendingPaymentStatus == PendingPaymentStatus.Due)
							{
								this.BindReservationInfo();
								this.HideAllStepTables();
								this.step4Table.Visible = true;
								flag1 = false;
							}
						}
						else if (this.PendingPaymentInfo.Status == 7)
						{
							this.AddModuleMessage(Localization.GetString("DueProcessing", base.LocalResourceFile), 0);
							Gafware.Modules.Reservations.ReservationInfo reservation = this.ReservationController.GetReservation(this.PendingPaymentInfo.ReservationID);
							if (reservation == null)
							{
								base.Response.Redirect(_navigationManager.NavigateURL());
							}
							this.ReservationInfo = reservation;
							this.BindReservationInfo();
							this.HideAllStepTables();
							this.step4Table.Visible = true;
							flag1 = false;
							this.PendingPaymentInfo = new Gafware.Modules.Reservations.PendingPaymentInfo();
						}
						else if (this.PendingPaymentInfo.Status == 0 || this.PendingPaymentInfo.Status == 4)
						{
							this.AddModuleMessage(Localization.GetString("ReservationPendingPayment", base.LocalResourceFile), 0);
							this.BindPendingPaymentInfo();
							this.HideAllStepTables();
							this.step4Table.Visible = true;
							flag1 = false;
						}
						else if (this.PendingPaymentInfo.Status == 1)
						{
							if (this.PendingPaymentInfo.PendingApprovalID == Null.NullInteger)
							{
								pendingApproval = null;
							}
							else
							{
								pendingApproval = (new PendingApprovalController()).GetPendingApproval(this.PendingPaymentInfo.PendingApprovalID);
							}
							Gafware.Modules.Reservations.PendingApprovalInfo pendingApprovalInfo = pendingApproval;
							if ((pendingApprovalInfo == null || pendingApprovalInfo.Status == 1) && this.PendingPaymentInfo.ReservationID != Null.NullInteger)
							{
								Gafware.Modules.Reservations.ReservationInfo reservationInfo = this.ReservationController.GetReservation(this.PendingPaymentInfo.ReservationID);
								if (reservationInfo == null)
								{
									base.Response.Redirect(_navigationManager.NavigateURL());
								}
								this.ReservationInfo = reservationInfo;
								this.AddModuleMessage(Localization.GetString("ReservationScheduled", base.LocalResourceFile), 0);
								this.BindReservationInfo();
							}
							else
							{
								this.PendingApprovalInfo = pendingApprovalInfo;
								if (this.PendingApprovalInfo.PendingApprovalID == Null.NullInteger)
								{
									base.Response.Redirect(_navigationManager.NavigateURL());
								}
								else
								{
									this.AddModuleMessage(Localization.GetString("ReservationPendingApproval", base.LocalResourceFile), 0);
									this.BindPendingApprovalInfo();
								}
							}
							this.HideAllStepTables();
							this.step4Table.Visible = true;
							flag1 = false;
							this.PendingPaymentInfo = new Gafware.Modules.Reservations.PendingPaymentInfo();
						}
						else if (this.PendingPaymentInfo.Status == 3)
						{
							this.AddModuleMessage(Localization.GetString("PendingPaymentTimeout", base.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
							flag1 = true;
							this.PendingPaymentInfo = new Gafware.Modules.Reservations.PendingPaymentInfo();
						}
					}
					else if (this.PendingApprovalInfo.PendingApprovalID != Null.NullInteger)
					{
						if (base.UserId == Null.NullInteger)
						{
							string str10 = HttpUtility.UrlEncode(HttpContext.Current.Request.RawUrl);
							if (base.PortalSettings.LoginTabId == Null.NullInteger)
							{
								base.Response.Redirect(_navigationManager.NavigateURL(base.TabId, "login", new string[] { string.Concat("returnurl=", str10) }));
							}
							else
							{
								base.Response.Redirect(_navigationManager.NavigateURL(base.PortalSettings.LoginTabId, string.Empty, new string[] { string.Concat("returnurl=", str10) }));
							}
						}
						else if (this.Helper.CanModerate(base.UserId, this.PendingApprovalInfo.CategoryID))
						{
							this.BindPendingApprovalInfo();
							this.BindPendingApprovalModeration();
							this.HideAllStepTables();
							this.step4Table.Visible = true;
							flag1 = false;
						}
					}
					if (!string.IsNullOrEmpty(base.Request.QueryString["EventID"]) && (this.Helper.CanViewReservations(base.UserId) || this.Helper.CanViewDuplicateReservations(base.UserId)))
					{
						try
						{
							this.ReservationInfo = this.ReservationController.GetReservation(int.Parse(base.Request.QueryString["EventID"]));
							this.BindReservationInfo();
							this.HideAllStepTables();
							this.step4Table.Visible = true;
							flag1 = false;
							if (!string.IsNullOrEmpty(base.Request.QueryString["ReturnUrl"]))
							{
								this.returnCommandButton.Visible = true;
							}
						}
						catch (Exception)
						{
						}
					}
					if (this.IsProfessional)
					{
						this.BindCustomFieldTableRowRepeater();
					}
					if (flag1)
					{
						if (base.UserId == Null.NullInteger)
						{
							this.firstNameTextBox.Text = base.Request.QueryString["FirstName"];
							this.lastNameTextBox.Text = base.Request.QueryString["LastName"];
							this.emailTextBox.Text = base.Request.QueryString["Email"];
							this.phoneTextBox.Text = base.Request.QueryString["Phone"];
							if (!this.ModuleSettings.ContactInfoFirst)
							{
								this.step3BackCommandButton.Visible = false;
								this.step3NextCommandButton.Visible = true;
								this.step3ConfirmCommandButton.Visible = false;
								this.contactInfoBackCommandButton.Visible = true;
								this.contactInfoNextCommandButton.Visible = false;
								this.contactInfoConfirmCommandButton.Visible = true;
							}
							else
							{
								this.contactInfoBackCommandButton.Visible = false;
								this.contactInfoConfirmCommandButton.Visible = false;
								this.contactInfoNextCommandButton.Visible = true;
								this.step3ConfirmCommandButton.Visible = true;
								this.step3NextCommandButton.Visible = false;
								this.step3BackCommandButton.Visible = true;
							}
						}
						else
						{
							this.firstNameTextBox.Text = base.UserInfo.FirstName;
							this.lastNameTextBox.Text = base.UserInfo.LastName;
							this.emailTextBox.Text = base.UserInfo.Email;
							this.phoneTextBox.Text = base.UserInfo.Profile.Telephone;
							if (!this.ModuleSettings.ContactInfoFirst)
							{
								this.step3BackCommandButton.Visible = false;
								this.step3NextCommandButton.Visible = !this.ModuleSettings.SkipContactInfoForAuthenticatedUsers;
								this.step3ConfirmCommandButton.Visible = this.ModuleSettings.SkipContactInfoForAuthenticatedUsers;
								this.contactInfoBackCommandButton.Visible = true;
								this.contactInfoNextCommandButton.Visible = false;
								this.contactInfoConfirmCommandButton.Visible = true;
							}
							else
							{
								this.contactInfoBackCommandButton.Visible = false;
								this.contactInfoConfirmCommandButton.Visible = false;
								this.contactInfoNextCommandButton.Visible = true;
								this.step3ConfirmCommandButton.Visible = true;
								this.step3NextCommandButton.Visible = false;
								this.step3BackCommandButton.Visible = !this.ModuleSettings.SkipContactInfoForAuthenticatedUsers;
							}
						}
						if (!this.ModuleSettings.ContactInfoFirst || base.UserId != Null.NullInteger && this.ModuleSettings.SkipContactInfoForAuthenticatedUsers)
						{
							this.Step2NextCommandButtonClicked(sender, e);
						}
						else
						{
							this.SetSelectedCategoryFromQueryString();
							if (!this.ModuleSettings.BindUponCategorySelection || this.SelectedCategory > 0)
							{
								if (!this.ModuleSettings.AllowCategorySelection || this.SelectCategoryLast)
								{
									this.BindAvailableDays();
								}
								else
								{
									this.BindCategories();
									if (this.SelectedCategory > 0)
									{
										this.BindAvailableDays();
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
			if (!base.IsPostBack)
			{
				Label label = this.step2ConfirmCommandButtonLabel;
				Label label1 = this.step3ConfirmCommandButtonLabel;
				string str = Localization.GetString("Confirm", base.LocalResourceFile);
				string str1 = str;
				label1.Text = str;
				label.Text = str1;
				this.cancelReservationCommandButtonLabel.Text = Localization.GetString("cancelReservationCommandButton", base.LocalResourceFile);
			}
			if (this.IsProfessional)
			{
				this.Page.ClientScript.RegisterOnSubmitStatement(this.Page.GetType(), "clearPlaceholders", "if (typeof (clearPlaceholders) == 'function') clearPlaceholders();");
				Label label2 = (this.ModuleSettings.ContactInfoFirst || base.UserId != Null.NullInteger && this.ModuleSettings.SkipContactInfoForAuthenticatedUsers ? this.step3ConfirmCommandButtonLabel : this.step2ConfirmCommandButtonLabel);
				Control control = (this.ModuleSettings.ContactInfoFirst || base.UserId != Null.NullInteger && this.ModuleSettings.SkipContactInfoForAuthenticatedUsers ? this.step3ConfirmAndPayLaterCommandButton : this.contactInfoConfirmAndPayLaterCommandButton);
				if (!this.ModuleSettings.AllowPayLater || !(this.Amount > decimal.Zero))
				{
					control.Visible = false;
				}
				else
				{
					control.Visible = true;
				}
				if (this.Amount > decimal.Zero)
				{
					label2.Text = string.Concat(Localization.GetString("ConfirmAndPay", base.LocalResourceFile), " ", Gafware.Modules.Reservations.Helper.GetFriendlyAmount(this.Amount, this.ModuleSettings.Currency));
				}
				else if (this.Amount < decimal.Zero)
				{
					label2.Text = string.Format(Localization.GetString("ConfirmAndRequestRefund", base.LocalResourceFile), Gafware.Modules.Reservations.Helper.GetFriendlyAmount(this.Amount * decimal.MinusOne, this.ModuleSettings.Currency));
				}
				if (this.CancellationAmount < decimal.Zero)
				{
					this.cancelReservationCommandButtonLabel.Text = string.Format(Localization.GetString("CancelAndRequestRefund", base.LocalResourceFile), Gafware.Modules.Reservations.Helper.GetFriendlyAmount(this.CancellationAmount * decimal.MinusOne, this.ModuleSettings.Currency));
				}
				if (this.DueAmount <= decimal.Zero)
				{
					this.payCommandButton.Visible = false;
				}
				else
				{
					this.payCommandButton.Visible = true;
					this.payCommandButtonLabel.Text = string.Format(Localization.GetString("payCommandButton", base.LocalResourceFile), Gafware.Modules.Reservations.Helper.GetFriendlyAmount(this.DueAmount, this.ModuleSettings.Currency));
				}
				foreach (DropDownList dropDownList in this.FindChildControlsByType(this.customFieldTableRowRepeater, typeof(DropDownList)))
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
			if (this.ModuleSettings.AllowCategorySelection)
			{
				if (this.ModuleSettings.CategorySelectionMode == Gafware.Modules.Reservations.ModuleSettings.SelectionModeEnum.DropDownList)
				{
					foreach (ListItem listItem in this.categoriesDropDownList.Items)
					{
						listItem.Attributes["class"] = (string)this.ViewState[string.Concat(listItem.Value, ".class")];
						if (listItem.Attributes["class"] != "Gafware_Modules_Reservations_UnavailableListItem")
						{
							continue;
						}
						listItem.Attributes["disabled"] = "disabled";
					}
				}
				else if (this.ModuleSettings.CategorySelectionMode == Gafware.Modules.Reservations.ModuleSettings.SelectionModeEnum.ListBox)
				{
					foreach (ListItem item1 in this.categoriesListBox.Items)
					{
						item1.Attributes["class"] = (string)this.ViewState[string.Concat(item1.Value, ".class")];
						if (item1.Attributes["class"] != "Gafware_Modules_Reservations_UnavailableListItem")
						{
							continue;
						}
						item1.Attributes["disabled"] = "disabled";
					}
				}
			}
			if (this.ModuleSettings.DisplayTimeOfDay)
			{
				if (this.ModuleSettings.TimeOfDaySelectionMode == Gafware.Modules.Reservations.ModuleSettings.SelectionModeEnum.DropDownList)
				{
					foreach (ListItem listItem1 in this.timeOfDayDropDownList.Items)
					{
						listItem1.Attributes["class"] = (string)this.ViewState[string.Concat(listItem1.Value, ".class")];
						if (listItem1.Attributes["class"] != "Gafware_Modules_Reservations_UnavailableListItem")
						{
							continue;
						}
						listItem1.Attributes["disabled"] = "disabled";
					}
				}
				else if (this.ModuleSettings.TimeOfDaySelectionMode == Gafware.Modules.Reservations.ModuleSettings.SelectionModeEnum.ListBox)
				{
					foreach (ListItem item2 in this.timeOfDayListBox.Items)
					{
						item2.Attributes["class"] = (string)this.ViewState[string.Concat(item2.Value, ".class")];
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
			this.MakePayment(this.DuePendingPaymentInfo, this.DueAmount, Localization.GetString("BalanceFee", base.LocalResourceFile));
		}

		private void PopulateEventInfoFromInput()
		{
			int totalMinutes;
			bool flag;
			this.ReservationInfo.TabModuleID = base.TabModuleId;
			this.ReservationInfo.CategoryID = (this.ModuleSettings.AllowCategorySelection ? this.SelectedCategory : 0);
			Gafware.Modules.Reservations.ReservationInfo reservationInfo = this.ReservationInfo;
			DateTime selectedDateTime = this.SelectedDateTime;
			DateTime startDateTime = new DateTime();
			reservationInfo.StartDateTime = (selectedDateTime == startDateTime ? this.SelectedDate : this.SelectedDateTime);
			Gafware.Modules.Reservations.ReservationInfo reservationInfo1 = this.ReservationInfo;
			TimeSpan selectedDuration = this.SelectedDuration;
			TimeSpan minReservationDuration = new TimeSpan();
			if (selectedDuration != minReservationDuration)
			{
				minReservationDuration = this.SelectedDuration;
				totalMinutes = (int)minReservationDuration.TotalMinutes;
			}
			else
			{
				minReservationDuration = this.MinReservationDuration;
				totalMinutes = (int)minReservationDuration.TotalMinutes;
			}
			reservationInfo1.Duration = totalMinutes;
			Gafware.Modules.Reservations.ReservationInfo reservationInfo2 = this.ReservationInfo;
			if (!this.ModuleSettings.SendReminders)
			{
				flag = false;
			}
			else
			{
				DateTime now = Gafware.Modules.Reservations.Helper.GetNow(this.ModuleSettings.TimeZone);
				startDateTime = this.ReservationInfo.StartDateTime;
				flag = now < startDateTime.Subtract(this.ModuleSettings.SendRemindersWhen);
			}
			reservationInfo2.SendReminder = flag;
			Gafware.Modules.Reservations.ReservationInfo totalMinutes1 = this.ReservationInfo;
			minReservationDuration = this.ModuleSettings.SendRemindersWhen;
			totalMinutes1.SendReminderWhen = (int)minReservationDuration.TotalMinutes;
			if (this.ReservationInfo.SendReminder)
			{
				this.ReservationInfo.SendReminderVia = this.ModuleSettings.SendRemindersVia;
				this.ReservationInfo.ReminderSent = false;
			}
			this.ReservationInfo.RequireConfirmation = (!this.ModuleSettings.RequireConfirmation ? false : this.ReservationInfo.SendReminder);
			Gafware.Modules.Reservations.ReservationInfo totalMinutes2 = this.ReservationInfo;
			minReservationDuration = this.ModuleSettings.RequireConfirmationWhen;
			totalMinutes2.RequireConfirmationWhen = (int)minReservationDuration.TotalMinutes;
			if (this.ReservationInfo.ReservationID != Null.NullInteger)
			{
				this.ReservationInfo.LastModifiedByUserID = base.UserId;
				this.ReservationInfo.LastModifiedOnDate = DateTime.Now;
				return;
			}
			this.ReservationInfo.FirstName = (new PortalSecurity()).InputFilter(this.firstNameTextBox.Text, (PortalSecurity.FilterFlag)6);
			this.ReservationInfo.LastName = (new PortalSecurity()).InputFilter(this.lastNameTextBox.Text, (PortalSecurity.FilterFlag)6);
			this.ReservationInfo.Email = (new PortalSecurity()).InputFilter(this.emailTextBox.Text, (PortalSecurity.FilterFlag)6);
			this.ReservationInfo.Phone = (this.phoneTextBox.Text != string.Empty ? (new PortalSecurity()).InputFilter(this.phoneTextBox.Text, (PortalSecurity.FilterFlag)6) : this.NotAvailable);
			this.ReservationInfo.Description = (new PortalSecurity()).InputFilter(this.descriptionTextbox.Text.Trim(), (PortalSecurity.FilterFlag)6);
			this.ReservationInfo.CreatedByUserID = base.UserId;
			this.ReservationInfo.CreatedOnDate = DateTime.Now;
		}

		protected void RescheduleReservationCommandButtonClicked(object sender, EventArgs e)
		{
			this.step3BackCommandButton.Visible = true;
			this.step3ConfirmCommandButton.Visible = true;
			this.step3NextCommandButton.Visible = false;
			this.Step2NextCommandButtonClicked(null, null);
		}

		protected void ReturnCommandButtonClicked(object sender, EventArgs e)
		{
			try
			{
				if (base.Request.QueryString["ReturnUrl"] != null)
				{
					base.Response.Redirect(base.Request.QueryString["ReturnUrl"]);
				}
				else
				{
					this.ModerateCommandButtonClicked(sender, e);
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
				this.ViewState["ReservationInfo"] = (this.ReservationInfo.ReservationID != Null.NullInteger ? (object)this.ReservationInfo.ReservationID : (object)null);
				this.ViewState["PendingApprovalID"] = (this.PendingApprovalInfo.PendingApprovalID != Null.NullInteger ? (object)this.PendingApprovalInfo.PendingApprovalID : (object)null);
				if (this.IsProfessional)
				{
					this.ViewState["PendingPaymentID"] = (this.PendingPaymentInfo.PendingPaymentID != Null.NullInteger ? (object)this.PendingPaymentInfo.PendingPaymentID : (object)null);
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
			if (this.IsProfessional)
			{
				this.BindCustomFieldTableRowRepeater();
			}
			if (this.ReservationInfo.ReservationID != Null.NullInteger)
			{
				this.firstNameTextBox.Text = this.ReservationInfo.FirstName;
				this.lastNameTextBox.Text = this.ReservationInfo.LastName;
				this.emailTextBox.Text = (this.ReservationInfo.Email != this.NotAvailable ? this.ReservationInfo.Email : string.Empty);
				this.phoneTextBox.Text = (this.ReservationInfo.Phone != this.NotAvailable ? this.ReservationInfo.Phone : string.Empty);
			}
			else if (this.PendingApprovalInfo.PendingApprovalID != Null.NullInteger)
			{
				this.firstNameTextBox.Text = this.PendingApprovalInfo.FirstName;
				this.lastNameTextBox.Text = this.PendingApprovalInfo.LastName;
				this.emailTextBox.Text = (this.PendingApprovalInfo.Email != this.NotAvailable ? this.PendingApprovalInfo.Email : string.Empty);
				this.phoneTextBox.Text = (this.PendingApprovalInfo.Phone != this.NotAvailable ? this.PendingApprovalInfo.Phone : string.Empty);
			}
			else if (this.IsProfessional && this.PendingPaymentInfo.PendingPaymentID != Null.NullInteger)
			{
				this.firstNameTextBox.Text = this.PendingPaymentInfo.FirstName;
				this.lastNameTextBox.Text = this.PendingPaymentInfo.LastName;
				this.emailTextBox.Text = (this.PendingPaymentInfo.Email != this.NotAvailable ? this.PendingPaymentInfo.Email : string.Empty);
				this.phoneTextBox.Text = (this.PendingPaymentInfo.Phone != this.NotAvailable ? this.PendingPaymentInfo.Phone : string.Empty);
			}
			this.ReservationInfo = new Gafware.Modules.Reservations.ReservationInfo();
			this.PendingApprovalInfo = new Gafware.Modules.Reservations.PendingApprovalInfo();
			this.PendingPaymentInfo = new Gafware.Modules.Reservations.PendingPaymentInfo();
			this.Step2NextCommandButtonClicked(sender, e);
		}

		protected bool SelectedCategoryChanged()
		{
			if (this.SelectCategoryLast)
			{
				return this.SelectedCategoryChangedLast();
			}
			DateTime dateTime = new DateTime();
			this.SelectedDate = dateTime;
			this.SelectedTimeOfDay = null;
			dateTime = new DateTime();
			this.SelectedDateTime = dateTime;
			this.SelectedDuration = new TimeSpan();
			this.availableDayTableRow.Visible = false;
			this.timesOfDayTableRow.Visible = false;
			this.timesTableRow.Visible = false;
			this.durationsTableRow.Visible = false;
			this.AvailableDaysCurrentPageIndex = 0;
			this.step3NextTable.Visible = false;
			if (this.IsCategoryAvailable(this.SelectedCategory))
			{
				this.availableDayTableRow.Visible = true;
				this.BindAvailableDays();
				this.BindCategories();
				return true;
			}
			if (this.GetCategoryIDsToRender().Count != 0)
			{
				this.AddModuleMessage(Localization.GetString("CategoryNotAvailable", base.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
			}
			if (!this.ModuleSettings.BindUponCategorySelection)
			{
				this.SelectedCategory = 0;
			}
			this.BindCategories();
			return false;
		}

		protected bool SelectedCategoryChangedLast()
		{
			this.step3NextTable.Visible = false;
			if (this.IsCategoryAvailable(this.SelectedCategory))
			{
				this.BindCategories();
				this.step3NextTable.Visible = true;
				return true;
			}
			DateTime selectedDateTime = this.SelectedDateTime;
			DateTime dateTime = new DateTime();
			if ((selectedDateTime != dateTime ? this.SelectedTimeChanged() : this.SelectedDateChanged()))
			{
				this.AddModuleMessage(Localization.GetString("CategoryNotAvailable", base.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
			}
			return false;
		}

		protected bool SelectedDateChanged()
		{
			bool flag;
			this.SelectedTimeOfDay = null;
			DateTime item = new DateTime();
			this.SelectedDateTime = item;
			TimeSpan timeSpan = new TimeSpan();
			this.SelectedDuration = timeSpan;
			this.timesOfDayTableRow.Visible = false;
			this.timesTableRow.Visible = false;
			this.durationsTableRow.Visible = false;
			this.step3NextTable.Visible = false;
			if (this.SelectCategoryLast)
			{
				this.SelectedCategory = 0;
				this.categoryTableRow.Visible = false;
			}
			if (!this.ModuleSettings.DisplayTimeOfDay)
			{
				this.AvailableTimesCurrentPageIndex = 0;
			}
			else
			{
				this.AvailableTimesOfDayCurrentPageIndex = 0;
			}
			List<DateTime> availableReservationDays = this.GetAvailableReservationDays(this.SelectedCategory);
			if (!availableReservationDays.Contains(this.SelectedDate))
			{
				if ((!this.ModuleSettings.AllowCategorySelection || this.SelectCategoryLast || this.SelectedCategoryChanged()) && !this.ModuleSettings.AllowCategorySelection && availableReservationDays.Count != 0)
				{
					this.AddModuleMessage(Localization.GetString("DayNotAvailable", base.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
				}
				if (!this.ModuleSettings.AllowCategorySelection || this.SelectCategoryLast)
				{
					item = new DateTime();
					this.SelectedDate = item;
					this.BindAvailableDays();
				}
				return false;
			}
			this.BindAvailableDays();
			TimeSpan minReservationDuration = this.MinReservationDuration;
			TimeSpan maxReservationDuration = this.MaxReservationDuration;
			if (this.SelectCategoryLast)
			{
				List<TimeSpan> timeSpans = new List<TimeSpan>();
				List<TimeSpan> timeSpans1 = new List<TimeSpan>();
				foreach (CategoryInfo availableCategory in this.GetAvailableCategories(this.SelectedDate))
				{
					timeSpans.Add(this.CategorySettingsDictionary[availableCategory.CategoryID].ReservationDuration);
					timeSpans1.Add(this.CategorySettingsDictionary[availableCategory.CategoryID].ReservationDurationMax);
				}
				minReservationDuration = timeSpans.Min<TimeSpan>();
				maxReservationDuration = timeSpans1.Max<TimeSpan>();
			}
			List<DateTime> availableReservationStartTimes = this.GetAvailableReservationStartTimes(this.SelectedCategory, this.SelectedDate);
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
				if (!this.ModuleSettings.DisplayTimeOfDay)
				{
					this.timesTableRow.Visible = true;
					this.BindAvailableTimes();
				}
				else
				{
					this.timesOfDayTableRow.Visible = true;
					this.BindAvailableTimesOfDay();
				}
			}
			else if (minReservationDuration < maxReservationDuration)
			{
				this.BindAvailableDurations();
				this.durationsTableRow.Visible = true;
			}
			else if (!this.SelectCategoryLast)
			{
				this.step3NextTable.Visible = true;
			}
			else
			{
				this.BindCategories();
				this.categoryTableRow.Visible = true;
			}
			return true;
		}

		protected bool SelectedDurationChanged()
		{
			int selectedCategory = this.SelectedCategory;
			DateTime selectedDateTime = this.SelectedDateTime;
			DateTime dateTime = new DateTime();
			List<TimeSpan> availableDurations = this.GetAvailableDurations(selectedCategory, (selectedDateTime == dateTime ? this.SelectedDate : this.SelectedDateTime));
			this.step3NextTable.Visible = false;
			if (this.SelectCategoryLast)
			{
				this.SelectedCategory = 0;
				this.categoryTableRow.Visible = false;
			}
			if (!availableDurations.Contains(this.SelectedDuration))
			{
				if (this.SelectedTimeChanged())
				{
					this.AddModuleMessage(Localization.GetString("DurationNotAvailable", base.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
				}
				return false;
			}
			this.BindAvailableDurations();
			if (!this.SelectCategoryLast)
			{
				this.step3NextTable.Visible = true;
			}
			else
			{
				this.BindCategories();
				this.categoryTableRow.Visible = true;
			}
			return true;
		}

		protected bool SelectedTimeChanged()
		{
			this.SelectedDuration = new TimeSpan();
			this.durationsTableRow.Visible = false;
			this.step3NextTable.Visible = false;
			if (this.SelectCategoryLast)
			{
				this.SelectedCategory = 0;
				this.categoryTableRow.Visible = false;
			}
			this.AvailableDurationsCurrentPageIndex = 0;
			if (!((this.ModuleSettings.DisplayTimeOfDay ? this.GetAvailableReservationStartTimes(this.SelectedCategory, this.SelectedDate, this.SelectedTimeOfDay) : this.GetAvailableReservationStartTimes(this.SelectedCategory, this.SelectedDate))).Contains(this.SelectedDateTime))
			{
				if (this.ModuleSettings.DisplayTimeOfDay && this.SelectedTimeOfDayChanged() || !this.ModuleSettings.DisplayTimeOfDay && this.SelectedDateChanged())
				{
					this.AddModuleMessage(Localization.GetString("TimeNotAvailable", base.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
				}
				return false;
			}
			this.BindAvailableTimes();
			TimeSpan minReservationDuration = this.MinReservationDuration;
			TimeSpan maxReservationDuration = this.MaxReservationDuration;
			if (this.SelectCategoryLast)
			{
				List<TimeSpan> timeSpans = new List<TimeSpan>();
				List<TimeSpan> timeSpans1 = new List<TimeSpan>();
				foreach (CategoryInfo availableCategory in this.GetAvailableCategories(this.SelectedDate))
				{
					timeSpans.Add(this.CategorySettingsDictionary[availableCategory.CategoryID].ReservationDuration);
					timeSpans1.Add(this.CategorySettingsDictionary[availableCategory.CategoryID].ReservationDurationMax);
				}
				minReservationDuration = timeSpans.Min<TimeSpan>();
				maxReservationDuration = timeSpans1.Max<TimeSpan>();
			}
			if (minReservationDuration < maxReservationDuration)
			{
				this.BindAvailableDurations();
				this.durationsTableRow.Visible = true;
			}
			else if (!this.SelectCategoryLast)
			{
				this.step3NextTable.Visible = true;
			}
			else
			{
				this.BindCategories();
				this.categoryTableRow.Visible = true;
			}
			return true;
		}

		protected bool SelectedTimeOfDayChanged()
		{
			this.SelectedDateTime = new DateTime();
			this.SelectedDuration = new TimeSpan();
			this.timesTableRow.Visible = false;
			this.durationsTableRow.Visible = false;
			if (this.SelectCategoryLast)
			{
				this.SelectedCategory = 0;
				this.categoryTableRow.Visible = false;
			}
			this.step3NextTable.Visible = false;
			this.AvailableTimesCurrentPageIndex = 0;
			if (this.IsTimeOfDayAvailable(this.SelectedCategory, this.SelectedDate, this.SelectedTimeOfDay))
			{
				this.BindAvailableTimesOfDay();
				this.BindAvailableTimes();
				this.timesTableRow.Visible = true;
				return true;
			}
			if (this.SelectedDateChanged())
			{
				this.AddModuleMessage(Localization.GetString("TimeOfDayNotAvailable", base.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
			}
			return false;
		}

		protected void SendVerificationCode(object sender, EventArgs e)
		{
			if (this.Page.IsValid)
			{
				this.Helper.SendVerificationCodeMail(this.viewEditEmailTextBox.Text, this.Helper.GenerateVerificationCode(this.viewEditEmailTextBox.Text));
				this.AddModuleMessage(Localization.GetString("VerificationCodeSent", base.LocalResourceFile), 0);
			}
		}

		private void SetSelectedCategoryFromQueryString()
		{
			if (this.ModuleSettings.AllowCategorySelection && !string.IsNullOrEmpty(base.Request.QueryString["Category"]))
			{
				foreach (int categoryIDsToRender in this.GetCategoryIDsToRender())
				{
					if (this.Helper.GetCategoryName(categoryIDsToRender).ToLower() != base.Request.QueryString["Category"].ToLower())
					{
						continue;
					}
					this.SelectedCategory = categoryIDsToRender;
				}
			}
		}

		public void SetTheme()
		{
			string str = string.Concat(new string[] { this.TemplateSourceDirectory, "/Themes/", this.ModuleSettings.Theme, "/", this.ModuleSettings.Theme, ".css" });
			HtmlLink htmlLink = new HtmlLink()
			{
				Href = str
			};
			htmlLink.Attributes.Add("rel", "stylesheet");
			htmlLink.Attributes.Add("type", "text/css");
			this.Page.Header.Controls.Add(htmlLink);
			str = string.Concat(new string[] { this.TemplateSourceDirectory, "/Themes/", this.ModuleSettings.Theme, "/", this.ModuleSettings.Theme, "-LTIE8.css" });
			htmlLink = new HtmlLink()
			{
				Href = str
			};
			htmlLink.Attributes.Add("rel", "stylesheet");
			htmlLink.Attributes.Add("type", "text/css");
			this.Page.Header.Controls.Add(new LiteralControl("<!--[if LT IE 8]>"));
			this.Page.Header.Controls.Add(htmlLink);
			this.Page.Header.Controls.Add(new LiteralControl("<![endif]-->"));
		}

		protected void Step2BackCommandButtonClicked(object sender, EventArgs e)
		{
			this.HideAllStepTables();
			this.step3Table.Visible = true;
		}

		protected void Step2ConfirmAndPayLaterCommandButtonClicked(object sender, EventArgs e)
		{
			if (this.Page.IsValid)
			{
				this.Confirm(false);
			}
		}

		protected void Step2ConfirmCommandButtonClicked(object sender, EventArgs e)
		{
			if (this.Page.IsValid)
			{
				this.Confirm(true);
			}
		}

		protected void Step2NextCommandButtonClicked(object sender, EventArgs e)
		{
			if (sender != this.contactInfoNextCommandButton || this.Page.IsValid)
			{
				this.HideAllStepTables();
				this.SelectedCategory = 0;
				DateTime dateTime = new DateTime();
				this.SelectedDate = dateTime;
				this.SelectedTimeOfDay = null;
				dateTime = new DateTime();
				this.SelectedDateTime = dateTime;
				this.categoryTableRow.Visible = false;
				this.availableDayTableRow.Visible = false;
				this.timesOfDayTableRow.Visible = false;
				this.timesTableRow.Visible = false;
				this.durationsTableRow.Visible = false;
				this.step3NextTable.Visible = false;
				this.CategoriesCurrentPageIndex = 0;
				this.AvailableDaysCurrentPageIndex = 0;
				this.AvailableTimesOfDayCurrentPageIndex = 0;
				this.AvailableTimesCurrentPageIndex = 0;
				this.step3Table.Visible = true;
				this.SetSelectedCategoryFromQueryString();
				if (!this.ModuleSettings.AllowCategorySelection || this.SelectCategoryLast)
				{
					this.availableDayTableRow.Visible = true;
					this.BindAvailableDays();
				}
				else
				{
					this.categoryTableRow.Visible = true;
					this.BindCategories();
					if (this.SelectedCategory > 0)
					{
						this.availableDayTableRow.Visible = true;
						this.BindAvailableDays();
						return;
					}
				}
			}
		}

		protected void Step3BackCommandButtonClicked(object sender, EventArgs e)
		{
			this.HideAllStepTables();
			if (this.ReservationInfo.ReservationID == Null.NullInteger)
			{
				this.contactInfoDiv.Visible = true;
				return;
			}
			this.step4Table.Visible = true;
		}

		protected void Step3ConfirmAndPayLaterCommandButtonClicked(object sender, EventArgs e)
		{
			this.Confirm(false);
		}

		protected void Step3ConfirmCommandButtonClicked(object sender, EventArgs e)
		{
			this.Confirm(true);
		}

		protected void Step3NextCommandButtonClicked(object sender, EventArgs e)
		{
			this.HideAllStepTables();
			this.contactInfoDiv.Visible = true;
		}

		protected void TimeDropDownListSelectedIndexChanged(object sender, EventArgs e)
		{
			this.SelectedDateTime = DateTime.Parse(this.timeDropDownList.SelectedValue);
			this.SelectedTimeChanged();
		}

		protected void TimeListBoxSelectedIndexChanged(object sender, EventArgs e)
		{
			this.SelectedDateTime = DateTime.Parse(this.timeListBox.SelectedValue);
			this.SelectedTimeChanged();
		}

		protected void TimeOfDayDropDownListSelectedIndexChanged(object sender, EventArgs e)
		{
			this.SelectedTimeOfDay = this.timeOfDayDropDownList.SelectedValue;
			this.SelectedTimeOfDayChanged();
		}

		protected void TimeOfDayLinkButtonClicked(object sender, CommandEventArgs e)
		{
			this.SelectedTimeOfDay = (string)e.CommandArgument;
			this.SelectedTimeOfDayChanged();
		}

		protected void TimeOfDayListBoxSelectedIndexChanged(object sender, EventArgs e)
		{
			this.SelectedTimeOfDay = this.timeOfDayListBox.SelectedValue;
			this.SelectedTimeOfDayChanged();
		}

		protected void ValidateEmail(object sender, ServerValidateEventArgs e)
		{
			WebControl webControl = (WebControl)((BaseValidator)sender).NamingContainer.FindControl(((BaseValidator)sender).ControlToValidate);
			if (this.Helper.IsValidEmail(e.Value))
			{
				webControl.CssClass = webControl.CssClass.Replace(" Gafware_Modules_Reservations_Invalid", string.Empty);
				return;
			}
			if (!webControl.CssClass.Contains(" Gafware_Modules_Reservations_Invalid"))
			{
				WebControl webControl1 = webControl;
				webControl1.CssClass = string.Concat(webControl1.CssClass, " Gafware_Modules_Reservations_Invalid");
			}
			this.AddModuleMessage(Localization.GetString("InvalidEmail", base.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
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
			if (!this.MissingRequiredFieldsModuleMessageAdded)
			{
				this.AddModuleMessage(Localization.GetString("MissingRequiredFields", base.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
				this.MissingRequiredFieldsModuleMessageAdded = true;
			}
			e.IsValid = false;
		}

		protected void ValidateViewEditEmail(object sender, ServerValidateEventArgs e)
		{
			e.IsValid = (!this.ModuleSettings.AllowLookupByPhone || this.viewEditEmailTextBox.Text != string.Empty ? true : this.viewEditPhoneTextBox.Text != string.Empty);
			if (!e.IsValid)
			{
				TextBox textBox = this.viewEditEmailTextBox;
				if (!textBox.CssClass.Contains(" Gafware_Modules_Reservations_Invalid"))
				{
					TextBox textBox1 = textBox;
					textBox1.CssClass = string.Concat(textBox1.CssClass, " Gafware_Modules_Reservations_Invalid");
				}
				if (!this.MissingRequiredFieldsModuleMessageAdded)
				{
					this.AddModuleMessage(Localization.GetString("MissingRequiredFields", base.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
					this.MissingRequiredFieldsModuleMessageAdded = true;
				}
			}
		}

		protected void ViewEditAnReservationCommandButtonClicked(object sender, EventArgs e)
		{
			this.HideAllStepTables();
			this.viewEditStep1Table.Visible = true;
			if (base.UserId != Null.NullInteger)
			{
				this.viewEditEmailTextBox.Text = base.UserInfo.Email;
				this.viewEditEmailTextBox.Enabled = !this.ModuleSettings.SkipContactInfoForAuthenticatedUsers;
			}
		}

		protected void ViewEditStep1BackCommandButtonClicked(object sender, EventArgs e)
		{
			this.HideAllStepTables();
			if (!this.ModuleSettings.ContactInfoFirst || base.UserId != Null.NullInteger && this.ModuleSettings.SkipContactInfoForAuthenticatedUsers)
			{
				this.Step2NextCommandButtonClicked(sender, e);
			}
			else
			{
				this.contactInfoDiv.Visible = true;
				this.SetSelectedCategoryFromQueryString();
				if (!this.ModuleSettings.BindUponCategorySelection || this.SelectedCategory > 0)
				{
					if (this.ModuleSettings.AllowCategorySelection)
					{
						this.BindCategories();
						return;
					}
					this.BindAvailableDays();
					return;
				}
			}
		}

		protected void ViewEditStep1NextCommandButtonClicked(object sender, EventArgs e)
		{
			bool flag;
			if (this.Page.IsValid)
			{
				if (this.viewEditEmailTextBox.Text != string.Empty && !this.IsValidVerificationCode(this.viewEditEmailTextBox.Text.Trim(), this.viewEditVerificationCodeTextBox.Text.Trim()))
				{
					this.AddModuleMessage(Localization.GetString("IncorrectVerificationCode", base.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
					return;
				}
				List<Gafware.Modules.Reservations.ReservationInfo> reservationInfos = new List<Gafware.Modules.Reservations.ReservationInfo>();
				bool flag1 = false;
				bool flag2 = false;
				bool flag3 = false;
				foreach (Gafware.Modules.Reservations.ReservationInfo reservationList in this.ReservationList)
				{
					flag = (this.viewEditEmailTextBox.Text == string.Empty ? false : reservationList.Email.ToLower() == this.viewEditEmailTextBox.Text.ToLower());
					if (!flag)
					{
						flag1 = (this.viewEditPhoneTextBox.Text == string.Empty ? false : this.GetPhoneLettersOrDigits(reservationList.Phone.ToLower()) == this.GetPhoneLettersOrDigits(this.viewEditPhoneTextBox.Text.ToLower()));
						flag2 = this.IsValidVerificationCode(reservationList.Email, this.viewEditVerificationCodeTextBox.Text.Trim());
					}
					if (!(flag | flag1) || this.FindByEventID(this.PendingApprovalList, reservationList.ReservationID) != null || this.IsProfessional && (this.FindByEventIDAndStatus(this.PendingPaymentList, reservationList.ReservationID, PendingPaymentStatus.Processing) != null || this.FindByEventIDAndStatus(this.PendingPaymentList, reservationList.ReservationID, PendingPaymentStatus.Held) != null))
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
					this.HideAllStepTables();
					if (reservationInfos.Count == 1)
					{
						this.ReservationInfo = reservationInfos[0];
						this.BindReservationInfo();
						this.step4Table.Visible = true;
						return;
					}
					this.viewEditRepeater.DataSource = reservationInfos;
					this.viewEditRepeater.DataBind();
					this.viewEditStep2Table.Visible = true;
					return;
				}
				if (flag3)
				{
					this.AddModuleMessage(Localization.GetString("IncorrectVerificationCode2", base.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
					return;
				}
				this.AddModuleMessage(Localization.GetString("NoReservation", base.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
			}
		}

		protected void ViewEditStep2BackCommandButtonClicked(object sender, EventArgs e)
		{
			this.HideAllStepTables();
			this.viewEditStep1Table.Visible = true;
		}

		protected void ViewEditStep2EventCommandButtonClicked(object sender, CommandEventArgs e)
		{
			foreach (Gafware.Modules.Reservations.ReservationInfo reservationList in this.ReservationList)
			{
				if (reservationList.ReservationID != int.Parse((string)e.CommandArgument))
				{
					continue;
				}
				this.ReservationInfo = reservationList;
				this.HideAllStepTables();
				this.BindReservationInfo();
				this.step4Table.Visible = true;
				return;
			}
			this.HideAllStepTables();
			this.BindReservationInfo();
			this.step4Table.Visible = true;
		}

		protected void ViewReservationsCalendarCommandButtonClicked(object sender, EventArgs e)
		{
			HttpResponse response = base.Response;
			string[] strArrays = new string[1];
			int moduleId = base.ModuleId;
			strArrays[0] = string.Concat("mid=", moduleId.ToString());
			response.Redirect(_navigationManager.NavigateURL("ViewReservationsCalendar", strArrays));
		}

        protected void ViewReservationsCommandButtonClicked(object sender, EventArgs e)
		{
			HttpResponse response = base.Response;
			string[] strArrays = new string[2];
			int moduleId = base.ModuleId;
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