using System.Xml.Serialization;
using System.Xml;

namespace GameStore.Web.Helpers.XmlProcessor
{
    public static class XmlMapperBase
    {
        // Deserialize string xml into T
        // from: https://tutexchange.com/how-to-post-xml-data-to-asp-net-core-web-api-using-httpclient-from-net-core-console-application/
        public static T Deserialize<T>(this string xml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StringReader reader = new StringReader(xml))
            {
                return (T)serializer.Deserialize(reader);
            }
        }

        // Serialize T into string xml
        // from: https://tutexchange.com/how-to-post-xml-data-to-asp-net-core-web-api-using-httpclient-from-net-core-console-application/
        public static string Serialize<T>(this T obj)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StringWriter stringWriter = new StringWriter())
            {
                using (XmlWriter writer = XmlWriter.Create(stringWriter))
                {
                    serializer.Serialize(writer, obj);
                    return stringWriter.ToString();
                }
            }
        }
    }
}
