/*
' Copyright (c) 2021 Gafware
'  All rights reserved.
' 
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
' TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
' THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
' DEALINGS IN THE SOFTWARE.
' 
*/

//using System.Xml;
using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Search;
using Gafware.Modules.Reservations.Data;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Gafware.Modules.Reservations.Components
{
	/// -----------------------------------------------------------------------------
	/// <summary>
	/// The Controller class for Reservations
	/// 
	/// The FeatureController class is defined as the BusinessController in the manifest file (.dnn)
	/// DotNetNuke will poll this class to find out which Interfaces the class implements. 
	/// 
	/// The IPortable interface is used to import/export content from a DNN module
	/// 
	/// The ISearchable interface is used by DNN to index the content of a module
	/// 
	/// The IUpgradeable interface allows module developers to execute code during the upgrade 
	/// process for a module.
	/// 
	/// Below you will find stubbed out implementations of each, uncomment and populate with your own data
	/// </summary>
	/// -----------------------------------------------------------------------------

	public class Controller : IUpgradeable
	{
		public Controller()
		{
		}

		private void ChangeThemeToResponsive(ModuleController moduleController, ModuleInfo module)
		{
			if ((string)module.TabModuleSettings["Theme"] != "Responsive")
			{
				moduleController.UpdateTabModuleSetting(module.TabModuleID, "Theme", "Responsive");
				ModuleController.SynchronizeModule(module.ModuleID);
			}
		}

		public static void DisableRequireConfirmation(int TabModuleID)
		{
			DataProvider.Instance().DisableRequireConfirmation(TabModuleID);
		}

		public static void DisableSendReminder(int TabModuleID)
		{
			DataProvider.Instance().DisableSendReminder(TabModuleID);
		}

		private void FixCreatedDateColumnName(ModuleController moduleController, ModuleInfo module)
		{
			ViewReservationsListSettings viewReservationsListSetting = new ViewReservationsListSettings(null);
			DuplicateReservationsListSettings duplicateReservationsListSetting = new DuplicateReservationsListSettings(null);
			ModerationListSettings moderationListSetting = new ModerationListSettings(null);
			CashierListSettings cashierListSetting = new CashierListSettings(null);
			this.FixCreatedDateColumnName(moduleController, module, viewReservationsListSetting.DISPLAYCOLUMNLIST_KEY);
			this.FixCreatedDateColumnName(moduleController, module, viewReservationsListSetting.SORTORDER_KEY);
			this.FixCreatedDateColumnName(moduleController, module, duplicateReservationsListSetting.DISPLAYCOLUMNLIST_KEY);
			this.FixCreatedDateColumnName(moduleController, module, duplicateReservationsListSetting.SORTORDER_KEY);
			this.FixCreatedDateColumnName(moduleController, module, moderationListSetting.DISPLAYCOLUMNLIST_KEY);
			this.FixCreatedDateColumnName(moduleController, module, moderationListSetting.SORTORDER_KEY);
			this.FixCreatedDateColumnName(moduleController, module, cashierListSetting.DISPLAYCOLUMNLIST_KEY);
			this.FixCreatedDateColumnName(moduleController, module, cashierListSetting.SORTORDER_KEY);
			ModuleController.SynchronizeModule(module.ModuleID);
		}

		private void FixCreatedDateColumnName(ModuleController moduleController, ModuleInfo module, string settingName)
		{
			string item = (string)module.TabModuleSettings[settingName];
			if (!string.IsNullOrEmpty(item) && item.IndexOf("CreatedDate") != -1)
			{
				item = item.Replace("CreatedDate", "CreatedOnDate");
				moduleController.UpdateTabModuleSetting(module.TabModuleID, settingName, item);
			}
		}

		public string UpgradeModule(string Version)
		{
			string str = ComponentBase<IHostController, HostController>.Instance.GetString(ModuleSettings.INSTALLEDON_KEY);
			if (str == null || str == string.Empty)
			{
				IHostController instance = ComponentBase<IHostController, HostController>.Instance;
				DateTime dateTime = DateTime.Now.AddDays(30);
				instance.Update(ModuleSettings.INSTALLEDON_KEY, Helper.Encrypt(dateTime.ToString(CultureInfo.InvariantCulture)), true);
			}
			ReminderSchedulerClient.Install();
			if (Version == "07.00.02")
			{
				try
				{
					ModuleController moduleController = new ModuleController();
					foreach (PortalInfo portal in (new PortalController()).GetPortals())
					{
						foreach (ModuleInfo modulesByDefinition in moduleController.GetModulesByDefinition(portal.PortalID, "Reservations"))
						{
							this.ChangeThemeToResponsive(moduleController, modulesByDefinition);
							this.FixCreatedDateColumnName(moduleController, modulesByDefinition);
						}
					}
				}
				catch (Exception)
				{
				}
			}
			return Version;
		}

	}
}