
# BlogApp

A full-stack blog application built with a **React** frontend and an **ASP.NET Core API (.NET 8)** backend. The original Blazor WebAssembly UI has been replaced with React styled using **plain CSS**. The backend continues to use **SQL Server** for data storage.

## Features

- **Blog Post CRUD**: Users can create, edit, view, and delete blog posts.
- **User-Specific Editing**: Only the creator of a blog post can edit it.
- **Commenting System**: Users can open a blog post and leave comments, similar to social media.
- **News Feed**: Blog posts are displayed as a list on the homepage.
- **Search Functionality**: Search blog posts by title.
- **Modern UI**: React components styled with plain CSS.

## Blog Post Details

Each blog post contains:
- **Title**
- **Content**
- **User** (Creator)
- **Date Created**

## Getting Started

### Prerequisites

- **Visual Studio 2022** or later
- **SQL Server** installed and configured
- .NET 8 SDK installed

### Setup Instructions

1. **Clone the Repository**:
   ```bash
   git clone https://github.com/your-repo-name/BlogAppBlazor.git
   cd BlogAppBlazor
   ```

2. **Configure the Database**:
   - Create an empty database in SQL Server named `BlogApp` using **Windows Authentication**.
   - Example query to create the database:
     ```sql
     CREATE DATABASE BlogApp;
     ```
3. **Run the Application**:
   - Install frontend dependencies:
     ```bash
     cd frontend
     npm install
     ```
   - In one terminal start the API:
     ```bash
     dotnet watch --project ../BlogApp.Api
     ```
   - In another terminal start the React frontend:
     ```bash
     npm run dev
     ```

### Technologies Used

- **Frontend**: React + Vite with plain CSS
- **Backend**: ASP.NET Core API
- **Database**: SQL Server
- **ORM**: Entity Framework Core

## Running Tests

This solution contains unit tests and early integration tests. You can run them
from the repository root using the `dotnet test` command:

```bash
dotnet test
```

This runs the projects `BlogApp.UnitTests` and `BlogApp.IntegrationTests`.

- **Unit tests** require no additional setup.
- **Integration tests** need a reachable SQL Server instance. Provide a
  connection string in `BlogApp.IntegrationTests/appsettings.json` using the
  `ConnectionStrings:TestDb` key. The test project will create and later remove
  a temporary database when it runs.
