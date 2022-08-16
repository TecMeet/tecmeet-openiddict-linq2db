/*
 * Licensed under the Apache License, Version 2.0 (http://www.apache.org/licenses/LICENSE-2.0)
 * See https://github.com/openiddict/openiddict-core for more information concerning
 * the license and the contributors participating to this project.
 */

using System.Collections.Immutable;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LinqToDB;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using TecMeet.OpenIddict.LinqToDB.Models;
using static OpenIddict.Abstractions.OpenIddictExceptions;

namespace TecMeet.OpenIddict.LinqToDB;

/// <summary>
/// Provides methods allowing to manage the applications stored in a database.
/// </summary>
/// <typeparam name="TContext">The type of the LinqToDB database context.</typeparam>
public class OpenIddictLinqToDBApplicationStore<TContext> :
    OpenIddictLinqToDBApplicationStore<OpenIddictLinqToDBApplication,
                                                  OpenIddictLinqToDBAuthorization,
                                                  OpenIddictLinqToDBToken, TContext, string>
    where TContext : IDataContext
{
    public OpenIddictLinqToDBApplicationStore(
        IMemoryCache cache,
        TContext context,
        IOptionsMonitor<OpenIddictLinqToDBOptions> options)
        : base(cache, context, options)
    {
    }
}

/// <summary>
/// Provides methods allowing to manage the applications stored in a database.
/// </summary>
/// <typeparam name="TContext">The type of the LinqToDB database context.</typeparam>
/// <typeparam name="TKey">The type of the entity primary keys.</typeparam>
public class OpenIddictLinqToDBApplicationStore<TContext, TKey> :
    OpenIddictLinqToDBApplicationStore<OpenIddictLinqToDBApplication<TKey>,
                                                  OpenIddictLinqToDBAuthorization<TKey>,
                                                  OpenIddictLinqToDBToken<TKey>, TContext, TKey>
    where TContext : IDataContext
    where TKey : notnull, IEquatable<TKey>
{
    public OpenIddictLinqToDBApplicationStore(
        IMemoryCache cache,
        TContext context,
        IOptionsMonitor<OpenIddictLinqToDBOptions> options)
        : base(cache, context, options)
    {
    }
}

/// <summary>
/// Provides methods allowing to manage the applications stored in a database.
/// </summary>
/// <typeparam name="TApplication">The type of the Application entity.</typeparam>
/// <typeparam name="TAuthorization">The type of the Authorization entity.</typeparam>
/// <typeparam name="TToken">The type of the Token entity.</typeparam>
/// <typeparam name="TContext">The type of the LinqToDB database context.</typeparam>
/// <typeparam name="TKey">The type of the entity primary keys.</typeparam>
public class OpenIddictLinqToDBApplicationStore<TApplication, TAuthorization, TToken, TContext, TKey> : IOpenIddictApplicationStore<TApplication>
    where TApplication : OpenIddictLinqToDBApplication<TKey>
    where TAuthorization : OpenIddictLinqToDBAuthorization<TKey>
    where TToken : OpenIddictLinqToDBToken<TKey>
    where TContext : IDataContext
    where TKey : notnull, IEquatable<TKey>
{
    public OpenIddictLinqToDBApplicationStore(
        IMemoryCache cache,
        TContext context,
        IOptionsMonitor<OpenIddictLinqToDBOptions> options)
    {
        Cache = cache ?? throw new ArgumentNullException(nameof(cache));
        Context = context ?? throw new ArgumentNullException(nameof(context));
        Options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Gets the memory cache associated with the current store.
    /// </summary>
    protected IMemoryCache Cache { get; }

    /// <summary>
    /// Gets the database context associated with the current store.
    /// </summary>
    protected TContext Context { get; }

    /// <summary>
    /// Gets the options associated with the current store.
    /// </summary>
    protected IOptionsMonitor<OpenIddictLinqToDBOptions> Options { get; }

    /// <summary>
    /// Gets the database set corresponding to the <typeparamref name="TApplication"/> entity.
    /// </summary>
    private ITable<TApplication> Applications => Context.GetTable<TApplication>();

    /// <summary>
    /// Gets the database set corresponding to the <typeparamref name="TAuthorization"/> entity.
    /// </summary>
    private ITable<TAuthorization> Authorizations => Context.GetTable<TAuthorization>();

    /// <summary>
    /// Gets the database set corresponding to the <typeparamref name="TToken"/> entity.
    /// </summary>
    private ITable<TToken> Tokens => Context.GetTable<TToken>();

    /// <inheritdoc/>
    public virtual async ValueTask<long> CountAsync(CancellationToken cancellationToken)
        => await Applications.AsQueryable().LongCountAsync(cancellationToken);

    /// <inheritdoc/>
    public virtual async ValueTask<long> CountAsync<TResult>(Func<IQueryable<TApplication>, IQueryable<TResult>> query, CancellationToken cancellationToken)
    {
        if (query is null)
        {
            throw new ArgumentNullException(nameof(query));
        }

        return await query(Applications).LongCountAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async ValueTask CreateAsync(TApplication application, CancellationToken cancellationToken)
    {
        if (application is null)
        {
            throw new ArgumentNullException(nameof(application));
        }

        await Context.InsertAsync(application, token: cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async ValueTask DeleteAsync(TApplication application, CancellationToken cancellationToken)
    {
        if (application is null)
        {
            throw new ArgumentNullException(nameof(application));
        }
        
        if (await Applications.Where(entity =>
                    entity.Id!.Equals(application.Id) &&
                    entity.ConcurrencyToken == application.ConcurrencyToken)
                .DeleteAsync(cancellationToken) is 0)
        {
            throw new ConcurrencyException(SR.GetResourceString(SR.ID0247));
        }
        
        // Delete the authorizations associated with the application.
        await Authorizations.Where(auth => auth.ApplicationId != null && auth.ApplicationId.Equals(application.Id))
            .DeleteAsync(cancellationToken);
        
        // Delete the tokens associated with the application.
        await Tokens.Where(token => token.ApplicationId != null && token.ApplicationId.Equals(application.Id))
            .DeleteAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async ValueTask<TApplication?> FindByClientIdAsync(string identifier, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(identifier))
        {
            throw new ArgumentException(SR.GetResourceString(SR.ID0195), nameof(identifier));
        }

        return await Applications.FirstOrDefaultAsync(app => app.ClientId == identifier, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async ValueTask<TApplication?> FindByIdAsync(string identifier, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(identifier))
        {
            throw new ArgumentException(SR.GetResourceString(SR.ID0195), nameof(identifier));
        }

        var key = ConvertIdentifierFromString(identifier);

        return await Applications.FirstOrDefaultAsync(app => app.Id!.Equals(key), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual IAsyncEnumerable<TApplication> FindByPostLogoutRedirectUriAsync(
        string address, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(address))
        {
            throw new ArgumentException(SR.GetResourceString(SR.ID0143), nameof(address));
        }

        // To optimize the efficiency of the query a bit, only applications whose stringified
        // PostLogoutRedirectUris contains the specified URL are returned. Once the applications
        // are retrieved, a second pass is made to ensure only valid elements are returned.
        // Implementers that use this method in a hot path may want to override this method
        // to use SQL Server 2016 functions like JSON_VALUE to make the query more efficient.

        return ExecuteAsync();

        async IAsyncEnumerable<TApplication> ExecuteAsync()
        {
            var applications =
                Applications.Where(app => app.PostLogoutRedirectUris != null &&
                                          app.PostLogoutRedirectUris.Contains(address));
            
            await foreach (var application in applications.AsAsyncEnumerable(cancellationToken))
            {
                var addresses = await GetPostLogoutRedirectUrisAsync(application, cancellationToken);
                if (addresses.Contains(address, StringComparer.Ordinal))
                {
                    yield return application;
                }
            }
        }
    }

    /// <inheritdoc/>
    public virtual IAsyncEnumerable<TApplication> FindByRedirectUriAsync(
        string address, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(address))
        {
            throw new ArgumentException(SR.GetResourceString(SR.ID0143), nameof(address));
        }

        // To optimize the efficiency of the query a bit, only applications whose stringified
        // RedirectUris property contains the specified URL are returned. Once the applications
        // are retrieved, a second pass is made to ensure only valid elements are returned.
        // Implementers that use this method in a hot path may want to override this method
        // to use SQL Server 2016 functions like JSON_VALUE to make the query more efficient.

        return ExecuteAsync(cancellationToken);

        async IAsyncEnumerable<TApplication> ExecuteAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var applications =
                Applications.Where(app => app.RedirectUris != null &&
                                          app.RedirectUris.Contains(address));
            
            await foreach (var application in applications.AsAsyncEnumerable(cancellationToken))
            {
                var addresses = await GetRedirectUrisAsync(application, cancellationToken);
                if (addresses.Contains(address, StringComparer.Ordinal))
                {
                    yield return application;
                }
            }
        }
    }

    /// <inheritdoc/>
    public virtual async ValueTask<TResult?> GetAsync<TState, TResult>(
        Func<IQueryable<TApplication>, TState, IQueryable<TResult>> query,
        TState state, CancellationToken cancellationToken)
    {
        if (query is null)
        {
            throw new ArgumentNullException(nameof(query));
        }

        return await query(Applications, state).FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public virtual ValueTask<string?> GetClientIdAsync(TApplication application, CancellationToken cancellationToken)
    {
        if (application is null)
        {
            throw new ArgumentNullException(nameof(application));
        }

        return new(application.ClientId);
    }

    /// <inheritdoc/>
    public virtual ValueTask<string?> GetClientSecretAsync(TApplication application, CancellationToken cancellationToken)
    {
        if (application is null)
        {
            throw new ArgumentNullException(nameof(application));
        }

        return new(application.ClientSecret);
    }

    /// <inheritdoc/>
    public virtual ValueTask<string?> GetClientTypeAsync(TApplication application, CancellationToken cancellationToken)
    {
        if (application is null)
        {
            throw new ArgumentNullException(nameof(application));
        }

        return new(application.Type);
    }

    /// <inheritdoc/>
    public virtual ValueTask<string?> GetConsentTypeAsync(TApplication application, CancellationToken cancellationToken)
    {
        if (application is null)
        {
            throw new ArgumentNullException(nameof(application));
        }

        return new(application.ConsentType);
    }

    /// <inheritdoc/>
    public virtual ValueTask<string?> GetDisplayNameAsync(TApplication application, CancellationToken cancellationToken)
    {
        if (application is null)
        {
            throw new ArgumentNullException(nameof(application));
        }

        return new(application.DisplayName);
    }

    /// <inheritdoc/>
    public virtual ValueTask<ImmutableDictionary<CultureInfo, string>> GetDisplayNamesAsync(TApplication application, CancellationToken cancellationToken)
    {
        if (application is null)
        {
            throw new ArgumentNullException(nameof(application));
        }

        if (string.IsNullOrEmpty(application.DisplayNames))
        {
            return new(ImmutableDictionary.Create<CultureInfo, string>());
        }

        // Note: parsing the stringified display names is an expensive operation.
        // To mitigate that, the resulting object is stored in the memory cache.
        var key = string.Concat("7762c378-c113-4564-b14b-1402b3949aaa", "\x1e", application.DisplayNames);
        var names = Cache.GetOrCreate(key, entry =>
        {
            entry.SetPriority(CacheItemPriority.High)
                 .SetSlidingExpiration(TimeSpan.FromMinutes(1));

            using var document = JsonDocument.Parse(application.DisplayNames);
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

        return new(names);
    }

    /// <inheritdoc/>
    public virtual ValueTask<string?> GetIdAsync(TApplication application, CancellationToken cancellationToken)
    {
        if (application is null)
        {
            throw new ArgumentNullException(nameof(application));
        }

        return new(ConvertIdentifierToString(application.Id));
    }

    /// <inheritdoc/>
    public virtual ValueTask<ImmutableArray<string>> GetPermissionsAsync(TApplication application, CancellationToken cancellationToken)
    {
        if (application is null)
        {
            throw new ArgumentNullException(nameof(application));
        }

        if (string.IsNullOrEmpty(application.Permissions))
        {
            return new(ImmutableArray.Create<string>());
        }

        // Note: parsing the stringified permissions is an expensive operation.
        // To mitigate that, the resulting array is stored in the memory cache.
        var key = string.Concat("0347e0aa-3a26-410a-97e8-a83bdeb21a1f", "\x1e", application.Permissions);
        var permissions = Cache.GetOrCreate(key, entry =>
        {
            entry.SetPriority(CacheItemPriority.High)
                 .SetSlidingExpiration(TimeSpan.FromMinutes(1));

            using var document = JsonDocument.Parse(application.Permissions);
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

        return new(permissions);
    }

    /// <inheritdoc/>
    public virtual ValueTask<ImmutableArray<string>> GetPostLogoutRedirectUrisAsync(TApplication application, CancellationToken cancellationToken)
    {
        if (application is null)
        {
            throw new ArgumentNullException(nameof(application));
        }

        if (string.IsNullOrEmpty(application.PostLogoutRedirectUris))
        {
            return new(ImmutableArray.Create<string>());
        }

        // Note: parsing the stringified addresses is an expensive operation.
        // To mitigate that, the resulting array is stored in the memory cache.
        var key = string.Concat("fb14dfb9-9216-4b77-bfa9-7e85f8201ff4", "\x1e", application.PostLogoutRedirectUris);
        var addresses = Cache.GetOrCreate(key, entry =>
        {
            entry.SetPriority(CacheItemPriority.High)
                 .SetSlidingExpiration(TimeSpan.FromMinutes(1));

            using var document = JsonDocument.Parse(application.PostLogoutRedirectUris);
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

        return new(addresses);
    }

    /// <inheritdoc/>
    public virtual ValueTask<ImmutableDictionary<string, JsonElement>> GetPropertiesAsync(TApplication application, CancellationToken cancellationToken)
    {
        if (application is null)
        {
            throw new ArgumentNullException(nameof(application));
        }

        if (string.IsNullOrEmpty(application.Properties))
        {
            return new(ImmutableDictionary.Create<string, JsonElement>());
        }

        // Note: parsing the stringified properties is an expensive operation.
        // To mitigate that, the resulting object is stored in the memory cache.
        var key = string.Concat("2e3e9680-5654-48d8-a27d-b8bb4f0f1d50", "\x1e", application.Properties);
        var properties = Cache.GetOrCreate(key, entry =>
        {
            entry.SetPriority(CacheItemPriority.High)
                 .SetSlidingExpiration(TimeSpan.FromMinutes(1));

            using var document = JsonDocument.Parse(application.Properties);
            var builder = ImmutableDictionary.CreateBuilder<string, JsonElement>();

            foreach (var property in document.RootElement.EnumerateObject())
            {
                builder[property.Name] = property.Value.Clone();
            }

            return builder.ToImmutable();
        });

        return new(properties);
    }

    /// <inheritdoc/>
    public virtual ValueTask<ImmutableArray<string>> GetRedirectUrisAsync(TApplication application, CancellationToken cancellationToken)
    {
        if (application is null)
        {
            throw new ArgumentNullException(nameof(application));
        }

        if (string.IsNullOrEmpty(application.RedirectUris))
        {
            return new(ImmutableArray.Create<string>());
        }

        // Note: parsing the stringified addresses is an expensive operation.
        // To mitigate that, the resulting array is stored in the memory cache.
        var key = string.Concat("851d6f08-2ee0-4452-bbe5-ab864611ecaa", "\x1e", application.RedirectUris);
        var addresses = Cache.GetOrCreate(key, entry =>
        {
            entry.SetPriority(CacheItemPriority.High)
                 .SetSlidingExpiration(TimeSpan.FromMinutes(1));

            using var document = JsonDocument.Parse(application.RedirectUris);
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

        return new(addresses);
    }

    /// <inheritdoc/>
    public virtual ValueTask<ImmutableArray<string>> GetRequirementsAsync(TApplication application, CancellationToken cancellationToken)
    {
        if (application is null)
        {
            throw new ArgumentNullException(nameof(application));
        }

        if (string.IsNullOrEmpty(application.Requirements))
        {
            return new(ImmutableArray.Create<string>());
        }

        // Note: parsing the stringified requirements is an expensive operation.
        // To mitigate that, the resulting array is stored in the memory cache.
        var key = string.Concat("b4808a89-8969-4512-895f-a909c62a8995", "\x1e", application.Requirements);
        var requirements = Cache.GetOrCreate(key, entry =>
        {
            entry.SetPriority(CacheItemPriority.High)
                 .SetSlidingExpiration(TimeSpan.FromMinutes(1));

            using var document = JsonDocument.Parse(application.Requirements);
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

        return new(requirements);
    }

    /// <inheritdoc/>
    public virtual ValueTask<TApplication> InstantiateAsync(CancellationToken cancellationToken)
    {
        try
        {
            return new(Activator.CreateInstance<TApplication>());
        }

        catch (MemberAccessException exception)
        {
            return new(Task.FromException<TApplication>(
                new InvalidOperationException(SR.GetResourceString(SR.ID0240), exception)));
        }
    }

    /// <inheritdoc/>
    public virtual IAsyncEnumerable<TApplication> ListAsync(int? count, int? offset, CancellationToken cancellationToken)
    {
        IQueryable<TApplication> query = Applications.OrderBy(application => application.Id!);

        if (offset.HasValue)
        {
            query = query.Skip(offset.Value);
        }

        if (count.HasValue)
        {
            query = query.Take(count.Value);
        }

        return query.AsAsyncEnumerable(cancellationToken);
    }

    /// <inheritdoc/>
    public virtual IAsyncEnumerable<TResult> ListAsync<TState, TResult>(
        Func<IQueryable<TApplication>, TState, IQueryable<TResult>> query,
        TState state, CancellationToken cancellationToken)
    {
        if (query is null)
        {
            throw new ArgumentNullException(nameof(query));
        }

        return query(Applications, state).AsAsyncEnumerable(cancellationToken);
    }

    /// <inheritdoc/>
    public virtual ValueTask SetClientIdAsync(TApplication application, string? identifier, CancellationToken cancellationToken)
    {
        if (application is null)
        {
            throw new ArgumentNullException(nameof(application));
        }

        application.ClientId = identifier;

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetClientSecretAsync(TApplication application, string? secret, CancellationToken cancellationToken)
    {
        if (application is null)
        {
            throw new ArgumentNullException(nameof(application));
        }

        application.ClientSecret = secret;

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetClientTypeAsync(TApplication application, string? type, CancellationToken cancellationToken)
    {
        if (application is null)
        {
            throw new ArgumentNullException(nameof(application));
        }

        application.Type = type;

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetConsentTypeAsync(TApplication application, string? type, CancellationToken cancellationToken)
    {
        if (application is null)
        {
            throw new ArgumentNullException(nameof(application));
        }

        application.ConsentType = type;

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetDisplayNameAsync(TApplication application, string? name, CancellationToken cancellationToken)
    {
        if (application is null)
        {
            throw new ArgumentNullException(nameof(application));
        }

        application.DisplayName = name;

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetDisplayNamesAsync(TApplication application,
        ImmutableDictionary<CultureInfo, string> names, CancellationToken cancellationToken)
    {
        if (application is null)
        {
            throw new ArgumentNullException(nameof(application));
        }

        if (names is not { Count: > 0 })
        {
            application.DisplayNames = null;

            return default;
        }

        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Indented = false
        });

        writer.WriteStartObject();

        foreach (var pair in names)
        {
            writer.WritePropertyName(pair.Key.Name);
            writer.WriteStringValue(pair.Value);
        }

        writer.WriteEndObject();
        writer.Flush();

        application.DisplayNames = Encoding.UTF8.GetString(stream.ToArray());

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetPermissionsAsync(TApplication application, ImmutableArray<string> permissions, CancellationToken cancellationToken)
    {
        if (application is null)
        {
            throw new ArgumentNullException(nameof(application));
        }

        if (permissions.IsDefaultOrEmpty)
        {
            application.Permissions = null;

            return default;
        }

        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Indented = false
        });

        writer.WriteStartArray();

        foreach (var permission in permissions)
        {
            writer.WriteStringValue(permission);
        }

        writer.WriteEndArray();
        writer.Flush();

        application.Permissions = Encoding.UTF8.GetString(stream.ToArray());

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetPostLogoutRedirectUrisAsync(TApplication application,
        ImmutableArray<string> addresses, CancellationToken cancellationToken)
    {
        if (application is null)
        {
            throw new ArgumentNullException(nameof(application));
        }

        if (addresses.IsDefaultOrEmpty)
        {
            application.PostLogoutRedirectUris = null;

            return default;
        }

        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Indented = false
        });

        writer.WriteStartArray();

        foreach (var address in addresses)
        {
            writer.WriteStringValue(address);
        }

        writer.WriteEndArray();
        writer.Flush();

        application.PostLogoutRedirectUris = Encoding.UTF8.GetString(stream.ToArray());

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetPropertiesAsync(TApplication application,
        ImmutableDictionary<string, JsonElement> properties, CancellationToken cancellationToken)
    {
        if (application is null)
        {
            throw new ArgumentNullException(nameof(application));
        }

        if (properties is not { Count: > 0 })
        {
            application.Properties = null;

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

        application.Properties = Encoding.UTF8.GetString(stream.ToArray());

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetRedirectUrisAsync(TApplication application,
        ImmutableArray<string> addresses, CancellationToken cancellationToken)
    {
        if (application is null)
        {
            throw new ArgumentNullException(nameof(application));
        }

        if (addresses.IsDefaultOrEmpty)
        {
            application.RedirectUris = null;

            return default;
        }

        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Indented = false
        });

        writer.WriteStartArray();

        foreach (var address in addresses)
        {
            writer.WriteStringValue(address);
        }

        writer.WriteEndArray();
        writer.Flush();

        application.RedirectUris = Encoding.UTF8.GetString(stream.ToArray());

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetRequirementsAsync(TApplication application, ImmutableArray<string> requirements, CancellationToken cancellationToken)
    {
        if (application is null)
        {
            throw new ArgumentNullException(nameof(application));
        }

        if (requirements.IsDefaultOrEmpty)
        {
            application.Requirements = null;

            return default;
        }

        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Indented = false
        });

        writer.WriteStartArray();

        foreach (var requirement in requirements)
        {
            writer.WriteStringValue(requirement);
        }

        writer.WriteEndArray();
        writer.Flush();

        application.Requirements = Encoding.UTF8.GetString(stream.ToArray());

        return default;
    }

    /// <inheritdoc/>
    public virtual async ValueTask UpdateAsync(TApplication application, CancellationToken cancellationToken)
    {
        if (application is null)
        {
            throw new ArgumentNullException(nameof(application));
        }

        // Do manual concurrency check before updating entity in db because
        // LinqToDB can not do "where" check when updating entire entity.
        var concurrencyChecked =
            await Tokens.AnyAsync(i => i.Id!.Equals(application.Id) && i.ConcurrencyToken == application.ConcurrencyToken,
                token: cancellationToken);
        
        if (!concurrencyChecked)
        {
            throw new ConcurrencyException(SR.GetResourceString(SR.ID0239));
        }

        // Generate a new concurrency token and attach it
        // to the token before persisting the changes.
        application.ConcurrencyToken = Guid.NewGuid().ToString();

        await Context.UpdateAsync(application, token: cancellationToken);
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
