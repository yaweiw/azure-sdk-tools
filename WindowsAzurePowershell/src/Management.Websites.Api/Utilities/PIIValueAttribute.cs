//------------------------------------------------------------------------------
// <copyright file="PIIValueAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Management.Websites.Api.Utilities
{
    using System;

    /// <summary>
    /// Marks a field as PII, so it won't be traced down to the logs
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class PIIValueAttribute : Attribute
    {
    }
}
