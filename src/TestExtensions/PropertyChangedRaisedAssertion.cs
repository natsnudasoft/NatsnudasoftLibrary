﻿// <copyright file="PropertyChangedRaisedAssertion.cs" company="natsnudasoft">
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
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reflection;
    using NatsnudaLibrary;
    using Ploeh.AutoFixture.Idioms;
    using Ploeh.AutoFixture.Kernel;

    /// <summary>
    /// Encapsulates a unit test that verifies whether properties raise a PropertyChanged event on
    /// their reflected owner type.
    /// </summary>
    /// <seealso cref="IdiomaticAssertion" />
    public sealed class PropertyChangedRaisedAssertion : IdiomaticAssertion
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyChangedRaisedAssertion"/> class.
        /// </summary>
        /// <param name="specimenBuilder">The anonymous object creation service to use to create new
        /// specimens.</param>
        /// <exception cref="ArgumentNullException"><paramref name="specimenBuilder"/> is
        /// <see langword="null"/>.</exception>
        public PropertyChangedRaisedAssertion(ISpecimenBuilder specimenBuilder)
        {
            ParameterValidation.IsNotNull(specimenBuilder, nameof(specimenBuilder));

            this.SpecimenBuilder = specimenBuilder;
        }

        /// <summary>
        /// Gets the anonymous object creation service.
        /// </summary>
        public ISpecimenBuilder SpecimenBuilder { get; }

        /// <summary>
        /// Verifies that the specified property raises the PropertyChanged event.
        /// </summary>
        /// <param name="propertyInfo">The property.</param>
        /// <remarks>
        /// This method does nothing if the property does not have a public set method, or if no
        /// public PropertyChanged event with the correct signature was found.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="propertyInfo"/> is
        /// <see langword="null"/>.</exception>
        /// <exception cref="PropertyChangedRaisedException">The specified property did not raise
        /// the PropertyChanged event.</exception>
        public override void Verify(PropertyInfo propertyInfo)
        {
            ParameterValidation.IsNotNull(propertyInfo, nameof(propertyInfo));

            var propertyChangedEventInfo = propertyInfo.ReflectedType.GetEvent("PropertyChanged");

            if (propertyChangedEventInfo != null && propertyInfo.GetSetMethod() != null &&
                propertyChangedEventInfo.EventHandlerType == typeof(PropertyChangedEventHandler) &&
                propertyChangedEventInfo.GetAddMethod() != null &&
                propertyChangedEventInfo.GetRemoveMethod() != null)
            {
                var receivedEvents = new List<string>();
                Action<object, PropertyChangedEventArgs> handler = (sender, e) =>
                {
                    receivedEvents.Add(e.PropertyName);
                };
                var handlerDelegate = Delegate.CreateDelegate(
                    propertyChangedEventInfo.EventHandlerType,
                    handler.Target,
                    handler.Method);
                var context = new SpecimenContext(this.SpecimenBuilder);
                var specimen = context.Resolve(propertyInfo.ReflectedType);
                var initialPropertyValue = propertyInfo.GetValue(specimen);
                object newPropertyValue;
                do
                {
                    newPropertyValue = context.Resolve(propertyInfo.PropertyType);
                }
                while (newPropertyValue == initialPropertyValue);

                try
                {
                    propertyChangedEventInfo.AddEventHandler(specimen, handlerDelegate);
                    propertyInfo.SetValue(specimen, newPropertyValue);
                }
                finally
                {
                    propertyChangedEventInfo.RemoveEventHandler(specimen, handlerDelegate);
                }

                if (receivedEvents.Count != 1 || receivedEvents[0] != propertyInfo.Name)
                {
                    throw new PropertyChangedRaisedException(propertyInfo);
                }
            }
        }
    }
}
