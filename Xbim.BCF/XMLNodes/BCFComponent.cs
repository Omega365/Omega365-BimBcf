using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Xbim.BCF.XMLNodes
{
    [XmlType("Component")]
    public class BCFComponent
    {
        private String _ifcGuid;
        /// <summary>
        /// The id of the component selected in a BIM tool
        /// </summary>
        [XmlAttribute]
        public String IfcGuid
        {
            get { return _ifcGuid; }
            set
            {
                if (value.Length == 22)
                {
                    _ifcGuid = value;
                }
                else
                {
                    throw new ArgumentException(this.GetType().Name + " - IfcGuid - IfcGuid must be 22 chars exactly");
                }
            }
        }
        public bool ShouldSerializeIfcGuid()
        {
            return !string.IsNullOrEmpty(IfcGuid);
        }
        /// <summary>
        /// Name of the system in which the component is originated
        /// </summary>
        [XmlElement(Order = 1)]
        public String OriginatingSystem { get; set; }
        public bool ShouldSerializeOriginatingSystem()
        {
            return !string.IsNullOrEmpty(OriginatingSystem);
        }
        /// <summary>
        /// System specific identifier of the component in the originating BIM tool
        /// </summary>
        [XmlElement(Order = 2)]
        public String AuthoringToolId { get; set; }
        public bool ShouldSerializeAuthoringToolId()
        {
            return !string.IsNullOrEmpty(AuthoringToolId);
        }
        /// <summary>
        /// This flag is true if the component is actually involved in the topic. If the flag is false, the component is involved as reference
        /// </summary>

        public BCFComponent()
        { }

        public BCFComponent(XElement node)
        {
            IfcGuid = (String)node.Attribute("IfcGuid") ?? "";
            OriginatingSystem = (String)node.Element("OriginatingSystem") ?? "";
            AuthoringToolId = (String)node.Element("AuthoringToolId") ?? "";
        }

        private bool IsHex(IEnumerable<char> chars)
        {
            bool isHex;
            foreach (var c in chars)
            {
                isHex = ((c >= '0' && c <= '9') ||
                         (c >= 'a' && c <= 'f') ||
                         (c >= 'A' && c <= 'F'));

                if (!isHex)
                    return false;
            }
            return true;
        }
    }
    public class BCFSelection : BCFComponent
    {
        [XmlAttribute]
        public bool Selected { get; set; }
        /// <summary>
        /// This flag is true when the component is visible in the visualization. 
        /// By setting this false, you can hide components that would prevent seeing the topic from the camera position and angle of the viewpoint.
        /// Default is true.
        /// </summary>

        public BCFSelection()
        { }

        public BCFSelection(XElement node) : base(node)
        {
            Selected = true;
        }
    }

    public class BCFVisibility : BCFComponent
    {
        [XmlAttribute]
        public bool Visibility { get; set; }
        /// <summary>
        /// This flag is true when the component is visible in the visualization. 
        /// By setting this false, you can hide components that would prevent seeing the topic from the camera position and angle of the viewpoint.
        /// Default is true.
        /// </summary>

        public BCFVisibility()
        { }

        public BCFVisibility(XElement node) : base(node)
        {
            bool defaultVisibility = (bool)node.Parent.Parent.Attribute("DefaultVisibility");
            Visibility = !defaultVisibility;
        }
    }

    [XmlType("Components")]

    public class BCFComponents
    {
        private List<BCFSelection> _selection;
        public List<BCFSelection> Selection
        {
            get { return _selection; } 
            set { _selection = value; }
        }

        private List<BCFVisibility> _visibility;
        public List<BCFVisibility> Visibility
        {
            get { return _visibility; }
            set { _visibility = value; }
        }

        public BCFComponents() 
        { }

        public BCFComponents(List<BCFSelection> selection, List<BCFVisibility> visibility) 
        {
            Selection = selection;
            Visibility = visibility;
        }
    }
}
