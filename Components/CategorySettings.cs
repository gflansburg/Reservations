using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Gafware.Modules.Reservations
{
	public class CategorySettings
	{
		public const string WORKINGHOURSEXCEPTIONSDEFINED_KEY = "WorkingHoursExceptionsDefined";

		private int PortalId;

		private int TabModuleId;

		private int CategoryID;

		private Hashtable _Settings;

		private Gafware.Modules.Reservations.ModuleSettings _ModuleSettings;

		public bool AllowCancellations
		{
			get
			{
				string item = (string)this.Settings["AllowCancellations"];
				if (item != null)
				{
					return bool.Parse(item);
				}
				return this.ModuleSettings.AllowCancellations;
			}
		}

		public bool AllowPayLater
		{
			get
			{
				string item = (string)this.Settings["AllowPayLater"];
				if (item != null)
				{
					return bool.Parse(item);
				}
				return this.ModuleSettings.AllowPayLater;
			}
		}

		public bool AllowRescheduling
		{
			get
			{
				string item = (string)this.Settings["AllowRescheduling"];
				if (item != null)
				{
					return bool.Parse(item);
				}
				return this.ModuleSettings.AllowRescheduling;
			}
		}

		public ArrayList BCCList
		{
			get
			{
				string item = (string)this.Settings["BCCList"];
				if (item == null)
				{
					return this.ModuleSettings.BCCList;
				}
				return this.ModuleSettings.DeserializeUserIDList(item);
			}
		}

		private decimal CancellationFee
		{
			get
			{
				string item = (string)this.Settings["CancellationFee"];
				if (item == null)
				{
					return this.ModuleSettings.CancellationFee;
				}
				return decimal.Parse(item, CultureInfo.InvariantCulture);
			}
		}

		public ArrayList CashierList
		{
			get
			{
				string item = (string)this.Settings["CashierList"];
				if (item == null)
				{
					return this.ModuleSettings.CashierList;
				}
				return this.ModuleSettings.DeserializeUserIDList(item);
			}
		}

		public ArrayList CategoryPermissionsList
		{
			get
			{
				string item = (string)this.Settings["CategoryPermissions"];
				if (item == null)
				{
					return this.ModuleSettings.CategoryPermissionsList;
				}
				return this.ModuleSettings.DeserializeRoleIDList(item);
			}
		}

		public int DaysAhead
		{
			get
			{
				string item = (string)this.Settings["DaysAhead"];
				if (item != null)
				{
					return int.Parse(item);
				}
				return this.ModuleSettings.DaysAhead;
			}
		}

		public decimal DepositFee
		{
			get
			{
				string item = (string)this.Settings["DepositFee"];
				if (item == null)
				{
					return this.ModuleSettings.DepositFee;
				}
				return decimal.Parse(item, CultureInfo.InvariantCulture);
			}
		}

		public Gafware.Modules.Reservations.FeeScheduleType FeeScheduleType
		{
			get
			{
				string item = (string)this.Settings["FeeScheduleType"];
				if (item == null)
				{
					if (!this.Settings.ContainsKey("SchedulingFee"))
					{
						return this.ModuleSettings.FeeScheduleType;
					}
					item = (this.SchedulingFee != decimal.Zero || this.ReschedulingFee != decimal.Zero || this.CancellationFee != decimal.Zero ? Gafware.Modules.Reservations.FeeScheduleType.Flat.ToString() : Gafware.Modules.Reservations.FeeScheduleType.Free.ToString());
				}
				return (Gafware.Modules.Reservations.FeeScheduleType)Enum.Parse(typeof(Gafware.Modules.Reservations.FeeScheduleType), item);
			}
		}

		public Gafware.Modules.Reservations.FlatFeeScheduleInfo FlatFeeScheduleInfo
		{
			get
			{
				Gafware.Modules.Reservations.FlatFeeScheduleInfo flatFeeScheduleInfo = new Gafware.Modules.Reservations.FlatFeeScheduleInfo()
				{
					DepositFee = this.DepositFee,
					ReservationFee = this.SchedulingFee,
					ReschedulingFee = this.ReschedulingFee,
					CancellationFee = this.CancellationFee
				};
				if (!this.Settings.ContainsKey("SchedulingFeeInterval"))
				{
					flatFeeScheduleInfo.Interval = (int)this.ReservationDuration.TotalMinutes;
				}
				else
				{
					flatFeeScheduleInfo.Interval = int.Parse((string)this.Settings["SchedulingFeeInterval"]);
				}
				return flatFeeScheduleInfo;
			}
		}

		public string this[string key]
		{
			get
			{
				return (string)this.Settings[key];
			}
		}

		public int MaxConflictingReservations
		{
			get
			{
				string item = (string)this.Settings["MaxReservationsPerTimeSlot"];
				if (item != null)
				{
					return int.Parse(item);
				}
				return this.ModuleSettings.MaxConflictingReservations;
			}
		}

		public int MaxReservationsPerUser
		{
			get
			{
				string item = (string)this.Settings["MaxReservationsPerUser"];
				if (item != null)
				{
					return int.Parse(item);
				}
				return this.ModuleSettings.MaxReservationsPerUser;
			}
		}

		public TimeSpan MinTimeAhead
		{
			get
			{
				string item = (string)this.Settings["MinTimeAhead"];
				if (item != null)
				{
					return TimeSpan.Parse(item);
				}
				return this.ModuleSettings.MinTimeAhead;
			}
		}

		public bool Moderate
		{
			get
			{
				string item = (string)this.Settings["Moderate"];
				if (item != null)
				{
					return bool.Parse(item);
				}
				return this.ModuleSettings.Moderate;
			}
		}

		public ArrayList ModeratorList
		{
			get
			{
				string item = (string)this.Settings["GlobalModeratorList"];
				if (item == null)
				{
					return this.ModuleSettings.ModeratorList;
				}
				return this.ModuleSettings.DeserializeUserIDList(item);
			}
		}

		private Gafware.Modules.Reservations.ModuleSettings ModuleSettings
		{
			get
			{
				if (this._ModuleSettings == null)
				{
					this._ModuleSettings = new Gafware.Modules.Reservations.ModuleSettings(this.PortalId, this.TabModuleId);
				}
				return this._ModuleSettings;
			}
		}

		public decimal ReschedulingFee
		{
			get
			{
				string item = (string)this.Settings["ReschedulingFee"];
				if (item == null)
				{
					return this.ModuleSettings.ReschedulingFee;
				}
				return decimal.Parse(item, CultureInfo.InvariantCulture);
			}
		}

		public TimeSpan ReservationDuration
		{
			get
			{
				string item = (string)this.Settings["ReservationDuration"];
				if (item == null)
				{
					return this.ModuleSettings.ReservationDuration;
				}
				return new TimeSpan(0, int.Parse(item), 0);
			}
		}

		public TimeSpan ReservationDurationInterval
		{
			get
			{
				string item = (string)this.Settings["ReservationDurationInterval"];
				if (item == null)
				{
					return this.ModuleSettings.ReservationDurationInterval;
				}
				return new TimeSpan(0, int.Parse(item), 0);
			}
		}

		public TimeSpan ReservationDurationMax
		{
			get
			{
				string item = (string)this.Settings["ReservationDurationMax"];
				if (item == null)
				{
					return this.ModuleSettings.ReservationDurationMax;
				}
				return new TimeSpan(0, int.Parse(item), 0);
			}
		}

		public TimeSpan ReservationInterval
		{
			get
			{
				string item = (string)this.Settings["ReservationInterval"];
				if (item == null)
				{
					return this.ModuleSettings.ReservationInterval;
				}
				return new TimeSpan(0, int.Parse(item), 0);
			}
		}

		public decimal SchedulingFee
		{
			get
			{
				string item = (string)this.Settings["SchedulingFee"];
				if (item == null)
				{
					return this.ModuleSettings.SchedulingFee;
				}
				return decimal.Parse(item, CultureInfo.InvariantCulture);
			}
		}

		public List<SeasonalFeeScheduleInfo> SeasonalFeeScheduleList
		{
			get
			{
				string empty = string.Empty;
				for (int i = 1; this.Settings.ContainsKey(string.Concat("SeasonalFeeScheduleList.", i)); i++)
				{
					empty = string.Concat(empty, this.Settings[string.Concat("SeasonalFeeScheduleList.", i)]);
				}
				if (!string.IsNullOrEmpty(empty))
				{
					return Helper.DeserializeSeasonalFeeScheduleList(empty);
				}
				return this.ModuleSettings.SeasonalFeeScheduleList;
			}
		}

		public Hashtable Settings
		{
			get
			{
				if (this._Settings == null)
				{
					this._Settings = new Hashtable();
					foreach (CategorySettingInfo categorySettingList in (new CategorySettingController()).GetCategorySettingList(this.CategoryID))
					{
						this._Settings.Add(categorySettingList.SettingName, categorySettingList.SettingValue);
					}
				}
				return this._Settings;
			}
		}

		public bool WorkingHoursExceptionsDefined
		{
			get
			{
				string item = (string)this.Settings["WorkingHoursExceptionsDefined"];
				if (item == null)
				{
					return false;
				}
				return bool.Parse(item);
			}
		}

		public CategorySettings(int PortalId, int TabModuleId, int CategoryID)
		{
			this.PortalId = PortalId;
			this.TabModuleId = TabModuleId;
			this.CategoryID = CategoryID;
		}

		public bool IsDefined(string settingName)
		{
			return this.Settings.ContainsKey(settingName);
		}
	}
}