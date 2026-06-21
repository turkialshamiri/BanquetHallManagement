using System;
using System.Collections.Generic;
using Volo.Abp;
using Volo.Abp.Domain.Values;

namespace BanquetHallManagement.Reservations;

public class TimeSlot : ValueObject
{
    public DateTime EventDate { get; private set; }

    public TimeSpan StartTime { get; private set; }

    public TimeSpan EndTime { get; private set; }

    public TimeSlot(DateTime eventDate, TimeSpan startTime, TimeSpan endTime)
    {
        if (endTime <= startTime)
        {
            throw new BusinessException(BanquetHallManagementDomainErrorCodes.ReservationInvalidTimeSlot);
        }

        EventDate = eventDate.Date;
        StartTime = startTime;
        EndTime = endTime;
    }

    private TimeSlot()
    {
    }

    public DateTime GetEventEndDateTime() => EventDate + EndTime;

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return EventDate;
        yield return StartTime;
        yield return EndTime;
    }
}
