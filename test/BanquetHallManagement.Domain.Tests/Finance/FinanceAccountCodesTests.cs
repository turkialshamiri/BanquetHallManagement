using BanquetHallManagement.Finance.Accounts;
using Shouldly;
using Xunit;

namespace BanquetHallManagement.Finance;

public class FinanceAccountCodesTests
{
    [Fact]
    public void Seed_Account_Codes_Should_Match_Chart_Of_Accounts()
    {
        FinanceAccountCodes.Cash.ShouldBe("1100");
        FinanceAccountCodes.CustomerRefundLiabilities.ShouldBe("2200");
        FinanceAccountCodes.DeferredRevenue.ShouldBe("2300");
        FinanceAccountCodes.HallRevenue.ShouldBe("4100");
        FinanceAccountCodes.ServiceRevenue.ShouldBe("4200");
        FinanceAccountCodes.NonRefundableDepositRevenue.ShouldBe("4110");
    }
}
