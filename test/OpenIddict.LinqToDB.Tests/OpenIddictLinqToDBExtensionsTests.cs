/*
 * Licensed under the Apache License, Version 2.0 (http://www.apache.org/licenses/LICENSE-2.0)
 * See https://github.com/openiddict/openiddict-core for more information concerning
 * the license and the contributors participating to this project.
 */

using Microsoft.Extensions.DependencyInjection;
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

    [Theory]
    [InlineData(typeof(OpenIddictLinqToDBApplicationStore<,,,>))]
    [InlineData(typeof(OpenIddictLinqToDBAuthorizationStore<,,,>))]
    [InlineData(typeof(OpenIddictLinqToDBScopeStore<,>))]
    [InlineData(typeof(OpenIddictLinqToDBTokenStore<,,,>))]
    public void UseLinqToDB_RegistersLinqToDBStore(Type type)
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new OpenIddictCoreBuilder(services);

        // Act
        builder.UseLinqToDB();

        // Assert
        Assert.Contains(services, service => service.ServiceType == type);
    }

    [Fact]
    public void UseLinqToDB_RegistersLinqToDBStoreInterfaces()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new OpenIddictCoreBuilder(services);

        // Act
        builder.UseLinqToDB();

        // Assert
        Assert.Contains(services, service => service.ServiceType == typeof(IOpenIddictApplicationStore<OpenIddictLinqToDBApplication>));
        Assert.Contains(services, service => service.ServiceType == typeof(IOpenIddictAuthorizationStore<OpenIddictLinqToDBAuthorization>));
        Assert.Contains(services, service => service.ServiceType == typeof(IOpenIddictScopeStore<OpenIddictLinqToDBScope>));
        Assert.Contains(services, service => service.ServiceType == typeof(IOpenIddictTokenStore<OpenIddictLinqToDBToken>));
    }
}
