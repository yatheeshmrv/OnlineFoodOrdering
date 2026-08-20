# Online Food Ordering & Order Management Platform

A full-stack web application for online food ordering and order management, built using **Angular, ASP.NET Core Web API, C#, Entity Framework Core, and SQL Server**.

The application provides separate functionality for **customers and administrators**, including authentication, food management, shopping cart operations, checkout, delivery addresses, order tracking, and administrative order management.

---

## Features

### Customer Features

* User registration and login
* JWT-based authentication
* Browse available food items
* Browse food items by category
* Add items to shopping cart
* Update item quantities
* Remove items from cart
* Clear shopping cart
* Save and manage delivery addresses
* Select delivery address during checkout
* Place food orders
* View order confirmation
* View previous orders
* View individual order details

### Admin Features

* Role-based Admin access
* Add food items
* Update food items
* Delete food items
* Manage food categories
* View customer orders
* View order details
* Update order status

---

## Technology Stack

### Frontend

* Angular
* TypeScript
* HTML
* CSS
* Angular Routing
* HTTP Client
* Route Guards
* HTTP Interceptors

### Backend

* C#
* ASP.NET Core Web API
* REST APIs
* Entity Framework Core
* ASP.NET Core Identity
* JWT Authentication
* Role-Based Authorization
* Dependency Injection
* FluentValidation
* Global Exception Handling

### Database

* SQL Server

### Testing

* Unit Testing
* Integration Testing

---

## Application Architecture

The backend follows a layered architecture:

```text
Angular Frontend
       |
       v
ASP.NET Core Web API
       |
       v
Controllers
       |
       v
Services
       |
       v
Repositories
       |
       v
Entity Framework Core
       |
       v
SQL Server
```

### Controller Layer

Handles HTTP requests and API responses.

### Service Layer

Contains application and business logic.

### Repository Layer

Handles communication with the database.

### Entity Framework Core

Provides object-relational mapping and database operations.

This structure keeps responsibilities separated and makes the application easier to maintain, test, and extend.

---

## Authentication & Authorization

The application uses **JWT authentication**.

After a successful login, the backend generates an access token that is used by the Angular application when calling protected API endpoints.

Role-based authorization is used to separate:

* Customer functionality
* Administrator functionality

Protected Angular routes are secured using route guards, while outgoing authenticated API requests are handled through an HTTP interceptor.

---

## Main Application Flow

```text
Register / Login
       |
       v
Browse Menu
       |
       v
Add Items to Cart
       |
       v
Manage Cart
       |
       v
Select Delivery Address
       |
       v
Checkout
       |
       v
Order Created
       |
       v
Order Confirmation
       |
       v
My Orders
```

---

## Backend Structure

```text
Controllers/
DTOs/
Models/
Repositories/
Services/
Validators/
Data/
Migrations/
Tests/
Program.cs
```

The project separates API endpoints, business logic, database operations, validation, and data models into dedicated components.

---

## Frontend Structure

The Angular frontend includes pages and functionality for:

```text
Home
Menu
Login
Register
Cart
Checkout
Order Confirmation
My Orders
Order Details

Admin Dashboard
Food Item Management
Food Category Management
Order Management
```

---

## REST API Functionality

The application provides APIs for:

### Authentication

* Register
* Login

### Food Categories

* View categories
* Create category
* Update category
* Delete category

### Food Items

* View food items
* Create food item
* Update food item
* Delete food item

### Cart

* View cart
* Add item
* Update quantity
* Remove item
* Clear cart
* Checkout

### Orders

* View customer's orders
* View individual order details
* Admin order listing
* Admin order details
* Update order status

---

## Validation & Error Handling

The backend uses **FluentValidation** to validate incoming request data.

Global exception handling is used to provide consistent error responses and prevent internal application exceptions from being directly exposed to clients.

---

## Testing

The project includes:

* Unit tests for application components and business logic
* Integration tests for API functionality
* Authentication and authorization testing
* Cart and checkout workflow testing
* Order management testing

---

## Security

The application implements:

* JWT authentication
* Role-based authorization
* Protected API endpoints
* Angular route guards
* HTTP authentication interceptor
* Server-side request validation
* Password management through ASP.NET Core Identity

Sensitive production credentials should be stored using environment variables or secure configuration and should not be committed to source control.

---

## Screenshots

Application screenshots will be added here after the portfolio version is finalized.

Planned screenshots:

1. Home Page
2. Food Menu
3. Shopping Cart
4. Checkout
5. Order Confirmation
6. My Orders
7. Admin Dashboard
8. Food Management
9. Order Management

---

## Live Demo

A publicly hosted demo version will be added after deployment.

---

## Future Improvements

Potential future enhancements include:

* Online payment gateway integration
* Email/SMS order notifications
* Restaurant search and filtering
* Order tracking
* Customer reviews and ratings
* Improved responsive design
* Cloud deployment
* Application monitoring and logging

---

## Purpose

This project was developed to demonstrate full-stack web application development using **Angular and ASP.NET Core**, including frontend development, REST API development, authentication, authorization, database integration, testing, and layered application architecture.

---

## Author

**Yatheesh M**

Full Stack .NET Developer

GitHub: https://github.com/yatheeshmrv
