import { Routes } from '@angular/router';

export const routes: Routes = [{
    path: '',
    pathMatch: 'full',
    loadComponent: () => {
        return import('./home/home.component').then (
            m => m.HomeComponent
        )
    }
},
{
    path: 'register',
    pathMatch: 'full',
    loadComponent: () => {
        return import('./register/register.component').then (
            m => m.RegisterComponent
        )
    }
},
{
    path: 'login',
    pathMatch: 'full',
    loadComponent: () => {
        return import('./login/login.component').then (
            m => m.LoginComponent
        )
    }
},
{
    path: 'start',
    pathMatch: 'full',
    loadComponent: () => {
        return import('./start/start.component').then (
            m => m.StartComponent
        )
    }
}];