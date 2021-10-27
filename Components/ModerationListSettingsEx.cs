using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Localization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Web.UI.WebControls;

namespace Gafware.Modules.Reservations
{
	public class ModerationListSettingsEx
	{
		public const string DISPLAYCOLUMNLIST_KEY = "ModerationListDisplayColumns";

		public const string SORTORDER_KEY = "ModerationListSortOrder";

		public const string ALLOWSORTING_KEY = "ModerationListAllowSorting";

		public const string ALLOWPAGING_KEY = "ModerationListAllowPaging";

		public const string PAGESIZE_KEY = "ModerationListPageSize";

		public const string PAGERMODE_KEY = "ModerationListPagerMode";

		public const string PAGERPOSITION_KEY = "ModerationListPagerPosition";

		public const string SORTCOLUMNCOLOR_KEY = "ModerationListSortColumnColor";

		public const string MAXTEXTLENGTH_KEY = "ModerationListMaxTextLength";

		private PortalModuleBase portalModuleBase;

		public bool AllowPaging
		{
			get
			{
				string item = (string)this.portalModuleBase.Settings["ModerationListAllowPaging"] ?? bool.TrueString;
				return bool.Parse(item);
			}
		}

		public bool AllowSorting
		{
			get
			{
				string item = (string)this.portalModuleBase.Settings["ModerationListAllowSorting"] ?? bool.TrueString;
				return bool.Parse(item);
			}
		}

		public List<DisplayColumnInfo> DisplayColumnList
		{
			get
			{
				string item = (string)this.portalModuleBase.Settings["ModerationListDisplayColumns"] ?? "CategoryName;False,FullName;True,StartDate;True,StartTime;True,EndTime;True,Duration;False,Email;False,Phone;False,Description;False,CreatedOnDate;False";
				return this.DeserializeDisplayColumnList(item);
			}
		}

		public int MaxTextLength
		{
			get
			{
				string item = (string)this.portalModuleBase.Settings["ModerationListMaxTextLength"];
				if (item == null)
				{
					item = "20";
				}
				else if (item == string.Empty)
				{
					item = Null.NullInteger.ToString();
				}
				return int.Parse(item);
			}
		}

		public Gafware.Modules.Reservations.PagerMode PagerMode
		{
			get
			{
				string item = (string)this.portalModuleBase.Settings["ModerationListPagerMode"] ?? 2.ToString();
				return (Gafware.Modules.Reservations.PagerMode)int.Parse(item);
			}
		}

		public System.Web.UI.WebControls.PagerPosition PagerPosition
		{
			get
			{
				string item = (string)this.portalModuleBase.Settings["ModerationListPagerPosition"] ?? 0.ToString();
				return (System.Web.UI.WebControls.PagerPosition)int.Parse(item);
			}
		}

		public int PageSize
		{
			get
			{
				string item = (string)this.portalModuleBase.Settings["ModerationListPageSize"] ?? "10";
				return int.Parse(item);
			}
		}

		public List<SortColumnInfo> SortColumnList
		{
			get
			{
				string item = (string)this.portalModuleBase.Settings["ModerationListSortOrder"] ?? "StartDate";
				return this.DeserializeSortColumnList(item);
			}
		}

		public List<DisplayColumnInfo> VisibleDisplayColumnList
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

		public ModerationListSettingsEx(PortalModuleBase portalModuleBase)
		{
			this.portalModuleBase = portalModuleBase;
		}

		public List<DisplayColumnInfo> DeserializeDisplayColumnList(string serializedDisplayColumnList)
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

		public List<SortColumnInfo> DeserializeSortColumnList(string serializedSortColumnList)
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
						Direction = (str[0] == '-' ? SortColumnInfo.SortDirection.Descending : SortColumnInfo.SortDirection.Ascending),
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

		public List<LocalizedEnum> LocalizeEnum(Type type)
		{
			List<LocalizedEnum> localizedEnums = new List<LocalizedEnum>();
			string[] names = Enum.GetNames(type);
			for (int i = 0; i < (int)names.Length; i++)
			{
				string str = names[i];
				localizedEnums.Add(new LocalizedEnum(Localization.GetString(str, this.portalModuleBase.LocalResourceFile), str));
			}
			return localizedEnums;
		}

		public string SerializeDisplayColumnList(List<DisplayColumnInfo> displayColumnList)
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

		public string SerializeSortColumnList(List<SortColumnInfo> sortColumnList)
		{
			string empty = string.Empty;
			foreach (SortColumnInfo sortColumnInfo in sortColumnList)
			{
				empty = string.Concat(empty, (empty == string.Empty ? string.Empty : ","), (sortColumnInfo.Direction == SortColumnInfo.SortDirection.Descending ? "-" : string.Empty), sortColumnInfo.ColumnName);
			}
			return empty;
		}
	}
}