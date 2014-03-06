using System;
using System.Reflection;
using System.Reflection.Emit;

namespace SPFSearchFix
{
    public static class MethodInfoExtensions
    {
        /// <summary>
        /// Returns a _private_ Property Value from a given Object. Uses Reflection.
        /// Throws a ArgumentOutOfRangeException if the Property is not found.
        /// </summary>
        /// <typeparam name="T">Type of the Property</typeparam>
        /// <param name="obj">Object from where the Property Value is returned</param>
        /// <param name="propName">Propertyname as string.</param>
        /// <returns>PropertyValue</returns>
        public static T GetPrivatePropertyValue<T>(this object obj, string propName)
        {
            if (obj == null) throw new ArgumentNullException("obj");
            PropertyInfo pi = obj.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (pi == null) throw new ArgumentOutOfRangeException("propName", string.Format("Property {0} was not found in Type {1}", propName, obj.GetType().FullName));
            return (T)pi.GetValue(obj, null);
        }

        /// <summary>
        /// Returns a private Property Value from a given Object. Uses Reflection.
        /// Throws a ArgumentOutOfRangeException if the Property is not found.
        /// </summary>
        /// <typeparam name="T">Type of the Property</typeparam>
        /// <param name="obj">Object from where the Property Value is returned</param>
        /// <param name="propName">Propertyname as string.</param>
        /// <returns>PropertyValue</returns>
        public static T GetPrivateFieldValue<T>(this object obj, string propName)
        {
            if (obj == null) throw new ArgumentNullException("obj");
            Type t = obj.GetType();
            FieldInfo fi = null;
            while (fi == null && t != null)
            {
                fi = t.GetField(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                t = t.BaseType;
            }
            if (fi == null) throw new ArgumentOutOfRangeException("propName", string.Format("Field {0} was not found in Type {1}", propName, obj.GetType().FullName));
            return (T)fi.GetValue(obj);
        }

        /// <summary>
        /// Sets a _private_ Property Value from a given Object. Uses Reflection.
        /// Throws a ArgumentOutOfRangeException if the Property is not found.
        /// </summary>
        /// <typeparam name="T">Type of the Property</typeparam>
        /// <param name="obj">Object from where the Property Value is set</param>
        /// <param name="propName">Propertyname as string.</param>
        /// <param name="val">Value to set.</param>
        /// <returns>PropertyValue</returns>
        public static void SetPrivatePropertyValue<T>(this object obj, string propName, T val)
        {
            Type t = obj.GetType();
            if (t.GetProperty(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) == null)
                throw new ArgumentOutOfRangeException("propName", string.Format("Property {0} was not found in Type {1}", propName, obj.GetType().FullName));
            t.InvokeMember(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.SetProperty | BindingFlags.Instance, null, obj, new object[] { val });
        }

        /// <summary>
        /// Set a private Property Value on a given Object. Uses Reflection.
        /// </summary>
        /// <typeparam name="T">Type of the Property</typeparam>
        /// <param name="obj">Object from where the Property Value is returned</param>
        /// <param name="propName">Propertyname as string.</param>
        /// <param name="val">the value to set</param>
        /// <exception cref="ArgumentOutOfRangeException">if the Property is not found</exception>
        public static void SetPrivateFieldValue<T>(this object obj, string propName, T val)
        {
            if (obj == null) throw new ArgumentNullException("obj");
            Type t = obj.GetType();
            FieldInfo fi = null;
            while (fi == null && t != null)
            {
                fi = t.GetField(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                t = t.BaseType;
            }
            if (fi == null) throw new ArgumentOutOfRangeException("propName", string.Format("Field {0} was not found in Type {1}", propName, obj.GetType().FullName));
            fi.SetValue(obj, val);
        }

        public static object InvokeNotOverride(this MethodInfo MethodInfo, object Object, params object[] Arguments)
        { // void return, this parameter
            var Parameters = MethodInfo.GetParameters();

            if (Parameters.Length == 0)
            {
                if (Arguments != null && Arguments.Length != 0) throw new Exception("The number of arguments does not match the number of parameters");
            }
            else
            {
                if (Parameters.Length != Arguments.Length) throw new Exception("The number of arguments does not match the number of parameters");
            }

            Type ReturnType = null;
            if (MethodInfo.ReturnType != typeof(void))
            {
                ReturnType = MethodInfo.ReturnType;
            }

            var Type = Object.GetType();
            var DynamicMethod = new DynamicMethod("", ReturnType, new Type[] { Type, typeof(Object) }, Type);
            var ILGenerator = DynamicMethod.GetILGenerator();
            ILGenerator.Emit(OpCodes.Ldarg_0); // this

            for (var i = 0; i < Parameters.Length; i++)
            {
                var Parameter = Parameters[i];

                ILGenerator.Emit(OpCodes.Ldarg_1); // load array argument

                // get element at index
                ILGenerator.Emit(OpCodes.Ldc_I4_S, i); // specify index
                ILGenerator.Emit(OpCodes.Ldelem_Ref); // get element

                var ParameterType = Parameter.ParameterType;
                if (ParameterType.IsPrimitive)
                {
                    ILGenerator.Emit(OpCodes.Unbox_Any, ParameterType);
                }
                else if (ParameterType == typeof(object))
                {
                    // do nothing
                }
                else
                {
                    ILGenerator.Emit(OpCodes.Castclass, ParameterType);
                }
            }

            ILGenerator.Emit(OpCodes.Call, MethodInfo);

            var TestLabel = ILGenerator.DefineLabel();

            ILGenerator.Emit(OpCodes.Ret);
            return DynamicMethod.Invoke(null, new object[] { Object, Arguments });
        }
    }
}
