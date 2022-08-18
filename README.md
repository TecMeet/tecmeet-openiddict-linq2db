# OpenIddict LinqToDB DataStore

This library is adapted from the OpenIddict EFCore and MongoDB libraries.
Thank you to Kévin Chalet for creating OpenIddict and for [encouraging
me](https://github.com/openiddict/openiddict-core/issues/1503) to create and maintain this library.

## How can I use this library?

#### Reference the `TecMeet.OpenIddict.LinqToDB` project
```xml
<PackageReference Include="TecMeet.OpenIddict.LinqToDB" Version="1.0.0" />
```

#### Configure OpenIddict to use LinqToDB stores:
```csharp
    services.AddOpenIddict()
        .AddCore(options =>
        {
            options.UseLinqToDB()
                   .UseDbContext<MyDataContext>();
        });
```

#### Add the needed database tables
This is the SQL needed to create the tables **OpenIddict** uses for `string` primary key types. You
can also use other key types but you need to also adapt the configuration code for that.
<details>
<summary>SQL table creation code</summary>

```postgresql

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
