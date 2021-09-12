﻿using System;

namespace chia.dotnet
{
    public record PoolInfo
    {
        public string Name { get; init; } = string.Empty;
        public Uri LogoUri { get; init; } = new Uri("http://localhost");
        public ulong MinimumDifficulty { get; init; }
        public uint RelativeLockHeight { get; init; }
        public byte ProtocolVersion { get; init; }
        public decimal Fee { get; init; }
        public string Description { get; init; } = string.Empty;
        public string? TargetPuzzleHash { get; init; }
        public int AuthenticationTokenTimeout { get; init; }
    }
}
