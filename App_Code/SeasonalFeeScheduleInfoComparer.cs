using System;
using System.Collections.Generic;

namespace Gafware.Modules.Reservations
{
	public class SeasonalFeeScheduleInfoComparer : IComparer<SeasonalFeeScheduleInfo>
	{
		public SeasonalFeeScheduleInfoComparer()
		{
		}

		public int Compare(SeasonalFeeScheduleInfo x, SeasonalFeeScheduleInfo y)
		{
			DateTime dateTime = new DateTime(2000, x.StartOnMonth, x.StartOnDay);
			return dateTime.CompareTo(new DateTime(2000, y.StartOnMonth, y.StartOnDay));
		}
	}
}