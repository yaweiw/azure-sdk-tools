//------------------------------------------------------------------------------
// <copyright file="PIIValueAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Web.Hosting.Administration
{
    /// <summary>
    /// Marks a field as PII, so it won't be traced down to the logs
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class PIIValueAttribute : Attribute
    {
    }
}
