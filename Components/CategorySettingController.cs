using DotNetNuke.Common.Utilities;
using Gafware.Modules.Reservations.Data;
using System;
using System.Collections.Generic;

namespace Gafware.Modules.Reservations
{
	public class CategorySettingController
	{
		public CategorySettingController()
		{
		}

		public Gafware.Modules.Reservations.CategorySettingInfo AddCategorySetting(Gafware.Modules.Reservations.CategorySettingInfo CategorySettingInfo)
		{
			DataProvider.Instance().AddCategorySetting(CategorySettingInfo.CategoryID, CategorySettingInfo.SettingName, CategorySettingInfo.SettingValue);
			return CategorySettingInfo;
		}

		public void DeleteCategorySetting(int CategoryID, string SettingName)
		{
			DataProvider.Instance().DeleteCategorySetting(CategoryID, SettingName);
		}

		public CategorySettingInfo GetCategorySetting(int CategoryID, string SettingName)
		{
			return (CategorySettingInfo)CBO.FillObject(DataProvider.Instance().GetCategorySetting(CategoryID, SettingName), typeof(CategorySettingInfo));
		}

		public List<CategorySettingInfo> GetCategorySettingList(int CategoryID)
		{
			return CBO.FillCollection<CategorySettingInfo>(DataProvider.Instance().GetCategorySettingList(CategoryID));
		}

		public void UpdateCategorySetting(Gafware.Modules.Reservations.CategorySettingInfo CategorySettingInfo)
		{
			DataProvider.Instance().UpdateCategorySetting(CategorySettingInfo.CategoryID, CategorySettingInfo.SettingName, CategorySettingInfo.SettingValue);
		}

		public void UpdateCategorySetting(int CategoryID, string SettingName, string SettingValue)
		{
			this.DeleteCategorySetting(CategoryID, SettingName);
			CategorySettingInfo categorySettingInfo = new CategorySettingInfo()
			{
				CategoryID = CategoryID,
				SettingName = SettingName,
				SettingValue = SettingValue
			};
			this.AddCategorySetting(categorySettingInfo);
		}
	}
}