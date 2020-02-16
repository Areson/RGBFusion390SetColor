using SelLEDControl;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;

namespace RGBFusion390SetColor
{
    public class LedCommand
    {
        private static readonly HashSet<sbyte> _simBrightness = new HashSet<sbyte>();

        public static void SimulateBrightness(sbyte areaId, bool enabled = true)
        {
            if (enabled)
            {
                _simBrightness.Add(areaId);
            }
            else
            {
                _simBrightness.Remove(areaId);
            }
        }

        [DefaultValue(-1)]
        public sbyte AreaId { get; set; }
        [DefaultValue(0)]
        public sbyte NewMode { get; set; }
        public Color NewColor { get; set; }
        [DefaultValue(5)]
        public sbyte Speed { get; set; }
        [DefaultValue(9)]
        public sbyte Bright { get; set; }
        public Animation.Animation Animation { get; set; }

        public CommUI.Area_class BuildArea()
        {
            var color = NewColor;

            if (_simBrightness.Contains(AreaId))
            {
                var brightness = (Bright + 1) / 10.0;
                color = Color.FromRgb((byte)(color.R * brightness), (byte)(color.G * brightness), (byte)(color.B * brightness));
            }

            var patternCombItem = new CommUI.Pattern_Comb_Item
            {
                Bg_Brush_Solid = { Color = color },
                Sel_Item = { Style = null }
            };
            patternCombItem.Sel_Item.Background = patternCombItem.Bg_Brush_Solid;

            patternCombItem.Sel_Item.Content = string.Empty;
            patternCombItem.But_Args = CommUI.Get_Color_Sceenes_class_From_Brush(patternCombItem.Bg_Brush_Solid);

            patternCombItem.Bri = Bright;
            patternCombItem.Speed = Speed;

            patternCombItem.Type = NewMode;

            return new CommUI.Area_class(patternCombItem, AreaId, null);
        }
    }
}
