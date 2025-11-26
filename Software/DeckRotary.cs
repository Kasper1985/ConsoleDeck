using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace ConsoleDeck;

public class DeckRotary : Control
{
    private int _diameter = 150;
    private Color _baseColor = ColorTranslator.FromHtml("#232323");
    private Color _borderColor = ColorTranslator.FromHtml("#2f2f2f");
    private Color _hoverColor = ColorTranslator.FromHtml("#333333");
    private Color _pressColor = ColorTranslator.FromHtml("#237f7f");
    private Color _rotateColor = ColorTranslator.FromHtml("#7f7f23");
    private Color _textColor = ColorTranslator.FromHtml("#b8b8b8");
    private bool _isHovered = false;
    private bool _isPressed = false;
    private bool _isRotatedLeft = false;
    private bool _isRotatedRight = false;

    [DefaultValue(150)]
    [Category("Appearance")]
    [Description("Gets or sets the diameter of the rotary control")]
    public int Diameter
    {
        get => _diameter;
        set 
        { 
            _diameter = value;
            Size = new Size(_diameter, _diameter);
            Invalidate();
        }
    }

    [DefaultValue(typeof(Color), "#232323")]
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

    [DefaultValue(typeof(Color), "#237f7f")]
    [Category("Appearance")]
    [Description("Gets or sets the press color of the button")]
    public Color PressColor
    {
        get => _pressColor;
        set { _pressColor = value; Invalidate(); }
    }

    [DefaultValue(typeof(Color), "#7f7f23")]
    [Category("Appearance")]
    [Description("Gets or sets the secondary press color of the button")]
    public Color RotateColor
    {
        get => _rotateColor;
        set { _rotateColor = value; Invalidate(); }
    }

    [DefaultValue(typeof(Color), "#b8b8b8")]
    [Category("Appearance")]
    [Description("Gets or sets the text color of the button")]
    public Color TextColor
    {
        get => _textColor;
        set { _textColor = value; Invalidate(); }
    }

    [DefaultValue("")]
    [Category("Appearance")]
    [Description("Gets or sets the image path of the rotary control")]
    public string ImagePath { get; set; } = string.Empty;

    public DeckRotary()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.SupportsTransparentBackColor | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        Font = new Font("Segoe UI", 12, FontStyle.Bold);
        Size = new Size(_diameter, _diameter);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        // Enable anti-aliasing and transparency
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.CompositingMode = CompositingMode.SourceOver;
        e.Graphics.CompositingQuality = CompositingQuality.HighQuality;

        Color currentColor = (_isRotatedLeft || _isRotatedRight)
            ? RotateColor
            : _isPressed
                ? PressColor
                : _isHovered
                    ? HoverColor
                    : BaseColor;

        using GraphicsPath path = new();
        using SolidBrush brush = new(currentColor);
        using Pen pen = new(BorderColor);
        path.AddEllipse(ClientRectangle.Resize(-1, -1));
        e.Graphics.FillPath(brush, path);
        e.Graphics.DrawPath(pen, path);

        if (!string.IsNullOrEmpty(Text))
        {
            SizeF textSize = e.Graphics.MeasureString(Text, Font);
            PointF textPos = new((Width - textSize.Width) / 2, (Height - textSize.Height) / 2);

            using SolidBrush textBrush = new(TextColor);
            e.Graphics.DrawString(Text, Font, textBrush, textPos);
        }
    
        if (!string.IsNullOrEmpty(ImagePath) && File.Exists(ImagePath))
        {
            var img = Image.FromFile(ImagePath);
            Point imgPos = new((Width - img.Width) / 2, (Height - img.Height) / 2);
            e.Graphics.DrawImage(img, new Rectangle(imgPos, img.Size));
        }
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

    public void PerformPress()
    {
        _isPressed = true;
        Invalidate();

        System.Timers.Timer timer = new() { Interval = 200 };
        timer.Elapsed += (s, ev) =>
        {
            _isPressed = false;
            Invalidate();
            timer.Dispose();
        };
        timer.Start();
    }

    public void PerformRotation(bool isLeft)
    {
        _isRotatedLeft = isLeft;
        _isRotatedRight = !isLeft;
        Invalidate();

        System.Timers.Timer timer = new() { Interval = 200 };
        timer.Elapsed += (s, ev) =>
        {
            _isRotatedLeft = _isRotatedRight = false;
            Invalidate();
            timer.Dispose();
        };
        timer.Start();
    }
}