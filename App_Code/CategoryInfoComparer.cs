using System;
using System.Collections;
using System.Collections.Generic;

namespace Gafware.Modules.Reservations
{
	internal class CategoryInfoComparer : IComparer
	{
		private List<CategoryInfo> categoryInfoList;

		public CategoryInfoComparer(List<CategoryInfo> categoryInfoList)
		{
			this.categoryInfoList = categoryInfoList;
		}

		public int Compare(object x, object y)
		{
			return this.FindCategoryInfoByCategoryID((int)x).Name.CompareTo(this.FindCategoryInfoByCategoryID((int)y).Name);
		}

		private CategoryInfo FindCategoryInfoByCategoryID(int categoryID)
		{
			CategoryInfo categoryInfo;
			List<CategoryInfo>.Enumerator enumerator = this.categoryInfoList.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					CategoryInfo current = enumerator.Current;
					if (current.CategoryID != categoryID)
					{
						continue;
					}
					categoryInfo = current;
					return categoryInfo;
				}
				return null;
			}
			finally
			{
				((IDisposable)enumerator).Dispose();
			}
		}
	}
}