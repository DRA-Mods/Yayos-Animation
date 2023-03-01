using System;
using Verse;

namespace yayoAni;

public static class Extensions
{
    public static bool ButtonTextTooltip(this Listing_Standard listing, string label, string tooltip, string highlightTag = null)
    {
#if IDEOLOGY
            var rect = listing.GetRect(30f);
#else
        var rect = listing.GetRect(30f, 1f);
#endif
        var pressed = false;

        if (listing.BoundingRectCached == null || rect.Overlaps(listing.BoundingRectCached.Value))
        {
            pressed = Widgets.ButtonText(rect, label);
            if (highlightTag != null) 
                UIHighlighter.HighlightOpportunity(rect, highlightTag);
        }

        if (!tooltip.NullOrEmpty())
        {
            if (Mouse.IsOver(rect))
                Widgets.DrawHighlight(rect);
            TooltipHandler.TipRegion(rect, tooltip);
        }

        listing.Gap(listing.verticalSpacing);
        return pressed;
    }        

    public static string After(this string s, char c)
    {
        if (s.IndexOf(c) == -1)
            throw new ArgumentException($"Char {c} not found in string {s}");
        return s.Substring(s.IndexOf(c) + 1);
    }

    public static string Until(this string s, char c)
    {
        if (s.IndexOf(c) == -1)
            throw new ArgumentException($"Char {c} not found in string {s}");
        return s.Substring(0, s.IndexOf(c));
    }
}