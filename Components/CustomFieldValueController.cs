using DotNetNuke.Common.Utilities;
using Gafware.Modules.Reservations.Data;
using System;
using System.Collections.Generic;

namespace Gafware.Modules.Reservations
{
	public class CustomFieldValueController
	{
		public CustomFieldValueController()
		{
		}

		public Gafware.Modules.Reservations.CustomFieldValueInfo AddCustomFieldValue(Gafware.Modules.Reservations.CustomFieldValueInfo CustomFieldValueInfo)
		{
			CustomFieldValueInfo.CustomFieldValueID = DataProvider.Instance().AddCustomFieldValue(CustomFieldValueInfo.CustomFieldDefinitionID, CustomFieldValueInfo.ReservationID, CustomFieldValueInfo.PendingPaymentID, CustomFieldValueInfo.PendingApprovalID, CustomFieldValueInfo.Value, CustomFieldValueInfo.CreatedByUserID, CustomFieldValueInfo.CreatedOnDate, CustomFieldValueInfo.LastModifiedByUserID, CustomFieldValueInfo.LastModifiedOnDate);
			return CustomFieldValueInfo;
		}

		public void DeleteCustomFieldValue(int CustomFieldValueID)
		{
			DataProvider.Instance().DeleteCustomFieldValue(CustomFieldValueID);
		}

		public CustomFieldValueInfo GetCustomFieldValue(int CustomFieldValueID)
		{
			return (CustomFieldValueInfo)CBO.FillObject<CustomFieldValueInfo>(DataProvider.Instance().GetCustomFieldValue(CustomFieldValueID));
		}

		public List<CustomFieldValueInfo> GetCustomFieldValueList(int CustomFieldDefinitionID)
		{
			return CBO.FillCollection<CustomFieldValueInfo>(DataProvider.Instance().GetCustomFieldValueList(CustomFieldDefinitionID));
		}

		public List<CustomFieldValueInfo> GetCustomFieldValueListByPendingApprovalID(int PendingApprovalID)
		{
			return CBO.FillCollection<CustomFieldValueInfo>(DataProvider.Instance().GetCustomFieldValueListByPendingApprovalID(PendingApprovalID));
		}

		public List<CustomFieldValueInfo> GetCustomFieldValueListByPendingPaymentID(int PendingPaymentID)
		{
			return CBO.FillCollection<CustomFieldValueInfo>(DataProvider.Instance().GetCustomFieldValueListByPendingPaymentID(PendingPaymentID));
		}

		public List<CustomFieldValueInfo> GetCustomFieldValueListByReservationID(int ReservationID)
		{
			return CBO.FillCollection<CustomFieldValueInfo>(DataProvider.Instance().GetCustomFieldValueListByReservationID(ReservationID));
		}

		public void UpdateCustomFieldValue(Gafware.Modules.Reservations.CustomFieldValueInfo CustomFieldValueInfo)
		{
			DataProvider.Instance().UpdateCustomFieldValue(CustomFieldValueInfo.CustomFieldDefinitionID, CustomFieldValueInfo.CustomFieldValueID, CustomFieldValueInfo.ReservationID, CustomFieldValueInfo.PendingPaymentID, CustomFieldValueInfo.PendingApprovalID, CustomFieldValueInfo.Value, CustomFieldValueInfo.CreatedByUserID, CustomFieldValueInfo.CreatedOnDate, CustomFieldValueInfo.LastModifiedByUserID, CustomFieldValueInfo.LastModifiedOnDate);
		}
	}
}