﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading;
using Microsoft.OpenApi.Any;
using RSql4Net.Models.Queries.Exceptions;

namespace RSql4Net.Models.Queries
{
    public static class RSqlQueryGetValueHelper
    {
        private static bool IsBool(Type type) => type == typeof(bool) || type == typeof(bool?);
        private static bool IsShort(Type type) => type == typeof(short) || type == typeof(short?);
        private static bool IsInt(Type type) => type == typeof(int) || type == typeof(int?);
        private static bool IsLong(Type type) => type == typeof(long) || type == typeof(long?);
        private static bool IsFloat(Type type) => type == typeof(float) || type == typeof(float?);
        private static bool IsDecimal(Type type) => type == typeof(decimal) || type == typeof(decimal?);
        private static bool IsDouble(Type type) => type == typeof(double) || type == typeof(double?);
        private static bool IsChar(Type type) => type == typeof(char) || type == typeof(char?);
        private static bool IsByte(Type type) => type == typeof(byte) || type == typeof(byte?);
        private static bool IsDateTime(Type type) => type == typeof(DateTime) || type == typeof(DateTime?);
        private static bool IsDateTimeOffset(Type type) => type == typeof(DateTimeOffset) || type == typeof(DateTimeOffset?);
        private static bool IsGuid(Type type) =>  type == typeof(Guid) || type == typeof(Guid?);
        private static bool IsNumeric(Type type) => IsShort(type) || IsInt(type) || IsLong(type) || IsByte(type) || IsFloat(type) || IsDecimal(type) || IsDouble(type);
        private static bool IsTemporal(Type type) => IsDateTime(type) || IsDateTimeOffset(type);
        private static bool IsAlphabetic(Type type) => type == typeof(string) || IsChar(type);
        
        private static bool IsEnum(Type type) =>
            type.IsEnum || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) &&
                            type.GetGenericArguments()[0].IsEnum);

        private static readonly char DecimalSeparator =
            Convert.ToChar(Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator);

        private static List<object> GetStrings(RSqlQueryParser.ArgumentsContext argumentsContext)
        {
            var items = new List<object>();
            foreach (var valueContext in argumentsContext.value())
            {
                if (valueContext.single_quote() != null || valueContext.double_quote() != null)
                {
                    var replace = valueContext.single_quote() != null ? "'" : "\"";
                    var value = valueContext.GetText();
                    if (value.Length == 2)
                    {
                        items.Add(string.Empty);
                    }

                    items.Add(value.Substring(1, value.Length - 2).Replace("\\" + replace, replace));
                }
                else
                {
                    items.Add(valueContext.GetText());
                }
            }

            return items;
        }

        private static List<object> GetShorts(RSqlQueryParser.ArgumentsContext argumentsContext)
        {
            var items = new List<object>();
            foreach (var valueContext in argumentsContext.value())
            {
                items.Add(short.Parse(valueContext.GetText()));
            }

            return items;
        }

        private static List<object> GetInts(RSqlQueryParser.ArgumentsContext argumentsContext)
        {
            var items = new List<object>();
            foreach (var valueContext in argumentsContext.value())
            {
                items.Add(int.Parse(valueContext.GetText()));
            }

            return items;
        }

        private static List<object> GetLongs(RSqlQueryParser.ArgumentsContext argumentsContext)
        {
            var items = new List<object>();
            foreach (var valueContext in argumentsContext.value())
            {
                items.Add(long.Parse(valueContext.GetText()));
            }

            return items;
        }

        private static List<object> GetDoubles(RSqlQueryParser.ArgumentsContext argumentsContext)
        {
            var items = new List<object>();
            foreach (var valueContext in argumentsContext.value())
            {
                items.Add(double.Parse(
                    valueContext.GetText().Replace('.', DecimalSeparator).Replace(',', DecimalSeparator),
                    CultureInfo.InvariantCulture));
            }

            return items;
        }

        private static List<object> GetFloats(RSqlQueryParser.ArgumentsContext argumentsContext)
        {
            var items = new List<object>();
            foreach (var valueContext in argumentsContext.value())
            {
                items.Add(float.Parse(valueContext.GetText(), CultureInfo.InvariantCulture));
            }

            return items;
        }

        private static List<object> GetDecimals(RSqlQueryParser.ArgumentsContext argumentsContext)
        {
            var items = new List<object>();
            foreach (var valueContext in argumentsContext.value())
            {
                items.Add(decimal.Parse(valueContext.GetText(), CultureInfo.InvariantCulture));
            }

            return items;
        }

        private static List<object> GetDateTimes(RSqlQueryParser.ArgumentsContext argumentsContext)
        {
            var items = new List<object>();
            foreach (var valueContext in argumentsContext.value())
            {
                items.Add(DateTime.Parse(valueContext.GetText(), CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind));
            }

            return items;
        }

        private static List<object> GetDateTimeOffsets(RSqlQueryParser.ArgumentsContext argumentsContext)
        {
            var items = new List<object>();
            foreach (var valueContext in argumentsContext.value())
            {
                items.Add(DateTimeOffset.Parse(valueContext.GetText(), CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind));
            }

            return items;
        }

        private static List<object> GetBooleans(RSqlQueryParser.ArgumentsContext argumentsContext)
        {
            var items = new List<object>();
            foreach (var valueContext in argumentsContext.value())
            {
                items.Add(bool.Parse(valueContext.GetText()));
            }

            return items;
        }

        private static List<object> GetChars(RSqlQueryParser.ArgumentsContext argumentsContext)
        {
            var items = new List<object>();
            foreach (var valueContext in argumentsContext.value())
            {
                items.Add(char.Parse(valueContext.GetText()));
            }

            return items;
        }

        private static List<object> GetGuids(RSqlQueryParser.ArgumentsContext argumentsContext)
        {
            var items = new List<object>();
            foreach (var valueContext in argumentsContext.value())
            {
                items.Add(Guid.Parse(valueContext.GetText()));
            }

            return items;
        }

        private static List<object> GetBytes(RSqlQueryParser.ArgumentsContext argumentsContext)
        {
            var items = new List<object>();
            foreach (var valueContext in argumentsContext.value())
            {
                items.Add(byte.Parse(valueContext.GetText()));
            }

            return items;
        }

        private static List<object> GetEnums(RSqlQueryParser.ArgumentsContext argumentsContext, Type type)
        {
            var enumType = type.IsGenericType ? type.GetGenericArguments()[0] : type;
            return argumentsContext.value()
                .Select(valueContext =>
                    {
                        try
                        {
                            return Enum.Parse(enumType, valueContext.GetText(), true);
                        }
                        catch (Exception e)
                        {
                            throw new InvalidConversionException(valueContext, enumType, e);
                        }
                    }
                )
                .ToList();
        }

        public static List<object> GetNumerics(Type type, RSqlQueryParser.ArgumentsContext argumentsContext)
        {
            if (IsByte(type))
            {
                return GetBytes(argumentsContext);
            }
            
            if (IsShort(type))
            {
                return GetShorts(argumentsContext);
            }

            if (IsLong(type))
            {
                return GetLongs(argumentsContext);
            }

            if (IsFloat(type))
            {
                return GetFloats(argumentsContext);
            }

            if (IsDouble(type))
            {
                return GetDoubles(argumentsContext);
            }

            if (IsDecimal(type))
            {
                return GetDecimals(argumentsContext);
            }
            return GetInts(argumentsContext);
        }

        public static List<object> GetTemporals(Type type, RSqlQueryParser.ArgumentsContext argumentsContext)
        {
            if (IsDateTimeOffset(type))
            {
                return GetDateTimeOffsets(argumentsContext);
            }
            return GetDateTimes(argumentsContext);
        }

        public static List<object> GetAlphabetics(Type type, RSqlQueryParser.ArgumentsContext argumentsContext)
        {
            if (IsChar(type))
            {
                return GetChars(argumentsContext);
            }
            return GetStrings(argumentsContext);
        }

        public static List<object> GetValues(Type type, RSqlQueryParser.ArgumentsContext argumentsContext)
        {
            if (argumentsContext?.value() == null || argumentsContext.value().Length == 0)
            {
                return new List<object>();
            }

            if (IsNumeric(type))
            {
                return GetNumerics(type, argumentsContext);
            }
            
            if (IsAlphabetic(type))
            {
                return GetAlphabetics(type, argumentsContext);
            }

            if (IsTemporal(type))
            {
                return GetTemporals(type, argumentsContext);
            }

            if (IsBool(type))
            {
                return GetBooleans(argumentsContext);
            }

            if (IsGuid(type))
            {
                return GetGuids(argumentsContext);
            }

            return IsEnum(type) ? GetEnums(argumentsContext, type) : new List<object>();
        }

        private static void CheckParameters(ParameterExpression parameter, ExpressionValue expressionValue, RSqlQueryParser.ComparisonContext context)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            if (expressionValue == null)
            {
                throw new ArgumentNullException(nameof(expressionValue));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
        }

        public static object GetAlphabetic<T>(ParameterExpression parameter, ExpressionValue expressionValue,
            RSqlQueryParser.ComparisonContext context,
            JsonNamingPolicy jsonNamingPolicy = null)
        {
            if (IsChar(expressionValue.Property.PropertyType))
            {
                return GetChar<T>(parameter, context.arguments().value()[0], jsonNamingPolicy);
            }
            
            return GetString<T>(parameter, context.arguments().value()[0], jsonNamingPolicy);
        }

        public static object GetTemporal<T>(ParameterExpression parameter, ExpressionValue expressionValue,
            RSqlQueryParser.ComparisonContext context,
            JsonNamingPolicy jsonNamingPolicy = null)
        { 
            if (IsDateTimeOffset(expressionValue.Property.PropertyType))
            {
                return GetDateTimeOffset<T>(parameter, context.arguments().value()[0], jsonNamingPolicy);
            }
            
            return GetDateTime<T>(parameter, context.arguments().value()[0], jsonNamingPolicy);
        }

        public static object GetNumeric<T>(ParameterExpression parameter, ExpressionValue expressionValue,
            RSqlQueryParser.ComparisonContext context,
            JsonNamingPolicy jsonNamingPolicy = null)
        {
            if (IsShort(expressionValue.Property.PropertyType))
            {
                return GetShort<T>(parameter, context.arguments().value()[0], jsonNamingPolicy);
            }
            
            if (IsLong(expressionValue.Property.PropertyType))
            {
                return GetLong<T>(parameter, context.arguments().value()[0], jsonNamingPolicy);
            }

            if (IsFloat(expressionValue.Property.PropertyType))
            {
                return GetFloat<T>(parameter, context.arguments().value()[0], jsonNamingPolicy);
            }

            if (IsDouble(expressionValue.Property.PropertyType))
            {
                return GetDouble<T>(parameter, context.arguments().value()[0], jsonNamingPolicy);
            }

            if (IsDecimal(expressionValue.Property.PropertyType))
            {
                return GetDecimal<T>(parameter, context.arguments().value()[0], jsonNamingPolicy);
            }
            
            if (IsByte(expressionValue.Property.PropertyType))
            {
                return GetByte<T>(parameter, context.arguments().value()[0], jsonNamingPolicy);
            }
            
            return GetInt<T>(parameter, context.arguments().value()[0], jsonNamingPolicy);
        }

        public static object GetValue<T>(ParameterExpression parameter, ExpressionValue expressionValue,
            RSqlQueryParser.ComparisonContext context,
            JsonNamingPolicy jsonNamingPolicy = null)
        {
            CheckParameters(parameter, expressionValue, context);

            if (IsNumeric(expressionValue.Property.PropertyType))
            {
                return GetNumeric<T>(parameter, expressionValue, context, jsonNamingPolicy);
            }

            if (IsTemporal(expressionValue.Property.PropertyType))
            {
                return GetTemporal<T>(parameter, expressionValue, context, jsonNamingPolicy);
            }

            if (IsAlphabetic(expressionValue.Property.PropertyType))
            {
                return GetAlphabetic<T>(parameter, expressionValue, context, jsonNamingPolicy);
            }

            if (IsBool(expressionValue.Property.PropertyType))
            {
                return GetBoolean<T>(parameter, context.arguments().value()[0], jsonNamingPolicy);
            }

            if (IsEnum(expressionValue.Property.PropertyType))
            {
                return GetEnum(context.arguments().value()[0],
                    expressionValue.Property.PropertyType);
            }

            if (IsGuid(expressionValue.Property.PropertyType))
            {
                return GetGuid<T>(parameter, context.arguments().value()[0], jsonNamingPolicy);
            }
            
            return null;
        }

        /// <summary>
        ///     try to convert value context to enum value
        /// </summary>
        /// <param name="valueContext"></param>
        /// <param name="enumType"></param>
        /// <returns></returns>
        /// <exception cref="InvalidConversionException"></exception>
        private static object GetEnum(RSqlQueryParser.ValueContext valueContext,
            Type enumType)
        {
            try
            {
                return Enum.Parse(enumType.IsGenericType ? enumType.GetGenericArguments()[0] : enumType,
                    valueContext.GetText(), true);
            }
            catch
            {
                throw new InvalidConversionException(valueContext, enumType);
            }
        }

        private static object GetString<T>(ParameterExpression parameter,
            RSqlQueryParser.ValueContext valueContext,
            JsonNamingPolicy jsonNamingPolicy = null)
        {
            if (valueContext.single_quote() != null || valueContext.double_quote() != null)
            {
                var replace = valueContext.single_quote() != null ? "'" : "\"";
                var value = valueContext.GetText();
                return value.Length == 2
                    ? string.Empty
                    : value.Substring(1, value.Length - 2).Replace("\\" + replace, replace);
            }

            if (ExpressionValue.TryParse<T>(parameter, valueContext.GetText(), jsonNamingPolicy, out var expression))
            {
                return expression;
            }

            return valueContext.GetText();
        }

        private static object GetGuid<T>(ParameterExpression parameter,
            RSqlQueryParser.ValueContext valueContext,
            JsonNamingPolicy jsonNamingPolicy = null)
        {
            if (Guid.TryParse(valueContext.GetText(), out var result))
            {
                return result;
            }

            throw new InvalidConversionException(valueContext, typeof(Guid));
        }

        private static object GetChar<T>(ParameterExpression parameter,
            RSqlQueryParser.ValueContext valueContext,
            JsonNamingPolicy jsonNamingPolicy = null)
        {
            if (char.TryParse(valueContext.GetText(), out var result))
            {
                return result;
            }

            if (ExpressionValue.TryParse<T>(parameter, valueContext.GetText(), jsonNamingPolicy, out var expression))
            {
                return expression;
            }

            throw new InvalidConversionException(valueContext, typeof(char));
        }

        private static object GetByte<T>(ParameterExpression parameter,
            RSqlQueryParser.ValueContext valueContext,
            JsonNamingPolicy jsonNamingPolicy = null)
        {
            if (byte.TryParse(valueContext.GetText(), out var result))
            {
                return result;
            }

            if (ExpressionValue.TryParse<T>(parameter, valueContext.GetText(), jsonNamingPolicy, out var expression))
            {
                return expression;
            }

            throw new InvalidConversionException(valueContext, typeof(byte));
        }

        private static object GetShort<T>(ParameterExpression parameter,
            RSqlQueryParser.ValueContext valueContext,
            JsonNamingPolicy jsonNamingPolicy = null)
        {
            if (short.TryParse(valueContext.GetText(), out var result))
            {
                return result;
            }

            if (ExpressionValue.TryParse<T>(parameter, valueContext.GetText(), jsonNamingPolicy, out var expression))
            {
                return expression;
            }

            throw new InvalidConversionException(valueContext, typeof(short));
        }

        private static object GetInt<T>(ParameterExpression parameter, RSqlQueryParser.ValueContext valueContext,
            JsonNamingPolicy jsonNamingPolicy = null)
        {
            if (int.TryParse(valueContext.GetText(), out var result))
            {
                return result;
            }

            if (ExpressionValue.TryParse<T>(parameter, valueContext.GetText(), jsonNamingPolicy, out var value))
            {
                return value;
            }

            throw new InvalidConversionException(valueContext, typeof(int));
        }

        private static object GetLong<T>(ParameterExpression parameter,
            RSqlQueryParser.ValueContext valueContext,
            JsonNamingPolicy jsonNamingPolicy = null)
        {
            if (long.TryParse(valueContext.GetText(), out var result))
            {
                return result;
            }

            if (ExpressionValue.TryParse<T>(parameter, valueContext.GetText(), jsonNamingPolicy, out var value))
            {
                return value;
            }

            throw new InvalidConversionException(valueContext, typeof(long));
        }

        private static object GetFloat<T>(ParameterExpression parameter,
            RSqlQueryParser.ValueContext valueContext,
            JsonNamingPolicy jsonNamingPolicy = null)
        {
            if (float.TryParse(
                valueContext.GetText().Replace(".",
                    Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator), out var result))
            {
                return result;
            }

            if (ExpressionValue.TryParse<T>(parameter, valueContext.GetText(), jsonNamingPolicy, out var value))
            {
                return value;
            }

            throw new InvalidConversionException(valueContext, typeof(float));
        }

        private static object GetDouble<T>(ParameterExpression parameter, RSqlQueryParser.ValueContext valueContext,
            JsonNamingPolicy jsonNamingPolicy = null)
        {
            if (double.TryParse(
                valueContext.GetText().Replace(".",
                    Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator), out var result))
            {
                return result;
            }

            if (ExpressionValue.TryParse<T>(parameter, valueContext.GetText(), jsonNamingPolicy, out var value))
            {
                return value;
            }

            throw new InvalidConversionException(valueContext, typeof(double));
        }

        private static object GetDecimal<T>(ParameterExpression parameter, RSqlQueryParser.ValueContext valueContext,
            JsonNamingPolicy jsonNamingPolicy = null)
        {
            if (decimal.TryParse(
                valueContext.GetText().Replace(".",
                    Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator), out var result))
            {
                return result;
            }

            if (ExpressionValue.TryParse<T>(parameter, valueContext.GetText(), jsonNamingPolicy, out var value))
            {
                return value;
            }

            throw new InvalidConversionException(valueContext, typeof(decimal));
        }

        private static object GetBoolean<T>(ParameterExpression parameter, RSqlQueryParser.ValueContext valueContext,
            JsonNamingPolicy jsonNamingPolicy = null)
        {
            if (bool.TryParse(valueContext.GetText(), out var result))
            {
                return result;
            }

            if (ExpressionValue.TryParse<T>(parameter, valueContext.GetText(), jsonNamingPolicy, out var value))
            {
                return value;
            }

            throw new InvalidConversionException(valueContext, typeof(bool));
        }

        private static object GetDateTime<T>(ParameterExpression parameter, RSqlQueryParser.ValueContext valueContext,
            JsonNamingPolicy jsonNamingPolicy = null)
        {
            try
            {
                return DateTime.Parse(valueContext.GetText(), CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind);
            }
            catch
            {
                if (ExpressionValue.TryParse<T>(parameter, valueContext.GetText(), jsonNamingPolicy, out var value))
                {
                    return value;
                }

                throw new InvalidConversionException(valueContext, typeof(DateTime));
            }
        }

        private static object GetDateTimeOffset<T>(ParameterExpression parameter, RSqlQueryParser.ValueContext valueContext,
            JsonNamingPolicy jsonNamingPolicy = null)
        {
            try
            {
                return DateTimeOffset.Parse(valueContext.GetText(), CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind);
            }
            catch
            {
                if (ExpressionValue.TryParse<T>(parameter, valueContext.GetText(), jsonNamingPolicy, out var value))
                {
                    return value;
                }

                throw new InvalidConversionException(valueContext, typeof(DateTimeOffset));
            }
        }
    }
}
