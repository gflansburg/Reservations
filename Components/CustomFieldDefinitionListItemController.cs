using DotNetNuke.Common.Utilities;
using Gafware.Modules.Reservations.Data;
using System;
using System.Collections.Generic;

namespace Gafware.Modules.Reservations
{
	public class CustomFieldDefinitionListItemController
	{
		public CustomFieldDefinitionListItemController()
		{
		}

		public Gafware.Modules.Reservations.CustomFieldDefinitionListItemInfo AddCustomFieldDefinitionListItem(Gafware.Modules.Reservations.CustomFieldDefinitionListItemInfo CustomFieldDefinitionListItemInfo)
		{
			CustomFieldDefinitionListItemInfo.CustomFieldDefinitionListItemID = DataProvider.Instance().AddCustomFieldDefinitionListItem(CustomFieldDefinitionListItemInfo.CustomFieldDefinitionID, CustomFieldDefinitionListItemInfo.Text, CustomFieldDefinitionListItemInfo.Value, CustomFieldDefinitionListItemInfo.SortOrder, CustomFieldDefinitionListItemInfo.CreatedByUserID, CustomFieldDefinitionListItemInfo.CreatedOnDate, CustomFieldDefinitionListItemInfo.LastModifiedByUserID, CustomFieldDefinitionListItemInfo.LastModifiedOnDate);
			return CustomFieldDefinitionListItemInfo;
		}

		public void DeleteCustomFieldDefinitionListItem(int CustomFieldDefinitionListItemID)
		{
			DataProvider.Instance().DeleteCustomFieldDefinitionListItem(CustomFieldDefinitionListItemID);
		}

		public CustomFieldDefinitionListItemInfo GetCustomFieldDefinitionListItem(int CustomFieldDefinitionListItemID)
		{
			return (CustomFieldDefinitionListItemInfo)CBO.FillObject(DataProvider.Instance().GetCustomFieldDefinitionListItem(CustomFieldDefinitionListItemID), typeof(CustomFieldDefinitionListItemInfo));
		}

		public List<CustomFieldDefinitionListItemInfo> GetCustomFieldDefinitionListItemList(int CustomFieldDefinitionID)
		{
			return CBO.FillCollection<CustomFieldDefinitionListItemInfo>(DataProvider.Instance().GetCustomFieldDefinitionListItemList(CustomFieldDefinitionID));
		}

		public void UpdateCustomFieldDefinitionListItem(Gafware.Modules.Reservations.CustomFieldDefinitionListItemInfo CustomFieldDefinitionListItemInfo)
		{
			DataProvider.Instance().UpdateCustomFieldDefinitionListItem(CustomFieldDefinitionListItemInfo.CustomFieldDefinitionID, CustomFieldDefinitionListItemInfo.CustomFieldDefinitionListItemID, CustomFieldDefinitionListItemInfo.Text, CustomFieldDefinitionListItemInfo.Value, CustomFieldDefinitionListItemInfo.SortOrder, CustomFieldDefinitionListItemInfo.CreatedByUserID, CustomFieldDefinitionListItemInfo.CreatedOnDate, CustomFieldDefinitionListItemInfo.LastModifiedByUserID, CustomFieldDefinitionListItemInfo.LastModifiedOnDate);
		}
	}
}