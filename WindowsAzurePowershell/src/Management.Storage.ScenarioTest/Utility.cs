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

using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using MS.Test.Common.MsTestLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CLITest
{
    class Utility
    {
        /// <summary>
        /// Generate a random string for azure object name
        /// @prefix: usually it's a string of letters, to avoid naming rule breaking
        /// @len: the length of random string after the prefix
        /// </summary> 
        public static string GenNameString(string prefix, int len = 8)
        {
            return prefix + Guid.NewGuid().ToString().Replace("-", "").Substring(0, len);
        }

        public static List<string> GenNameLists(string prefix, int count = 1, int len = 8)
        {
            List<string> names = new List<string>();

            for (int i = 0; i < count; i++)
            {
                names.Add(Utility.GenNameString(prefix, len));
            }

            return names;
        }

        public static string GenConnectionString(string StorageAccountName, string StorageAccountKey)
        {
            return String.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", StorageAccountName, StorageAccountKey);
        }

        /// <summary>
        /// Generate the data for output comparision
        /// </summary> 
        public static Dictionary<string, object> GenComparisonData(StorageObjectType objType, string name)
        {
            Dictionary<string, object> dic = new Dictionary<string, object> { 
                {"Name", name },
                {"Context", null}
            };

            switch (objType)
            {
                case StorageObjectType.Container:
                    dic.Add("PublicAccess", BlobContainerPublicAccessType.Off);        // default value is Off
                    dic.Add("LastModified", null);
                    dic.Add("Permission", null);
                    break;
                case StorageObjectType.Blob:
                    dic.Add("BlobType", null);      // need to validate this later
                    dic.Add("Length", null);        // need to validate this later
                    dic.Add("ContentType", null);   // the return value of upload operation is always null
                    dic.Add("LastModified", null);  // need to validate this later
                    dic.Add("SnapshotTime", null);  // need to validate this later
                    break;
                case StorageObjectType.Queue:
                    dic.Add("ApproximateMessageCount", 0);
                    dic.Add("EncodeMessage", true);
                    break;
                case StorageObjectType.Table:
                    break;
                default:
                    throw new Exception(String.Format("Object type:{0} not identified!", objType));
            }

            return dic;
        }

        /// <summary>
        /// Generate the data for output comparision
        /// </summary> 
        public static string GenComparisonData(string FunctionName, bool Success)
        {
            return String.Format("{0} operation should {1}.", FunctionName, Success ? "succeed" : "fail");
        }

        /// <summary>
        /// Compare two entities, usually one from XSCL, one from PowerShell
        /// </summary> 
        public static bool CompareEntity<T>(T v1, T v2)
        {
            bool bResult = true;

            if (v1 == null || v2 == null)
            {
                if (v1 == null && v2 == null)
                {
                    Test.Info("Skip compare null objects");
                    return true;
                }
                else
                {
                    Test.AssertFail(string.Format("v1 is {0}, but v2 is {1}", v1, v2));
                    return false;
                }
            }
            
            foreach (var propertyInfo in typeof(T).GetProperties())
            {
                if (propertyInfo.Name.Equals("ServiceClient"))
                    continue;

                object o1 = null;
                object o2 = null;

                try
                {
                    o1 = propertyInfo.GetValue(v1, null);
                    o2 = propertyInfo.GetValue(v2, null);
                }
                catch
                { 
                    //skip the comparison when throw exception
                    string msg = string.Format("Skip compare '{0}' property in type {1}", propertyInfo.Name, typeof(T));
                    Trace.WriteLine(msg);
                    Test.Warn(msg);
                    continue;
                }

                if (propertyInfo.Name.Equals("Metadata"))
                {
                    if (v1.GetType() == typeof(CloudBlobContainer) 
                        || v1.GetType() == typeof(CloudBlockBlob)
                        || v1.GetType() == typeof(CloudPageBlob)
                        || v1.GetType() == typeof(CloudQueue)
                        || v1.GetType() == typeof(CloudTable))
                    {
                        bResult = ((IDictionary<string, string>)o1).SequenceEqual((IDictionary<string, string>)o2);
                    }
                    else
                    {
                        bResult = o1.Equals(o2);
                    }
                }
                else if (propertyInfo.Name.Equals("Properties"))
                {
                    if (v1.GetType() == typeof(CloudBlockBlob)
                        || v1.GetType() == typeof(CloudPageBlob))
                    {
                        bResult = CompareEntity((BlobProperties)o1, (BlobProperties)o2);
                    }
                    else if (v1.GetType() == typeof(CloudBlobContainer))
                    {
                        bResult = CompareEntity((BlobContainerProperties)o1, (BlobContainerProperties)o2);
                    }
                }
                else if (propertyInfo.Name.Equals("SharedAccessPolicies"))
                {
                    if (v1.GetType() == typeof(BlobContainerPermissions))
                    {
                        bResult = CompareEntity((SharedAccessBlobPolicies)o1, (SharedAccessBlobPolicies)o2);
                    }
                    else
                    {
                        bResult = o1.Equals(o2);
                    }
                }
                else
                {
                    if (o1 == null)
                    {
                        if (o2 != null)
                            bResult = false;
                    }
                    else
                    {
                        //compare according to type
                        if (o1 is ICollection<string>)
                        {
                            bResult = ((ICollection<string>)o1).SequenceEqual((ICollection<string>)o2);
                        }
                        else if (o1 is ICollection<SharedAccessBlobPolicy>)
                        {
                            bResult = CompareEntity((ICollection<SharedAccessBlobPolicy>)o1, (ICollection<SharedAccessBlobPolicy>)o2);
                        }
                        else
                        {
                            bResult = o1.Equals(o2);
                        }
                    }
                }

                if (bResult == false)
                {
                    Test.Error("Property Mismatch: {0} in type {1}. {2} != {3}", propertyInfo.Name, typeof(T), o1, o2);
                    break;
                }
                else
                {
                    Test.Verbose("Property {0} in type {1}: {2} == {3}", propertyInfo.Name, typeof(T), o1, o2);
                }
            }
            return bResult;
        }
    }
}
