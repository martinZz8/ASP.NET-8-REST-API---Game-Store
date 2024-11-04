using GameStore.Web.Dtos.Error;
using GameStore.Web.Dtos.User;
using GameStore.Web.Services;
using Microsoft.AspNetCore.Mvc;
using GameStore.Web.Enums;
using Microsoft.AspNetCore.Authorization;
using GameStore.Web.Helpers;
using GameStore.Web.Consts;
using GameStore.Web.Dtos.User.Full;

namespace GameStore.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<GameController> _logger;
        private readonly IUserService _userService;

        public UserController(ILogger<GameController> logger, IUserService userSerive)
        {
            _logger = logger;
            _userService = userSerive;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterUserDto userDto)
        {
            ResultRegisterUserDto resultDto = await _userService.RegisterUser(userDto);

            if (resultDto.ErrorDto != null)
            {
                return Conflict(resultDto.ErrorDto);
            }

            return Created();
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginUser([FromBody] LoginUserDto userDto)
        {
            ResultLoginUserDto resultDto = await _userService.LoginUser(userDto);

            if (resultDto.ErrorDto != null)
            {
                return BadRequest(resultDto.ErrorDto with
                {
                    Message = "Error during login. Given email and/or password are wrong."
                });
            }

            return Ok(new SuccessfullyLoggedUserDto()
            {
                JWT = resultDto.JWT!,
                User = resultDto.User!
            });
        }        

        [HttpGet("all")]
        [Authorize(Roles = UserRoleConsts.ADMINISTRATOR)]
        public async Task<IActionResult> GetAllUsers()
        {
            IEnumerable<UserDtoFull> userDtos = await _userService.GetAllUsers();
            return Ok(userDtos);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = UserRoleConsts.ADMINISTRATOR)]
        public async Task<IActionResult> GetUserById([FromRoute] Guid id)
        {
            UserDtoFull? userDto = await _userService.GetUserById(id);

            if (userDto == null)
            {
                return NotFound();
            }

            return Ok(userDto);
        }

        [HttpGet(Name = UserControllerConsts.GET_CURRENT_USER_DATA_NAME)]
        [Authorize]
        public async Task<IActionResult> GetCurrentUserData()
        {
            // Get user Id from JWT token claim
            // from: https://stackoverflow.com/questions/45315274/get-claims-from-a-webapi-controller-jwt-token
            string? userId = UserJWTHelper.GetUserId(HttpContext);

            if (userId == null)
            {
                return NotFound(new ErrorDto()
                {
                    ErrorTypeName = Enum.GetName(ErrorTypeEnum.READ),
                    Message = "Error during try to read user data. Probably given JWT (bearer token) is invalid."
                });
            }

            if (!Guid.TryParse(userId, out Guid userIdGuid))
            {
                return Conflict(new ErrorDto()
                {
                    ErrorTypeName = Enum.GetName(ErrorTypeEnum.READ),
                    Message = "Error during try to read user data. Given id has invalid format (it's not Guid format)."
                });
            }

            UserDtoFull? userDto = await _userService.GetUserById(userIdGuid);

            if (userDto == null)
            {
                return NotFound(new ErrorDto()
                {
                    ErrorTypeName = Enum.GetName(ErrorTypeEnum.READ),
                    Message = "Error during try to read user data. Probably given JWT (bearer token) is invalid."
                });
            }

            return Ok(userDto); // Note: For custom status code create new object of class "ObjectResult". We pass body object as constructor argument and set "StatusCode" property in object initializer.
        }

        [HttpPut("{id}")]
        [Authorize(Roles = UserRoleConsts.ADMINISTRATOR)]
        public async Task<IActionResult> UpdateUser([FromRoute]Guid id, [FromBody]UpdateUserDto userDto)
        {
            ResultUpdateUserDto? resultDto = await _userService.UpdateUser(id, userDto);

            if (resultDto.ErrorDto != null)
            {
                return Conflict(resultDto.ErrorDto);
            }

            if (resultDto.User == null)
            {
                return NotFound();
            }

            return Ok(resultDto.User);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = UserRoleConsts.ADMINISTRATOR)]
        public async Task<IActionResult> DeleteUser([FromRoute]Guid id)
        {
            bool isRemovedSuccessfully = await _userService.DeleteUserById(id);

            if (!isRemovedSuccessfully)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
