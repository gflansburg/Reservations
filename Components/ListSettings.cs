using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.WebControls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Gafware.Modules.Reservations
{
	public abstract class ListSettings
	{
		protected PortalModuleBase portalModuleBase;

		protected Gafware.Modules.Reservations.Helper _Helper;

		public virtual bool AllowPaging
		{
			get
			{
				string item = (string)this.portalModuleBase.Settings[this.ALLOWPAGING_KEY] ?? bool.TrueString;
				return bool.Parse(item);
			}
		}

		public abstract string ALLOWPAGING_KEY
		{
			get;
		}

		public virtual bool AllowSorting
		{
			get
			{
				string item = (string)this.portalModuleBase.Settings[this.ALLOWSORTING_KEY] ?? bool.TrueString;
				return bool.Parse(item);
			}
		}

		public abstract string ALLOWSORTING_KEY
		{
			get;
		}

		public virtual List<CommandButton> CommandButtonList
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public virtual List<DisplayColumnInfo> DisplayColumnList
		{
			get
			{
				string item = (string)this.portalModuleBase.Settings[this.DISPLAYCOLUMNLIST_KEY] ?? this.DisplayColumnList_Default;
				return this.DeserializeDisplayColumnList(item);
			}
		}

		public virtual string DisplayColumnList_Default
		{
			get
			{
				return string.Empty;
			}
		}

		public abstract string DISPLAYCOLUMNLIST_KEY
		{
			get;
		}

		protected Gafware.Modules.Reservations.Helper Helper
		{
			get
			{
				if (this._Helper == null)
				{
					this._Helper = new Gafware.Modules.Reservations.Helper(this.portalModuleBase.PortalId, this.portalModuleBase.TabModuleId, this.portalModuleBase.LocalResourceFile);
				}
				return this._Helper;
			}
		}

		public virtual string IdPropertyName
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public virtual Type InfoType
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public virtual Gafware.Modules.Reservations.PagerMode PagerMode
		{
			get
			{
				string item = (string)this.portalModuleBase.Settings[this.PAGERMODE_KEY] ?? 2.ToString();
				return (Gafware.Modules.Reservations.PagerMode)int.Parse(item);
			}
		}

		public abstract string PAGERMODE_KEY
		{
			get;
		}

		public virtual System.Web.UI.WebControls.PagerPosition PagerPosition
		{
			get
			{
				string item = (string)this.portalModuleBase.Settings[this.PAGERPOSITION_KEY] ?? 0.ToString();
				return (System.Web.UI.WebControls.PagerPosition)int.Parse(item);
			}
		}

		public abstract string PAGERPOSITION_KEY
		{
			get;
		}

		public virtual int PageSize
		{
			get
			{
				string item = (string)this.portalModuleBase.Settings[this.PAGESIZE_KEY] ?? "10";
				return int.Parse(item);
			}
		}

		public abstract string PAGESIZE_KEY
		{
			get;
		}

		public abstract string SORTCOLUMNCOLOR_KEY
		{
			get;
		}

		public virtual List<SortColumnInfo> SortColumnList
		{
			get
			{
				string item = (string)this.portalModuleBase.Settings[this.SORTORDER_KEY] ?? this.SortColumnList_Default;
				return this.DeserializeSortColumnList(item);
			}
		}

		public virtual string SortColumnList_Default
		{
			get
			{
				return string.Empty;
			}
		}

		public abstract string SORTORDER_KEY
		{
			get;
		}

		public virtual List<DisplayColumnInfo> VisibleDisplayColumnList
		{
			get
			{
				List<DisplayColumnInfo> displayColumnInfos = new List<DisplayColumnInfo>();
				foreach (DisplayColumnInfo displayColumnList in this.DisplayColumnList)
				{
					if (!displayColumnList.Visible)
					{
						continue;
					}
					displayColumnInfos.Add(displayColumnList);
				}
				return displayColumnInfos;
			}
		}

		public ListSettings(PortalModuleBase portalModuleBase)
		{
			this.portalModuleBase = portalModuleBase;
		}

		public virtual bool ApplyCustomFilter(object info, string columnName, string text)
		{
			throw new NotImplementedException();
		}

		public virtual bool CanViewList(int userID)
		{
			throw new NotImplementedException();
		}

		public virtual List<DisplayColumnInfo> DeserializeDisplayColumnList(string serializedDisplayColumnList)
		{
			List<DisplayColumnInfo> displayColumnInfos = new List<DisplayColumnInfo>();
			int num = 0;
			string[] strArrays = serializedDisplayColumnList.Split(new char[] { ',' });
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				string str = strArrays[i];
				DisplayColumnInfo displayColumnInfo = new DisplayColumnInfo()
				{
					ColumnName = str.Split(new char[] { ';' })[0],
					DisplayOrder = num,
					Visible = bool.Parse(str.Split(new char[] { ';' })[1])
				};
				displayColumnInfo.LocalizedColumnName = Localization.GetString(displayColumnInfo.ColumnName, this.portalModuleBase.LocalResourceFile);
				displayColumnInfos.Add(displayColumnInfo);
				num++;
			}
			return displayColumnInfos;
		}

		public virtual List<SortColumnInfo> DeserializeSortColumnList(string serializedSortColumnList)
		{
			List<SortColumnInfo> sortColumnInfos = new List<SortColumnInfo>();
			if (serializedSortColumnList != string.Empty)
			{
				string[] strArrays = serializedSortColumnList.Split(new char[] { ',' });
				for (int i = 0; i < (int)strArrays.Length; i++)
				{
					string str = strArrays[i];
					SortColumnInfo sortColumnInfo = new SortColumnInfo()
					{
						Direction = (str[0] == '-' ? SortColumnInfo.SortDirection.Descending : SortColumnInfo.SortDirection.Ascending)
					};
					sortColumnInfo.ColumnName = (sortColumnInfo.Direction == SortColumnInfo.SortDirection.Descending ? str.Substring(1) : str);
					sortColumnInfo.LocalizedColumnName = Localization.GetString(sortColumnInfo.ColumnName, this.portalModuleBase.LocalResourceFile);
					SortColumnInfo.SortDirection direction = sortColumnInfo.Direction;
					sortColumnInfo.LocalizedDirection = Localization.GetString(direction.ToString(), this.portalModuleBase.LocalResourceFile);
					sortColumnInfos.Add(sortColumnInfo);
				}
			}
			return sortColumnInfos;
		}

		public virtual object Eval(object container, string expression)
		{
			return DataBinder.Eval(container, expression);
		}

		public virtual bool ExcludeFromDateRangeFilters(string columnName)
		{
			return false;
		}

		public virtual string GetDefaultFilterValue(string columnName)
		{
			throw new NotImplementedException();
		}

		public virtual string GetDisplayFormat(string columnName)
		{
			return "{0}";
		}

		public virtual IList GetEmptyList()
		{
			throw new NotImplementedException();
		}

		public virtual WebControl GetFilterControl(string columnName, EventHandler eventHandler)
		{
			throw new NotImplementedException();
		}

		public virtual IList GetList(OrderedDictionary FiltersOrderedDictionary)
		{
			throw new NotImplementedException();
		}

		public virtual void HandleCommand(int id, string commandName, PortalModuleBase portalModuleBase = null, string returnUrl = null)
		{
			throw new NotImplementedException();
		}

		public virtual bool ImplementsCustomFilter(string columnName)
		{
			throw new NotImplementedException();
		}

		public virtual bool IsHighlightable(string columnName)
		{
			return true;
		}

		public virtual List<LocalizedEnum> LocalizeEnum(Type type)
		{
			List<LocalizedEnum> localizedEnums = new List<LocalizedEnum>();
			string[] names = Enum.GetNames(type);
			for (int i = 0; i < (int)names.Length; i++)
			{
				string str = names[i];
				localizedEnums.Add(new LocalizedEnum(Localization.GetString(str, this.portalModuleBase.LocalResourceFile), str));
			}
			localizedEnums.Sort();
			return localizedEnums;
		}

		public virtual string SerializeDisplayColumnList(List<DisplayColumnInfo> displayColumnList)
		{
			string empty = string.Empty;
			foreach (DisplayColumnInfo displayColumnInfo in displayColumnList)
			{
				string[] columnName = new string[] { empty, null, null, null, null };
				columnName[1] = (empty == string.Empty ? string.Empty : ",");
				columnName[2] = displayColumnInfo.ColumnName;
				columnName[3] = ";";
				columnName[4] = displayColumnInfo.Visible.ToString();
				empty = string.Concat(columnName);
			}
			return empty;
		}

		public virtual string SerializeSortColumnList(List<SortColumnInfo> sortColumnList)
		{
			string empty = string.Empty;
			foreach (SortColumnInfo sortColumnInfo in sortColumnList)
			{
				empty = string.Concat(empty, (empty == string.Empty ? string.Empty : ","), (sortColumnInfo.Direction == SortColumnInfo.SortDirection.Descending ? "-" : string.Empty), sortColumnInfo.ColumnName);
			}
			return empty;
		}

		public virtual bool SkipFilter(string columnName)
		{
			throw new NotImplementedException();
		}

		public virtual void SortList(ICollection list, List<SortColumnInfo> sortColumnList)
		{
			throw new NotImplementedException();
		}
	}
}