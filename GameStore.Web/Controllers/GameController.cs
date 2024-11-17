using GameStore.Web.Consts;
using GameStore.Web.Dtos.Error;
using GameStore.Web.Dtos.Game;
using GameStore.Web.Services;
using Microsoft.AspNetCore.Mvc;
using GameStore.Web.Enums;
using Microsoft.AspNetCore.Authorization;
using GameStore.Web.Dtos.Game.Full;

namespace GameStore.Web.Controllers
{
    // securing controller from: https://learn.microsoft.com/en-us/aspnet/web-api/overview/security/authentication-and-authorization-in-aspnet-web-api
    // way to set authorization policies: https://www.telerik.com/blogs/asp-net-core-basics-authentication-authorization-jwt
    // way to set multiple required roles: https://stackoverflow.com/questions/49426781/jwt-authentication-by-role-claims-in-asp-net-core-identity
    [ApiController]
    [Route("[controller]")]
    public class GameController: ControllerBase
    {
        private readonly ILogger<GameController> _logger;
        private readonly IGameSerive _gameService;

        public GameController(ILogger<GameController> logger, IGameSerive gameSerive)
        {
            _logger = logger;
            _gameService = gameSerive;
        }

        // Note: We can use "FromUri" (with object to map) instead of many "FromQuery" params
        // Note2: To query param "Sort" we can pass strings: "GameGenre", "Name", "OnSale"
        // Note3: To query param "OrderBy" we can pass strings: "Name", "Genre", "CreateDate"
        [HttpGet("all")]
        public async Task<IActionResult> GetAllGames(
            [FromQuery(Name = "FilterGameGenre")]string? sortGameGenre = null,
            [FromQuery(Name = "FilterName")]string? sortName = null,
            [FromQuery(Name = "FilterOnSale")]bool? sortOnSale = null,
            [FromQuery(Name = "OrderBy")]string? orderBy = null,
            [FromQuery(Name = "OrderAscending")]bool orderAscending = true
        )
        {
            ResultGetAllGamesDto<GameDto> resultGames = await _gameService.GetAllGames<GameDto>(sortGameGenre, sortName, sortOnSale, orderBy, orderAscending);

            if (resultGames.ErrorDto != null)
            {
                return Conflict(resultGames.ErrorDto);
            }

            return Ok(resultGames.GamesDto);
        }

        [HttpGet("full/all")]
        [Authorize(Roles = UserRoleConsts.ADMINISTRATOR)]
        public async Task<IActionResult> GetAllGamesFull(
            [FromQuery(Name = "FilterGameGenre")] string? sortGameGenre = null,
            [FromQuery(Name = "FilterName")] string? sortName = null,
            [FromQuery(Name = "FilterOnSale")] bool? sortOnSale = null,
            [FromQuery(Name = "OrderBy")] string? orderBy = null,
            [FromQuery(Name = "OrderAscending")] bool orderAscending = true
        )
        {
            ResultGetAllGamesDto<GameDtoFull> gameDtos = await _gameService.GetAllGames<GameDtoFull>(sortGameGenre, sortName, sortOnSale, orderBy, orderAscending);
            return Ok(gameDtos);
        }

        [HttpGet("{id}", Name=GameControllerConsts.GET_GAME_BY_ID_NAME)]
        public async Task<IActionResult> GetGameById([FromRoute]Guid id) //alternative: "[FromQuery]"
        {
            GameDto? foundGame = await _gameService.GetGameById<GameDto>(id);

            if (foundGame == null)
            {
                return NotFound();
            }

            return Ok(foundGame);
        }

        [HttpGet("full/{id}", Name = GameControllerConsts.GET_GAME_FULL_BY_ID_NAME)]
        [Authorize(Roles = UserRoleConsts.ADMINISTRATOR)]
        public async Task<IActionResult> GetGameFullById([FromRoute] Guid id)
        {
            GameDtoFull? foundGame = await _gameService.GetGameById<GameDtoFull>(id);

            if (foundGame == null)
            {
                return NotFound();
            }

            return Ok(foundGame);
        }

        [HttpPost]
        [Authorize(Roles = UserRoleConsts.ADMINISTRATOR)]
        public async Task<IActionResult> AddGame([FromBody]AddGameDto gameDto)
        {
            GameDto? createdGameDto = await _gameService.AddGame(gameDto);

            if (createdGameDto == null)
            {
                return Conflict(new ErrorDto()
                {
                    ErrorTypeName = Enum.GetName(ErrorTypeEnum.CREATE),
                    Message = "Error during creation of new game. Probably given name of genres are invalid (some of them might not exist)."
                });
            }

            // Note: The below function adds newly created game to the body of response.
            // It also adds the url to retrieve that element back again.
            return CreatedAtRoute(GameControllerConsts.GET_GAME_BY_ID_NAME, new { id = createdGameDto.Id }, createdGameDto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = UserRoleConsts.ADMINISTRATOR)]
        public async Task<IActionResult> UpdateGame([FromRoute]Guid id, [FromBody]UpdateGameDto gameDto)
        {
            GameDto? updatedGameDto = await _gameService.UpdateGame(id, gameDto);

            if (updatedGameDto == null)
            {
                return NotFound();
            }

            return Ok(updatedGameDto);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = UserRoleConsts.ADMINISTRATOR)]
        public async Task<IActionResult> DeleteGameById([FromRoute]Guid id)
        {
            bool isRemovedSuccessfully = await _gameService.DeleteGameById(id);

            if (!isRemovedSuccessfully)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
