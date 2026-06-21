using System;
using BanquetHallManagement.Finance.Sequences;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace BanquetHallManagement.EntityFrameworkCore.Finance.Repositories;

public class EfCoreFinanceNumberSequenceRepository :
    EfCoreRepository<BanquetHallManagementDbContext, FinanceNumberSequence, Guid>,
    IFinanceNumberSequenceRepository
{
    public EfCoreFinanceNumberSequenceRepository(
        IDbContextProvider<BanquetHallManagementDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }
}
