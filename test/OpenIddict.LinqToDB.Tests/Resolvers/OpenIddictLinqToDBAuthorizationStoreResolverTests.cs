/*
 * Licensed under the Apache License, Version 2.0 (http://www.apache.org/licenses/LICENSE-2.0)
 * See https://github.com/openiddict/openiddict-core for more information concerning
 * the license and the contributors participating to this project.
 */

using LinqToDB.Data;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using OpenIddict.Core;
using TecMeet.OpenIddict.LinqToDB;
using TecMeet.OpenIddict.LinqToDB.Models;
using Xunit;

namespace OpenIddict.LinqToDB.Tests;

public class OpenIddictLinqToDBAuthorizationStoreResolverTests
{
    [Fact]
    public void Get_ReturnsCustomStoreCorrespondingToTheSpecifiedTypeWhenAvailable()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<IOpenIddictAuthorizationStore<CustomAuthorization>>());

        var options = Mock.Of<IOptionsMonitor<OpenIddictCoreOptions>>();
        var provider = services.BuildServiceProvider();
        var resolver = new OpenIddictLinqToDBAuthorizationStoreResolver(options, provider);

        // Act and assert
        Assert.NotNull(resolver.Get<CustomAuthorization>());
    }

    [Fact]
    public void Get_ThrowsAnExceptionForInvalidEntityType()
    {
        // Arrange
        var services = new ServiceCollection();

        var options = Mock.Of<IOptionsMonitor<OpenIddictCoreOptions>>();
        var provider = services.BuildServiceProvider();
        var resolver = new OpenIddictLinqToDBAuthorizationStoreResolver(options, provider);

        // Act and assert
        var exception = Assert.Throws<InvalidOperationException>(resolver.Get<CustomAuthorization>);

        Assert.Equal(SR.GetResourceString(SR.ID0254), exception.Message);
    }

    [Fact]
    public void Get_ThrowsAnExceptionWhenDbContextTypeIsNotAvailable()
    {
        // Arrange
        var services = new ServiceCollection();

        var options = Mock.Of<IOptionsMonitor<OpenIddictCoreOptions>>();
        var provider = services.BuildServiceProvider();
        var resolver = new OpenIddictLinqToDBAuthorizationStoreResolver(options, provider);

        // Act and assert
        var exception = Assert.Throws<InvalidOperationException>(resolver.Get<OpenIddictLinqToDBAuthorization>);

        Assert.Equal(SR.GetResourceString(SR.ID0253), exception.Message);
    }

    [Fact]
    public void Get_ReturnsDefaultStoreCorrespondingToTheSpecifiedTypeWhenAvailable()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<IOpenIddictAuthorizationStore<CustomAuthorization>>());
        services.AddSingleton(CreateStore());

        var options = Mock.Of<IOptionsMonitor<OpenIddictCoreOptions>>();
        var provider = services.BuildServiceProvider();
        var resolver = new OpenIddictLinqToDBAuthorizationStoreResolver(options, provider);

        // Act and assert
        Assert.NotNull(resolver.Get<MyAuthorization>());
    }

    private static OpenIddictLinqToDBAuthorizationStore<MyAuthorization, MyApplication, MyToken, long> CreateStore()
        => new Mock<OpenIddictLinqToDBAuthorizationStore<MyAuthorization, MyApplication, MyToken, long>>(
            Mock.Of<IMemoryCache>(),
            Mock.Of<DbContext>()).Object;

    public class DbContext : DataConnection { }
    
    public class CustomAuthorization { }

    public class MyApplication : OpenIddictLinqToDBApplication<long> { }
    public class MyAuthorization : OpenIddictLinqToDBAuthorization<long> { }
    public class MyScope : OpenIddictLinqToDBScope<long> { }
    public class MyToken : OpenIddictLinqToDBToken<long> { }
}
