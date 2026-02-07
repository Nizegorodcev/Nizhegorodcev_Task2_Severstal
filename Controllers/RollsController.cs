using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Nizhegorodcev_Task2_Severstal.DTOs.Requests;
using Nizhegorodcev_Task2_Severstal.DTOs.Responses;
using Nizhegorodcev_Task2_Severstal.Models;
using Nizhegorodcev_Task2_Severstal.Repositories;

namespace Nizhegorodcev_Task2_Severstal.Controllers
{
    /// <summary>
    /// Контроллер для управления рулонами металла на складе
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class RollsController : ControllerBase
    {
        private readonly IRollRepository _repository;
        private readonly ILogger<RollsController> _logger;

        public RollsController(IRollRepository repository, ILogger<RollsController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        /// <summary>
        /// Получить список всех рулонов с возможностью фильтрации
        /// </summary>
        /// <param name="filter">Параметры фильтрации (опционально)</param>
        /// <returns>Список рулонов</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<RollResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<RollResponse>>> GetRolls([FromQuery] RollFilterRequest? filter)
        {
            try
            {
                var rolls = await _repository.GetAllAsync(filter);
                var responses = rolls.Select(ToResponse);
                return Ok(responses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка рулонов");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Получить рулон по ID
        /// </summary>
        /// <param name="id">Идентификатор рулона</param>
        /// <returns>Рулон</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(RollResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RollResponse>> GetRoll(int id)
        {
            try
            {
                var roll = await _repository.GetByIdAsync(id);
                if (roll == null)
                    return NotFound($"Рулон с ID {id} не найден");

                return Ok(ToResponse(roll));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при получении рулона с ID {id}");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Добавить новый рулон на склад
        /// </summary>
        /// <param name="request">Данные нового рулона</param>
        /// <returns>Созданный рулон</returns>
        [HttpPost]
        [ProducesResponseType(typeof(RollResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RollResponse>> CreateRoll([FromBody] CreateRollRequest request)
        {
            try
            {
                // Добавляем проверку ModelState
                if (!ModelState.IsValid)
                {
                    return ValidationProblem(ModelState);
                }

                var roll = new MetalRoll(request.Length, request.Weight);
                var createdRoll = await _repository.AddAsync(roll);

                var response = ToResponse(createdRoll);
                return CreatedAtAction(nameof(GetRoll), new { id = createdRoll.Id }, response);
            }
            catch (DomainException ex)
            {
                return BadRequest(new { Error = ex.Message, Parameter = ex.ParameterName });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании рулона");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Удалить рулон по ID (мягкое удаление)
        /// </summary>
        /// <param name="id">Идентификатор рулона</param>
        /// <returns>Удаленный рулон</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(RollResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RollResponse>> DeleteRoll(int id)
        {
            try
            {
                var roll = await _repository.DeleteAsync(id);
                if (roll == null)
                    return NotFound($"Рулон с ID {id} не найден или уже удален");

                return Ok(ToResponse(roll));
            }
            catch (DomainException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при удалении рулона с ID {id}");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Обновить параметры рулона
        /// </summary>
        /// <param name="id">Идентификатор рулона</param>
        /// <param name="request">Новые параметры</param>
        /// <returns>Обновленный рулон</returns>
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(RollResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RollResponse>> UpdateRoll(int id, [FromBody] UpdateRollRequest request)
        {
            try
            {
                var roll = await _repository.GetByIdAsync(id);
                if (roll == null)
                    return NotFound($"Рулон с ID {id} не найден");

                // Используем метод Update модели
                roll.Update(
                    request.Length ?? roll.Length,
                    request.Weight ?? roll.Weight
                );

                var updatedRoll = await _repository.UpdateAsync(roll);
                return Ok(ToResponse(updatedRoll));
            }
            catch (DomainException ex)
            {
                return BadRequest(new { Error = ex.Message, Parameter = ex.ParameterName });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при обновлении рулона с ID {id}");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Получить статистику по рулонам за период
        /// </summary>
        /// <param name="request">Период (начальная и конечная дата)</param>
        /// <returns>Статистика</returns>
        [HttpGet("statistics")]
        [ProducesResponseType(typeof(StatisticsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<StatisticsResponse>> GetStatistics([FromQuery] StatisticsRequest request)
        {
            try
            {
                if (request.StartDate > request.EndDate)
                    return BadRequest("Дата начала не может быть позже даты окончания");

                var statistics = await _repository.GetStatisticsAsync(request.StartDate, request.EndDate);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении статистики");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Преобразование модели в DTO ответа
        /// </summary>
        private static RollResponse ToResponse(MetalRoll roll)
        {
            return new RollResponse
            {
                Id = roll.Id,
                Length = roll.Length,
                Weight = roll.Weight,
                AddedDate = roll.AddedDate,
                DeletedDate = roll.DeletedDate,
                IsDeleted = roll.IsDeleted
            };
        }
    }
}