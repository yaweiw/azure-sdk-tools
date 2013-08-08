// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

using System;
using System.Management.Automation;
using System.Net;
using Microsoft.WindowsAzure.Management.Utilities.MediaService;
using Microsoft.WindowsAzure.Management.Utilities.MediaService.Services;
using Microsoft.WindowsAzure.Management.Utilities.MediaService.Services.MediaServicesEntities;
using Microsoft.WindowsAzure.Management.Utilities.Properties;

namespace Microsoft.WindowsAzure.Management.MediaService
{
    public enum KeyType
    {
        Primary,
        Secondary
    }

    /// <summary>
    ///     Gets an azure website.
    /// </summary>
    [Cmdlet(VerbsCommon.New, "AzureMediaServiceKey", SupportsShouldProcess = true), OutputType(typeof (string))]
    public class NewAzureMediaServiceKeyCommand : MediaServiceBaseCmdlet
    {
        /// <summary>
        ///     Initializes a new instance of the NewAzureMediaServicesKeyCommand class.
        /// </summary>
        public NewAzureMediaServiceKeyCommand() : this(null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the NewAzureMediaServicesKeyCommand class.
        /// </summary>
        /// <param name="channel">
        ///     Channel used for communication with Azure's service management APIs.
        /// </param>
        public NewAzureMediaServiceKeyCommand(IMediaServiceManagement channel)
        {
            Channel = channel;
        }

        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The media services account name.")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The media services key type <Primary|Secondary>.")]
        [ValidateNotNullOrEmpty]
        public KeyType KeyType { get; set; }

        [Parameter(Position = 2, HelpMessage = "Do not confirm regeneration of the key.")]
        public SwitchParameter Force { get; set; }

        public IMediaServicesClient MediaServicesClient { get; set; }

        public override void ExecuteCmdlet()
        {
            ConfirmAction(Force.IsPresent,
                          string.Format(Resources.RegenerateKeyWarning),
                          Resources.RegenerateKeyWhatIfMessage,
                          string.Empty,
                          () =>
                              {
                                  InvokeInOperationContext(() =>
                                      {
                                          RetryCall(s =>
                                              {
                                                  try
                                                  {
                                                      Channel.RegenerateMediaServicesAccount(s, Name, KeyType.ToString());
                                                  }
                                                  catch (Exception x)
                                                  {
                                                      var webx = x.InnerException as WebException;
                                                      if (webx != null && ((HttpWebResponse) webx.Response).StatusCode == HttpStatusCode.NotFound)
                                                      {
                                                          throw new Exception(string.Format(Resources.InvalidMediaServicesAccount, Name));
                                                      }
                                                      else
                                                      {
                                                          throw;
                                                      }
                                                  }

                                                  MediaServiceAccountDetails details = Channel.GetMediaService(s, Name);
                                                  string result = KeyType == KeyType.Primary ? details.AccountKeys.Primary : details.AccountKeys.Secondary;
                                                  WriteObject(result);
                                              });
                                      });
                              });
        }
    }
}