using DotNetNuke.Common.Utilities;
using System;

namespace Gafware.Modules.Reservations
{
	public class CategoryInfo
	{
		private int _TabModuleID = Null.NullInteger;

		private int _CategoryID = Null.NullInteger;

		private string _Name = Null.NullString;

		private int _CreatedByUserID = Null.NullInteger;

		private DateTime _CreatedOnDate = Null.NullDate;

		private int _LastModifiedByUserID = Null.NullInteger;

		private DateTime _LastModifiedOnDate = Null.NullDate;

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

		public CategoryInfo()
		{
		}
	}
}