using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BanquetHallManagement.Finance.Sequences;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace BanquetHallManagement.Finance.Services;

public class FinanceNumberSequenceManager : DomainService
{
    private readonly IRepository<FinanceNumberSequence, Guid> _sequenceRepository;

    public FinanceNumberSequenceManager(IRepository<FinanceNumberSequence, Guid> sequenceRepository)
    {
        _sequenceRepository = sequenceRepository;
    }

    public async Task<string> GenerateNextAsync(
        string prefix,
        CancellationToken cancellationToken = default)
    {
        var year = Clock.Now.Year;
        var query = await _sequenceRepository.GetQueryableAsync();

        var sequence = await AsyncExecuter.FirstOrDefaultAsync(
            query.Where(s => s.Prefix == prefix && s.Year == year),
            cancellationToken);

        if (sequence == null)
        {
            sequence = new FinanceNumberSequence(GuidGenerator.Create(), prefix, year);
            await _sequenceRepository.InsertAsync(sequence, autoSave: true, cancellationToken);
        }

        var nextNumber = sequence.GetNextNumber();
        await _sequenceRepository.UpdateAsync(sequence, autoSave: true, cancellationToken);

        return FinanceNumberSequence.Format(prefix, year, nextNumber);
    }
}
