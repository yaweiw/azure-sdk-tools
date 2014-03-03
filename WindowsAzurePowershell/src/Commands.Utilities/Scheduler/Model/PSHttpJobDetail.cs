using System;
using System.Collections.Generic;

namespace Microsoft.WindowsAzure.Commands.Utilities.Scheduler.Model
{
    public class PSHttpJobDetail : PSJobDetail
    {
        public string Method { get; internal set; }

        public Uri Uri { get; internal set; }

        public string Body { get; internal set; }

        public IDictionary<string, string> Headers { get; internal set; }

    }
}
