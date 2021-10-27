using DotNetNuke.Entities.Users;
using System;
using System.Collections;

namespace Gafware.Modules.Reservations
{
	internal class EmailComparer : IComparer
	{
		public EmailComparer()
		{
		}

		public int Compare(object x, object y)
		{
			if(x.GetType() == typeof(string) && y.GetType() == typeof(string))
            {
				return ((string)x).CompareTo((string)y);
			}
			else if(x.GetType() == typeof(string) && y.GetType() == typeof(UserInfo))
            {
				return ((string)x).CompareTo(((UserInfo)y).Email);
			}
			else if(x.GetType() == typeof(UserInfo) && y.GetType() == typeof(string))
            {
				return ((UserInfo)x).Email.CompareTo((string)y);
			}
			return ((UserInfo)x).Email.CompareTo(((UserInfo)y).Email);
		}
	}
}