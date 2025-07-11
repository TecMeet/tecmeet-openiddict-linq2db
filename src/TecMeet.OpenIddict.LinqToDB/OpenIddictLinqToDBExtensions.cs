/*
 * Licensed under the Apache License, Version 2.0 (http://www.apache.org/licenses/LICENSE-2.0)
 * See https://github.com/openiddict/openiddict-core for more information concerning
 * the license and the contributors participating to this project.
 */

using LinqToDB.Mapping;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenIddict.LinqToDB;
using TecMeet.OpenIddict.LinqToDB.Models;

namespace TecMeet.OpenIddict.LinqToDB;

/// <summary>
/// Exposes extensions allowing to register the OpenIddict LinqToDB services.
/// </summary>
public static class OpenIddictLinqToDBExtensions
{
    /// <summary>
    /// Registers the LinqToDB stores services in the DI container and
    /// configures OpenIddict to use the LinqToDB entities by default.
    /// </summary>
    /// <param name="builder">The services builder used by OpenIddict to register new services.</param>
    /// <remarks>This extension can be safely called multiple times.</remarks>
    /// <returns>The <see cref="OpenIddictLinqToDBBuilder"/>.</returns>
    public static OpenIddictLinqToDBBuilder UseLinqToDB(this OpenIddictCoreBuilder builder)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        // Since LinqToDB may be used with databases performing case-insensitive
        // or culture-sensitive comparisons, ensure the additional filtering logic is enforced
        // in case case-sensitive stores were registered before this extension was called.
        builder.Configure(options => options.DisableAdditionalFiltering = false);

        builder.SetDefaultApplicationEntity<OpenIddictLinqToDBApplication>()
               .SetDefaultAuthorizationEntity<OpenIddictLinqToDBAuthorization>()
               .SetDefaultScopeEntity<OpenIddictLinqToDBScope>()
               .SetDefaultTokenEntity<OpenIddictLinqToDBToken>();

        builder.Services.TryAddScoped(typeof(OpenIddictLinqToDBApplicationStore<,,,>));
        builder.Services.TryAddScoped(typeof(OpenIddictLinqToDBAuthorizationStore<,,,>));
        builder.Services.TryAddScoped(typeof(OpenIddictLinqToDBScopeStore<,>));
        builder.Services.TryAddScoped(typeof(OpenIddictLinqToDBTokenStore<,,,>));

        // Register the default stores for the default entities
        builder.Services.TryAddScoped<IOpenIddictApplicationStore<OpenIddictLinqToDBApplication>>(provider =>
            provider.GetRequiredService<OpenIddictLinqToDBApplicationStore<OpenIddictLinqToDBApplication, OpenIddictLinqToDBAuthorization, OpenIddictLinqToDBToken, Guid>>());
        
        builder.Services.TryAddScoped<IOpenIddictAuthorizationStore<OpenIddictLinqToDBAuthorization>>(provider =>
            provider.GetRequiredService<OpenIddictLinqToDBAuthorizationStore<OpenIddictLinqToDBAuthorization, OpenIddictLinqToDBApplication, OpenIddictLinqToDBToken, Guid>>());
        
        builder.Services.TryAddScoped<IOpenIddictScopeStore<OpenIddictLinqToDBScope>>(provider =>
            provider.GetRequiredService<OpenIddictLinqToDBScopeStore<OpenIddictLinqToDBScope, Guid>>());
        
        builder.Services.TryAddScoped<IOpenIddictTokenStore<OpenIddictLinqToDBToken>>(provider =>
            provider.GetRequiredService<OpenIddictLinqToDBTokenStore<OpenIddictLinqToDBToken, OpenIddictLinqToDBApplication, OpenIddictLinqToDBAuthorization, Guid>>());

        return new OpenIddictLinqToDBBuilder(builder.Services);
    }

    /// <summary>
    /// Registers the LinqToDB stores services in the DI container and
    /// configures OpenIddict to use the LinqToDB entities by default.
    /// </summary>
    /// <param name="builder">The services builder used by OpenIddict to register new services.</param>
    /// <param name="configuration">The configuration delegate used to configure the LinqToDB services.</param>
    /// <remarks>This extension can be safely called multiple times.</remarks>
    /// <returns>The <see cref="OpenIddictCoreBuilder"/>.</returns>
    public static OpenIddictCoreBuilder UseLinqToDB(
        this OpenIddictCoreBuilder builder, Action<OpenIddictLinqToDBBuilder> configuration)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (configuration is null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        configuration(builder.UseLinqToDB());

        return builder;
    }

    /// <summary>
    /// Registers the OpenIddict entity sets in the LinqToDB context
    /// using the default OpenIddict models and the default key type (string).
    /// </summary>
    /// <param name="mappingSchema">The LinqToDB MappingSchema that you're using. If it's not customized you can use MappingSchema.Default</param>
    /// <param name="dbOptions">Set this parameter if you want to use different database table names. See <see cref="OpenIddictLinqToDBNameOptions"/></param>
    /// <returns>The LinqToDB context builder.</returns>
    public static MappingSchema UseOpenIddict(this MappingSchema mappingSchema, 
        OpenIddictLinqToDBNameOptions? dbOptions = null)
        => mappingSchema.UseOpenIddict<string>(dbOptions);

    /// <summary>
    /// Registers the OpenIddict entity sets in the LinqToDB 
    /// context using the default OpenIddict models and the specified key type.
    /// </summary>
    /// <param name="mappingSchema">The LinqToDB MappingSchema that you're using. If it's not customized you can use MappingSchema.Default</param>
    /// <param name="dbOptions">Set this parameter if you want to use different database table names. See <see cref="OpenIddictLinqToDBNameOptions"/></param>
    /// <returns>The LinqToDB context builder.</returns>
    public static MappingSchema UseOpenIddict<TKey>(this MappingSchema mappingSchema, 
        OpenIddictLinqToDBNameOptions? dbOptions = null)
        where TKey : notnull, IEquatable<TKey>
        => mappingSchema.UseOpenIddict<OpenIddictLinqToDBApplication<TKey>,
            OpenIddictLinqToDBAuthorization<TKey>,
            OpenIddictLinqToDBScope<TKey>,
            OpenIddictLinqToDBToken<TKey>, TKey>(dbOptions);

    /// <summary>
    /// Registers the OpenIddict entity sets in the LinqToDB
    /// context using the specified entities and the specified key type.
    /// </summary>
    /// <param name="dbOptions">Set this parameter if you want to use different database table names. See <see cref="OpenIddictLinqToDBNameOptions"/></param>
    /// <param name="mappingSchema">The LinqToDB MappingSchema that you're using. If it's not customized you can use MappingSchema.Default</param>
    /// <returns>The LinqToDB context builder.</returns>
    public static MappingSchema UseOpenIddict<TApplication, TAuthorization, TScope, TToken, TKey>(
        this MappingSchema mappingSchema, OpenIddictLinqToDBNameOptions? dbOptions = null)
        where TApplication : OpenIddictLinqToDBApplication<TKey>
        where TAuthorization : OpenIddictLinqToDBAuthorization<TKey>
        where TScope : OpenIddictLinqToDBScope<TKey>
        where TToken : OpenIddictLinqToDBToken<TKey>
        where TKey : notnull, IEquatable<TKey>
    {
        if (mappingSchema is null)
        {
            throw new ArgumentNullException(nameof(mappingSchema));
        }

        var options = dbOptions ?? new OpenIddictLinqToDBNameOptions();

        new FluentMappingBuilder(mappingSchema)
            .ConfigureApplicationMapping<TKey, TApplication>(options.ApplicationsTableName)
            .ConfigureAuthorizationMapping<TKey, TAuthorization>(options.AuthorizationsTableName)
            .ConfigureScopeMapping<TKey, TScope>(options.ScopesTableName)
            .ConfigureTokenMapping<TKey, TToken>(options.TokensTableName)
            .Build();

        return mappingSchema;
    }
}
