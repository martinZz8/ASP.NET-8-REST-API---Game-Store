using System.Xml.Serialization;

namespace GameStore.Web.Dtos.XmlProcessor
{
    // Note: Normally enums are serialized/deserialized by their names (not integer values hidden behind them)
    // We can use here "[XmlEnum(Name = "<number>")]" annotation before each of the enum values to change the way of serailization/deserialization of them
    // from: https://stackoverflow.com/questions/22668800/how-do-i-accept-an-int-in-xml-and-serialize-it-as-an-enum
    public enum TagEnum
    {
        COMEDY,
        DRAMA,
        THRILLER,
        HORROR,
        ACTION
    }

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

        [XmlArray(ElementName = "Tags")]
        [XmlArrayItem(ElementName = "Tag")]
        public SongRequestXmlTagDto[]? Tags { get; set; }
    }

    // from: https://stackoverflow.com/questions/6696603/c-sharp-xml-element-with-attribute-and-node-value
    public record SongRequestXmlPersonDto
    {
        [XmlAttribute(AttributeName = "gender")]
        public string Gender { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    public record SongRequestXmlTagDto
    {
        [XmlAttribute(AttributeName = "weight")]
        public int Weight { get; set; }

        [XmlText]
        public TagEnum Text { get; set; }
    }
}
