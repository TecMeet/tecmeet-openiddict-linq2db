/*
 * Licensed under the Apache License, Version 2.0 (http://www.apache.org/licenses/LICENSE-2.0)
 * See https://github.com/openiddict/openiddict-core for more information concerning
 * the license and the contributors participating to this project.
 */

using System.Diagnostics;

namespace TecMeet.OpenIddict.LinqToDB.Models;

/// <summary>
/// Represents an OpenIddict application.
/// </summary>
public class OpenIddictLinqToDBApplication : OpenIddictLinqToDBApplication<Guid> {}

/// <summary>
/// Represents an OpenIddict application.
/// </summary>
[DebuggerDisplay("Id = {Id.ToString(),nq} ; ClientId = {ClientId,nq} ; Type = {Type,nq}")]
public class OpenIddictLinqToDBApplication<TKey>
    where TKey : notnull, IEquatable<TKey>
{
    /// <summary>
    /// Gets or sets the application type associated with the current application.
    /// </summary>
    public virtual string? ApplicationType { get; set; }

    /// <summary>
    /// Gets or sets the client identifier associated with the current application.
    /// </summary>
    public virtual string? ClientId { get; set; }

    /// <summary>
    /// Gets or sets the client secret associated with the current application.
    /// Note: depending on the application manager used to create this instance,
    /// this property may be hashed or encrypted for security reasons.
    /// </summary>
    public virtual string? ClientSecret { get; set; }

    /// <summary>
    /// Gets or sets the concurrency token.
    /// </summary>
    public virtual string? ConcurrencyToken { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the consent type associated with the current application.
    /// </summary>
    public virtual string? ConsentType { get; set; }

    /// <summary>
    /// Gets or sets the display name associated with the current application.
    /// </summary>
    public virtual string? DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the localized display names
    /// associated with the current application,
    /// serialized as a JSON object.
    /// </summary>
    public virtual string? DisplayNames { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier associated with the current application.
    /// </summary>
    public virtual TKey? Id { get; set; }

    /// <summary>
    /// Gets or sets the JSON Web Key Set associated with
    /// the application, serialized as a JSON object.
    /// </summary>
    public virtual string? JsonWebKeySet { get; set; }

    /// <summary>
    /// Gets or sets the permissions associated with the
    /// current application, serialized as a JSON array.
    /// </summary>
    public virtual string? Permissions { get; set; }

    /// <summary>
    /// Gets or sets the logout callback URLs associated with
    /// the current application, serialized as a JSON array.
    /// </summary>
    public virtual string? PostLogoutRedirectUris { get; set; }

    /// <summary>
    /// Gets or sets the additional properties serialized as a JSON object,
    /// or <see langword="null"/> if no bag was associated with the current application.
    /// </summary>
    public virtual string? Properties { get; set; }

    /// <summary>
    /// Gets or sets the callback URLs associated with the
    /// current application, serialized as a JSON array.
    /// </summary>
    public virtual string? RedirectUris { get; set; }

    /// <summary>
    /// Gets or sets the requirements associated with the
    /// current application, serialized as a JSON array.
    /// </summary>
    public virtual string? Requirements { get; set; }

    /// <summary>
    /// Gets or sets the application type associated with the current application.
    /// </summary>
    public virtual string? ClientType { get; set; }

    /// <summary>
    /// Gets or sets the settings serialized as a JSON object.
    /// </summary>
    public virtual string? Settings { get; set; }
}
