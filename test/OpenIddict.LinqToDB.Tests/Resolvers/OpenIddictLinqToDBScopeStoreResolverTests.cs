﻿/*
 * Licensed under the Apache License, Version 2.0 (http://www.apache.org/licenses/LICENSE-2.0)
 * See https://github.com/openiddict/openiddict-core for more information concerning
 * the license and the contributors participating to this project.
 */

using LinqToDB.Data;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using TecMeet.OpenIddict.LinqToDB;
using TecMeet.OpenIddict.LinqToDB.Models;
using Xunit;

namespace OpenIddict.LinqToDB.Tests;

public class OpenIddictLinqToDBScopeStoreResolverTests
{
    [Fact]
    public void Get_ReturnsCustomStoreCorrespondingToTheSpecifiedTypeWhenAvailable()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<IOpenIddictScopeStore<CustomScope>>());

        var provider = services.BuildServiceProvider();
        var resolver = new OpenIddictLinqToDBScopeStoreResolver(provider);

        // Act and assert
        Assert.NotNull(resolver.Get<CustomScope>());
    }

    [Fact]
    public void Get_ThrowsAnExceptionForInvalidEntityType()
    {
        // Arrange
        var services = new ServiceCollection();

        var provider = services.BuildServiceProvider();
        var resolver = new OpenIddictLinqToDBScopeStoreResolver(provider);

        // Act and assert
        var exception = Assert.Throws<InvalidOperationException>(resolver.Get<CustomScope>);

        Assert.Equal(SR.GetResourceString(SR.ID0259), exception.Message);
    }

    [Fact]
    public void Get_ReturnsDefaultStoreCorrespondingToTheSpecifiedTypeWhenAvailable()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<IOpenIddictScopeStore<CustomScope>>());
        services.AddSingleton(CreateStore());

        var provider = services.BuildServiceProvider();
        var resolver = new OpenIddictLinqToDBScopeStoreResolver(provider);

        // Act and assert
        Assert.NotNull(resolver.Get<MyScope>());
    }

    private static OpenIddictLinqToDBScopeStore<MyScope, long> CreateStore()
        => new Mock<OpenIddictLinqToDBScopeStore<MyScope, long>>(
            Mock.Of<IMemoryCache>(),
            Mock.Of<DataConnection>()).Object;

    public class CustomScope { }

    public class DataContext : DataConnection { }
    public class MyScope : OpenIddictLinqToDBScope<long> {}
}
