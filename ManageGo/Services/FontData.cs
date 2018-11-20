using System;
using System.Linq;
using Xamarin.Forms;

namespace ManageGo
{
    public class FontData
    {
        public double FontSize { get; set; }
        public FontAttributes FontAttributes { get; set; }
        public Color TextColor { get; set; }
        public string FontFamily { get; set; }

        public static FontData DefaultValues()
        {
            return new FontData
            {
                FontSize = (double)Label.FontSizeProperty.DefaultValue,
                FontAttributes = (FontAttributes)Label.FontAttributesProperty.DefaultValue,
                TextColor = (Color)Label.TextColorProperty.DefaultValue,
                FontFamily = Label.FontFamilyProperty.DefaultValue.ToString()
            };
        }

        public static FontData FromResource(string resourceName)
        {
            var resource = Application.Current.Resources[resourceName];
            if (resource == null)
            {
                return DefaultValues();
            }
            var style = (Style)resource;

            var data = new FontData();
            var colorSetter = style.Setters.FirstOrDefault(x => x.Property == Label.TextColorProperty);
            var attrSetter = style.Setters.FirstOrDefault(x => x.Property == Label.FontAttributesProperty);
            var fontSizeSetter = style.Setters.FirstOrDefault(x => x.Property == Label.FontSizeProperty);
            var fontFamilySetter = style.Setters.FirstOrDefault(x => x.Property == Label.FontFamilyProperty);

            data.TextColor = colorSetter?.Value as Color? ?? (Color)Label.TextColorProperty.DefaultValue;
            data.FontSize = fontSizeSetter?.Value as double? ?? (double)Label.FontSizeProperty.DefaultValue;

            data.FontFamily = fontFamilySetter != null && fontFamilySetter.Value != null
                ? fontFamilySetter.Value.ToString()
                : Label.FontFamilyProperty.DefaultValue?.ToString();

            data.FontAttributes = attrSetter?.Value != null
                ? (FontAttributes)Enum.Parse(typeof(FontAttributes), attrSetter.Value.ToString())
                : (FontAttributes)Label.FontAttributesProperty.DefaultValue;

            return data;
        }
    }
}
