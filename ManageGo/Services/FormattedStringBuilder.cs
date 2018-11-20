using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace ManageGo
{
    public class FormattedStringBuilder
    {
        private static readonly Dictionary<string, FontData> _fontDataCache = new Dictionary<string, FontData>();
        private readonly IList<Span> _spans = new List<Span>();
        private bool _withSpaces = true;

        public FormattedStringBuilder Span(string text)
        {
            return Span(text, styleResource: null);
        }

        public FormattedStringBuilder Span(string text, Span span)
        {
            span.Text = text;
            _spans.Add(span);
            return this;
        }

        public FormattedStringBuilder Span(Span span)
        {
            _spans.Add(span);
            return this;
        }

        public FormattedStringBuilder Span(string text, string styleResource)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentNullException($"'Text' cannot be empty.");
            }

            FontData data;
            if (_fontDataCache.ContainsKey(styleResource))
            {
                data = _fontDataCache[styleResource];
            }
            else
            {
                data = !string.IsNullOrWhiteSpace(styleResource)
                    ? FontData.FromResource(styleResource)
                    : FontData.DefaultValues();
                _fontDataCache.Add(styleResource, data);
            }
            _spans.Add(new Span
            {
                Text = text,
                FontAttributes = data.FontAttributes,
                FontFamily = data.FontFamily,
                FontSize = data.FontSize,
                ForegroundColor = data.TextColor
            });
            return this;
        }

        public FormattedStringBuilder WithoutSpaces()
        {
            _withSpaces = false;
            return this;
        }

        public FormattedString Build()
        {
            var result = new FormattedString();
            var count = _spans.Count;
            for (var index = 0; index < count; index++)
            {
                var span = _spans[index];
                result.Spans.Add(span);
                if (index < count && _withSpaces)
                {
                    result.Spans.Add(new Span { Text = " " });
                }
            }
            return result;
        }
    }
}
