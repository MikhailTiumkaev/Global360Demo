export interface Todo {
  id: number;
  title: string;
  description: string;
  createdAt: Date;
}

export interface CreateTodo {
  title: string;
  description?: string;
}