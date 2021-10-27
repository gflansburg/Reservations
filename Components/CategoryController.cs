using DotNetNuke.Common.Utilities;
using Gafware.Modules.Reservations.Data;
using System;
using System.Collections.Generic;

namespace Gafware.Modules.Reservations
{
	public class CategoryController
	{
		public CategoryController()
		{
		}

		public Gafware.Modules.Reservations.CategoryInfo AddCategory(Gafware.Modules.Reservations.CategoryInfo CategoryInfo)
		{
			CategoryInfo.CategoryID = DataProvider.Instance().AddCategory(CategoryInfo.TabModuleID, CategoryInfo.Name, CategoryInfo.CreatedByUserID, CategoryInfo.CreatedOnDate, CategoryInfo.LastModifiedByUserID, CategoryInfo.LastModifiedOnDate);
			return CategoryInfo;
		}

		public void DeleteCategory(int CategoryID)
		{
			DataProvider.Instance().DeleteCategory(CategoryID);
		}

		public CategoryInfo GetCategory(int CategoryID)
		{
			return (CategoryInfo)CBO.FillObject(DataProvider.Instance().GetCategory(CategoryID), typeof(CategoryInfo));
		}

		public List<CategoryInfo> GetCategoryList(int TabModuleID)
		{
			return CBO.FillCollection<CategoryInfo>(DataProvider.Instance().GetCategoryList(TabModuleID));
		}

		public void UpdateCategory(Gafware.Modules.Reservations.CategoryInfo CategoryInfo)
		{
			DataProvider.Instance().UpdateCategory(CategoryInfo.TabModuleID, CategoryInfo.CategoryID, CategoryInfo.Name, CategoryInfo.CreatedByUserID, CategoryInfo.CreatedOnDate, CategoryInfo.LastModifiedByUserID, CategoryInfo.LastModifiedOnDate);
		}
	}
}