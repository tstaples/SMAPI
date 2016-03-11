using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StardewModdingAPI.Helpers
{
    public static class ReflectionHelper
    {
        private class FieldCompatability
        {
            public List<Tuple<FieldInfo, FieldInfo>> MatchingFields = new List<Tuple<FieldInfo, FieldInfo>>();
            public List<Tuple<FieldInfo, FieldInfo>> FixableMismatchingFields = new List<Tuple<FieldInfo, FieldInfo>>();
            public List<Tuple<FieldInfo, FieldInfo>> UnfixableMismatchingFields = new List<Tuple<FieldInfo, FieldInfo>>();
            public List<FieldInfo> FieldsMissingFromA = new List<FieldInfo>();
            public List<FieldInfo> FieldsMissingFromB = new List<FieldInfo>();
        }

        private static FieldCompatability AssessFieldCompatbility(FieldInfo[] a, FieldInfo[] b)
        {
            FieldCompatability results = new FieldCompatability();
            List<FieldInfo> editableB = new List<FieldInfo>(b);

            foreach (var leftField in a)
            {
                FieldInfo matchingField = null;
                foreach(var rightField in editableB)
                {
                    if(leftField.Name == rightField.Name)
                    {
                        matchingField = rightField;
                        if (leftField.FieldType == rightField.FieldType)
                        {
                            results.MatchingFields.Add(Tuple.Create(leftField, rightField));
                        }
                        else if(leftField.FieldType.FullName == rightField.FieldType.FullName)
                        {
                            results.FixableMismatchingFields.Add(Tuple.Create(leftField, rightField));
                        }
                        else
                        {
                            results.UnfixableMismatchingFields.Add(Tuple.Create(leftField, rightField));
                        }
                        break;
                    }
                }

                if (matchingField != null)
                {
                    editableB.Remove(matchingField);
                }
                else
                {
                    results.FieldsMissingFromB.Add(leftField);
                }
            }

            results.FieldsMissingFromA.AddRange(editableB);
            return results;
        }

        private static void WarnMismatch(FieldCompatability compatibility, string source)
        {
            if (compatibility.UnfixableMismatchingFields.Any())
            {
                Log.Warning("Unfixable type mismatch in {0}. Mods which depend on these types may fail to work properly", source);
                foreach (var mismatch in compatibility.UnfixableMismatchingFields)
                {
                    Log.Warning("- {0} is not of type {1}, is of type {2}", mismatch.Item1.Name, mismatch.Item1.FieldType.FullName, mismatch.Item2.FieldType.FullName);
                }
            }
        }

        private static void WarnMissing(FieldCompatability compatibility, string source)
        {
            if (compatibility.FieldsMissingFromA.Any() || compatibility.FieldsMissingFromB.Any())
            {
                Log.Warning("The following fields are not present in both objects", source);

                foreach (var mismatch in compatibility.FieldsMissingFromA)
                    Log.Warning("- Left Object Missing {0} ({1})", mismatch.Name, mismatch.FieldType.FullName);
                foreach (var mismatch in compatibility.FieldsMissingFromA)
                    Log.Warning("- Right Object Missing {0} ({1})", mismatch.Name, mismatch.FieldType.FullName);
            }
        }
        
        private static void RecursiveMemberwiseCast(Type toType, Type fromType, object to, object from)
        {
            Log.Verbose("Writing {0}", toType.Name);

            Stack<Tuple<Type, Type, object, object>> pendingCasts = new Stack<Tuple<Type, Type, object, object>>();
            List<object> alreadyProcessed = new List<object>();

            pendingCasts.Push(Tuple.Create(toType, fromType,
                       to, from));

            while (pendingCasts.Count > 0)
            {
                Tuple<Type, Type, object, object> current = pendingCasts.Pop();

                Type castTo = current.Item1;
                Type castFrom = current.Item2;
                object objectToSet = current.Item3;
                object baseObject = current.Item4;

                var targetFields = castTo
                .GetFields().Where(n => !n.IsInitOnly && !n.IsLiteral).ToArray();
                var baseFields = castFrom
                     .GetFields().Where(n => !n.IsInitOnly && !n.IsLiteral).ToArray();

                var compatibility = AssessFieldCompatbility(targetFields, baseFields);

                WarnMissing(compatibility, from.GetType().FullName);
                WarnMismatch(compatibility, from.GetType().FullName);

                foreach (var match in compatibility.MatchingFields)
                {
                    var toValue = match.Item1.GetValue(objectToSet);
                    var fromValue = match.Item2.GetValue(baseObject);

                    match.Item2.SetValue(objectToSet, fromValue);
                }

                foreach (var fixableMismatch in compatibility.FixableMismatchingFields)
                {
                    var toValue = fixableMismatch.Item1.GetValue(objectToSet);
                    var fromValue = fixableMismatch.Item2.GetValue(baseObject);

                    if (fromValue != null && !alreadyProcessed.Any(n => Object.ReferenceEquals(n, fromValue)))
                    {
                        alreadyProcessed.Add(fromValue);
                        //pendingCasts.Push(Tuple.Create(fixableMismatch.Item1.FieldType, fixableMismatch.Item2.FieldType,
                        //   toValue, fromValue));
                    }
                }
            }
        }
            
        public static T MemberwiseCast<T>(this object @base) where T : new()
        {
            T retObj = new T();
            RecursiveMemberwiseCast(@base.GetType(), retObj.GetType().BaseType, retObj, @base);
            return retObj;
        }
    }
}
