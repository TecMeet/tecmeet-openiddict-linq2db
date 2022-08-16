/*
 * Licensed under the Apache License, Version 2.0 (http://www.apache.org/licenses/LICENSE-2.0)
 * See https://github.com/openiddict/openiddict-core for more information concerning
 * the license and the contributors participating to this project.
 */

namespace OpenIddict.LinqToDB;

public class OpenIddictLinqToDBNameOptions
{
    /// <summary>
    /// Gets or sets the name of the applications table (by default, auth_openiddict_applications).
    /// </summary>
    public string ApplicationsTableName { get; set; } = "auth_openiddict_applications";

    /// <summary>
    /// Gets or sets the name of the authorizations table (by default, auth_openiddict_authorizations).
    /// </summary>
    public string AuthorizationsTableName { get; set; } = "auth_openiddict_authorizations";

    /// <summary>
    /// Gets or sets the name of the scopes table (by default, auth_openiddict_scopes).
    /// </summary>
    public string ScopesTableName { get; set; } = "auth_openiddict_scopes";

    /// <summary>
    /// Gets or sets the name of the tokens table (by default, auth_openiddict_tokens).
    /// </summary>
    public string TokensTableName { get; set; } = "auth_openiddict_tokens";
}