﻿/*
 * Licensed under the Apache License, Version 2.0 (http://www.apache.org/licenses/LICENSE-2.0)
 * See https://github.com/openiddict/openiddict-core for more information concerning
 * the license and the contributors participating to this project.
 */

using LinqToDB;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using OpenIddict.Core;
using TecMeet.OpenIddict.LinqToDB;
using TecMeet.OpenIddict.LinqToDB.Models;
using Xunit;

namespace OpenIddict.LinqToDB.Tests;

public class OpenIddictLinqToDBApplicationStoreResolverTests
{
    [Fact]
    public void Get_ReturnsCustomStoreCorrespondingToTheSpecifiedTypeWhenAvailable()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<IOpenIddictApplicationStore<CustomApplication>>());

        var options = Mock.Of<IOptionsMonitor<OpenIddictCoreOptions>>();
        var provider = services.BuildServiceProvider();
        var resolver = new OpenIddictLinqToDBApplicationStoreResolver(options, provider);

        // Act and assert
        Assert.NotNull(resolver.Get<CustomApplication>());
    }

    [Fact]
    public void Get_ThrowsAnExceptionForInvalidEntityType()
    {
        // Arrange
        var services = new ServiceCollection();

        var options = Mock.Of<IOptionsMonitor<OpenIddictCoreOptions>>();
        var provider = services.BuildServiceProvider();
        var resolver = new OpenIddictLinqToDBApplicationStoreResolver(options, provider);

        // Act and assert
        var exception = Assert.Throws<InvalidOperationException>(resolver.Get<CustomApplication>);

        Assert.Equal(SR.GetResourceString(SR.ID0256), exception.Message);
    }

    [Fact]
    public void Get_ReturnsDefaultStoreCorrespondingToTheSpecifiedTypeWhenAvailable()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<IOpenIddictApplicationStore<CustomApplication>>());
        services.AddSingleton(CreateStore());

        var options = Mock.Of<IOptionsMonitor<OpenIddictCoreOptions>>(
            mock => mock.CurrentValue == new OpenIddictCoreOptions
            {
                DefaultTokenType = typeof(MyToken),
                DefaultAuthorizationType = typeof(MyAuthorization)
            });
        
        var provider = services.BuildServiceProvider();
        var resolver = new OpenIddictLinqToDBApplicationStoreResolver(options, provider);

        // Act and assert
        Assert.NotNull(resolver.Get<MyApplication>());
    }

    private static OpenIddictLinqToDBApplicationStore<MyApplication, MyAuthorization, MyToken, long> CreateStore()
        => new Mock<OpenIddictLinqToDBApplicationStore<MyApplication, MyAuthorization, MyToken, long>>(
            Mock.Of<IMemoryCache>(),
            Mock.Of<IDataContext>()
            ).Object;

    public class CustomApplication { }

    public class MyApplication : OpenIddictLinqToDBApplication<long> { }
    public class MyAuthorization : OpenIddictLinqToDBAuthorization<long> { }
    public class MyToken : OpenIddictLinqToDBToken<long> { }
}
