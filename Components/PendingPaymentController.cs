using DotNetNuke.Common.Utilities;
using Gafware.Modules.Reservations.Data;
using System;
using System.Collections.Generic;

namespace Gafware.Modules.Reservations
{
	public class PendingPaymentController
	{
		public PendingPaymentController()
		{
		}

		public Gafware.Modules.Reservations.PendingPaymentInfo AddPendingPayment(Gafware.Modules.Reservations.PendingPaymentInfo PendingPaymentInfo)
		{
			PendingPaymentInfo.PendingPaymentID = DataProvider.Instance().AddPendingPayment(PendingPaymentInfo.TabModuleID, PendingPaymentInfo.PortalID, PendingPaymentInfo.ReservationID, PendingPaymentInfo.PendingApprovalID, PendingPaymentInfo.CategoryID, PendingPaymentInfo.StartDateTime, PendingPaymentInfo.Duration, PendingPaymentInfo.FirstName, PendingPaymentInfo.LastName, PendingPaymentInfo.Email, PendingPaymentInfo.Phone, PendingPaymentInfo.Description, PendingPaymentInfo.Amount, PendingPaymentInfo.RefundableAmount, PendingPaymentInfo.Currency, PendingPaymentInfo.Status, PendingPaymentInfo.CreatedByUserID, PendingPaymentInfo.CreatedOnDate, PendingPaymentInfo.LastModifiedByUserID, PendingPaymentInfo.LastModifiedOnDate);
			return PendingPaymentInfo;
		}

		public void DeletePendingPayment(int PendingPaymentID)
		{
			DataProvider.Instance().DeletePendingPayment(PendingPaymentID);
		}

		public PendingPaymentInfo GetPendingPayment(int PendingPaymentID)
		{
			return (PendingPaymentInfo)CBO.FillObject(DataProvider.Instance().GetPendingPayment(PendingPaymentID), typeof(PendingPaymentInfo));
		}

		public List<PendingPaymentInfo> GetPendingPaymentList(int TabModuleID)
		{
			return CBO.FillCollection<PendingPaymentInfo>(DataProvider.Instance().GetPendingPaymentList(TabModuleID));
		}

		public void UpdatePendingPayment(Gafware.Modules.Reservations.PendingPaymentInfo PendingPaymentInfo)
		{
			DataProvider.Instance().UpdatePendingPayment(PendingPaymentInfo.TabModuleID, PendingPaymentInfo.PendingPaymentID, PendingPaymentInfo.PortalID, PendingPaymentInfo.ReservationID, PendingPaymentInfo.PendingApprovalID, PendingPaymentInfo.CategoryID, PendingPaymentInfo.StartDateTime, PendingPaymentInfo.Duration, PendingPaymentInfo.FirstName, PendingPaymentInfo.LastName, PendingPaymentInfo.Email, PendingPaymentInfo.Phone, PendingPaymentInfo.Description, PendingPaymentInfo.Amount, PendingPaymentInfo.RefundableAmount, PendingPaymentInfo.Currency, PendingPaymentInfo.Status, PendingPaymentInfo.CreatedByUserID, PendingPaymentInfo.CreatedOnDate, PendingPaymentInfo.LastModifiedByUserID, PendingPaymentInfo.LastModifiedOnDate);
		}
	}
}