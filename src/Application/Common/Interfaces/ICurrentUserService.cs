﻿namespace Application.Common.Interfaces;

public interface ICurrentUserService
{
    string? UserName { get; }
    string? UserId { get; }
}

