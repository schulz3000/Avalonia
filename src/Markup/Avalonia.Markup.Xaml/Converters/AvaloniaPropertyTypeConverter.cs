// Copyright (c) The Avalonia Project. All rights reserved.
// Licensed under the MIT license. See licence.md file in the project root for full license information.

using System;
using System.Globalization;
using System.Linq;

namespace Avalonia.Markup.Xaml.Converters
{
    using Avalonia.Styling;
    using Portable.Xaml;
#if !OMNIXAML

    using Portable.Xaml.ComponentModel;
    using Portable.Xaml.Markup;

    public class AvaloniaPropertyTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var s = (string)value;

            string typeName;
            string propertyName;
            Type type = null;

            ParseProperty(s, out typeName, out propertyName);

            if (typeName == null)
            {
                var amb = context.GetService<IAmbientProvider>();
                var sc = context.GetService<IXamlSchemaContextProvider>().SchemaContext;
                var xamlStyleType = sc.GetXamlType(typeof(Style));
                var ambValue = amb.GetFirstAmbientValue(xamlStyleType) as AmbientPropertyValue;

                var style = ambValue?.Value as Style;
                type = style.Selector?.TargetType;

                if (type == null)
                {
                    throw new Exception(
                        "Could not determine the target type. Please fully qualify the property name.");
                }
            }
            else
            {
                //type = context.TypeRepository.GetByQualifiedName(typeName)?.UnderlyingType;

                if (type == null)
                {
                    throw new Exception($"Could not find type '{typeName}'.");
                }
            }

            // First look for non-attached property on the type and then look for an attached property.
            var property = AvaloniaPropertyRegistry.Instance.FindRegistered(type, s) ??
                           AvaloniaPropertyRegistry.Instance.GetAttached(type)
                           .FirstOrDefault(x => x.Name == propertyName);

            if (property == null)
            {
                throw new Exception(
                    $"Could not find AvaloniaProperty '{type.Name}.{propertyName}'.");
            }

            return property;
        }

        private void ParseProperty(string s, out string typeName, out string propertyName)
        {
            var split = s.Split('.');

            if (split.Length == 1)
            {
                typeName = null;
                propertyName = split[0];
            }
            else if (split.Length == 2)
            {
                typeName = split[0];
                propertyName = split[1];
            }
            else
            {
                throw new Exception($"Invalid property name: '{s}'.");
            }
        }
    }

#else

    using OmniXaml;
    using OmniXaml.TypeConversion;

    public class AvaloniaPropertyTypeConverter : ITypeConverter
    {
        public bool CanConvertFrom(IValueContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public bool CanConvertTo(IValueContext context, Type destinationType)
        {
            return false;
        }

        public object ConvertFrom(IValueContext context, CultureInfo culture, object value)
        {
            var s = (string)value;

            string typeName;
            string propertyName;
            Type type;

            ParseProperty(s, out typeName, out propertyName);

            if (typeName == null)
            {
                var styleType = context.TypeRepository.GetByType(typeof(Style));
                var style = (Style)context.TopDownValueContext.GetLastInstance(styleType);
                type = style.Selector?.TargetType;

                if (type == null)
                {
                    throw new ParseException(
                        "Could not determine the target type. Please fully qualify the property name.");
                }
            }
            else
            {
                type = context.TypeRepository.GetByQualifiedName(typeName)?.UnderlyingType;

                if (type == null)
                {
                    throw new ParseException($"Could not find type '{typeName}'.");
                }
            }

            // First look for non-attached property on the type and then look for an attached property.
            var property = AvaloniaPropertyRegistry.Instance.FindRegistered(type, s) ??
                           AvaloniaPropertyRegistry.Instance.GetAttached(type)
                           .FirstOrDefault(x => x.Name == propertyName);

            if (property == null)
            {
                throw new ParseException(
                    $"Could not find AvaloniaProperty '{type.Name}.{propertyName}'.");
            }

            return property;
        }

        public object ConvertTo(IValueContext context, CultureInfo culture, object value, Type destinationType)
        {
            throw new NotImplementedException();
        }

        private void ParseProperty(string s, out string typeName, out string propertyName)
        {
            var split = s.Split('.');

            if (split.Length == 1)
            {
                typeName = null;
                propertyName = split[0];
            }
            else if (split.Length == 2)
            {
                typeName = split[0];
                propertyName = split[1];
            }
            else
            {
                throw new ParseException($"Invalid property name: '{s}'.");
            }
        }
    }
#endif
}