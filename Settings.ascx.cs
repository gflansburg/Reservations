using DotNetNuke.Abstractions;
using DotNetNuke.Common;
using DotNetNuke.Common.Lists;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.UserControls;
using Microsoft.Extensions.DependencyInjection;
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

		private readonly INavigationManager _navigationManager;

		public Settings()
		{
			_navigationManager = DependencyProvider.GetRequiredService<INavigationManager>();
		}

		private ArrayList BCCList
		{
			get
			{
				if (_BCCList == null)
				{
					if (ViewState["BCCList"] != null)
					{
						//_BCCList = ModuleSettings.DeserializeUserIDList((string)ViewState["BCCList"]);
						_BCCList = ModuleSettings.DeserializeEmailList((string)ViewState["BCCList"]);
					}
					else
					{
						int num = (!IsPostBack || bccListCategoryDropDownList.SelectedValue == null || !(bccListCategoryDropDownList.SelectedValue != string.Empty) ? Null.NullInteger : int.Parse(bccListCategoryDropDownList.SelectedValue));
						if (num != Null.NullInteger)
						{
							_BCCList = (new CategorySettings(PortalId, TabModuleId, num)).BCCList;
						}
						else
						{
							_BCCList = ModuleSettings.BCCList;
						}
					}
				}
				return _BCCList;
			}
		}

		private ArrayList CashierList
		{
			get
			{
				if (_CashierList == null)
				{
					if (ViewState["CashierList"] != null)
					{
						//_CashierList = ModuleSettings.DeserializeUserIDList((string)ViewState["CashierList"]);
						_CashierList = ModuleSettings.DeserializeEmailList((string)ViewState["CashierList"]);
					}
					else
					{
						int num = (!IsPostBack || cashierListCategoryDropDownList.SelectedValue == null || !(cashierListCategoryDropDownList.SelectedValue != string.Empty) ? Null.NullInteger : int.Parse(cashierListCategoryDropDownList.SelectedValue));
						if (num != Null.NullInteger)
						{
							_CashierList = (new CategorySettings(PortalId, TabModuleId, num)).CashierList;
						}
						else
						{
							_CashierList = ModuleSettings.CashierList;
						}
					}
				}
				return _CashierList;
			}
		}

		private List<CategoryInfo> CategoryList
		{
			get
			{
				if (_CategoryList == null)
				{
					if (ViewState["CategoryList"] != null)
					{
						_CategoryList = DeserializeCategoryList((string)ViewState["CategoryList"]);
					}
					else
					{
						_CategoryList = (new CategoryController()).GetCategoryList(TabModuleId);
						_CategoryList.Sort((CategoryInfo x, CategoryInfo y) => x.Name.CompareTo(y.Name));
					}
				}
				return _CategoryList;
			}
		}

		private ArrayList CategoryPermissionsList
		{
			get
			{
				if (_CategoryPermissionsList == null)
				{
					if (ViewState["CategoryPermissionsList"] != null)
					{
						_CategoryPermissionsList = ModuleSettings.DeserializeRoleIDList((string)ViewState["CategoryPermissionsList"]);
					}
					else
					{
						int num = (!IsPostBack || categoryPermissionsDropDownList.SelectedValue == null || !(categoryPermissionsDropDownList.SelectedValue != string.Empty) ? Null.NullInteger : int.Parse(categoryPermissionsDropDownList.SelectedValue));
						if (num != Null.NullInteger)
						{
							_CategoryPermissionsList = (new CategorySettings(PortalId, TabModuleId, num)).CategoryPermissionsList;
						}
						else
						{
							_CategoryPermissionsList = ModuleSettings.CategoryPermissionsList;
						}
					}
				}
				return _CategoryPermissionsList;
			}
		}

		private ArrayList DuplicateReservationsList
		{
			get
			{
				if (_DuplicateReservationsList == null)
				{
					if (ViewState["DuplicateReservationsList"] != null)
					{
						//_DuplicateReservationsList = ModuleSettings.DeserializeUserIDList((string)ViewState["DuplicateReservationsList"]);
						_DuplicateReservationsList = ModuleSettings.DeserializeEmailList((string)ViewState["DuplicateReservationsList"]);
					}
					else
					{
						_DuplicateReservationsList = ModuleSettings.DuplicateReservationsList;
					}
				}
				return _DuplicateReservationsList;
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
				if (!_IsProfesional.HasValue)
				{
					_IsProfesional = new bool?(Helper.GetEdition(ModuleSettings.ActivationCode) != "Standard");
				}
				return _IsProfesional.Value;
			}
		}

		private ArrayList ModerationHours
		{
			get
			{
				if (_ModerationHours == null)
				{
					if (ViewState["ModerationHours"] != null)
					{
						_ModerationHours = DeserializeWorkingHours((string)ViewState["ModerationHours"]);
					}
					else
					{
						_ModerationHours = new ArrayList();
						int num = (!IsPostBack || moderationCategoryDropDownList.SelectedValue == null || !(moderationCategoryDropDownList.SelectedValue != string.Empty) ? Null.NullInteger : int.Parse(moderationCategoryDropDownList.SelectedValue));
						Hashtable settings = null;
						if (num != Null.NullInteger)
						{
							settings = (new CategorySettings(PortalId, TabModuleId, num)).Settings;
							if (!settings.ContainsKey(string.Concat("Moderation.", DayOfWeek.Monday.ToString())))
							{
								settings = ModuleSettings.Settings;
							}
						}
						else
						{
							settings = ModuleSettings.Settings;
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
								_ModerationHours.Add(workingHoursInfo);
							}
						}
					}
				}
				return _ModerationHours;
			}
		}

		private ArrayList ModeratorList
		{
			get
			{
				if (_ModeratorList == null)
				{
					if (ViewState["ModeratorList"] != null)
					{
						//_ModeratorList = ModuleSettings.DeserializeUserIDList((string)ViewState["ModeratorList"]);
						_ModeratorList = ModuleSettings.DeserializeEmailList((string)ViewState["ModeratorList"]);
					}
					else
					{
						int num = (!IsPostBack || moderationCategoryDropDownList.SelectedValue == null || !(moderationCategoryDropDownList.SelectedValue != string.Empty) ? Null.NullInteger : int.Parse(moderationCategoryDropDownList.SelectedValue));
						if (num != Null.NullInteger)
						{
							_ModeratorList = (new CategorySettings(PortalId, TabModuleId, num)).ModeratorList;
						}
						else
						{
							_ModeratorList = ModuleSettings.ModeratorList;
						}
					}
				}
				return _ModeratorList;
			}
		}

		private new Gafware.Modules.Reservations.ModuleSettings ModuleSettings
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

		private ArrayList TimeOfDayList
		{
			get
			{
				if (_TimeOfDayList == null)
				{
					if (ViewState["TimeOfDayList"] != null)
					{
						_TimeOfDayList = ModuleSettings.DeserializeTimeOfDayList((string)ViewState["TimeOfDayList"]);
					}
					else
					{
						_TimeOfDayList = ModuleSettings.TimeOfDayList;
					}
				}
				return _TimeOfDayList;
			}
		}

		private ArrayList Users
		{
			get
			{
				if (_Users == null)
				{
					_Users = UserController.GetUsers(PortalId);
					foreach (UserInfo user in UserController.GetUsers(Null.NullInteger))
					{
						if (!user.IsSuperUser || FindUserInfoByUserId(_Users, user.UserID) != null)
						{
							continue;
						}
						_Users.Add(user);
					}
					_Users.Sort(new UserInfoComparer());
				}
				return _Users;
			}
		}

		private ArrayList ViewReservationsList
		{
			get
			{
				if (_ViewReservationsList == null)
				{
					if (ViewState["ViewReservationsList"] != null)
					{
						//_ViewReservationsList = ModuleSettings.DeserializeUserIDList((string)ViewState["ViewReservationsList"]);
						_ViewReservationsList = ModuleSettings.DeserializeEmailList((string)ViewState["ViewReservationsList"]);
					}
					else
					{
						_ViewReservationsList = ModuleSettings.ViewReservationsList;
					}
				}
				return _ViewReservationsList;
			}
		}

		private List<RecurrencePattern> WorkingHours
		{
			get
			{
				if (_WorkingHours == null)
				{
					if (ViewState["WorkingHours"] != null)
					{
						_WorkingHours = DeserializeRecurrencePatternList((string)ViewState["WorkingHours"]);
					}
					else
					{
						_WorkingHours = new List<RecurrencePattern>();
						int num = (!IsPostBack || workingHoursCategoryDropDownList.SelectedValue == null || !(workingHoursCategoryDropDownList.SelectedValue != string.Empty) ? Null.NullInteger : int.Parse(workingHoursCategoryDropDownList.SelectedValue));
						Hashtable settings = null;
						if (num != Null.NullInteger)
						{
							settings = (new CategorySettings(PortalId, TabModuleId, num)).Settings;
							if (!settings.ContainsKey("WorkingHours.1"))
							{
								settings = ModuleSettings.Settings;
							}
						}
						else
						{
							settings = ModuleSettings.Settings;
						}
						int num1 = 1;
						while (settings.ContainsKey(string.Concat("WorkingHours.", num1)))
						{
							if (!string.IsNullOrEmpty((string)settings[string.Concat("WorkingHours.", num1)]))
							{
								_WorkingHours.Add(Helper.DeserializeRecurrencePattern((string)settings[string.Concat("WorkingHours.", num1)]));
								num1++;
							}
							else
							{
								break;
							}
						}
					}
				}
				return _WorkingHours;
			}
		}

		private ArrayList WorkingHoursExceptions
		{
			get
			{
				if (_WorkingHoursExceptions == null)
				{
					if (ViewState["WorkingHoursExceptions"] != null)
					{
						_WorkingHoursExceptions = DeserializeWorkingHoursExceptions((string)ViewState["WorkingHoursExceptions"]);
					}
					else
					{
						int num = (!IsPostBack || workingHoursExceptionsCategoryDropDownList.SelectedValue == null || !(workingHoursExceptionsCategoryDropDownList.SelectedValue != string.Empty) ? Null.NullInteger : int.Parse(workingHoursExceptionsCategoryDropDownList.SelectedValue));
						if (num != Null.NullInteger)
						{
							CategorySettings categorySetting = new CategorySettings(PortalId, TabModuleId, num);
							if (!categorySetting.WorkingHoursExceptionsDefined)
							{
								_WorkingHoursExceptions = GetWorkingHoursExceptions(ModuleSettings.Settings);
							}
							else
							{
								_WorkingHoursExceptions = GetWorkingHoursExceptions(categorySetting.Settings);
							}
						}
						else
						{
							_WorkingHoursExceptions = GetWorkingHoursExceptions(ModuleSettings.Settings);
						}
					}
				}
				return _WorkingHoursExceptions;
			}
		}

		protected void AddCashierCommandButtonClicked(object sender, EventArgs e)
		{
			/*if (cashierListUsersDropDownList.Visible && cashierListUsersDropDownList.SelectedValue != "-1")
			{
				//UserInfo userInfo = FindUserInfoByUserId(Users, int.Parse(cashierListUsersDropDownList.SelectedValue));
				UserInfo userInfo = FindUserInfoByEmail(Users, cashierListUsersDropDownList.SelectedValue);
				//if (FindUserInfoByUserId(CashierList, int.Parse(cashierListUsersDropDownList.SelectedValue)) == null)
				if (FindUserInfoByEmail(CashierList, cashierListUsersDropDownList.SelectedValue) == null)
				{
					CashierList.Add(userInfo);
					BindCashierListDataGrid();
					BindUsersDropDownList(cashierListUsersDropDownList, CashierList);
					return;
				}
			}
			else if (cashierListUsernameTextBox.Visible)*/
			{
				UserInfo userByName = UserController.GetUserByName(PortalId, cashierListUsernameTextBox.Text);
				if (userByName == null && Helper.IsValidEmail2(cashierListUsernameTextBox.Text))
				{
					IList<UserInfo> users = (new UserController()).GetUsersBasicSearch(PortalId, 0, 1, "Email", true, "Email", cashierListUsernameTextBox.Text);
					if (users != null && users.Count > 0)
					{
						userByName = users[0];
					}
					else
					{
						userByName = new UserInfo();
						userByName.UserID = -1;
						userByName.Email = cashierListUsernameTextBox.Text;
						userByName.DisplayName = cashierListUsernameTextBox.Text;
					}
				}
				if (userByName == null)
				{
					IList<UserInfo> users = (new UserController()).GetUsersBasicSearch(PortalId, 0, 1, "DisplayName", true, "DisplayName", cashierListUsernameTextBox.Text);
					if (users != null && users.Count > 0)
					{
						userByName = (UserInfo)users[0];
					}
				}
				//if (userByName != null && FindUserInfoByUserId(CashierList, userByName.UserID) == null)
				if (userByName != null && FindUserInfoByEmail(CashierList, userByName.Email) == null)
				{
					CashierList.Add(userByName);
					BindCashierListDataGrid();
				}
				cashierListUsernameTextBox.Text = string.Empty;
			}
		}

		protected void AddCategoryCommandButtonClicked(object sender, EventArgs e)
		{
			if (Page.IsValid)
			{
				CategoryInfo categoryInfo = new CategoryInfo()
				{
					TabModuleID = TabModuleId,
					Name = categoryNameTextBox.Text.Trim(),
					CreatedByUserID = UserId,
					CreatedOnDate = DateTime.Now
				};
				categoryInfo = (new CategoryController()).AddCategory(categoryInfo);
				CategoryList.Add(categoryInfo);
				CategoryList.Sort((CategoryInfo x, CategoryInfo y) => x.Name.CompareTo(y.Name));
				categoryNameTextBox.Text = string.Empty;
				BindCategoryListDataGrid();
				RebindCategoryDependentSections();
			}
		}

		protected void AddDuplicateReservationsCommandButtonClicked(object sender, EventArgs e)
		{
			/*if (duplicateReservationsUsersDropDownList.Visible && duplicateReservationsUsersDropDownList.SelectedValue != "-1")
			{
				//UserInfo userInfo = FindUserInfoByUserId(Users, int.Parse(duplicateReservationsUsersDropDownList.SelectedValue));
				UserInfo userInfo = FindUserInfoByEmail(Users, duplicateReservationsUsersDropDownList.SelectedValue);
				//if (FindUserInfoByUserId(DuplicateReservationsList, int.Parse(duplicateReservationsUsersDropDownList.SelectedValue)) == null)
				if (FindUserInfoByEmail(DuplicateReservationsList, duplicateReservationsUsersDropDownList.SelectedValue) == null)
				{
					DuplicateReservationsList.Add(userInfo);
					BindDuplicateReservationsDataGrid();
					BindUsersDropDownList(duplicateReservationsUsersDropDownList, DuplicateReservationsList);
					return;
				}
			}
			else if (duplicateReservationsUsernameTextBox.Visible)*/
			{
				UserInfo userByName = UserController.GetUserByName(PortalId, duplicateReservationsUsernameTextBox.Text);
				if (userByName == null && Helper.IsValidEmail2(duplicateReservationsUsernameTextBox.Text))
				{
					IList<UserInfo> users = (new UserController()).GetUsersBasicSearch(PortalId, 0, 1, "Email", true, "Email", duplicateReservationsUsernameTextBox.Text);
					if (users != null && users.Count > 0)
					{
						userByName = users[0];
					}
					else
					{
						userByName = new UserInfo();
						userByName.UserID = -1;
						userByName.Email = duplicateReservationsUsernameTextBox.Text;
						userByName.DisplayName = duplicateReservationsUsernameTextBox.Text;
					}
				}
				if (userByName == null)
				{
					IList<UserInfo> users = (new UserController()).GetUsersBasicSearch(PortalId, 0, 1, "DisplayName", true, "DisplayName", duplicateReservationsUsernameTextBox.Text);
					if (users != null && users.Count > 0)
					{
						userByName = (UserInfo)users[0];
					}
				}
				//if (userByName != null && FindUserInfoByUserId(DuplicateReservationsList, userByName.UserID) == null)
				if (userByName != null && FindUserInfoByEmail(DuplicateReservationsList, userByName.Email) == null)
				{
					DuplicateReservationsList.Add(userByName);
					BindDuplicateReservationsDataGrid();
				}
				duplicateReservationsUsernameTextBox.Text = string.Empty;
			}
		}

		protected void AddGlobalModeratorCommandButtonClicked(object sender, EventArgs e)
		{
			/*if (moderatorUsersDropDownList.Visible && moderatorUsersDropDownList.SelectedValue != "-1")
			{
				//UserInfo userInfo = FindUserInfoByUserId(Users, int.Parse(moderatorUsersDropDownList.SelectedValue));
				UserInfo userInfo = FindUserInfoByEmail(Users, moderatorUsersDropDownList.SelectedValue);
				//if (FindUserInfoByUserId(ModeratorList, int.Parse(moderatorUsersDropDownList.SelectedValue)) == null)
				if (FindUserInfoByEmail(ModeratorList, moderatorUsersDropDownList.SelectedValue) == null)
				{
					ModeratorList.Add(userInfo);
					BindModeratorsDataGrid();
					BindUsersDropDownList(moderatorUsersDropDownList, ModeratorList);
					return;
				}
			}
			else if (moderatorUsernameTextBox.Visible)*/
			{
				UserInfo userByName = UserController.GetUserByName(PortalId, moderatorUsernameTextBox.Text);
				if (userByName == null && Helper.IsValidEmail2(moderatorUsernameTextBox.Text))
				{
					IList<UserInfo> users = (new UserController()).GetUsersBasicSearch(PortalId, 0, 1, "Email", true, "Email", moderatorUsernameTextBox.Text);
					if (users != null && users.Count > 0)
					{
						userByName = users[0];
					}
					else
					{
						userByName = new UserInfo();
						userByName.UserID = -1;
						userByName.Email = moderatorUsernameTextBox.Text;
						userByName.DisplayName = moderatorUsernameTextBox.Text;
					}
				}
				if (userByName == null)
				{
					IList<UserInfo> users = (new UserController()).GetUsersBasicSearch(PortalId, 0, 1, "DisplayName", true, "DisplayName", moderatorUsernameTextBox.Text);
					if (users != null && users.Count > 0)
					{
						userByName = (UserInfo)users[0];
					}
				}
				//if (userByName != null && FindUserInfoByUserId(ModeratorList, userByName.UserID) == null)
				if (userByName != null && FindUserInfoByEmail(ModeratorList, userByName.Email) == null)
				{
					ModeratorList.Add(userByName);
					BindModeratorsDataGrid();
				}
				moderatorUsernameTextBox.Text = string.Empty;
			}
		}

		protected void AddModerationHoursCommandButtonClicked(object sender, EventArgs e)
		{
			try
			{
				if (Page.IsValid)
				{
					WorkingHoursInfo workingHoursInfo = new WorkingHoursInfo()
					{
						DayOfWeek = (DayOfWeek)int.Parse(moderationWeekDaysDropDownList.SelectedValue)
					};
					for (int i = 0; i < ModerationHours.Count; i++)
					{
						if (((WorkingHoursInfo)ModerationHours[i]).DayOfWeek == workingHoursInfo.DayOfWeek && ((WorkingHoursInfo)ModerationHours[i]).AllDay)
						{
							ModerationHours.RemoveAt(i);
							i--;
						}
					}
					workingHoursInfo.StartTime = GetTime(moderationStartHourDropDownList, moderationStartMinuteDropDownList, moderationStartAMPMDropDownList);
					workingHoursInfo.EndTime = GetTime(moderationEndHourDropDownList, moderationEndMinuteDropDownList, moderationEndAMPMDropDownList);
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
					ModerationHours.Add(workingHoursInfo);
					BindModerationHoursDataGrid();
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
				if (Page.IsValid)
				{
					TimeOfDayInfo timeOfDayInfo = new TimeOfDayInfo()
					{
						Name = timeOfDayNameTextBox.Text.Trim(),
						StartTime = GetTime(timeOfDayStartHourDropDownList, timeOfDayStartMinuteDropDownList, timeOfDayStartAMPMDropDownList),
						EndTime = GetTime(timeOfDayEndHourDropDownList, timeOfDayEndMinuteDropDownList, timeOfDayEndAMPMDropDownList)
					};
					TimeSpan endTime = timeOfDayInfo.EndTime;
					TimeSpan timeSpan = new TimeSpan();
					if (endTime == timeSpan)
					{
						timeSpan = timeOfDayInfo.EndTime;
						timeOfDayInfo.EndTime = timeSpan.Add(new TimeSpan(1, 0, 0, 0));
					}
					TimeOfDayList.Add(timeOfDayInfo);
					TimeOfDayList.Sort();
					BindTimeOfDayDataGrid();
				}
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
			}
		}

		protected void AddUserCommandButtonClicked(object sender, EventArgs e)
		{
			/*if (usersDropDownList.Visible && usersDropDownList.SelectedValue != "-1")
			{
				//UserInfo userInfo = FindUserInfoByUserId(Users, int.Parse(usersDropDownList.SelectedValue));
				UserInfo userInfo = FindUserInfoByEmail(Users, usersDropDownList.SelectedValue);
				//if (FindUserInfoByUserId(BCCList, int.Parse(usersDropDownList.SelectedValue)) == null)
				if (FindUserInfoByEmail(BCCList, usersDropDownList.SelectedValue) == null)
				{
					BCCList.Add(userInfo);
					BindBCCListDataGrid();
					BindUsersDropDownList(usersDropDownList, BCCList);
					return;
				}
			}
			else if (usernameTextBox.Visible)*/
			{
				UserInfo userByName = UserController.GetUserByName(PortalId, usernameTextBox.Text);
				if(userByName == null && Helper.IsValidEmail2(usernameTextBox.Text))
                {
					IList<UserInfo> users = (new UserController()).GetUsersBasicSearch(PortalId, 0, 1, "Email", true, "Email", usernameTextBox.Text);
					if (users != null && users.Count > 0)
					{
						userByName = (UserInfo)users[0];
					}
					else
                    {
						userByName = new UserInfo();
						userByName.UserID = -1;
						userByName.Email = usernameTextBox.Text;
						userByName.DisplayName = usernameTextBox.Text;
                    }
				}
				if(userByName == null)
                {
					IList<UserInfo> users = (new UserController()).GetUsersBasicSearch(PortalId, 0, 1, "DisplayName", true, "DisplayName", usernameTextBox.Text);
					if (users != null && users.Count > 0)
					{
						userByName = (UserInfo)users[0];
					}
				}
				//if (userByName != null && FindUserInfoByUserId(BCCList, userByName.UserID) == null)
				if (userByName != null && FindUserInfoByEmail(BCCList, userByName.Email) == null)
				{
					BCCList.Add(userByName);
					BindBCCListDataGrid();
				}
				usernameTextBox.Text = string.Empty;
			}
		}

		protected void AddViewReservationsCommandButtonClicked(object sender, EventArgs e)
		{
			/*if (viewReservationsUsersDropDownList.Visible && viewReservationsUsersDropDownList.SelectedValue != "-1")
			{
				//UserInfo userInfo = FindUserInfoByUserId(Users, int.Parse(viewReservationsUsersDropDownList.SelectedValue));
				UserInfo userInfo = FindUserInfoByEmail(Users, viewReservationsUsersDropDownList.SelectedValue);
				//if (FindUserInfoByUserId(ViewReservationsList, int.Parse(viewReservationsUsersDropDownList.SelectedValue)) == null)
				if (FindUserInfoByEmail(ViewReservationsList, viewReservationsUsersDropDownList.SelectedValue) == null)
				{
					ViewReservationsList.Add(userInfo);
					BindViewReservationsDataGrid();
					BindUsersDropDownList(viewReservationsUsersDropDownList, ViewReservationsList);
					return;
				}
			}
			else if (viewReservationsUsernameTextBox.Visible)*/
			{
				UserInfo userByName = UserController.GetUserByName(PortalId, viewReservationsUsernameTextBox.Text);
				if (userByName == null && Helper.IsValidEmail2(viewReservationsUsernameTextBox.Text))
				{
					IList<UserInfo> users = (new UserController()).GetUsersBasicSearch(PortalId, 0, 1, "Email", true, "Email", viewReservationsUsernameTextBox.Text);
					if (users != null && users.Count > 0)
					{
						userByName = users[0];
					}
					else
					{
						userByName = new UserInfo();
						userByName.UserID = -1;
						userByName.Email = viewReservationsUsernameTextBox.Text;
						userByName.DisplayName = viewReservationsUsernameTextBox.Text;
					}
				}
				if (userByName == null)
				{
					IList<UserInfo> users = (new UserController()).GetUsersBasicSearch(PortalId, 0, 1, "DisplayName", true, "DisplayName", viewReservationsUsernameTextBox.Text);
					if (users != null && users.Count > 0)
					{
						userByName = (UserInfo)users[0];
					}
				}
				//if (userByName != null && FindUserInfoByUserId(ViewReservationsList, userByName.UserID) == null)
				if (userByName != null && FindUserInfoByEmail(ViewReservationsList, userByName.Email) == null)
				{
					ViewReservationsList.Add(userByName);
					BindViewReservationsDataGrid();
				}
				viewReservationsUsernameTextBox.Text = string.Empty;
			}
		}

		protected void AddWorkingHours(object sender, EventArgs e)
		{
			try
			{
				recurrencepatterncontrol.Visible = true;
				addWorkingHoursCommandButton.Visible = false;
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
				if (Page.IsValid)
				{
					WorkingHoursExceptionInfo workingHoursExceptionInfo = new WorkingHoursExceptionInfo()
					{
						Date = DateTime.Parse(workingHoursExceptionDateTextBox.Text)
					};
					if (!workingHoursExceptionNoWorkingHoursRadioButton.Checked)
					{
						for (int i = 0; i < WorkingHoursExceptions.Count; i++)
						{
							WorkingHoursExceptionInfo item = (WorkingHoursExceptionInfo)WorkingHoursExceptions[i];
							if (item.Date == workingHoursExceptionInfo.Date && (item.StartTime == item.EndTime || item.AllDay))
							{
								WorkingHoursExceptions.RemoveAt(i);
								i--;
							}
						}
						workingHoursExceptionInfo.StartTime = GetTime(workingHoursExceptionStartHourDropDownList, workingHoursExceptionStartMinuteDropDownList, workingHoursExceptionStartAMPMDropDownList);
						workingHoursExceptionInfo.EndTime = GetTime(workingHoursExceptionEndHourDropDownList, workingHoursExceptionEndMinuteDropDownList, workingHoursExceptionEndAMPMDropDownList);
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
						for (int j = 0; j < WorkingHoursExceptions.Count; j++)
						{
							if (((WorkingHoursExceptionInfo)WorkingHoursExceptions[j]).Date == workingHoursExceptionInfo.Date)
							{
								WorkingHoursExceptions.RemoveAt(j);
								j--;
							}
						}
					}
					WorkingHoursExceptions.Add(workingHoursExceptionInfo);
					BindWorkingHoursExceptionsDataGrid();
				}
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
			}
		}

		protected void AllowCategorySelectionCheckBoxCheckedChanged(object sender, EventArgs e)
		{
			RebindCategoryDependentSections();
		}

		protected void BCCListCategoryDropDownListSelectedIndexChanged(object sender, EventArgs e)
		{
			BindBCCListSection(int.Parse(bccListCategoryDropDownList.SelectedValue));
		}

		protected void BCCListResetCommandButtonClicked(object sender, EventArgs e)
		{
			int num = int.Parse(bccListCategoryDropDownList.SelectedValue);
			(new CategorySettingController()).DeleteCategorySetting(num, "BCCList");
			BindCategoriesDropDownList(bccListCategoryDropDownList, "BCCList", null, null);
			BindBCCListSection(num);
		}

		protected void BCCListUpdateCommandButtonClicked(object sender, EventArgs e)
		{
			UpdateBCCListSection(true);
		}

		protected void BindAMPMDropDownList(DropDownList dropDownList)
		{
			dropDownList.Visible = !Is24HourClock;
			if (!Is24HourClock)
			{
				dropDownList.Items.Clear();
				dropDownList.Items.Add(new ListItem(CultureInfo.CurrentCulture.DateTimeFormat.AMDesignator, "AM"));
				dropDownList.Items.Add(new ListItem(CultureInfo.CurrentCulture.DateTimeFormat.PMDesignator, "PM"));
			}
		}

		protected void BindBCCListDataGrid()
		{
			bccListDataGrid.DataSource = BCCList;
			bccListDataGrid.DataBind();
			noUsersLabel.Visible = bccListDataGrid.Items.Count == 0;
		}

		protected void BindBCCListSection()
		{
			bool flag;
			HtmlTableRow htmlTableRow = bccListUpdateResetTableRow;
			HtmlTableRow htmlTableRow1 = bccListCategoryTableRow;
			flag = (IsPostBack ? allowCategorySelectionCheckBox.Checked : ModuleSettings.AllowCategorySelection);
			bool flag1 = flag;
			htmlTableRow1.Visible = flag;
			htmlTableRow.Visible = flag1;
			if (bccListCategoryTableRow.Visible)
			{
				BindCategoriesDropDownList(bccListCategoryDropDownList, "BCCList", null, null);
			}
			BindBCCListSection(Null.NullInteger);
		}

		protected void BindBCCListSection(int categoryID)
		{
			bccListCategoryDropDownList.SelectedValue = categoryID.ToString();
			CategorySettings categorySetting = null;
			if (categoryID != Null.NullInteger)
			{
				categorySetting = new CategorySettings(PortalId, TabModuleId, categoryID);
			}
			ViewState.Remove("BCCList");
			BindBCCListDataGrid();
			//if (Users.Count > 100)
			{
				RequiredFieldValidator requiredFieldValidator = usernameRequiredFieldValidator;
				TextBox textBox = usernameTextBox;
				bool flag = false;
				//usersDropDownList.Visible = false;
				bool flag1 = !flag;
				bool flag2 = flag1;
				textBox.Visible = flag1;
				requiredFieldValidator.Visible = flag2;
			}
			/*else
			{
				BindUsersDropDownList(usersDropDownList, BCCList);
			}*/
			bccListResetCommandButton.Visible = (categorySetting == null ? false : categorySetting.IsDefined("BCCList"));
		}

		protected void BindCashierListDataGrid()
		{
			cashierListDataGrid.DataSource = CashierList;
			cashierListDataGrid.DataBind();
			noCashiersLabel.Visible = cashierListDataGrid.Items.Count == 0;
		}

		protected void BindCashierListSection()
		{
			bool flag;
			HtmlTableRow htmlTableRow = cashierUpdateResetTableRow;
			HtmlTableRow htmlTableRow1 = cashierListCategoryTableRow;
			flag = (IsPostBack ? allowCategorySelectionCheckBox.Checked : ModuleSettings.AllowCategorySelection);
			bool flag1 = flag;
			htmlTableRow1.Visible = flag;
			htmlTableRow.Visible = flag1;
			if (cashierListCategoryTableRow.Visible)
			{
				BindCategoriesDropDownList(cashierListCategoryDropDownList, "CashierList", null, null);
			}
			BindCashierListSection(Null.NullInteger);
		}

		protected void BindCashierListSection(int categoryID)
		{
			cashierListCategoryDropDownList.SelectedValue = categoryID.ToString();
			CategorySettings categorySetting = null;
			if (categoryID != Null.NullInteger)
			{
				categorySetting = new CategorySettings(PortalId, TabModuleId, categoryID);
			}
			ViewState.Remove("CashierList");
			BindCashierListDataGrid();
			//if (Users.Count > 100)
			{
				RequiredFieldValidator requiredFieldValidator = cashierListUsernameRequiredFieldValidator;
				TextBox textBox = cashierListUsernameTextBox;
				bool flag = false;
				//cashierListUsersDropDownList.Visible = false;
				bool flag1 = !flag;
				bool flag2 = flag1;
				textBox.Visible = flag1;
				requiredFieldValidator.Visible = flag2;
			}
			/*else
			{
				BindUsersDropDownList(cashierListUsersDropDownList, CashierList);
			}*/
			cashierListResetCommandButton.Visible = (categorySetting == null ? false : categorySetting.IsDefined("CashierList"));
		}

		private void BindCategoriesDropDownList(DropDownList dropDownList, string settingName, string settingName2 = null, string settingName3 = null)
		{
			int categoryID;
			dropDownList.Items.Clear();
			foreach (CategoryInfo categoryList in (new CategoryController()).GetCategoryList(TabModuleId))
			{
				CategorySettings categorySetting = new CategorySettings(PortalId, TabModuleId, categoryList.CategoryID);
				ListItemCollection items = dropDownList.Items;
				string str = string.Concat(categoryList.Name, (categorySetting.IsDefined(settingName) || settingName2 != null && categorySetting.IsDefined(settingName2) || settingName3 != null && categorySetting.IsDefined(settingName3) ? Server.HtmlDecode(" &#10004;") : string.Empty));
				categoryID = categoryList.CategoryID;
				items.Add(new ListItem(str, categoryID.ToString()));
			}
			string str1 = Localization.GetString("All", LocalResourceFile);
			categoryID = Null.NullInteger;
			ListItem listItem = new ListItem(str1, categoryID.ToString());
			dropDownList.Items.Insert(0, listItem);
		}

		protected void BindCategoriesSection()
		{
			HtmlTableRow htmlTableRow = selectCategoryLastTableRow;
			HtmlTableRow htmlTableRow1 = categoryListTableRow2;
			HtmlTableRow htmlTableRow2 = categoryListTableRow;
			HtmlTableRow htmlTableRow3 = categorySelectionModeTableRow;
			HtmlTableRow htmlTableRow4 = bindUponSelectionTableRow;
			HtmlTableRow htmlTableRow5 = allowCrossCategoryConflictsTableRow;
			HtmlTableRow htmlTableRow6 = displayUnavailableCategoriesTableRow;
			CheckBox checkBox = allowCategorySelectionCheckBox;
			bool allowCategorySelection = ModuleSettings.AllowCategorySelection;
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
			selectCategoryLastCheckBox.Checked = ModuleSettings.SelectCategoryLast;
			preventCrossCategoryConflictsCheckBox.Checked = ModuleSettings.PreventCrossCategoryConflicts;
			bindUponSelectionCheckBox.Checked = ModuleSettings.BindUponCategorySelection;
			displayUnavailableCategoriesCheckBox.Checked = ModuleSettings.DisplayUnavailableCategories;
			SelectCategoryLastChanged(null, null);
			PreventCrossCategoryConflictsChanged(null, null);
			BindCategoryListDataGrid();
		}

		protected void BindCategoryListDataGrid()
		{
			categoryListDataGrid.DataSource = CategoryList;
			categoryListDataGrid.DataBind();
			noCategoriesLabel.Visible = categoryListDataGrid.Items.Count == 0;
		}

		protected void BindCategoryPermissionsSection()
		{
			LinkButton linkButton = categoryPermissionsUpdateCommandButton;
			HtmlTableRow htmlTableRow = selectCategoryLastTableRow;
			HtmlTableRow htmlTableRow1 = categoryPermissionsTableRow;
			HtmlTableRow htmlTableRow2 = allowCrossCategoryConflictsTableRow;
			HtmlTableRow htmlTableRow3 = categorySelectionModeTableRow;
			HtmlTableRow htmlTableRow4 = bindUponSelectionTableRow;
			HtmlTableRow htmlTableRow5 = displayUnavailableCategoriesTableRow;
			HtmlTableRow htmlTableRow6 = categoryListTableRow;
			HtmlTableRow htmlTableRow7 = categoryListTableRow2;
			bool @checked = allowCategorySelectionCheckBox.Checked;
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
			bindUponSelectionTableRow.Visible = (!allowCategorySelectionCheckBox.Checked || preventCrossCategoryConflictsCheckBox.Checked ? false : !selectCategoryLastCheckBox.Checked);
			if (allowCategorySelectionCheckBox.Checked)
			{
				BindCategoriesDropDownList(categoryPermissionsDropDownList, "CategoryPermissions", null, null);
			}
			BindCategoryPermissionsSection(Null.NullInteger);
		}

		protected void BindCategoryPermissionsSection(int categoryID)
		{
			categoryPermissionsDropDownList.SelectedValue = categoryID.ToString();
			CategorySettings categorySetting = null;
			if (categoryID != Null.NullInteger)
			{
				categorySetting = new CategorySettings(PortalId, TabModuleId, categoryID);
			}
			ViewState.Remove("CategoryPermissionsList");
			_CategoryPermissionsList = null;
			BindRolesCheckboxList();
			categoryPermissionsResetCommandButton.Visible = (categorySetting == null ? false : categorySetting.IsDefined("CategoryPermissions"));
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
			duplicateReservationsDataGrid.DataSource = DuplicateReservationsList;
			duplicateReservationsDataGrid.DataBind();
			noDuplicateReservationsLabel.Visible = duplicateReservationsDataGrid.Items.Count == 0;
		}

		protected void BindDuplicateReservationsSection()
		{
			ViewState.Remove("DuplicateReservationsList");
			BindDuplicateReservationsDataGrid();
			/*if (Users.Count <= 100)
			{
				BindUsersDropDownList(duplicateReservationsUsersDropDownList, DuplicateReservationsList);
				return;
			}*/
			RequiredFieldValidator requiredFieldValidator = duplicateReservationsUsernameRequiredFieldValidator;
			TextBox textBox = duplicateReservationsUsernameTextBox;
			bool flag = false;
			//duplicateReservationsUsersDropDownList.Visible = false;
			bool flag1 = !flag;
			bool flag2 = flag1;
			textBox.Visible = flag1;
			requiredFieldValidator.Visible = flag2;
		}

		protected void BindGeneralSettingsSection()
		{
			BindTimeZoneDropDownList();
			if (timeZoneDropDownList.Items.FindByValue(ModuleSettings.TimeZone.Id) != null)
			{
				timeZoneDropDownList.SelectedValue = ModuleSettings.TimeZone.Id;
			}
			BindThemesDropDownList();
			if (themeDropDownList.Items.FindByValue(ModuleSettings.Theme) != null)
			{
				themeDropDownList.SelectedValue = ModuleSettings.Theme;
			}
			contactInfoFirstCheckBox.Checked = ModuleSettings.ContactInfoFirst;
			categorySelectionModeList.Checked = ModuleSettings.CategorySelectionMode == Gafware.Modules.Reservations.ModuleSettings.SelectionModeEnum.HorizontalScroll;
			categorySelectionModeList.Enabled = Helper.IsjQuery17orHigher;
			categorySelectionModeDropDownList.Checked = ModuleSettings.CategorySelectionMode == Gafware.Modules.Reservations.ModuleSettings.SelectionModeEnum.DropDownList;
			categorySelectionModeListBox.Checked = ModuleSettings.CategorySelectionMode == Gafware.Modules.Reservations.ModuleSettings.SelectionModeEnum.ListBox;
			RadioButton radioButton = displayListRadioButton;
			RadioButton radioButton1 = displayCalendarRadioButton;
			bool displayCalendar = ModuleSettings.DisplayCalendar;
			bool flag = displayCalendar;
			radioButton1.Checked = displayCalendar;
			radioButton.Checked = !flag;
			displayListRadioButton.Enabled = Helper.IsjQuery17orHigher;
			timeOfDaySelectionModeList.Checked = ModuleSettings.TimeOfDaySelectionMode == Gafware.Modules.Reservations.ModuleSettings.SelectionModeEnum.HorizontalScroll;
			timeOfDaySelectionModeList.Enabled = Helper.IsjQuery17orHigher;
			timeOfDaySelectionModeDropDownList.Checked = ModuleSettings.TimeOfDaySelectionMode == Gafware.Modules.Reservations.ModuleSettings.SelectionModeEnum.DropDownList;
			timeOfDaySelectionModeListBox.Checked = ModuleSettings.TimeOfDaySelectionMode == Gafware.Modules.Reservations.ModuleSettings.SelectionModeEnum.ListBox;
			timeSelectionModeList.Checked = ModuleSettings.TimeSelectionMode == Gafware.Modules.Reservations.ModuleSettings.SelectionModeEnum.HorizontalScroll;
			timeSelectionModeList.Enabled = Helper.IsjQuery17orHigher;
			timeSelectionModeDropDownList.Checked = ModuleSettings.TimeSelectionMode == Gafware.Modules.Reservations.ModuleSettings.SelectionModeEnum.DropDownList;
			timeSelectionModeListBox.Checked = ModuleSettings.TimeSelectionMode == Gafware.Modules.Reservations.ModuleSettings.SelectionModeEnum.ListBox;
			durationSelectionModeList.Checked = ModuleSettings.DurationSelectionMode == Gafware.Modules.Reservations.ModuleSettings.SelectionModeEnum.HorizontalScroll;
			durationSelectionModeList.Enabled = Helper.IsjQuery17orHigher;
			durationSelectionModeDropDownList.Checked = ModuleSettings.DurationSelectionMode == Gafware.Modules.Reservations.ModuleSettings.SelectionModeEnum.DropDownList;
			durationSelectionModeListBox.Checked = ModuleSettings.DurationSelectionMode == Gafware.Modules.Reservations.ModuleSettings.SelectionModeEnum.ListBox;
			displayRemainingReservationsCheckBox.Checked = ModuleSettings.DisplayRemainingReservations;
			displayEndTimeCheckBox.Checked = ModuleSettings.DisplayEndTime;
			allowDescriptionCheckBox.Checked = ModuleSettings.AllowDescription;
			allowSchedulingAnotherReservationCheckBox.Checked = ModuleSettings.AllowSchedulingAnotherReservation;
			requireEmailCheckBox.Checked = ModuleSettings.RequireEmail;
			requirePhoneCheckBox.Checked = ModuleSettings.RequirePhone;
			allowLookupByPhoneCheckBox.Checked = ModuleSettings.AllowLookupByPhone;
			redirectUrlTextBox.Text = ModuleSettings.RedirectUrl;
			skipContactInfoCheckBox.Checked = ModuleSettings.SkipContactInfoForAuthenticatedUsers;
			requireVerificationCodeTableRow.Visible = requireEmailCheckBox.Checked;
			requireVerificationCodeCheckBox.Checked = ModuleSettings.RequireVerificationCode;
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
				if (Is24HourClock)
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
				string str = date1.ToString((Is24HourClock ? "HH" : "hh"));
				double totalHours = date1.TimeOfDay.TotalHours;
				items.Add(new ListItem(str, totalHours.ToString()));
				date1 = date1.AddHours(1);
			}
		}

		protected void BindMailTemplates()
		{
			mailTemplateDropDownList.Items.Clear();
			mailTemplateDropDownList.Items.Add(new ListItem(Localization.GetString("Confirmation", LocalResourceFile), "Confirmation"));
			mailTemplateDropDownList.Items.Add(new ListItem(Localization.GetString("Modification", LocalResourceFile), "Modification"));
			mailTemplateDropDownList.Items.Add(new ListItem(Localization.GetString("Rescheduled", LocalResourceFile), "Rescheduled"));
			mailTemplateDropDownList.Items.Add(new ListItem(Localization.GetString("Cancellation", LocalResourceFile), "Cancellation"));
			mailTemplateDropDownList.Items.Add(new ListItem(Localization.GetString("Moderator", LocalResourceFile), "Moderator"));
			mailTemplateDropDownList.Items.Add(new ListItem(Localization.GetString("Declined", LocalResourceFile), "Declined"));
			mailTemplateDropDownList.Items.Add(new ListItem(Localization.GetString("Reminder", LocalResourceFile), "Reminder"));
			mailTemplateDropDownList.Items.Add(new ListItem(Localization.GetString("VerificationCode", LocalResourceFile), "VerificationCode"));
			mailTemplateDropDownList.Items.Add(new ListItem(Localization.GetString("DuplicateReservation", LocalResourceFile), "DuplicateReservation"));
			if (IsProfessional)
			{
				mailTemplateDropDownList.Items.Add(new ListItem(Localization.GetString("PendingRescheduleRefund", LocalResourceFile), "PendingRescheduleRefund"));
				mailTemplateDropDownList.Items.Add(new ListItem(Localization.GetString("PendingCancellationRefund", LocalResourceFile), "PendingCancellationRefund"));
				mailTemplateDropDownList.Items.Add(new ListItem(Localization.GetString("PendingDeclinationRefund", LocalResourceFile), "PendingDeclinationRefund"));
			}
			mailTemplateDropDownList.SelectedIndex = 0;
			MailTemplateDropDownListSelectedIndexChanged(null, null);
		}

		protected void BindMailTemplatesSection()
		{
			mailFromTextBox.Text = ModuleSettings.MailFrom;
			attachICalendarCheckBox.Checked = ModuleSettings.AttachICalendar;
			iCalendarAttachmentFileNameTextBox.Text = ModuleSettings.ICalendarAttachmentFileName;
			BindMailTemplates();
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
			ModerationHours.Sort(new WorkingHourInfoComparer());
			moderationHoursDataGrid.DataSource = ModerationHours;
			moderationHoursDataGrid.DataBind();
			noModerationHoursLabel.Visible = moderationHoursDataGrid.Items.Count == 0;
		}

		protected void BindModerationSection()
		{
			bool flag;
			HtmlTableRow htmlTableRow = moderationUpdateResetTableRow;
			HtmlTableRow htmlTableRow1 = moderationCategoryTableRow;
			flag = (IsPostBack ? allowCategorySelectionCheckBox.Checked : ModuleSettings.AllowCategorySelection);
			bool flag1 = flag;
			htmlTableRow1.Visible = flag;
			htmlTableRow.Visible = flag1;
			if (moderationCategoryTableRow.Visible)
			{
				BindCategoriesDropDownList(moderationCategoryDropDownList, "Moderate", null, null);
			}
			if (!IsPostBack)
			{
				BindWeekDaysDropDownList(moderationWeekDaysDropDownList);
				BindHoursDropDownList(moderationStartHourDropDownList);
				BindHoursDropDownList(moderationEndHourDropDownList);
				BindMinutesDropDownList(moderationStartMinuteDropDownList);
				BindMinutesDropDownList(moderationEndMinuteDropDownList);
				BindAMPMDropDownList(moderationStartAMPMDropDownList);
				BindAMPMDropDownList(moderationEndAMPMDropDownList);
				moderationStartHourDropDownList.SelectedValue = "8";
				if (!Is24HourClock)
				{
					moderationEndHourDropDownList.SelectedValue = "5";
					moderationEndAMPMDropDownList.SelectedValue = "PM";
				}
				else
				{
					moderationEndHourDropDownList.SelectedValue = "17";
				}
			}
			BindModerationSection(Null.NullInteger);
		}

		protected void BindModerationSection(int categoryID)
		{
			bool flag;
			moderationCategoryDropDownList.SelectedValue = categoryID.ToString();
			CategorySettings categorySetting = null;
			if (categoryID != Null.NullInteger)
			{
				categorySetting = new CategorySettings(PortalId, TabModuleId, categoryID);
			}
			HtmlTableRow htmlTableRow = moderationHoursTableRow;
			HtmlTableRow htmlTableRow1 = globalModeratorsDropDownListTableRow;
			HtmlTableRow htmlTableRow2 = globalModeratorsDataGridTableRow;
			CheckBox checkBox = moderateCheckBox;
			flag = (categorySetting != null ? categorySetting.Moderate : ModuleSettings.Moderate);
			bool flag1 = flag;
			checkBox.Checked = flag;
			bool flag2 = flag1;
			bool flag3 = flag2;
			htmlTableRow2.Visible = flag2;
			bool flag4 = flag3;
			bool flag5 = flag4;
			htmlTableRow1.Visible = flag4;
			htmlTableRow.Visible = flag5;
			ViewState.Remove("ModeratorList");
			_ModeratorList = null;
			ViewState.Remove("ModerationHours");
			_ModerationHours = null;
			BindModeratorsDataGrid();
			//if (Users.Count > 100)
			{
				RequiredFieldValidator requiredFieldValidator = moderatorUsernameRequiredFieldValidator;
				TextBox textBox = moderatorUsernameTextBox;
				flag3 = false;
				//moderatorUsersDropDownList.Visible = false;
				bool flag6 = !flag3;
				flag5 = flag6;
				textBox.Visible = flag6;
				requiredFieldValidator.Visible = flag5;
			}
			/*else
			{
				BindUsersDropDownList(moderatorUsersDropDownList, ModeratorList);
			}*/
			BindModerationHoursDataGrid();
			moderationResetCommandButton.Visible = (categorySetting == null ? false : categorySetting.IsDefined("Moderate"));
		}

		protected void BindModeratorsDataGrid()
		{
			globalModeratorsDataGrid.DataSource = ModeratorList;
			globalModeratorsDataGrid.DataBind();
			noGlobalModeratorsLabel.Visible = globalModeratorsDataGrid.Items.Count == 0;
		}

		protected void BindRemindersSection()
		{
			bool flag;
			sendRemindersCheckBox.Checked = ModuleSettings.SendReminders;
			requireConfirmationCheckBox.Checked = ModuleSettings.RequireConfirmation;
			BindTimeSpanDropDownList(sendRemindersWhenDropDownList);
			SetTimeSpan(ModuleSettings.SendRemindersWhen, sendRemindersWhenTextBox, sendRemindersWhenDropDownList);
			BindTimeSpanDropDownList(requireConfirmationWhenDropDownList);
			SetTimeSpan(ModuleSettings.RequireConfirmationWhen, requireConfirmationWhenTextBox, requireConfirmationWhenDropDownList);
			sendRemindersViaDropDownList.DataSource = Helper.LocalizeEnum(typeof(SendReminderVia), LocalResourceFile);
			sendRemindersViaDropDownList.DataTextField = "LocalizedName";
			sendRemindersViaDropDownList.DataValueField = "Value";
			sendRemindersViaDropDownList.DataBind();
			sendRemindersViaDropDownList.SelectedValue = ModuleSettings.SendRemindersVia.ToString();
			HtmlTableRow htmlTableRow = requireConfirmationTableRow;
			HtmlTableRow htmlTableRow1 = sendRemindersWhenTableRow;
			HtmlTableRow htmlTableRow2 = sendRemindersViaTableRow;
			bool @checked = sendRemindersCheckBox.Checked;
			bool flag1 = @checked;
			htmlTableRow2.Visible = @checked;
			bool flag2 = flag1;
			bool flag3 = flag2;
			htmlTableRow1.Visible = flag2;
			htmlTableRow.Visible = flag3;
			HtmlTableRow htmlTableRow3 = requireConfirmationTableRow2;
			HtmlTableRow htmlTableRow4 = requireConfirmationWhenTableRow;
			flag = (!requireConfirmationTableRow.Visible ? false : requireConfirmationCheckBox.Checked);
			flag3 = flag;
			htmlTableRow4.Visible = flag;
			htmlTableRow3.Visible = flag3;
			sendRemindersViaTableRow.Visible = (!sendRemindersViaTableRow.Visible ? false : IsProfessional);
		}

		protected void BindReservationFeesSection()
		{
			bool flag;
			HtmlTableRow htmlTableRow = reservationFeesUpdateResetTableRow;
			HtmlTableRow htmlTableRow1 = reservationFeesCategoryTableRow;
			flag = (IsPostBack ? allowCategorySelectionCheckBox.Checked : ModuleSettings.AllowCategorySelection);
			bool flag1 = flag;
			htmlTableRow1.Visible = flag;
			htmlTableRow.Visible = flag1;
			if (reservationFeesCategoryTableRow.Visible)
			{
				BindCategoriesDropDownList(reservationFeesCategoryDropDownList, "SchedulingFee", "FeeScheduleType", null);
			}
			payPalAccountTextBox.Text = ModuleSettings.PayPalAccount;
			payPalUrlTextBox.Text = ModuleSettings.PayPalUrl;
			payPalItemDescriptionTextBox.Text = ModuleSettings.ItemDescription;
			pendingPaymentExpirationTextBox.Text = ModuleSettings.PendingPaymentExpiration.TotalMinutes.ToString();
			allowPayLaterCheckBox.Checked = ModuleSettings.AllowPayLater;
			BindCurrencyDropDownList(currencyDropDownList);
			currencyDropDownList.SelectedValue = ModuleSettings.Currency;
			paymentMethodDropDownList.Items.Clear();
			ListItemCollection items = paymentMethodDropDownList.Items;
			PaymentMethod paymentMethod = PaymentMethod.PayPalPaymentsStandard;
			string str = Localization.GetString(paymentMethod.ToString(), LocalResourceFile);
			paymentMethod = PaymentMethod.PayPalPaymentsStandard;
			items.Add(new ListItem(str, paymentMethod.ToString()));
			ListItemCollection listItemCollections = paymentMethodDropDownList.Items;
			paymentMethod = PaymentMethod.AuthorizeNetSIM;
			string str1 = Localization.GetString(paymentMethod.ToString(), LocalResourceFile);
			paymentMethod = PaymentMethod.AuthorizeNetSIM;
			listItemCollections.Add(new ListItem(str1, paymentMethod.ToString()));
			DropDownList dropDownList = paymentMethodDropDownList;
			paymentMethod = ModuleSettings.PaymentMethod;
			dropDownList.SelectedValue = paymentMethod.ToString();
			authorizeNetApiLoginTextBox.Text = ModuleSettings.AuthorizeNetApiLogin;
			authorizeNetTransactionKeyTextBox.Text = ModuleSettings.AuthorizeNetTransactionKey;
			authorizeNetMerchantHashTextBox.Text = ModuleSettings.AuthorizeNetMerchantHash;
			authorizeNetTestModeCheckBox.Checked = ModuleSettings.AuthorizeNetTestMode;
			BindReservationFeesSection(Null.NullInteger);
			PaymentMethodChanged(null, null);
		}

		protected void BindReservationFeesSection(int categoryID)
		{
			PaymentMethod paymentMethod;
			bool selectedValue;
			bool flag;
			bool flag1;
			reservationFeesCategoryDropDownList.SelectedValue = categoryID.ToString();
			CategorySettings categorySetting = null;
			if (categoryID != Null.NullInteger)
			{
				categorySetting = new CategorySettings(PortalId, TabModuleId, categoryID);
			}
			paymentMethodTableRow.Visible = categorySetting == null;
			HtmlTableRow htmlTableRow = payPalAccountTableRow;
			HtmlTableRow htmlTableRow1 = payPalUrlTableRow;
			if (categorySetting != null)
			{
				selectedValue = false;
			}
			else
			{
				paymentMethod = PaymentMethod.PayPalPaymentsStandard;
				selectedValue = paymentMethodDropDownList.SelectedValue == paymentMethod.ToString();
			}
			bool flag2 = selectedValue;
			htmlTableRow1.Visible = selectedValue;
			htmlTableRow.Visible = flag2;
			HtmlTableRow htmlTableRow2 = authorizeNetApiLoginTableRow;
			HtmlTableRow htmlTableRow3 = authorizeNetMerchantHashTableRow;
			HtmlTableRow htmlTableRow4 = authorizeNetTestModeTableRow;
			HtmlTableRow htmlTableRow5 = authorizeNetTransactionKeyTableRow;
			if (categorySetting != null)
			{
				flag = false;
			}
			else
			{
				paymentMethod = PaymentMethod.AuthorizeNetSIM;
				flag = paymentMethodDropDownList.SelectedValue == paymentMethod.ToString();
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
			itemDescriptionTableRow.Visible = categorySetting == null;
			pendingPaymentExpirationTableRow.Visible = categorySetting == null;
			currencyTableRow.Visible = categorySetting == null;
			allowPayLaterTableRow.Visible = categorySetting == null;
			LinkButton linkButton = reservationFeesResetCommandButton;
			if (categorySetting == null)
			{
				flag1 = false;
			}
			else
			{
				flag1 = (categorySetting.IsDefined("SchedulingFee") ? true : categorySetting.IsDefined("FeeScheduleType"));
			}
			linkButton.Visible = flag1;
			feeschedulecontrol.Currency = ModuleSettings.Currency;
			feeschedulecontrol.FeeScheduleType = (categorySetting != null ? categorySetting.FeeScheduleType : ModuleSettings.FeeScheduleType);
			if (feeschedulecontrol.FeeScheduleType == FeeScheduleType.Flat)
			{
				feeschedulecontrol.FlatFeeScheduleInfo = (categorySetting != null ? categorySetting.FlatFeeScheduleInfo : ModuleSettings.FlatFeeScheduleInfo);
			}
			else if (feeschedulecontrol.FeeScheduleType == FeeScheduleType.Seasonal)
			{
				feeschedulecontrol.SeasonalFeeScheduleList = (categorySetting != null ? categorySetting.SeasonalFeeScheduleList : ModuleSettings.SeasonalFeeScheduleList);
			}
			feeschedulecontrol.DataBind();
		}

		protected void BindReservationSettingsSection()
		{
			bool flag;
			HtmlTableRow htmlTableRow = reservationSettingsUpdateResetTableRow;
			HtmlTableRow htmlTableRow1 = reservationSettingsCategoryTableRow;
			flag = (IsPostBack ? allowCategorySelectionCheckBox.Checked : ModuleSettings.AllowCategorySelection);
			bool flag1 = flag;
			htmlTableRow1.Visible = flag;
			htmlTableRow.Visible = flag1;
			if (reservationSettingsCategoryTableRow.Visible)
			{
				BindCategoriesDropDownList(reservationSettingsCategoryDropDownList, "AllowCancellations", null, null);
			}
			if (!IsPostBack)
			{
				BindTimeSpanDropDownList(minTimeAheadDropDownList);
				BindTimeSpanDropDownList(reservationIntervalDropDownList);
				BindTimeSpanDropDownList(reservationDurationDropDownList);
				BindTimeSpanDropDownList(reservationDurationMaxDropDownList);
				BindTimeSpanDropDownList(reservationDurationIntervalDropDownList);
			}
			BindReservationSettingsSection(Null.NullInteger);
		}

		protected void BindReservationSettingsSection(int categoryID)
		{
			int daysAhead;
			string str;
			string str1;
			string empty;
			reservationSettingsCategoryDropDownList.SelectedValue = categoryID.ToString();
			CategorySettings categorySetting = null;
			if (categoryID != Null.NullInteger)
			{
				categorySetting = new CategorySettings(PortalId, TabModuleId, categoryID);
			}
			allowCancellationsCheckBox.Checked = (categorySetting != null ? categorySetting.AllowCancellations : ModuleSettings.AllowCancellations);
			allowReschedulingCheckBox.Checked = (categorySetting != null ? categorySetting.AllowRescheduling : ModuleSettings.AllowRescheduling);
			TextBox textBox = daysAheadTextBox;
			if (categorySetting != null)
			{
				str = categorySetting.DaysAhead.ToString();
			}
			else
			{
				daysAhead = ModuleSettings.DaysAhead;
				str = daysAhead.ToString();
			}
			textBox.Text = str;
			TextBox textBox1 = maxConflictingReservationsTextBox;
			if (categorySetting != null)
			{
				str1 = categorySetting.MaxConflictingReservations.ToString();
			}
			else
			{
				daysAhead = ModuleSettings.MaxConflictingReservations;
				str1 = daysAhead.ToString();
			}
			textBox1.Text = str1;
			TextBox textBox2 = maxReservationsPerUserTextBox;
			if (categorySetting != null && categorySetting.MaxReservationsPerUser != Null.NullInteger)
			{
				empty = categorySetting.MaxReservationsPerUser.ToString();
			}
			else if (ModuleSettings.MaxReservationsPerUser != Null.NullInteger)
			{
				daysAhead = ModuleSettings.MaxReservationsPerUser;
				empty = daysAhead.ToString();
			}
			else
			{
				empty = string.Empty;
			}
			textBox2.Text = empty;
			if (categorySetting == null)
			{
				SetTimeSpan(ModuleSettings.MinTimeAhead, minTimeAheadTextBox, minTimeAheadDropDownList);
				SetTimeSpan(ModuleSettings.ReservationInterval, reservationIntervalTextBox, reservationIntervalDropDownList);
				SetTimeSpan(ModuleSettings.ReservationDuration, reservationDurationTextBox, reservationDurationDropDownList);
				SetTimeSpan(ModuleSettings.ReservationDurationMax, reservationDurationMaxTextBox, reservationDurationMaxDropDownList);
				SetTimeSpan(ModuleSettings.ReservationDurationInterval, reservationDurationIntervalTextBox, reservationDurationIntervalDropDownList);
			}
			else
			{
				SetTimeSpan(categorySetting.MinTimeAhead, minTimeAheadTextBox, minTimeAheadDropDownList);
				SetTimeSpan(categorySetting.ReservationInterval, reservationIntervalTextBox, reservationIntervalDropDownList);
				SetTimeSpan(categorySetting.ReservationDuration, reservationDurationTextBox, reservationDurationDropDownList);
				SetTimeSpan(categorySetting.ReservationDurationMax, reservationDurationMaxTextBox, reservationDurationMaxDropDownList);
				SetTimeSpan(categorySetting.ReservationDurationInterval, reservationDurationIntervalTextBox, reservationDurationIntervalDropDownList);
			}
			reservationSettingsResetCommandButton.Visible = (categorySetting == null ? false : categorySetting.IsDefined("AllowCancellations"));
		}

		protected void BindRolesCheckboxList()
		{
			categoryPermissionsCheckboxList.Items.Clear();
			categoryPermissionsCheckboxList.DataSource = (new RoleController()).GetRoles(PortalId); //.GetPortalRoles(PortalId);
			categoryPermissionsCheckboxList.DataTextField = "RoleName";
			categoryPermissionsCheckboxList.DataValueField = "RoleID";
			categoryPermissionsCheckboxList.DataBind();
			categoryPermissionsCheckboxList.Items.Insert(0, new ListItem(Localization.GetString("AllUsers", LocalResourceFile), "-1"));
			categoryPermissionsCheckboxList.Items.Add(new ListItem(Localization.GetString("UnauthenticatedUsers", LocalResourceFile), "-3"));
			foreach (ListItem item in categoryPermissionsCheckboxList.Items)
			{
				item.Selected = CategoryPermissionsList.IndexOf(int.Parse(item.Value)) != -1;
			}
		}

		protected void BindSMSTemplates()
		{
			smsTemplateDropDownList.Items.Clear();
			smsTemplateDropDownList.Items.Add(new ListItem(Localization.GetString("Reminder", LocalResourceFile), "Reminder"));
			smsTemplateDropDownList.SelectedIndex = 0;
			SMSTemplateDropDownListSelectedIndexChanged(null, null);
		}

		protected void BindSMSTemplatesSection()
		{
			smsTemplatesSectionTableRow.Visible = IsProfessional;
			twilioAccountSIDTextBox.Text = ModuleSettings.TwilioAccountSID;
			twilioAuthTokenTextBox.Text = ModuleSettings.TwilioAuthToken;
			twilioFromTextBox.Text = ModuleSettings.TwilioFrom;
			BindSMSTemplates();
		}

		protected void BindThemesDropDownList()
		{
			themeDropDownList.DataSource = (new DirectoryInfo(Server.MapPath(string.Concat(TemplateSourceDirectory, "/Themes")))).GetDirectories();
			themeDropDownList.DataTextField = "Name";
			themeDropDownList.DataValueField = "Name";
			themeDropDownList.DataBind();
		}

		protected void BindTimeOfDayDataGrid()
		{
			timeOfDayDataGrid.DataSource = TimeOfDayList;
			timeOfDayDataGrid.DataBind();
			noTimeOfDayLabel.Visible = timeOfDayDataGrid.Items.Count == 0;
		}

		protected void BindTimeOfDaySection()
		{
			if (!IsPostBack)
			{
				BindHoursDropDownList(timeOfDayStartHourDropDownList);
				BindHoursDropDownList(timeOfDayEndHourDropDownList);
				BindMinutesDropDownList(timeOfDayStartMinuteDropDownList);
				BindMinutesDropDownList(timeOfDayEndMinuteDropDownList);
				BindAMPMDropDownList(timeOfDayStartAMPMDropDownList);
				BindAMPMDropDownList(timeOfDayEndAMPMDropDownList);
			}
			displayTimeOfDayCheckBox.Checked = ModuleSettings.DisplayTimeOfDay;
			displayUnavailableTimeOfDayCheckBox.Checked = ModuleSettings.DisplayUnavailableTimeOfDay;
			BindTimeOfDayDataGrid();
			HtmlTableRow htmlTableRow = timeOfDaySelectionModeTableRow;
			HtmlTableRow htmlTableRow1 = displayUnavailableTimeOfDayTableRow;
			HtmlTableRow htmlTableRow2 = timeOfDayTableRow;
			bool @checked = displayTimeOfDayCheckBox.Checked;
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
			dropDownList.Items.Add(new ListItem(Localization.GetString("Minutes", LocalResourceFile), "M"));
			dropDownList.Items.Add(new ListItem(Localization.GetString("Hours", LocalResourceFile), "H"));
			dropDownList.Items.Add(new ListItem(Localization.GetString("Days", LocalResourceFile), "D"));
		}

		protected void BindTimeZoneDropDownList()
		{
			timeZoneDropDownList.DataSource = TimeZoneInfo.GetSystemTimeZones();
			timeZoneDropDownList.DataTextField = "DisplayName";
			timeZoneDropDownList.DataValueField = "Id";
			timeZoneDropDownList.DataBind();
		}

		protected void BindUsersDropDownList(DropDownList dropDownList, ArrayList usersToExclude)
		{
			dropDownList.Items.Clear();
			foreach (UserInfo user in Users)
			{
				//if (FindUserInfoByUserId(usersToExclude, user.UserID) != null)
				if (FindUserInfoByEmail(usersToExclude, user.Email) != null)
				{
					continue;
				}
				ListItemCollection items = dropDownList.Items;
				//string displayName = user.DisplayName;
				//int userID = user.UserID;
				//items.Add(new ListItem(displayName, userID.ToString()));
				items.Add(new ListItem(user.DisplayName, user.Email));
			}
			dropDownList.Items.Insert(0, new ListItem(Localization.GetString("NoneSpecified", LocalResourceFile), "-1"));
		}

		protected void BindViewReservationsDataGrid()
		{
			viewReservationsDataGrid.DataSource = ViewReservationsList;
			viewReservationsDataGrid.DataBind();
			noViewReservationsLabel.Visible = viewReservationsDataGrid.Items.Count == 0;
		}

		protected void BindViewReservationsSection()
		{
			ViewState.Remove("ViewReservationsList");
			BindViewReservationsDataGrid();
			/*if (Users.Count <= 100)
			{
				BindUsersDropDownList(viewReservationsUsersDropDownList, ViewReservationsList);
				return;
			}*/
			RequiredFieldValidator requiredFieldValidator = viewReservationsUsernameRequiredFieldValidator;
			TextBox textBox = viewReservationsUsernameTextBox;
			bool flag = false;
			//viewReservationsUsersDropDownList.Visible = false;
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
			workingHoursDataGrid.DataSource = WorkingHours;
			workingHoursDataGrid.DataBind();
			noWorkingHoursLabel.Visible = workingHoursDataGrid.Items.Count == 0;
		}

		protected void BindWorkingHoursExceptionsDataGrid()
		{
			WorkingHoursExceptions.Sort(new WorkingHoursExceptionInfoComparer());
			workingHoursExceptionsWorkingHoursDataGrid.DataSource = WorkingHoursExceptions;
			workingHoursExceptionsWorkingHoursDataGrid.DataBind();
			workingHoursExceptionsNoWorkingHoursLabel.Visible = workingHoursExceptionsWorkingHoursDataGrid.Items.Count == 0;
		}

		protected void BindWorkingHoursExceptionsSection()
		{
			bool flag;
			HtmlTableRow htmlTableRow = workingHoursExceptionsUpdateResetTableRow;
			HtmlTableRow htmlTableRow1 = workingHoursExceptionsCategoryTableRow;
			flag = (IsPostBack ? allowCategorySelectionCheckBox.Checked : ModuleSettings.AllowCategorySelection);
			bool flag1 = flag;
			htmlTableRow1.Visible = flag;
			htmlTableRow.Visible = flag1;
			if (workingHoursExceptionsCategoryTableRow.Visible)
			{
				BindCategoriesDropDownList(workingHoursExceptionsCategoryDropDownList, "WorkingHoursExceptionsDefined", null, null);
			}
			if (!IsPostBack)
			{
				BindHoursDropDownList(workingHoursExceptionStartHourDropDownList);
				BindHoursDropDownList(workingHoursExceptionEndHourDropDownList);
				BindMinutesDropDownList(workingHoursExceptionStartMinuteDropDownList);
				BindMinutesDropDownList(workingHoursExceptionEndMinuteDropDownList);
				BindAMPMDropDownList(workingHoursExceptionStartAMPMDropDownList);
				BindAMPMDropDownList(workingHoursExceptionEndAMPMDropDownList);
				workingHoursExceptionStartHourDropDownList.SelectedValue = "8";
				if (!Is24HourClock)
				{
					workingHoursExceptionEndHourDropDownList.SelectedValue = "5";
					workingHoursExceptionEndAMPMDropDownList.SelectedValue = "PM";
				}
				else
				{
					workingHoursExceptionEndHourDropDownList.SelectedValue = "17";
				}
				workingHoursExceptionDateImage.Attributes.Add("onclick", DotNetNuke.Common.Utilities.Calendar.InvokePopupCal(workingHoursExceptionDateTextBox));
			}
			BindWorkingHoursExceptionsSection(Null.NullInteger);
		}

		protected void BindWorkingHoursExceptionsSection(int categoryID)
		{
			workingHoursExceptionsCategoryDropDownList.SelectedValue = categoryID.ToString();
			CategorySettings categorySetting = null;
			if (categoryID != Null.NullInteger)
			{
				categorySetting = new CategorySettings(PortalId, TabModuleId, categoryID);
			}
			ViewState.Remove("WorkingHoursExceptions");
			BindWorkingHoursExceptionsDataGrid();
			workingHoursExceptionsResetCommandButton.Visible = (categorySetting == null ? false : categorySetting.WorkingHoursExceptionsDefined);
		}

		protected void BindWorkingHoursSection()
		{
			bool flag;
			HtmlTableRow htmlTableRow = workingHoursUpdateResetTableRow;
			HtmlTableRow htmlTableRow1 = workingHoursCategoryTableRow;
			flag = (IsPostBack ? allowCategorySelectionCheckBox.Checked : ModuleSettings.AllowCategorySelection);
			bool flag1 = flag;
			htmlTableRow1.Visible = flag;
			htmlTableRow.Visible = flag1;
			if (workingHoursCategoryTableRow.Visible)
			{
				BindCategoriesDropDownList(workingHoursCategoryDropDownList, "WorkingHours.1", null, null);
			}
			if (!IsPostBack)
			{
				recurrencepatterncontrol.SubmitText = Localization.GetString("addCommandButton", LocalResourceFile);
			}
			BindWorkingHoursSection(Null.NullInteger);
		}

		protected void BindWorkingHoursSection(int categoryID)
		{
			workingHoursCategoryDropDownList.SelectedValue = categoryID.ToString();
			CategorySettings categorySetting = null;
			if (categoryID != Null.NullInteger)
			{
				categorySetting = new CategorySettings(PortalId, TabModuleId, categoryID);
			}
			ViewState.Remove("WorkingHours");
			_WorkingHours = null;
			BindWorkingHoursDataGrid();
			workingHoursResetCommandButton.Visible = (categorySetting == null ? false : categorySetting.IsDefined("WorkingHours.1"));
		}

		protected void CancelSettingsCommandButtonClicked(object sender, EventArgs e)
		{
			Response.Redirect(_navigationManager.NavigateURL());
		}

		protected void CashierListCategoryDropDownListSelectedIndexChanged(object sender, EventArgs e)
		{
			BindCashierListSection(int.Parse(cashierListCategoryDropDownList.SelectedValue));
		}

		protected void CashierListResetCommandButtonClicked(object sender, EventArgs e)
		{
			int num = int.Parse(cashierListCategoryDropDownList.SelectedValue);
			(new CategorySettingController()).DeleteCategorySetting(num, "CashierList");
			BindCategoriesDropDownList(cashierListCategoryDropDownList, "CashierList", null, null);
			BindCashierListSection(num);
		}

		protected void CashierListUpdateCommandButtonClicked(object sender, EventArgs e)
		{
			UpdateCashierListSection(true);
		}

		protected void CategoryPermissionsDropDownListSelectedIndexChanged(object sender, EventArgs e)
		{
			BindCategoryPermissionsSection(int.Parse(categoryPermissionsDropDownList.SelectedValue));
		}

		protected void CategoryPermissionsResetCommandButtonClicked(object sender, EventArgs e)
		{
			int num = int.Parse(categoryPermissionsDropDownList.SelectedValue);
			(new CategorySettingController()).DeleteCategorySetting(num, "CategoryPermissions");
			BindCategoriesDropDownList(categoryPermissionsDropDownList, "CategoryPermissions", null, null);
			BindCategoryPermissionsSection(num);
		}

		protected void CategoryPermissionsUpdateCommandButtonClicked(object sender, EventArgs e)
		{
			UpdateCategoryPermissionsSection(true);
		}

		protected void DeleteCashier(object sender, DataGridCommandEventArgs e)
		{
			//CashierList.RemoveAt(FindUserInfoIndexByUserId(CashierList, int.Parse(((Label)e.Item.FindControl("userId")).Text)));
			CashierList.RemoveAt(FindUserInfoIndexByEmail(CashierList, ((Label)e.Item.FindControl("email")).Text));
			BindCashierListDataGrid();
			/*if (cashierListUsersDropDownList.Visible)
			{
				BindUsersDropDownList(cashierListUsersDropDownList, CashierList);
			}*/
		}

		protected void DeleteCategory(object sender, DataGridCommandEventArgs e)
		{
			int num = int.Parse(((Label)e.Item.FindControl("categoryID")).Text);
			(new CategoryController()).DeleteCategory(num);
			CategoryList.RemoveAt(CategoryList.FindIndex((CategoryInfo x) => x.CategoryID == num));
			BindCategoryListDataGrid();
			RebindCategoryDependentSections();
		}

		protected void DeleteDuplicateReservations(object sender, DataGridCommandEventArgs e)
		{
			//DuplicateReservationsList.RemoveAt(FindUserInfoIndexByUserId(DuplicateReservationsList, int.Parse(((Label)e.Item.FindControl("userId")).Text)));
			DuplicateReservationsList.RemoveAt(FindUserInfoIndexByEmail(DuplicateReservationsList, ((Label)e.Item.FindControl("email")).Text));
			BindDuplicateReservationsDataGrid();
			/*if (duplicateReservationsUsersDropDownList.Visible)
			{
				BindUsersDropDownList(duplicateReservationsUsersDropDownList, DuplicateReservationsList);
			}*/
		}

		protected void DeleteGlobalModerator(object sender, DataGridCommandEventArgs e)
		{
			//ModeratorList.RemoveAt(FindUserInfoIndexByUserId(ModeratorList, int.Parse(((Label)e.Item.FindControl("userId")).Text)));
			ModeratorList.RemoveAt(FindUserInfoIndexByEmail(ModeratorList, ((Label)e.Item.FindControl("email")).Text));
			BindModeratorsDataGrid();
			/*if (moderatorUsersDropDownList.Visible)
			{
				BindUsersDropDownList(moderatorUsersDropDownList, ModeratorList);
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
				foreach (WorkingHoursInfo moderationHour in ModerationHours)
				{
					if (moderationHour.DayOfWeek == dayOfWeek && (moderationHour.StartTime == timeSpan && moderationHour.EndTime == timeSpan1 || moderationHour.AllDay == flag))
					{
						break;
					}
					num++;
				}
				if (num < ModerationHours.Count)
				{
					ModerationHours.RemoveAt(num);
					BindModerationHoursDataGrid();
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
				IEnumerator enumerator = TimeOfDayList.GetEnumerator();
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
				if (num < TimeOfDayList.Count)
				{
					TimeOfDayList.RemoveAt(num);
					BindTimeOfDayDataGrid();
				}
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
			}
		}

		protected void DeleteUser(object sender, DataGridCommandEventArgs e)
		{
			//BCCList.RemoveAt(FindUserInfoIndexByUserId(BCCList, int.Parse(((Label)e.Item.FindControl("userId")).Text)));
			BCCList.RemoveAt(FindUserInfoIndexByEmail(BCCList, ((Label)e.Item.FindControl("email")).Text));
			BindBCCListDataGrid();
			/*if (usersDropDownList.Visible)
			{
				BindUsersDropDownList(usersDropDownList, BCCList);
			}*/
		}

		protected void DeleteViewReservations(object sender, DataGridCommandEventArgs e)
		{
			//ViewReservationsList.RemoveAt(FindUserInfoIndexByUserId(ViewReservationsList, int.Parse(((Label)e.Item.FindControl("userId")).Text)));
			ViewReservationsList.RemoveAt(FindUserInfoIndexByEmail(ViewReservationsList, ((Label)e.Item.FindControl("email")).Text));
			BindViewReservationsDataGrid();
			/*if (viewReservationsUsersDropDownList.Visible)
			{
				BindUsersDropDownList(viewReservationsUsersDropDownList, ViewReservationsList);
			}*/
		}

		protected void DeleteWorkingHours(object sender, DataGridCommandEventArgs e)
		{
			try
			{
				WorkingHours.RemoveAt(e.Item.ItemIndex);
				BindWorkingHoursDataGrid();
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
				foreach (WorkingHoursExceptionInfo workingHoursException in WorkingHoursExceptions)
				{
					if (workingHoursException.Date == dateTime && (workingHoursException.StartTime == timeSpan && workingHoursException.EndTime == timeSpan1 || workingHoursException.AllDay & flag))
					{
						break;
					}
					num++;
				}
				if (num < WorkingHoursExceptions.Count)
				{
					WorkingHoursExceptions.RemoveAt(num);
					BindWorkingHoursExceptionsDataGrid();
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
					return current;
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
			if (!Is24HourClock && amPmDropDownList.SelectedValue == "PM")
			{
				num = num + 12;
			}
			num1 = int.Parse(minuteDropDownList.SelectedValue);
			return new TimeSpan(num, num1, 0);
		}

		protected string GetTimeOfDay(TimeOfDayInfo timeOfDayInfo)
		{
			DateTime dateTime = new DateTime();
			dateTime = dateTime.Add(timeOfDayInfo.StartTime);
			string shortTimeString = dateTime.ToShortTimeString();
			dateTime = new DateTime();
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
				str = Localization.GetString("AllDay", LocalResourceFile);
			}
			else
			{
				string[] shortTimeString = new string[] { Localization.GetString("fromLabel", LocalResourceFile), " ", null, null, null, null, null };
				DateTime dateTime = new DateTime();
				dateTime = dateTime.Add(workingHoursInfo.StartTime);
				shortTimeString[2] = dateTime.ToShortTimeString();
				shortTimeString[3] = " ";
				shortTimeString[4] = Localization.GetString("toLabel", LocalResourceFile);
				shortTimeString[5] = " ";
				dateTime = new DateTime();
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
				str = Localization.GetString("AllDay", LocalResourceFile);
			}
			else if (workingHoursExceptionInfo.StartTime == workingHoursExceptionInfo.EndTime)
			{
				str = Localization.GetString("noWorkingHoursLabel", LocalResourceFile);
			}
			else
			{
				string[] shortTimeString = new string[] { Localization.GetString("fromLabel", LocalResourceFile), " ", null, null, null, null, null };
				date = new DateTime();
				date = date.Add(workingHoursExceptionInfo.StartTime);
				shortTimeString[2] = date.ToShortTimeString();
				shortTimeString[3] = " ";
				shortTimeString[4] = Localization.GetString("toLabel", LocalResourceFile);
				shortTimeString[5] = " ";
				date = new DateTime();
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
				if (!Page.IsPostBack)
				{
					BindGeneralSettingsSection();
					BindCategoriesSection();
					BindCategoryPermissionsSection();
					BindReservationSettingsSection();
					if (!IsProfessional)
					{
						feesSectionTableRow.Visible = false;
						cashierListSectionTableRow.Visible = false;
					}
					else
					{
						BindReservationFeesSection();
						BindCashierListSection();
					}
					BindWorkingHoursSection();
					BindWorkingHoursExceptionsSection();
					BindTimeOfDaySection();
					BindBCCListSection();
					BindModerationSection();
					BindViewReservationsSection();
					BindDuplicateReservationsSection();
					BindRemindersSection();
					BindMailTemplatesSection();
					BindSMSTemplatesSection();
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
			if (mailTemplateDropDownList.SelectedValue == "Confirmation")
			{
				mailTemplateSubjectTextBox.Text = ModuleSettings.ConfirmationMailSubject;
				mailTemplateBodyTextBox.Text = ModuleSettings.ConfirmationMailBody;
				RadioButton radioButton = mailTemplateBodyTypeHtmlRadioButton;
				RadioButton radioButton1 = mailTemplateBodyTypeTextRadioButton;
				bool lower = ModuleSettings.ConfirmationMailBodyType.ToLower() == "text";
				flag = lower;
				radioButton1.Checked = lower;
				radioButton.Checked = !flag;
				return;
			}
			if (mailTemplateDropDownList.SelectedValue == "Modification")
			{
				mailTemplateSubjectTextBox.Text = ModuleSettings.ModificationMailSubject;
				mailTemplateBodyTextBox.Text = ModuleSettings.ModificationMailBody;
				RadioButton radioButton2 = mailTemplateBodyTypeHtmlRadioButton;
				RadioButton radioButton3 = mailTemplateBodyTypeTextRadioButton;
				bool lower1 = ModuleSettings.ModificationMailBodyType.ToLower() == "text";
				flag = lower1;
				radioButton3.Checked = lower1;
				radioButton2.Checked = !flag;
				return;
			}
			if (mailTemplateDropDownList.SelectedValue == "Rescheduled")
			{
				mailTemplateSubjectTextBox.Text = ModuleSettings.RescheduledMailSubject;
				mailTemplateBodyTextBox.Text = ModuleSettings.RescheduledMailBody;
				RadioButton radioButton4 = mailTemplateBodyTypeHtmlRadioButton;
				RadioButton radioButton5 = mailTemplateBodyTypeTextRadioButton;
				bool flag1 = ModuleSettings.RescheduledMailBodyType.ToLower() == "text";
				flag = flag1;
				radioButton5.Checked = flag1;
				radioButton4.Checked = !flag;
				return;
			}
			if (mailTemplateDropDownList.SelectedValue == "Cancellation")
			{
				mailTemplateSubjectTextBox.Text = ModuleSettings.CancellationMailSubject;
				mailTemplateBodyTextBox.Text = ModuleSettings.CancellationMailBody;
				RadioButton radioButton6 = mailTemplateBodyTypeHtmlRadioButton;
				RadioButton radioButton7 = mailTemplateBodyTypeTextRadioButton;
				bool lower2 = ModuleSettings.CancellationMailBodyType.ToLower() == "text";
				flag = lower2;
				radioButton7.Checked = lower2;
				radioButton6.Checked = !flag;
				return;
			}
			if (mailTemplateDropDownList.SelectedValue == "Moderator")
			{
				mailTemplateSubjectTextBox.Text = ModuleSettings.ModeratorMailSubject;
				mailTemplateBodyTextBox.Text = ModuleSettings.ModeratorMailBody;
				RadioButton radioButton8 = mailTemplateBodyTypeHtmlRadioButton;
				RadioButton radioButton9 = mailTemplateBodyTypeTextRadioButton;
				bool flag2 = ModuleSettings.ModeratorMailBodyType.ToLower() == "text";
				flag = flag2;
				radioButton9.Checked = flag2;
				radioButton8.Checked = !flag;
				return;
			}
			if (mailTemplateDropDownList.SelectedValue == "Declined")
			{
				mailTemplateSubjectTextBox.Text = ModuleSettings.DeclinedMailSubject;
				mailTemplateBodyTextBox.Text = ModuleSettings.DeclinedMailBody;
				RadioButton radioButton10 = mailTemplateBodyTypeHtmlRadioButton;
				RadioButton radioButton11 = mailTemplateBodyTypeTextRadioButton;
				bool lower3 = ModuleSettings.DeclinedMailBodyType.ToLower() == "text";
				flag = lower3;
				radioButton11.Checked = lower3;
				radioButton10.Checked = !flag;
				return;
			}
			if (mailTemplateDropDownList.SelectedValue == "VerificationCode")
			{
				mailTemplateSubjectTextBox.Text = ModuleSettings.VerificationCodeMailSubject;
				mailTemplateBodyTextBox.Text = ModuleSettings.VerificationCodeMailBody;
				RadioButton radioButton12 = mailTemplateBodyTypeHtmlRadioButton;
				RadioButton radioButton13 = mailTemplateBodyTypeTextRadioButton;
				bool flag3 = ModuleSettings.VerificationCodeMailBodyType.ToLower() == "text";
				flag = flag3;
				radioButton13.Checked = flag3;
				radioButton12.Checked = !flag;
				return;
			}
			if (mailTemplateDropDownList.SelectedValue == "DuplicateReservation")
			{
				mailTemplateSubjectTextBox.Text = ModuleSettings.DuplicateReservationMailSubject;
				mailTemplateBodyTextBox.Text = ModuleSettings.DuplicateReservationMailBody;
				RadioButton radioButton14 = mailTemplateBodyTypeHtmlRadioButton;
				RadioButton radioButton15 = mailTemplateBodyTypeTextRadioButton;
				bool lower4 = ModuleSettings.DuplicateReservationMailBodyType.ToLower() == "text";
				flag = lower4;
				radioButton15.Checked = lower4;
				radioButton14.Checked = !flag;
				return;
			}
			if (mailTemplateDropDownList.SelectedValue == "Reminder")
			{
				mailTemplateSubjectTextBox.Text = ModuleSettings.ReminderMailSubject;
				mailTemplateBodyTextBox.Text = ModuleSettings.ReminderMailBody;
				RadioButton radioButton16 = mailTemplateBodyTypeHtmlRadioButton;
				RadioButton radioButton17 = mailTemplateBodyTypeTextRadioButton;
				bool flag4 = ModuleSettings.ReminderMailBodyType.ToLower() == "text";
				flag = flag4;
				radioButton17.Checked = flag4;
				radioButton16.Checked = !flag;
				return;
			}
			if (IsProfessional)
			{
				if (mailTemplateDropDownList.SelectedValue == "PendingRescheduleRefund")
				{
					mailTemplateSubjectTextBox.Text = ModuleSettings.PendingRescheduleRefundMailSubject;
					mailTemplateBodyTextBox.Text = ModuleSettings.PendingRescheduleRefundMailBody;
					RadioButton radioButton18 = mailTemplateBodyTypeHtmlRadioButton;
					RadioButton radioButton19 = mailTemplateBodyTypeTextRadioButton;
					bool lower5 = ModuleSettings.PendingRescheduleRefundMailBodyType.ToLower() == "text";
					flag = lower5;
					radioButton19.Checked = lower5;
					radioButton18.Checked = !flag;
					return;
				}
				if (mailTemplateDropDownList.SelectedValue == "PendingCancellationRefund")
				{
					mailTemplateSubjectTextBox.Text = ModuleSettings.PendingCancellationRefundMailSubject;
					mailTemplateBodyTextBox.Text = ModuleSettings.PendingCancellationRefundMailBody;
					RadioButton radioButton20 = mailTemplateBodyTypeHtmlRadioButton;
					RadioButton radioButton21 = mailTemplateBodyTypeTextRadioButton;
					bool flag5 = ModuleSettings.PendingCancellationRefundMailBodyType.ToLower() == "text";
					flag = flag5;
					radioButton21.Checked = flag5;
					radioButton20.Checked = !flag;
					return;
				}
				if (mailTemplateDropDownList.SelectedValue == "PendingDeclinationRefund")
				{
					mailTemplateSubjectTextBox.Text = ModuleSettings.PendingDeclinationRefundMailSubject;
					mailTemplateBodyTextBox.Text = ModuleSettings.PendingDeclinationRefundMailBody;
					RadioButton radioButton22 = mailTemplateBodyTypeHtmlRadioButton;
					RadioButton radioButton23 = mailTemplateBodyTypeTextRadioButton;
					bool lower6 = ModuleSettings.PendingDeclinationRefundMailBodyType.ToLower() == "text";
					flag = lower6;
					radioButton23.Checked = lower6;
					radioButton22.Checked = !flag;
				}
			}
		}

		protected void ModerateCheckBoxCheckChanged(object sender, EventArgs e)
		{
			HtmlTableRow htmlTableRow = moderationHoursTableRow;
			HtmlTableRow htmlTableRow1 = globalModeratorsDropDownListTableRow;
			HtmlTableRow htmlTableRow2 = globalModeratorsDataGridTableRow;
			CheckBox checkBox = moderateCheckBox;
			bool @checked = moderateCheckBox.Checked;
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
			BindModerationSection(int.Parse(moderationCategoryDropDownList.SelectedValue));
		}

		protected void ModerationResetCommandButtonClicked(object sender, EventArgs e)
		{
			int num = int.Parse(moderationCategoryDropDownList.SelectedValue);
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
			BindCategoriesDropDownList(moderationCategoryDropDownList, "Moderate", null, null);
			BindModerationSection(num);
		}

		protected void ModerationUpdateCommandButtonClicked(object sender, EventArgs e)
		{
			UpdateModerationSection(true);
		}

		protected void Page_Init(object sender, EventArgs e)
		{
			try
			{
				SetTheme();
				categoryPermissionsUpdateCommandButton.Click += new EventHandler(CategoryPermissionsUpdateCommandButtonClicked);
				categoryPermissionsResetCommandButton.Click += new EventHandler(CategoryPermissionsResetCommandButtonClicked);
				reservationSettingsUpdateCommandButton.Click += new EventHandler(ReservationSettingsUpdateCommandButtonClicked);
				reservationSettingsResetCommandButton.Click += new EventHandler(ReservationSettingsResetCommandButtonClicked);
				if (IsProfessional)
				{
					reservationFeesUpdateCommandButton.Click += new EventHandler(ReservationFeesUpdateCommandButtonClicked);
					reservationFeesResetCommandButton.Click += new EventHandler(ReservationFeesResetCommandButtonClicked);
					cashierListUpdateCommandButton.Click += new EventHandler(CashierListUpdateCommandButtonClicked);
					cashierListResetCommandButton.Click += new EventHandler(CashierListResetCommandButtonClicked);
				}
				recurrencepatterncontrol.RecurrencePatternSubmitted += new RecurrencePatternSubmitted(RecurrencePatternSubmitted);
				workingHoursUpdateCommandButton.Click += new EventHandler(WorkingHoursUpdateCommandButtonClicked);
				workingHoursResetCommandButton.Click += new EventHandler(WorkingHoursResetCommandButtonClicked);
				workingHoursExceptionsUpdateCommandButton.Click += new EventHandler(WorkingHoursExceptionsUpdateCommandButtonClicked);
				workingHoursExceptionsResetCommandButton.Click += new EventHandler(WorkingHoursExceptionsResetCommandButtonClicked);
				bccListUpdateCommandButton.Click += new EventHandler(BCCListUpdateCommandButtonClicked);
				bccListResetCommandButton.Click += new EventHandler(BCCListResetCommandButtonClicked);
				moderationUpdateCommandButton.Click += new EventHandler(ModerationUpdateCommandButtonClicked);
				moderationResetCommandButton.Click += new EventHandler(ModerationResetCommandButtonClicked);
				updateMailTemplateCommandButton.Click += new EventHandler(UpdateMailTemplateCommandButtonClicked);
				resetMailTemplateCommandButton.Click += new EventHandler(ResetMailTemplateCommandButtonClicked);
				updateSMSTemplateCommandButton.Click += new EventHandler(UpdateSMSTemplateCommandButtonClicked);
				resetSMSTemplateCommandButton.Click += new EventHandler(ResetSMSTemplateCommandButtonClicked);
				updateSettingsCommandButton.Click += new EventHandler(UpdateSettingsCommandButtonClicked);
				cancelSettingsCommandButton.Click += new EventHandler(CancelSettingsCommandButtonClicked);
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
				if (!Page.IsPostBack && Request.QueryString["ctl"].ToLower() == "editsettings")
				{
					LoadSettings();
					updateCancelTableRow.Visible = true;
				}
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
			}
		}

		protected void PaymentMethodChanged(object sender, EventArgs e)
		{
			HtmlTableRow htmlTableRow = payPalAccountTableRow;
			HtmlTableRow htmlTableRow1 = payPalUrlTableRow;
			PaymentMethod paymentMethod = PaymentMethod.PayPalPaymentsStandard;
			bool selectedValue = paymentMethodDropDownList.SelectedValue == paymentMethod.ToString();
			bool flag = selectedValue;
			htmlTableRow1.Visible = selectedValue;
			htmlTableRow.Visible = flag;
			HtmlTableRow htmlTableRow2 = authorizeNetApiLoginTableRow;
			HtmlTableRow htmlTableRow3 = authorizeNetMerchantHashTableRow;
			HtmlTableRow htmlTableRow4 = authorizeNetTestModeTableRow;
			HtmlTableRow htmlTableRow5 = authorizeNetTransactionKeyTableRow;
			paymentMethod = PaymentMethod.AuthorizeNetSIM;
			bool selectedValue1 = paymentMethodDropDownList.SelectedValue == paymentMethod.ToString();
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
			bindUponSelectionTableRow.Visible = (preventCrossCategoryConflictsCheckBox.Checked ? false : !selectCategoryLastCheckBox.Checked);
		}

		private void RebindCategoryDependentSections()
		{
			BindCategoryPermissionsSection();
			BindReservationSettingsSection();
			if (IsProfessional)
			{
				BindReservationFeesSection();
				BindCashierListSection();
			}
			BindWorkingHoursSection();
			BindWorkingHoursExceptionsSection();
			BindBCCListSection();
			BindModerationSection();
		}

		protected void RecurrencePatternSubmitted(IRecurrencePattern recurrencePattern)
		{
			WorkingHours.Add((RecurrencePattern)recurrencePattern);
			BindWorkingHoursDataGrid();
			recurrencepatterncontrol.Visible = false;
			addWorkingHoursCommandButton.Visible = true;
		}

		protected void RequireEmailCheckBoxCheckChanged(object sender, EventArgs e)
		{
			requireVerificationCodeTableRow.Visible = requireEmailCheckBox.Checked;
			requireVerificationCodeCheckBox.Checked = false;
		}

		protected void ReservationFeesCategoryDropDownListSelectedIndexChanged(object sender, EventArgs e)
		{
			BindReservationFeesSection(int.Parse(reservationFeesCategoryDropDownList.SelectedValue));
		}

		protected void ReservationFeesResetCommandButtonClicked(object sender, EventArgs e)
		{
			int num = int.Parse(reservationFeesCategoryDropDownList.SelectedValue);
			CategorySettingController categorySettingController = new CategorySettingController();
			categorySettingController.DeleteCategorySetting(num, "FeeScheduleType");
			categorySettingController.DeleteCategorySetting(num, "DepositFee");
			categorySettingController.DeleteCategorySetting(num, "SchedulingFee");
			categorySettingController.DeleteCategorySetting(num, "ReschedulingFee");
			categorySettingController.DeleteCategorySetting(num, "CancellationFee");
			categorySettingController.DeleteCategorySetting(num, "SchedulingFeeInterval");
			int num1 = 1;
			Hashtable settings = (new CategorySettings(PortalId, TabModuleId, num)).Settings;
			while (settings.ContainsKey(string.Concat("SeasonalFeeScheduleList.", num1)))
			{
				categorySettingController.DeleteCategorySetting(num, string.Concat("SeasonalFeeScheduleList.", num1));
				num1++;
			}
			BindCategoriesDropDownList(reservationFeesCategoryDropDownList, "SchedulingFee", "FeeScheduleType", null);
			BindReservationFeesSection(num);
		}

		protected void ReservationFeesUpdateCommandButtonClicked(object sender, EventArgs e)
		{
			UpdateReservationFeesSection();
		}

		protected void ReservationSettingsCategoryDropDownListSelectedIndexChanged(object sender, EventArgs e)
		{
			BindReservationSettingsSection(int.Parse(reservationSettingsCategoryDropDownList.SelectedValue));
		}

		protected void ReservationSettingsResetCommandButtonClicked(object sender, EventArgs e)
		{
			int num = int.Parse(reservationSettingsCategoryDropDownList.SelectedValue);
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
			BindCategoriesDropDownList(reservationSettingsCategoryDropDownList, "AllowCancellations", null, null);
			BindReservationSettingsSection(num);
		}

		protected void ReservationSettingsUpdateCommandButtonClicked(object sender, EventArgs e)
		{
			UpdateReservationSettingsSection(true);
		}

		protected void ResetMailTemplateCommandButtonClicked(object sender, EventArgs e)
		{
			bool flag;
			if (mailTemplateDropDownList.SelectedValue == "Confirmation")
			{
				mailTemplateSubjectTextBox.Text = Localization.GetString("ConfirmationMailSubject", LocalResourceFile);
				mailTemplateBodyTextBox.Text = Localization.GetString("ConfirmationMailBody", LocalResourceFile);
				RadioButton radioButton = mailTemplateBodyTypeHtmlRadioButton;
				RadioButton radioButton1 = mailTemplateBodyTypeTextRadioButton;
				bool lower = Localization.GetString("ConfirmationMailBodyType", LocalResourceFile).ToLower() == "text";
				flag = lower;
				radioButton1.Checked = lower;
				radioButton.Checked = !flag;
			}
			else if (mailTemplateDropDownList.SelectedValue == "Modification")
			{
				mailTemplateSubjectTextBox.Text = Localization.GetString("ModificationMailSubject", LocalResourceFile);
				mailTemplateBodyTextBox.Text = Localization.GetString("ModificationMailBody", LocalResourceFile);
				RadioButton radioButton2 = mailTemplateBodyTypeHtmlRadioButton;
				RadioButton radioButton3 = mailTemplateBodyTypeTextRadioButton;
				bool lower1 = Localization.GetString("ModificationMailBodyType", LocalResourceFile).ToLower() == "text";
				flag = lower1;
				radioButton3.Checked = lower1;
				radioButton2.Checked = !flag;
			}
			else if (mailTemplateDropDownList.SelectedValue == "Rescheduled")
			{
				mailTemplateSubjectTextBox.Text = Localization.GetString("RescheduledMailSubject", LocalResourceFile);
				mailTemplateBodyTextBox.Text = Localization.GetString("RescheduledMailBody", LocalResourceFile);
				RadioButton radioButton4 = mailTemplateBodyTypeHtmlRadioButton;
				RadioButton radioButton5 = mailTemplateBodyTypeTextRadioButton;
				bool flag1 = Localization.GetString("RescheduledMailBodyType", LocalResourceFile).ToLower() == "text";
				flag = flag1;
				radioButton5.Checked = flag1;
				radioButton4.Checked = !flag;
			}
			else if (mailTemplateDropDownList.SelectedValue == "Cancellation")
			{
				mailTemplateSubjectTextBox.Text = Localization.GetString("CancellationMailSubject", LocalResourceFile);
				mailTemplateBodyTextBox.Text = Localization.GetString("CancellationMailBody", LocalResourceFile);
				RadioButton radioButton6 = mailTemplateBodyTypeHtmlRadioButton;
				RadioButton radioButton7 = mailTemplateBodyTypeTextRadioButton;
				bool lower2 = Localization.GetString("CancellationMailBodyType", LocalResourceFile).ToLower() == "text";
				flag = lower2;
				radioButton7.Checked = lower2;
				radioButton6.Checked = !flag;
			}
			else if (mailTemplateDropDownList.SelectedValue == "Moderator")
			{
				mailTemplateSubjectTextBox.Text = Localization.GetString("ModeratorMailSubject", LocalResourceFile);
				mailTemplateBodyTextBox.Text = Localization.GetString("ModeratorMailBody", LocalResourceFile);
				RadioButton radioButton8 = mailTemplateBodyTypeHtmlRadioButton;
				RadioButton radioButton9 = mailTemplateBodyTypeTextRadioButton;
				bool flag2 = Localization.GetString("ModeratorMailBodyType", LocalResourceFile).ToLower() == "text";
				flag = flag2;
				radioButton9.Checked = flag2;
				radioButton8.Checked = !flag;
			}
			else if (mailTemplateDropDownList.SelectedValue == "Declined")
			{
				mailTemplateSubjectTextBox.Text = Localization.GetString("DeclinedMailSubject", LocalResourceFile);
				mailTemplateBodyTextBox.Text = Localization.GetString("DeclinedMailBody", LocalResourceFile);
				RadioButton radioButton10 = mailTemplateBodyTypeHtmlRadioButton;
				RadioButton radioButton11 = mailTemplateBodyTypeTextRadioButton;
				bool lower3 = Localization.GetString("DeclinedMailBodyType", LocalResourceFile).ToLower() == "text";
				flag = lower3;
				radioButton11.Checked = lower3;
				radioButton10.Checked = !flag;
			}
			else if (mailTemplateDropDownList.SelectedValue == "VerificationCode")
			{
				mailTemplateSubjectTextBox.Text = Localization.GetString("VerificationCodeMailSubject", LocalResourceFile);
				mailTemplateBodyTextBox.Text = Localization.GetString("VerificationCodeMailBody", LocalResourceFile);
				RadioButton radioButton12 = mailTemplateBodyTypeHtmlRadioButton;
				RadioButton radioButton13 = mailTemplateBodyTypeTextRadioButton;
				bool flag3 = Localization.GetString("VerificationCodeMailBodyType", LocalResourceFile).ToLower() == "text";
				flag = flag3;
				radioButton13.Checked = flag3;
				radioButton12.Checked = !flag;
			}
			else if (mailTemplateDropDownList.SelectedValue == "DuplicateReservation")
			{
				mailTemplateSubjectTextBox.Text = Localization.GetString("DuplicateReservationMailSubject", LocalResourceFile);
				mailTemplateBodyTextBox.Text = Localization.GetString("DuplicateReservationMailBody", LocalResourceFile);
				RadioButton radioButton14 = mailTemplateBodyTypeHtmlRadioButton;
				RadioButton radioButton15 = mailTemplateBodyTypeTextRadioButton;
				bool lower4 = Localization.GetString("DuplicateReservationMailBodyType", LocalResourceFile).ToLower() == "text";
				flag = lower4;
				radioButton15.Checked = lower4;
				radioButton14.Checked = !flag;
			}
			else if (mailTemplateDropDownList.SelectedValue == "Reminder")
			{
				mailTemplateSubjectTextBox.Text = Localization.GetString("ReminderMailSubject", LocalResourceFile);
				mailTemplateBodyTextBox.Text = Localization.GetString("ReminderMailBody", LocalResourceFile);
				RadioButton radioButton16 = mailTemplateBodyTypeHtmlRadioButton;
				RadioButton radioButton17 = mailTemplateBodyTypeTextRadioButton;
				bool flag4 = Localization.GetString("ReminderMailBodyType", LocalResourceFile).ToLower() == "text";
				flag = flag4;
				radioButton17.Checked = flag4;
				radioButton16.Checked = !flag;
			}
			else if (mailTemplateDropDownList.SelectedValue == "PendingRescheduleRefund")
			{
				mailTemplateSubjectTextBox.Text = Localization.GetString("PendingRescheduleRefundMailSubject", LocalResourceFile);
				mailTemplateBodyTextBox.Text = Localization.GetString("PendingRescheduleRefundMailBody", LocalResourceFile);
				RadioButton radioButton18 = mailTemplateBodyTypeHtmlRadioButton;
				RadioButton radioButton19 = mailTemplateBodyTypeTextRadioButton;
				bool lower5 = Localization.GetString("PendingRescheduleRefundMailBodyType", LocalResourceFile).ToLower() == "text";
				flag = lower5;
				radioButton19.Checked = lower5;
				radioButton18.Checked = !flag;
			}
			else if (mailTemplateDropDownList.SelectedValue == "PendingCancellationRefund")
			{
				mailTemplateSubjectTextBox.Text = Localization.GetString("PendingCancellationRefundMailSubject", LocalResourceFile);
				mailTemplateBodyTextBox.Text = Localization.GetString("PendingCancellationRefundMailBody", LocalResourceFile);
				RadioButton radioButton20 = mailTemplateBodyTypeHtmlRadioButton;
				RadioButton radioButton21 = mailTemplateBodyTypeTextRadioButton;
				bool flag5 = Localization.GetString("PendingCancellationRefundMailBodyType", LocalResourceFile).ToLower() == "text";
				flag = flag5;
				radioButton21.Checked = flag5;
				radioButton20.Checked = !flag;
			}
			else if (mailTemplateDropDownList.SelectedValue == "PendingDeclinationRefund")
			{
				mailTemplateSubjectTextBox.Text = Localization.GetString("PendingDeclinationRefundMailSubject", LocalResourceFile);
				mailTemplateBodyTextBox.Text = Localization.GetString("PendingDeclinationRefundMailBody", LocalResourceFile);
				RadioButton radioButton22 = mailTemplateBodyTypeHtmlRadioButton;
				RadioButton radioButton23 = mailTemplateBodyTypeTextRadioButton;
				bool lower6 = Localization.GetString("PendingDeclinationRefundMailBodyType", LocalResourceFile).ToLower() == "text";
				flag = lower6;
				radioButton23.Checked = lower6;
				radioButton22.Checked = !flag;
			}
			iCalendarAttachmentFileNameTextBox.Text = Localization.GetString("ICalendarAttachmentFileName", LocalResourceFile);
			UpdateMailTemplateCommandButtonClicked(sender, e);
		}

		protected void ResetSMSTemplateCommandButtonClicked(object sender, EventArgs e)
		{
			if (smsTemplateDropDownList.SelectedValue == "Reminder")
			{
				smsTemplateBodyTextBox.Text = Localization.GetString("ReminderSMS", LocalResourceFile);
			}
			UpdateSMSTemplateCommandButtonClicked(sender, e);
		}

		protected override object SaveViewState()
		{
			try
			{
				ViewState["CategoryList"] = SerializeCategoryList(CategoryList);
				//ViewState["CashierList"] = ModuleSettings.SerializeUserIDList(CashierList);
				ViewState["CashierList"] = ModuleSettings.SerializeEmailList(CashierList);
				//ViewState["BCCList"] = ModuleSettings.SerializeUserIDList(BCCList);
				ViewState["BCCList"] = ModuleSettings.SerializeEmailList(BCCList);
				ViewState["TimeOfDayList"] = ModuleSettings.SerializeTimeOfDayList(TimeOfDayList);
				//ViewState["ModeratorList"] = ModuleSettings.SerializeUserIDList(ModeratorList);
				ViewState["ModeratorList"] = ModuleSettings.SerializeEmailList(ModeratorList);
				//ViewState["ViewReservationsList"] = ModuleSettings.SerializeUserIDList(ViewReservationsList);
				ViewState["ViewReservationsList"] = ModuleSettings.SerializeEmailList(ViewReservationsList);
				//ViewState["DuplicateReservationsList"] = ModuleSettings.SerializeUserIDList(DuplicateReservationsList);
				ViewState["DuplicateReservationsList"] = ModuleSettings.SerializeEmailList(DuplicateReservationsList);
				ViewState["WorkingHours"] = SerializeRecurrencePatternList(WorkingHours);
				ViewState["WorkingHoursExceptions"] = SerializeWorkingHoursExceptions(WorkingHoursExceptions);
				ViewState["ModerationHours"] = SerializeWorkingHours(ModerationHours);
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
			}
			return base.SaveViewState();
		}

		protected void SelectCategoryLastChanged(object sender, EventArgs e)
		{
			bindUponSelectionTableRow.Visible = (preventCrossCategoryConflictsCheckBox.Checked ? false : !selectCategoryLastCheckBox.Checked);
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
			HtmlTableRow htmlTableRow = requireConfirmationTableRow2;
			HtmlTableRow htmlTableRow1 = requireConfirmationWhenTableRow;
			bool @checked = requireConfirmationCheckBox.Checked;
			bool flag = @checked;
			htmlTableRow1.Visible = @checked;
			htmlTableRow.Visible = flag;
		}

		protected void ShowHideSendRemindersWhenTableRow(object sender, EventArgs e)
		{
			bool flag;
			HtmlTableRow htmlTableRow = requireConfirmationTableRow;
			HtmlTableRow htmlTableRow1 = requireConfirmationWhenTableRow;
			HtmlTableRow htmlTableRow2 = sendRemindersWhenTableRow;
			HtmlTableRow htmlTableRow3 = sendRemindersViaTableRow;
			bool @checked = sendRemindersCheckBox.Checked;
			bool flag1 = @checked;
			htmlTableRow3.Visible = @checked;
			bool flag2 = flag1;
			bool flag3 = flag2;
			htmlTableRow2.Visible = flag2;
			bool flag4 = flag3;
			bool flag5 = flag4;
			htmlTableRow1.Visible = flag4;
			htmlTableRow.Visible = flag5;
			HtmlTableRow htmlTableRow4 = requireConfirmationTableRow2;
			HtmlTableRow htmlTableRow5 = requireConfirmationWhenTableRow;
			flag = (!requireConfirmationTableRow.Visible ? false : requireConfirmationCheckBox.Checked);
			flag5 = flag;
			htmlTableRow5.Visible = flag;
			htmlTableRow4.Visible = flag5;
			sendRemindersViaTableRow.Visible = (!sendRemindersViaTableRow.Visible ? false : IsProfessional);
		}

		protected void ShowHideTimeOfDayTableRow(object sender, EventArgs e)
		{
			HtmlTableRow htmlTableRow = timeOfDaySelectionModeTableRow;
			HtmlTableRow htmlTableRow1 = displayUnavailableTimeOfDayTableRow;
			HtmlTableRow htmlTableRow2 = timeOfDayTableRow;
			bool @checked = displayTimeOfDayCheckBox.Checked;
			bool flag = @checked;
			htmlTableRow2.Visible = @checked;
			bool flag1 = flag;
			bool flag2 = flag1;
			htmlTableRow1.Visible = flag1;
			htmlTableRow.Visible = flag2;
		}

		protected void SMSTemplateDropDownListSelectedIndexChanged(object sender, EventArgs e)
		{
			if (smsTemplateDropDownList.SelectedValue == "Reminder")
			{
				smsTemplateBodyTextBox.Text = ModuleSettings.ReminderSMS;
			}
		}

		protected string TimeSpanToString(TimeSpan timeSpan)
		{
			double totalMinutes;
			if (timeSpan.TotalMinutes == 0)
			{
				totalMinutes = timeSpan.TotalMinutes;
				return string.Concat(totalMinutes.ToString(), " ", Localization.GetString("Minutes", LocalResourceFile));
			}
			if (Convert.ToInt32(timeSpan.TotalMinutes) % 1440 == 0)
			{
				totalMinutes = timeSpan.TotalDays;
				return string.Concat(totalMinutes.ToString(), " ", Localization.GetString("Days", LocalResourceFile));
			}
			if (Convert.ToInt32(timeSpan.TotalMinutes) % 60 == 0)
			{
				totalMinutes = timeSpan.TotalHours;
				return string.Concat(totalMinutes.ToString(), " ", Localization.GetString("Hours", LocalResourceFile));
			}
			totalMinutes = timeSpan.TotalMinutes;
			return string.Concat(totalMinutes.ToString(), " ", Localization.GetString("Minutes", LocalResourceFile));
		}

		private void UnhighlightInvalidControl(WebControl control)
		{
			control.CssClass = control.CssClass.Replace(" Gafware_Modules_Reservations_Invalid", string.Empty);
		}

		protected void UpdateBCCListSection(bool updateSelectedCategorySettings)
		{
			if (Page.IsValid)
			{
				int num = (bccListCategoryDropDownList.SelectedValue == null || !(bccListCategoryDropDownList.SelectedValue != string.Empty) ? Null.NullInteger : int.Parse(bccListCategoryDropDownList.SelectedValue));
				if (num != Null.NullInteger)
				{
					if (!updateSelectedCategorySettings)
					{
						return;
					}
					//(new CategorySettingController()).UpdateCategorySetting(num, "BCCList", ModuleSettings.SerializeUserIDList(BCCList));
					(new CategorySettingController()).UpdateCategorySetting(num, "BCCList", ModuleSettings.SerializeEmailList(BCCList));
				}
				else
				{
					//(new ModuleController()).UpdateTabModuleSetting(TabModuleId, "BCCList", ModuleSettings.SerializeUserIDList(BCCList));
					(new ModuleController()).UpdateTabModuleSetting(TabModuleId, "BCCList", ModuleSettings.SerializeEmailList(BCCList));
					ModuleController.SynchronizeModule(ModuleId);
					_ModuleSettings = null;
				}
				BindCategoriesDropDownList(bccListCategoryDropDownList, "BCCList", null, null);
				BindBCCListSection(num);
			}
		}

		protected void UpdateCashierListSection(bool updateSelectedCategorySettings)
		{
			if (Page.IsValid)
			{
				int num = (cashierListCategoryDropDownList.SelectedValue == null || !(cashierListCategoryDropDownList.SelectedValue != string.Empty) ? Null.NullInteger : int.Parse(cashierListCategoryDropDownList.SelectedValue));
				if (num != Null.NullInteger)
				{
					if (!updateSelectedCategorySettings)
					{
						return;
					}
					(new CategorySettingController()).UpdateCategorySetting(num, "CashierList", ModuleSettings.SerializeUserIDList(CashierList));
				}
				else
				{
					(new ModuleController()).UpdateTabModuleSetting(TabModuleId, "CashierList", ModuleSettings.SerializeUserIDList(CashierList));
					ModuleController.SynchronizeModule(ModuleId);
					_ModuleSettings = null;
				}
				BindCategoriesDropDownList(cashierListCategoryDropDownList, "CashierList", null, null);
				BindCashierListSection(num);
			}
		}

		protected void UpdateCategoryPermissionsSection(bool updateSelectedCategorySettings)
		{
			if (Page.IsValid)
			{
				foreach (ListItem item in categoryPermissionsCheckboxList.Items)
				{
					if (!item.Selected || CategoryPermissionsList.IndexOf(int.Parse(item.Value)) != -1)
					{
						if (item.Selected || CategoryPermissionsList.IndexOf(int.Parse(item.Value)) == -1)
						{
							continue;
						}
						CategoryPermissionsList.Remove(int.Parse(item.Value));
					}
					else
					{
						CategoryPermissionsList.Add(int.Parse(item.Value));
					}
				}
				int num = (categoryPermissionsDropDownList.SelectedValue == null || !(categoryPermissionsDropDownList.SelectedValue != string.Empty) ? Null.NullInteger : int.Parse(categoryPermissionsDropDownList.SelectedValue));
				if (num != Null.NullInteger)
				{
					if (!updateSelectedCategorySettings)
					{
						return;
					}
					(new CategorySettingController()).UpdateCategorySetting(num, "CategoryPermissions", ModuleSettings.SerializeRoleIDList(CategoryPermissionsList));
				}
				else
				{
					(new ModuleController()).UpdateTabModuleSetting(TabModuleId, "CategoryPermissions", ModuleSettings.SerializeRoleIDList(CategoryPermissionsList));
					ModuleController.SynchronizeModule(ModuleId);
					_ModuleSettings = null;
				}
				BindCategoriesDropDownList(categoryPermissionsDropDownList, "CategoryPermissions", null, null);
				BindCategoryPermissionsSection(num);
			}
		}

		protected void UpdateDuplicateReservationsSection()
		{
			if (Page.IsValid)
			{
				(new ModuleController()).UpdateTabModuleSetting(TabModuleId, "DuplicateReservationsList", ModuleSettings.SerializeUserIDList(DuplicateReservationsList));
				ModuleController.SynchronizeModule(ModuleId);
				_ModuleSettings = null;
			}
		}

		protected void UpdateMailTemplateCommandButtonClicked(object sender, EventArgs e)
		{
			if (Page.IsValid)
			{
				ModuleController moduleController = new ModuleController();
				if (mailTemplateDropDownList.SelectedValue == "Confirmation")
				{
					moduleController.UpdateTabModuleSetting(TabModuleId, "ConfirmationMailSubject", mailTemplateSubjectTextBox.Text);
					moduleController.UpdateTabModuleSetting(TabModuleId, "ConfirmationMailBody", mailTemplateBodyTextBox.Text);
					moduleController.UpdateTabModuleSetting(TabModuleId, "ConfirmationMailBodyType", (mailTemplateBodyTypeHtmlRadioButton.Checked ? "HTML" : "Text"));
				}
				else if (mailTemplateDropDownList.SelectedValue == "Modification")
				{
					moduleController.UpdateTabModuleSetting(TabModuleId, "ModificationMailSubject", mailTemplateSubjectTextBox.Text);
					moduleController.UpdateTabModuleSetting(TabModuleId, "ModificationMailBody", mailTemplateBodyTextBox.Text);
					moduleController.UpdateTabModuleSetting(TabModuleId, "ModificationMailBodyType", (mailTemplateBodyTypeHtmlRadioButton.Checked ? "HTML" : "Text"));
				}
				else if (mailTemplateDropDownList.SelectedValue == "Rescheduled")
				{
					moduleController.UpdateTabModuleSetting(TabModuleId, "RescheduledMailSubject", mailTemplateSubjectTextBox.Text);
					moduleController.UpdateTabModuleSetting(TabModuleId, "RescheduledMailBody", mailTemplateBodyTextBox.Text);
					moduleController.UpdateTabModuleSetting(TabModuleId, "RescheduledMailBodyType", (mailTemplateBodyTypeHtmlRadioButton.Checked ? "HTML" : "Text"));
				}
				else if (mailTemplateDropDownList.SelectedValue == "Cancellation")
				{
					moduleController.UpdateTabModuleSetting(TabModuleId, "CancellationMailSubject", mailTemplateSubjectTextBox.Text);
					moduleController.UpdateTabModuleSetting(TabModuleId, "CancellationMailBody", mailTemplateBodyTextBox.Text);
					moduleController.UpdateTabModuleSetting(TabModuleId, "CancellationMailBodyType", (mailTemplateBodyTypeHtmlRadioButton.Checked ? "HTML" : "Text"));
				}
				else if (mailTemplateDropDownList.SelectedValue == "Moderator")
				{
					moduleController.UpdateTabModuleSetting(TabModuleId, "ModeratorMailSubject", mailTemplateSubjectTextBox.Text);
					moduleController.UpdateTabModuleSetting(TabModuleId, "ModeratorMailBody", mailTemplateBodyTextBox.Text);
					moduleController.UpdateTabModuleSetting(TabModuleId, "ModeratorMailBodyType", (mailTemplateBodyTypeHtmlRadioButton.Checked ? "HTML" : "Text"));
				}
				else if (mailTemplateDropDownList.SelectedValue == "Declined")
				{
					moduleController.UpdateTabModuleSetting(TabModuleId, "DeclinedMailSubject", mailTemplateSubjectTextBox.Text);
					moduleController.UpdateTabModuleSetting(TabModuleId, "DeclinedMailBody", mailTemplateBodyTextBox.Text);
					moduleController.UpdateTabModuleSetting(TabModuleId, "DeclinedMailBodyType", (mailTemplateBodyTypeHtmlRadioButton.Checked ? "HTML" : "Text"));
				}
				else if (mailTemplateDropDownList.SelectedValue == "VerificationCode")
				{
					moduleController.UpdateTabModuleSetting(TabModuleId, "VerificationCodeMailSubject", mailTemplateSubjectTextBox.Text);
					moduleController.UpdateTabModuleSetting(TabModuleId, "VerificationCodeMailBody", mailTemplateBodyTextBox.Text);
					moduleController.UpdateTabModuleSetting(TabModuleId, "VerificationCodeMailBodyType", (mailTemplateBodyTypeHtmlRadioButton.Checked ? "HTML" : "Text"));
				}
				else if (mailTemplateDropDownList.SelectedValue == "DuplicateReservation")
				{
					moduleController.UpdateTabModuleSetting(TabModuleId, "DuplicateReservationMailSubject", mailTemplateSubjectTextBox.Text);
					moduleController.UpdateTabModuleSetting(TabModuleId, "DuplicateReservationMailBody", mailTemplateBodyTextBox.Text);
					moduleController.UpdateTabModuleSetting(TabModuleId, "DuplicateReservationMailBodyType", (mailTemplateBodyTypeHtmlRadioButton.Checked ? "HTML" : "Text"));
				}
				else if (mailTemplateDropDownList.SelectedValue == "Reminder")
				{
					moduleController.UpdateTabModuleSetting(TabModuleId, "ReminderMailSubject", mailTemplateSubjectTextBox.Text);
					moduleController.UpdateTabModuleSetting(TabModuleId, "ReminderMailBody", mailTemplateBodyTextBox.Text);
					moduleController.UpdateTabModuleSetting(TabModuleId, "ReminderMailBodyType", (mailTemplateBodyTypeHtmlRadioButton.Checked ? "HTML" : "Text"));
				}
				else if (IsProfessional)
				{
					if (mailTemplateDropDownList.SelectedValue == "PendingRescheduleRefund")
					{
						moduleController.UpdateTabModuleSetting(TabModuleId, "PendingRescheduleRefundMailSubject", mailTemplateSubjectTextBox.Text);
						moduleController.UpdateTabModuleSetting(TabModuleId, "PendingRescheduleRefundMailBody", mailTemplateBodyTextBox.Text);
						moduleController.UpdateTabModuleSetting(TabModuleId, "PendingRescheduleRefundMailBodyType", (mailTemplateBodyTypeHtmlRadioButton.Checked ? "HTML" : "Text"));
					}
					else if (mailTemplateDropDownList.SelectedValue == "PendingCancellationRefund")
					{
						moduleController.UpdateTabModuleSetting(TabModuleId, "PendingCancellationRefundMailSubject", mailTemplateSubjectTextBox.Text);
						moduleController.UpdateTabModuleSetting(TabModuleId, "PendingCancellationRefundMailBody", mailTemplateBodyTextBox.Text);
						moduleController.UpdateTabModuleSetting(TabModuleId, "PendingCancellationRefundMailBodyType", (mailTemplateBodyTypeHtmlRadioButton.Checked ? "HTML" : "Text"));
					}
					else if (mailTemplateDropDownList.SelectedValue == "PendingDeclinationRefund")
					{
						moduleController.UpdateTabModuleSetting(TabModuleId, "PendingDeclinationRefundMailSubject", mailTemplateSubjectTextBox.Text);
						moduleController.UpdateTabModuleSetting(TabModuleId, "PendingDeclinationRefundMailBody", mailTemplateBodyTextBox.Text);
						moduleController.UpdateTabModuleSetting(TabModuleId, "PendingDeclinationRefundMailBodyType", (mailTemplateBodyTypeHtmlRadioButton.Checked ? "HTML" : "Text"));
					}
				}
				moduleController.UpdateTabModuleSetting(TabModuleId, "MailFrom", mailFromTextBox.Text);
				int tabModuleId = TabModuleId;
				bool @checked = attachICalendarCheckBox.Checked;
				moduleController.UpdateTabModuleSetting(tabModuleId, "AttachiCalendar", @checked.ToString());
				moduleController.UpdateTabModuleSetting(TabModuleId, "ICalendarAttachmentFileName", iCalendarAttachmentFileNameTextBox.Text);
				ModuleController.SynchronizeModule(ModuleId);
				_ModuleSettings = null;
			}
		}

		protected void UpdateModerationSection(bool updateSelectedCategorySettings)
		{
			bool @checked;
			if (Page.IsValid)
			{
				int num = (moderationCategoryDropDownList.SelectedValue == null || !(moderationCategoryDropDownList.SelectedValue != string.Empty) ? Null.NullInteger : int.Parse(moderationCategoryDropDownList.SelectedValue));
				ModuleController moduleController = null;
				CategorySettingController categorySettingController = null;
				if (num != Null.NullInteger)
				{
					if (!updateSelectedCategorySettings)
					{
						return;
					}
					categorySettingController = new CategorySettingController();
					@checked = moderateCheckBox.Checked;
					categorySettingController.UpdateCategorySetting(num, "Moderate", @checked.ToString());
					categorySettingController.UpdateCategorySetting(num, "GlobalModeratorList", ModuleSettings.SerializeUserIDList(ModeratorList));
				}
				else
				{
					moduleController = new ModuleController();
					int tabModuleId = TabModuleId;
					@checked = moderateCheckBox.Checked;
					moduleController.UpdateTabModuleSetting(tabModuleId, "Moderate", @checked.ToString());
					moduleController.UpdateTabModuleSetting(TabModuleId, "GlobalModeratorList", ModuleSettings.SerializeUserIDList(ModeratorList));
				}
				ModerationHours.Sort(new WorkingHourInfoComparer());
				foreach (DayOfWeek value in Enum.GetValues(typeof(DayOfWeek)))
				{
					string empty = string.Empty;
					foreach (WorkingHoursInfo moderationHour in ModerationHours)
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
						moduleController.UpdateTabModuleSetting(TabModuleId, string.Concat("Moderation.", value.ToString()), empty);
					}
				}
				if (moduleController != null)
				{
					ModuleController.SynchronizeModule(ModuleId);
					_ModuleSettings = null;
				}
				BindCategoriesDropDownList(moderationCategoryDropDownList, "Moderate", null, null);
				BindModerationSection(num);
			}
		}

		protected void UpdateReservationFeesSection()
		{
			FeeScheduleType feeScheduleType;
			decimal depositFee;
			int interval;
			int j;
			int i;
			if (Page.IsValid)
			{
				ModuleController moduleController = new ModuleController();
				moduleController.UpdateTabModuleSetting(TabModuleId, "PaymentMethod", paymentMethodDropDownList.SelectedValue);
				moduleController.UpdateTabModuleSetting(TabModuleId, "AuthorizeNetApiLogin", authorizeNetApiLoginTextBox.Text);
				moduleController.UpdateTabModuleSetting(TabModuleId, "AuthorizeNetTransactionKey", authorizeNetTransactionKeyTextBox.Text);
				moduleController.UpdateTabModuleSetting(TabModuleId, "AuthorizeNetMerchantHash", authorizeNetMerchantHashTextBox.Text);
				int tabModuleId = TabModuleId;
				bool @checked = authorizeNetTestModeCheckBox.Checked;
				moduleController.UpdateTabModuleSetting(tabModuleId, "AuthorizeNetTestMode", @checked.ToString());
				moduleController.UpdateTabModuleSetting(TabModuleId, "PayPalAccount", payPalAccountTextBox.Text);
				moduleController.UpdateTabModuleSetting(TabModuleId, "PayPalSite", payPalUrlTextBox.Text);
				moduleController.UpdateTabModuleSetting(TabModuleId, "PayPalItemDescription", payPalItemDescriptionTextBox.Text);
				moduleController.UpdateTabModuleSetting(TabModuleId, "PendingPaymentExpiration", pendingPaymentExpirationTextBox.Text);
				moduleController.UpdateTabModuleSetting(TabModuleId, "Currency", currencyDropDownList.SelectedValue);
				int num = TabModuleId;
				@checked = allowPayLaterCheckBox.Checked;
				moduleController.UpdateTabModuleSetting(num, "AllowPayLater", @checked.ToString());
				int num1 = (reservationFeesCategoryDropDownList.SelectedValue == null || !(reservationFeesCategoryDropDownList.SelectedValue != string.Empty) ? Null.NullInteger : int.Parse(reservationFeesCategoryDropDownList.SelectedValue));
				if (num1 != Null.NullInteger)
				{
					CategorySettingController categorySettingController = new CategorySettingController();
					feeScheduleType = feeschedulecontrol.GetFeeScheduleType();
					categorySettingController.UpdateCategorySetting(num1, "FeeScheduleType", feeScheduleType.ToString());
					if (feeschedulecontrol.GetFeeScheduleType() == FeeScheduleType.Flat)
					{
						FlatFeeScheduleInfo flatFeeScheduleInfo = feeschedulecontrol.GetFlatFeeScheduleInfo();
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
					else if (feeschedulecontrol.GetFeeScheduleType() == FeeScheduleType.Seasonal)
					{
						Hashtable settings = (new CategorySettings(PortalId, TabModuleId, num1)).Settings;
						for (i = 1; settings.ContainsKey(string.Concat("SeasonalFeeScheduleList.", i)); i++)
						{
							categorySettingController.DeleteCategorySetting(num1, string.Concat("SeasonalFeeScheduleList.", i));
						}
						i = 1;
						foreach (string str in Helper.ChunksUpto(Helper.SerializeSeasonalFeeScheduleList(feeschedulecontrol.SeasonalFeeScheduleList), 2000))
						{
							categorySettingController.UpdateCategorySetting(num1, string.Concat("SeasonalFeeScheduleList.", i), str);
							i++;
						}
					}
				}
				else
				{
					int tabModuleId1 = TabModuleId;
					feeScheduleType = feeschedulecontrol.GetFeeScheduleType();
					moduleController.UpdateTabModuleSetting(tabModuleId1, "FeeScheduleType", feeScheduleType.ToString());
					if (feeschedulecontrol.GetFeeScheduleType() == FeeScheduleType.Flat)
					{
						FlatFeeScheduleInfo flatFeeScheduleInfo1 = feeschedulecontrol.GetFlatFeeScheduleInfo();
						int tabModuleId2 = TabModuleId;
						depositFee = flatFeeScheduleInfo1.DepositFee;
						moduleController.UpdateTabModuleSetting(tabModuleId2, "DepositFee", depositFee.ToString(CultureInfo.InvariantCulture));
						int num2 = TabModuleId;
						depositFee = flatFeeScheduleInfo1.ReservationFee;
						moduleController.UpdateTabModuleSetting(num2, "SchedulingFee", depositFee.ToString(CultureInfo.InvariantCulture));
						int tabModuleId3 = TabModuleId;
						depositFee = flatFeeScheduleInfo1.ReschedulingFee;
						moduleController.UpdateTabModuleSetting(tabModuleId3, "ReschedulingFee", depositFee.ToString(CultureInfo.InvariantCulture));
						int num3 = TabModuleId;
						depositFee = flatFeeScheduleInfo1.CancellationFee;
						moduleController.UpdateTabModuleSetting(num3, "CancellationFee", depositFee.ToString(CultureInfo.InvariantCulture));
						int tabModuleId4 = TabModuleId;
						interval = flatFeeScheduleInfo1.Interval;
						moduleController.UpdateTabModuleSetting(tabModuleId4, "SchedulingFeeInterval", interval.ToString());
					}
					else if (feeschedulecontrol.GetFeeScheduleType() == FeeScheduleType.Seasonal)
					{
						for (j = 1; Settings.ContainsKey(string.Concat("SeasonalFeeScheduleList.", j)); j++)
						{
							moduleController.DeleteTabModuleSetting(TabModuleId, string.Concat("SeasonalFeeScheduleList.", j));
						}
						j = 1;
						foreach (string str1 in Helper.ChunksUpto(Helper.SerializeSeasonalFeeScheduleList(feeschedulecontrol.SeasonalFeeScheduleList), 2000))
						{
							moduleController.UpdateTabModuleSetting(TabModuleId, string.Concat("SeasonalFeeScheduleList.", j), str1);
							j++;
						}
					}
				}
				ModuleController.SynchronizeModule(ModuleId);
				_ModuleSettings = null;
				BindCategoriesDropDownList(reservationFeesCategoryDropDownList, "SchedulingFee", "FeeScheduleType", null);
				BindReservationFeesSection(num1);
			}
		}

		protected void UpdateReservationSettingsSection(bool updateSelectedCategorySettings)
		{
			if (Page.IsValid)
			{
				int category = (reservationSettingsCategoryDropDownList.SelectedValue == null || !(reservationSettingsCategoryDropDownList.SelectedValue != string.Empty) ? Null.NullInteger : int.Parse(reservationSettingsCategoryDropDownList.SelectedValue));
				TimeSpan minTimeAhead = GetTimeSpan(minTimeAheadTextBox, minTimeAheadDropDownList);
				double reservationInterval = GetTimeSpan(reservationIntervalTextBox, reservationIntervalDropDownList).TotalMinutes;
				double reservationDuration = GetTimeSpan(reservationDurationTextBox, reservationDurationDropDownList).TotalMinutes;
				double reservationDurationMax = GetTimeSpan(reservationDurationMaxTextBox, reservationDurationMaxDropDownList).TotalMinutes;
				double reservationDurationInterval = GetTimeSpan(reservationDurationIntervalTextBox, reservationDurationIntervalDropDownList).TotalMinutes;
				bool allowCancellations = allowCancellationsCheckBox.Checked;
				bool allowRescheduling = allowReschedulingCheckBox.Checked;
				if (category != Null.NullInteger)
				{
					if (!updateSelectedCategorySettings)
					{
						return;
					}
					CategorySettingController categorySettingController = new CategorySettingController();
					categorySettingController.UpdateCategorySetting(category, "AllowCancellations", allowCancellations.ToString());
					categorySettingController.UpdateCategorySetting(category, "AllowRescheduling", allowRescheduling.ToString());
					categorySettingController.UpdateCategorySetting(category, "ReservationInterval", reservationInterval.ToString());
					categorySettingController.UpdateCategorySetting(category, "ReservationDuration", reservationDuration.ToString());
					categorySettingController.UpdateCategorySetting(category, "ReservationDurationMax", reservationDurationMax.ToString());
					categorySettingController.UpdateCategorySetting(category, "ReservationDurationInterval", reservationDurationInterval.ToString());
					categorySettingController.UpdateCategorySetting(category, "MinTimeAhead", minTimeAhead.ToString());
					categorySettingController.UpdateCategorySetting(category, "DaysAhead", daysAheadTextBox.Text);
					categorySettingController.UpdateCategorySetting(category, "MaxReservationsPerTimeSlot", maxConflictingReservationsTextBox.Text);
					categorySettingController.UpdateCategorySetting(category, "MaxReservationsPerUser", (string.IsNullOrEmpty(maxReservationsPerUserTextBox.Text.Trim()) ? Null.NullInteger.ToString() : maxReservationsPerUserTextBox.Text.Trim()));
				}
				else
				{
					ModuleController moduleController = new ModuleController();
					moduleController.UpdateTabModuleSetting(TabModuleId, "AllowCancellations", allowCancellations.ToString());
					moduleController.UpdateTabModuleSetting(TabModuleId, "AllowRescheduling", allowRescheduling.ToString());
					moduleController.UpdateTabModuleSetting(TabModuleId, "ReservationInterval", reservationInterval.ToString());
					moduleController.UpdateTabModuleSetting(TabModuleId, "ReservationDuration", reservationDuration.ToString());
					moduleController.UpdateTabModuleSetting(TabModuleId, "ReservationDurationMax", reservationDurationMax.ToString());
					moduleController.UpdateTabModuleSetting(TabModuleId, "ReservationDurationInterval", reservationDurationInterval.ToString());
					moduleController.UpdateTabModuleSetting(TabModuleId, "MinTimeAhead", minTimeAhead.ToString());
					moduleController.UpdateTabModuleSetting(TabModuleId, "DaysAhead", daysAheadTextBox.Text);
					moduleController.UpdateTabModuleSetting(TabModuleId, "MaxReservationsPerTimeSlot", maxConflictingReservationsTextBox.Text);
					moduleController.UpdateTabModuleSetting(TabModuleId, "MaxReservationsPerUser", (string.IsNullOrEmpty(maxReservationsPerUserTextBox.Text.Trim()) ? Null.NullInteger.ToString() : maxReservationsPerUserTextBox.Text.Trim()));
					ModuleController.SynchronizeModule(ModuleId);
					_ModuleSettings = null;
				}
				BindCategoriesDropDownList(reservationSettingsCategoryDropDownList, "AllowCancellations", null, null);
				BindReservationSettingsSection(category);
			}
		}

		public override void UpdateSettings()
		{
			try
			{
				if (Page.IsValid)
				{
					ModuleController moduleController = new ModuleController();
					UpdateCategoryPermissionsSection(false);
					UpdateReservationSettingsSection(false);
					if (IsProfessional)
					{
						UpdateReservationFeesSection();
						UpdateCashierListSection(false);
					}
					UpdateWorkingHoursSection();
					UpdateWorkingHoursExceptionsSection(false);
					UpdateTimeOfDaySection();
					UpdateBCCListSection(false);
					UpdateModerationSection(false);
					UpdateViewReservationsSection();
					UpdateDuplicateReservationsSection();
					moduleController.UpdateTabModuleSetting(TabModuleId, "TimeZone", timeZoneDropDownList.SelectedValue);
					moduleController.UpdateTabModuleSetting(TabModuleId, "AllowCategorySelection", allowCategorySelectionCheckBox.Checked.ToString());
					moduleController.UpdateTabModuleSetting(TabModuleId, "CategorySelectionMode", (categorySelectionModeList.Checked ? 1 : (categorySelectionModeDropDownList.Checked ? 2 : 3)).ToString());
					moduleController.UpdateTabModuleSetting(TabModuleId, "DisplayCalendar", displayCalendarRadioButton.Checked.ToString());
					moduleController.UpdateTabModuleSetting(TabModuleId, "TimeOfDaySelectionMode", (timeOfDaySelectionModeList.Checked ? 1 : (timeOfDaySelectionModeDropDownList.Checked ? 2 : 3)).ToString());
					moduleController.UpdateTabModuleSetting(TabModuleId, "TimeSelectionMode", (timeSelectionModeList.Checked ? 1 : (timeSelectionModeDropDownList.Checked ? 2 : 3)).ToString());
					moduleController.UpdateTabModuleSetting(TabModuleId, "DurationSelectionMode", (durationSelectionModeList.Checked ? 1 : (durationSelectionModeDropDownList.Checked ? 2 : 3)).ToString());
					moduleController.UpdateTabModuleSetting(TabModuleId, "SelectCategoryLast", ((!allowCategorySelectionCheckBox.Checked ? false : selectCategoryLastCheckBox.Checked)).ToString());
					moduleController.UpdateTabModuleSetting(TabModuleId, "AllowCrossCategoryConflicts", ((!allowCategorySelectionCheckBox.Checked ? false : preventCrossCategoryConflictsCheckBox.Checked)).ToString());
					moduleController.UpdateTabModuleSetting(TabModuleId, "BindUponCategorySelection", ((!allowCategorySelectionCheckBox.Checked || selectCategoryLastCheckBox.Checked || preventCrossCategoryConflictsCheckBox.Checked ? false : bindUponSelectionCheckBox.Checked)).ToString());
					moduleController.UpdateTabModuleSetting(TabModuleId, "DisplayUnavailableCategories", ((!allowCategorySelectionCheckBox.Checked ? false : displayUnavailableCategoriesCheckBox.Checked)).ToString());
					moduleController.UpdateTabModuleSetting(TabModuleId, "AllowDescription", allowDescriptionCheckBox.Checked.ToString());
					moduleController.UpdateTabModuleSetting(TabModuleId, "AllowSchedulingAnotherReservation", allowSchedulingAnotherReservationCheckBox.Checked.ToString());
					moduleController.UpdateTabModuleSetting(TabModuleId, "DisplayRemainingReservations", displayRemainingReservationsCheckBox.Checked.ToString());
					moduleController.UpdateTabModuleSetting(TabModuleId, "ContactInfoFirst", contactInfoFirstCheckBox.Checked.ToString());
					moduleController.UpdateTabModuleSetting(TabModuleId, "DisplayEndTime", displayEndTimeCheckBox.Checked.ToString());
					moduleController.UpdateTabModuleSetting(TabModuleId, "ICalendarAttachmentFileName", iCalendarAttachmentFileNameTextBox.Text);
					moduleController.UpdateTabModuleSetting(TabModuleId, "Theme", themeDropDownList.SelectedValue);
					moduleController.UpdateTabModuleSetting(TabModuleId, "RequireEmail", requireEmailCheckBox.Checked.ToString());
					moduleController.UpdateTabModuleSetting(TabModuleId, "RequirePhone", requirePhoneCheckBox.Checked.ToString());
					moduleController.UpdateTabModuleSetting(TabModuleId, "AllowLookupByPhone", allowLookupByPhoneCheckBox.Checked.ToString());
					moduleController.UpdateTabModuleSetting(TabModuleId, "RedirectUrl", redirectUrlTextBox.Text);
					moduleController.UpdateTabModuleSetting(TabModuleId, "RequireVerificationCode", requireVerificationCodeCheckBox.Checked.ToString());
					moduleController.UpdateTabModuleSetting(TabModuleId, "SkipContactInfoForAuthenticatedUsers", skipContactInfoCheckBox.Checked.ToString());
					moduleController.UpdateTabModuleSetting(TabModuleId, "AttachiCalendar", attachICalendarCheckBox.Checked.ToString());
					if (ModuleSettings.SendReminders && !sendRemindersCheckBox.Checked)
					{
						Controller.DisableSendReminder(TabModuleId);
					}
					moduleController.UpdateTabModuleSetting(TabModuleId, "SendReminders", sendRemindersCheckBox.Checked.ToString());
					TimeSpan timeSpan = GetTimeSpan(sendRemindersWhenTextBox, sendRemindersWhenDropDownList);
					moduleController.UpdateTabModuleSetting(TabModuleId, "SendRemindersWhen", timeSpan.ToString());
					moduleController.UpdateTabModuleSetting(TabModuleId, "SendRemindersVia", sendRemindersViaDropDownList.SelectedValue);
					bool flag = (!sendRemindersCheckBox.Checked ? false : requireConfirmationCheckBox.Checked);
					if (ModuleSettings.RequireConfirmation && !flag)
					{
						Controller.DisableRequireConfirmation(TabModuleId);
					}
					moduleController.UpdateTabModuleSetting(TabModuleId, "RequireConfirmation", flag.ToString());
					timeSpan = GetTimeSpan(requireConfirmationWhenTextBox, requireConfirmationWhenDropDownList);
					moduleController.UpdateTabModuleSetting(TabModuleId, "RequireConfirmationWhen", timeSpan.ToString());
					if (!TabModuleSettings.ContainsKey("VerificationCodeSalt"))
					{
						Guid guid = Guid.NewGuid();
						moduleController.UpdateTabModuleSetting(TabModuleId, "VerificationCodeSalt", guid.ToString());
					}
					ModuleController.SynchronizeModule(ModuleId);
				}
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
			}
		}

		protected void UpdateSettingsCommandButtonClicked(object sender, EventArgs e)
		{
			if (Page.IsValid)
			{
				UpdateSettings();
				CancelSettingsCommandButtonClicked(sender, e);
			}
		}

		protected void UpdateSMSTemplateCommandButtonClicked(object sender, EventArgs e)
		{
			ModuleController moduleController = new ModuleController();
			if (smsTemplateDropDownList.SelectedValue == "Reminder")
			{
				moduleController.UpdateTabModuleSetting(TabModuleId, "ReminderSMS", smsTemplateBodyTextBox.Text);
			}
			moduleController.UpdateTabModuleSetting(TabModuleId, "TwilioAccountSID", twilioAccountSIDTextBox.Text);
			moduleController.UpdateTabModuleSetting(TabModuleId, "TwilioAuthToken", twilioAuthTokenTextBox.Text);
			moduleController.UpdateTabModuleSetting(TabModuleId, "TwilioFrom", twilioFromTextBox.Text);
			ModuleController.SynchronizeModule(ModuleId);
			_ModuleSettings = null;
		}

		protected void UpdateTimeOfDaySection()
		{
			if (Page.IsValid)
			{
				ModuleController moduleController = new ModuleController();
				int tabModuleId = TabModuleId;
				bool @checked = displayTimeOfDayCheckBox.Checked;
				moduleController.UpdateTabModuleSetting(tabModuleId, "DisplayTimeOfDay", @checked.ToString());
				int num = TabModuleId;
				@checked = displayUnavailableTimeOfDayCheckBox.Checked;
				moduleController.UpdateTabModuleSetting(num, "DisplayUnavailableTimeOfDay", @checked.ToString());
				moduleController.UpdateTabModuleSetting(TabModuleId, "TimesOfDay", ModuleSettings.SerializeTimeOfDayList(TimeOfDayList));
				ModuleController.SynchronizeModule(ModuleId);
				_ModuleSettings = null;
			}
		}

		protected void UpdateViewReservationsSection()
		{
			if (Page.IsValid)
			{
				(new ModuleController()).UpdateTabModuleSetting(TabModuleId, "ViewReservationsList", ModuleSettings.SerializeUserIDList(ViewReservationsList));
				ModuleController.SynchronizeModule(ModuleId);
				_ModuleSettings = null;
			}
		}

		protected void UpdateWorkingHoursExceptionsSection(bool updateSelectedCategorySettings)
		{
			DateTime dateTime;
			TimeSpan endTime;
			string str;
			if (Page.IsValid)
			{
				int num = (workingHoursExceptionsCategoryDropDownList.SelectedValue == null || !(workingHoursExceptionsCategoryDropDownList.SelectedValue != string.Empty) ? Null.NullInteger : int.Parse(workingHoursExceptionsCategoryDropDownList.SelectedValue));
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
					settings = (new CategorySettings(PortalId, TabModuleId, num)).Settings;
				}
				else
				{
					moduleController = new ModuleController();
					settings = ModuleSettings.Settings;
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
						moduleController.DeleteTabModuleSetting(TabModuleId, key);
					}
				}
				if (WorkingHoursExceptions.Count > 0)
				{
					Hashtable hashtables = new Hashtable();
					foreach (WorkingHoursExceptionInfo workingHoursException in WorkingHoursExceptions)
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
							moduleController.UpdateTabModuleSetting(TabModuleId, key1.ToString("d", CultureInfo.InvariantCulture), (string)hashtables[key1]);
						}
					}
				}
				if (categorySettingController == null)
				{
					ModuleController.SynchronizeModule(ModuleId);
					_ModuleSettings = null;
				}
				else
				{
					categorySettingController.UpdateCategorySetting(num, "WorkingHoursExceptionsDefined", bool.TrueString);
				}
				BindCategoriesDropDownList(workingHoursExceptionsCategoryDropDownList, "WorkingHoursExceptionsDefined", null, null);
				BindWorkingHoursExceptionsSection(num);
			}
		}

		protected void UpdateWorkingHoursSection()
		{
			if (Page.IsValid)
			{
				int num = (workingHoursCategoryDropDownList.SelectedValue == null || !(workingHoursCategoryDropDownList.SelectedValue != string.Empty) ? Null.NullInteger : int.Parse(workingHoursCategoryDropDownList.SelectedValue));
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
				foreach (RecurrencePattern workingHour in WorkingHours)
				{
					if (moduleController == null)
					{
						categorySettingController.UpdateCategorySetting(num, string.Concat("WorkingHours.", num1), Helper.SerializeRecurrencePattern(workingHour));
					}
					else
					{
						moduleController.UpdateTabModuleSetting(TabModuleId, string.Concat("WorkingHours.", num1), Helper.SerializeRecurrencePattern(workingHour));
					}
					num1++;
				}
				if (num != Null.NullInteger && WorkingHours.Count == 0)
				{
					categorySettingController.UpdateCategorySetting(num, "WorkingHours.1", string.Empty);
					num1++;
				}
				Hashtable settings = null;
				if (moduleController == null)
				{
					settings = (new CategorySettings(PortalId, TabModuleId, num)).Settings;
					if (!settings.ContainsKey("WorkingHours.1"))
					{
						settings = ModuleSettings.Settings;
					}
				}
				else
				{
					settings = ModuleSettings.Settings;
				}
				while (settings.ContainsKey(string.Concat("WorkingHours.", num1)))
				{
					if (moduleController == null)
					{
						categorySettingController.DeleteCategorySetting(num, string.Concat("WorkingHours.", num1));
					}
					else
					{
						moduleController.DeleteTabModuleSetting(TabModuleId, string.Concat("WorkingHours.", num1));
					}
					num1++;
				}
				if (moduleController != null)
				{
					ModuleController.SynchronizeModule(ModuleId);
					_ModuleSettings = null;
				}
				BindCategoriesDropDownList(workingHoursCategoryDropDownList, "WorkingHours.1", null, null);
				BindWorkingHoursSection(num);
			}
		}

		protected void ValidateAuthorizeNetApiLogin(object sender, ServerValidateEventArgs e)
		{
			e.IsValid = (authorizeNetApiLoginTextBox.Text != string.Empty ? true : feeschedulecontrol.GetFeeScheduleType() == FeeScheduleType.Free);
		}

		protected void ValidateAuthorizeNetMerchantHash(object sender, ServerValidateEventArgs e)
		{
			e.IsValid = (authorizeNetMerchantHashTextBox.Text != string.Empty ? true : feeschedulecontrol.GetFeeScheduleType() == FeeScheduleType.Free);
		}

		protected void ValidateAuthorizeNetTransactionKey(object sender, ServerValidateEventArgs e)
		{
			e.IsValid = (authorizeNetTransactionKeyTextBox.Text != string.Empty ? true : feeschedulecontrol.GetFeeScheduleType() == FeeScheduleType.Free);
		}

		protected void ValidateModerationHours(object sender, ServerValidateEventArgs e)
		{
			e.IsValid = true;
			DayOfWeek dayOfWeek = (DayOfWeek)int.Parse(moderationWeekDaysDropDownList.SelectedValue);
			TimeSpan time = GetTime(moderationStartHourDropDownList, moderationStartMinuteDropDownList, moderationStartAMPMDropDownList);
			TimeSpan timeSpan = GetTime(moderationEndHourDropDownList, moderationEndMinuteDropDownList, moderationEndAMPMDropDownList);
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
				foreach (WorkingHoursInfo moderationHour in ModerationHours)
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
			e.IsValid = (payPalAccountTextBox.Text != string.Empty ? true : feeschedulecontrol.GetFeeScheduleType() == FeeScheduleType.Free);
		}

		protected void ValidatePayPalUrl(object sender, ServerValidateEventArgs e)
		{
			e.IsValid = (payPalUrlTextBox.Text != string.Empty ? true : feeschedulecontrol.GetFeeScheduleType() == FeeScheduleType.Free);
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
			if (!int.TryParse(reservationIntervalTextBox.Text, out num))
			{
				e.IsValid = false;
				return;
			}
			TimeSpan timeSpan = GetTimeSpan(reservationIntervalTextBox, reservationIntervalDropDownList);
			e.IsValid = timeSpan.TotalDays <= 1;
		}

		protected void ValidateTimeOfDay(object sender, ServerValidateEventArgs e)
		{
			string text = timeOfDayNameTextBox.Text;
			TimeSpan time = GetTime(timeOfDayStartHourDropDownList, timeOfDayStartMinuteDropDownList, timeOfDayStartAMPMDropDownList);
			TimeSpan timeSpan = GetTime(timeOfDayEndHourDropDownList, timeOfDayEndMinuteDropDownList, timeOfDayEndAMPMDropDownList);
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
			foreach (TimeOfDayInfo timeOfDayList in TimeOfDayList)
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
			foreach (TimeOfDayInfo timeOfDayList in TimeOfDayList)
			{
				TimeSpan endTime = timeOfDayList.EndTime;
				timeSpan = timeSpan.Add(endTime.Subtract(timeOfDayList.StartTime));
			}
			return timeSpan.TotalDays == 1;
		}

		protected void ValidateTimeOfDayList(object sender, ServerValidateEventArgs e)
		{
			e.IsValid = ValidateTimeOfDayList();
			timeOfDaySectionHead.IsExpanded = (timeOfDaySectionHead.IsExpanded ? true : !e.IsValid);
		}

		protected void ValidateWorkingHoursException(object sender, ServerValidateEventArgs e)
		{
			DateTime dateTime;
			TimeSpan timeSpan;
			e.IsValid = true;
			if (!DateTime.TryParse(workingHoursExceptionDateTextBox.Text, out dateTime))
			{
				HighlightInvalidControl(workingHoursExceptionDateTextBox);
				e.IsValid = false;
			}
			else if (!workingHoursExceptionNoWorkingHoursRadioButton.Checked)
			{
				TimeSpan time = GetTime(workingHoursExceptionStartHourDropDownList, workingHoursExceptionStartMinuteDropDownList, workingHoursExceptionStartAMPMDropDownList);
				TimeSpan time1 = GetTime(workingHoursExceptionEndHourDropDownList, workingHoursExceptionEndMinuteDropDownList, workingHoursExceptionEndAMPMDropDownList);
				timeSpan = new TimeSpan();
				if (time1 == timeSpan)
				{
					time1 = new TimeSpan(1, 0, 0, 0);
				}
				if (time1 <= time)
				{
					HighlightInvalidControl(workingHoursExceptionStartHourDropDownList);
					HighlightInvalidControl(workingHoursExceptionStartMinuteDropDownList);
					HighlightInvalidControl(workingHoursExceptionStartAMPMDropDownList);
					HighlightInvalidControl(workingHoursExceptionEndHourDropDownList);
					HighlightInvalidControl(workingHoursExceptionEndMinuteDropDownList);
					HighlightInvalidControl(workingHoursExceptionEndAMPMDropDownList);
					e.IsValid = false;
				}
				else
				{
					foreach (WorkingHoursExceptionInfo workingHoursException in WorkingHoursExceptions)
					{
						if (!(workingHoursException.Date == dateTime) || (!(time <= workingHoursException.StartTime) || !(time1 > workingHoursException.StartTime)) && (!(workingHoursException.StartTime <= time) || !(workingHoursException.EndTime > time)))
						{
							continue;
						}
						HighlightInvalidControl(workingHoursExceptionStartHourDropDownList);
						HighlightInvalidControl(workingHoursExceptionStartMinuteDropDownList);
						HighlightInvalidControl(workingHoursExceptionStartAMPMDropDownList);
						HighlightInvalidControl(workingHoursExceptionEndHourDropDownList);
						HighlightInvalidControl(workingHoursExceptionEndMinuteDropDownList);
						HighlightInvalidControl(workingHoursExceptionEndAMPMDropDownList);
						e.IsValid = false;
						return;
					}
				}
			}
			else
			{
				foreach (WorkingHoursExceptionInfo workingHoursExceptionInfo in WorkingHoursExceptions)
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
					HighlightInvalidControl(workingHoursExceptionDateTextBox);
					e.IsValid = false;
					return;
				}
			}
			if (e.IsValid)
			{
				UnhighlightInvalidControl(workingHoursExceptionDateTextBox);
				UnhighlightInvalidControl(workingHoursExceptionStartHourDropDownList);
				UnhighlightInvalidControl(workingHoursExceptionStartMinuteDropDownList);
				UnhighlightInvalidControl(workingHoursExceptionStartAMPMDropDownList);
				UnhighlightInvalidControl(workingHoursExceptionEndHourDropDownList);
				UnhighlightInvalidControl(workingHoursExceptionEndMinuteDropDownList);
				UnhighlightInvalidControl(workingHoursExceptionEndAMPMDropDownList);
			}
		}

		protected void ValidateWorkingHoursExceptionDate(object sender, ServerValidateEventArgs e)
		{
			DateTime dateTime;
			e.IsValid = DateTime.TryParse(e.Value, out dateTime);
			if (!e.IsValid)
			{
				HighlightInvalidControl(workingHoursExceptionDateTextBox);
				return;
			}
			UnhighlightInvalidControl(workingHoursExceptionDateTextBox);
		}

		protected void WorkingHoursCategoryDropDownListSelectedIndexChanged(object sender, EventArgs e)
		{
			BindWorkingHoursSection(int.Parse(workingHoursCategoryDropDownList.SelectedValue));
		}

		protected void WorkingHoursExceptionsCategoryDropDownListSelectedIndexChanged(object sender, EventArgs e)
		{
			BindWorkingHoursExceptionsSection(int.Parse(workingHoursExceptionsCategoryDropDownList.SelectedValue));
		}

		protected void WorkingHoursExceptionsResetCommandButtonClicked(object sender, EventArgs e)
		{
			DateTime dateTime;
			int num = int.Parse(workingHoursExceptionsCategoryDropDownList.SelectedValue);
			CategorySettingController categorySettingController = new CategorySettingController();
			foreach (string key in (new CategorySettings(PortalId, TabModuleId, num)).Settings.Keys)
			{
				if (!DateTime.TryParse(key, CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.None, out dateTime))
				{
					continue;
				}
				categorySettingController.DeleteCategorySetting(num, key);
			}
			categorySettingController.DeleteCategorySetting(num, "WorkingHoursExceptionsDefined");
			BindCategoriesDropDownList(workingHoursExceptionsCategoryDropDownList, "WorkingHoursExceptionsDefined", null, null);
			BindWorkingHoursExceptionsSection(num);
		}

		protected void WorkingHoursExceptionsUpdateCommandButtonClicked(object sender, EventArgs e)
		{
			UpdateWorkingHoursExceptionsSection(true);
		}

		protected void WorkingHoursResetCommandButtonClicked(object sender, EventArgs e)
		{
			int num = int.Parse(workingHoursCategoryDropDownList.SelectedValue);
			CategorySettingController categorySettingController = new CategorySettingController();
			Hashtable settings = (new CategorySettings(PortalId, TabModuleId, num)).Settings;
			for (int i = 1; settings.ContainsKey(string.Concat("WorkingHours.", i)); i++)
			{
				categorySettingController.DeleteCategorySetting(num, string.Concat("WorkingHours.", i));
			}
			BindCategoriesDropDownList(workingHoursCategoryDropDownList, "WorkingHours.1", null, null);
			BindWorkingHoursSection(num);
		}

		protected void WorkingHoursUpdateCommandButtonClicked(object sender, EventArgs e)
		{
			UpdateWorkingHoursSection();
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
				Page.Header.Controls.Add(css);
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
				Page.Header.Controls.Add(script);
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
				Page.Header.Controls.Add(literal);
			}
		}
	}
}