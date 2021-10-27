using DotNetNuke.Common.Utilities;
using Gafware.Modules.Reservations.Data;
using System;
using System.Collections.Generic;

namespace Gafware.Modules.Reservations
{
	public class ReservationController
	{
		public ReservationController()
		{
		}

		public Gafware.Modules.Reservations.ReservationInfo AddReservation(Gafware.Modules.Reservations.ReservationInfo ReservationInfo)
		{
			ReservationInfo.ReservationID = DataProvider.Instance().AddReservation(ReservationInfo.TabModuleID, ReservationInfo.CategoryID, ReservationInfo.StartDateTime, ReservationInfo.Duration, ReservationInfo.FirstName, ReservationInfo.LastName, ReservationInfo.Email, ReservationInfo.Phone, ReservationInfo.Description, ReservationInfo.SendReminder, ReservationInfo.SendReminderWhen, ReservationInfo.ReminderSent, ReservationInfo.RequireConfirmation, ReservationInfo.RequireConfirmationWhen, ReservationInfo.Confirmed, ReservationInfo.CreatedByUserID, ReservationInfo.CreatedOnDate, ReservationInfo.LastModifiedByUserID, ReservationInfo.LastModifiedOnDate, ReservationInfo.SendReminderVia);
			return ReservationInfo;
		}

		public void DeleteReservation(int ReservationID)
		{
			DataProvider.Instance().DeleteReservation(ReservationID);
		}

		public ReservationInfo GetReservation(int ReservationID)
		{
			return (ReservationInfo)CBO.FillObject(DataProvider.Instance().GetReservation(ReservationID), typeof(ReservationInfo));
		}

		public List<ReservationInfo> GetReservationList(int TabModuleID)
		{
			return CBO.FillCollection<ReservationInfo>(DataProvider.Instance().GetReservationList(TabModuleID));
		}

		public List<ReservationInfo> GetReservationListByDateRange(int TabModuleID, DateTime from, DateTime to)
		{
			return CBO.FillCollection<ReservationInfo>(DataProvider.Instance().GetReservationListByDateRange(TabModuleID, from, to));
		}

		public List<ReservationInfo> GetReservationListByDateRangeAndCategoryID(int TabModuleID, DateTime From, DateTime To, int CategoryID)
		{
			return CBO.FillCollection<ReservationInfo>(DataProvider.Instance().GetReservationListByDateRangeAndCategoryID(TabModuleID, From, To, CategoryID));
		}

		public List<ReservationInfo> GetReservationListToSendReminders()
		{
			return CBO.FillCollection<ReservationInfo>(DataProvider.Instance().GetReservationListToSendReminders());
		}

		public ReservationInfo SaveReservation(ReservationInfo reservationInfo)
		{
			if (!Null.IsNull(reservationInfo.ReservationID))
			{
				this.UpdateReservation(reservationInfo);
			}
			else
			{
				reservationInfo = this.AddReservation(reservationInfo);
			}
			return reservationInfo;
		}

		public void UpdateReservation(Gafware.Modules.Reservations.ReservationInfo ReservationInfo)
		{
			DataProvider.Instance().UpdateReservation(ReservationInfo.TabModuleID, ReservationInfo.ReservationID, ReservationInfo.CategoryID, ReservationInfo.StartDateTime, ReservationInfo.Duration, ReservationInfo.FirstName, ReservationInfo.LastName, ReservationInfo.Email, ReservationInfo.Phone, ReservationInfo.Description, ReservationInfo.SendReminder, ReservationInfo.SendReminderWhen, ReservationInfo.ReminderSent, ReservationInfo.RequireConfirmation, ReservationInfo.RequireConfirmationWhen, ReservationInfo.Confirmed, ReservationInfo.CreatedByUserID, ReservationInfo.CreatedOnDate, ReservationInfo.LastModifiedByUserID, ReservationInfo.LastModifiedOnDate, ReservationInfo.SendReminderVia);
		}
	}
}