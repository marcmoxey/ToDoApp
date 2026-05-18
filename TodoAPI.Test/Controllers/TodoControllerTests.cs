using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using TodoAPI.Controllers;
using TodoLibrary.Contracts;
using TodoLibrary.Services;

namespace TodoAPI.Tests.Controllers
{
    // This class tests TodoController in isolation — no database, no real service.
    // We use Moq to create a fake ITodoService we fully control, so we can test
    // the controller's HTTP behavior (status codes, response bodies, error handling)
    // without any real data access.
     
    public class TodoControllerTests
    {
        private readonly Mock<ITodoService> _mockService;
        private readonly TodoController _controller;

        // A fixed user ID representing the logged-in user throughout these tests
        private readonly Guid _userId = Guid.NewGuid();

        // Runs before every test. Sets up the mock service, builds the controller,
        // and injects a fake HttpContext with a "sub" claim to simulate a valid JWT —
        // otherwise GetUserId() would throw and every test would fail.
        public TodoControllerTests()
        {
            _mockService = new Mock<ITodoService>();

            // Mock.Of<T> creates a simple no-op logger — we don't need real log output in tests
            var logger = Mock.Of<ILogger<TodoController>>();

            // Pass the mock's .Object (the actual fake instance) to the controller
            _controller = new TodoController(_mockService.Object, logger);

            // Manually build the ClaimsPrincipal that normally comes from JWT middleware.
            // The controller's GetUserId() reads the "sub" claim — this satisfies that.
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim("sub", _userId.ToString())
                    }))
                }
            };
        }

        // ======================
        // POST - Create
        // ======================

        [Fact]
        public async Task Create_ShouldReturnOk_WhenTodoIsCreated()
        {
            // Arrange: tell the mock what to return when CreateTodo is called
            var request = new CreateTodoRequest { Task = "New task" };
            var response = new TodoResponse { Id = 1, Task = "New task", IsComplete = false };

            _mockService
                .Setup(s => s.CreateTodo(_userId, request))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.Create(request);

            // Assert: successful creation returns 200 OK
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Create_ShouldReturnCreatedTodo()
        {
            // Arrange
            var request = new CreateTodoRequest { Task = "New task" };
            var response = new TodoResponse { Id = 1, Task = "New task", IsComplete = false };

            _mockService
                .Setup(s => s.CreateTodo(_userId, request))
                .ReturnsAsync(response);

            // Act: cast to OkObjectResult so we can inspect the response body
            var result = await _controller.Create(request) as OkObjectResult;

            // Assert: the body should be the todo the service returned
            Assert.Equal(response, result.Value);
        }

        [Fact]
        public async Task Create_ShouldCallServiceWithCorrectUserId()
        {
            // Arrange
            var request = new CreateTodoRequest { Task = "New task" };

            _mockService
                .Setup(s => s.CreateTodo(_userId, request))
                .ReturnsAsync(new TodoResponse());

            // Act
            await _controller.Create(request);

            // Assert: Verify() confirms the mock was called with the right arguments
            // exactly once — proving the controller extracted the user ID from the
            // JWT claim and passed it through correctly
            _mockService.Verify(s => s.CreateTodo(_userId, request), Times.Once);
        }

        [Fact]
        public async Task Create_ShouldReturnBadRequest_WhenServiceThrows()
        {
            // Arrange: make the service throw an unexpected exception
            var request = new CreateTodoRequest { Task = "New task" };

            _mockService
                .Setup(s => s.CreateTodo(_userId, request))
                .ThrowsAsync(new Exception("Something went wrong"));

            // Act
            var result = await _controller.Create(request);

            // Assert: the controller's catch block should return 400 Bad Request
            // instead of letting the exception bubble up to the caller
            Assert.IsType<BadRequestResult>(result);
        }

        // ======================
        // GET - GetAll
        // ======================

        [Fact]
        public async Task GetAll_ShouldReturnOk()
        {
            // Arrange
            _mockService
                .Setup(s => s.GetAllAssigned(_userId))
                .ReturnsAsync(new List<TodoResponse>());

            // Act
            var result = await _controller.GetAll();

            // Assert: even an empty list should be 200, not 404
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetAll_ShouldReturnListOfTodos()
        {
            // Arrange: service returns two todos
            var todos = new List<TodoResponse>
            {
                new() { Id = 1, Task = "Task 1", IsComplete = false },
                new() { Id = 2, Task = "Task 2", IsComplete = true }
            };

            _mockService
                .Setup(s => s.GetAllAssigned(_userId))
                .ReturnsAsync(todos);

            // Act
            var result = await _controller.GetAll() as OkObjectResult;

            // Assert: the exact list from the service should be in the response body
            Assert.Equal(todos, result.Value);
        }

        [Fact]
        public async Task GetAll_ShouldReturnEmptyList_WhenNoTodos()
        {
            // Arrange
            _mockService
                .Setup(s => s.GetAllAssigned(_userId))
                .ReturnsAsync(new List<TodoResponse>());

            // Act
            var result = await _controller.GetAll() as OkObjectResult;

            // Assert: empty list in the body — not null, not missing
            Assert.Empty(result.Value as List<TodoResponse>);
        }

        [Fact]
        public async Task GetAll_ShouldReturnBadRequest_WhenServiceThrows()
        {
            // Arrange
            _mockService
                .Setup(s => s.GetAllAssigned(_userId))
                .ThrowsAsync(new Exception("DB error"));

            // Act
            var result = await _controller.GetAll();

            // Assert: exception is caught and mapped to 400
            Assert.IsType<BadRequestResult>(result);
        }

        // ======================
        // GET - Get (single)
        // ======================
        // NOTE: The current controller does NOT check if the todo is null —
        // it returns Ok() regardless. There is no NotFound() path here anymore.
        // If this is unintentional, the controller should be updated to add a null check.

        [Fact]
        public async Task Get_ShouldReturnOk_WhenTodoExists()
        {
            // Arrange
            var todo = new TodoResponse { Id = 1, Task = "Task", IsComplete = false };

            _mockService
                .Setup(s => s.GetOneAssigned(1, _userId))
                .ReturnsAsync(todo);

            // Act
            var result = await _controller.Get(1);

            // Assert: todo found → 200 OK
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Get_ShouldReturnTodo_WhenTodoExists()
        {
            // Arrange
            var todo = new TodoResponse { Id = 1, Task = "Task", IsComplete = false };

            _mockService
                .Setup(s => s.GetOneAssigned(1, _userId))
                .ReturnsAsync(todo);

            // Act
            var result = await _controller.Get(1) as OkObjectResult;

            // Assert: the todo is in the response body
            Assert.Equal(todo, result.Value);
        }

        [Fact]
        public async Task Get_ShouldReturnOk_WhenTodoDoesNotExist()
        {
            // Arrange: service returns null (todo not found or wrong user)
            _mockService
                .Setup(s => s.GetOneAssigned(999, _userId))
                .ReturnsAsync((TodoResponse)null);

            // Act
            var result = await _controller.Get(999);

            // Assert: the controller does NOT check for null — it returns Ok(null).
            // This means the caller gets a 200 with an empty body when nothing is found.
            // Consider adding a null check if a 404 is the desired behavior.
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Get_ShouldReturnBadRequest_WhenServiceThrows()
        {
            // Arrange
            _mockService
                .Setup(s => s.GetOneAssigned(1, _userId))
                .ThrowsAsync(new Exception("DB error"));

            // Act
            var result = await _controller.Get(1);

            // Assert: exception caught → 400
            Assert.IsType<BadRequestResult>(result);
        }

        // ======================
        // PUT - Update
        // ======================
        // NOTE: The current controller does NOT check if the updated todo is null —
        // it returns Ok() regardless. There is no NotFound() path here anymore.

        [Fact]
        public async Task Update_ShouldReturnOk_WhenTodoIsUpdated()
        {
            // Arrange
            var request = new UpdateTodoRequest { Task = "Updated task" };
            var response = new TodoResponse { Id = 1, Task = "Updated task", IsComplete = false };

            _mockService
                .Setup(s => s.UpdateTodo(1, _userId, request))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.Update(1, request);

            // Assert: successful update → 200
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Update_ShouldReturnUpdatedTodo()
        {
            // Arrange
            var request = new UpdateTodoRequest { Task = "Updated task" };
            var response = new TodoResponse { Id = 1, Task = "Updated task", IsComplete = false };

            _mockService
                .Setup(s => s.UpdateTodo(1, _userId, request))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.Update(1, request) as OkObjectResult;

            // Assert: the updated todo should be in the response body
            Assert.Equal(response, result.Value);
        }

        [Fact]
        public async Task Update_ShouldReturnOk_WhenTodoDoesNotExist()
        {
            // Arrange: service returns null because the todo wasn't found
            // It.IsAny<UpdateTodoRequest>() matches any request object
            _mockService
                .Setup(s => s.UpdateTodo(999, _userId, It.IsAny<UpdateTodoRequest>()))
                .ReturnsAsync((TodoResponse)null);

            // Act
            var result = await _controller.Update(999, new UpdateTodoRequest { Task = "x" });

            // Assert: the controller returns Ok(null) instead of 404 —
            // there is no null check in the current implementation.
            // Consider adding one if 404 is the desired behavior here.
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Update_ShouldReturnBadRequest_WhenServiceThrows()
        {
            // Arrange
            var request = new UpdateTodoRequest { Task = "Updated task" };

            _mockService
                .Setup(s => s.UpdateTodo(1, _userId, request))
                .ThrowsAsync(new Exception("DB error"));

            // Act
            var result = await _controller.Update(1, request);

            // Assert: exception caught → 400
            Assert.IsType<BadRequestResult>(result);
        }

        // ======================
        // PUT - Complete
        // ======================
        // NOTE: The controller ignores the bool returned by CompleteTodo —
        // it always returns Ok() regardless of whether the todo was found.

        [Fact]
        public async Task Complete_ShouldReturnOk_WhenSuccessful()
        {
            // Arrange: service returns true (todo found and completed)
            _mockService
                .Setup(s => s.CompleteTodo(1, _userId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.Complete(1);

            // Assert: 200 OK — the controller uses Ok() not NoContent() here
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task Complete_ShouldReturnOk_WhenTodoDoesNotExist()
        {
            // Arrange: service returns false (todo not found or wrong user)
            _mockService
                .Setup(s => s.CompleteTodo(999, _userId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.Complete(999);

            // Assert: the controller does NOT check the bool — it returns Ok()
            // even when the service says it failed. Consider checking `success`
            // and returning NotFound() if false is the desired behavior.
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task Complete_ShouldCallServiceWithCorrectIds()
        {
            // Arrange
            _mockService
                .Setup(s => s.CompleteTodo(1, _userId))
                .ReturnsAsync(true);

            // Act
            await _controller.Complete(1);

            // Assert: confirm the controller passed both the route ID and the
            // JWT user ID to the service exactly once
            _mockService.Verify(s => s.CompleteTodo(1, _userId), Times.Once);
        }

        [Fact]
        public async Task Complete_ShouldReturnBadRequest_WhenServiceThrows()
        {
            // Arrange
            _mockService
                .Setup(s => s.CompleteTodo(1, _userId))
                .ThrowsAsync(new Exception("DB error"));

            // Act
            var result = await _controller.Complete(1);

            // Assert: exception caught → 400
            Assert.IsType<BadRequestResult>(result);
        }

        // ======================
        // DELETE - Delete
        // ======================
        // NOTE: The controller ignores the bool returned by DeleteTodo —
        // it always returns Ok() regardless of whether the todo was found.

        [Fact]
        public async Task Delete_ShouldReturnOk_WhenSuccessful()
        {
            // Arrange: service returns true (todo found and deleted)
            _mockService
                .Setup(s => s.DeleteTodo(1, _userId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.Delete(1);

            // Assert: 200 OK — the controller uses Ok() not NoContent()
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task Delete_ShouldReturnOk_WhenTodoDoesNotExist()
        {
            // Arrange: service returns false — nothing was deleted
            _mockService
                .Setup(s => s.DeleteTodo(999, _userId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.Delete(999);

            // Assert: the controller ignores the false return and still gives Ok().
            // Consider checking `success` and returning NotFound() if false.
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task Delete_ShouldCallServiceWithCorrectIds()
        {
            // Arrange
            _mockService
                .Setup(s => s.DeleteTodo(1, _userId))
                .ReturnsAsync(true);

            // Act
            await _controller.Delete(1);

            // Assert: confirm both the route ID and JWT user ID were passed correctly
            _mockService.Verify(s => s.DeleteTodo(1, _userId), Times.Once);
        }

        [Fact]
        public async Task Delete_ShouldReturnBadRequest_WhenServiceThrows()
        {
            // Arrange
            _mockService
                .Setup(s => s.DeleteTodo(1, _userId))
                .ThrowsAsync(new Exception("DB error"));

            // Act
            var result = await _controller.Delete(1);

            // Assert: exception caught → 400
            Assert.IsType<BadRequestResult>(result);
        }
    }
}