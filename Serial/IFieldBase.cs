﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

namespace Serial;

internal interface IFieldBase
{
    bool IsSupported { get; }
    string? Name { get; }
}