# WebShop

WebShop is an e-commerce application I developed using C# and ASP.NET Core as part of my .NET development studies.

The project focuses on building a functional online store with user authentication, product management, shopping cart functionality and order handling.

It gave me practical experience in backend development, REST APIs, databases and role-based authorization.

## Technologies

- C#
- ASP.NET Core
- Razor Pages
- Entity Framework Core
- SQL Server
- ASP.NET Core Identity
- REST APIs

## Features

### Product Management
- Browse available products
- View individual product details
- Create, update and delete products as an administrator
- Store product information using SQL Server

### Authentication and Authorization
- User authentication with ASP.NET Core Identity
- Three access levels: Guest, User and Admin
- Role-based access to protected functionality
- Automatic role creation during application startup

### Shopping Cart
- Add products to the cart
- Increase or decrease item quantities
- Remove products from the cart

### Order Management
- Create and view orders as a user
- View all orders as an administrator
- Update order statuses

## API Endpoints

### Public
- GET /api/me
- GET /api/public/products
- GET /api/public/products/{id}

### User
- GET /api/user/cart
- POST /api/user/cart/{productId}
- POST /api/user/cart/items/{cartItemId}/increase
- POST /api/user/cart/items/{cartItemId}/decrease
- DELETE /api/user/cart/items/{cartItemId}
- POST /api/user/orders
- GET /api/user/orders

### Admin
- POST /api/admin/products
- PUT /api/admin/products/{id}
- DELETE /api/admin/products/{id}
- GET /api/admin/orders
- PUT /api/admin/orders/{id}/status

## Database

The application uses SQL Server with Entity Framework Core to manage product data.

Initial product data is automatically generated when the database is empty.

## Future Improvements

- Assign the User role during registration
- Expand database persistence for shopping cart data
- Implement policy-based authorization
- Improve the user interface and overall user experience

## About the Project

I built WebShop to strengthen my knowledge of C# and .NET while gaining hands-on experience developing an application with real-world functionality.

The project helped me better understand how authentication, databases, APIs and frontend components work together.

## Developer

Venujan Nagendirakumar

Full Stack Developer
