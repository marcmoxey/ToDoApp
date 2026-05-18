namespace TodoLibrary.Contracts
{
    public class TodoResponse
    {
        public int Id { get; set; }
        public string Task { get; set; }
        public bool IsComplete { get; set; }
    }
}
