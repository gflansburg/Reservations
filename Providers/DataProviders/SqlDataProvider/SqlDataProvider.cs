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

using DotNetNuke.Common.Utilities;
using DotNetNuke.Framework.Providers;
using Microsoft.ApplicationBlocks.Data;
using System;
using System.Data;

namespace Gafware.Modules.Reservations.Data
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// SQL Server implementation of the abstract DataProvider class
    /// 
    /// This concreted data provider class provides the implementation of the abstract methods 
    /// from data dataprovider.cs
    /// 
    /// In most cases you will only modify the Public methods region below.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class SqlDataProvider : DataProvider
    {

        #region Private Members

        private const string ProviderType = "data";
        private const string ModuleQualifier = "Gafware_";

        private readonly ProviderConfiguration _providerConfiguration = ProviderConfiguration.GetProviderConfiguration(ProviderType);
        private readonly string _connectionString;
        private readonly string _providerPath;
        private readonly string _objectQualifier;
        private readonly string _databaseOwner;

        #endregion

        #region Constructors

        public SqlDataProvider()
        {

            // Read the configuration specific information for this provider
            Provider objProvider = (Provider)(_providerConfiguration.Providers[_providerConfiguration.DefaultProvider]);

            // Read the attributes for this provider

            //Get Connection string from web.config
            _connectionString = Config.GetConnectionString();

            if (string.IsNullOrEmpty(_connectionString))
            {
                // Use connection string specified in provider
                _connectionString = objProvider.Attributes["connectionString"];
            }

            _providerPath = objProvider.Attributes["providerPath"];

            _objectQualifier = objProvider.Attributes["objectQualifier"];
            if (!string.IsNullOrEmpty(_objectQualifier) && _objectQualifier.EndsWith("_", StringComparison.Ordinal) == false)
            {
                _objectQualifier += "_";
            }

            _databaseOwner = objProvider.Attributes["databaseOwner"];
            if (!string.IsNullOrEmpty(_databaseOwner) && _databaseOwner.EndsWith(".", StringComparison.Ordinal) == false)
            {
                _databaseOwner += ".";
            }

        }

        #endregion

        #region Properties

        public string ConnectionString
        {
            get
            {
                return _connectionString;
            }
        }

        public string ProviderPath
        {
            get
            {
                return _providerPath;
            }
        }

        public string ObjectQualifier
        {
            get
            {
                return _objectQualifier;
            }
        }

        public string DatabaseOwner
        {
            get
            {
                return _databaseOwner;
            }
        }

        #endregion

        #region Private Methods

        private static object GetNull(object field)
        {
            return Null.GetNull(field, DBNull.Value);
        }

		#endregion

		#region Public Methods

		public override int AddCategory(int TabModuleID, string Name, int CreatedByUserID, DateTime CreatedOnDate, int LastModifiedByUserID, DateTime LastModifiedOnDate)
		{
			object obj = SqlHelper.ExecuteScalar(this.ConnectionString, this.GetFullyQualifiedName("AddCategory"), new object[] { TabModuleID, Name, CreatedByUserID, CreatedOnDate, GetNull(LastModifiedByUserID), GetNull(LastModifiedOnDate) });
			if (Null.IsNull(obj) || obj == DBNull.Value)
			{
				return Null.NullInteger;
			}
			return Convert.ToInt32(obj);
		}

		public override int AddCategorySetting(int CategoryID, string SettingName, string SettingValue)
		{
			object obj = SqlHelper.ExecuteScalar(this.ConnectionString, this.GetFullyQualifiedName("AddCategorySetting"), new object[] { CategoryID, SettingName, SettingValue });
			if (Null.IsNull(obj) || obj == DBNull.Value)
			{
				return Null.NullInteger;
			}
			return Convert.ToInt32(obj);
		}

		public override int AddCustomFieldDefinition(int TabModuleID, string Name, string Label, string Type, string OptionType, string Title, bool AddToPreviousRow, bool IsRequired, bool IsActive, string DefaultValue, int MaxLength, int SortOrder, int CreatedByUserID, DateTime CreatedOnDate, int LastModifiedByUserID, DateTime LastModifiedOnDate, bool HideLabel)
		{
			object obj = SqlHelper.ExecuteScalar(this.ConnectionString, this.GetFullyQualifiedName("AddCustomFieldDefinition"), new object[] { TabModuleID, Name, GetNull(Label), Type, OptionType, GetNull(Title), AddToPreviousRow, IsRequired, IsActive, GetNull(DefaultValue), GetNull(MaxLength), SortOrder, CreatedByUserID, CreatedOnDate, GetNull(LastModifiedByUserID), GetNull(LastModifiedOnDate), GetNull(HideLabel) });
			if (Null.IsNull(obj) || obj == DBNull.Value)
			{
				return Null.NullInteger;
			}
			return Convert.ToInt32(obj);
		}

		public override int AddCustomFieldDefinitionListItem(int CustomFieldDefinitionID, string Text, string Value, int SortOrder, int CreatedByUserID, DateTime CreatedOnDate, int LastModifiedByUserID, DateTime LastModifiedOnDate)
		{
			object obj = SqlHelper.ExecuteScalar(this.ConnectionString, this.GetFullyQualifiedName("AddCustomFieldDefinitionListItem"), new object[] { CustomFieldDefinitionID, Text, Value, SortOrder, CreatedByUserID, CreatedOnDate, GetNull(LastModifiedByUserID), GetNull(LastModifiedOnDate) });
			if (Null.IsNull(obj) || obj == DBNull.Value)
			{
				return Null.NullInteger;
			}
			return Convert.ToInt32(obj);
		}

		public override int AddCustomFieldValue(int CustomFieldDefinitionID, int ReservationID, int PendingPaymentID, int PendingApprovalID, string Value, int CreatedByUserID, DateTime CreatedOnDate, int LastModifiedByUserID, DateTime LastModifiedOnDate)
		{
			object obj = SqlHelper.ExecuteScalar(this.ConnectionString, this.GetFullyQualifiedName("AddCustomFieldValue"), new object[] { CustomFieldDefinitionID, GetNull(ReservationID), GetNull(PendingPaymentID), GetNull(PendingApprovalID), Value, CreatedByUserID, CreatedOnDate, GetNull(LastModifiedByUserID), GetNull(LastModifiedOnDate) });
			if (Null.IsNull(obj) || obj == DBNull.Value)
			{
				return Null.NullInteger;
			}
			return Convert.ToInt32(obj);
		}

		public override int AddICalendar(int ReservationID, string UID, int Sequence, string Organizer)
		{
			object obj = SqlHelper.ExecuteScalar(this.ConnectionString, this.GetFullyQualifiedName("AddICalendar"), new object[] { ReservationID, UID, Sequence, Organizer });
			if (Null.IsNull(obj) || obj == DBNull.Value)
			{
				return Null.NullInteger;
			}
			return Convert.ToInt32(obj);
		}

		public override int AddPendingApproval(int TabModuleID, int PortalID, int ReservationID, int CategoryID, DateTime StartDateTime, int Duration, string FirstName, string LastName, string Email, string Phone, string Description, int Status, int CreatedByUserID, DateTime CreatedOnDate, int LastModifiedByUserID, DateTime LastModifiedOnDate)
		{
			object obj = SqlHelper.ExecuteScalar(this.ConnectionString, this.GetFullyQualifiedName("AddPendingApproval"), new object[] { TabModuleID, PortalID, GetNull(ReservationID), GetNull(CategoryID), StartDateTime, Duration, FirstName, LastName, Email, GetNull(Phone), GetNull(Description), Status, GetNull(CreatedByUserID), CreatedOnDate, GetNull(LastModifiedByUserID), GetNull(LastModifiedOnDate) });
			if (Null.IsNull(obj) || obj == DBNull.Value)
			{
				return Null.NullInteger;
			}
			return Convert.ToInt32(obj);
		}

		public override int AddPendingPayment(int TabModuleID, int PortalID, int ReservationID, int PendingApprovalID, int CategoryID, DateTime StartDateTime, int Duration, string FirstName, string LastName, string Email, string Phone, string Description, decimal Amount, decimal RefundableAmount, string Currency, int Status, int CreatedByUserID, DateTime CreatedOnDate, int LastModifiedByUserID, DateTime LastModifiedOnDate)
		{
			object obj = SqlHelper.ExecuteScalar(this.ConnectionString, this.GetFullyQualifiedName("AddPendingPayment"), new object[] { TabModuleID, PortalID, GetNull(ReservationID), GetNull(PendingApprovalID), GetNull(CategoryID), StartDateTime, Duration, FirstName, LastName, Email, GetNull(Phone), GetNull(Description), Amount, GetNull(RefundableAmount), Currency, Status, GetNull(CreatedByUserID), CreatedOnDate, GetNull(LastModifiedByUserID), GetNull(LastModifiedOnDate) });
			if (Null.IsNull(obj) || obj == DBNull.Value)
			{
				return Null.NullInteger;
			}
			return Convert.ToInt32(obj);
		}

		public override int AddReservation(int TabModuleID, int CategoryID, DateTime StartDateTime, int Duration, string FirstName, string LastName, string Email, string Phone, string Description, bool SendReminder, int SendReminderWhen, bool ReminderSent, bool RequireConfirmation, int RequireConfirmationWhen, bool Confirmed, int CreatedByUserID, DateTime CreatedOnDate, int LastModifiedByUserID, DateTime LastModifiedOnDate, int SendReminderVia)
		{
			object obj = SqlHelper.ExecuteScalar(this.ConnectionString, this.GetFullyQualifiedName("AddReservation"), new object[] { TabModuleID, GetNull(CategoryID), StartDateTime, Duration, FirstName, LastName, Email, GetNull(Phone), GetNull(Description), SendReminder, GetNull(SendReminderWhen), ReminderSent, RequireConfirmation, GetNull(RequireConfirmationWhen), Confirmed, CreatedByUserID, CreatedOnDate, GetNull(LastModifiedByUserID), GetNull(LastModifiedOnDate), GetNull(SendReminderVia) });
			if (Null.IsNull(obj) || obj == DBNull.Value)
			{
				return Null.NullInteger;
			}
			return Convert.ToInt32(obj);
		}

		public override void DeleteCategory(int CategoryID)
		{
			SqlHelper.ExecuteNonQuery(this.ConnectionString, this.GetFullyQualifiedName("DeleteCategory"), new object[] { CategoryID });
		}

		public override void DeleteCategorySetting(int CategoryID, string SettingName)
		{
			SqlHelper.ExecuteNonQuery(this.ConnectionString, this.GetFullyQualifiedName("DeleteCategorySetting"), new object[] { CategoryID, SettingName });
		}

		public override void DeleteCustomFieldDefinition(int CustomFieldDefinitionID)
		{
			SqlHelper.ExecuteNonQuery(this.ConnectionString, this.GetFullyQualifiedName("DeleteCustomFieldDefinition"), new object[] { CustomFieldDefinitionID });
		}

		public override void DeleteCustomFieldDefinitionListItem(int CustomFieldDefinitionListItemID)
		{
			SqlHelper.ExecuteNonQuery(this.ConnectionString, this.GetFullyQualifiedName("DeleteCustomFieldDefinitionListItem"), new object[] { CustomFieldDefinitionListItemID });
		}

		public override void DeleteCustomFieldValue(int CustomFieldValueID)
		{
			SqlHelper.ExecuteNonQuery(this.ConnectionString, this.GetFullyQualifiedName("DeleteCustomFieldValue"), new object[] { CustomFieldValueID });
		}

		public override void DeleteICalendar(int ReservationID)
		{
			SqlHelper.ExecuteNonQuery(this.ConnectionString, this.GetFullyQualifiedName("DeleteICalendar"), new object[] { ReservationID });
		}

		public override void DeletePendingApproval(int PendingApprovalID)
		{
			SqlHelper.ExecuteNonQuery(this.ConnectionString, this.GetFullyQualifiedName("DeletePendingApproval"), new object[] { PendingApprovalID });
		}

		public override void DeletePendingPayment(int PendingPaymentID)
		{
			SqlHelper.ExecuteNonQuery(this.ConnectionString, this.GetFullyQualifiedName("DeletePendingPayment"), new object[] { PendingPaymentID });
		}

		public override void DeleteReservation(int ReservationID)
		{
			SqlHelper.ExecuteNonQuery(this.ConnectionString, this.GetFullyQualifiedName("DeleteReservation"), new object[] { ReservationID });
		}

		public override void DisableRequireConfirmation(int TabModuleID)
		{
			SqlHelper.ExecuteNonQuery(this.ConnectionString, this.GetFullyQualifiedName("DisableRequireConfirmation"), new object[] { TabModuleID });
		}

		public override void DisableSendReminder(int TabModuleID)
		{
			SqlHelper.ExecuteNonQuery(this.ConnectionString, this.GetFullyQualifiedName("DisableSendReminder"), new object[] { TabModuleID });
		}

		public override IDataReader GetActiveCustomFieldDefinitionList(int TabModuleID)
		{
			return SqlHelper.ExecuteReader(this.ConnectionString, this.GetFullyQualifiedName("GetActiveCustomFieldDefinitionList"), new object[] { GetNull(TabModuleID) });
		}

		public override IDataReader GetCategory(int CategoryID)
		{
			return SqlHelper.ExecuteReader(this.ConnectionString, this.GetFullyQualifiedName("GetCategory"), new object[] { CategoryID });
		}

		public override IDataReader GetCategoryList(int TabModuleID)
		{
			return SqlHelper.ExecuteReader(this.ConnectionString, this.GetFullyQualifiedName("GetCategoryList"), new object[] { GetNull(TabModuleID) });
		}

		public override IDataReader GetCategorySetting(int CategoryID, string SettingName)
		{
			return SqlHelper.ExecuteReader(this.ConnectionString, this.GetFullyQualifiedName("GetCategorySetting"), new object[] { CategoryID, SettingName });
		}

		public override IDataReader GetCategorySettingList(int CategoryID)
		{
			return SqlHelper.ExecuteReader(this.ConnectionString, this.GetFullyQualifiedName("GetCategorySettingList"), new object[] { GetNull(CategoryID) });
		}

		public override IDataReader GetCustomFieldDefinition(int CustomFieldDefinitionID)
		{
			return SqlHelper.ExecuteReader(this.ConnectionString, this.GetFullyQualifiedName("GetCustomFieldDefinition"), new object[] { CustomFieldDefinitionID });
		}

		public override IDataReader GetCustomFieldDefinitionList(int TabModuleID)
		{
			return SqlHelper.ExecuteReader(this.ConnectionString, this.GetFullyQualifiedName("GetCustomFieldDefinitionList"), new object[] { GetNull(TabModuleID) });
		}

		public override IDataReader GetCustomFieldDefinitionListItem(int CustomFieldDefinitionListItemID)
		{
			return SqlHelper.ExecuteReader(this.ConnectionString, this.GetFullyQualifiedName("GetCustomFieldDefinitionListItem"), new object[] { CustomFieldDefinitionListItemID });
		}

		public override IDataReader GetCustomFieldDefinitionListItemList(int CustomFieldDefinitionID)
		{
			return SqlHelper.ExecuteReader(this.ConnectionString, this.GetFullyQualifiedName("GetCustomFieldDefinitionListItemList"), new object[] { GetNull(CustomFieldDefinitionID) });
		}

		public override IDataReader GetCustomFieldValue(int CustomFieldValueID)
		{
			return SqlHelper.ExecuteReader(this.ConnectionString, this.GetFullyQualifiedName("GetCustomFieldValue"), new object[] { CustomFieldValueID });
		}

		public override IDataReader GetCustomFieldValueList(int CustomFieldDefinitionID)
		{
			return SqlHelper.ExecuteReader(this.ConnectionString, this.GetFullyQualifiedName("GetCustomFieldValueList"), new object[] { GetNull(CustomFieldDefinitionID) });
		}

		public override IDataReader GetCustomFieldValueListByPendingApprovalID(int PendingApprovalID)
		{
			return SqlHelper.ExecuteReader(this.ConnectionString, this.GetFullyQualifiedName("GetCustomFieldValueListByPendingApprovalID"), new object[] { PendingApprovalID });
		}

		public override IDataReader GetCustomFieldValueListByPendingPaymentID(int PendingPaymentID)
		{
			return SqlHelper.ExecuteReader(this.ConnectionString, this.GetFullyQualifiedName("GetCustomFieldValueListByPendingPaymentID"), new object[] { PendingPaymentID });
		}

		public override IDataReader GetCustomFieldValueListByReservationID(int ReservationID)
		{
			return SqlHelper.ExecuteReader(this.ConnectionString, this.GetFullyQualifiedName("GetCustomFieldValueListByReservationID"), new object[] { ReservationID });
		}

		private string GetFullyQualifiedName(string name)
		{
			return string.Concat(this.DatabaseOwner, this.ObjectQualifier, ModuleQualifier, "Reservations_", name);
		}

		private string GetBaseName(string name)
		{
			return string.Concat(this.DatabaseOwner, this.ObjectQualifier, name);
		}

		public override IDataReader GetICalendar(int ReservationID)
		{
			return SqlHelper.ExecuteReader(this.ConnectionString, this.GetFullyQualifiedName("GetICalendar"), new object[] { ReservationID });
		}

		public override IDataReader GetICalendarList(int ReservationID)
		{
			return SqlHelper.ExecuteReader(this.ConnectionString, this.GetFullyQualifiedName("GetICalendarList"), new object[] { GetNull(ReservationID) });
		}

		public override IDataReader GetPendingApproval(int PendingApprovalID)
		{
			return SqlHelper.ExecuteReader(this.ConnectionString, this.GetFullyQualifiedName("GetPendingApproval"), new object[] { PendingApprovalID });
		}

		public override IDataReader GetPendingApprovalList(int TabModuleID)
		{
			return SqlHelper.ExecuteReader(this.ConnectionString, this.GetFullyQualifiedName("GetPendingApprovalList"), new object[] { GetNull(TabModuleID) });
		}

		public override IDataReader GetPendingPayment(int PendingPaymentID)
		{
			return SqlHelper.ExecuteReader(this.ConnectionString, this.GetFullyQualifiedName("GetPendingPayment"), new object[] { PendingPaymentID });
		}

		public override IDataReader GetPendingPaymentList(int TabModuleID)
		{
			return SqlHelper.ExecuteReader(this.ConnectionString, this.GetFullyQualifiedName("GetPendingPaymentList"), new object[] { GetNull(TabModuleID) });
		}

		public override IDataReader GetReservation(int ReservationID)
		{
			return SqlHelper.ExecuteReader(this.ConnectionString, this.GetFullyQualifiedName("GetReservation"), new object[] { ReservationID });
		}

		public override IDataReader GetReservationList(int TabModuleID)
		{
			return SqlHelper.ExecuteReader(this.ConnectionString, this.GetFullyQualifiedName("GetReservationList"), new object[] { GetNull(TabModuleID) });
		}

		public override IDataReader GetTabModuleSettings(int TabModuleID)
		{
			return SqlHelper.ExecuteReader(this.ConnectionString, this.GetBaseName("GetTabModuleSettings"), new object[] { GetNull(TabModuleID) });
		}

		public override IDataReader GetReservationListByDateRange(int TabModuleID, DateTime From, DateTime To)
		{
			return SqlHelper.ExecuteReader(this.ConnectionString, this.GetFullyQualifiedName("GetReservationListByDateRange"), new object[] { TabModuleID, From, To });
		}

		public override IDataReader GetReservationListByDateRangeAndCategoryID(int TabModuleID, DateTime From, DateTime To, int CategoryID)
		{
			return SqlHelper.ExecuteReader(this.ConnectionString, this.GetFullyQualifiedName("GetReservationListByDateRangeAndCategoryID"), new object[] { TabModuleID, From, To, GetNull(CategoryID) });
		}

		public override IDataReader GetReservationListToSendReminders()
		{
			return SqlHelper.ExecuteReader(this.ConnectionString, this.GetFullyQualifiedName("GetReservationListToSendReminders"), new object[0]);
		}

		public override void UpdateCategory(int TabModuleID, int CategoryID, string Name, int CreatedByUserID, DateTime CreatedOnDate, int LastModifiedByUserID, DateTime LastModifiedOnDate)
		{
			SqlHelper.ExecuteNonQuery(this.ConnectionString, this.GetFullyQualifiedName("UpdateCategory"), new object[] { TabModuleID, CategoryID, Name, CreatedByUserID, CreatedOnDate, GetNull(LastModifiedByUserID), GetNull(LastModifiedOnDate) });
		}

		public override void UpdateCategorySetting(int CategoryID, string SettingName, string SettingValue)
		{
			SqlHelper.ExecuteNonQuery(this.ConnectionString, this.GetFullyQualifiedName("UpdateCategorySetting"), new object[] { CategoryID, SettingName, SettingValue });
		}

		public override void UpdateCustomFieldDefinition(int TabModuleID, int CustomFieldDefinitionID, string Name, string Label, string Type, string OptionType, string Title, bool AddToPreviousRow, bool IsRequired, bool IsActive, string DefaultValue, int MaxLength, int SortOrder, int CreatedByUserID, DateTime CreatedOnDate, int LastModifiedByUserID, DateTime LastModifiedOnDate, bool HideLabel)
		{
			SqlHelper.ExecuteNonQuery(this.ConnectionString, this.GetFullyQualifiedName("UpdateCustomFieldDefinition"), new object[] { TabModuleID, CustomFieldDefinitionID, Name, GetNull(Label), Type, OptionType, GetNull(Title), AddToPreviousRow, IsRequired, IsActive, GetNull(DefaultValue), GetNull(MaxLength), SortOrder, CreatedByUserID, CreatedOnDate, GetNull(LastModifiedByUserID), GetNull(LastModifiedOnDate), GetNull(HideLabel) });
		}

		public override void UpdateCustomFieldDefinitionListItem(int CustomFieldDefinitionID, int CustomFieldDefinitionListItemID, string Text, string Value, int SortOrder, int CreatedByUserID, DateTime CreatedOnDate, int LastModifiedByUserID, DateTime LastModifiedOnDate)
		{
			SqlHelper.ExecuteNonQuery(this.ConnectionString, this.GetFullyQualifiedName("UpdateCustomFieldDefinitionListItem"), new object[] { CustomFieldDefinitionID, CustomFieldDefinitionListItemID, Text, Value, SortOrder, CreatedByUserID, CreatedOnDate, GetNull(LastModifiedByUserID), GetNull(LastModifiedOnDate) });
		}

		public override void UpdateCustomFieldValue(int CustomFieldDefinitionID, int CustomFieldValueID, int ReservationID, int PendingPaymentID, int PendingApprovalID, string Value, int CreatedByUserID, DateTime CreatedOnDate, int LastModifiedByUserID, DateTime LastModifiedOnDate)
		{
			SqlHelper.ExecuteNonQuery(this.ConnectionString, this.GetFullyQualifiedName("UpdateCustomFieldValue"), new object[] { CustomFieldDefinitionID, CustomFieldValueID, GetNull(ReservationID), GetNull(PendingPaymentID), GetNull(PendingApprovalID), Value, CreatedByUserID, CreatedOnDate, GetNull(LastModifiedByUserID), GetNull(LastModifiedOnDate) });
		}

		public override void UpdateICalendar(int ReservationID, string UID, int Sequence, string Organizer)
		{
			SqlHelper.ExecuteNonQuery(this.ConnectionString, this.GetFullyQualifiedName("UpdateICalendar"), new object[] { ReservationID, UID, Sequence, Organizer });
		}

		public override void UpdatePendingApproval(int TabModuleID, int PendingApprovalID, int PortalID, int ReservationID, int CategoryID, DateTime StartDateTime, int Duration, string FirstName, string LastName, string Email, string Phone, string Description, int Status, int CreatedByUserID, DateTime CreatedOnDate, int LastModifiedByUserID, DateTime LastModifiedOnDate)
		{
			SqlHelper.ExecuteNonQuery(this.ConnectionString, this.GetFullyQualifiedName("UpdatePendingApproval"), new object[] { TabModuleID, PendingApprovalID, PortalID, GetNull(ReservationID), GetNull(CategoryID), StartDateTime, Duration, FirstName, LastName, Email, GetNull(Phone), GetNull(Description), Status, GetNull(CreatedByUserID), CreatedOnDate, GetNull(LastModifiedByUserID), GetNull(LastModifiedOnDate) });
		}

		public override void UpdatePendingPayment(int TabModuleID, int PendingPaymentID, int PortalID, int ReservationID, int PendingApprovalID, int CategoryID, DateTime StartDateTime, int Duration, string FirstName, string LastName, string Email, string Phone, string Description, decimal Amount, decimal RefundableAmount, string Currency, int Status, int CreatedByUserID, DateTime CreatedOnDate, int LastModifiedByUserID, DateTime LastModifiedOnDate)
		{
			SqlHelper.ExecuteNonQuery(this.ConnectionString, this.GetFullyQualifiedName("UpdatePendingPayment"), new object[] { TabModuleID, PendingPaymentID, PortalID, GetNull(ReservationID), GetNull(PendingApprovalID), GetNull(CategoryID), StartDateTime, Duration, FirstName, LastName, Email, GetNull(Phone), GetNull(Description), Amount, GetNull(RefundableAmount), Currency, Status, GetNull(CreatedByUserID), CreatedOnDate, GetNull(LastModifiedByUserID), GetNull(LastModifiedOnDate) });
		}

		public override void UpdateReservation(int TabModuleID, int ReservationID, int CategoryID, DateTime StartDateTime, int Duration, string FirstName, string LastName, string Email, string Phone, string Description, bool SendReminder, int SendReminderWhen, bool ReminderSent, bool RequireConfirmation, int RequireConfirmationWhen, bool Confirmed, int CreatedByUserID, DateTime CreatedOnDate, int LastModifiedByUserID, DateTime LastModifiedOnDate, int SendReminderVia)
		{
			SqlHelper.ExecuteNonQuery(this.ConnectionString, this.GetFullyQualifiedName("UpdateReservation"), new object[] { TabModuleID, ReservationID, GetNull(CategoryID), StartDateTime, Duration, FirstName, LastName, Email, GetNull(Phone), GetNull(Description), SendReminder, GetNull(SendReminderWhen), ReminderSent, RequireConfirmation, GetNull(RequireConfirmationWhen), Confirmed, CreatedByUserID, CreatedOnDate, GetNull(LastModifiedByUserID), GetNull(LastModifiedOnDate), GetNull(SendReminderVia) });
		}

		#endregion

	}

}