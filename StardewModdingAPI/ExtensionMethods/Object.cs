using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StardewModdingAPI.ExtensionMethods
{
    //From https://github.com/jpmikkers/net-object-deep-copy/blob/master/ObjectExtensions.cs
    public static class ObjectExtensions
    {
        public static T Copy<T>(this object original)
        {
            return (T)new DeepCopyContext().InternalCopy(original, true);
        }

        private class DeepCopyContext
        {
            private static readonly Func<object, object> CloneMethod;
            private readonly Dictionary<Object, Object> m_Visited;
            private readonly Dictionary<Type, FieldInfo[]> m_NonShallowFieldCache;

            static DeepCopyContext()
            {
                MethodInfo cloneMethod = typeof(Object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance);
                var p1 = Expression.Parameter(typeof(object));
                var body = Expression.Call(p1, cloneMethod);
                CloneMethod = Expression.Lambda<Func<object, object>>(body, p1).Compile();
                //Console.WriteLine("typeof(object) contains {0} nonshallow fields", NonShallowFields(typeof(object)).Count());
            }

            public DeepCopyContext()
            {
                m_Visited = new Dictionary<object, object>(new ReferenceEqualityComparer());
                m_NonShallowFieldCache = new Dictionary<Type, FieldInfo[]>();
            }

            private static bool IsPrimitive(Type type)
            {
                if (type.IsValueType && type.IsPrimitive) return true;
                if (type == typeof(String)) return true;
                if (type == typeof(Decimal)) return true;
                if (type == typeof(DateTime)) return true;
                return false;
            }

            public Object InternalCopy(Object originalObject, bool includeInObjectGraph)
            {
                if (originalObject == null) return null;
                var typeToReflect = originalObject.GetType();
                if (IsPrimitive(typeToReflect)) return originalObject;

                if (typeof(XElement).IsAssignableFrom(typeToReflect)) return new XElement(originalObject as XElement);
                if (typeof(Delegate).IsAssignableFrom(typeToReflect)) return null;

                if (includeInObjectGraph)
                {
                    object result;
                    if (m_Visited.TryGetValue(originalObject, out result)) return result;
                }

                var cloneObject = CloneMethod(originalObject);

                if (includeInObjectGraph)
                {
                    m_Visited.Add(originalObject, cloneObject);
                }

                if (typeToReflect.IsArray)
                {
                    var arrayElementType = typeToReflect.GetElementType();

                    if (IsPrimitive(arrayElementType))
                    {
                        // for an array of primitives, do nothing. The shallow clone is enough.
                    }
                    else if (arrayElementType.IsValueType)
                    {
                        // if its an array of structs, there's no need to check and add the individual elements to 'visited', because in .NET it's impossible to create
                        // references to individual array elements.
                        Array clonedArray = (Array)cloneObject;
                        clonedArray.ForEach((array, indices) => array.SetValue(InternalCopy(clonedArray.GetValue(indices), false), indices));
                    }
                    else
                    {
                        // it's an array of ref types
                        Array clonedArray = (Array)cloneObject;
                        clonedArray.ForEach((array, indices) => array.SetValue(InternalCopy(clonedArray.GetValue(indices), true), indices));
                    }
                }
                else
                {
                    foreach (var fieldInfo in CachedNonShallowFields(typeToReflect))
                    {
                        var originalFieldValue = fieldInfo.GetValue(originalObject);
                        // a valuetype field can never have a reference pointing to it, so don't check the object graph in that case

                        Log.Error("Replace this with a recurse-less version");
                        var clonedFieldValue = InternalCopy(originalFieldValue, !fieldInfo.FieldType.IsValueType);
                        fieldInfo.SetValue(cloneObject, clonedFieldValue);
                    }
                }

                return cloneObject;
            }

            private FieldInfo[] CachedNonShallowFields(Type typeToReflect)
            {
                FieldInfo[] result;

                if (!m_NonShallowFieldCache.TryGetValue(typeToReflect, out result))
                {
                    result = NonShallowFields(typeToReflect).ToArray();
                    m_NonShallowFieldCache[typeToReflect] = result;
                }

                return result;
            }

            /// <summary>
            /// From the given type hierarchy (i.e. including all base types), return all fields that should be deep-copied
            /// </summary>
            /// <param name="typeToReflect"></param>
            /// <returns></returns>
            private static IEnumerable<FieldInfo> NonShallowFields(Type typeToReflect)
            {
                while (typeToReflect != typeof(object))
                {
                    foreach (var fieldInfo in typeToReflect.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly))
                    {
                        if (IsPrimitive(fieldInfo.FieldType)) continue; // this is 5% faster than a where clause..
                        yield return fieldInfo;
                    }
                    typeToReflect = typeToReflect.BaseType;
                }
            }
        }
    }

    public class ReferenceEqualityComparer : EqualityComparer<Object>
    {
        public override bool Equals(object x, object y)
        {
            return ReferenceEquals(x, y);
        }

        public override int GetHashCode(object obj)
        {
            if (obj == null) return 0;
            // The RuntimeHelpers.GetHashCode method always calls the Object.GetHashCode method non-virtually, 
            // even if the object's type has overridden the Object.GetHashCode method.
            return RuntimeHelpers.GetHashCode(obj);
        }
    }
    
}
