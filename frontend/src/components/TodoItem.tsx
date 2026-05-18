import type { Todo } from "../types/todo";

interface Props {
  todo: Todo;
  onComplete: (id: number) => void;
  onEdit: (todo: Todo) => void;
  onDelete: (id: number) => void;
}

function TodoItem({ todo, onComplete, onEdit, onDelete }: Props) {
  return (
    <li className={`todo-item ${todo.isComplete ? "done" : ""}`}>
      {/* Checkbox button — marks the todo as complete */}
      <button
        className={`check-btn ${todo.isComplete ? "checked" : ""}`}
        onClick={() => !todo.isComplete && onComplete(todo.id)}
        disabled={todo.isComplete}
        title="mark complete"
      >
        {todo.isComplete && <span className="check-icon">✓</span>}
      </button>

      {/* Task text — struck through when complete */}
      <span className={`task-text ${todo.isComplete ? "strikethrough" : ""}`}>
        {todo.task}
      </span>

      {/* Action buttons — Edit is hidden once a todo is complete */}
      <div className="todo-actions">
        {!todo.isComplete && (
          <button
            className="btn-ghost btn-edit"
            onClick={() => onEdit(todo)}
            title="edit todo"
          >
            edit
          </button>
        )}
        <button
          className="del-btn"
          onClick={() => onDelete(todo.id)}
          title="delete todo"
        >
          ×
        </button>
      </div>
    </li>
  );
}

export default TodoItem;
