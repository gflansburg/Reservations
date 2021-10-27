using System;
using System.Collections.Generic;

namespace Gafware.Modules.Reservations
{
	public class AdvancedColumnListComparer<T> : IComparer<T>
	{
		private List<SortColumnInfo> sortColumnList;

		private ListSettings listSettings;

		public AdvancedColumnListComparer(ListSettings listSettings, List<SortColumnInfo> sortColumnList)
		{
			this.sortColumnList = sortColumnList;
			this.listSettings = listSettings;
		}

		public int Compare(T x, T y)
		{
			if (this.sortColumnList.Count == 0)
			{
				return 0;
			}
			return this.Compare(0, x, y);
		}

		private int Compare(int sortColumnIndex, T x, T y)
		{
			int num = 0;
			if (sortColumnIndex >= this.sortColumnList.Count)
			{
				return 0;
			}
			SortColumnInfo item = this.sortColumnList[sortColumnIndex];
			num = (item.Direction != SortColumnInfo.SortDirection.Ascending ? this.CompareValues(item.ColumnName, y, x) : this.CompareValues(item.ColumnName, x, y));
			if (num != 0)
			{
				return num;
			}
			return this.Compare(sortColumnIndex + 1, x, y);
		}

		private int CompareValues(string columnName, T x, T y)
		{
			object obj = this.listSettings.Eval(x, columnName);
			object obj1 = this.listSettings.Eval(y, columnName);
			if (obj == null && obj1 == null)
			{
				return 0;
			}
			if (obj == null)
			{
				return -1;
			}
			if (obj1 == null)
			{
				return 1;
			}
			return ((IComparable)obj).CompareTo(obj1);
		}
	}
}