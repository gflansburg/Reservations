using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Localization;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Gafware.Modules.Reservations
{
    public partial class RecurrencePatternControl : PortalModuleBase
    {
		public event Gafware.Modules.Reservations.RecurrencePatternSubmitted RecurrencePatternSubmitted;

		protected bool Is24HourClock
		{
			get
			{
				return string.IsNullOrEmpty(CultureInfo.CurrentCulture.DateTimeFormat.AMDesignator);
			}
		}

		public string SubmitImageUrl
		{
			get
			{
				return (string)this.ViewState["SubmitImageUrl"];
			}
			set
			{
				this.ViewState["SubmitImageUrl"] = value;
			}
		}

		public string SubmitText
		{
			get
			{
				return (string)this.ViewState["SubmitText"];
			}
			set
			{
				this.ViewState["SubmitText"] = value;
			}
		}

		public RecurrencePatternControl()
		{
		}

		protected void BindDropDowns()
		{
			this.monthlyDayPositionDropDownList.DataTextField = "LocalizedName";
			this.monthlyDayPositionDropDownList.DataValueField = "Value";
			this.monthlyDayPositionDropDownList.DataSource = Helper.LocalizeEnum(typeof(DayPosition), base.LocalResourceFile);
			this.monthlyDayPositionDropDownList.DataBind();
			this.monthlyDayTypeDropDownList.DataTextField = "LocalizedName";
			this.monthlyDayTypeDropDownList.DataValueField = "Value";
			this.monthlyDayTypeDropDownList.DataSource = Helper.LocalizeEnum(typeof(DayType), base.LocalResourceFile);
			this.monthlyDayTypeDropDownList.DataBind();
			this.yearlyDayPositionDropDownList.DataTextField = "LocalizedName";
			this.yearlyDayPositionDropDownList.DataValueField = "Value";
			this.yearlyDayPositionDropDownList.DataSource = Helper.LocalizeEnum(typeof(DayPosition), base.LocalResourceFile);
			this.yearlyDayPositionDropDownList.DataBind();
			this.yearlyDayTypeDropDownList.DataTextField = "LocalizedName";
			this.yearlyDayTypeDropDownList.DataValueField = "Value";
			this.yearlyDayTypeDropDownList.DataSource = Helper.LocalizeEnum(typeof(DayType), base.LocalResourceFile);
			this.yearlyDayTypeDropDownList.DataBind();
			for (int i = 1; i <= 12; i++)
			{
				ListItemCollection items = this.yearlyMonthDropDownList.Items;
				DateTime dateTime = new DateTime(2000, i, 1);
				items.Add(new ListItem(dateTime.ToString("MMMM"), i.ToString()));
				ListItemCollection listItemCollections = this.yearlyMonthDropDownList2.Items;
				dateTime = new DateTime(2000, i, 1);
				listItemCollections.Add(new ListItem(dateTime.ToString("MMMM"), i.ToString()));
			}
		}

		protected void EnableDisableRecurrenceAllDayEvent(bool enabled)
		{
			this.EnableDisableRecurrenceStartTime(enabled);
			this.EnableDisableRecurrenceEndTime((!enabled ? false : this.recurrenceEndTimeRadioButton.Checked));
			this.EnableDisableRecurrenceDuration((!enabled ? false : this.recurrenceDurationRadioButton.Checked));
			this.EnableDisableRecurrenceStartAndEndTimeRadioButton(enabled);
			this.EnableDisableRecurrenceDurationRadioButton(enabled);
		}

		protected void EnableDisableRecurrenceDuration(bool enabled)
		{
			this.recurrenceDurationDays.Enabled = enabled;
			this.recurrenceDurationHours.Enabled = enabled;
			this.recurrenceDurationMinutes.Enabled = enabled;
			this.recurrenceDurationValidatorsPlaceHolder.Visible = enabled;
		}

		protected void EnableDisableRecurrenceDurationRadioButton(bool enabled)
		{
			this.recurrenceDurationRadioButton.Enabled = enabled;
		}

		protected void EnableDisableRecurrenceEndTime(bool enabled)
		{
			this.recurrenceEndTimeHour.Enabled = enabled;
			this.recurrenceEndTimeMinutes.Enabled = enabled;
			this.recurrenceEndTimeAM.Enabled = enabled;
			this.recurrenceEndTimePM.Enabled = enabled;
			this.recurrenceEndTimeValidatorsPlaceHolder.Visible = enabled;
		}

		protected void EnableDisableRecurrenceStartAndEndTimeRadioButton(bool enabled)
		{
			this.recurrenceEndTimeRadioButton.Enabled = enabled;
		}

		protected void EnableDisableRecurrenceStartTime(bool enabled)
		{
			this.recurrenceStartTimeHour.Enabled = enabled;
			this.recurrenceStartTimeMinutes.Enabled = enabled;
			this.recurrenceStartTimeAM.Enabled = enabled;
			this.recurrenceStartTimePM.Enabled = enabled;
			this.recurrenceStartTimeValidatorsPlaceHolder.Visible = enabled;
		}

		protected void OnRecurrencePatternSubmitted(IRecurrencePattern recurrencePattern)
		{
			if (this.RecurrencePatternSubmitted != null)
			{
				this.RecurrencePatternSubmitted(recurrencePattern);
			}
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			if (!base.IsPostBack)
			{
				this.BindDropDowns();
				this.recurrenceStartTimeMinutes.Text = "00";
				this.recurrenceEndTimeMinutes.Text = "00";
				this.recurrenceStartDate.Text = DateTime.Today.ToShortDateString();
				DayType dayType = DayType.Sunday;
				this.weeklySundayCheckBox.Text = Localization.GetString(dayType.ToString(), base.LocalResourceFile);
				dayType = DayType.Monday;
				this.weeklyMondayCheckBox.Text = Localization.GetString(dayType.ToString(), base.LocalResourceFile);
				dayType = DayType.Tuesday;
				this.weeklyTuesdayCheckBox.Text = Localization.GetString(dayType.ToString(), base.LocalResourceFile);
				dayType = DayType.Wednesday;
				this.weeklyWednesdayCheckBox.Text = Localization.GetString(dayType.ToString(), base.LocalResourceFile);
				dayType = DayType.Thursday;
				this.weeklyThursdayCheckBox.Text = Localization.GetString(dayType.ToString(), base.LocalResourceFile);
				dayType = DayType.Friday;
				this.weeklyFridayCheckBox.Text = Localization.GetString(dayType.ToString(), base.LocalResourceFile);
				dayType = DayType.Saturday;
				this.weeklySaturdayCheckBox.Text = Localization.GetString(dayType.ToString(), base.LocalResourceFile);
				this.EnableDisableRecurrenceAllDayEvent(true);
			}
		}

		protected void Page_PreRender(object sender, EventArgs e)
		{
			this.submitCommandButtonImage.ImageUrl = (string.IsNullOrEmpty(this.SubmitImageUrl) ? "~/images/save.gif" : this.SubmitImageUrl);
			this.submitCommandButtonLabel.Text = (string.IsNullOrEmpty(this.SubmitText) ? Localization.GetString("Submit", base.LocalResourceFile) : this.SubmitText);
			RadioButton radioButton = this.recurrenceStartTimeAM;
			RadioButton radioButton1 = this.recurrenceStartTimePM;
			RadioButton radioButton2 = this.recurrenceEndTimeAM;
			bool is24HourClock = !this.Is24HourClock;
			bool flag = is24HourClock;
			this.recurrenceEndTimePM.Visible = is24HourClock;
			bool flag1 = flag;
			bool flag2 = flag1;
			radioButton2.Visible = flag1;
			bool flag3 = flag2;
			bool flag4 = flag3;
			radioButton1.Visible = flag3;
			radioButton.Visible = flag4;
			this.recurrenceEndTimeLabel.DataBind();
			this.recurrenceDurationLabel.DataBind();
		}

		protected void RecurrenceAllDayEventChanged(object sender, EventArgs e)
		{
			this.EnableDisableRecurrenceAllDayEvent(!this.recurrenceAllDayEvent.Checked);
		}

		protected void RecurrencePatternChanged(object sender, EventArgs e)
		{
			this.dailyPatternPlaceHolder.Visible = this.daily.Checked;
			this.weeklyPatternPlaceHolder.Visible = this.weekly.Checked;
			this.monthlyPatternPlaceHolder.Visible = this.monthly.Checked;
			this.yearlyPatternPlaceHolder.Visible = this.yearly.Checked;
		}

		protected void RecurrenceStartAndEndTimeGroupCheckedChanged(object sender, EventArgs e)
		{
			this.EnableDisableRecurrenceEndTime(this.recurrenceEndTimeRadioButton.Checked);
			this.EnableDisableRecurrenceDuration(this.recurrenceDurationRadioButton.Checked);
		}

		protected void SubmitClicked(object sender, EventArgs e)
		{
			int num;
			int num1;
			if (this.Page.IsValid)
			{
				RecurrencePattern recurrencePattern = new RecurrencePattern()
				{
					StartDate = DateTime.Parse(this.recurrenceStartDate.Text)
				};
				if (this.recurrenceEndDateRadioButton.Checked)
				{
					recurrencePattern.EndDate = new DateTime?(DateTime.Parse(this.recurrenceEndDate.Text));
				}
				else if (this.recurrenceEndAfterRadioButton.Checked)
				{
					recurrencePattern.EndAfter = new int?(int.Parse(this.recurrenceEndAfterTextBox.Text));
				}
				if (this.recurrenceAllDayEvent.Checked)
				{
					recurrencePattern.Duration = new TimeSpan(1, 0, 0, 0);
				}
				else
				{
					int num2 = int.Parse(this.recurrenceStartTimeHour.Text);
					if (!this.Is24HourClock)
					{
						if (this.recurrenceStartTimeAM.Checked)
						{
							num1 = (num2 == 12 ? 0 : num2);
						}
						else
						{
							num1 = (num2 == 12 ? 12 : num2 + 12);
						}
						num2 = num1;
					}
					recurrencePattern.StartTime = new TimeSpan(num2, int.Parse(this.recurrenceStartTimeMinutes.Text), 0);
					if (!this.recurrenceEndTimeRadioButton.Checked)
					{
						recurrencePattern.Duration = new TimeSpan((this.recurrenceDurationDays.Text != string.Empty ? int.Parse(this.recurrenceDurationDays.Text) : 0), (this.recurrenceDurationHours.Text != string.Empty ? int.Parse(this.recurrenceDurationHours.Text) : 0), (this.recurrenceDurationMinutes.Text != string.Empty ? int.Parse(this.recurrenceDurationMinutes.Text) : 0), 0);
					}
					else
					{
						num2 = int.Parse(this.recurrenceEndTimeHour.Text);
						if (!this.Is24HourClock)
						{
							if (this.recurrenceEndTimeAM.Checked)
							{
								num = (num2 == 12 ? 0 : num2);
							}
							else
							{
								num = (num2 == 12 ? 12 : num2 + 12);
							}
							num2 = num;
						}
						TimeSpan timeSpan = new TimeSpan(num2, int.Parse(this.recurrenceEndTimeMinutes.Text), 0);
						recurrencePattern.Duration = timeSpan.Subtract(recurrencePattern.StartTime);
						if (recurrencePattern.Duration.TotalMinutes < 0)
						{
							timeSpan = recurrencePattern.Duration;
							recurrencePattern.Duration = timeSpan.Add(new TimeSpan(1, 0, 0, 0));
						}
					}
				}
				if (this.daily.Checked)
				{
					recurrencePattern.Pattern = Pattern.Daily;
					if (!this.dailyEveryDayRadioButton.Checked)
					{
						recurrencePattern.EveryWeekDay = true;
					}
					else
					{
						recurrencePattern.Every = new int?(int.Parse(this.dailyEveryDayTextBox.Text));
					}
				}
				else if (this.weekly.Checked)
				{
					recurrencePattern.Pattern = Pattern.Weekly;
					recurrencePattern.Every = new int?(int.Parse(this.weeklyEveryWeekTextBox.Text));
					recurrencePattern.Monday = this.weeklyMondayCheckBox.Checked;
					recurrencePattern.Tuesday = this.weeklyTuesdayCheckBox.Checked;
					recurrencePattern.Wednesday = this.weeklyWednesdayCheckBox.Checked;
					recurrencePattern.Thursday = this.weeklyThursdayCheckBox.Checked;
					recurrencePattern.Friday = this.weeklyFridayCheckBox.Checked;
					recurrencePattern.Saturday = this.weeklySaturdayCheckBox.Checked;
					recurrencePattern.Sunday = this.weeklySundayCheckBox.Checked;
				}
				else if (this.monthly.Checked)
				{
					recurrencePattern.Pattern = Pattern.Monthly;
					if (!this.monthlyDayXofEveryYMonth.Checked)
					{
						recurrencePattern.DayPosition = new DayPosition?((DayPosition)int.Parse(this.monthlyDayPositionDropDownList.SelectedValue));
						recurrencePattern.DayType = new DayType?((DayType)int.Parse(this.monthlyDayTypeDropDownList.SelectedValue));
						recurrencePattern.Every = new int?(int.Parse(this.monthlyMonthTextBox2.Text));
					}
					else
					{
						recurrencePattern.Day = new int?(int.Parse(this.monthlyDayTextBox.Text));
						recurrencePattern.Every = new int?(int.Parse(this.monthlyMonthTextBox.Text));
					}
				}
				else if (this.yearly.Checked)
				{
					recurrencePattern.Pattern = Pattern.Yearly;
					if (!this.yearlyMonthDay.Checked)
					{
						recurrencePattern.DayPosition = new DayPosition?((DayPosition)int.Parse(this.yearlyDayPositionDropDownList.SelectedValue));
						recurrencePattern.DayType = new DayType?((DayType)int.Parse(this.yearlyDayTypeDropDownList.SelectedValue));
						recurrencePattern.Month = new int?(int.Parse(this.yearlyMonthDropDownList2.SelectedValue));
					}
					else
					{
						recurrencePattern.Month = new int?(int.Parse(this.yearlyMonthDropDownList.SelectedValue));
						recurrencePattern.Day = new int?(int.Parse(this.yearlyDayTextBox.Text));
					}
				}
				this.OnRecurrencePatternSubmitted(recurrencePattern);
				this.recurrenceStartTimeHour.Text = "";
				this.recurrenceStartTimeMinutes.Text = "00";
				this.recurrenceStartTimeAM.Checked = true;
				this.recurrenceStartTimePM.Checked = false;
				this.recurrenceEndTimeHour.Text = "";
				this.recurrenceEndTimeMinutes.Text = "00";
				this.recurrenceEndTimeAM.Checked = true;
				this.recurrenceEndTimePM.Checked = false;
				this.recurrenceAllDayEvent.Checked = false;
				this.RecurrenceAllDayEventChanged(sender, e);
				this.recurrenceEndTimeRadioButton.Checked = true;
				this.recurrenceDurationRadioButton.Checked = false;
				this.RecurrenceStartAndEndTimeGroupCheckedChanged(sender, e);
				this.recurrenceDurationDays.Text = "";
				this.recurrenceDurationHours.Text = "";
				this.recurrenceDurationMinutes.Text = "";
				this.daily.Checked = false;
				this.weekly.Checked = true;
				this.monthly.Checked = false;
				this.yearly.Checked = false;
				this.RecurrencePatternChanged(sender, e);
				this.dailyEveryDayRadioButton.Checked = true;
				this.dailyEveryWeekDayRadioButton.Checked = false;
				this.dailyEveryDayTextBox.Text = "1";
				this.weeklyEveryWeekTextBox.Text = "1";
				this.weeklySundayCheckBox.Checked = false;
				this.weeklyMondayCheckBox.Checked = false;
				this.weeklyTuesdayCheckBox.Checked = false;
				this.weeklyWednesdayCheckBox.Checked = false;
				this.weeklyThursdayCheckBox.Checked = false;
				this.weeklyFridayCheckBox.Checked = false;
				this.weeklySaturdayCheckBox.Checked = false;
				this.monthlyDayXofEveryYMonth.Checked = true;
				this.monthlyXYofEveryZ.Checked = false;
				this.monthlyDayTextBox.Text = "1";
				this.monthlyMonthTextBox.Text = "1";
				this.monthlyDayPositionDropDownList.SelectedValue = 1.ToString();
				this.monthlyDayTypeDropDownList.SelectedValue = 1.ToString();
				this.monthlyMonthTextBox2.Text = "1";
				this.yearlyMonthDay.Checked = true;
				this.yearlyXYofZ.Checked = false;
				this.yearlyMonthDropDownList.SelectedValue = "1";
				this.yearlyDayTextBox.Text = "1";
				this.yearlyDayPositionDropDownList.SelectedValue = 1.ToString();
				this.yearlyDayTypeDropDownList.SelectedValue = 1.ToString();
				this.yearlyMonthDropDownList2.SelectedValue = "1";
				this.recurrenceStartDate.Text = DateTime.Today.ToShortDateString();
				this.recurrenceNoEndDateRadioButton.Checked = true;
				this.recurrenceEndAfterRadioButton.Checked = false;
				this.recurrenceEndDateRadioButton.Checked = false;
				this.recurrenceEndAfterTextBox.Text = "10";
				this.recurrenceEndDate.Text = "";
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

		protected void ValidateHour(object sender, ServerValidateEventArgs e)
		{
			try
			{
				int num = int.Parse(e.Value);
				if (!this.Is24HourClock)
				{
					e.IsValid = (num <= 0 ? false : num <= 12);
				}
				else
				{
					e.IsValid = (num < 0 ? false : num <= 23);
				}
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

		protected void ValidateMinutes(object sender, ServerValidateEventArgs e)
		{
			try
			{
				int num = int.Parse(e.Value);
				e.IsValid = (num < 0 ? false : num <= 59);
			}
			catch (Exception exception)
			{
				e.IsValid = false;
			}
		}

		protected void ValidateRecurrenceDuration(object sender, ServerValidateEventArgs e)
		{
			e.IsValid = (this.recurrenceDurationDays.Text != string.Empty || this.recurrenceDurationHours.Text != string.Empty ? true : this.recurrenceDurationMinutes.Text != string.Empty);
		}

		protected void ValidateRecurrenceEndAfterRadioButton(object sender, ServerValidateEventArgs e)
		{
			e.IsValid = (!this.recurrenceEndAfterRadioButton.Checked ? true : this.recurrenceEndAfterTextBox.Text.Trim().Length != 0);
		}

		protected void ValidateRecurrenceEndDate(object sender, ServerValidateEventArgs e)
		{
			try
			{
				if (!this.recurrenceEndDateRadioButton.Checked)
				{
					e.IsValid = true;
				}
				else
				{
					DateTime dateTime = DateTime.Parse(this.recurrenceStartDate.Text);
					DateTime dateTime1 = DateTime.Parse(this.recurrenceEndDate.Text);
					e.IsValid = dateTime <= dateTime1;
				}
			}
			catch (Exception exception)
			{
				e.IsValid = true;
			}
		}

		protected void ValidateRecurrenceEndTime(object sender, ServerValidateEventArgs e)
		{
			int num;
			int num1;
			try
			{
				DateTime dateTime = DateTime.Parse(this.recurrenceStartDate.Text);
				DateTime dateTime1 = DateTime.Parse(this.recurrenceStartDate.Text);
				if (!this.recurrenceAllDayEvent.Checked)
				{
					int num2 = int.Parse(this.recurrenceStartTimeHour.Text);
					if (this.recurrenceStartTimeAM.Checked)
					{
						num = (num2 == 12 ? 0 : num2);
					}
					else
					{
						num = (num2 == 12 ? 12 : num2 + 12);
					}
					num2 = num;
					dateTime = dateTime.Add(new TimeSpan(num2, int.Parse(this.recurrenceStartTimeMinutes.Text), 0));
					num2 = int.Parse(this.recurrenceEndTimeHour.Text);
					if (this.recurrenceEndTimeAM.Checked)
					{
						num1 = (num2 == 12 ? 0 : num2);
					}
					else
					{
						num1 = (num2 == 12 ? 12 : num2 + 12);
					}
					num2 = num1;
					dateTime1 = dateTime1.Add(new TimeSpan(num2, int.Parse(this.recurrenceEndTimeMinutes.Text), 0));
				}
				e.IsValid = dateTime <= dateTime1;
			}
			catch (Exception exception)
			{
				e.IsValid = true;
			}
		}

		protected void ValidateRecurrenceEndTimeNotEmpty(object sender, ServerValidateEventArgs e)
		{
			e.IsValid = (!this.recurrenceEndDateRadioButton.Checked ? true : this.recurrenceEndDate.Text != string.Empty);
		}

		protected void ValidateWeeklyDay(object sender, ServerValidateEventArgs e)
		{
			e.IsValid = (this.weeklySundayCheckBox.Checked || this.weeklyMondayCheckBox.Checked || this.weeklyTuesdayCheckBox.Checked || this.weeklyWednesdayCheckBox.Checked || this.weeklyThursdayCheckBox.Checked || this.weeklyFridayCheckBox.Checked ? true : this.weeklySaturdayCheckBox.Checked);
		}
	}
}