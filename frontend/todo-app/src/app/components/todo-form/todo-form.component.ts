import { Component, EventEmitter, Output, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { TodoService } from '../../services/todo.service';

@Component({
  imports: [
    ReactiveFormsModule
  ],
  selector: 'app-todo-form',
  templateUrl: './todo-form.component.html',
  styleUrls: ['./todo-form.component.css']
})
export class TodoFormComponent {
  @Output() todoCreated = new EventEmitter<void>();
  
  private readonly fb = inject(FormBuilder);
  private readonly todoService = inject(TodoService);
  
  todoForm: FormGroup;
  submitting = signal<boolean>(false);

  constructor() {
    this.todoForm = this.fb.group({
      title: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(200)]],
      description: ['', Validators.maxLength(1000)]
    });
  }

  resetForm(): void {
    this.todoForm.reset();
  }

  onSubmit(): void {
    if (this.todoForm.invalid) {
      Object.keys(this.todoForm.controls).forEach(key => {
        const control = this.todoForm.get(key);
        control?.markAsTouched();
      });
      return;
    }

    this.submitting.set(true);

    const formValue = this.todoForm.value;
    const todoData = {
      title: formValue.title,
      description: formValue.description
    };

    this.todoService.createTodo(todoData).subscribe({
      next: () => {
        this.submitting.set(false);
        this.todoCreated.emit();
      },
      error: (err) => {
        console.error('Error creating todo:', err);
        this.submitting.set(false);
      }
    });

    this.resetForm();
  }

  get titleControl() { return this.todoForm.get('title'); }
}