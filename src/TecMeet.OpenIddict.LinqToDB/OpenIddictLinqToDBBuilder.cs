/*
 * Licensed under the Apache License, Version 2.0 (http://www.apache.org/licenses/LICENSE-2.0)
 * See https://github.com/openiddict/openiddict-core for more information concerning
 * the license and the contributors participating to this project.
 */

using System.ComponentModel;
using OpenIddict.Core;
using TecMeet.OpenIddict.LinqToDB.Models;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Exposes the necessary methods required to configure the OpenIddict LinqToDB services.
/// </summary>
public class OpenIddictLinqToDBBuilder
{
    /// <summary>
    /// Initializes a new instance of <see cref="OpenIddictLinqToDBBuilder"/>.
    /// </summary>
    /// <param name="services">The services collection.</param>
    public OpenIddictLinqToDBBuilder(IServiceCollection services)
        => Services = services ?? throw new ArgumentNullException(nameof(services));

    /// <summary>
    /// Gets the services collection.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public IServiceCollection Services { get; }

    /// <summary>
    /// Configures OpenIddict to use the default OpenIddict
    /// LinqToDB entities, with the specified key type.
    /// </summary>
    /// <returns>The <see cref="OpenIddictLinqToDBBuilder"/>.</returns>
    public OpenIddictLinqToDBBuilder ReplaceDefaultEntities<TKey>()
        where TKey : notnull, IEquatable<TKey>
        => ReplaceDefaultEntities<OpenIddictLinqToDBApplication<TKey>,
                                  OpenIddictLinqToDBAuthorization<TKey>,
                                  OpenIddictLinqToDBScope<TKey>,
                                  OpenIddictLinqToDBToken<TKey>, TKey>();

    /// <summary>
    /// Configures OpenIddict to use the specified entities, derived
    /// from the default OpenIddict LinqToDB entities.
    /// </summary>
    /// <returns>The <see cref="OpenIddictLinqToDBBuilder"/>.</returns>
    public OpenIddictLinqToDBBuilder ReplaceDefaultEntities<TApplication, TAuthorization, TScope, TToken, TKey>()
        where TApplication : OpenIddictLinqToDBApplication<TKey>
        where TAuthorization : OpenIddictLinqToDBAuthorization<TKey>
        where TScope : OpenIddictLinqToDBScope<TKey>
        where TToken : OpenIddictLinqToDBToken<TKey>
        where TKey : notnull, IEquatable<TKey>
    {
        Services.Configure<OpenIddictCoreOptions>(options =>
        {
            options.DefaultApplicationType = typeof(TApplication);
            options.DefaultAuthorizationType = typeof(TAuthorization);
            options.DefaultScopeType = typeof(TScope);
            options.DefaultTokenType = typeof(TToken);
        });

        return this;
    }

    /// <inheritdoc/>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override bool Equals(object? obj) => base.Equals(obj);

    /// <inheritdoc/>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override int GetHashCode() => base.GetHashCode();

    /// <inheritdoc/>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override string? ToString() => base.ToString();
}
