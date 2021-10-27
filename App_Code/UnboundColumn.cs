using System;
using System.Runtime.CompilerServices;
using System.Web.UI.WebControls;

namespace Gafware.Modules.Reservations
{
	internal class UnboundColumn : DataGridColumn
	{
		public string DataField
		{
			get;
			set;
		}

		public UnboundColumn()
		{
		}
	}
}