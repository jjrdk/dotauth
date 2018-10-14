// Copyright 2015 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace System.Security.Cryptography.Algorithms.Extensions
{
    using Collections;
    using Diagnostics.Contracts;
    using Newtonsoft.Json;
    using Text;

    internal enum SecurityElementType
    {
        Regular = 0,
        Format = 1,
        Comment = 2
    }

    internal interface ISecurityElementFactory
    {
        SecurityElement CreateSecurityElement();
    }

    [JsonObject]
    [Runtime.InteropServices.ComVisible(true)]
    public sealed class SecurityElement : ISecurityElementFactory
    {
        internal string m_strTag;
        internal string m_strText;
        internal ArrayList m_lAttributes;
        internal SecurityElementType m_type = SecurityElementType.Regular;

        private static readonly char[] s_tagIllegalCharacters = new[] { ' ', '<', '>' };
        private static readonly char[] s_textIllegalCharacters = new[] { '<', '>' };
        private static readonly char[] s_valueIllegalCharacters = new[] { '<', '>', '\"' };

        private const int c_AttributesTypical = 4 * 2;  // 4 attributes, times 2 strings per attribute 
        private const int c_ChildrenTypical = 1;

        private static readonly string[] s_escapeStringPairs = new[]
            { 
                // these must be all once character escape sequences or a new escaping algorithm is needed
                "<", "&lt;",
                ">", "&gt;",
                "\"", "&quot;",
                "\'", "&apos;",
                "&", "&amp;"
            };

        //-------------------------- Constructors --------------------------- 

        internal SecurityElement()
        {
        }

        ////// ISecurityElementFactory implementation

        SecurityElement ISecurityElementFactory.CreateSecurityElement()
        {
            return this;
        }

        public SecurityElement(string tag)
        {
            if (tag == null)
            {
                throw new ArgumentNullException("tag");
            }

            if (!IsValidTag(tag))
            {
                throw new ArgumentException("Argument_InvalidElementTag");
            }

            Contract.EndContractBlock();

            m_strTag = tag;
            m_strText = null;
        }

        public SecurityElement(string tag, string text)
        {
            if (tag == null)
            {
                throw new ArgumentNullException("tag");
            }

            if (!IsValidTag(tag))
            {
                throw new ArgumentException("Argument_InvalidElementTag");
            }

            if (text != null && !IsValidText(text))
            {
                throw new ArgumentException("Argument_InvalidElementText");
            }

            Contract.EndContractBlock();

            m_strTag = tag;
            m_strText = text;
        }

        //-------------------------- Properties ----------------------------- 

        public ArrayList Children
        {
            get
            {
                ConvertSecurityElementFactories();
                return m_lChildren;
            }

            set
            {
                if (value != null)
                {
                    IEnumerator enumerator = value.GetEnumerator();

                    while (enumerator.MoveNext())
                    {
                        if (enumerator.Current == null)
                        {
                            throw new ArgumentException("ArgumentNull_Child");
                        }
                    }
                }

                m_lChildren = value;
            }
        }

        internal void ConvertSecurityElementFactories()
        {
            if (m_lChildren == null)
            {
                return;
            }

            for (int i = 0; i < m_lChildren.Count; ++i)
            {
                ISecurityElementFactory iseFactory = m_lChildren[i] as ISecurityElementFactory;
                if (iseFactory != null && !(m_lChildren[i] is SecurityElement))
                {
                    m_lChildren[i] = iseFactory.CreateSecurityElement();
                }
            }
        }

        internal ArrayList InternalChildren =>
                // Beware!  This array list can contain SecurityElements and other ISecurityElementFactories. 
                // If you want to get a consistent SecurityElement view, call get_Children.
                m_lChildren;

        //-------------------------- Public Methods -----------------------------

        internal void AddAttributeSafe(string name, string value)
        {
            if (m_lAttributes == null)
            {
                m_lAttributes = new ArrayList(c_AttributesTypical);
            }
            else
            {
                int iMax = m_lAttributes.Count;
                Contract.Assert(iMax % 2 == 0, "Odd number of strings means the attr/value pairs were not added correctly");

                for (int i = 0; i < iMax; i += 2)
                {
                    string strAttrName = (string)m_lAttributes[i];

                    if (string.Equals(strAttrName, name))
                    {
                        throw new ArgumentException("Argument_AttributeNamesMustBeUnique");
                    }
                }
            }

            m_lAttributes.Add(name);
            m_lAttributes.Add(value);
        }

        public void AddAttribute(string name, string value)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            if (!IsValidAttributeName(name))
            {
                throw new ArgumentException("Argument_InvalidElementName");
            }

            if (!IsValidAttributeValue(value))
            {
                throw new ArgumentException("Argument_InvalidElementValue");
            }

            Contract.EndContractBlock();

            AddAttributeSafe(name, value);
        }

        public void AddChild(SecurityElement child)
        {
            if (child == null)
            {
                throw new ArgumentNullException("child");
            }

            Contract.EndContractBlock();

            if (m_lChildren == null)
            {
                m_lChildren = new ArrayList(c_ChildrenTypical);
            }

            m_lChildren.Add(child);
        }

        [Runtime.InteropServices.ComVisible(false)]
        public SecurityElement Copy()
        {
            SecurityElement element = new SecurityElement(m_strTag, m_strText)
            {
                m_lChildren = m_lChildren == null ? null : new ArrayList(m_lChildren),
                m_lAttributes = m_lAttributes == null ? null : new ArrayList(m_lAttributes)
            };

            return element;
        }

        [Pure]
        public static bool IsValidTag(string tag)
        {
            if (tag == null)
            {
                return false;
            }

            return tag.IndexOfAny(s_tagIllegalCharacters) == -1;
        }

        [Pure]
        public static bool IsValidText(string text)
        {
            if (text == null)
            {
                return false;
            }

            return text.IndexOfAny(s_textIllegalCharacters) == -1;
        }

        [Pure]
        public static bool IsValidAttributeName(string name)
        {
            return IsValidTag(name);
        }

        [Pure]
        public static bool IsValidAttributeValue(string value)
        {
            if (value == null)
            {
                return false;
            }

            return value.IndexOfAny(s_valueIllegalCharacters) == -1;
        }

        private static string GetUnescapeSequence(string str, int index, out int newIndex)
        {
            int maxCompareLength = str.Length - index;

            int iMax = s_escapeStringPairs.Length;
            Contract.Assert(iMax % 2 == 0, "Odd number of strings means the attr/value pairs were not added correctly");

            for (int i = 0; i < iMax; i += 2)
            {
                var strEscSeq = s_escapeStringPairs[i];
                var strEscValue = s_escapeStringPairs[i + 1];

                int length = strEscValue.Length;

                if (length <= maxCompareLength && string.Compare(strEscValue, 0, str, index, length, StringComparison.Ordinal) == 0)
                {
                    newIndex = index + strEscValue.Length;
                    return strEscSeq;
                }
            }

            newIndex = index + 1;
            return str[index].ToString();
        }
        
        private static string Unescape(string str)
        {
            if (str == null)
            {
                return null;
            }

            StringBuilder sb = null;

            int strLen = str.Length;
            int index; // Pointer into the string that indicates the location of the current '&' character
            int newIndex = 0; // Pointer into the string that indicates the start index of the "remainging" string (that still needs to be processed). 

            do
            {
                index = str.IndexOf('&', newIndex);

                if (index == -1)
                {
                    if (sb == null)
                    {
                        return str;
                    }
                    else
                    {
                        sb.Append(str, newIndex, strLen - newIndex);
                        return sb.ToString();
                    }
                }
                else
                {
                    if (sb == null)
                    {
                        sb = new StringBuilder();
                    }

                    sb.Append(str, newIndex, index - newIndex);
                    sb.Append(GetUnescapeSequence(str, index, out newIndex)); // updates the newIndex too

                }
            }
            while (true);

            // C# reports a warning if I leave this in, but I still kinda want to just in case. 
            // Contract.Assert( false, "If you got here, the execution engine or compiler is really confused" );
            // return str; 
        }

        private delegate void ToStringHelperFunc(object obj, string str);

        private static void ToStringHelperStringBuilder(object obj, string str)
        {
            ((StringBuilder)obj).Append(str);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            ToString(sb, new ToStringHelperFunc(ToStringHelperStringBuilder));

            return sb.ToString();
        }

        private void ToString(object obj, ToStringHelperFunc func)
        {
            // First add the indent 

            // func( obj, indent ); 

            // Add in the opening bracket and the tag.

            func(obj, "<");

            switch (m_type)
            {
                case SecurityElementType.Format:
                    func(obj, "?");
                    break;

                case SecurityElementType.Comment:
                    func(obj, "!");
                    break;

                default:
                    break;
            }

            func(obj, m_strTag);

            // If there are any attributes, plop those in.

            if (m_lAttributes != null && m_lAttributes.Count > 0)
            {
                func(obj, " ");

                int iMax = m_lAttributes.Count;
                Contract.Assert(iMax % 2 == 0, "Odd number of strings means the attr/value pairs were not added correctly");

                for (int i = 0; i < iMax; i += 2)
                {
                    string strAttrName = (string)m_lAttributes[i];
                    string strAttrValue = (string)m_lAttributes[i + 1];

                    func(obj, strAttrName);
                    func(obj, "=\"");
                    func(obj, strAttrValue);
                    func(obj, "\"");

                    if (i != m_lAttributes.Count - 2)
                    {
                        if (m_type == SecurityElementType.Regular)
                        {
                            func(obj, Environment.NewLine);
                        }
                        else
                        {
                            func(obj, " ");
                        }
                    }
                }
            }

            if (m_strText == null && (m_lChildren == null || m_lChildren.Count == 0))
            {
                // If we are a single tag with no children, just add the end of tag text. 

                switch (m_type)
                {
                    case SecurityElementType.Comment:
                        func(obj, ">");
                        break;

                    case SecurityElementType.Format:
                        func(obj, " ?>");
                        break;

                    default:
                        func(obj, "/>");
                        break;
                }
                func(obj, Environment.NewLine);
            }
            else
            {
                // Close the current tag.

                func(obj, ">");

                // Output the text 

                func(obj, m_strText);

                // Output any children.

                if (m_lChildren != null)
                {
                    ConvertSecurityElementFactories();

                    func(obj, Environment.NewLine);

                    // String childIndent = indent + s_strIndent; 

                    for (int i = 0; i < m_lChildren.Count; ++i)
                    {
                        ((SecurityElement)m_lChildren[i]).ToString(obj, func);
                    }

                    // In the case where we have children, the close tag will not be on the same line as the 
                    // opening tag, so we need to indent.

                    // func( obj, indent );
                }

                // Output the closing tag 

                func(obj, "</");
                func(obj, m_strTag);
                func(obj, ">");
                func(obj, Environment.NewLine);
            }
        }

        internal string SearchForTextOfLocalName(string strLocalName)
        {
            if (strLocalName == null)
            {
                throw new ArgumentNullException("strLocalName");
            }

            Contract.EndContractBlock();

            if (m_strTag == null)
            {
                return null;
            }
            if (m_strTag.Equals(strLocalName) || m_strTag.EndsWith(":" + strLocalName, StringComparison.Ordinal))
            {
                return Unescape(m_strText);
            }

            if (m_lChildren == null)
            {
                return null;
            }

            var enumerator = m_lChildren.GetEnumerator();

            while (enumerator.MoveNext())
            {
                var current = ((SecurityElement)enumerator.Current).SearchForTextOfLocalName(strLocalName);

                if (current != null)
                {
                    return current;
                }
            }

            return null;
        }
    }
}
