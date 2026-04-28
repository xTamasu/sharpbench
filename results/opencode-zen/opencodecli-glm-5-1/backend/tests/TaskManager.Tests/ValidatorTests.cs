// Unit tests for FluentValidation validators
using FluentValidation;
using TaskManager.Application.DTOs;
using TaskManager.Application.Validators;
using TaskManager.Domain.Enums;

namespace TaskManager.Tests;

public class ValidatorTests
{
    [Fact]
    public void RegisterRequestValidator_ValidRequest_Passes()
    {
        var validator = new RegisterRequestValidator();
        var result = validator.Validate(new RegisterRequest("test@test.com", "password123", "User"));
        Assert.True(result.IsValid);
    }

    [Fact]
    public void RegisterRequestValidator_EmptyEmail_Fails()
    {
        var validator = new RegisterRequestValidator();
        var result = validator.Validate(new RegisterRequest("", "password123", "User"));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void RegisterRequestValidator_InvalidEmail_Fails()
    {
        var validator = new RegisterRequestValidator();
        var result = validator.Validate(new RegisterRequest("not-an-email", "password123", "User"));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void RegisterRequestValidator_ShortPassword_Fails()
    {
        var validator = new RegisterRequestValidator();
        var result = validator.Validate(new RegisterRequest("test@test.com", "12345", "User"));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void LoginRequestValidator_ValidRequest_Passes()
    {
        var validator = new LoginRequestValidator();
        var result = validator.Validate(new LoginRequest("test@test.com", "password"));
        Assert.True(result.IsValid);
    }

    [Fact]
    public void LoginRequestValidator_EmptyPassword_Fails()
    {
        var validator = new LoginRequestValidator();
        var result = validator.Validate(new LoginRequest("test@test.com", ""));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void CreateTaskRequestValidator_TitleTooLong_Fails()
    {
        var validator = new CreateTaskRequestValidator();
        var result = validator.Validate(new CreateTaskRequest(new string('a', 201), null, TaskItemStatus.Todo, TaskPriority.Medium, null, null));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void CreateTaskRequestValidator_DescriptionTooLong_Fails()
    {
        var validator = new CreateTaskRequestValidator();
        var result = validator.Validate(new CreateTaskRequest("Title", new string('a', 5001), TaskItemStatus.Todo, TaskPriority.Medium, null, null));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void CreateTaskRequestValidator_EmptyTitle_Fails()
    {
        var validator = new CreateTaskRequestValidator();
        var result = validator.Validate(new CreateTaskRequest("", null, TaskItemStatus.Todo, TaskPriority.Medium, null, null));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void CreateCommentRequestValidator_BodyTooLong_Fails()
    {
        var validator = new CreateCommentRequestValidator();
        var result = validator.Validate(new CreateCommentRequest(new string('a', 2001)));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void CreateCommentRequestValidator_EmptyBody_Fails()
    {
        var validator = new CreateCommentRequestValidator();
        var result = validator.Validate(new CreateCommentRequest(""));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void UpdateCommentRequestValidator_ValidRequest_Passes()
    {
        var validator = new UpdateCommentRequestValidator();
        var result = validator.Validate(new UpdateCommentRequest("Updated body"));
        Assert.True(result.IsValid);
    }
}