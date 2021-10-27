using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace Gafware.Modules.Reservations
{
	public class PaymentNotificationHelper
	{
		public PaymentNotificationHelper()
		{
		}

		public static void ProcessPaymentNotification(ModuleSettings moduleSettings, string localResourceFile, PendingPaymentInfo pendingPaymentInfo, PendingPaymentStatus paymentNotificationStatus, decimal amount)
		{
			if (paymentNotificationStatus != PendingPaymentStatus.Paid || pendingPaymentInfo.Status != 0 && pendingPaymentInfo.Status != 4 && pendingPaymentInfo.Status != 7)
			{
				if (paymentNotificationStatus == PendingPaymentStatus.Held)
				{
					if (pendingPaymentInfo.Status != 7)
					{
						pendingPaymentInfo.Status = 4;
						pendingPaymentInfo.LastModifiedOnDate = DateTime.Now;
						(new PendingPaymentController()).UpdatePendingPayment(pendingPaymentInfo);
						return;
					}
				}
				else if (paymentNotificationStatus == PendingPaymentStatus.Void)
				{
					if (pendingPaymentInfo.Status != 7)
					{
						pendingPaymentInfo.Status = 2;
						pendingPaymentInfo.LastModifiedOnDate = DateTime.Now;
						(new PendingPaymentController()).UpdatePendingPayment(pendingPaymentInfo);
						return;
					}
				}
				else if (paymentNotificationStatus == PendingPaymentStatus.Refunded)
				{
					PendingPaymentInfo now = Helper.FindPendingPaymentInfoByEventIDAndStatus((new PendingPaymentController()).GetPendingPaymentList(pendingPaymentInfo.TabModuleID), pendingPaymentInfo.ReservationID, 5);
					if (now != null)
					{
						if (now.Amount != amount)
						{
							decimal num = amount - now.Amount;
							if (num > decimal.Zero)
							{
								PendingPaymentInfo minusOne = Helper.CreatePendingPaymentInfoFromPendingPaymentInfo(now);
								minusOne.Amount = num * decimal.MinusOne;
								minusOne.RefundableAmount = decimal.Zero;
								minusOne.Status = 5;
								minusOne.CreatedOnDate = DateTime.Now;
								(new PendingPaymentController()).AddPendingPayment(minusOne);
								PendingPaymentInfo pendingPaymentInfo1 = now;
								pendingPaymentInfo1.Amount = pendingPaymentInfo1.Amount + num;
							}
						}
						now.Status = 6;
						now.LastModifiedOnDate = DateTime.Now;
						(new PendingPaymentController()).UpdatePendingPayment(now);
					}
				}
				return;
			}
			if (pendingPaymentInfo.Status != 7 && !(amount >= pendingPaymentInfo.Amount))
			{
				pendingPaymentInfo.Status = 2;
				pendingPaymentInfo.LastModifiedOnDate = DateTime.Now;
				(new PendingPaymentController()).UpdatePendingPayment(pendingPaymentInfo);
				return;
			}
			ReservationController reservationController = new ReservationController();
			PortalInfo portal = (new PortalController()).GetPortal(pendingPaymentInfo.PortalID);
			Thread.CurrentThread.CurrentCulture = new CultureInfo(portal.DefaultLanguage);
			Helper helper = new Helper(pendingPaymentInfo.PortalID, pendingPaymentInfo.TabModuleID, localResourceFile);
			bool reservationID = pendingPaymentInfo.ReservationID == Null.NullInteger;
			if (pendingPaymentInfo.Status == 0 || pendingPaymentInfo.Status == 4)
			{
				bool moderate = moduleSettings.Moderate;
				if (pendingPaymentInfo.CategoryID == Null.NullInteger || pendingPaymentInfo.CategoryID == 0)
				{
					moderate = (!moderate ? false : helper.MustModerate(null, moduleSettings.Settings, pendingPaymentInfo.StartDateTime, pendingPaymentInfo.Duration));
				}
				else
				{
					CategorySettings categorySetting = new CategorySettings(portal.PortalID, pendingPaymentInfo.TabModuleID, pendingPaymentInfo.CategoryID);
					moderate = (!categorySetting.Moderate ? false : helper.MustModerate(categorySetting.Settings, moduleSettings.Settings, pendingPaymentInfo.StartDateTime, pendingPaymentInfo.Duration));
				}
				if (!moderate || pendingPaymentInfo.CreatedByUserID != Null.NullInteger && helper.CanModerate(pendingPaymentInfo.CreatedByUserID, pendingPaymentInfo.CategoryID))
				{
					ReservationInfo reservationInfo = (reservationID ? new ReservationInfo() : reservationController.GetReservation(pendingPaymentInfo.ReservationID));
					Helper.SetEventInfoPropertiesFromPendingPaymentInfo(reservationInfo, pendingPaymentInfo, moduleSettings, (pendingPaymentInfo.CreatedByUserID == Null.NullInteger ? portal.AdministratorId : pendingPaymentInfo.CreatedByUserID), (pendingPaymentInfo.CreatedByUserID == Null.NullInteger ? (new UserController()).GetUser(portal.PortalID, portal.AdministratorId).DisplayName : pendingPaymentInfo.CreatedByDisplayName));
					reservationInfo = reservationController.SaveReservation(reservationInfo);
					if (reservationID && Helper.GetEdition(moduleSettings.ActivationCode) != "Standard")
					{
						CustomFieldValueController customFieldValueController = new CustomFieldValueController();
						foreach (CustomFieldValueInfo customFieldValueListByPendingPaymentID in (new CustomFieldValueController()).GetCustomFieldValueListByPendingPaymentID(pendingPaymentInfo.PendingPaymentID))
						{
							customFieldValueListByPendingPaymentID.ReservationID = reservationInfo.ReservationID;
							customFieldValueListByPendingPaymentID.LastModifiedByUserID = pendingPaymentInfo.CreatedByUserID;
							customFieldValueListByPendingPaymentID.LastModifiedOnDate = DateTime.Now;
							customFieldValueController.UpdateCustomFieldValue(customFieldValueListByPendingPaymentID);
						}
					}
					if (!reservationID)
					{
						Helper.UpdateDueAndPendingRefundPaymentInfoFromEventInfo(reservationInfo, pendingPaymentInfo.TabModuleID);
					}
					pendingPaymentInfo.ReservationID = reservationInfo.ReservationID;
					if (!reservationID)
					{
						helper.SendRescheduledMail(reservationInfo);
					}
					else
					{
						helper.SendConfirmationMail(reservationInfo);
					}
				}
				else
				{
					PendingApprovalInfo pendingApprovalInfo = (new PendingApprovalController()).AddPendingApproval(Helper.CreatePendingApprovalInfoInfoFromPendingPaymentInfo(pendingPaymentInfo, PendingApprovalStatus.Pending));
					pendingPaymentInfo.PendingApprovalID = pendingApprovalInfo.PendingApprovalID;
					if (reservationID && Helper.GetEdition(moduleSettings.ActivationCode) != "Standard")
					{
						CustomFieldValueController customFieldValueController1 = new CustomFieldValueController();
						foreach (CustomFieldValueInfo pendingApprovalID in (new CustomFieldValueController()).GetCustomFieldValueListByPendingPaymentID(pendingPaymentInfo.PendingPaymentID))
						{
							pendingApprovalID.PendingApprovalID = pendingApprovalInfo.PendingApprovalID;
							pendingApprovalID.LastModifiedByUserID = pendingPaymentInfo.CreatedByUserID;
							pendingApprovalID.LastModifiedOnDate = DateTime.Now;
							customFieldValueController1.UpdateCustomFieldValue(pendingApprovalID);
						}
					}
					helper.SendModeratorMail(pendingApprovalInfo);
				}
				if (amount > pendingPaymentInfo.Amount)
				{
					decimal num1 = amount - pendingPaymentInfo.Amount;
					if (!reservationID)
					{
						PendingPaymentInfo zero = Helper.FindPendingPaymentInfoByEventIDAndStatus((new PendingPaymentController()).GetPendingPaymentList(pendingPaymentInfo.TabModuleID), pendingPaymentInfo.ReservationID, 7);
						if (zero != null)
						{
							decimal num2 = zero.Amount;
							if (num2 > num1)
							{
								zero.Amount = num1;
								zero.LastModifiedOnDate = DateTime.Now;
								zero.Status = 1;
								(new PendingPaymentController()).UpdatePendingPayment(zero);
								zero = Helper.CreatePendingPaymentInfoFromPendingPaymentInfo(zero);
								zero.Amount = num2 - num1;
								zero.RefundableAmount = decimal.Zero;
								zero.CreatedOnDate = DateTime.Now;
								zero.Status = 7;
								(new PendingPaymentController()).AddPendingPayment(zero);
							}
							else
							{
								zero.LastModifiedOnDate = DateTime.Now;
								zero.Status = 1;
								(new PendingPaymentController()).UpdatePendingPayment(zero);
							}
							num1 = num1 - num2;
						}
					}
					if (num1 > decimal.Zero)
					{
						ReservationInfo reservation = reservationController.GetReservation(pendingPaymentInfo.ReservationID);
						if (reservation != null)
						{
							Helper.AddOrUpdatePendingPaymentInfo(reservation, pendingPaymentInfo.TabModuleID, pendingPaymentInfo.PortalID, num1 * decimal.MinusOne, num1 * decimal.MinusOne, moduleSettings.Currency, pendingPaymentInfo.CreatedByUserID, 5);
						}
					}
				}
			}
			else if (pendingPaymentInfo.Status == 7)
			{
				if (amount > pendingPaymentInfo.Amount)
				{
					ReservationInfo reservation1 = reservationController.GetReservation(pendingPaymentInfo.ReservationID);
					if (reservation1 != null)
					{
						Helper.AddOrUpdatePendingPaymentInfo(reservation1, pendingPaymentInfo.TabModuleID, pendingPaymentInfo.PortalID, (amount - pendingPaymentInfo.Amount) * decimal.MinusOne, decimal.Zero, moduleSettings.Currency, pendingPaymentInfo.CreatedByUserID, 5);
					}
				}
				else if (amount < pendingPaymentInfo.Amount)
				{
					PendingPaymentInfo pendingPaymentInfo2 = pendingPaymentInfo;
					pendingPaymentInfo2.Amount = pendingPaymentInfo2.Amount - amount;
					PendingPaymentInfo zero1 = Helper.CreatePendingPaymentInfoFromPendingPaymentInfo(pendingPaymentInfo);
					zero1.Amount = amount;
					zero1.RefundableAmount = decimal.Zero;
					zero1.CreatedOnDate = DateTime.Now;
					zero1.Status = 7;
					(new PendingPaymentController()).AddPendingPayment(zero1);
				}
			}
			pendingPaymentInfo.LastModifiedOnDate = DateTime.Now;
			pendingPaymentInfo.Status = 1;
			(new PendingPaymentController()).UpdatePendingPayment(pendingPaymentInfo);
		}
	}
}