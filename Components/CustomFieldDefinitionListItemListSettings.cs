using DotNetNuke.Entities.Modules;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Gafware.Modules.Reservations
{
	public class CustomFieldDefinitionListItemListSettings : ListSettings
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
				return "CustomFieldDefinitionListItemListAllowPaging";
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
				return "CustomFieldDefinitionListItemListAllowSorting";
			}
		}

		public override List<DisplayColumnInfo> DisplayColumnList
		{
			get
			{
				string item = (string)this.portalModuleBase.Settings[this.DISPLAYCOLUMNLIST_KEY] ?? "SortOrder;False,Text;True,Value;True";
				return this.DeserializeDisplayColumnList(item);
			}
		}

		public override string DISPLAYCOLUMNLIST_KEY
		{
			get
			{
				return "CustomFieldDefinitionListItemListDisplayColumns";
			}
		}

		public override string PAGERMODE_KEY
		{
			get
			{
				return "CustomFieldDefinitionListItemListPagerMode";
			}
		}

		public override string PAGERPOSITION_KEY
		{
			get
			{
				return "CustomFieldDefinitionListItemListPagerPosition";
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
				return "CustomFieldDefinitionListItemListPageSize";
			}
		}

		public override string SORTCOLUMNCOLOR_KEY
		{
			get
			{
				return "CustomFieldDefinitionListItemListSortColumnColor";
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
				return "CustomFieldDefinitionListItemListSortOrder";
			}
		}

		public CustomFieldDefinitionListItemListSettings(PortalModuleBase portalModuleBase) : base(portalModuleBase)
		{
		}
	}
}