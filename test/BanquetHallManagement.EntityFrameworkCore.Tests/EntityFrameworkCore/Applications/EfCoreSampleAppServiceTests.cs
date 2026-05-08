using BanquetHallManagement.Samples;
using Xunit;

namespace BanquetHallManagement.EntityFrameworkCore.Applications;

[Collection(BanquetHallManagementTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<BanquetHallManagementEntityFrameworkCoreTestModule>
{

}
