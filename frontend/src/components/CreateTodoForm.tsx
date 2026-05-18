import { useState } from "react";

interface Props {
  onCreate: (task: string) => void;
}

function CreateTodoForm({ onCreate }: Props) {
  const [task, setTask] = useState("");

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!task.trim()) return; // don't submit empty tasks
    onCreate(task);
    setTask(""); // clear the input after submitting
  };

  return (
    <form onSubmit={handleSubmit}>
      <input
        type="text"
        placeholder="Add a new todo..."
        value={task}
        onChange={(e) => setTask(e.target.value)}
      />
      <button type="submit">Add</button>
    </form>
  );
}

export default CreateTodoForm;
