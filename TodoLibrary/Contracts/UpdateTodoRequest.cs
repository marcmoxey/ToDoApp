namespace TodoLibrary.Contracts
{
    public class UpdateTodoRequest
    {
        public string Task { get; set; }
        public bool IsComplete { get; set; }
    }
}
