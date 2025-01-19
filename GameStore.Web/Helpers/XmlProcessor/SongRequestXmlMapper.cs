using GameStore.Web.Dtos.XmlProcessor;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace GameStore.Web.Helpers.XmlProcessor
{
    public static class SongRequestXmlMapper
    {
        // SongRequestXml object copy
        public static SongRequestXmlDto Copy(this SongRequestXmlDto songRequestXml)
        {
            return new SongRequestXmlDto()
            {
                Name = songRequestXml.Name,
                HoursLength = songRequestXml.HoursLength,
                ReleaseDate = songRequestXml.ReleaseDate,
                Writer = new SongRequestXmlPersonDto()
                {
                    Gender = songRequestXml.Writer.Gender,
                    Text = songRequestXml.Writer.Text
                },
                Singer = new SongRequestXmlPersonDto()
                {
                    Gender = songRequestXml.Singer.Gender,
                    Text = songRequestXml.Singer.Text
                },
                Genre = songRequestXml.Genre,
                Rating = songRequestXml.Rating
            };
        }

        // Deserialize string xml into SongRequestXml
        // from: https://tutexchange.com/how-to-post-xml-data-to-asp-net-core-web-api-using-httpclient-from-net-core-console-application/
        public static SongRequestXmlDto Deserialize(this string xml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SongRequestXmlDto));
            using (StringReader reader = new StringReader(xml))
            {
                return (SongRequestXmlDto)serializer.Deserialize(reader);
            }
        }

        // Serialize SongRequestXml into string xml
        // from: https://tutexchange.com/how-to-post-xml-data-to-asp-net-core-web-api-using-httpclient-from-net-core-console-application/
        public static string Serialize(this SongRequestXmlDto obj)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SongRequestXmlDto));
            using (StringWriter stringWriter = new StringWriter())
            {
                using (XmlWriter writer = XmlWriter.Create(stringWriter))
                {
                    serializer.Serialize(writer, obj);
                    return stringWriter.ToString();
                }
            }
        }

        /// <summary>
        /// Method to return xml string wtih declarations from given XDocument object
        /// </summary>
        /// <returns>Xml document with declarations</returns>
        /// Note: "StringBuilder" instance is not needed in here
        public static string ToStringWithDeclarations(this XDocument xDoc)
        {
            StringBuilder builder = new StringBuilder();
            using (StringWriter writer = new StringWriter(builder))
            {
                xDoc.Save(writer);
                return builder.ToString();
            }
        }
    }
}
