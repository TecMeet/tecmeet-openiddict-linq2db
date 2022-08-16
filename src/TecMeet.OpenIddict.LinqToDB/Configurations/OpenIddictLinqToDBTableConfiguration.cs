/*
 * Licensed under the Apache License, Version 2.0 (http://www.apache.org/licenses/LICENSE-2.0)
 * See https://github.com/openiddict/openiddict-core for more information concerning
 * the license and the contributors participating to this project.
 */

using System.ComponentModel;
using LinqToDB.Mapping;
using TecMeet.OpenIddict.LinqToDB.Models;

namespace TecMeet.OpenIddict.LinqToDB;

/// <summary>
/// Defines relational mappings for the database entities.
/// </summary>
internal static class OpenIddictLinqToDBTableConfiguration
{
    /// <summary>
    /// Set up the LinqToDB schema mapping for the Application table
    /// </summary>
    /// <param name="builder">The LinqToDB fluent mapping builder</param>
    /// <param name="applicationTableName">The name for the table in the database</param>
    /// <typeparam name="TApplication">The Application entity type to map</typeparam>
    /// <typeparam name="TKey">The primary key type for the Openiddict database tables</typeparam>
    /// <returns>LinqToDB fluent mapping builder</returns>
    internal static FluentMappingBuilder ConfigureApplicationMapping<TKey, TApplication>(this FluentMappingBuilder builder,
           string applicationTableName)
        where TKey : notnull, IEquatable<TKey> 
        where TApplication : OpenIddictLinqToDBApplication<TKey>
    {
        if (builder is null) throw new ArgumentNullException(nameof(builder));
        if (applicationTableName == null) throw new ArgumentNullException(nameof(applicationTableName));

        // only set table info and primary key, other regular properties are inferred by LinqToDB
        builder.Entity<TApplication>()
            .HasTableName(applicationTableName)
            .Property(i => i.Id).IsPrimaryKey();

        return builder;
    }
    
    /// <summary>
    /// Set up the LinqToDB schema mapping for the Application table
    /// </summary>
    /// <param name="builder">The LinqToDB fluent mapping builder</param>
    /// <param name="authorizationTableName">The name for the table in the database</param>
    /// <typeparam name="TAuthorization">The Authorization entity type to map</typeparam>
    /// <typeparam name="TKey">The primary key type for the Openiddict database tables</typeparam>
    /// <returns>LinqToDB fluent mapping builder</returns>
    internal static FluentMappingBuilder ConfigureAuthorizationMapping<TKey, TAuthorization>(this FluentMappingBuilder builder,
           string authorizationTableName)
        where TKey : notnull, IEquatable<TKey> 
        where TAuthorization : OpenIddictLinqToDBAuthorization<TKey>
    {
        if (builder is null) throw new ArgumentNullException(nameof(builder));
        if (authorizationTableName == null) throw new ArgumentNullException(nameof(authorizationTableName));

        // only set table info and primary key, other regular properties are inferred by LinqToDB
        builder.Entity<TAuthorization>()
            .HasTableName(authorizationTableName)
            .Property(i => i.Id).IsPrimaryKey();

        return builder;
    }
    
    /// <summary>
    /// Set up the LinqToDB schema mapping for the Application table
    /// </summary>
    /// <param name="builder">The LinqToDB fluent mapping builder</param>
    /// <param name="scopeTableName">The name for the table in the database</param>
    /// <typeparam name="TScope">The Scope entity type to map</typeparam>
    /// <typeparam name="TKey">The primary key type for the Openiddict database tables</typeparam>
    /// <returns>LinqToDB fluent mapping builder</returns>
    internal static FluentMappingBuilder ConfigureScopeMapping<TKey, TScope>(this FluentMappingBuilder builder,
           string scopeTableName)
        where TKey : notnull, IEquatable<TKey> 
        where TScope : OpenIddictLinqToDBScope<TKey>
    {
        if (builder is null) throw new ArgumentNullException(nameof(builder));
        if (scopeTableName == null) throw new ArgumentNullException(nameof(scopeTableName));

        // only set table info and primary key, other regular properties are inferred by LinqToDB
        builder.Entity<TScope>()
            .HasTableName(scopeTableName)
            .Property(i => i.Id).IsPrimaryKey();

        return builder;
    }
    
    /// <summary>
    /// Set up the LinqToDB schema mapping for the Application table
    /// </summary>
    /// <param name="builder">The LinqToDB fluent mapping builder</param>
    /// <param name="tokenTableName">The name for the table in the database</param>
    /// <typeparam name="TToken">The Token entity type to map</typeparam>
    /// <typeparam name="TKey">The primary key type for the Openiddict database tables</typeparam>
    /// <returns>LinqToDB fluent mapping builder</returns>
    internal static FluentMappingBuilder ConfigureTokenMapping<TKey, TToken>(this FluentMappingBuilder builder,
           string tokenTableName)
        where TKey : notnull, IEquatable<TKey> 
        where TToken : OpenIddictLinqToDBToken<TKey>
    {
        if (builder is null) throw new ArgumentNullException(nameof(builder));
        if (tokenTableName == null) throw new ArgumentNullException(nameof(tokenTableName));

        // only set table info and primary key, other regular properties are inferred by LinqToDB
        builder.Entity<TToken>()
            .HasTableName(tokenTableName)
            .Property(i => i.Id).IsPrimaryKey();

        return builder;
    }
}