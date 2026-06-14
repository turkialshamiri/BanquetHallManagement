namespace BanquetHallManagement;

public static class BanquetHallManagementDomainErrorCodes
{
    public const string ReservationCannotConfirm =
        "BanquetHallManagement:Reservation:CannotConfirm";

    public const string ReservationCannotCancel =
        "BanquetHallManagement:Reservation:CannotCancel";

    public const string ReservationCannotAutoCancelWithPayments =
        "BanquetHallManagement:Reservation:CannotAutoCancelWithPayments";

    public const string ReservationCannotComplete =
        "BanquetHallManagement:Reservation:CannotComplete";

    public const string ReservationCannotUpdate =
        "BanquetHallManagement:Reservation:CannotUpdate";

    public const string ReservationCannotDelete =
        "BanquetHallManagement:Reservation:CannotDelete";

    public const string ReservationCannotArchive =
        "BanquetHallManagement:Reservation:CannotArchive";

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

    public const string ReservationCannotMarkFullyPaid =
        "BanquetHallManagement:Reservation:CannotMarkFullyPaid";

    public const string ReservationCannotConfirmHallEntry =
        "BanquetHallManagement:Reservation:CannotConfirmHallEntry";

    public const string ReservationNotFound =
        "BanquetHallManagement:Reservation:NotFound";

    public const string PaymentDepositBelowMinimum =
        "BanquetHallManagement:Payment:DepositBelowMinimum";

    public const string PaymentInstallmentBelowMinimum =
        "BanquetHallManagement:Payment:InstallmentBelowMinimum";

    public const string PaymentInvalidReservationStatus =
        "BanquetHallManagement:Payment:InvalidReservationStatus";

    public const string PaymentAlreadyRecorded =
        "BanquetHallManagement:Payment:AlreadyRecorded";

    public const string PaymentAmountInvalid =
        "BanquetHallManagement:Payment:AmountInvalid";

    public const string PaymentAmountExceedsRemaining =
        "BanquetHallManagement:Payment:AmountExceedsRemaining";

    public const string PaymentNotFound =
        "BanquetHallManagement:Payment:NotFound";

    public const string AccountNotFound =
        "BanquetHallManagement:Finance:AccountNotFound";

    public const string JournalEntryUnbalanced =
        "BanquetHallManagement:JournalEntry:Unbalanced";

    public const string JournalEntryHasNoLines =
        "BanquetHallManagement:JournalEntry:HasNoLines";

    public const string JournalEntryLineAmountInvalid =
        "BanquetHallManagement:JournalEntryLine:AmountInvalid";

    public const string JournalEntryAlreadyPosted =
        "BanquetHallManagement:JournalEntry:AlreadyPosted";

    public const string JournalEntryNotFound =
        "BanquetHallManagement:JournalEntry:NotFound";

    public const string JournalEntryDuplicatePosting =
        "BanquetHallManagement:JournalEntry:DuplicatePosting";

    public const string PaymentJournalPostingNotSupported =
        "BanquetHallManagement:Payment:JournalPostingNotSupported";

    public const string InvoiceNotFound =
        "BanquetHallManagement:Invoice:NotFound";

    public const string InvoiceDuplicateCreation =
        "BanquetHallManagement:Invoice:DuplicateCreation";

    public const string InvoiceCreationNotSupported =
        "BanquetHallManagement:Invoice:CreationNotSupported";

    public const string RefundNotAllowed =
        "BanquetHallManagement:Refund:NotAllowed";

    public const string RefundAmountExceedsLiability =
        "BanquetHallManagement:Refund:AmountExceedsLiability";

    public const string HallAccessCardNotFound =
        "BanquetHallManagement:HallAccessCard:NotFound";
}
