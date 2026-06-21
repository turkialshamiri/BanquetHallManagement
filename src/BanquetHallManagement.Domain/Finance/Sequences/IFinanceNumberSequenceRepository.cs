using System;
using Volo.Abp.Domain.Repositories;

namespace BanquetHallManagement.Finance.Sequences;

public interface IFinanceNumberSequenceRepository : IRepository<FinanceNumberSequence, Guid>
{
}
