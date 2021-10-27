using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins;
using DotNetNuke.UI.Skins.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Serialization;

namespace Gafware.Modules.Reservations
{
    public partial class EditCustomFieldDefinition : PortalModuleBase
	{
        protected Gafware.Modules.Reservations.ModuleSettings _ModuleSettings;

        protected Gafware.Modules.Reservations.CustomFieldDefinitionInfo _CustomFieldDefinitionInfo;

        protected CustomFieldDefinitionListItemListSettings _ListSettings;

        protected List<CustomFieldDefinitionListItemInfo> _List;

		protected Gafware.Modules.Reservations.CustomFieldDefinitionInfo CustomFieldDefinitionInfo
		{
			get
			{
				if (this._CustomFieldDefinitionInfo == null)
				{
					if (base.Request.QueryString["CustomFieldDefinitionID"] != null && this.ViewState["CustomFieldDefinitionID"] == null)
					{
						this.ViewState["CustomFieldDefinitionID"] = int.Parse(base.Request.QueryString["CustomFieldDefinitionID"]);
					}
					if (this.ViewState["CustomFieldDefinitionID"] == null)
					{
						this._CustomFieldDefinitionInfo = new Gafware.Modules.Reservations.CustomFieldDefinitionInfo();
					}
					else
					{
						this._CustomFieldDefinitionInfo = (new CustomFieldDefinitionController()).GetCustomFieldDefinition((int)this.ViewState["CustomFieldDefinitionID"]);
						if (this._CustomFieldDefinitionInfo == null || this._CustomFieldDefinitionInfo.TabModuleID != base.TabModuleId)
						{
							base.Response.Redirect(Globals.NavigateURL(), true);
						}
					}
				}
				return this._CustomFieldDefinitionInfo;
			}
			set
			{
				this._CustomFieldDefinitionInfo = value;
				if (value == null)
				{
					this.ViewState.Remove("CustomFieldDefinitionID");
					return;
				}
				this.ViewState["CustomFieldDefinitionID"] = this._CustomFieldDefinitionInfo.CustomFieldDefinitionID;
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

		private List<CustomFieldDefinitionListItemInfo> List
		{
			get
			{
				if (this._List == null)
				{
					if (this.ViewState["List"] != null)
					{
						this._List = this.DeserializeCustomFieldDefinitionListItemInfoList((string)this.ViewState["List"]);
					}
					else if (!Null.IsNull(this.CustomFieldDefinitionInfo.CustomFieldDefinitionID))
					{
						this._List = (new CustomFieldDefinitionListItemController()).GetCustomFieldDefinitionListItemList(this.CustomFieldDefinitionInfo.CustomFieldDefinitionID);
					}
					else
					{
						this._List = new List<CustomFieldDefinitionListItemInfo>();
					}
					this.SortList();
				}
				return this._List;
			}
		}

		protected CustomFieldDefinitionListItemListSettings ListSettings
		{
			get
			{
				if (this._ListSettings == null)
				{
					this._ListSettings = new CustomFieldDefinitionListItemListSettings(this);
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

		public EditCustomFieldDefinition()
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

		public void AddListItem(object source, EventArgs e)
		{
			try
			{
				if (this.Page.IsValid)
				{
					CustomFieldDefinitionListItemInfo customFieldDefinitionListItemInfo = new CustomFieldDefinitionListItemInfo()
					{
						Text = this.textTextBox.Text.Trim(),
						SortOrder = this.List.Count,
						CreatedByUserID = base.UserId,
						CreatedOnDate = DateTime.Now
					};
					customFieldDefinitionListItemInfo.Value = (this.valueTextBox.Text.Trim() != string.Empty ? this.valueTextBox.Text.Trim() : customFieldDefinitionListItemInfo.Text);
					this.List.Add(customFieldDefinitionListItemInfo);
					this.BindData();
					this.textTextBox.Text = string.Empty;
					this.valueTextBox.Text = string.Empty;
				}
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
			}
		}

		private void BindData()
		{
			this.SortList();
			this.dataGrid.DataSource = this.List;
			this.dataGrid.DataBind();
			this.numberOfRecordsFoundLabel.Visible = this.List.Count == 0;
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

		public List<CustomFieldDefinitionListItemInfo> DeserializeCustomFieldDefinitionListItemInfoList(string serializedCustomFieldDefinitionListItemInfoList)
		{
			List<CustomFieldDefinitionListItemInfo> customFieldDefinitionListItemInfos;
			StringReader stringReader = null;
			try
			{
				stringReader = new StringReader(serializedCustomFieldDefinitionListItemInfoList);
				customFieldDefinitionListItemInfos = new List<CustomFieldDefinitionListItemInfo>((CustomFieldDefinitionListItemInfo[])(new XmlSerializer(typeof(CustomFieldDefinitionListItemInfo[]))).Deserialize(new XmlTextReader(stringReader)));
			}
			finally
			{
				if (stringReader != null)
				{
					stringReader.Close();
				}
			}
			return customFieldDefinitionListItemInfos;
		}

		protected int IndexOfCustomFieldDefinitionListItemByID(int id, List<CustomFieldDefinitionListItemInfo> list)
		{
			int num;
			int num1 = 0;
			List<CustomFieldDefinitionListItemInfo>.Enumerator enumerator = list.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.CustomFieldDefinitionListItemID != id)
					{
						num1++;
					}
					else
					{
						num = num1;
						return num;
					}
				}
				return -1;
			}
			finally
			{
				((IDisposable)enumerator).Dispose();
			}
			return num;
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
					this.List.RemoveAt(e.Item.DataSetIndex);
					for (int i = e.Item.DataSetIndex; i < this.List.Count; i++)
					{
						CustomFieldDefinitionListItemInfo item = this.List[i];
						item.SortOrder = item.SortOrder - 1;
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
							if (typeof(CustomFieldDefinitionListItemInfo).GetProperty(this.ListSettings.VisibleDisplayColumnList[num - 1].ColumnName).PropertyType.Equals(typeof(bool)))
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
					CustomFieldDefinitionListItemInfo dataItem = (CustomFieldDefinitionListItemInfo)e.Item.DataItem;
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
			foreach (DisplayColumnInfo visibleDisplayColumnList in this.ListSettings.VisibleDisplayColumnList)
			{
				this.AddColumn(Localization.GetString(visibleDisplayColumnList.ColumnName, base.LocalResourceFile), visibleDisplayColumnList.ColumnName, (typeof(CustomFieldDefinitionListItemInfo).GetProperty(visibleDisplayColumnList.ColumnName).PropertyType.Equals(typeof(DateTime)) ? "{0:d}" : string.Empty));
			}
		}

		private void Move(int index, SortColumnInfo.SortDirection direction)
		{
			if (index >= 0)
			{
				CustomFieldDefinitionListItemInfo item = null;
				CustomFieldDefinitionListItemInfo customFieldDefinitionListItemInfo = null;
				if (direction != SortColumnInfo.SortDirection.Ascending)
				{
					if (direction == SortColumnInfo.SortDirection.Descending)
					{
						if (index < this.List.Count)
						{
							item = this.List[index];
							customFieldDefinitionListItemInfo = this.List[index + 1];
						}
					}
				}
				else if (index > 0)
				{
					item = this.List[index];
					customFieldDefinitionListItemInfo = this.List[index - 1];
				}
				int sortOrder = item.SortOrder;
				item.SortOrder = customFieldDefinitionListItemInfo.SortOrder;
				customFieldDefinitionListItemInfo.SortOrder = sortOrder;
			}
			this.BindData();
		}

		protected void MultiLine_Checked(object sender, EventArgs e)
		{
			try
			{
				this.numberOfRowsTableRow.Visible = this.multiLineCheckBox.Checked;
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
				if (!base.IsPostBack)
				{
					if (!this.HasEditPermissions)
					{
						base.Response.Redirect(Globals.NavigateURL(), true);
					}
					if (!string.IsNullOrEmpty(base.Request.QueryString["ModuleMessage"]) && !string.IsNullOrEmpty(base.Request.QueryString["ModuleMessageType"]))
					{
						Skin.AddModuleMessage(this, Localization.GetString(base.Request.QueryString["ModuleMessage"], base.LocalResourceFile), (ModuleMessage.ModuleMessageType)Enum.Parse(typeof(ModuleMessage.ModuleMessageType), base.Request.QueryString["ModuleMessageType"], true));
					}
					this.typeDropDownList.DataSource = Helper.LocalizeEnumSorted(typeof(CustomFieldDefinitionType), base.LocalResourceFile);
					this.typeDropDownList.DataTextField = "LocalizedName";
					this.typeDropDownList.DataValueField = "Name";
					this.typeDropDownList.DataBind();
					this.directionRadioButtonList.DataSource = Helper.LocalizeEnumSorted(typeof(Direction), base.LocalResourceFile);
					this.directionRadioButtonList.DataTextField = "LocalizedName";
					this.directionRadioButtonList.DataValueField = "Name";
					this.directionRadioButtonList.DataBind();
					this.directionRadioButtonList.SelectedValue = Direction.Horizontal.ToString();
					this.directionRadioButtonList.SelectedValue = RepeatDirection.Vertical.ToString();
					if (Null.IsNull(this.CustomFieldDefinitionInfo.CustomFieldDefinitionID))
					{
						this.typeDropDownList.SelectedValue = CustomFieldDefinitionType.TextBox.ToString();
					}
					else
					{
						this.nameTextBox.Text = this.CustomFieldDefinitionInfo.Name;
						this.typeDropDownList.SelectedValue = this.CustomFieldDefinitionInfo.Type;
						this.labelTextBox.Text = this.CustomFieldDefinitionInfo.Label;
						this.hideLabelCheckBox.Checked = this.CustomFieldDefinitionInfo.HideLabel;
						this.titleTextBox.Text = this.CustomFieldDefinitionInfo.Title;
						this.addToPreviousRowCheckBox.Checked = this.CustomFieldDefinitionInfo.AddToPreviousRow;
						this.requiredCheckBox.Checked = this.CustomFieldDefinitionInfo.IsRequired;
						if (this.CustomFieldDefinitionInfo.Type == CustomFieldDefinitionType.TextBox.ToString())
						{
							this.multiLineCheckBox.Checked = this.CustomFieldDefinitionInfo.OptionType != "1";
							this.numberOfRowsTextBox.Text = (this.multiLineCheckBox.Checked ? this.CustomFieldDefinitionInfo.OptionType : string.Empty);
						}
						else if (this.CustomFieldDefinitionInfo.Type == CustomFieldDefinitionType.CheckBoxList.ToString() || this.CustomFieldDefinitionInfo.Type == CustomFieldDefinitionType.RadioButtonList.ToString())
						{
							this.directionTableRow.Visible = true;
							this.directionRadioButtonList.SelectedValue = this.CustomFieldDefinitionInfo.OptionType;
						}
						if (this.CustomFieldDefinitionInfo.Type == CustomFieldDefinitionType.ListBox.ToString())
						{
							this.numberOfRowsTextBox.Text = this.CustomFieldDefinitionInfo.NumberOfRows.ToString();
							this.multiSelectCheckBox.Checked = this.CustomFieldDefinitionInfo.IsMultiSelect;
						}
					}
					this.MultiLine_Checked(sender, e);
					this.TypeDropDownList_SelectedIndexChanged(sender, e);
					this.BindData();
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
				this.ViewState["List"] = this.SerializeCustomFieldDefinitionListItemInfoList(this.List);
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
			}
			return base.SaveViewState();
		}

		public string SerializeCustomFieldDefinitionListItemInfoList(List<CustomFieldDefinitionListItemInfo> workflowEventActionInfoList)
		{
			string str;
			CustomFieldDefinitionListItemInfo[] array = workflowEventActionInfoList.ToArray();
			StringWriter stringWriter = null;
			try
			{
				stringWriter = new StringWriter();
				(new XmlSerializer(typeof(CustomFieldDefinitionListItemInfo[]))).Serialize(new XmlTextWriter(stringWriter), array);
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

		public void SortList()
		{
			if (this.ListSettings.SortColumnList.Count > 0)
			{
				this.List.Sort(new ColumnListComparer<CustomFieldDefinitionListItemInfo>(this.ListSettings.SortColumnList));
			}
		}

		protected void TypeDropDownList_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				this.multiLineTableRow.Visible = false;
				this.numberOfRowsTableRow.Visible = false;
				this.requiredTableRow.Visible = true;
				this.titleTableRow.Visible = false;
				this.directionTableRow.Visible = false;
				this.listItemsTableRow.Visible = false;
				this.multiSelectTableRow.Visible = false;
				if (this.typeDropDownList.SelectedValue == CustomFieldDefinitionType.CheckBox.ToString())
				{
					this.requiredTableRow.Visible = false;
					this.titleTableRow.Visible = true;
				}
				else if (this.typeDropDownList.SelectedValue == CustomFieldDefinitionType.TextBox.ToString())
				{
					this.titleTableRow.Visible = true;
					this.multiLineTableRow.Visible = true;
					this.numberOfRowsTableRow.Visible = this.multiLineCheckBox.Checked;
				}
				else if (this.typeDropDownList.SelectedValue == CustomFieldDefinitionType.DropDownList.ToString())
				{
					this.titleTableRow.Visible = true;
					this.listItemsTableRow.Visible = true;
				}
				else if (this.typeDropDownList.SelectedValue == CustomFieldDefinitionType.ListBox.ToString())
				{
					this.listItemsTableRow.Visible = true;
					this.numberOfRowsTableRow.Visible = true;
					this.multiSelectTableRow.Visible = true;
				}
				else if (this.typeDropDownList.SelectedValue == CustomFieldDefinitionType.CheckBoxList.ToString())
				{
					this.requiredTableRow.Visible = false;
					this.directionTableRow.Visible = true;
					this.listItemsTableRow.Visible = true;
				}
				else if (this.typeDropDownList.SelectedValue == CustomFieldDefinitionType.RadioButtonList.ToString())
				{
					this.directionTableRow.Visible = true;
					this.listItemsTableRow.Visible = true;
				}
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
			}
		}

		protected void UpdateClicked(object sender, EventArgs e)
		{
			try
			{
				if (this.Page.IsValid)
				{
					this.CustomFieldDefinitionInfo.TabModuleID = base.TabModuleId;
					this.CustomFieldDefinitionInfo.Name = this.nameTextBox.Text;
					this.CustomFieldDefinitionInfo.Label = (this.labelTableRow.Visible ? this.labelTextBox.Text : Null.NullString);
					this.CustomFieldDefinitionInfo.HideLabel = this.hideLabelCheckBox.Checked;
					this.CustomFieldDefinitionInfo.Type = this.typeDropDownList.SelectedValue;
					this.CustomFieldDefinitionInfo.Title = (this.titleTableRow.Visible ? this.titleTextBox.Text : Null.NullString);
					this.CustomFieldDefinitionInfo.AddToPreviousRow = this.addToPreviousRowCheckBox.Checked;
					this.CustomFieldDefinitionInfo.IsRequired = (this.requiredTableRow.Visible ? this.requiredCheckBox.Checked : false);
					if (this.CustomFieldDefinitionInfo.Type == CustomFieldDefinitionType.TextBox.ToString())
					{
						this.CustomFieldDefinitionInfo.OptionType = (this.multiLineCheckBox.Checked ? this.numberOfRowsTextBox.Text : "1");
					}
					else if (this.CustomFieldDefinitionInfo.Type == CustomFieldDefinitionType.CheckBoxList.ToString() || this.CustomFieldDefinitionInfo.Type == CustomFieldDefinitionType.RadioButtonList.ToString())
					{
						this.CustomFieldDefinitionInfo.OptionType = this.directionRadioButtonList.SelectedValue;
					}
					else if (this.CustomFieldDefinitionInfo.Type == CustomFieldDefinitionType.ListBox.ToString())
					{
						this.CustomFieldDefinitionInfo.OptionType = string.Concat(this.numberOfRowsTextBox.Text, (this.multiSelectCheckBox.Checked ? "m" : "s"));
					}
					if (!Null.IsNull(this.CustomFieldDefinitionInfo.CustomFieldDefinitionID))
					{
						this.CustomFieldDefinitionInfo.LastModifiedByUserID = base.UserId;
						this.CustomFieldDefinitionInfo.LastModifiedOnDate = DateTime.Now;
						(new CustomFieldDefinitionController()).UpdateCustomFieldDefinition(this.CustomFieldDefinitionInfo);
					}
					else
					{
						this.CustomFieldDefinitionInfo.SortOrder = (new CustomFieldDefinitionController()).GetActiveCustomFieldDefinitionList(base.TabModuleId).Count;
						this.CustomFieldDefinitionInfo.IsActive = true;
						this.CustomFieldDefinitionInfo.CreatedByUserID = base.UserId;
						this.CustomFieldDefinitionInfo.CreatedOnDate = DateTime.Now;
						this.CustomFieldDefinitionInfo = (new CustomFieldDefinitionController()).AddCustomFieldDefinition(this.CustomFieldDefinitionInfo);
					}
					this.UpdateList();
					base.Response.Redirect(this.ReturnUrl, true);
				}
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
			}
		}

		protected void UpdateList()
		{
			List<CustomFieldDefinitionListItemInfo> customFieldDefinitionListItemList = (new CustomFieldDefinitionListItemController()).GetCustomFieldDefinitionListItemList(this.CustomFieldDefinitionInfo.CustomFieldDefinitionID);
			foreach (CustomFieldDefinitionListItemInfo list in this.List)
			{
				if (!Null.IsNull(list.CustomFieldDefinitionListItemID))
				{
					list.LastModifiedByUserID = base.UserId;
					list.LastModifiedOnDate = DateTime.Now;
					(new CustomFieldDefinitionListItemController()).UpdateCustomFieldDefinitionListItem(list);
				}
				else
				{
					list.CustomFieldDefinitionID = this.CustomFieldDefinitionInfo.CustomFieldDefinitionID;
					list.CreatedByUserID = base.UserId;
					list.CreatedOnDate = DateTime.Now;
					(new CustomFieldDefinitionListItemController()).AddCustomFieldDefinitionListItem(list);
				}
			}
			foreach (CustomFieldDefinitionListItemInfo customFieldDefinitionListItemInfo in customFieldDefinitionListItemList)
			{
				if (this.IndexOfCustomFieldDefinitionListItemByID(customFieldDefinitionListItemInfo.CustomFieldDefinitionListItemID, this.List) != -1)
				{
					continue;
				}
				(new CustomFieldDefinitionListItemController()).DeleteCustomFieldDefinitionListItem(customFieldDefinitionListItemInfo.CustomFieldDefinitionListItemID);
			}
		}

		protected void ValidateName(object sender, ServerValidateEventArgs e)
		{
			try
			{
				foreach (Gafware.Modules.Reservations.CustomFieldDefinitionInfo activeCustomFieldDefinitionList in (new CustomFieldDefinitionController()).GetActiveCustomFieldDefinitionList(base.TabModuleId))
				{
					if (activeCustomFieldDefinitionList.CustomFieldDefinitionID == this.CustomFieldDefinitionInfo.CustomFieldDefinitionID || !(activeCustomFieldDefinitionList.Name == e.Value))
					{
						continue;
					}
					Skin.AddModuleMessage(this, Localization.GetString("NameAlreadyExists", base.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
					e.IsValid = false;
					return;
				}
				e.IsValid = true;
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
			}
		}
	}
}