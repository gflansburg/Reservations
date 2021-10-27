using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using System;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.UI;

namespace Gafware.Modules.Reservations
{
    public partial class IPN : System.Web.UI.Page
    {
		public IPN()
		{
		}

		private void Page_Load(object sender, EventArgs e)
		{
			PendingPaymentStatus pendingPaymentStatu;
			try
			{
				PendingPaymentInfo pendingPayment = (new PendingPaymentController()).GetPendingPayment(int.Parse(base.Request.Params["item_number"]));
				ModuleSettings moduleSetting = new ModuleSettings(pendingPayment.PortalID, pendingPayment.TabModuleID);
				if (Helper.GetEdition(moduleSetting.ActivationCode) != "Standard")
				{
					HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(string.Concat(moduleSetting.PayPalUrl, (moduleSetting.PayPalUrl.EndsWith("/") ? string.Empty : "/"), "cgi-bin/webscr"));
					httpWebRequest.AllowAutoRedirect = false;
					httpWebRequest.Method = "POST";
					httpWebRequest.ContentType = "application/x-www-form-urlencoded";
					Stream requestStream = httpWebRequest.GetRequestStream();
					byte[] bytes = Encoding.UTF8.GetBytes(string.Concat(base.Request.Form.ToString(), "&cmd=_notify-validate"));
					requestStream.Write(bytes, 0, (int)bytes.Length);
					requestStream.Close();
					HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse();
					if (response.StatusCode == HttpStatusCode.OK && (new StreamReader(response.GetResponseStream(), Encoding.UTF8)).ReadLine() == "VERIFIED")
					{
						if (base.Request.Params["payment_status"] == "Completed")
						{
							pendingPaymentStatu = PendingPaymentStatus.Paid;
						}
						else if (base.Request.Params["payment_status"] == "Pending")
						{
							pendingPaymentStatu = PendingPaymentStatus.Held;
						}
						else
						{
							pendingPaymentStatu = (base.Request.Params["payment_status"] == "Refunded" ? PendingPaymentStatus.Refunded : PendingPaymentStatus.Void);
						}
						PendingPaymentStatus pendingPaymentStatu1 = pendingPaymentStatu;
						decimal num = decimal.Parse(base.Request.Params["mc_gross"], CultureInfo.InvariantCulture);
						string str = Path.Combine(string.Concat(this.TemplateSourceDirectory, "/"), string.Concat(Localization.LocalResourceDirectory, "/MakeReservation"));
						PaymentNotificationHelper.ProcessPaymentNotification(moduleSetting, str, pendingPayment, pendingPaymentStatu1, num);
					}
					response.Close();
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				Exceptions.LogException(new ModuleLoadException(string.Concat("IPN, Paypal Exception: ", exception.Message, " at: ", exception.Source)));
			}
		}
	}
}