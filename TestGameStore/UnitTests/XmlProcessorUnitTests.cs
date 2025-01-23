using GameStore.Web.Dtos.XmlProcessor;
using GameStore.Web.Helpers.XmlProcessor;
using GameStore.Web.Services;
using System.Xml.Linq;
using FluentAssertions;
using System.Globalization;

namespace TestGameStore.UnitTests
{
    public class XmlProcessorUnitTests
    {
        // - Services --
        private readonly IXmlProcessorService _xmlProcessorService;

        // -- Constructors --
        public XmlProcessorUnitTests()
        {
            _xmlProcessorService = GetXmlProcessorService();
        }

        // -- Test methods --
        // Note: Naming convention of testing methods: ClassName_MethodName_ExpectedResult  
        [Fact]
        public void XmlProcessorService_IncrementRating_IncrementRating()
        {
            // 1. Arrange - Get your variables and whatever you need before the "Act" state
            string xDocString = CreateSampleXDocument().ToStringWithDeclarations();
            SongRequestXmlDto songRequestXmlDto = xDocString.Deserialize<SongRequestXmlDto>();

            // 2. Act - Execute the methods to perform actions
            ResultIncrementSongRequestRatingDto result = _xmlProcessorService.IncrementRating(songRequestXmlDto);            

            // 3. Assert - Whatever is returned/performed after "Act" state is what you want
            result.ErrorDto.Should().BeNull();
            result.SongRequestXml.Should().NotBeNull();

            // Parse resulting xml string to SongRequestXmlDto
            SongRequestXmlDto resultSongRequestXmlDto = result.SongRequestXml.Deserialize<SongRequestXmlDto>();

            // Compare properties of the original and parsed objects
            songRequestXmlDto.Name.Should().Be(resultSongRequestXmlDto.Name);
            songRequestXmlDto.HoursLength.Should().Be(resultSongRequestXmlDto.HoursLength);
            songRequestXmlDto.ReleaseDate.Should().Be(resultSongRequestXmlDto.ReleaseDate);
            songRequestXmlDto.Writer.Gender.Should().Be(resultSongRequestXmlDto.Writer.Gender);
            songRequestXmlDto.Writer.Text.Should().Be(resultSongRequestXmlDto.Writer.Text);
            songRequestXmlDto.Singer.Gender.Should().Be(resultSongRequestXmlDto.Singer.Gender);
            songRequestXmlDto.Singer.Text.Should().Be(resultSongRequestXmlDto.Singer.Text);
            songRequestXmlDto.Genre.Should().Be(resultSongRequestXmlDto.Genre);
            songRequestXmlDto.Rating.Should().Be(resultSongRequestXmlDto.Rating-1); // Note: The service method incremented rating
            songRequestXmlDto.Tags.Length.Should().Be(resultSongRequestXmlDto.Tags.Length);

            for (int i = 0; i < songRequestXmlDto.Tags.Length; i++)
            {
                songRequestXmlDto.Tags[i].Weight.Should().Be(resultSongRequestXmlDto.Tags[i].Weight);
                songRequestXmlDto.Tags[i].Text.Should().Be(resultSongRequestXmlDto.Tags[i].Text);
            }
        }

        [Fact]
        public void XmlProcessorService_IncrementRating_HaventIncrementedRating()
        {
            // 1. Arrange - Get your variables and whatever you need before the "Act" state
            string xDocString = CreateSampleXDocument(10).ToStringWithDeclarations();
            SongRequestXmlDto songRequestXmlDto = xDocString.Deserialize<SongRequestXmlDto>();

            // 2. Act - Execute the methods to perform actions
            ResultIncrementSongRequestRatingDto result = _xmlProcessorService.IncrementRating(songRequestXmlDto);

            // 3. Assert - Whatever is returned/performed after "Act" state is what you want
            result.ErrorDto.Should().NotBeNull(); // Note: We couldn't increment rating because it's already 10, so we received error object
            result.SongRequestXml.Should().BeNull();
        }

        [Fact]
        public void XmlProcessorService_IncrementRatingFromXElement_IncrementRating()
        {
            // 1. Arrange - Get your variables and whatever you need before the "Act" state
            XDocument xDoc = CreateSampleXDocument();

            // 2. Act - Execute the methods to perform actions
            ResultIncrementSongRequestRatingDto result = _xmlProcessorService.IncrementRatingFromXElement(xDoc.Root);

            // 3. Assert - Whatever is returned/performed after "Act" state is what you want
            result.ErrorDto.Should().BeNull();
            result.SongRequestXml.Should().NotBeNull();

            // Parse resulting xml string to XDocument
            XDocument resulXDoc = XDocument.Parse(result.SongRequestXml);

            // Compare fields of the original and parsed objects
            xDoc.Root.Element("Name").Value.Should().Be(resulXDoc.Root.Element("Name").Value);
            double.Parse(xDoc.Root.Element("Length").Value, CultureInfo.InvariantCulture).Should().Be(double.Parse(resulXDoc.Root.Element("Length").Value, CultureInfo.InvariantCulture));
            DateOnly.Parse(xDoc.Root.Element("ReleaseDate").Value).Should().Be(DateOnly.Parse(resulXDoc.Root.Element("ReleaseDate").Value));
            xDoc.Root.Element("Writer").Attribute("gender").Value.Should().Be(resulXDoc.Root.Element("Writer").Attribute("gender").Value);
            xDoc.Root.Element("Writer").Value.Should().Be(resulXDoc.Root.Element("Writer").Value);
            xDoc.Root.Element("Singer").Attribute("gender").Value.Should().Be(resulXDoc.Root.Element("Singer").Attribute("gender").Value);
            xDoc.Root.Element("Singer").Value.Should().Be(resulXDoc.Root.Element("Singer").Value);
            xDoc.Root.Element("Genre").Value.Should().Be(resulXDoc.Root.Element("Genre").Value);
            Int32.Parse(xDoc.Root.Element("Rating").Value).Should().Be(Int32.Parse(resulXDoc.Root.Element("Rating").Value)-1); // Note: The service method incremented rating

            XElement[] xDocTags = xDoc.Root.Element("Tags").Elements("Tag").ToArray();
            XElement[] resulXDocTags = resulXDoc.Root.Element("Tags").Elements("Tag").ToArray();
            xDocTags.Count().Should().Be(resulXDocTags.Count());

            for (int i = 0; i < xDocTags.Count(); i++)
            {
                xDocTags[i].Attribute("weight").Value.Should().Be(resulXDocTags[i].Attribute("weight").Value);
                xDocTags[i].Value.Should().Be(resulXDocTags[i].Value);
            }
        }

        [Fact]
        public void XmlProcessorService_IncrementRatingFromXElement_HaventIncrementedRating()
        {
            // 1. Arrange - Get your variables and whatever you need before the "Act" state
            XDocument xDoc = CreateSampleXDocument(10);

            // 2. Act - Execute the methods to perform actions
            ResultIncrementSongRequestRatingDto result = _xmlProcessorService.IncrementRatingFromXElement(xDoc.Root);

            // 3. Assert - Whatever is returned/performed after "Act" state is what you want
            result.ErrorDto.Should().NotBeNull(); // Note: We couldn't increment rating because it's already 10, so we received error object
            result.SongRequestXml.Should().BeNull();
        }

        // -- Private methods --
        private IXmlProcessorService GetXmlProcessorService()
        {
            return new XmlProcessorService();
        }

        // from: https://stackoverflow.com/questions/2948255/xml-file-creation-using-xdocument-in-c-sharp
        // Note: When acquiring name of specific enum, we can use either:
        // - plain <specific_enum_value> (the underyling "ToString()" method is casted implicitly),
        // - method "<specific_enum_value>.ToString()",
        // - key word "nameof()",
        // - method "Enum.GetName(<specific_enum_value>)"
        private XDocument CreateSampleXDocument(int ratingToSet = 5)
        {
            return new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement("SongRequest",
                    new XElement("Name", "Oo Antava..Oo Oo Antava"),
                    new XElement("Length", 3.48),
                    new XElement("ReleaseDate", (new DateOnly(2021, 1, 2)).ToString("yyyy-MM-dd")),
                    new XElement("Writer", "Chandrabose", new XAttribute("gender", "male")),
                    new XElement("Singer", "Devi Sri Prasad", new XAttribute("gender", "male")),
                    new XElement("Genre", "Indian pop"),
                    new XElement("Rating", ratingToSet),
                    new XElement("Tags",
                        new XElement("Tag", TagEnum.DRAMA.ToString(), new XAttribute("weight", 1)),
                        new XElement("Tag", TagEnum.COMEDY.ToString(), new XAttribute("weight", 2))
                    )
                )
            );
        }
    }
}
