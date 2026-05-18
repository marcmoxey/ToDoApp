import TodoItem from "./TodoItem";
import type { Todo } from "../types/todo";

interface Props {
  todos: Todo[];
  onComplete: (id: number) => void;
  onEdit: (todo: Todo) => void;
  onDelete: (id: number) => void;
}

function TodoList({ todos, onComplete, onEdit, onDelete }: Props) {
  if (todos.length === 0) {
    return <p className="empty">no todos yet — add one above!</p>;
  }

  return (
    <ul className="todo-list">
      {todos.map((todo) => (
        <TodoItem
          key={todo.id}
          todo={todo}
          onComplete={onComplete}
          onEdit={onEdit}
          onDelete={onDelete}
        />
      ))}
    </ul>
  );
}

export default TodoList;
