﻿/*
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
/// Exposes a method allowing to resolve an application store.
/// </summary>
public class OpenIddictLinqToDBApplicationStoreResolver : IOpenIddictApplicationStoreResolver
{
    private readonly ConcurrentDictionary<Type, Type> _cache = new();
    private readonly IOptionsMonitor<OpenIddictCoreOptions> _options;
    private readonly IServiceProvider _provider;

    public OpenIddictLinqToDBApplicationStoreResolver(
        IOptionsMonitor<OpenIddictCoreOptions> options,
        IServiceProvider provider)
    {
        _options = options;
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
    }

    /// <summary>
    /// Returns an application store compatible with the specified application type or throws an
    /// <see cref="InvalidOperationException"/> if no store can be built using the specified type.
    /// </summary>
    /// <typeparam name="TApplication">The type of the Application entity.</typeparam>
    /// <returns>An <see cref="IOpenIddictApplicationStore{TApplication}"/>.</returns>
    public IOpenIddictApplicationStore<TApplication> Get<TApplication>() where TApplication : class
    {
        var store = _provider.GetService<IOpenIddictApplicationStore<TApplication>>();
        if (store is not null)
        {
            return store;
        }

        var type = _cache.GetOrAdd(typeof(TApplication), key =>
        {
            var root = OpenIddictHelpers.FindGenericBaseType(key, typeof(OpenIddictLinqToDBApplication<>)) ??
                       throw new InvalidOperationException(SR.GetResourceString(SR.ID0256));

            return typeof(OpenIddictLinqToDBApplicationStore<,,,>).MakeGenericType(
                /* TApplication: */ key,
                /* TAuthorization: */ _options.CurrentValue.DefaultAuthorizationType!,
                /* TToken: */ _options.CurrentValue.DefaultTokenType!,
                /* TKey: */ root.GenericTypeArguments[0]);
        });

        return (IOpenIddictApplicationStore<TApplication>) _provider.GetRequiredService(type);
    }
}
