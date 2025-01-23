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
                Rating = songRequestXml.Rating,
                Tags = songRequestXml.Tags?.Select(it => new SongRequestXmlTagDto()
                {
                    Weight = it.Weight,
                    Text = it.Text
                }).ToArray()
            };
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
