using FluentAssertions;
using LinqToDB;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.Collections.Immutable;
using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using TecMeet.OpenIddict.LinqToDB;
using TecMeet.OpenIddict.LinqToDB.Models;
using Xunit;

namespace OpenIddict.LinqToDB.Tests.Stores;

public class OpenIddictLinqToDBApplicationStoreTests
{
    [Fact]
    public void Constructor_NullCacheShouldThrow()
    {
        // Arrange
        var context = Mock.Of<IDataContext>();
        var cache = (IMemoryCache)null!;

        // Act
        var act = () =>
            new OpenIddictLinqToDBApplicationStore(
                cache,
                context);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("cache");
    }

    [Fact]
    public void Constructor_NullContextShouldThrow()
    {
        // Arrange
        var context = (IDataContext)null!;
        var cache = Mock.Of<IMemoryCache>();

        // Act
        var act = () =>
            new OpenIddictLinqToDBApplicationStore(
                cache,
                context);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("context");
    }

    [Fact]
    public void CreateAsync_NullApplicationParameterShouldThrow()
    {
        // Arrange
        var store = CreateStore();

        // Act
        var act = async () =>
            await store.CreateAsync(null!, new CancellationToken());

        // Assert
        act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("application");
    }

    [Fact]
    public void DeleteAsync_NullApplicationParameterShouldThrow()
    {
        // Arrange
        var store = CreateStore();

        // Act
        var act = async () =>
            await store.DeleteAsync(null!, new CancellationToken());

        // Assert
        act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("application");
    }

    [Fact]
    public void FindByClientIdAsync_NullOrEmptyIdentifierParameterShouldThrow()
    {
        // Arrange
        var store = CreateStore();

        // Act
        var act = async () =>
            await store.FindByClientIdAsync(null!, new CancellationToken());

        // Assert
        act.Should().ThrowAsync<ArgumentException>(SR.GetResourceString(SR.ID0195)).WithParameterName("identifier");
    }

    [Fact]
    public void FindByIdAsync_NullOrEmptyIdentifierParameterShouldThrow()
    {
        // Arrange
        var store = CreateStore();

        // Act
        var act = async () =>
            await store.FindByIdAsync(null!, new CancellationToken());

        // Assert
        act.Should().ThrowAsync<ArgumentException>(SR.GetResourceString(SR.ID0195)).WithParameterName("identifier");
    }

    [Fact]
    public void FindByPostLogoutRedirectUriAsync_NullOrEmptyAddressParameterShouldThrow()
    {
        // Arrange
        var store = CreateStore();

        // Act
        var act = () =>
            store.FindByPostLogoutRedirectUriAsync(null!, new CancellationToken());

        // Assert
        act.Should().Throw<ArgumentException>(SR.GetResourceString(SR.ID0143)).WithParameterName("uri");
    }

    [Fact]
    public void FindByRedirectUriAsync_NullOrEmptyAddressParameterShouldThrow()
    {
        // Arrange
        var store = CreateStore();

        // Act
        var act = () =>
            store.FindByRedirectUriAsync(null!, new CancellationToken());

        // Assert
        act.Should().Throw<ArgumentException>(SR.GetResourceString(SR.ID0143)).WithParameterName("uri");
    }

    [Fact]
    public void GetApplicationTypeAsync_NullApplicationParameterShouldThrow()
    {
        // Arrange
        var store = CreateStore();

        // Act
        var act = () =>
            store.GetApplicationTypeAsync(null!, new CancellationToken());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("application");
    }

    [Fact]
    public async Task GetApplicationTypeAsync_ApplicationTypeShouldBeReturned()
    {
        // Arrange
        var store = CreateStore();
        var application = CreateApplication();
        application.ApplicationType = ApplicationTypes.Web;

        // Act
        var result = await store.GetApplicationTypeAsync(application, new CancellationToken());

        // Assert
        result.Should().Be(ApplicationTypes.Web);
    }

    [Fact]
    public void GetClientIdAsync_NullApplicationParameterShouldThrow()
    {
        // Arrange
        var store = CreateStore();

        // Act
        var act = () =>
            store.GetClientIdAsync(null!, new CancellationToken());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("application");
    }

    [Fact]
    public async Task GetClientIdAsync_ClientIdShouldBeReturned()
    {
        // Arrange
        var store = CreateStore();
        var application = CreateApplication();
        application.ClientId = "abc123";

        // Act
        var result = await store.GetClientIdAsync(application, new CancellationToken());

        // Assert
        result.Should().Be("abc123");
    }

    [Fact]
    public void GetClientSecretAsync_NullApplicationParameterShouldThrow()
    {
        // Arrange
        var store = CreateStore();

        // Act
        var act = () =>
            store.GetClientSecretAsync(null!, new CancellationToken());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("application");
    }

    [Fact]
    public async Task GetClientSecretAsync_ClientSecretShouldBeReturned()
    {
        // Arrange
        var store = CreateStore();
        var application = CreateApplication();
        application.ClientSecret = "secret123";

        // Act
        var result = await store.GetClientSecretAsync(application, new CancellationToken());

        // Assert
        result.Should().Be("secret123");
    }

    [Fact]
    public void GetClientTypeAsync_NullApplicationParameterShouldThrow()
    {
        // Arrange
        var store = CreateStore();

        // Act
        var act = () =>
            store.GetClientTypeAsync(null!, new CancellationToken());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("application");
    }

    [Fact]
    public async Task GetClientTypeAsync_ClientTypeShouldBeReturned()
    {
        // Arrange
        var store = CreateStore();
        var application = CreateApplication();
        application.ClientType = "clientType123";

        // Act
        var result = await store.GetClientTypeAsync(application, new CancellationToken());

        // Assert
        result.Should().Be("clientType123");
    }

    [Fact]
    public void GetConsentTypeAsync_NullApplicationParameterShouldThrow()
    {
        // Arrange
        var store = CreateStore();

        // Act
        var act = () =>
            store.GetConsentTypeAsync(null!, new CancellationToken());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("application");
    }

    [Fact]
    public async Task GetConsentTypeAsync_ConsentTypeShouldBeReturned()
    {
        // Arrange
        var store = CreateStore();
        var application = CreateApplication();
        application.ConsentType = "consentType123";

        // Act
        var result = await store.GetConsentTypeAsync(application, new CancellationToken());

        // Assert
        result.Should().Be("consentType123");
    }

    [Fact]
    public void GetDisplayNameAsync_NullApplicationParameterShouldThrow()
    {
        // Arrange
        var store = CreateStore();

        // Act
        var act = () =>
            store.GetDisplayNameAsync(null!, new CancellationToken());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("application");
    }

    [Fact]
    public async Task GetDisplayNameAsync_DisplayNameShouldBeReturned()
    {
        // Arrange
        var store = CreateStore();
        var application = CreateApplication();
        application.DisplayName = "display123";

        // Act
        var result = await store.GetDisplayNameAsync(application, new CancellationToken());

        // Assert
        result.Should().Be("display123");
    }

    [Fact]
    public void GetDisplayNamesAsync_NullApplicationParameterShouldThrow()
    {
        // Arrange
        var store = CreateStore();

        // Act
        var act = () =>
            store.GetDisplayNamesAsync(null!, new CancellationToken());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("application");
    }

    [Fact]
    public async Task GetDisplayNamesAsync_DisplayNamesShouldBeReturned()
    {
        // Arrange
        var application = CreateApplication();
        application.DisplayNames = "{\"af\":\"name1\",\"ar\":\"name2\",\"az\":\"\",\"be\":\"name3\",\"ca\":null}";
        var memoryCache = CreateMemoryCache();
        var store = CreateStore(memoryCache);

        // Act
        var result = await store.GetDisplayNamesAsync(application, new CancellationToken());

        // Assert
        result.Should().HaveCount(3);
        result.Should().NotContainValue("");
        result.Should().ContainValues(new[] { "name1", "name2", "name3" });
    }

    [Fact]
    public async Task GetDisplayNamesAsync_EmptyDisplayNamesShouldReturnEmptyDictionary()
    {
        // Arrange
        var application = CreateApplication();
        application.DisplayNames = "";
        var memoryCache = CreateMemoryCache();
        var store = CreateStore(memoryCache);

        // Act
        var result = await store.GetDisplayNamesAsync(application, new CancellationToken());

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(0);
    }

    [Fact]
    public async Task GetDisplayNamesAsync_NullDisplayNamesShouldReturnEmptyDictionary()
    {
        // Arrange
        var application = CreateApplication();
        application.DisplayNames = null;
        var memoryCache = CreateMemoryCache();
        var store = CreateStore(memoryCache);

        // Act
        var result = await store.GetDisplayNamesAsync(application, new CancellationToken());

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(0);
    }

    [Fact]
    public void GetIdAsync_NullApplicationParameterShouldThrow()
    {
        // Arrange
        var store = CreateStore();

        // Act
        var act = () =>
            store.GetIdAsync(null!, new CancellationToken());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("application");
    }

    [Fact]
    public async Task GetIdAsync_StringIdShouldBeReturned()
    {
        // Arrange
        var application = CreateApplication();
        application.Id = "e7fe1a6a-5d2f-4ea9-be72-2c567259c74d";
        var store = CreateStore();

        // Act
        var result = await store.GetIdAsync(application, new CancellationToken());

        // Assert
        result.Should().Be("e7fe1a6a-5d2f-4ea9-be72-2c567259c74d");
    }

    [Fact]
    public async Task GetIdAsync_LongIdShouldBeReturned()
    {
        // Arrange
        var application = CreateApplication<long>();
        application.Id = 999555;
        var store = CreateStore<long>();

        // Act
        var result = await store.GetIdAsync(application, new CancellationToken());

        // Assert
        result.Should().Be("999555");
    }

    [Fact]
    public async Task GetIdAsync_GuidIdShouldBeReturned()
    {
        // Arrange
        var application = CreateApplication<Guid>();
        application.Id = Guid.Parse("58152781-fc2d-4923-9665-d4eb1d27f28c");
        var store = CreateStore<Guid>();

        // Act
        var result = await store.GetIdAsync(application, new CancellationToken());

        // Assert
        result.Should().Be("58152781-fc2d-4923-9665-d4eb1d27f28c");
    }

    [Fact]
    public void GetJsonWebKeySetAsync_NullApplicationParameterShouldThrow()
    {
        // Arrange
        var store = CreateStore();

        // Act
        var act = () =>
            store.GetJsonWebKeySetAsync(null!, new CancellationToken());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("application");
    }

    [Fact]
    public async void GetJsonWebKeySetAsync_NullJsonWebKeySetParameterShouldThrow()
    {
        // Arrange
        var store = CreateStore();
        var application = CreateApplication();

        // Act
        var result = await store.GetJsonWebKeySetAsync(application, new CancellationToken());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetJsonWebKeySetAsync_JsonWebKeySetShouldBeReturned()
    {
        // Arrange
        var memoryCache = CreateMemoryCache();
        var store = CreateStore(memoryCache);
        var application = CreateApplication();
        application.JsonWebKeySet = "{\"keys\":[{\"crv\":\"P-256\",\"key_ops\":[],\"kty\":\"EC\",\"oth\":[],\"x\":\"I23kaVsRRAWIez_pqEZOByJFmlXda6iSQ4QqcH23Ir8\",\"x5c\":[],\"y\":\"GmDz1-ZbFZwbBMTbJe0jmDoiIYE2l-vj00u8sU55r0s\"}]}";

        // Act
        var result = await store.GetJsonWebKeySetAsync(application, new CancellationToken());

        // Assert
        result.Should().NotBeNull();
        result!.Keys.Should().HaveCount(1);
    }

    [Fact]
    public void GetPermissionsAsync_NullApplicationParameterShouldThrow()
    {
        // Arrange
        var store = CreateStore();

        // Act
        var act = () =>
            store.GetPermissionsAsync(null!, new CancellationToken());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("application");
    }

    [Fact]
    public async Task GetPermissionsAsync_PermissionsShouldBeReturned()
    {
        // Arrange
        var application = CreateApplication();
        application.Permissions = "[\"\",null,\"DoThis\",null,\"DoThat\"]";
        var memoryCache = CreateMemoryCache();
        var store = CreateStore(memoryCache);

        // Act
        var result = await store.GetPermissionsAsync(application, new CancellationToken());

        // Assert
        result.Should().HaveCount(2);
        result.Should().NotContain("");
        result.Should().Contain(new[] { "DoThis", "DoThat" });
    }

    [Fact]
    public async Task GetPermissionsAsync_EmptyPermissionsShouldReturnEmptyDictionary()
    {
        // Arrange
        var application = CreateApplication();
        application.Permissions = "";
        var memoryCache = CreateMemoryCache();
        var store = CreateStore(memoryCache);

        // Act
        var result = await store.GetPermissionsAsync(application, new CancellationToken());

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(0);
    }

    [Fact]
    public async Task GetPermissionsAsync_NullPermissionsShouldReturnEmptyDictionary()
    {
        // Arrange
        var application = CreateApplication();
        application.Permissions = null;
        var memoryCache = CreateMemoryCache();
        var store = CreateStore(memoryCache);

        // Act
        var result = await store.GetPermissionsAsync(application, new CancellationToken());

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(0);
    }

    [Fact]
    public void GetPostLogoutRedirectUrisAsync_NullApplicationParameterShouldThrow()
    {
        // Arrange
        var store = CreateStore();

        // Act
        var act = () =>
            store.GetPostLogoutRedirectUrisAsync(null!, new CancellationToken());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("application");
    }

    [Fact]
    public async Task GetPostLogoutRedirectUrisAsync_ValuesShouldBeReturned()
    {
        // Arrange
        var application = CreateApplication();
        application.PostLogoutRedirectUris = "[\"\",null,\"Redirect1\",null,\"Redirect2\"]";
        var memoryCache = CreateMemoryCache();
        var store = CreateStore(memoryCache);

        // Act
        var result = await store.GetPostLogoutRedirectUrisAsync(application, new CancellationToken());

        // Assert
        result.Should().HaveCount(2);
        result.Should().NotContain("");
        result.Should().Contain(new[] { "Redirect1", "Redirect2" });
    }

    [Fact]
    public async Task GetPostLogoutRedirectUrisAsync_EmptyValuesShouldReturnEmptyDictionary()
    {
        // Arrange
        var application = CreateApplication();
        application.PostLogoutRedirectUris = "";
        var memoryCache = CreateMemoryCache();
        var store = CreateStore(memoryCache);

        // Act
        var result = await store.GetPostLogoutRedirectUrisAsync(application, new CancellationToken());

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(0);
    }

    [Fact]
    public async Task GetPostLogoutRedirectUrisAsync_NullValuesShouldReturnEmptyDictionary()
    {
        // Arrange
        var application = CreateApplication();
        application.PostLogoutRedirectUris = null;
        var memoryCache = CreateMemoryCache();
        var store = CreateStore(memoryCache);

        // Act
        var result = await store.GetPostLogoutRedirectUrisAsync(application, new CancellationToken());

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(0);
    }

    [Fact]
    public void GetPropertiesAsync_NullApplicationParameterShouldThrow()
    {
        // Arrange
        var store = CreateStore();

        // Act
        var act = () =>
            store.GetPropertiesAsync(null!, new CancellationToken());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("application");
    }

    [Fact]
    public async Task GetPropertiesAsync_ValuesShouldBeReturned()
    {
        // Arrange
        var application = CreateApplication();
        application.Properties = "{\"a\":\"prop1\",\"c\":\"\",\"e\":null}";
        var memoryCache = CreateMemoryCache();
        var store = CreateStore(memoryCache);

        // Act
        var result = await store.GetPropertiesAsync(application, new CancellationToken());

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(i => i.Value.GetString() == "prop1");
        // TODO not sure if this "should" be expected logic or if the app should check the values for empty/null
        result.Should().Contain(i => i.Value.GetString() == "");
        result.Should().Contain(i => i.Value.GetString() == null);
    }

    [Fact]
    public async Task GetPropertiesAsync_EmptyDisplayNamesShouldReturnEmptyDictionary()
    {
        // Arrange
        var application = CreateApplication();
        application.Properties = "";
        var memoryCache = CreateMemoryCache();
        var store = CreateStore(memoryCache);

        // Act
        var result = await store.GetPropertiesAsync(application, new CancellationToken());

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(0);
    }

    [Fact]
    public async Task GetPropertiesAsync_NullDisplayNamesShouldReturnEmptyDictionary()
    {
        // Arrange
        var application = CreateApplication();
        application.Properties = null;
        var memoryCache = CreateMemoryCache();
        var store = CreateStore(memoryCache);

        // Act
        var result = await store.GetPropertiesAsync(application, new CancellationToken());

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(0);
    }

    [Fact]
    public void GetRedirectUrisAsync_NullApplicationParameterShouldThrow()
    {
        // Arrange
        var store = CreateStore();

        // Act
        var act = () =>
            store.GetRedirectUrisAsync(null!, new CancellationToken());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("application");
    }

    [Fact]
    public async Task GetRedirectUrisAsync_ValuesShouldBeReturned()
    {
        // Arrange
        var application = CreateApplication();
        application.RedirectUris = "[\"\",null,\"RedirectUri1\",null,\"RedirectUri2\"]";
        var memoryCache = CreateMemoryCache();
        var store = CreateStore(memoryCache);

        // Act
        var result = await store.GetRedirectUrisAsync(application, new CancellationToken());

        // Assert
        result.Should().HaveCount(2);
        result.Should().NotContain("");
        result.Should().Contain(new[] { "RedirectUri1", "RedirectUri2" });
    }

    [Fact]
    public async Task GetRedirectUrisAsync_EmptyValuesShouldReturnEmptyDictionary()
    {
        // Arrange
        var application = CreateApplication();
        application.RedirectUris = "";
        var memoryCache = CreateMemoryCache();
        var store = CreateStore(memoryCache);

        // Act
        var result = await store.GetRedirectUrisAsync(application, new CancellationToken());

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(0);
    }

    [Fact]
    public async Task GetRedirectUrisAsync_NullValuesShouldReturnEmptyDictionary()
    {
        // Arrange
        var application = CreateApplication();
        application.RedirectUris = null;
        var memoryCache = CreateMemoryCache();
        var store = CreateStore(memoryCache);

        // Act
        var result = await store.GetRedirectUrisAsync(application, new CancellationToken());

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(0);
    }

    [Fact]
    public void GetRequirementsAsync_NullApplicationParameterShouldThrow()
    {
        // Arrange
        var store = CreateStore();

        // Act
        var act = () =>
            store.GetRequirementsAsync(null!, new CancellationToken());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("application");
    }

    [Fact]
    public async Task GetRequirementsAsync_ValuesShouldBeReturned()
    {
        // Arrange
        var application = CreateApplication();
        application.Requirements = "[\"\",null,\"RedirectUri1\",null,\"RedirectUri2\"]";
        var memoryCache = CreateMemoryCache();
        var store = CreateStore(memoryCache);

        // Act
        var result = await store.GetRequirementsAsync(application, new CancellationToken());

        // Assert
        result.Should().HaveCount(2);
        result.Should().NotContain("");
        result.Should().Contain(new[] { "RedirectUri1", "RedirectUri2" });
    }

    [Fact]
    public async Task GetRequirementsAsync_EmptyValuesShouldReturnEmptyDictionary()
    {
        // Arrange
        var application = CreateApplication();
        application.Requirements = "";
        var memoryCache = CreateMemoryCache();
        var store = CreateStore(memoryCache);

        // Act
        var result = await store.GetRequirementsAsync(application, new CancellationToken());

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(0);
    }

    [Fact]
    public async Task GetRequirementsAsync_NullValuesShouldReturnEmptyDictionary()
    {
        // Arrange
        var application = CreateApplication();
        application.Requirements = null;
        var memoryCache = CreateMemoryCache();
        var store = CreateStore(memoryCache);

        // Act
        var result = await store.GetRequirementsAsync(application, new CancellationToken());

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(0);
    }

    [Fact]
    public void GetSettingsAsync_NullApplicationParameterShouldThrow()
    {
        // Arrange
        var store = CreateStore();

        // Act
        var act = () =>
            store.GetSettingsAsync(null!, new CancellationToken());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("application");
    }

    [Fact]
    public async void GetSettingsAsync_NullValuesShouldReturnEmptyDictionary()
    {
        // Arrange
        var store = CreateStore();
        var application = CreateApplication();
        application.Settings = null;

        // Act
        var result = await store.GetSettingsAsync(application, new CancellationToken());

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(0);
    }

    [Fact]
    public async void GetSettingsAsync_ValuesShouldReturn()
    {
        // Arrange
        var memoryCache = CreateMemoryCache();
        var store = CreateStore(memoryCache);
        var application = CreateApplication();
        application.Settings = "{\"tkn_lft:act\":\"00:10:00\"}";

        // Act
        var result = await store.GetSettingsAsync(application, new CancellationToken());

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.Should().ContainKey(Settings.TokenLifetimes.AccessToken);
    }

    [Fact]
    public async Task InstantiateAsync_CreatedTypeIsCorrect()
    {
        // Arrange
        var store = CreateStore<CustomApplication, CustomAuthorization, CustomToken, long>();

        // Act
        var result = await store.InstantiateAsync(new CancellationToken());

        // Assert
        result.Should().BeOfType<CustomApplication>();
    }

    [Fact]
    public void SetApplicationTypeAsync_NullApplicationParameterShouldThrow()
    {
        // Arrange
        var store = CreateStore();

        // Act
        var act = () =>
            store.SetApplicationTypeAsync(null!, null, new CancellationToken());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("application");
    }

    [Fact]
    public async void SetApplicationTypeAsync_ApplicationTypeToNullShouldWork()
    {
        // Arrange
        var store = CreateStore();
        var application = CreateApplication();
        application.ApplicationType = ApplicationTypes.Web;

        // Act
        await store.SetApplicationTypeAsync(application, null, new CancellationToken());

        // Assert
        application.ApplicationType.Should().BeNull();
    }

    [Fact]
    public async void SetApplicationTypeAsync_ApplicationTypeToOtherValueShouldWork()
    {
        // Arrange
        var store = CreateStore();
        var application = CreateApplication();
        application.ApplicationType = ApplicationTypes.Web;

        // Act
        await store.SetClientIdAsync(application, ApplicationTypes.Native, new CancellationToken());

        // Assert
        application.ClientId.Should().Be(ApplicationTypes.Native);
    }

    [Fact]
    public void SetClientIdAsync_NullApplicationParameterShouldThrow()
    {
        // Arrange
        var store = CreateStore();

        // Act
        var act = () =>
            store.SetClientIdAsync(null!, null, new CancellationToken());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("application");
    }

    // TODO is this actually correct logic?
    [Fact]
    public async Task SetClientIdAsync_SettingIdToNullShouldWork()
    {
        // Arrange
        var store = CreateStore();
        var application = CreateApplication();
        application.ClientId = "123";

        // Act
        await store.SetClientIdAsync(application, null, new CancellationToken());

        // Assert
        application.ClientId.Should().BeNull();
    }

    [Fact]
    public async Task SetClientIdAsync_SettingToOtherValueShouldWork()
    {
        // Arrange
        var store = CreateStore();
        var application = CreateApplication();
        application.ClientId = "123";

        // Act
        await store.SetClientIdAsync(application, "456", new CancellationToken());

        // Assert
        application.ClientId.Should().Be("456");
    }

    [Fact]
    public void SetClientSecretAsync_NullApplicationParameterShouldThrow()
    {
        // Arrange
        var store = CreateStore();

        // Act
        var act = () =>
            store.SetClientSecretAsync(null!, null, new CancellationToken());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("application");
    }

    // TODO is this actually correct logic?
    [Fact]
    public async Task SetClientSecretAsync_SettingIdToNullShouldWork()
    {
        // Arrange
        var store = CreateStore();
        var application = CreateApplication();
        application.ClientSecret = "123";

        // Act
        await store.SetClientSecretAsync(application, null, new CancellationToken());

        // Assert
        application.ClientSecret.Should().BeNull();
    }

    [Fact]
    public async Task SetClientSecretAsync_SettingToOtherValueShouldWork()
    {
        // Arrange
        var store = CreateStore();
        var application = CreateApplication();
        application.ClientSecret = "123";

        // Act
        await store.SetClientSecretAsync(application, "456", new CancellationToken());

        // Assert
        application.ClientSecret.Should().Be("456");
    }

    [Fact]
    public void SetClientTypeAsync_NullApplicationParameterShouldThrow()
    {
        // Arrange
        var store = CreateStore();

        // Act
        var act = () =>
            store.SetClientTypeAsync(null!, null, new CancellationToken());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("application");
    }

    // TODO is this actually correct logic?
    [Fact]
    public async Task SetClientTypeAsync_SettingIdToNullShouldWork()
    {
        // Arrange
        var store = CreateStore();
        var application = CreateApplication();
        application.ClientType = "123";

        // Act
        await store.SetClientTypeAsync(application, null, new CancellationToken());

        // Assert
        application.ClientType.Should().BeNull();
    }

    [Fact]
    public async Task SetClientTypeAsync_SettingToOtherValueShouldWork()
    {
        // Arrange
        var store = CreateStore();
        var application = CreateApplication();
        application.ClientType = "123";

        // Act
        await store.SetClientTypeAsync(application, "456", new CancellationToken());

        // Assert
        application.ClientType.Should().Be("456");
    }

    [Fact]
    public void SetConsentTypeAsync_NullApplicationParameterShouldThrow()
    {
        // Arrange
        var store = CreateStore();

        // Act
        var act = () =>
            store.SetConsentTypeAsync(null!, null, new CancellationToken());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("application");
    }

    // TODO is this actually correct logic?
    [Fact]
    public async Task SetConsentTypeAsync_SettingIdToNullShouldWork()
    {
        // Arrange
        var store = CreateStore();
        var application = CreateApplication();
        application.ConsentType = "123";

        // Act
        await store.SetConsentTypeAsync(application, null, new CancellationToken());

        // Assert
        application.ConsentType.Should().BeNull();
    }

    [Fact]
    public async Task SetConsentTypeAsync_SettingToOtherValueShouldWork()
    {
        // Arrange
        var store = CreateStore();
        var application = CreateApplication();
        application.ConsentType = "123";

        // Act
        await store.SetConsentTypeAsync(application, "456", new CancellationToken());

        // Assert
        application.ConsentType.Should().Be("456");
    }

    [Fact]
    public void SetDisplayNameAsync_NullApplicationParameterShouldThrow()
    {
        // Arrange
        var store = CreateStore();

        // Act
        var act = () =>
            store.SetDisplayNameAsync(null!, null, new CancellationToken());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("application");
    }

    // TODO is this actually correct logic?
    [Fact]
    public async Task SetDisplayNameAsync_SettingIdToNullShouldWork()
    {
        // Arrange
        var store = CreateStore();
        var application = CreateApplication();
        application.DisplayName = "123";

        // Act
        await store.SetDisplayNameAsync(application, null, new CancellationToken());

        // Assert
        application.DisplayName.Should().BeNull();
    }

    [Fact]
    public async Task SetDisplayNameAsync_SettingToOtherValueShouldWork()
    {
        // Arrange
        var store = CreateStore();
        var application = CreateApplication();
        application.DisplayName = "123";

        // Act
        await store.SetDisplayNameAsync(application, "456", new CancellationToken());

        // Assert
        application.DisplayName.Should().Be("456");
    }

    [Fact]
    public void SetJsonWebKeySetAsync_NullApplicationParameterShouldThrow()
    {
        // Arrange
        var store = CreateStore();

        // Act
        var act = () =>
            store.SetJsonWebKeySetAsync(null!, null, new CancellationToken());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("application");
    }

    [Fact]
    public async Task SetJsonWebKeySetAsync_SettingIdToNullShouldWork()
    {
        // Arrange
        var store = CreateStore();
        var application = CreateApplication();
        application.JsonWebKeySet = "123";

        // Act
        await store.SetJsonWebKeySetAsync(application, null, new CancellationToken());

        // Assert
        application.DisplayName.Should().BeNull();
    }

    [Fact]
    public async Task SetJsonWebKeySetAsync_SettingToOtherValueShouldWork()
    {
        // Arrange
        var store = CreateStore();
        var application = CreateApplication();
        var set = new JsonWebKeySet
        {
            Keys = {
                       // On supported platforms, this application authenticates by generating JWT client
                       // assertions that are signed using a signing key instead of using a client secret.
                       //
                       // Note: while the client needs access to the private key, the server only needs
                       // to know the public key to be able to validate the client assertions it receives.
                       JsonWebKeyConverter.ConvertFromECDsaSecurityKey(GetECDsaSigningKey($"""
                           -----BEGIN PUBLIC KEY-----
                           MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEI23kaVsRRAWIez/pqEZOByJFmlXd
                           a6iSQ4QqcH23Ir8aYPPX5lsVnBsExNsl7SOYOiIhgTaX6+PTS7yxTnmvSw==
                           -----END PUBLIC KEY-----
                           """))
                   }
        };

        // Act
        await store.SetJsonWebKeySetAsync(application, set, new CancellationToken());

        // Assert
        application.JsonWebKeySet.Should().Be("{\"keys\":[{\"crv\":\"P-256\",\"key_ops\":[],\"kty\":\"EC\",\"oth\":[],\"x\":\"I23kaVsRRAWIez_pqEZOByJFmlXda6iSQ4QqcH23Ir8\",\"x5c\":[],\"y\":\"GmDz1-ZbFZwbBMTbJe0jmDoiIYE2l-vj00u8sU55r0s\"}]}");
    }

    [Fact]
    public void SetDisplayNamesAsync_NullApplicationParameterShouldThrow()
    {
        // Arrange
        var store = CreateStore();

        // Act
        var act = () =>
            store.SetDisplayNamesAsync(null!, ImmutableDictionary<CultureInfo, string>.Empty, new CancellationToken());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("application");
    }

    [Fact]
    public void SetPermissionsAsync_NullApplicationParameterShouldThrow()
    {
        // Arrange
        var store = CreateStore();

        // Act
        var act = () =>
            store.SetPermissionsAsync(null!, ImmutableArray<string>.Empty, new CancellationToken());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("application");
    }

    [Fact]
    public void SetPostLogoutRedirectUrisAsync_NullApplicationParameterShouldThrow()
    {
        // Arrange
        var store = CreateStore();

        // Act
        var act = () =>
            store.SetPostLogoutRedirectUrisAsync(null!, ImmutableArray<string>.Empty, new CancellationToken());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("application");
    }

    [Fact]
    public void SetPropertiesAsync_NullApplicationParameterShouldThrow()
    {
        // Arrange
        var store = CreateStore();

        // Act
        var act = () =>
            store.SetPropertiesAsync(null!, ImmutableDictionary<string, JsonElement>.Empty, new CancellationToken());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("application");
    }

    [Fact]
    public void SetRedirectUrisAsync_NullApplicationParameterShouldThrow()
    {
        // Arrange
        var store = CreateStore();

        // Act
        var act = () =>
            store.SetRedirectUrisAsync(null!, ImmutableArray<string>.Empty, new CancellationToken());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("application");
    }

    [Fact]
    public void SetRequirementsAsync_NullApplicationParameterShouldThrow()
    {
        // Arrange
        var store = CreateStore();

        // Act
        var act = () =>
            store.SetRequirementsAsync(null!, ImmutableArray<string>.Empty, new CancellationToken());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("application");
    }

    [Fact]
    public void SetSettingsAsync_NullApplicationParameterShouldThrow()
    {
        // Arrange
        var store = CreateStore();

        // Act
        var act = () =>
            store.SetSettingsAsync(null!, ImmutableDictionary<string, string>.Empty, new CancellationToken());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("application");
    }

    [Fact]
    public async void SetSettingsAsync_SettingsToOtherValueShouldWork()
    {
        // Arrange
        var store = CreateStore();
        var application = CreateApplication();
        application.Settings = "{\"tkn_lft:act\":\"00:15:00\"}";

        var settings = new Dictionary<string, string>
        {
            [Settings.TokenLifetimes.AccessToken] = TimeSpan.FromMinutes(10).ToString("c", CultureInfo.InvariantCulture)
        };

        // Act
        await store.SetSettingsAsync(application, settings.ToImmutableDictionary(), new CancellationToken());

        // Assert
        application.Settings.Should().Be("{\"tkn_lft:act\":\"00:10:00\"}");
    }

    [Fact]
    public void UpdateAsync_NullApplicationParameterShouldThrow()
    {
        // Arrange
        var store = CreateStore();

        // Act
        var act = async () =>
            await store.UpdateAsync(null!, new CancellationToken());

        // Assert
        act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("application");
    }

    private static IOpenIddictApplicationStore<OpenIddictLinqToDBApplication> CreateStore(IMemoryCache? memoryCache = null)
        => new OpenIddictLinqToDBApplicationStore(memoryCache ?? Mock.Of<IMemoryCache>(), Mock.Of<IDataContext>());

    private static IOpenIddictApplicationStore<OpenIddictLinqToDBApplication<T>> CreateStore<T>(IMemoryCache? memoryCache = null) where T : IEquatable<T>
        => new OpenIddictLinqToDBApplicationStore<T>(memoryCache ?? Mock.Of<IMemoryCache>(), Mock.Of<IDataContext>());

    private static IOpenIddictApplicationStore<TApplication> CreateStore<TApplication, TAuthorization, TToken, TKey>(IMemoryCache? memoryCache = null)
        where TKey : IEquatable<TKey>
        where TApplication : OpenIddictLinqToDBApplication<TKey>
        where TAuthorization : OpenIddictLinqToDBAuthorization<TKey>
        where TToken : OpenIddictLinqToDBToken<TKey>
        => new OpenIddictLinqToDBApplicationStore<TApplication, TAuthorization, TToken, TKey>(memoryCache ?? Mock.Of<IMemoryCache>(), Mock.Of<IDataContext>());

    private static OpenIddictLinqToDBApplication CreateApplication() => new();
    private static OpenIddictLinqToDBApplication<T> CreateApplication<T>() where T : IEquatable<T>
        => new();

    private static IMemoryCache? CreateMemoryCache() =>
        new ServiceCollection().AddMemoryCache()
            .BuildServiceProvider().GetService<IMemoryCache>();

    static ECDsaSecurityKey GetECDsaSigningKey(ReadOnlySpan<char> key)
    {
        var algorithm = ECDsa.Create();
        algorithm.ImportFromPem(key);

        return new ECDsaSecurityKey(algorithm);
    }

    private class CustomApplication : OpenIddictLinqToDBApplication<long> { }
    private class CustomAuthorization : OpenIddictLinqToDBAuthorization<long> { }
    private class CustomToken : OpenIddictLinqToDBToken<long> { }
}