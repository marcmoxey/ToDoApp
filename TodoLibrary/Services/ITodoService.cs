using TodoLibrary.Contracts;

namespace TodoLibrary.Services
{
    public interface ITodoService
    {
        Task<bool> CompleteTodo(int todoId, Guid userId);
        Task<TodoResponse> CreateTodo(Guid userId, CreateTodoRequest request);
        Task<bool> DeleteTodo(int todoId, Guid userId);
        Task<List<TodoResponse>> GetAllAssigned(Guid userId);
        Task<TodoResponse> GetOneAssigned(int todoId, Guid userId);
        Task<TodoResponse> UpdateTodo(int todoId, Guid userId, UpdateTodoRequest request);
    }
}