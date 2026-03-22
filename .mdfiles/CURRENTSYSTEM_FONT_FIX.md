# CurrentSystemControl Font Size Consistency Fix

## Current Issues

The CurrentSystemControl uses inconsistent font sizes compared to other controls in the app (like ExoBioSalesHistory).

## Standard Font Sizes in the App

Based on ExoBioSalesHistory and other controls:
- **Major headers:** 16pt (SemiBold)
- **Section labels/counts:** 12pt (secondary color)
- **Primary values:** 14pt (SemiBold)
- **Secondary text:** 10pt (secondary color)
- **Badges:** 10pt (SemiBold)

## Changes Needed in CurrentSystemControl.xaml

### Header Section (Lines ~35-38)
**Current:**
```xml
<TextBlock Text="{Binding ValuableBodiesCount, Mode=OneWay}" 
           FontSize="11" 
           Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
```

**Should be:**
```xml
<TextBlock Text="{Binding ValuableBodiesCount, Mode=OneWay}" 
           FontSize="12" 
           Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
```

### Body Name (Lines ~57-59)
**Current:**
```xml
<TextBlock Text="{x:Bind BodyName}" 
           FontSize="13" 
           FontWeight="SemiBold"/>
```

**Should be:**
```xml
<TextBlock Text="{x:Bind BodyName}" 
           FontSize="14" 
           FontWeight="SemiBold"/>
```

### Badge Text (Lines ~71, 82, 93)
**Current:**
```xml
<TextBlock Text="LANDABLE"
           FontSize="8"
           FontWeight="SemiBold"
           Foreground="White"/>
```

**Should be:**
```xml
<TextBlock Text="LANDABLE"
           FontSize="10"
           FontWeight="SemiBold"
           Foreground="White"/>
```

*(Apply to all three badges: LANDABLE, TERRAFORMABLE, BIOLOGICAL)*

### Discovery Status (Lines ~117-120)
**Current:**
```xml
<TextBlock Grid.Column="1"
           Text="{x:Bind DiscoveryStatus}" 
           FontSize="9" 
           Foreground="{ThemeResource SystemFillColorSuccessBrush}"
           VerticalAlignment="Center"/>
```

**Should be:**
```xml
<TextBlock Grid.Column="1"
           Text="{x:Bind DiscoveryStatus}" 
           FontSize="10" 
           Foreground="{ThemeResource SystemFillColorSuccessBrush}"
           VerticalAlignment="Center"/>
```

## Summary of Changes

| Element | Old Size | New Size |
|---------|----------|----------|
| ValuableBodiesCount | 11pt | 12pt |
| Body Name | 13pt | 14pt |
| Badge Text (all 3) | 8pt | 10pt |
| Discovery Status | 9pt | 10pt |

These changes will make CurrentSystemControl consistent with the app's standard font sizing used in ExoBioSalesHistory and other controls.
