import { Injectable, inject, signal, computed, DestroyRef } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, catchError, map, of, tap, switchMap } from 'rxjs';
import { Todo, CreateTodo } from '../models/todo.model';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { toSignal, toObservable } from '@angular/core/rxjs-interop';
import { environment } from '../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class TodoService {
  private readonly http = inject(HttpClient);
  private readonly destroyRef = inject(DestroyRef);
  private readonly apiUrl = environment.apiUrl;
  
  // Signal-based state management
  private readonly todosSignal = signal<Todo[]>([]);
  private readonly loadingSignal = signal<boolean>(false);
  private readonly errorSignal = signal<string | null>(null);
  private readonly searchTermSignal = signal<string>('');
  
  // Public readonly signals
  public readonly todos = this.todosSignal.asReadonly();
  public readonly loading = this.loadingSignal.asReadonly();
  public readonly error = this.errorSignal.asReadonly();
  public readonly searchTerm = this.searchTermSignal.asReadonly();
  
  constructor() {
    this.loadTodos();
  }

  private loadTodos(): void {
    this.loadingSignal.set(true);
    this.errorSignal.set(null);
    
    this.http.get<Todo[]>(this.apiUrl).pipe(
      tap(todos => {
        // Convert date strings to Date objects
        const processedTodos = todos.map(t => ({
          ...t,
          createdAt: new Date(t.createdAt)
        }));
        this.todosSignal.set(processedTodos);
        this.loadingSignal.set(false);
      }),
      catchError(error => {
        this.errorSignal.set('Failed to load todos');
        this.loadingSignal.set(false);
        console.error('Error loading todos:', error);
        return of([]);
      }),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe();
  }

  public readonly filteredTodos = computed(() => {
    const term = this.searchTerm().toLowerCase();
    if (!term) return this.todos();
    
    return this.todos().filter(t => 
      t.title.toLowerCase().includes(term) ||
      t.description.toLowerCase().includes(term)
    );
  });

  getTodo(id: number): Observable<Todo> {
    return this.http.get<Todo>(`${this.apiUrl}/${id}`).pipe(
      map(t => ({
        ...t,
        createdAt: new Date(t.createdAt)
      }))
    );
  }


  createTodo(todo: CreateTodo): Observable<Todo> {
    this.loadingSignal.set(true);
    
    return this.http.post<Todo>(this.apiUrl, todo).pipe(
      map(newTodo => ({
        ...newTodo,
        createdAt: new Date(newTodo.createdAt)
      })),
      tap(newTodo => {
        this.todosSignal.update(todos => [...todos, newTodo]);
        this.loadingSignal.set(false);
      }),
      catchError(error => {
        this.errorSignal.set(error.error?.message || 'Failed to create todo');
        this.loadingSignal.set(false);
        throw error;
      })
    );
  }

  deleteTodo(id: number): Observable<void> {
    this.loadingSignal.set(true);
    
    return this.http.delete<void>(`${this.apiUrl}/${id}`).pipe(
      tap(() => {
        this.todosSignal.update(todos => todos.filter(t => t.id !== id));
        this.loadingSignal.set(false);
      }),
      catchError(error => {
        this.errorSignal.set('Failed to delete todo');
        this.loadingSignal.set(false);
        throw error;
      })
    );
  }

  setSearchTerm(term: string): void {
    this.searchTermSignal.set(term);
  }

  clearError(): void {
    this.errorSignal.set(null);
  }

  refresh(): void {
    this.loadTodos();
  }
}