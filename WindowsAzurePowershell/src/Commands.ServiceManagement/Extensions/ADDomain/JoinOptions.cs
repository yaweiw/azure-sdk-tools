using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.Extensions
{
    /// <summary>
    /// Options for joining a computer to a domain
    /// </summary>
    [Flags]
    public enum JoinOptions : uint
    {
        /// <summary>
        /// Joins the computer to a domain. If this value is not specified, joins the computer to a workgroup.
        /// </summary>
        JoinDomain = 0x1,

        /// <summary>
        /// Create account on the domain
        /// </summary>
        AccountCreate = 0x2,

        /// <summary>
        /// Join operation is part of an upgrade
        /// </summary>
        Win9XUpgrade = 0x10,

        /// <summary>
        /// Perform an unsecure join
        /// </summary>
        UnsecuredJoin = 0x40,

        /// <summary>
        /// Indicate that the password passed to the join operation is the local machine account password, not a user password.
        /// It's valid only for unsecure join
        /// </summary>
        PasswordPass = 0x80,

        /// <summary>
        /// Writing SPN and DNSHostName attributes on the computer object should be deferred until the rename operation that
        /// follows the join operation
        /// </summary>
        DeferSPNSet = 0x100,

        /// <summary>
        /// Join the target machine with a new name queried from the registry. This options is used if the rename has been called prior
        /// to rebooting the machine
        /// </summary>
        JoinWithNewName = 0x400,

        /// <summary>
        /// Use a readonly domain controller
        /// </summary>
        JoinReadOnly = 0x800,

        /// <summary>
        /// Invoke during insatll
        /// </summary>
        InstallInvoke = 0x40000
    }
}
