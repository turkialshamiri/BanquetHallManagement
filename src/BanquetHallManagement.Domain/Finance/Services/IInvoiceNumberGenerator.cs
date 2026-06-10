using System.Threading;
using System.Threading.Tasks;

namespace BanquetHallManagement.Finance.Services;

public interface IInvoiceNumberGenerator
{
    Task<string> GenerateAsync(CancellationToken cancellationToken = default);
}
