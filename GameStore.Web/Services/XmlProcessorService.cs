using GameStore.Web.Dtos.Error;
using GameStore.Web.Dtos.XmlProcessor;
using GameStore.Web.Enums;
using GameStore.Web.Helpers.XmlProcessor;
using System.Text;
using System.Xml.Linq;

namespace GameStore.Web.Services
{
    public interface IXmlProcessorService
    {
        ResultIncrementSongRequestRatingDto IncrementRating(SongRequestXmlDto songRequestXml);
        ResultIncrementSongRequestRatingDto IncrementRatingFromXElement(XElement songRequestXml);
    }

    public class XmlProcessorService : IXmlProcessorService
    {
        #region Public methods
        public XmlProcessorService()
        {}

        public ResultIncrementSongRequestRatingDto IncrementRating(SongRequestXmlDto songRequestXml)
        {
            SongRequestXmlDto songRequestXmlCopy = songRequestXml.Copy();

            // Increment rating if possible
            if (IncrementRatingIfPossible(songRequestXmlCopy))
                return new ResultIncrementSongRequestRatingDto()
                {
                    SongRequestXml = songRequestXmlCopy.Serialize()
                };

            return new ResultIncrementSongRequestRatingDto()
            {
                ErrorDto = new ErrorDto()
                {
                    Message = "Couldn't increment rating, because it has currently max value (10)",
                    ErrorTypeName = Enum.GetName(ErrorTypeEnum.UPDATE)
                }
            };
        }

        public ResultIncrementSongRequestRatingDto IncrementRatingFromXElement(XElement songRequestXml)
        {
            XElement rootCopy = new XElement(songRequestXml);
            XElement? ratingNode = rootCopy.Element("Rating");

            // Check if we found node "Rating"
            if (ratingNode != null)
            {
                // Try to parse rating node value into string
                if (Int32.TryParse(ratingNode.Value, out int currentRating))
                {
                    // Check if we can increment rating
                    if (currentRating < 10)
                    {
                        ratingNode.Value = (currentRating + 1).ToString();

                        // Create response xml document
                        // Note: "responseDoc.ToString()" doesn't include declarations, so we use "responseDoc.Save()" (from: https://stackoverflow.com/questions/28183461/xdocument-xdeclaration-not-appearing-in-tostring-result)
                        XDocument responseDoc = new XDocument(new XDeclaration("1.0", "utf-8", null));
                        responseDoc.Add(rootCopy);

                        return new ResultIncrementSongRequestRatingDto()
                        {
                            SongRequestXml = responseDoc.ToStringWithDeclarations()
                        };
                    }                        
                    else
                    {
                        return new ResultIncrementSongRequestRatingDto()
                        {
                            ErrorDto = new ErrorDto()
                            {
                                Message = "Couldn't increment rating, because it has currently max value (10)",
                                ErrorTypeName = Enum.GetName(ErrorTypeEnum.UPDATE)
                            }
                        };
                    }
                }
                else
                {
                    return new ResultIncrementSongRequestRatingDto()
                    {
                        ErrorDto = new ErrorDto()
                        {
                            Message = "Couldn't parse \"Rating\" node value to integer",
                            ErrorTypeName = Enum.GetName(ErrorTypeEnum.UPDATE)
                        }
                    };
                }                
            }
            else
            {
                return new ResultIncrementSongRequestRatingDto()
                {
                    ErrorDto = new ErrorDto()
                    {
                        Message = "Couldn't find \"Rating\" node",
                        ErrorTypeName = Enum.GetName(ErrorTypeEnum.UPDATE)
                    }
                };
            }
        }
        #endregion

        #region Private methods        
        /// <summary>
        /// Method to increment rating when possible - it means, when current rating is less than 10
        /// </summary>
        /// <param name="songRequestXml"></param>
        /// <returns>True, if we incremented rating, otherwise false</returns>
        private bool IncrementRatingIfPossible(SongRequestXmlDto songRequestXml)
        {
            if (songRequestXml.Rating < 10)
            {
                songRequestXml.Rating++;
                return true;
            }

            return false;
        }
        #endregion
    }
}
