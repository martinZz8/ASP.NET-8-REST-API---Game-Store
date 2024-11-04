using GameStore.Web.Consts;
using GameStore.Web.Dtos.Error;
using GameStore.Web.Dtos.GameGenre;
using GameStore.Web.Enums;
using GameStore.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameStore.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = UserRoleConsts.ADMINISTRATOR)]
    public class GameGenreController: ControllerBase
    {
        private readonly ILogger<GameController> _logger;
        private readonly IGameGenreService _gameGenreService;

        public GameGenreController(ILogger<GameController> logger, IGameGenreService gameGenreService)
        {
            _logger = logger;
            _gameGenreService = gameGenreService;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllGameGenres()
        {
            IEnumerable<GameGenreDto> gameGenres = await _gameGenreService.GetAllGameGenres();
            return Ok(gameGenres);
        }

        [HttpGet("{id}", Name = GameGenreControllerConsts.GET_GAME_GENDRE_BY_ID_NAME)]
        public async Task<IActionResult> GetGameGenreById([FromRoute]Guid id)
        {
            GameGenreDto? gameGenre = await _gameGenreService.GetGameGenreById(id);

            if (gameGenre == null)
            {
                return NotFound();
            }

            return Ok(gameGenre);
        }

        [HttpGet]
        public async Task<IActionResult> GetGameGenreByName([FromQuery(Name = "Name")]string name)
        {
            GameGenreDto? gameGenre = await _gameGenreService.GetGameGenreByName(name);

            if (gameGenre == null)
            {
                return NotFound();
            }

            return Ok(gameGenre);
        }

        [HttpPost]
        public async Task<IActionResult> AddGameGenre([FromBody]AddGameGenreDto gameGenreDto)
        {
            GameGenreDto? createdGameGenre = await _gameGenreService.AddGameGenre(gameGenreDto);

            if (createdGameGenre == null)
            {
                return Conflict(new ErrorDto()
                {
                    ErrorTypeName = Enum.GetName(ErrorTypeEnum.CREATE),
                    Message = "Error during creation of new game genre. Given game genre name is currently in use."
                });
            }

            return CreatedAtRoute(GameGenreControllerConsts.GET_GAME_GENDRE_BY_ID_NAME, new { id = createdGameGenre.Id }, createdGameGenre);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGameGenre([FromRoute]Guid id, [FromBody]UpdateGameGenreDto gameGenreDto)
        {
            GameGenreDto? updatedGameGenreDto = await _gameGenreService.UpdateGameGenre(id, gameGenreDto);

            if (updatedGameGenreDto == null)
            {
                return NotFound();
            }

            return Ok(updatedGameGenreDto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGameGenreById([FromRoute]Guid id)
        {
            bool isRemovedSuccessfully = await _gameGenreService.DeleteGameGenreById(id);

            if (!isRemovedSuccessfully)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteGameGenreByName([FromQuery(Name = "Name")]string name)
        {
            bool isRemovedSuccessfully = await _gameGenreService.DeleteGameGenreByName(name);

            if (!isRemovedSuccessfully)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
