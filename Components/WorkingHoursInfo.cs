using System;
using System.Xml.Serialization;

namespace Gafware.Modules.Reservations
{
	public class WorkingHoursInfo
	{
		private System.DayOfWeek _DayOfWeek;

		private TimeSpan _StartTime;

		private TimeSpan _EndTime;

		private bool _AllDay;

		public bool AllDay
		{
			get
			{
				return this._AllDay;
			}
			set
			{
				this._AllDay = value;
			}
		}

		public System.DayOfWeek DayOfWeek
		{
			get
			{
				return this._DayOfWeek;
			}
			set
			{
				this._DayOfWeek = value;
			}
		}

		[XmlIgnore]
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

		[XmlIgnore]
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

		[XmlElement("EndTime", DataType="duration")]
		public string XmlEndTime
		{
			get
			{
				return this._EndTime.ToString();
			}
			set
			{
				this._EndTime = TimeSpan.Parse(value);
			}
		}

		[XmlElement("StartTime", DataType="duration")]
		public string XmlStartTime
		{
			get
			{
				return this._StartTime.ToString();
			}
			set
			{
				this._StartTime = TimeSpan.Parse(value);
			}
		}

		public WorkingHoursInfo()
		{
		}
	}
}