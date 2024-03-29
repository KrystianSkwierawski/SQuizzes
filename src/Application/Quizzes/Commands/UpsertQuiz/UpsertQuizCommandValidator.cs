﻿using Application.Common.Interfaces;
using FluentValidation;
using Newtonsoft.Json.Linq;

namespace Application.Quizzes.Commands.UpsertQuiz;

public class UpsertQuizCommandValidator : AbstractValidator<UpsertQuizCommand>
{
    private readonly ICaptachaAPIService _captachaAPIService;

    public UpsertQuizCommandValidator(ICaptachaAPIService captachaAPIService)
    {
        _captachaAPIService = captachaAPIService;
    }

    public UpsertQuizCommandValidator()
    {
        RuleFor(command => command.UpsertQuizVm.Title)
            .MaximumLength(50)
            .NotNull()
            .NotEmpty();

        // 8Kbps Bitrate per seconds = 1KB. 300 seconds
        // 320Kbps Bitrate per second = 40KB. 7.5 seconds
        RuleForEach(command => command.UpsertQuizVm.Files)
            .Must(file => file.Length < (300 * 1024)) // 300KB for each file
            .Must(file => HasSupportedFileFormat(file.FileName));

        RuleFor(command => command.UpsertQuizVm.Files.Count())
            .LessThanOrEqualTo(30);

        RuleFor(command => command.UpsertQuizVm.Captacha)
            .MustAsync(BeValid);
    }

    private async Task<bool> BeValid(string captacha, CancellationToken cancellationToken)
    {
        if (Environment.GetEnvironmentVariable("RunningTests") == "true")
            return true;

        JObject response = await _captachaAPIService.GetResponseAsync(captacha);

        bool isValid = (response["success"]?.Value<bool>() == true);

        return isValid;
    }

    public static bool HasSupportedFileFormat(string fileName)
    {
        string[] supportedFormats = { "mp3", "wav", "ogg" };

        string format = fileName.Split(".")[1].ToLower();
        return supportedFormats.Contains(format);
    }
}

