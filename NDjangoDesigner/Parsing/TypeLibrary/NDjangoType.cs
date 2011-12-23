using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace NDjango.Designer.Parsing.TypeLibrary
{
    public class NDjangoType : Type
    {

        private readonly string fullName;

        private readonly List<MemberInfo> members= new List<MemberInfo>();

        public NDjangoType(string fullName)
        {
            this.fullName = fullName;
        }

        public void MergeType(Type typeToMergeWith)
        {
            var methods = typeToMergeWith.GetMethods();
            var properties = typeToMergeWith.GetProperties();
            var fields = typeToMergeWith.GetFields();


            for (var i = 0; i < methods.Count(); i++)
            {
                var method = methods[i];
                var memberIndex = members.FindIndex(m => m.Name == method.Name);
                if (memberIndex != -1)
                {
                    members[memberIndex] = method;
                    continue;
                }
                members.Add(method);
            }

            for (var i = 0; i < properties.Count(); i++)
            {
                var property = properties[i];
                var memberIndex = members.FindIndex(m => m.Name == property.Name);
                if (memberIndex != -1)
                {
                    members[memberIndex] = property;
                    continue;
                }
                members.Add(property);
            }

            for (var i = 0; i < fields.Count(); i++)
            {
                var field = fields[i];
                var memberIndex = members.FindIndex(m => m.Name == field.Name);
                if (memberIndex != -1)
                {
                    members[memberIndex] = field;
                    continue;
                }
                members.Add(field);
            }

        }

        public void AddMember(Type type, string memberName, MemberTypes memberType)
        {
            switch (memberType)
            {
                case MemberTypes.Property:
                    members.Add(new NDjangoPropertyInfo(this, type, memberName));
                    break;
                case MemberTypes.Method:
                    members.Add(new NDjangoMethodInfo(this, type, memberName));
                    break;
                default:
                    members.Add(new NDjangoFieldInfo(this, type, memberName));
                    break;
            }
            
        }

        #region Type members implementations

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
            return members.Where(m => m.MemberType == MemberTypes.Property).Select(m => m as PropertyInfo).ToArray();
        }

        protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            throw new NotImplementedException();
        }

        public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
        {
            return members.Where(m => m.MemberType == MemberTypes.Method).Select(m => m as MethodInfo).ToArray();
        }

        public override FieldInfo GetField(string name, BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public override FieldInfo[] GetFields(BindingFlags bindingAttr)
        {
            return members.Where(m => m.MemberType == MemberTypes.Field).Select(m => m as FieldInfo).ToArray();
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
            get { throw new NotImplementedException(); }
        }

        protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            throw new NotImplementedException();
        }

        public override string Name
        {
            get { return fullName.Split('.').Last(); }
        }

        public override Guid GUID
        {
            get { throw new NotImplementedException(); }
        }

        public override Module Module
        {
            get { throw new NotImplementedException(); }
        }

        public override Assembly Assembly
        {
            get { throw new NotImplementedException(); }
        }

        public override string FullName
        {
            get { return fullName; }
        }

        public override string Namespace
        {
            get { throw new NotImplementedException(); }
        }

        public override string AssemblyQualifiedName
        {
            get { throw new NotImplementedException(); }
        }

        public override Type BaseType
        {
            get { throw new NotImplementedException(); }
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
