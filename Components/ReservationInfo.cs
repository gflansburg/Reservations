using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using System;

namespace Gafware.Modules.Reservations
{
	public class ReservationInfo
	{
		private int _TabModuleID = Null.NullInteger;

		private int _ReservationID = Null.NullInteger;

		private int _CategoryID = Null.NullInteger;

		private DateTime _StartDateTime = Null.NullDate;

		private int _Duration = Null.NullInteger;

		private string _FirstName = Null.NullString;

		private string _LastName = Null.NullString;

		private string _Email = Null.NullString;

		private string _Phone = Null.NullString;

		private string _Description = Null.NullString;

		private bool _SendReminder = Null.NullBoolean;

		private int _SendReminderWhen = Null.NullInteger;

		private bool _ReminderSent = Null.NullBoolean;

		private bool _RequireConfirmation = Null.NullBoolean;

		private int _RequireConfirmationWhen = Null.NullInteger;

		private bool _Confirmed = Null.NullBoolean;

		private int _CreatedByUserID = Null.NullInteger;

		private DateTime _CreatedOnDate = Null.NullDate;

		private int _LastModifiedByUserID = Null.NullInteger;

		private DateTime _LastModifiedOnDate = Null.NullDate;

		private int _SendReminderVia = Null.NullInteger;

		private UserInfo _CreatedByUserInfo;

		private UserInfo _LastModifiedByUserInfo;

		private Gafware.Modules.Reservations.CategoryInfo _CategoryInfo;

		public int CategoryID
		{
			get
			{
				return this._CategoryID;
			}
			set
			{
				this._CategoryID = value;
			}
		}

		public Gafware.Modules.Reservations.CategoryInfo CategoryInfo
		{
			get
			{
				if (this._CategoryInfo == null)
				{
					this._CategoryInfo = (new CategoryController()).GetCategory(this.CategoryID);
				}
				return this._CategoryInfo;
			}
		}

		public string CategoryName
		{
			get
			{
				if (this.CategoryInfo == null)
				{
					return string.Empty;
				}
				return this.CategoryInfo.Name;
			}
		}

		public bool Confirmed
		{
			get
			{
				return this._Confirmed;
			}
			set
			{
				this._Confirmed = value;
			}
		}

		public string CreatedByDisplayName
		{
			get
			{
				if (this.CreatedByUserInfo == null)
				{
					return string.Empty;
				}
				return this.CreatedByUserInfo.DisplayName;
			}
		}

		public int CreatedByUserID
		{
			get
			{
				return this._CreatedByUserID;
			}
			set
			{
				this._CreatedByUserID = value;
			}
		}

		public UserInfo CreatedByUserInfo
		{
			get
			{
				if (this._CreatedByUserInfo == null)
				{
					this._CreatedByUserInfo = (new UserController()).GetUser(PortalController.Instance.GetCurrentSettings().PortalId, this.CreatedByUserID);
				}
				return this._CreatedByUserInfo;
			}
		}

		public DateTime CreatedOnDate
		{
			get
			{
				return this._CreatedOnDate;
			}
			set
			{
				this._CreatedOnDate = value;
			}
		}

		public string Description
		{
			get
			{
				return this._Description;
			}
			set
			{
				this._Description = value;
			}
		}

		public int Duration
		{
			get
			{
				return this._Duration;
			}
			set
			{
				this._Duration = value;
			}
		}

		public string Email
		{
			get
			{
				return this._Email;
			}
			set
			{
				this._Email = value;
			}
		}

		public DateTime EndTime
		{
			get
			{
				return this.StartDateTime.AddMinutes((double)this.Duration);
			}
		}

		public string FirstName
		{
			get
			{
				return this._FirstName;
			}
			set
			{
				this._FirstName = value;
			}
		}

		public string FullName
		{
			get
			{
				return string.Concat(this.FirstName, " ", this.LastName);
			}
		}

		public string LastModifiedByDisplayName
		{
			get
			{
				if (this.LastModifiedByUserInfo == null)
				{
					return string.Empty;
				}
				return this.LastModifiedByUserInfo.DisplayName;
			}
		}

		public int LastModifiedByUserID
		{
			get
			{
				return this._LastModifiedByUserID;
			}
			set
			{
				this._LastModifiedByUserID = value;
			}
		}

		public UserInfo LastModifiedByUserInfo
		{
			get
			{
				if (this._LastModifiedByUserInfo == null)
				{
					this._LastModifiedByUserInfo = (new UserController()).GetUser(PortalSettings.Current.PortalId, this._LastModifiedByUserID);
				}
				return this._LastModifiedByUserInfo;
			}
		}

		public DateTime LastModifiedOnDate
		{
			get
			{
				return this._LastModifiedOnDate;
			}
			set
			{
				this._LastModifiedOnDate = value;
			}
		}

		public string LastName
		{
			get
			{
				return this._LastName;
			}
			set
			{
				this._LastName = value;
			}
		}

		public string Phone
		{
			get
			{
				return this._Phone;
			}
			set
			{
				this._Phone = value;
			}
		}

		public bool ReminderSent
		{
			get
			{
				return this._ReminderSent;
			}
			set
			{
				this._ReminderSent = value;
			}
		}

		public bool RequireConfirmation
		{
			get
			{
				return this._RequireConfirmation;
			}
			set
			{
				this._RequireConfirmation = value;
			}
		}

		public int RequireConfirmationWhen
		{
			get
			{
				return this._RequireConfirmationWhen;
			}
			set
			{
				this._RequireConfirmationWhen = value;
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

		public bool SendReminder
		{
			get
			{
				return this._SendReminder;
			}
			set
			{
				this._SendReminder = value;
			}
		}

		public int SendReminderVia
		{
			get
			{
				return this._SendReminderVia;
			}
			set
			{
				this._SendReminderVia = value;
			}
		}

		public int SendReminderWhen
		{
			get
			{
				return this._SendReminderWhen;
			}
			set
			{
				this._SendReminderWhen = value;
			}
		}

		public DateTime StartDate
		{
			get
			{
				return this.StartDateTime.Date;
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

		public DateTime StartTime
		{
			get
			{
				return this.StartDateTime;
			}
		}

		public int TabModuleID
		{
			get
			{
				return this._TabModuleID;
			}
			set
			{
				this._TabModuleID = value;
			}
		}

		public ReservationInfo()
		{
		}
	}
}