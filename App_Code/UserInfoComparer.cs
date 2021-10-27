using DotNetNuke.Entities.Users;
using System;
using System.Collections;

namespace Gafware.Modules.Reservations
{
	internal class UserInfoComparer : IComparer
	{
		public UserInfoComparer()
		{
		}

		public int Compare(object x, object y)
		{
			return ((UserInfo)x).DisplayName.CompareTo(((UserInfo)y).DisplayName);
		}
	}
}