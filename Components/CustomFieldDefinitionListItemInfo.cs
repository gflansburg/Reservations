using DotNetNuke.Common.Utilities;
using System;

namespace Gafware.Modules.Reservations
{
	public class CustomFieldDefinitionListItemInfo
	{
		private int _CustomFieldDefinitionID = Null.NullInteger;

		private int _CustomFieldDefinitionListItemID = Null.NullInteger;

		private string _Text = Null.NullString;

		private string _Value = Null.NullString;

		private int _SortOrder = Null.NullInteger;

		private int _CreatedByUserID = Null.NullInteger;

		private DateTime _CreatedOnDate = Null.NullDate;

		private int _LastModifiedByUserID = Null.NullInteger;

		private DateTime _LastModifiedOnDate = Null.NullDate;

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

		public int CustomFieldDefinitionListItemID
		{
			get
			{
				return this._CustomFieldDefinitionListItemID;
			}
			set
			{
				this._CustomFieldDefinitionListItemID = value;
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

		public string Text
		{
			get
			{
				return this._Text;
			}
			set
			{
				this._Text = value;
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

		public CustomFieldDefinitionListItemInfo()
		{
		}
	}
}