﻿using System.Security.Claims;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;

namespace OpenIddict.Extensions;

/// <summary>
/// Exposes common helpers used by the OpenIddict assemblies.
/// </summary>
internal static class OpenIddictHelpers
{
    /// <summary>
    /// Finds the first base type that matches the specified generic type definition.
    /// </summary>
    /// <param name="type">The type to introspect.</param>
    /// <param name="definition">The generic type definition.</param>
    /// <returns>A <see cref="Type"/> instance if the base type was found, <see langword="null"/> otherwise.</returns>
    public static Type? FindGenericBaseType(Type type, Type definition)
        => FindGenericBaseTypes(type, definition).FirstOrDefault();

    /// <summary>
    /// Finds all the base types that matches the specified generic type definition.
    /// </summary>
    /// <param name="type">The type to introspect.</param>
    /// <param name="definition">The generic type definition.</param>
    /// <returns>A <see cref="Type"/> instance if the base type was found, <see langword="null"/> otherwise.</returns>
    public static IEnumerable<Type> FindGenericBaseTypes(Type type, Type definition)
    {
        if (type is null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        if (definition is null)
        {
            throw new ArgumentNullException(nameof(definition));
        }

        if (!definition.IsGenericTypeDefinition)
        {
            throw new ArgumentException(SR.GetResourceString(SR.ID0263), nameof(definition));
        }

        if (definition.IsInterface)
        {
            foreach (var contract in type.GetInterfaces())
            {
                if (!contract.IsGenericType && !contract.IsConstructedGenericType)
                {
                    continue;
                }

                if (contract.GetGenericTypeDefinition() == definition)
                {
                    yield return contract;
                }
            }
        }

        else
        {
            for (var candidate = type; candidate is not null; candidate = candidate.BaseType)
            {
                if (!candidate.IsGenericType && !candidate.IsConstructedGenericType)
                {
                    continue;
                }

                if (candidate.GetGenericTypeDefinition() == definition)
                {
                    yield return candidate;
                }
            }
        }
    }

}
