using System;
using System.Collections;

namespace Gafware.Modules.Reservations
{
	internal class EventInfoComparer : IComparer
	{
		public EventInfoComparer()
		{
		}

		public int Compare(object x, object y)
		{
			return ((ReservationInfo)x).StartDateTime.CompareTo(((ReservationInfo)y).StartDateTime);
		}
	}
}