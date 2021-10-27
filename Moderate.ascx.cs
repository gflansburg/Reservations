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
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Gafware.Modules.Reservations
{
    public partial class Moderate : PortalModuleBase
    {
		protected Gafware.Modules.Reservations.Helper _Helper;

		protected Gafware.Modules.Reservations.ModuleSettings _ModuleSettings;

		protected Gafware.Modules.Reservations.ModerationListSettings _ModerationListSettings;

		protected Gafware.Modules.Reservations.ModuleSecurity _ModuleSecurity;

		protected List<SortColumnInfo> _SortColumnList;

		protected List<PendingApprovalInfo> _PendingApprovalInfoList;

		protected OrderedDictionary _FiltersOrderedDictionary;

		protected ReservationController _ProxyController;

		protected List<CategoryInfo> _CategoryList;

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

		private OrderedDictionary FiltersOrderedDictionary
		{
			get
			{
				if (this._FiltersOrderedDictionary == null)
				{
					this._FiltersOrderedDictionary = new OrderedDictionary();
					foreach (DisplayColumnInfo displayColumnList in this.ModerationListSettings.DisplayColumnList)
					{
						if (!displayColumnList.Visible)
						{
							continue;
						}
						string empty = string.Empty;
						empty = (!base.IsPostBack ? base.Request.QueryString[displayColumnList.ColumnName] : this.GetFilterText(displayColumnList.ColumnName));
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

		private Gafware.Modules.Reservations.ModerationListSettings ModerationListSettings
		{
			get
			{
				if (this._ModerationListSettings == null)
				{
					this._ModerationListSettings = new Gafware.Modules.Reservations.ModerationListSettings(this);
				}
				return this._ModerationListSettings;
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

		private List<PendingApprovalInfo> PendingApprovalInfoList
		{
			get
			{
				if (this._PendingApprovalInfoList == null)
				{
					this._PendingApprovalInfoList = new List<PendingApprovalInfo>();
					foreach (PendingApprovalInfo list in (
						from pendingApprovalInfo in (new PendingApprovalController()).GetPendingApprovalList(base.TabModuleId)
						where pendingApprovalInfo.Status == 0
						select pendingApprovalInfo).ToList<PendingApprovalInfo>())
					{
						if (!this.Helper.CanModerate(base.UserId, list.CategoryID))
						{
							continue;
						}
						this._PendingApprovalInfoList.Add(list);
					}
					foreach (DisplayColumnInfo displayColumnList in this.ModerationListSettings.DisplayColumnList)
					{
						if (!displayColumnList.Visible)
						{
							continue;
						}
						this._PendingApprovalInfoList = this.Filter(this._PendingApprovalInfoList, displayColumnList.ColumnName, (string)this.FiltersOrderedDictionary[displayColumnList.ColumnName]);
					}
				}
				return this._PendingApprovalInfoList;
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
				string str = string.Concat("&ctl=Moderate&mid=", moduleId.ToString());
				foreach (DisplayColumnInfo displayColumnList in this.ModerationListSettings.DisplayColumnList)
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
					str = string.Concat(new string[] { str, "&", displayColumnList.ColumnName, "=", item });
				}
				if (this.SortColumnList.Count > 0)
				{
					str = string.Concat(str, "&SortColumnList=", this.ModerationListSettings.SerializeSortColumnList(this.SortColumnList));
				}
				if (str != string.Empty)
				{
					str = str.Substring(1);
				}
				return str;
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
							this._SortColumnList = this.ModerationListSettings.DeserializeSortColumnList((string)this.ViewState["SortColumnList"]);
						}
						else
						{
							this.SortColumnList = this.ModerationListSettings.SortColumnList;
						}
					}
					else if (base.Request.QueryString["SortColumnList"] != null)
					{
						this.SortColumnList = this.ModerationListSettings.DeserializeSortColumnList(base.Request.QueryString["SortColumnList"]);
					}
					else
					{
						this.SortColumnList = this.ModerationListSettings.SortColumnList;
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
				this.ViewState["SortColumnList"] = this.ModerationListSettings.SerializeSortColumnList(this._SortColumnList);
			}
		}

		public Moderate()
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
				this.PendingApprovalInfoList.Sort(new ColumnListComparer<PendingApprovalInfo>(this.SortColumnList));
			}
			int count = this.PendingApprovalInfoList.Count / this.dataGrid.PageSize;
			int num = this.PendingApprovalInfoList.Count % this.dataGrid.PageSize;
			if (this.ModerationListSettings.AllowPaging)
			{
				int count1 = this.PendingApprovalInfoList.Count / this.ModerationListSettings.PageSize + (this.PendingApprovalInfoList.Count % this.ModerationListSettings.PageSize == 0 ? 0 : 1) - 1;
				if (this.PendingApprovalInfoList.Count == 0)
				{
					this.dataGrid.CurrentPageIndex = 0;
				}
				else if (this.dataGrid.CurrentPageIndex > count1)
				{
					this.dataGrid.CurrentPageIndex = count1;
				}
			}
			this.dataGrid.DataSource = this.PendingApprovalInfoList;
			this.dataGrid.DataBind();
			Label label = this.numberOfRecordsFoundLabel;
			string str = Localization.GetString("Total", base.LocalResourceFile);
			int num1 = this.PendingApprovalInfoList.Count;
			label.Text = string.Format(str, num1.ToString());
		}

		protected void CancelCommandButtonClicked(object sender, EventArgs e)
		{
			base.Response.Redirect(Globals.NavigateURL(), true);
		}

		public List<PendingApprovalInfo> Filter(List<PendingApprovalInfo> list, string columnName, string text)
		{
			string str;
			if (text == string.Empty)
			{
				return list;
			}
			List<PendingApprovalInfo> pendingApprovalInfos = new List<PendingApprovalInfo>();
			PropertyInfo property = typeof(PendingApprovalInfo).GetProperty(columnName);
			foreach (PendingApprovalInfo pendingApprovalInfo in list)
			{
				object value = property.GetValue(pendingApprovalInfo, null);
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
				pendingApprovalInfos.Add(pendingApprovalInfo);
			}
			return pendingApprovalInfos;
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

		public string GetFilterText(string columnName)
		{
			Control control = this.dataGrid.Controls[0].Controls[(this.ModerationListSettings.AllowPaging ? 1 : 0)].FindControl(columnName);
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
					strArrays[1] = "List=ModerationListSettings";
					strArrays[2] = "Control=Moderate";
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
				else if (e.CommandName == "Approve" || e.CommandName == "Decline")
				{
					int num = int.Parse(((Label)e.Item.FindControl("pendingApprovalID")).Text);
					foreach (PendingApprovalInfo pendingApprovalInfoList in this.PendingApprovalInfoList)
					{
						if (pendingApprovalInfoList.PendingApprovalID != num)
						{
							continue;
						}
						this.UpdateStatus(pendingApprovalInfoList, (e.CommandName == "Approve" ? PendingApprovalStatus.Approved : PendingApprovalStatus.Declined));
						this._PendingApprovalInfoList = null;
						this.BindData();
						break;
					}
				}
				else if (e.CommandName == "View")
				{
					int num1 = int.Parse(((Label)e.Item.FindControl("pendingApprovalID")).Text);
					HttpResponse httpResponse = base.Response;
					int tabId = base.TabId;
					string str1 = string.Empty;
					string[] strArrays1 = new string[] { string.Concat("PendingApprovalID=", num1.ToString()), null };
					HttpServerUtility httpServerUtility = base.Server;
					string empty2 = string.Empty;
					string[] queryStringParams1 = new string[] { this.QueryStringParams, null };
					if (this.dataGrid.CurrentPageIndex > 0)
					{
						moduleId = this.dataGrid.CurrentPageIndex + 1;
						empty = string.Concat("currentpage=", moduleId.ToString());
					}
					else
					{
						empty = string.Empty;
					}
					queryStringParams1[1] = empty;
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
			try
			{
				if (e.Item.ItemType == ListItemType.Header)
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
					foreach (DisplayColumnInfo displayColumnList in this.ModerationListSettings.DisplayColumnList)
					{
						if (!displayColumnList.Visible)
						{
							continue;
						}
						TextBox textBox = new TextBox()
						{
							ID = displayColumnList.ColumnName,
							CssClass = "NormalTextBox filterTextBox Gafware_Modules_Reservations_Input"
						};
						tableCell = new TableCell()
						{
							CssClass = string.Concat(base.GetType().BaseType.Namespace.Replace(".", "_"), "_DataGrid_FilterStyle")
						};
						tableCell.Controls.Add(textBox);
						dataGridItem.Cells.Add(tableCell);
					}
					this.dataGrid.Controls[0].Controls.AddAt((this.ModerationListSettings.AllowPaging ? 1 : 0), dataGridItem);
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
			string cssClass;
			try
			{
				if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem || e.Item.ItemType == ListItemType.SelectedItem)
				{
					int num = 0;
					foreach (TableCell control in e.Item.Controls)
					{
						string text = control.Text;
						if (num > 0)
						{
							if (Null.IsNull(DataBinder.Eval(e.Item.DataItem, this.ModerationListSettings.VisibleDisplayColumnList[num - 1].ColumnName)))
							{
								control.Text = Localization.GetString("None", base.LocalResourceFile);
							}
							string item = (string)this.FiltersOrderedDictionary[this.ModerationListSettings.VisibleDisplayColumnList[num - 1].ColumnName];
							if (item == string.Empty)
							{
								LiteralControl literalControl = new LiteralControl(string.Concat(new string[] { "<div title=\"", control.Text, "\">", control.Text, "</div>" }));
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
			foreach (DisplayColumnInfo displayColumnList in this.ModerationListSettings.DisplayColumnList)
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
					str = (typeof(PendingApprovalInfo).GetProperty(displayColumnList.ColumnName).PropertyType.Equals(typeof(DateTime)) ? "{0:d}" : string.Empty);
				}
				this.AddColumn(str1, columnName, str);
			}
		}

		protected void Page_Init(object sender, EventArgs e)
		{
			try
			{
				this.SetTheme();
				if (this.ModerationListSettings.AllowPaging && this.ModerationListSettings.PagerMode != Gafware.Modules.Reservations.PagerMode.DotNetNuke)
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
				if (!this.IsModerator)
				{
					base.Response.Redirect(Globals.NavigateURL(), true);
				}
				if (!base.IsPostBack)
				{
					this.dataGrid.AllowSorting = this.ModerationListSettings.AllowSorting;
					if (this.ModerationListSettings.AllowPaging)
					{
						this.dataGrid.AllowPaging = true;
						this.dataGrid.PageSize = this.ModerationListSettings.PageSize;
						if (this.ModerationListSettings.PagerMode != Gafware.Modules.Reservations.PagerMode.DotNetNuke)
						{
							this.dataGrid.PagerStyle.Position = this.ModerationListSettings.PagerPosition;
							if (this.ModerationListSettings.PagerMode != Gafware.Modules.Reservations.PagerMode.NextPrev)
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
			try
			{
				if (this.ModerationListSettings.AllowPaging && this.ModerationListSettings.PagerMode == Gafware.Modules.Reservations.PagerMode.DotNetNuke)
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
					int count = this.PendingApprovalInfoList.Count;
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
					this.topPagingControlDiv.Visible = (this.ModerationListSettings.PagerPosition == PagerPosition.Top ? true : this.ModerationListSettings.PagerPosition == PagerPosition.TopAndBottom);
					this.bottomPagingControlDiv.Visible = (this.ModerationListSettings.PagerPosition == PagerPosition.Bottom ? true : this.ModerationListSettings.PagerPosition == PagerPosition.TopAndBottom);
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
				this.ViewState["SortColumnList"] = this.ModerationListSettings.SerializeSortColumnList(this._SortColumnList);
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
			}
			return base.SaveViewState();
		}

		public void SetFiltersText()
		{
			foreach (DisplayColumnInfo displayColumnList in this.ModerationListSettings.DisplayColumnList)
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
			Control control = this.dataGrid.Controls[0].Controls[(this.ModerationListSettings.AllowPaging ? 1 : 0)].FindControl(columnName);
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

		private void UpdateStatus(PendingApprovalInfo pendingApprovalInfo, PendingApprovalStatus status)
		{
			this.Helper.ModifyPendingApprovalStatus(pendingApprovalInfo, status, base.UserInfo);
			if (status == PendingApprovalStatus.Approved)
			{
				Gafware.Modules.Reservations.Helper.AddModuleMessage(this, Localization.GetString("Approved", base.LocalResourceFile), 0);
				return;
			}
			Gafware.Modules.Reservations.Helper.AddModuleMessage(this, Localization.GetString("Declined", base.LocalResourceFile), 0);
		}
	}
}