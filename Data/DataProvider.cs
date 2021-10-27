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
using System;
using System.Data;


namespace Gafware.Modules.Reservations.Data
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// An abstract class for the data access layer
    /// 
    /// The abstract data provider provides the methods that a control data provider (sqldataprovider)
    /// must implement. You'll find two commented out examples in the Abstract methods region below.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public abstract class DataProvider
    {

        #region Shared/Static Methods

        private static DataProvider provider;

        // return the provider
        public static DataProvider Instance()
        {
            if (provider == null)
            {
                const string assembly = "Gafware.Modules.Reservations.Data.SqlDataprovider,Gafware.Reservations";
                Type objectType = Type.GetType(assembly, true, true);

                provider = (DataProvider)Activator.CreateInstance(objectType);
                DataCache.SetCache(objectType.FullName, provider);
            }

            return provider;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Not returning class state information")]
        public static IDbConnection GetConnection()
        {
            const string providerType = "data";
            ProviderConfiguration _providerConfiguration = ProviderConfiguration.GetProviderConfiguration(providerType);

            Provider objProvider = ((Provider)_providerConfiguration.Providers[_providerConfiguration.DefaultProvider]);
            string _connectionString;
            if (!String.IsNullOrEmpty(objProvider.Attributes["connectionStringName"]) && !String.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings[objProvider.Attributes["connectionStringName"]]))
            {
                _connectionString = System.Configuration.ConfigurationManager.AppSettings[objProvider.Attributes["connectionStringName"]];
            }
            else
            {
                _connectionString = objProvider.Attributes["connectionString"];
            }

            IDbConnection newConnection = new System.Data.SqlClient.SqlConnection();
            newConnection.ConnectionString = _connectionString.ToString();
            newConnection.Open();
            return newConnection;
        }

        #endregion

        #region Abstract methods

        public abstract int AddCategory(int TabModuleID, string Name, int CreatedByUserID, DateTime CreatedOnDate, int LastModifiedByUserID, DateTime LastModifiedOnDate);

        public abstract int AddCategorySetting(int CategoryID, string SettingName, string SettingValue);

        public abstract int AddCustomFieldDefinition(int TabModuleID, string Name, string Label, string Type, string OptionType, string Title, bool AddToPreviousRow, bool IsRequired, bool IsActive, string DefaultValue, int MaxLength, int SortOrder, int CreatedByUserID, DateTime CreatedOnDate, int LastModifiedByUserID, DateTime LastModifiedOnDate, bool HideLabel);

        public abstract int AddCustomFieldDefinitionListItem(int CustomFieldDefinitionID, string Text, string Value, int SortOrder, int CreatedByUserID, DateTime CreatedOnDate, int LastModifiedByUserID, DateTime LastModifiedOnDate);

        public abstract int AddCustomFieldValue(int CustomFieldDefinitionID, int ReservationID, int PendingPaymentID, int PendingApprovalID, string Value, int CreatedByUserID, DateTime CreatedOnDate, int LastModifiedByUserID, DateTime LastModifiedOnDate);

        public abstract int AddICalendar(int ReservationID, string UID, int Sequence, string Organizer);

        public abstract int AddPendingApproval(int TabModuleID, int PortalID, int ReservationID, int CategoryID, DateTime StartDateTime, int Duration, string FirstName, string LastName, string Email, string Phone, string Description, int Status, int CreatedByUserID, DateTime CreatedOnDate, int LastModifiedByUserID, DateTime LastModifiedOnDate);

        public abstract int AddPendingPayment(int TabModuleID, int PortalID, int ReservationID, int PendingApprovalID, int CategoryID, DateTime StartDateTime, int Duration, string FirstName, string LastName, string Email, string Phone, string Description, decimal Amount, decimal RefundableAmount, string Currency, int Status, int CreatedByUserID, DateTime CreatedOnDate, int LastModifiedByUserID, DateTime LastModifiedOnDate);

        public abstract int AddReservation(int TabModuleID, int CategoryID, DateTime StartDateTime, int Duration, string FirstName, string LastName, string Email, string Phone, string Description, bool SendReminder, int SendReminderWhen, bool ReminderSent, bool RequireConfirmation, int RequireConfirmationWhen, bool Confirmed, int CreatedByUserID, DateTime CreatedOnDate, int LastModifiedByUserID, DateTime LastModifiedOnDate, int SendReminderVia);

        public abstract void DeleteCategory(int CategoryID);

        public abstract void DeleteCategorySetting(int CategoryID, string SettingName);

        public abstract void DeleteCustomFieldDefinition(int CustomFieldDefinitionID);

        public abstract void DeleteCustomFieldDefinitionListItem(int CustomFieldDefinitionListItemID);

        public abstract void DeleteCustomFieldValue(int CustomFieldValueID);

        public abstract void DeleteICalendar(int ReservationID);

        public abstract void DeletePendingApproval(int PendingApprovalID);

        public abstract void DeletePendingPayment(int PendingPaymentID);

        public abstract void DeleteReservation(int ReservationID);

        public abstract void DisableRequireConfirmation(int TabModuleID);

        public abstract void DisableSendReminder(int TabModuleID);

        public abstract IDataReader GetActiveCustomFieldDefinitionList(int TabModuleID);

        public abstract IDataReader GetCategory(int CategoryID);

        public abstract IDataReader GetCategoryList(int TabModuleID);

        public abstract IDataReader GetCategorySetting(int CategoryID, string SettingName);

        public abstract IDataReader GetCategorySettingList(int CategoryID);

        public abstract IDataReader GetCustomFieldDefinition(int CustomFieldDefinitionID);

        public abstract IDataReader GetCustomFieldDefinitionList(int TabModuleID);

        public abstract IDataReader GetCustomFieldDefinitionListItem(int CustomFieldDefinitionListItemID);

        public abstract IDataReader GetCustomFieldDefinitionListItemList(int CustomFieldDefinitionID);

        public abstract IDataReader GetCustomFieldValue(int CustomFieldValueID);

        public abstract IDataReader GetCustomFieldValueList(int CustomFieldDefinitionID);

        public abstract IDataReader GetCustomFieldValueListByPendingApprovalID(int PendingApprovalID);

        public abstract IDataReader GetCustomFieldValueListByPendingPaymentID(int PendingPaymentID);

        public abstract IDataReader GetCustomFieldValueListByReservationID(int EventID);

        public abstract IDataReader GetICalendar(int ReservationID);

        public abstract IDataReader GetICalendarList(int ReservationID);

        public abstract IDataReader GetPendingApproval(int PendingApprovalID);

        public abstract IDataReader GetPendingApprovalList(int TabModuleID);

        public abstract IDataReader GetPendingPayment(int PendingPaymentID);

        public abstract IDataReader GetPendingPaymentList(int TabModuleID);

        public abstract IDataReader GetReservation(int ReservationID);

        public abstract IDataReader GetReservationList(int TabModuleID);

        public abstract IDataReader GetReservationListByDateRange(int TabModuleID, DateTime From, DateTime To);

        public abstract IDataReader GetReservationListByDateRangeAndCategoryID(int TabModuleID, DateTime From, DateTime To, int CategoryID);

        public abstract IDataReader GetReservationListToSendReminders();

        public abstract void UpdateCategory(int TabModuleID, int CategoryID, string Name, int CreatedByUserID, DateTime CreatedOnDate, int LastModifiedByUserID, DateTime LastModifiedOnDate);

        public abstract void UpdateCategorySetting(int CategoryID, string SettingName, string SettingValue);

        public abstract void UpdateCustomFieldDefinition(int TabModuleID, int CustomFieldDefinitionID, string Name, string Label, string Type, string OptionType, string Title, bool AddToPreviousRow, bool IsRequired, bool IsActive, string DefaultValue, int MaxLength, int SortOrder, int CreatedByUserID, DateTime CreatedOnDate, int LastModifiedByUserID, DateTime LastModifiedOnDate, bool HideLabel);

        public abstract void UpdateCustomFieldDefinitionListItem(int CustomFieldDefinitionID, int CustomFieldDefinitionListItemID, string Text, string Value, int SortOrder, int CreatedByUserID, DateTime CreatedOnDate, int LastModifiedByUserID, DateTime LastModifiedOnDate);

        public abstract void UpdateCustomFieldValue(int CustomFieldDefinitionID, int CustomFieldValueID, int ReservationID, int PendingPaymentID, int PendingApprovalID, string Value, int CreatedByUserID, DateTime CreatedOnDate, int LastModifiedByUserID, DateTime LastModifiedOnDate);

        public abstract void UpdateICalendar(int ReservationID, string UID, int Sequence, string Organizer);

        public abstract void UpdatePendingApproval(int TabModuleID, int PendingApprovalID, int PortalID, int ReservationID, int CategoryID, DateTime StartDateTime, int Duration, string FirstName, string LastName, string Email, string Phone, string Description, int Status, int CreatedByUserID, DateTime CreatedOnDate, int LastModifiedByUserID, DateTime LastModifiedOnDate);

        public abstract void UpdatePendingPayment(int TabModuleID, int PendingPaymentID, int PortalID, int ReservationID, int PendingApprovalID, int CategoryID, DateTime StartDateTime, int Duration, string FirstName, string LastName, string Email, string Phone, string Description, decimal Amount, decimal RefundableAmount, string Currency, int Status, int CreatedByUserID, DateTime CreatedOnDate, int LastModifiedByUserID, DateTime LastModifiedOnDate);

        public abstract void UpdateReservation(int TabModuleID, int ReservationID, int CategoryID, DateTime StartDateTime, int Duration, string FirstName, string LastName, string Email, string Phone, string Description, bool SendReminder, int SendReminderWhen, bool ReminderSent, bool RequireConfirmation, int RequireConfirmationWhen, bool Confirmed, int CreatedByUserID, DateTime CreatedOnDate, int LastModifiedByUserID, DateTime LastModifiedOnDate, int SendReminderVia);

        #endregion

    }

}