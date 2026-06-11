using BanquetHallManagement.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace BanquetHallManagement.Permissions;

public class BanquetHallManagementPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var group = context.AddGroup(BanquetHallManagementPermissions.GroupName, L("Permission:BanquetHallManagement"));

        var dashboard = group.AddPermission(BanquetHallManagementPermissions.Dashboard.Default, L("Permission:Dashboard"));
        dashboard.AddChild(BanquetHallManagementPermissions.Dashboard.ViewRevenue, L("Permission:Dashboard.ViewRevenue"));

        var halls = group.AddPermission(BanquetHallManagementPermissions.Halls.Default, L("Permission:Halls"));
        halls.AddChild(BanquetHallManagementPermissions.Halls.Create, L("Permission:Halls.Create"));
        halls.AddChild(BanquetHallManagementPermissions.Halls.Update, L("Permission:Halls.Update"));
        halls.AddChild(BanquetHallManagementPermissions.Halls.Delete, L("Permission:Halls.Delete"));

        var customers = group.AddPermission(BanquetHallManagementPermissions.Customers.Default, L("Permission:Customers"));
        customers.AddChild(BanquetHallManagementPermissions.Customers.Create, L("Permission:Customers.Create"));
        customers.AddChild(BanquetHallManagementPermissions.Customers.Update, L("Permission:Customers.Update"));
        customers.AddChild(BanquetHallManagementPermissions.Customers.Delete, L("Permission:Customers.Delete"));

        var services = group.AddPermission(BanquetHallManagementPermissions.Services.Default, L("Permission:Services"));
        services.AddChild(BanquetHallManagementPermissions.Services.Create, L("Permission:Services.Create"));
        services.AddChild(BanquetHallManagementPermissions.Services.Update, L("Permission:Services.Update"));
        services.AddChild(BanquetHallManagementPermissions.Services.Delete, L("Permission:Services.Delete"));

        var reservations = group.AddPermission(BanquetHallManagementPermissions.Reservations.Default, L("Permission:Reservations"));
        reservations.AddChild(BanquetHallManagementPermissions.Reservations.Create, L("Permission:Reservations.Create"));
        reservations.AddChild(BanquetHallManagementPermissions.Reservations.Update, L("Permission:Reservations.Update"));
        reservations.AddChild(BanquetHallManagementPermissions.Reservations.Delete, L("Permission:Reservations.Delete"));
        reservations.AddChild(BanquetHallManagementPermissions.Reservations.Confirm, L("Permission:Reservations.Confirm"));
        reservations.AddChild(BanquetHallManagementPermissions.Reservations.Cancel, L("Permission:Reservations.Cancel"));
        reservations.AddChild(BanquetHallManagementPermissions.Reservations.Complete, L("Permission:Reservations.Complete"));
        reservations.AddChild(BanquetHallManagementPermissions.Reservations.RecordPayment, L("Permission:Reservations.RecordPayment"));
        reservations.AddChild(BanquetHallManagementPermissions.Reservations.ConfirmHallEntry, L("Permission:Reservations.ConfirmHallEntry"));

        var finance = group.AddPermission(BanquetHallManagementPermissions.Finance.Default, L("Permission:Finance"));
        finance.AddChild(BanquetHallManagementPermissions.Finance.ViewAccounts, L("Permission:Finance.ViewAccounts"));
        finance.AddChild(BanquetHallManagementPermissions.Finance.ViewJournalEntries, L("Permission:Finance.ViewJournalEntries"));
        finance.AddChild(BanquetHallManagementPermissions.Finance.ViewReports, L("Permission:Finance.ViewReports"));
        finance.AddChild(BanquetHallManagementPermissions.Finance.PaymentsCreate, L("Permission:Finance.Payments.Create"));
        finance.AddChild(BanquetHallManagementPermissions.Finance.PaymentsView, L("Permission:Finance.Payments.View"));
        finance.AddChild(BanquetHallManagementPermissions.Finance.InvoicesView, L("Permission:Finance.Invoices.View"));
        finance.AddChild(BanquetHallManagementPermissions.Finance.InvoicesPrint, L("Permission:Finance.Invoices.Print"));
        finance.AddChild(BanquetHallManagementPermissions.Finance.HallAccessCardsView, L("Permission:Finance.HallAccessCards.View"));
        finance.AddChild(BanquetHallManagementPermissions.Finance.HallAccessCardsPrint, L("Permission:Finance.HallAccessCards.Print"));
        finance.AddChild(BanquetHallManagementPermissions.Finance.RefundsView, L("Permission:Finance.Refunds.View"));
        finance.AddChild(BanquetHallManagementPermissions.Finance.RefundsProcess, L("Permission:Finance.Refunds.Process"));

        group.AddPermission(BanquetHallManagementPermissions.Reports.Default, L("Permission:Reports"));

        var users = group.AddPermission(BanquetHallManagementPermissions.Users.Default, L("Permission:Users"));
        users.AddChild(BanquetHallManagementPermissions.Users.Create, L("Permission:Users.Create"));
        users.AddChild(BanquetHallManagementPermissions.Users.Update, L("Permission:Users.Update"));
        users.AddChild(BanquetHallManagementPermissions.Users.Delete, L("Permission:Users.Delete"));
        users.AddChild(BanquetHallManagementPermissions.Users.ResetPassword, L("Permission:Users.ResetPassword"));
        users.AddChild(BanquetHallManagementPermissions.Users.ManageRoles, L("Permission:Users.ManageRoles"));
        users.AddChild(BanquetHallManagementPermissions.Users.Activate, L("Permission:Users.Activate"));
        users.AddChild(BanquetHallManagementPermissions.Users.Deactivate, L("Permission:Users.Deactivate"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<BanquetHallManagementResource>(name);
    }
}
