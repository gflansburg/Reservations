using System;
using System.Runtime.CompilerServices;

namespace Gafware.Modules.Reservations
{
	public class FlatFeeScheduleInfo
	{
		public decimal CancellationFee
		{
			get;
			set;
		}

		public decimal DepositFee
		{
			get;
			set;
		}

		public int Interval
		{
			get;
			set;
		}

		public decimal ReschedulingFee
		{
			get;
			set;
		}

		public decimal ReservationFee
		{
			get;
			set;
		}

		public FlatFeeScheduleInfo()
		{
		}
	}
}