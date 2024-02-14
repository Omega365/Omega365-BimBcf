using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using System.Xml.Serialization;
using Xbim.BCF.XMLNodes;
using System;

namespace Xbim.BCF
{
    [XmlType("VisualizationInfo")]
    public class VisualizationXMLFile
    {
        [XmlElement(Order = 1)]
        public BCFComponents Components;
        [XmlElement(Order = 2)]
        public BCFOrthogonalCamera OrthogonalCamera { get; set; }
        [XmlElement(Order = 3)]
        public BCFPerspectiveCamera PerspectiveCamera { get; set; }
        [XmlArray(Order = 4)]
        public List<BCFLine> Lines;
        [XmlArray(Order = 5)]
        public List<BCFClippingPlane> ClippingPlanes;
        [XmlElement(ElementName = "Bitmaps", Order = 6)]
        public List<BCFBitmap> Bitmaps;

        public VisualizationXMLFile()
        {
            Components = new BCFComponents();
            Lines = new List<BCFLine>();
            ClippingPlanes = new List<BCFClippingPlane>();
            Bitmaps = new List<BCFBitmap>();
        }

        public VisualizationXMLFile(XDocument xdoc)
        {
            Lines = new List<BCFLine>();
            ClippingPlanes = new List<BCFClippingPlane>();
            Bitmaps = new List<BCFBitmap>();
            
            var orth = xdoc.Root.Elements("OrthogonalCamera").FirstOrDefault();
            if (orth != null)
            {
                OrthogonalCamera = new BCFOrthogonalCamera(orth);
            }
            var pers = xdoc.Root.Elements("PerspectiveCamera").FirstOrDefault();
            if (pers != null)
            {
                PerspectiveCamera = new BCFPerspectiveCamera(pers);
            }


            var selection = new List<BCFSelection>();
            var visibility = new List<BCFVisibility>();
            
            System.Xml.Linq.XElement componentElements = xdoc.Root.Element("Components");
            if (componentElements != null)
            {
                System.Xml.Linq.XElement selectionElements = componentElements.Element("Selection");
                if (selectionElements != null)
                {
                    foreach (var element in selectionElements.Elements("Component"))
                    {
                        selection.Add(new BCFSelection(element));
                    }
                }
                System.Xml.Linq.XElement visibilityElements = componentElements.Element("Visibility");
                if (visibilityElements != null)
                {
                    System.Xml.Linq.XElement exceptionElements = visibilityElements.Element("Exceptions");
                    if (exceptionElements != null)
                    {
                        foreach (var element in exceptionElements.Elements("Component"))
                        {
                            visibility.Add(new BCFVisibility(element));
                        }
                    }
                }
            }

            Components = new BCFComponents(selection, visibility);
            
            var lines = xdoc.Root.Elements("Lines").FirstOrDefault();
            if (lines != null)
            {
                foreach (var line in xdoc.Root.Element("Lines").Elements("Line"))
                {
                    Lines.Add(new BCFLine(line));
                }
            }
            var clippingplanes = xdoc.Root.Elements("ClippingPlanes").FirstOrDefault();
            if (clippingplanes != null)
            {
                foreach (var plane in xdoc.Root.Element("ClippingPlanes").Elements("ClippingPlane"))
                {
                    ClippingPlanes.Add(new BCFClippingPlane(plane));
                }
            }
            var bitmaps = xdoc.Root.Elements("Bitmaps").FirstOrDefault();
            if (bitmaps != null)
            {
                foreach (var bmap in xdoc.Root.Elements("Bitmaps"))
                {
                    Bitmaps.Add(new BCFBitmap(bmap));
                }
            }
        }
    }
}
