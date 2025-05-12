using BusinessLayer.Interfaces;
using EntityLayer.DTOs;
using EntityLayer.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _service;

        public UserController(IUserService service)
        {
            _service = service;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto login)
        {
            var result = await _service.GetUserAsync(login.Username, login.Password);
            if (result != null)
            {
                return Ok(result);
            }
            return Unauthorized();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            var result = await _service.AddUserAsync(user);
            if (!result)
            {
                return BadRequest("Kayıt işlemi başarısız.");
            }

            return Ok("Yeni kullanıcı başarılı bir şekilde oluşturuldu.");
        }

        [HttpGet("id")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _service.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound("Böyle bir kullanıcı bulunamadı!");
            }

            return Ok("Kullanıcı eklendi.");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserUpdateDto userup)
        {
            if (id != userup.Id)
            {
                return BadRequest("Kullanıcı Id'si uyuşmuyor.");
            }

            var user = await _service.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound("Böyle bir kullanıcı bulunamadı!");
            }

            user.Name = userup.Name;
            user.Surname = userup.Surname;
            user.UserName = userup.UserName;
            user.Password = userup.Password;
            user.RoleId = userup.RoleId;

            bool result = await _service.UpdateUserAsync(user);

            if (!result)
                return StatusCode(StatusCodes.Status500InternalServerError, "Kullanıcı güncellenemedi.");

            return Ok("Kullanıcı güncellendi.");
        }
    }
}
