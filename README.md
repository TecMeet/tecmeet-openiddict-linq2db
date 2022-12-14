# OpenIddict LinqToDB DataStore

This library is adapted from the OpenIddict EFCore and MongoDB libraries.
Thank you to Kévin Chalet for creating OpenIddict and for [encouraging
me](https://github.com/openiddict/openiddict-core/issues/1503) to create and maintain this library.

## Latest changes
In version `1.0.0-preview7` the database models have been updated to use NodaTime's
`Instant` instead of `DateTime`. You will likely need to add the following configuration
to the LinqToDB mapper: 
1. In your project, add a package reference to `Npgsql.NodaTime`
2. Add `NpgsqlConnection.GlobalTypeMapper.UseNodaTime();`

## How can I use this library?

#### Reference the `TecMeet.OpenIddict.LinqToDB` project
```xml
<PackageReference Include="TecMeet.OpenIddict.LinqToDB" Version="1.0.0" />
```

#### Configure table mappings for LinqToDB:
```csharp
services.AddLinqToDBContext<DbContext>((provider, options) =>
    {
        options.UsePostgreSQL(connectionString);
        options.UseDefaultLogging(provider);
        
        // The LinqToDB MappingSchema. Use MappingSchema.Default if you
        // have not set up your own separate schema.
        options.UseOpenIddict(MappingSchema.Default);
        
        // Or, also set the table names if you want to use something else
        // than the default.
        // options.UseOpenIddict(MappingSchema.Default, new OpenIddictLinqToDBNameOptions
        // {
        //     ApplicationsTableName = "auth_openiddict_applications",
        //     AuthorizationsTableName = "auth_openiddict_authorizations",
        //     ScopesTableName = "auth_openiddict_scopes",
        //     TokensTableName = "auth_openiddict_tokens"
        // });
    });
```

#### Configure OpenIddict to use LinqToDB stores:
```csharp
services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseLinqToDB();
    });
```

#### Add the needed database tables
This is the SQL needed for PostgreSQL to create the tables **OpenIddict** uses for `string` primary key types. You
can also use other key types but you need to also adapt the configuration code for that.
<details>
<summary>SQL table creation code</summary>

```postgresql
CREATE TABLE auth_openiddict_applications
(
    type                      text      NULL,
    requirements              text      NULL,
    redirect_uris             text      NULL,
    properties                text      NULL,
    post_logout_redirect_uris text      NULL,
    permissions               text      NULL,
    id                        text  NOT NULL DEFAULT gen_random_uuid(),
    display_names             text      NULL,
    display_name              text      NULL,
    consent_type              text      NULL,
    concurrency_token         text      NULL,
    client_secret             text      NULL,
    client_id                 text      NULL,
    
    CONSTRAINT "PK_auth_openiddict_applications" PRIMARY KEY (id)
);

CREATE TABLE auth_openiddict_authorizations
(
    type              text           NULL,
    subject           text           NULL,
    status            text           NULL,
    scopes            text           NULL,
    properties        text           NULL,
    id                text       NOT NULL DEFAULT gen_random_uuid(),
    creation_date     timestamptz      NULL,
    concurrency_token text           NULL,
    application_id    text           NULL,

    CONSTRAINT "PK_auth_openiddict_authorizations" PRIMARY KEY (id)
);

CREATE TABLE auth_openiddict_scopes
(
    resources         text      NULL,
    properties        text      NULL,
    name              text      NULL,
    id                text  NOT NULL DEFAULT gen_random_uuid(),
    display_names     text      NULL,
    display_name      text      NULL,
    descriptions      text      NULL,
    description       text      NULL,
    concurrency_token text      NULL,

    CONSTRAINT "PK_auth_openiddict_scopes" PRIMARY KEY (id)
);

CREATE TABLE auth_openiddict_tokens
(
    type              text           NULL,
    subject           text           NULL,
    status            text           NULL,
    reference_id      text           NULL,
    redemption_date   timestamptz      NULL,
    properties        text           NULL,
    payload           text           NULL,
    id                text       NOT NULL DEFAULT gen_random_uuid(),
    expiration_date   timestamptz      NULL,
    creation_date     timestamptz      NULL,
    concurrency_token text           NULL,
    authorization_id  text           NULL,
    application_id    text           NULL,

    CONSTRAINT "PK_auth_openiddict_tokens" PRIMARY KEY (id)
);
```
</details>

## What's OpenIddict?
It's created and maintained by Kévin Chalet.

OpenIddict aims at providing a **versatile solution** to implement **OpenID Connect client, server and token validation support in any ASP.NET Core 2.1 (and higher) application**.
**ASP.NET 4.6.1 (and higher) applications are also fully supported thanks to a native Microsoft.Owin 4.2 integration**.

OpenIddict fully supports the **[code/implicit/hybrid flows](http://openid.net/specs/openid-connect-core-1_0.html)**,
the **[client credentials/resource owner password grants](https://tools.ietf.org/html/rfc6749)** and the [device authorization flow](https://tools.ietf.org/html/rfc8628).

OpenIddict natively supports **[Entity Framework Core](https://www.nuget.org/packages/OpenIddict.EntityFrameworkCore)**,
**[Entity Framework 6](https://www.nuget.org/packages/OpenIddict.EntityFramework)** and **[MongoDB](https://www.nuget.org/packages/OpenIddict.MongoDb)**
out-of-the-box and custom stores can be implemented to support other providers (that's what this repo is,
a custom store for LinqToDB).

--------------

## Security policy

Security issues or bugs should be reported privately by emailing tim at tecmeet dot com.
You should receive a response within 24 hours. If for some reason you do not, please follow up via email to ensure we received your original message.

--------------

## License

This project is licensed under the **Apache License**. This means that you can use, modify and distribute it freely.
See [http://www.apache.org/licenses/LICENSE-2.0.html](http://www.apache.org/licenses/LICENSE-2.0.html) for more details.
