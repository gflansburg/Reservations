using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.UserControls;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Gafware.Modules.Reservations
{
    public partial class FeeScheduleControl : PortalModuleBase
    {
        private string _Currency;

        private List<SeasonalFeeScheduleInfo> _SeasonalFeeScheduleList;

		public string Currency
		{
			get
			{
				if (this._Currency == null && this.ViewState["Currency"] != null)
				{
					this._Currency = (string)this.ViewState["Currency"];
				}
				return this._Currency;
			}
			set
			{
				this._Currency = value;
			}
		}

		public Gafware.Modules.Reservations.FeeScheduleType FeeScheduleType
		{
			get;
			set;
		}

		public Gafware.Modules.Reservations.FlatFeeScheduleInfo FlatFeeScheduleInfo
		{
			get;
			set;
		}

		public List<SeasonalFeeScheduleInfo> SeasonalFeeScheduleList
		{
			get
			{
				if (this._SeasonalFeeScheduleList == null)
				{
					if (this.ViewState["SeasonalFeeScheduleList"] != null)
					{
						this._SeasonalFeeScheduleList = Helper.DeserializeSeasonalFeeScheduleList((string)this.ViewState["SeasonalFeeScheduleList"]);
					}
					else
					{
						this._SeasonalFeeScheduleList = new List<SeasonalFeeScheduleInfo>();
					}
				}
				return this._SeasonalFeeScheduleList;
			}
			set
			{
				this._SeasonalFeeScheduleList = value;
			}
		}

		public FeeScheduleControl()
		{
		}

		protected void AddClicked(object sender, EventArgs e)
		{
			this.seasonalAddCommandButton.Visible = false;
			LinkButton linkButton = this.seasonalCancelCommandButton;
			bool flag = true;
			this.seasonalSaveCommandButton.Visible = true;
			linkButton.Visible = flag;
			HtmlTableRow htmlTableRow = this.addTableRow1;
			HtmlTableRow htmlTableRow1 = this.addTableRow2;
			HtmlTableRow htmlTableRow2 = this.addTableRow3;
			bool flag1 = true;
			this.addTableRow4.Visible = true;
			bool flag2 = flag1;
			bool flag3 = flag2;
			htmlTableRow2.Visible = flag2;
			bool flag4 = flag3;
			flag = flag4;
			htmlTableRow1.Visible = flag4;
			htmlTableRow.Visible = flag;
		}

		protected void BindDataGrid()
		{
			this.seasonalFeesDataGrid.DataSource = this.SeasonalFeeScheduleList;
			this.seasonalFeesDataGrid.DataBind();
			DataGrid dataGrid = this.seasonalFeesDataGrid;
			bool count = this.SeasonalFeeScheduleList.Count == 0;
			bool flag = count;
			this.noSeasonalFeesLabel.Visible = count;
			dataGrid.Visible = !flag;
		}

		protected void BindDropDowns()
		{
			this.BindTimeSpanDropDownList(this.schedulingFeeIntervalDropDownList);
			this.BindTimeSpanDropDownList(this.seasonalSchedulingFeeIntervalDropDownList);
			this.seasonalStartMonthDropDownList.Items.Clear();
			this.seasonalEndMonthDropDownList.Items.Clear();
			for (int i = 1; i <= 12; i++)
			{
				ListItemCollection items = this.seasonalStartMonthDropDownList.Items;
				DateTime dateTime = new DateTime(2000, i, 1);
				items.Add(new ListItem(dateTime.ToString("MMMM"), i.ToString()));
				ListItemCollection listItemCollections = this.seasonalEndMonthDropDownList.Items;
				dateTime = new DateTime(2000, i, 1);
				listItemCollections.Add(new ListItem(dateTime.ToString("MMMM"), i.ToString()));
			}
		}

		protected void BindTimeSpanDropDownList(DropDownList dropDownList)
		{
			dropDownList.Items.Clear();
			dropDownList.Items.Add(new ListItem(Localization.GetString("Minutes", base.LocalResourceFile), "M"));
			dropDownList.Items.Add(new ListItem(Localization.GetString("Hours", base.LocalResourceFile), "H"));
			dropDownList.Items.Add(new ListItem(Localization.GetString("Days", base.LocalResourceFile), "D"));
		}

		protected void CancelClicked(object sender, EventArgs e)
		{
			this.seasonalAddCommandButton.Visible = true;
			LinkButton linkButton = this.seasonalCancelCommandButton;
			bool flag = false;
			this.seasonalSaveCommandButton.Visible = false;
			linkButton.Visible = flag;
			HtmlTableRow htmlTableRow = this.addTableRow1;
			HtmlTableRow htmlTableRow1 = this.addTableRow2;
			HtmlTableRow htmlTableRow2 = this.addTableRow3;
			bool flag1 = false;
			this.addTableRow4.Visible = false;
			bool flag2 = flag1;
			bool flag3 = flag2;
			htmlTableRow2.Visible = flag2;
			bool flag4 = flag3;
			flag = flag4;
			htmlTableRow1.Visible = flag4;
			htmlTableRow.Visible = flag;
		}

		protected bool Conflicts(DateTime startDateTime1, DateTime endDateTime1, DateTime startDateTime2, DateTime endDateTime2)
		{
			if (startDateTime1 < startDateTime2 && endDateTime1 >= startDateTime2)
			{
				return true;
			}
			if (startDateTime2 >= startDateTime1)
			{
				return false;
			}
			return endDateTime2 >= startDateTime1;
		}

		private bool Conflicts(int startMonth1, int startDay1, int endMonth1, int endDay1, int startMonth2, int startDay2, int endMonth2, int endDay2, int year1)
		{
			DateTime dateTime = new DateTime(year1, startMonth1, startDay1);
			DateTime dateTime1 = new DateTime(year1, endMonth1, endDay1);
			if (dateTime1 < dateTime)
			{
				dateTime = dateTime.AddYears(-1);
			}
			DateTime dateTime2 = new DateTime(2000, startMonth2, startDay2);
			DateTime dateTime3 = new DateTime(2000, endMonth2, endDay2);
			if (dateTime3 < dateTime2)
			{
				dateTime2 = dateTime2.AddYears(-1);
			}
			return this.Conflicts(dateTime, dateTime1, dateTime2, dateTime3);
		}

		private bool Conflicts(int startMonth1, int startDay1, int endMonth1, int endDay1, int startMonth2, int startDay2, int endMonth2, int endDay2)
		{
			if (this.Conflicts(startMonth1, startDay1, endMonth1, endDay1, startMonth2, startDay2, endMonth2, endDay2, 1999) || this.Conflicts(startMonth1, startDay1, endMonth1, endDay1, startMonth2, startDay2, endMonth2, endDay2, 2000))
			{
				return true;
			}
			return this.Conflicts(startMonth1, startDay1, endMonth1, endDay1, startMonth2, startDay2, endMonth2, endDay2, 2001);
		}

		public override void DataBind()
		{
			this.BindDropDowns();
			this.freeScheduleRadioButton.Checked = this.FeeScheduleType == Gafware.Modules.Reservations.FeeScheduleType.Free;
			this.flatScheduleRadioButton.Checked = this.FeeScheduleType == Gafware.Modules.Reservations.FeeScheduleType.Flat;
			this.seasonalScheduleRadioButton.Checked = this.FeeScheduleType == Gafware.Modules.Reservations.FeeScheduleType.Seasonal;
			this.ScheduleTypeChanged(null, null);
			if (this.FeeScheduleType == Gafware.Modules.Reservations.FeeScheduleType.Flat)
			{
				this.depositFeeTextBox.Text = string.Format("{0:F}", this.FlatFeeScheduleInfo.DepositFee);
				this.schedulingFeeTextBox.Text = string.Format("{0:F}", this.FlatFeeScheduleInfo.ReservationFee);
				this.reschedulingFeeTextBox.Text = string.Format("{0:F}", this.FlatFeeScheduleInfo.ReschedulingFee);
				this.cancellationFeeTextBox.Text = string.Format("{0:F}", this.FlatFeeScheduleInfo.CancellationFee);
				this.SetTimeSpan(TimeSpan.FromMinutes((double)this.FlatFeeScheduleInfo.Interval), this.schedulingFeeIntervalTextBox, this.schedulingFeeIntervalDropDownList);
			}
			if (this.FeeScheduleType == Gafware.Modules.Reservations.FeeScheduleType.Seasonal)
			{
				this.BindDataGrid();
			}
		}

		protected void DeleteSeasonalFee(object sender, DataGridCommandEventArgs e)
		{
			this.SeasonalFeeScheduleList.RemoveAt(e.Item.DataSetIndex);
			this.BindDataGrid();
		}

		public Gafware.Modules.Reservations.FeeScheduleType GetFeeScheduleType()
		{
			if (this.freeScheduleRadioButton.Checked)
			{
				return Gafware.Modules.Reservations.FeeScheduleType.Free;
			}
			if (!this.flatScheduleRadioButton.Checked)
			{
				return Gafware.Modules.Reservations.FeeScheduleType.Seasonal;
			}
			return Gafware.Modules.Reservations.FeeScheduleType.Flat;
		}

		public Gafware.Modules.Reservations.FlatFeeScheduleInfo GetFlatFeeScheduleInfo()
		{
			Gafware.Modules.Reservations.FlatFeeScheduleInfo flatFeeScheduleInfo = new Gafware.Modules.Reservations.FlatFeeScheduleInfo()
			{
				DepositFee = decimal.Parse(this.depositFeeTextBox.Text),
				ReservationFee = decimal.Parse(this.schedulingFeeTextBox.Text),
				ReschedulingFee = decimal.Parse(this.reschedulingFeeTextBox.Text),
				CancellationFee = decimal.Parse(this.cancellationFeeTextBox.Text)
			};
			TimeSpan timeSpan = this.GetTimeSpan(this.schedulingFeeIntervalTextBox, this.schedulingFeeIntervalDropDownList);
			flatFeeScheduleInfo.Interval = (int)timeSpan.TotalMinutes;
			return flatFeeScheduleInfo;
		}

		protected string GetFriendlyInterval(SeasonalFeeScheduleInfo fee)
		{
			TimeSpan timeSpan = TimeSpan.FromMinutes((double)fee.Interval);
			if (timeSpan.TotalMinutes % 1440 == 0)
			{
				return string.Concat((int)timeSpan.TotalDays, " ", Localization.GetString("days", base.LocalResourceFile));
			}
			if (timeSpan.TotalMinutes % 60 == 0)
			{
				return string.Concat((int)timeSpan.TotalHours, " ", Localization.GetString("hours", base.LocalResourceFile));
			}
			return string.Concat((int)timeSpan.TotalMinutes, " ", Localization.GetString("minutes", base.LocalResourceFile));
		}

		protected string GetSeasonalFee(object _fee)
		{
			SeasonalFeeScheduleInfo seasonalFeeScheduleInfo = (SeasonalFeeScheduleInfo)_fee;
			object[] str = new object[37];
			str[0] = Localization.GetString("From", base.LocalResourceFile);
			str[1] = " ";
			DateTime dateTime = new DateTime(2000, seasonalFeeScheduleInfo.StartOnMonth, 1);
			str[2] = dateTime.ToString("MMMM");
			str[3] = " ";
			str[4] = seasonalFeeScheduleInfo.StartOnDay;
			str[5] = " ";
			str[6] = Localization.GetString("to", base.LocalResourceFile);
			str[7] = " ";
			dateTime = new DateTime(2000, seasonalFeeScheduleInfo.EndByMonth, 1);
			str[8] = dateTime.ToString("MMMM");
			str[9] = " ";
			str[10] = seasonalFeeScheduleInfo.EndByDay;
			str[11] = "<br />";
			str[12] = Localization.GetString("depositFeeLabel", base.LocalResourceFile);
			str[13] = " ";
			str[14] = Helper.GetFriendlyAmount(seasonalFeeScheduleInfo.DepositFee, this.Currency);
			str[15] = " ";
			str[16] = Localization.GetString("depositFeeTypeLabel", base.LocalResourceFile);
			str[17] = "<br />";
			str[18] = Localization.GetString("schedulingFeeLabel", base.LocalResourceFile);
			str[19] = " ";
			str[20] = Helper.GetFriendlyAmount(seasonalFeeScheduleInfo.ReservationFee, this.Currency);
			str[21] = " ";
			str[22] = Localization.GetString("schedulingFeeTypeLabel", base.LocalResourceFile);
			str[23] = " ";
			str[24] = this.GetFriendlyInterval(seasonalFeeScheduleInfo);
			str[25] = "<br />";
			str[26] = Localization.GetString("reschedulingFeeLabel", base.LocalResourceFile);
			str[27] = " ";
			str[28] = Helper.GetFriendlyAmount(seasonalFeeScheduleInfo.ReschedulingFee, this.Currency);
			str[29] = " ";
			str[30] = Localization.GetString("reschedulingFeeTypeLabel", base.LocalResourceFile);
			str[31] = "<br />";
			str[32] = Localization.GetString("cancellationFeeLabel", base.LocalResourceFile);
			str[33] = " ";
			str[34] = Helper.GetFriendlyAmount(seasonalFeeScheduleInfo.CancellationFee, this.Currency);
			str[35] = " ";
			str[36] = Localization.GetString("cancellationFeeTypeLabel", base.LocalResourceFile);
			return string.Concat(str);
		}

		protected TimeSpan GetTimeSpan(TextBox textBox, DropDownList dropDownList)
		{
			return new TimeSpan((dropDownList.SelectedValue == "D" ? int.Parse(textBox.Text) : 0), (dropDownList.SelectedValue == "H" ? int.Parse(textBox.Text) : 0), (dropDownList.SelectedValue == "M" ? int.Parse(textBox.Text) : 0), 0);
		}

		protected void Page_Init(object sender, EventArgs e)
		{
			this.seasonalAddCommandButton.Click += new EventHandler(this.AddClicked);
			this.seasonalSaveCommandButton.Click += new EventHandler(this.SaveClicked);
			this.seasonalCancelCommandButton.Click += new EventHandler(this.CancelClicked);
		}

		protected void SaveClicked(object sender, EventArgs e)
		{
			if (this.Page.IsValid)
			{
				SeasonalFeeScheduleInfo seasonalFeeScheduleInfo = new SeasonalFeeScheduleInfo()
				{
					StartOnDay = int.Parse(this.seasonalStartDayTextBox.Text),
					StartOnMonth = int.Parse(this.seasonalStartMonthDropDownList.SelectedValue),
					EndByDay = int.Parse(this.seasonalEndDayTextBox.Text),
					EndByMonth = int.Parse(this.seasonalEndMonthDropDownList.SelectedValue),
					DepositFee = decimal.Parse(this.seasonalDepositFeeTextBox.Text),
					ReservationFee = decimal.Parse(this.seasonalSchedulingFeeTextBox.Text),
					ReschedulingFee = decimal.Parse(this.seasonalReschedulingFeeTextBox.Text),
					CancellationFee = decimal.Parse(this.seasonalCancellationFeeTextBox.Text)
				};
				TimeSpan timeSpan = this.GetTimeSpan(this.seasonalSchedulingFeeIntervalTextBox, this.seasonalSchedulingFeeIntervalDropDownList);
				seasonalFeeScheduleInfo.Interval = (int)timeSpan.TotalMinutes;
				this.SeasonalFeeScheduleList.Add(seasonalFeeScheduleInfo);
				this.SeasonalFeeScheduleList.Sort(new SeasonalFeeScheduleInfoComparer());
				this.BindDataGrid();
				this.seasonalStartDayTextBox.Text = "1";
				this.seasonalStartMonthDropDownList.SelectedValue = "1";
				this.seasonalEndDayTextBox.Text = "1";
				this.seasonalEndMonthDropDownList.SelectedValue = "1";
				this.seasonalDepositFeeTextBox.Text = string.Empty;
				this.seasonalSchedulingFeeTextBox.Text = string.Empty;
				this.seasonalReschedulingFeeTextBox.Text = string.Empty;
				this.seasonalCancellationFeeTextBox.Text = string.Empty;
				this.seasonalSchedulingFeeIntervalTextBox.Text = string.Empty;
				this.seasonalSchedulingFeeIntervalDropDownList.SelectedValue = "M";
				this.seasonalAddCommandButton.Visible = true;
				LinkButton linkButton = this.seasonalCancelCommandButton;
				bool flag = false;
				this.seasonalSaveCommandButton.Visible = false;
				linkButton.Visible = flag;
				HtmlTableRow htmlTableRow = this.addTableRow1;
				HtmlTableRow htmlTableRow1 = this.addTableRow2;
				HtmlTableRow htmlTableRow2 = this.addTableRow3;
				bool flag1 = false;
				this.addTableRow4.Visible = false;
				bool flag2 = flag1;
				bool flag3 = flag2;
				htmlTableRow2.Visible = flag2;
				bool flag4 = flag3;
				flag = flag4;
				htmlTableRow1.Visible = flag4;
				htmlTableRow.Visible = flag;
			}
		}

		protected override object SaveViewState()
		{
			try
			{
				this.ViewState["SeasonalFeeScheduleList"] = Helper.SerializeSeasonalFeeScheduleList(this.SeasonalFeeScheduleList);
				this.ViewState["Currency"] = this.Currency;
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
			}
			return base.SaveViewState();
		}

		protected void ScheduleTypeChanged(object sender, EventArgs e)
		{
			this.freechedulePlaceHolder.Visible = this.freeScheduleRadioButton.Checked;
			this.flatSchedulePlaceHolder.Visible = this.flatScheduleRadioButton.Checked;
			this.seasonalSchedulePlaceHolder.Visible = this.seasonalScheduleRadioButton.Checked;
		}

		protected void SetTimeSpan(TimeSpan timeSpan, TextBox textBox, DropDownList dropDownList)
		{
			double totalMinutes;
			if (timeSpan.TotalMinutes == 0)
			{
				totalMinutes = timeSpan.TotalMinutes;
				textBox.Text = totalMinutes.ToString();
				dropDownList.SelectedValue = "M";
				return;
			}
			if (Convert.ToInt32(timeSpan.TotalMinutes) % 1440 == 0)
			{
				totalMinutes = timeSpan.TotalDays;
				textBox.Text = totalMinutes.ToString();
				dropDownList.SelectedValue = "D";
				return;
			}
			if (Convert.ToInt32(timeSpan.TotalMinutes) % 60 == 0)
			{
				totalMinutes = timeSpan.TotalHours;
				textBox.Text = totalMinutes.ToString();
				dropDownList.SelectedValue = "H";
				return;
			}
			totalMinutes = timeSpan.TotalMinutes;
			textBox.Text = totalMinutes.ToString();
			dropDownList.SelectedValue = "M";
		}

		protected void ValidateCancellationFee(object sender, ServerValidateEventArgs e)
		{
			try
			{
				e.IsValid = decimal.Parse(this.schedulingFeeTextBox.Text) >= decimal.Parse(e.Value);
			}
			catch (Exception exception)
			{
				e.IsValid = false;
			}
		}

		protected void ValidateCancellationFee2(object sender, ServerValidateEventArgs e)
		{
			try
			{
				e.IsValid = decimal.Parse(this.seasonalSchedulingFeeTextBox.Text) >= decimal.Parse(e.Value);
			}
			catch (Exception exception)
			{
				e.IsValid = false;
			}
		}

		protected void ValidateDateRange(object sender, ServerValidateEventArgs e)
		{
			try
			{
				int num = int.Parse(this.seasonalStartMonthDropDownList.SelectedValue);
				int num1 = int.Parse(this.seasonalStartDayTextBox.Text);
				int num2 = int.Parse(this.seasonalEndMonthDropDownList.SelectedValue);
				int num3 = int.Parse(this.seasonalEndDayTextBox.Text);
				DateTime dateTime = new DateTime(1999, num, num1);
				DateTime dateTime1 = new DateTime(1999, num2, num3);
				foreach (SeasonalFeeScheduleInfo seasonalFeeScheduleList in this.SeasonalFeeScheduleList)
				{
					if (!this.Conflicts(num, num1, num2, num3, seasonalFeeScheduleList.StartOnMonth, seasonalFeeScheduleList.StartOnDay, seasonalFeeScheduleList.EndByMonth, seasonalFeeScheduleList.EndByDay))
					{
						continue;
					}
					e.IsValid = false;
				}
			}
			catch (Exception exception)
			{
				e.IsValid = false;
			}
		}

		protected void ValidateDateTime(object sender, ServerValidateEventArgs e)
		{
			try
			{
				DateTime.Parse(e.Value);
				e.IsValid = true;
			}
			catch (Exception exception)
			{
				e.IsValid = false;
			}
		}

		protected void ValidateInt32(object sender, ServerValidateEventArgs e)
		{
			try
			{
				int.Parse(e.Value);
				e.IsValid = true;
			}
			catch (Exception exception)
			{
				e.IsValid = false;
			}
		}

		protected void ValidateSeasonalFeeList(object sender, ServerValidateEventArgs e)
		{
			bool flag;
			try
			{
				if (!this.seasonalScheduleRadioButton.Checked)
				{
					e.IsValid = true;
				}
				else
				{
					TimeSpan timeSpan = new TimeSpan();
					foreach (SeasonalFeeScheduleInfo seasonalFeeScheduleList in this.SeasonalFeeScheduleList)
					{
						DateTime dateTime = new DateTime(2001, seasonalFeeScheduleList.EndByMonth, seasonalFeeScheduleList.EndByDay);
						TimeSpan timeSpan1 = timeSpan.Add(dateTime.Subtract(new DateTime(2001, seasonalFeeScheduleList.StartOnMonth, seasonalFeeScheduleList.StartOnDay)));
						timeSpan = timeSpan1.Add(TimeSpan.FromDays(1));
					}
					ServerValidateEventArgs serverValidateEventArg = e;
					if (this.SeasonalFeeScheduleList.Count <= 0)
					{
						flag = false;
					}
					else
					{
						flag = (timeSpan.TotalDays == 365 ? true : timeSpan.TotalDays == 0);
					}
					serverValidateEventArg.IsValid = flag;
				}
			}
			catch (Exception exception)
			{
				e.IsValid = false;
			}
		}
	}
}