import { Routes } from '@angular/router';

export const routes: Routes = [
    { path: 'comments', loadComponent: () => import('./features/comments-list/comments-list.component').then(m => m.CommentsListComponent) },
];
