using GameStore.Web.Dtos.XmlProcessor;
using GameStore.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Xml.Linq;

namespace GameStore.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class XmlProcessorController : ControllerBase
    {
        private readonly ILogger<GameController> _logger;
        private readonly IXmlProcessorService _xmlProcessorService;

        public XmlProcessorController(ILogger<GameController> logger, IXmlProcessorService xmlProcessorService)
        {
            _logger = logger;
            _xmlProcessorService = xmlProcessorService;
        }

        [HttpPut("increment-rating")]
        [Consumes("application/xml")] // Note: That's not needed here
        public IActionResult IncrementRating([FromBody] SongRequestXmlDto songRequestXml)
        {
            ResultIncrementSongRequestRatingDto resultIncrement = _xmlProcessorService.IncrementRating(songRequestXml);
            
            if (resultIncrement.ErrorDto != null)
                // Converting error object -> JSON string -> XML node
                return Conflict(JsonConvert.DeserializeXmlNode(
                    JsonConvert.SerializeObject(resultIncrement.ErrorDto),
                    "Error"
                ).OuterXml);

            return Ok(resultIncrement.SongRequestXml);
        }

        [HttpPut("increment-rating-from-xelement")]
        [Consumes("application/xml")] // Note: That's not needed here
        // Note: We need to pass here "XElement" instead of "XDocument" (or plain string). This element is root element of the passed xml (so it's the "SongRequest" tag)
        public IActionResult IncrementRatingFromXElement([FromBody] XElement songRequestXml)
        {
            ResultIncrementSongRequestRatingDto resultIncrement = _xmlProcessorService.IncrementRatingFromXElement(songRequestXml);

            if (resultIncrement.ErrorDto != null)
                // Converting error object -> JSON string -> XML node
                return Conflict(JsonConvert.DeserializeXmlNode(
                    JsonConvert.SerializeObject(resultIncrement.ErrorDto),
                    "Error"
                ).OuterXml);

            return Ok(resultIncrement.SongRequestXml);
        }
    }
}
