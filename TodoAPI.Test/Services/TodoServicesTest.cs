using Microsoft.EntityFrameworkCore;
using TodoLibrary.Contracts;
using TodoLibrary.DataAccess;
using TodoLibrary.Services;

namespace TodoAPI.Tests.Services
{
    // This class tests TodoService directly against a real (in-memory) database.
    // We use EF Core's InMemory provider so tests run instantly with no SQL Server
    // or Postgres required — no connection string, no server, no migrations.
    //
    // IDisposable is implemented so xUnit automatically calls Dispose() after each
    // test, which cleans up the DbContext and releases memory.
    public class TodoServiceTests : IDisposable
    {
        private readonly TodoContext _context;
        private readonly TodoService _service;

        // A fixed user ID used across tests to simulate a logged-in user.
        // Generated once per test class instance so every test in this
        // class shares the same _userId.
        private readonly Guid _userId = Guid.NewGuid();

        // The constructor runs before EVERY test method.
        // Each test gets a brand new in-memory database (Guid.NewGuid() as the name
        // guarantees a unique DB per test), so tests are completely isolated —
        // data created in one test cannot affect another.
        public TodoServiceTests()
        {
            var options = new DbContextOptionsBuilder<TodoContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // unique DB name = fresh slate per test
                .Options;

            _context = new TodoContext(options);
            _service = new TodoService(_context); // real service, real EF, fake DB
        }

        // Called automatically by xUnit after each test finishes.
        // Disposes the DbContext to free up memory properly.
        public void Dispose()
        {
            _context.Dispose();
        }

        // ======================
        // CreateTodo
        // ======================
        // These tests verify that CreateTodo saves a new todo and returns
        // the correct data back to the caller.

        [Fact]
        public async Task CreateTodo_ShouldReturnTodoResponse()
        {
            // Arrange: build a request with a task name
            var request = new CreateTodoRequest { Task = "Buy milk" };

            // Act: call the real service method
            var result = await _service.CreateTodo(_userId, request);

            // Assert: the service should return something, not null
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CreateTodo_ShouldReturnCorrectTask()
        {
            // Arrange
            var request = new CreateTodoRequest { Task = "Buy milk" };

            // Act
            var result = await _service.CreateTodo(_userId, request);

            // Assert: the task name in the response should match what we sent in
            Assert.Equal("Buy milk", result.Task);
        }

        [Fact]
        public async Task CreateTodo_ShouldDefaultToNotComplete()
        {
            // Arrange
            var request = new CreateTodoRequest { Task = "Buy milk" };

            // Act
            var result = await _service.CreateTodo(_userId, request);

            // Assert: new todos should never start as complete —
            // the service hardcodes IsComplete = false on creation
            Assert.False(result.IsComplete);
        }

        [Fact]
        public async Task CreateTodo_ShouldPersistToDatabase()
        {
            // Arrange
            var request = new CreateTodoRequest { Task = "Buy milk" };

            // Act
            var result = await _service.CreateTodo(_userId, request);

            // Assert: go directly to the DbContext (bypassing the service) and
            // confirm the record was actually written to the database.
            // This catches bugs where the service builds the response object
            // but forgets to call SaveChangesAsync().
            var saved = await _context.Todos.FindAsync(result.Id);
            Assert.NotNull(saved);
        }

        [Fact]
        public async Task CreateTodo_ShouldAssignCorrectUser()
        {
            // Arrange
            var request = new CreateTodoRequest { Task = "Buy milk" };

            // Act
            var result = await _service.CreateTodo(_userId, request);

            // Assert: fetch the raw DB record and confirm AssignedTo was set
            // to the user who made the request — not null, not another user's ID
            var saved = await _context.Todos.FindAsync(result.Id);
            Assert.Equal(_userId, saved.AssignedTo);
        }

        // ======================
        // GetAllAssigned
        // ======================
        // These tests verify that GetAllAssigned returns only the todos
        // that belong to the requesting user, never anyone else's.

        [Fact]
        public async Task GetAllAssigned_ShouldReturnOnlyCurrentUsersTodos()
        {
            // Arrange: create one todo for our user and one for a different user
            var otherUserId = Guid.NewGuid();
            await _service.CreateTodo(_userId, new CreateTodoRequest { Task = "My task" });
            await _service.CreateTodo(otherUserId, new CreateTodoRequest { Task = "Their task" });

            // Act
            var results = await _service.GetAllAssigned(_userId);

            // Assert: only 1 result should come back — the other user's todo
            // should be filtered out by the WHERE clause in the service
            Assert.Single(results);
        }

        [Fact]
        public async Task GetAllAssigned_ShouldReturnAllCurrentUsersTodos()
        {
            // Arrange: create 3 todos all belonging to the same user
            await _service.CreateTodo(_userId, new CreateTodoRequest { Task = "Task 1" });
            await _service.CreateTodo(_userId, new CreateTodoRequest { Task = "Task 2" });
            await _service.CreateTodo(_userId, new CreateTodoRequest { Task = "Task 3" });

            // Act
            var results = await _service.GetAllAssigned(_userId);

            // Assert: all 3 should be returned — nothing should be dropped
            Assert.Equal(3, results.Count);
        }

        [Fact]
        public async Task GetAllAssigned_ShouldReturnEmptyList_WhenNoTodos()
        {
            // Arrange: nothing — the database starts empty for this test

            // Act
            var results = await _service.GetAllAssigned(_userId);

            // Assert: an empty list is valid and expected, not null
            Assert.Empty(results);
        }

        [Fact]
        public async Task GetAllAssigned_ShouldNotReturnOtherUsersTodos()
        {
            // Arrange: create a todo for a completely different user
            var otherUserId = Guid.NewGuid();
            await _service.CreateTodo(otherUserId, new CreateTodoRequest { Task = "Their task" });

            // Act: fetch todos for our user
            var results = await _service.GetAllAssigned(_userId);

            // Assert: our user has no todos, so the list should be empty.
            // If the service has a bug in its WHERE clause, this would leak
            // another user's data — this test catches that.
            Assert.Empty(results);
        }

        // ======================
        // GetOneAssigned
        // ======================
        // These tests verify that GetOneAssigned returns the right todo,
        // and that it respects ownership — users can't see each other's items.

        [Fact]
        public async Task GetOneAssigned_ShouldReturnCorrectTodo()
        {
            // Arrange: create a todo and capture its generated ID
            var created = await _service.CreateTodo(_userId, new CreateTodoRequest { Task = "Find me" });

            // Act: fetch that specific todo by its ID
            var result = await _service.GetOneAssigned(created.Id, _userId);

            // Assert: the returned item should exist and match what we created
            Assert.NotNull(result);
            Assert.Equal("Find me", result.Task);
        }

        [Fact]
        public async Task GetOneAssigned_ShouldReturnNull_WhenTodoDoesNotExist()
        {
            // Arrange: nothing — 999 is an ID that doesn't exist in the empty DB

            // Act
            var result = await _service.GetOneAssigned(999, _userId);

            // Assert: null is the correct response when a todo isn't found —
            // the controller maps this to a 404
            Assert.Null(result);
        }

        [Fact]
        public async Task GetOneAssigned_ShouldReturnNull_WhenTodoBelongsToOtherUser()
        {
            // Arrange: create a todo under a different user's account
            var otherUserId = Guid.NewGuid();
            var created = await _service.CreateTodo(otherUserId, new CreateTodoRequest { Task = "Not yours" });

            // Act: try to fetch it as our user
            var result = await _service.GetOneAssigned(created.Id, _userId);

            // Assert: even though the todo exists in the DB, our user shouldn't
            // be able to see it — the service filters by both Id AND AssignedTo
            Assert.Null(result);
        }

        // ======================
        // UpdateTodo
        // ======================
        // These tests verify that UpdateTodo changes the right record,
        // saves it to the database, and blocks cross-user updates.

        [Fact]
        public async Task UpdateTodo_ShouldUpdateTask()
        {
            // Arrange: create a todo with an original task name
            var created = await _service.CreateTodo(_userId, new CreateTodoRequest { Task = "Old task" });

            // Act: update it with a new task name
            var result = await _service.UpdateTodo(created.Id, _userId, new UpdateTodoRequest { Task = "New task" });

            // Assert: the returned response should reflect the new task name
            Assert.Equal("New task", result.Task);
        }

        [Fact]
        public async Task UpdateTodo_ShouldPersistChangeToDatabase()
        {
            // Arrange
            var created = await _service.CreateTodo(_userId, new CreateTodoRequest { Task = "Old task" });

            // Act
            await _service.UpdateTodo(created.Id, _userId, new UpdateTodoRequest { Task = "New task" });

            // Assert: go directly to the DbContext to verify the change was actually
            // saved — not just returned in memory. This catches cases where the
            // service updates the object but forgets SaveChangesAsync().
            var saved = await _context.Todos.FindAsync(created.Id);
            Assert.Equal("New task", saved.Task);
        }

        [Fact]
        public async Task UpdateTodo_ShouldReturnNull_WhenTodoDoesNotExist()
        {
            // Arrange: no todo with ID 999 exists

            // Act
            var result = await _service.UpdateTodo(999, _userId, new UpdateTodoRequest { Task = "x" });

            // Assert: null signals "not found" back to the controller
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateTodo_ShouldReturnNull_WhenTodoBelongsToOtherUser()
        {
            // Arrange: create a todo under a different user
            var otherUserId = Guid.NewGuid();
            var created = await _service.CreateTodo(otherUserId, new CreateTodoRequest { Task = "Not yours" });

            // Act: try to update it as our user
            var result = await _service.UpdateTodo(created.Id, _userId, new UpdateTodoRequest { Task = "Hacked" });

            // Assert: the service should return null because the WHERE clause
            // filters by both Id AND AssignedTo — preventing cross-user modification
            Assert.Null(result);
        }

        // ======================
        // CompleteTodo
        // ======================
        // These tests verify that CompleteTodo flips IsComplete to true,
        // and that it won't complete a todo that doesn't belong to the user.

        [Fact]
        public async Task CompleteTodo_ShouldReturnTrue_WhenSuccessful()
        {
            // Arrange: create a todo to mark as complete
            var created = await _service.CreateTodo(_userId, new CreateTodoRequest { Task = "Finish me" });

            // Act
            var result = await _service.CompleteTodo(created.Id, _userId);

            // Assert: true means the operation succeeded
            Assert.True(result);
        }

        [Fact]
        public async Task CompleteTodo_ShouldMarkTodoAsComplete()
        {
            // Arrange
            var created = await _service.CreateTodo(_userId, new CreateTodoRequest { Task = "Finish me" });

            // Act
            await _service.CompleteTodo(created.Id, _userId);

            // Assert: go to the database directly and verify IsComplete was
            // actually flipped to true — not just that the method returned true
            var saved = await _context.Todos.FindAsync(created.Id);
            Assert.True(saved.IsComplete);
        }

        [Fact]
        public async Task CompleteTodo_ShouldReturnFalse_WhenTodoDoesNotExist()
        {
            // Arrange: ID 999 doesn't exist in the DB

            // Act
            var result = await _service.CompleteTodo(999, _userId);

            // Assert: false means "not found" — the controller maps this to 404
            Assert.False(result);
        }

        [Fact]
        public async Task CompleteTodo_ShouldReturnFalse_WhenTodoBelongsToOtherUser()
        {
            // Arrange: create a todo for someone else
            var otherUserId = Guid.NewGuid();
            var created = await _service.CreateTodo(otherUserId, new CreateTodoRequest { Task = "Not yours" });

            // Act: try to complete it as our user
            var result = await _service.CompleteTodo(created.Id, _userId);

            // Assert: our user can't complete another user's todo —
            // the service won't find it because AssignedTo won't match
            Assert.False(result);
        }

        // ======================
        // DeleteTodo
        // ======================
        // These tests verify that DeleteTodo removes the record from the database,
        // and that users can only delete their own todos.

        [Fact]
        public async Task DeleteTodo_ShouldReturnTrue_WhenSuccessful()
        {
            // Arrange
            var created = await _service.CreateTodo(_userId, new CreateTodoRequest { Task = "Delete me" });

            // Act
            var result = await _service.DeleteTodo(created.Id, _userId);

            // Assert: true means the delete was found and executed
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteTodo_ShouldRemoveFromDatabase()
        {
            // Arrange
            var created = await _service.CreateTodo(_userId, new CreateTodoRequest { Task = "Delete me" });

            // Act
            await _service.DeleteTodo(created.Id, _userId);

            // Assert: go directly to the DB and confirm the record is gone.
            // FindAsync returns null when no record exists for that ID.
            var saved = await _context.Todos.FindAsync(created.Id);
            Assert.Null(saved);
        }

        [Fact]
        public async Task DeleteTodo_ShouldReturnFalse_WhenTodoDoesNotExist()
        {
            // Arrange: ID 999 doesn't exist

            // Act
            var result = await _service.DeleteTodo(999, _userId);

            // Assert: false signals "not found" to the controller
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteTodo_ShouldReturnFalse_WhenTodoBelongsToOtherUser()
        {
            // Arrange: create a todo under a different user
            var otherUserId = Guid.NewGuid();
            var created = await _service.CreateTodo(otherUserId, new CreateTodoRequest { Task = "Not yours" });

            // Act: attempt to delete it as our user
            var result = await _service.DeleteTodo(created.Id, _userId);

            // Assert: the delete should be blocked because AssignedTo doesn't match
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteTodo_ShouldNotDeleteOtherUsersTodo()
        {
            // Arrange: create a todo under a different user
            var otherUserId = Guid.NewGuid();
            var created = await _service.CreateTodo(otherUserId, new CreateTodoRequest { Task = "Not yours" });

            // Act: our user tries to delete it
            await _service.DeleteTodo(created.Id, _userId);

            // Assert: the other user's todo should still exist in the database —
            // this is the flip side of the test above, confirming the record
            // wasn't accidentally deleted even though we tried
            var saved = await _context.Todos.FindAsync(created.Id);
            Assert.NotNull(saved);
        }
    }
}