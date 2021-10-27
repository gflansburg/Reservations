using DotNetNuke.Entities.Modules;
using DotNetNuke.Security.Permissions;
using System;

namespace Gafware.Modules.Reservations
{
	public class ModuleSecurity
	{
		private ModuleInfo moduleInfo;

		public bool HasEditPermissions
		{
			get
			{
				return ModulePermissionController.CanEditModuleContent(this.moduleInfo);
			}
		}

		public ModuleSecurity(ModuleInfo moduleInfo)
		{
			this.moduleInfo = moduleInfo;
		}
	}
}