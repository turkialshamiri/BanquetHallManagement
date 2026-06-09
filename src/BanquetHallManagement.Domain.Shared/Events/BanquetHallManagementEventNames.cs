namespace BanquetHallManagement.Events;

/// <summary>
/// Central registry of distributed event names for the Banquet Hall Management system.
/// </summary>
public static class BanquetHallManagementEventNames
{
    public const string ReservationCreated = "BanquetHallManagement.Reservation.Created";

    public const string ReservationUpdated = "BanquetHallManagement.Reservation.Updated";

    public const string ReservationDeleted = "BanquetHallManagement.Reservation.Deleted";

    public const string ReservationConfirmed = "BanquetHallManagement.Reservation.Confirmed";

    public const string ReservationCancelled = "BanquetHallManagement.Reservation.Cancelled";

    public const string ReservationCompleted = "BanquetHallManagement.Reservation.Completed";

    public const string ReservationAudit = "BanquetHallManagement.Reservation.Audit";

    public const string ReservationNotification = "BanquetHallManagement.Reservation.Notification";
}
