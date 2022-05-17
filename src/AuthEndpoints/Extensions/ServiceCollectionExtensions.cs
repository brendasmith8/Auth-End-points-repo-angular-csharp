﻿using AuthEndpoints.Models;
using AuthEndpoints.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace AuthEndpoints;

/// <summary>
/// Provides extensions to easily bootstrap authendpoints
/// </summary>
public static class ServiceCollectionExtensions
{
    internal static AuthEndpointsBuilder ConfigureServices<TUserKey, TUser>(IServiceCollection services)
        where TUserKey : IEquatable<TUserKey>
        where TUser : IdentityUser<TUserKey>
    {
        services.TryAddSingleton<IAccessTokenClaimsProvider<TUser>, AccessTokenClaimsProvider<TUserKey, TUser>>();
        services.TryAddSingleton<IRefreshTokenClaimsProvider<TUser>, RefreshTokenClaimsProvider<TUserKey, TUser>>();
        services.TryAddScoped<IAccessTokenGenerator<TUser>, AccessTokenGenerator<TUser>>();
        services.TryAddScoped<IRefreshTokenGenerator<TUser>, RefreshTokenGenerator<TUser>>();
        services.TryAddScoped<ITokenValidator, RefreshTokenValidator>();
        services.TryAddScoped<IAuthenticator<TUser, AuthenticatedUserResponse>, UserAuthenticator<TUser>>();

        services.TryAddScoped<IdentityErrorDescriber>();
        services.TryAddScoped<JwtSecurityTokenHandler>();

        return new AuthEndpointsBuilder(typeof(TUser), services);
    }

    /// <summary>
    /// Adds the AuthEndpoints core services
    /// </summary>
    /// <typeparam name="TUserKey"></typeparam>
    /// <typeparam name="TUser"></typeparam>
    /// <param name="services"></param>
    /// <returns>A <see cref="AuthEndpointsBuilder"/> for creating and configuring the AuthEndpoints system.</returns>
    public static AuthEndpointsBuilder AddAuthEndpoints<TUserKey, TUser>(this IServiceCollection services)
        where TUserKey : IEquatable<TUserKey>
        where TUser : IdentityUser<TUserKey>
    {
        services.AddOptions<AuthEndpointsOptions>();
        return services.AddAuthEndpoints<TUserKey, TUser>(o => { });
    }

    /// <summary>
    /// Adds and configures the AuthEndpoints system.
    /// </summary>
    /// <typeparam name="TUserKey">The type representing a User's primary key in the system.</typeparam>
    /// <typeparam name="TUser">The type representing a User in the system.</typeparam>
    /// <param name="services">The services available in the application.</param>
    /// <param name="setup">An action to configure the <see cref="AuthEndpointsOptions"/>.</param>
    /// <returns>A <see cref="AuthEndpointsBuilder"/> for creating and configuring the AuthEndpoints system.</returns>
    public static AuthEndpointsBuilder AddAuthEndpoints<TUserKey, TUser>(this IServiceCollection services, Action<AuthEndpointsOptions> setup)
        where TUserKey : IEquatable<TUserKey>
        where TUser : IdentityUser<TUserKey>
    {
        if (setup != null)
        {
            services.Configure(setup);
        }

        return ConfigureServices<TUserKey, TUser>(services);
    }

    /// <summary>
    /// Adds and configures the AuthEndpoints system.
    /// </summary>
    /// <typeparam name="TUserKey"></typeparam>
    /// <typeparam name="TUser"></typeparam>
    /// <param name="services"></param>
    /// <param name="customOptions"></param>
    /// <returns>A <see cref="AuthEndpointsBuilder"/> for creating and configuring the AuthEndpoints system.</returns>
    public static AuthEndpointsBuilder AddAuthEndpoints<TUserKey, TUser>(this IServiceCollection services, AuthEndpointsOptions customOptions)
        where TUserKey : IEquatable<TUserKey>
        where TUser : IdentityUser<TUserKey>
    {
        services.AddOptions<AuthEndpointsOptions>().Configure(options =>
        {
            options.AccessTokenSecret = customOptions.AccessTokenSecret;
            options.RefreshTokenSecret = customOptions.RefreshTokenSecret;
            options.AccessTokenExpirationMinutes = customOptions.AccessTokenExpirationMinutes;
            options.RefreshTokenExpirationMinutes = customOptions.RefreshTokenExpirationMinutes;
            options.Audience = customOptions.Audience;
            options.Issuer = customOptions.Issuer;
            options.AccessTokenValidationParameters = customOptions.AccessTokenValidationParameters;
            options.RefreshTokenValidationParameters = customOptions.RefreshTokenValidationParameters;
        });
        return ConfigureServices<TUserKey, TUser>(services);
    }
}
