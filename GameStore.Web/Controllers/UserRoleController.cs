using GameStore.Web.Consts;
using GameStore.Web.Dtos.Error;
using GameStore.Web.Dtos.GameGenre;
using GameStore.Web.Dtos.UserRole;
using GameStore.Web.Enums;
using GameStore.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameStore.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = UserRoleConsts.ADMINISTRATOR)]
    public class UserRoleController : ControllerBase
    {
        private readonly ILogger<UserRoleController> _logger;
        private readonly IUserRoleService _userRoleService;

        public UserRoleController(ILogger<UserRoleController> logger, IUserRoleService userRoleService)
        {
            _logger = logger;
            _userRoleService = userRoleService;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllUserRoles()
        {
            IEnumerable<UserRoleDto> userRoles = await _userRoleService.GetAllUserRoles();
            return Ok(userRoles);
        }

        [HttpGet("{id}", Name = UserRoleControllerConsts.GET_USER_ROLE_BY_ID_NAME)]
        public async Task<IActionResult> GetUserRoleById([FromRoute] Guid id)
        {
            UserRoleDto? userRole = await _userRoleService.GetUserRoleById(id);

            if (userRole == null)
            {
                return NotFound();
            }

            return Ok(userRole);
        }

        [HttpGet]
        public async Task<IActionResult> GetUserRoleByName([FromQuery(Name = "Name")]string name)
        {
            UserRoleDto? userRole = await _userRoleService.GetUserRoleByName(name);

            if (userRole == null)
            {
                return NotFound();
            }

            return Ok(userRole);
        }

        [HttpPost]
        public async Task<IActionResult> AddUserRole([FromBody]AddUserRoleDto userRoleDto)
        {
            UserRoleDto? createdUserRole = await _userRoleService.AddUserRole(userRoleDto);

            if (createdUserRole == null)
            {
                return Conflict(new ErrorDto()
                {
                    ErrorTypeName = Enum.GetName(ErrorTypeEnum.CREATE),
                    Message = "Error during creation of new user role. Given user role name is currently in use."
                });
            }

            return CreatedAtRoute(UserRoleControllerConsts.GET_USER_ROLE_BY_ID_NAME, new { id = createdUserRole.Id }, createdUserRole);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUserRole([FromRoute]Guid id, [FromBody]UpdateUserRoleDto userRoleDto)
        {
            UserRoleDto? updatedUserRoleDto = await _userRoleService.UpdateUserRole(id, userRoleDto);

            if (updatedUserRoleDto == null)
            {
                return NotFound();
            }

            return Ok(updatedUserRoleDto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserRoleById([FromRoute]Guid id)
        {
            bool isRemovedSuccessfully = await _userRoleService.DeleteUserRoleById(id);

            if (!isRemovedSuccessfully)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteUserRoleByName([FromQuery(Name = "Name")]string name)
        {
            bool isRemovedSuccessfully = await _userRoleService.DeleteUserRoleByName(name);
            
            if (!isRemovedSuccessfully)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
