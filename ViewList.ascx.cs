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
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Gafware.Modules.Reservations
{
    public partial class ViewList : PortalModuleBase
    {
		protected IList _List;

		protected Gafware.Modules.Reservations.ListSettings _ListSettings;

		protected Gafware.Modules.Reservations.Helper _Helper;

		protected Gafware.Modules.Reservations.ModuleSettings _ModuleSettings;

		protected List<SortColumnInfo> _SortColumnList;

		protected OrderedDictionary _FiltersOrderedDictionary;

		protected string _NotAvailable;

		private const string QUOTE = "\"";

		private const string ESCAPED_QUOTE = "\"\"";

		private static char[] CHARACTERS_THAT_MUST_BE_QUOTED;

		protected bool CanViewList
		{
			get
			{
				if (base.UserId == Null.NullInteger)
				{
					return false;
				}
				return this.ListSettings.CanViewList(base.UserId);
			}
		}

		public string FilterAndSortQueryStringParams
		{
			get
			{
				string empty = string.Empty;
				foreach (DisplayColumnInfo displayColumnList in this.ListSettings.DisplayColumnList)
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
					empty = string.Concat(empty, "&SortColumnList=", this.ListSettings.SerializeSortColumnList(this.SortColumnList));
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
					foreach (DisplayColumnInfo displayColumnList in this.ListSettings.DisplayColumnList)
					{
						if (!displayColumnList.Visible)
						{
							continue;
						}
						string empty = string.Empty;
						if (!base.IsPostBack)
						{
							empty = base.Request.QueryString[displayColumnList.ColumnName];
							List<string> strs = new List<string>(base.Request.QueryString.AllKeys);
							if (!strs.Contains("currentpage") && !strs.Contains("Returning") && !this.IsPrintable)
							{
								empty = this.ListSettings.GetDefaultFilterValue(displayColumnList.ColumnName);
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

		protected string IdPropertyName
		{
			get
			{
				return this.ListSettings.IdPropertyName;
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

		protected IList List
		{
			get
			{
				if (this._List == null)
				{
					this._List = this.ListSettings.GetList(this.FiltersOrderedDictionary);
					foreach (DisplayColumnInfo displayColumnList in this.ListSettings.DisplayColumnList)
					{
						if (!displayColumnList.Visible)
						{
							continue;
						}
						this._List = this.Filter(this._List, displayColumnList.ColumnName, (string)this.FiltersOrderedDictionary[displayColumnList.ColumnName]);
					}
				}
				return this._List;
			}
		}

		protected Gafware.Modules.Reservations.ListSettings ListSettings
		{
			get
			{
				if (this._ListSettings == null)
				{
					try
					{
						this._ListSettings = (Gafware.Modules.Reservations.ListSettings)Activator.CreateInstance(base.GetType().BaseType.Assembly.GetType(string.Concat(base.GetType().BaseType.Namespace, ".", base.Request.QueryString["List"])), new object[] { this });
					}
					catch (Exception exception)
					{
						base.Response.Redirect(Globals.NavigateURL());
					}
				}
				return this._ListSettings;
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

		public string QueryStringParams
		{
			get
			{
				string[] item = new string[] { "&ctl=", base.Request.QueryString["ctl"], "&mid=", null, null, null, null, null };
				item[3] = base.ModuleId.ToString();
				item[4] = "&List=";
				item[5] = base.Request.QueryString["List"];
				item[6] = "&";
				item[7] = this.FilterAndSortQueryStringParams;
				return string.Concat(item);
			}
		}

		private string ReturnUrl
		{
			get
			{
				string empty;
				HttpServerUtility server = base.Server;
				string str = string.Empty;
				string[] queryStringParams = new string[] { this.QueryStringParams, "Returning=True", null };
				if (this.dataGrid.CurrentPageIndex > 0)
				{
					int currentPageIndex = this.dataGrid.CurrentPageIndex + 1;
					empty = string.Concat("currentpage=", currentPageIndex.ToString());
				}
				else
				{
					empty = string.Empty;
				}
				queryStringParams[2] = empty;
				return server.UrlEncode(Globals.NavigateURL(str, queryStringParams));
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
							this._SortColumnList = this.ListSettings.DeserializeSortColumnList((string)this.ViewState["SortColumnList"]);
						}
						else
						{
							this.SortColumnList = this.ListSettings.SortColumnList;
						}
					}
					else if (base.Request.QueryString["SortColumnList"] != null)
					{
						this.SortColumnList = this.ListSettings.DeserializeSortColumnList(base.Request.QueryString["SortColumnList"]);
					}
					else
					{
						this.SortColumnList = this.ListSettings.SortColumnList;
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
				this.ViewState["SortColumnList"] = this.ListSettings.SerializeSortColumnList(this._SortColumnList);
			}
		}

		static ViewList()
		{
			ViewList.CHARACTERS_THAT_MUST_BE_QUOTED = new char[] { ',', '\"', '\n' };
		}

		public ViewList()
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
			if (this.ListSettings.InfoType.GetProperty(dataField) == null)
			{
				UnboundColumn unboundColumn = new UnboundColumn()
				{
					DataField = dataField,
					HeaderText = title,
					SortExpression = dataField
				};
				unboundColumn.HeaderStyle.CssClass = string.Concat(base.GetType().BaseType.Namespace.Replace(".", "_"), "_DataGrid_", dataField, "_HeaderStyle");
				unboundColumn.ItemStyle.CssClass = string.Concat(base.GetType().BaseType.Namespace.Replace(".", "_"), "_DataGrid_", dataField, "_ItemStyle");
				this.dataGrid.Columns.Add(unboundColumn);
				return;
			}
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
				this.ListSettings.SortList(this.List, this.SortColumnList);
			}
			int count = this.List.Count / this.dataGrid.PageSize;
			int num = this.List.Count % this.dataGrid.PageSize;
			if (this.ListSettings.AllowPaging)
			{
				int count1 = this.List.Count / this.ListSettings.PageSize + (this.List.Count % this.ListSettings.PageSize == 0 ? 0 : 1) - 1;
				if (this.List.Count == 0)
				{
					this.dataGrid.CurrentPageIndex = 0;
				}
				else if (this.dataGrid.CurrentPageIndex > count1)
				{
					this.dataGrid.CurrentPageIndex = count1;
				}
			}
			this.dataGrid.DataSource = this.List;
			this.dataGrid.DataBind();
			Label label = this.numberOfRecordsFoundLabel;
			string str = Localization.GetString("Total", base.LocalResourceFile);
			int num1 = this.List.Count;
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
					DateTime fromDate = this.Helper.GetFromDate(text);
					DateTime toDate = this.Helper.GetToDate(text);
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
				DateTime fromDate = this.Helper.GetFromDate(filterControl.Text);
				DateTime toDate = this.Helper.GetToDate(filterControl.Text);
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
			HtmlTable htmlTable = (HtmlTable)sender;
			string str = htmlTable.ID.Substring(0, htmlTable.ID.IndexOf("_"));
			TextBox filterControl = (TextBox)this.GetFilterControl(str);
			DateTime fromDate = this.Helper.GetFromDate(filterControl.Text);
			DateTime toDate = this.Helper.GetToDate(filterControl.Text);
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
			bool flag = (item == null ? false : item.EndsWith(string.Concat("$", str, "_Calendar")));
			htmlTable.Style.Add("display", (flag ? "inherit" : "none"));
		}

		protected void DateFilterCalendarSelectionChanged(object sender, EventArgs e)
		{
			DateTime visibleDate;
			string d = ((Control)sender).ID;
			string str = d.Substring(0, d.IndexOf("_"));
			TextBox filterControl = (TextBox)this.GetFilterControl(str);
			System.Web.UI.WebControls.Calendar calendar = (System.Web.UI.WebControls.Calendar)sender;
			DateTime fromDate = this.Helper.GetFromDate(filterControl.Text);
			DateTime toDate = this.Helper.GetToDate(filterControl.Text);
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
						filterControl.Text = string.Concat(fromDate.ToShortDateString(), this.Helper.DateRangeSeparator, selectedDate.ToShortDateString());
					}
					else
					{
						filterControl.Text = string.Concat(selectedDate.ToShortDateString(), this.Helper.DateRangeSeparator, fromDate.ToShortDateString());
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

		public static string Escape(string s)
		{
			if (s.Contains("\""))
			{
				s = s.Replace("\"", "\"\"");
			}
			if (s.IndexOfAny(ViewList.CHARACTERS_THAT_MUST_BE_QUOTED) > -1)
			{
				s = string.Concat("\"", s, "\"");
			}
			return s;
		}

		public void ExportCSVCommandButtonClicked(object source, EventArgs e)
		{
			string str;
			base.Response.Clear();
			base.Response.ContentType = "application/octet-stream";
			base.Response.AppendHeader("Content-disposition", "attachment; filename=Export.csv");
			string empty = string.Empty;
			foreach (DisplayColumnInfo visibleDisplayColumnList in this.ListSettings.VisibleDisplayColumnList)
			{
				string[] localizedColumnName = new string[] { empty, null, null, null, null };
				localizedColumnName[1] = (empty == string.Empty ? string.Empty : ",");
				localizedColumnName[2] = "\"";
				localizedColumnName[3] = visibleDisplayColumnList.LocalizedColumnName;
				localizedColumnName[4] = "\"";
				empty = string.Concat(localizedColumnName);
			}
			base.Response.Write(empty);
			base.Response.Write(Environment.NewLine);
			if (this.SortColumnList.Count > 0)
			{
				this.ListSettings.SortList(this.List, this.SortColumnList);
			}
			foreach (object list in this.List)
			{
				empty = string.Empty;
				foreach (DisplayColumnInfo displayColumnInfo in this.ListSettings.VisibleDisplayColumnList)
				{
					object obj = this.ListSettings.Eval(list, displayColumnInfo.ColumnName);
					if (obj != null && obj.GetType() == typeof(bool))
					{
						bool flag = (bool)obj;
						str = Localization.GetString(flag.ToString(), base.LocalResourceFile);
					}
					else if (Null.IsNull(obj))
					{
						str = Localization.GetString("None", base.LocalResourceFile);
					}
					else if (displayColumnInfo.ColumnName == "StartTime" || displayColumnInfo.ColumnName == "EndTime")
					{
						str = ((DateTime)obj).ToShortTimeString();
					}
					else
					{
						if (obj.GetType() == typeof(DateTime))
						{
							if (((DateTime)obj).TimeOfDay != new TimeSpan())
							{
								goto Label1;
							}
							str = ((DateTime)obj).ToShortDateString();
							goto Label0;
						}
					Label1:
						str = obj.ToString();
					}
				Label0:
					string str1 = str;
					empty = string.Concat(empty, (empty == string.Empty ? string.Empty : ","), ViewList.Escape(str1));
				}
				base.Response.Write(empty);
				base.Response.Write(Environment.NewLine);
			}
			base.Response.Flush();
			base.Response.End();
		}

		public IList Filter(IList list, string columnName, string text)
		{
			if (text == string.Empty || this.ListSettings.SkipFilter(columnName))
			{
				return list;
			}
			PropertyInfo property = this.ListSettings.InfoType.GetProperty(columnName);
			IList emptyList = this.ListSettings.GetEmptyList();
			foreach (object obj in list)
			{
				if (this.ListSettings.ImplementsCustomFilter(columnName))
				{
					if (!this.ListSettings.ApplyCustomFilter(obj, columnName, text))
					{
						continue;
					}
					emptyList.Add(obj);
				}
				else if (!property.PropertyType.Equals(typeof(DateTime)) || this.ListSettings.ExcludeFromDateRangeFilters(columnName))
				{
					object value = property.GetValue(obj, null);
					string str = HtmlUtils.Clean(string.Format(this.ListSettings.GetDisplayFormat(columnName), value), true);
					if (str == null || str == string.Empty)
					{
						str = Localization.GetString("None", base.LocalResourceFile);
					}
					if (!str.ToLower().Contains(text.ToLower()))
					{
						continue;
					}
					emptyList.Add(obj);
				}
				else
				{
					DateTime dateTime = (DateTime)property.GetValue(obj, null);
					DateTime fromDate = this.Helper.GetFromDate(text);
					DateTime toDate = this.Helper.GetToDate(text);
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
					emptyList.Add(obj);
				}
			}
			return emptyList;
		}

		public Control GetFilterControl(string id)
		{
			return this.dataGrid.Controls[0].Controls[(this.ListSettings.AllowPaging ? 1 : 0)].FindControl(id);
		}

		public string GetFilterText(string columnName)
		{
			Control control = this.dataGrid.Controls[0].Controls[(this.ListSettings.AllowPaging ? 1 : 0)].FindControl(columnName);
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

		private string Highlight(string text, string searchExpression)
		{
			if (text.ToLower().IndexOf(searchExpression.ToLower()) != -1)
			{
				text = string.Concat(new string[] { text.Substring(0, text.ToLower().IndexOf(searchExpression.ToLower())), "<span class=\"", base.GetType().BaseType.Namespace.Replace(".", "_"), "_DataGrid_Highlight\">", text.Substring(text.ToLower().IndexOf(searchExpression.ToLower()), searchExpression.Length), "</span>", text.Substring(text.ToLower().IndexOf(searchExpression.ToLower()) + searchExpression.Length) });
			}
			return text;
		}

		public void ItemCommand(object source, DataGridCommandEventArgs e)
		{
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
					int moduleId = base.ModuleId;
					strArrays[0] = string.Concat("mid=", moduleId.ToString());
					strArrays[1] = string.Concat("List=", base.Request.QueryString["List"]);
					strArrays[2] = string.Concat("Control=", base.Request.QueryString["ctl"]);
					strArrays[3] = string.Concat("ReturnUrl=", this.ReturnUrl);
					response.Redirect(Globals.NavigateURL("ListSettings", strArrays));
				}
				else if (e.CommandName != "Sort")
				{
					this.ListSettings.HandleCommand(int.Parse(((Label)e.Item.FindControl("idLabel")).Text), e.CommandName, this, this.ReturnUrl);
					this.BindData();
					this.SetFiltersText();
				}
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
			}
		}

		public void ItemCreated(object source, DataGridItemEventArgs e)
		{
			try
			{
				if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem && !this.IsPrintable)
				{
					foreach (CommandButton commandButtonList in this.ListSettings.CommandButtonList)
					{
						ImageButton imageButton = new ImageButton()
						{
							CommandName = commandButtonList.CommandName,
							ImageUrl = commandButtonList.ImageUrl
						};
						foreach (string key in commandButtonList.Attributes.Keys)
						{
							imageButton.Attributes.Add(key, commandButtonList.Attributes[key]);
						}
						e.Item.Cells[0].Controls.Add(imageButton);
					}
				}
				else if (e.Item.ItemType == ListItemType.Header && !this.IsPrintable)
				{
					DataGridItem dataGridItem = new DataGridItem(0, 0, ListItemType.Header);
					ImageButton imageButton1 = new ImageButton()
					{
						CommandName = "Search",
						ImageUrl = "~/images/icon_search_16px.gif"
					};
					imageButton1.Attributes.Add("title", Localization.GetString("Search", base.LocalResourceFile));
					imageButton1.CssClass = "filterImageButton";
					TableCell tableCell = new TableCell();
					tableCell.Controls.Add(imageButton1);
					dataGridItem.Cells.Add(tableCell);
					foreach (DisplayColumnInfo visibleDisplayColumnList in this.ListSettings.VisibleDisplayColumnList)
					{
						if (!visibleDisplayColumnList.Visible)
						{
							continue;
						}
						WebControl filterControl = this.ListSettings.GetFilterControl(visibleDisplayColumnList.ColumnName, new EventHandler(this.Search));
						if (filterControl == null)
						{
							filterControl = new TextBox();
						}
						filterControl.ID = visibleDisplayColumnList.ColumnName;
						filterControl.CssClass = "NormalTextBox filterTextBox Gafware_Modules_Reservations_Input";
						tableCell = new TableCell()
						{
							CssClass = string.Concat(base.GetType().BaseType.Namespace.Replace(".", "_"), "_DataGrid_FilterStyle")
						};
						tableCell.Controls.Add(filterControl);
						dataGridItem.Cells.Add(tableCell);
						PropertyInfo property = this.ListSettings.InfoType.GetProperty(visibleDisplayColumnList.ColumnName);
						if (property == null || !property.PropertyType.Equals(typeof(DateTime)) || this.ListSettings.ExcludeFromDateRangeFilters(visibleDisplayColumnList.ColumnName))
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
						TextBox textBox = (TextBox)filterControl;
						textBox.CssClass = string.Concat(textBox.CssClass, " Gafware_Modules_Reservations_DateFilter_", visibleDisplayColumnList.ColumnName, "_TextBox Gafware_Modules_Reservations_DateFilter_TextBox");
						textBox.Attributes.Add("onfocus", string.Concat("showFilterCalendar(\"", visibleDisplayColumnList.ColumnName, "\");"));
						htmlTable.PreRender += new EventHandler(this.DateFilterCalendarPreRender);
					}
					this.dataGrid.Controls[0].Controls.AddAt((this.ListSettings.AllowPaging ? 1 : 0), dataGridItem);
				}
				if (e.Item.ItemType == ListItemType.Header)
				{
					string str = string.Concat(" ", base.GetType().BaseType.Namespace.Replace(".", "_"), "_DataGrid_ItemStyle_Sorted");
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
							if (this.dataGrid.Columns[i].ItemStyle.CssClass.IndexOf(str) != -1)
							{
								this.dataGrid.Columns[i].ItemStyle.CssClass = this.dataGrid.Columns[i].ItemStyle.CssClass.Replace(str, string.Empty);
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
							itemStyle.CssClass = string.Concat(itemStyle.CssClass, str);
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
			string str;
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
						if (num > 0)
						{
							string columnName = this.ListSettings.VisibleDisplayColumnList[num - 1].ColumnName;
							object obj = this.ListSettings.Eval(e.Item.DataItem, columnName);
							if (obj != null)
							{
								str = obj.ToString();
							}
							else
							{
								str = null;
							}
							string str1 = str;
							if (this.ListSettings.InfoType.GetProperty(columnName) == null)
							{
								control.Text = str1;
							}
							if (Null.IsNull(obj))
							{
								control.Text = Localization.GetString("None", base.LocalResourceFile);
							}
							string item = (string)this.FiltersOrderedDictionary[columnName];
							string text = control.Text;
							if (item != string.Empty && this.ListSettings.IsHighlightable(columnName))
							{
								text = this.Highlight(control.Text, item);
							}
							control.Text = string.Concat(new string[] { "<div title=\"", HtmlUtils.Clean(control.Text, true).Trim(), "\">", text, "</div>" });
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
			foreach (DisplayColumnInfo displayColumnList in this.ListSettings.DisplayColumnList)
			{
				if (!displayColumnList.Visible)
				{
					continue;
				}
				this.AddColumn(displayColumnList.LocalizedColumnName, displayColumnList.ColumnName, this.ListSettings.GetDisplayFormat(displayColumnList.ColumnName));
			}
		}

		protected void Page_Init(object sender, EventArgs e)
		{
			try
			{
				this.SetTheme();
				if (this.ListSettings.AllowPaging && this.ListSettings.PagerMode != Gafware.Modules.Reservations.PagerMode.DotNetNuke)
				{
					this.dataGrid.PageIndexChanged += new DataGridPageChangedEventHandler(this.PageChanged);
				}
				this.masterDiv.Attributes["class"] = string.Concat("Gafware_Modules_Reservations_", base.Request.QueryString["ctl"]);
				this.LoadColumns();
				Gafware.Modules.Reservations.Helper.DisableAJAX(this.exportLinkButton);
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
				if (!this.CanViewList)
				{
					base.Response.Redirect(Globals.NavigateURL(), true);
				}
				if (!base.IsPostBack)
				{
					this.dataGrid.AllowSorting = (!this.ListSettings.AllowSorting ? false : !this.IsPrintable);
					if (this.ListSettings.AllowPaging && !this.IsPrintable)
					{
						this.dataGrid.AllowPaging = true;
						this.dataGrid.PageSize = this.ListSettings.PageSize;
						if (this.ListSettings.PagerMode != Gafware.Modules.Reservations.PagerMode.DotNetNuke)
						{
							this.dataGrid.PagerStyle.Position = this.ListSettings.PagerPosition;
							if (this.ListSettings.PagerMode != Gafware.Modules.Reservations.PagerMode.NextPrev)
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
				this.printCommandButton.OnClientClick = string.Concat("window.open('", Globals.NavigateURL(string.Empty, new string[] { this.QueryStringParams, "Printable=True", string.Concat("SkinSrc=", base.Request.ApplicationPath, "/Portals/_default/Skins/_default/No%20Skin"), string.Concat("ContainerSrc=", base.Request.ApplicationPath, "/Portals/_default/Containers/_default/No%20Container") }), "'); return false;");
				this.buttonsTable.Visible = !this.IsPrintable;
				if (this.ListSettings.AllowPaging && this.ListSettings.PagerMode == Gafware.Modules.Reservations.PagerMode.DotNetNuke)
				{
					PagingControl pagingControl = this.topPagingControl;
					PagingControl pagingControl1 = this.bottomPagingControl;
					int pageSize = this.dataGrid.PageSize;
					int num = pageSize;
					pagingControl1.PageSize = pageSize;
					pagingControl.PageSize = num;
					PagingControl pagingControl2 = this.topPagingControl;
					int currentPageIndex = this.dataGrid.CurrentPageIndex + 1;
					num = currentPageIndex;
					this.bottomPagingControl.CurrentPage = currentPageIndex;
					pagingControl2.CurrentPage = num;
					PagingControl pagingControl3 = this.topPagingControl;
					PagingControl pagingControl4 = this.bottomPagingControl;
					int count = this.List.Count;
					num = count;
					pagingControl4.TotalRecords = count;
					pagingControl3.TotalRecords = num;
					PagingControl pagingControl5 = this.topPagingControl;
					PagingControl pagingControl6 = this.bottomPagingControl;
					int tabId = base.TabId;
					num = tabId;
					pagingControl6.TabID = tabId;
					pagingControl5.TabID = num;
					PagingControl pagingControl7 = this.topPagingControl;
					PagingControl pagingControl8 = this.bottomPagingControl;
					string queryStringParams = this.QueryStringParams;
					string str = queryStringParams;
					pagingControl8.QuerystringParams = queryStringParams;
					pagingControl7.QuerystringParams = str;
					this.topPagingControlDiv.Visible = (this.ListSettings.PagerPosition == PagerPosition.Top ? true : this.ListSettings.PagerPosition == PagerPosition.TopAndBottom);
					this.bottomPagingControlDiv.Visible = (this.ListSettings.PagerPosition == PagerPosition.Bottom ? true : this.ListSettings.PagerPosition == PagerPosition.TopAndBottom);
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
				this.ViewState["SortColumnList"] = this.ListSettings.SerializeSortColumnList(this._SortColumnList);
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
			foreach (DisplayColumnInfo displayColumnList in this.ListSettings.DisplayColumnList)
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
			Control control = this.dataGrid.Controls[0].Controls[(this.ListSettings.AllowPaging ? 1 : 0)].FindControl(columnName);
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
	}
}