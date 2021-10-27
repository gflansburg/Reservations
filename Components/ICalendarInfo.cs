using DotNetNuke.Common.Utilities;
using System;

namespace Gafware.Modules.Reservations
{
	public class ICalendarInfo
	{
		private int _ReservationID = Null.NullInteger;

		private string _UID = Null.NullString;

		private int _Sequence = Null.NullInteger;

		private string _Organizer = Null.NullString;

		public string Organizer
		{
			get
			{
				return this._Organizer;
			}
			set
			{
				this._Organizer = value;
			}
		}

		public int ReservationID
		{
			get
			{
				return this._ReservationID;
			}
			set
			{
				this._ReservationID = value;
			}
		}

		public int Sequence
		{
			get
			{
				return this._Sequence;
			}
			set
			{
				this._Sequence = value;
			}
		}

		public string UID
		{
			get
			{
				return this._UID;
			}
			set
			{
				this._UID = value;
			}
		}

		public ICalendarInfo()
		{
		}
	}
}