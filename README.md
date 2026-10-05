# HelpDesk CRM

HelpDesk CRM is a full-stack Customer Relationship Management and HelpDesk application built using ASP.NET Core, C#, Entity Framework Core, MySQL, REST APIs, Razor Views, HTML, CSS, and JavaScript.

The system provides a centralized platform for managing customers, employees, support tickets, customer accounts, administrative operations, and employee tasks.

The project consists of separate MVC and Web API applications, enabling structured communication between the frontend, backend services, and database.

## Key Features

- Customer Management
- Employee Management
- Ticket Management
- Customer Account Management
- Authentication and Authorization
- Role-Based Access
- Password Hashing
- RESTful API Endpoints
- API Logging Middleware
- CRUD Operations
- Entity Framework Core
- MySQL Database Integration
- Database Migrations
- LINQ
- Frontend–Backend Integration

## Architecture

The project is organized into two major applications:

### HelpDeskCRM

The ASP.NET Core MVC application responsible for the user interface, controllers, views, business workflows, and interaction with backend services.

### HelpDeskCRM.API

A dedicated ASP.NET Core Web API application providing RESTful endpoints and backend functionality for database-driven operations.

## Tech Stack

- C#
- ASP.NET Core
- ASP.NET Core MVC
- ASP.NET Core Web API
- Entity Framework Core
- MySQL
- LINQ
- REST APIs
- Razor
- HTML
- CSS
- JavaScript

## Project Structure

```text
HelpDeskCRM-GitHub/
│
├── HelpDeskCRM/
│   └── ASP.NET Core MVC Application
│
├── HelpDeskCRM.API/
│   └── ASP.NET Core Web API
│
├── .gitignore
└── README.md