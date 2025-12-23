using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sahty.API.DTOs;
using Sahty.API.Repositories.Interfaces;
using Sahty.Metiers;
using Sahty.Shared.Auth;
using System.Linq;
using System.Threading.Tasks;

namespace Sahty.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MedicationsController : ControllerBase
    {
        private readonly IMedicationRepository _repository;

        public MedicationsController(IMedicationRepository repository)
        {
            _repository = repository;
        }

        [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Doctor},{AppRoles.Pharmacist}")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var medications = await _repository.GetAllAsync();
            var dtos = medications.Select(m => new MedicationDto
            {
                Id = m.Id,
                Name = m.Name,
                Stock = m.Stock,
                Description = m.Description
            });
            return Ok(dtos);
        }

        [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Doctor},{AppRoles.Pharmacist}")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var med = await _repository.GetByIdAsync(id);
            if (med == null) return NotFound();
            return Ok(new MedicationDto
            {
                Id = med.Id,
                Name = med.Name,
                Stock = med.Stock,
                Description = med.Description
            });
        }

        [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Pharmacist}")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MedicationCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var med = new Medication
            {
                Name = dto.Name,
                Stock = dto.Stock,
                Description = dto.Description
            };

            var created = await _repository.CreateAsync(med);

            var resultDto = new MedicationDto
            {
                Id = created.Id,
                Name = created.Name,
                Stock = created.Stock,
                Description = created.Description
            };

            return CreatedAtAction(nameof(GetById), new { id = resultDto.Id }, resultDto);
        }

        [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Pharmacist}")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] MedicationUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (id != dto.Id) return BadRequest();

            var med = new Medication
            {
                Id = dto.Id,
                Name = dto.Name,
                Stock = dto.Stock,
                Description = dto.Description
            };

            var updated = await _repository.UpdateAsync(med);
            if (updated == null) return NotFound();

            var resultDto = new MedicationDto
            {
                Id = med.Id,
                Name = med.Name,
                Stock = med.Stock,
                Description = med.Description
            };

            return Ok(resultDto);
        }

        [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Pharmacist}")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _repository.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
