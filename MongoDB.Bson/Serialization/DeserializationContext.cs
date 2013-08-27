﻿/* Copyright 2010-2013 10gen Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using MongoDB.Bson.IO;

namespace MongoDB.Bson.Serialization
{
    /// <summary>
    /// Represents all the contextual information needed by a serializer to deserialize a value.
    /// </summary>
    public class DeserializationContext
    {
        // private fields
        private readonly bool _allowDuplicateElementNames;
        private readonly Type _nominalType;
        private readonly DeserializationContext _parent;
        private readonly BsonReader _reader;

        // constructors
        private DeserializationContext(
            DeserializationContext parent,
            BsonReader reader,
            Type nominalType,
            bool allowDuplicateElementNames)
        {
            _parent = parent;
            _reader = reader;
            _nominalType = nominalType;
            _allowDuplicateElementNames = allowDuplicateElementNames;
        }

        // public properties
        /// <summary>
        /// Gets a value indicating whether to allow duplicate element names.
        /// </summary>
        /// <value>
        /// <c>true</c> if duplicate element names shoud be allowed; otherwise, <c>false</c>.
        /// </value>
        public bool AllowDuplicateElementNames
        {
            get { return _allowDuplicateElementNames; }
        }

        /// <summary>
        /// Gets the nominal type.
        /// </summary>
        /// <value>
        /// The nominal type.
        /// </value>
        public Type NominalType
        {
            get { return _nominalType; }
        }

        /// <summary>
        /// Gets the parent context.
        /// </summary>
        /// <value>
        /// The parent context. The parent of the root context is null.
        /// </value>
        public DeserializationContext Parent
        {
            get { return _parent; }
        }

        /// <summary>
        /// Gets the reader.
        /// </summary>
        /// <value>
        /// The reader.
        /// </value>
        public BsonReader Reader
        {
            get { return _reader; }
        }

        // public static methods
        /// <summary>
        /// Creates a root context for the specified serializer.
        /// </summary>
        /// <typeparam name="TNominalType">The nominal type.</typeparam>
        /// <param name="reader">The reader.</param>
        /// <param name="serializer">The serializer.</param>
        /// <returns>A root context.</returns>
        public static DeserializationContext CreateRoot<TNominalType>(
            BsonReader reader,
            Action<Builder> configurator = null)
        {
            var builder = new Builder(null, reader, typeof(TNominalType));
            if (configurator != null)
            {
                configurator(builder);
            }
            return builder.Build();
        }

        /// <summary>
        /// Creates a root context for the specified serializer.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="serializer">The serializer.</param>
        /// <returns>A root context.</returns>
        public static DeserializationContext CreateRoot(
            BsonReader reader, 
            Type nominalType,
            Action<Builder> configurator = null)
        {
            var builder = new Builder(null, reader, nominalType);
            if (configurator != null)
            {
                configurator(builder);
            }
            return builder.Build();
        }

        // public methods
        /// <summary>
        /// Creates a child context for the specified serializer.
        /// </summary>
        /// <typeparam name="TNominalType">The nominal type.</typeparam>
        /// <param name="serializer">The serializer.</param>
        /// <returns>A child context.</returns>
        public DeserializationContext CreateChild<TNominalType>(
            Action<Builder> configurator = null)
        {
            var builder = new Builder(this, _reader, typeof(TNominalType));
            if (configurator != null)
            {
                configurator(builder);
            }
            return builder.Build();
        }

        /// <summary>
        /// Creates a child context for the specified serializer.
        /// </summary>
        /// <param name="serializer">The serializer.</param>
        /// <returns>A child context.</returns>
        public DeserializationContext CreateChild(
            Type nominalType,
            Action<Builder> configurator = null)
        {
            var builder = new Builder(this, _reader, nominalType);
            if (configurator != null)
            {
                configurator(builder);
            }
            return builder.Build();
        }

        /// <summary>
        /// Creates a child context and calls the Deserialize method of the serializer with it.
        /// </summary>
        /// <typeparam name="TNominalType">The nominal type.</typeparam>
        /// <param name="serializer">The serializer.</param>
        /// <returns>The deserialized value.</returns>
        public TNominalType DeserializeWithChildContext<TNominalType>(
            IBsonSerializer<TNominalType> serializer,
            Action<Builder> configurator = null)
        {
            var childContext = CreateChild<TNominalType>(configurator);
            return serializer.Deserialize(childContext);
        }

        /// <summary>
        /// Creates a child context and calls the Deserialize method of the serializer with it.
        /// </summary>
        /// <param name="serializer">The serializer.</param>
        /// <returns>The deserialized value.</returns>
        public object DeserializeWithChildContext(
            IBsonSerializer serializer,
            Action<Builder> configurator = null)
        {
            var childContext = CreateChild(serializer.ValueType, configurator);
            return serializer.Deserialize(childContext);
        }

        // nested classes
        public class Builder
        {
            // private fields
            private bool _allowDuplicateElementNames;
            private Type _nominalType;
            private DeserializationContext _parent;
            private BsonReader _reader;

            // constructors
            public Builder(DeserializationContext parent, BsonReader reader, Type nominalType)
            {
                if (reader == null)
                {
                    throw new ArgumentNullException("reader");
                }
                if (nominalType == null)
                {
                    throw new ArgumentNullException("nominalType");
                }

                _parent = parent;
                _reader = reader;
                _nominalType = nominalType;
                if (parent != null)
                {
                    _allowDuplicateElementNames = parent.AllowDuplicateElementNames;
                }
            }

            // properties
            public bool AllowDuplicateElementNames
            {
                get { return _allowDuplicateElementNames; }
                set { _allowDuplicateElementNames = value; }
            }

            public Type NominalType
            {
                get { return _nominalType; }
            }

            public DeserializationContext Parent
            {
                get { return _parent; }
            }

            public BsonReader Reader
            {
                get { return _reader; }
            }

            // public methods
            public DeserializationContext Build()
            {
                return new DeserializationContext(_parent, _reader, _nominalType, _allowDuplicateElementNames);
            }
        }
    }
}