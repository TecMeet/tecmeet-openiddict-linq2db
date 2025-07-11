﻿/*
 * Licensed under the Apache License, Version 2.0 (http://www.apache.org/licenses/LICENSE-2.0)
 * See https://github.com/openiddict/openiddict-core for more information concerning
 * the license and the contributors participating to this project.
 */

using System.Collections.Immutable;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LinqToDB;
using Microsoft.Extensions.Caching.Memory;
using TecMeet.OpenIddict.LinqToDB.Models;
using static OpenIddict.Abstractions.OpenIddictExceptions;

namespace TecMeet.OpenIddict.LinqToDB;

/// <summary>
/// Provides methods allowing to manage the scopes stored in a database.
/// </summary>
public class OpenIddictLinqToDBScopeStore : OpenIddictLinqToDBScopeStore<OpenIddictLinqToDBScope, Guid>
{
    public OpenIddictLinqToDBScopeStore(
        IMemoryCache cache,
        IDataContext context)
        : base(cache, context)
    {
    }
}

/// <summary>
/// Provides methods allowing to manage the scopes stored in a database.
/// </summary>
/// <typeparam name="TKey">The type of the entity primary keys.</typeparam>
public class OpenIddictLinqToDBScopeStore<TKey> : OpenIddictLinqToDBScopeStore<OpenIddictLinqToDBScope<TKey>, TKey>
    where TKey : notnull, IEquatable<TKey>
{
    public OpenIddictLinqToDBScopeStore(
        IMemoryCache cache,
        IDataContext context)
        : base(cache, context)
    {
    }
}

/// <summary>
/// Provides methods allowing to manage the scopes stored in a database.
/// </summary>
/// <typeparam name="TScope">The type of the Scope entity.</typeparam>
/// <typeparam name="TKey">The type of the entity primary keys.</typeparam>
public class OpenIddictLinqToDBScopeStore<TScope, TKey> : IOpenIddictScopeStore<TScope>
    where TScope : OpenIddictLinqToDBScope<TKey>
    where TKey : notnull, IEquatable<TKey>
{
    public OpenIddictLinqToDBScopeStore(
        IMemoryCache cache,
        IDataContext context)
    {
        Cache = cache ?? throw new ArgumentNullException(nameof(cache));
        Context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Gets the memory cache associated with the current store.
    /// </summary>
    protected IMemoryCache Cache { get; }

    /// <summary>
    /// Gets the database context associated with the current store.
    /// </summary>
    protected IDataContext Context { get; }

    /// <summary>
    /// Gets the database set corresponding to the <typeparamref name="TScope"/> entity.
    /// </summary>
    private ITable<TScope> Scopes => Context.GetTable<TScope>();

    /// <inheritdoc/>
    public virtual async ValueTask<long> CountAsync(CancellationToken cancellationToken)
        => await Scopes.AsQueryable().LongCountAsync(cancellationToken);

    /// <inheritdoc/>
    public virtual async ValueTask<long> CountAsync<TResult>(Func<IQueryable<TScope>, IQueryable<TResult>> query, CancellationToken cancellationToken)
    {
        if (query is null)
        {
            throw new ArgumentNullException(nameof(query));
        }

        return await query(Scopes).LongCountAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async ValueTask CreateAsync(TScope scope, CancellationToken cancellationToken)
    {
        if (scope is null)
        {
            throw new ArgumentNullException(nameof(scope));
        }

        scope.Id = (TKey) await Context.InsertWithIdentityAsync(scope, token: cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async ValueTask DeleteAsync(TScope scope, CancellationToken cancellationToken)
    {
        if (scope is null)
        {
            throw new ArgumentNullException(nameof(scope));
        }

        if (await Scopes.Where(s =>
                    s.Id!.Equals(scope.Id) &&
                    s.ConcurrencyToken == scope.ConcurrencyToken)
                .DeleteAsync(cancellationToken) is 0)
        {
            throw new ConcurrencyException(SR.GetResourceString(SR.ID0245));
        }
    }

    /// <inheritdoc/>
    public virtual async ValueTask<TScope?> FindByIdAsync(string identifier, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(identifier))
        {
            throw new ArgumentException(SR.GetResourceString(SR.ID0195), nameof(identifier));
        }

        var key = ConvertIdentifierFromString(identifier);

        return await Scopes.FirstOrDefaultAsync(scope => scope.Id!.Equals(key), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async ValueTask<TScope?> FindByNameAsync(string name, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException(SR.GetResourceString(SR.ID0202), nameof(name));
        }

        return await Scopes.FirstOrDefaultAsync(scope => scope.Name == name, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual IAsyncEnumerable<TScope> FindByNamesAsync(
        ImmutableArray<string> names, CancellationToken cancellationToken)
    {
        if (names.Any(string.IsNullOrEmpty))
        {
            throw new ArgumentException(SR.GetResourceString(SR.ID0203), nameof(names));
        }

        return ExecuteAsync();
        async IAsyncEnumerable<TScope> ExecuteAsync()
        {
            await foreach (var scope in Scopes.Where(s => names.Contains(s.Name!)).AsAsyncEnumerable()
                               .WithCancellation(cancellationToken))
            {
                yield return scope;
            }
        }
    }

    /// <inheritdoc/>
    public virtual IAsyncEnumerable<TScope> FindByResourceAsync(
        string resource, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(resource))
        {
            throw new ArgumentException(SR.GetResourceString(SR.ID0062), nameof(resource));
        }

        // To optimize the efficiency of the query a bit, only scopes whose stringified
        // Resources column contains the specified resource are returned. Once the scopes
        // are retrieved, a second pass is made to ensure only valid elements are returned.
        // Implementers that use this method in a hot path may want to override this method
        // to use SQL Server 2016 functions like JSON_VALUE to make the query more efficient.

        return ExecuteAsync(cancellationToken);

        async IAsyncEnumerable<TScope> ExecuteAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var scopes = Scopes.Where(s => s.Resources != null && s.Resources.Contains(resource))
                .AsAsyncEnumerable().WithCancellation(cancellationToken);

            await foreach (var scope in scopes)
            {
                var resources = await GetResourcesAsync(scope, cancellationToken);
                if (resources.Contains(resource, StringComparer.Ordinal))
                {
                    yield return scope;
                }
            }
        }
    }

    /// <inheritdoc/>
    public virtual async ValueTask<TResult?> GetAsync<TState, TResult>(
        Func<IQueryable<TScope>, TState, IQueryable<TResult>> query,
        TState state, CancellationToken cancellationToken)
    {
        if (query is null)
        {
            throw new ArgumentNullException(nameof(query));
        }

        return await query(Scopes, state).FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public virtual ValueTask<string?> GetDescriptionAsync(TScope scope, CancellationToken cancellationToken)
    {
        if (scope is null)
        {
            throw new ArgumentNullException(nameof(scope));
        }

        return new(scope.Description);
    }

    /// <inheritdoc/>
    public virtual ValueTask<ImmutableDictionary<CultureInfo, string>> GetDescriptionsAsync(TScope scope, CancellationToken cancellationToken)
    {
        if (scope is null)
        {
            throw new ArgumentNullException(nameof(scope));
        }

        if (string.IsNullOrEmpty(scope.Descriptions))
        {
            return new(ImmutableDictionary.Create<CultureInfo, string>());
        }

        // Note: parsing the stringified descriptions is an expensive operation.
        // To mitigate that, the resulting object is stored in the memory cache.
        var key = string.Concat("42891062-8f69-43ba-9111-db7e8ded2553", "\x1e", scope.Descriptions);
        var descriptions = Cache.GetOrCreate(key, entry =>
        {
            entry.SetPriority(CacheItemPriority.High)
                 .SetSlidingExpiration(TimeSpan.FromMinutes(1));

            using var document = JsonDocument.Parse(scope.Descriptions);
            var builder = ImmutableDictionary.CreateBuilder<CultureInfo, string>();

            foreach (var property in document.RootElement.EnumerateObject())
            {
                var value = property.Value.GetString();
                if (string.IsNullOrEmpty(value))
                {
                    continue;
                }

                builder[CultureInfo.GetCultureInfo(property.Name)] = value;
            }

            return builder.ToImmutable();
        });

        return new(descriptions ?? ImmutableDictionary<CultureInfo, string>.Empty);
    }

    /// <inheritdoc/>
    public virtual ValueTask<string?> GetDisplayNameAsync(TScope scope, CancellationToken cancellationToken)
    {
        if (scope is null)
        {
            throw new ArgumentNullException(nameof(scope));
        }

        return new(scope.DisplayName);
    }

    /// <inheritdoc/>
    public virtual ValueTask<ImmutableDictionary<CultureInfo, string>> GetDisplayNamesAsync(TScope scope, CancellationToken cancellationToken)
    {
        if (scope is null)
        {
            throw new ArgumentNullException(nameof(scope));
        }

        if (string.IsNullOrEmpty(scope.DisplayNames))
        {
            return new(ImmutableDictionary.Create<CultureInfo, string>());
        }

        // Note: parsing the stringified display names is an expensive operation.
        // To mitigate that, the resulting object is stored in the memory cache.
        var key = string.Concat("e17d437b-bdd2-43f3-974e-46d524f4bae1", "\x1e", scope.DisplayNames);
        var names = Cache.GetOrCreate(key, entry =>
        {
            entry.SetPriority(CacheItemPriority.High)
                 .SetSlidingExpiration(TimeSpan.FromMinutes(1));

            using var document = JsonDocument.Parse(scope.DisplayNames);
            var builder = ImmutableDictionary.CreateBuilder<CultureInfo, string>();

            foreach (var property in document.RootElement.EnumerateObject())
            {
                var value = property.Value.GetString();
                if (string.IsNullOrEmpty(value))
                {
                    continue;
                }

                builder[CultureInfo.GetCultureInfo(property.Name)] = value;
            }

            return builder.ToImmutable();
        });

        return new(names ?? ImmutableDictionary<CultureInfo, string>.Empty);
    }

    /// <inheritdoc/>
    public virtual ValueTask<string?> GetIdAsync(TScope scope, CancellationToken cancellationToken)
    {
        if (scope is null)
        {
            throw new ArgumentNullException(nameof(scope));
        }

        return new(ConvertIdentifierToString(scope.Id));
    }

    /// <inheritdoc/>
    public virtual ValueTask<string?> GetNameAsync(TScope scope, CancellationToken cancellationToken)
    {
        if (scope is null)
        {
            throw new ArgumentNullException(nameof(scope));
        }

        return new(scope.Name);
    }

    /// <inheritdoc/>
    public virtual ValueTask<ImmutableDictionary<string, JsonElement>> GetPropertiesAsync(TScope scope, CancellationToken cancellationToken)
    {
        if (scope is null)
        {
            throw new ArgumentNullException(nameof(scope));
        }

        if (string.IsNullOrEmpty(scope.Properties))
        {
            return new(ImmutableDictionary.Create<string, JsonElement>());
        }

        // Note: parsing the stringified properties is an expensive operation.
        // To mitigate that, the resulting object is stored in the memory cache.
        var key = string.Concat("78d8dfdd-3870-442e-b62e-dc9bf6eaeff7", "\x1e", scope.Properties);
        var properties = Cache.GetOrCreate(key, entry =>
        {
            entry.SetPriority(CacheItemPriority.High)
                 .SetSlidingExpiration(TimeSpan.FromMinutes(1));

            using var document = JsonDocument.Parse(scope.Properties);
            var builder = ImmutableDictionary.CreateBuilder<string, JsonElement>();

            foreach (var property in document.RootElement.EnumerateObject())
            {
                builder[property.Name] = property.Value.Clone();
            }

            return builder.ToImmutable();
        });

        return new(properties ?? ImmutableDictionary<string, JsonElement>.Empty);
    }

    /// <inheritdoc/>
    public virtual ValueTask<ImmutableArray<string>> GetResourcesAsync(TScope scope, CancellationToken cancellationToken)
    {
        if (scope is null)
        {
            throw new ArgumentNullException(nameof(scope));
        }

        if (string.IsNullOrEmpty(scope.Resources))
        {
            return new(ImmutableArray.Create<string>());
        }

        // Note: parsing the stringified resources is an expensive operation.
        // To mitigate that, the resulting array is stored in the memory cache.
        var key = string.Concat("b6148250-aede-4fb9-a621-07c9bcf238c3", "\x1e", scope.Resources);
        var resources = Cache.GetOrCreate(key, entry =>
        {
            entry.SetPriority(CacheItemPriority.High)
                 .SetSlidingExpiration(TimeSpan.FromMinutes(1));

            using var document = JsonDocument.Parse(scope.Resources);
            var builder = ImmutableArray.CreateBuilder<string>(document.RootElement.GetArrayLength());

            foreach (var element in document.RootElement.EnumerateArray())
            {
                var value = element.GetString();
                if (string.IsNullOrEmpty(value))
                {
                    continue;
                }

                builder.Add(value);
            }

            return builder.ToImmutable();
        });

        return new(resources);
    }

    /// <inheritdoc/>
    public virtual ValueTask<TScope> InstantiateAsync(CancellationToken cancellationToken)
    {
        try
        {
            return new(Activator.CreateInstance<TScope>());
        }

        catch (MemberAccessException exception)
        {
            return new(Task.FromException<TScope>(
                new InvalidOperationException(SR.GetResourceString(SR.ID0246), exception)));
        }
    }

    /// <inheritdoc/>
    public virtual IAsyncEnumerable<TScope> ListAsync(int? count, int? offset, CancellationToken cancellationToken)
    {
        IQueryable<TScope> query = Scopes.AsQueryable().OrderBy(scope => scope.Id!);

        if (offset.HasValue)
        {
            query = query.Skip(offset.Value);
        }

        if (count.HasValue)
        {
            query = query.Take(count.Value);
        }

        return ExecuteAsync();
        async IAsyncEnumerable<TScope> ExecuteAsync()
        {
            await foreach (var q in query.AsAsyncEnumerable().WithCancellation(cancellationToken))
            {
                yield return q;
            }
        }
    }

    /// <inheritdoc/>
    public virtual IAsyncEnumerable<TResult> ListAsync<TState, TResult>(
        Func<IQueryable<TScope>, TState, IQueryable<TResult>> query,
        TState state, CancellationToken cancellationToken)
    {
        if (query is null)
        {
            throw new ArgumentNullException(nameof(query));
        }

        return ExecuteAsync();
        async IAsyncEnumerable<TResult> ExecuteAsync()
        {
            await foreach (var q in query(Scopes, state).AsAsyncEnumerable().WithCancellation(cancellationToken))
            {
                yield return q;
            }
        }
    }

    /// <inheritdoc/>
    public virtual ValueTask SetDescriptionAsync(TScope scope, string? description, CancellationToken cancellationToken)
    {
        if (scope is null)
        {
            throw new ArgumentNullException(nameof(scope));
        }

        scope.Description = description;

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetDescriptionsAsync(TScope scope,
        ImmutableDictionary<CultureInfo, string> descriptions, CancellationToken cancellationToken)
    {
        if (scope is null)
        {
            throw new ArgumentNullException(nameof(scope));
        }

        if (descriptions is not { Count: > 0 })
        {
            scope.Descriptions = null;

            return default;
        }

        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Indented = false
        });

        writer.WriteStartObject();

        foreach (var description in descriptions)
        {
            writer.WritePropertyName(description.Key.Name);
            writer.WriteStringValue(description.Value);
        }

        writer.WriteEndObject();
        writer.Flush();

        scope.Descriptions = Encoding.UTF8.GetString(stream.ToArray());

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetDisplayNameAsync(TScope scope, string? name, CancellationToken cancellationToken)
    {
        if (scope is null)
        {
            throw new ArgumentNullException(nameof(scope));
        }

        scope.DisplayName = name;

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetDisplayNamesAsync(TScope scope,
        ImmutableDictionary<CultureInfo, string> names, CancellationToken cancellationToken)
    {
        if (scope is null)
        {
            throw new ArgumentNullException(nameof(scope));
        }

        if (names is not { Count: > 0 })
        {
            scope.DisplayNames = null;

            return default;
        }

        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Indented = false
        });

        writer.WriteStartObject();

        foreach (var name in names)
        {
            writer.WritePropertyName(name.Key.Name);
            writer.WriteStringValue(name.Value);
        }

        writer.WriteEndObject();
        writer.Flush();

        scope.DisplayNames = Encoding.UTF8.GetString(stream.ToArray());

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetNameAsync(TScope scope, string? name, CancellationToken cancellationToken)
    {
        if (scope is null)
        {
            throw new ArgumentNullException(nameof(scope));
        }

        scope.Name = name;

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetPropertiesAsync(TScope scope,
        ImmutableDictionary<string, JsonElement> properties, CancellationToken cancellationToken)
    {
        if (scope is null)
        {
            throw new ArgumentNullException(nameof(scope));
        }

        if (properties is not { Count: > 0 })
        {
            scope.Properties = null;

            return default;
        }

        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Indented = false
        });

        writer.WriteStartObject();

        foreach (var property in properties)
        {
            writer.WritePropertyName(property.Key);
            property.Value.WriteTo(writer);
        }

        writer.WriteEndObject();
        writer.Flush();

        scope.Properties = Encoding.UTF8.GetString(stream.ToArray());

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetResourcesAsync(TScope scope, ImmutableArray<string> resources, CancellationToken cancellationToken)
    {
        if (scope is null)
        {
            throw new ArgumentNullException(nameof(scope));
        }

        if (resources.IsDefaultOrEmpty)
        {
            scope.Resources = null;

            return default;
        }

        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Indented = false
        });

        writer.WriteStartArray();

        foreach (var resource in resources)
        {
            writer.WriteStringValue(resource);
        }

        writer.WriteEndArray();
        writer.Flush();

        scope.Resources = Encoding.UTF8.GetString(stream.ToArray());

        return default;
    }

    /// <inheritdoc/>
    public virtual async ValueTask UpdateAsync(TScope scope, CancellationToken cancellationToken)
    {
        if (scope is null)
        {
            throw new ArgumentNullException(nameof(scope));
        }

        // Do manual concurrency check before updating entity in db because
        // LinqToDB can not do "where" check when updating entire entity.
        var concurrencyChecked =
            await Scopes.AnyAsync(i => i.Id!.Equals(scope.Id) && i.ConcurrencyToken == scope.ConcurrencyToken,
                token: cancellationToken);
        
        if (!concurrencyChecked)
        {
            throw new ConcurrencyException(SR.GetResourceString(SR.ID0245));
        }

        // Generate a new concurrency token and attach it
        // to the token before persisting the changes.
        scope.ConcurrencyToken = Guid.NewGuid().ToString();

        await Context.UpdateAsync(scope, token: cancellationToken);
    }

    /// <summary>
    /// Converts the provided identifier to a strongly typed key object.
    /// </summary>
    /// <param name="identifier">The identifier to convert.</param>
    /// <returns>An instance of <typeparamref name="TKey"/> representing the provided identifier.</returns>
    public virtual TKey? ConvertIdentifierFromString(string? identifier)
    {
        if (string.IsNullOrEmpty(identifier))
        {
            return default;
        }

        return (TKey?) TypeDescriptor.GetConverter(typeof(TKey)).ConvertFromInvariantString(identifier);
    }

    /// <summary>
    /// Converts the provided identifier to its string representation.
    /// </summary>
    /// <param name="identifier">The identifier to convert.</param>
    /// <returns>A <see cref="string"/> representation of the provided identifier.</returns>
    public virtual string? ConvertIdentifierToString(TKey? identifier)
    {
        if (Equals(identifier, default(TKey)))
        {
            return null;
        }

        return TypeDescriptor.GetConverter(typeof(TKey)).ConvertToInvariantString(identifier);
    }
}
