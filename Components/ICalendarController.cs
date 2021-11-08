using DotNetNuke.Common.Utilities;
using Gafware.Modules.Reservations.Data;
using System;
using System.Collections.Generic;

namespace Gafware.Modules.Reservations
{
	public class ICalendarController
	{
		public ICalendarController()
		{
		}

		public Gafware.Modules.Reservations.ICalendarInfo AddICalendar(Gafware.Modules.Reservations.ICalendarInfo ICalendarInfo)
		{
			DataProvider.Instance().AddICalendar(ICalendarInfo.ReservationID, ICalendarInfo.UID, ICalendarInfo.Sequence, ICalendarInfo.Organizer);
			return ICalendarInfo;
		}

		public void DeleteICalendar(int ReservationID)
		{
			DataProvider.Instance().DeleteICalendar(ReservationID);
		}

		public ICalendarInfo GetICalendar(int ReservationID)
		{
			return (ICalendarInfo)CBO.FillObject<ICalendarInfo>(DataProvider.Instance().GetICalendar(ReservationID));
		}

		public List<ICalendarInfo> GetICalendarList(int ReservationID)
		{
			return CBO.FillCollection<ICalendarInfo>(DataProvider.Instance().GetICalendarList(ReservationID));
		}

		public void UpdateICalendar(Gafware.Modules.Reservations.ICalendarInfo ICalendarInfo)
		{
			DataProvider.Instance().UpdateICalendar(ICalendarInfo.ReservationID, ICalendarInfo.UID, ICalendarInfo.Sequence, ICalendarInfo.Organizer);
		}
	}
}