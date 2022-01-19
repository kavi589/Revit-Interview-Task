using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BeamHoleCreation
{
	[XmlRoot(ElementName = "Hole")]
	public class Hole
	{

		[XmlElement(ElementName = "Units")]
		public string Units { get; set; }

		[XmlElement(ElementName = "BeamLength")]
		public  int BeamLength { get; set; }

		[XmlElement(ElementName = "Shape")]
		public  string Shape { get; set; }

		[XmlElement(ElementName = "HeightDiameter")]
		public  double HeightDiameter { get; set; }

		[XmlElement(ElementName = "LengthValue")]
		public  double LengthValue { get; set; }

		[XmlElement(ElementName = "SpacingValue")]
		public  double SpacingValue { get; set; }

		[XmlElement(ElementName = "CellCountValue")]
		public  int CellCountValue { get; set; }

		[XmlElement(ElementName = "VerticalMode")]
		public  string VerticalMode { get; set; }

		[XmlElement(ElementName = "EndPostValue")]
		public  int EndPostValue { get; set; }
	}
}
