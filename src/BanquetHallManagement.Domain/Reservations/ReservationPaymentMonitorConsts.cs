using System;

namespace BanquetHallManagement.Reservations;

public static class ReservationPaymentMonitorConsts
{
    public const int JobPeriodMinutes = 15;

    public static readonly TimeSpan AutoCancelWindow = TimeSpan.FromHours(2);
}
