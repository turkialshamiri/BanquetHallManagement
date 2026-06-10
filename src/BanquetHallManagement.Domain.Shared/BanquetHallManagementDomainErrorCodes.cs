namespace BanquetHallManagement;

public static class BanquetHallManagementDomainErrorCodes
{
    public const string ReservationCannotConfirm =
        "BanquetHallManagement:Reservation:CannotConfirm";

    public const string ReservationCannotCancel =
        "BanquetHallManagement:Reservation:CannotCancel";

    public const string ReservationCannotComplete =
        "BanquetHallManagement:Reservation:CannotComplete";

    public const string ReservationCannotUpdate =
        "BanquetHallManagement:Reservation:CannotUpdate";

    public const string ReservationCannotDelete =
        "BanquetHallManagement:Reservation:CannotDelete";

    public const string HallUnderMaintenance =
        "BanquetHallManagement:Hall:UnderMaintenance";

    public const string HallStatusCannotBeBooked =
        "BanquetHallManagement:Hall:StatusCannotBeBooked";

    public const string HallStatusInvalid =
        "BanquetHallManagement:Hall:StatusInvalid";

    public const string HallCannotDeleteHasReservations =
        "BanquetHallManagement:Hall:CannotDeleteHasReservations";

    public const string CustomerCannotDeleteHasReservations =
        "BanquetHallManagement:Customer:CannotDeleteHasReservations";

    public const string ServiceCannotDeleteUsedByReservations =
        "BanquetHallManagement:Service:CannotDeleteUsedByReservations";

    public const string ReservationSchedulingConflict =
        "BanquetHallManagement:Reservation:SchedulingConflict";
}
