using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace BanquetHallManagement.Finance.Payments;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class PaymentToInstallmentPaymentResultDtoMapper : MapperBase<Payment, InstallmentPaymentResultDto>
{
    [MapperIgnoreTarget(nameof(InstallmentPaymentResultDto.PaymentType))]
    [MapperIgnoreTarget(nameof(InstallmentPaymentResultDto.RemainingAmount))]
    [MapperIgnoreTarget(nameof(InstallmentPaymentResultDto.IsFullyPaid))]
    [MapperIgnoreTarget(nameof(InstallmentPaymentResultDto.HallAccessCardId))]
    public override partial InstallmentPaymentResultDto Map(Payment source);

    [MapperIgnoreTarget(nameof(InstallmentPaymentResultDto.PaymentType))]
    [MapperIgnoreTarget(nameof(InstallmentPaymentResultDto.RemainingAmount))]
    [MapperIgnoreTarget(nameof(InstallmentPaymentResultDto.IsFullyPaid))]
    [MapperIgnoreTarget(nameof(InstallmentPaymentResultDto.HallAccessCardId))]
    public override partial void Map(Payment source, InstallmentPaymentResultDto destination);
}
