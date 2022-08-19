using FluentAssertions;
using LinqToDB;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using TecMeet.OpenIddict.LinqToDB;
using Xunit;

namespace OpenIddict.LinqToDB.Tests.Stores;

public class OpenIddictLinqToDBApplicationStoreTests
{
    [Fact]
    public void Constructor_NullCacheShouldThrow()
    {
        // Arrange
        var context = Mock.Of<IDataContext>();
        var cache = (IMemoryCache) null!;
        
        // Act
        var act = () =>
            new OpenIddictLinqToDBApplicationStore(
                cache,
                context);
        
        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("cache");
    }
    
    [Fact]
    public void Constructor_NullContextShouldThrow()
    {
        // Arrange
        var context = (IDataContext) null!;
        var cache = Mock.Of<IMemoryCache>();
        
        // Act
        var act = () =>
            new OpenIddictLinqToDBApplicationStore(
                cache,
                context);
        
        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("context");
    }

    [Fact]
    public void CreateAsync_NullApplicationParameterShouldThrow()
    {
        // Arrange
        var context = Mock.Of<IDataContext>();
        var cache = Mock.Of<IMemoryCache>();
        var store = new OpenIddictLinqToDBApplicationStore(cache, context);
        
        // Act
        var act = async () =>
            await store.CreateAsync(null!, new CancellationToken());
        
        // Assert
        act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("application");
    }
}