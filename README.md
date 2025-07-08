# OpenIddict LinqToDB DataStore

This library is adapted from the OpenIddict EFCore and MongoDB libraries.
Thank you to Kévin Chalet for creating OpenIddict and for [encouraging
me](https://github.com/openiddict/openiddict-core/issues/1503) to create and maintain this library.

## Latest changes

### 5.0.0
- Upgraded OpenIddict from 6.x to 7.x

### 4.0.0
- Upgraded OpenIddict from 5.x to 6.x

### 3.1.0
- Upgrade packages
- Remove .NET 6 and 7 targets

### 3.0.0
- Upgraded OpenIddict from 4.x to 5.x
- Requires migrating database with something like:
 <details>
<summary>Migrate OpenIddict from 4.x to 5.x (PostgreSQL)</summary>

```sql
ALTER TABLE auth_openiddict_applications RENAME COLUMN type TO client_type;
ALTER TABLE auth_openiddict_applications ADD COLUMN application_type text;
ALTER TABLE auth_openiddict_applications ADD COLUMN settings text;
ALTER TABLE auth_openiddict_applications ADD COLUMN json_web_key_set text;
```
</details>

### 2.0.1
- Fix "update application" concurrency bug, PR #2

### 2.0.0 (actually 2.0.412392)
- Now using Linq2DB 5.0.0 stable

### 2.0.0-RC2
- Now using only MappingSchema to call UseOpenIddict (see example code below).
This is a breaking change.
You should not use RC1, the performance is very bad because the schema
was updated in every scope and so the LinqToDB queries were not being cached properly.

### 2.0.0-RC1
- Now using Linq2DB 5.0.0 preview.1 (not backward compatible)

### 1.0.0-preview7
In version `1.0.0-preview7` the database models have been updated to use NodaTime's
`Instant` instead of `DateTime`. You will likely need to add the following configuration
to the LinqToDB mapper: 
1. In your project, add a package reference to `Npgsql.NodaTime`
2. Add `NpgsqlConnection.GlobalTypeMapper.UseNodaTime();`. In Linq2DB 5.0.0
this needs to be the data source builder's `UseNodaTime()` as shown below

## How can I use this library?

#### Reference the `TecMeet.OpenIddict.LinqToDB` project
```xml
<PackageReference Include="TecMeet.OpenIddict.LinqToDB" Version="2.0.0" />
```

#### Configure table mappings for LinqToDB:
```csharp
var ms = new MappingSchema();
ms.UseOpenIddict();

// Or, also set the table names if you want to use something else
// than the default.
// ms.UseOpenIddict(new OpenIddictLinqToDBNameOptions
// {
//     ApplicationsTableName = "auth_openiddict_applications",
//     AuthorizationsTableName = "auth_openiddict_authorizations",
//     ScopesTableName = "auth_openiddict_scopes",
//     TokensTableName = "auth_openiddict_tokens"
// });

var dsBuilder = new NpgsqlDataSourceBuilder(connectionString);
dsBuilder.UseNodaTime();
var ds = dsBuilder.Build();

var dataOptions = new DataOptions()
    .UseMappingSchema(ms)
    .UseConnectionFactory(PostgreSQLTools.GetDataProvider(PostgreSQLVersion.v95), ds.CreateConnection);

services.AddLinqToDBContext<DbContext>((provider, _) =>
{
    dataOptions.UseDefaultLogging(provider);
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
