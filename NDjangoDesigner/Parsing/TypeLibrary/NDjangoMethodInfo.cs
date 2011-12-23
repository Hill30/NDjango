using System;
using System.Globalization;
using System.Reflection;

namespace NDjango.Designer.Parsing.TypeLibrary
{
    public class NDjangoMethodInfo : MethodInfo
    {
        private readonly NDjangoType parent;
        private readonly string name;
        private readonly Type type;

        public NDjangoMethodInfo(NDjangoType parent, Type type, string name)
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

        public override ParameterInfo[] GetParameters()
        {
            throw new NotImplementedException();
        }

        public override MethodImplAttributes GetMethodImplementationFlags()
        {
            throw new NotImplementedException();
        }

        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override MethodInfo GetBaseDefinition()
        {
            throw new NotImplementedException();
        }

        public override ICustomAttributeProvider ReturnTypeCustomAttributes
        {
            get { throw new NotImplementedException(); }
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

        public override RuntimeMethodHandle MethodHandle
        {
            get { throw new NotImplementedException(); }
        }

        public override MethodAttributes Attributes
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