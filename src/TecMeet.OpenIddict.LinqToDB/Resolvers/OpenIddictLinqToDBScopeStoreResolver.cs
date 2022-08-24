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
/// Exposes a method allowing to resolve a scope store.
/// </summary>
public class OpenIddictLinqToDBScopeStoreResolver : IOpenIddictScopeStoreResolver
{
    private readonly TypeResolutionCache _cache;
    private readonly IServiceProvider _provider;

    public OpenIddictLinqToDBScopeStoreResolver(
        TypeResolutionCache cache,
        IServiceProvider provider)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
    }

    /// <summary>
    /// Returns a scope store compatible with the specified scope type or throws an
    /// <see cref="InvalidOperationException"/> if no store can be built using the specified type.
    /// </summary>
    /// <typeparam name="TScope">The type of the Scope entity.</typeparam>
    /// <returns>An <see cref="IOpenIddictScopeStore{TScope}"/>.</returns>
    public IOpenIddictScopeStore<TScope> Get<TScope>() where TScope : class
    {
        var store = _provider.GetService<IOpenIddictScopeStore<TScope>>();
        if (store is not null)
        {
            return store;
        }

        var type = _cache.GetOrAdd(typeof(TScope), key =>
        {
            var root = OpenIddictHelpers.FindGenericBaseType(key, typeof(OpenIddictLinqToDBScope<>)) ??
                       throw new InvalidOperationException(SR.GetResourceString(SR.ID0256));

            return typeof(OpenIddictLinqToDBScopeStore<,>).MakeGenericType(
                /* TScope: */ key,
                /* TKey: */ root.GenericTypeArguments[0]);
        });

        return (IOpenIddictScopeStore<TScope>) _provider.GetRequiredService(type);
    }
    
    // Note: LinqToDB resolvers are registered as scoped dependencies as their inner
    // service provider must be able to resolve scoped services (typically, the store they return).
    // To avoid having to declare a static type resolution cache, a special cache service is used
    // here and registered as a singleton dependency so that its content persists beyond the scope.
    public class TypeResolutionCache : ConcurrentDictionary<Type, Type> { }
}
