using System;
using System.Xml.Serialization;

namespace Gafware.Modules.Reservations
{
	public class RecurrencePattern : IRecurrencePattern
	{
		private DateTime _StartDate;

		private TimeSpan _StartTime;

		private TimeSpan _Duration;

		private DateTime? _EndDate;

		private Gafware.Modules.Reservations.Pattern _Pattern;

		private int? _Every;

		private bool _EveryWeekDay;

		private bool _Monday;

		private bool _Tuesday;

		private bool _Wednesday;

		private bool _Thursday;

		private bool _Friday;

		private bool _Saturday;

		private bool _Sunday;

		private Gafware.Modules.Reservations.DayPosition? _DayPosition;

		private Gafware.Modules.Reservations.DayType? _DayType;

		private int? _Day;

		private int? _Month;

		private int? _EndAfter;

		public int? Day
		{
			get
			{
				return JustDecompileGenerated_get_Day();
			}
			set
			{
				JustDecompileGenerated_set_Day(value);
			}
		}

		public int? JustDecompileGenerated_get_Day()
		{
			return this._Day;
		}

		public void JustDecompileGenerated_set_Day(int? value)
		{
			this._Day = value;
		}

		public Gafware.Modules.Reservations.DayPosition? DayPosition
		{
			get
			{
				return JustDecompileGenerated_get_DayPosition();
			}
			set
			{
				JustDecompileGenerated_set_DayPosition(value);
			}
		}

		public Gafware.Modules.Reservations.DayPosition? JustDecompileGenerated_get_DayPosition()
		{
			return this._DayPosition;
		}

		public void JustDecompileGenerated_set_DayPosition(Gafware.Modules.Reservations.DayPosition? value)
		{
			this._DayPosition = value;
		}

		public Gafware.Modules.Reservations.DayType? DayType
		{
			get
			{
				return JustDecompileGenerated_get_DayType();
			}
			set
			{
				JustDecompileGenerated_set_DayType(value);
			}
		}

		public Gafware.Modules.Reservations.DayType? JustDecompileGenerated_get_DayType()
		{
			return this._DayType;
		}

		public void JustDecompileGenerated_set_DayType(Gafware.Modules.Reservations.DayType? value)
		{
			this._DayType = value;
		}

		[XmlIgnore]
		public TimeSpan Duration
		{
			get
			{
				return JustDecompileGenerated_get_Duration();
			}
			set
			{
				JustDecompileGenerated_set_Duration(value);
			}
		}

		public TimeSpan JustDecompileGenerated_get_Duration()
		{
			return this._Duration;
		}

		public void JustDecompileGenerated_set_Duration(TimeSpan value)
		{
			this._Duration = value;
		}

		[XmlElement("Duration")]
		public long DurationTicks
		{
			get
			{
				return this.Duration.Ticks;
			}
			set
			{
				this.Duration = new TimeSpan(value);
			}
		}

		public int? EndAfter
		{
			get
			{
				return JustDecompileGenerated_get_EndAfter();
			}
			set
			{
				JustDecompileGenerated_set_EndAfter(value);
			}
		}

		public int? JustDecompileGenerated_get_EndAfter()
		{
			return this._EndAfter;
		}

		public void JustDecompileGenerated_set_EndAfter(int? value)
		{
			this._EndAfter = value;
		}

		public DateTime? EndDate
		{
			get
			{
				return JustDecompileGenerated_get_EndDate();
			}
			set
			{
				JustDecompileGenerated_set_EndDate(value);
			}
		}

		public DateTime? JustDecompileGenerated_get_EndDate()
		{
			return this._EndDate;
		}

		public void JustDecompileGenerated_set_EndDate(DateTime? value)
		{
			this._EndDate = value;
		}

		public int? Every
		{
			get
			{
				return JustDecompileGenerated_get_Every();
			}
			set
			{
				JustDecompileGenerated_set_Every(value);
			}
		}

		public int? JustDecompileGenerated_get_Every()
		{
			return this._Every;
		}

		public void JustDecompileGenerated_set_Every(int? value)
		{
			this._Every = value;
		}

		public bool EveryWeekDay
		{
			get
			{
				return JustDecompileGenerated_get_EveryWeekDay();
			}
			set
			{
				JustDecompileGenerated_set_EveryWeekDay(value);
			}
		}

		public bool JustDecompileGenerated_get_EveryWeekDay()
		{
			return this._EveryWeekDay;
		}

		public void JustDecompileGenerated_set_EveryWeekDay(bool value)
		{
			this._EveryWeekDay = value;
		}

		public bool Friday
		{
			get
			{
				return JustDecompileGenerated_get_Friday();
			}
			set
			{
				JustDecompileGenerated_set_Friday(value);
			}
		}

		public bool JustDecompileGenerated_get_Friday()
		{
			return this._Friday;
		}

		public void JustDecompileGenerated_set_Friday(bool value)
		{
			this._Friday = value;
		}

		public bool IsAllDay
		{
			get
			{
				if (this.StartTime != new TimeSpan(0, 0, 0))
				{
					return false;
				}
				return this.Duration.TotalHours == 24;
			}
		}

		public bool Monday
		{
			get
			{
				return JustDecompileGenerated_get_Monday();
			}
			set
			{
				JustDecompileGenerated_set_Monday(value);
			}
		}

		public bool JustDecompileGenerated_get_Monday()
		{
			return this._Monday;
		}

		public void JustDecompileGenerated_set_Monday(bool value)
		{
			this._Monday = value;
		}

		public int? Month
		{
			get
			{
				return JustDecompileGenerated_get_Month();
			}
			set
			{
				JustDecompileGenerated_set_Month(value);
			}
		}

		public int? JustDecompileGenerated_get_Month()
		{
			return this._Month;
		}

		public void JustDecompileGenerated_set_Month(int? value)
		{
			this._Month = value;
		}

		public Gafware.Modules.Reservations.Pattern Pattern
		{
			get
			{
				return JustDecompileGenerated_get_Pattern();
			}
			set
			{
				JustDecompileGenerated_set_Pattern(value);
			}
		}

		public Gafware.Modules.Reservations.Pattern JustDecompileGenerated_get_Pattern()
		{
			return this._Pattern;
		}

		public void JustDecompileGenerated_set_Pattern(Gafware.Modules.Reservations.Pattern value)
		{
			this._Pattern = value;
		}

		public bool Saturday
		{
			get
			{
				return JustDecompileGenerated_get_Saturday();
			}
			set
			{
				JustDecompileGenerated_set_Saturday(value);
			}
		}

		public bool JustDecompileGenerated_get_Saturday()
		{
			return this._Saturday;
		}

		public void JustDecompileGenerated_set_Saturday(bool value)
		{
			this._Saturday = value;
		}

		public DateTime StartDate
		{
			get
			{
				return JustDecompileGenerated_get_StartDate();
			}
			set
			{
				JustDecompileGenerated_set_StartDate(value);
			}
		}

		public DateTime JustDecompileGenerated_get_StartDate()
		{
			return this._StartDate;
		}

		public void JustDecompileGenerated_set_StartDate(DateTime value)
		{
			this._StartDate = value;
		}

		[XmlIgnore]
		public TimeSpan StartTime
		{
			get
			{
				return JustDecompileGenerated_get_StartTime();
			}
			set
			{
				JustDecompileGenerated_set_StartTime(value);
			}
		}

		public TimeSpan JustDecompileGenerated_get_StartTime()
		{
			return this._StartTime;
		}

		public void JustDecompileGenerated_set_StartTime(TimeSpan value)
		{
			this._StartTime = value;
		}

		[XmlElement("StartTime")]
		public long StartTimeTicks
		{
			get
			{
				return this.StartTime.Ticks;
			}
			set
			{
				this.StartTime = new TimeSpan(value);
			}
		}

		public bool Sunday
		{
			get
			{
				return JustDecompileGenerated_get_Sunday();
			}
			set
			{
				JustDecompileGenerated_set_Sunday(value);
			}
		}

		public bool JustDecompileGenerated_get_Sunday()
		{
			return this._Sunday;
		}

		public void JustDecompileGenerated_set_Sunday(bool value)
		{
			this._Sunday = value;
		}

		public bool Thursday
		{
			get
			{
				return JustDecompileGenerated_get_Thursday();
			}
			set
			{
				JustDecompileGenerated_set_Thursday(value);
			}
		}

		public bool JustDecompileGenerated_get_Thursday()
		{
			return this._Thursday;
		}

		public void JustDecompileGenerated_set_Thursday(bool value)
		{
			this._Thursday = value;
		}

		public bool Tuesday
		{
			get
			{
				return JustDecompileGenerated_get_Tuesday();
			}
			set
			{
				JustDecompileGenerated_set_Tuesday(value);
			}
		}

		public bool JustDecompileGenerated_get_Tuesday()
		{
			return this._Tuesday;
		}

		public void JustDecompileGenerated_set_Tuesday(bool value)
		{
			this._Tuesday = value;
		}

		public bool Wednesday
		{
			get
			{
				return JustDecompileGenerated_get_Wednesday();
			}
			set
			{
				JustDecompileGenerated_set_Wednesday(value);
			}
		}

		public bool JustDecompileGenerated_get_Wednesday()
		{
			return this._Wednesday;
		}

		public void JustDecompileGenerated_set_Wednesday(bool value)
		{
			this._Wednesday = value;
		}

		public RecurrencePattern()
		{
		}
	}
}