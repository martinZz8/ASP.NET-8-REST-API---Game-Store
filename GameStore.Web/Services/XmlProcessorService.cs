using GameStore.Web.Dtos.Error;
using GameStore.Web.Dtos.XmlProcessor;
using GameStore.Web.Enums;
using GameStore.Web.Helpers.XmlProcessor;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
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

            // Validate Tag nodes (whether "weight" attributes are numerous and enum values match each other)
            // These elements are not mandatory
            IEnumerable<XElement>? tagNodes = rootCopy.Element("Tags").Elements("Tag");

            if (tagNodes != null)
            {
                foreach(XElement tagNode in tagNodes)
                {
                    // Check "weight" attribute (wehther it's integer with double quotes)
                    string weightStr = tagNode.Attribute("weight")?.Value;

                    if (weightStr != null)
                    {
                        if (!Int32.TryParse(weightStr, out int weightInt))
                        {
                            return new ResultIncrementSongRequestRatingDto()
                            {
                                ErrorDto = new ErrorDto()
                                {
                                    Message = "\"Tag\" node has \"weight\" attribute that's not integer (with double quotes)",
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
                                Message = "\"Tag\" node is missing mandatory \"weight\" attribute",
                                ErrorTypeName = Enum.GetName(ErrorTypeEnum.UPDATE)
                            }
                        };
                    }

                    // Check value (whether is available enum)
                    string enumStr = tagNode.Value;

                    if (!enumStr.IsNullOrEmpty())
                    {
                        // Check if enumStr is number (we don't accept them, howewer we could parse them to enum or check of their existence)
                        if (Int32.TryParse(enumStr, out int parsedEnumStr))
                        {
                            return new ResultIncrementSongRequestRatingDto()
                            {
                                ErrorDto = new ErrorDto()
                                {
                                    Message = "\"Tag\" node contains value which is integer (you need to specify string representation)",
                                    ErrorTypeName = Enum.GetName(ErrorTypeEnum.UPDATE)
                                }
                            };
                        }

                        // Checking if value (string name) exists inside our "TagEnum"
                        // Note: We can do it by either using "Enum.TryParse()" method (which accepts string, or integer as string) or "Enum.IsDefined()" (which accepts object - string or integer)
                        // from https://stackoverflow.com/questions/29482/how-do-i-cast-int-to-enum-in-c
                        if (!Enum.IsDefined(typeof(TagEnum), enumStr))
                        {
                            return new ResultIncrementSongRequestRatingDto()
                            {
                                ErrorDto = new ErrorDto()
                                {
                                    Message = $"\"Tag\" node contains value which is not accepted. Accepted values: {string.Join(", " , Enum.GetNames<TagEnum>())}",
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
                                Message = "\"Tag\" node is missing mandatory value",
                                ErrorTypeName = Enum.GetName(ErrorTypeEnum.UPDATE)
                            }
                        };
                    }
                }
            }

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
                        XDocument responseDoc = new XDocument(new XDeclaration("1.0", "utf-8", null));
                        responseDoc.Add(rootCopy);

                        // Note: "responseDoc.ToString()" doesn't include declarations, so we use "responseDoc.Save()", which is implemented in extensions method "responseDoc.ToStringWithDeclarations()"
                        // from: https://stackoverflow.com/questions/28183461/xdocument-xdeclaration-not-appearing-in-tostring-result
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
