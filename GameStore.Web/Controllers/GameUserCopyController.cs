using GameStore.Web.Consts;
using GameStore.Web.Dtos.Error;
using GameStore.Web.Dtos.GameUserCopy;
using GameStore.Web.Enums;
using GameStore.Web.Helpers;
using GameStore.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameStore.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GameUserCopyController : ControllerBase
    {
        private readonly ILogger<GameController> _logger;
        private readonly IGameUserCopyService _gameUserCopyService;

        public GameUserCopyController(ILogger<GameController> logger, IGameUserCopyService gameUserCopyService)
        {
            _logger = logger;
            _gameUserCopyService = gameUserCopyService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddGameUserCopy([FromBody]AddGameUserCopy gameUserCopy)
        {
            GameUserCopyDto? gameUserCopyDto = await _gameUserCopyService.AddGameUserCopy(gameUserCopy, new Guid(UserJWTHelper.GetUserId(HttpContext)));

            if (gameUserCopyDto == null)
            {
                return Conflict(new ErrorDto()
                {
                    ErrorTypeName = Enum.GetName(ErrorTypeEnum.CREATE),
                    Message = "Error during creation of game user copy. There doesn't exist game with given id."
                });
            }

            return CreatedAtRoute(UserControllerConsts.GET_CURRENT_USER_DATA_NAME, gameUserCopyDto);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteGameUserCopy([FromRoute]Guid id)
        {
            ResultDeleteGameUserCopyDto resultDto = await _gameUserCopyService.DeleteGameUserCopy(id, new Guid(UserJWTHelper.GetUserId(HttpContext)));

            if (resultDto.ErrorExtendedDto != null)
            {
                if (resultDto.ErrorExtendedDto.IsConflict)
                {
                    return Conflict(resultDto.ErrorExtendedDto.ToErrorDto());
                }

                return NotFound(resultDto.ErrorExtendedDto.ToErrorDto());
            }

            return NoContent();
        }
    }
}
