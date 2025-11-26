using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace ConsoleDeck;

public class CloseButton : Control
{
    private int _cornerRadius = 5;
    private Size _buttonSize = new(40, 30);
    private Color _baseColor = ColorTranslator.FromHtml("#1f1f1f");
    private Color _borderColor = ColorTranslator.FromHtml("#2f2f2f");
    private Color _hoverColor = ColorTranslator.FromHtml("#e81123");
    private Color _pressColor = ColorTranslator.FromHtml("#9b0b17");
    private Color _textColor = ColorTranslator.FromHtml("#ffffff");
    private bool _isHovered = false;
    private bool _isPressed = false;

    [DefaultValue(5)]
    [Category("Appearance")]
    [Description("Gets or sets the corner radius of the button")]
    public int CornerRadius
    {
        get => _cornerRadius;
        set { _cornerRadius = value; Invalidate(); }
    }

    [DefaultValue(typeof(Size), "40, 30")]
    [Category("Appearance")]
    [Description("Gets or sets the size of the button (width and height)")]
    public Size ButtonSize
    {
        get => _buttonSize;
        set { _buttonSize = value; Invalidate(); }
    }

    [DefaultValue(typeof(Color), "#1f1f1f")]
    [Category("Appearance")]
    [Description("Gets or sets the base color of the button")]
    public Color BaseColor
    {
        get => _baseColor;
        set { _baseColor = value; Invalidate(); }
    }

    [DefaultValue(typeof(Color), "#2f2f2f")]
    [Category("Appearance")]
    [Description("Gets or sets the border color of the button")]
    public Color BorderColor
    {
        get => _borderColor;
        set { _borderColor = value; Invalidate(); }
    }

    [DefaultValue(typeof(Color), "#333333")]
    [Category("Appearance")]
    [Description("Gets or sets the hover color of the button")]
    public Color HoverColor
    {
        get => _hoverColor;
        set { _hoverColor = value; Invalidate(); }
    }

    [DefaultValue(typeof(Color), "#1a1a1a")]
    [Category("Appearance")]
    [Description("Gets or sets the press color of the button")]
    public Color PressColor
    {
        get => _pressColor;
        set { _pressColor = value; Invalidate(); }
    }

    [DefaultValue(typeof(Color), "#b8b8b8")]
    [Category("Appearance")]
    [Description("Gets or sets the text color of the button")]
    public Color TextColor
    {
        get => _textColor;
        set { _textColor = value; Invalidate(); }
    }

    public CloseButton()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
        Font = new Font("Segoe UI", 9, FontStyle.Bold);
        Size = ButtonSize;
        Text = "✕";
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        Color currentColor = _isPressed ? PressColor : (_isHovered ? HoverColor : BaseColor);

        using GraphicsPath path = new();
        using SolidBrush brush = new(currentColor);
        // Create one corner rounded rectangle path
        int diameter = CornerRadius * 2;
        path.AddLine(0, 0, Width - diameter, 0);
        path.AddArc(Width - diameter, 0, diameter, diameter, 270, 90);
        path.AddLine(Width, diameter, Width, Height);
        path.AddLine(Width, Height, 0, Height);
        path.CloseFigure();
        e.Graphics.FillPath(brush, path);

        using GraphicsPath borderPath = new();
        // Create border path
        using Pen pen = new(BorderColor, 2);
        borderPath.AddLine(0, 0, Width - diameter, 0);
        borderPath.AddArc(Width - diameter, 0, diameter, diameter, 270, 90);
        borderPath.AddLine(Width, diameter, Width, Height);
        e.Graphics.DrawPath(pen, borderPath);


        // Draw the "X" text centered
        SizeF textSize = e.Graphics.MeasureString(Text, Font);
        PointF textPos = new((Width - textSize.Width) / 2, (Height - textSize.Height) / 2);

        using SolidBrush textBrush = new(TextColor);
        e.Graphics.DrawString(Text, Font, textBrush, textPos);
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        _isHovered = true;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _isHovered = false;
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button == MouseButtons.Left)
        {
            _isPressed = true;
            Invalidate();
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (e.Button == MouseButtons.Left)
        {
            _isPressed = false;
            Invalidate();
            OnClick(e);
        }
    }
}