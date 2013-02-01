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

namespace Microsoft.Samples.WindowsAzure.ServiceManagement.Utilities
{
    using System;
    using System.ServiceModel.Channels;
    using System.Text;
    using System.Xml;

    public class General
    {
        public static string ReadBody(ref Message originalMessage)
        {
            StringBuilder strBuilder = new StringBuilder();

            using (MessageBuffer messageBuffer = originalMessage.CreateBufferedCopy(int.MaxValue))
            {
                Message message = messageBuffer.CreateMessage();
                XmlWriter writer = XmlWriter.Create(strBuilder);
                using (XmlDictionaryWriter dictionaryWriter = XmlDictionaryWriter.CreateDictionaryWriter(writer))
                {
                    message.WriteBodyContents(dictionaryWriter);
                }

                originalMessage = messageBuffer.CreateMessage();
            }

            return Beautify(strBuilder.ToString());
        }

        /// <summary>
        /// Formats given string into well formatted XML.
        /// </summary>
        /// <param name="unformattedXml">The unformatted xml string</param>
        /// <returns>The formatted XML string</returns>
        public static string Beautify(string unformattedXml)
        {
            string formattedXml = string.Empty;
            if (!string.IsNullOrEmpty(unformattedXml))
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(unformattedXml);
                StringBuilder stringBuilder = new StringBuilder();
                XmlWriterSettings settings = new XmlWriterSettings()
                {
                    Indent = true,
                    IndentChars = "\t",
                    NewLineChars = Environment.NewLine,
                    NewLineHandling = NewLineHandling.Replace
                };
                using (XmlWriter writer = XmlWriter.Create(stringBuilder, settings))
                {
                    doc.Save(writer);
                }
                formattedXml = stringBuilder.ToString();
            }

            return formattedXml;
        }
    }
}
