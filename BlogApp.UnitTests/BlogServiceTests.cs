using AutoMapper;
using BlogApp.Core.Context;
using BlogApp.Core.Services;
using BlogApp.Domain.Dtos;
using BlogApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BlogApp.UnitTests
{
    public class BlogServiceTests
    {
        private readonly IMapper _mapper;

        public BlogServiceTests()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Blog, BlogDto>();
                cfg.CreateMap<BlogDtoCreate, Blog>();
            });
            _mapper = config.CreateMapper();
        }

        private IAppDbContext GetInMemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task AddAsync_ShouldCreateBlog()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var context = GetInMemoryDbContext(dbName);
            context.Users.Add(new ApplicationUser { Id = "user1", UserName = "user1" });
            await context.SaveChangesAsync();
            var service = new BlogService(context, _mapper);
            var dto = new BlogDtoCreate { Name = "Test Blog", Content = "Test Content" };

            // Act
            var result = await service.AddAsync(dto, "user1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Blog", result.Name);
            Assert.Single(context.Blogs);
        }

        [Fact]
        public async Task AddAsync_ShouldThrow_WhenDailyLimitReached()
        {
            var dbName = Guid.NewGuid().ToString();
            var context = GetInMemoryDbContext(dbName);
            context.Users.Add(new ApplicationUser { Id = "user1", UserName = "user1", LastPostDate = DateTime.UtcNow.Date, PostCount = 1 });
            await context.SaveChangesAsync();
            var service = new BlogService(context, _mapper);
            var dto = new BlogDtoCreate { Name = "Test", Content = "c" };

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await service.AddAsync(dto, "user1"));
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnBlog()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var context = GetInMemoryDbContext(dbName);
            var blog = new Blog { Name = "Existing", Content = "Content", UserId = "userX" };
            context.Blogs.Add(blog);
            await context.SaveChangesAsync();

            var service = new BlogService(context, _mapper);

            // Act
            var result = await service.GetByIdAsync(blog.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Existing", result.Name);
        }

        [Fact]
        public async Task GetPagedResponseAsync_ShouldReturnCorrectSubset()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var context = GetInMemoryDbContext(dbName);
            for (int i = 1; i <= 10; i++)
            {
                context.Blogs.Add(new Blog { Name = $"Blog{i}", CreatedAt = DateTime.Now });
            }
            await context.SaveChangesAsync();

            var service = new BlogService(context, _mapper);

            // Act
            var result = await service.GetPagedResponseAsync(pageNumber: 2, pageSize: 3);

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Contains(result, b => b.Name == "Blog5");
            Assert.Contains(result, b => b.Name == "Blog6");
            Assert.Contains(result, b => b.Name == "Blog7");
        }

        [Fact]
        public async Task UpdateAsync_ShouldModifyBlog()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var context = GetInMemoryDbContext(dbName);
            var blog = new Blog { Name = "OldName", Content = "OldContent", UserId = "user1" };
            context.Blogs.Add(blog);
            await context.SaveChangesAsync();

            var service = new BlogService(context, _mapper);
            var updateDto = new BlogDtoCreate { Name = "NewName", Content = "NewContent" };

            // Act
            var result = await service.UpdateAsync(blog.Id, updateDto, "user1");

            // Assert
            Assert.Equal("NewName", result.Name);
            Assert.Equal("NewContent", result.Content);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveBlog()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var context = GetInMemoryDbContext(dbName);
            var blog = new Blog { Name = "ToDelete", UserId = "user1" };
            context.Blogs.Add(blog);
            await context.SaveChangesAsync();

            var service = new BlogService(context, _mapper);

            // Act
            await service.DeleteAsync(blog.Id, "user1");

            // Assert
            Assert.Empty(context.Blogs);
        }

        [Fact]
        public async Task Search_ShouldReturnMatchingBlogs()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var context = GetInMemoryDbContext(dbName);
            context.Blogs.AddRange(
                new Blog { Name = "HelloWorld" },
                new Blog { Name = "HelloUniverse" },
                new Blog { Name = "SomethingElse" }
            );
            await context.SaveChangesAsync();

            var service = new BlogService(context, _mapper);

            // Act
            var result = await service.Search("Hello");

            // Assert
            Assert.Equal(2, result.Count);
            Assert.True(result.All(b => b.Name.Contains("Hello")));
        }

        [Fact]
        public async Task Search_ShouldThrow_WhenTermIsEmpty()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var context = GetInMemoryDbContext(dbName);
            context.Blogs.Add(new Blog { Name = "Sample" });
            await context.SaveChangesAsync();

            var service = new BlogService(context, _mapper);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () => await service.Search(string.Empty));
        }

        [Fact]
        public async Task GetTotal_ShouldReturnCount()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var context = GetInMemoryDbContext(dbName);
            context.Blogs.AddRange(
                new Blog { Name = "A" },
                new Blog { Name = "B" }
            );
            await context.SaveChangesAsync();

            var service = new BlogService(context, _mapper);

            // Act
            var total = await service.GetTotal();

            // Assert
            Assert.Equal(2, total);
        }
    }
}
