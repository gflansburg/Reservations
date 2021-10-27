using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.WebControls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Gafware.Modules.Reservations
{
    public partial class ViewReservations : PortalModuleBase
    {
		protected Gafware.Modules.Reservations.Helper _Helper;

		protected Gafware.Modules.Reservations.ModuleSettings _ModuleSettings;

		protected Gafware.Modules.Reservations.ViewReservationsListSettings _ViewReservationsListSettings;

		protected Gafware.Modules.Reservations.ModuleSecurity _ModuleSecurity;

		protected List<SortColumnInfo> _SortColumnList;

		protected List<ReservationInfo> _ReservationsList;

		protected OrderedDictionary _FiltersOrderedDictionary;

		protected ReservationController _ProxyController;

		protected List<CategoryInfo> _CategoryList;

		protected string _NotAvailable;

		protected bool CanViewReservations
		{
			get
			{
				if (base.UserId == Null.NullInteger)
				{
					return false;
				}
				return this.Helper.CanViewReservations(base.UserId);
			}
		}

		protected List<CategoryInfo> CategoryList
		{
			get
			{
				if (this._CategoryList == null)
				{
					this._CategoryList = (new CategoryController()).GetCategoryList(base.TabModuleId);
				}
				return this._CategoryList;
			}
		}

		public string FilterAndSortQueryStringParams
		{
			get
			{
				string empty = string.Empty;
				foreach (DisplayColumnInfo displayColumnList in this.ViewReservationsListSettings.DisplayColumnList)
				{
					if (!displayColumnList.Visible)
					{
						continue;
					}
					string item = (string)this.FiltersOrderedDictionary[displayColumnList.ColumnName];
					if (item == string.Empty)
					{
						continue;
					}
					empty = string.Concat(new string[] { empty, "&", displayColumnList.ColumnName, "=", item });
				}
				if (this.SortColumnList.Count > 0)
				{
					empty = string.Concat(empty, "&SortColumnList=", this.ViewReservationsListSettings.SerializeSortColumnList(this.SortColumnList));
				}
				if (empty != string.Empty)
				{
					empty = empty.Substring(1);
				}
				return empty;
			}
		}

		private OrderedDictionary FiltersOrderedDictionary
		{
			get
			{
				if (this._FiltersOrderedDictionary == null)
				{
					this._FiltersOrderedDictionary = new OrderedDictionary();
					foreach (DisplayColumnInfo displayColumnList in this.ViewReservationsListSettings.DisplayColumnList)
					{
						if (!displayColumnList.Visible)
						{
							continue;
						}
						string empty = string.Empty;
						if (!base.IsPostBack)
						{
							empty = base.Request.QueryString[displayColumnList.ColumnName];
							if (displayColumnList.ColumnName == "StartDate" && !(new ArrayList(base.Request.QueryString.AllKeys)).Contains("StartDate") && !(new ArrayList(base.Request.QueryString.AllKeys)).Contains("currentpage") && !(new ArrayList(base.Request.QueryString.AllKeys)).Contains("Returning") && !this.IsPrintable)
							{
								string shortDateString = DateTime.Today.ToShortDateString();
								string rangeSeparator = this.RangeSeparator;
								DateTime dateTime = DateTime.Today.AddDays(7);
								empty = string.Concat(shortDateString, rangeSeparator, dateTime.ToShortDateString());
							}
						}
						else
						{
							empty = this.GetFilterText(displayColumnList.ColumnName);
						}
						this._FiltersOrderedDictionary.Add(displayColumnList.ColumnName, (empty == null ? string.Empty : empty));
					}
				}
				return this._FiltersOrderedDictionary;
			}
		}

		protected bool HasEditPermissions
		{
			get
			{
				return (new Gafware.Modules.Reservations.ModuleSecurity(base.ModuleConfiguration)).HasEditPermissions;
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

		protected bool IsPrintable
		{
			get
			{
				bool flag = false;
				return bool.TryParse(base.Request.QueryString["Printable"], out flag) & flag;
			}
		}

		private Gafware.Modules.Reservations.ModuleSecurity ModuleSecurity
		{
			get
			{
				if (this._ModuleSecurity == null)
				{
					this._ModuleSecurity = new Gafware.Modules.Reservations.ModuleSecurity(base.ModuleConfiguration);
				}
				return this._ModuleSecurity;
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

		protected string NotAvailable
		{
			get
			{
				if (this._NotAvailable == null)
				{
					this._NotAvailable = Localization.GetString("NotAvailable", base.LocalResourceFile);
				}
				return this._NotAvailable;
			}
		}

		protected ReservationController ProxyController
		{
			get
			{
				if (this._ProxyController == null)
				{
					this._ProxyController = new ReservationController();
				}
				return this._ProxyController;
			}
		}

		public string QueryStringParams
		{
			get
			{
				int moduleId = base.ModuleId;
				return string.Concat("&ctl=ViewReservations&mid=", moduleId.ToString(), "&", this.FilterAndSortQueryStringParams);
			}
		}

		private string RangeSeparator
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

		private List<ReservationInfo> ReservationsList
		{
			get
			{
				if (this._ReservationsList == null)
				{
					this._ReservationsList = new List<ReservationInfo>();
					string item = (string)this.FiltersOrderedDictionary["StartDate"];
					DateTime now = DateTime.Now;
					now = now.Date;
					DateTime date = now.AddDays(-365);
					now = DateTime.Now;
					now = now.Date;
					DateTime dateTime = now.AddDays(366);
					if (!string.IsNullOrEmpty(item))
					{
						DateTime fromDate = this.GetFromDate(item);
						DateTime toDate = this.GetToDate(item);
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
					List<ReservationInfo> reservationListByDateRange = this.ProxyController.GetReservationListByDateRange(base.TabModuleId, date, dateTime);
					this._ReservationsList = new List<ReservationInfo>();
					foreach (ReservationInfo reservationInfo in reservationListByDateRange)
					{
						this._ReservationsList.Add(reservationInfo);
					}
					foreach (DisplayColumnInfo displayColumnList in this.ViewReservationsListSettings.DisplayColumnList)
					{
						if (!displayColumnList.Visible)
						{
							continue;
						}
						this._ReservationsList = this.Filter(this._ReservationsList, displayColumnList.ColumnName, (string)this.FiltersOrderedDictionary[displayColumnList.ColumnName]);
					}
				}
				return this._ReservationsList;
			}
		}

		private List<SortColumnInfo> SortColumnList
		{
			get
			{
				if (this._SortColumnList == null)
				{
					if (base.IsPostBack)
					{
						if (this.ViewState["SortColumnList"] != null)
						{
							this._SortColumnList = this.ViewReservationsListSettings.DeserializeSortColumnList((string)this.ViewState["SortColumnList"]);
						}
						else
						{
							this.SortColumnList = this.ViewReservationsListSettings.SortColumnList;
						}
					}
					else if (base.Request.QueryString["SortColumnList"] != null)
					{
						this.SortColumnList = this.ViewReservationsListSettings.DeserializeSortColumnList(base.Request.QueryString["SortColumnList"]);
					}
					else
					{
						this.SortColumnList = this.ViewReservationsListSettings.SortColumnList;
					}
				}
				return this._SortColumnList;
			}
			set
			{
				this.ViewState.Remove("SortColumnList");
				if (value == null)
				{
					this._SortColumnList = null;
					return;
				}
				this._SortColumnList = value;
				this.ViewState["SortColumnList"] = this.ViewReservationsListSettings.SerializeSortColumnList(this._SortColumnList);
			}
		}

		private Gafware.Modules.Reservations.ViewReservationsListSettings ViewReservationsListSettings
		{
			get
			{
				if (this._ViewReservationsListSettings == null)
				{
					this._ViewReservationsListSettings = new Gafware.Modules.Reservations.ViewReservationsListSettings(this);
				}
				return this._ViewReservationsListSettings;
			}
		}

		public ViewReservations()
		{
		}

		private void AddColumn(string dataField)
		{
			this.AddColumn(dataField, dataField, "");
		}

		private void AddColumn(string title, string dataField)
		{
			this.AddColumn(title, dataField, "");
		}

		private void AddColumn(string title, string dataField, string format)
		{
			BoundColumn boundColumn = new BoundColumn()
			{
				DataField = dataField,
				DataFormatString = format,
				HeaderText = title,
				SortExpression = dataField
			};
			boundColumn.HeaderStyle.CssClass = string.Concat(base.GetType().BaseType.Namespace.Replace(".", "_"), "_DataGrid_", dataField, "_HeaderStyle");
			boundColumn.ItemStyle.CssClass = string.Concat(base.GetType().BaseType.Namespace.Replace(".", "_"), "_DataGrid_", dataField, "_ItemStyle");
			this.dataGrid.Columns.Add(boundColumn);
		}

		private void BindData()
		{
			if (this.SortColumnList.Count > 0)
			{
				this.ReservationsList.Sort(new ColumnListComparer<ReservationInfo>(this.SortColumnList));
			}
			int count = this.ReservationsList.Count / this.dataGrid.PageSize;
			int num = this.ReservationsList.Count % this.dataGrid.PageSize;
			if (this.ViewReservationsListSettings.AllowPaging)
			{
				int count1 = this.ReservationsList.Count / this.ViewReservationsListSettings.PageSize + (this.ReservationsList.Count % this.ViewReservationsListSettings.PageSize == 0 ? 0 : 1) - 1;
				if (this.ReservationsList.Count == 0)
				{
					this.dataGrid.CurrentPageIndex = 0;
				}
				else if (this.dataGrid.CurrentPageIndex > count1)
				{
					this.dataGrid.CurrentPageIndex = count1;
				}
			}
			this.dataGrid.DataSource = this.ReservationsList;
			this.dataGrid.DataBind();
			Label label = this.numberOfRecordsFoundLabel;
			string str = Localization.GetString("Total", base.LocalResourceFile);
			int num1 = this.ReservationsList.Count;
			label.Text = string.Format(str, num1.ToString());
		}

		protected void CancelCommandButtonClicked(object sender, EventArgs e)
		{
			base.Response.Redirect(Globals.NavigateURL(), true);
		}

		private Control CreateCalendar(string columnName)
		{
			System.Web.UI.WebControls.Calendar calendar1 = new System.Web.UI.WebControls.Calendar()
			{
				ID = string.Concat(columnName, "_Calendar"),
				CssClass = "Gafware_Modules_Reservations_DateFilter_Calendar"
			};
			calendar1.SelectionChanged += new EventHandler(this.DateFilterCalendarSelectionChanged);
			calendar1.PreRender += new EventHandler((object sender, EventArgs e) => {
				string item = this.Page.Request.Params["__EVENTTARGET"];
				if (string.IsNullOrEmpty(item) || this.Page.FindControl(item) != calendar1)
				{
					string text = ((TextBox)this.GetFilterControl(columnName)).Text;
					DateTime fromDate = this.GetFromDate(text);
					DateTime toDate = this.GetToDate(text);
					DateTime dateTime = new DateTime();
					if (fromDate == dateTime)
					{
						dateTime = new DateTime();
						if (toDate == dateTime)
						{
							return;
						}
					}
					System.Web.UI.WebControls.Calendar calendar = calendar1;
					dateTime = new DateTime();
					calendar.VisibleDate = (fromDate != dateTime ? fromDate : toDate);
				}
			});
			calendar1.DayRender += new DayRenderEventHandler(this.DateFilterCalendarDayRender);
			calendar1.CellPadding = 0;
			calendar1.CellSpacing = 0;
			calendar1.NextPrevFormat = NextPrevFormat.ShortMonth;
			calendar1.DayHeaderStyle.CssClass = "Gafware_Modules_Reservations_DateFilter_DayHeaderStyle Normal";
			calendar1.DayStyle.CssClass = "Gafware_Modules_Reservations_DateFilter_DayStyle Normal";
			calendar1.NextPrevStyle.CssClass = "Gafware_Modules_Reservations_DateFilter_NextPrevStyle Normal";
			calendar1.OtherMonthDayStyle.CssClass = "Gafware_Modules_Reservations_DateFilter_OtherMonthDayStyle Gafware_Modules_Reservations_DateFilter_DayStyle Normal";
			calendar1.SelectedDayStyle.CssClass = "Gafware_Modules_Reservations_DateFilter_SelectedDayStyle Gafware_Modules_Reservations_DateFilter_DayStyle Normal";
			calendar1.SelectorStyle.CssClass = "Gafware_Modules_Reservations_DateFilter_SelectorStyle";
			calendar1.TitleStyle.CssClass = "Gafware_Modules_Reservations_DateFilter_TitleStyle Normal";
			calendar1.TitleStyle.BackColor = Color.FromName("Transparent");
			calendar1.TodayDayStyle.CssClass = "Gafware_Modules_Reservations_DateFilter_TodayDayStyle Gafware_Modules_Reservations_DateFilter_DayStyle Normal";
			calendar1.WeekendDayStyle.CssClass = "Gafware_Modules_Reservations_DateFilter_WeekendDayStyle Gafware_Modules_Reservations_DateFilter_DayStyle Normal";
			return calendar1;
		}

		protected void DateFilterCalendarDayRender(object sender, DayRenderEventArgs e)
		{
			try
			{
				string d = ((Control)sender).ID;
				string str = d.Substring(0, d.IndexOf("_"));
				TextBox filterControl = (TextBox)this.GetFilterControl(str);
				System.Web.UI.WebControls.Calendar calendar = (System.Web.UI.WebControls.Calendar)sender;
				DateTime fromDate = this.GetFromDate(filterControl.Text);
				DateTime toDate = this.GetToDate(filterControl.Text);
				if (e.Day.Date == fromDate || e.Day.Date == toDate)
				{
					e.Cell.CssClass = string.Concat("Gafware_Modules_Reservations_DateFilter_SelectedDayStyle ", e.Cell.CssClass);
				}
				else if (e.Day.Date > fromDate && e.Day.Date < toDate)
				{
					e.Cell.CssClass = string.Concat("Gafware_Modules_Reservations_DateFilter_SelectedRangeStyle ", e.Cell.CssClass);
				}
				TimeSpan date = e.Day.Date - new DateTime(2000, 1, 1);
				int days = date.Days;
				TableCell cell = e.Cell;
				cell.CssClass = string.Concat(new object[] { cell.CssClass, " Gafware_Modules_Reservations_DateFilter_TableCell Gafware_Modules_Reservations_DateFilter_", str, "_TableCell Gafware_Modules_Reservations_DateFilter_", str, "_TableCell_", days });
				e.Cell.Attributes.Add("date", days.ToString());
				e.Cell.Attributes.Add("columnName", str);
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
			}
		}

		private void DateFilterCalendarPreRender(object sender, EventArgs e)
		{
			bool flag;
			HtmlTable htmlTable = (HtmlTable)sender;
			string str = htmlTable.ID.Substring(0, htmlTable.ID.IndexOf("_"));
			TextBox filterControl = (TextBox)this.GetFilterControl(str);
			DateTime fromDate = this.GetFromDate(filterControl.Text);
			DateTime toDate = this.GetToDate(filterControl.Text);
			htmlTable.Attributes.Remove("date");
			DateTime dateTime = new DateTime();
			if (fromDate != dateTime)
			{
				dateTime = new DateTime();
				if (toDate == dateTime)
				{
					AttributeCollection attributes = htmlTable.Attributes;
					TimeSpan timeSpan = fromDate - new DateTime(2000, 1, 1);
					attributes.Add("date", timeSpan.Days.ToString());
				}
			}
			htmlTable.Style.Remove("display");
			htmlTable.Style.Remove("left");
			string item = this.Page.Request.Params["__EVENTTARGET"];
			if (item == null || !item.EndsWith(string.Concat("$", str, "_Calendar")))
			{
				flag = false;
			}
			else
			{
				dateTime = new DateTime();
				flag = toDate == dateTime;
			}
			bool flag1 = flag;
			htmlTable.Style.Add("display", (flag1 ? "inherit" : "none"));
		}

		protected void DateFilterCalendarSelectionChanged(object sender, EventArgs e)
		{
			DateTime visibleDate;
			string d = ((Control)sender).ID;
			string str = d.Substring(0, d.IndexOf("_"));
			TextBox filterControl = (TextBox)this.GetFilterControl(str);
			System.Web.UI.WebControls.Calendar calendar = (System.Web.UI.WebControls.Calendar)sender;
			DateTime fromDate = this.GetFromDate(filterControl.Text);
			DateTime toDate = this.GetToDate(filterControl.Text);
			DateTime selectedDate = calendar.SelectedDate;
			DateTime dateTime = new DateTime();
			if (fromDate != dateTime)
			{
				dateTime = new DateTime();
				if (toDate != dateTime)
				{
					filterControl.Text = selectedDate.ToShortDateString();
					visibleDate = calendar.VisibleDate;
					this.Search(sender, e);
					((System.Web.UI.WebControls.Calendar)this.GetFilterControl(string.Concat(str, "_Calendar"))).VisibleDate = visibleDate;
					return;
				}
				dateTime = new DateTime();
				if (toDate == dateTime)
				{
					if (selectedDate == fromDate)
					{
						filterControl.Text = selectedDate.ToShortDateString();
					}
					else if (selectedDate >= fromDate)
					{
						filterControl.Text = string.Concat(fromDate.ToShortDateString(), this.RangeSeparator, selectedDate.ToShortDateString());
					}
					else
					{
						filterControl.Text = string.Concat(selectedDate.ToShortDateString(), this.RangeSeparator, fromDate.ToShortDateString());
					}
				}
			}
			else
			{
				filterControl.Text = selectedDate.ToShortDateString();
				visibleDate = calendar.VisibleDate;
				this.Search(sender, e);
				((System.Web.UI.WebControls.Calendar)this.GetFilterControl(string.Concat(str, "_Calendar"))).VisibleDate = visibleDate;
				return;
			}
			visibleDate = calendar.VisibleDate;
			this.Search(sender, e);
			((System.Web.UI.WebControls.Calendar)this.GetFilterControl(string.Concat(str, "_Calendar"))).VisibleDate = visibleDate;
		}

		public List<ReservationInfo> Filter(List<ReservationInfo> list, string columnName, string text)
		{
			string str;
			PropertyInfo property = typeof(ReservationInfo).GetProperty(columnName);
			if (text == string.Empty)
			{
				return list;
			}
			if (columnName == "StartDate")
			{
				return list;
			}
			List<ReservationInfo> reservationInfos = new List<ReservationInfo>();
			foreach (ReservationInfo reservationInfo in list)
			{
				if (columnName == "CategoryName")
				{
					int num = int.Parse(text);
					if (num != Null.NullInteger && reservationInfo.CategoryID != num)
					{
						continue;
					}
					reservationInfos.Add(reservationInfo);
				}
				else if (!property.PropertyType.Equals(typeof(DateTime)))
				{
					object value = property.GetValue(reservationInfo, null);
					if (columnName == "StartTime" || columnName == "EndTime")
					{
						str = string.Format("{0:t}", value);
					}
					else
					{
						str = (property.PropertyType.Equals(typeof(DateTime)) || property.PropertyType.Equals(typeof(DateTimeOffset)) ? string.Format("{0:d}", value) : value.ToString());
					}
					string str1 = str;
					if (str1 == null || str1 == string.Empty)
					{
						str1 = Localization.GetString("None", base.LocalResourceFile);
					}
					if (!str1.ToLower().Contains(text.ToLower()))
					{
						continue;
					}
					reservationInfos.Add(reservationInfo);
				}
				else
				{
					DateTime dateTime = (DateTime)property.GetValue(reservationInfo, null);
					DateTime fromDate = this.GetFromDate(text);
					DateTime toDate = this.GetToDate(text);
					DateTime dateTime1 = new DateTime();
					if (fromDate != dateTime1)
					{
						dateTime1 = new DateTime();
						if (toDate != dateTime1 && dateTime.Date >= fromDate && dateTime.Date <= toDate)
						{
							goto Label0;
						}
					}
					dateTime1 = new DateTime();
					if (!(fromDate != dateTime1) || !(dateTime.Date == fromDate))
					{
						dateTime1 = new DateTime();
						if (!(toDate != dateTime1) || !(dateTime.Date == toDate))
						{
							continue;
						}
					}
				Label0:
					reservationInfos.Add(reservationInfo);
				}
			}
			return reservationInfos;
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
			return reservationInfo;
		}

		private List<ReservationInfo> FindEventInfoListByEmailOrPhone(ArrayList eventInfoList, string email, string phone, int startIndex)
		{
			List<ReservationInfo> reservationInfos = new List<ReservationInfo>();
			while (startIndex < eventInfoList.Count)
			{
				ReservationInfo item = (ReservationInfo)eventInfoList[startIndex];
				if (!string.IsNullOrEmpty(item.Email) && !string.IsNullOrEmpty(email) && item.Email != this.NotAvailable && email != this.NotAvailable && item.Email.Trim().ToLower() == email.Trim().ToLower() || !string.IsNullOrEmpty(item.Phone) && !string.IsNullOrEmpty(phone) && item.Phone != this.NotAvailable && phone != this.NotAvailable && item.Phone.Trim().ToLower() == phone.Trim().ToLower())
				{
					reservationInfos.Add(item);
				}
				startIndex++;
			}
			return reservationInfos;
		}

		private string GetCategoryName(int categoryID)
		{
			string name;
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
				return null;
			}
			finally
			{
				((IDisposable)enumerator).Dispose();
			}
			return name;
		}

		public Control GetFilterControl(string id)
		{
			return this.dataGrid.Controls[0].Controls[(this.ViewReservationsListSettings.AllowPaging ? 1 : 0)].FindControl(id);
		}

		public string GetFilterText(string columnName)
		{
			Control control = this.dataGrid.Controls[0].Controls[(this.ViewReservationsListSettings.AllowPaging ? 1 : 0)].FindControl(columnName);
			if (control is TextBox)
			{
				return ((TextBox)control).Text;
			}
			if (!(control is DropDownList))
			{
				return string.Empty;
			}
			return ((DropDownList)control).SelectedValue;
		}

		private DateTime GetFromDate(string range)
		{
			return this.TryParseDate(range.Split(this.RangeSeparator.ToCharArray())[0].Trim());
		}

		private DateTime GetToDate(string range)
		{
			if (!range.Contains(this.RangeSeparator))
			{
				return new DateTime();
			}
			return this.TryParseDate(range.Split(this.RangeSeparator.ToCharArray())[1].Trim());
		}

		private void Highlight(TableCell tableCell, string searchExpression)
		{
			string text = tableCell.Text;
			if (text.ToLower().IndexOf(searchExpression.ToLower()) != -1)
			{
				tableCell.Controls.Add(new LiteralControl(string.Concat(new string[] { "<div title=\"", tableCell.Text, "\">", text.Substring(0, text.ToLower().IndexOf(searchExpression.ToLower())), "<span class=\"", base.GetType().BaseType.Namespace.Replace(".", "_"), "_DataGrid_Highlight\">", text.Substring(text.ToLower().IndexOf(searchExpression.ToLower()), searchExpression.Length), "</span>", text.Substring(text.ToLower().IndexOf(searchExpression.ToLower()) + searchExpression.Length), "</div>" })));
			}
		}

		public void ItemCommand(object source, DataGridCommandEventArgs e)
		{
			int moduleId;
			string empty;
			string str;
			try
			{
				OrderedDictionary orderedDictionaries = new OrderedDictionary();
				if (e.CommandName == "Search")
				{
					this.dataGrid.CurrentPageIndex = 0;
					this.BindData();
					this.SetFiltersText();
				}
				else if (e.CommandName == "Settings")
				{
					HttpResponse response = base.Response;
					string[] strArrays = new string[4];
					moduleId = base.ModuleId;
					strArrays[0] = string.Concat("mid=", moduleId.ToString());
					strArrays[1] = "List=ViewReservationsListSettings";
					strArrays[2] = "Control=ViewReservations";
					HttpServerUtility server = base.Server;
					string empty1 = string.Empty;
					string[] queryStringParams = new string[] { this.QueryStringParams, "Returning=True", null };
					if (this.dataGrid.CurrentPageIndex > 0)
					{
						moduleId = this.dataGrid.CurrentPageIndex + 1;
						str = string.Concat("currentpage=", moduleId.ToString());
					}
					else
					{
						str = string.Empty;
					}
					queryStringParams[2] = str;
					strArrays[3] = string.Concat("ReturnUrl=", server.UrlEncode(Globals.NavigateURL(empty1, queryStringParams)));
					response.Redirect(Globals.NavigateURL("ListSettings", strArrays));
				}
				else if (e.CommandName == "View")
				{
					int num = int.Parse(((Label)e.Item.FindControl("reservationID")).Text);
					HttpResponse httpResponse = base.Response;
					int tabId = base.TabId;
					string str1 = string.Empty;
					string[] strArrays1 = new string[] { string.Concat("EventID=", num.ToString()), null };
					HttpServerUtility httpServerUtility = base.Server;
					string empty2 = string.Empty;
					string[] queryStringParams1 = new string[] { this.QueryStringParams, "Returning=True", null };
					if (this.dataGrid.CurrentPageIndex > 0)
					{
						moduleId = this.dataGrid.CurrentPageIndex + 1;
						empty = string.Concat("currentpage=", moduleId.ToString());
					}
					else
					{
						empty = string.Empty;
					}
					queryStringParams1[2] = empty;
					strArrays1[1] = string.Concat("ReturnUrl=", httpServerUtility.UrlEncode(Globals.NavigateURL(empty2, queryStringParams1)));
					httpResponse.Redirect(Globals.NavigateURL(tabId, str1, strArrays1));
				}
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
			}
		}

		public void ItemCreated(object source, DataGridItemEventArgs e)
		{
			Control textBox;
			try
			{
				if (e.Item.ItemType == ListItemType.Header && !this.IsPrintable)
				{
					DataGridItem dataGridItem = new DataGridItem(0, 0, ListItemType.Header);
					ImageButton imageButton = new ImageButton()
					{
						CommandName = "Search",
						ImageUrl = "~/images/icon_search_16px.gif"
					};
					imageButton.Attributes.Add("title", Localization.GetString("Search", base.LocalResourceFile));
					imageButton.CssClass = "filterImageButton";
					TableCell tableCell = new TableCell();
					tableCell.Controls.Add(imageButton);
					dataGridItem.Cells.Add(tableCell);
					foreach (DisplayColumnInfo visibleDisplayColumnList in this.ViewReservationsListSettings.VisibleDisplayColumnList)
					{
						if (!visibleDisplayColumnList.Visible)
						{
							continue;
						}
						if (visibleDisplayColumnList.ColumnName != "CategoryName")
						{
							textBox = new TextBox()
							{
								ID = visibleDisplayColumnList.ColumnName,
								CssClass = "NormalTextBox filterTextBox Gafware_Modules_Reservations_Input"
							};
						}
						else
						{
							DropDownList dropDownList = new DropDownList()
							{
								ID = visibleDisplayColumnList.ColumnName,
								CssClass = "NormalTextBox filterTextBox Gafware_Modules_Reservations_Input",
								DataSource = (new CategoryController()).GetCategoryList(base.TabModuleId),
								DataTextField = "Name",
								DataValueField = "CategoryID"
							};
							dropDownList.DataBind();
							ListItemCollection items = dropDownList.Items;
							string str = Localization.GetString("All", base.LocalResourceFile);
							int nullInteger = Null.NullInteger;
							items.Insert(0, new ListItem(str, nullInteger.ToString()));
							dropDownList.SelectedIndexChanged += new EventHandler(this.Search);
							dropDownList.AutoPostBack = true;
							textBox = dropDownList;
						}
						tableCell = new TableCell()
						{
							CssClass = string.Concat(base.GetType().BaseType.Namespace.Replace(".", "_"), "_DataGrid_FilterStyle")
						};
						tableCell.Controls.Add(textBox);
						dataGridItem.Cells.Add(tableCell);
						if (!typeof(ReservationInfo).GetProperty(visibleDisplayColumnList.ColumnName).PropertyType.Equals(typeof(DateTime)) || !(visibleDisplayColumnList.ColumnName != "StartTime") || !(visibleDisplayColumnList.ColumnName != "EndTime"))
						{
							continue;
						}
						HtmlTable htmlTable = new HtmlTable()
						{
							ID = string.Concat(visibleDisplayColumnList.ColumnName, "_AbsoluteTable")
						};
						htmlTable.Attributes.Add("class", string.Concat("Gafware_Modules_Reservations_DateFilter_", visibleDisplayColumnList.ColumnName, "_AbsoluteTable Gafware_Modules_Reservations_DateFilter_AbsoluteTable"));
						htmlTable.Style.Add("position", "absolute");
						htmlTable.Rows.Add(new HtmlTableRow());
						htmlTable.Rows[0].Cells.Add(new HtmlTableCell());
						htmlTable.Rows[0].Cells[0].Controls.Add(this.CreateCalendar(visibleDisplayColumnList.ColumnName));
						tableCell.Controls.Add(htmlTable);
						TextBox textBox1 = (TextBox)textBox;
						textBox1.CssClass = string.Concat(textBox1.CssClass, " Gafware_Modules_Reservations_DateFilter_", visibleDisplayColumnList.ColumnName, "_TextBox Gafware_Modules_Reservations_DateFilter_TextBox");
						textBox1.Attributes.Add("onfocus", string.Concat("showFilterCalendar(\"", visibleDisplayColumnList.ColumnName, "\");"));
						htmlTable.PreRender += new EventHandler(this.DateFilterCalendarPreRender);
					}
					this.dataGrid.Controls[0].Controls.AddAt((this.ViewReservationsListSettings.AllowPaging ? 1 : 0), dataGridItem);
				}
				if (e.Item.ItemType == ListItemType.Header)
				{
					string str1 = string.Concat(" ", base.GetType().BaseType.Namespace.Replace(".", "_"), "_DataGrid_ItemStyle_Sorted");
					for (int i = 1; i < this.dataGrid.Columns.Count; i++)
					{
						if (this.SortColumnList.Count <= 0 || !(this.SortColumnList[0].ColumnName == this.dataGrid.Columns[i].SortExpression))
						{
							TableCell item = e.Item.Cells[i];
							if (item.Controls.Count != 0)
							{
								LinkButton text = (LinkButton)item.Controls[0];
								item.Controls.RemoveAt(0);
								text.ToolTip = text.Text;
								text.Controls.Add(new LiteralControl(text.Text));
								Panel panel = new Panel();
								panel.Controls.Add(text);
								item.Controls.Add(panel);
							}
							else
							{
								item.Controls.Add(new Panel());
								item.Controls[0].Controls.Add(new Label());
								((Label)item.Controls[0].Controls[0]).ToolTip = item.Text;
								item.Controls[0].Controls[0].Controls.Add(new LiteralControl(item.Text));
							}
							if (this.dataGrid.Columns[i].ItemStyle.CssClass.IndexOf(str1) != -1)
							{
								this.dataGrid.Columns[i].ItemStyle.CssClass = this.dataGrid.Columns[i].ItemStyle.CssClass.Replace(str1, string.Empty);
							}
						}
						else
						{
							TableCell item1 = e.Item.Cells[i];
							HtmlImage htmlImage = new HtmlImage()
							{
								Src = (this.SortColumnList[0].Direction == SortColumnInfo.SortDirection.Ascending ? "~/images/sortascending.gif" : "~/images/sortdescending.gif"),
								Alt = Localization.GetString((this.SortColumnList[0].Direction == SortColumnInfo.SortDirection.Ascending ? "Ascending" : "Descending"), base.LocalResourceFile),
								Border = 0
							};
							if (item1.Controls.Count != 0)
							{
								LinkButton linkButton = (LinkButton)item1.Controls[0];
								item1.Controls.RemoveAt(0);
								linkButton.ToolTip = linkButton.Text;
								linkButton.Controls.Add(new LiteralControl(linkButton.Text));
								linkButton.Controls.Add(htmlImage);
								Panel panel1 = new Panel();
								panel1.Controls.Add(linkButton);
								item1.Controls.Add(panel1);
							}
							else
							{
								item1.Controls.Add(new Panel());
								item1.Controls[0].Controls.Add(new Label());
								((Label)item1.Controls[0].Controls[0]).ToolTip = item1.Text;
								item1.Controls[0].Controls[0].Controls.Add(new LiteralControl(item1.Text));
								item1.Controls[0].Controls[0].Controls.Add(htmlImage);
							}
							TableItemStyle itemStyle = this.dataGrid.Columns[i].ItemStyle;
							itemStyle.CssClass = string.Concat(itemStyle.CssClass, str1);
						}
					}
				}
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
			}
		}

		public void ItemDataBound(object source, DataGridItemEventArgs e)
		{
			string cssClass;
			try
			{
				if (e.Item.ItemType == ListItemType.Header)
				{
					e.Item.Cells[0].Visible = !this.IsPrintable;
				}
				else if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem || e.Item.ItemType == ListItemType.SelectedItem)
				{
					e.Item.Cells[0].Visible = !this.IsPrintable;
					int num = 0;
					foreach (TableCell control in e.Item.Controls)
					{
						string text = control.Text;
						if (num > 0)
						{
							if (Null.IsNull(DataBinder.Eval(e.Item.DataItem, this.ViewReservationsListSettings.VisibleDisplayColumnList[num - 1].ColumnName)))
							{
								control.Text = Localization.GetString("None", base.LocalResourceFile);
							}
							string item = (string)this.FiltersOrderedDictionary[this.ViewReservationsListSettings.VisibleDisplayColumnList[num - 1].ColumnName];
							if (!(this.ViewReservationsListSettings.VisibleDisplayColumnList[num - 1].ColumnName != "Email") || !(item != string.Empty))
							{
								LiteralControl literalControl = null;
								literalControl = (this.ViewReservationsListSettings.VisibleDisplayColumnList[num - 1].ColumnName != "Email" ? new LiteralControl(string.Concat(new string[] { "<div title=\"", control.Text, "\">", control.Text, "</div>" })) : new LiteralControl(string.Concat(new string[] { "<div title=\"", ((ReservationInfo)e.Item.DataItem).Email, "\">", control.Text, "</div>" })));
								control.Controls.Add(literalControl);
							}
							else
							{
								this.Highlight(control, item);
							}
						}
						num++;
					}
					if (e.Item.ItemType == ListItemType.Item)
					{
						cssClass = this.dataGrid.ItemStyle.CssClass;
					}
					else
					{
						cssClass = (e.Item.ItemType == ListItemType.AlternatingItem ? this.dataGrid.AlternatingItemStyle.CssClass : this.dataGrid.SelectedItemStyle.CssClass);
					}
					e.Item.CssClass = cssClass;
				}
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
			}
		}

		private void LoadColumns()
		{
			string str;
			foreach (DisplayColumnInfo displayColumnList in this.ViewReservationsListSettings.DisplayColumnList)
			{
				if (!displayColumnList.Visible)
				{
					continue;
				}
				string str1 = Localization.GetString(displayColumnList.ColumnName, base.LocalResourceFile);
				string columnName = displayColumnList.ColumnName;
				if (displayColumnList.ColumnName == "StartTime" || displayColumnList.ColumnName == "EndTime")
				{
					str = "{0:t}";
				}
				else
				{
					str = (typeof(ReservationInfo).GetProperty(displayColumnList.ColumnName).PropertyType.Equals(typeof(DateTime)) ? "{0:d}" : string.Empty);
				}
				this.AddColumn(str1, columnName, str);
			}
		}

		protected void Page_Init(object sender, EventArgs e)
		{
			try
			{
				this.SetTheme();
				if (this.ViewReservationsListSettings.AllowPaging && this.ViewReservationsListSettings.PagerMode != Gafware.Modules.Reservations.PagerMode.DotNetNuke)
				{
					this.dataGrid.PageIndexChanged += new DataGridPageChangedEventHandler(this.PageChanged);
				}
				this.LoadColumns();
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
				if (!this.CanViewReservations)
				{
					base.Response.Redirect(Globals.NavigateURL(), true);
				}
				if (!base.IsPostBack)
				{
					this.dataGrid.AllowSorting = (!this.ViewReservationsListSettings.AllowSorting ? false : !this.IsPrintable);
					if (this.ViewReservationsListSettings.AllowPaging)
					{
						this.dataGrid.AllowPaging = true;
						this.dataGrid.PageSize = this.ViewReservationsListSettings.PageSize;
						if (this.ViewReservationsListSettings.PagerMode != Gafware.Modules.Reservations.PagerMode.DotNetNuke)
						{
							this.dataGrid.PagerStyle.Position = this.ViewReservationsListSettings.PagerPosition;
							if (this.ViewReservationsListSettings.PagerMode != Gafware.Modules.Reservations.PagerMode.NextPrev)
							{
								this.dataGrid.PagerStyle.Mode = System.Web.UI.WebControls.PagerMode.NumericPages;
							}
							else
							{
								this.dataGrid.PagerStyle.Mode = System.Web.UI.WebControls.PagerMode.NextPrev;
								this.dataGrid.PagerStyle.NextPageText = Localization.GetString("NextPage", base.LocalResourceFile);
								this.dataGrid.PagerStyle.PrevPageText = Localization.GetString("PrevPage", base.LocalResourceFile);
							}
						}
						else
						{
							this.dataGrid.PagerStyle.Visible = false;
							if (base.Request.QueryString["currentpage"] == null)
							{
								this.dataGrid.CurrentPageIndex = 0;
							}
							else
							{
								this.dataGrid.CurrentPageIndex = int.Parse(base.Request.QueryString["currentpage"]) - 1;
							}
						}
					}
					this.BindData();
					this.SetFiltersText();
				}
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
			}
		}

		protected void Page_PreRender(object sender, EventArgs e)
		{
			bool flag;
			try
			{
				LinkButton linkButton = this.printCommandButton;
				string[] filterAndSortQueryStringParams = new string[5];
				int moduleId = base.ModuleId;
				filterAndSortQueryStringParams[0] = string.Concat("mid=", moduleId.ToString());
				filterAndSortQueryStringParams[1] = this.FilterAndSortQueryStringParams;
				filterAndSortQueryStringParams[2] = "Printable=True";
				filterAndSortQueryStringParams[3] = string.Concat("SkinSrc=", base.Request.ApplicationPath, "/Portals/_default/Skins/_default/No%20Skin");
				filterAndSortQueryStringParams[4] = string.Concat("ContainerSrc=", base.Request.ApplicationPath, "/Portals/_default/Containers/_default/No%20Container");
				linkButton.OnClientClick = string.Concat("window.open('", Globals.NavigateURL("ViewReservations", filterAndSortQueryStringParams), "'); return false;");
				this.buttonsTable.Visible = !this.IsPrintable;
				if (this.ViewReservationsListSettings.AllowPaging && this.ViewReservationsListSettings.PagerMode == Gafware.Modules.Reservations.PagerMode.DotNetNuke)
				{
					PagingControl pagingControl = this.topPagingControl;
					PagingControl pagingControl1 = this.bottomPagingControl;
					int pageSize = this.dataGrid.PageSize;
					moduleId = pageSize;
					pagingControl1.PageSize = pageSize;
					pagingControl.PageSize = moduleId;
					PagingControl pagingControl2 = this.topPagingControl;
					int currentPageIndex = this.dataGrid.CurrentPageIndex + 1;
					moduleId = currentPageIndex;
					this.bottomPagingControl.CurrentPage = currentPageIndex;
					pagingControl2.CurrentPage = moduleId;
					PagingControl pagingControl3 = this.topPagingControl;
					PagingControl pagingControl4 = this.bottomPagingControl;
					int count = this.ReservationsList.Count;
					moduleId = count;
					pagingControl4.TotalRecords = count;
					pagingControl3.TotalRecords = moduleId;
					PagingControl pagingControl5 = this.topPagingControl;
					PagingControl pagingControl6 = this.bottomPagingControl;
					int tabId = base.TabId;
					moduleId = tabId;
					pagingControl6.TabID = tabId;
					pagingControl5.TabID = moduleId;
					PagingControl pagingControl7 = this.topPagingControl;
					PagingControl pagingControl8 = this.bottomPagingControl;
					string queryStringParams = this.QueryStringParams;
					string str = queryStringParams;
					pagingControl8.QuerystringParams = queryStringParams;
					pagingControl7.QuerystringParams = str;
					this.topPagingControlDiv.Visible = (this.ViewReservationsListSettings.PagerPosition == PagerPosition.Top ? true : this.ViewReservationsListSettings.PagerPosition == PagerPosition.TopAndBottom);
					this.bottomPagingControlDiv.Visible = (this.ViewReservationsListSettings.PagerPosition == PagerPosition.Bottom ? true : this.ViewReservationsListSettings.PagerPosition == PagerPosition.TopAndBottom);
				}
				if (bool.TryParse(base.Request.QueryString["Printable"], out flag) & flag)
				{
					this.dataGrid.Width = new Unit(850);
				}
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
			}
		}

		public void PageChanged(object source, DataGridPageChangedEventArgs e)
		{
			try
			{
				this.dataGrid.CurrentPageIndex = e.NewPageIndex;
				this.BindData();
				this.SetFiltersText();
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
				this.ViewState["SortColumnList"] = this.ViewReservationsListSettings.SerializeSortColumnList(this._SortColumnList);
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
			}
			return base.SaveViewState();
		}

		public void Search(object source, EventArgs e)
		{
			try
			{
				this.dataGrid.CurrentPageIndex = 0;
				this.BindData();
				this.SetFiltersText();
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
			}
		}

		public void SetFiltersText()
		{
			foreach (DisplayColumnInfo displayColumnList in this.ViewReservationsListSettings.DisplayColumnList)
			{
				if (!displayColumnList.Visible)
				{
					continue;
				}
				this.SetFilterText(displayColumnList.ColumnName, (string)this.FiltersOrderedDictionary[displayColumnList.ColumnName]);
			}
		}

		public void SetFilterText(string columnName, string text)
		{
			Control control = this.dataGrid.Controls[0].Controls[(this.ViewReservationsListSettings.AllowPaging ? 1 : 0)].FindControl(columnName);
			if (control is TextBox)
			{
				((TextBox)control).Text = text;
				return;
			}
			if (control is DropDownList)
			{
				((DropDownList)control).SelectedValue = text;
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

		public void SortCommand(object source, DataGridSortCommandEventArgs e)
		{
			try
			{
				SortColumnInfo sortColumnInfo = new SortColumnInfo()
				{
					ColumnName = e.SortExpression,
					Direction = SortColumnInfo.SortDirection.Ascending
				};
				if (this.SortColumnList.Count > 0 && this.SortColumnList[0].ColumnName == sortColumnInfo.ColumnName)
				{
					sortColumnInfo.Direction = (this.SortColumnList[0].Direction == SortColumnInfo.SortDirection.Ascending ? SortColumnInfo.SortDirection.Descending : SortColumnInfo.SortDirection.Ascending);
				}
				this.SortColumnList.Insert(0, sortColumnInfo);
				this.BindData();
				this.SetFiltersText();
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
			}
		}

		private DateTime TryParseDate(string date)
		{
			DateTime dateTime = new DateTime();
			DateTime.TryParse(date, out dateTime);
			return dateTime;
		}
	}
}