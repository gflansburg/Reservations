using DotNetNuke.Abstractions;
using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.UserControls;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Gafware.Modules.Reservations
{
    public partial class ViewListSettings : PortalModuleBase
    {
        protected Gafware.Modules.Reservations.ModuleSettings _ModuleSettings;

        private Gafware.Modules.Reservations.ListSettings _ListSettings;

        private List<DisplayColumnInfo> _DisplayColumnList;

        private List<SortColumnInfo> _SortColumnList;

		private readonly INavigationManager _navigationManager;
		
		public ViewListSettings()
		{
			_navigationManager = DependencyProvider.GetRequiredService<INavigationManager>();
		}

		private List<DisplayColumnInfo> DisplayColumnList
		{
			get
			{
				if (this._DisplayColumnList == null)
				{
					if (this.ViewState["DisplayColumnList"] != null)
					{
						this._DisplayColumnList = this.ListSettings.DeserializeDisplayColumnList((string)this.ViewState["DisplayColumnList"]);
					}
					else
					{
						this._DisplayColumnList = this.ListSettings.DisplayColumnList;
					}
					int num = 0;
					if (this.displayColumnDataGrid.Items.Count != 0)
					{
						foreach (DisplayColumnInfo displayColumnList in this.DisplayColumnList)
						{
							displayColumnList.Visible = ((CheckBox)this.displayColumnDataGrid.Items[num].FindControl("visibleCheckBox")).Checked;
							num++;
						}
					}
				}
				return this._DisplayColumnList;
			}
			set
			{
				this.ViewState.Remove("DisplayColumnList");
				if (value == null)
				{
					this._DisplayColumnList = null;
					return;
				}
				this._DisplayColumnList = value;
				this.ViewState["DisplayColumnList"] = this._ListSettings.SerializeDisplayColumnList(this._DisplayColumnList);
			}
		}

		protected bool HasEditPermissions
		{
			get
			{
				return (new ModuleSecurity(base.ModuleConfiguration)).HasEditPermissions;
			}
		}

		private Gafware.Modules.Reservations.ListSettings ListSettings
		{
			get
			{
				if (this._ListSettings == null)
				{
					try
					{
						Type type = base.GetType().BaseType.Assembly.GetType(string.Concat(base.GetType().BaseType.Namespace, ".", base.Request.QueryString["List"]));
						this._ListSettings = (Gafware.Modules.Reservations.ListSettings)Activator.CreateInstance(type, new object[] { this });
					}
					catch (Exception)
					{
						base.Response.Redirect(_navigationManager.NavigateURL());
					}
				}
				return this._ListSettings;
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

		private List<SortColumnInfo> SortColumnList
		{
			get
			{
				if (this._SortColumnList == null)
				{
					if (this.ViewState["SortColumnList"] != null)
					{
						this._SortColumnList = this.ListSettings.DeserializeSortColumnList((string)this.ViewState["SortColumnList"]);
					}
					else
					{
						this._SortColumnList = this.ListSettings.SortColumnList;
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
				this.ViewState["SortColumnList"] = this._ListSettings.SerializeSortColumnList(this._SortColumnList);
			}
		}

		protected void AddSortColumnCommandButtonClicked(object source, EventArgs e)
		{
			try
			{
				if (this.sortColumnDropDownList.SelectedValue != string.Empty)
				{
					SortColumnInfo sortColumnInfo = new SortColumnInfo()
					{
						ColumnName = this.sortColumnDropDownList.SelectedValue,
						Direction = (SortColumnInfo.SortDirection)Enum.Parse(typeof(SortColumnInfo.SortDirection), this.sortOrderDirectionDropDownList.SelectedValue)
					};
					sortColumnInfo.LocalizedColumnName = this.DisplayColumnList[this.IndexOfByColumnName(this.DisplayColumnList, sortColumnInfo.ColumnName)].LocalizedColumnName;
					SortColumnInfo.SortDirection direction = sortColumnInfo.Direction;
					sortColumnInfo.LocalizedDirection = Localization.GetString(direction.ToString(), base.LocalResourceFile);
					this.SortColumnList.Add(sortColumnInfo);
					this.SortColumnList = this.SortColumnList;
					this.BindSortColumnDropDownLists();
					this.BindSortColumnDataGrid();
				}
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
			}
		}

		protected void AllowPagingCheckedChanged(object source, EventArgs e)
		{
			try
			{
				this.ShowHidePagingTableRows(this.allowPagingCheckBox.Checked);
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
			}
		}

		private void BindDisplayColumnDataGrid()
		{
			this.displayColumnDataGrid.DataSource = this.DisplayColumnList;
			this.displayColumnDataGrid.DataBind();
			((ImageButton)this.displayColumnDataGrid.Items[this.displayColumnDataGrid.Items.Count - 1].FindControl("downImageButton")).Visible = false;
			((ImageButton)this.displayColumnDataGrid.Items[0].FindControl("upImageButton")).Visible = false;
			((ImageButton)this.displayColumnDataGrid.Items[0].FindControl("downImageButton")).Style.Add("margin-left", "19px");
		}

		private void BindPagerModeDropDownList()
		{
			this.pagerModeDropDownList.DataSource = this.ListSettings.LocalizeEnum(typeof(Gafware.Modules.Reservations.PagerMode));
			this.pagerModeDropDownList.DataTextField = "LocalizedName";
			this.pagerModeDropDownList.DataValueField = "Name";
			this.pagerModeDropDownList.DataBind();
		}

		private void BindPagerPositionDropDownList()
		{
			this.pagerPositionDropDownList.DataSource = this.ListSettings.LocalizeEnum(typeof(PagerPosition));
			this.pagerPositionDropDownList.DataTextField = "LocalizedName";
			this.pagerPositionDropDownList.DataValueField = "Name";
			this.pagerPositionDropDownList.DataBind();
		}

		private void BindSortColumnDataGrid()
		{
			this.sortColumnDataGrid.DataSource = this.SortColumnList;
			this.sortColumnDataGrid.DataBind();
			this.noSortColumnsLabel.Visible = this.sortColumnDataGrid.Items.Count == 0;
		}

		private void BindSortColumnDropDownLists()
		{
			this.sortColumnDropDownList.Items.Clear();
			foreach (DisplayColumnInfo displayColumnList in this.DisplayColumnList)
			{
				if (this.IndexOfByColumnName(this.SortColumnList, displayColumnList.ColumnName) != -1)
				{
					continue;
				}
				this.sortColumnDropDownList.Items.Add(new ListItem(displayColumnList.LocalizedColumnName, displayColumnList.ColumnName));
			}
			this.sortOrderDirectionDropDownList.DataSource = this.ListSettings.LocalizeEnum(typeof(SortColumnInfo.SortDirection));
			this.sortOrderDirectionDropDownList.DataTextField = "LocalizedName";
			this.sortOrderDirectionDropDownList.DataValueField = "Name";
			this.sortOrderDirectionDropDownList.DataBind();
		}

		protected void Cancel(object sender, EventArgs e)
		{
			try
			{
				base.Response.Redirect((base.Request.QueryString["ReturnUrl"] != null ? base.Request.QueryString["ReturnUrl"] : _navigationManager.NavigateURL()), true);
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
			}
		}

		protected void DisplayColumnDataGridItemCommand(object source, DataGridCommandEventArgs e)
		{
			try
			{
				string commandName = e.CommandName;
				if (commandName == "DisplayOrderDown")
				{
					this.SwapColumn(e.CommandArgument.ToString(), SortColumnInfo.SortDirection.Descending);
				}
				else if (commandName == "DisplayOrderUp")
				{
					this.SwapColumn(e.CommandArgument.ToString(), SortColumnInfo.SortDirection.Ascending);
				}
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
			}
		}

		private int IndexOfByColumnName(List<DisplayColumnInfo> displayColumnList, string columnName)
		{
			int num = 0;
			List<DisplayColumnInfo>.Enumerator enumerator = displayColumnList.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.ColumnName != columnName)
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
				((IDisposable)enumerator).Dispose();
			}
			return -1;
		}

		private int IndexOfByColumnName(List<SortColumnInfo> sortColumnList, string columnName)
		{
			int num = 0;
			List<SortColumnInfo>.Enumerator enumerator = sortColumnList.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.ColumnName != columnName)
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
				((IDisposable)enumerator).Dispose();
			}
			return -1;
		}

		public void LoadSettings()
		{
			try
			{
				Localization.LocalizeDataGrid(ref this.displayColumnDataGrid, base.LocalResourceFile);
				Localization.LocalizeDataGrid(ref this.sortColumnDataGrid, base.LocalResourceFile);
				this.BindDisplayColumnDataGrid();
				this.BindSortColumnDropDownLists();
				this.BindSortColumnDataGrid();
				this.allowUsersToSortCheckBox.Checked = this.ListSettings.AllowSorting;
				CheckBox checkBox = this.allowPagingCheckBox;
				bool allowPaging = this.ListSettings.AllowPaging;
				bool flag = allowPaging;
				checkBox.Checked = allowPaging;
				this.ShowHidePagingTableRows(flag);
				this.pageSizeTextBox.Text = this.ListSettings.PageSize.ToString();
				this.BindPagerModeDropDownList();
				this.pagerModeDropDownList.SelectedValue = this.ListSettings.PagerMode.ToString();
				this.BindPagerPositionDropDownList();
				this.pagerPositionDropDownList.SelectedValue = this.ListSettings.PagerPosition.ToString();
				this.restoreDefaultTableCell.Visible = (new ModuleController()).GetTabModule(base.TabModuleId).TabModuleSettings.ContainsKey(this.ListSettings.DISPLAYCOLUMNLIST_KEY);
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
			}
		}

		protected void Page_Init(object sender, EventArgs e)
		{
			try
			{
				this.SetTheme();
				this.restoreDefaultCommandButton.DataBind();
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
				if (!this.HasEditPermissions)
				{
					base.Response.Redirect(_navigationManager.NavigateURL());
				}
				if (!this.Page.IsPostBack && !string.IsNullOrEmpty(base.Request.QueryString["List"]))
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

		protected void RestoreDefaultSettings(object sender, EventArgs e)
		{
			string str;
			try
			{
				ModuleController moduleController = new ModuleController();
				moduleController.DeleteTabModuleSetting(base.TabModuleId, this.ListSettings.DISPLAYCOLUMNLIST_KEY);
				moduleController.DeleteTabModuleSetting(base.TabModuleId, this.ListSettings.SORTORDER_KEY);
				moduleController.DeleteTabModuleSetting(base.TabModuleId, this.ListSettings.ALLOWSORTING_KEY);
				moduleController.DeleteTabModuleSetting(base.TabModuleId, this.ListSettings.ALLOWPAGING_KEY);
				moduleController.DeleteTabModuleSetting(base.TabModuleId, this.ListSettings.PAGESIZE_KEY);
				moduleController.DeleteTabModuleSetting(base.TabModuleId, this.ListSettings.PAGERMODE_KEY);
				moduleController.DeleteTabModuleSetting(base.TabModuleId, this.ListSettings.PAGERPOSITION_KEY);
				ModuleController.SynchronizeModule(base.ModuleId);
				HttpResponse response = base.Response;
				if (base.Request.QueryString["Control"] != null)
				{
					str = _navigationManager.NavigateURL(base.Request.QueryString["Control"], new string[] { string.Concat("mid=", base.ModuleId), string.Concat("List=", base.Request.QueryString["List"]) });
				}
				else
				{
					str = (base.Request.QueryString["ReturnUrl"] != null ? base.Request.QueryString["ReturnUrl"] : _navigationManager.NavigateURL());
				}
				response.Redirect(str, true);
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
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

		private void ShowHidePagingTableRows(bool visible)
		{
			HtmlTableRow htmlTableRow = this.pagerPositionTableRow;
			HtmlTableRow htmlTableRow1 = this.pagerModeTableRow;
			bool flag = visible;
			bool flag1 = flag;
			this.pageSizeTableRow.Visible = flag;
			bool flag2 = flag1;
			bool flag3 = flag2;
			htmlTableRow1.Visible = flag2;
			htmlTableRow.Visible = flag3;
		}

		protected void SortColumnDataGridDeleteCommand(object source, DataGridCommandEventArgs e)
		{
			try
			{
				this.SortColumnList.RemoveAt(this.IndexOfByColumnName(this.SortColumnList, e.CommandArgument.ToString()));
				this.SortColumnList = this.SortColumnList;
				this.BindSortColumnDropDownLists();
				this.BindSortColumnDataGrid();
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
			}
		}

		private void SwapColumn(string columnName, SortColumnInfo.SortDirection direction)
		{
			int displayOrder;
			int num = this.IndexOfByColumnName(this.DisplayColumnList, columnName);
			if (num >= 0)
			{
				if (direction != SortColumnInfo.SortDirection.Ascending)
				{
					if (direction == SortColumnInfo.SortDirection.Descending)
					{
						if (num < this.DisplayColumnList.Count)
						{
							displayOrder = this.DisplayColumnList[num].DisplayOrder;
							this.DisplayColumnList[num].DisplayOrder = this.DisplayColumnList[num + 1].DisplayOrder;
							this.DisplayColumnList[num + 1].DisplayOrder = displayOrder;
						}
					}
				}
				else if (num > 0)
				{
					displayOrder = this.DisplayColumnList[num].DisplayOrder;
					this.DisplayColumnList[num].DisplayOrder = this.DisplayColumnList[num - 1].DisplayOrder;
					this.DisplayColumnList[num - 1].DisplayOrder = displayOrder;
				}
				this.DisplayColumnList.Sort();
				this.DisplayColumnList = this.DisplayColumnList;
			}
			this.BindDisplayColumnDataGrid();
		}

		protected void Update(object sender, EventArgs e)
		{
			string str;
			try
			{
				if (this.Page.IsValid)
				{
					this.UpdateSettings();
					HttpResponse response = base.Response;
					if (base.Request.QueryString["Control"] != null)
					{
						str = _navigationManager.NavigateURL(base.Request.QueryString["Control"], new string[] { string.Concat("mid=", base.ModuleId), string.Concat("List=", base.Request.QueryString["List"]) });
					}
					else
					{
						str = (base.Request.QueryString["ReturnUrl"] != null ? base.Request.QueryString["ReturnUrl"] : _navigationManager.NavigateURL());
					}
					response.Redirect(str, true);
				}
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
			}
		}

		public void UpdateSettings()
		{
			try
			{
				ModuleController moduleController = new ModuleController();
				moduleController.UpdateTabModuleSetting(base.TabModuleId, this.ListSettings.DISPLAYCOLUMNLIST_KEY, this.ListSettings.SerializeDisplayColumnList(this.DisplayColumnList));
				moduleController.UpdateTabModuleSetting(base.TabModuleId, this.ListSettings.SORTORDER_KEY, this.ListSettings.SerializeSortColumnList(this.SortColumnList));
				int tabModuleId = base.TabModuleId;
				string aLLOWSORTINGKEY = this.ListSettings.ALLOWSORTING_KEY;
				bool @checked = this.allowUsersToSortCheckBox.Checked;
				moduleController.UpdateTabModuleSetting(tabModuleId, aLLOWSORTINGKEY, @checked.ToString());
				int num = base.TabModuleId;
				string aLLOWPAGINGKEY = this.ListSettings.ALLOWPAGING_KEY;
				@checked = this.allowPagingCheckBox.Checked;
				moduleController.UpdateTabModuleSetting(num, aLLOWPAGINGKEY, @checked.ToString());
				moduleController.UpdateTabModuleSetting(base.TabModuleId, this.ListSettings.PAGESIZE_KEY, this.pageSizeTextBox.Text);
				int tabModuleId1 = base.TabModuleId;
				string pAGERMODEKEY = this.ListSettings.PAGERMODE_KEY;
				int num1 = (int)Enum.Parse(typeof(Gafware.Modules.Reservations.PagerMode), this.pagerModeDropDownList.SelectedValue);
				moduleController.UpdateTabModuleSetting(tabModuleId1, pAGERMODEKEY, num1.ToString());
				int tabModuleId2 = base.TabModuleId;
				string pAGERPOSITIONKEY = this.ListSettings.PAGERPOSITION_KEY;
				num1 = (int)Enum.Parse(typeof(PagerPosition), this.pagerPositionDropDownList.SelectedValue);
				moduleController.UpdateTabModuleSetting(tabModuleId2, pAGERPOSITIONKEY, num1.ToString());
				ModuleController.SynchronizeModule(base.ModuleId);
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
			}
		}
	}
}