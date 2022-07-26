﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AccountManager.Core.Models.RiotGames.League.Requests
{
    public sealed class LeagueSignInRequest
    {
        [JsonPropertyName("username")]
        public string? Username { get; set; }
        [JsonPropertyName("password")]
        public string? Password { get; set; }
        [JsonPropertyName("persistLogin")]
        public bool StaySignedIn { get; set; }
        
    }
}
