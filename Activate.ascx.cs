using DotNetNuke.Abstractions;
using DotNetNuke.Application;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins;
using DotNetNuke.UI.Skins.Controls;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Gafware.Modules.Reservations
{
    public partial class Activate : PortalModuleBase
    {
        private Gafware.Modules.Reservations.ModuleSettings _ModuleSettings;

		private readonly INavigationManager _navigationManager;
		
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

		protected string ModuleUrl
		{
			get
			{
				string str = _navigationManager.NavigateURL();
				if (!str.StartsWith("http"))
				{
					str = string.Concat((base.Request.IsSecureConnection ? "https://" : "http://"), base.Request.Url.Host, (base.Request.Url.IsDefaultPort ? string.Empty : string.Concat(":", base.Request.Url.Port)), _navigationManager.NavigateURL());
				}
				return str;
			}
		}

		public Activate()
		{
			_navigationManager = DependencyProvider.GetRequiredService<INavigationManager>();
		}

		protected void ActivateCommandButtonClicked(object sender, EventArgs e)
		{
			try
			{
				if (this.Page.IsValid)
				{
					if (!Helper.ValidateActivationCode(this.activationCodeTextBox.Text.Trim()))
					{
						Skin.AddModuleMessage(this, Localization.GetString("InvalidActivationCode", base.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
					}
					else
					{
						ComponentBase<IHostController, HostController>.Instance.Update(ModuleSettings.ACTIVATIONCODE_KEY, this.activationCodeTextBox.Text.Trim(), true);
						ComponentBase<IHostController, HostController>.Instance.Update(ModuleSettings.EDITION_KEY, Helper.Encrypt(this.editionList.SelectedValue), true);
						DataCache.ClearHostCache(true);
						HttpResponse response = base.Response;
						string empty = string.Empty;
						string[] strArrays = new string[] { "ModuleMessage=ActivationSuccessful", null };
						ModuleMessage.ModuleMessageType moduleMessageType = 0;
						strArrays[1] = string.Concat("ModuleMessageType=", moduleMessageType.ToString());
						response.Redirect(_navigationManager.NavigateURL(empty, strArrays));
					}
				}
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
			}
		}

		protected void Page_Init(object sender, EventArgs e)
		{
			try
			{
				this.SetTheme();
				this.DataBind();
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
				if (!this.Page.IsPostBack)
				{
					if (!ModulePermissionController.HasModuleAccess(DotNetNuke.Security.SecurityAccessLevel.Edit, "EDIT", base.ModuleConfiguration))
					{
						base.Response.Redirect(_navigationManager.NavigateURL(), true);
					}
					Helper.DisplayModuleMessageIfAny(this);
					this.editionList.DataSource = Helper.editions;
					this.editionList.DataBind();
					this.activationFingerprintTextBox.Text = Helper.Encrypt(Helper.GetFingerprint());
					//if (!string.IsNullOrEmpty(base.Request.QueryString["Invoice"]))
					//{
					//	this.activationInvoiceTextBox.Text = base.Request.QueryString["Invoice"];
					//}
					if (!string.IsNullOrEmpty(base.Request.QueryString["Edition"]))
					{
						this.editionList.SelectedValue = base.Request.QueryString["Edition"];
					}
					else
                    {
						try
                        {
							this.editionList.SelectedValue = !String.IsNullOrEmpty(ComponentBase<IHostController, HostController>.Instance.GetString(ModuleSettings.EDITION_KEY)) ? Helper.Decrypt(ComponentBase<IHostController, HostController>.Instance.GetString(ModuleSettings.EDITION_KEY)) : "Standard";
						}
						catch
                        {
                        }
					}
					if (!string.IsNullOrEmpty(base.Request.QueryString["Email"]))
					{
						this.activationEmailTextBox.Text = base.Request.QueryString["Email"];
					}
					else
                    {
						this.activationEmailTextBox.Text = DotNetNuke.Entities.Portals.PortalSettings.Current.Email;
					}
					if (!string.IsNullOrEmpty(base.Request.QueryString["ActivationCode"]))
					{
						this.activationCodeTextBox.Text = base.Request.QueryString["ActivationCode"];
					}
					else
                    {
						try
						{
							this.activationCodeTextBox.Text = Helper.Decrypt(ComponentBase<IHostController, HostController>.Instance.GetString(ModuleSettings.ACTIVATIONCODE_KEY));
						}
						catch
                        {
                        }
					}
				}
			}
			catch (Exception exception)
			{
				Exceptions.ProcessModuleLoadException(this, exception);
			}
		}

		protected void RequestActivationCodeCommandButtonClicked(object sender, EventArgs e)
		{
			try
			{
				if (this.Page.IsValid)
				{
					string str = DotNetNukeContext.Current.Application.Version.ToString();
					string version = base.ModuleConfiguration.DesktopModule.Version;

					string activationCode = Helper.Encrypt(Helper.GetFingerprint().ToLower());
					this.activationCodeTextBox.Text = activationCode.ToUpper();

					/*try
					{
						string empty = string.Empty;
						string activationCode = (new WebService()).GetActivationCode(this.activationInvoiceTextBox.Text.Trim(), this.activationFingerprintTextBox.Text, this.activationEmailTextBox.Text.Trim(), version, str, this.ModuleUrl, out empty);
						this.activationCodeTextBox.Text = activationCode;
						if (!string.IsNullOrEmpty(empty))
						{
							Skin.AddModuleMessage(this, empty, ModuleMessage.ModuleMessageType.RedError);
						}
					}
					catch (SecurityException securityException)
					{
						HttpResponse response = base.Response;
						string[] strArrays = new string[] { "http://www.Gafware.com/Activation.aspx?Invoice=", base.Server.UrlEncode(this.activationInvoiceTextBox.Text.Trim()), "&Fingerprint=", base.Server.UrlEncode(this.activationFingerprintTextBox.Text), "&DotNetNuke=", base.Server.UrlEncode(str), "&Version=", base.Server.UrlEncode(version), "&Email=", base.Server.UrlEncode(this.activationEmailTextBox.Text.Trim()), "&ReturnUrl=", null };
						strArrays[11] = base.Server.UrlEncode(_navigationManager.NavigateURL("Activate", new string[] { string.Concat("mid=", base.ModuleId) }));
						response.Redirect(string.Concat(strArrays));
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						Skin.AddModuleMessage(this, string.Concat(Localization.GetString("ActivationCodeRequestFailed", base.LocalResourceFile), exception.Message), ModuleMessage.ModuleMessageType.RedError);
					}*/
				}
			}
			catch (Exception exception2)
			{
				Exceptions.ProcessModuleLoadException(this, exception2);
			}
		}

		private void SetTheme()
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