using System;
using System.Xml;

namespace Eto.Drawing
{
	/// <summary>
	/// Xml extensions to read/write Eto.Drawing structs to xml
	/// </summary>
	/// <copyright>(c) 2014 by Curtis Wensley</copyright>
	/// <license type="BSD-3">See LICENSE for full terms</license>
	public static class XmlExtensions
	{
		/// <summary>
		/// Gets a <see cref="Size"/> struct as a set of attributes of the specified <paramref name="element"/>
		/// </summary>
		/// <remarks>
		/// This will read attributes with suffixes "-width" and "-height" prefixed by <paramref name="baseName"/>.
		/// For example, if you specify "myProperty" as the base name, then it will read attributes "myProperty-width" and "myProperty-height".
		/// 
		/// Both the width and height must be specified as attributes for this to return a value.
		/// </remarks>
		/// <param name="element">Element to read the width and height attributes from</param>
		/// <param name="baseName">Base attribute name prefix</param>
		/// <returns>A size struct if both the width and height attributes are specified, or null otherwise</returns>
		public static Size? GetSizeAttributes(this XmlElement element, string baseName)
		{
			var width = element.GetIntAttribute(baseName + "-width");
			var height = element.GetIntAttribute(baseName + "-height");
			if (width != null && height != null)
				return new Size(width.Value, height.Value);
			return null;
		}

		/// <summary>
		/// Sets attributes on the specified <paramref name="element"/> with width and height attributes of the specified value
		/// </summary>
		/// <remarks>
		/// This will write attributes with suffixes "-width" and "-height" prefixed by <paramref name="baseName"/>.
		/// For example, if you specify "myProperty" as the base name, then it will write attributes "myProperty-width" and "myProperty-height".
		/// 
		/// Passing null as the size will not write either attribute value.
		/// </remarks>
		/// <param name="element">Element to write the width and height attributes on</param>
		/// <param name="baseName">Base attribute name prefix</param>
		/// <param name="value">Value to set the width and height attributes, if not null</param>
		public static void SetSizeAttributes(this XmlElement element, string baseName, Size? value)
		{
			if (value != null)
			{
				element.SetAttribute(baseName + "-width", value.Value.Width);
				element.SetAttribute(baseName + "-height", value.Value.Height);
			}
		}

		/// <summary>
		/// Writes the specified size <paramref name="value"/> to a child of the specified <paramref name="element"/> with the given name
		/// </summary>
		/// <remarks>
		/// The child element will contain "width" and "height" attributes for the value of the size.
		/// If the value is null, no child element will be written.
		/// </remarks>
		/// <param name="element">Element to append the child element to if <paramref name="value"/> is not null</param>
		/// <param name="elementName">Name of the element to append</param>
		/// <param name="value">Size value to write</param>
		public static void WriteChildSizeXml(this XmlElement element, string elementName, Size? value)
		{
			if (value != null)
				element.WriteChildXml(elementName, new SizeSaver { Size = value.Value });
		}

		/// <summary>
		/// Reads a child of the <paramref name="element"/> with the given <paramref name="elementName"/> as a <see cref="Size"/>
		/// </summary>
		/// <remarks>
		/// The child element must contain both "width" and "height" attributes for the value of the size.
		/// </remarks>
		/// <param name="element">Element to read from</param>
		/// <param name="elementName">Name of the element to read into the Size struct</param>
		/// <returns>A new Size struct if the element exists, or null if not</returns>
		public static Size? ReadChildSizeXml(this XmlElement element, string elementName)
		{
			var size = element.ReadChildXml<SizeSaver>(elementName);
			return size == null ? null : (Size?)size.Size;
		}

		class SizeSaver : IXmlReadable
		{
			public Size Size { get; set; }

			public void ReadXml(XmlElement element)
			{
				var width = element.GetIntAttribute("width") ?? 0;
				var height = element.GetIntAttribute("height") ?? 0;
				Size = new Size(width, height);
			}

			public void WriteXml(XmlElement element)
			{
				element.SetAttribute("width", Size.Width);
				element.SetAttribute("height", Size.Height);
			}
		}

		/// <summary>
		/// Writes the child rectangle xml.
		/// </summary>
		/// <param name="element">Element.</param>
		/// <param name="elementName">Element name.</param>
		/// <param name="rect">Rect.</param>
		public static void WriteChildRectangleXml(this XmlElement element, string elementName, Rectangle? rect)
		{
			if (rect != null)
				element.WriteChildXml(elementName, new RectSaver { Rectangle = rect.Value });
		}

		/// <summary>
		/// Reads the child rectangle xml.
		/// </summary>
		/// <returns>The child rectangle xml.</returns>
		/// <param name="element">Element.</param>
		/// <param name="elementName">Element name.</param>
		public static Rectangle? ReadChildRectangleXml(this XmlElement element, string elementName)
		{
			var rect = element.ReadChildXml<RectSaver>(elementName);
			return rect == null ? null : (Rectangle?)rect.Rectangle;
		}

		class RectSaver : IXmlReadable
		{
			public Rectangle Rectangle { get; set; }

			public void ReadXml(XmlElement element)
			{
				var ps = new PointSaver();
				ps.ReadXml(element);
				var ss = new SizeSaver();
				ss.ReadXml(element);
				Rectangle = new Rectangle(ps.Point, ss.Size);
			}

			public void WriteXml(XmlElement element)
			{
				var ps = new PointSaver { Point = Rectangle.Location };
				ps.WriteXml(element);
				var ss = new SizeSaver { Size = Rectangle.Size };
				ss.WriteXml(element);
			}
		}

		/// <summary>
		/// Writes the child point xml.
		/// </summary>
		/// <param name="element">Element.</param>
		/// <param name="elementName">Element name.</param>
		/// <param name="point">Point.</param>
		public static void WriteChildPointXml(this XmlElement element, string elementName, Point? point)
		{
			if (point != null)
				element.WriteChildXml(elementName, new PointSaver { Point = point.Value });
		}

		/// <summary>
		/// Reads the child point xml.
		/// </summary>
		/// <returns>The child point xml.</returns>
		/// <param name="element">Element.</param>
		/// <param name="elementName">Element name.</param>
		public static Point? ReadChildPointXml(this XmlElement element, string elementName)
		{
			var point = element.ReadChildXml<PointSaver>(elementName);
			return point == null ? null : (Point?)point.Point;
		}

		class PointSaver : IXmlReadable
		{
			public Point Point { get; set; }

			public void ReadXml(XmlElement element)
			{
				var x = element.GetIntAttribute("x") ?? 0;
				var y = element.GetIntAttribute("y") ?? 0;
				Point = new Point(x, y);
			}

			public void WriteXml(XmlElement element)
			{
				element.SetAttribute("x", Point.X);
				element.SetAttribute("y", Point.Y);
			}
		}
	}
}