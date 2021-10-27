using System;

namespace Gafware.Modules.Reservations
{
	public class OccurrenceCalculator
	{
		private IRecurrencePattern _RecurrencePattern;

		private DateTime? _FirstOccurrenceDate;

		private DateTime? _LastOccurrenceDate;

		private DayPosition? _DayPosition
		{
			get
			{
				return this._RecurrencePattern.DayPosition;
			}
		}

		private DayType? _DayType
		{
			get
			{
				return this._RecurrencePattern.DayType;
			}
		}

		private int? Day
		{
			get
			{
				return this._RecurrencePattern.Day;
			}
		}

		private TimeSpan Duration
		{
			get
			{
				return this._RecurrencePattern.Duration;
			}
		}

		private int? EndAfter
		{
			get
			{
				return this._RecurrencePattern.EndAfter;
			}
		}

		private DateTime? EndDate
		{
			get
			{
				return this._RecurrencePattern.EndDate;
			}
		}

		private int? Every
		{
			get
			{
				return this._RecurrencePattern.Every;
			}
		}

		private bool EveryWeekDay
		{
			get
			{
				return this._RecurrencePattern.EveryWeekDay;
			}
		}

		public DateTime FirstOccurrenceDate
		{
			get
			{
				if (!this._FirstOccurrenceDate.HasValue)
				{
					this._FirstOccurrenceDate = new DateTime?(this.CalculateFirstOccurrenceDate());
				}
				return this._FirstOccurrenceDate.Value;
			}
		}

		private DateTime FirstOccurrenceDateTime
		{
			get
			{
				return this.FirstOccurrenceDate.Add(this.StartTime);
			}
		}

		private DateTime FirstOccurrenceEndDateTime
		{
			get
			{
				return this.FirstOccurrenceDateTime.Add(this.Duration);
			}
		}

		private bool Friday
		{
			get
			{
				return this._RecurrencePattern.Friday;
			}
		}

		private bool IsAllDay
		{
			get
			{
				return this._RecurrencePattern.IsAllDay;
			}
		}

		private bool IsMultiDay
		{
			get
			{
				if (this.Duration.TotalHours <= 24 && this.FirstOccurrenceEndDateTime.TimeOfDay == new TimeSpan(0, 0, 0))
				{
					return false;
				}
				return this.FirstOccurrenceDateTime.Date < this.FirstOccurrenceEndDateTime.Date;
			}
		}

		private DateTime LastOccurrence
		{
			get
			{
				if (!this._LastOccurrenceDate.HasValue)
				{
					this._LastOccurrenceDate = this.CalculateLastOccurrence();
					if (!this._LastOccurrenceDate.HasValue)
					{
						this._LastOccurrenceDate = new DateTime?(DateTime.MaxValue);
					}
				}
				return this._LastOccurrenceDate.Value;
			}
		}

		private bool Monday
		{
			get
			{
				return this._RecurrencePattern.Monday;
			}
		}

		private int? Month
		{
			get
			{
				return this._RecurrencePattern.Month;
			}
		}

		private Gafware.Modules.Reservations.Pattern Pattern
		{
			get
			{
				return this._RecurrencePattern.Pattern;
			}
		}

		private bool Saturday
		{
			get
			{
				return this._RecurrencePattern.Saturday;
			}
		}

		private DateTime StartDate
		{
			get
			{
				return this._RecurrencePattern.StartDate;
			}
		}

		private TimeSpan StartTime
		{
			get
			{
				return this._RecurrencePattern.StartTime;
			}
		}

		public bool Sunday
		{
			get
			{
				return this._RecurrencePattern.Sunday;
			}
		}

		private bool Thursday
		{
			get
			{
				return this._RecurrencePattern.Thursday;
			}
		}

		private bool Tuesday
		{
			get
			{
				return this._RecurrencePattern.Tuesday;
			}
		}

		private bool Wednesday
		{
			get
			{
				return this._RecurrencePattern.Wednesday;
			}
		}

		public OccurrenceCalculator(IRecurrencePattern recurrencePattern)
		{
			this._RecurrencePattern = recurrencePattern;
		}

		private DateTime CalculateDayPositionOfDayType(int year, int month)
		{
			DayPosition? nullable;
			DateTime i;
			DayOfWeek dayOfWeek;
			int num;
			DayType? nullable1 = this._DayType;
			if ((nullable1.GetValueOrDefault() == DayType.Day ? nullable1.HasValue : false))
			{
				int num1 = year;
				int num2 = month;
				nullable = this._DayPosition;
				return new DateTime(num1, num2, ((nullable.GetValueOrDefault() == DayPosition.Last ? nullable.HasValue : false) ? DateTime.DaysInMonth(year, month) : 1));
			}
			nullable1 = this._DayType;
			if ((nullable1.GetValueOrDefault() == DayType.WeekDay ? !nullable1.HasValue : true))
			{
				nullable1 = this._DayType;
				if ((nullable1.GetValueOrDefault() == DayType.WeekendDay ? !nullable1.HasValue : true))
				{
					nullable1 = this._DayType;
					if ((nullable1.GetValueOrDefault() == DayType.Monday ? !nullable1.HasValue : true))
					{
						nullable1 = this._DayType;
						if ((nullable1.GetValueOrDefault() == DayType.Tuesday ? !nullable1.HasValue : true))
						{
							nullable1 = this._DayType;
							if ((nullable1.GetValueOrDefault() == DayType.Wednesday ? !nullable1.HasValue : true))
							{
								nullable1 = this._DayType;
								if ((nullable1.GetValueOrDefault() == DayType.Thursday ? !nullable1.HasValue : true))
								{
									nullable1 = this._DayType;
									if ((nullable1.GetValueOrDefault() == DayType.Friday ? !nullable1.HasValue : true))
									{
										nullable1 = this._DayType;
										if ((nullable1.GetValueOrDefault() == DayType.Saturday ? !nullable1.HasValue : true))
										{
											nullable1 = this._DayType;
											if ((nullable1.GetValueOrDefault() == DayType.Sunday ? !nullable1.HasValue : true))
											{
												throw new Exception();
											}
										}
									}
								}
							}
						}
					}
					nullable1 = this._DayType;
					if ((nullable1.GetValueOrDefault() == DayType.Monday ? nullable1.HasValue : false))
					{
						dayOfWeek = DayOfWeek.Monday;
					}
					else
					{
						nullable1 = this._DayType;
						if ((nullable1.GetValueOrDefault() == DayType.Tuesday ? nullable1.HasValue : false))
						{
							dayOfWeek = DayOfWeek.Tuesday;
						}
						else
						{
							nullable1 = this._DayType;
							if ((nullable1.GetValueOrDefault() == DayType.Wednesday ? nullable1.HasValue : false))
							{
								dayOfWeek = DayOfWeek.Wednesday;
							}
							else
							{
								nullable1 = this._DayType;
								if ((nullable1.GetValueOrDefault() == DayType.Thursday ? nullable1.HasValue : false))
								{
									dayOfWeek = DayOfWeek.Thursday;
								}
								else
								{
									nullable1 = this._DayType;
									if ((nullable1.GetValueOrDefault() == DayType.Friday ? nullable1.HasValue : false))
									{
										dayOfWeek = DayOfWeek.Friday;
									}
									else
									{
										nullable1 = this._DayType;
										dayOfWeek = ((nullable1.GetValueOrDefault() == DayType.Saturday ? nullable1.HasValue : false) ? DayOfWeek.Saturday : DayOfWeek.Sunday);
									}
								}
							}
						}
					}
					DayOfWeek dayOfWeek1 = dayOfWeek;
					int num3 = year;
					int num4 = month;
					nullable = this._DayPosition;
					DateTime dateTime = new DateTime(num3, num4, ((nullable.GetValueOrDefault() == DayPosition.Last ? nullable.HasValue : false) ? DateTime.DaysInMonth(year, month) : 1));
					nullable = this._DayPosition;
					int num5 = ((nullable.GetValueOrDefault() == DayPosition.Last ? nullable.HasValue : false) ? -1 : 1);
					while (dateTime.DayOfWeek != dayOfWeek1)
					{
						dateTime = dateTime.AddDays((double)num5);
					}
					nullable = this._DayPosition;
					if ((nullable.GetValueOrDefault() == DayPosition.First ? nullable.HasValue : false))
					{
						num = 0;
					}
					else
					{
						nullable = this._DayPosition;
						if ((nullable.GetValueOrDefault() == DayPosition.Second ? nullable.HasValue : false))
						{
							num = 1;
						}
						else
						{
							nullable = this._DayPosition;
							if ((nullable.GetValueOrDefault() == DayPosition.Third ? nullable.HasValue : false))
							{
								num = 2;
							}
							else
							{
								nullable = this._DayPosition;
								num = ((nullable.GetValueOrDefault() == DayPosition.Fourth ? nullable.HasValue : false) ? 3 : 0);
							}
						}
					}
					num5 = num * 7;
					return dateTime.AddDays((double)num5);
				}
			}
			nullable = this._DayPosition;
			if ((nullable.GetValueOrDefault() == DayPosition.Last ? nullable.HasValue : false))
			{
				for (i = new DateTime(year, month, DateTime.DaysInMonth(year, month)); i.Month == month; i = i.AddDays(-1))
				{
					nullable1 = this._DayType;
					if ((nullable1.GetValueOrDefault() == DayType.WeekendDay ? nullable1.HasValue : false) && i.DayOfWeek != DayOfWeek.Saturday && i.DayOfWeek != DayOfWeek.Sunday)
					{
						break;
					}
					nullable1 = this._DayType;
					if ((nullable1.GetValueOrDefault() == DayType.WeekDay ? nullable1.HasValue : false) && (i.DayOfWeek == DayOfWeek.Saturday || i.DayOfWeek == DayOfWeek.Sunday))
					{
						break;
					}
				}
				return i;
			}
			DateTime dateTime1 = new DateTime(year, month, 1);
			int num6 = 0;
			while (true)
			{
				nullable1 = this._DayType;
				if ((nullable1.GetValueOrDefault() == DayType.WeekDay ? !nullable1.HasValue : true) || dateTime1.DayOfWeek == DayOfWeek.Saturday || dateTime1.DayOfWeek == DayOfWeek.Sunday)
				{
					nullable1 = this._DayType;
					if ((nullable1.GetValueOrDefault() == DayType.WeekendDay ? !nullable1.HasValue : true) || dateTime1.DayOfWeek != DayOfWeek.Saturday && dateTime1.DayOfWeek != DayOfWeek.Sunday)
					{
						goto Label0;
					}
				}
				num6++;
			Label0:
				nullable = this._DayPosition;
				if (num6 == (int)nullable.Value)
				{
					break;
				}
				dateTime1 = dateTime1.AddDays(1);
			}
			return dateTime1;
		}

		private DateTime CalculateFirstOccurrenceDate()
		{
			DateTime dateTime;
			int value;
			int obj;
			DateTime startDate = this.StartDate;
			if (this.Pattern == Gafware.Modules.Reservations.Pattern.Daily)
			{
				if (this.Every.HasValue)
				{
					return startDate;
				}
				if (startDate.DayOfWeek == DayOfWeek.Saturday)
				{
					obj = 2;
				}
				else if (startDate.DayOfWeek == DayOfWeek.Sunday)
				{
					obj = 1;
				}
				else
				{
					obj = 0;
				}
				return startDate.AddDays(obj);
			}
			if (this.Pattern == Gafware.Modules.Reservations.Pattern.Weekly)
			{
				DateTime dateTime1 = startDate;
				while ((!this.Sunday || dateTime1.DayOfWeek != DayOfWeek.Sunday) && (!this.Monday || dateTime1.DayOfWeek != DayOfWeek.Monday) && (!this.Tuesday || dateTime1.DayOfWeek != DayOfWeek.Tuesday) && (!this.Wednesday || dateTime1.DayOfWeek != DayOfWeek.Wednesday) && (!this.Thursday || dateTime1.DayOfWeek != DayOfWeek.Thursday) && (!this.Friday || dateTime1.DayOfWeek != DayOfWeek.Friday) && (!this.Saturday || dateTime1.DayOfWeek != DayOfWeek.Saturday))
				{
					dateTime1 = dateTime1.AddDays(1);
				}
				return dateTime1;
			}
			if (this.Pattern == Gafware.Modules.Reservations.Pattern.Monthly)
			{
				dateTime = new DateTime(startDate.Year, startDate.Month, 1);
				DateTime dateTime2 = dateTime.AddMonths(1);
				if (!this.Day.HasValue)
				{
					DateTime dateTime3 = this.CalculateDayPositionOfDayType(startDate.Year, startDate.Month);
					if (dateTime3 < startDate)
					{
						dateTime3 = this.CalculateDayPositionOfDayType(dateTime2.Year, dateTime2.Month);
					}
					return dateTime3;
				}
				DateTime dateTime4 = new DateTime(startDate.Year, startDate.Month, (DateTime.DaysInMonth(startDate.Year, startDate.Month) < this.Day.Value ? DateTime.DaysInMonth(startDate.Year, startDate.Month) : this.Day.Value));
				if (dateTime4 < startDate)
				{
					dateTime4 = new DateTime(dateTime2.Year, dateTime2.Month, (DateTime.DaysInMonth(dateTime2.Year, dateTime2.Month) < this.Day.Value ? DateTime.DaysInMonth(dateTime2.Year, dateTime2.Month) : this.Day.Value));
				}
				return dateTime4;
			}
			if (this.Pattern != Gafware.Modules.Reservations.Pattern.Yearly)
			{
				return startDate;
			}
			int year = startDate.Year;
			int? month = this.Month;
			dateTime = new DateTime(year, month.Value, 1);
			DateTime dateTime5 = dateTime.AddYears(1);
			if (!this.Day.HasValue)
			{
				int num = startDate.Year;
				month = this.Month;
				DateTime dateTime6 = this.CalculateDayPositionOfDayType(num, month.Value);
				if (dateTime6 < startDate)
				{
					dateTime6 = this.CalculateDayPositionOfDayType(dateTime5.Year, dateTime5.Month);
				}
				return dateTime6;
			}
			int year1 = startDate.Year;
			int value1 = this.Month.Value;
			if (DateTime.DaysInMonth(startDate.Year, this.Month.Value) < this.Day.Value)
			{
				int num1 = startDate.Year;
				month = this.Month;
				value = DateTime.DaysInMonth(num1, month.Value);
			}
			else
			{
				value = this.Day.Value;
			}
			DateTime dateTime7 = new DateTime(year1, value1, value);
			if (dateTime7 < startDate)
			{
				dateTime7 = new DateTime(dateTime5.Year, dateTime5.Month, (DateTime.DaysInMonth(dateTime5.Year, dateTime5.Month) < this.Day.Value ? DateTime.DaysInMonth(dateTime5.Year, dateTime5.Month) : this.Day.Value));
			}
			return dateTime7;
		}

		private DateTime? CalculateLastOccurrence()
		{
			int? endAfter;
			DateTime dateTime;
			object obj;
			DateTime? endDate = this.EndDate;
			if (!this.EndAfter.HasValue)
			{
				return endDate;
			}
			DateTime firstOccurrenceDate = this.FirstOccurrenceDate;
			if (this.Pattern == Gafware.Modules.Reservations.Pattern.Daily)
			{
				if (this.Every.HasValue)
				{
					endAfter = this.EndAfter;
					int value = endAfter.Value - 1;
					endAfter = this.Every;
					return new DateTime?(firstOccurrenceDate.AddDays((double)(value * endAfter.Value)));
				}
				endAfter = this.EndAfter;
				int num = endAfter.Value / 5;
				endAfter = this.EndAfter;
				int value1 = endAfter.Value % 5 - 1;
				DateTime dateTime1 = firstOccurrenceDate.AddDays((double)(num * 7 + value1));
				if (dateTime1.DayOfWeek == DayOfWeek.Saturday)
				{
					obj = 2;
				}
				else if (dateTime1.DayOfWeek == DayOfWeek.Sunday)
				{
					obj = 1;
				}
				else
				{
					obj = null;
				}
				return new DateTime?(dateTime1.AddDays((double)obj));
			}
			if (this.Pattern == Gafware.Modules.Reservations.Pattern.Weekly)
			{
				int num1 = Convert.ToInt32(this.Monday) + Convert.ToInt32(this.Tuesday) + Convert.ToInt32(this.Wednesday) + Convert.ToInt32(this.Thursday) + Convert.ToInt32(this.Friday) + Convert.ToInt32(this.Saturday) + Convert.ToInt32(this.Sunday);
				endAfter = this.EndAfter;
				int value2 = endAfter.Value / num1;
				endAfter = this.EndAfter;
				int num2 = endAfter.Value % num1;
				endAfter = this.Every;
				DateTime dateTime2 = firstOccurrenceDate.AddDays((double)(7 * value2 * endAfter.Value));
				int num3 = 1;
				if (num2 == 0)
				{
					num3 = -1;
					num2 = 1;
					dateTime2 = dateTime2.AddDays(-1);
				}
				int num4 = 0;
				while (true)
				{
					if (this.Sunday && dateTime2.DayOfWeek == DayOfWeek.Sunday || this.Monday && dateTime2.DayOfWeek == DayOfWeek.Monday || this.Tuesday && dateTime2.DayOfWeek == DayOfWeek.Tuesday || this.Wednesday && dateTime2.DayOfWeek == DayOfWeek.Wednesday || this.Thursday && dateTime2.DayOfWeek == DayOfWeek.Thursday || this.Friday && dateTime2.DayOfWeek == DayOfWeek.Friday || this.Saturday && dateTime2.DayOfWeek == DayOfWeek.Saturday)
					{
						num4++;
						if (num4 == num2)
						{
							break;
						}
					}
					dateTime2 = dateTime2.AddDays((double)num3);
				}
				return new DateTime?(dateTime2);
			}
			if (this.Pattern != Gafware.Modules.Reservations.Pattern.Monthly)
			{
				if (this.Pattern != Gafware.Modules.Reservations.Pattern.Yearly)
				{
					return endDate;
				}
				dateTime = new DateTime(firstOccurrenceDate.Year, firstOccurrenceDate.Month, 1);
				endAfter = this.EndAfter;
				DateTime dateTime3 = dateTime.AddYears(endAfter.Value - 1);
				if (!this.Day.HasValue)
				{
					return new DateTime?(this.CalculateDayPositionOfDayType(dateTime3.Year, dateTime3.Month));
				}
				return new DateTime?(new DateTime(dateTime3.Year, dateTime3.Month, (DateTime.DaysInMonth(dateTime3.Year, dateTime3.Month) < this.Day.Value ? DateTime.DaysInMonth(dateTime3.Year, dateTime3.Month) : this.Day.Value)));
			}
			dateTime = new DateTime(firstOccurrenceDate.Year, firstOccurrenceDate.Month, 1);
			int value3 = this.Every.Value;
			endAfter = this.EndAfter;
			DateTime dateTime4 = dateTime.AddMonths(value3 * (endAfter.Value - 1));
			if (!this.Day.HasValue)
			{
				return new DateTime?(this.CalculateDayPositionOfDayType(dateTime4.Year, dateTime4.Month));
			}
			return new DateTime?(new DateTime(dateTime4.Year, dateTime4.Month, (DateTime.DaysInMonth(dateTime4.Year, dateTime4.Month) < this.Day.Value ? DateTime.DaysInMonth(dateTime4.Year, dateTime4.Month) : this.Day.Value)));
		}

		public DateTime? CalculateNextOccurrence(DateTime dateTime)
		{
			int? every;
			DateTime dateTime1;
			if (!this.OccursOnDay(dateTime))
			{
				DateTime? nullable = new DateTime?(this.FirstOccurrenceDate);
				while (nullable.HasValue && nullable.Value <= dateTime)
				{
					nullable = this.CalculateNextOccurrence(nullable.Value);
				}
				return nullable;
			}
			DateTime dateTime2 = dateTime;
			if (this.Pattern == Gafware.Modules.Reservations.Pattern.Daily)
			{
				if (!this.Every.HasValue)
				{
					dateTime2 = dateTime.AddDays((double)((dateTime.DayOfWeek == DayOfWeek.Friday ? 3 : 1)));
				}
				else
				{
					every = this.Every;
					dateTime2 = dateTime.AddDays((double)every.Value);
				}
			}
			else if (this.Pattern == Gafware.Modules.Reservations.Pattern.Weekly)
			{
				do
				{
					dateTime = dateTime.AddDays(1);
					if (dateTime.DayOfWeek != DayOfWeek.Sunday)
					{
						continue;
					}
					every = this.Every;
					dateTime = dateTime.AddDays((double)((every.Value - 1) * 7));
				}
				while ((!this.Sunday || dateTime.DayOfWeek != DayOfWeek.Sunday) && (!this.Monday || dateTime.DayOfWeek != DayOfWeek.Monday) && (!this.Tuesday || dateTime.DayOfWeek != DayOfWeek.Tuesday) && (!this.Wednesday || dateTime.DayOfWeek != DayOfWeek.Wednesday) && (!this.Thursday || dateTime.DayOfWeek != DayOfWeek.Thursday) && (!this.Friday || dateTime.DayOfWeek != DayOfWeek.Friday) && (!this.Saturday || dateTime.DayOfWeek != DayOfWeek.Saturday));
				dateTime2 = dateTime;
			}
			else if (this.Pattern == Gafware.Modules.Reservations.Pattern.Monthly)
			{
				dateTime1 = new DateTime(dateTime.Year, dateTime.Month, 1);
				every = this.Every;
				DateTime dateTime3 = dateTime1.AddMonths(every.Value);
				dateTime2 = (!this.Day.HasValue ? this.CalculateDayPositionOfDayType(dateTime3.Year, dateTime3.Month) : new DateTime(dateTime3.Year, dateTime3.Month, (DateTime.DaysInMonth(dateTime3.Year, dateTime3.Month) < this.Day.Value ? DateTime.DaysInMonth(dateTime3.Year, dateTime3.Month) : this.Day.Value)));
			}
			else if (this.Pattern == Gafware.Modules.Reservations.Pattern.Yearly)
			{
				dateTime1 = new DateTime(dateTime.Year, dateTime.Month, 1);
				DateTime dateTime4 = dateTime1.AddYears(1);
				dateTime2 = (!this.Day.HasValue ? this.CalculateDayPositionOfDayType(dateTime4.Year, dateTime4.Month) : new DateTime(dateTime4.Year, dateTime4.Month, (DateTime.DaysInMonth(dateTime4.Year, dateTime4.Month) < this.Day.Value ? DateTime.DaysInMonth(dateTime4.Year, dateTime4.Month) : this.Day.Value)));
			}
			object obj = this.CalculateLastOccurrence();
			if (obj == null || !((DateTime)obj < dateTime2))
			{
				return new DateTime?(dateTime2);
			}
			return null;
		}

		private bool OccursOnDateDaily(DateTime date)
		{
			int? nullable;
			DateTime firstOccurrenceDate = this.FirstOccurrenceDate;
			int? every = this.Every;
			if (!every.HasValue)
			{
				if (date.DayOfWeek == DayOfWeek.Saturday)
				{
					return false;
				}
				return date.DayOfWeek != DayOfWeek.Sunday;
			}
			int days = date.Subtract(firstOccurrenceDate).Days;
			int? every1 = this.Every;
			if (every1.HasValue)
			{
				nullable = new int?(days % every1.GetValueOrDefault());
			}
			else
			{
				nullable = null;
			}
			every = nullable;
			if (every.GetValueOrDefault() != 0)
			{
				return false;
			}
			return every.HasValue;
		}

		private bool OccursOnDateMonthly(DateTime date)
		{
			int? day;
			int num;
			int? every;
			int? nullable;
			int? nullable1;
			int? nullable2;
			DateTime firstOccurrenceDate = this.FirstOccurrenceDate;
			int year = date.Year * 12 + date.Month - (firstOccurrenceDate.Year * 12 + firstOccurrenceDate.Month);
			if (this._DayType.HasValue)
			{
				num = year;
				every = this.Every;
				if (every.HasValue)
				{
					nullable1 = new int?(num % every.GetValueOrDefault());
				}
				else
				{
					nullable = null;
					nullable1 = nullable;
				}
				day = nullable1;
				if ((day.GetValueOrDefault() == 0 ? !day.HasValue : true))
				{
					return false;
				}
				return this.CalculateDayPositionOfDayType(date.Year, date.Month) == date;
			}
			num = year;
			every = this.Every;
			if (every.HasValue)
			{
				nullable2 = new int?(num % every.GetValueOrDefault());
			}
			else
			{
				nullable = null;
				nullable2 = nullable;
			}
			day = nullable2;
			if ((day.GetValueOrDefault() == 0 ? !day.HasValue : true))
			{
				return false;
			}
			int day1 = date.Day;
			day = this.Day;
			if ((day1 == day.GetValueOrDefault() ? day.HasValue : false))
			{
				return true;
			}
			day = this.Day;
			int num1 = DateTime.DaysInMonth(date.Year, date.Month);
			if ((day.GetValueOrDefault() > num1 ? !day.HasValue : true))
			{
				return false;
			}
			return date.Day == DateTime.DaysInMonth(date.Year, date.Month);
		}

		private bool OccursOnDateWeekly(DateTime date)
		{
			int? nullable;
			DateTime firstOccurrenceDate = this.FirstOccurrenceDate;
			TimeSpan timeSpan = date.Subtract(firstOccurrenceDate);
			int days = timeSpan.Days / 7 + (date.DayOfWeek >= firstOccurrenceDate.DayOfWeek ? 0 : 1);
			int? every = this.Every;
			if (every.HasValue)
			{
				nullable = new int?(days % every.GetValueOrDefault());
			}
			else
			{
				nullable = null;
			}
			int? nullable1 = nullable;
			if ((nullable1.GetValueOrDefault() == 0 ? !nullable1.HasValue : true))
			{
				return false;
			}
			if (this.Monday && date.DayOfWeek == DayOfWeek.Monday || this.Tuesday && date.DayOfWeek == DayOfWeek.Tuesday || this.Wednesday && date.DayOfWeek == DayOfWeek.Wednesday || this.Thursday && date.DayOfWeek == DayOfWeek.Thursday || this.Friday && date.DayOfWeek == DayOfWeek.Friday || this.Saturday && date.DayOfWeek == DayOfWeek.Saturday)
			{
				return true;
			}
			if (!this.Sunday)
			{
				return false;
			}
			return date.DayOfWeek == DayOfWeek.Sunday;
		}

		private bool OccursOnDateYearly(DateTime date)
		{
			DateTime firstOccurrenceDate = this.FirstOccurrenceDate;
			if (!this.Day.HasValue)
			{
				return this.CalculateDayPositionOfDayType(date.Year, date.Month) == date;
			}
			if (date.Month != this.Month.Value)
			{
				return false;
			}
			if (date.Day == this.Day.Value)
			{
				return true;
			}
			if (DateTime.DaysInMonth(date.Year, date.Month) >= this.Day.Value)
			{
				return false;
			}
			return date.Day == DateTime.DaysInMonth(date.Year, date.Month);
		}

		public bool OccursOnDay(DateTime date)
		{
			DateTime dateTime;
			DateTime dateTime1;
			return this.OccursOnDay(date, out dateTime, out dateTime1);
		}

		public bool OccursOnDay(DateTime day, out DateTime startDateTime_, out DateTime endDateTime_)
		{
			DateTime value;
			TimeSpan duration;
			startDateTime_ = new DateTime();
			endDateTime_ = new DateTime();
			day = day.Date;
			if (this.FirstOccurrenceDate <= day)
			{
				if (this.EndDate.HasValue)
				{
					value = this.EndDate.Value;
					value = value.Add(this.StartTime);
					if (value.Add(this.Duration) <= day)
					{
						return false;
					}
				}
				value = this.FirstOccurrenceEndDateTime;
				value = day.Add(value.TimeOfDay);
				for (DateTime i = value.Subtract(this.Duration); i.Date <= day.Date; i = i.AddDays(1))
				{
					bool flag = false;
					if (this.Pattern == Gafware.Modules.Reservations.Pattern.Daily)
					{
						flag = this.OccursOnDateDaily(i.Date);
					}
					else if (this.Pattern == Gafware.Modules.Reservations.Pattern.Weekly)
					{
						flag = this.OccursOnDateWeekly(i.Date);
					}
					else if (this.Pattern == Gafware.Modules.Reservations.Pattern.Monthly)
					{
						flag = this.OccursOnDateMonthly(i.Date);
					}
					else if (this.Pattern == Gafware.Modules.Reservations.Pattern.Yearly)
					{
						flag = this.OccursOnDateYearly(i.Date);
					}
					if (flag)
					{
						DateTime dateTime = day;
						value = i.Date;
						value = value.Add(this.StartTime);
						if (this.Duration.TotalMinutes == 0)
						{
							TimeSpan timeSpan = this.Duration;
							duration = timeSpan.Add(new TimeSpan(0, 0, 1));
						}
						else
						{
							duration = this.Duration;
						}
						if (dateTime < value.Add(duration))
						{
							value = i.Date;
							startDateTime_ = value.Add(this.StartTime);
							endDateTime_ = startDateTime_.Add(this.Duration);
							return true;
						}
					}
				}
				return false;
			}
			return false;
		}
	}
}