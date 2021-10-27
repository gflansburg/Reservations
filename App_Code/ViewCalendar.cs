using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Gafware.Modules.Reservations
{
	public class ViewCalendar : PortalModuleBase
	{
		protected IList _List;

		protected Gafware.Modules.Reservations.Helper _Helper;

		protected Gafware.Modules.Reservations.ModuleSettings _ModuleSettings;

		protected HtmlGenericControl masterDiv;

		protected DropDownList categoryDropDownList;

		protected System.Web.UI.WebControls.Calendar calendar;

		protected HtmlTable buttonsTable;

		protected LinkButton returnCommandButton;

		protected bool CanView
		{
			get
			{
				if (base.UserId == Null.NullInteger)
				{
					return false;
				}
				return this.Helper.CanViewReservations(base.UserId);
			}
		}

		protected Gafware.Modules.Reservations.Helper Helper
		{
			get
			{
				if (this._Helper == null)
				{
					this._Helper = new Gafware.Modules.Reservations.Helper(base.PortalId, base.TabModuleId, base.LocalResourceFile);
				}
				return this._Helper;
			}
		}

		protected IList List
		{
			get
			{
				if (this._List == null)
				{
					this._List = (new ReservationController()).GetReservationListByDateRangeAndCategoryID(base.TabModuleId, this.calendar.VisibleDate, this.calendar.VisibleDate, int.Parse(this.categoryDropDownList.SelectedValue));
				}
				return this._List;
			}
		}

		private Gafware.Modules.Reservations.ModuleSettings ModuleSettings
		{
			get
			{
				if (this._ModuleSettings == null)
				{
					this._ModuleSettings = new Gafware.Modules.Reservations.ModuleSettings(base.PortalId, base.TabModuleId);
				}
				return this._ModuleSettings;
			}
		}

		private string ReturnUrl
		{
			get
			{
				return base.Server.UrlEncode(Globals.NavigateURL(string.Empty));
			}
		}

		public ViewCalendar()
		{
		}

		private void BindData()
		{
			this.categoryDropDownList.DataSource = (new CategoryController()).GetCategoryList(base.TabModuleId);
			this.categoryDropDownList.DataTextField = "Name";
			this.categoryDropDownList.DataValueField = "CategoryID";
			this.categoryDropDownList.DataBind();
			ListItemCollection items = this.categoryDropDownList.Items;
			string str = Localization.GetString("All", base.LocalResourceFile);
			int nullInteger = Null.NullInteger;
			items.Insert(0, new ListItem(str, nullInteger.ToString()));
		}

		protected void CalendarDayRender(object sender, DayRenderEventArgs e)
		{
			if (!e.Day.IsOtherMonth)
			{
				if (e.Day.IsSelected)
				{
					e.Cell.Text = e.Day.DayNumberText;
				}
				return;
			}
			e.Day.IsSelectable = false;
			e.Cell.Text = "";
			e.Cell.CssClass = string.Concat("Gafware_Modules_Reservations_HiddenDayStyle ", e.Cell.CssClass);
		}

		protected void CalendarPreRender(object sender, EventArgs e)
		{
			try
			{
				ControlCollection controls = this.calendar.Controls;
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
			}
		}

		protected void CalendarSelectionChanged(object sender, EventArgs e)
		{
		}

		protected void CancelCommandButtonClicked(object sender, EventArgs e)
		{
			base.Response.Redirect(Globals.NavigateURL(), true);
		}

		protected void Page_Init(object sender, EventArgs e)
		{
			try
			{
				this.SetTheme();
				this.masterDiv.Attributes["class"] = string.Concat("Gafware_Modules_Reservations_", base.Request.QueryString["ctl"]);
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
			}
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			try
			{
				if (!this.CanView)
				{
					base.Response.Redirect(Globals.NavigateURL(), true);
				}
				if (!base.IsPostBack)
				{
					this.BindData();
				}
				this.calendar.PrevMonthText = string.Concat("<img src=\"", this.TemplateSourceDirectory, "/Images/back.png\">");
				this.calendar.NextMonthText = string.Concat("<img src=\"", this.TemplateSourceDirectory, "/Images/next.png\">");
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
			}
		}

		protected void Page_PreRender(object sender, EventArgs e)
		{
		}

		protected override object SaveViewState()
		{
			return base.SaveViewState();
		}

		public void SetTheme()
		{
			string str = string.Concat(new string[] { this.TemplateSourceDirectory, "/Themes/", this.ModuleSettings.Theme, "/", this.ModuleSettings.Theme, ".css" });
			HtmlLink htmlLink = new HtmlLink()
			{
				Href = str
			};
			htmlLink.Attributes.Add("rel", "stylesheet");
			htmlLink.Attributes.Add("type", "text/css");
			this.Page.Header.Controls.Add(htmlLink);
			str = string.Concat(new string[] { this.TemplateSourceDirectory, "/Themes/", this.ModuleSettings.Theme, "/", this.ModuleSettings.Theme, "-LTIE8.css" });
			htmlLink = new HtmlLink()
			{
				Href = str
			};
			htmlLink.Attributes.Add("rel", "stylesheet");
			htmlLink.Attributes.Add("type", "text/css");
			this.Page.Header.Controls.Add(new LiteralControl("<!--[if LT IE 8]>"));
			this.Page.Header.Controls.Add(htmlLink);
			this.Page.Header.Controls.Add(new LiteralControl("<![endif]-->"));
		}
	}
}