using Store.Application.Common;

namespace Store.Application.Tests.Common;

public class PagedResultTests
{
    [Theory]
    [InlineData(0, 12, 0)]
    [InlineData(1, 12, 1)]
    [InlineData(12, 12, 1)]
    [InlineData(13, 12, 2)]
    public void TotalPages_rounds_up(int totalCount, int pageSize, int expectedPages)
    {
        var result = new PagedResult<int>([], 1, pageSize, totalCount);

        Assert.Equal(expectedPages, result.TotalPages);
    }
}
