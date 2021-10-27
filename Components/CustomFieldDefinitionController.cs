using DotNetNuke.Common.Utilities;
using Gafware.Modules.Reservations.Data;
using System;
using System.Collections.Generic;

namespace Gafware.Modules.Reservations
{
	public class CustomFieldDefinitionController
	{
		public CustomFieldDefinitionController()
		{
		}

		public Gafware.Modules.Reservations.CustomFieldDefinitionInfo AddCustomFieldDefinition(Gafware.Modules.Reservations.CustomFieldDefinitionInfo CustomFieldDefinitionInfo)
		{
			CustomFieldDefinitionInfo.CustomFieldDefinitionID = DataProvider.Instance().AddCustomFieldDefinition(CustomFieldDefinitionInfo.TabModuleID, CustomFieldDefinitionInfo.Name, CustomFieldDefinitionInfo.Label, CustomFieldDefinitionInfo.Type, CustomFieldDefinitionInfo.OptionType, CustomFieldDefinitionInfo.Title, CustomFieldDefinitionInfo.AddToPreviousRow, CustomFieldDefinitionInfo.IsRequired, CustomFieldDefinitionInfo.IsActive, CustomFieldDefinitionInfo.DefaultValue, CustomFieldDefinitionInfo.MaxLength, CustomFieldDefinitionInfo.SortOrder, CustomFieldDefinitionInfo.CreatedByUserID, CustomFieldDefinitionInfo.CreatedOnDate, CustomFieldDefinitionInfo.LastModifiedByUserID, CustomFieldDefinitionInfo.LastModifiedOnDate, CustomFieldDefinitionInfo.HideLabel);
			return CustomFieldDefinitionInfo;
		}

		public void DeleteCustomFieldDefinition(int CustomFieldDefinitionID)
		{
			DataProvider.Instance().DeleteCustomFieldDefinition(CustomFieldDefinitionID);
		}

		public List<CustomFieldDefinitionInfo> GetActiveCustomFieldDefinitionList(int TabModuleID)
		{
			return CBO.FillCollection<CustomFieldDefinitionInfo>(DataProvider.Instance().GetActiveCustomFieldDefinitionList(TabModuleID));
		}

		public CustomFieldDefinitionInfo GetCustomFieldDefinition(int CustomFieldDefinitionID)
		{
			return (CustomFieldDefinitionInfo)CBO.FillObject(DataProvider.Instance().GetCustomFieldDefinition(CustomFieldDefinitionID), typeof(CustomFieldDefinitionInfo));
		}

		public List<CustomFieldDefinitionInfo> GetCustomFieldDefinitionList(int TabModuleID)
		{
			return CBO.FillCollection<CustomFieldDefinitionInfo>(DataProvider.Instance().GetCustomFieldDefinitionList(TabModuleID));
		}

		public void UpdateCustomFieldDefinition(Gafware.Modules.Reservations.CustomFieldDefinitionInfo CustomFieldDefinitionInfo)
		{
			DataProvider.Instance().UpdateCustomFieldDefinition(CustomFieldDefinitionInfo.TabModuleID, CustomFieldDefinitionInfo.CustomFieldDefinitionID, CustomFieldDefinitionInfo.Name, CustomFieldDefinitionInfo.Label, CustomFieldDefinitionInfo.Type, CustomFieldDefinitionInfo.OptionType, CustomFieldDefinitionInfo.Title, CustomFieldDefinitionInfo.AddToPreviousRow, CustomFieldDefinitionInfo.IsRequired, CustomFieldDefinitionInfo.IsActive, CustomFieldDefinitionInfo.DefaultValue, CustomFieldDefinitionInfo.MaxLength, CustomFieldDefinitionInfo.SortOrder, CustomFieldDefinitionInfo.CreatedByUserID, CustomFieldDefinitionInfo.CreatedOnDate, CustomFieldDefinitionInfo.LastModifiedByUserID, CustomFieldDefinitionInfo.LastModifiedOnDate, CustomFieldDefinitionInfo.HideLabel);
		}
	}
}