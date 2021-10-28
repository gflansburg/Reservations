using DotNetNuke.Common;
using DotNetNuke.Common.Lists;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.UserControls;
using Gafware.Modules.Reservations.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Serialization;

namespace Gafware.Modules.Reservations
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The Settings class manages Module Settings
    /// 
    /// Typically your settings control would be used to manage settings for your module.
    /// There are two types of settings, ModuleSettings, and TabModuleSettings.
    /// 
    /// ModuleSettings apply to all "copies" of a module on a site, no matter which page the module is on. 
    /// 
    /// TabModuleSettings apply only to the current module on the current page, if you copy that module to
    /// another page the settings are not transferred.
    /// 
    /// If you happen to save both TabModuleSettings and ModuleSettings, TabModuleSettings overrides ModuleSettings.
    /// 
    /// Below we have some examples of how to access these settings but you will need to uncomment to use.
    /// 
    /// Because the control inherits from ReservationsSettingsBase you have access to any custom properties
    /// defined there, as well as properties from DNN such as PortalId, ModuleId, TabId, UserId and many more.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class Settings : ModuleSettingsBase
    {
        private Gafware.Modules.Reservations.ModuleSettings _ModuleSettings;

        private ArrayList _Users;

        private bool? _IsProfesional;

        private List<CategoryInfo> _CategoryList;

        private ArrayList _CategoryPermissionsList;

        private ArrayList _CashierList;

        private List<RecurrencePattern> _WorkingHours;

        private ArrayList _WorkingHoursExceptions;

        private ArrayList _TimeOfDayList;

        private ArrayList _BCCList;

        private ArrayList _ModeratorList;

        private ArrayList _ModerationHours;

        private ArrayList _DuplicateReservationsList;

        private ArrayList _ViewReservationsList;

		private ArrayList BCCList
		{
			get
			{
				if (this._BCCList == null)
				{
					if (this.ViewState["BCCList"] != null)
					{
						//this._BCCList = this.ModuleSettings.DeserializeUserIDList((string)this.ViewState["BCCList"]);
						this._BCCList = this.ModuleSettings.DeserializeEmailList((string)this.ViewState["BCCList"]);
					}
					else
					{
						int num = (!base.IsPostBack || this.bccListCategoryDropDownList.SelectedValue == null || !(this.bccListCategoryDropDownList.SelectedValue != string.Empty) ? Null.NullInteger : int.Parse(this.bccListCategoryDropDownList.SelectedValue));
						if (num != Null.NullInteger)
						{
							this._BCCList = (new CategorySettings(base.PortalId, base.TabModuleId, num)).BCCList;
						}
						else
						{
							this._BCCList = this.ModuleSettings.BCCList;
						}
					}
				}
				return this._BCCList;
			}
		}

		private ArrayList CashierList
		{
			get
			{
				if (this._CashierList == null)
				{
					if (this.ViewState["CashierList"] != null)
					{
						//this._CashierList = this.ModuleSettings.DeserializeUserIDList((string)this.ViewState["CashierList"]);
						this._CashierList = this.ModuleSettings.DeserializeEmailList((string)this.ViewState["CashierList"]);
					}
					else
					{
						int num = (!base.IsPostBack || this.cashierListCategoryDropDownList.SelectedValue == null || !(this.cashierListCategoryDropDownList.SelectedValue != string.Empty) ? Null.NullInteger : int.Parse(this.cashierListCategoryDropDownList.SelectedValue));
						if (num != Null.NullInteger)
						{
							this._CashierList = (new CategorySettings(base.PortalId, base.TabModuleId, num)).CashierList;
						}
						else
						{
							this._CashierList = this.ModuleSettings.CashierList;
						}
					}
				}
				return this._CashierList;
			}
		}

		private List<CategoryInfo> CategoryList
		{
			get
			{
				if (this._CategoryList == null)
				{
					if (this.ViewState["CategoryList"] != null)
					{
						this._CategoryList = this.DeserializeCategoryList((string)this.ViewState["CategoryList"]);
					}
					else
					{
						this._CategoryList = (new CategoryController()).GetCategoryList(base.TabModuleId);
						this._CategoryList.Sort((CategoryInfo x, CategoryInfo y) => x.Name.CompareTo(y.Name));
					}
				}
				return this._CategoryList;
			}
		}

		private ArrayList CategoryPermissionsList
		{
			get
			{
				if (this._CategoryPermissionsList == null)
				{
					if (this.ViewState["CategoryPermissionsList"] != null)
					{
						this._CategoryPermissionsList = this.ModuleSettings.DeserializeRoleIDList((string)this.ViewState["CategoryPermissionsList"]);
					}
					else
					{
						int num = (!base.IsPostBack || this.categoryPermissionsDropDownList.SelectedValue == null || !(this.categoryPermissionsDropDownList.SelectedValue != string.Empty) ? Null.NullInteger : int.Parse(this.categoryPermissionsDropDownList.SelectedValue));
						if (num != Null.NullInteger)
						{
							this._CategoryPermissionsList = (new CategorySettings(base.PortalId, base.TabModuleId, num)).CategoryPermissionsList;
						}
						else
						{
							this._CategoryPermissionsList = this.ModuleSettings.CategoryPermissionsList;
						}
					}
				}
				return this._CategoryPermissionsList;
			}
		}

		private ArrayList DuplicateReservationsList
		{
			get
			{
				if (this._DuplicateReservationsList == null)
				{
					if (this.ViewState["DuplicateReservationsList"] != null)
					{
						//this._DuplicateReservationsList = this.ModuleSettings.DeserializeUserIDList((string)this.ViewState["DuplicateReservationsList"]);
						this._DuplicateReservationsList = this.ModuleSettings.DeserializeEmailList((string)this.ViewState["DuplicateReservationsList"]);
					}
					else
					{
						this._DuplicateReservationsList = this.ModuleSettings.DuplicateReservationsList;
					}
				}
				return this._DuplicateReservationsList;
			}
		}

		protected bool Is24HourClock
		{
			get
			{
				return string.IsNullOrEmpty(CultureInfo.CurrentCulture.DateTimeFormat.AMDesignator);
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

		private ArrayList ModerationHours
		{
			get
			{
				if (this._ModerationHours == null)
				{
					if (this.ViewState["ModerationHours"] != null)
					{
						this._ModerationHours = this.DeserializeWorkingHours((string)this.ViewState["ModerationHours"]);
					}
					else
					{
						this._ModerationHours = new ArrayList();
						int num = (!base.IsPostBack || this.moderationCategoryDropDownList.SelectedValue == null || !(this.moderationCategoryDropDownList.SelectedValue != string.Empty) ? Null.NullInteger : int.Parse(this.moderationCategoryDropDownList.SelectedValue));
						Hashtable settings = null;
						if (num != Null.NullInteger)
						{
							settings = (new CategorySettings(base.PortalId, base.TabModuleId, num)).Settings;
							if (!settings.ContainsKey(string.Concat("Moderation.", DayOfWeek.Monday.ToString())))
							{
								settings = this.ModuleSettings.Settings;
							}
						}
						else
						{
							settings = this.ModuleSettings.Settings;
						}
						foreach (DayOfWeek value in Enum.GetValues(typeof(DayOfWeek)))
						{
							if (!settings.ContainsKey(string.Concat("Moderation.", value.ToString())) || !((string)settings[string.Concat("Moderation.", value.ToString())] != string.Empty))
							{
								continue;
							}
							string[] strArrays = ((string)settings[string.Concat("Moderation.", value.ToString())]).Split(new char[] { ';' });
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
								this._ModerationHours.Add(workingHoursInfo);
							}
						}
					}
				}
				return this._ModerationHours;
			}
		}

		private ArrayList ModeratorList
		{
			get
			{
				if (this._ModeratorList == null)
				{
					if (this.ViewState["ModeratorList"] != null)
					{
						//this._ModeratorList = this.ModuleSettings.DeserializeUserIDList((string)this.ViewState["ModeratorList"]);
						this._ModeratorList = this.ModuleSettings.DeserializeEmailList((string)this.ViewState["ModeratorList"]);
					}
					else
					{
						int num = (!base.IsPostBack || this.moderationCategoryDropDownList.SelectedValue == null || !(this.moderationCategoryDropDownList.SelectedValue != string.Empty) ? Null.NullInteger : int.Parse(this.moderationCategoryDropDownList.SelectedValue));
						if (num != Null.NullInteger)
						{
							this._ModeratorList = (new CategorySettings(base.PortalId, base.TabModuleId, num)).ModeratorList;
						}
						else
						{
							this._ModeratorList = this.ModuleSettings.ModeratorList;
						}
					}
				}
				return this._ModeratorList;
			}
		}

		private Gafware.Modules.Reservations.ModuleSettings ModuleSettings
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

		private ArrayList TimeOfDayList
		{
			get
			{
				if (this._TimeOfDayList == null)
				{
					if (this.ViewState["TimeOfDayList"] != null)
					{
						this._TimeOfDayList = this.ModuleSettings.DeserializeTimeOfDayList((string)this.ViewState["TimeOfDayList"]);
					}
					else
					{
						this._TimeOfDayList = this.ModuleSettings.TimeOfDayList;
					}
				}
				return this._TimeOfDayList;
			}
		}

		private ArrayList Users
		{
			get
			{
				if (this._Users == null)
				{
					this._Users = UserController.GetUsers(base.PortalId);
					foreach (UserInfo user in UserController.GetUsers(Null.NullInteger))
					{
						if (!user.IsSuperUser || this.FindUserInfoByUserId(this._Users, user.UserID) != null)
						{
							continue;
						}
						this._Users.Add(user);
					}
					this._Users.Sort(new UserInfoComparer());
				}
				return this._Users;
			}
		}

		private ArrayList ViewReservationsList
		{
			get
			{
				if (this._ViewReservationsList == null)
				{
					if (this.ViewState["ViewReservationsList"] != null)
					{
						//this._ViewReservationsList = this.ModuleSettings.DeserializeUserIDList((string)this.ViewState["ViewReservationsList"]);
						this._ViewReservationsList = this.ModuleSettings.DeserializeEmailList((string)this.ViewState["ViewReservationsList"]);
					}
					else
					{
						this._ViewReservationsList = this.ModuleSettings.ViewReservationsList;
					}
				}
				return this._ViewReservationsList;
			}
		}

		private List<RecurrencePattern> WorkingHours
		{
			get
			{
				if (this._WorkingHours == null)
				{
					if (this.ViewState["WorkingHours"] != null)
					{
						this._WorkingHours = this.DeserializeRecurrencePatternList((string)this.ViewState["WorkingHours"]);
					}
					else
					{
						this._WorkingHours = new List<RecurrencePattern>();
						int num = (!base.IsPostBack || this.workingHoursCategoryDropDownList.SelectedValue == null || !(this.workingHoursCategoryDropDownList.SelectedValue != string.Empty) ? Null.NullInteger : int.Parse(this.workingHoursCategoryDropDownList.SelectedValue));
						Hashtable settings = null;
						if (num != Null.NullInteger)
						{
							settings = (new CategorySettings(base.PortalId, base.TabModuleId, num)).Settings;
							if (!settings.ContainsKey("WorkingHours.1"))
							{
								settings = this.ModuleSettings.Settings;
							}
						}
						else
						{
							settings = this.ModuleSettings.Settings;
						}
						int num1 = 1;
						while (settings.ContainsKey(string.Concat("WorkingHours.", num1)))
						{
							if (!string.IsNullOrEmpty((string)settings[string.Concat("WorkingHours.", num1)]))
							{
								this._WorkingHours.Add(Helper.DeserializeRecurrencePattern((string)settings[string.Concat("WorkingHours.", num1)]));
								num1++;
							}
							else
							{
								break;
							}
						}
					}
				}
				return this._WorkingHours;
			}
		}

		private ArrayList WorkingHoursExceptions
		{
			get
			{
				if (this._WorkingHoursExceptions == null)
				{
					if (this.ViewState["WorkingHoursExceptions"] != null)
					{
						this._WorkingHoursExceptions = this.DeserializeWorkingHoursExceptions((string)this.ViewState["WorkingHoursExceptions"]);
					}
					else
					{
						int num = (!base.IsPostBack || this.workingHoursExceptionsCategoryDropDownList.SelectedValue == null || !(this.workingHoursExceptionsCategoryDropDownList.SelectedValue != string.Empty) ? Null.NullInteger : int.Parse(this.workingHoursExceptionsCategoryDropDownList.SelectedValue));
						if (num != Null.NullInteger)
						{
							CategorySettings categorySetting = new CategorySettings(base.PortalId, base.TabModuleId, num);
							if (!categorySetting.WorkingHoursExceptionsDefined)
							{
								this._WorkingHoursExceptions = this.GetWorkingHoursExceptions(this.ModuleSettings.Settings);
							}
							else
							{
								this._WorkingHoursExceptions = this.GetWorkingHoursExceptions(categorySetting.Settings);
							}
						}
						else
						{
							this._WorkingHoursExceptions = this.GetWorkingHoursExceptions(this.ModuleSettings.Settings);
						}
					}
				}
				return this._WorkingHoursExceptions;
			}
		}

		public Settings()
		{
		}

		protected void AddCashierCommandButtonClicked(object sender, EventArgs e)
		{
			/*if (this.cashierListUsersDropDownList.Visible && this.cashierListUsersDropDownList.SelectedValue != "-1")
			{
				//UserInfo userInfo = this.FindUserInfoByUserId(this.Users, int.Parse(this.cashierListUsersDropDownList.SelectedValue));
				UserInfo userInfo = this.FindUserInfoByEmail(this.Users, this.cashierListUsersDropDownList.SelectedValue);
				//if (this.FindUserInfoByUserId(this.CashierList, int.Parse(this.cashierListUsersDropDownList.SelectedValue)) == null)
				if (this.FindUserInfoByEmail(this.CashierList, this.cashierListUsersDropDownList.SelectedValue) == null)
				{
					this.CashierList.Add(userInfo);
					this.BindCashierListDataGrid();
					this.BindUsersDropDownList(this.cashierListUsersDropDownList, this.CashierList);
					return;
				}
			}
			else if (this.cashierListUsernameTextBox.Visible)*/
			{
				UserInfo userByName = UserController.GetUserByName(base.PortalId, this.cashierListUsernameTextBox.Text);
				if (userByName == null && Helper.IsValidEmail2(this.cashierListUsernameTextBox.Text))
				{
					IList<UserInfo> users = (new UserController()).GetUsersBasicSearch(this.PortalId, 0, 1, "Email", true, "Email", this.cashierListUsernameTextBox.Text);
					if (users != null && users.Count > 0)
					{
						userByName = users[0];
					}
					else
					{
						userByName = new UserInfo();
						userByName.UserID = -1;
						userByName.Email = this.cashierListUsernameTextBox.Text;
						userByName.DisplayName = this.cashierListUsernameTextBox.Text;
					}
				}
				if (userByName == null)
				{
					IList<UserInfo> users = (new UserController()).GetUsersBasicSearch(this.PortalId, 0, 1, "DisplayName", true, "DisplayName", this.cashierListUsernameTextBox.Text);
					if (users != null && users.Count > 0)
					{
						userByName = (UserInfo)users[0];
					}
				}
				//if (userByName != null && this.FindUserInfoByUserId(this.CashierList, userByName.UserID) == null)
				if (userByName != null && this.FindUserInfoByEmail(this.CashierList, userByName.Email) == null)
				{
					this.CashierList.Add(userByName);
					this.BindCashierListDataGrid();
				}
				this.cashierListUsernameTextBox.Text = string.Empty;
			}
		}

		protected void AddCategoryCommandButtonClicked(object sender, EventArgs e)
		{
			if (this.Page.IsValid)
			{
				CategoryInfo categoryInfo = new CategoryInfo()
				{
					TabModuleID = base.TabModuleId,
					Name = this.categoryNameTextBox.Text.Trim(),
					CreatedByUserID = base.UserId,
					CreatedOnDate = DateTime.Now
				};
				categoryInfo = (new CategoryController()).AddCategory(categoryInfo);
				this.CategoryList.Add(categoryInfo);
				this.CategoryList.Sort((CategoryInfo x, CategoryInfo y) => x.Name.CompareTo(y.Name));
				this.categoryNameTextBox.Text = string.Empty;
				this.BindCategoryListDataGrid();
				this.RebindCategoryDependentSections();
			}
		}

		protected void AddDuplicateReservationsCommandButtonClicked(object sender, EventArgs e)
		{
			/*if (this.duplicateReservationsUsersDropDownList.Visible && this.duplicateReservationsUsersDropDownList.SelectedValue != "-1")
			{
				//UserInfo userInfo = this.FindUserInfoByUserId(this.Users, int.Parse(this.duplicateReservationsUsersDropDownList.SelectedValue));
				UserInfo userInfo = this.FindUserInfoByEmail(this.Users, this.duplicateReservationsUsersDropDownList.SelectedValue);
				//if (this.FindUserInfoByUserId(this.DuplicateReservationsList, int.Parse(this.duplicateReservationsUsersDropDownList.SelectedValue)) == null)
				if (this.FindUserInfoByEmail(this.DuplicateReservationsList, this.duplicateReservationsUsersDropDownList.SelectedValue) == null)
				{
					this.DuplicateReservationsList.Add(userInfo);
					this.BindDuplicateReservationsDataGrid();
					this.BindUsersDropDownList(this.duplicateReservationsUsersDropDownList, this.DuplicateReservationsList);
					return;
				}
			}
			else if (this.duplicateReservationsUsernameTextBox.Visible)*/
			{
				UserInfo userByName = UserController.GetUserByName(base.PortalId, this.duplicateReservationsUsernameTextBox.Text);
				if (userByName == null && Helper.IsValidEmail2(this.duplicateReservationsUsernameTextBox.Text))
				{
					IList<UserInfo> users = (new UserController()).GetUsersBasicSearch(this.PortalId, 0, 1, "Email", true, "Email", this.duplicateReservationsUsernameTextBox.Text);
					if (users != null && users.Count > 0)
					{
						userByName = users[0];
					}
					else
					{
						userByName = new UserInfo();
						userByName.UserID = -1;
						userByName.Email = this.duplicateReservationsUsernameTextBox.Text;
						userByName.DisplayName = this.duplicateReservationsUsernameTextBox.Text;
					}
				}
				if (userByName == null)
				{
					IList<UserInfo> users = (new UserController()).GetUsersBasicSearch(this.PortalId, 0, 1, "DisplayName", true, "DisplayName", this.duplicateReservationsUsernameTextBox.Text);
					if (users != null && users.Count > 0)
					{
						userByName = (UserInfo)users[0];
					}
				}
				//if (userByName != null && this.FindUserInfoByUserId(this.DuplicateReservationsList, userByName.UserID) == null)
				if (userByName != null && this.FindUserInfoByEmail(this.DuplicateReservationsList, userByName.Email) == null)
				{
					this.DuplicateReservationsList.Add(userByName);
					this.BindDuplicateReservationsDataGrid();
				}
				this.duplicateReservationsUsernameTextBox.Text = string.Empty;
			}
		}

		protected void AddGlobalModeratorCommandButtonClicked(object sender, EventArgs e)
		{
			/*if (this.moderatorUsersDropDownList.Visible && this.moderatorUsersDropDownList.SelectedValue != "-1")
			{
				//UserInfo userInfo = this.FindUserInfoByUserId(this.Users, int.Parse(this.moderatorUsersDropDownList.SelectedValue));
				UserInfo userInfo = this.FindUserInfoByEmail(this.Users, this.moderatorUsersDropDownList.SelectedValue);
				//if (this.FindUserInfoByUserId(this.ModeratorList, int.Parse(this.moderatorUsersDropDownList.SelectedValue)) == null)
				if (this.FindUserInfoByEmail(this.ModeratorList, this.moderatorUsersDropDownList.SelectedValue) == null)
				{
					this.ModeratorList.Add(userInfo);
					this.BindModeratorsDataGrid();
					this.BindUsersDropDownList(this.moderatorUsersDropDownList, this.ModeratorList);
					return;
				}
			}
			else if (this.moderatorUsernameTextBox.Visible)*/
			{
				UserInfo userByName = UserController.GetUserByName(base.PortalId, this.moderatorUsernameTextBox.Text);
				if (userByName == null && Helper.IsValidEmail2(this.moderatorUsernameTextBox.Text))
				{
					IList<UserInfo> users = (new UserController()).GetUsersBasicSearch(this.PortalId, 0, 1, "Email", true, "Email", this.moderatorUsernameTextBox.Text);
					if (users != null && users.Count > 0)
					{
						userByName = users[0];
					}
					else
					{
						userByName = new UserInfo();
						userByName.UserID = -1;
						userByName.Email = this.moderatorUsernameTextBox.Text;
						userByName.DisplayName = this.moderatorUsernameTextBox.Text;
					}
				}
				if (userByName == null)
				{
					IList<UserInfo> users = (new UserController()).GetUsersBasicSearch(this.PortalId, 0, 1, "DisplayName", true, "DisplayName", this.moderatorUsernameTextBox.Text);
					if (users != null && users.Count > 0)
					{
						userByName = (UserInfo)users[0];
					}
				}
				//if (userByName != null && this.FindUserInfoByUserId(this.ModeratorList, userByName.UserID) == null)
				if (userByName != null && this.FindUserInfoByEmail(this.ModeratorList, userByName.Email) == null)
				{
					this.ModeratorList.Add(userByName);
					this.BindModeratorsDataGrid();
				}
				this.moderatorUsernameTextBox.Text = string.Empty;
			}
		}

		protected void AddModerationHoursCommandButtonClicked(object sender, EventArgs e)
		{
			try
			{
				if (this.Page.IsValid)
				{
					WorkingHoursInfo workingHoursInfo = new WorkingHoursInfo()
					{
						DayOfWeek = (DayOfWeek)int.Parse(this.moderationWeekDaysDropDownList.SelectedValue)
					};
					for (int i = 0; i < this.ModerationHours.Count; i++)
					{
						if (((WorkingHoursInfo)this.ModerationHours[i]).DayOfWeek == workingHoursInfo.DayOfWeek && ((WorkingHoursInfo)this.ModerationHours[i]).AllDay)
						{
							this.ModerationHours.RemoveAt(i);
							i--;
						}
					}
					workingHoursInfo.StartTime = this.GetTime(this.moderationStartHourDropDownList, this.moderationStartMinuteDropDownList, this.moderationStartAMPMDropDownList);
					workingHoursInfo.EndTime = this.GetTime(this.moderationEndHourDropDownList, this.moderationEndMinuteDropDownList, this.moderationEndAMPMDropDownList);
					TimeSpan startTime = workingHoursInfo.StartTime;
					TimeSpan timeSpan = new TimeSpan();
					if (startTime == timeSpan)
					{
						TimeSpan endTime = workingHoursInfo.EndTime;
						timeSpan = new TimeSpan();
						if (endTime != timeSpan)
						{
							goto Label1;
						}
						workingHoursInfo.AllDay = true;
						goto Label0;
					}
				Label1:
					TimeSpan endTime1 = workingHoursInfo.EndTime;
					timeSpan = new TimeSpan();
					if (endTime1 == timeSpan)
					{
						workingHoursInfo.EndTime = new TimeSpan(1, 0, 0, 0);
					}
				Label0:
					this.ModerationHours.Add(workingHoursInfo);
					this.BindModerationHoursDataGrid();
				}
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
			}
		}

		protected void AddTimeOfDayCommandButtonClicked(object sender, EventArgs e)
		{
			try
			{
				if (this.Page.IsValid)
				{
					TimeOfDayInfo timeOfDayInfo = new TimeOfDayInfo()
					{
						Name = this.timeOfDayNameTextBox.Text.Trim(),
						StartTime = this.GetTime(this.timeOfDayStartHourDropDownList, this.timeOfDayStartMinuteDropDownList, this.timeOfDayStartAMPMDropDownList),
						EndTime = this.GetTime(this.timeOfDayEndHourDropDownList, this.timeOfDayEndMinuteDropDownList, this.timeOfDayEndAMPMDropDownList)
					};
					TimeSpan endTime = timeOfDayInfo.EndTime;
					TimeSpan timeSpan = new TimeSpan();
					if (endTime == timeSpan)
					{
						timeSpan = timeOfDayInfo.EndTime;
						timeOfDayInfo.EndTime = timeSpan.Add(new TimeSpan(1, 0, 0, 0));
					}
					this.TimeOfDayList.Add(timeOfDayInfo);
					this.TimeOfDayList.Sort();
					this.BindTimeOfDayDataGrid();
				}
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
			}
		}

		protected void AddUserCommandButtonClicked(object sender, EventArgs e)
		{
			/*if (this.usersDropDownList.Visible && this.usersDropDownList.SelectedValue != "-1")
			{
				//UserInfo userInfo = this.FindUserInfoByUserId(this.Users, int.Parse(this.usersDropDownList.SelectedValue));
				UserInfo userInfo = this.FindUserInfoByEmail(this.Users, this.usersDropDownList.SelectedValue);
				//if (this.FindUserInfoByUserId(this.BCCList, int.Parse(this.usersDropDownList.SelectedValue)) == null)
				if (this.FindUserInfoByEmail(this.BCCList, this.usersDropDownList.SelectedValue) == null)
				{
					this.BCCList.Add(userInfo);
					this.BindBCCListDataGrid();
					this.BindUsersDropDownList(this.usersDropDownList, this.BCCList);
					return;
				}
			}
			else if (this.usernameTextBox.Visible)*/
			{
				UserInfo userByName = UserController.GetUserByName(base.PortalId, this.usernameTextBox.Text);
				if(userByName == null && Helper.IsValidEmail2(this.usernameTextBox.Text))
                {
					IList<UserInfo> users = (new UserController()).GetUsersBasicSearch(this.PortalId, 0, 1, "Email", true, "Email", this.usernameTextBox.Text);
					if (users != null && users.Count > 0)
					{
						userByName = (UserInfo)users[0];
					}
					else
                    {
						userByName = new UserInfo();
						userByName.UserID = -1;
						userByName.Email = this.usernameTextBox.Text;
						userByName.DisplayName = this.usernameTextBox.Text;
                    }
				}
				if(userByName == null)
                {
					IList<UserInfo> users = (new UserController()).GetUsersBasicSearch(this.PortalId, 0, 1, "DisplayName", true, "DisplayName", this.usernameTextBox.Text);
					if (users != null && users.Count > 0)
					{
						userByName = (UserInfo)users[0];
					}
				}
				//if (userByName != null && this.FindUserInfoByUserId(this.BCCList, userByName.UserID) == null)
				if (userByName != null && this.FindUserInfoByEmail(this.BCCList, userByName.Email) == null)
				{
					this.BCCList.Add(userByName);
					this.BindBCCListDataGrid();
				}
				this.usernameTextBox.Text = string.Empty;
			}
		}

		protected void AddViewReservationsCommandButtonClicked(object sender, EventArgs e)
		{
			/*if (this.viewReservationsUsersDropDownList.Visible && this.viewReservationsUsersDropDownList.SelectedValue != "-1")
			{
				//UserInfo userInfo = this.FindUserInfoByUserId(this.Users, int.Parse(this.viewReservationsUsersDropDownList.SelectedValue));
				UserInfo userInfo = this.FindUserInfoByEmail(this.Users, this.viewReservationsUsersDropDownList.SelectedValue);
				//if (this.FindUserInfoByUserId(this.ViewReservationsList, int.Parse(this.viewReservationsUsersDropDownList.SelectedValue)) == null)
				if (this.FindUserInfoByEmail(this.ViewReservationsList, this.viewReservationsUsersDropDownList.SelectedValue) == null)
				{
					this.ViewReservationsList.Add(userInfo);
					this.BindViewReservationsDataGrid();
					this.BindUsersDropDownList(this.viewReservationsUsersDropDownList, this.ViewReservationsList);
					return;
				}
			}
			else if (this.viewReservationsUsernameTextBox.Visible)*/
			{
				UserInfo userByName = UserController.GetUserByName(base.PortalId, this.viewReservationsUsernameTextBox.Text);
				if (userByName == null && Helper.IsValidEmail2(this.viewReservationsUsernameTextBox.Text))
				{
					IList<UserInfo> users = (new UserController()).GetUsersBasicSearch(this.PortalId, 0, 1, "Email", true, "Email", this.viewReservationsUsernameTextBox.Text);
					if (users != null && users.Count > 0)
					{
						userByName = users[0];
					}
					else
					{
						userByName = new UserInfo();
						userByName.UserID = -1;
						userByName.Email = this.viewReservationsUsernameTextBox.Text;
						userByName.DisplayName = this.viewReservationsUsernameTextBox.Text;
					}
				}
				if (userByName == null)
				{
					IList<UserInfo> users = (new UserController()).GetUsersBasicSearch(this.PortalId, 0, 1, "DisplayName", true, "DisplayName", this.viewReservationsUsernameTextBox.Text);
					if (users != null && users.Count > 0)
					{
						userByName = (UserInfo)users[0];
					}
				}
				//if (userByName != null && this.FindUserInfoByUserId(this.ViewReservationsList, userByName.UserID) == null)
				if (userByName != null && this.FindUserInfoByEmail(this.ViewReservationsList, userByName.Email) == null)
				{
					this.ViewReservationsList.Add(userByName);
					this.BindViewReservationsDataGrid();
				}
				this.viewReservationsUsernameTextBox.Text = string.Empty;
			}
		}

		protected void AddWorkingHours(object sender, EventArgs e)
		{
			try
			{
				this.recurrencepatterncontrol.Visible = true;
				this.addWorkingHoursCommandButton.Visible = false;
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
			}
		}

		protected void AddWorkingHoursExceptionCommandButtonClicked(object sender, EventArgs e)
		{
			try
			{
				if (this.Page.IsValid)
				{
					WorkingHoursExceptionInfo workingHoursExceptionInfo = new WorkingHoursExceptionInfo()
					{
						Date = DateTime.Parse(this.workingHoursExceptionDateTextBox.Text)
					};
					if (!this.workingHoursExceptionNoWorkingHoursRadioButton.Checked)
					{
						for (int i = 0; i < this.WorkingHoursExceptions.Count; i++)
						{
							WorkingHoursExceptionInfo item = (WorkingHoursExceptionInfo)this.WorkingHoursExceptions[i];
							if (item.Date == workingHoursExceptionInfo.Date && (item.StartTime == item.EndTime || item.AllDay))
							{
								this.WorkingHoursExceptions.RemoveAt(i);
								i--;
							}
						}
						workingHoursExceptionInfo.StartTime = this.GetTime(this.workingHoursExceptionStartHourDropDownList, this.workingHoursExceptionStartMinuteDropDownList, this.workingHoursExceptionStartAMPMDropDownList);
						workingHoursExceptionInfo.EndTime = this.GetTime(this.workingHoursExceptionEndHourDropDownList, this.workingHoursExceptionEndMinuteDropDownList, this.workingHoursExceptionEndAMPMDropDownList);
						TimeSpan startTime = workingHoursExceptionInfo.StartTime;
						TimeSpan timeSpan = new TimeSpan();
						if (startTime == timeSpan)
						{
							TimeSpan endTime = workingHoursExceptionInfo.EndTime;
							timeSpan = new TimeSpan();
							if (endTime == timeSpan)
							{
								workingHoursExceptionInfo.AllDay = true;
							}
						}
						TimeSpan endTime1 = workingHoursExceptionInfo.EndTime;
						timeSpan = new TimeSpan();
						if (endTime1 == timeSpan)
						{
							workingHoursExceptionInfo.EndTime = new TimeSpan(1, 0, 0, 0);
						}
					}
					else
					{
						for (int j = 0; j < this.WorkingHoursExceptions.Count; j++)
						{
							if (((WorkingHoursExceptionInfo)this.WorkingHoursExceptions[j]).Date == workingHoursExceptionInfo.Date)
							{
								this.WorkingHoursExceptions.RemoveAt(j);
								j--;
							}
						}
					}
					this.WorkingHoursExceptions.Add(workingHoursExceptionInfo);
					this.BindWorkingHoursExceptionsDataGrid();
				}
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
			}
		}

		protected void AllowCategorySelectionCheckBoxCheckedChanged(object sender, EventArgs e)
		{
			this.RebindCategoryDependentSections();
		}

		protected void BCCListCategoryDropDownListSelectedIndexChanged(object sender, EventArgs e)
		{
			this.BindBCCListSection(int.Parse(this.bccListCategoryDropDownList.SelectedValue));
		}

		protected void BCCListResetCommandButtonClicked(object sender, EventArgs e)
		{
			int num = int.Parse(this.bccListCategoryDropDownList.SelectedValue);
			(new CategorySettingController()).DeleteCategorySetting(num, "BCCList");
			this.BindCategoriesDropDownList(this.bccListCategoryDropDownList, "BCCList", null, null);
			this.BindBCCListSection(num);
		}

		protected void BCCListUpdateCommandButtonClicked(object sender, EventArgs e)
		{
			this.UpdateBCCListSection(true);
		}

		protected void BindAMPMDropDownList(DropDownList dropDownList)
		{
			dropDownList.Visible = !this.Is24HourClock;
			if (!this.Is24HourClock)
			{
				dropDownList.Items.Clear();
				dropDownList.Items.Add(new ListItem(CultureInfo.CurrentCulture.DateTimeFormat.AMDesignator, "AM"));
				dropDownList.Items.Add(new ListItem(CultureInfo.CurrentCulture.DateTimeFormat.PMDesignator, "PM"));
			}
		}

		protected void BindBCCListDataGrid()
		{
			this.bccListDataGrid.DataSource = this.BCCList;
			this.bccListDataGrid.DataBind();
			this.noUsersLabel.Visible = this.bccListDataGrid.Items.Count == 0;
		}

		protected void BindBCCListSection()
		{
			bool flag;
			HtmlTableRow htmlTableRow = this.bccListUpdateResetTableRow;
			HtmlTableRow htmlTableRow1 = this.bccListCategoryTableRow;
			flag = (base.IsPostBack ? this.allowCategorySelectionCheckBox.Checked : this.ModuleSettings.AllowCategorySelection);
			bool flag1 = flag;
			htmlTableRow1.Visible = flag;
			htmlTableRow.Visible = flag1;
			if (this.bccListCategoryTableRow.Visible)
			{
				this.BindCategoriesDropDownList(this.bccListCategoryDropDownList, "BCCList", null, null);
			}
			this.BindBCCListSection(Null.NullInteger);
		}

		protected void BindBCCListSection(int categoryID)
		{
			this.bccListCategoryDropDownList.SelectedValue = categoryID.ToString();
			CategorySettings categorySetting = null;
			if (categoryID != Null.NullInteger)
			{
				categorySetting = new CategorySettings(base.PortalId, base.TabModuleId, categoryID);
			}
			this.ViewState.Remove("BCCList");
			this.BindBCCListDataGrid();
			//if (this.Users.Count > 100)
			{
				RequiredFieldValidator requiredFieldValidator = this.usernameRequiredFieldValidator;
				TextBox textBox = this.usernameTextBox;
				bool flag = false;
				//this.usersDropDownList.Visible = false;
				bool flag1 = !flag;
				bool flag2 = flag1;
				textBox.Visible = flag1;
				requiredFieldValidator.Visible = flag2;
			}
			/*else
			{
				this.BindUsersDropDownList(this.usersDropDownList, this.BCCList);
			}*/
			this.bccListResetCommandButton.Visible = (categorySetting == null ? false : categorySetting.IsDefined("BCCList"));
		}

		protected void BindCashierListDataGrid()
		{
			this.cashierListDataGrid.DataSource = this.CashierList;
			this.cashierListDataGrid.DataBind();
			this.noCashiersLabel.Visible = this.cashierListDataGrid.Items.Count == 0;
		}

		protected void BindCashierListSection()
		{
			bool flag;
			HtmlTableRow htmlTableRow = this.cashierUpdateResetTableRow;
			HtmlTableRow htmlTableRow1 = this.cashierListCategoryTableRow;
			flag = (base.IsPostBack ? this.allowCategorySelectionCheckBox.Checked : this.ModuleSettings.AllowCategorySelection);
			bool flag1 = flag;
			htmlTableRow1.Visible = flag;
			htmlTableRow.Visible = flag1;
			if (this.cashierListCategoryTableRow.Visible)
			{
				this.BindCategoriesDropDownList(this.cashierListCategoryDropDownList, "CashierList", null, null);
			}
			this.BindCashierListSection(Null.NullInteger);
		}

		protected void BindCashierListSection(int categoryID)
		{
			this.cashierListCategoryDropDownList.SelectedValue = categoryID.ToString();
			CategorySettings categorySetting = null;
			if (categoryID != Null.NullInteger)
			{
				categorySetting = new CategorySettings(base.PortalId, base.TabModuleId, categoryID);
			}
			this.ViewState.Remove("CashierList");
			this.BindCashierListDataGrid();
			//if (this.Users.Count > 100)
			{
				RequiredFieldValidator requiredFieldValidator = this.cashierListUsernameRequiredFieldValidator;
				TextBox textBox = this.cashierListUsernameTextBox;
				bool flag = false;
				//this.cashierListUsersDropDownList.Visible = false;
				bool flag1 = !flag;
				bool flag2 = flag1;
				textBox.Visible = flag1;
				requiredFieldValidator.Visible = flag2;
			}
			/*else
			{
				this.BindUsersDropDownList(this.cashierListUsersDropDownList, this.CashierList);
			}*/
			this.cashierListResetCommandButton.Visible = (categorySetting == null ? false : categorySetting.IsDefined("CashierList"));
		}

		private void BindCategoriesDropDownList(DropDownList dropDownList, string settingName, string settingName2 = null, string settingName3 = null)
		{
			int categoryID;
			dropDownList.Items.Clear();
			foreach (CategoryInfo categoryList in (new CategoryController()).GetCategoryList(base.TabModuleId))
			{
				CategorySettings categorySetting = new CategorySettings(base.PortalId, base.TabModuleId, categoryList.CategoryID);
				ListItemCollection items = dropDownList.Items;
				string str = string.Concat(categoryList.Name, (categorySetting.IsDefined(settingName) || settingName2 != null && categorySetting.IsDefined(settingName2) || settingName3 != null && categorySetting.IsDefined(settingName3) ? base.Server.HtmlDecode(" &#10004;") : string.Empty));
				categoryID = categoryList.CategoryID;
				items.Add(new ListItem(str, categoryID.ToString()));
			}
			string str1 = Localization.GetString("All", base.LocalResourceFile);
			categoryID = Null.NullInteger;
			ListItem listItem = new ListItem(str1, categoryID.ToString());
			dropDownList.Items.Insert(0, listItem);
		}

		protected void BindCategoriesSection()
		{
			HtmlTableRow htmlTableRow = this.selectCategoryLastTableRow;
			HtmlTableRow htmlTableRow1 = this.categoryListTableRow2;
			HtmlTableRow htmlTableRow2 = this.categoryListTableRow;
			HtmlTableRow htmlTableRow3 = this.categorySelectionModeTableRow;
			HtmlTableRow htmlTableRow4 = this.bindUponSelectionTableRow;
			HtmlTableRow htmlTableRow5 = this.allowCrossCategoryConflictsTableRow;
			HtmlTableRow htmlTableRow6 = this.displayUnavailableCategoriesTableRow;
			CheckBox checkBox = this.allowCategorySelectionCheckBox;
			bool allowCategorySelection = this.ModuleSettings.AllowCategorySelection;
			bool flag = allowCategorySelection;
			checkBox.Checked = allowCategorySelection;
			bool flag1 = flag;
			bool flag2 = flag1;
			htmlTableRow6.Visible = flag1;
			bool flag3 = flag2;
			bool flag4 = flag3;
			htmlTableRow5.Visible = flag3;
			bool flag5 = flag4;
			bool flag6 = flag5;
			htmlTableRow4.Visible = flag5;
			bool flag7 = flag6;
			bool flag8 = flag7;
			htmlTableRow3.Visible = flag7;
			bool flag9 = flag8;
			bool flag10 = flag9;
			htmlTableRow2.Visible = flag9;
			bool flag11 = flag10;
			bool flag12 = flag11;
			htmlTableRow1.Visible = flag11;
			htmlTableRow.Visible = flag12;
			this.selectCategoryLastCheckBox.Checked = this.ModuleSettings.SelectCategoryLast;
			this.preventCrossCategoryConflictsCheckBox.Checked = this.ModuleSettings.PreventCrossCategoryConflicts;
			this.bindUponSelectionCheckBox.Checked = this.ModuleSettings.BindUponCategorySelection;
			this.displayUnavailableCategoriesCheckBox.Checked = this.ModuleSettings.DisplayUnavailableCategories;
			this.SelectCategoryLastChanged(null, null);
			this.PreventCrossCategoryConflictsChanged(null, null);
			this.BindCategoryListDataGrid();
		}

		protected void BindCategoryListDataGrid()
		{
			this.categoryListDataGrid.DataSource = this.CategoryList;
			this.categoryListDataGrid.DataBind();
			this.noCategoriesLabel.Visible = this.categoryListDataGrid.Items.Count == 0;
		}

		protected void BindCategoryPermissionsSection()
		{
			LinkButton linkButton = this.categoryPermissionsUpdateCommandButton;
			HtmlTableRow htmlTableRow = this.selectCategoryLastTableRow;
			HtmlTableRow htmlTableRow1 = this.categoryPermissionsTableRow;
			HtmlTableRow htmlTableRow2 = this.allowCrossCategoryConflictsTableRow;
			HtmlTableRow htmlTableRow3 = this.categorySelectionModeTableRow;
			HtmlTableRow htmlTableRow4 = this.bindUponSelectionTableRow;
			HtmlTableRow htmlTableRow5 = this.displayUnavailableCategoriesTableRow;
			HtmlTableRow htmlTableRow6 = this.categoryListTableRow;
			HtmlTableRow htmlTableRow7 = this.categoryListTableRow2;
			bool @checked = this.allowCategorySelectionCheckBox.Checked;
			bool flag = @checked;
			htmlTableRow7.Visible = @checked;
			bool flag1 = flag;
			bool flag2 = flag1;
			htmlTableRow6.Visible = flag1;
			bool flag3 = flag2;
			bool flag4 = flag3;
			htmlTableRow5.Visible = flag3;
			bool flag5 = flag4;
			bool flag6 = flag5;
			htmlTableRow4.Visible = flag5;
			bool flag7 = flag6;
			bool flag8 = flag7;
			htmlTableRow3.Visible = flag7;
			bool flag9 = flag8;
			bool flag10 = flag9;
			htmlTableRow2.Visible = flag9;
			bool flag11 = flag10;
			bool flag12 = flag11;
			htmlTableRow1.Visible = flag11;
			bool flag13 = flag12;
			bool flag14 = flag13;
			htmlTableRow.Visible = flag13;
			linkButton.Visible = flag14;
			this.bindUponSelectionTableRow.Visible = (!this.allowCategorySelectionCheckBox.Checked || this.preventCrossCategoryConflictsCheckBox.Checked ? false : !this.selectCategoryLastCheckBox.Checked);
			if (this.allowCategorySelectionCheckBox.Checked)
			{
				this.BindCategoriesDropDownList(this.categoryPermissionsDropDownList, "CategoryPermissions", null, null);
			}
			this.BindCategoryPermissionsSection(Null.NullInteger);
		}

		protected void BindCategoryPermissionsSection(int categoryID)
		{
			this.categoryPermissionsDropDownList.SelectedValue = categoryID.ToString();
			CategorySettings categorySetting = null;
			if (categoryID != Null.NullInteger)
			{
				categorySetting = new CategorySettings(base.PortalId, base.TabModuleId, categoryID);
			}
			this.ViewState.Remove("CategoryPermissionsList");
			this._CategoryPermissionsList = null;
			this.BindRolesCheckboxList();
			this.categoryPermissionsResetCommandButton.Visible = (categorySetting == null ? false : categorySetting.IsDefined("CategoryPermissions"));
		}

		protected void BindCurrencyDropDownList(DropDownList dropDownList)
		{
			ListController listController = new ListController();
			MethodInfo method = listController.GetType().GetMethod("GetListEntryInfoItems", new Type[] { typeof(string) }) ?? listController.GetType().GetMethod("GetListEntryInfoCollection", new Type[] { typeof(string) });
			dropDownList.DataSource = method.Invoke(listController, new object[] { "Currency" });
			dropDownList.DataTextField = "Text";
			dropDownList.DataValueField = "Value";
			dropDownList.DataBind();
		}

		protected void BindDuplicateReservationsDataGrid()
		{
			this.duplicateReservationsDataGrid.DataSource = this.DuplicateReservationsList;
			this.duplicateReservationsDataGrid.DataBind();
			this.noDuplicateReservationsLabel.Visible = this.duplicateReservationsDataGrid.Items.Count == 0;
		}

		protected void BindDuplicateReservationsSection()
		{
			this.ViewState.Remove("DuplicateReservationsList");
			this.BindDuplicateReservationsDataGrid();
			/*if (this.Users.Count <= 100)
			{
				this.BindUsersDropDownList(this.duplicateReservationsUsersDropDownList, this.DuplicateReservationsList);
				return;
			}*/
			RequiredFieldValidator requiredFieldValidator = this.duplicateReservationsUsernameRequiredFieldValidator;
			TextBox textBox = this.duplicateReservationsUsernameTextBox;
			bool flag = false;
			//this.duplicateReservationsUsersDropDownList.Visible = false;
			bool flag1 = !flag;
			bool flag2 = flag1;
			textBox.Visible = flag1;
			requiredFieldValidator.Visible = flag2;
		}

		protected void BindGeneralSettingsSection()
		{
			this.BindTimeZoneDropDownList();
			if (this.timeZoneDropDownList.Items.FindByValue(this.ModuleSettings.TimeZone.Id) != null)
			{
				this.timeZoneDropDownList.SelectedValue = this.ModuleSettings.TimeZone.Id;
			}
			this.BindThemesDropDownList();
			if (this.themeDropDownList.Items.FindByValue(this.ModuleSettings.Theme) != null)
			{
				this.themeDropDownList.SelectedValue = this.ModuleSettings.Theme;
			}
			this.contactInfoFirstCheckBox.Checked = this.ModuleSettings.ContactInfoFirst;
			this.categorySelectionModeList.Checked = this.ModuleSettings.CategorySelectionMode == Gafware.Modules.Reservations.ModuleSettings.SelectionModeEnum.HorizontalScroll;
			this.categorySelectionModeList.Enabled = Helper.IsjQuery17orHigher;
			this.categorySelectionModeDropDownList.Checked = this.ModuleSettings.CategorySelectionMode == Gafware.Modules.Reservations.ModuleSettings.SelectionModeEnum.DropDownList;
			this.categorySelectionModeListBox.Checked = this.ModuleSettings.CategorySelectionMode == Gafware.Modules.Reservations.ModuleSettings.SelectionModeEnum.ListBox;
			RadioButton radioButton = this.displayListRadioButton;
			RadioButton radioButton1 = this.displayCalendarRadioButton;
			bool displayCalendar = this.ModuleSettings.DisplayCalendar;
			bool flag = displayCalendar;
			radioButton1.Checked = displayCalendar;
			radioButton.Checked = !flag;
			this.displayListRadioButton.Enabled = Helper.IsjQuery17orHigher;
			this.timeOfDaySelectionModeList.Checked = this.ModuleSettings.TimeOfDaySelectionMode == Gafware.Modules.Reservations.ModuleSettings.SelectionModeEnum.HorizontalScroll;
			this.timeOfDaySelectionModeList.Enabled = Helper.IsjQuery17orHigher;
			this.timeOfDaySelectionModeDropDownList.Checked = this.ModuleSettings.TimeOfDaySelectionMode == Gafware.Modules.Reservations.ModuleSettings.SelectionModeEnum.DropDownList;
			this.timeOfDaySelectionModeListBox.Checked = this.ModuleSettings.TimeOfDaySelectionMode == Gafware.Modules.Reservations.ModuleSettings.SelectionModeEnum.ListBox;
			this.timeSelectionModeList.Checked = this.ModuleSettings.TimeSelectionMode == Gafware.Modules.Reservations.ModuleSettings.SelectionModeEnum.HorizontalScroll;
			this.timeSelectionModeList.Enabled = Helper.IsjQuery17orHigher;
			this.timeSelectionModeDropDownList.Checked = this.ModuleSettings.TimeSelectionMode == Gafware.Modules.Reservations.ModuleSettings.SelectionModeEnum.DropDownList;
			this.timeSelectionModeListBox.Checked = this.ModuleSettings.TimeSelectionMode == Gafware.Modules.Reservations.ModuleSettings.SelectionModeEnum.ListBox;
			this.durationSelectionModeList.Checked = this.ModuleSettings.DurationSelectionMode == Gafware.Modules.Reservations.ModuleSettings.SelectionModeEnum.HorizontalScroll;
			this.durationSelectionModeList.Enabled = Helper.IsjQuery17orHigher;
			this.durationSelectionModeDropDownList.Checked = this.ModuleSettings.DurationSelectionMode == Gafware.Modules.Reservations.ModuleSettings.SelectionModeEnum.DropDownList;
			this.durationSelectionModeListBox.Checked = this.ModuleSettings.DurationSelectionMode == Gafware.Modules.Reservations.ModuleSettings.SelectionModeEnum.ListBox;
			this.displayRemainingReservationsCheckBox.Checked = this.ModuleSettings.DisplayRemainingReservations;
			this.displayEndTimeCheckBox.Checked = this.ModuleSettings.DisplayEndTime;
			this.allowDescriptionCheckBox.Checked = this.ModuleSettings.AllowDescription;
			this.allowSchedulingAnotherReservationCheckBox.Checked = this.ModuleSettings.AllowSchedulingAnotherReservation;
			this.requireEmailCheckBox.Checked = this.ModuleSettings.RequireEmail;
			this.requirePhoneCheckBox.Checked = this.ModuleSettings.RequirePhone;
			this.allowLookupByPhoneCheckBox.Checked = this.ModuleSettings.AllowLookupByPhone;
			this.redirectUrlTextBox.Text = this.ModuleSettings.RedirectUrl;
			this.skipContactInfoCheckBox.Checked = this.ModuleSettings.SkipContactInfoForAuthenticatedUsers;
			this.requireVerificationCodeTableRow.Visible = this.requireEmailCheckBox.Checked;
			this.requireVerificationCodeCheckBox.Checked = this.ModuleSettings.RequireVerificationCode;
		}

		protected void BindHoursDropDownList(DropDownList dropDownList)
		{
			DateTime date;
			DateTime dateTime;
			dropDownList.Items.Clear();
			DateTime date1 = DateTime.Now.Date;
			while (true)
			{
				DateTime dateTime1 = date1;
				if (this.Is24HourClock)
				{
					date = DateTime.Now.Date;
					dateTime = date.AddDays(1);
				}
				else
				{
					date = DateTime.Now.Date;
					dateTime = date.AddHours(12);
				}
				if (dateTime1 >= dateTime)
				{
					break;
				}
				ListItemCollection items = dropDownList.Items;
				string str = date1.ToString((this.Is24HourClock ? "HH" : "hh"));
				double totalHours = date1.TimeOfDay.TotalHours;
				items.Add(new ListItem(str, totalHours.ToString()));
				date1 = date1.AddHours(1);
			}
		}

		protected void BindMailTemplates()
		{
			this.mailTemplateDropDownList.Items.Clear();
			this.mailTemplateDropDownList.Items.Add(new ListItem(Localization.GetString("Confirmation", base.LocalResourceFile), "Confirmation"));
			this.mailTemplateDropDownList.Items.Add(new ListItem(Localization.GetString("Modification", base.LocalResourceFile), "Modification"));
			this.mailTemplateDropDownList.Items.Add(new ListItem(Localization.GetString("Rescheduled", base.LocalResourceFile), "Rescheduled"));
			this.mailTemplateDropDownList.Items.Add(new ListItem(Localization.GetString("Cancellation", base.LocalResourceFile), "Cancellation"));
			this.mailTemplateDropDownList.Items.Add(new ListItem(Localization.GetString("Moderator", base.LocalResourceFile), "Moderator"));
			this.mailTemplateDropDownList.Items.Add(new ListItem(Localization.GetString("Declined", base.LocalResourceFile), "Declined"));
			this.mailTemplateDropDownList.Items.Add(new ListItem(Localization.GetString("Reminder", base.LocalResourceFile), "Reminder"));
			this.mailTemplateDropDownList.Items.Add(new ListItem(Localization.GetString("VerificationCode", base.LocalResourceFile), "VerificationCode"));
			this.mailTemplateDropDownList.Items.Add(new ListItem(Localization.GetString("DuplicateReservation", base.LocalResourceFile), "DuplicateReservation"));
			if (this.IsProfessional)
			{
				this.mailTemplateDropDownList.Items.Add(new ListItem(Localization.GetString("PendingRescheduleRefund", base.LocalResourceFile), "PendingRescheduleRefund"));
				this.mailTemplateDropDownList.Items.Add(new ListItem(Localization.GetString("PendingCancellationRefund", base.LocalResourceFile), "PendingCancellationRefund"));
				this.mailTemplateDropDownList.Items.Add(new ListItem(Localization.GetString("PendingDeclinationRefund", base.LocalResourceFile), "PendingDeclinationRefund"));
			}
			this.mailTemplateDropDownList.SelectedIndex = 0;
			this.MailTemplateDropDownListSelectedIndexChanged(null, null);
		}

		protected void BindMailTemplatesSection()
		{
			this.mailFromTextBox.Text = this.ModuleSettings.MailFrom;
			this.attachICalendarCheckBox.Checked = this.ModuleSettings.AttachICalendar;
			this.iCalendarAttachmentFileNameTextBox.Text = this.ModuleSettings.ICalendarAttachmentFileName;
			this.BindMailTemplates();
		}

		protected void BindMinutesDropDownList(DropDownList dropDownList)
		{
			dropDownList.Items.Clear();
			for (int i = 0; i < 60; i = i + 5)
			{
				dropDownList.Items.Add(new ListItem(i.ToString("00"), i.ToString()));
			}
		}

		protected void BindModerationHoursDataGrid()
		{
			this.ModerationHours.Sort(new WorkingHourInfoComparer());
			this.moderationHoursDataGrid.DataSource = this.ModerationHours;
			this.moderationHoursDataGrid.DataBind();
			this.noModerationHoursLabel.Visible = this.moderationHoursDataGrid.Items.Count == 0;
		}

		protected void BindModerationSection()
		{
			bool flag;
			HtmlTableRow htmlTableRow = this.moderationUpdateResetTableRow;
			HtmlTableRow htmlTableRow1 = this.moderationCategoryTableRow;
			flag = (base.IsPostBack ? this.allowCategorySelectionCheckBox.Checked : this.ModuleSettings.AllowCategorySelection);
			bool flag1 = flag;
			htmlTableRow1.Visible = flag;
			htmlTableRow.Visible = flag1;
			if (this.moderationCategoryTableRow.Visible)
			{
				this.BindCategoriesDropDownList(this.moderationCategoryDropDownList, "Moderate", null, null);
			}
			if (!base.IsPostBack)
			{
				this.BindWeekDaysDropDownList(this.moderationWeekDaysDropDownList);
				this.BindHoursDropDownList(this.moderationStartHourDropDownList);
				this.BindHoursDropDownList(this.moderationEndHourDropDownList);
				this.BindMinutesDropDownList(this.moderationStartMinuteDropDownList);
				this.BindMinutesDropDownList(this.moderationEndMinuteDropDownList);
				this.BindAMPMDropDownList(this.moderationStartAMPMDropDownList);
				this.BindAMPMDropDownList(this.moderationEndAMPMDropDownList);
				this.moderationStartHourDropDownList.SelectedValue = "8";
				if (!this.Is24HourClock)
				{
					this.moderationEndHourDropDownList.SelectedValue = "5";
					this.moderationEndAMPMDropDownList.SelectedValue = "PM";
				}
				else
				{
					this.moderationEndHourDropDownList.SelectedValue = "17";
				}
			}
			this.BindModerationSection(Null.NullInteger);
		}

		protected void BindModerationSection(int categoryID)
		{
			bool flag;
			this.moderationCategoryDropDownList.SelectedValue = categoryID.ToString();
			CategorySettings categorySetting = null;
			if (categoryID != Null.NullInteger)
			{
				categorySetting = new CategorySettings(base.PortalId, base.TabModuleId, categoryID);
			}
			HtmlTableRow htmlTableRow = this.moderationHoursTableRow;
			HtmlTableRow htmlTableRow1 = this.globalModeratorsDropDownListTableRow;
			HtmlTableRow htmlTableRow2 = this.globalModeratorsDataGridTableRow;
			CheckBox checkBox = this.moderateCheckBox;
			flag = (categorySetting != null ? categorySetting.Moderate : this.ModuleSettings.Moderate);
			bool flag1 = flag;
			checkBox.Checked = flag;
			bool flag2 = flag1;
			bool flag3 = flag2;
			htmlTableRow2.Visible = flag2;
			bool flag4 = flag3;
			bool flag5 = flag4;
			htmlTableRow1.Visible = flag4;
			htmlTableRow.Visible = flag5;
			this.ViewState.Remove("ModeratorList");
			this._ModeratorList = null;
			this.ViewState.Remove("ModerationHours");
			this._ModerationHours = null;
			this.BindModeratorsDataGrid();
			//if (this.Users.Count > 100)
			{
				RequiredFieldValidator requiredFieldValidator = this.moderatorUsernameRequiredFieldValidator;
				TextBox textBox = this.moderatorUsernameTextBox;
				flag3 = false;
				//this.moderatorUsersDropDownList.Visible = false;
				bool flag6 = !flag3;
				flag5 = flag6;
				textBox.Visible = flag6;
				requiredFieldValidator.Visible = flag5;
			}
			/*else
			{
				this.BindUsersDropDownList(this.moderatorUsersDropDownList, this.ModeratorList);
			}*/
			this.BindModerationHoursDataGrid();
			this.moderationResetCommandButton.Visible = (categorySetting == null ? false : categorySetting.IsDefined("Moderate"));
		}

		protected void BindModeratorsDataGrid()
		{
			this.globalModeratorsDataGrid.DataSource = this.ModeratorList;
			this.globalModeratorsDataGrid.DataBind();
			this.noGlobalModeratorsLabel.Visible = this.globalModeratorsDataGrid.Items.Count == 0;
		}

		protected void BindRemindersSection()
		{
			bool flag;
			this.sendRemindersCheckBox.Checked = this.ModuleSettings.SendReminders;
			this.requireConfirmationCheckBox.Checked = this.ModuleSettings.RequireConfirmation;
			this.BindTimeSpanDropDownList(this.sendRemindersWhenDropDownList);
			this.SetTimeSpan(this.ModuleSettings.SendRemindersWhen, this.sendRemindersWhenTextBox, this.sendRemindersWhenDropDownList);
			this.BindTimeSpanDropDownList(this.requireConfirmationWhenDropDownList);
			this.SetTimeSpan(this.ModuleSettings.RequireConfirmationWhen, this.requireConfirmationWhenTextBox, this.requireConfirmationWhenDropDownList);
			this.sendRemindersViaDropDownList.DataSource = Helper.LocalizeEnum(typeof(SendReminderVia), base.LocalResourceFile);
			this.sendRemindersViaDropDownList.DataTextField = "LocalizedName";
			this.sendRemindersViaDropDownList.DataValueField = "Value";
			this.sendRemindersViaDropDownList.DataBind();
			this.sendRemindersViaDropDownList.SelectedValue = this.ModuleSettings.SendRemindersVia.ToString();
			HtmlTableRow htmlTableRow = this.requireConfirmationTableRow;
			HtmlTableRow htmlTableRow1 = this.sendRemindersWhenTableRow;
			HtmlTableRow htmlTableRow2 = this.sendRemindersViaTableRow;
			bool @checked = this.sendRemindersCheckBox.Checked;
			bool flag1 = @checked;
			htmlTableRow2.Visible = @checked;
			bool flag2 = flag1;
			bool flag3 = flag2;
			htmlTableRow1.Visible = flag2;
			htmlTableRow.Visible = flag3;
			HtmlTableRow htmlTableRow3 = this.requireConfirmationTableRow2;
			HtmlTableRow htmlTableRow4 = this.requireConfirmationWhenTableRow;
			flag = (!this.requireConfirmationTableRow.Visible ? false : this.requireConfirmationCheckBox.Checked);
			flag3 = flag;
			htmlTableRow4.Visible = flag;
			htmlTableRow3.Visible = flag3;
			this.sendRemindersViaTableRow.Visible = (!this.sendRemindersViaTableRow.Visible ? false : this.IsProfessional);
		}

		protected void BindReservationFeesSection()
		{
			bool flag;
			HtmlTableRow htmlTableRow = this.reservationFeesUpdateResetTableRow;
			HtmlTableRow htmlTableRow1 = this.reservationFeesCategoryTableRow;
			flag = (base.IsPostBack ? this.allowCategorySelectionCheckBox.Checked : this.ModuleSettings.AllowCategorySelection);
			bool flag1 = flag;
			htmlTableRow1.Visible = flag;
			htmlTableRow.Visible = flag1;
			if (this.reservationFeesCategoryTableRow.Visible)
			{
				this.BindCategoriesDropDownList(this.reservationFeesCategoryDropDownList, "SchedulingFee", "FeeScheduleType", null);
			}
			this.payPalAccountTextBox.Text = this.ModuleSettings.PayPalAccount;
			this.payPalUrlTextBox.Text = this.ModuleSettings.PayPalUrl;
			this.payPalItemDescriptionTextBox.Text = this.ModuleSettings.ItemDescription;
			this.pendingPaymentExpirationTextBox.Text = this.ModuleSettings.PendingPaymentExpiration.TotalMinutes.ToString();
			this.allowPayLaterCheckBox.Checked = this.ModuleSettings.AllowPayLater;
			this.BindCurrencyDropDownList(this.currencyDropDownList);
			this.currencyDropDownList.SelectedValue = this.ModuleSettings.Currency;
			this.paymentMethodDropDownList.Items.Clear();
			ListItemCollection items = this.paymentMethodDropDownList.Items;
			PaymentMethod paymentMethod = PaymentMethod.PayPalPaymentsStandard;
			string str = Localization.GetString(paymentMethod.ToString(), base.LocalResourceFile);
			paymentMethod = PaymentMethod.PayPalPaymentsStandard;
			items.Add(new ListItem(str, paymentMethod.ToString()));
			ListItemCollection listItemCollections = this.paymentMethodDropDownList.Items;
			paymentMethod = PaymentMethod.AuthorizeNetSIM;
			string str1 = Localization.GetString(paymentMethod.ToString(), base.LocalResourceFile);
			paymentMethod = PaymentMethod.AuthorizeNetSIM;
			listItemCollections.Add(new ListItem(str1, paymentMethod.ToString()));
			DropDownList dropDownList = this.paymentMethodDropDownList;
			paymentMethod = this.ModuleSettings.PaymentMethod;
			dropDownList.SelectedValue = paymentMethod.ToString();
			this.authorizeNetApiLoginTextBox.Text = this.ModuleSettings.AuthorizeNetApiLogin;
			this.authorizeNetTransactionKeyTextBox.Text = this.ModuleSettings.AuthorizeNetTransactionKey;
			this.authorizeNetMerchantHashTextBox.Text = this.ModuleSettings.AuthorizeNetMerchantHash;
			this.authorizeNetTestModeCheckBox.Checked = this.ModuleSettings.AuthorizeNetTestMode;
			this.BindReservationFeesSection(Null.NullInteger);
			this.PaymentMethodChanged(null, null);
		}

		protected void BindReservationFeesSection(int categoryID)
		{
			PaymentMethod paymentMethod;
			bool selectedValue;
			bool flag;
			bool flag1;
			this.reservationFeesCategoryDropDownList.SelectedValue = categoryID.ToString();
			CategorySettings categorySetting = null;
			if (categoryID != Null.NullInteger)
			{
				categorySetting = new CategorySettings(base.PortalId, base.TabModuleId, categoryID);
			}
			this.paymentMethodTableRow.Visible = categorySetting == null;
			HtmlTableRow htmlTableRow = this.payPalAccountTableRow;
			HtmlTableRow htmlTableRow1 = this.payPalUrlTableRow;
			if (categorySetting != null)
			{
				selectedValue = false;
			}
			else
			{
				paymentMethod = PaymentMethod.PayPalPaymentsStandard;
				selectedValue = this.paymentMethodDropDownList.SelectedValue == paymentMethod.ToString();
			}
			bool flag2 = selectedValue;
			htmlTableRow1.Visible = selectedValue;
			htmlTableRow.Visible = flag2;
			HtmlTableRow htmlTableRow2 = this.authorizeNetApiLoginTableRow;
			HtmlTableRow htmlTableRow3 = this.authorizeNetMerchantHashTableRow;
			HtmlTableRow htmlTableRow4 = this.authorizeNetTestModeTableRow;
			HtmlTableRow htmlTableRow5 = this.authorizeNetTransactionKeyTableRow;
			if (categorySetting != null)
			{
				flag = false;
			}
			else
			{
				paymentMethod = PaymentMethod.AuthorizeNetSIM;
				flag = this.paymentMethodDropDownList.SelectedValue == paymentMethod.ToString();
			}
			bool flag3 = flag;
			htmlTableRow5.Visible = flag;
			bool flag4 = flag3;
			bool flag5 = flag4;
			htmlTableRow4.Visible = flag4;
			bool flag6 = flag5;
			flag2 = flag6;
			htmlTableRow3.Visible = flag6;
			htmlTableRow2.Visible = flag2;
			this.itemDescriptionTableRow.Visible = categorySetting == null;
			this.pendingPaymentExpirationTableRow.Visible = categorySetting == null;
			this.currencyTableRow.Visible = categorySetting == null;
			this.allowPayLaterTableRow.Visible = categorySetting == null;
			LinkButton linkButton = this.reservationFeesResetCommandButton;
			if (categorySetting == null)
			{
				flag1 = false;
			}
			else
			{
				flag1 = (categorySetting.IsDefined("SchedulingFee") ? true : categorySetting.IsDefined("FeeScheduleType"));
			}
			linkButton.Visible = flag1;
			this.feeschedulecontrol.Currency = this.ModuleSettings.Currency;
			this.feeschedulecontrol.FeeScheduleType = (categorySetting != null ? categorySetting.FeeScheduleType : this.ModuleSettings.FeeScheduleType);
			if (this.feeschedulecontrol.FeeScheduleType == FeeScheduleType.Flat)
			{
				this.feeschedulecontrol.FlatFeeScheduleInfo = (categorySetting != null ? categorySetting.FlatFeeScheduleInfo : this.ModuleSettings.FlatFeeScheduleInfo);
			}
			else if (this.feeschedulecontrol.FeeScheduleType == FeeScheduleType.Seasonal)
			{
				this.feeschedulecontrol.SeasonalFeeScheduleList = (categorySetting != null ? categorySetting.SeasonalFeeScheduleList : this.ModuleSettings.SeasonalFeeScheduleList);
			}
			this.feeschedulecontrol.DataBind();
		}

		protected void BindReservationSettingsSection()
		{
			bool flag;
			HtmlTableRow htmlTableRow = this.reservationSettingsUpdateResetTableRow;
			HtmlTableRow htmlTableRow1 = this.reservationSettingsCategoryTableRow;
			flag = (base.IsPostBack ? this.allowCategorySelectionCheckBox.Checked : this.ModuleSettings.AllowCategorySelection);
			bool flag1 = flag;
			htmlTableRow1.Visible = flag;
			htmlTableRow.Visible = flag1;
			if (this.reservationSettingsCategoryTableRow.Visible)
			{
				this.BindCategoriesDropDownList(this.reservationSettingsCategoryDropDownList, "AllowCancellations", null, null);
			}
			if (!base.IsPostBack)
			{
				this.BindTimeSpanDropDownList(this.minTimeAheadDropDownList);
				this.BindTimeSpanDropDownList(this.reservationIntervalDropDownList);
				this.BindTimeSpanDropDownList(this.reservationDurationDropDownList);
				this.BindTimeSpanDropDownList(this.reservationDurationMaxDropDownList);
				this.BindTimeSpanDropDownList(this.reservationDurationIntervalDropDownList);
			}
			this.BindReservationSettingsSection(Null.NullInteger);
		}

		protected void BindReservationSettingsSection(int categoryID)
		{
			int daysAhead;
			string str;
			string str1;
			string empty;
			this.reservationSettingsCategoryDropDownList.SelectedValue = categoryID.ToString();
			CategorySettings categorySetting = null;
			if (categoryID != Null.NullInteger)
			{
				categorySetting = new CategorySettings(base.PortalId, base.TabModuleId, categoryID);
			}
			this.allowCancellationsCheckBox.Checked = (categorySetting != null ? categorySetting.AllowCancellations : this.ModuleSettings.AllowCancellations);
			this.allowReschedulingCheckBox.Checked = (categorySetting != null ? categorySetting.AllowRescheduling : this.ModuleSettings.AllowRescheduling);
			TextBox textBox = this.daysAheadTextBox;
			if (categorySetting != null)
			{
				str = categorySetting.DaysAhead.ToString();
			}
			else
			{
				daysAhead = this.ModuleSettings.DaysAhead;
				str = daysAhead.ToString();
			}
			textBox.Text = str;
			TextBox textBox1 = this.maxConflictingReservationsTextBox;
			if (categorySetting != null)
			{
				str1 = categorySetting.MaxConflictingReservations.ToString();
			}
			else
			{
				daysAhead = this.ModuleSettings.MaxConflictingReservations;
				str1 = daysAhead.ToString();
			}
			textBox1.Text = str1;
			TextBox textBox2 = this.maxReservationsPerUserTextBox;
			if (categorySetting != null && categorySetting.MaxReservationsPerUser != Null.NullInteger)
			{
				empty = categorySetting.MaxReservationsPerUser.ToString();
			}
			else if (this.ModuleSettings.MaxReservationsPerUser != Null.NullInteger)
			{
				daysAhead = this.ModuleSettings.MaxReservationsPerUser;
				empty = daysAhead.ToString();
			}
			else
			{
				empty = string.Empty;
			}
			textBox2.Text = empty;
			if (categorySetting == null)
			{
				this.SetTimeSpan(this.ModuleSettings.MinTimeAhead, this.minTimeAheadTextBox, this.minTimeAheadDropDownList);
				this.SetTimeSpan(this.ModuleSettings.ReservationInterval, this.reservationIntervalTextBox, this.reservationIntervalDropDownList);
				this.SetTimeSpan(this.ModuleSettings.ReservationDuration, this.reservationDurationTextBox, this.reservationDurationDropDownList);
				this.SetTimeSpan(this.ModuleSettings.ReservationDurationMax, this.reservationDurationMaxTextBox, this.reservationDurationMaxDropDownList);
				this.SetTimeSpan(this.ModuleSettings.ReservationDurationInterval, this.reservationDurationIntervalTextBox, this.reservationDurationIntervalDropDownList);
			}
			else
			{
				this.SetTimeSpan(categorySetting.MinTimeAhead, this.minTimeAheadTextBox, this.minTimeAheadDropDownList);
				this.SetTimeSpan(categorySetting.ReservationInterval, this.reservationIntervalTextBox, this.reservationIntervalDropDownList);
				this.SetTimeSpan(categorySetting.ReservationDuration, this.reservationDurationTextBox, this.reservationDurationDropDownList);
				this.SetTimeSpan(categorySetting.ReservationDurationMax, this.reservationDurationMaxTextBox, this.reservationDurationMaxDropDownList);
				this.SetTimeSpan(categorySetting.ReservationDurationInterval, this.reservationDurationIntervalTextBox, this.reservationDurationIntervalDropDownList);
			}
			this.reservationSettingsResetCommandButton.Visible = (categorySetting == null ? false : categorySetting.IsDefined("AllowCancellations"));
		}

		protected void BindRolesCheckboxList()
		{
			this.categoryPermissionsCheckboxList.Items.Clear();
			this.categoryPermissionsCheckboxList.DataSource = (new RoleController()).GetPortalRoles(base.PortalId);
			this.categoryPermissionsCheckboxList.DataTextField = "RoleName";
			this.categoryPermissionsCheckboxList.DataValueField = "RoleID";
			this.categoryPermissionsCheckboxList.DataBind();
			this.categoryPermissionsCheckboxList.Items.Insert(0, new ListItem(Localization.GetString("AllUsers", base.LocalResourceFile), "-1"));
			this.categoryPermissionsCheckboxList.Items.Add(new ListItem(Localization.GetString("UnauthenticatedUsers", base.LocalResourceFile), "-3"));
			foreach (ListItem item in this.categoryPermissionsCheckboxList.Items)
			{
				item.Selected = this.CategoryPermissionsList.IndexOf(int.Parse(item.Value)) != -1;
			}
		}

		protected void BindSMSTemplates()
		{
			this.smsTemplateDropDownList.Items.Clear();
			this.smsTemplateDropDownList.Items.Add(new ListItem(Localization.GetString("Reminder", base.LocalResourceFile), "Reminder"));
			this.smsTemplateDropDownList.SelectedIndex = 0;
			this.SMSTemplateDropDownListSelectedIndexChanged(null, null);
		}

		protected void BindSMSTemplatesSection()
		{
			this.smsTemplatesSectionTableRow.Visible = this.IsProfessional;
			this.twilioAccountSIDTextBox.Text = this.ModuleSettings.TwilioAccountSID;
			this.twilioAuthTokenTextBox.Text = this.ModuleSettings.TwilioAuthToken;
			this.twilioFromTextBox.Text = this.ModuleSettings.TwilioFrom;
			this.BindSMSTemplates();
		}

		protected void BindThemesDropDownList()
		{
			this.themeDropDownList.DataSource = (new DirectoryInfo(base.Server.MapPath(string.Concat(this.TemplateSourceDirectory, "/Themes")))).GetDirectories();
			this.themeDropDownList.DataTextField = "Name";
			this.themeDropDownList.DataValueField = "Name";
			this.themeDropDownList.DataBind();
		}

		protected void BindTimeOfDayDataGrid()
		{
			this.timeOfDayDataGrid.DataSource = this.TimeOfDayList;
			this.timeOfDayDataGrid.DataBind();
			this.noTimeOfDayLabel.Visible = this.timeOfDayDataGrid.Items.Count == 0;
		}

		protected void BindTimeOfDaySection()
		{
			if (!base.IsPostBack)
			{
				this.BindHoursDropDownList(this.timeOfDayStartHourDropDownList);
				this.BindHoursDropDownList(this.timeOfDayEndHourDropDownList);
				this.BindMinutesDropDownList(this.timeOfDayStartMinuteDropDownList);
				this.BindMinutesDropDownList(this.timeOfDayEndMinuteDropDownList);
				this.BindAMPMDropDownList(this.timeOfDayStartAMPMDropDownList);
				this.BindAMPMDropDownList(this.timeOfDayEndAMPMDropDownList);
			}
			this.displayTimeOfDayCheckBox.Checked = this.ModuleSettings.DisplayTimeOfDay;
			this.displayUnavailableTimeOfDayCheckBox.Checked = this.ModuleSettings.DisplayUnavailableTimeOfDay;
			this.BindTimeOfDayDataGrid();
			HtmlTableRow htmlTableRow = this.timeOfDaySelectionModeTableRow;
			HtmlTableRow htmlTableRow1 = this.displayUnavailableTimeOfDayTableRow;
			HtmlTableRow htmlTableRow2 = this.timeOfDayTableRow;
			bool @checked = this.displayTimeOfDayCheckBox.Checked;
			bool flag = @checked;
			htmlTableRow2.Visible = @checked;
			bool flag1 = flag;
			bool flag2 = flag1;
			htmlTableRow1.Visible = flag1;
			htmlTableRow.Visible = flag2;
		}

		protected void BindTimeSpanDropDownList(DropDownList dropDownList)
		{
			dropDownList.Items.Clear();
			dropDownList.Items.Add(new ListItem(Localization.GetString("Minutes", base.LocalResourceFile), "M"));
			dropDownList.Items.Add(new ListItem(Localization.GetString("Hours", base.LocalResourceFile), "H"));
			dropDownList.Items.Add(new ListItem(Localization.GetString("Days", base.LocalResourceFile), "D"));
		}

		protected void BindTimeZoneDropDownList()
		{
			this.timeZoneDropDownList.DataSource = TimeZoneInfo.GetSystemTimeZones();
			this.timeZoneDropDownList.DataTextField = "DisplayName";
			this.timeZoneDropDownList.DataValueField = "Id";
			this.timeZoneDropDownList.DataBind();
		}

		protected void BindUsersDropDownList(DropDownList dropDownList, ArrayList usersToExclude)
		{
			dropDownList.Items.Clear();
			foreach (UserInfo user in this.Users)
			{
				//if (this.FindUserInfoByUserId(usersToExclude, user.UserID) != null)
				if (this.FindUserInfoByEmail(usersToExclude, user.Email) != null)
				{
					continue;
				}
				ListItemCollection items = dropDownList.Items;
				//string displayName = user.DisplayName;
				//int userID = user.UserID;
				//items.Add(new ListItem(displayName, userID.ToString()));
				items.Add(new ListItem(user.DisplayName, user.Email));
			}
			dropDownList.Items.Insert(0, new ListItem(Localization.GetString("NoneSpecified", base.LocalResourceFile), "-1"));
		}

		protected void BindViewReservationsDataGrid()
		{
			this.viewReservationsDataGrid.DataSource = this.ViewReservationsList;
			this.viewReservationsDataGrid.DataBind();
			this.noViewReservationsLabel.Visible = this.viewReservationsDataGrid.Items.Count == 0;
		}

		protected void BindViewReservationsSection()
		{
			this.ViewState.Remove("ViewReservationsList");
			this.BindViewReservationsDataGrid();
			/*if (this.Users.Count <= 100)
			{
				this.BindUsersDropDownList(this.viewReservationsUsersDropDownList, this.ViewReservationsList);
				return;
			}*/
			RequiredFieldValidator requiredFieldValidator = this.viewReservationsUsernameRequiredFieldValidator;
			TextBox textBox = this.viewReservationsUsernameTextBox;
			bool flag = false;
			//this.viewReservationsUsersDropDownList.Visible = false;
			bool flag1 = !flag;
			bool flag2 = flag1;
			textBox.Visible = flag1;
			requiredFieldValidator.Visible = flag2;
		}

		protected void BindWeekDaysDropDownList(DropDownList weekDaysDropDownList)
		{
			weekDaysDropDownList.Items.Clear();
			int num = 1;
			weekDaysDropDownList.Items.Add(new ListItem(CultureInfo.CurrentCulture.DateTimeFormat.DayNames[1], num.ToString()));
			num = 2;
			weekDaysDropDownList.Items.Add(new ListItem(CultureInfo.CurrentCulture.DateTimeFormat.DayNames[2], num.ToString()));
			num = 3;
			weekDaysDropDownList.Items.Add(new ListItem(CultureInfo.CurrentCulture.DateTimeFormat.DayNames[3], num.ToString()));
			num = 4;
			weekDaysDropDownList.Items.Add(new ListItem(CultureInfo.CurrentCulture.DateTimeFormat.DayNames[4], num.ToString()));
			num = 5;
			weekDaysDropDownList.Items.Add(new ListItem(CultureInfo.CurrentCulture.DateTimeFormat.DayNames[5], num.ToString()));
			num = 6;
			weekDaysDropDownList.Items.Add(new ListItem(CultureInfo.CurrentCulture.DateTimeFormat.DayNames[6], num.ToString()));
			num = 0;
			weekDaysDropDownList.Items.Add(new ListItem(CultureInfo.CurrentCulture.DateTimeFormat.DayNames[0], num.ToString()));
		}

		protected void BindWorkingHoursDataGrid()
		{
			this.workingHoursDataGrid.DataSource = this.WorkingHours;
			this.workingHoursDataGrid.DataBind();
			this.noWorkingHoursLabel.Visible = this.workingHoursDataGrid.Items.Count == 0;
		}

		protected void BindWorkingHoursExceptionsDataGrid()
		{
			this.WorkingHoursExceptions.Sort(new WorkingHoursExceptionInfoComparer());
			this.workingHoursExceptionsWorkingHoursDataGrid.DataSource = this.WorkingHoursExceptions;
			this.workingHoursExceptionsWorkingHoursDataGrid.DataBind();
			this.workingHoursExceptionsNoWorkingHoursLabel.Visible = this.workingHoursExceptionsWorkingHoursDataGrid.Items.Count == 0;
		}

		protected void BindWorkingHoursExceptionsSection()
		{
			bool flag;
			HtmlTableRow htmlTableRow = this.workingHoursExceptionsUpdateResetTableRow;
			HtmlTableRow htmlTableRow1 = this.workingHoursExceptionsCategoryTableRow;
			flag = (base.IsPostBack ? this.allowCategorySelectionCheckBox.Checked : this.ModuleSettings.AllowCategorySelection);
			bool flag1 = flag;
			htmlTableRow1.Visible = flag;
			htmlTableRow.Visible = flag1;
			if (this.workingHoursExceptionsCategoryTableRow.Visible)
			{
				this.BindCategoriesDropDownList(this.workingHoursExceptionsCategoryDropDownList, "WorkingHoursExceptionsDefined", null, null);
			}
			if (!base.IsPostBack)
			{
				this.BindHoursDropDownList(this.workingHoursExceptionStartHourDropDownList);
				this.BindHoursDropDownList(this.workingHoursExceptionEndHourDropDownList);
				this.BindMinutesDropDownList(this.workingHoursExceptionStartMinuteDropDownList);
				this.BindMinutesDropDownList(this.workingHoursExceptionEndMinuteDropDownList);
				this.BindAMPMDropDownList(this.workingHoursExceptionStartAMPMDropDownList);
				this.BindAMPMDropDownList(this.workingHoursExceptionEndAMPMDropDownList);
				this.workingHoursExceptionStartHourDropDownList.SelectedValue = "8";
				if (!this.Is24HourClock)
				{
					this.workingHoursExceptionEndHourDropDownList.SelectedValue = "5";
					this.workingHoursExceptionEndAMPMDropDownList.SelectedValue = "PM";
				}
				else
				{
					this.workingHoursExceptionEndHourDropDownList.SelectedValue = "17";
				}
				this.workingHoursExceptionDateImage.Attributes.Add("onclick", DotNetNuke.Common.Utilities.Calendar.InvokePopupCal(this.workingHoursExceptionDateTextBox));
			}
			this.BindWorkingHoursExceptionsSection(Null.NullInteger);
		}

		protected void BindWorkingHoursExceptionsSection(int categoryID)
		{
			this.workingHoursExceptionsCategoryDropDownList.SelectedValue = categoryID.ToString();
			CategorySettings categorySetting = null;
			if (categoryID != Null.NullInteger)
			{
				categorySetting = new CategorySettings(base.PortalId, base.TabModuleId, categoryID);
			}
			this.ViewState.Remove("WorkingHoursExceptions");
			this.BindWorkingHoursExceptionsDataGrid();
			this.workingHoursExceptionsResetCommandButton.Visible = (categorySetting == null ? false : categorySetting.WorkingHoursExceptionsDefined);
		}

		protected void BindWorkingHoursSection()
		{
			bool flag;
			HtmlTableRow htmlTableRow = this.workingHoursUpdateResetTableRow;
			HtmlTableRow htmlTableRow1 = this.workingHoursCategoryTableRow;
			flag = (base.IsPostBack ? this.allowCategorySelectionCheckBox.Checked : this.ModuleSettings.AllowCategorySelection);
			bool flag1 = flag;
			htmlTableRow1.Visible = flag;
			htmlTableRow.Visible = flag1;
			if (this.workingHoursCategoryTableRow.Visible)
			{
				this.BindCategoriesDropDownList(this.workingHoursCategoryDropDownList, "WorkingHours.1", null, null);
			}
			if (!base.IsPostBack)
			{
				this.recurrencepatterncontrol.SubmitText = Localization.GetString("addCommandButton", base.LocalResourceFile);
			}
			this.BindWorkingHoursSection(Null.NullInteger);
		}

		protected void BindWorkingHoursSection(int categoryID)
		{
			this.workingHoursCategoryDropDownList.SelectedValue = categoryID.ToString();
			CategorySettings categorySetting = null;
			if (categoryID != Null.NullInteger)
			{
				categorySetting = new CategorySettings(base.PortalId, base.TabModuleId, categoryID);
			}
			this.ViewState.Remove("WorkingHours");
			this._WorkingHours = null;
			this.BindWorkingHoursDataGrid();
			this.workingHoursResetCommandButton.Visible = (categorySetting == null ? false : categorySetting.IsDefined("WorkingHours.1"));
		}

		protected void CancelSettingsCommandButtonClicked(object sender, EventArgs e)
		{
			base.Response.Redirect(Globals.NavigateURL());
		}

		protected void CashierListCategoryDropDownListSelectedIndexChanged(object sender, EventArgs e)
		{
			this.BindCashierListSection(int.Parse(this.cashierListCategoryDropDownList.SelectedValue));
		}

		protected void CashierListResetCommandButtonClicked(object sender, EventArgs e)
		{
			int num = int.Parse(this.cashierListCategoryDropDownList.SelectedValue);
			(new CategorySettingController()).DeleteCategorySetting(num, "CashierList");
			this.BindCategoriesDropDownList(this.cashierListCategoryDropDownList, "CashierList", null, null);
			this.BindCashierListSection(num);
		}

		protected void CashierListUpdateCommandButtonClicked(object sender, EventArgs e)
		{
			this.UpdateCashierListSection(true);
		}

		protected void CategoryPermissionsDropDownListSelectedIndexChanged(object sender, EventArgs e)
		{
			this.BindCategoryPermissionsSection(int.Parse(this.categoryPermissionsDropDownList.SelectedValue));
		}

		protected void CategoryPermissionsResetCommandButtonClicked(object sender, EventArgs e)
		{
			int num = int.Parse(this.categoryPermissionsDropDownList.SelectedValue);
			(new CategorySettingController()).DeleteCategorySetting(num, "CategoryPermissions");
			this.BindCategoriesDropDownList(this.categoryPermissionsDropDownList, "CategoryPermissions", null, null);
			this.BindCategoryPermissionsSection(num);
		}

		protected void CategoryPermissionsUpdateCommandButtonClicked(object sender, EventArgs e)
		{
			this.UpdateCategoryPermissionsSection(true);
		}

		protected void DeleteCashier(object sender, DataGridCommandEventArgs e)
		{
			//this.CashierList.RemoveAt(this.FindUserInfoIndexByUserId(this.CashierList, int.Parse(((Label)e.Item.FindControl("userId")).Text)));
			this.CashierList.RemoveAt(this.FindUserInfoIndexByEmail(this.CashierList, ((Label)e.Item.FindControl("email")).Text));
			this.BindCashierListDataGrid();
			/*if (this.cashierListUsersDropDownList.Visible)
			{
				this.BindUsersDropDownList(this.cashierListUsersDropDownList, this.CashierList);
			}*/
		}

		protected void DeleteCategory(object sender, DataGridCommandEventArgs e)
		{
			int num = int.Parse(((Label)e.Item.FindControl("categoryID")).Text);
			(new CategoryController()).DeleteCategory(num);
			this.CategoryList.RemoveAt(this.CategoryList.FindIndex((CategoryInfo x) => x.CategoryID == num));
			this.BindCategoryListDataGrid();
			this.RebindCategoryDependentSections();
		}

		protected void DeleteDuplicateReservations(object sender, DataGridCommandEventArgs e)
		{
			//this.DuplicateReservationsList.RemoveAt(this.FindUserInfoIndexByUserId(this.DuplicateReservationsList, int.Parse(((Label)e.Item.FindControl("userId")).Text)));
			this.DuplicateReservationsList.RemoveAt(this.FindUserInfoIndexByEmail(this.DuplicateReservationsList, ((Label)e.Item.FindControl("email")).Text));
			this.BindDuplicateReservationsDataGrid();
			/*if (this.duplicateReservationsUsersDropDownList.Visible)
			{
				this.BindUsersDropDownList(this.duplicateReservationsUsersDropDownList, this.DuplicateReservationsList);
			}*/
		}

		protected void DeleteGlobalModerator(object sender, DataGridCommandEventArgs e)
		{
			//this.ModeratorList.RemoveAt(this.FindUserInfoIndexByUserId(this.ModeratorList, int.Parse(((Label)e.Item.FindControl("userId")).Text)));
			this.ModeratorList.RemoveAt(this.FindUserInfoIndexByEmail(this.ModeratorList, ((Label)e.Item.FindControl("email")).Text));
			this.BindModeratorsDataGrid();
			/*if (this.moderatorUsersDropDownList.Visible)
			{
				this.BindUsersDropDownList(this.moderatorUsersDropDownList, this.ModeratorList);
			}*/
		}

		protected void DeleteModerationHours(object sender, DataGridCommandEventArgs e)
		{
			try
			{
				DayOfWeek dayOfWeek = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), ((Label)e.Item.FindControl("dayOfWeekLabel")).Text);
				TimeSpan timeSpan = TimeSpan.Parse(((Label)e.Item.FindControl("startTimeLabel")).Text);
				TimeSpan timeSpan1 = TimeSpan.Parse(((Label)e.Item.FindControl("endTimeLabel")).Text);
				bool flag = bool.Parse(((Label)e.Item.FindControl("allDayLabel")).Text);
				int num = 0;
				foreach (WorkingHoursInfo moderationHour in this.ModerationHours)
				{
					if (moderationHour.DayOfWeek == dayOfWeek && (moderationHour.StartTime == timeSpan && moderationHour.EndTime == timeSpan1 || moderationHour.AllDay == flag))
					{
						break;
					}
					num++;
				}
				if (num < this.ModerationHours.Count)
				{
					this.ModerationHours.RemoveAt(num);
					this.BindModerationHoursDataGrid();
				}
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
			}
		}

		protected void DeleteTimeOfDay(object sender, DataGridCommandEventArgs e)
		{
			try
			{
				string text = ((Label)e.Item.FindControl("timeOfDayNameLabel")).Text;
				int num = 0;
				IEnumerator enumerator = this.TimeOfDayList.GetEnumerator();
				try
				{
					while (enumerator.MoveNext() && !(((TimeOfDayInfo)enumerator.Current).Name == text))
					{
						num++;
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
				if (num < this.TimeOfDayList.Count)
				{
					this.TimeOfDayList.RemoveAt(num);
					this.BindTimeOfDayDataGrid();
				}
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
			}
		}

		protected void DeleteUser(object sender, DataGridCommandEventArgs e)
		{
			//this.BCCList.RemoveAt(this.FindUserInfoIndexByUserId(this.BCCList, int.Parse(((Label)e.Item.FindControl("userId")).Text)));
			this.BCCList.RemoveAt(this.FindUserInfoIndexByEmail(this.BCCList, ((Label)e.Item.FindControl("email")).Text));
			this.BindBCCListDataGrid();
			/*if (this.usersDropDownList.Visible)
			{
				this.BindUsersDropDownList(this.usersDropDownList, this.BCCList);
			}*/
		}

		protected void DeleteViewReservations(object sender, DataGridCommandEventArgs e)
		{
			//this.ViewReservationsList.RemoveAt(this.FindUserInfoIndexByUserId(this.ViewReservationsList, int.Parse(((Label)e.Item.FindControl("userId")).Text)));
			this.ViewReservationsList.RemoveAt(this.FindUserInfoIndexByEmail(this.ViewReservationsList, ((Label)e.Item.FindControl("email")).Text));
			this.BindViewReservationsDataGrid();
			/*if (this.viewReservationsUsersDropDownList.Visible)
			{
				this.BindUsersDropDownList(this.viewReservationsUsersDropDownList, this.ViewReservationsList);
			}*/
		}

		protected void DeleteWorkingHours(object sender, DataGridCommandEventArgs e)
		{
			try
			{
				this.WorkingHours.RemoveAt(e.Item.ItemIndex);
				this.BindWorkingHoursDataGrid();
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
			}
		}

		protected void DeleteWorkingHoursException(object sender, DataGridCommandEventArgs e)
		{
			try
			{
				DateTime dateTime = DateTime.Parse(((Label)e.Item.FindControl("dateLabel")).Text);
				TimeSpan timeSpan = TimeSpan.Parse(((Label)e.Item.FindControl("startTimeLabel")).Text);
				TimeSpan timeSpan1 = TimeSpan.Parse(((Label)e.Item.FindControl("endTimeLabel")).Text);
				bool flag = bool.Parse(((Label)e.Item.FindControl("allDayLabel")).Text);
				int num = 0;
				foreach (WorkingHoursExceptionInfo workingHoursException in this.WorkingHoursExceptions)
				{
					if (workingHoursException.Date == dateTime && (workingHoursException.StartTime == timeSpan && workingHoursException.EndTime == timeSpan1 || workingHoursException.AllDay & flag))
					{
						break;
					}
					num++;
				}
				if (num < this.WorkingHoursExceptions.Count)
				{
					this.WorkingHoursExceptions.RemoveAt(num);
					this.BindWorkingHoursExceptionsDataGrid();
				}
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
			}
		}

		public List<CategoryInfo> DeserializeCategoryList(string serializedCategoryList)
		{
			List<CategoryInfo> categoryInfos;
			StringReader stringReader = null;
			try
			{
				stringReader = new StringReader(serializedCategoryList);
				categoryInfos = new List<CategoryInfo>((CategoryInfo[])(new XmlSerializer(typeof(CategoryInfo[]))).Deserialize(new XmlTextReader(stringReader)));
			}
			finally
			{
				if (stringReader != null)
				{
					stringReader.Close();
				}
			}
			return categoryInfos;
		}

		public List<RecurrencePattern> DeserializeRecurrencePatternList(string serializedWorkingHours)
		{
			List<RecurrencePattern> recurrencePatterns;
			StringReader stringReader = null;
			try
			{
				stringReader = new StringReader(serializedWorkingHours);
				recurrencePatterns = (List<RecurrencePattern>)(new XmlSerializer(typeof(List<RecurrencePattern>))).Deserialize(new XmlTextReader(stringReader));
			}
			finally
			{
				if (stringReader != null)
				{
					stringReader.Close();
				}
			}
			return recurrencePatterns;
		}

		public ArrayList DeserializeWorkingHours(string serializedWorkingHours)
		{
			ArrayList arrayLists;
			StringReader stringReader = null;
			try
			{
				stringReader = new StringReader(serializedWorkingHours);
				arrayLists = new ArrayList((WorkingHoursInfo[])(new XmlSerializer(typeof(WorkingHoursInfo[]))).Deserialize(new XmlTextReader(stringReader)));
			}
			finally
			{
				if (stringReader != null)
				{
					stringReader.Close();
				}
			}
			return arrayLists;
		}

		public ArrayList DeserializeWorkingHoursExceptions(string serializedWorkingHoursExceptions)
		{
			ArrayList arrayLists;
			StringReader stringReader = null;
			try
			{
				stringReader = new StringReader(serializedWorkingHoursExceptions);
				arrayLists = new ArrayList((WorkingHoursExceptionInfo[])(new XmlSerializer(typeof(WorkingHoursExceptionInfo[]))).Deserialize(new XmlTextReader(stringReader)));
			}
			finally
			{
				if (stringReader != null)
				{
					stringReader.Close();
				}
			}
			return arrayLists;
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
			return userInfo;
		}

		private UserInfo FindUserInfoByEmail(ArrayList users, string email)
		{
			IEnumerator enumerator = users.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					UserInfo userInfo = (UserInfo)enumerator.Current;
					if (userInfo.Email.Equals(email))
					{
						return userInfo;
					}
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

		private int FindUserInfoIndexByUserId(ArrayList users, int userId)
		{
			int num = 0;
			IEnumerator enumerator = users.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					if (((UserInfo)enumerator.Current).UserID != userId)
					{
						num++;
					}
					else
					{
						return num;
					}
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
			return -1;
		}

		private int FindUserInfoIndexByEmail(ArrayList users, string email)
		{
			int num = 0;
			IEnumerator enumerator = users.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					if (((UserInfo)enumerator.Current).Email.Equals(email))
					{
						return num;
					}
					else
					{
						num++;
					}
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
			return -1;
		}

		protected TimeSpan GetTime(DropDownList hourDropDownList, DropDownList minuteDropDownList, DropDownList amPmDropDownList)
		{
			int num = 0;
			int num1 = 0;
			num = int.Parse(hourDropDownList.SelectedValue);
			if (!this.Is24HourClock && amPmDropDownList.SelectedValue == "PM")
			{
				num = num + 12;
			}
			num1 = int.Parse(minuteDropDownList.SelectedValue);
			return new TimeSpan(num, num1, 0);
		}

		protected string GetTimeOfDay(TimeOfDayInfo timeOfDayInfo)
		{
			DateTime dateTime = new DateTime();
			dateTime = dateTime;
			dateTime = dateTime.Add(timeOfDayInfo.StartTime);
			string shortTimeString = dateTime.ToShortTimeString();
			dateTime = new DateTime();
			dateTime = dateTime;
			dateTime = dateTime.Add(timeOfDayInfo.EndTime);
			return string.Concat(shortTimeString, " - ", dateTime.ToShortTimeString());
		}

		protected TimeSpan GetTimeSpan(TextBox textBox, DropDownList dropDownList)
		{
			return new TimeSpan((dropDownList.SelectedValue == "D" ? int.Parse(textBox.Text) : 0), (dropDownList.SelectedValue == "H" ? int.Parse(textBox.Text) : 0), (dropDownList.SelectedValue == "M" ? int.Parse(textBox.Text) : 0), 0);
		}

		protected string GetWorkingHours(object recurrencePattern)
		{
			return Helper.GetRecurrencePatternText((RecurrencePattern)recurrencePattern);
		}

		protected string GetWorkingHours(WorkingHoursInfo workingHoursInfo)
		{
			string str;
			string dayNames = CultureInfo.CurrentCulture.DateTimeFormat.DayNames[(int)workingHoursInfo.DayOfWeek];
			if (workingHoursInfo.AllDay)
			{
				str = Localization.GetString("AllDay", base.LocalResourceFile);
			}
			else
			{
				string[] shortTimeString = new string[] { Localization.GetString("fromLabel", base.LocalResourceFile), " ", null, null, null, null, null };
				DateTime dateTime = new DateTime();
				dateTime = dateTime;
				dateTime = dateTime.Add(workingHoursInfo.StartTime);
				shortTimeString[2] = dateTime.ToShortTimeString();
				shortTimeString[3] = " ";
				shortTimeString[4] = Localization.GetString("toLabel", base.LocalResourceFile);
				shortTimeString[5] = " ";
				dateTime = new DateTime();
				dateTime = dateTime;
				dateTime = dateTime.Add(workingHoursInfo.EndTime);
				shortTimeString[6] = dateTime.ToShortTimeString();
				str = string.Concat(shortTimeString);
			}
			return string.Concat(dayNames, " - ", str);
		}

		protected string GetWorkingHoursException(WorkingHoursExceptionInfo workingHoursExceptionInfo)
		{
			string str;
			DateTime date = workingHoursExceptionInfo.Date;
			string longDateString = date.ToLongDateString();
			if (workingHoursExceptionInfo.AllDay)
			{
				str = Localization.GetString("AllDay", base.LocalResourceFile);
			}
			else if (workingHoursExceptionInfo.StartTime == workingHoursExceptionInfo.EndTime)
			{
				str = Localization.GetString("noWorkingHoursLabel", base.LocalResourceFile);
			}
			else
			{
				string[] shortTimeString = new string[] { Localization.GetString("fromLabel", base.LocalResourceFile), " ", null, null, null, null, null };
				date = new DateTime();
				date = date;
				date = date.Add(workingHoursExceptionInfo.StartTime);
				shortTimeString[2] = date.ToShortTimeString();
				shortTimeString[3] = " ";
				shortTimeString[4] = Localization.GetString("toLabel", base.LocalResourceFile);
				shortTimeString[5] = " ";
				date = new DateTime();
				date = date;
				date = date.Add(workingHoursExceptionInfo.EndTime);
				shortTimeString[6] = date.ToShortTimeString();
				str = string.Concat(shortTimeString);
			}
			return string.Concat(longDateString, " - ", str);
		}

		protected ArrayList GetWorkingHoursExceptions(Hashtable settings)
		{
			DateTime dateTime;
			ArrayList arrayLists = new ArrayList();
			foreach (string key in settings.Keys)
			{
				if (!DateTime.TryParse(key, CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.None, out dateTime))
				{
					continue;
				}
				string[] strArrays = ((string)settings[key]).Split(new char[] { ';' });
				for (int i = 0; i < (int)strArrays.Length; i++)
				{
					string str = strArrays[i];
					WorkingHoursExceptionInfo workingHoursExceptionInfo = new WorkingHoursExceptionInfo()
					{
						Date = dateTime
					};
					if (str.ToLower() == "all day")
					{
						workingHoursExceptionInfo.AllDay = true;
					}
					else if (str != string.Empty && str.ToLower() != "none")
					{
						workingHoursExceptionInfo.StartTime = TimeSpan.Parse(str.Split(new char[] { '-' })[0]);
						workingHoursExceptionInfo.EndTime = TimeSpan.Parse(str.Split(new char[] { '-' })[1]);
					}
					arrayLists.Add(workingHoursExceptionInfo);
				}
			}
			return arrayLists;
		}

		private void HighlightInvalidControl(WebControl control)
		{
			if (!control.CssClass.Contains(" Gafware_Modules_Reservations_Invalid"))
			{
				WebControl webControl = control;
				webControl.CssClass = string.Concat(webControl.CssClass, " Gafware_Modules_Reservations_Invalid");
			}
		}

		public override void LoadSettings()
		{
			try
			{
				if (!this.Page.IsPostBack)
				{
					this.BindGeneralSettingsSection();
					this.BindCategoriesSection();
					this.BindCategoryPermissionsSection();
					this.BindReservationSettingsSection();
					if (!this.IsProfessional)
					{
						this.feesSectionTableRow.Visible = false;
						this.cashierListSectionTableRow.Visible = false;
					}
					else
					{
						this.BindReservationFeesSection();
						this.BindCashierListSection();
					}
					this.BindWorkingHoursSection();
					this.BindWorkingHoursExceptionsSection();
					this.BindTimeOfDaySection();
					this.BindBCCListSection();
					this.BindModerationSection();
					this.BindViewReservationsSection();
					this.BindDuplicateReservationsSection();
					this.BindRemindersSection();
					this.BindMailTemplatesSection();
					this.BindSMSTemplatesSection();
				}
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
			}
		}

		protected void MailTemplateDropDownListSelectedIndexChanged(object sender, EventArgs e)
		{
			bool flag;
			if (this.mailTemplateDropDownList.SelectedValue == "Confirmation")
			{
				this.mailTemplateSubjectTextBox.Text = this.ModuleSettings.ConfirmationMailSubject;
				this.mailTemplateBodyTextBox.Text = this.ModuleSettings.ConfirmationMailBody;
				RadioButton radioButton = this.mailTemplateBodyTypeHtmlRadioButton;
				RadioButton radioButton1 = this.mailTemplateBodyTypeTextRadioButton;
				bool lower = this.ModuleSettings.ConfirmationMailBodyType.ToLower() == "text";
				flag = lower;
				radioButton1.Checked = lower;
				radioButton.Checked = !flag;
				return;
			}
			if (this.mailTemplateDropDownList.SelectedValue == "Modification")
			{
				this.mailTemplateSubjectTextBox.Text = this.ModuleSettings.ModificationMailSubject;
				this.mailTemplateBodyTextBox.Text = this.ModuleSettings.ModificationMailBody;
				RadioButton radioButton2 = this.mailTemplateBodyTypeHtmlRadioButton;
				RadioButton radioButton3 = this.mailTemplateBodyTypeTextRadioButton;
				bool lower1 = this.ModuleSettings.ModificationMailBodyType.ToLower() == "text";
				flag = lower1;
				radioButton3.Checked = lower1;
				radioButton2.Checked = !flag;
				return;
			}
			if (this.mailTemplateDropDownList.SelectedValue == "Rescheduled")
			{
				this.mailTemplateSubjectTextBox.Text = this.ModuleSettings.RescheduledMailSubject;
				this.mailTemplateBodyTextBox.Text = this.ModuleSettings.RescheduledMailBody;
				RadioButton radioButton4 = this.mailTemplateBodyTypeHtmlRadioButton;
				RadioButton radioButton5 = this.mailTemplateBodyTypeTextRadioButton;
				bool flag1 = this.ModuleSettings.RescheduledMailBodyType.ToLower() == "text";
				flag = flag1;
				radioButton5.Checked = flag1;
				radioButton4.Checked = !flag;
				return;
			}
			if (this.mailTemplateDropDownList.SelectedValue == "Cancellation")
			{
				this.mailTemplateSubjectTextBox.Text = this.ModuleSettings.CancellationMailSubject;
				this.mailTemplateBodyTextBox.Text = this.ModuleSettings.CancellationMailBody;
				RadioButton radioButton6 = this.mailTemplateBodyTypeHtmlRadioButton;
				RadioButton radioButton7 = this.mailTemplateBodyTypeTextRadioButton;
				bool lower2 = this.ModuleSettings.CancellationMailBodyType.ToLower() == "text";
				flag = lower2;
				radioButton7.Checked = lower2;
				radioButton6.Checked = !flag;
				return;
			}
			if (this.mailTemplateDropDownList.SelectedValue == "Moderator")
			{
				this.mailTemplateSubjectTextBox.Text = this.ModuleSettings.ModeratorMailSubject;
				this.mailTemplateBodyTextBox.Text = this.ModuleSettings.ModeratorMailBody;
				RadioButton radioButton8 = this.mailTemplateBodyTypeHtmlRadioButton;
				RadioButton radioButton9 = this.mailTemplateBodyTypeTextRadioButton;
				bool flag2 = this.ModuleSettings.ModeratorMailBodyType.ToLower() == "text";
				flag = flag2;
				radioButton9.Checked = flag2;
				radioButton8.Checked = !flag;
				return;
			}
			if (this.mailTemplateDropDownList.SelectedValue == "Declined")
			{
				this.mailTemplateSubjectTextBox.Text = this.ModuleSettings.DeclinedMailSubject;
				this.mailTemplateBodyTextBox.Text = this.ModuleSettings.DeclinedMailBody;
				RadioButton radioButton10 = this.mailTemplateBodyTypeHtmlRadioButton;
				RadioButton radioButton11 = this.mailTemplateBodyTypeTextRadioButton;
				bool lower3 = this.ModuleSettings.DeclinedMailBodyType.ToLower() == "text";
				flag = lower3;
				radioButton11.Checked = lower3;
				radioButton10.Checked = !flag;
				return;
			}
			if (this.mailTemplateDropDownList.SelectedValue == "VerificationCode")
			{
				this.mailTemplateSubjectTextBox.Text = this.ModuleSettings.VerificationCodeMailSubject;
				this.mailTemplateBodyTextBox.Text = this.ModuleSettings.VerificationCodeMailBody;
				RadioButton radioButton12 = this.mailTemplateBodyTypeHtmlRadioButton;
				RadioButton radioButton13 = this.mailTemplateBodyTypeTextRadioButton;
				bool flag3 = this.ModuleSettings.VerificationCodeMailBodyType.ToLower() == "text";
				flag = flag3;
				radioButton13.Checked = flag3;
				radioButton12.Checked = !flag;
				return;
			}
			if (this.mailTemplateDropDownList.SelectedValue == "DuplicateReservation")
			{
				this.mailTemplateSubjectTextBox.Text = this.ModuleSettings.DuplicateReservationMailSubject;
				this.mailTemplateBodyTextBox.Text = this.ModuleSettings.DuplicateReservationMailBody;
				RadioButton radioButton14 = this.mailTemplateBodyTypeHtmlRadioButton;
				RadioButton radioButton15 = this.mailTemplateBodyTypeTextRadioButton;
				bool lower4 = this.ModuleSettings.DuplicateReservationMailBodyType.ToLower() == "text";
				flag = lower4;
				radioButton15.Checked = lower4;
				radioButton14.Checked = !flag;
				return;
			}
			if (this.mailTemplateDropDownList.SelectedValue == "Reminder")
			{
				this.mailTemplateSubjectTextBox.Text = this.ModuleSettings.ReminderMailSubject;
				this.mailTemplateBodyTextBox.Text = this.ModuleSettings.ReminderMailBody;
				RadioButton radioButton16 = this.mailTemplateBodyTypeHtmlRadioButton;
				RadioButton radioButton17 = this.mailTemplateBodyTypeTextRadioButton;
				bool flag4 = this.ModuleSettings.ReminderMailBodyType.ToLower() == "text";
				flag = flag4;
				radioButton17.Checked = flag4;
				radioButton16.Checked = !flag;
				return;
			}
			if (this.IsProfessional)
			{
				if (this.mailTemplateDropDownList.SelectedValue == "PendingRescheduleRefund")
				{
					this.mailTemplateSubjectTextBox.Text = this.ModuleSettings.PendingRescheduleRefundMailSubject;
					this.mailTemplateBodyTextBox.Text = this.ModuleSettings.PendingRescheduleRefundMailBody;
					RadioButton radioButton18 = this.mailTemplateBodyTypeHtmlRadioButton;
					RadioButton radioButton19 = this.mailTemplateBodyTypeTextRadioButton;
					bool lower5 = this.ModuleSettings.PendingRescheduleRefundMailBodyType.ToLower() == "text";
					flag = lower5;
					radioButton19.Checked = lower5;
					radioButton18.Checked = !flag;
					return;
				}
				if (this.mailTemplateDropDownList.SelectedValue == "PendingCancellationRefund")
				{
					this.mailTemplateSubjectTextBox.Text = this.ModuleSettings.PendingCancellationRefundMailSubject;
					this.mailTemplateBodyTextBox.Text = this.ModuleSettings.PendingCancellationRefundMailBody;
					RadioButton radioButton20 = this.mailTemplateBodyTypeHtmlRadioButton;
					RadioButton radioButton21 = this.mailTemplateBodyTypeTextRadioButton;
					bool flag5 = this.ModuleSettings.PendingCancellationRefundMailBodyType.ToLower() == "text";
					flag = flag5;
					radioButton21.Checked = flag5;
					radioButton20.Checked = !flag;
					return;
				}
				if (this.mailTemplateDropDownList.SelectedValue == "PendingDeclinationRefund")
				{
					this.mailTemplateSubjectTextBox.Text = this.ModuleSettings.PendingDeclinationRefundMailSubject;
					this.mailTemplateBodyTextBox.Text = this.ModuleSettings.PendingDeclinationRefundMailBody;
					RadioButton radioButton22 = this.mailTemplateBodyTypeHtmlRadioButton;
					RadioButton radioButton23 = this.mailTemplateBodyTypeTextRadioButton;
					bool lower6 = this.ModuleSettings.PendingDeclinationRefundMailBodyType.ToLower() == "text";
					flag = lower6;
					radioButton23.Checked = lower6;
					radioButton22.Checked = !flag;
				}
			}
		}

		protected void ModerateCheckBoxCheckChanged(object sender, EventArgs e)
		{
			HtmlTableRow htmlTableRow = this.moderationHoursTableRow;
			HtmlTableRow htmlTableRow1 = this.globalModeratorsDropDownListTableRow;
			HtmlTableRow htmlTableRow2 = this.globalModeratorsDataGridTableRow;
			CheckBox checkBox = this.moderateCheckBox;
			bool @checked = this.moderateCheckBox.Checked;
			bool flag = @checked;
			checkBox.Checked = @checked;
			bool flag1 = flag;
			bool flag2 = flag1;
			htmlTableRow2.Visible = flag1;
			bool flag3 = flag2;
			bool flag4 = flag3;
			htmlTableRow1.Visible = flag3;
			htmlTableRow.Visible = flag4;
		}

		protected void ModerationCategoryDropDownListSelectedIndexChanged(object sender, EventArgs e)
		{
			this.BindModerationSection(int.Parse(this.moderationCategoryDropDownList.SelectedValue));
		}

		protected void ModerationResetCommandButtonClicked(object sender, EventArgs e)
		{
			int num = int.Parse(this.moderationCategoryDropDownList.SelectedValue);
			CategorySettingController categorySettingController = new CategorySettingController();
			categorySettingController.DeleteCategorySetting(num, "Moderate");
			categorySettingController.DeleteCategorySetting(num, "GlobalModeratorList");
			DayOfWeek dayOfWeek = DayOfWeek.Monday;
			categorySettingController.DeleteCategorySetting(num, string.Concat("Moderation.", dayOfWeek.ToString()));
			dayOfWeek = DayOfWeek.Tuesday;
			categorySettingController.DeleteCategorySetting(num, string.Concat("Moderation.", dayOfWeek.ToString()));
			dayOfWeek = DayOfWeek.Wednesday;
			categorySettingController.DeleteCategorySetting(num, string.Concat("Moderation.", dayOfWeek.ToString()));
			dayOfWeek = DayOfWeek.Thursday;
			categorySettingController.DeleteCategorySetting(num, string.Concat("Moderation.", dayOfWeek.ToString()));
			dayOfWeek = DayOfWeek.Friday;
			categorySettingController.DeleteCategorySetting(num, string.Concat("Moderation.", dayOfWeek.ToString()));
			dayOfWeek = DayOfWeek.Saturday;
			categorySettingController.DeleteCategorySetting(num, string.Concat("Moderation.", dayOfWeek.ToString()));
			dayOfWeek = DayOfWeek.Sunday;
			categorySettingController.DeleteCategorySetting(num, string.Concat("Moderation.", dayOfWeek.ToString()));
			this.BindCategoriesDropDownList(this.moderationCategoryDropDownList, "Moderate", null, null);
			this.BindModerationSection(num);
		}

		protected void ModerationUpdateCommandButtonClicked(object sender, EventArgs e)
		{
			this.UpdateModerationSection(true);
		}

		protected void Page_Init(object sender, EventArgs e)
		{
			try
			{
				this.SetTheme();
				this.categoryPermissionsUpdateCommandButton.Click += new EventHandler(this.CategoryPermissionsUpdateCommandButtonClicked);
				this.categoryPermissionsResetCommandButton.Click += new EventHandler(this.CategoryPermissionsResetCommandButtonClicked);
				this.reservationSettingsUpdateCommandButton.Click += new EventHandler(this.ReservationSettingsUpdateCommandButtonClicked);
				this.reservationSettingsResetCommandButton.Click += new EventHandler(this.ReservationSettingsResetCommandButtonClicked);
				if (this.IsProfessional)
				{
					this.reservationFeesUpdateCommandButton.Click += new EventHandler(this.ReservationFeesUpdateCommandButtonClicked);
					this.reservationFeesResetCommandButton.Click += new EventHandler(this.ReservationFeesResetCommandButtonClicked);
					this.cashierListUpdateCommandButton.Click += new EventHandler(this.CashierListUpdateCommandButtonClicked);
					this.cashierListResetCommandButton.Click += new EventHandler(this.CashierListResetCommandButtonClicked);
				}
				this.recurrencepatterncontrol.RecurrencePatternSubmitted += new RecurrencePatternSubmitted(this.RecurrencePatternSubmitted);
				this.workingHoursUpdateCommandButton.Click += new EventHandler(this.WorkingHoursUpdateCommandButtonClicked);
				this.workingHoursResetCommandButton.Click += new EventHandler(this.WorkingHoursResetCommandButtonClicked);
				this.workingHoursExceptionsUpdateCommandButton.Click += new EventHandler(this.WorkingHoursExceptionsUpdateCommandButtonClicked);
				this.workingHoursExceptionsResetCommandButton.Click += new EventHandler(this.WorkingHoursExceptionsResetCommandButtonClicked);
				this.bccListUpdateCommandButton.Click += new EventHandler(this.BCCListUpdateCommandButtonClicked);
				this.bccListResetCommandButton.Click += new EventHandler(this.BCCListResetCommandButtonClicked);
				this.moderationUpdateCommandButton.Click += new EventHandler(this.ModerationUpdateCommandButtonClicked);
				this.moderationResetCommandButton.Click += new EventHandler(this.ModerationResetCommandButtonClicked);
				this.updateMailTemplateCommandButton.Click += new EventHandler(this.UpdateMailTemplateCommandButtonClicked);
				this.resetMailTemplateCommandButton.Click += new EventHandler(this.ResetMailTemplateCommandButtonClicked);
				this.updateSMSTemplateCommandButton.Click += new EventHandler(this.UpdateSMSTemplateCommandButtonClicked);
				this.resetSMSTemplateCommandButton.Click += new EventHandler(this.ResetSMSTemplateCommandButtonClicked);
				this.updateSettingsCommandButton.Click += new EventHandler(this.UpdateSettingsCommandButtonClicked);
				this.cancelSettingsCommandButton.Click += new EventHandler(this.CancelSettingsCommandButtonClicked);
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
			}
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			try
			{
				if (!this.Page.IsPostBack && base.Request.QueryString["ctl"].ToLower() == "editsettings")
				{
					this.LoadSettings();
					this.updateCancelTableRow.Visible = true;
				}
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
			}
		}

		protected void PaymentMethodChanged(object sender, EventArgs e)
		{
			HtmlTableRow htmlTableRow = this.payPalAccountTableRow;
			HtmlTableRow htmlTableRow1 = this.payPalUrlTableRow;
			PaymentMethod paymentMethod = PaymentMethod.PayPalPaymentsStandard;
			bool selectedValue = this.paymentMethodDropDownList.SelectedValue == paymentMethod.ToString();
			bool flag = selectedValue;
			htmlTableRow1.Visible = selectedValue;
			htmlTableRow.Visible = flag;
			HtmlTableRow htmlTableRow2 = this.authorizeNetApiLoginTableRow;
			HtmlTableRow htmlTableRow3 = this.authorizeNetMerchantHashTableRow;
			HtmlTableRow htmlTableRow4 = this.authorizeNetTestModeTableRow;
			HtmlTableRow htmlTableRow5 = this.authorizeNetTransactionKeyTableRow;
			paymentMethod = PaymentMethod.AuthorizeNetSIM;
			bool selectedValue1 = this.paymentMethodDropDownList.SelectedValue == paymentMethod.ToString();
			bool flag1 = selectedValue1;
			htmlTableRow5.Visible = selectedValue1;
			bool flag2 = flag1;
			bool flag3 = flag2;
			htmlTableRow4.Visible = flag2;
			bool flag4 = flag3;
			flag = flag4;
			htmlTableRow3.Visible = flag4;
			htmlTableRow2.Visible = flag;
		}

		protected void PreventCrossCategoryConflictsChanged(object sender, EventArgs e)
		{
			this.bindUponSelectionTableRow.Visible = (this.preventCrossCategoryConflictsCheckBox.Checked ? false : !this.selectCategoryLastCheckBox.Checked);
		}

		private void RebindCategoryDependentSections()
		{
			this.BindCategoryPermissionsSection();
			this.BindReservationSettingsSection();
			if (this.IsProfessional)
			{
				this.BindReservationFeesSection();
				this.BindCashierListSection();
			}
			this.BindWorkingHoursSection();
			this.BindWorkingHoursExceptionsSection();
			this.BindBCCListSection();
			this.BindModerationSection();
		}

		protected void RecurrencePatternSubmitted(IRecurrencePattern recurrencePattern)
		{
			this.WorkingHours.Add((RecurrencePattern)recurrencePattern);
			this.BindWorkingHoursDataGrid();
			this.recurrencepatterncontrol.Visible = false;
			this.addWorkingHoursCommandButton.Visible = true;
		}

		protected void RequireEmailCheckBoxCheckChanged(object sender, EventArgs e)
		{
			this.requireVerificationCodeTableRow.Visible = this.requireEmailCheckBox.Checked;
			this.requireVerificationCodeCheckBox.Checked = false;
		}

		protected void ReservationFeesCategoryDropDownListSelectedIndexChanged(object sender, EventArgs e)
		{
			this.BindReservationFeesSection(int.Parse(this.reservationFeesCategoryDropDownList.SelectedValue));
		}

		protected void ReservationFeesResetCommandButtonClicked(object sender, EventArgs e)
		{
			int num = int.Parse(this.reservationFeesCategoryDropDownList.SelectedValue);
			CategorySettingController categorySettingController = new CategorySettingController();
			categorySettingController.DeleteCategorySetting(num, "FeeScheduleType");
			categorySettingController.DeleteCategorySetting(num, "DepositFee");
			categorySettingController.DeleteCategorySetting(num, "SchedulingFee");
			categorySettingController.DeleteCategorySetting(num, "ReschedulingFee");
			categorySettingController.DeleteCategorySetting(num, "CancellationFee");
			categorySettingController.DeleteCategorySetting(num, "SchedulingFeeInterval");
			int num1 = 1;
			Hashtable settings = (new CategorySettings(base.PortalId, base.TabModuleId, num)).Settings;
			while (settings.ContainsKey(string.Concat("SeasonalFeeScheduleList.", num1)))
			{
				categorySettingController.DeleteCategorySetting(num, string.Concat("SeasonalFeeScheduleList.", num1));
				num1++;
			}
			this.BindCategoriesDropDownList(this.reservationFeesCategoryDropDownList, "SchedulingFee", "FeeScheduleType", null);
			this.BindReservationFeesSection(num);
		}

		protected void ReservationFeesUpdateCommandButtonClicked(object sender, EventArgs e)
		{
			this.UpdateReservationFeesSection();
		}

		protected void ReservationSettingsCategoryDropDownListSelectedIndexChanged(object sender, EventArgs e)
		{
			this.BindReservationSettingsSection(int.Parse(this.reservationSettingsCategoryDropDownList.SelectedValue));
		}

		protected void ReservationSettingsResetCommandButtonClicked(object sender, EventArgs e)
		{
			int num = int.Parse(this.reservationSettingsCategoryDropDownList.SelectedValue);
			CategorySettingController categorySettingController = new CategorySettingController();
			categorySettingController.DeleteCategorySetting(num, "AllowCancellations");
			categorySettingController.DeleteCategorySetting(num, "AllowRescheduling");
			categorySettingController.DeleteCategorySetting(num, "MinTimeAhead");
			categorySettingController.DeleteCategorySetting(num, "DaysAhead");
			categorySettingController.DeleteCategorySetting(num, "ReservationInterval");
			categorySettingController.DeleteCategorySetting(num, "ReservationDuration");
			categorySettingController.DeleteCategorySetting(num, "ReservationDurationMax");
			categorySettingController.DeleteCategorySetting(num, "ReservationDurationInterval");
			categorySettingController.DeleteCategorySetting(num, "MaxReservationsPerTimeSlot");
			categorySettingController.DeleteCategorySetting(num, "MaxReservationsPerUser");
			this.BindCategoriesDropDownList(this.reservationSettingsCategoryDropDownList, "AllowCancellations", null, null);
			this.BindReservationSettingsSection(num);
		}

		protected void ReservationSettingsUpdateCommandButtonClicked(object sender, EventArgs e)
		{
			this.UpdateReservationSettingsSection(true);
		}

		protected void ResetMailTemplateCommandButtonClicked(object sender, EventArgs e)
		{
			bool flag;
			if (this.mailTemplateDropDownList.SelectedValue == "Confirmation")
			{
				this.mailTemplateSubjectTextBox.Text = Localization.GetString("ConfirmationMailSubject", base.LocalResourceFile);
				this.mailTemplateBodyTextBox.Text = Localization.GetString("ConfirmationMailBody", base.LocalResourceFile);
				RadioButton radioButton = this.mailTemplateBodyTypeHtmlRadioButton;
				RadioButton radioButton1 = this.mailTemplateBodyTypeTextRadioButton;
				bool lower = Localization.GetString("ConfirmationMailBodyType", base.LocalResourceFile).ToLower() == "text";
				flag = lower;
				radioButton1.Checked = lower;
				radioButton.Checked = !flag;
			}
			else if (this.mailTemplateDropDownList.SelectedValue == "Modification")
			{
				this.mailTemplateSubjectTextBox.Text = Localization.GetString("ModificationMailSubject", base.LocalResourceFile);
				this.mailTemplateBodyTextBox.Text = Localization.GetString("ModificationMailBody", base.LocalResourceFile);
				RadioButton radioButton2 = this.mailTemplateBodyTypeHtmlRadioButton;
				RadioButton radioButton3 = this.mailTemplateBodyTypeTextRadioButton;
				bool lower1 = Localization.GetString("ModificationMailBodyType", base.LocalResourceFile).ToLower() == "text";
				flag = lower1;
				radioButton3.Checked = lower1;
				radioButton2.Checked = !flag;
			}
			else if (this.mailTemplateDropDownList.SelectedValue == "Rescheduled")
			{
				this.mailTemplateSubjectTextBox.Text = Localization.GetString("RescheduledMailSubject", base.LocalResourceFile);
				this.mailTemplateBodyTextBox.Text = Localization.GetString("RescheduledMailBody", base.LocalResourceFile);
				RadioButton radioButton4 = this.mailTemplateBodyTypeHtmlRadioButton;
				RadioButton radioButton5 = this.mailTemplateBodyTypeTextRadioButton;
				bool flag1 = Localization.GetString("RescheduledMailBodyType", base.LocalResourceFile).ToLower() == "text";
				flag = flag1;
				radioButton5.Checked = flag1;
				radioButton4.Checked = !flag;
			}
			else if (this.mailTemplateDropDownList.SelectedValue == "Cancellation")
			{
				this.mailTemplateSubjectTextBox.Text = Localization.GetString("CancellationMailSubject", base.LocalResourceFile);
				this.mailTemplateBodyTextBox.Text = Localization.GetString("CancellationMailBody", base.LocalResourceFile);
				RadioButton radioButton6 = this.mailTemplateBodyTypeHtmlRadioButton;
				RadioButton radioButton7 = this.mailTemplateBodyTypeTextRadioButton;
				bool lower2 = Localization.GetString("CancellationMailBodyType", base.LocalResourceFile).ToLower() == "text";
				flag = lower2;
				radioButton7.Checked = lower2;
				radioButton6.Checked = !flag;
			}
			else if (this.mailTemplateDropDownList.SelectedValue == "Moderator")
			{
				this.mailTemplateSubjectTextBox.Text = Localization.GetString("ModeratorMailSubject", base.LocalResourceFile);
				this.mailTemplateBodyTextBox.Text = Localization.GetString("ModeratorMailBody", base.LocalResourceFile);
				RadioButton radioButton8 = this.mailTemplateBodyTypeHtmlRadioButton;
				RadioButton radioButton9 = this.mailTemplateBodyTypeTextRadioButton;
				bool flag2 = Localization.GetString("ModeratorMailBodyType", base.LocalResourceFile).ToLower() == "text";
				flag = flag2;
				radioButton9.Checked = flag2;
				radioButton8.Checked = !flag;
			}
			else if (this.mailTemplateDropDownList.SelectedValue == "Declined")
			{
				this.mailTemplateSubjectTextBox.Text = Localization.GetString("DeclinedMailSubject", base.LocalResourceFile);
				this.mailTemplateBodyTextBox.Text = Localization.GetString("DeclinedMailBody", base.LocalResourceFile);
				RadioButton radioButton10 = this.mailTemplateBodyTypeHtmlRadioButton;
				RadioButton radioButton11 = this.mailTemplateBodyTypeTextRadioButton;
				bool lower3 = Localization.GetString("DeclinedMailBodyType", base.LocalResourceFile).ToLower() == "text";
				flag = lower3;
				radioButton11.Checked = lower3;
				radioButton10.Checked = !flag;
			}
			else if (this.mailTemplateDropDownList.SelectedValue == "VerificationCode")
			{
				this.mailTemplateSubjectTextBox.Text = Localization.GetString("VerificationCodeMailSubject", base.LocalResourceFile);
				this.mailTemplateBodyTextBox.Text = Localization.GetString("VerificationCodeMailBody", base.LocalResourceFile);
				RadioButton radioButton12 = this.mailTemplateBodyTypeHtmlRadioButton;
				RadioButton radioButton13 = this.mailTemplateBodyTypeTextRadioButton;
				bool flag3 = Localization.GetString("VerificationCodeMailBodyType", base.LocalResourceFile).ToLower() == "text";
				flag = flag3;
				radioButton13.Checked = flag3;
				radioButton12.Checked = !flag;
			}
			else if (this.mailTemplateDropDownList.SelectedValue == "DuplicateReservation")
			{
				this.mailTemplateSubjectTextBox.Text = Localization.GetString("DuplicateReservationMailSubject", base.LocalResourceFile);
				this.mailTemplateBodyTextBox.Text = Localization.GetString("DuplicateReservationMailBody", base.LocalResourceFile);
				RadioButton radioButton14 = this.mailTemplateBodyTypeHtmlRadioButton;
				RadioButton radioButton15 = this.mailTemplateBodyTypeTextRadioButton;
				bool lower4 = Localization.GetString("DuplicateReservationMailBodyType", base.LocalResourceFile).ToLower() == "text";
				flag = lower4;
				radioButton15.Checked = lower4;
				radioButton14.Checked = !flag;
			}
			else if (this.mailTemplateDropDownList.SelectedValue == "Reminder")
			{
				this.mailTemplateSubjectTextBox.Text = Localization.GetString("ReminderMailSubject", base.LocalResourceFile);
				this.mailTemplateBodyTextBox.Text = Localization.GetString("ReminderMailBody", base.LocalResourceFile);
				RadioButton radioButton16 = this.mailTemplateBodyTypeHtmlRadioButton;
				RadioButton radioButton17 = this.mailTemplateBodyTypeTextRadioButton;
				bool flag4 = Localization.GetString("ReminderMailBodyType", base.LocalResourceFile).ToLower() == "text";
				flag = flag4;
				radioButton17.Checked = flag4;
				radioButton16.Checked = !flag;
			}
			else if (this.mailTemplateDropDownList.SelectedValue == "PendingRescheduleRefund")
			{
				this.mailTemplateSubjectTextBox.Text = Localization.GetString("PendingRescheduleRefundMailSubject", base.LocalResourceFile);
				this.mailTemplateBodyTextBox.Text = Localization.GetString("PendingRescheduleRefundMailBody", base.LocalResourceFile);
				RadioButton radioButton18 = this.mailTemplateBodyTypeHtmlRadioButton;
				RadioButton radioButton19 = this.mailTemplateBodyTypeTextRadioButton;
				bool lower5 = Localization.GetString("PendingRescheduleRefundMailBodyType", base.LocalResourceFile).ToLower() == "text";
				flag = lower5;
				radioButton19.Checked = lower5;
				radioButton18.Checked = !flag;
			}
			else if (this.mailTemplateDropDownList.SelectedValue == "PendingCancellationRefund")
			{
				this.mailTemplateSubjectTextBox.Text = Localization.GetString("PendingCancellationRefundMailSubject", base.LocalResourceFile);
				this.mailTemplateBodyTextBox.Text = Localization.GetString("PendingCancellationRefundMailBody", base.LocalResourceFile);
				RadioButton radioButton20 = this.mailTemplateBodyTypeHtmlRadioButton;
				RadioButton radioButton21 = this.mailTemplateBodyTypeTextRadioButton;
				bool flag5 = Localization.GetString("PendingCancellationRefundMailBodyType", base.LocalResourceFile).ToLower() == "text";
				flag = flag5;
				radioButton21.Checked = flag5;
				radioButton20.Checked = !flag;
			}
			else if (this.mailTemplateDropDownList.SelectedValue == "PendingDeclinationRefund")
			{
				this.mailTemplateSubjectTextBox.Text = Localization.GetString("PendingDeclinationRefundMailSubject", base.LocalResourceFile);
				this.mailTemplateBodyTextBox.Text = Localization.GetString("PendingDeclinationRefundMailBody", base.LocalResourceFile);
				RadioButton radioButton22 = this.mailTemplateBodyTypeHtmlRadioButton;
				RadioButton radioButton23 = this.mailTemplateBodyTypeTextRadioButton;
				bool lower6 = Localization.GetString("PendingDeclinationRefundMailBodyType", base.LocalResourceFile).ToLower() == "text";
				flag = lower6;
				radioButton23.Checked = lower6;
				radioButton22.Checked = !flag;
			}
			this.iCalendarAttachmentFileNameTextBox.Text = Localization.GetString("ICalendarAttachmentFileName", base.LocalResourceFile);
			this.UpdateMailTemplateCommandButtonClicked(sender, e);
		}

		protected void ResetSMSTemplateCommandButtonClicked(object sender, EventArgs e)
		{
			if (this.smsTemplateDropDownList.SelectedValue == "Reminder")
			{
				this.smsTemplateBodyTextBox.Text = Localization.GetString("ReminderSMS", base.LocalResourceFile);
			}
			this.UpdateSMSTemplateCommandButtonClicked(sender, e);
		}

		protected override object SaveViewState()
		{
			try
			{
				this.ViewState["CategoryList"] = this.SerializeCategoryList(this.CategoryList);
				//this.ViewState["CashierList"] = this.ModuleSettings.SerializeUserIDList(this.CashierList);
				this.ViewState["CashierList"] = this.ModuleSettings.SerializeEmailList(this.CashierList);
				//this.ViewState["BCCList"] = this.ModuleSettings.SerializeUserIDList(this.BCCList);
				this.ViewState["BCCList"] = this.ModuleSettings.SerializeEmailList(this.BCCList);
				this.ViewState["TimeOfDayList"] = this.ModuleSettings.SerializeTimeOfDayList(this.TimeOfDayList);
				//this.ViewState["ModeratorList"] = this.ModuleSettings.SerializeUserIDList(this.ModeratorList);
				this.ViewState["ModeratorList"] = this.ModuleSettings.SerializeEmailList(this.ModeratorList);
				//this.ViewState["ViewReservationsList"] = this.ModuleSettings.SerializeUserIDList(this.ViewReservationsList);
				this.ViewState["ViewReservationsList"] = this.ModuleSettings.SerializeEmailList(this.ViewReservationsList);
				//this.ViewState["DuplicateReservationsList"] = this.ModuleSettings.SerializeUserIDList(this.DuplicateReservationsList);
				this.ViewState["DuplicateReservationsList"] = this.ModuleSettings.SerializeEmailList(this.DuplicateReservationsList);
				this.ViewState["WorkingHours"] = this.SerializeRecurrencePatternList(this.WorkingHours);
				this.ViewState["WorkingHoursExceptions"] = this.SerializeWorkingHoursExceptions(this.WorkingHoursExceptions);
				this.ViewState["ModerationHours"] = this.SerializeWorkingHours(this.ModerationHours);
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
			}
			return base.SaveViewState();
		}

		protected void SelectCategoryLastChanged(object sender, EventArgs e)
		{
			this.bindUponSelectionTableRow.Visible = (this.preventCrossCategoryConflictsCheckBox.Checked ? false : !this.selectCategoryLastCheckBox.Checked);
		}

		public string SerializeCategoryList(List<CategoryInfo> categoryList)
		{
			string str;
			CategoryInfo[] array = categoryList.ToArray();
			StringWriter stringWriter = null;
			try
			{
				stringWriter = new StringWriter();
				(new XmlSerializer(typeof(CategoryInfo[]))).Serialize(new XmlTextWriter(stringWriter), array);
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

		public string SerializeRecurrencePatternList(List<RecurrencePattern> workingHours)
		{
			string str;
			StringWriter stringWriter = null;
			try
			{
				stringWriter = new StringWriter();
				(new XmlSerializer(typeof(List<RecurrencePattern>))).Serialize(new XmlTextWriter(stringWriter), workingHours);
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

		public string SerializeWorkingHours(ArrayList workingHours)
		{
			string str;
			WorkingHoursInfo[] array = (WorkingHoursInfo[])workingHours.ToArray(typeof(WorkingHoursInfo));
			StringWriter stringWriter = null;
			try
			{
				stringWriter = new StringWriter();
				(new XmlSerializer(typeof(WorkingHoursInfo[]))).Serialize(new XmlTextWriter(stringWriter), array);
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

		public string SerializeWorkingHoursExceptions(ArrayList workingHoursExceptions)
		{
			string str;
			WorkingHoursExceptionInfo[] array = (WorkingHoursExceptionInfo[])workingHoursExceptions.ToArray(typeof(WorkingHoursExceptionInfo));
			StringWriter stringWriter = null;
			try
			{
				stringWriter = new StringWriter();
				(new XmlSerializer(typeof(WorkingHoursExceptionInfo[]))).Serialize(new XmlTextWriter(stringWriter), array);
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

		protected void SetTimeSpan(TimeSpan timeSpan, TextBox textBox, DropDownList dropDownList)
		{
			double totalMinutes;
			if (timeSpan.TotalMinutes == 0)
			{
				totalMinutes = timeSpan.TotalMinutes;
				textBox.Text = totalMinutes.ToString();
				dropDownList.SelectedValue = "M";
				return;
			}
			if (Convert.ToInt32(timeSpan.TotalMinutes) % 1440 == 0)
			{
				totalMinutes = timeSpan.TotalDays;
				textBox.Text = totalMinutes.ToString();
				dropDownList.SelectedValue = "D";
				return;
			}
			if (Convert.ToInt32(timeSpan.TotalMinutes) % 60 == 0)
			{
				totalMinutes = timeSpan.TotalHours;
				textBox.Text = totalMinutes.ToString();
				dropDownList.SelectedValue = "H";
				return;
			}
			totalMinutes = timeSpan.TotalMinutes;
			textBox.Text = totalMinutes.ToString();
			dropDownList.SelectedValue = "M";
		}

		protected void ShowHideRequireConfirmationWhenTableRow(object sender, EventArgs e)
		{
			HtmlTableRow htmlTableRow = this.requireConfirmationTableRow2;
			HtmlTableRow htmlTableRow1 = this.requireConfirmationWhenTableRow;
			bool @checked = this.requireConfirmationCheckBox.Checked;
			bool flag = @checked;
			htmlTableRow1.Visible = @checked;
			htmlTableRow.Visible = flag;
		}

		protected void ShowHideSendRemindersWhenTableRow(object sender, EventArgs e)
		{
			bool flag;
			HtmlTableRow htmlTableRow = this.requireConfirmationTableRow;
			HtmlTableRow htmlTableRow1 = this.requireConfirmationWhenTableRow;
			HtmlTableRow htmlTableRow2 = this.sendRemindersWhenTableRow;
			HtmlTableRow htmlTableRow3 = this.sendRemindersViaTableRow;
			bool @checked = this.sendRemindersCheckBox.Checked;
			bool flag1 = @checked;
			htmlTableRow3.Visible = @checked;
			bool flag2 = flag1;
			bool flag3 = flag2;
			htmlTableRow2.Visible = flag2;
			bool flag4 = flag3;
			bool flag5 = flag4;
			htmlTableRow1.Visible = flag4;
			htmlTableRow.Visible = flag5;
			HtmlTableRow htmlTableRow4 = this.requireConfirmationTableRow2;
			HtmlTableRow htmlTableRow5 = this.requireConfirmationWhenTableRow;
			flag = (!this.requireConfirmationTableRow.Visible ? false : this.requireConfirmationCheckBox.Checked);
			flag5 = flag;
			htmlTableRow5.Visible = flag;
			htmlTableRow4.Visible = flag5;
			this.sendRemindersViaTableRow.Visible = (!this.sendRemindersViaTableRow.Visible ? false : this.IsProfessional);
		}

		protected void ShowHideTimeOfDayTableRow(object sender, EventArgs e)
		{
			HtmlTableRow htmlTableRow = this.timeOfDaySelectionModeTableRow;
			HtmlTableRow htmlTableRow1 = this.displayUnavailableTimeOfDayTableRow;
			HtmlTableRow htmlTableRow2 = this.timeOfDayTableRow;
			bool @checked = this.displayTimeOfDayCheckBox.Checked;
			bool flag = @checked;
			htmlTableRow2.Visible = @checked;
			bool flag1 = flag;
			bool flag2 = flag1;
			htmlTableRow1.Visible = flag1;
			htmlTableRow.Visible = flag2;
		}

		protected void SMSTemplateDropDownListSelectedIndexChanged(object sender, EventArgs e)
		{
			if (this.smsTemplateDropDownList.SelectedValue == "Reminder")
			{
				this.smsTemplateBodyTextBox.Text = this.ModuleSettings.ReminderSMS;
			}
		}

		protected string TimeSpanToString(TimeSpan timeSpan)
		{
			double totalMinutes;
			if (timeSpan.TotalMinutes == 0)
			{
				totalMinutes = timeSpan.TotalMinutes;
				return string.Concat(totalMinutes.ToString(), " ", Localization.GetString("Minutes", base.LocalResourceFile));
			}
			if (Convert.ToInt32(timeSpan.TotalMinutes) % 1440 == 0)
			{
				totalMinutes = timeSpan.TotalDays;
				return string.Concat(totalMinutes.ToString(), " ", Localization.GetString("Days", base.LocalResourceFile));
			}
			if (Convert.ToInt32(timeSpan.TotalMinutes) % 60 == 0)
			{
				totalMinutes = timeSpan.TotalHours;
				return string.Concat(totalMinutes.ToString(), " ", Localization.GetString("Hours", base.LocalResourceFile));
			}
			totalMinutes = timeSpan.TotalMinutes;
			return string.Concat(totalMinutes.ToString(), " ", Localization.GetString("Minutes", base.LocalResourceFile));
		}

		private void UnhighlightInvalidControl(WebControl control)
		{
			control.CssClass = control.CssClass.Replace(" Gafware_Modules_Reservations_Invalid", string.Empty);
		}

		protected void UpdateBCCListSection(bool updateSelectedCategorySettings)
		{
			if (this.Page.IsValid)
			{
				int num = (this.bccListCategoryDropDownList.SelectedValue == null || !(this.bccListCategoryDropDownList.SelectedValue != string.Empty) ? Null.NullInteger : int.Parse(this.bccListCategoryDropDownList.SelectedValue));
				if (num != Null.NullInteger)
				{
					if (!updateSelectedCategorySettings)
					{
						return;
					}
					//(new CategorySettingController()).UpdateCategorySetting(num, "BCCList", this.ModuleSettings.SerializeUserIDList(this.BCCList));
					(new CategorySettingController()).UpdateCategorySetting(num, "BCCList", this.ModuleSettings.SerializeEmailList(this.BCCList));
				}
				else
				{
					//(new ModuleController()).UpdateTabModuleSetting(base.TabModuleId, "BCCList", this.ModuleSettings.SerializeUserIDList(this.BCCList));
					(new ModuleController()).UpdateTabModuleSetting(base.TabModuleId, "BCCList", this.ModuleSettings.SerializeEmailList(this.BCCList));
					ModuleController.SynchronizeModule(base.ModuleId);
					this._ModuleSettings = null;
				}
				this.BindCategoriesDropDownList(this.bccListCategoryDropDownList, "BCCList", null, null);
				this.BindBCCListSection(num);
			}
		}

		protected void UpdateCashierListSection(bool updateSelectedCategorySettings)
		{
			if (this.Page.IsValid)
			{
				int num = (this.cashierListCategoryDropDownList.SelectedValue == null || !(this.cashierListCategoryDropDownList.SelectedValue != string.Empty) ? Null.NullInteger : int.Parse(this.cashierListCategoryDropDownList.SelectedValue));
				if (num != Null.NullInteger)
				{
					if (!updateSelectedCategorySettings)
					{
						return;
					}
					(new CategorySettingController()).UpdateCategorySetting(num, "CashierList", this.ModuleSettings.SerializeUserIDList(this.CashierList));
				}
				else
				{
					(new ModuleController()).UpdateTabModuleSetting(base.TabModuleId, "CashierList", this.ModuleSettings.SerializeUserIDList(this.CashierList));
					ModuleController.SynchronizeModule(base.ModuleId);
					this._ModuleSettings = null;
				}
				this.BindCategoriesDropDownList(this.cashierListCategoryDropDownList, "CashierList", null, null);
				this.BindCashierListSection(num);
			}
		}

		protected void UpdateCategoryPermissionsSection(bool updateSelectedCategorySettings)
		{
			if (this.Page.IsValid)
			{
				foreach (ListItem item in this.categoryPermissionsCheckboxList.Items)
				{
					if (!item.Selected || this.CategoryPermissionsList.IndexOf(int.Parse(item.Value)) != -1)
					{
						if (item.Selected || this.CategoryPermissionsList.IndexOf(int.Parse(item.Value)) == -1)
						{
							continue;
						}
						this.CategoryPermissionsList.Remove(int.Parse(item.Value));
					}
					else
					{
						this.CategoryPermissionsList.Add(int.Parse(item.Value));
					}
				}
				int num = (this.categoryPermissionsDropDownList.SelectedValue == null || !(this.categoryPermissionsDropDownList.SelectedValue != string.Empty) ? Null.NullInteger : int.Parse(this.categoryPermissionsDropDownList.SelectedValue));
				if (num != Null.NullInteger)
				{
					if (!updateSelectedCategorySettings)
					{
						return;
					}
					(new CategorySettingController()).UpdateCategorySetting(num, "CategoryPermissions", this.ModuleSettings.SerializeRoleIDList(this.CategoryPermissionsList));
				}
				else
				{
					(new ModuleController()).UpdateTabModuleSetting(base.TabModuleId, "CategoryPermissions", this.ModuleSettings.SerializeRoleIDList(this.CategoryPermissionsList));
					ModuleController.SynchronizeModule(base.ModuleId);
					this._ModuleSettings = null;
				}
				this.BindCategoriesDropDownList(this.categoryPermissionsDropDownList, "CategoryPermissions", null, null);
				this.BindCategoryPermissionsSection(num);
			}
		}

		protected void UpdateDuplicateReservationsSection()
		{
			if (this.Page.IsValid)
			{
				(new ModuleController()).UpdateTabModuleSetting(base.TabModuleId, "DuplicateReservationsList", this.ModuleSettings.SerializeUserIDList(this.DuplicateReservationsList));
				ModuleController.SynchronizeModule(base.ModuleId);
				this._ModuleSettings = null;
			}
		}

		protected void UpdateMailTemplateCommandButtonClicked(object sender, EventArgs e)
		{
			if (this.Page.IsValid)
			{
				ModuleController moduleController = new ModuleController();
				if (this.mailTemplateDropDownList.SelectedValue == "Confirmation")
				{
					moduleController.UpdateTabModuleSetting(base.TabModuleId, "ConfirmationMailSubject", this.mailTemplateSubjectTextBox.Text);
					moduleController.UpdateTabModuleSetting(base.TabModuleId, "ConfirmationMailBody", this.mailTemplateBodyTextBox.Text);
					moduleController.UpdateTabModuleSetting(base.TabModuleId, "ConfirmationMailBodyType", (this.mailTemplateBodyTypeHtmlRadioButton.Checked ? "HTML" : "Text"));
				}
				else if (this.mailTemplateDropDownList.SelectedValue == "Modification")
				{
					moduleController.UpdateTabModuleSetting(base.TabModuleId, "ModificationMailSubject", this.mailTemplateSubjectTextBox.Text);
					moduleController.UpdateTabModuleSetting(base.TabModuleId, "ModificationMailBody", this.mailTemplateBodyTextBox.Text);
					moduleController.UpdateTabModuleSetting(base.TabModuleId, "ModificationMailBodyType", (this.mailTemplateBodyTypeHtmlRadioButton.Checked ? "HTML" : "Text"));
				}
				else if (this.mailTemplateDropDownList.SelectedValue == "Rescheduled")
				{
					moduleController.UpdateTabModuleSetting(base.TabModuleId, "RescheduledMailSubject", this.mailTemplateSubjectTextBox.Text);
					moduleController.UpdateTabModuleSetting(base.TabModuleId, "RescheduledMailBody", this.mailTemplateBodyTextBox.Text);
					moduleController.UpdateTabModuleSetting(base.TabModuleId, "RescheduledMailBodyType", (this.mailTemplateBodyTypeHtmlRadioButton.Checked ? "HTML" : "Text"));
				}
				else if (this.mailTemplateDropDownList.SelectedValue == "Cancellation")
				{
					moduleController.UpdateTabModuleSetting(base.TabModuleId, "CancellationMailSubject", this.mailTemplateSubjectTextBox.Text);
					moduleController.UpdateTabModuleSetting(base.TabModuleId, "CancellationMailBody", this.mailTemplateBodyTextBox.Text);
					moduleController.UpdateTabModuleSetting(base.TabModuleId, "CancellationMailBodyType", (this.mailTemplateBodyTypeHtmlRadioButton.Checked ? "HTML" : "Text"));
				}
				else if (this.mailTemplateDropDownList.SelectedValue == "Moderator")
				{
					moduleController.UpdateTabModuleSetting(base.TabModuleId, "ModeratorMailSubject", this.mailTemplateSubjectTextBox.Text);
					moduleController.UpdateTabModuleSetting(base.TabModuleId, "ModeratorMailBody", this.mailTemplateBodyTextBox.Text);
					moduleController.UpdateTabModuleSetting(base.TabModuleId, "ModeratorMailBodyType", (this.mailTemplateBodyTypeHtmlRadioButton.Checked ? "HTML" : "Text"));
				}
				else if (this.mailTemplateDropDownList.SelectedValue == "Declined")
				{
					moduleController.UpdateTabModuleSetting(base.TabModuleId, "DeclinedMailSubject", this.mailTemplateSubjectTextBox.Text);
					moduleController.UpdateTabModuleSetting(base.TabModuleId, "DeclinedMailBody", this.mailTemplateBodyTextBox.Text);
					moduleController.UpdateTabModuleSetting(base.TabModuleId, "DeclinedMailBodyType", (this.mailTemplateBodyTypeHtmlRadioButton.Checked ? "HTML" : "Text"));
				}
				else if (this.mailTemplateDropDownList.SelectedValue == "VerificationCode")
				{
					moduleController.UpdateTabModuleSetting(base.TabModuleId, "VerificationCodeMailSubject", this.mailTemplateSubjectTextBox.Text);
					moduleController.UpdateTabModuleSetting(base.TabModuleId, "VerificationCodeMailBody", this.mailTemplateBodyTextBox.Text);
					moduleController.UpdateTabModuleSetting(base.TabModuleId, "VerificationCodeMailBodyType", (this.mailTemplateBodyTypeHtmlRadioButton.Checked ? "HTML" : "Text"));
				}
				else if (this.mailTemplateDropDownList.SelectedValue == "DuplicateReservation")
				{
					moduleController.UpdateTabModuleSetting(base.TabModuleId, "DuplicateReservationMailSubject", this.mailTemplateSubjectTextBox.Text);
					moduleController.UpdateTabModuleSetting(base.TabModuleId, "DuplicateReservationMailBody", this.mailTemplateBodyTextBox.Text);
					moduleController.UpdateTabModuleSetting(base.TabModuleId, "DuplicateReservationMailBodyType", (this.mailTemplateBodyTypeHtmlRadioButton.Checked ? "HTML" : "Text"));
				}
				else if (this.mailTemplateDropDownList.SelectedValue == "Reminder")
				{
					moduleController.UpdateTabModuleSetting(base.TabModuleId, "ReminderMailSubject", this.mailTemplateSubjectTextBox.Text);
					moduleController.UpdateTabModuleSetting(base.TabModuleId, "ReminderMailBody", this.mailTemplateBodyTextBox.Text);
					moduleController.UpdateTabModuleSetting(base.TabModuleId, "ReminderMailBodyType", (this.mailTemplateBodyTypeHtmlRadioButton.Checked ? "HTML" : "Text"));
				}
				else if (this.IsProfessional)
				{
					if (this.mailTemplateDropDownList.SelectedValue == "PendingRescheduleRefund")
					{
						moduleController.UpdateTabModuleSetting(base.TabModuleId, "PendingRescheduleRefundMailSubject", this.mailTemplateSubjectTextBox.Text);
						moduleController.UpdateTabModuleSetting(base.TabModuleId, "PendingRescheduleRefundMailBody", this.mailTemplateBodyTextBox.Text);
						moduleController.UpdateTabModuleSetting(base.TabModuleId, "PendingRescheduleRefundMailBodyType", (this.mailTemplateBodyTypeHtmlRadioButton.Checked ? "HTML" : "Text"));
					}
					else if (this.mailTemplateDropDownList.SelectedValue == "PendingCancellationRefund")
					{
						moduleController.UpdateTabModuleSetting(base.TabModuleId, "PendingCancellationRefundMailSubject", this.mailTemplateSubjectTextBox.Text);
						moduleController.UpdateTabModuleSetting(base.TabModuleId, "PendingCancellationRefundMailBody", this.mailTemplateBodyTextBox.Text);
						moduleController.UpdateTabModuleSetting(base.TabModuleId, "PendingCancellationRefundMailBodyType", (this.mailTemplateBodyTypeHtmlRadioButton.Checked ? "HTML" : "Text"));
					}
					else if (this.mailTemplateDropDownList.SelectedValue == "PendingDeclinationRefund")
					{
						moduleController.UpdateTabModuleSetting(base.TabModuleId, "PendingDeclinationRefundMailSubject", this.mailTemplateSubjectTextBox.Text);
						moduleController.UpdateTabModuleSetting(base.TabModuleId, "PendingDeclinationRefundMailBody", this.mailTemplateBodyTextBox.Text);
						moduleController.UpdateTabModuleSetting(base.TabModuleId, "PendingDeclinationRefundMailBodyType", (this.mailTemplateBodyTypeHtmlRadioButton.Checked ? "HTML" : "Text"));
					}
				}
				moduleController.UpdateTabModuleSetting(base.TabModuleId, "MailFrom", this.mailFromTextBox.Text);
				int tabModuleId = base.TabModuleId;
				bool @checked = this.attachICalendarCheckBox.Checked;
				moduleController.UpdateTabModuleSetting(tabModuleId, "AttachiCalendar", @checked.ToString());
				moduleController.UpdateTabModuleSetting(base.TabModuleId, "ICalendarAttachmentFileName", this.iCalendarAttachmentFileNameTextBox.Text);
				ModuleController.SynchronizeModule(base.ModuleId);
				this._ModuleSettings = null;
			}
		}

		protected void UpdateModerationSection(bool updateSelectedCategorySettings)
		{
			bool @checked;
			if (this.Page.IsValid)
			{
				int num = (this.moderationCategoryDropDownList.SelectedValue == null || !(this.moderationCategoryDropDownList.SelectedValue != string.Empty) ? Null.NullInteger : int.Parse(this.moderationCategoryDropDownList.SelectedValue));
				ModuleController moduleController = null;
				CategorySettingController categorySettingController = null;
				if (num != Null.NullInteger)
				{
					if (!updateSelectedCategorySettings)
					{
						return;
					}
					categorySettingController = new CategorySettingController();
					@checked = this.moderateCheckBox.Checked;
					categorySettingController.UpdateCategorySetting(num, "Moderate", @checked.ToString());
					categorySettingController.UpdateCategorySetting(num, "GlobalModeratorList", this.ModuleSettings.SerializeUserIDList(this.ModeratorList));
				}
				else
				{
					moduleController = new ModuleController();
					int tabModuleId = base.TabModuleId;
					@checked = this.moderateCheckBox.Checked;
					moduleController.UpdateTabModuleSetting(tabModuleId, "Moderate", @checked.ToString());
					moduleController.UpdateTabModuleSetting(base.TabModuleId, "GlobalModeratorList", this.ModuleSettings.SerializeUserIDList(this.ModeratorList));
				}
				this.ModerationHours.Sort(new WorkingHourInfoComparer());
				foreach (DayOfWeek value in Enum.GetValues(typeof(DayOfWeek)))
				{
					string empty = string.Empty;
					foreach (WorkingHoursInfo moderationHour in this.ModerationHours)
					{
						if (moderationHour.DayOfWeek != value)
						{
							continue;
						}
						if (!moderationHour.AllDay)
						{
							string[] str = new string[] { empty, null, null, null, null };
							str[1] = (empty == string.Empty ? string.Empty : ";");
							TimeSpan startTime = moderationHour.StartTime;
							str[2] = startTime.ToString();
							str[3] = "-";
							startTime = moderationHour.EndTime;
							str[4] = startTime.ToString();
							empty = string.Concat(str);
						}
						else
						{
							empty = "All Day";
						}
					}
					if (moduleController == null)
					{
						categorySettingController.UpdateCategorySetting(num, string.Concat("Moderation.", value.ToString()), empty);
					}
					else
					{
						moduleController.UpdateTabModuleSetting(base.TabModuleId, string.Concat("Moderation.", value.ToString()), empty);
					}
				}
				if (moduleController != null)
				{
					ModuleController.SynchronizeModule(base.ModuleId);
					this._ModuleSettings = null;
				}
				this.BindCategoriesDropDownList(this.moderationCategoryDropDownList, "Moderate", null, null);
				this.BindModerationSection(num);
			}
		}

		protected void UpdateReservationFeesSection()
		{
			FeeScheduleType feeScheduleType;
			decimal depositFee;
			int interval;
			int j;
			int i;
			if (this.Page.IsValid)
			{
				ModuleController moduleController = new ModuleController();
				moduleController.UpdateTabModuleSetting(base.TabModuleId, "PaymentMethod", this.paymentMethodDropDownList.SelectedValue);
				moduleController.UpdateTabModuleSetting(base.TabModuleId, "AuthorizeNetApiLogin", this.authorizeNetApiLoginTextBox.Text);
				moduleController.UpdateTabModuleSetting(base.TabModuleId, "AuthorizeNetTransactionKey", this.authorizeNetTransactionKeyTextBox.Text);
				moduleController.UpdateTabModuleSetting(base.TabModuleId, "AuthorizeNetMerchantHash", this.authorizeNetMerchantHashTextBox.Text);
				int tabModuleId = base.TabModuleId;
				bool @checked = this.authorizeNetTestModeCheckBox.Checked;
				moduleController.UpdateTabModuleSetting(tabModuleId, "AuthorizeNetTestMode", @checked.ToString());
				moduleController.UpdateTabModuleSetting(base.TabModuleId, "PayPalAccount", this.payPalAccountTextBox.Text);
				moduleController.UpdateTabModuleSetting(base.TabModuleId, "PayPalSite", this.payPalUrlTextBox.Text);
				moduleController.UpdateTabModuleSetting(base.TabModuleId, "PayPalItemDescription", this.payPalItemDescriptionTextBox.Text);
				moduleController.UpdateTabModuleSetting(base.TabModuleId, "PendingPaymentExpiration", this.pendingPaymentExpirationTextBox.Text);
				moduleController.UpdateTabModuleSetting(base.TabModuleId, "Currency", this.currencyDropDownList.SelectedValue);
				int num = base.TabModuleId;
				@checked = this.allowPayLaterCheckBox.Checked;
				moduleController.UpdateTabModuleSetting(num, "AllowPayLater", @checked.ToString());
				int num1 = (this.reservationFeesCategoryDropDownList.SelectedValue == null || !(this.reservationFeesCategoryDropDownList.SelectedValue != string.Empty) ? Null.NullInteger : int.Parse(this.reservationFeesCategoryDropDownList.SelectedValue));
				if (num1 != Null.NullInteger)
				{
					CategorySettingController categorySettingController = new CategorySettingController();
					feeScheduleType = this.feeschedulecontrol.GetFeeScheduleType();
					categorySettingController.UpdateCategorySetting(num1, "FeeScheduleType", feeScheduleType.ToString());
					if (this.feeschedulecontrol.GetFeeScheduleType() == FeeScheduleType.Flat)
					{
						FlatFeeScheduleInfo flatFeeScheduleInfo = this.feeschedulecontrol.GetFlatFeeScheduleInfo();
						depositFee = flatFeeScheduleInfo.DepositFee;
						categorySettingController.UpdateCategorySetting(num1, "DepositFee", depositFee.ToString(CultureInfo.InvariantCulture));
						depositFee = flatFeeScheduleInfo.ReservationFee;
						categorySettingController.UpdateCategorySetting(num1, "SchedulingFee", depositFee.ToString(CultureInfo.InvariantCulture));
						depositFee = flatFeeScheduleInfo.ReschedulingFee;
						categorySettingController.UpdateCategorySetting(num1, "ReschedulingFee", depositFee.ToString(CultureInfo.InvariantCulture));
						depositFee = flatFeeScheduleInfo.CancellationFee;
						categorySettingController.UpdateCategorySetting(num1, "CancellationFee", depositFee.ToString(CultureInfo.InvariantCulture));
						interval = flatFeeScheduleInfo.Interval;
						categorySettingController.UpdateCategorySetting(num1, "SchedulingFeeInterval", interval.ToString());
					}
					else if (this.feeschedulecontrol.GetFeeScheduleType() == FeeScheduleType.Seasonal)
					{
						Hashtable settings = (new CategorySettings(base.PortalId, base.TabModuleId, num1)).Settings;
						for (i = 1; settings.ContainsKey(string.Concat("SeasonalFeeScheduleList.", i)); i++)
						{
							categorySettingController.DeleteCategorySetting(num1, string.Concat("SeasonalFeeScheduleList.", i));
						}
						i = 1;
						foreach (string str in Helper.ChunksUpto(Helper.SerializeSeasonalFeeScheduleList(this.feeschedulecontrol.SeasonalFeeScheduleList), 2000))
						{
							categorySettingController.UpdateCategorySetting(num1, string.Concat("SeasonalFeeScheduleList.", i), str);
							i++;
						}
					}
				}
				else
				{
					int tabModuleId1 = base.TabModuleId;
					feeScheduleType = this.feeschedulecontrol.GetFeeScheduleType();
					moduleController.UpdateTabModuleSetting(tabModuleId1, "FeeScheduleType", feeScheduleType.ToString());
					if (this.feeschedulecontrol.GetFeeScheduleType() == FeeScheduleType.Flat)
					{
						FlatFeeScheduleInfo flatFeeScheduleInfo1 = this.feeschedulecontrol.GetFlatFeeScheduleInfo();
						int tabModuleId2 = base.TabModuleId;
						depositFee = flatFeeScheduleInfo1.DepositFee;
						moduleController.UpdateTabModuleSetting(tabModuleId2, "DepositFee", depositFee.ToString(CultureInfo.InvariantCulture));
						int num2 = base.TabModuleId;
						depositFee = flatFeeScheduleInfo1.ReservationFee;
						moduleController.UpdateTabModuleSetting(num2, "SchedulingFee", depositFee.ToString(CultureInfo.InvariantCulture));
						int tabModuleId3 = base.TabModuleId;
						depositFee = flatFeeScheduleInfo1.ReschedulingFee;
						moduleController.UpdateTabModuleSetting(tabModuleId3, "ReschedulingFee", depositFee.ToString(CultureInfo.InvariantCulture));
						int num3 = base.TabModuleId;
						depositFee = flatFeeScheduleInfo1.CancellationFee;
						moduleController.UpdateTabModuleSetting(num3, "CancellationFee", depositFee.ToString(CultureInfo.InvariantCulture));
						int tabModuleId4 = base.TabModuleId;
						interval = flatFeeScheduleInfo1.Interval;
						moduleController.UpdateTabModuleSetting(tabModuleId4, "SchedulingFeeInterval", interval.ToString());
					}
					else if (this.feeschedulecontrol.GetFeeScheduleType() == FeeScheduleType.Seasonal)
					{
						for (j = 1; base.Settings.ContainsKey(string.Concat("SeasonalFeeScheduleList.", j)); j++)
						{
							moduleController.DeleteTabModuleSetting(base.TabModuleId, string.Concat("SeasonalFeeScheduleList.", j));
						}
						j = 1;
						foreach (string str1 in Helper.ChunksUpto(Helper.SerializeSeasonalFeeScheduleList(this.feeschedulecontrol.SeasonalFeeScheduleList), 2000))
						{
							moduleController.UpdateTabModuleSetting(base.TabModuleId, string.Concat("SeasonalFeeScheduleList.", j), str1);
							j++;
						}
					}
				}
				ModuleController.SynchronizeModule(base.ModuleId);
				this._ModuleSettings = null;
				this.BindCategoriesDropDownList(this.reservationFeesCategoryDropDownList, "SchedulingFee", "FeeScheduleType", null);
				this.BindReservationFeesSection(num1);
			}
		}

		protected void UpdateReservationSettingsSection(bool updateSelectedCategorySettings)
		{
			bool @checked;
			if (this.Page.IsValid)
			{
				int num = (this.reservationSettingsCategoryDropDownList.SelectedValue == null || !(this.reservationSettingsCategoryDropDownList.SelectedValue != string.Empty) ? Null.NullInteger : int.Parse(this.reservationSettingsCategoryDropDownList.SelectedValue));
				TimeSpan timeSpan = this.GetTimeSpan(this.minTimeAheadTextBox, this.minTimeAheadDropDownList);
				TimeSpan timeSpan1 = this.GetTimeSpan(this.reservationIntervalTextBox, this.reservationIntervalDropDownList);
				double totalMinutes = timeSpan1.TotalMinutes;
				timeSpan1 = this.GetTimeSpan(this.reservationDurationTextBox, this.reservationDurationDropDownList);
				double totalMinutes1 = timeSpan1.TotalMinutes;
				timeSpan1 = this.GetTimeSpan(this.reservationDurationMaxTextBox, this.reservationDurationMaxDropDownList);
				double num1 = timeSpan1.TotalMinutes;
				timeSpan1 = this.GetTimeSpan(this.reservationDurationIntervalTextBox, this.reservationDurationIntervalDropDownList);
				double totalMinutes2 = timeSpan1.TotalMinutes;
				if (num != Null.NullInteger)
				{
					if (!updateSelectedCategorySettings)
					{
						return;
					}
					CategorySettingController categorySettingController = new CategorySettingController();
					@checked = this.allowCancellationsCheckBox.Checked;
					categorySettingController.UpdateCategorySetting(num, "AllowCancellations", @checked.ToString());
					@checked = this.allowReschedulingCheckBox.Checked;
					categorySettingController.UpdateCategorySetting(num, "AllowRescheduling", @checked.ToString());
					categorySettingController.UpdateCategorySetting(num, "ReservationInterval", totalMinutes.ToString());
					categorySettingController.UpdateCategorySetting(num, "ReservationDuration", totalMinutes1.ToString());
					categorySettingController.UpdateCategorySetting(num, "ReservationDurationMax", num1.ToString());
					categorySettingController.UpdateCategorySetting(num, "ReservationDurationInterval", totalMinutes2.ToString());
					categorySettingController.UpdateCategorySetting(num, "MinTimeAhead", timeSpan.ToString());
					categorySettingController.UpdateCategorySetting(num, "DaysAhead", this.daysAheadTextBox.Text);
					categorySettingController.UpdateCategorySetting(num, "MaxReservationsPerTimeSlot", this.maxConflictingReservationsTextBox.Text);
					categorySettingController.UpdateCategorySetting(num, "MaxReservationsPerUser", (string.IsNullOrEmpty(this.maxReservationsPerUserTextBox.Text.Trim()) ? Null.NullInteger.ToString() : this.maxReservationsPerUserTextBox.Text.Trim()));
				}
				else
				{
					ModuleController moduleController = new ModuleController();
					int tabModuleId = base.TabModuleId;
					@checked = this.allowCancellationsCheckBox.Checked;
					moduleController.UpdateTabModuleSetting(tabModuleId, "AllowCancellations", @checked.ToString());
					int tabModuleId1 = base.TabModuleId;
					@checked = this.allowReschedulingCheckBox.Checked;
					moduleController.UpdateTabModuleSetting(tabModuleId1, "AllowRescheduling", @checked.ToString());
					moduleController.UpdateTabModuleSetting(base.TabModuleId, "ReservationInterval", totalMinutes.ToString());
					moduleController.UpdateTabModuleSetting(base.TabModuleId, "ReservationDuration", totalMinutes1.ToString());
					moduleController.UpdateTabModuleSetting(base.TabModuleId, "ReservationDurationMax", num1.ToString());
					moduleController.UpdateTabModuleSetting(base.TabModuleId, "ReservationDurationInterval", totalMinutes2.ToString());
					moduleController.UpdateTabModuleSetting(base.TabModuleId, "MinTimeAhead", timeSpan.ToString());
					moduleController.UpdateTabModuleSetting(base.TabModuleId, "DaysAhead", this.daysAheadTextBox.Text);
					moduleController.UpdateTabModuleSetting(base.TabModuleId, "MaxReservationsPerTimeSlot", this.maxConflictingReservationsTextBox.Text);
					moduleController.UpdateTabModuleSetting(base.TabModuleId, "MaxReservationsPerUser", (string.IsNullOrEmpty(this.maxReservationsPerUserTextBox.Text.Trim()) ? Null.NullInteger.ToString() : this.maxReservationsPerUserTextBox.Text.Trim()));
					ModuleController.SynchronizeModule(base.ModuleId);
					this._ModuleSettings = null;
				}
				this.BindCategoriesDropDownList(this.reservationSettingsCategoryDropDownList, "AllowCancellations", null, null);
				this.BindReservationSettingsSection(num);
			}
		}

		public override void UpdateSettings()
		{
			int num;
			int num1;
			int num2;
			int num3;
			try
			{
				if (this.Page.IsValid)
				{
					ModuleController moduleController = new ModuleController();
					this.UpdateCategoryPermissionsSection(false);
					this.UpdateReservationSettingsSection(false);
					if (this.IsProfessional)
					{
						this.UpdateReservationFeesSection();
						this.UpdateCashierListSection(false);
					}
					this.UpdateWorkingHoursSection();
					this.UpdateWorkingHoursExceptionsSection(false);
					this.UpdateTimeOfDaySection();
					this.UpdateBCCListSection(false);
					this.UpdateModerationSection(false);
					this.UpdateViewReservationsSection();
					this.UpdateDuplicateReservationsSection();
					moduleController.UpdateTabModuleSetting(base.TabModuleId, "TimeZone", this.timeZoneDropDownList.SelectedValue);
					int tabModuleId = base.TabModuleId;
					bool @checked = this.allowCategorySelectionCheckBox.Checked;
					moduleController.UpdateTabModuleSetting(tabModuleId, "AllowCategorySelection", @checked.ToString());
					ModuleController moduleController1 = moduleController;
					int tabModuleId1 = base.TabModuleId;
					if (this.categorySelectionModeList.Checked)
					{
						num = 1;
					}
					else
					{
						num = (this.categorySelectionModeDropDownList.Checked ? 2 : 3);
					}
					moduleController1.UpdateTabModuleSetting(tabModuleId1, "CategorySelectionMode", num.ToString());
					int tabModuleId2 = base.TabModuleId;
					@checked = this.displayCalendarRadioButton.Checked;
					moduleController.UpdateTabModuleSetting(tabModuleId2, "DisplayCalendar", @checked.ToString());
					ModuleController moduleController2 = moduleController;
					int tabModuleId3 = base.TabModuleId;
					if (this.timeOfDaySelectionModeList.Checked)
					{
						num1 = 1;
					}
					else
					{
						num1 = (this.timeOfDaySelectionModeDropDownList.Checked ? 2 : 3);
					}
					moduleController2.UpdateTabModuleSetting(tabModuleId3, "TimeOfDaySelectionMode", num1.ToString());
					ModuleController moduleController3 = moduleController;
					int tabModuleId4 = base.TabModuleId;
					if (this.timeSelectionModeList.Checked)
					{
						num2 = 1;
					}
					else
					{
						num2 = (this.timeSelectionModeDropDownList.Checked ? 2 : 3);
					}
					moduleController3.UpdateTabModuleSetting(tabModuleId4, "TimeSelectionMode", num2.ToString());
					ModuleController moduleController4 = moduleController;
					int num4 = base.TabModuleId;
					if (this.durationSelectionModeList.Checked)
					{
						num3 = 1;
					}
					else
					{
						num3 = (this.durationSelectionModeDropDownList.Checked ? 2 : 3);
					}
					moduleController4.UpdateTabModuleSetting(num4, "DurationSelectionMode", num3.ToString());
					moduleController.UpdateTabModuleSetting(base.TabModuleId, "SelectCategoryLast", ((!this.allowCategorySelectionCheckBox.Checked ? false : this.selectCategoryLastCheckBox.Checked)).ToString());
					moduleController.UpdateTabModuleSetting(base.TabModuleId, "AllowCrossCategoryConflicts", ((!this.allowCategorySelectionCheckBox.Checked ? false : this.preventCrossCategoryConflictsCheckBox.Checked)).ToString());
					moduleController.UpdateTabModuleSetting(base.TabModuleId, "BindUponCategorySelection", ((!this.allowCategorySelectionCheckBox.Checked || this.selectCategoryLastCheckBox.Checked || this.preventCrossCategoryConflictsCheckBox.Checked ? false : this.bindUponSelectionCheckBox.Checked)).ToString());
					moduleController.UpdateTabModuleSetting(base.TabModuleId, "DisplayUnavailableCategories", ((!this.allowCategorySelectionCheckBox.Checked ? false : this.displayUnavailableCategoriesCheckBox.Checked)).ToString());
					int tabModuleId5 = base.TabModuleId;
					@checked = this.allowDescriptionCheckBox.Checked;
					moduleController.UpdateTabModuleSetting(tabModuleId5, "AllowDescription", @checked.ToString());
					int num5 = base.TabModuleId;
					@checked = this.allowSchedulingAnotherReservationCheckBox.Checked;
					moduleController.UpdateTabModuleSetting(num5, "AllowSchedulingAnotherReservation", @checked.ToString());
					int tabModuleId6 = base.TabModuleId;
					@checked = this.displayRemainingReservationsCheckBox.Checked;
					moduleController.UpdateTabModuleSetting(tabModuleId6, "DisplayRemainingReservations", @checked.ToString());
					int num6 = base.TabModuleId;
					@checked = this.contactInfoFirstCheckBox.Checked;
					moduleController.UpdateTabModuleSetting(num6, "ContactInfoFirst", @checked.ToString());
					int tabModuleId7 = base.TabModuleId;
					@checked = this.displayEndTimeCheckBox.Checked;
					moduleController.UpdateTabModuleSetting(tabModuleId7, "DisplayEndTime", @checked.ToString());
					moduleController.UpdateTabModuleSetting(base.TabModuleId, "ICalendarAttachmentFileName", this.iCalendarAttachmentFileNameTextBox.Text);
					moduleController.UpdateTabModuleSetting(base.TabModuleId, "Theme", this.themeDropDownList.SelectedValue);
					int num7 = base.TabModuleId;
					@checked = this.requireEmailCheckBox.Checked;
					moduleController.UpdateTabModuleSetting(num7, "RequireEmail", @checked.ToString());
					int tabModuleId8 = base.TabModuleId;
					@checked = this.requirePhoneCheckBox.Checked;
					moduleController.UpdateTabModuleSetting(tabModuleId8, "RequirePhone", @checked.ToString());
					int num8 = base.TabModuleId;
					@checked = this.allowLookupByPhoneCheckBox.Checked;
					moduleController.UpdateTabModuleSetting(num8, "AllowLookupByPhone", @checked.ToString());
					moduleController.UpdateTabModuleSetting(base.TabModuleId, "RedirectUrl", this.redirectUrlTextBox.Text);
					int tabModuleId9 = base.TabModuleId;
					@checked = this.requireVerificationCodeCheckBox.Checked;
					moduleController.UpdateTabModuleSetting(tabModuleId9, "RequireVerificationCode", @checked.ToString());
					int num9 = base.TabModuleId;
					@checked = this.skipContactInfoCheckBox.Checked;
					moduleController.UpdateTabModuleSetting(num9, "SkipContactInfoForAuthenticatedUsers", @checked.ToString());
					int tabModuleId10 = base.TabModuleId;
					@checked = this.attachICalendarCheckBox.Checked;
					moduleController.UpdateTabModuleSetting(tabModuleId10, "AttachiCalendar", @checked.ToString());
					if (this.ModuleSettings.SendReminders && !this.sendRemindersCheckBox.Checked)
					{
						Controller.DisableSendReminder(base.TabModuleId);
					}
					int num10 = base.TabModuleId;
					@checked = this.sendRemindersCheckBox.Checked;
					moduleController.UpdateTabModuleSetting(num10, "SendReminders", @checked.ToString());
					int tabModuleId11 = base.TabModuleId;
					TimeSpan timeSpan = this.GetTimeSpan(this.sendRemindersWhenTextBox, this.sendRemindersWhenDropDownList);
					moduleController.UpdateTabModuleSetting(tabModuleId11, "SendRemindersWhen", timeSpan.ToString());
					moduleController.UpdateTabModuleSetting(base.TabModuleId, "SendRemindersVia", this.sendRemindersViaDropDownList.SelectedValue);
					bool flag = (!this.sendRemindersCheckBox.Checked ? false : this.requireConfirmationCheckBox.Checked);
					if (this.ModuleSettings.RequireConfirmation && !flag)
					{
						Controller.DisableRequireConfirmation(base.TabModuleId);
					}
					moduleController.UpdateTabModuleSetting(base.TabModuleId, "RequireConfirmation", flag.ToString());
					int num11 = base.TabModuleId;
					timeSpan = this.GetTimeSpan(this.requireConfirmationWhenTextBox, this.requireConfirmationWhenDropDownList);
					moduleController.UpdateTabModuleSetting(num11, "RequireConfirmationWhen", timeSpan.ToString());
					if (!base.TabModuleSettings.ContainsKey("VerificationCodeSalt"))
					{
						int tabModuleId12 = base.TabModuleId;
						Guid guid = Guid.NewGuid();
						moduleController.UpdateTabModuleSetting(tabModuleId12, "VerificationCodeSalt", guid.ToString());
					}
					ModuleController.SynchronizeModule(base.ModuleId);
				}
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
			}
		}

		protected void UpdateSettingsCommandButtonClicked(object sender, EventArgs e)
		{
			if (this.Page.IsValid)
			{
				this.UpdateSettings();
				this.CancelSettingsCommandButtonClicked(sender, e);
			}
		}

		protected void UpdateSMSTemplateCommandButtonClicked(object sender, EventArgs e)
		{
			ModuleController moduleController = new ModuleController();
			if (this.smsTemplateDropDownList.SelectedValue == "Reminder")
			{
				moduleController.UpdateTabModuleSetting(base.TabModuleId, "ReminderSMS", this.smsTemplateBodyTextBox.Text);
			}
			moduleController.UpdateTabModuleSetting(base.TabModuleId, "TwilioAccountSID", this.twilioAccountSIDTextBox.Text);
			moduleController.UpdateTabModuleSetting(base.TabModuleId, "TwilioAuthToken", this.twilioAuthTokenTextBox.Text);
			moduleController.UpdateTabModuleSetting(base.TabModuleId, "TwilioFrom", this.twilioFromTextBox.Text);
			ModuleController.SynchronizeModule(base.ModuleId);
			this._ModuleSettings = null;
		}

		protected void UpdateTimeOfDaySection()
		{
			if (this.Page.IsValid)
			{
				ModuleController moduleController = new ModuleController();
				int tabModuleId = base.TabModuleId;
				bool @checked = this.displayTimeOfDayCheckBox.Checked;
				moduleController.UpdateTabModuleSetting(tabModuleId, "DisplayTimeOfDay", @checked.ToString());
				int num = base.TabModuleId;
				@checked = this.displayUnavailableTimeOfDayCheckBox.Checked;
				moduleController.UpdateTabModuleSetting(num, "DisplayUnavailableTimeOfDay", @checked.ToString());
				moduleController.UpdateTabModuleSetting(base.TabModuleId, "TimesOfDay", this.ModuleSettings.SerializeTimeOfDayList(this.TimeOfDayList));
				ModuleController.SynchronizeModule(base.ModuleId);
				this._ModuleSettings = null;
			}
		}

		protected void UpdateViewReservationsSection()
		{
			if (this.Page.IsValid)
			{
				(new ModuleController()).UpdateTabModuleSetting(base.TabModuleId, "ViewReservationsList", this.ModuleSettings.SerializeUserIDList(this.ViewReservationsList));
				ModuleController.SynchronizeModule(base.ModuleId);
				this._ModuleSettings = null;
			}
		}

		protected void UpdateWorkingHoursExceptionsSection(bool updateSelectedCategorySettings)
		{
			DateTime dateTime;
			TimeSpan endTime;
			string str;
			if (this.Page.IsValid)
			{
				int num = (this.workingHoursExceptionsCategoryDropDownList.SelectedValue == null || !(this.workingHoursExceptionsCategoryDropDownList.SelectedValue != string.Empty) ? Null.NullInteger : int.Parse(this.workingHoursExceptionsCategoryDropDownList.SelectedValue));
				ModuleController moduleController = null;
				CategorySettingController categorySettingController = null;
				Hashtable settings = null;
				if (num != Null.NullInteger)
				{
					if (!updateSelectedCategorySettings)
					{
						return;
					}
					categorySettingController = new CategorySettingController();
					settings = (new CategorySettings(base.PortalId, base.TabModuleId, num)).Settings;
				}
				else
				{
					moduleController = new ModuleController();
					settings = this.ModuleSettings.Settings;
				}
				foreach (string key in settings.Keys)
				{
					if (!DateTime.TryParse(key, CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.None, out dateTime))
					{
						continue;
					}
					if (moduleController == null)
					{
						categorySettingController.DeleteCategorySetting(num, key);
					}
					else
					{
						moduleController.DeleteTabModuleSetting(base.TabModuleId, key);
					}
				}
				if (this.WorkingHoursExceptions.Count > 0)
				{
					Hashtable hashtables = new Hashtable();
					foreach (WorkingHoursExceptionInfo workingHoursException in this.WorkingHoursExceptions)
					{
						if (!hashtables.Contains(workingHoursException.Date))
						{
							if (workingHoursException.AllDay)
							{
								str = "All Day";
							}
							else if (workingHoursException.StartTime == workingHoursException.EndTime)
							{
								str = "None";
							}
							else
							{
								string str1 = workingHoursException.StartTime.ToString();
								endTime = workingHoursException.EndTime;
								str = string.Concat(str1, "-", endTime.ToString());
							}
							hashtables.Add(workingHoursException.Date, str);
						}
						else
						{
							string item = (string)hashtables[workingHoursException.Date];
							string[] strArrays = new string[] { item, ";", null, null, null };
							endTime = workingHoursException.StartTime;
							strArrays[2] = endTime.ToString();
							strArrays[3] = "-";
							endTime = workingHoursException.EndTime;
							strArrays[4] = endTime.ToString();
							item = string.Concat(strArrays);
							hashtables[workingHoursException.Date] = item;
						}
					}
					foreach (DateTime key1 in hashtables.Keys)
					{
						if (moduleController == null)
						{
							categorySettingController.UpdateCategorySetting(num, key1.ToString("d", CultureInfo.InvariantCulture), (string)hashtables[key1]);
						}
						else
						{
							moduleController.UpdateTabModuleSetting(base.TabModuleId, key1.ToString("d", CultureInfo.InvariantCulture), (string)hashtables[key1]);
						}
					}
				}
				if (categorySettingController == null)
				{
					ModuleController.SynchronizeModule(base.ModuleId);
					this._ModuleSettings = null;
				}
				else
				{
					categorySettingController.UpdateCategorySetting(num, "WorkingHoursExceptionsDefined", bool.TrueString);
				}
				this.BindCategoriesDropDownList(this.workingHoursExceptionsCategoryDropDownList, "WorkingHoursExceptionsDefined", null, null);
				this.BindWorkingHoursExceptionsSection(num);
			}
		}

		protected void UpdateWorkingHoursSection()
		{
			if (this.Page.IsValid)
			{
				int num = (this.workingHoursCategoryDropDownList.SelectedValue == null || !(this.workingHoursCategoryDropDownList.SelectedValue != string.Empty) ? Null.NullInteger : int.Parse(this.workingHoursCategoryDropDownList.SelectedValue));
				ModuleController moduleController = null;
				CategorySettingController categorySettingController = null;
				if (num != Null.NullInteger)
				{
					categorySettingController = new CategorySettingController();
				}
				else
				{
					moduleController = new ModuleController();
				}
				int num1 = 1;
				foreach (RecurrencePattern workingHour in this.WorkingHours)
				{
					if (moduleController == null)
					{
						categorySettingController.UpdateCategorySetting(num, string.Concat("WorkingHours.", num1), Helper.SerializeRecurrencePattern(workingHour));
					}
					else
					{
						moduleController.UpdateTabModuleSetting(base.TabModuleId, string.Concat("WorkingHours.", num1), Helper.SerializeRecurrencePattern(workingHour));
					}
					num1++;
				}
				if (num != Null.NullInteger && this.WorkingHours.Count == 0)
				{
					categorySettingController.UpdateCategorySetting(num, "WorkingHours.1", string.Empty);
					num1++;
				}
				Hashtable settings = null;
				if (moduleController == null)
				{
					settings = (new CategorySettings(base.PortalId, base.TabModuleId, num)).Settings;
					if (!settings.ContainsKey("WorkingHours.1"))
					{
						settings = this.ModuleSettings.Settings;
					}
				}
				else
				{
					settings = this.ModuleSettings.Settings;
				}
				while (settings.ContainsKey(string.Concat("WorkingHours.", num1)))
				{
					if (moduleController == null)
					{
						categorySettingController.DeleteCategorySetting(num, string.Concat("WorkingHours.", num1));
					}
					else
					{
						moduleController.DeleteTabModuleSetting(base.TabModuleId, string.Concat("WorkingHours.", num1));
					}
					num1++;
				}
				if (moduleController != null)
				{
					ModuleController.SynchronizeModule(base.ModuleId);
					this._ModuleSettings = null;
				}
				this.BindCategoriesDropDownList(this.workingHoursCategoryDropDownList, "WorkingHours.1", null, null);
				this.BindWorkingHoursSection(num);
			}
		}

		protected void ValidateAuthorizeNetApiLogin(object sender, ServerValidateEventArgs e)
		{
			e.IsValid = (this.authorizeNetApiLoginTextBox.Text != string.Empty ? true : this.feeschedulecontrol.GetFeeScheduleType() == FeeScheduleType.Free);
		}

		protected void ValidateAuthorizeNetMerchantHash(object sender, ServerValidateEventArgs e)
		{
			e.IsValid = (this.authorizeNetMerchantHashTextBox.Text != string.Empty ? true : this.feeschedulecontrol.GetFeeScheduleType() == FeeScheduleType.Free);
		}

		protected void ValidateAuthorizeNetTransactionKey(object sender, ServerValidateEventArgs e)
		{
			e.IsValid = (this.authorizeNetTransactionKeyTextBox.Text != string.Empty ? true : this.feeschedulecontrol.GetFeeScheduleType() == FeeScheduleType.Free);
		}

		protected void ValidateModerationHours(object sender, ServerValidateEventArgs e)
		{
			e.IsValid = true;
			DayOfWeek dayOfWeek = (DayOfWeek)int.Parse(this.moderationWeekDaysDropDownList.SelectedValue);
			TimeSpan time = this.GetTime(this.moderationStartHourDropDownList, this.moderationStartMinuteDropDownList, this.moderationStartAMPMDropDownList);
			TimeSpan timeSpan = this.GetTime(this.moderationEndHourDropDownList, this.moderationEndMinuteDropDownList, this.moderationEndAMPMDropDownList);
			if (timeSpan == new TimeSpan())
			{
				timeSpan = new TimeSpan(1, 0, 0, 0);
			}
			if (timeSpan <= time)
			{
				e.IsValid = false;
			}
			else
			{
				foreach (WorkingHoursInfo moderationHour in this.ModerationHours)
				{
					if (moderationHour.DayOfWeek != dayOfWeek || (!(time <= moderationHour.StartTime) || !(timeSpan > moderationHour.StartTime)) && (!(moderationHour.StartTime <= time) || !(moderationHour.EndTime > time)))
					{
						continue;
					}
					e.IsValid = false;
					return;
				}
			}
		}

		protected void ValidatePayPalAccount(object sender, ServerValidateEventArgs e)
		{
			e.IsValid = (this.payPalAccountTextBox.Text != string.Empty ? true : this.feeschedulecontrol.GetFeeScheduleType() == FeeScheduleType.Free);
		}

		protected void ValidatePayPalUrl(object sender, ServerValidateEventArgs e)
		{
			e.IsValid = (this.payPalUrlTextBox.Text != string.Empty ? true : this.feeschedulecontrol.GetFeeScheduleType() == FeeScheduleType.Free);
		}

		protected void ValidateReservationDuration(object sender, ServerValidateEventArgs e)
		{
			e.IsValid = true;
		}

		protected void ValidateReservationDurationInterval(object sender, ServerValidateEventArgs e)
		{
			e.IsValid = true;
		}

		protected void ValidateReservationDurationMax(object sender, ServerValidateEventArgs e)
		{
			e.IsValid = true;
		}

		protected void ValidateReservationInterval(object sender, ServerValidateEventArgs e)
		{
			int num;
			if (!int.TryParse(this.reservationIntervalTextBox.Text, out num))
			{
				e.IsValid = false;
				return;
			}
			TimeSpan timeSpan = this.GetTimeSpan(this.reservationIntervalTextBox, this.reservationIntervalDropDownList);
			e.IsValid = timeSpan.TotalDays <= 1;
		}

		protected void ValidateTimeOfDay(object sender, ServerValidateEventArgs e)
		{
			string text = this.timeOfDayNameTextBox.Text;
			TimeSpan time = this.GetTime(this.timeOfDayStartHourDropDownList, this.timeOfDayStartMinuteDropDownList, this.timeOfDayStartAMPMDropDownList);
			TimeSpan timeSpan = this.GetTime(this.timeOfDayEndHourDropDownList, this.timeOfDayEndMinuteDropDownList, this.timeOfDayEndAMPMDropDownList);
			TimeSpan timeSpan1 = new TimeSpan();
			if (timeSpan == timeSpan1)
			{
				timeSpan = timeSpan.Add(new TimeSpan(1, 0, 0, 0));
			}
			if (time >= timeSpan)
			{
				timeSpan1 = new TimeSpan();
				if (timeSpan != timeSpan1)
				{
					e.IsValid = false;
					return;
				}
			}
			foreach (TimeOfDayInfo timeOfDayList in this.TimeOfDayList)
			{
				if ((!(timeOfDayList.StartTime <= time) || !(timeOfDayList.EndTime > time)) && (!(time <= timeOfDayList.StartTime) || !(timeSpan > timeOfDayList.StartTime)))
				{
					if (timeOfDayList.Name.ToLower().Trim() != text.ToLower().Trim())
					{
						continue;
					}
					e.IsValid = false;
					return;
				}
				else
				{
					e.IsValid = false;
					return;
				}
			}
		}

		protected bool ValidateTimeOfDayList()
		{
			TimeSpan timeSpan = new TimeSpan();
			foreach (TimeOfDayInfo timeOfDayList in this.TimeOfDayList)
			{
				TimeSpan endTime = timeOfDayList.EndTime;
				timeSpan = timeSpan.Add(endTime.Subtract(timeOfDayList.StartTime));
			}
			return timeSpan.TotalDays == 1;
		}

		protected void ValidateTimeOfDayList(object sender, ServerValidateEventArgs e)
		{
			e.IsValid = this.ValidateTimeOfDayList();
			this.timeOfDaySectionHead.IsExpanded = (this.timeOfDaySectionHead.IsExpanded ? true : !e.IsValid);
		}

		protected void ValidateWorkingHoursException(object sender, ServerValidateEventArgs e)
		{
			DateTime dateTime;
			TimeSpan timeSpan;
			e.IsValid = true;
			if (!DateTime.TryParse(this.workingHoursExceptionDateTextBox.Text, out dateTime))
			{
				this.HighlightInvalidControl(this.workingHoursExceptionDateTextBox);
				e.IsValid = false;
			}
			else if (!this.workingHoursExceptionNoWorkingHoursRadioButton.Checked)
			{
				TimeSpan time = this.GetTime(this.workingHoursExceptionStartHourDropDownList, this.workingHoursExceptionStartMinuteDropDownList, this.workingHoursExceptionStartAMPMDropDownList);
				TimeSpan time1 = this.GetTime(this.workingHoursExceptionEndHourDropDownList, this.workingHoursExceptionEndMinuteDropDownList, this.workingHoursExceptionEndAMPMDropDownList);
				timeSpan = new TimeSpan();
				if (time1 == timeSpan)
				{
					time1 = new TimeSpan(1, 0, 0, 0);
				}
				if (time1 <= time)
				{
					this.HighlightInvalidControl(this.workingHoursExceptionStartHourDropDownList);
					this.HighlightInvalidControl(this.workingHoursExceptionStartMinuteDropDownList);
					this.HighlightInvalidControl(this.workingHoursExceptionStartAMPMDropDownList);
					this.HighlightInvalidControl(this.workingHoursExceptionEndHourDropDownList);
					this.HighlightInvalidControl(this.workingHoursExceptionEndMinuteDropDownList);
					this.HighlightInvalidControl(this.workingHoursExceptionEndAMPMDropDownList);
					e.IsValid = false;
				}
				else
				{
					foreach (WorkingHoursExceptionInfo workingHoursException in this.WorkingHoursExceptions)
					{
						if (!(workingHoursException.Date == dateTime) || (!(time <= workingHoursException.StartTime) || !(time1 > workingHoursException.StartTime)) && (!(workingHoursException.StartTime <= time) || !(workingHoursException.EndTime > time)))
						{
							continue;
						}
						this.HighlightInvalidControl(this.workingHoursExceptionStartHourDropDownList);
						this.HighlightInvalidControl(this.workingHoursExceptionStartMinuteDropDownList);
						this.HighlightInvalidControl(this.workingHoursExceptionStartAMPMDropDownList);
						this.HighlightInvalidControl(this.workingHoursExceptionEndHourDropDownList);
						this.HighlightInvalidControl(this.workingHoursExceptionEndMinuteDropDownList);
						this.HighlightInvalidControl(this.workingHoursExceptionEndAMPMDropDownList);
						e.IsValid = false;
						return;
					}
				}
			}
			else
			{
				foreach (WorkingHoursExceptionInfo workingHoursExceptionInfo in this.WorkingHoursExceptions)
				{
					if (workingHoursExceptionInfo.Date != dateTime)
					{
						continue;
					}
					TimeSpan startTime = workingHoursExceptionInfo.StartTime;
					timeSpan = new TimeSpan();
					if (startTime != timeSpan)
					{
						continue;
					}
					TimeSpan endTime = workingHoursExceptionInfo.EndTime;
					timeSpan = new TimeSpan();
					if (endTime != timeSpan)
					{
						continue;
					}
					this.HighlightInvalidControl(this.workingHoursExceptionDateTextBox);
					e.IsValid = false;
					return;
				}
			}
			if (e.IsValid)
			{
				this.UnhighlightInvalidControl(this.workingHoursExceptionDateTextBox);
				this.UnhighlightInvalidControl(this.workingHoursExceptionStartHourDropDownList);
				this.UnhighlightInvalidControl(this.workingHoursExceptionStartMinuteDropDownList);
				this.UnhighlightInvalidControl(this.workingHoursExceptionStartAMPMDropDownList);
				this.UnhighlightInvalidControl(this.workingHoursExceptionEndHourDropDownList);
				this.UnhighlightInvalidControl(this.workingHoursExceptionEndMinuteDropDownList);
				this.UnhighlightInvalidControl(this.workingHoursExceptionEndAMPMDropDownList);
			}
		}

		protected void ValidateWorkingHoursExceptionDate(object sender, ServerValidateEventArgs e)
		{
			DateTime dateTime;
			e.IsValid = DateTime.TryParse(e.Value, out dateTime);
			if (!e.IsValid)
			{
				this.HighlightInvalidControl(this.workingHoursExceptionDateTextBox);
				return;
			}
			this.UnhighlightInvalidControl(this.workingHoursExceptionDateTextBox);
		}

		protected void WorkingHoursCategoryDropDownListSelectedIndexChanged(object sender, EventArgs e)
		{
			this.BindWorkingHoursSection(int.Parse(this.workingHoursCategoryDropDownList.SelectedValue));
		}

		protected void WorkingHoursExceptionsCategoryDropDownListSelectedIndexChanged(object sender, EventArgs e)
		{
			this.BindWorkingHoursExceptionsSection(int.Parse(this.workingHoursExceptionsCategoryDropDownList.SelectedValue));
		}

		protected void WorkingHoursExceptionsResetCommandButtonClicked(object sender, EventArgs e)
		{
			DateTime dateTime;
			int num = int.Parse(this.workingHoursExceptionsCategoryDropDownList.SelectedValue);
			CategorySettingController categorySettingController = new CategorySettingController();
			foreach (string key in (new CategorySettings(base.PortalId, base.TabModuleId, num)).Settings.Keys)
			{
				if (!DateTime.TryParse(key, CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.None, out dateTime))
				{
					continue;
				}
				categorySettingController.DeleteCategorySetting(num, key);
			}
			categorySettingController.DeleteCategorySetting(num, "WorkingHoursExceptionsDefined");
			this.BindCategoriesDropDownList(this.workingHoursExceptionsCategoryDropDownList, "WorkingHoursExceptionsDefined", null, null);
			this.BindWorkingHoursExceptionsSection(num);
		}

		protected void WorkingHoursExceptionsUpdateCommandButtonClicked(object sender, EventArgs e)
		{
			this.UpdateWorkingHoursExceptionsSection(true);
		}

		protected void WorkingHoursResetCommandButtonClicked(object sender, EventArgs e)
		{
			int num = int.Parse(this.workingHoursCategoryDropDownList.SelectedValue);
			CategorySettingController categorySettingController = new CategorySettingController();
			Hashtable settings = (new CategorySettings(base.PortalId, base.TabModuleId, num)).Settings;
			for (int i = 1; settings.ContainsKey(string.Concat("WorkingHours.", i)); i++)
			{
				categorySettingController.DeleteCategorySetting(num, string.Concat("WorkingHours.", i));
			}
			this.BindCategoriesDropDownList(this.workingHoursCategoryDropDownList, "WorkingHours.1", null, null);
			this.BindWorkingHoursSection(num);
		}

		protected void WorkingHoursUpdateCommandButtonClicked(object sender, EventArgs e)
		{
			this.UpdateWorkingHoursSection();
		}

		protected void Page_PreRender(object sender, EventArgs e)
		{
			System.Web.UI.HtmlControls.HtmlGenericControl css = (System.Web.UI.HtmlControls.HtmlGenericControl)Page.Header.FindControl("ComponentStyleAutoComplete");
			if (css == null)
			{
				css = new System.Web.UI.HtmlControls.HtmlGenericControl("link")
				{
					ID = "ComponentStyleAutoComplete"
				};
				css.Attributes.Add("type", "text/css");
				css.Attributes.Add("rel", "stylesheet");
				css.Attributes.Add("href", ControlPath + "jquery.auto-complete.css");
				this.Page.Header.Controls.Add(css);
			}
			System.Web.UI.HtmlControls.HtmlGenericControl script = (System.Web.UI.HtmlControls.HtmlGenericControl)Page.Header.FindControl("ComponentScriptAutoComplete");
			if (script == null)
			{
				script = new System.Web.UI.HtmlControls.HtmlGenericControl("script")
				{
					ID = "ComponentScriptAutoComplete"
				};
				script.Attributes.Add("language", "javascript");
				script.Attributes.Add("type", "text/javascript");
				script.Attributes.Add("src", ControlPath + "jquery.auto-complete.js");
				this.Page.Header.Controls.Add(script);
			}
			System.Web.UI.HtmlControls.HtmlGenericControl literal = (System.Web.UI.HtmlControls.HtmlGenericControl)Page.Header.FindControl("ComponentScriptReservations");
			if (literal == null)
			{
				literal = new System.Web.UI.HtmlControls.HtmlGenericControl("script")
				{
					ID = "ComponentScriptReservations"
				};
				literal.Attributes.Add("language", "javascript");
				literal.Attributes.Add("type", "text/javascript");
				System.Text.StringBuilder sb = new System.Text.StringBuilder();
				sb.AppendLine("jQuery(document).ready(function() {");
				sb.AppendLine("  var prm = Sys.WebForms.PageRequestManager.getInstance();");
				sb.AppendLine("  prm.add_endRequest(MyEndRequest);");
				sb.AppendLine("  initReservationsJavascript();");
				sb.AppendLine("});");
				sb.AppendLine("function MyEndRequest(sender, args) {");
				sb.AppendLine("  initReservationsJavascript();");
				sb.AppendLine("}");
				sb.AppendLine("function initReservationsJavascript() {");
				sb.AppendLine("  $('#" + usernameTextBox.ClientID + "').autoComplete({");
				sb.AppendLine("    source: function(term, response) { $.getJSON('" + ControlPath + "SearchUser.ashx', { q: term }, function(data) { response(data); }); },");
				sb.AppendLine("    cache: false,");
				sb.AppendLine("    minChars: 1,");
				sb.AppendLine("    onSelect: function(event, term, item) {");
				sb.AppendLine("      $('#" + usernameTextBox.ClientID + "').val(term);");
				sb.AppendLine("        " + Page.ClientScript.GetPostBackEventReference(addUser, String.Empty) + ";");
				sb.AppendLine("    }");
				sb.AppendLine("  });");

				sb.AppendLine("  $('#" + moderatorUsernameTextBox.ClientID + "').autoComplete({");
				sb.AppendLine("    source: function(term, response) { $.getJSON('" + ControlPath + "SearchUser.ashx', { q: term }, function(data) { response(data); }); },");
				sb.AppendLine("    cache: false,");
				sb.AppendLine("    minChars: 1,");
				sb.AppendLine("    onSelect: function(event, term, item) {");
				sb.AppendLine("      $('#" + moderatorUsernameTextBox.ClientID + "').val(term);");
				sb.AppendLine("        " + Page.ClientScript.GetPostBackEventReference(AddGlobalModerator, String.Empty) + ";");
				sb.AppendLine("    }");
				sb.AppendLine("  });");

				sb.AppendLine("  $('#" + duplicateReservationsUsernameTextBox.ClientID + "').autoComplete({");
				sb.AppendLine("    source: function(term, response) { $.getJSON('" + ControlPath + "SearchUser.ashx', { q: term }, function(data) { response(data); }); },");
				sb.AppendLine("    cache: false,");
				sb.AppendLine("    minChars: 1,");
				sb.AppendLine("    onSelect: function(event, term, item) {");
				sb.AppendLine("      $('#" + duplicateReservationsUsernameTextBox.ClientID + "').val(term);");
				sb.AppendLine("        " + Page.ClientScript.GetPostBackEventReference(AddDuplicateReservations, String.Empty) + ";");
				sb.AppendLine("    }");
				sb.AppendLine("  });");

				sb.AppendLine("  $('#" + viewReservationsUsernameTextBox.ClientID + "').autoComplete({");
				sb.AppendLine("    source: function(term, response) { $.getJSON('" + ControlPath + "SearchUser.ashx', { q: term }, function(data) { response(data); }); },");
				sb.AppendLine("    cache: false,");
				sb.AppendLine("    minChars: 1,");
				sb.AppendLine("    onSelect: function(event, term, item) {");
				sb.AppendLine("      $('#" + viewReservationsUsernameTextBox.ClientID + "').val(term);");
				sb.AppendLine("        " + Page.ClientScript.GetPostBackEventReference(AddViewReservations, String.Empty) + ";");
				sb.AppendLine("    }");
				sb.AppendLine("  });");

				sb.AppendLine("  $('#" + cashierListUsernameTextBox.ClientID + "').autoComplete({");
				sb.AppendLine("    source: function(term, response) { $.getJSON('" + ControlPath + "SearchUser.ashx', { q: term }, function(data) { response(data); }); },");
				sb.AppendLine("    cache: false,");
				sb.AppendLine("    minChars: 1,");
				sb.AppendLine("    onSelect: function(event, term, item) {");
				sb.AppendLine("      $('#" + cashierListUsernameTextBox.ClientID + "').val(term);");
				sb.AppendLine("        " + Page.ClientScript.GetPostBackEventReference(AddCashier, String.Empty) + ";");
				sb.AppendLine("    }");
				sb.AppendLine("  });");

				sb.AppendLine("}");
				literal.InnerHtml = sb.ToString();
				this.Page.Header.Controls.Add(literal);
			}
		}
	}
}