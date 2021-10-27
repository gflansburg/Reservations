using DotNetNuke.Common.Utilities;
using System;

namespace Gafware.Modules.Reservations
{
	public class LocalizedEnum : IComparable<LocalizedEnum>
	{
		private int @value;

		private string localizedName;

		private string name;

		public string LocalizedName
		{
			get
			{
				return this.localizedName;
			}
		}

		public string Name
		{
			get
			{
				return this.name;
			}
		}

		public int Value
		{
			get
			{
				return this.@value;
			}
		}

		public LocalizedEnum(string localizedName, string name) : this(localizedName, name, Null.NullInteger)
		{
		}

		public LocalizedEnum(string localizedName, string name, int value)
		{
			this.localizedName = localizedName;
			this.name = name;
			this.@value = value;
		}

		public int CompareTo(LocalizedEnum other)
		{
			return this.LocalizedName.CompareTo(other.LocalizedName);
		}
	}
}