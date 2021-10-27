using System;

namespace Gafware.Modules.Reservations
{
	public class SortColumnInfo
	{
		private string _ColumnName;

		private SortColumnInfo.SortDirection _Direction;

		private string _LocalizedColumnName;

		private string _LocalizedDirection;

		public string ColumnName
		{
			get
			{
				return this._ColumnName;
			}
			set
			{
				this._ColumnName = value;
			}
		}

		public SortColumnInfo.SortDirection Direction
		{
			get
			{
				return this._Direction;
			}
			set
			{
				this._Direction = value;
			}
		}

		public string LocalizedColumnName
		{
			get
			{
				if (!string.IsNullOrEmpty(this._LocalizedColumnName))
				{
					return this._LocalizedColumnName;
				}
				return this.ColumnName;
			}
			set
			{
				this._LocalizedColumnName = value;
			}
		}

		public string LocalizedDirection
		{
			get
			{
				return this._LocalizedDirection;
			}
			set
			{
				this._LocalizedDirection = value;
			}
		}

		public SortColumnInfo()
		{
		}

		public enum SortDirection
		{
			Ascending,
			Descending
		}
	}
}