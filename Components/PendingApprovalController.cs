using DotNetNuke.Common.Utilities;
using Gafware.Modules.Reservations.Data;
using System;
using System.Collections.Generic;

namespace Gafware.Modules.Reservations
{
	public class PendingApprovalController
	{
		public PendingApprovalController()
		{
		}

		public Gafware.Modules.Reservations.PendingApprovalInfo AddPendingApproval(Gafware.Modules.Reservations.PendingApprovalInfo PendingApprovalInfo)
		{
			PendingApprovalInfo.PendingApprovalID = DataProvider.Instance().AddPendingApproval(PendingApprovalInfo.TabModuleID, PendingApprovalInfo.PortalID, PendingApprovalInfo.ReservationID, PendingApprovalInfo.CategoryID, PendingApprovalInfo.StartDateTime, PendingApprovalInfo.Duration, PendingApprovalInfo.FirstName, PendingApprovalInfo.LastName, PendingApprovalInfo.Email, PendingApprovalInfo.Phone, PendingApprovalInfo.Description, PendingApprovalInfo.Status, PendingApprovalInfo.CreatedByUserID, PendingApprovalInfo.CreatedOnDate, PendingApprovalInfo.LastModifiedByUserID, PendingApprovalInfo.LastModifiedOnDate);
			return PendingApprovalInfo;
		}

		public void DeletePendingApproval(int PendingApprovalID)
		{
			DataProvider.Instance().DeletePendingApproval(PendingApprovalID);
		}

		public PendingApprovalInfo GetPendingApproval(int PendingApprovalID)
		{
			return (PendingApprovalInfo)CBO.FillObject(DataProvider.Instance().GetPendingApproval(PendingApprovalID), typeof(PendingApprovalInfo));
		}

		public List<PendingApprovalInfo> GetPendingApprovalList(int TabModuleID)
		{
			return CBO.FillCollection<PendingApprovalInfo>(DataProvider.Instance().GetPendingApprovalList(TabModuleID));
		}

		public void UpdatePendingApproval(Gafware.Modules.Reservations.PendingApprovalInfo PendingApprovalInfo)
		{
			DataProvider.Instance().UpdatePendingApproval(PendingApprovalInfo.TabModuleID, PendingApprovalInfo.PendingApprovalID, PendingApprovalInfo.PortalID, PendingApprovalInfo.ReservationID, PendingApprovalInfo.CategoryID, PendingApprovalInfo.StartDateTime, PendingApprovalInfo.Duration, PendingApprovalInfo.FirstName, PendingApprovalInfo.LastName, PendingApprovalInfo.Email, PendingApprovalInfo.Phone, PendingApprovalInfo.Description, PendingApprovalInfo.Status, PendingApprovalInfo.CreatedByUserID, PendingApprovalInfo.CreatedOnDate, PendingApprovalInfo.LastModifiedByUserID, PendingApprovalInfo.LastModifiedOnDate);
		}
	}
}