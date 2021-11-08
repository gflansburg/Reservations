using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.WebControls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Gafware.Modules.Reservations
{
	public class ViewReservationsListSettings : ListSettings
	{
		private List<ReservationInfo> _ReservationsList;

		private List<CommandButton> _CommandButtonList;

		public override string ALLOWPAGING_KEY
		{
			get
			{
				return "ViewReservationsAllowPaging";
			}
		}

		public override string ALLOWSORTING_KEY
		{
			get
			{
				return "ViewReservationsAllowSorting";
			}
		}

		public override List<CommandButton> CommandButtonList
		{
			get
			{
				if (this._CommandButtonList == null)
				{
					this._CommandButtonList = new List<CommandButton>(new CommandButton[] { new CommandButton() });
					this._CommandButtonList[0].CommandName = "View";
					this._CommandButtonList[0].ImageUrl = "~/images/view.gif";
					this._CommandButtonList[0].Attributes["ResourceKey"] = "viewCommandButton";
				}
				return this._CommandButtonList;
			}
		}

		public override List<DisplayColumnInfo> DisplayColumnList
		{
			get
			{
				List<DisplayColumnInfo> displayColumnInfos = this.DeserializeDisplayColumnList(this.DisplayColumnList_Default);
				List<DisplayColumnInfo> displayColumnList = base.DisplayColumnList;
				List<CustomFieldDefinitionInfo> activeCustomFieldDefinitionList = (new CustomFieldDefinitionController()).GetActiveCustomFieldDefinitionList(this.portalModuleBase.TabModuleId);
				List<DisplayColumnInfo> displayColumnInfos1 = new List<DisplayColumnInfo>();
				foreach (DisplayColumnInfo count in displayColumnList)
				{
					if (displayColumnInfos.Find((DisplayColumnInfo diplayColumnInfo2) => diplayColumnInfo2.ColumnName == count.ColumnName) == null && activeCustomFieldDefinitionList.Find((CustomFieldDefinitionInfo customFieldDefinitionInfo2) => customFieldDefinitionInfo2.Name == count.ColumnName) == null)
					{
						continue;
					}
					count.DisplayOrder = displayColumnInfos1.Count;
					displayColumnInfos1.Add(count);
				}
				foreach (CustomFieldDefinitionInfo customFieldDefinitionInfo in activeCustomFieldDefinitionList)
				{
					if (displayColumnInfos1.Find((DisplayColumnInfo diplayColumnInfo2) => diplayColumnInfo2.ColumnName == customFieldDefinitionInfo.Name) != null)
					{
						continue;
					}
					DisplayColumnInfo displayColumnInfo = new DisplayColumnInfo()
					{
						ColumnName = customFieldDefinitionInfo.Name,
						DisplayOrder = displayColumnInfos1.Count,
						Visible = false
					};
					displayColumnInfos1.Add(displayColumnInfo);
				}
				return displayColumnInfos1;
			}
		}

		public override string DisplayColumnList_Default
		{
			get
			{
				return "CategoryName;True,FullName;True,Email;True,Phone;True,StartDate;True,StartTime;True,EndTime;True,Duration;False,Description;False,CreatedOnDate;False";
			}
		}

		public override string DISPLAYCOLUMNLIST_KEY
		{
			get
			{
				return "ViewReservationsDisplayColumns";
			}
		}

		public override string IdPropertyName
		{
			get
			{
				return "ReservationID";
			}
		}

		public override Type InfoType
		{
			get
			{
				return typeof(ReservationInfo);
			}
		}

		public override string PAGERMODE_KEY
		{
			get
			{
				return "ViewReservationsPagerMode";
			}
		}

		public override string PAGERPOSITION_KEY
		{
			get
			{
				return "ViewReservationsPagerPosition";
			}
		}

		public override string PAGESIZE_KEY
		{
			get
			{
				return "ViewReservationsPageSize";
			}
		}

		public override string SORTCOLUMNCOLOR_KEY
		{
			get
			{
				return "ViewReservationsSortColumnColor";
			}
		}

		public override string SortColumnList_Default
		{
			get
			{
				return "StartTime";
			}
		}

		public override string SORTORDER_KEY
		{
			get
			{
				return "ViewReservationsSortOrder";
			}
		}

		public ViewReservationsListSettings(PortalModuleBase portalModuleBase) : base(portalModuleBase)
		{
		}

		public override bool ApplyCustomFilter(object info, string columnName, string text)
		{
			string str;
			if (!this.ImplementsCustomFilter(columnName))
			{
				throw new ArgumentException();
			}
			if (columnName == "CategoryName")
			{
				if (string.IsNullOrEmpty(text))
				{
					return true;
				}
				return ((ReservationInfo)info).CategoryID == int.Parse(text);
			}
			object obj = this.Eval(info, columnName);
			if (obj != null)
			{
				str = obj.ToString();
			}
			else
			{
				str = null;
			}
			string str1 = str;
			if (string.IsNullOrEmpty(str1))
			{
				str1 = Localization.GetString("None", this.portalModuleBase.LocalResourceFile);
			}
			if (string.IsNullOrEmpty(text))
			{
				return true;
			}
			if (str1 == null)
			{
				return false;
			}
			return str1.IndexOf(text) != -1;
		}

		public override bool CanViewList(int userID)
		{
			return base.Helper.CanViewReservations(userID);
		}

		public override object Eval(object container, string expression)
		{
			ReservationInfo reservationInfo = (ReservationInfo)container;
			if (reservationInfo.GetType().GetProperty(expression) != null)
			{
				return base.Eval(container, expression);
			}
			List<CustomFieldDefinitionInfo> activeCustomFieldDefinitionList = (new CustomFieldDefinitionController()).GetActiveCustomFieldDefinitionList(this.portalModuleBase.TabModuleId);
			CustomFieldDefinitionInfo customFieldDefinitionInfo = activeCustomFieldDefinitionList.Find((CustomFieldDefinitionInfo customFieldDefinitionInfo2) => customFieldDefinitionInfo2.Name == expression);
			if (customFieldDefinitionInfo == null)
			{
				return null;
			}
			CustomFieldValueInfo customFieldValueInfo = (new CustomFieldValueController()).GetCustomFieldValueListByReservationID(reservationInfo.ReservationID).Find((CustomFieldValueInfo customFieldValue2) => customFieldValue2.CustomFieldDefinitionID == customFieldDefinitionInfo.CustomFieldDefinitionID);
			if (customFieldValueInfo == null)
			{
				return null;
			}
			return customFieldValueInfo.Value;
		}

		public override bool ExcludeFromDateRangeFilters(string columnName)
		{
			if (columnName == "StartTime")
			{
				return true;
			}
			return columnName == "EndTime";
		}

		public override string GetDefaultFilterValue(string columnName)
		{
			if (columnName != "StartDate")
			{
				return string.Empty;
			}
			string shortDateString = DateTime.Today.ToShortDateString();
			string dateRangeSeparator = base.Helper.DateRangeSeparator;
			DateTime dateTime = DateTime.Today.AddDays(7);
			return string.Concat(shortDateString, dateRangeSeparator, dateTime.ToShortDateString());
		}

		public override string GetDisplayFormat(string columnName)
		{
			PropertyInfo property = this.InfoType.GetProperty(columnName);
			if (property == null || !property.PropertyType.Equals(typeof(DateTime)))
			{
				return "{0}";
			}
			if (!(columnName == "StartTime") && !(columnName == "EndTime"))
			{
				return "{0:d}";
			}
			return "{0:t}";
		}

		public override IList GetEmptyList()
		{
			return new List<ReservationInfo>();
		}

		public override WebControl GetFilterControl(string columnName, EventHandler eventHandler)
		{
			if (columnName != "CategoryName")
			{
				return null;
			}
			DropDownList dropDownList = new DropDownList()
			{
				DataSource = (new CategoryController()).GetCategoryList(this.portalModuleBase.TabModuleId),
				DataTextField = "Name",
				DataValueField = "CategoryID"
			};
			dropDownList.DataBind();
			dropDownList.Items.Insert(0, new ListItem(Localization.GetString("All", this.portalModuleBase.LocalResourceFile), string.Empty));
			dropDownList.SelectedIndexChanged += eventHandler;
			dropDownList.AutoPostBack = true;
			return dropDownList;
		}

		public override IList GetList(OrderedDictionary filtersOrderedDictionary)
		{
			if (this._ReservationsList == null)
			{
				this._ReservationsList = new List<ReservationInfo>();
				string item = (string)filtersOrderedDictionary["StartDate"];
				DateTime now = DateTime.Now;
				now = now.Date;
				DateTime date = now.AddDays(-365);
				now = DateTime.Now;
				now = now.Date;
				DateTime dateTime = now.AddDays(366);
				if (!string.IsNullOrEmpty(item))
				{
					DateTime fromDate = base.Helper.GetFromDate(item);
					DateTime toDate = base.Helper.GetToDate(item);
					now = new DateTime();
					if (fromDate != now)
					{
						date = fromDate.Date;
					}
					now = new DateTime();
					if (toDate == now)
					{
						now = new DateTime();
						if (fromDate != now)
						{
							now = fromDate.Date;
							dateTime = now.AddDays(1);
						}
					}
					else
					{
						now = toDate.Date;
						dateTime = now.AddDays(1);
					}
				}
				this._ReservationsList = (new ReservationController()).GetReservationListByDateRange(this.portalModuleBase.TabModuleId, date, dateTime);
			}
			return this._ReservationsList;
		}

		public override void HandleCommand(int id, string commandName, PortalModuleBase portalModuleBase = null, string returnUrl = null)
		{
			if (commandName == "View")
			{
#pragma warning disable CS0618 // Type or member is obsolete
                HttpContext.Current.Response.Redirect(Globals.NavigateURL(portalModuleBase.TabId, string.Empty, new string[] { string.Concat("EventID=", id.ToString()), string.Concat("ReturnUrl=", returnUrl) }));
#pragma warning restore CS0618 // Type or member is obsolete
            }
		}

		public override bool ImplementsCustomFilter(string columnName)
		{
			if (columnName == "CategoryName")
			{
				return true;
			}
			return (new CustomFieldDefinitionController()).GetActiveCustomFieldDefinitionList(this.portalModuleBase.TabModuleId).Find((CustomFieldDefinitionInfo customFieldDefinitionInfo) => customFieldDefinitionInfo.Name == columnName) != null;
		}

		public override bool SkipFilter(string columnName)
		{
			return columnName == "StartDate";
		}

		public override void SortList(ICollection list, List<SortColumnInfo> sortColumnList)
		{
			((List<ReservationInfo>)list).Sort(new AdvancedColumnListComparer<ReservationInfo>(this, sortColumnList));
		}
	}
}