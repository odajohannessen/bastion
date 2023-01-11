using Xunit.Abstractions;

namespace Bastion.Tests.IntegrationTests;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        int a = 1;
        Assert.Equal(1, a);

    }
}