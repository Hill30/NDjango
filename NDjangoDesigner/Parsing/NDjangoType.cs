using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NDjango.Designer.Parsing
{
    public class NDjangoMemberInfo : MemberInfo
    {
    
        private readonly string name;

        public NDjangoMemberInfo(string name)
        {
            this.name = name;
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return new object[0];
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return true;
        }

        public override MemberTypes MemberType
        {
            get { return MemberTypes.All; }
        }

        public override string Name
        {
            get { return name; }
        }

        public override Type DeclaringType
        {
            get { return typeof(object); }
        }

        public override Type ReflectedType
        {
            get { return typeof(object); }
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return new object[0];
        }
    }

    public class NDjangoType : Type
    {
        private string name;

        private Guid guid;

        private Module module;

        private Assembly assembly;

        private string fullName;

        private string ns;

        private string assemblyQualifiedName;

        private Type baseType;

        private Type underlyingSystemType;

        private List<MemberInfo> members;

        public NDjangoType()
        {
            members = new List<MemberInfo>();
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            throw new NotImplementedException();
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }

        public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public override Type GetInterface(string name, bool ignoreCase)
        {
            throw new NotImplementedException();
        }

        public override Type[] GetInterfaces()
        {
            throw new NotImplementedException();
        }

        public override EventInfo GetEvent(string name, BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public override EventInfo[] GetEvents(BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public override Type[] GetNestedTypes(BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public override Type GetNestedType(string name, BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public override Type GetElementType()
        {
            throw new NotImplementedException();
        }

        protected override bool HasElementTypeImpl()
        {
            throw new NotImplementedException();
        }

        protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
        {
            throw new NotImplementedException();
        }

        public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            throw new NotImplementedException();
        }

        public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public override FieldInfo GetField(string name, BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public override FieldInfo[] GetFields(BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public void AddMember(string memberName)
        {
            if(members == null) members = new List<MemberInfo>();
            members.Add(new NDjangoMemberInfo(name));
        }

        public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
        {
            return members.ToArray();
        }

        protected override TypeAttributes GetAttributeFlagsImpl()
        {
            throw new NotImplementedException();
        }

        protected override bool IsArrayImpl()
        {
            throw new NotImplementedException();
        }

        protected override bool IsByRefImpl()
        {
            throw new NotImplementedException();
        }

        protected override bool IsPointerImpl()
        {
            throw new NotImplementedException();
        }

        protected override bool IsPrimitiveImpl()
        {
            throw new NotImplementedException();
        }

        protected override bool IsCOMObjectImpl()
        {
            throw new NotImplementedException();
        }

        public override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
        {
            throw new NotImplementedException();
        }

        public override Type UnderlyingSystemType
        {
            get { return underlyingSystemType; }
        }

        protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            throw new NotImplementedException();
        }

        public override string Name
        {
            get { return name; }
        }

        public override Guid GUID
        {
            get { return guid; }
        }

        public override Module Module
        {
            get { return module; }
        }

        public override Assembly Assembly
        {
            get { return assembly; }
        }

        public override string FullName
        {
            get { return fullName; }
        }

        public override string Namespace
        {
            get { return ns; }
        }

        public override string AssemblyQualifiedName
        {
            get { return assemblyQualifiedName; }
        }

        public override Type BaseType
        {
            get { return baseType; }
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }
    }
}
