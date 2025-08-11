using MudBlazor;

namespace FinTrack.Maui.Themes;

public static class FinTrackTheme
{
    public static MudTheme DefaultTheme => new()
    {
        PaletteLight = new PaletteLight()
        {
            Primary = "#2E7D32", // Financial green for primary actions
            Secondary = "#1976D2", // Professional blue for secondary elements
            Tertiary = "#FF6F00", // Accent orange for highlights
            Success = "#4CAF50", // Success green
            Warning = "#FF9800", // Warning orange
            Error = "#F44336", // Error red
            Info = "#2196F3", // Info blue
            Background = "#FAFAFA", // Light background
            Surface = "#FFFFFF", // Card surfaces
            AppbarBackground = "#2E7D32", // Primary green for app bar
            DrawerBackground = "#FFFFFF", // White drawer background
            TextPrimary = "#212121", // Dark text
            TextSecondary = "#757575", // Secondary text
            ActionDefault = "#757575", // Default action color
            ActionDisabled = "#BDBDBD", // Disabled actions
            ActionDisabledBackground = "#E0E0E0", // Disabled background
            Divider = "#E0E0E0", // Divider lines
            DividerLight = "#F5F5F5", // Light dividers
            TableLines = "#E0E0E0", // Table borders
            LinesDefault = "#E0E0E0", // Default lines
            LinesInputs = "#BDBDBD", // Input borders
            TextDisabled = "#9E9E9E", // Disabled text
            GrayDefault = "#9E9E9E", // Default gray
            GrayLight = "#F5F5F5", // Light gray
            GrayLighter = "#FAFAFA", // Lighter gray
            GrayDark = "#616161", // Dark gray
            GrayDarker = "#424242", // Darker gray
            OverlayDark = "rgba(33,33,33,0.4)", // Dark overlay
            OverlayLight = "rgba(255,255,255,0.4)" // Light overlay
        },
        PaletteDark = new PaletteDark()
        {
            Primary = "#4CAF50", // Brighter green for dark mode
            Secondary = "#42A5F5", // Brighter blue for dark mode
            Tertiary = "#FFB74D", // Softer orange for dark mode
            Success = "#66BB6A", // Success green
            Warning = "#FFB74D", // Warning orange
            Error = "#EF5350", // Error red
            Info = "#42A5F5", // Info blue
            Background = "#121212", // Dark background
            Surface = "#1E1E1E", // Card surfaces
            AppbarBackground = "#1E1E1E", // Dark app bar
            DrawerBackground = "#1E1E1E", // Dark drawer
            TextPrimary = "#FFFFFF", // Light text
            TextSecondary = "#B0B0B0", // Secondary text
            ActionDefault = "#B0B0B0", // Default action color
            ActionDisabled = "#616161", // Disabled actions
            ActionDisabledBackground = "#2C2C2C", // Disabled background
            Divider = "#373737", // Divider lines
            DividerLight = "#2C2C2C", // Light dividers
            TableLines = "#373737", // Table borders
            LinesDefault = "#373737", // Default lines
            LinesInputs = "#616161", // Input borders
            TextDisabled = "#616161", // Disabled text
            GrayDefault = "#9E9E9E", // Default gray
            GrayLight = "#2C2C2C", // Light gray
            GrayLighter = "#1E1E1E", // Lighter gray
            GrayDark = "#B0B0B0", // Dark gray
            GrayDarker = "#FFFFFF", // Darker gray
            OverlayDark = "rgba(0,0,0,0.7)", // Dark overlay
            OverlayLight = "rgba(255,255,255,0.1)" // Light overlay
        },
        Typography = new Typography()
        {
            Default = new Default()
            {
                FontFamily = new[] { "Roboto", "Helvetica", "Arial", "sans-serif" },
                FontSize = "0.875rem",
                FontWeight = 400,
                LineHeight = 1.43,
                LetterSpacing = "0.01071em"
            },
            H1 = new H1()
            {
                FontFamily = new[] { "Roboto", "Helvetica", "Arial", "sans-serif" },
                FontSize = "6rem",
                FontWeight = 300,
                LineHeight = 1.167,
                LetterSpacing = "-0.01562em"
            },
            H2 = new H2()
            {
                FontFamily = new[] { "Roboto", "Helvetica", "Arial", "sans-serif" },
                FontSize = "3.75rem",
                FontWeight = 300,
                LineHeight = 1.2,
                LetterSpacing = "-0.00833em"
            },
            H3 = new H3()
            {
                FontFamily = new[] { "Roboto", "Helvetica", "Arial", "sans-serif" },
                FontSize = "3rem",
                FontWeight = 400,
                LineHeight = 1.167,
                LetterSpacing = "0em"
            },
            H4 = new H4()
            {
                FontFamily = new[] { "Roboto", "Helvetica", "Arial", "sans-serif" },
                FontSize = "2.125rem",
                FontWeight = 400,
                LineHeight = 1.235,
                LetterSpacing = "0.00735em"
            },
            H5 = new H5()
            {
                FontFamily = new[] { "Roboto", "Helvetica", "Arial", "sans-serif" },
                FontSize = "1.5rem",
                FontWeight = 400,
                LineHeight = 1.334,
                LetterSpacing = "0em"
            },
            H6 = new H6()
            {
                FontFamily = new[] { "Roboto", "Helvetica", "Arial", "sans-serif" },
                FontSize = "1.25rem",
                FontWeight = 500,
                LineHeight = 1.6,
                LetterSpacing = "0.0075em"
            },
            Button = new MudBlazor.Button()
            {
                FontFamily = new[] { "Roboto", "Helvetica", "Arial", "sans-serif" },
                FontSize = "0.875rem",
                FontWeight = 500,
                LineHeight = 1.75,
                LetterSpacing = "0.02857em",
                TextTransform = "uppercase"
            },
            Body1 = new Body1()
            {
                FontFamily = new[] { "Roboto", "Helvetica", "Arial", "sans-serif" },
                FontSize = "1rem",
                FontWeight = 400,
                LineHeight = 1.5,
                LetterSpacing = "0.00938em"
            },
            Body2 = new Body2()
            {
                FontFamily = new[] { "Roboto", "Helvetica", "Arial", "sans-serif" },
                FontSize = "0.875rem",
                FontWeight = 400,
                LineHeight = 1.43,
                LetterSpacing = "0.01071em"
            },
            Caption = new Caption()
            {
                FontFamily = new[] { "Roboto", "Helvetica", "Arial", "sans-serif" },
                FontSize = "0.75rem",
                FontWeight = 400,
                LineHeight = 1.66,
                LetterSpacing = "0.03333em"
            },
            Subtitle1 = new Subtitle1()
            {
                FontFamily = new[] { "Roboto", "Helvetica", "Arial", "sans-serif" },
                FontSize = "1rem",
                FontWeight = 400,
                LineHeight = 1.75,
                LetterSpacing = "0.00938em"
            },
            Subtitle2 = new Subtitle2()
            {
                FontFamily = new[] { "Roboto", "Helvetica", "Arial", "sans-serif" },
                FontSize = "0.875rem",
                FontWeight = 500,
                LineHeight = 1.57,
                LetterSpacing = "0.00714em"
            },
            Overline = new Overline()
            {
                FontFamily = new[] { "Roboto", "Helvetica", "Arial", "sans-serif" },
                FontSize = "0.75rem",
                FontWeight = 400,
                LineHeight = 2.66,
                LetterSpacing = "0.08333em",
                TextTransform = "uppercase"
            }
        },
        Shadows = new MudBlazor.Shadow(),
        LayoutProperties = new LayoutProperties()
        {
            DefaultBorderRadius = "4px",
            AppbarHeight = "64px",
            DrawerWidthLeft = "240px",
            DrawerWidthRight = "240px"
        },
        ZIndex = new ZIndex()
        {
            Drawer = 1200,
            AppBar = 1100,
            Dialog = 1300,
            Popover = 1400,
            Snackbar = 1400,
            Tooltip = 1500
        }
    };
}