using BanquetHallManagement.Finance.Sequences;
using Shouldly;
using Xunit;

namespace BanquetHallManagement.Finance;

public class FinanceNumberSequenceTests
{
    [Fact]
    public void GetNextNumber_Should_Increment_LastNumber()
    {
        var sequence = new FinanceNumberSequence(
            System.Guid.NewGuid(),
            "JE",
            2026,
            5);

        sequence.GetNextNumber().ShouldBe(6);
        sequence.GetNextNumber().ShouldBe(7);
    }

    [Fact]
    public void Format_Should_Return_Zero_Padded_Number()
    {
        FinanceNumberSequence.Format("JE", 2026, 1)
            .ShouldBe("JE-2026-00001");
    }
}
