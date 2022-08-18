/*
 * Licensed under the Apache License, Version 2.0 (http://www.apache.org/licenses/LICENSE-2.0)
 * See https://github.com/openiddict/openiddict-core for more information concerning
 * the license and the contributors participating to this project.
 */

using LinqToDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenIddict.Core;
using TecMeet.OpenIddict.LinqToDB;
using TecMeet.OpenIddict.LinqToDB.Models;
using Xunit;

namespace OpenIddict.LinqToDB.Tests;

public class OpenIddictLinqToDBExtensionsTests
{
    [Fact]
    public void UseLinqToDB_ThrowsAnExceptionForNullBuilder()
    {
        // Arrange
        var builder = (OpenIddictCoreBuilder) null!;

        // Act and assert
        var exception = Assert.Throws<ArgumentNullException>(() => builder.UseLinqToDB());

        Assert.Equal("builder", exception.ParamName);
    }

    [Fact]
    public void UseLinqToDB_ThrowsAnExceptionForNullConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new OpenIddictCoreBuilder(services);

        // Act and assert
        var exception = Assert.Throws<ArgumentNullException>(() => builder.UseLinqToDB(configuration: null!));

        Assert.Equal("configuration", exception.ParamName);
    }

    [Fact]
    public void UseLinqToDB_RegistersDefaultEntities()
    {
        // Arrange
        var services = new ServiceCollection().AddOptions();
        var builder = new OpenIddictCoreBuilder(services);

        // Act
        builder.UseLinqToDB();

        // Assert
        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptionsMonitor<OpenIddictCoreOptions>>().CurrentValue;

        Assert.Equal(typeof(OpenIddictLinqToDBApplication), options.DefaultApplicationType);
        Assert.Equal(typeof(OpenIddictLinqToDBAuthorization), options.DefaultAuthorizationType);
        Assert.Equal(typeof(OpenIddictLinqToDBScope), options.DefaultScopeType);
        Assert.Equal(typeof(OpenIddictLinqToDBToken), options.DefaultTokenType);
    }

    [Theory]
    [InlineData(typeof(IOpenIddictApplicationStoreResolver), typeof(OpenIddictLinqToDBApplicationStoreResolver))]
    [InlineData(typeof(IOpenIddictAuthorizationStoreResolver), typeof(OpenIddictLinqToDBAuthorizationStoreResolver))]
    [InlineData(typeof(IOpenIddictScopeStoreResolver), typeof(OpenIddictLinqToDBScopeStoreResolver))]
    [InlineData(typeof(IOpenIddictTokenStoreResolver), typeof(OpenIddictLinqToDBTokenStoreResolver))]
    public void UseLinqToDB_RegistersLinqToDBStoreResolvers(Type serviceType, Type implementationType)
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new OpenIddictCoreBuilder(services);

        // Act
        builder.UseLinqToDB();

        // Assert
        Assert.Contains(services, service => service.ServiceType == serviceType &&
                                             service.ImplementationType == implementationType);
    }

    [Theory]
    [InlineData(typeof(OpenIddictLinqToDBApplicationStore<>))]
    [InlineData(typeof(OpenIddictLinqToDBAuthorizationStore<>))]
    [InlineData(typeof(OpenIddictLinqToDBScopeStore<>))]
    [InlineData(typeof(OpenIddictLinqToDBTokenStore<>))]
    public void UseLinqToDB_RegistersLinqToDBStore(Type type)
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new OpenIddictCoreBuilder(services);

        // Act
        builder.UseLinqToDB();

        // Assert
        Assert.Contains(services, service => service.ServiceType == type && service.ImplementationType == type);
    }
}
