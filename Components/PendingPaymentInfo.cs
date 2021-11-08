using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using System;

namespace Gafware.Modules.Reservations
{
	public class PendingPaymentInfo
	{
		private UserInfo _CreatedByUserInfo;

		private UserInfo _LastModifiedByUserInfo;

		private Gafware.Modules.Reservations.CategoryInfo _CategoryInfo;

		private int _TabModuleID = Null.NullInteger;

		private int _PendingPaymentID = Null.NullInteger;

		private int _PortalID = Null.NullInteger;

		private int _ReservationID = Null.NullInteger;

		private int _PendingApprovalID = Null.NullInteger;

		private int _CategoryID = Null.NullInteger;

		private DateTime _StartDateTime = Null.NullDate;

		private int _Duration = Null.NullInteger;

		private string _FirstName = Null.NullString;

		private string _LastName = Null.NullString;

		private string _Email = Null.NullString;

		private string _Phone = Null.NullString;

		private string _Description = Null.NullString;

		private decimal _Amount = Null.NullDecimal;

		private decimal _RefundableAmount = Null.NullDecimal;

		private string _Currency = Null.NullString;

		private int _Status = Null.NullInteger;

		private int _CreatedByUserID = Null.NullInteger;

		private DateTime _CreatedOnDate = Null.NullDate;

		private int _LastModifiedByUserID = Null.NullInteger;

		private DateTime _LastModifiedOnDate = Null.NullDate;

		public decimal Amount
		{
			get
			{
				return this._Amount;
			}
			set
			{
				this._Amount = value;
			}
		}

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

		public string Currency
		{
			get
			{
				return this._Currency;
			}
			set
			{
				this._Currency = value;
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

		public int PendingApprovalID
		{
			get
			{
				return this._PendingApprovalID;
			}
			set
			{
				this._PendingApprovalID = value;
			}
		}

		public int PendingPaymentID
		{
			get
			{
				return this._PendingPaymentID;
			}
			set
			{
				this._PendingPaymentID = value;
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

		public int PortalID
		{
			get
			{
				return this._PortalID;
			}
			set
			{
				this._PortalID = value;
			}
		}

		public decimal RefundableAmount
		{
			get
			{
				return this._RefundableAmount;
			}
			set
			{
				this._RefundableAmount = value;
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

		public int Status
		{
			get
			{
				return this._Status;
			}
			set
			{
				this._Status = value;
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

		public PendingPaymentInfo()
		{
		}
	}
}