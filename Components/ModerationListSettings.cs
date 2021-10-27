using DotNetNuke.Entities.Modules;
using System;

namespace Gafware.Modules.Reservations
{
	public class ModerationListSettings : ListSettings
	{
		public override string ALLOWPAGING_KEY
		{
			get
			{
				return "ModerationListAllowPaging";
			}
		}

		public override string ALLOWSORTING_KEY
		{
			get
			{
				return "ModerationListAllowSorting";
			}
		}

		public override string DisplayColumnList_Default
		{
			get
			{
				return "CategoryName;True,FullName;True,Email;True,Phone;True,StartDate;True,StartTime;True,EndTime;True,Duration;False,Description;False,CreatedOnDate;False";
			}
		}

		public override string DISPLAYCOLUMNLIST_KEY
		{
			get
			{
				return "ModerationListDisplayColumns";
			}
		}

		public override string PAGERMODE_KEY
		{
			get
			{
				return "ModerationListPagerMode";
			}
		}

		public override string PAGERPOSITION_KEY
		{
			get
			{
				return "ModerationListPagerPosition";
			}
		}

		public override string PAGESIZE_KEY
		{
			get
			{
				return "ModerationListPageSize";
			}
		}

		public override string SORTCOLUMNCOLOR_KEY
		{
			get
			{
				return "ModerationListSortColumnColor";
			}
		}

		public override string SortColumnList_Default
		{
			get
			{
				return "StartTime";
			}
		}

		public override string SORTORDER_KEY
		{
			get
			{
				return "ModerationListSortOrder";
			}
		}

		public ModerationListSettings(PortalModuleBase portalModuleBase) : base(portalModuleBase)
		{
		}
	}
}