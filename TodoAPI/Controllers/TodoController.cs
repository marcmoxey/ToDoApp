using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using TodoLibrary.Contracts;
using TodoLibrary.Services;

namespace TodoAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TodoController : ControllerBase
    {
        private readonly ITodoService _service;
        private readonly ILogger<TodoController> _logger;

        public TodoController(ITodoService service, ILogger<TodoController> logger)
        {
            _service = service;
            _logger = logger;
        }

        private Guid GetUserId()
        {
            var userId = User.Claims
                .FirstOrDefault(x => x.Type == "sub")?.Value;

            if (userId == null)
                throw new UnauthorizedAccessException("User not found");

            return Guid.Parse(userId);
        }



        // POST api/Todos
        [HttpPost(Name = "CreateTodo")]
        public async Task<IActionResult> Create(CreateTodoRequest request)
        {
            _logger.LogInformation("POST: api/Todos (Task: {Task})", request.Task);
            try
            {
                var userId = GetUserId();

                var todo = await _service.CreateTodo(userId, request);

                return Ok(todo);

            } catch (Exception ex)
            {
                _logger.LogError(ex, "The POST call to api/Todos failed");
                return BadRequest();
            }
          
        }


        // GET: api/Todos
        [HttpGet(Name = "GetAllTodos")]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("GET: api/Todos");
            try
            {
                var userId = GetUserId();

                var todos = await _service.GetAllAssigned(userId);

                return Ok(todos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "The GET call to api/Todos failed");
                return BadRequest();
            }

        }


        // GET api/Todos/5
        [HttpGet("{todoId}", Name = "GetOneTodo")]
        public async Task<IActionResult> Get(int todoId)
        {
            _logger.LogInformation("GET: api/Todos/{TodoId}", todoId);

            try
            {
                var userId = GetUserId();

                var todo = await _service.GetOneAssigned(todoId, userId);

                return Ok(todo);

            } catch (Exception ex)
            {
                _logger.LogError(ex, "The GET call to {ApiPath} failed. The Id was {TodoId}", $"api/Todos/Id", todoId);
                return BadRequest();
            }
         
        }


        // PUT api/Todos/5
        [HttpPut("{todoId}", Name = "UpdateTodoTask")]
        public async Task<IActionResult> Update(int todoId, UpdateTodoRequest request)
        {
            _logger.LogInformation("PUT: api/Todos/{TodoId} (Task: Task})", todoId, request.Task);

            try
            {
                var userId = GetUserId();

                var todo = await _service.UpdateTodo(todoId, userId, request);

                return Ok(todo);
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "The PUT call to api/Todos/{TodoId} failed. Task value was {Task}", todoId, request.Task);
                return BadRequest();
            }

        }


        // PUT api/Todos/5/Complete
        [HttpPut("{todoId}/Complete", Name = "CompeleteTodo")]
        public async Task<IActionResult> Complete(int todoId)
        {
            _logger.LogInformation("PUT: api/Todos/{TodoId}/Complete", todoId);
            try
            {
                var userId = GetUserId();

                var success = await _service.CompleteTodo(todoId, userId);

                return Ok();
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "The PUT call to api/Todos/{TodoId}/Complete failed.", todoId);
                return BadRequest();
            }
         
        }


        // DELETE api/Todos/5
        [HttpDelete("{todoId}", Name = "DeleteTodo")]
        public async Task<IActionResult> Delete(int todoId)
        {
            try
            {
                var userId = GetUserId();

                var success = await _service.DeleteTodo(todoId, userId);

                return Ok();    
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "The DELETE call to api/Todos/{TodoId} failed.", todoId);
                return BadRequest();
            }


       
        }
    }
}
