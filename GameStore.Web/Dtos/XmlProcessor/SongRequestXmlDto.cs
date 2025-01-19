using System.Xml.Serialization;

namespace GameStore.Web.Dtos.XmlProcessor
{
    // Note: The xml document is case sensitive in names
    [XmlRoot(ElementName = "SongRequest")]
    public record SongRequestXmlDto
    {
        [XmlElement(ElementName = "Name")]
        public string Name { get; set; }

        [XmlElement(ElementName = "Length")]
        public double HoursLength { get; set; }

        // Note: We need to use "DateTime" C# type instead of "DateOnly" (because deserializer doesn't support it)
        // Note2: Parsing only xsd "date" part instead of whole xsd "dateTime" (that specified the argument "DataType" of "XmlElement" decorator/data_annotator)
        [XmlElement(ElementName = "ReleaseDate", DataType = "date")]
        public DateTime ReleaseDate { get; set; }

        [XmlElement(ElementName = "Writer")]
        public SongRequestXmlPersonDto Writer { get; set; }

        [XmlElement(ElementName = "Singer")]
        public SongRequestXmlPersonDto Singer { get; set; }

        [XmlElement(ElementName = "Genre")]
        public string Genre { get; set; }

        [XmlElement(ElementName = "Rating")]
        public int Rating { get; set; }
    }

    // from: https://stackoverflow.com/questions/6696603/c-sharp-xml-element-with-attribute-and-node-value
    public record SongRequestXmlPersonDto
    {
        [XmlAttribute(AttributeName = "gender")]
        public string Gender { get; set; }

        [XmlText]
        public string Text { get; set; }
    }
}
