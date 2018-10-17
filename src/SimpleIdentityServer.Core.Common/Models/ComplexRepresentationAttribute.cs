namespace SimpleIdentityServer.Core.Common.Models
{
    using System.Collections.Generic;
    using System.Linq;
    using DTOs;

    public class ComplexRepresentationAttribute : RepresentationAttribute
    {
        public ComplexRepresentationAttribute(SchemaAttributeResponse type): base(type)
        {
        }

        public IEnumerable<RepresentationAttribute> Values { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var complexRepresentation = obj as ComplexRepresentationAttribute;
            if (complexRepresentation == null)
            {
                return false;
            }

            var result = Values.All(v => complexRepresentation.Values.Contains(v));
            return result;
        }

        public override int GetHashCode()
        {
            int result = 0;
            foreach(var value in Values)
            {
                result = result ^ value.GetHashCode();
            }

            return result;
        }

        protected override object CloneObj()
        {
            var newValues = new List<RepresentationAttribute>();
            if (Values != null)
            {
                foreach (var value in Values)
                {
                    newValues.Add((RepresentationAttribute)value.Clone());
                }
            }

            return new ComplexRepresentationAttribute(SchemaAttribute)
            {
                Values = newValues
            };
        }

        protected override int CompareTo(RepresentationAttribute attr)
        {
            var complex = attr as ComplexRepresentationAttribute;
            if (complex == null || complex.Values == null)
            {
                return 1;
            }

            if (Values == null)
            {
                return -1;
            }

            var sourcePrimary = Values.FirstOrDefault(p => p.SchemaAttribute != null && p.SchemaAttribute.Name == ScimConstants.MultiValueAttributeNames.Primary);
            var targetPrimary = complex.Values.FirstOrDefault(p => p.SchemaAttribute != null && p.SchemaAttribute.Name == ScimConstants.MultiValueAttributeNames.Primary);
            if (sourcePrimary == null || targetPrimary == null)
            {
                return 0;
            }

            return sourcePrimary.CompareTo(targetPrimary);
        }
    }
}