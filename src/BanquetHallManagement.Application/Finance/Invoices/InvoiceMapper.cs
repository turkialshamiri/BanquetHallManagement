using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace BanquetHallManagement.Finance.Invoices;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class InvoiceToInvoiceDtoMapper : MapperBase<Invoice, InvoiceDto>
{
    [MapperIgnoreTarget(nameof(InvoiceDto.InvoiceType))]
    public override partial InvoiceDto Map(Invoice source);

    [MapperIgnoreTarget(nameof(InvoiceDto.InvoiceType))]
    public override partial void Map(Invoice source, InvoiceDto destination);
}
