﻿namespace ChatTask.Shared.DTOs;

public class SendMessageDto
{
    public Guid UserId { get; set; }
    public Guid ToUserId { get; set; }
    public string Message { get; set; } = string.Empty;
}