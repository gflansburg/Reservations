using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using System;

namespace Gafware.Modules.Reservations
{
	public class CustomFieldValueInfo
	{
		private UserInfo _CreatedByUserInfo;

		private UserInfo _LastModifiedByUserInfo;

		private Gafware.Modules.Reservations.CustomFieldDefinitionInfo _CustomFieldDefinitionInfo;

		private int _CustomFieldDefinitionID = Null.NullInteger;

		private int _CustomFieldValueID = Null.NullInteger;

		private int _ReservationID = Null.NullInteger;

		private int _PendingPaymentID = Null.NullInteger;

		private int _PendingApprovalID = Null.NullInteger;

		private string _Value = Null.NullString;

		private int _CreatedByUserID = Null.NullInteger;

		private DateTime _CreatedOnDate = Null.NullDate;

		private int _LastModifiedByUserID = Null.NullInteger;

		private DateTime _LastModifiedOnDate = Null.NullDate;

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

		public int CustomFieldDefinitionID
		{
			get
			{
				return this._CustomFieldDefinitionID;
			}
			set
			{
				this._CustomFieldDefinitionID = value;
			}
		}

		public Gafware.Modules.Reservations.CustomFieldDefinitionInfo CustomFieldDefinitionInfo
		{
			get
			{
				if (this._CustomFieldDefinitionInfo == null)
				{
					this._CustomFieldDefinitionInfo = (new CustomFieldDefinitionController()).GetCustomFieldDefinition(this.CustomFieldDefinitionID);
				}
				return this._CustomFieldDefinitionInfo;
			}
		}

		public int CustomFieldValueID
		{
			get
			{
				return this._CustomFieldValueID;
			}
			set
			{
				this._CustomFieldValueID = value;
			}
		}

		public string Label
		{
			get
			{
				return this.CustomFieldDefinitionInfo.Label;
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

		public int SortOrder
		{
			get
			{
				return this.CustomFieldDefinitionInfo.SortOrder;
			}
		}

		public string Value
		{
			get
			{
				return this._Value;
			}
			set
			{
				this._Value = value;
			}
		}

		public CustomFieldValueInfo()
		{
		}
	}
}