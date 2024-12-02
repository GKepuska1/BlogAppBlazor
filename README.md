  BlogApp

A full-stack blog application built with Blazor WebAssembly (.NET 8) for the frontend, ASP.NET Core API (.NET 8) for the backend, and SQL Server as the database. The app provides CRUD functionality for blog posts and allows users to leave comments, all styled with the MudBlazor UI library.
Features

    Blog Post CRUD: Users can create, edit, view, and delete blog posts.
    User-Specific Editing: Only the creator of a blog post can edit it.
    Commenting System: Users can open a blog post and leave comments, similar to social media.
    News Feed: Blog posts are displayed as a list on the homepage.
    Search Functionality: Search blog posts by title.
    Modern UI: Built using the MudBlazor library for a responsive and sleek interface.

Blog Post Details

Each blog post contains:

    Title
    Content
    User (Creator)
    Date Created

Getting Started
Prerequisites

    Visual Studio 2022 or later
    SQL Server installed and configured
    .NET 8 SDK installed

Setup Instructions

    Clone the Repository:

git clone https://github.com/your-repo-name/BlogApp.git
cd BlogApp

Configure the Database:

    Create an empty database in SQL Server named BlogApp using Windows Authentication.
    Example query to create the database:

    CREATE DATABASE BlogApp;

Run EF Core Migrations:

    Open the solution file BlogApp.sln in Visual Studio.
    Build the solution to restore dependencies.
    Navigate to the BlogApp.Api project folder in the terminal and run:

        dotnet ef database update

    Run the Application:
        In Visual Studio, set multiple startup projects:
            BlogApp (Frontend)
            BlogApp.Api (Backend)
        Start the solution.

    Access the Application:
        Open a browser and navigate to the frontend URL (e.g., https://localhost:5001 or http://localhost:5000).

Technologies Used

    Frontend: Blazor WebAssembly with MudBlazor
    Backend: ASP.NET Core API
    Database: SQL Server
    ORM: Entity Framework Core

Development Notes

    Ensure you have proper SQL Server permissions for EF Core migrations.
    Update the connection string in appsettings.json if you are not using Windows Authentication or if your database is hosted on another server.
