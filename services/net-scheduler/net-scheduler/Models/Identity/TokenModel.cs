﻿namespace NetScheduler.Models.Identity;

public class TokenModel
{
    public TokenModel(string token)
    {
        Token = token;
    }

    public string Token { get; set; }
}