using RGBFusion390SetColor.Animations;
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
        public double Bright { get; set; }
        public Animation Animation { get; set; }                

        public Color GetTransformedColor()
        {
            if (_simBrightness.Contains(AreaId))
            {
                var brightness = (Bright + 1) / 11.0;
                return Color.FromRgb((byte)(NewColor.R * brightness), (byte)(NewColor.G * brightness), (byte)(NewColor.B * brightness));
            }
            else
            {
                return NewColor;
            }
        }

        public byte GetTransfromedBrightness()
        {
            if (_simBrightness.Contains(AreaId))
            {
                return 9;
            }
            else
            {
                return (byte)Bright;
            }
        }

        public CommUI.Area_class BuildArea()
        {
            var patternCombItem = new CommUI.Pattern_Comb_Item
            {
                Bg_Brush_Solid = { Color = GetTransformedColor() },
                Sel_Item = { Style = null }
            };
            patternCombItem.Sel_Item.Background = patternCombItem.Bg_Brush_Solid;

            patternCombItem.Sel_Item.Content = string.Empty;
            patternCombItem.But_Args = CommUI.Get_Color_Sceenes_class_From_Brush(patternCombItem.Bg_Brush_Solid);

            patternCombItem.Bri = GetTransfromedBrightness();
            patternCombItem.Speed = Speed;

            patternCombItem.Type = NewMode;

            return new CommUI.Area_class(patternCombItem, AreaId, null);
        }
    }
}
