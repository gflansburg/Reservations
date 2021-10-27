using System;
using System.Collections;

namespace Gafware.Modules.Reservations
{
	internal class WorkingHourInfoComparer : IComparer
	{
		public WorkingHourInfoComparer()
		{
		}

		public int Compare(object x, object y)
		{
			int dayOfWeek = (int)((WorkingHoursInfo)x).DayOfWeek;
			int num = dayOfWeek.CompareTo((int)((WorkingHoursInfo)y).DayOfWeek);
			if (num != 0)
			{
				return num;
			}
			return ((WorkingHoursInfo)x).StartTime.CompareTo(((WorkingHoursInfo)y).StartTime);
		}
	}
}