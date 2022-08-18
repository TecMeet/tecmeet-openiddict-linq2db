/*
 * Licensed under the Apache License, Version 2.0 (http://www.apache.org/licenses/LICENSE-2.0)
 * See https://github.com/openiddict/openiddict-core for more information concerning
 * the license and the contributors participating to this project.
 */

using LinqToDB.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenIddict.Core;
using TecMeet.OpenIddict.LinqToDB.Models;
using Xunit;

namespace OpenIddict.LinqToDB.Tests;

public class OpenIddictLinqToDBBuilderTests
{
    [Fact]
    public void Constructor_ThrowsAnExceptionForNullServices()
    {
        // Arrange
        var services = (IServiceCollection) null!;

        // Act and assert
        var exception = Assert.Throws<ArgumentNullException>(() => new OpenIddictLinqToDBBuilder(services));

        Assert.Equal("services", exception.ParamName);
    }

    [Fact]
    public void ReplaceDefaultEntity_SetKey_ApplicationKeyIsCorrectlySet()
    {
        // Arrange
        var services = CreateServices();
        var builder = CreateBuilder(services);

        // Act
        builder.ReplaceDefaultEntities<int>();

        // Assert
        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptionsMonitor<OpenIddictCoreOptions>>().CurrentValue;

        Assert.Equal(typeof(OpenIddictLinqToDBApplication<int>), options.DefaultApplicationType);
        Assert.Equal(typeof(OpenIddictLinqToDBAuthorization<int>), options.DefaultAuthorizationType);
        Assert.Equal(typeof(OpenIddictLinqToDBScope<int>), options.DefaultScopeType);
        Assert.Equal(typeof(OpenIddictLinqToDBToken<int>), options.DefaultTokenType);
    }

    [Fact]
    public void ReplaceDefaultEntity_CustomTypesForStringKeyCorrectlySet()
    {
        // Arrange
        var services = CreateServices();
        var builder = CreateBuilder(services);

        // Act
        builder.ReplaceDefaultEntities<CustomApplication, CustomAuthorization, CustomScope, CustomToken, string>();

        // Assert
        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptionsMonitor<OpenIddictCoreOptions>>().CurrentValue;

        Assert.Equal(typeof(CustomApplication), options.DefaultApplicationType);
        Assert.Equal(typeof(CustomAuthorization), options.DefaultAuthorizationType);
        Assert.Equal(typeof(CustomScope), options.DefaultScopeType);
        Assert.Equal(typeof(CustomToken), options.DefaultTokenType);
    }

    [Fact]
    public void ReplaceDefaultEntity_CustomTypesForIntKeyCorrectlySet()
    {
        // Arrange
        var services = CreateServices();
        var builder = CreateBuilder(services);

        // Act
        builder.ReplaceDefaultEntities<CustomApplicationInt, CustomAuthorizationInt, CustomScopeInt, CustomTokenInt, int>();

        // Assert
        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptionsMonitor<OpenIddictCoreOptions>>().CurrentValue;

        Assert.Equal(typeof(CustomApplicationInt), options.DefaultApplicationType);
        Assert.Equal(typeof(CustomAuthorizationInt), options.DefaultAuthorizationType);
        Assert.Equal(typeof(CustomScopeInt), options.DefaultScopeType);
        Assert.Equal(typeof(CustomTokenInt), options.DefaultTokenType);
    }

    private static OpenIddictLinqToDBBuilder CreateBuilder(IServiceCollection services)
        => services.AddOpenIddict().AddCore().UseLinqToDB();

    private static IServiceCollection CreateServices()
    {
        var services = new ServiceCollection();
        services.AddOptions();

        return services;
    }

    public class CustomApplication : OpenIddictLinqToDBApplication { }
    public class CustomAuthorization : OpenIddictLinqToDBAuthorization { }
    public class CustomScope : OpenIddictLinqToDBScope { }
    public class CustomToken : OpenIddictLinqToDBToken { }
    
    public class CustomApplicationInt : OpenIddictLinqToDBApplication<int> { }
    public class CustomAuthorizationInt : OpenIddictLinqToDBAuthorization<int> { }
    public class CustomScopeInt : OpenIddictLinqToDBScope<int> { }
    public class CustomTokenInt : OpenIddictLinqToDBToken<int> { }
    
    public class DataContext : DataConnection { }
}
