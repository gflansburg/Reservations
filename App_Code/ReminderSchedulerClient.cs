using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Scheduling;
using System;
using System.Collections.Generic;
using DotNetNuke.Abstractions;

namespace Gafware.Modules.Reservations
{
	public class ReminderSchedulerClient : SchedulerClient
	{
		public ReminderSchedulerClient(ScheduleHistoryItem scheduleHistoryItem)
		{
			base.ScheduleHistoryItem = scheduleHistoryItem;
		}

		public override void DoWork()
		{
			try
			{
				base.Progressing();
				string str = this.SendReminders();
				base.ScheduleHistoryItem.AddLogNote(str);
				base.ScheduleHistoryItem.Succeeded = true;
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				base.ScheduleHistoryItem.Succeeded = false;
				base.ScheduleHistoryItem.AddLogNote(string.Concat("Failed: ", exception.ToString()));
				base.Errored(ref exception);
				Exceptions.LogException(exception);
			}
		}

		public static void Install()
		{
			ScheduleItem scheduleItem = new ScheduleItem();
			scheduleItem = SchedulingProvider.Instance().GetSchedule("Gafware.Modules.Reservations.ReminderSchedulerClient, Gafware.Modules.Reservations", null);
			if (scheduleItem == null)
			{
				scheduleItem = new ScheduleItem();
				scheduleItem.TypeFullName = "Gafware.Modules.Reservations.ReminderSchedulerClient, Gafware.Modules.Reservations";
				scheduleItem.TimeLapse = 1;
				scheduleItem.TimeLapseMeasurement = "h";
				scheduleItem.RetryTimeLapse = 30;
				scheduleItem.RetryTimeLapseMeasurement = "m";
				scheduleItem.RetainHistoryNum = 10;
				scheduleItem.Enabled = true;
				scheduleItem.ObjectDependencies = "";
				scheduleItem.FriendlyName = "Send Reservations Reminders";
				SchedulingProvider.Instance().AddSchedule(scheduleItem);
			}
		}

		private string SendReminders()
		{
			bool? nullable = null;
			int num = 0;
			string empty = string.Empty;
			List<ReservationInfo> reservationListToSendReminders = (new ReservationController()).GetReservationListToSendReminders();
			Dictionary<int, ModuleSettings> nums = new Dictionary<int, ModuleSettings>();
			Dictionary<int, Helper> nums1 = new Dictionary<int, Helper>();
			ReservationController reservationController = new ReservationController();
			foreach (ReservationInfo reservationListToSendReminder in reservationListToSendReminders)
			{
				if (!nums.ContainsKey(reservationListToSendReminder.TabModuleID))
				{
					ModuleInfo tabModule = (new ModuleController()).GetTabModule(reservationListToSendReminder.TabModuleID);
					if (tabModule != null && !tabModule.IsDeleted)
					{
						nums.Add(reservationListToSendReminder.TabModuleID, new ModuleSettings(tabModule.PortalID, tabModule.TabModuleID));
						nums1.Add(reservationListToSendReminder.TabModuleID, new Helper(tabModule.PortalID, reservationListToSendReminder.TabModuleID, string.Concat(new string[] { Globals.ApplicationPath, "/DesktopModules/", tabModule.DesktopModule.FolderName, "/", Localization.LocalResourceDirectory, "/SharedResources.resx" })));
					}
				}
				ModuleSettings item = nums[reservationListToSendReminder.TabModuleID];
				if (!nullable.HasValue)
				{
					nullable = new bool?(Helper.GetEdition(item.ActivationCode) != "Standard");
				}
				if (item == null)
				{
					continue;
				}
				DateTime startDateTime = reservationListToSendReminder.StartDateTime;
				DateTime dateTime = startDateTime.AddMinutes((double)(-1 * reservationListToSendReminder.SendReminderWhen));
				if (dateTime.Subtract(item.TimeZone.GetUtcOffset(dateTime)) > DateTime.Now.ToUniversalTime())
				{
					continue;
				}
				if (reservationListToSendReminder.SendReminderVia == 1 || reservationListToSendReminder.SendReminderVia == 3)
				{
					nums1[reservationListToSendReminder.TabModuleID].SendReminderMail(reservationListToSendReminder);
				}
				if (nullable.Value && (reservationListToSendReminder.SendReminderVia == 2 || reservationListToSendReminder.SendReminderVia == 3))
				{
					nums1[reservationListToSendReminder.TabModuleID].SendReminderSMS(reservationListToSendReminder);
				}
				reservationListToSendReminder.ReminderSent = true;
				reservationListToSendReminder.LastModifiedOnDate = DateTime.Now;
				reservationController.UpdateReservation(reservationListToSendReminder);
				num++;
			}
			return string.Concat(" - ", num, " reminder(s) sent.");
		}
	}
}