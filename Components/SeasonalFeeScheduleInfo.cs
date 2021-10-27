using System;
using System.Runtime.CompilerServices;

namespace Gafware.Modules.Reservations
{
	public class SeasonalFeeScheduleInfo : FlatFeeScheduleInfo
	{
		public int EndByDay
		{
			get;
			set;
		}

		public int EndByMonth
		{
			get;
			set;
		}

		public int StartOnDay
		{
			get;
			set;
		}

		public int StartOnMonth
		{
			get;
			set;
		}

		public SeasonalFeeScheduleInfo()
		{
		}
	}
}