using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace BanquetHallManagement.Finance.Payments;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class PaymentToPaymentDtoMapper : MapperBase<Payment, PaymentDto>
{
    [MapperIgnoreTarget(nameof(PaymentDto.PaymentType))]
    public override partial PaymentDto Map(Payment source);

    [MapperIgnoreTarget(nameof(PaymentDto.PaymentType))]
    public override partial void Map(Payment source, PaymentDto destination);
}
