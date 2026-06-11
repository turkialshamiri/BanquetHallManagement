namespace BanquetHallManagement.Permissions;



public static class BanquetHallManagementPermissions

{

    public const string GroupName = "BanquetHallManagement";



    public static class Dashboard

    {

        public const string Default = GroupName + ".Dashboard";

        public const string ViewRevenue = Default + ".ViewRevenue";

    }



    public static class Halls

    {

        public const string Default = GroupName + ".Halls";

        public const string Create = Default + ".Create";

        public const string Update = Default + ".Update";

        public const string Delete = Default + ".Delete";

    }



    public static class Customers

    {

        public const string Default = GroupName + ".Customers";

        public const string Create = Default + ".Create";

        public const string Update = Default + ".Update";

        public const string Delete = Default + ".Delete";

    }



    public static class Services

    {

        public const string Default = GroupName + ".Services";

        public const string Create = Default + ".Create";

        public const string Update = Default + ".Update";

        public const string Delete = Default + ".Delete";

    }



    public static class Reservations

    {

        public const string Default = GroupName + ".Reservations";

        public const string Create = Default + ".Create";

        public const string Update = Default + ".Update";

        public const string Delete = Default + ".Delete";

        public const string Confirm = Default + ".Confirm";

        public const string Cancel = Default + ".Cancel";

        public const string Complete = Default + ".Complete";

        public const string RecordPayment = Default + ".RecordPayment";

        public const string ConfirmHallEntry = Default + ".ConfirmHallEntry";

    }



    public static class Finance

    {

        public const string Default = GroupName + ".Finance";

        public const string ViewAccounts = Default + ".ViewAccounts";

        public const string ViewJournalEntries = Default + ".ViewJournalEntries";

        public const string ViewReports = Default + ".ViewReports";

        public const string PaymentsCreate = Default + ".Payments.Create";

        public const string PaymentsView = Default + ".Payments.View";

        public const string InvoicesView = Default + ".Invoices.View";

        public const string InvoicesPrint = Default + ".Invoices.Print";

        public const string HallAccessCardsView = Default + ".HallAccessCards.View";

        public const string HallAccessCardsPrint = Default + ".HallAccessCards.Print";

    }



    public static class Reports

    {

        public const string Default = GroupName + ".Reports";

    }



    public static class Users

    {

        public const string Default = GroupName + ".Users";

        public const string Create = Default + ".Create";

        public const string Update = Default + ".Update";

        public const string Delete = Default + ".Delete";

        public const string ResetPassword = Default + ".ResetPassword";

        public const string ManageRoles = Default + ".ManageRoles";

        public const string Activate = Default + ".Activate";

        public const string Deactivate = Default + ".Deactivate";

    }

}


    