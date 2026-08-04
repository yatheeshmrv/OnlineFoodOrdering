import { Routes } from '@angular/router';

import { adminGuard } from './Core/guards/admin.guard';
import { authGuard } from './Core/guards/auth.guard';
import { AdminDashboard } from './features/admin/admin-dashboard/admin-dashboard.component';
import { AdminFoodCategories } from './features/admin/admin-food-categories/admin-food-categories.component';
import { AdminFoodItems } from './features/admin/admin-food-items/admin-food-items.component';
import { AdminOrders } from './features/admin/admin-orders/admin-orders.component';
import { Login } from './features/auth/login/login.component';
import { Register } from './features/auth/register/register.component';
import { Cart } from './features/cart/cart.component';
import { Home } from './features/home/home.component';
import { Menu } from './features/menu/menu.component';
import { MyOrders } from './features/my-orders/my-orders.component';
import { OrderConfirmation } from './features/order-confirmation/order-confirmation.component';
import { OrderDetails } from './features/order-details/order-details.component';

export const routes: Routes = [
  {
    path: '',
    component: Home,
    title: 'Home | Food Ordering'
  },
  {
    path: 'menu',
    component: Menu,
    title: 'Menu | Food Ordering'
  },
  {
    path: 'login',
    component: Login,
    title: 'Login | Food Ordering'
  },
  {
    path: 'register',
    component: Register,
    title: 'Register | Food Ordering'
  },
  {
    path: 'cart',
    component: Cart,
    canActivate: [authGuard],
    title: 'Cart | Food Ordering'
  },
  {
    path: 'order-confirmation/:orderId',
    component: OrderConfirmation,
    canActivate: [authGuard],
    title: 'Order Confirmation | Food Ordering'
  },
  {
    path: 'my-orders/:orderId',
    component: OrderDetails,
    // Protects individual customer orders.
    // The backend must also verify order ownership.
    canActivate: [authGuard],
    title: 'Order Details | Food Ordering'
  },
  {
    path: 'my-orders',
    component: MyOrders,
    canActivate: [authGuard],
    title: 'My Orders | Food Ordering'
  },
  // Specific Admin routes must appear before /admin.
  {
    path: 'admin/food-items',
    component: AdminFoodItems,
    canActivate: [adminGuard],
    title: 'Manage Food Items | Food Ordering'
  },
  {
    path: 'admin/food-categories',
    component: AdminFoodCategories,
    canActivate: [adminGuard],
    title: 'Manage Food Categories | Food Ordering'
  },
  {
    path: 'admin/orders',
    component: AdminOrders,
    canActivate: [adminGuard],
    title: 'Manage Orders | Food Ordering'
  },
  {
    path: 'admin',
    component: AdminDashboard,
    canActivate: [adminGuard],
    title: 'Admin Dashboard | Food Ordering'
  },
  // Redirects unknown URLs to the Home page.
  // Wildcard routes must always remain last.
  {
    path: '**',
    redirectTo: ''
  }
];
