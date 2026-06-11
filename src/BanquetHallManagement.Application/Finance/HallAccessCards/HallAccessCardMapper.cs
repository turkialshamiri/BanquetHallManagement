using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace BanquetHallManagement.Finance.HallAccessCards;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class HallAccessCardToHallAccessCardDtoMapper : MapperBase<HallAccessCard, HallAccessCardDto>
{
    public override partial HallAccessCardDto Map(HallAccessCard source);

    public override partial void Map(HallAccessCard source, HallAccessCardDto destination);
}
