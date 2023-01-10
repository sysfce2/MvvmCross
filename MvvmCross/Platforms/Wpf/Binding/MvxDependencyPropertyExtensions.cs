// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MS-PL license.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace MvvmCross.Platforms.Wpf.Binding
{
    public static class MvxDependencyPropertyExtensions
    {
        [RequiresUnreferencedCode("In case the type is non-primitive, the trimmer cannot statically analyze the object's type so its members may be trimmed")]
        public static TypeConverter TypeConverter(
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]this Type type)
        {
            var typeConverter =
                type.GetCustomAttributes(typeof(TypeConverterAttribute), true).FirstOrDefault() as
                TypeConverterAttribute;
            if (typeConverter == null)
                return null;

            var converterType = Type.GetType(typeConverter.ConverterTypeName);
            if (converterType == null)
                return null;
            var converter = Activator.CreateInstance(converterType) as TypeConverter;

            return converter;
        }

        public static PropertyInfo FindActualProperty(
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]this Type type, string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            var property = type.GetProperty(name, BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance);
            return property;
        }

        public static FieldInfo FindDependencyPropertyInfo(
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]this Type type, string dependencyPropertyName)
        {
            if (string.IsNullOrEmpty(dependencyPropertyName))
                return null;

            if (!EnsureIsDependencyPropertyName(ref dependencyPropertyName))
                return null;

            var candidateType = type;
            while (candidateType != null)
            {
                var fieldInfo = candidateType.GetField(dependencyPropertyName, BindingFlags.Static | BindingFlags.Public);
                if (fieldInfo != null)
                    return fieldInfo;

                candidateType = candidateType.BaseType;
            }

            return null;
        }

        public static DependencyProperty FindDependencyProperty(
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]this Type type, string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            var propertyInfo = FindDependencyPropertyInfo(type, name);

            return propertyInfo?.GetValue(null) as DependencyProperty;
        }

        private static bool EnsureIsDependencyPropertyName(ref string dependencyPropertyName)
        {
            if (string.IsNullOrEmpty(dependencyPropertyName))
                return false;

            if (!dependencyPropertyName.EndsWith("Property"))
                dependencyPropertyName += "Property";
            return true;
        }
    }
}
