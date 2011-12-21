using System;
using System.Globalization;
using System.Reflection;

namespace NDjango.Designer.Parsing.TypeLibrary
{
    public class NDjangoPropertyInfo : PropertyInfo
    {
        private readonly NDjangoType parent;
        private readonly string name;
        private readonly Type type;

        public NDjangoPropertyInfo(NDjangoType parent, Type type, string name)
        {
            this.parent = parent;
            this.type = type;
            this.name = name;
          
        }

        /// <summary>
        /// When overridden in a derived class, returns an array of all custom attributes applied to this member.
        /// </summary>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public override object[] GetCustomAttributes(bool inherit)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// When overridden in a derived class, indicates whether one or more attributes of the specified type or of its derived types is applied to this member.
        /// </summary>
        /// <param name="attributeType"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public override bool IsDefined(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }

        public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override MethodInfo[] GetAccessors(bool nonPublic)
        {
            throw new NotImplementedException();
        }

        public override MethodInfo GetGetMethod(bool nonPublic)
        {
            throw new NotImplementedException();
        }

        public override MethodInfo GetSetMethod(bool nonPublic)
        {
            throw new NotImplementedException();
        }

        public override ParameterInfo[] GetIndexParameters()
        {
            throw new NotImplementedException();
        }

        public override string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Gets the class that declares this member.
        /// </summary>
        public override Type DeclaringType
        {
            get { return parent; }
        }

        /// <summary>
        /// Gets the class object that was used to obtain this instance of MemberInfo.
        /// </summary>
        public override Type ReflectedType
        {
            get { return parent; }
        }

        public override Type PropertyType
        {
            get { return type; }
        }

        public override PropertyAttributes Attributes
        {
            get { throw new NotImplementedException(); }
        }


        public override bool CanRead
        {
            get { throw new NotImplementedException(); }
        }

        public override bool CanWrite
        {
            get { throw new NotImplementedException(); }
        }



        /// <summary>
        /// When overridden in a derived class, returns an array of custom attributes applied to this member and identified by Type.
        /// </summary>
        /// <param name="attributeType"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();

        }


    }
}