using System;

namespace BanquetHallManagement.Reservations;

internal static class ReservationConcurrencyHelper
{
    public static bool IsTransientSchedulingFailure(Exception exception)
    {
        for (var current = exception; current != null; current = current.InnerException)
        {
            var message = current.Message;

            if (message.Contains("1205", StringComparison.Ordinal) ||
                message.Contains("1222", StringComparison.Ordinal) ||
                message.Contains("3928", StringComparison.Ordinal) ||
                message.Contains("Failed to acquire reservation scheduling lock", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
