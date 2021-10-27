using DotNetNuke.Common.Utilities;
using System;

namespace Gafware.Modules.Reservations
{
	public class CategorySettingInfo
	{
		private int _CategoryID = Null.NullInteger;

		private string _SettingName = Null.NullString;

		private string _SettingValue = Null.NullString;

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

		public string SettingName
		{
			get
			{
				return this._SettingName;
			}
			set
			{
				this._SettingName = value;
			}
		}

		public string SettingValue
		{
			get
			{
				return this._SettingValue;
			}
			set
			{
				this._SettingValue = value;
			}
		}

		public CategorySettingInfo()
		{
		}
	}
}