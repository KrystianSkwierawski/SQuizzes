﻿using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Quizzes.Commands.DeleteQuiz;
using Application.Quizzes.Commands.UpsertQuiz;
using Domain.Entities;
using Domain.ValueObjects;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Application.IntegrationTests.Quizzes.Commands;

using static System.Net.WebRequestMethods;
using static Testing;
public class CreateQuizTests : TestBase
{
    [Test]
    public async Task ShouldRequireMinimumFields()
    {
        var command = new UpsertQuizCommand();

        await FluentActions.Invoking(() =>
             SendAsync(command)).Should().ThrowAsync<Exception>();
    }


    [Test]
    public async Task ShouldCreateQuiz()
    {
        //Arrang
        var user = await RunAsDefaultUserAsync();

        IList<IFormFile> files = new List<IFormFile>() {
            new FormFile(null, 0, 0, null, "sfx.wav")
        };


        UpsertQuizCommand command = new()
        {
            UpsertQuizVm = new()
            {
                Title = "test",
                Files = files
            }
        };

        //Act
        var quizId = await SendAsync(command);

        //Assert
        Quiz result = await FindAsync<Quiz>(quizId);

        result.Should().NotBeNull();
        result.Title.Should().Be(command.UpsertQuizVm.Title);
        result.CreatedBy.Should().Be(user.Item1);
        result.Author.Should().Be(user.Item2);

        string nameWithoutExtension = Path.GetFileNameWithoutExtension(files[0].FileName);
        string extension = Path.GetExtension(files[0].FileName);
        string encodedNameWithExtension = "0" + extension; //index + extension

        result.SFXs[0].Name.Should().Be(nameWithoutExtension);
        result.SFXs[0].EncodedNameWithExtension.Should().Be(encodedNameWithExtension);

        // Application.IntegrationTests\bin\Debug\net6.0\wwwroot\assets\SFXs\{id}
        string directory = Path.Combine("./wwwroot", "assets", "SFXs", quizId);
        Directory.Exists(directory).Should().BeTrue();
    }

    [Test]
    public async Task ShouldUpdateQuiz()
    {
        //Arrang
        var (userId, userName) = await RunAsDefaultUserAsync();

        IList<IFormFile> files = new List<IFormFile>() {
            new FormFile(null, 0, 0, null, "sfx.wav")
        };

        var quizId = await SendAsync(new UpsertQuizCommand
        {
            UpsertQuizVm = new()
            {
                Title = "test",
                Files = files
            }
        });

        var command = new UpsertQuizCommand()
        {
            UpsertQuizVm = new()
            {
                Id = quizId,
                Title = "edit",
                Files = files
            }
        };
        //Act
        await SendAsync(command);

        //Assert
        Quiz result = await FindAsync<Quiz>(quizId);

        result.Should().NotBeNull();
        result.Title.Should().Be(command.UpsertQuizVm.Title);
        result.CreatedBy.Should().Be(userId);
        result.Author.Should().Be(userName);

        string nameWithoutExtension = Path.GetFileNameWithoutExtension(files[0].FileName);
        string extension = Path.GetExtension(files[0].FileName);
        string encodedNameWithExtension = "0" + extension; //index + extension

        result.SFXs[0].Name.Should().Be(nameWithoutExtension);
        result.SFXs[0].EncodedNameWithExtension.Should().Be(encodedNameWithExtension);

        // Application.IntegrationTests\bin\Debug\net6.0\wwwroot\assets\SFXs\{id}
        string directory = Path.Combine("./wwwroot", "assets", "SFXs", quizId);
        Directory.Exists(directory).Should().BeTrue();
    }
}

