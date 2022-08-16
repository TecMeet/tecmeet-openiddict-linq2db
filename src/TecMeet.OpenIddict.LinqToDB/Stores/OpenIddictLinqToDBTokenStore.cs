/*
 * Licensed under the Apache License, Version 2.0 (http://www.apache.org/licenses/LICENSE-2.0)
 * See https://github.com/openiddict/openiddict-core for more information concerning
 * the license and the contributors participating to this project.
 */

using System.Collections.Immutable;
using System.ComponentModel;
using System.Data;
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
/// Provides methods allowing to manage the tokens stored in a database.
/// </summary>
/// <typeparam name="TContext">The type of the LinqToDB database context.</typeparam>
public class OpenIddictLinqToDBTokenStore<TContext> :
    OpenIddictLinqToDBTokenStore<OpenIddictLinqToDBToken,
                                            OpenIddictLinqToDBApplication,
                                            OpenIddictLinqToDBAuthorization, TContext, string>
    where TContext : IDataContext
{
    public OpenIddictLinqToDBTokenStore(
        IMemoryCache cache,
        TContext context,
        IOptionsMonitor<OpenIddictLinqToDBOptions> options)
        : base(cache, context, options)
    {
    }
}

/// <summary>
/// Provides methods allowing to manage the tokens stored in a database.
/// </summary>
/// <typeparam name="TContext">The type of the LinqToDB database context.</typeparam>
/// <typeparam name="TKey">The type of the entity primary keys.</typeparam>
public class OpenIddictLinqToDBTokenStore<TContext, TKey> :
    OpenIddictLinqToDBTokenStore<OpenIddictLinqToDBToken<TKey>,
                                            OpenIddictLinqToDBApplication<TKey>,
                                            OpenIddictLinqToDBAuthorization<TKey>, TContext, TKey>
    where TContext : IDataContext
    where TKey : notnull, IEquatable<TKey>
{
    public OpenIddictLinqToDBTokenStore(
        IMemoryCache cache,
        TContext context,
        IOptionsMonitor<OpenIddictLinqToDBOptions> options)
        : base(cache, context, options)
    {
    }
}

/// <summary>
/// Provides methods allowing to manage the tokens stored in a database.
/// </summary>
/// <typeparam name="TToken">The type of the Token entity.</typeparam>
/// <typeparam name="TApplication">The type of the Application entity.</typeparam>
/// <typeparam name="TAuthorization">The type of the Authorization entity.</typeparam>
/// <typeparam name="TContext">The type of the LinqToDB database context.</typeparam>
/// <typeparam name="TKey">The type of the entity primary keys.</typeparam>
public class OpenIddictLinqToDBTokenStore<TToken, TApplication, TAuthorization, TContext, TKey> : IOpenIddictTokenStore<TToken>
    where TToken : OpenIddictLinqToDBToken<TKey>
    where TApplication : OpenIddictLinqToDBApplication<TKey>
    where TAuthorization : OpenIddictLinqToDBAuthorization<TKey>
    where TContext : IDataContext
    where TKey : notnull, IEquatable<TKey>
{
    public OpenIddictLinqToDBTokenStore(
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
        => await Tokens.AsQueryable().LongCountAsync(cancellationToken);

    /// <inheritdoc/>
    public virtual async ValueTask<long> CountAsync<TResult>(Func<IQueryable<TToken>, IQueryable<TResult>> query, CancellationToken cancellationToken)
    {
        if (query is null)
        {
            throw new ArgumentNullException(nameof(query));
        }

        return await query(Tokens).LongCountAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async ValueTask CreateAsync(TToken token, CancellationToken cancellationToken)
    {
        if (token is null)
        {
            throw new ArgumentNullException(nameof(token));
        }

        await Context.InsertAsync(token, token: cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async ValueTask DeleteAsync(TToken token, CancellationToken cancellationToken)
    {
        if (token is null)
        {
            throw new ArgumentNullException(nameof(token));
        }
        
        if (await Tokens.Where(entity =>
                entity.Id!.Equals(token.Id) &&
                entity.ConcurrencyToken == token.ConcurrencyToken)
            .DeleteAsync(cancellationToken) is 0)
        {
            throw new ConcurrencyException(SR.GetResourceString(SR.ID0247));
        }
    }

    /// <inheritdoc/>
    public virtual IAsyncEnumerable<TToken> FindAsync(string subject, string client, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(subject))
        {
            throw new ArgumentException(SR.GetResourceString(SR.ID0198), nameof(subject));
        }

        if (string.IsNullOrEmpty(client))
        {
            throw new ArgumentException(SR.GetResourceString(SR.ID0124), nameof(client));
        }

        var appId = ConvertIdentifierFromString(client);

        return ExecuteAsync();
        // need inline method so that cancellationToken can be used
        async IAsyncEnumerable<TToken> ExecuteAsync()
        {
            await foreach (var token in Tokens
                               .Where(token => token.ApplicationId!.Equals(appId) && token.Subject == subject)
                               .AsAsyncEnumerable().WithCancellation(cancellationToken))
            {
                yield return token;
            }
        }
    }

    /// <inheritdoc/>
    public virtual IAsyncEnumerable<TToken> FindAsync(
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

        var appId = ConvertIdentifierFromString(client);

        return ExecuteAsync();
        // need inline method so that cancellationToken can be used
        async IAsyncEnumerable<TToken> ExecuteAsync()
        {
            await foreach (var token in Tokens
                               .Where(token => token.ApplicationId!.Equals(appId) && 
                                               token.Subject == subject &&
                                               token.Status == status)
                               .AsAsyncEnumerable().WithCancellation(cancellationToken))
            {
                yield return token;
            }
        }
    }

    /// <inheritdoc/>
    public virtual IAsyncEnumerable<TToken> FindAsync(
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

        var appId = ConvertIdentifierFromString(client);

        return ExecuteAsync();
        // need inline method so that cancellationToken can be used
        async IAsyncEnumerable<TToken> ExecuteAsync()
        {
            await foreach (var token in Tokens
                               .Where(token => token.ApplicationId!.Equals(appId) && 
                                               token.Subject == subject &&
                                               token.Status == status &&
                                               token.Type == type)
                               .AsAsyncEnumerable().WithCancellation(cancellationToken))
            {
                yield return token;
            }
        }
    }

    /// <inheritdoc/>
    public virtual IAsyncEnumerable<TToken> FindByApplicationIdAsync(string identifier, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(identifier))
        {
            throw new ArgumentException(SR.GetResourceString(SR.ID0195), nameof(identifier));
        }

        var appId = ConvertIdentifierFromString(identifier);

        return ExecuteAsync();
        // need inline method so that cancellationToken can be used
        async IAsyncEnumerable<TToken> ExecuteAsync()
        {
            await foreach (var token in Tokens
                               .Where(token => token.ApplicationId!.Equals(appId))
                               .AsAsyncEnumerable().WithCancellation(cancellationToken))
            {
                yield return token;
            }
        }
    }

    /// <inheritdoc/>
    public virtual IAsyncEnumerable<TToken> FindByAuthorizationIdAsync(string identifier, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(identifier))
        {
            throw new ArgumentException(SR.GetResourceString(SR.ID0195), nameof(identifier));
        }

        var authId = ConvertIdentifierFromString(identifier);

        return ExecuteAsync();
        // need inline method so that cancellationToken can be used
        async IAsyncEnumerable<TToken> ExecuteAsync()
        {
            await foreach (var token in Tokens
                               .Where(token => token.AuthorizationId!.Equals(authId))
                               .AsAsyncEnumerable().WithCancellation(cancellationToken))
            {
                yield return token;
            }
        }
    }

    /// <inheritdoc/>
    public virtual async ValueTask<TToken?> FindByIdAsync(string identifier, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(identifier))
        {
            throw new ArgumentException(SR.GetResourceString(SR.ID0195), nameof(identifier));
        }

        var tokenId = ConvertIdentifierFromString(identifier);
        return await Tokens.FirstOrDefaultAsync(token => token.Id!.Equals(tokenId), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async ValueTask<TToken?> FindByReferenceIdAsync(string identifier, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(identifier))
        {
            throw new ArgumentException(SR.GetResourceString(SR.ID0195), nameof(identifier));
        }

        return await Tokens.FirstOrDefaultAsync(token => token.ReferenceId == identifier, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual IAsyncEnumerable<TToken> FindBySubjectAsync(string subject, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(subject))
        {
            throw new ArgumentException(SR.GetResourceString(SR.ID0198), nameof(subject));
        }

        return ExecuteAsync();
        // need inline method so that cancellationToken can be used
        async IAsyncEnumerable<TToken> ExecuteAsync()
        {
            await foreach (var token in Tokens
                               .Where(token => token.Subject == subject)
                               .AsAsyncEnumerable().WithCancellation(cancellationToken))
            {
                yield return token;
            }
        }
    }

    /// <inheritdoc/>
    public virtual ValueTask<string?> GetApplicationIdAsync(TToken token, CancellationToken cancellationToken)
    {
        if (token is null)
        {
            throw new ArgumentNullException(nameof(token));
        }

        return new(ConvertIdentifierToString(token.ApplicationId));
    }

    /// <inheritdoc/>
    public virtual async ValueTask<TResult?> GetAsync<TState, TResult>(
        Func<IQueryable<TToken>, TState, IQueryable<TResult>> query,
        TState state, CancellationToken cancellationToken)
    {
        if (query is null)
        {
            throw new ArgumentNullException(nameof(query));
        }

        return await query(Tokens, state).FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public virtual ValueTask<string?> GetAuthorizationIdAsync(TToken token, CancellationToken cancellationToken)
    {
        if (token is null)
        {
            throw new ArgumentNullException(nameof(token));
        }

        return new(ConvertIdentifierToString(token.AuthorizationId));
    }

    /// <inheritdoc/>
    public virtual ValueTask<DateTimeOffset?> GetCreationDateAsync(TToken token, CancellationToken cancellationToken)
    {
        if (token is null)
        {
            throw new ArgumentNullException(nameof(token));
        }

        if (token.CreationDate is null)
        {
            return new(result: null);
        }

        return new(DateTime.SpecifyKind(token.CreationDate.Value, DateTimeKind.Utc));
    }

    /// <inheritdoc/>
    public virtual ValueTask<DateTimeOffset?> GetExpirationDateAsync(TToken token, CancellationToken cancellationToken)
    {
        if (token is null)
        {
            throw new ArgumentNullException(nameof(token));
        }

        if (token.ExpirationDate is null)
        {
            return new(result: null);
        }

        return new(DateTime.SpecifyKind(token.ExpirationDate.Value, DateTimeKind.Utc));
    }

    /// <inheritdoc/>
    public virtual ValueTask<string?> GetIdAsync(TToken token, CancellationToken cancellationToken)
    {
        if (token is null)
        {
            throw new ArgumentNullException(nameof(token));
        }

        return new(ConvertIdentifierToString(token.Id));
    }

    /// <inheritdoc/>
    public virtual ValueTask<string?> GetPayloadAsync(TToken token, CancellationToken cancellationToken)
    {
        if (token is null)
        {
            throw new ArgumentNullException(nameof(token));
        }

        return new(token.Payload);
    }

    /// <inheritdoc/>
    public virtual ValueTask<ImmutableDictionary<string, JsonElement>> GetPropertiesAsync(TToken token, CancellationToken cancellationToken)
    {
        if (token is null)
        {
            throw new ArgumentNullException(nameof(token));
        }

        if (string.IsNullOrEmpty(token.Properties))
        {
            return new(ImmutableDictionary.Create<string, JsonElement>());
        }

        // Note: parsing the stringified properties is an expensive operation.
        // To mitigate that, the resulting object is stored in the memory cache.
        var key = string.Concat("d0509397-1bbf-40e7-97e1-5e6d7bc2536c", "\x1e", token.Properties);
        var properties = Cache.GetOrCreate(key, entry =>
        {
            entry.SetPriority(CacheItemPriority.High)
                 .SetSlidingExpiration(TimeSpan.FromMinutes(1));

            using var document = JsonDocument.Parse(token.Properties);
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
    public virtual ValueTask<DateTimeOffset?> GetRedemptionDateAsync(TToken token, CancellationToken cancellationToken)
    {
        if (token is null)
        {
            throw new ArgumentNullException(nameof(token));
        }

        if (token.RedemptionDate is null)
        {
            return new(result: null);
        }

        return new(DateTime.SpecifyKind(token.RedemptionDate.Value, DateTimeKind.Utc));
    }

    /// <inheritdoc/>
    public virtual ValueTask<string?> GetReferenceIdAsync(TToken token, CancellationToken cancellationToken)
    {
        if (token is null)
        {
            throw new ArgumentNullException(nameof(token));
        }

        return new(token.ReferenceId);
    }

    /// <inheritdoc/>
    public virtual ValueTask<string?> GetStatusAsync(TToken token, CancellationToken cancellationToken)
    {
        if (token is null)
        {
            throw new ArgumentNullException(nameof(token));
        }

        return new(token.Status);
    }

    /// <inheritdoc/>
    public virtual ValueTask<string?> GetSubjectAsync(TToken token, CancellationToken cancellationToken)
    {
        if (token is null)
        {
            throw new ArgumentNullException(nameof(token));
        }

        return new(token.Subject);
    }

    /// <inheritdoc/>
    public virtual ValueTask<string?> GetTypeAsync(TToken token, CancellationToken cancellationToken)
    {
        if (token is null)
        {
            throw new ArgumentNullException(nameof(token));
        }

        return new(token.Type);
    }

    /// <inheritdoc/>
    public virtual ValueTask<TToken> InstantiateAsync(CancellationToken cancellationToken)
    {
        try
        {
            return new(Activator.CreateInstance<TToken>());
        }

        catch (MemberAccessException exception)
        {
            return new(Task.FromException<TToken>(
                new InvalidOperationException(SR.GetResourceString(SR.ID0248), exception)));
        }
    }

    /// <inheritdoc/>
    public virtual IAsyncEnumerable<TToken> ListAsync(int? count, int? offset, CancellationToken cancellationToken)
    {
        IQueryable<TToken> query = Tokens.OrderBy(token => token.Id);

        if (offset.HasValue)
        {
            query = query.Skip(offset.Value);
        }

        if (count.HasValue)
        {
            query = query.Take(count.Value);
        }

        return ExecuteAsync();
        async IAsyncEnumerable<TToken> ExecuteAsync()
        {
            await foreach (var token in query.AsAsyncEnumerable().WithCancellation(cancellationToken))
            {
                yield return token;
            }
        }
    }

    /// <inheritdoc/>
    public virtual IAsyncEnumerable<TResult> ListAsync<TState, TResult>(
        Func<IQueryable<TToken>, TState, IQueryable<TResult>> query,
        TState state, CancellationToken cancellationToken)
    {
        if (query is null)
        {
            throw new ArgumentNullException(nameof(query));
        }

        return ExecuteAsync();
        async IAsyncEnumerable<TResult> ExecuteAsync()
        {
            await foreach (var token in query(Tokens, state).AsAsyncEnumerable().WithCancellation(cancellationToken))
            {
                yield return token;
            }
        }
    }

    /// <inheritdoc/>
    public virtual async ValueTask PruneAsync(DateTimeOffset threshold, CancellationToken cancellationToken)
    {
        // Note: the Oracle MySQL provider doesn't support DateTimeOffset and is unable
        // to create a SQL query with an expression calling DateTimeOffset.UtcDateTime.
        // To work around this limitation, the threshold represented as a DateTimeOffset
        // instance is manually converted to a UTC DateTime instance outside the query.
        var date = threshold.UtcDateTime;

        await Tokens
            .SelectMany(token => Applications.LeftJoin(app => token.ApplicationId!.Equals(app.Id)),
                (token, app) => new {token, app})
            .SelectMany(i => Authorizations.LeftJoin(auth => i.token.AuthorizationId!.Equals(auth.Id)),
                (i, auth) => new {i.token, i.app, auth})
            .Where(i => i.token.CreationDate < date)
            .Where(i => (i.token.Status != Statuses.Inactive && i.token.Status != Statuses.Valid) ||
                        (i.auth.Id != null && i.auth.Status != Statuses.Valid) ||
                        i.token.ExpirationDate < DateTime.UtcNow
            )
            .OrderBy(i => i.token.Id)
            .Take(1_000)
            .Select(i => i.token)
            .DeleteAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public virtual ValueTask SetApplicationIdAsync(TToken token, string? identifier, CancellationToken cancellationToken)
    {
        if (token is null)
        {
            throw new ArgumentNullException(nameof(token));
        }

        if (!string.IsNullOrEmpty(identifier))
        {
            token.ApplicationId = ConvertIdentifierFromString(identifier);
        }
        else
        {
            token.ApplicationId = default;
        }

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetAuthorizationIdAsync(TToken token, string? identifier, CancellationToken cancellationToken)
    {
        if (token is null)
        {
            throw new ArgumentNullException(nameof(token));
        }
        
        if (!string.IsNullOrEmpty(identifier))
        {
            token.AuthorizationId = ConvertIdentifierFromString(identifier);
        }

        else
        {
            token.AuthorizationId = default;
        }

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetCreationDateAsync(TToken token, DateTimeOffset? date, CancellationToken cancellationToken)
    {
        if (token is null)
        {
            throw new ArgumentNullException(nameof(token));
        }

        token.CreationDate = date?.UtcDateTime;

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetExpirationDateAsync(TToken token, DateTimeOffset? date, CancellationToken cancellationToken)
    {
        if (token is null)
        {
            throw new ArgumentNullException(nameof(token));
        }

        token.ExpirationDate = date?.UtcDateTime;

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetPayloadAsync(TToken token, string? payload, CancellationToken cancellationToken)
    {
        if (token is null)
        {
            throw new ArgumentNullException(nameof(token));
        }

        token.Payload = payload;

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetPropertiesAsync(TToken token,
        ImmutableDictionary<string, JsonElement> properties, CancellationToken cancellationToken)
    {
        if (token is null)
        {
            throw new ArgumentNullException(nameof(token));
        }

        if (properties is not { Count: > 0 })
        {
            token.Properties = null;

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

        token.Properties = Encoding.UTF8.GetString(stream.ToArray());

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetRedemptionDateAsync(TToken token, DateTimeOffset? date, CancellationToken cancellationToken)
    {
        if (token is null)
        {
            throw new ArgumentNullException(nameof(token));
        }

        token.RedemptionDate = date?.UtcDateTime;

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetReferenceIdAsync(TToken token, string? identifier, CancellationToken cancellationToken)
    {
        if (token is null)
        {
            throw new ArgumentNullException(nameof(token));
        }

        token.ReferenceId = identifier;

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetStatusAsync(TToken token, string? status, CancellationToken cancellationToken)
    {
        if (token is null)
        {
            throw new ArgumentNullException(nameof(token));
        }

        token.Status = status;

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetSubjectAsync(TToken token, string? subject, CancellationToken cancellationToken)
    {
        if (token is null)
        {
            throw new ArgumentNullException(nameof(token));
        }

        token.Subject = subject;

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetTypeAsync(TToken token, string? type, CancellationToken cancellationToken)
    {
        if (token is null)
        {
            throw new ArgumentNullException(nameof(token));
        }

        token.Type = type;

        return default;
    }

    /// <inheritdoc/>
    public virtual async ValueTask UpdateAsync(TToken token, CancellationToken cancellationToken)
    {
        if (token is null)
        {
            throw new ArgumentNullException(nameof(token));
        }

        // Do manual concurrency check before updating entity in db because
        // LinqToDB can not do "where" check when updating entire entity.
        var concurrencyChecked =
            await Tokens.AnyAsync(i => i.Id!.Equals(token.Id) && i.ConcurrencyToken == token.ConcurrencyToken,
                token: cancellationToken);
        
        if (!concurrencyChecked)
        {
            throw new ConcurrencyException(SR.GetResourceString(SR.ID0247));
        }

        // Generate a new concurrency token and attach it
        // to the token before persisting the changes.
        token.ConcurrencyToken = Guid.NewGuid().ToString();

        await Context.UpdateAsync(token, token: cancellationToken);
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
