/*
 * Licensed under the Apache License, Version 2.0 (http://www.apache.org/licenses/LICENSE-2.0)
 * See https://github.com/openiddict/openiddict-core for more information concerning
 * the license and the contributors participating to this project.
 */

using System.Diagnostics;
using NodaTime;

namespace TecMeet.OpenIddict.LinqToDB.Models;

/// <summary>
/// Represents an OpenIddict token.
/// </summary>
public class OpenIddictLinqToDBToken : OpenIddictLinqToDBToken<string> {}

/// <summary>
/// Represents an OpenIddict token.
/// </summary>
[DebuggerDisplay("Id = {Id.ToString(),nq} ; Subject = {Subject,nq} ; Type = {Type,nq} ; Status = {Status,nq}")]
public class OpenIddictLinqToDBToken<TKey>
    where TKey : notnull, IEquatable<TKey>
{
    /// <summary>
    /// Gets or sets the application associated with the current token.
    /// </summary>
    public virtual TKey? ApplicationId { get; set; }

    /// <summary>
    /// Gets or sets the authorization associated with the current token.
    /// </summary>
    public virtual TKey? AuthorizationId { get; set; }

    /// <summary>
    /// Gets or sets the concurrency token.
    /// </summary>
    public virtual string? ConcurrencyToken { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the UTC creation date of the current token.
    /// </summary>
    public virtual Instant? CreationDate { get; set; }

    /// <summary>
    /// Gets or sets the UTC expiration date of the current token.
    /// </summary>
    public virtual Instant? ExpirationDate { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier associated with the current token.
    /// </summary>
    public virtual TKey? Id { get; set; }

    /// <summary>
    /// Gets or sets the payload of the current token, if applicable.
    /// Note: this property is only used for reference tokens
    /// and may be encrypted for security reasons.
    /// </summary>
    public virtual string? Payload { get; set; }

    /// <summary>
    /// Gets or sets the additional properties serialized as a JSON object,
    /// or <see langword="null"/> if no bag was associated with the current token.
    /// </summary>
    public virtual string? Properties { get; set; }

    /// <summary>
    /// Gets or sets the UTC redemption date of the current token.
    /// </summary>
    public virtual Instant? RedemptionDate { get; set; }

    /// <summary>
    /// Gets or sets the reference identifier associated
    /// with the current token, if applicable.
    /// Note: this property is only used for reference tokens
    /// and may be hashed or encrypted for security reasons.
    /// </summary>
    public virtual string? ReferenceId { get; set; }

    /// <summary>
    /// Gets or sets the status of the current token.
    /// </summary>
    public virtual string? Status { get; set; }

    /// <summary>
    /// Gets or sets the subject associated with the current token.
    /// </summary>
    public virtual string? Subject { get; set; }

    /// <summary>
    /// Gets or sets the type of the current token.
    /// </summary>
    public virtual string? Type { get; set; }
}
