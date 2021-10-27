using AuthorizeNet;
using DotNetNuke.Common;
using DotNetNuke.Entities.Content;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins.Controls;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Gafware.Modules.Reservations
{
    public partial class AuthorizeNetSIMRelayResponse : System.Web.UI.Page
    {
        private AuthorizeNet.SIMResponse _SIMResponse;

        private Gafware.Modules.Reservations.PendingPaymentInfo _PendingPaymentInfo;

		private Gafware.Modules.Reservations.PendingPaymentInfo PendingPaymentInfo
		{
			get
			{
				if (this._PendingPaymentInfo == null)
				{
					this._PendingPaymentInfo = (new PendingPaymentController()).GetPendingPayment(int.Parse(this.SIMResponse.InvoiceNumber));
				}
				return this._PendingPaymentInfo;
			}
		}

		protected string ReceiptUrl
		{
			get
			{
				int pendingPaymentID;
				PendingPaymentStatus status;
				if (this.Status == PendingPaymentStatus.Paid)
				{
					int tabID = (new ModuleController()).GetTabModule(this.PendingPaymentInfo.TabModuleID).TabID;
					string empty = string.Empty;
					string[] strArrays = new string[3];
					pendingPaymentID = this.PendingPaymentInfo.PendingPaymentID;
					strArrays[0] = string.Concat("PendingPaymentID=", pendingPaymentID.ToString());
					status = this.Status;
					strArrays[1] = string.Concat("Status=", status.ToString());
					strArrays[2] = string.Concat("Email=", this.PendingPaymentInfo.Email);
					return Globals.NavigateURL(tabID, empty, strArrays);
				}
				if (this.Status != PendingPaymentStatus.Held)
				{
					return Globals.NavigateURL((new ModuleController()).GetTabModule(this.PendingPaymentInfo.TabModuleID).TabID, string.Empty, new string[] { string.Concat("ModuleMessage=", base.Server.UrlEncode(this.SIMResponse.Message)), string.Concat("ModuleMessageType=", (ModuleMessage.ModuleMessageType)2) });
				}
				int num = (new ModuleController()).GetTabModule(this.PendingPaymentInfo.TabModuleID).TabID;
				string str = string.Empty;
				string[] strArrays1 = new string[5];
				pendingPaymentID = this.PendingPaymentInfo.PendingPaymentID;
				strArrays1[0] = string.Concat("PendingPaymentID=", pendingPaymentID.ToString());
				status = this.Status;
				strArrays1[1] = string.Concat("Status=", status.ToString());
				strArrays1[2] = string.Concat("Email=", this.PendingPaymentInfo.Email);
				strArrays1[3] = string.Concat("ModuleMessage=", base.Server.UrlEncode(this.SIMResponse.Message));
				strArrays1[4] = string.Concat("ModuleMessageType=", (ModuleMessage.ModuleMessageType)1);
				return Globals.NavigateURL(num, str, strArrays1);
			}
		}

		private AuthorizeNet.SIMResponse SIMResponse
		{
			get
			{
				if (this._SIMResponse == null)
				{
					this._SIMResponse = new AuthorizeNet.SIMResponse(base.Request.Params);
				}
				return this._SIMResponse;
			}
		}

		private PendingPaymentStatus Status
		{
			get;
			set;
		}

		public AuthorizeNetSIMRelayResponse()
		{
		}

		private void Page_Load(object sender, EventArgs e)
		{
			PendingPaymentStatus pendingPaymentStatu;
			try
			{
				ModuleSettings moduleSetting = new ModuleSettings(this.PendingPaymentInfo.PortalID, this.PendingPaymentInfo.TabModuleID);
				if (!(Helper.GetEdition(moduleSetting.ActivationCode) == "Standard") && this.SIMResponse.Validate(moduleSetting.AuthorizeNetMerchantHash, moduleSetting.AuthorizeNetApiLogin))
				{
					if (this.SIMResponse.ResponseCode == "1")
					{
						pendingPaymentStatu = PendingPaymentStatus.Paid;
					}
					else
					{
						pendingPaymentStatu = (this.SIMResponse.ResponseCode == "4" ? PendingPaymentStatus.Held : PendingPaymentStatus.Void);
					}
					this.Status = pendingPaymentStatu;
					string str = Path.Combine(string.Concat(this.TemplateSourceDirectory, "/"), string.Concat(Localization.LocalResourceDirectory, "/MakeReservation"));
					this.label.Text = Localization.GetString("AuthorizeNetSIMReceiptLinkLabel", str);
					PaymentNotificationHelper.ProcessPaymentNotification(moduleSetting, str, this.PendingPaymentInfo, this.Status, this.SIMResponse.Amount);
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				Exceptions.LogException(new ModuleLoadException(string.Concat("Auhtorize.Net SIM Relay Response Exception: ", exception.Message, " at: ", exception.Source)));
			}
		}
	}
}