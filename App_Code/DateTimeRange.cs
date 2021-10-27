using System;

namespace Gafware.Modules.Reservations
{
	public class DateTimeRange : IComparable
	{
		private DateTime _StartDateTime;

		private DateTime _EndDateTime;

		public DateTime EndDateTime
		{
			get
			{
				return this._EndDateTime;
			}
			set
			{
				this._EndDateTime = value;
			}
		}

		public DateTime StartDateTime
		{
			get
			{
				return this._StartDateTime;
			}
			set
			{
				this._StartDateTime = value;
			}
		}

		public DateTimeRange()
		{
		}

		public int CompareTo(object obj)
		{
			if (!(obj is DateTimeRange))
			{
				throw new NotImplementedException();
			}
			return this.StartDateTime.CompareTo(((DateTimeRange)obj).StartDateTime);
		}
	}
}