using System;

namespace Gafware.Modules.Reservations
{
	public enum PendingPaymentStatus
	{
		Processing,
		Paid,
		Void,
		Expired,
		Held,
		PendingRefund,
		Refunded,
		Due,
		PendingApproval
	}
}