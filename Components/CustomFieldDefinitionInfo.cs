using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using System;
using System.Web.UI.WebControls;

namespace Gafware.Modules.Reservations
{
	public class CustomFieldDefinitionInfo
	{
		private UserInfo _CreatedByUserInfo;

		private UserInfo _LastModifiedByUserInfo;

		private int _TabModuleID = Null.NullInteger;

		private int _CustomFieldDefinitionID = Null.NullInteger;

		private string _Name = Null.NullString;

		private string _Label = Null.NullString;

		private string _Type = Null.NullString;

		private string _OptionType = Null.NullString;

		private string _Title = Null.NullString;

		private bool _AddToPreviousRow = Null.NullBoolean;

		private bool _IsRequired = Null.NullBoolean;

		private bool _IsActive = Null.NullBoolean;

		private string _DefaultValue = Null.NullString;

		private int _MaxLength = Null.NullInteger;

		private int _SortOrder = Null.NullInteger;

		private int _CreatedByUserID = Null.NullInteger;

		private DateTime _CreatedOnDate = Null.NullDate;

		private int _LastModifiedByUserID = Null.NullInteger;

		private DateTime _LastModifiedOnDate = Null.NullDate;

		private bool _HideLabel = Null.NullBoolean;

		public bool AddToPreviousRow
		{
			get
			{
				return this._AddToPreviousRow;
			}
			set
			{
				this._AddToPreviousRow = value;
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
					this._CreatedByUserInfo = (new UserController()).GetUser(PortalController.GetCurrentPortalSettings().PortalId, this.CreatedByUserID);
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

		public Gafware.Modules.Reservations.CustomFieldDefinitionType CustomFieldDefinitionType
		{
			get
			{
				return (Gafware.Modules.Reservations.CustomFieldDefinitionType)Enum.Parse(typeof(Gafware.Modules.Reservations.CustomFieldDefinitionType), this.Type);
			}
		}

		public string DefaultValue
		{
			get
			{
				return this._DefaultValue;
			}
			set
			{
				this._DefaultValue = value;
			}
		}

		public bool HideLabel
		{
			get
			{
				return this._HideLabel;
			}
			set
			{
				this._HideLabel = value;
			}
		}

		public bool IsActive
		{
			get
			{
				return this._IsActive;
			}
			set
			{
				this._IsActive = value;
			}
		}

		public bool IsMultiSelect
		{
			get
			{
				return this.OptionType.EndsWith("m");
			}
		}

		public bool IsRequired
		{
			get
			{
				return this._IsRequired;
			}
			set
			{
				this._IsRequired = value;
			}
		}

		public string Label
		{
			get
			{
				return this._Label;
			}
			set
			{
				this._Label = value;
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

		public int MaxLength
		{
			get
			{
				return this._MaxLength;
			}
			set
			{
				this._MaxLength = value;
			}
		}

		public string Name
		{
			get
			{
				return this._Name;
			}
			set
			{
				this._Name = value;
			}
		}

		public int NumberOfRows
		{
			get
			{
				return int.Parse(this.OptionType.Substring(0, (char.IsLetter(this.OptionType[this.OptionType.Length - 1]) ? this.OptionType.Length - 1 : this.OptionType.Length)));
			}
		}

		public string OptionType
		{
			get
			{
				return this._OptionType;
			}
			set
			{
				this._OptionType = value;
			}
		}

		public System.Web.UI.WebControls.RepeatDirection RepeatDirection
		{
			get
			{
				return (System.Web.UI.WebControls.RepeatDirection)Enum.Parse(typeof(System.Web.UI.WebControls.RepeatDirection), this.OptionType);
			}
		}

		public int SortOrder
		{
			get
			{
				return this._SortOrder;
			}
			set
			{
				this._SortOrder = value;
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

		public string Title
		{
			get
			{
				return this._Title;
			}
			set
			{
				this._Title = value;
			}
		}

		public string Type
		{
			get
			{
				return this._Type;
			}
			set
			{
				this._Type = value;
			}
		}

		public CustomFieldDefinitionInfo()
		{
		}
	}
}