using System;

namespace Gafware.Modules.Reservations
{
	public class TimeOfDayInfo : IComparable
	{
		private string _Name;

		private TimeSpan _StartTime;

		private TimeSpan _EndTime;

		public TimeSpan EndTime
		{
			get
			{
				return this._EndTime;
			}
			set
			{
				this._EndTime = value;
			}
		}

		public string Name
		{
			get
			{
				return this._Name;
			}
			set
			{
				this._Name = value;
			}
		}

		public TimeSpan StartTime
		{
			get
			{
				return this._StartTime;
			}
			set
			{
				this._StartTime = value;
			}
		}

		public TimeOfDayInfo()
		{
		}

		public int CompareTo(object obj)
		{
			TimeOfDayInfo timeOfDayInfo = (TimeOfDayInfo)obj;
			return this.StartTime.CompareTo(timeOfDayInfo.StartTime);
		}
	}
}