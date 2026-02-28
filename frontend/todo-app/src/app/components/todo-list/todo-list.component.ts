import { Component, inject, signal, computed, viewChild } from '@angular/core';
import { TodoService } from '../../services/todo.service';
import { TodoFormComponent } from '../todo-form/todo-form.component'
import { CommonModule, DatePipe } from '@angular/common';

@Component({
  imports: [
    TodoFormComponent,
    DatePipe,
    CommonModule
  ],
  selector: 'app-todo-list',
  templateUrl: './todo-list.component.html',
  styleUrls: ['./todo-list.component.css']
})
export class TodoListComponent {
  readonly todoService = inject(TodoService);
  
  // View children
  readonly searchInput = viewChild<HTMLInputElement>('searchInput');
  
  // Signals from service
  readonly todos = this.todoService.todos;
  readonly loading = this.todoService.loading;
  readonly error = this.todoService.error;
  
  // Local signals
  readonly selectedTodoId = signal<number | null>(null);

  // Computed sorted todos
  readonly displayedTodos = computed(() => {
    return this.todos().sort(a=>a.createdAt.getTime())
  });

  deleteTodo(id: number): void {
    if (confirm('Are you sure you want to delete this todo?')) {
      this.selectedTodoId.set(id);
      this.todoService.deleteTodo(id).subscribe({
        next: () => {
          this.selectedTodoId.set(null);
        },
        error: (err) => {
          console.error('Error deleting todo:', err);
          this.selectedTodoId.set(null);
        }
      });
    }
  }

  refresh(): void {
    this.todoService.refresh();
  }

  clearError(): void {
    this.todoService.clearError();
  }

  isSelected(id: number): boolean {
    return this.selectedTodoId() === id;
  }
}