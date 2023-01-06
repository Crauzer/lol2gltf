﻿using System;
using System.Collections.Generic;

namespace lol2gltf.Core.ConversionOptions
{
    public interface IBaseSimpleSkinToGltf
    {
        public string SimpleSkinPath { get; set; }

        public Dictionary<string, ReadOnlyMemory<byte>> MaterialTextures { get; set; }

        public string OutputPath { get; set; }
    }
}
