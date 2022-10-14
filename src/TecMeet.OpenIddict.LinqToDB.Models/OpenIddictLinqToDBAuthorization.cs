/*
 * Licensed under the Apache License, Version 2.0 (http://www.apache.org/licenses/LICENSE-2.0)
 * See https://github.com/openiddict/openiddict-core for more information concerning
 * the license and the contributors participating to this project.
 */

using System.Diagnostics;
using NodaTime;

namespace TecMeet.OpenIddict.LinqToDB.Models;

/// <summary>
/// Represents an OpenIddict authorization.
/// </summary>
public class OpenIddictLinqToDBAuthorization : OpenIddictLinqToDBAuthorization<string> {}

/// <summary>
/// Represents an OpenIddict authorization.
/// </summary>
[DebuggerDisplay("Id = {Id.ToString(),nq} ; Subject = {Subject,nq} ; Type = {Type,nq} ; Status = {Status,nq}")]
public class OpenIddictLinqToDBAuthorization<TKey>
    where TKey : notnull, IEquatable<TKey>
{
    /// <summary>
    /// Gets or sets the application associated with the current authorization.
    /// </summary>
    public virtual TKey? ApplicationId { get; set; }

    /// <summary>
    /// Gets or sets the concurrency token.
    /// </summary>
    public virtual string? ConcurrencyToken { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the UTC creation date of the current authorization.
    /// </summary>
    public virtual Instant? CreationDate { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier associated with the current authorization.
    /// </summary>
    public virtual TKey? Id { get; set; }

    /// <summary>
    /// Gets or sets the additional properties serialized as a JSON object,
    /// or <see langword="null"/> if no bag was associated with the current authorization.
    /// </summary>
    public virtual string? Properties { get; set; }

    /// <summary>
    /// Gets or sets the scopes associated with the current
    /// authorization, serialized as a JSON array.
    /// </summary>
    public virtual string? Scopes { get; set; }

    /// <summary>
    /// Gets or sets the status of the current authorization.
    /// </summary>
    public virtual string? Status { get; set; }

    /// <summary>
    /// Gets or sets the subject associated with the current authorization.
    /// </summary>
    public virtual string? Subject { get; set; }

    /// <summary>
    /// Gets or sets the type of the current authorization.
    /// </summary>
    public virtual string? Type { get; set; }
}
