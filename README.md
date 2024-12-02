
# BlogApp

A full-stack blog application built with **Blazor WebAssembly (.NET 8)** for the frontend, **ASP.NET Core API (.NET 8)** for the backend, and **SQL Server** as the database. The app provides CRUD functionality for blog posts and allows users to leave comments, all styled with the **MudBlazor** UI library.

## Features

- **Blog Post CRUD**: Users can create, edit, view, and delete blog posts.
- **User-Specific Editing**: Only the creator of a blog post can edit it.
- **Commenting System**: Users can open a blog post and leave comments, similar to social media.
- **News Feed**: Blog posts are displayed as a list on the homepage.
- **Search Functionality**: Search blog posts by title.
- **Modern UI**: Built using the partily MudBlazor library.

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
   git clone https://github.com/your-repo-name/BlogApp.git
   cd BlogApp
   ```

2. **Configure the Database**:
   - Create an empty database in SQL Server named `BlogApp` using **Windows Authentication**.
   - Example query to create the database:
     ```sql
     CREATE DATABASE BlogApp;
     ```
3. **Run the Application**:
   - In Visual Studio, set **multiple startup projects**:
     - `BlogApp` (Frontend)
     - `BlogApp.Api` (Backend)

   - The migration will be applied when the app is running.
   - Start the solution.

### Technologies Used

- **Frontend**: Blazor WebAssembly with MudBlazor
- **Backend**: ASP.NET Core API
- **Database**: SQL Server
- **ORM**: Entity Framework Core
