/*
 * Licensed under the Apache License, Version 2.0 (http://www.apache.org/licenses/LICENSE-2.0)
 * See https://github.com/openiddict/openiddict-core for more information concerning
 * the license and the contributors participating to this project.
 */

using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenIddict.Core;
using OpenIddict.Extensions;
using TecMeet.OpenIddict.LinqToDB.Models;

namespace TecMeet.OpenIddict.LinqToDB;

/// <summary>
/// Exposes a method allowing to resolve a token store.
/// </summary>
public class OpenIddictLinqToDBTokenStoreResolver : IOpenIddictTokenStoreResolver
{
    private readonly IOptionsMonitor<OpenIddictCoreOptions> _options;
    private readonly TypeResolutionCache _cache;
    private readonly IServiceProvider _provider;

    public OpenIddictLinqToDBTokenStoreResolver(
        TypeResolutionCache cache,
        IOptionsMonitor<OpenIddictCoreOptions> options,
        IServiceProvider provider)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
    }

    /// <summary>
    /// Returns a token store compatible with the specified token type or throws an
    /// <see cref="InvalidOperationException"/> if no store can be built using the specified type.
    /// </summary>
    /// <typeparam name="TToken">The type of the Token entity.</typeparam>
    /// <returns>An <see cref="IOpenIddictTokenStore{TToken}"/>.</returns>
    public IOpenIddictTokenStore<TToken> Get<TToken>()
        where TToken : class
    {
        var store = _provider.GetService<IOpenIddictTokenStore<TToken>>();
        if (store is not null)
        {
            return store;
        }

        var type = _cache.GetOrAdd(typeof(TToken), key =>
        {
            var root = OpenIddictHelpers.FindGenericBaseType(key, typeof(OpenIddictLinqToDBToken<>)) ??
                       throw new InvalidOperationException(SR.GetResourceString(SR.ID0256));

            return typeof(OpenIddictLinqToDBTokenStore<,,,>).MakeGenericType(
                /* TToken: */ key,
                /* TApplication: */ _options.CurrentValue.DefaultApplicationType!,
                /* TAuthorization: */ _options.CurrentValue.DefaultAuthorizationType!,
                /* TKey: */ root.GenericTypeArguments[0]);
        });

        return (IOpenIddictTokenStore<TToken>) _provider.GetRequiredService(type);
    }
    
    // Note: LinqToDB resolvers are registered as scoped dependencies as their inner
    // service provider must be able to resolve scoped services (typically, the store they return).
    // To avoid having to declare a static type resolution cache, a special cache service is used
    // here and registered as a singleton dependency so that its content persists beyond the scope.
    public class TypeResolutionCache : ConcurrentDictionary<Type, Type> { }
}
