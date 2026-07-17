using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace MSP.Unity.Internal
{
    /// <summary>Minimal JSON encoder for bridge params (string/bool/number/null/maps/arrays).</summary>
    internal static class MSPParamsJson
    {
        internal static string Serialize(MSPInitializationParameters parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            return Serialize(new Dictionary<string, object>
            {
                { "prebidApiKey", parameters.PrebidApiKey },
                { "sourceApp", parameters.SourceApp },
                { "orgId", parameters.OrgId },
                { "appId", parameters.AppId },
                { "prebidHost", parameters.PrebidHost },
                { "isAgeRestrictedUser", parameters.IsAgeRestrictedUser },
                { "isInTestMode", parameters.IsInTestMode },
                { "parameters", parameters.Parameters },
                { "appPackageName", parameters.AppPackageName },
                { "appVersionName", parameters.AppVersionName }
            });
        }

        internal static string Serialize(IDictionary<string, object> map)
        {
            if (map == null || map.Count == 0)
            {
                return "{}";
            }

            var builder = new StringBuilder(64);
            WriteObject(builder, map);
            return builder.ToString();
        }

        private static void WriteObject(StringBuilder builder, IDictionary<string, object> map)
        {
            builder.Append('{');
            var first = true;
            foreach (var entry in map)
            {
                if (entry.Key == null)
                {
                    continue;
                }

                if (!first)
                {
                    builder.Append(',');
                }

                first = false;
                WriteString(builder, entry.Key);
                builder.Append(':');
                WriteValue(builder, entry.Value);
            }

            builder.Append('}');
        }

        private static void WriteObject(StringBuilder builder, IDictionary map)
        {
            builder.Append('{');
            var first = true;
            foreach (DictionaryEntry entry in map)
            {
                if (entry.Key == null)
                {
                    continue;
                }

                if (!first)
                {
                    builder.Append(',');
                }

                first = false;
                WriteString(builder, Convert.ToString(entry.Key, CultureInfo.InvariantCulture) ?? string.Empty);
                builder.Append(':');
                WriteValue(builder, entry.Value);
            }

            builder.Append('}');
        }

        private static void WriteValue(StringBuilder builder, object value)
        {
            if (value == null)
            {
                builder.Append("null");
                return;
            }

            switch (value)
            {
                case string text:
                    WriteString(builder, text);
                    return;
                case bool flag:
                    builder.Append(flag ? "true" : "false");
                    return;
                case IDictionary<string, object> typedDictionary:
                    WriteObject(builder, typedDictionary);
                    return;
                case IDictionary dictionary:
                    WriteObject(builder, dictionary);
                    return;
                case IEnumerable enumerable when !(value is string):
                    builder.Append('[');
                    var first = true;
                    foreach (var item in enumerable)
                    {
                        if (!first)
                        {
                            builder.Append(',');
                        }

                        first = false;
                        WriteValue(builder, item);
                    }

                    builder.Append(']');
                    return;
            }

            switch (Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    builder.Append(Convert.ToString(value, CultureInfo.InvariantCulture));
                    return;
                default:
                    WriteString(builder, Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty);
                    return;
            }
        }

        private static void WriteString(StringBuilder builder, string text)
        {
            builder.Append('"');
            foreach (var ch in text)
            {
                switch (ch)
                {
                    case '\\':
                        builder.Append("\\\\");
                        break;
                    case '"':
                        builder.Append("\\\"");
                        break;
                    case '\b':
                        builder.Append("\\b");
                        break;
                    case '\f':
                        builder.Append("\\f");
                        break;
                    case '\n':
                        builder.Append("\\n");
                        break;
                    case '\r':
                        builder.Append("\\r");
                        break;
                    case '\t':
                        builder.Append("\\t");
                        break;
                    default:
                        if (ch < ' ')
                        {
                            builder.AppendFormat(CultureInfo.InvariantCulture, "\\u{0:x4}", (int)ch);
                        }
                        else
                        {
                            builder.Append(ch);
                        }

                        break;
                }
            }

            builder.Append('"');
        }
    }
}
