using DotNetNuke.Abstractions;
using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.WebControls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.SqlTypes;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Extensions.DependencyInjection;

namespace Gafware.Modules.Reservations
{
	public class DuplicateReservationsListSettings : ListSettings
	{
		private List<ReservationInfo> _ReservationsList;

		private List<CommandButton> _CommandButtonList;

		protected string _NotAvailable;

		public DuplicateReservationsListSettings(PortalModuleBase portalModuleBase) : base(portalModuleBase)
		{
		}

		public override string ALLOWPAGING_KEY
		{
			get
			{
				return "DuplicateReservationsAllowPaging";
			}
		}

		public override string ALLOWSORTING_KEY
		{
			get
			{
				return "DuplicateReservationsAllowSorting";
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
				return "DuplicateReservationsDisplayColumns";
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

		protected string NotAvailable
		{
			get
			{
				if (this._NotAvailable == null)
				{
					this._NotAvailable = Localization.GetString("NotAvailable", string.Concat(new string[] { Globals.ApplicationPath, "/DesktopModules/", DesktopModuleController.GetDesktopModuleByModuleName("Gafware.Modules.Reservations", PortalSettings.Current.PortalId).FolderName, "/", Localization.LocalResourceDirectory, "/SharedResources" }));
				}
				return this._NotAvailable;
			}
		}

		public override string PAGERMODE_KEY
		{
			get
			{
				return "DuplicateReservationsPagerMode";
			}
		}

		public override string PAGERPOSITION_KEY
		{
			get
			{
				return "DuplicateReservationsPagerPosition";
			}
		}

		public override string PAGESIZE_KEY
		{
			get
			{
				return "DuplicateReservationsPageSize";
			}
		}

		public override string SORTCOLUMNCOLOR_KEY
		{
			get
			{
				return "DuplicateReservationsSortColumnColor";
			}
		}

		public override string SORTORDER_KEY
		{
			get
			{
				return "DuplicateReservationsSortOrder";
			}
		}

		public override bool ApplyCustomFilter(object info, string columnName, string text)
		{
			if (columnName != "CategoryName")
			{
				throw new NotImplementedException();
			}
			if (string.IsNullOrEmpty(text))
			{
				return true;
			}
			return ((ReservationInfo)info).CategoryID == int.Parse(text);
		}

		public override bool CanViewList(int userID)
		{
			return base.Helper.CanViewDuplicateReservations(userID);
		}

		public override bool ExcludeFromDateRangeFilters(string columnName)
		{
			if (columnName == "StartTime")
			{
				return true;
			}
			return columnName == "EndTime";
		}

		private ReservationInfo FindEventInfoByEventID(List<ReservationInfo> eventInfoList, int eventID)
		{
			ReservationInfo reservationInfo;
			List<ReservationInfo>.Enumerator enumerator = eventInfoList.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					ReservationInfo current = enumerator.Current;
					if (current.ReservationID != eventID)
					{
						continue;
					}
					reservationInfo = current;
					return reservationInfo;
				}
				return null;
			}
			finally
			{
				((IDisposable)enumerator).Dispose();
			}
		}

		private List<ReservationInfo> FindEventInfoListByEmailOrPhone(List<ReservationInfo> eventInfoList, string email, string phone, int startIndex)
		{
			List<ReservationInfo> reservationInfos = new List<ReservationInfo>();
			while (startIndex < eventInfoList.Count)
			{
				ReservationInfo item = eventInfoList[startIndex];
				if (!string.IsNullOrEmpty(item.Email) && !string.IsNullOrEmpty(email) && item.Email != this.NotAvailable && email != this.NotAvailable && item.Email.Trim().ToLower() == email.Trim().ToLower() || !string.IsNullOrEmpty(item.Phone) && !string.IsNullOrEmpty(phone) && item.Phone != this.NotAvailable && phone != this.NotAvailable && item.Phone.Trim().ToLower() == phone.Trim().ToLower())
				{
					reservationInfos.Add(item);
				}
				startIndex++;
			}
			return reservationInfos;
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
			if (!this.InfoType.GetProperty(columnName).PropertyType.Equals(typeof(DateTime)))
			{
				if (columnName == "Email")
				{
					return "<a href=\"mailto:{0}\">{0}</a>";
				}
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
				DateTime value = SqlDateTime.MinValue.Value;
				DateTime dateTime = SqlDateTime.MaxValue.Value;
				if (!string.IsNullOrEmpty(item))
				{
					DateTime fromDate = base.Helper.GetFromDate(item);
					DateTime toDate = base.Helper.GetToDate(item);
					DateTime date = new DateTime();
					if (fromDate != date)
					{
						value = fromDate.Date;
					}
					date = new DateTime();
					if (toDate == date)
					{
						date = new DateTime();
						if (fromDate != date)
						{
							date = fromDate.Date;
							dateTime = date.AddDays(1);
						}
					}
					else
					{
						date = toDate.Date;
						dateTime = date.AddDays(1);
					}
				}
				List<ReservationInfo> reservationListByDateRange = (new ReservationController()).GetReservationListByDateRange(this.portalModuleBase.TabModuleId, value, dateTime);
				int num = 0;
				foreach (ReservationInfo reservationInfo in reservationListByDateRange)
				{
					if (this.FindEventInfoByEventID(this._ReservationsList, reservationInfo.ReservationID) == null)
					{
						List<ReservationInfo> reservationInfos = this.FindEventInfoListByEmailOrPhone(reservationListByDateRange, reservationInfo.Email, reservationInfo.Phone, num + 1);
						if (reservationInfos.Count > 0)
						{
							this._ReservationsList.Add(reservationInfo);
							foreach (ReservationInfo reservationInfo1 in reservationInfos)
							{
								this._ReservationsList.Add(reservationInfo1);
							}
						}
					}
					num++;
				}
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
			return columnName == "CategoryName";
		}

		public override bool IsHighlightable(string columnName)
		{
			return columnName != "Email";
		}

		public override bool SkipFilter(string columnName)
		{
			return columnName == "StartDate";
		}

		public override void SortList(ICollection list, List<SortColumnInfo> sortColumnList)
		{
			((List<ReservationInfo>)list).Sort(new ColumnListComparer<ReservationInfo>(sortColumnList));
		}
	}
}