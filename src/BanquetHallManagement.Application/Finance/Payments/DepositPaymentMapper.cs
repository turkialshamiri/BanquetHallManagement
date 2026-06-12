using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace BanquetHallManagement.Finance.Payments;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class PaymentToDepositPaymentResultDtoMapper : MapperBase<Payment, DepositPaymentResultDto>
{
    [MapperIgnoreTarget(nameof(DepositPaymentResultDto.PaymentType))]
    [MapperIgnoreTarget(nameof(DepositPaymentResultDto.IsFullyPaid))]
    [MapperIgnoreTarget(nameof(DepositPaymentResultDto.HallAccessCardId))]
    public override partial DepositPaymentResultDto Map(Payment source);

    [MapperIgnoreTarget(nameof(DepositPaymentResultDto.PaymentType))]
    [MapperIgnoreTarget(nameof(DepositPaymentResultDto.IsFullyPaid))]
    [MapperIgnoreTarget(nameof(DepositPaymentResultDto.HallAccessCardId))]
    public override partial void Map(Payment source, DepositPaymentResultDto destination);
}
