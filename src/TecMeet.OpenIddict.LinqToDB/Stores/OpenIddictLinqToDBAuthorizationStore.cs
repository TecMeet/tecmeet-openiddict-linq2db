/*
 * Licensed under the Apache License, Version 2.0 (http://www.apache.org/licenses/LICENSE-2.0)
 * See https://github.com/openiddict/openiddict-core for more information concerning
 * the license and the contributors participating to this project.
 */

using System.Collections.Immutable;
using System.ComponentModel;
using System.Data;
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
/// Provides methods allowing to manage the authorizations stored in a database.
/// </summary>
/// <typeparam name="TContext">The type of the LinqToDB database context.</typeparam>
public class OpenIddictLinqToDBAuthorizationStore<TContext> :
    OpenIddictLinqToDBAuthorizationStore<OpenIddictLinqToDBAuthorization,
                                                    OpenIddictLinqToDBApplication,
                                                    OpenIddictLinqToDBToken, TContext, string>
    where TContext : IDataContext
{
    public OpenIddictLinqToDBAuthorizationStore(
        IMemoryCache cache,
        TContext context,
        IOptionsMonitor<OpenIddictLinqToDBOptions> options)
        : base(cache, context, options)
    {
    }
}

/// <summary>
/// Provides methods allowing to manage the authorizations stored in a database.
/// </summary>
/// <typeparam name="TContext">The type of the LinqToDB database context.</typeparam>
/// <typeparam name="TKey">The type of the entity primary keys.</typeparam>
public class OpenIddictLinqToDBAuthorizationStore<TContext, TKey> :
    OpenIddictLinqToDBAuthorizationStore<OpenIddictLinqToDBAuthorization<TKey>,
                                                    OpenIddictLinqToDBApplication<TKey>,
                                                    OpenIddictLinqToDBToken<TKey>, TContext, TKey>
    where TContext : IDataContext
    where TKey : notnull, IEquatable<TKey>
{
    public OpenIddictLinqToDBAuthorizationStore(
        IMemoryCache cache,
        TContext context,
        IOptionsMonitor<OpenIddictLinqToDBOptions> options)
        : base(cache, context, options)
    {
    }
}

/// <summary>
/// Provides methods allowing to manage the authorizations stored in a database.
/// </summary>
/// <typeparam name="TAuthorization">The type of the Authorization entity.</typeparam>
/// <typeparam name="TApplication">The type of the Application entity.</typeparam>
/// <typeparam name="TToken">The type of the Token entity.</typeparam>
/// <typeparam name="TContext">The type of the LinqToDB database context.</typeparam>
/// <typeparam name="TKey">The type of the entity primary keys.</typeparam>
public class OpenIddictLinqToDBAuthorizationStore<TAuthorization, TApplication, TToken, TContext, TKey> : IOpenIddictAuthorizationStore<TAuthorization>
    where TAuthorization : OpenIddictLinqToDBAuthorization<TKey>
    where TApplication : OpenIddictLinqToDBApplication<TKey>
    where TToken : OpenIddictLinqToDBToken<TKey>
    where TContext : IDataContext
    where TKey : notnull, IEquatable<TKey>
{
    public OpenIddictLinqToDBAuthorizationStore(
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
        => await Authorizations.AsQueryable().LongCountAsync(cancellationToken);

    /// <inheritdoc/>
    public virtual async ValueTask<long> CountAsync<TResult>(Func<IQueryable<TAuthorization>, IQueryable<TResult>> query, CancellationToken cancellationToken)
    {
        if (query is null)
        {
            throw new ArgumentNullException(nameof(query));
        }

        return await query(Authorizations).LongCountAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async ValueTask CreateAsync(TAuthorization authorization, CancellationToken cancellationToken)
    {
        if (authorization is null)
        {
            throw new ArgumentNullException(nameof(authorization));
        }

        await Context.InsertAsync(authorization, token: cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async ValueTask DeleteAsync(TAuthorization authorization, CancellationToken cancellationToken)
    {
        if (authorization is null)
        {
            throw new ArgumentNullException(nameof(authorization));
        }

        if (await Authorizations.Where(entity =>
                    entity.Id!.Equals(authorization.Id) &&
                    entity.ConcurrencyToken == authorization.ConcurrencyToken)
                .DeleteAsync(cancellationToken) is 0)
        {
            throw new ConcurrencyException(SR.GetResourceString(SR.ID0241));
        }
        
        // Delete the tokens associated with the authorization.
        await Tokens.Where(token => token.AuthorizationId != null && token.AuthorizationId.Equals(authorization.Id))
            .DeleteAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public virtual IAsyncEnumerable<TAuthorization> FindAsync(
        string subject, string client, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(subject))
        {
            throw new ArgumentException(SR.GetResourceString(SR.ID0198), nameof(subject));
        }

        if (string.IsNullOrEmpty(client))
        {
            throw new ArgumentException(SR.GetResourceString(SR.ID0124), nameof(client));
        }

        var key = ConvertIdentifierFromString(client);

        var query = Authorizations.Where(auth =>
            auth.Subject == subject &&
            auth.ApplicationId != null && auth.ApplicationId.Equals(key));

        return query.AsAsyncEnumerable(cancellationToken);
    }

    /// <inheritdoc/>
    public virtual IAsyncEnumerable<TAuthorization> FindAsync(
        string subject, string client,
        string status, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(subject))
        {
            throw new ArgumentException(SR.GetResourceString(SR.ID0198), nameof(subject));
        }

        if (string.IsNullOrEmpty(client))
        {
            throw new ArgumentException(SR.GetResourceString(SR.ID0124), nameof(client));
        }

        if (string.IsNullOrEmpty(status))
        {
            throw new ArgumentException(SR.GetResourceString(SR.ID0199), nameof(status));
        }

        var key = ConvertIdentifierFromString(client);
        
        var query = Authorizations.Where(auth =>
            auth.Subject == subject &&
            auth.Status == status &&
            auth.ApplicationId != null && auth.ApplicationId.Equals(key));
        
        return query.AsAsyncEnumerable(cancellationToken);
    }

    /// <inheritdoc/>
    public virtual IAsyncEnumerable<TAuthorization> FindAsync(
        string subject, string client,
        string status, string type, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(subject))
        {
            throw new ArgumentException(SR.GetResourceString(SR.ID0198), nameof(subject));
        }

        if (string.IsNullOrEmpty(client))
        {
            throw new ArgumentException(SR.GetResourceString(SR.ID0124), nameof(client));
        }

        if (string.IsNullOrEmpty(status))
        {
            throw new ArgumentException(SR.GetResourceString(SR.ID0199), nameof(status));
        }

        if (string.IsNullOrEmpty(type))
        {
            throw new ArgumentException(SR.GetResourceString(SR.ID0200), nameof(type));
        }

        var key = ConvertIdentifierFromString(client);
        
        var query = Authorizations.Where(auth =>
            auth.Subject == subject &&
            auth.Status == status &&
            auth.Type == type &&
            auth.ApplicationId != null && auth.ApplicationId.Equals(key));
        
        return query.AsAsyncEnumerable(cancellationToken);
    }

    /// <inheritdoc/>
    public virtual IAsyncEnumerable<TAuthorization> FindAsync(
        string subject, string client,
        string status, string type,
        ImmutableArray<string> scopes, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(subject))
        {
            throw new ArgumentException(SR.GetResourceString(SR.ID0198), nameof(subject));
        }

        if (string.IsNullOrEmpty(client))
        {
            throw new ArgumentException(SR.GetResourceString(SR.ID0124), nameof(client));
        }

        if (string.IsNullOrEmpty(status))
        {
            throw new ArgumentException(SR.GetResourceString(SR.ID0199), nameof(status));
        }

        if (string.IsNullOrEmpty(type))
        {
            throw new ArgumentException(SR.GetResourceString(SR.ID0200), nameof(type));
        }
        
        var key = ConvertIdentifierFromString(client);
            
        var authorizations = Authorizations.Where(auth =>
                auth.Subject == subject &&
                auth.Status == status &&
                auth.Type == type &&
                auth.ApplicationId != null && auth.ApplicationId.Equals(key));

        return ExecuteAsync(cancellationToken);
        async IAsyncEnumerable<TAuthorization> ExecuteAsync([EnumeratorCancellation] CancellationToken cToken)
        {
            await foreach (var authorization in authorizations.AsAsyncEnumerable(cToken))
            {
                if (new HashSet<string>(await GetScopesAsync(authorization, cToken), StringComparer.Ordinal)
                    .IsSupersetOf(scopes))
                {
                    yield return authorization;
                }
            }
        }
    }

    /// <inheritdoc/>
    public virtual IAsyncEnumerable<TAuthorization> FindByApplicationIdAsync(
        string identifier, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(identifier))
        {
            throw new ArgumentException(SR.GetResourceString(SR.ID0195), nameof(identifier));
        }

        var appId = ConvertIdentifierFromString(identifier);

        return Authorizations
            .Where(auth => auth.ApplicationId != null && auth.ApplicationId.Equals(appId))
            .AsAsyncEnumerable(cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async ValueTask<TAuthorization?> FindByIdAsync(string identifier, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(identifier))
        {
            throw new ArgumentException(SR.GetResourceString(SR.ID0195), nameof(identifier));
        }

        var key = ConvertIdentifierFromString(identifier);

        return await Authorizations.FirstOrDefaultAsync(auth => auth.Id!.Equals(key), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual IAsyncEnumerable<TAuthorization> FindBySubjectAsync(
        string subject, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(subject))
        {
            throw new ArgumentException(SR.GetResourceString(SR.ID0198), nameof(subject));
        }

        return Authorizations
            .Where(auth => auth.Subject == subject)
            .AsAsyncEnumerable(cancellationToken);
    }

    /// <inheritdoc/>
    public virtual ValueTask<string?> GetApplicationIdAsync(TAuthorization authorization, CancellationToken cancellationToken)
    {
        if (authorization is null)
        {
            throw new ArgumentNullException(nameof(authorization));
        }

        return new(ConvertIdentifierToString(authorization.ApplicationId));
    }

    /// <inheritdoc/>
    public virtual async ValueTask<TResult?> GetAsync<TState, TResult>(
        Func<IQueryable<TAuthorization>, TState, IQueryable<TResult>> query,
        TState state, CancellationToken cancellationToken)
    {
        if (query is null)
        {
            throw new ArgumentNullException(nameof(query));
        }

        return await query(Authorizations, state).FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public virtual ValueTask<DateTimeOffset?> GetCreationDateAsync(TAuthorization authorization, CancellationToken cancellationToken)
    {
        if (authorization is null)
        {
            throw new ArgumentNullException(nameof(authorization));
        }

        if (authorization.CreationDate is null)
        {
            return new(result: null);
        }

        return new(DateTime.SpecifyKind(authorization.CreationDate.Value, DateTimeKind.Utc));
    }

    /// <inheritdoc/>
    public virtual ValueTask<string?> GetIdAsync(TAuthorization authorization, CancellationToken cancellationToken)
    {
        if (authorization is null)
        {
            throw new ArgumentNullException(nameof(authorization));
        }

        return new(ConvertIdentifierToString(authorization.Id));
    }

    /// <inheritdoc/>
    public virtual ValueTask<ImmutableDictionary<string, JsonElement>> GetPropertiesAsync(TAuthorization authorization, CancellationToken cancellationToken)
    {
        if (authorization is null)
        {
            throw new ArgumentNullException(nameof(authorization));
        }

        if (string.IsNullOrEmpty(authorization.Properties))
        {
            return new(ImmutableDictionary.Create<string, JsonElement>());
        }

        // Note: parsing the stringified properties is an expensive operation.
        // To mitigate that, the resulting object is stored in the memory cache.
        var key = string.Concat("68056e1a-dbcf-412b-9a6a-d791c7dbe726", "\x1e", authorization.Properties);
        var properties = Cache.GetOrCreate(key, entry =>
        {
            entry.SetPriority(CacheItemPriority.High)
                 .SetSlidingExpiration(TimeSpan.FromMinutes(1));

            using var document = JsonDocument.Parse(authorization.Properties);
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
    public virtual ValueTask<ImmutableArray<string>> GetScopesAsync(TAuthorization authorization, CancellationToken cancellationToken)
    {
        if (authorization is null)
        {
            throw new ArgumentNullException(nameof(authorization));
        }

        if (string.IsNullOrEmpty(authorization.Scopes))
        {
            return new(ImmutableArray.Create<string>());
        }

        // Note: parsing the stringified scopes is an expensive operation.
        // To mitigate that, the resulting array is stored in the memory cache.
        var key = string.Concat("2ba4ab0f-e2ec-4d48-b3bd-28e2bb660c75", "\x1e", authorization.Scopes);
        var scopes = Cache.GetOrCreate(key, entry =>
        {
            entry.SetPriority(CacheItemPriority.High)
                 .SetSlidingExpiration(TimeSpan.FromMinutes(1));

            using var document = JsonDocument.Parse(authorization.Scopes);
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

        return new(scopes);
    }

    /// <inheritdoc/>
    public virtual ValueTask<string?> GetStatusAsync(TAuthorization authorization, CancellationToken cancellationToken)
    {
        if (authorization is null)
        {
            throw new ArgumentNullException(nameof(authorization));
        }

        return new(authorization.Status);
    }

    /// <inheritdoc/>
    public virtual ValueTask<string?> GetSubjectAsync(TAuthorization authorization, CancellationToken cancellationToken)
    {
        if (authorization is null)
        {
            throw new ArgumentNullException(nameof(authorization));
        }

        return new(authorization.Subject);
    }

    /// <inheritdoc/>
    public virtual ValueTask<string?> GetTypeAsync(TAuthorization authorization, CancellationToken cancellationToken)
    {
        if (authorization is null)
        {
            throw new ArgumentNullException(nameof(authorization));
        }

        return new(authorization.Type);
    }

    /// <inheritdoc/>
    public virtual ValueTask<TAuthorization> InstantiateAsync(CancellationToken cancellationToken)
    {
        try
        {
            return new(Activator.CreateInstance<TAuthorization>());
        }

        catch (MemberAccessException exception)
        {
            return new(Task.FromException<TAuthorization>(
                new InvalidOperationException(SR.GetResourceString(SR.ID0242), exception)));
        }
    }

    /// <inheritdoc/>
    public virtual IAsyncEnumerable<TAuthorization> ListAsync(int? count, int? offset, CancellationToken cancellationToken)
    {
        IQueryable<TAuthorization> query = Authorizations.OrderBy(authorization => authorization.Id!);

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
        Func<IQueryable<TAuthorization>, TState, IQueryable<TResult>> query,
        TState state, CancellationToken cancellationToken)
    {
        if (query is null)
        {
            throw new ArgumentNullException(nameof(query));
        }

        return query(Authorizations, state).AsAsyncEnumerable(cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async ValueTask PruneAsync(DateTimeOffset threshold, CancellationToken cancellationToken)
    {
        // TODO test for multiple tokens per authorization. If that is possible
        await Authorizations
            .SelectMany(auth => Tokens.LeftJoin(token => auth.Id!.Equals(token.AuthorizationId)),
                (auth, token) => new {auth, token})
            .Where(i => i.auth.CreationDate < threshold.UtcDateTime)
            .Where(i => i.auth.Status != Statuses.Valid ||
                        (i.auth.Type == AuthorizationTypes.AdHoc && i.token.Id == null))
            .Select(i => i.auth)
            .DeleteAsync(token: cancellationToken);
    }

    /// <inheritdoc/>
    public virtual ValueTask SetApplicationIdAsync(TAuthorization authorization,
        string? identifier, CancellationToken cancellationToken)
    {
        if (authorization is null)
        {
            throw new ArgumentNullException(nameof(authorization));
        }

        if (!string.IsNullOrEmpty(identifier))
        {
            authorization.ApplicationId = ConvertIdentifierFromString(identifier);
        }
        else
        {
            authorization.ApplicationId = default;
        }

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetCreationDateAsync(TAuthorization authorization,
        DateTimeOffset? date, CancellationToken cancellationToken)
    {
        if (authorization is null)
        {
            throw new ArgumentNullException(nameof(authorization));
        }

        authorization.CreationDate = date?.UtcDateTime;

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetPropertiesAsync(TAuthorization authorization,
        ImmutableDictionary<string, JsonElement> properties, CancellationToken cancellationToken)
    {
        if (authorization is null)
        {
            throw new ArgumentNullException(nameof(authorization));
        }

        if (properties is not { Count: > 0 })
        {
            authorization.Properties = null;

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

        authorization.Properties = Encoding.UTF8.GetString(stream.ToArray());

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetScopesAsync(TAuthorization authorization,
        ImmutableArray<string> scopes, CancellationToken cancellationToken)
    {
        if (authorization is null)
        {
            throw new ArgumentNullException(nameof(authorization));
        }

        if (scopes.IsDefaultOrEmpty)
        {
            authorization.Scopes = null;

            return default;
        }

        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Indented = false
        });

        writer.WriteStartArray();

        foreach (var scope in scopes)
        {
            writer.WriteStringValue(scope);
        }

        writer.WriteEndArray();
        writer.Flush();

        authorization.Scopes = Encoding.UTF8.GetString(stream.ToArray());

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetStatusAsync(TAuthorization authorization,
        string? status, CancellationToken cancellationToken)
    {
        if (authorization is null)
        {
            throw new ArgumentNullException(nameof(authorization));
        }

        authorization.Status = status;

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetSubjectAsync(TAuthorization authorization,
        string? subject, CancellationToken cancellationToken)
    {
        if (authorization is null)
        {
            throw new ArgumentNullException(nameof(authorization));
        }

        authorization.Subject = subject;

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetTypeAsync(TAuthorization authorization,
        string? type, CancellationToken cancellationToken)
    {
        if (authorization is null)
        {
            throw new ArgumentNullException(nameof(authorization));
        }

        authorization.Type = type;

        return default;
    }

    /// <inheritdoc/>
    public virtual async ValueTask UpdateAsync(TAuthorization authorization, CancellationToken cancellationToken)
    {
        if (authorization is null)
        {
            throw new ArgumentNullException(nameof(authorization));
        }

        // Do manual concurrency check before updating entity in db because
        // LinqToDB can not do "where" check when updating entire entity.
        var concurrencyChecked =
            await Tokens.AnyAsync(i => i.Id!.Equals(authorization.Id) && i.ConcurrencyToken == authorization.ConcurrencyToken,
                token: cancellationToken);
        
        if (!concurrencyChecked)
        {
            throw new ConcurrencyException(SR.GetResourceString(SR.ID0241));
        }

        // Generate a new concurrency token and attach it
        // to the token before persisting the changes.
        authorization.ConcurrencyToken = Guid.NewGuid().ToString();

        await Context.UpdateAsync(authorization, token: cancellationToken);
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
