using TaskManagement.Services;
using Xunit;

namespace TaskManagement.Tests;

public class CategoryServiceTests
{
    [Fact]
    public async Task GetCategories_ReturnsSeededCategoriesInOrder()
    {
        var service = new CategoryService(TestDb.Create());

        var categories = await service.GetCategoriesAsync();

        Assert.Equal(2, categories.Count);
        Assert.Equal("Work", categories[0].Name);
        Assert.Equal("Personal", categories[1].Name);
    }
}
