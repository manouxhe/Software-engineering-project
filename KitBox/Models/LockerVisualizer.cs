using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace KitBox;

public class LockerVisualizer : Control
{
    //déclaration de propriété
    public static readonly StyledProperty<IEnumerable<object>> LockersSourceProperty =
        AvaloniaProperty.Register<LockerVisualizer, IEnumerable<object>>(nameof(LockersSource));

    public static readonly StyledProperty<object> AngleIronBrushProperty =
        AvaloniaProperty.Register<LockerVisualizer, object>(nameof(AngleIronBrush), Brushes.Black);

    // Nouvelles propriétés pour recevoir les vraies dimensions depuis le XAML
    public static readonly StyledProperty<double> CabinetWidthProperty =
        AvaloniaProperty.Register<LockerVisualizer, double>(nameof(CabinetWidth), 60);

    public static readonly StyledProperty<double> CabinetDepthProperty =
        AvaloniaProperty.Register<LockerVisualizer, double>(nameof(CabinetDepth), 40);

    public IEnumerable<object> LockersSource
    {
        get => GetValue(LockersSourceProperty);
        set => SetValue(LockersSourceProperty, value);
    }

    public object AngleIronBrush
    {
        get => GetValue(AngleIronBrushProperty);
        set => SetValue(AngleIronBrushProperty, value);
    }

    public double CabinetWidth
    {
        get => GetValue(CabinetWidthProperty);
        set => SetValue(CabinetWidthProperty, value);
    }

    public double CabinetDepth
    {
        get => GetValue(CabinetDepthProperty);
        set => SetValue(CabinetDepthProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == LockersSourceProperty)
        {
            //si LockersSource change on se désabonne de l'ancienne collection et s'abonne à la nouvelle
            if (change.OldValue is INotifyCollectionChanged oldCollection)
                oldCollection.CollectionChanged -= OnLockersCollectionChanged;
            if (change.NewValue is INotifyCollectionChanged newCollection)
                newCollection.CollectionChanged += OnLockersCollectionChanged;
            InvalidateVisual();
        }
        //Si changement dans les dimensions et les couleurs on redessine
        if (change.Property == AngleIronBrushProperty ||
            change.Property == CabinetWidthProperty ||
            change.Property == CabinetDepthProperty)
            InvalidateVisual();
    }

    private void OnLockersCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        => InvalidateVisual();

    private Point Project(double x, double y, double z, Point origin, double scale, double angleH, double angleV)
    {
        //les deux angles définissent le style de vue 3D
        double px = (x - z) * Math.Cos(angleH);
        double py = (x + z) * Math.Sin(angleV) - y;
        //on applique l'échelle
        return new Point(origin.X + px * scale, origin.Y + py * scale);
    }

    private bool IsTransparentColor(string? color) =>
        color?.ToLower() is "verre" or "glass";

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        //si pas de casier on ne dessine rien
        if (LockersSource == null || !LockersSource.Any() || Bounds.Width <= 0 || Bounds.Height <= 0)
            return;

        var lockers = LockersSource.Cast<dynamic>().OrderBy(l => (int)l.Position).ToList();

        // On utilise les vraies dimensions du Cabinet
        double realWidth = CabinetWidth > 0 ? CabinetWidth : 60;
        double realDepth = CabinetDepth > 0 ? CabinetDepth : 40;
        //totalHeight est la somme de tous les casiers
        double totalHeight = lockers.Sum(l => (double)l.Height);
        if (totalHeight <= 0) totalHeight = 100;

        // On normalise les dimensions pour que le ratio largeur/profondeur/hauteur
        // soit respecté, mais que tout rentre toujours dans les bounds.
        // On fixe une hauteur de référence, et on calcule largeur/profondeur proportionnellement.
        double refHeight = 100.0; // unité de référence
        double normWidth  = realWidth  / totalHeight * refHeight;
        double normDepth  = realDepth  / totalHeight * refHeight;
        double normHeight = refHeight;

        double angleH = Math.PI / 10;
        double angleV = Math.PI / 14;
        double margin = 30;

        double projectedWidth  = (normWidth + normDepth) * Math.Cos(angleH);
        double projectedHeight = (normWidth + normDepth) * Math.Sin(angleV) + normHeight;       

        double scale = Math.Min(
            (Bounds.Width - margin * 2) / projectedWidth,
            (Bounds.Height - margin * 2) / projectedHeight
        );
        if (scale <= 0) return;

        // Centrage horizontal et vertical dans les bounds
        double totalProjectedW = projectedWidth  * scale;
        double totalProjectedH = projectedHeight * scale;
        double originX = Bounds.Width / 2;
        double originY = margin + normHeight * scale;   

        Point origin = new Point(originX, originY);

        IBrush ironBrush = AngleIronBrush is string s ? ParseColor(s) : (AngleIronBrush as IBrush ?? Brushes.Black);
        Pen outlinePen = new Pen(Brushes.Black, 0.8);
        Pen ironPen = new Pen(ironBrush, 2);

        double currentY = 0;

        foreach (var locker in lockers)
        {
            double rawH = (double)locker.Height;
            double h = rawH / totalHeight * normHeight;

            bool hasDoor = (bool)locker.HasDoor;
            string? doorColorStr = locker.DoorColor?.ToString();
            bool isGlassDoor = hasDoor && IsTransparentColor(doorColorStr);

            IBrush panelBrush = ParseColor(locker.PanelColor?.ToString());
            IBrush sideBrush = DarkenBrush(panelBrush, 0.72);
            IBrush topBrush = DarkenBrush(panelBrush, 0.88);
            IBrush innerBrush = DarkenBrush(panelBrush, 0.80);
            IBrush shelfBrush = DarkenBrush(panelBrush, 0.65);

            double fz = normDepth / 2;   //fz est la profondeur de la face avant
            double bz = -normDepth * 0.3;    //bz est ou se trouve le fond intérieur
            //on projette les 8 coins du cube en 2D (L/R pour préciser si gauche ou droite et F/B pour avant/arriere)
            Point bFL = Project(-normWidth / 2, currentY, fz, origin, scale, angleH, angleV);
            Point bFR = Project(normWidth / 2, currentY, fz, origin, scale, angleH, angleV);
            Point tFL = Project(-normWidth / 2, currentY + h, fz, origin, scale, angleH, angleV);
            Point tFR = Project(normWidth / 2, currentY + h, fz, origin, scale, angleH, angleV);
            Point bBL = Project(-normWidth / 2, currentY, -normDepth / 2, origin, scale, angleH, angleV);
            Point bBR = Project(normWidth / 2, currentY, -normDepth / 2, origin, scale, angleH, angleV);
            Point tBL = Project(-normWidth / 2, currentY + h, -normDepth / 2, origin, scale, angleH, angleV);
            Point tBR = Project(normWidth / 2, currentY + h, -normDepth / 2, origin, scale, angleH, angleV);
            //les 4 coins intérieurs
            Point ibFL = Project(-normWidth / 2, currentY, bz, origin, scale, angleH, angleV);
            Point ibFR = Project(normWidth / 2, currentY, bz, origin, scale, angleH, angleV);
            Point itFL = Project(-normWidth / 2, currentY + h, bz, origin, scale, angleH, angleV);
            Point itFR = Project(normWidth / 2, currentY + h, bz, origin, scale, angleH, angleV);
            //on dessine la face droite en relient le bas avant au bas arriere etc
            DrawFace(context, sideBrush, outlinePen, bFR, bBR, tBR, tFR);
            //on dessine le dessus
            DrawFace(context, topBrush, outlinePen, tFL, tFR, tBR, tBL);
            //si le casier est ouvert ou a une porte en verre alors on dessine aussi les faces intérieures
            if (!hasDoor || isGlassDoor)
            {
                DrawFace(context, shelfBrush, outlinePen, bFL, bFR, ibFR, ibFL);
                DrawFace(context, DarkenBrush(panelBrush, 0.82), outlinePen, tFL, tFR, itFR, itFL);
                DrawFace(context, innerBrush, outlinePen, ibFL, ibFR, itFR, itFL);
                DrawFace(context, shelfBrush, outlinePen, bFL, ibFL, itFL, tFL);
                DrawFace(context, shelfBrush, outlinePen, bFR, ibFR, itFR, tFR);
            }
            //si le casier a une porte (pas en verre) alors on dessine la face avant avec la couleur de porte + une petite ellipse pour représenter la poignée
            if (hasDoor)
            {
                IBrush doorBrush = ParseColor(doorColorStr);
                DrawFace(context, doorBrush, outlinePen, bFL, bFR, tFR, tFL);
                var handlePos = Project(normWidth / 4, currentY + h / 2, fz, origin, scale, angleH, angleV);
                context.DrawEllipse(Brushes.Silver, new Pen(Brushes.Gray, 0.5), handlePos, 3, 3);//poignée
            }
            //dessin des cornière
            context.DrawLine(ironPen, bFL, tFL);
            context.DrawLine(ironPen, bFR, tFR);
            context.DrawLine(ironPen, bBR, tBR);
            context.DrawLine(ironPen, tFL, tFR);
            context.DrawLine(ironPen, tFR, tBR);
            context.DrawLine(ironPen, tFL, tBL);
            context.DrawLine(ironPen, tBL, tBR);

            currentY += h;  //on monte pour pouvoir dessiner le casier suivant
        }
    }

    private void DrawFace(DrawingContext ctx, IBrush brush, Pen pen, Point p1, Point p2, Point p3, Point p4)
    {
        var geom = new StreamGeometry();
        using (var c = geom.Open())
        {
            c.BeginFigure(p1, true);
            c.LineTo(p2); c.LineTo(p3); c.LineTo(p4);
            c.EndFigure(true);
        }
        ctx.DrawGeometry(brush, pen, geom);
    }

    private IBrush DarkenBrush(IBrush brush, double factor)
    {
        if (brush is SolidColorBrush scb)
        {
            var c = scb.Color;
            return new SolidColorBrush(Color.FromArgb(c.A,
                (byte)(c.R * factor), (byte)(c.G * factor), (byte)(c.B * factor)));
        }
        return brush;
    }

    private IBrush ParseColor(string? color) => color?.ToLower() switch
    {
        "blanc" or "white" => new SolidColorBrush(Color.Parse("#F0F0F0")),
        "noir" or "black" => new SolidColorBrush(Color.Parse("#2C2C2C")),
        "brun" or "brown" or "marron" => new SolidColorBrush(Color.Parse("#8B4513")),
        "rouge" or "red" => new SolidColorBrush(Color.Parse("#C0392B")),
        "bleu" or "blue" => new SolidColorBrush(Color.Parse("#2980B9")),
        "vert" or "green" => new SolidColorBrush(Color.Parse("#27AE60")),
        "gris" or "gray" => new SolidColorBrush(Color.Parse("#7F8C8D")),
        "verre" or "glass" => new SolidColorBrush(Color.FromArgb(60, 173, 216, 230)),
        _ => new SolidColorBrush(Color.Parse("#BDC3C7")),
    };
}