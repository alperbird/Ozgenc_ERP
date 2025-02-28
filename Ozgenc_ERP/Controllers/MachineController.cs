using BusinessLayer.Services;
using EntityLayer.DTOs;
using EntityLayer.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ApiLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MachineController : ControllerBase
    {
        private readonly MachineService _service;

        public MachineController(MachineService service)
        {
            _service = service;
        }

        [HttpGet]
        [Produces("application/json")]
        public async Task<ActionResult<List<MachineDto>>> GetAllMachines()
        {
            var machines = await _service.GetAllMachineAsync();
            var machineDtos = machines.Select(m => new MachineDto
            {
                Id = m.Id,
                MakineAdi = m.MakineAdi
            }).ToList();

            return Ok(machineDtos);
        }

    }

}
