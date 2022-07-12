using System.Reflection;
using System.Text.RegularExpressions;

namespace AccountManager.Core.Static
{
    public static class AcfDeserializer
    {
        private const char TrimChar = '"';

        private static bool TrySetProperty(object obj, string property, object value)
        {
            var properties = property.Split(".");
            PropertyInfo? currentProp = obj?.GetType()?.GetProperty(properties[0], BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            if (properties.Length > 1)
            {
                for (int i = 0; i < properties.Length - 1; i++)
                {
                    var val = currentProp?.GetValue(obj);

                    if (currentProp is null)
                        return false;

                    if (val is null)
                    {
                        val = Activator.CreateInstance(currentProp.PropertyType);
                        currentProp.SetValue(obj, val, null);
                    }

                    if (val is not null)
                    {
                        obj = val;
                        if (i < properties.Length - 1)
                            currentProp = obj?.GetType()?.GetProperty(properties[i + 1], BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    }
                }
            }

            if (currentProp is not null && currentProp.CanWrite)
            {
                currentProp.SetValue(obj, value, null);
                return true;
            }

            return false;
        }

        public static T Deserialize<T>(string[] manifestLines) where T : new()
        {
            var manifestObj = new T();
            var dictionary = new Dictionary<string, string>();
            var manifestFileLines = manifestLines;
            var currentPath = "";
            Dictionary<string, string> currentDictionary = dictionary;

            foreach (var line in manifestFileLines)
            {
                var trimmedLine = line.Trim();
                var quotedString = Regex.Matches(trimmedLine, "\"[^\" ][^\"]*\"");
                if (quotedString.Count == 1)
                {
                    currentPath = $"{currentPath}{quotedString[0]?.Value?.Trim(TrimChar)}.";
                }
                else if (quotedString.Count == 2)
                {
                    currentDictionary[$"{currentPath}{quotedString[0].Value.Trim('"')}"] = quotedString[1].Value.Trim('"');
                    TrySetProperty(manifestObj, $"{currentPath}{quotedString[0].Value.Trim('"')}", quotedString[1].Value.Trim('"'));
                }
                else
                {
                    if (trimmedLine == "}")
                        currentPath = string.Join(("/"), currentPath.Split("/")[..^1]);
                }
            }


            return manifestObj;
        }
    }
}
