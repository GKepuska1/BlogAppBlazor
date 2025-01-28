using AutoMapper;
using BlogApp.Core.Context;
using BlogApp.Core.Services;
using BlogApp.Domain.Dtos;
using BlogApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BlogApp.UnitTests
{
    public class CommentServiceTests
    {
        private readonly IMapper _mapper;

        public CommentServiceTests()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Comment, CommentDto>();
                cfg.CreateMap<CommentDtoCreate, Comment>();
            });
            _mapper = config.CreateMapper();
        }

        private IAppDbContext GetInMemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            var context = new AppDbContext(options);
            return context;
        }

        [Fact]
        public async Task GetByBlogIdAsync_ShouldReturnCorrectComments()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString(); 
            var context = GetInMemoryDbContext(dbName);
            context.Comments.Add(new Comment { BlogId = 1, Content = "Comment A", CreatedAt = DateTime.Now });
            context.Comments.Add(new Comment { BlogId = 1, Content = "Comment B", CreatedAt = DateTime.Now });
            context.Comments.Add(new Comment { BlogId = 2, Content = "Comment C", CreatedAt = DateTime.Now });
            await context.SaveChangesAsync();

            var service = new CommentService(context, _mapper);

            // Act
            var result = await service.GetByBlogIdAsync(1);

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task CreateAsync_ShouldAddComment()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var context = GetInMemoryDbContext(dbName);
            var service = new CommentService(context, _mapper);

            var dto = new CommentDtoCreate { Content = "Newly added comment" };

            // Act
            var created = await service.CreateAsync(blogId: 10, dto, userId: "User123");

            // Assert
            Assert.NotNull(created);
            Assert.Equal("Newly added comment", created.Content);

            // Verify it's in the database
            var all = context.Comments.ToList();
            Assert.Single(all);
            Assert.Equal(10, all[0].BlogId);
        }

        [Fact]
        public async Task UpdateAsync_ShouldModifyComment()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var context = GetInMemoryDbContext(dbName);

            // Seed a comment
            var comment = new Comment
            {
                UserId = "User123",
                BlogId = 99,
                Content = "Old Content",
                CreatedAt = DateTime.Now
            };
            context.Comments.Add(comment);
            await context.SaveChangesAsync();

            var service = new CommentService(context, _mapper);
            var updateDto = new CommentDtoCreate { Content = "Updated Content" };

            // Act
            var updated = await service.UpdateAsync(comment.Id, updateDto, "User123");

            // Assert
            Assert.Equal("Updated Content", updated.Content);

            // Check the actual DB record
            var inDb = context.Comments.Find(comment.Id);
            Assert.Equal("Updated Content", inDb.Content);
        }

        [Fact]
        public async Task DeleteCommentsByBlogIdAsync_ShouldRemoveAll()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var context = GetInMemoryDbContext(dbName);

            context.Comments.Add(new Comment { BlogId = 1, Content = "Comment A" });
            context.Comments.Add(new Comment { BlogId = 1, Content = "Comment B" });
            context.Comments.Add(new Comment { BlogId = 2, Content = "Comment C" });
            await context.SaveChangesAsync();

            var service = new CommentService(context, _mapper);

            // Act
            await service.DeleteCommentsByBlogIdAsync(1);

            // Assert
            Assert.Single(context.Comments.ToList());
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnCommentIfExists()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var context = GetInMemoryDbContext(dbName);

            var comment = new Comment { Content = "Some comment" };
            context.Comments.Add(comment);
            await context.SaveChangesAsync();

            var service = new CommentService(context, _mapper);

            // Act
            var found = await service.GetByIdAsync(comment.Id);

            // Assert
            Assert.NotNull(found);
            Assert.Equal("Some comment", found.Content);
        }

        [Fact]
        public async Task DeleteCommentAsync_ShouldRemoveComment()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var context = GetInMemoryDbContext(dbName);

            var comment = new Comment { Content = "To be deleted" };
            context.Comments.Add(comment);
            await context.SaveChangesAsync();

            var service = new CommentService(context, _mapper);

            // Act
            await service.DeleteCommentAsync(comment);

            // Assert
            Assert.Empty(context.Comments);
        }
    }

}