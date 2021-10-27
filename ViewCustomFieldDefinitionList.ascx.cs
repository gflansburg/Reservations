using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Modules;
using DotNetNuke.UI.Skins;
using DotNetNuke.UI.Skins.Controls;
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
    public partial class ViewCustomFieldDefinitionList : PortalModuleBase
    {
        protected Gafware.Modules.Reservations.ModuleSettings _ModuleSettings;

        protected CustomFieldDefinitionListSettings _ListSettings;

        protected List<CustomFieldDefinitionInfo> _List;

		protected string AddLinkButtonUrl
		{
			get
			{
				ModuleInstanceContext moduleInstanceContext = new ModuleInstanceContext();
				int tabId = base.TabId;
				string[] strArrays = new string[2];
				int moduleId = base.ModuleId;
				strArrays[0] = string.Concat("mid=", moduleId.ToString());
				HttpServerUtility server = base.Server;
				string[] strArrays1 = new string[1];
				moduleId = base.ModuleId;
				strArrays1[0] = string.Concat("mid=", moduleId.ToString());
				strArrays[1] = string.Concat("ReturnUrl=", server.UrlEncode(Globals.NavigateURL("ViewCustomFieldDefinitionList", strArrays1)));
				return moduleInstanceContext.NavigateUrl(tabId, "EditCustomFieldDefinition", false, strArrays).Replace("550", "400").Replace("950", "650");
			}
		}

		protected bool HasEditPermissions
		{
			get
			{
				if (Null.IsNull(base.UserId))
				{
					return false;
				}
				return ModulePermissionController.HasModuleAccess(DotNetNuke.Security.SecurityAccessLevel.Edit, "EDIT", base.ModuleConfiguration);
			}
		}

		private List<CustomFieldDefinitionInfo> List
		{
			get
			{
				if (this._List == null)
				{
					this._List = (new CustomFieldDefinitionController()).GetActiveCustomFieldDefinitionList(base.TabModuleId);
					this.SortList();
				}
				return this._List;
			}
		}

		protected CustomFieldDefinitionListSettings ListSettings
		{
			get
			{
				if (this._ListSettings == null)
				{
					this._ListSettings = new CustomFieldDefinitionListSettings(this);
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

		protected string ReturnUrl
		{
			get
			{
				if (string.IsNullOrEmpty(base.Request.QueryString["ReturnUrl"]))
				{
					return Globals.NavigateURL();
				}
				return base.Request.QueryString["ReturnUrl"];
			}
		}

		public ViewCustomFieldDefinitionList()
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
			this.dataGrid.Columns.AddAt(this.dataGrid.Columns.Count - 1, boundColumn);
		}

		private void BindData()
		{
			this.SortList();
			this.dataGrid.DataSource = this.List;
			this.dataGrid.DataBind();
			Label label = this.numberOfRecordsFoundLabel;
			string str = Localization.GetString("Total", base.LocalResourceFile);
			int count = this.List.Count;
			label.Text = string.Format(str, count.ToString());
			if (this.dataGrid.Items.Count > 0)
			{
				((ImageButton)this.dataGrid.Items[this.dataGrid.Items.Count - 1].FindControl("downImageButton")).Visible = false;
				((ImageButton)this.dataGrid.Items[0].FindControl("upImageButton")).Visible = false;
				((ImageButton)this.dataGrid.Items[0].FindControl("downImageButton")).Style.Add("margin-left", "19px");
			}
		}

		public void ItemCommand(object source, DataGridCommandEventArgs e)
		{
			try
			{
				OrderedDictionary orderedDictionaries = new OrderedDictionary();
				string commandName = e.CommandName;
				if (commandName == "DisplayOrderDown")
				{
					this.Move(e.Item.DataSetIndex, SortColumnInfo.SortDirection.Descending);
				}
				else if (commandName == "DisplayOrderUp")
				{
					this.Move(e.Item.DataSetIndex, SortColumnInfo.SortDirection.Ascending);
				}
				else if (commandName == "Delete")
				{
					CustomFieldDefinitionController customFieldDefinitionController = new CustomFieldDefinitionController();
					customFieldDefinitionController.DeleteCustomFieldDefinition(this.List[e.Item.DataSetIndex].CustomFieldDefinitionID);
					this.List.RemoveAt(e.Item.DataSetIndex);
					for (int i = e.Item.DataSetIndex; i < this.List.Count; i++)
					{
						CustomFieldDefinitionInfo item = this.List[i];
						item.SortOrder = item.SortOrder - 1;
						customFieldDefinitionController.UpdateCustomFieldDefinition(this.List[i]);
					}
					this.BindData();
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
					string str = string.Concat(" ", base.GetType().BaseType.Namespace.Replace(".", "_"), "_DataGrid_ItemStyle_Sorted");
					for (int i = 1; i < this.dataGrid.Columns.Count - 1; i++)
					{
						if (this.ListSettings.SortColumnList.Count <= 0 || !(this.ListSettings.SortColumnList[0].ColumnName == this.dataGrid.Columns[i].SortExpression))
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
							TableCell tableCell = e.Item.Cells[i];
							HtmlImage htmlImage = new HtmlImage()
							{
								Src = (this.ListSettings.SortColumnList[0].Direction == SortColumnInfo.SortDirection.Ascending ? "~/images/sortascending.gif" : "~/images/sortdescending.gif"),
								Alt = Localization.GetString((this.ListSettings.SortColumnList[0].Direction == SortColumnInfo.SortDirection.Ascending ? "Ascending" : "Descending"), base.LocalResourceFile),
								Border = 0
							};
							if (tableCell.Controls.Count != 0)
							{
								LinkButton linkButton = (LinkButton)tableCell.Controls[0];
								tableCell.Controls.RemoveAt(0);
								linkButton.ToolTip = linkButton.Text;
								linkButton.Controls.Add(new LiteralControl(linkButton.Text));
								linkButton.Controls.Add(htmlImage);
								Panel panel1 = new Panel();
								panel1.Controls.Add(linkButton);
								tableCell.Controls.Add(panel1);
							}
							else
							{
								tableCell.Controls.Add(new Panel());
								tableCell.Controls[0].Controls.Add(new Label());
								((Label)tableCell.Controls[0].Controls[0]).ToolTip = tableCell.Text;
								tableCell.Controls[0].Controls[0].Controls.Add(new LiteralControl(tableCell.Text));
								tableCell.Controls[0].Controls[0].Controls.Add(htmlImage);
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
						if (num > 0 && num < e.Item.Controls.Count - 1)
						{
							if (typeof(CustomFieldDefinitionInfo).GetProperty(this.ListSettings.VisibleDisplayColumnList[num - 1].ColumnName).PropertyType.Equals(typeof(bool)))
							{
								control.Text = Localization.GetString(DataBinder.Eval(e.Item.DataItem, this.ListSettings.VisibleDisplayColumnList[num - 1].ColumnName).ToString(), base.LocalResourceFile);
							}
							else if (Null.IsNull(DataBinder.Eval(e.Item.DataItem, this.ListSettings.VisibleDisplayColumnList[num - 1].ColumnName)))
							{
								control.Text = Localization.GetString("None", base.LocalResourceFile);
							}
							LiteralControl literalControl = new LiteralControl(string.Concat(new string[] { "<div title=\"", control.Text, "\">", control.Text, "</div>" }));
							control.Controls.Add(literalControl);
						}
						num++;
					}
					CustomFieldDefinitionInfo dataItem = (CustomFieldDefinitionInfo)e.Item.DataItem;
					if (e.Item.ItemType == ListItemType.Item)
					{
						cssClass = this.dataGrid.ItemStyle.CssClass;
					}
					else
					{
						cssClass = (e.Item.ItemType == ListItemType.AlternatingItem ? this.dataGrid.AlternatingItemStyle.CssClass : this.dataGrid.SelectedItemStyle.CssClass);
					}
					e.Item.CssClass = cssClass;
					HtmlAnchor htmlAnchor = (HtmlAnchor)e.Item.FindControl("viewCommandButton");
					ModuleInstanceContext moduleInstanceContext = new ModuleInstanceContext();
					int tabId = base.TabId;
					string[] strArrays = new string[3];
					int moduleId = base.ModuleId;
					strArrays[0] = string.Concat("mid=", moduleId.ToString());
					moduleId = dataItem.CustomFieldDefinitionID;
					strArrays[1] = string.Concat("CustomFieldDefinitionID=", moduleId.ToString());
					HttpServerUtility server = base.Server;
					string[] strArrays1 = new string[1];
					moduleId = base.ModuleId;
					strArrays1[0] = string.Concat("mid=", moduleId.ToString());
					strArrays[2] = string.Concat("ReturnUrl=", server.UrlEncode(Globals.NavigateURL("ViewCustomFieldDefinitionList", strArrays1)));
					htmlAnchor.HRef = moduleInstanceContext.NavigateUrl(tabId, "EditCustomFieldDefinition", false, strArrays).Replace("550", "400").Replace("950", "650");
				}
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
			}
		}

		private void LoadColumns()
		{
			foreach (DisplayColumnInfo visibleDisplayColumnList in this.ListSettings.VisibleDisplayColumnList)
			{
				this.AddColumn(Localization.GetString(visibleDisplayColumnList.ColumnName, base.LocalResourceFile), visibleDisplayColumnList.ColumnName, (typeof(CustomFieldDefinitionInfo).GetProperty(visibleDisplayColumnList.ColumnName).PropertyType.Equals(typeof(DateTime)) ? "{0:d}" : string.Empty));
			}
		}

		private void Move(int index, SortColumnInfo.SortDirection direction)
		{
			if (index >= 0)
			{
				CustomFieldDefinitionInfo item = null;
				CustomFieldDefinitionInfo userId = null;
				if (direction != SortColumnInfo.SortDirection.Ascending)
				{
					if (direction == SortColumnInfo.SortDirection.Descending)
					{
						if (index < this.List.Count)
						{
							item = this.List[index];
							userId = this.List[index + 1];
						}
					}
				}
				else if (index > 0)
				{
					item = this.List[index];
					userId = this.List[index - 1];
				}
				int sortOrder = item.SortOrder;
				item.SortOrder = userId.SortOrder;
				userId.SortOrder = sortOrder;
				item.LastModifiedByUserID = base.UserId;
				item.LastModifiedOnDate = DateTime.Now;
				userId.LastModifiedByUserID = base.UserId;
				userId.LastModifiedOnDate = DateTime.Now;
				CustomFieldDefinitionController customFieldDefinitionController = new CustomFieldDefinitionController();
				customFieldDefinitionController.UpdateCustomFieldDefinition(item);
				customFieldDefinitionController.UpdateCustomFieldDefinition(userId);
			}
			this.BindData();
		}

		protected void Page_Init(object sender, EventArgs e)
		{
			try
			{
				this.SetTheme();
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
				if (!ModulePermissionController.HasModuleAccess(DotNetNuke.Security.SecurityAccessLevel.Edit, "EDIT", base.ModuleConfiguration))
				{
					base.Response.Redirect(Globals.NavigateURL(), true);
				}
				if (!base.IsPostBack)
				{
					if (!string.IsNullOrEmpty(base.Request.QueryString["ModuleMessage"]) && !string.IsNullOrEmpty(base.Request.QueryString["ModuleMessageType"]))
					{
						Skin.AddModuleMessage(this, Localization.GetString(base.Request.QueryString["ModuleMessage"], base.LocalResourceFile), (ModuleMessage.ModuleMessageType)Enum.Parse(typeof(ModuleMessage.ModuleMessageType), base.Request.QueryString["ModuleMessageType"], true));
					}
					this.BindData();
				}
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
			}
		}

		protected void Page_PreRender(object sender, EventArgs e)
		{
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

		public void SortList()
		{
			if (this.ListSettings.SortColumnList.Count > 0)
			{
				this.List.Sort(new ColumnListComparer<CustomFieldDefinitionInfo>(this.ListSettings.SortColumnList));
			}
		}
	}
}