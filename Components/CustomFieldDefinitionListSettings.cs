using DotNetNuke.Entities.Modules;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Gafware.Modules.Reservations
{
	public class CustomFieldDefinitionListSettings : ListSettings
	{
		public override bool AllowPaging
		{
			get
			{
				return false;
			}
		}

		public override string ALLOWPAGING_KEY
		{
			get
			{
				return "CustomFieldDefinitionListAllowPaging";
			}
		}

		public override bool AllowSorting
		{
			get
			{
				return false;
			}
		}

		public override string ALLOWSORTING_KEY
		{
			get
			{
				return "CustomFieldDefinitionListAllowSorting";
			}
		}

		public override List<DisplayColumnInfo> DisplayColumnList
		{
			get
			{
				string item = (string)this.portalModuleBase.Settings[this.DISPLAYCOLUMNLIST_KEY] ?? "SortOrder;False,Name;True,Label;True,Title;True,Type;True,IsRequired;True,CreatedByDisplayName;False,CreatedOnDate;False,LastModifiedByDisplayName;False,LastModifiedOnDate;False";
				return this.DeserializeDisplayColumnList(item);
			}
		}

		public override string DISPLAYCOLUMNLIST_KEY
		{
			get
			{
				return "CustomFieldDefinitionListDisplayColumns";
			}
		}

		public override string PAGERMODE_KEY
		{
			get
			{
				return "CustomFieldDefinitionListPagerMode";
			}
		}

		public override string PAGERPOSITION_KEY
		{
			get
			{
				return "CustomFieldDefinitionListPagerPosition";
			}
		}

		public override int PageSize
		{
			get
			{
				string item = (string)this.portalModuleBase.Settings[this.PAGESIZE_KEY] ?? "10";
				return int.Parse(item);
			}
		}

		public override string PAGESIZE_KEY
		{
			get
			{
				return "CustomFieldDefinitionListPageSize";
			}
		}

		public override string SORTCOLUMNCOLOR_KEY
		{
			get
			{
				return "CustomFieldDefinitionListSortColumnColor";
			}
		}

		public override List<SortColumnInfo> SortColumnList
		{
			get
			{
				string item = (string)this.portalModuleBase.Settings[this.SORTORDER_KEY] ?? "SortOrder";
				return this.DeserializeSortColumnList(item);
			}
		}

		public override string SORTORDER_KEY
		{
			get
			{
				return "CustomFieldDefinitionListSortOrder";
			}
		}

		public CustomFieldDefinitionListSettings(PortalModuleBase portalModuleBase) : base(portalModuleBase)
		{
		}
	}
}