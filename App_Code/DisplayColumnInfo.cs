using System;

namespace Gafware.Modules.Reservations
{
	public class DisplayColumnInfo : IComparable
	{
		private string _ColumnName;

		private int _DisplayOrder;

		private string _LocalizedColumnName;

		private bool _Visible;

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

		public int DisplayOrder
		{
			get
			{
				return this._DisplayOrder;
			}
			set
			{
				this._DisplayOrder = value;
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

		public bool Visible
		{
			get
			{
				return this._Visible;
			}
			set
			{
				this._Visible = value;
			}
		}

		public DisplayColumnInfo()
		{
		}

		public int CompareTo(object obj)
		{
			DisplayColumnInfo displayColumnInfo = (DisplayColumnInfo)obj;
			return this.DisplayOrder.CompareTo(displayColumnInfo.DisplayOrder);
		}
	}
}