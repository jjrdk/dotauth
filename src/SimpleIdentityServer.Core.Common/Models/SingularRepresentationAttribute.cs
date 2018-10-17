namespace SimpleIdentityServer.Core.Common.Models
{
    using System;
    using DTOs;

    public class SingularRepresentationAttribute<T> : RepresentationAttribute
    {
        public SingularRepresentationAttribute(SchemaAttributeResponse type, T value): base(type)
        {
            Value = value;
        }

        public T Value { get; set; }

        public override dynamic GetValue()
        {
            return Value;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var representationObj = obj as SingularRepresentationAttribute<T>;
            if (representationObj == null)
            {
                return false;
            }

            return representationObj.Value.Equals(Value);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override bool SetValue(RepresentationAttribute attr)
        {
            if (attr == null || attr.SchemaAttribute == null || SchemaAttribute == null)
            {
                return false;
            }

            var target = attr as SingularRepresentationAttribute<T>;
            if (target == null)
            {
                return false;
            }
            
            Value = target.Value;
            return true;
        }

        protected override object CloneObj()
        {
            return new SingularRepresentationAttribute<T>(SchemaAttribute, Value);
        }

        protected override int CompareTo(RepresentationAttribute attr)
        {
            var singular = attr as SingularRepresentationAttribute<T>;
            if (singular == null)
            {
                return 1;
            }

            return CompareTo(singular.Value);
        }

        private int CompareTo(T target)
        {
            // TODO : Compare the IEnumerable !!
            if (SchemaAttribute.MultiValued)
            {
                return 0;
            }

            switch (SchemaAttribute.Type)
            {
                case ScimConstants.SchemaAttributeTypes.String:
                    var ss = Value as string;
                    var ts = target as string;
                    return ss.CompareTo(ts);
                case ScimConstants.SchemaAttributeTypes.Boolean:
                    var sb = bool.Parse(Value as string);
                    var tb = bool.Parse(target as string);
                    return sb.CompareTo(tb);
                case ScimConstants.SchemaAttributeTypes.Integer:
                    var si = int.Parse(Value as string);
                    var ti = int.Parse(target as string);
                    return si.CompareTo(ti);
                case ScimConstants.SchemaAttributeTypes.Decimal:
                    var sd = decimal.Parse(Value as string);
                    var td = decimal.Parse(target as string);
                    return sd.CompareTo(td);
                case ScimConstants.SchemaAttributeTypes.DateTime:
                    var sdt = DateTime.Parse(Value as string);
                    var tdt = DateTime.Parse(target as string);
                    return sdt.CompareTo(tdt);
            }

            return -1;
        }
    }
}