using Microsoft.EntityFrameworkCore;
using TodoLibrary.Contracts;
using TodoLibrary.DataAccess;
using TodoLibrary.Models;

namespace TodoLibrary.Services
{
    public class TodoService : ITodoService
    {
        private readonly TodoContext _context;

        public TodoService(TodoContext context)
        {
            _context = context;
        }


        public async Task<TodoResponse> CreateTodo(Guid userId, CreateTodoRequest request)
        {
            var todo = new TodoModel
            {
                Task = request.Task,
                AssignedTo = userId,
                IsComplete = false
            };

            await _context.Todos.AddAsync(todo);
            await _context.SaveChangesAsync();

            return new TodoResponse
            {
                Id = todo.Id,
                Task = todo.Task,
                IsComplete = todo.IsComplete
            };
        }


        public async Task<List<TodoResponse>> GetAllAssigned(Guid userId)
        {
            return await _context.Todos
                .Where(x => x.AssignedTo == userId)
                .Select(x => new TodoResponse
                {
                    Id = x.Id,
                    Task = x.Task,
                    IsComplete = x.IsComplete
                })
                .ToListAsync();
        }


        public async Task<TodoResponse> GetOneAssigned(int todoId, Guid userId)
        {
#pragma warning disable CS8603 // Possible null reference return.
            return await _context.Todos
                .Where(x => x.Id == todoId && x.AssignedTo == userId)
                .Select(x => new TodoResponse
                {
                    Id = x.Id,
                    Task = x.Task,
                    IsComplete = x.IsComplete
                })
                .FirstOrDefaultAsync();
#pragma warning restore CS8603 // Possible null reference return.
        }


        public async Task<TodoResponse> UpdateTodo(int todoId, Guid userId, UpdateTodoRequest request)
        {
            var todo = await _context.Todos
                .FirstOrDefaultAsync(x => x.Id == todoId && x.AssignedTo == userId);

            if (todo == null)
                return null;

            todo.Task = request.Task;


            await _context.SaveChangesAsync();

            return new TodoResponse
            {
                Id = todo.Id,
                Task = todo.Task,
                IsComplete = todo.IsComplete
            };
        }


        public async Task<bool> CompleteTodo(int todoId, Guid userId)
        {
            var todo = await _context.Todos
                .FirstOrDefaultAsync(x => x.Id == todoId && x.AssignedTo == userId);

            if (todo == null)
                return false;

            todo.IsComplete = true;

            await _context.SaveChangesAsync();

            return true;
        }


        public async Task<bool> DeleteTodo(int todoId, Guid userId)
        {
            var todo = await _context.Todos
                .FirstOrDefaultAsync(x => x.Id == todoId && x.AssignedTo == userId);

            if (todo == null)
                return false;

            _context.Todos.Remove(todo);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}