using System;

namespace Gafware.Modules.Reservations
{
	public interface IRecurrencePattern
	{
		int? Day
		{
			get;
		}

		Gafware.Modules.Reservations.DayPosition? DayPosition
		{
			get;
		}

		Gafware.Modules.Reservations.DayType? DayType
		{
			get;
		}

		TimeSpan Duration
		{
			get;
		}

		int? EndAfter
		{
			get;
		}

		DateTime? EndDate
		{
			get;
		}

		int? Every
		{
			get;
		}

		bool EveryWeekDay
		{
			get;
		}

		bool Friday
		{
			get;
		}

		bool IsAllDay
		{
			get;
		}

		bool Monday
		{
			get;
		}

		int? Month
		{
			get;
		}

		Gafware.Modules.Reservations.Pattern Pattern
		{
			get;
		}

		bool Saturday
		{
			get;
		}

		DateTime StartDate
		{
			get;
		}

		TimeSpan StartTime
		{
			get;
		}

		bool Sunday
		{
			get;
		}

		bool Thursday
		{
			get;
		}

		bool Tuesday
		{
			get;
		}

		bool Wednesday
		{
			get;
		}
	}
}