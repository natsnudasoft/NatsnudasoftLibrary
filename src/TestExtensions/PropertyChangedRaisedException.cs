﻿// <copyright file="PropertyChangedRaisedException.cs" company="natsnudasoft">
// Copyright (c) Adrian John Dunstan. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

namespace Natsnudasoft.NatsnudaLibrary.TestExtensions
{
    using System;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using static System.FormattableString;

    /// <summary>
    /// The exception that is thrown when a property does not raise the PropertyChanged event.
    /// </summary>
    /// <seealso cref="Exception" />
    /// <seealso cref="PropertyChangedRaisedAssertion"/>
    [Serializable]
    public sealed class PropertyChangedRaisedException : Exception
    {
        private const string PropertyInfoTypeName = nameof(PropertyInfo) + "TypeName";
        private const string PropertyInfoMemberName = nameof(PropertyInfo) + "MemberName";
        private const string DefaultMessage = "A property did not raise the PropertyChanged event.";

        [NonSerialized]
        private readonly PropertyInfo propertyInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyChangedRaisedException"/> class.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Naming",
            "CA2204:Literals should be spelled correctly",
            MessageId = "PropertyChanged",
            Justification = "Name of event.")]
        public PropertyChangedRaisedException()
            : base(DefaultMessage)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyChangedRaisedException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.
        /// </param>
        public PropertyChangedRaisedException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyChangedRaisedException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.
        /// </param>
        /// <param name="innerException">The exception that is the cause of the current exception.
        /// </param>
        public PropertyChangedRaisedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyChangedRaisedException"/> class.
        /// </summary>
        /// <param name="propertyInfo">The property information for the property that did correctly
        /// raise the PropertyChanged event.</param>
        public PropertyChangedRaisedException(PropertyInfo propertyInfo)
            : base(CreateMessage(propertyInfo))
        {
            this.propertyInfo = propertyInfo;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyChangedRaisedException"/> class.
        /// </summary>
        /// <param name="propertyInfo">The property information for the property that did correctly
        /// raise the PropertyChanged event.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.
        /// </param>
        public PropertyChangedRaisedException(
            PropertyInfo propertyInfo,
            Exception innerException)
            : base(CreateMessage(propertyInfo), innerException)
        {
            this.propertyInfo = propertyInfo;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        private PropertyChangedRaisedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            var typeName = info.GetString(PropertyInfoTypeName);
            var memberName = info.GetString(PropertyInfoMemberName);
            const BindingFlags bindingFlags = BindingFlags.Static |
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic;
            this.propertyInfo = Type.GetType(typeName).GetProperty(memberName, bindingFlags);
        }

        /// <summary>
        /// Gets the property information for the property that did correctly raise the
        /// PropertyChanged event.
        /// </summary>
        public PropertyInfo PropertyInfo => this.propertyInfo;

        /// <inheritdoc/>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(PropertyInfoTypeName, this.PropertyInfo.DeclaringType.FullName);
            info.AddValue(PropertyInfoMemberName, this.PropertyInfo.Name);
        }

        private static string CreateMessage(PropertyInfo propertyInfo)
        {
            return Invariant(
                $"Property \'{propertyInfo.Name}\' did not raise the PropertyChanged event.");
        }
    }
}