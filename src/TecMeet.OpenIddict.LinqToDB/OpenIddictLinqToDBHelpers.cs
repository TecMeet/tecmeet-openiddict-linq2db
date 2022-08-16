/*
 * Licensed under the Apache License, Version 2.0 (http://www.apache.org/licenses/LICENSE-2.0)
 * See https://github.com/openiddict/openiddict-core for more information concerning
 * the license and the contributors participating to this project.
 */

using System.Runtime.CompilerServices;
using LinqToDB.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.LinqToDB;
using TecMeet.OpenIddict.LinqToDB;
using TecMeet.OpenIddict.LinqToDB.Models;

namespace LinqToDB;

/// <summary>
/// Exposes extensions simplifying the integration between OpenIddict and LinqToDB.
/// </summary>
public static class OpenIddictLinqToDBHelpers
{
    /// <summary>
    /// Registers the OpenIddict entity sets in the LinqToDB context
    /// using the default OpenIddict models and the default key type (string).
    /// </summary>
    /// <param name="builder">The builder used to configure the LinqToDB context.</param>
    /// <param name="dbOptions">Set this parameter if you want to use different database table names. See <see cref="OpenIddictLinqToDBNameOptions"/></param>
    /// <returns>The LinqToDB context builder.</returns>
    public static LinqToDBConnectionOptionsBuilder UseOpenIddict(this LinqToDBConnectionOptionsBuilder builder, 
        OpenIddictLinqToDBNameOptions? dbOptions = null)
        => builder.UseOpenIddict<string>(dbOptions);

    /// <summary>
    /// Registers the OpenIddict entity sets in the LinqToDB 
    /// context using the default OpenIddict models and the specified key type.
    /// </summary>
    /// <remarks>
    /// Note: when using a custom key type, the new key type MUST be registered by calling
    /// <see cref="OpenIddictLinqToDBBuilder.ReplaceDefaultEntities{TKey}"/>.
    /// </remarks>
    /// <param name="builder">The builder used to configure the LinqToDB context.</param>
    /// <param name="dbOptions">Set this parameter if you want to use different database table names. See <see cref="OpenIddictLinqToDBNameOptions"/></param>
    /// <returns>The LinqToDB context builder.</returns>
    public static LinqToDBConnectionOptionsBuilder UseOpenIddict<TKey>(this LinqToDBConnectionOptionsBuilder builder, 
        OpenIddictLinqToDBNameOptions? dbOptions = null)
        where TKey : notnull, IEquatable<TKey>
        => builder.UseOpenIddict<OpenIddictLinqToDBApplication<TKey>,
            OpenIddictLinqToDBAuthorization<TKey>,
            OpenIddictLinqToDBScope<TKey>,
            OpenIddictLinqToDBToken<TKey>, TKey>(dbOptions);

    /// <summary>
    /// Registers the OpenIddict entity sets in the LinqToDB
    /// context using the specified entities and the specified key type.
    /// </summary>
    /// <remarks>
    /// Note: when using custom entities, the new entities MUST be registered by calling
    /// <see cref="OpenIddictLinqToDBBuilder.ReplaceDefaultEntities{TApplication, TAuthorization, TScope, TToken, TKey}"/>.
    /// </remarks>
    /// <param name="builder">The builder used to configure the LinqToDB context.</param>
    /// <param name="dbOptions">Set this parameter if you want to use different database table names. See <see cref="OpenIddictLinqToDBNameOptions"/></param>
    /// <returns>The LinqToDB context builder.</returns>
    public static LinqToDBConnectionOptionsBuilder UseOpenIddict<TApplication, TAuthorization, TScope, TToken, TKey>(
        this LinqToDBConnectionOptionsBuilder builder, OpenIddictLinqToDBNameOptions? dbOptions = null)
        where TApplication : OpenIddictLinqToDBApplication<TKey>
        where TAuthorization : OpenIddictLinqToDBAuthorization<TKey>
        where TScope : OpenIddictLinqToDBScope<TKey>
        where TToken : OpenIddictLinqToDBToken<TKey>
        where TKey : notnull, IEquatable<TKey>
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        var options = dbOptions ?? new OpenIddictLinqToDBNameOptions();

        var schema = builder.MappingSchema;
        if (schema == null) return builder;

        schema.GetFluentMappingBuilder()
            .ConfigureApplicationMapping<TKey, TApplication>(options.ApplicationsTableName)
            .ConfigureAuthorizationMapping<TKey, TAuthorization>(options.AuthorizationsTableName)
            .ConfigureScopeMapping<TKey, TScope>(options.ScopesTableName)
            .ConfigureTokenMapping<TKey, TToken>(options.TokensTableName);

        return builder;
    }

#if SUPPORTS_BCL_ASYNC_ENUMERABLE
    /// <summary>
    /// Executes the query and returns the results as a streamed async enumeration.
    /// </summary>
    /// <typeparam name="T">The type of the returned entities.</typeparam>
    /// <param name="source">The query source.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
    /// <returns>The non-streamed async enumeration containing the results.</returns>
#else
    /// <summary>
    /// Executes the query and returns the results as a non-streamed async enumeration.
    /// </summary>
    /// <typeparam name="T">The type of the returned entities.</typeparam>
    /// <param name="source">The query source.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
    /// <returns>The non-streamed async enumeration containing the results.</returns>
#endif
    internal static IAsyncEnumerable<T> AsAsyncEnumerable<T>(this IQueryable<T> source, CancellationToken cancellationToken)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        return ExecuteAsync(source, cancellationToken);

        static async IAsyncEnumerable<T> ExecuteAsync(IQueryable<T> source, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
#if SUPPORTS_BCL_ASYNC_ENUMERABLE
            await foreach (var element in source.AsAsyncEnumerable().WithCancellation(cancellationToken))
            {
                yield return element;
            }
#else
            foreach (var element in await source.ToListAsync(cancellationToken))
            {
                yield return element;
            }
#endif
        }
    }
}
