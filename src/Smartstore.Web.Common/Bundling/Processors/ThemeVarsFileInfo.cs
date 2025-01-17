﻿using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.FileProviders;
using Smartstore.IO;
using Smartstore.Utilities;
using Smartstore.Web.Theming;

namespace Smartstore.Web.Bundling.Processors
{
    internal class ThemeVarsFileInfo : IFileInfo, IFileHashProvider
    {
        private readonly string _theme;
        private readonly int _storeId;
        private readonly ThemeVariableRepository _repo;

        private string _content;
        private int? _contentHash;

        public ThemeVarsFileInfo(string name, string theme, int storeId, ThemeVariableRepository repo)
        {
            Guard.NotNull(repo, nameof(repo));

            _theme = theme;
            _storeId = storeId;
            _repo = repo;

            Name = name;
            PhysicalPath = name;
        }

        public bool Exists => true;

        public bool IsDirectory => false;

        public DateTimeOffset LastModified => DateTimeOffset.MinValue;

        public long Length => CreateReadStream().Length;

        public string Name { get; }

        public string PhysicalPath { get; }

        public Stream CreateReadStream()
        {
            return GenerateStreamFromString(GetContent());
        }

        public int GetFileHash()
        {
            if (_contentHash == null)
            {
                var css = GetContent();
                _contentHash = (int)XxHashUnsafe.ComputeHash(css);
            }

            return _contentHash.Value;
        }

        private string GetContent()
        {
            if (_content == null)
            {
                var css = string.Empty;
                if (_theme.HasValue())
                {
                    css = _repo.GetPreprocessorCssAsync(_theme, _storeId).Await();
                }

                _content = css;
            }

            return _content;
        }

        private static Stream GenerateStreamFromString(string value)
        {
            var stream = new MemoryStream();

            using (var writer = new StreamWriter(stream, Encoding.Unicode, 1024, true))
            {
                writer.Write(value);
                writer.Flush();
                stream.Seek(0, SeekOrigin.Begin);
                return stream;
            }
        }
    }
}
