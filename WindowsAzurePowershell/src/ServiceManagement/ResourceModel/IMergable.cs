// ----------------------------------------------------------------------------------
//
// Copyright 2011 Microsoft Corporation
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

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization;

    /// <summary>
    /// Custom Interface for DataType to provide type-specific logic for resolving derived type.
    /// </summary>
    public interface IResolvable
    {
        object ResolveType();
    }

    public interface IMergable
    {
        void Merge(object other);
    }

    public interface IMergable<T> : IMergable
    {
        void Merge(T other);
    }

    /// <summary>
    /// Generic Mergable implementation with programmability helper to define API contract.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DataContract]
    public abstract class Mergable<T> : IResolvable, IMergable<T>, IExtensibleDataObject where T : Mergable<T>
    {
        #region Field backing store
        private Dictionary<string, object> propertyStore;

        private Dictionary<string, object> PropertyStore
        {
            get
            {
                if (this.propertyStore == null)
                {
                    this.propertyStore = new Dictionary<string, object>();
                }

                return this.propertyStore;
            }
        }
        #endregion

        protected TValue GetValue<TValue>(string fieldName)
        {
            object value;

            if (this.PropertyStore.TryGetValue(fieldName, out value))
            {
                return (TValue)value;
            }

            return default(TValue);
        }

        protected void SetValue<TValue>(string fieldName, TValue value)
        {
            this.PropertyStore[fieldName] = value;
        }

        /// <summary>
        /// Helper to act as a backing store for value Type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        protected Nullable<TValue> GetField<TValue>(string fieldName) where TValue : struct
        {
            object value;
            if (this.PropertyStore.TryGetValue(fieldName, out value))
            {
                return new Nullable<TValue>((TValue)value);
            }
            else
            {
                return new Nullable<TValue>();
            }
        }

        /// <summary>
        /// Helper to act as backing store for value type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        protected void SetField<TValue>(string fieldName, Nullable<TValue> value) where TValue : struct
        {
            if (value.HasValue)
            {
                this.PropertyStore[fieldName] = value.Value;
            }
        }

        #region IResolvable Members

        public virtual object ResolveType()
        {
            return this;
        }

        #endregion

        protected TValue Convert<TValue>()
        {
            DataContractSerializer sourceSerializer = new DataContractSerializer(this.GetType());
            DataContractSerializer destinationSerializer = new DataContractSerializer(typeof(TValue));

            MemoryStream stream = new MemoryStream();
            sourceSerializer.WriteObject(stream, this);
            stream.Position = 0;
            return (TValue)destinationSerializer.ReadObject(stream);
        }

        #region IMergable Members

        void IMergable.Merge(object other)
        {
            ((IMergable<T>)this).Merge((T)other);
        }

        #endregion

        #region IMergable<T> members
        void IMergable<T>.Merge(T other)
        {
            Mergable<T> otherObject = (Mergable<T>)other;

            foreach (KeyValuePair<string, object> kvPair in otherObject.PropertyStore)
            {
                object currentValue;

                if (this.PropertyStore.TryGetValue(kvPair.Key, out currentValue))
                {
                    IMergable mergableValue = currentValue as IMergable;

                    if (mergableValue != null)
                    {
                        mergableValue.Merge(kvPair.Value);
                        continue;
                    }
                }

                this.PropertyStore[kvPair.Key] = kvPair.Value;
            }
        }
        #endregion

        #region IExtensibleDataObject Members

        ExtensionDataObject IExtensibleDataObject.ExtensionData
        {
            get;
            set;
        }

        #endregion
    }
}