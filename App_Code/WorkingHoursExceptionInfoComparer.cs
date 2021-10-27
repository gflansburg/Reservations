using System;
using System.Collections;

namespace Gafware.Modules.Reservations
{
	internal class WorkingHoursExceptionInfoComparer : IComparer
	{
		public WorkingHoursExceptionInfoComparer()
		{
		}

		public int Compare(object x, object y)
		{
			DateTime date = ((WorkingHoursExceptionInfo)x).Date;
			int num = date.CompareTo(((WorkingHoursExceptionInfo)y).Date);
			if (num != 0)
			{
				return num;
			}
			return ((WorkingHoursExceptionInfo)x).StartTime.CompareTo(((WorkingHoursExceptionInfo)y).StartTime);
		}
	}
}