using System;
using System.Collections.Generic;
using System.Reflection;

namespace Gafware.Modules.Reservations
{
	public class ColumnListComparer<T> : IComparer<T>
	{
		private List<SortColumnInfo> sortColumnList;

		public ColumnListComparer(List<SortColumnInfo> sortColumnList)
		{
			this.sortColumnList = sortColumnList;
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
			PropertyInfo property = x.GetType().GetProperty(columnName);
			PropertyInfo propertyInfo = y.GetType().GetProperty(columnName);
			object value = property.GetValue(x, null);
			object obj = propertyInfo.GetValue(y, null);
			if (value == null && obj == null)
			{
				return 0;
			}
			if (value == null)
			{
				return -1;
			}
			if (obj == null)
			{
				return 1;
			}
			return ((IComparable)value).CompareTo(obj);
		}
	}
}