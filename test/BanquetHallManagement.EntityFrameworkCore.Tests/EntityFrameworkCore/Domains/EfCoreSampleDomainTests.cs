using BanquetHallManagement.Samples;
using Xunit;

namespace BanquetHallManagement.EntityFrameworkCore.Domains;

[Collection(BanquetHallManagementTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<BanquetHallManagementEntityFrameworkCoreTestModule>
{

}
