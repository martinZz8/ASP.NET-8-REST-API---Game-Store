using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GameStore.Web.Services;
using GameStore.Web.Dtos.GameFileDescription.Full;
using GameStore.Web.Consts;
using GameStore.Web.Dtos.GameFileDescription;

namespace GameStore.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GameFileDescriptionController: ControllerBase
    {
        private readonly ILogger<GameFileDescriptionController> _logger;
        private readonly IGameFileDescriptionService _gameFileDescriptionService;

        public GameFileDescriptionController(ILogger<GameFileDescriptionController> logger, IGameFileDescriptionService gameFileDescriptionService)
        {
            _logger = logger;
            _gameFileDescriptionService = gameFileDescriptionService;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllGameFileDescriptions()
        {
            IEnumerable<GameFileDescriptionDto> gameFileDescriptions = await _gameFileDescriptionService.GetAllGameFileDescriptions();

            return Ok(gameFileDescriptions);
        }

        [HttpGet("full/all")]
        public async Task<IActionResult> GetAllGameFileDescriptionsFull()
        {
            IEnumerable<GameFileDescriptionDtoFull> gameFileDescriptionsFull = await _gameFileDescriptionService.GetAllGameFileDescriptionsFull();

            return Ok(gameFileDescriptionsFull);
        }

        [HttpGet("{id}", Name = GameFileDescriptionControllerConsts.GET_GAME_FILE_DESCRIPTION_BY_ID_NAME)]
        public async Task<IActionResult> GetGameFileDescriptionById([FromRoute]Guid id)
        {
            GameFileDescriptionDtoFull? foundGameFileDescription = await _gameFileDescriptionService.GetGameFileDescriptionById(id);

            if (foundGameFileDescription == null)
            {
                return NotFound();
            }

            return Ok(foundGameFileDescription);
        }

        [HttpGet("gameId/{gameId}", Name = GameFileDescriptionControllerConsts.GET_GAME_FILE_DESCRIPTION_BY_GAME_ID_NAME)]
        public async Task<IActionResult> GetGameFileDescriptionByGameId([FromRoute]Guid gameId)
        {
            ResultGameFileDescriptionDtoFull? resultGameFileDescription = await _gameFileDescriptionService.GetGameFileDescriptionByGameId(gameId);

            if (resultGameFileDescription.ErrorDto != null)
            {
                return NotFound(resultGameFileDescription.ErrorDto);
            }

            return Ok(resultGameFileDescription.GameFileDescriptionDtoFull);
        }

        [HttpPost]
        [Authorize(Roles = UserRoleConsts.ADMINISTRATOR)]
        // Note: We could also use data annotation "[FromForm] <Type> form" inside arguments of this function, to get parsed form data
        public async Task<IActionResult> AddGameFileDescription()
        {
            // take the form arguments based on: https://stackoverflow.com/questions/40798148/how-to-read-formdata-into-webapi
            ResultGameFileDescriptionDto resultGameDescription = await _gameFileDescriptionService.AddGameFileDescription(HttpContext.Request.Form);
            
            if (resultGameDescription.ErrorDto != null)
            {
                return Conflict(resultGameDescription.ErrorDto);
            }

            return CreatedAtRoute(GameFileDescriptionControllerConsts.GET_GAME_FILE_DESCRIPTION_BY_ID_NAME, new { id = resultGameDescription.GameFileDescriptionDto.Id }, resultGameDescription.GameFileDescriptionDto);
        }
    }
}
