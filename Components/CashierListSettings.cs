using DotNetNuke.Entities.Modules;
using System;

namespace Gafware.Modules.Reservations
{
	public class CashierListSettings : ListSettings
	{
		public override string ALLOWPAGING_KEY
		{
			get
			{
				return "CashierAllowPaging";
			}
		}

		public override string ALLOWSORTING_KEY
		{
			get
			{
				return "CashierAllowSorting";
			}
		}

		public override string DisplayColumnList_Default
		{
			get
			{
				return "CategoryName;False,FullName;True,Email;True,StartDate;True,StartTime;True,EndTime;True,Duration;False,Phone;False,Description;False,CreatedOnDate;False,Amount;True,Currency;False,Status;True";
			}
		}

		public override string DISPLAYCOLUMNLIST_KEY
		{
			get
			{
				return "CashierDisplayColumns";
			}
		}

		public override string PAGERMODE_KEY
		{
			get
			{
				return "CashierPagerMode";
			}
		}

		public override string PAGERPOSITION_KEY
		{
			get
			{
				return "CashierPagerPosition";
			}
		}

		public override string PAGESIZE_KEY
		{
			get
			{
				return "CashierPageSize";
			}
		}

		public override string SORTCOLUMNCOLOR_KEY
		{
			get
			{
				return "CashierSortColumnColor";
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
				return "CashierSortOrder";
			}
		}

		public CashierListSettings(PortalModuleBase portalModuleBase) : base(portalModuleBase)
		{
		}
	}
}