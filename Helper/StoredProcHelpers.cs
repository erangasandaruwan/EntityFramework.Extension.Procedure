using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFramework.Procedure.Utility.Helper
{
    public static class StoredProcHelpers
    {

        /// <summary>
        /// Get properties of a type that do not have the 'NotMapped' attribute
        /// </summary>
        /// <param name="t">Type to examine for properites</param>
        /// <returns>Array of properties that can be filled</returns>
        public static PropertyInfo[] GetMappedProperties(this Type t)
        {
            var props1 = t.GetProperties();
            var props2 = props1
                .Where(p => p.GetAttribute<NotMappedAttribute>() == null)
                .Select(p => p);
            return props2.ToArray();
        }

        /// <summary>
        /// Get an attribute for a property
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        public static T GetAttribute<T>(this PropertyInfo propertyInfo)
            where T : Attribute
        {
            var attributes = propertyInfo.GetCustomAttributes(typeof(T), false).FirstOrDefault();
            return (T)attributes;
        }

        /// <summary>
        /// List all properties for an object by name, allow for attributes to override the name.
        /// </summary>
        /// <param name="props"></param>
        /// <returns></returns>
        public static Dictionary<string, PropertyInfo> GetPropertiesByName(PropertyInfo[] props)
        {
            var propertyMap = new Dictionary<string, PropertyInfo>(props.Length);
            foreach (var p in props)
            {
                var name = p.Name;
                propertyMap.Add(name, p);
            }

            return propertyMap;
        }

        /// <summary>
        /// Match DbDataReader "columns" with properties of the destination object by Name and Ordinal attributes. Name attributes
        /// are accepted before Ordinal attributes and both can be freely intermixed in a target object.
        /// </summary>
        /// <param name="reader">Returned data containing values to map to the object</param>
        /// <param name="t">Object containing target properties</param>
        /// <param name="currentType">Iterator over defined return types, should be the Type of the object t param</param>
        /// <returns></returns>
        public static PropertyInfo[] MatchRecordProperties(this DbDataReader reader, object t, IEnumerator currentType)
        {
            // get properties to save for the current destination type
            var props = ((Type)currentType.Current).GetMappedProperties();
            var propertyMap = StoredProcHelpers.GetPropertiesByName(props);
            var propertyList = new PropertyInfo[reader.FieldCount];

            for (var i = 0; i < reader.FieldCount; i++)
            {
                PropertyInfo propertyInfo = null;
                var name = reader.GetName(i);
                try
                {
                    // if we don't have this property in our map, just skip it. Note: we're doing a currentculture w/ no case search for the key.
                    var key = propertyMap.Keys.FirstOrDefault(k => k.Equals(name, StringComparison.CurrentCultureIgnoreCase));
                    if (!string.IsNullOrEmpty(key))
                    {
                        // get the relevant property for this column
                        propertyInfo = propertyMap[key];
                    }
                }
                catch (Exception ex)
                {
                    var outer = new Exception($"Exception identifying matching property for return column {name} in {t.GetType().Name}", ex);
                    throw outer;
                }

                propertyList[i] = propertyInfo;
            }

            return propertyList;
        }

        /// <summary>
        /// Read data for the current result row from a reader into a destination object, by the name
        /// of the properties on the destination object.
        /// </summary>
        /// <param name="reader">data reader holding return data</param>
        /// <param name="t">object to populate</param>
        /// <returns></returns>
        /// <param name="props">properties list to copy from result set row 'reader' to object 't'</param>
        /// <param name="token">Cancellation token for asyc process cancellation</param>
        public static async Task<object> ReadRecordAsync(this DbDataReader reader, object t, PropertyInfo[] props, CancellationToken token)
        {

            for (var i = 0; i < reader.FieldCount; i++)
            {
                var propertyInfo = props[i];
                if (null == propertyInfo)
                    continue;

                var name = reader.GetName(i);

                try
                {
                    var data = await reader.GetFieldValueAsync<object>(i);
                    propertyInfo.SetValue(t, data is DBNull ? null : data, null);
                }
                catch (Exception ex)
                {
                    if (ex is IndexOutOfRangeException)
                    {
                        if (propertyInfo.CanWrite)
                        {
                            propertyInfo.SetValue(t, null, null);
                        }
                    }
                    else
                    {
                        var outer = new Exception($"Exception processing return column {name} in {t.GetType().Name}", ex);
                        throw outer;
                    }
                }
            }

            return t;
        }
    }
}
